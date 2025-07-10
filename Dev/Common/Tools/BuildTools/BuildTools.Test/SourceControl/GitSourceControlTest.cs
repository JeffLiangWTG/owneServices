using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using LibGit2Sharp;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using WTG.DevTools.SourceControl;

namespace CargoWise.BuildTools.Testing
{
	sealed class GitSourceControlTest : TestCase
	{
		public void TestUnstagedEditedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(filePath, "two");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals("one", File.ReadAllText(filePath));
				}
			}
		}

		public void TestStagedEditedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(filePath, "two");
					Commands.Stage(repository, "*");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals("one", File.ReadAllText(filePath));
				}
			}
		}

		public void TestUnstagedAddedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				File.WriteAllText(filePath, "one");
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals(false, File.Exists(filePath));
				}
			}
		}

		public void TestStagedAddedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals(false, File.Exists(filePath));
				}
			}
		}

		public void TestUnstagedDeletedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.Delete(filePath);
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals("one", File.ReadAllText(filePath));
				}
			}
		}

		public void TestStageDeletedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.Delete(filePath);
					Commands.Stage(repository, "*");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithPendingChanges());
					AssertEquals(true, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(true, sourceControl.IsDifferent(filePath));
					sourceControl.UndoCheckOut(filePath, false);
					AssertEquals("one", File.ReadAllText(filePath));
				}
			}
		}

		public void TestIgnoredFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(tempDirectory.DirectoryName, ".gitignore"), "foo");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(filePath, "one");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(false, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(Array.Empty<string>(), sourceControl.GetFilesWithPendingChanges());
					AssertEquals(false, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(false, sourceControl.IsDifferent(filePath));
				}
			}
		}

		public void TestCleanFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals(true, sourceControl.IsFileInSourceControl(filePath));
					AssertArrayEqualsByElements(Array.Empty<string>(), sourceControl.GetFilesWithPendingChanges());
					AssertEquals(false, sourceControl.IsFileCheckedOutByMe(filePath));
					AssertEquals(false, sourceControl.IsDifferent(filePath));
				}
			}
		}

		public void TestNoDevelopmentBranch()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertEquals("master", sourceControl.GetCurrentBranchName());
					AssertArrayEqualsByElements(Array.Empty<string>(), sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestDevelopmentBranch()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var filePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737");
					Commands.Checkout(repository, branch);
					File.WriteAllText(filePath, "updated");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertEquals("BRE/WI00332737", sourceControl.GetCurrentBranchName());
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestChangesInCurrentDevelopmentBranchIncludesUncomittedChangesWithNoDevelopmentBranch()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				var filePath = Path.Combine(tempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(tempDirectory.DirectoryName))
				{
					File.WriteAllText(filePath, "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(filePath, "two");
					Commands.Stage(repository, "*");
				}
				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestChangesInCurrentDevelopmentBranchIncludesUncomittedChangesWithDevelopmentBranch()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var filePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737");
					Commands.Checkout(repository, branch);
					File.WriteAllText(filePath, "updated");
					Commands.Stage(repository, "*");
				}
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestDeleteChangesInCurrentDevelopmentBranch()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var filePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737");
					Commands.Checkout(repository, branch);
					File.Delete(filePath);
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
					AssertArrayEqualsByElements(Array.Empty<string>(), sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo(), includeDeletedFiles: false));
				}
			}
		}

		public void TestFileWithChangeInBranchAndUncomittedChangeReturnedOnce()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var filePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737");
					Commands.Checkout(repository, branch);
					File.WriteAllText(filePath, "updated");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(filePath, "updated again");
				}
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { filePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestPatchBackBranch()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "bar"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					repository.CreateBranch("releases/CW20210503");
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "car"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "bar"), "two");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var fooFilePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				GitCli.CheckoutAsync(localTempDirectory.DirectoryName, "releases/CW20210503").GetAwaiter().GetResult();
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737/DPR");
					Commands.Checkout(repository, branch);
					File.WriteAllText(fooFilePath, "updated");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { fooFilePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetReleaseInfo(ReleaseRings.Codes.DPR, new DateTime(2021, 5, 3))));
				}
			}
		}

		public void TestFilesWithChangesInCurrentBranchWhenMasterIsAheadOfBranch()
		{
			using (var remoteTempDirectory = new TempDirectory())
			using (var localTempDirectory = new TempDirectory())
			{
				Repository.Init(remoteTempDirectory.DirectoryName);
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "foo"), "one");
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "bar"), "one");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				Repository.Clone(remoteTempDirectory.DirectoryName, localTempDirectory.DirectoryName);
				var fooFilePath = Path.Combine(localTempDirectory.DirectoryName, "foo");
				using (var repository = new Repository(localTempDirectory.DirectoryName))
				{
					var branch = repository.CreateBranch("BRE/WI00332737");
					Commands.Checkout(repository, branch);
					File.WriteAllText(fooFilePath, "updated");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				using (var repository = new Repository(remoteTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(remoteTempDirectory.DirectoryName, "bar"), "updated");
					Commands.Stage(repository, "*");
					repository.Commit(string.Empty, GetSignature(), GetSignature());
				}
				GitCli.FetchAsync(localTempDirectory.DirectoryName, "origin master:master").GetAwaiter().GetResult();
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					AssertArrayEqualsByElements(new[] { fooFilePath }, sourceControl.GetFilesWithChangesInCurrentBranch(GetAlpReleaseInfo()));
				}
			}
		}

		public void TestWithAdditionalRepository()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);

				using (var sourceControl = new GitSourceControl(tempDirectory.DirectoryName))
				{
					var mock = repository.Create<ISourceControl>();
					mock.Setup(x => x.Dispose());

					using (var agg = (AggregatedSourceControl)sourceControl.WithAdditionalRepository(mock.Object, "Q:\\Foo"))
					{
						AssertContainsExactElementsInExactOrder(new[] { tempDirectory.DirectoryName, "Q:\\Foo" }, agg.Children.Select(x => TrimTrailingSeparator(x.Key)));
						AssertContainsExactElementsInExactOrder(new[] { sourceControl, mock.Object }, agg.Children.Select(x => x.Value));
					}
				}
			}
		}

		public void TestUpdateAppsettingPropertiesCorrectlyUpdatesProperties()
		{
			using (var localTempDirectory = new TempDirectory())
			{
				var repo = Repository.Init(localTempDirectory.DirectoryName);
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig1.json"), "{\"testKeyA\": \"testVal1\",\"testKeyB\":\"testVal2\",\"testKeyParentA\":{\"testKeyChild1\":\"testValChild1\",\"testKeyChild2\":\"testValChild2\"}}");
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig2.json"), "{\"testKeyC\": \"testVal1\",\"testKeyD\":\"testVal2\",\"testKeyParentB\":{\"testKeyChild3\":\"testValChild3\",\"testKeyChild4\":\"testValChild4\"}, \"testKeyParentC\":{\"testKeySubParentA\":{\"testKeySubChild1\":\"testValSubChild1\",\"testKeySubChild2\":\"testValSubChild2\"}}}");
					var files = new List<string> { "testConfig1.json", "testConfig2.json" };

					var updates = new Dictionary<string, object>();
					updates.Add("testKeyA", "Update1");
					updates.Add("testKeyB", "Update2");
					updates.Add("testKeyC", "Update3");
					updates.Add("testKeyParentB:testKeyChild3", "Update4");
					updates.Add("testKeyParentC:testKeySubParentA:testKeySubChild1", "Update5");

					sourceControl.UpdateAppsettingProperties(files, updates, new List<string>());

					var jsonObj1 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig1.json")) as JObject;
					var jsonObj2 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig2.json")) as JObject;
					AssertEquals(jsonObj1["testKeyA"].ToString(), "Update1");
					AssertEquals(jsonObj1["testKeyB"].ToString(), "Update2");
					AssertEquals(jsonObj2["testKeyC"].ToString(), "Update3");
					AssertEquals(jsonObj2["testKeyParentB"]["testKeyChild3"].ToString(), "Update4");
					AssertEquals(jsonObj2["testKeyParentC"]["testKeySubParentA"]["testKeySubChild1"].ToString(), "Update5");
				}
			}
		}

		public void TestUpdateAppsettingPropertiesCorrectlyDeletesProperties()
		{
			using (var localTempDirectory = new TempDirectory())
			{
				var repo = Repository.Init(localTempDirectory.DirectoryName);
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig1.json"), "{\"testKeyA\": \"testVal1\",\"testKeyB\":\"testVal2\",\"testKeyParentA\":{\"testKeyChild1\":\"testValChild1\",\"testKeyChild2\":\"testValChild2\"}}");
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig2.json"), "{\"testKeyC\": \"testVal1\",\"testKeyD\":\"testVal2\",\"testKeyParentB\":{\"testKeyChild3\":\"testValChild3\",\"testKeyChild4\":\"testValChild4\"}, \"testKeyParentC\":{\"testKeySubParentA\":{\"testKeySubChild1\":\"testValSubChild1\",\"testKeySubChild2\":\"testValSubChild2\"}}}");
					var files = new List<string> { "testConfig1.json", "testConfig2.json" };

					var deletions = new List<string>();
					deletions.Add("testKeyD");
					deletions.Add("testKeyParentA");
					deletions.Add("testKeyParentB:testKeyChild4");
					deletions.Add("testKeyParentC:testKeySubParentA:testKeySubChild2");

					sourceControl.UpdateAppsettingProperties(files, new Dictionary<string, object>(), deletions);

					var jsonObj1 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig1.json")) as JObject;
					var jsonObj2 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig2.json")) as JObject;

					AssertEquals(jsonObj2.ContainsKey("testKeyD"), false);
					AssertEquals(jsonObj1.ContainsKey("testKeyParentA"), false);
					AssertEquals(jsonObj2.ContainsKey("testKeyParentB"), true);
					var testKeyParentB = jsonObj2["testKeyParentB"] as JObject;
					AssertEquals(testKeyParentB.ContainsKey("testKeyChild4"), false);
					AssertEquals(jsonObj2.ContainsKey("testKeyParentC"), true);
					var testKeyParentC = jsonObj2["testKeyParentC"] as JObject;
					AssertEquals(testKeyParentC.ContainsKey("testKeySubParentA"), true);
					var testKeySubParentA = testKeyParentC["testKeySubParentA"] as JObject;
					AssertEquals(testKeySubParentA.ContainsKey("testKeySubChild2"), false);
				}
			}
		}

		public void TestUpdateAppsettingPropertiesOnlyUpdatesPropertyIfItIsPresentAndDoesNotAddItIn()
		{
			using (var localTempDirectory = new TempDirectory())
			{
				var repo = Repository.Init(localTempDirectory.DirectoryName);
				using (var sourceControl = new GitSourceControl(localTempDirectory.DirectoryName))
				{
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig1.json"), "{\"testKeyA\": \"testVal1\",\"testKeyB\":\"testVal2\",\"testKeyParentA\":{\"testKeyChild1\":\"testValChild1\",\"testKeyChild2\":\"testValChild2\"}}");
					File.WriteAllText(Path.Combine(localTempDirectory, "testConfig2.json"), "{\"testKeyC\": \"testVal1\",\"testKeyD\":\"testVal2\",\"testKeyParentB\":{\"testKeyChild3\":\"testValChild3\",\"testKeyChild4\":\"testValChild4\"}, \"testKeyParentC\":{\"testKeySubParentA\":{\"testKeySubChild1\":\"testValSubChild1\",\"testKeySubChild2\":\"testValSubChild2\"}}}");
					var files = new List<string> { "testConfig1.json", "testConfig2.json" };

					var updates = new Dictionary<string, object>();
					updates.Add("testKeyNotReal", "Update1");
					updates.Add("testKeyParentB:testKeyNotRealChild", "Update2");
					updates.Add("testKeyParentC:testKeySubParentA:testKeyNotRealSubChild", "Update3");

					sourceControl.UpdateAppsettingProperties(files, updates, new List<string>());

					var jsonObj1 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig1.json")) as JObject;
					var jsonObj2 = JsonConvert.DeserializeObject(File.ReadAllText($"{localTempDirectory.DirectoryName}/testConfig2.json")) as JObject;

					AssertEquals(jsonObj1.ContainsKey("testKeyNotReal"), false);
					AssertEquals(jsonObj2.ContainsKey("testKeyNotReal"), false);

					AssertEquals(jsonObj1.ContainsKey("testKeyNotRealChild"), false);
					AssertEquals(jsonObj2.ContainsKey("testKeyNotRealChild"), false);
					var testKeyParentB = jsonObj2["testKeyParentB"] as JObject;
					AssertEquals(testKeyParentB.ContainsKey("testKeyNotRealChild"), false);

					AssertEquals(jsonObj1.ContainsKey("testKeyNotRealSubChild"), false);
					AssertEquals(jsonObj2.ContainsKey("testKeyNotRealSubChild"), false);
					var testKeySubParentA = jsonObj2["testKeyParentC"]["testKeySubParentA"] as JObject;
					AssertEquals(testKeySubParentA.ContainsKey("testKeyNotRealSubChild"), false);
				}
			}
		}

		IReleaseInfo GetAlpReleaseInfo()
		{
			var mockReleaseInfo = new Mock<IReleaseInfo>();
			mockReleaseInfo.SetupGet(o => o.ReleaseRing).Returns(ReleaseRings.Codes.ALP);
			return mockReleaseInfo.Object;
		}

		IReleaseInfo GetReleaseInfo(string releaseRing, DateTime releaseDate)
		{
			var mockReleaseInfo = new Mock<IReleaseInfo>();
			mockReleaseInfo.SetupGet(o => o.ReleaseRing).Returns(releaseRing);
			mockReleaseInfo.SetupGet(o => o.ReleaseDate).Returns(releaseDate);
			return mockReleaseInfo.Object;
		}

		static Signature GetSignature() => new Signature(Environment.UserName, Environment.UserName + "@wisetechglobal.com", DateTimeOffset.Now);

		static string TrimTrailingSeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			var finalChar = path[path.Length - 1];

			if (finalChar == Path.DirectorySeparatorChar || finalChar == Path.AltDirectorySeparatorChar)
			{
				return path.Substring(0, path.Length - 1);
			}

			return path;
		}
	}
}
