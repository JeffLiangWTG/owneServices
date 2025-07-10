using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.IO;
using CargoWise.Shared;
using Dat.Integration;
using WTG.DeploymentUtils.FileSystem;

namespace Enterprise.Dat.Implementation
{
	class WinzorTestReport : IDeploymentProcess
	{
		const string FormBasherTestName = "TestBashingForm";
		const string ModuleBasherTestName = "TestModuleShowsAndCanSearch";

		public WinzorTestReport(DeploymentConfiguration configuration)
		{
			Configuration = configuration;
		}

		public DeploymentConfiguration Configuration { get; }

		public void Deploy(ITaskLogger logger)
		{
			var tempFile = Temp.GetTempFileName();
			var apiClient = new SubmissionsApiClient();
			try
			{
				using (logger.RecordTask($"Downloading test methods"))
				using (var fs = File.Create(tempFile))
				using (var testMethodsStream = apiClient.DownloadLatestTestMethodsFileAsync("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev").GetAwaiter().GetResult())
				{
					testMethodsStream.CopyToAsync(fs).GetAwaiter().GetResult();
				}

				using (var reader = new StreamReader(new GZipStream(File.OpenRead(tempFile), CompressionMode.Decompress)))
				{
					var tests = TestMethodsDataFile.ReadTestMethodsData(reader);

					var winzorCargoWiseAssemblies = Directory.GetFiles(Path.Combine(Configuration.BinariesPath, "winzor"), "*.dll", SearchOption.TopDirectoryOnly)
						.Select(Path.GetFileNameWithoutExtension)
						.ToHashSet();

					var cargoWiseGuiTests = tests
						.Where(t => !IsWinzorTestAssembly(t.Identifier.ScopeName) && winzorCargoWiseAssemblies.Contains(t.Identifier.ScopeName))
						.ToHashSet();

					var winzorTests = tests
						.Where(t => IsWinzorTestAssembly(t.Identifier.ScopeName))
						.Select(t => t.Identifier.ElementName.Substring(7) + "." + t.Identifier.TargetName)
						.ToHashSet();

					var explicitWinzorTests = LoadExplicitList();

					var invalidGUIReferences = LoadInvalidGUIReferenceBaseline();
					var invalidGuiProjectCargoWiseTests = tests
						.Where(t => !IsWinzorTestAssembly(t.Identifier.ScopeName) && invalidGUIReferences.Contains(t.Identifier.ScopeName))
						.ToHashSet();

					WinzorTestStatus GetTestStatus(string testFullName)
					{
						if (winzorTests.Contains(testFullName))
						{
							// The test is passing and included in the test discovery
							return WinzorTestStatus.Pass;
						}
						else if (explicitWinzorTests.Contains(testFullName))
						{
							// The test is failing and included in the explicit list
							return WinzorTestStatus.Fail;
						}
						else
						{
							// The test is neither passing or failing, so it must not be included in the Winzor codebase
							return WinzorTestStatus.Removed;
						}
					}

					var scopeCounts = new Dictionary<string, ScopeCount>();
					var typeCounts = new Dictionary<string, TypeCount>();
					var testsRemovedFromWinzor = new List<string>();
					foreach (var cargoWiseGuiTest in cargoWiseGuiTests)
					{
						var testFullName = cargoWiseGuiTest.Identifier.ElementName + "." + cargoWiseGuiTest.Identifier.TargetName;

						var testStatus = GetTestStatus(testFullName);
						if (testStatus == WinzorTestStatus.Removed)
						{
							testsRemovedFromWinzor.Add(testFullName);
							// The test is not part of the Winzor codebase, so it should not be counted in the main stats.
							continue;
						}

						if (!scopeCounts.TryGetValue(cargoWiseGuiTest.Identifier.ScopeName, out var scopeCount))
						{
							scopeCount = new ScopeCount();
							scopeCounts.Add(cargoWiseGuiTest.Identifier.ScopeName, scopeCount);
						}
						if (!typeCounts.TryGetValue(cargoWiseGuiTest.Identifier.ElementName, out var typeCount))
						{
							typeCount = new TypeCount();
							typeCounts.Add(cargoWiseGuiTest.Identifier.ElementName, typeCount);
						}

						scopeCount.Increment(testStatus);
						typeCount.Increment(cargoWiseGuiTest.Identifier.TargetName, testStatus);
					}

					var results = new StringBuilder();
					results.AppendLine("Winzor Tests Report");

					var matched = scopeCounts.Sum(c => c.Value.Passing);
					var total = scopeCounts.Sum(c => c.Value.Total);
					results.Append($"Total Tests: {matched.ToString("N0")} / {total.ToString("N0")} ({(int)(((double)matched / total) * 100)}%)");
					results.AppendLine("<br/>");

					matched = typeCounts.Count(c => c.Value.ContainsFormBasherTest && c.Value.AllPassing);
					total = typeCounts.Count(c => c.Value.ContainsFormBasherTest);
					results.Append($"Forms with all tests passing: {matched.ToString("N0")} / {total.ToString("N0")} ({(int)(((double)matched / total) * 100)}%)");
					results.AppendLine("<br/>");

					matched = typeCounts.Count(c => c.Value.ContainsModuleBasherTest && c.Value.AllPassing);
					total = typeCounts.Count(c => c.Value.ContainsModuleBasherTest);
					results.Append($"Modules with all tests passing: {matched.ToString("N0")} / {total.ToString("N0")} ({(int)(((double)matched / total) * 100)}%)");
					results.AppendLine("<br/>");

					results.Append($"Tests removed from Winzor: {testsRemovedFromWinzor.Count.ToString("N0")}");
					results.AppendLine("<br/>");

					results.Append($"Unit tests discovered in projects with invalid GUI references: {invalidGuiProjectCargoWiseTests.Count.ToString("N0")}");
					results.AppendLine("<br/>");

					void CreateDocumentAndAppendToMessage(string title, Action<TextWriter> document)
					{
						using (var ms = new MemoryStream())
						using (var writer = new StreamWriter(ms))
						{
							document(writer);
							writer.Flush();
							ms.Position = 0;
							var url = apiClient.UploadTestFailureDataAndGetUrlAsync(ms).GetAwaiter().GetResult().Trim('\"');
							results.AppendLine($"<a href=\"{url}\">{title}</a><br/>");
						}
					}

					CreateDocumentAndAppendToMessage("Tests by assembly", writer =>
					{
						foreach (var item in scopeCounts.OrderBy(c => c.Key))
						{
							writer.WriteLine($"{item.Key}: {item.Value.Passing} / {item.Value.Total} ({item.Value.Percentage}%)");
						}
					});

					var allFailingTests = typeCounts.Where(c => !c.Value.AllPassing);
					if (allFailingTests.Any())
					{
						CreateDocumentAndAppendToMessage("All failing tests", writer =>
						{
							foreach (var item in typeCounts.Where(c => !c.Value.AllPassing).OrderBy(c => c.Key))
							{
								item.Value.FailingTests.Sort();
								foreach (var failingTest in item.Value.FailingTests)
								{
									writer.WriteLine($"{item.Key}.{failingTest}");
								}
							}
						});
					}

					var failingFormTests = typeCounts.Where(c => c.Value.ContainsFormBasherTest && !c.Value.AllPassing);
					if (failingFormTests.Any())
					{
						CreateDocumentAndAppendToMessage("Failing form tests", writer =>
						{
							foreach (var item in failingFormTests.OrderBy(c => c.Key))
							{
								foreach (var failingTest in item.Value.FailingTests)
								{
									writer.WriteLine($"{item.Key}.{failingTest}");
								}
							}
						});
					}

					var failingModuleTests = typeCounts.Where(c => c.Value.ContainsModuleBasherTest && !c.Value.AllPassing);
					if (failingModuleTests.Any())
					{
						CreateDocumentAndAppendToMessage("Failing module tests", writer =>
						{
							foreach (var item in failingModuleTests.OrderBy(c => c.Key))
							{
								foreach (var failingTest in item.Value.FailingTests)
								{
									writer.WriteLine($"{item.Key}.{failingTest}");
								}
							}
						});
					}

					if (testsRemovedFromWinzor.Count > 0)
					{
						CreateDocumentAndAppendToMessage("All tests removed from Winzor", writer =>
						{
							foreach (var test in testsRemovedFromWinzor)
							{
								writer.WriteLine(test);
							}
						});
					}

					if (invalidGuiProjectCargoWiseTests.Count > 0)
					{
						CreateDocumentAndAppendToMessage("Test projects with invalid GUI references", writer =>
						{
							foreach (var group in invalidGuiProjectCargoWiseTests.GroupBy(t => t.Identifier.ScopeName).OrderBy(g => g.Key))
							{
								writer.WriteLine($"{group.Key}: {group.Count()} ContainsFormTests: {group.Any(t => t.Identifier.TargetName == FormBasherTestName)} ContainsModuleTests: {group.Any(t => t.Identifier.TargetName == ModuleBasherTestName)}");
							}
						});
					}

					logger.RecordInfo(results.ToString());

					var toAddress = "a34e54bf.WiseTechGlobal.onmicrosoft.com@apac.teams.ms";
					var fromAddress = "dat@wisetechglobal.com";
					using (var client = new SmtpClient("mail.wtg.zone") { Credentials = new NetworkCredential(@"CORP\noreply", "M@1lN0R3ply") })
					using (var message = new MailMessage(fromAddress, toAddress))
					{
						message.ReplyToList.Add(fromAddress);
						message.Subject = "Winzor Tests Report";
						message.Body = results.ToString();
						message.IsBodyHtml = true;

						client.Send(message);
					}
				}
			}
			finally
			{
				FileIO.DeleteFile(tempFile);
			}
		}

		static bool IsWinzorTestAssembly(string assemblyName)
			=> assemblyName.StartsWith(@"winzor\", StringComparison.InvariantCultureIgnoreCase);

		HashSet<string> LoadExplicitList()
		{
			var explicitList = new HashSet<string>();
			using var stream = Assembly.LoadFrom(Path.Combine(Configuration.BinariesPath, "Winzor", "WinzorTestAdapter.dll")).GetManifestResourceStream("WinzorTestAdapter.Explicit.txt");
			using var reader = new StreamReader(stream);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				explicitList.Add(line);
			}
			return explicitList;
		}

		HashSet<string> LoadInvalidGUIReferenceBaseline()
		{
			var explicitList = new HashSet<string>();
			using var stream = Assembly.LoadFrom(Path.Combine(Configuration.BinariesPath, "Enterprise.ReflectionTest.dll")).GetManifestResourceStream("Enterprise.ReflectionTest.BusinessProjectsShouldNotReferenceGUIProjectsBaseline.txt");
			using var reader = new StreamReader(stream);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				explicitList.Add(line);
			}
			return explicitList;
		}

		class ScopeCount
		{
			public int Total { get; private set; }
			public int Passing { get; private set; }
			public int Percentage => (int)(((double)Passing / Total) * 100);

			public void Increment(WinzorTestStatus status)
			{
				Total++;
				if (status == WinzorTestStatus.Pass)
				{
					Passing++;
				}
			}
		}

		class TypeCount
		{
			public bool AllPassing { get; private set; } = true;
			public bool ContainsFormBasherTest { get; private set; }
			public bool ContainsModuleBasherTest { get; private set; }
			public List<string> FailingTests { get; } = new List<string>();

			public void Increment(string testName, WinzorTestStatus status)
			{
				if (status == WinzorTestStatus.Fail)
				{
					FailingTests.Add(testName);
					AllPassing = false;
				}

				if (testName == FormBasherTestName)
				{
					ContainsFormBasherTest = true;
				}
				else if (testName == ModuleBasherTestName)
				{
					ContainsModuleBasherTest = true;
				}
			}
		}

		enum WinzorTestStatus
		{
			Pass,
			Fail,
			Removed,
		}
	}
}
