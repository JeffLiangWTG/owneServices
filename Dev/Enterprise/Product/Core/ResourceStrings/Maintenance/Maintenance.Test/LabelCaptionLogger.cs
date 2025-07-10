using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Maintenance.Test
{
	// TODO: Report unbashed controls

	public class LabelCaptionLogger
	{
		public void Run(string command)
		{
			if (string.IsNullOrEmpty(command))
			{
				using (var tempDir = new TempDirectory())
				{
					RunAllBasherTests(tempDir.DirectoryName);
					RemoveDuplicatesAndCountContexts(tempDir.DirectoryName);
					LogContexts(tempDir.DirectoryName);
				}
			}
			else
			{
				string[] arguments = command.Split(',');
				RunBasherTests(arguments[0], arguments[1]);
			}
		}

		void RunAllBasherTests(string outputDirectory)
		{
			foreach (var assemblyName in AssembliesUnderTest.AllAssemblies)
			{
				string[] args = new string[CommandLineArguments.UsedToLaunchApplication.UnparsedArguments.Length];
				for (int i = 0; i < args.Length; i++)
				{
					string arg = CommandLineArguments.UsedToLaunchApplication.UnparsedArguments[i];
					if (arg.StartsWith("-LabelCaptionLogger"))
					{
						arg = "-LabelCaptionLogger:\"" + assemblyName + "," + outputDirectory + "\"";
					}
					args[i] = arg;
				}
				Process process = Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ExeFileNames.CargoWiseWindowsDesktopExe), string.Join(" ", args));
				if (!process.WaitForExit((int)TimeSpan.FromMinutes(60).TotalMilliseconds))
				{
					process.Kill();
					using (var errorWriter = File.AppendText(Path.Combine(outputDirectory, "error")))
					{
						errorWriter.Write(assemblyName);
						errorWriter.Write("\tProcess hung");
						errorWriter.WriteLine();
					}
				}
			}
		}

		void RunBasherTests(string assemblyName, string outputDirectory)
		{
			TestingState.Setup();
			SnailTestAttribute.IncludeSnailTests = true;
			ZLabelCaptionCache.Instance.DataHit += new ZLabelCaptionCache.DataHitDelegate(Instance_DataHit);
			try
			{
				var formBasherTestTypeRetriever = new SubClassRetriever(new string[] { assemblyName }, typeof(ZFormBasherTest));
				foreach (Type formBasherTestType in formBasherTestTypeRetriever.Retrieve())
				{
					if (TranslationFileModuleMapping.Instance.Lookup(formBasherTestType.Namespace).ModuleType == TranslationFileModuleMapping.ModuleTypes.GUI)
					{
						try
						{
							if (writer == null)
							{
								writer = new StreamWriter(Path.Combine(outputDirectory, assemblyName + ".log"), true);
							}
							currentBasher = formBasherTestType;
							ITest test = new TestSuite(formBasherTestType).NewTest("TestBashingForm");
							test.Run(new TestResult(Array.Empty<ITestListener>()));
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							using (var errorWriter = File.AppendText(Path.Combine(outputDirectory, "error")))
							{
								errorWriter.Write(formBasherTestType.FullName);
								errorWriter.Write('\t');
								errorWriter.Write(ex.Message);
								errorWriter.WriteLine();
							}
						}
					}
				}
			}
			finally
			{
				if (writer != null)
				{
					writer.Close();
				}
				ZLabelCaptionCache.Instance.DataHit -= new ZLabelCaptionCache.DataHitDelegate(Instance_DataHit);
			}
		}

		void Instance_DataHit(ResourceStringData data, Control control)
		{
			if (data != null && control != null)
			{
				var context = new CaptionContext();
				context.key = data.Key;
				Point relativeLocation;
				Control bashedControl;
				context.path = currentBasher.Assembly.GetName().Name + "," + currentBasher.FullName + ":" + GetControlPath(data, control, out relativeLocation, out bashedControl);
				context.position = (control.Location.Y + relativeLocation.Y) * 10000 + (control.Location.X + relativeLocation.X);
				if (bashedControl != null)
				{
					context.control = bashedControl.GetType().Assembly.GetName().Name + "," + bashedControl.GetType().FullName;
				}
				context.Write(writer);
			}
		}

		string GetControlPath(ResourceStringData data, Control control, out Point relativeLocation, out Control bashControl)
		{
			relativeLocation = new Point();
			bashControl = null;
			bool pastMasterParent = false;
			string path = string.Empty;
			Control current = control;
			while ((current = current.Parent) != null)
			{
				if (current is TabPage && !pastMasterParent)
				{
					if (IsDefaultTab((TabPage)current))
					{
						path = current.Name + (!string.IsNullOrEmpty(path) ? "\\" + path : "");
						path = "@" + (current = current.Parent).Name + "\\" + path;
					}
					else
					{
						path = current.Name + (!string.IsNullOrEmpty(path) ? "@" + path : "");
					}
					pastMasterParent = true;
				}
				else if (current is Form)
				{
					if (!string.IsNullOrEmpty(path) && !pastMasterParent)
					{
						path = "@" + path;
					}
				}
				else
				{
					path = current.Name + (!string.IsNullOrEmpty(path) ? "\\" + path : "");
				}
				if (!pastMasterParent)
				{
					relativeLocation.Offset(current.Location);
				}
				if (bashControl == null && data.Key.StartsWith(current.Name + "|"))
				{
					bashControl = current;
				}
			}
			return path;
		}

		bool IsDefaultTab(TabPage tabPage)
		{
			return tabPage.Parent is TabControl && ((TabControl)tabPage.Parent).TabPages.IndexOf(tabPage) == 0;
		}

		TextWriter writer;
		Type currentBasher;

		void RemoveDuplicatesAndCountContexts(string directory)
		{
			foreach (string file in Directory.GetFiles(directory, "*.log"))
			{
				DoRemoveDuplicatesAndCountContexts(file);
			}
		}

		void DoRemoveDuplicatesAndCountContexts(string file)
		{
			var dictionary = new SortedDictionary<CaptionContextKey, CaptionContext>();
			using (var reader = new StreamReader(file))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var value = CaptionContext.Parse(line);
					var key = new CaptionContextKey(value);
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, value);
					}
				}
			}

			string currentContextPath = null;
			int count = 0;
			using (var writer = new StreamWriter(file))
			{
				foreach (var value in dictionary.Values)
				{
					var item = ResourceStringsFactory.Lookup(Res.DefaultLanguage, value.key);
					if (item != null && TranslationFileModuleMapping.Instance.Lookup(TranslationFileExporter.GetNamespaceFromClassName(item.HD_ContextClassName)).ModuleType == TranslationFileModuleMapping.ModuleTypes.GUI)
					{
						value.Write(writer);

						string contextPath = GUIStringContextInfo.Parse(value.path).GroupingKey;
						if (contextPath == currentContextPath)
						{
							count++;
						}
						else
						{
							if (currentContextPath != null)
							{
								contextCounts[currentContextPath] = count;
							}
							currentContextPath = contextPath;
							count = 1;
						}
					}
				}
				if (currentContextPath != null)
				{
					contextCounts[currentContextPath] = count;
				}
			}
		}

		void LogContexts(string logDirectory)
		{
			ResourceStringsMetaData.UseSourceFile = true;
			var metaData = ResourceStringsMetaData.GetInstance();
			ResetExistingContexts(metaData);

			var logged = new List<HelpDataString>();
			using (var keySort = new BucketSort<CaptionContext>(new CaptionContextSerializer(), new CaptionKeyComparer(), 100000))
			{
				foreach (string file in Directory.GetFiles(logDirectory, "*.log"))
				{
					using (var reader = new StreamReader(file))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							keySort.Add(CaptionContext.Parse(line));
						}
					}
				}

				CaptionContext bestForKey = null;
				int bestCount = 0;
				foreach (var item in keySort)
				{
					var itemContext = GUIStringContextInfo.Parse(item.path);
					if (bestForKey != null && bestForKey.key != item.key)
					{
						if (bestCount > 3)
						{
							logged.Add(Log(bestForKey));
						}
						bestForKey = null;
						bestCount = 0;
					}
					if (TranslationFileModuleMapping.Instance.Lookup(itemContext.BasherNamespace).ModuleType == TranslationFileModuleMapping.ModuleTypes.GUI)
					{
						if (bestForKey == null)
						{
							bestForKey = item;
							bestCount = contextCounts[itemContext.GroupingKey];
						}
						else
						{
							int itemCount = contextCounts[itemContext.GroupingKey];
							if (itemCount > bestCount)
							{
								bestForKey = item;
								bestCount = itemCount;
							}
							else if (itemCount == bestCount)
							{
								if (item.control.Split(',')[0] == itemContext.BasherAssemblyName) // control assembly is the same as basher assembly
								{
									bestForKey = item;
								}
								else if (item.path.CompareTo(bestForKey.path) < 0)
								{
									bestForKey = item;
								}
							}
						}
					}
				}
				if (bestForKey != null && bestCount > 3)
				{
					logged.Add(Log(bestForKey));
				}
			}

			metaData.Save(Array.FindAll(logged.ToArray(), item => item != null));
		}

		void ResetExistingContexts(ResourceStringsMetaData metaData)
		{
			var query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_ControlPath, SQLComparisonOperator.NotEqual, string.Empty);
			var matches = ResourceStringsFactory.Load(query);
			foreach (var match in matches)
			{
				match.HD_ControlPath = string.Empty;
				match.HD_ControlIndexInParent = 0;
			}
			metaData.Save(matches);
		}

		HelpDataString Log(CaptionContext item)
		{
			var helpDataString = ResourceStringsFactory.Lookup(Res.DefaultLanguage, item.key, false);
			if (helpDataString != null)
			{
				helpDataString.HD_ControlPath = item.path;
				helpDataString.HD_ControlIndexInParent = item.position;
			}
			return helpDataString;
		}

		readonly Dictionary<string, int> contextCounts = new Dictionary<string, int>();

		class CaptionContextKey : IComparable
		{
			public CaptionContextKey(CaptionContext context)
			{
				this.context = context;
			}

			readonly CaptionContext context;

			public int CompareTo(object obj)
			{
				var key = obj as CaptionContextKey ?? throw new InvalidOperationException("Cannot compare a CaptionContextKey to a " + obj.GetType());

				int result = this.context.path.CompareTo(key.context.path);
				if (result == 0)
				{
					result = this.context.key.CompareTo(key.context.key);
				}
				return result;
			}
		}

		class CaptionContext
		{
			public static CaptionContext Parse(string line)
			{
				string[] parts = line.Split('\t');
				CaptionContext context = new CaptionContext();
				context.key = parts[0];
				context.path = parts[1];
				context.position = int.Parse(parts[2]);
				context.control = parts[3];
				return context;
			}

			public void Write(TextWriter writer)
			{
				writer.Write(key);
				writer.Write('\t');
				writer.Write(path);
				writer.Write('\t');
				writer.Write(position);
				writer.Write('\t');
				writer.Write(control);
				writer.WriteLine();
			}

			public string key;
			public string path;
			public int position;
			public string control;
		}

		class CaptionContextSerializer : BucketSort<CaptionContext>.ISerializer
		{
			public CaptionContext Read(TextReader reader)
			{
				CaptionContext value = null;
				string line = reader.ReadLine();
				if (line != null)
				{
					value = CaptionContext.Parse(line);
				}
				return value;
			}

			public void Write(CaptionContext item, TextWriter writer)
			{
				item.Write(writer);
			}
		}

		class CaptionKeyComparer : IComparer<CaptionContext>
		{
			public int Compare(CaptionContext x, CaptionContext y)
			{
				int result = x.key.CompareTo(y.key);
				if (result == 0)
				{
					result = x.path.CompareTo(y.path);
				}
				return result;
			}
		}
	}
}
