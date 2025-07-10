using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI.CustomizableData.Testing
{
	sealed class CustomizableDataTranslationFormTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			using (var testHelper = new CustomizableDataTestHelper())
			{
				using (TempFile file = TempFile.NewWithExtension(".csv"))
				{
					using (var form = new CustomizableDataTranslationFormForTest(
						new CustomizableDataTranslationPageForTest(testHelper.CustomizableDataResourceStrings,
							testHelper.GetMultilingualString("two"), null), Path.GetDirectoryName(file.Filename)))
					{
						form.Show();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						form.Import();

						AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Import has been successful", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestImport_Error()
		{
			using (var testHelper = new CustomizableDataTestHelper())
			{
				using (TempFile file = TempFile.NewWithExtension(".csv"))
				{
					var page = new CustomizableDataTranslationPageForTest(testHelper.CustomizableDataResourceStrings,
						testHelper.GetMultilingualString("two"), null);

					using (var form = new CustomizableDataTranslationFormForTest(page, Path.GetDirectoryName(file.Filename)))
					{
						form.Show();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						page.ImportError = "boom";
						form.Import();

						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Import completed with errors: boom", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		class CustomizableDataTranslationPageForTest : CustomizableDataTranslationPage
		{
			public CustomizableDataTranslationPageForTest(CustomizableDataResourceStrings customizable, ResourceString initialValue, object context) : base(customizable, initialValue, context)
			{
			}

			public override string ImportEntries(Stream importFileStream, string fileName)
			{
				return ImportError;
			}

			public string ImportError { get; set; }
		}

		public void TestExport()
		{
			using (var testHelper = new CustomizableDataTestHelper())
			{
				using (var form = new CustomizableDataTranslationFormForTest(
					new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings,
						testHelper.GetMultilingualString("two"), null), "X:\\"))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					form.Export();
					Assert(form.SavedFiles.Count >= 33);

					string prefix = @"X:\Customizable Data Test-";

					var contentLines =
("\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
+ "\"EN-US,\"zero\",\"{0}\"\r\n"
+ "\"EN-US,\"one\",\"{0}\"\r\n"
+ "\"EN-US,\"two\",\"{0}\"\r\n"
+ "\"EN-US,\"three\",\"{0}\"\r\n"
+ "\"EN-US,\"four\",\"{0}\"\r\n"
+ "\"EN-US,\"five\",\"{0}\"\r\n"
+ "\"EN-US,\"six\",\"{0}\"\r\n"
+ "\"EN-US,\"seven\",\"{0}\"\r\n"
+ "\"EN-US,\"eight\",\"{0}\"\r\n"
+ "\"EN-US,\"nine\",\"{0}\"\r\n"
+ "\"EN-US,\"ten\",\"{0}\"")
.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var entry in form.SavedFiles)
					{
						Assert(entry.Key.StartsWith(prefix));
						Assert(entry.Key.EndsWith(@".csv"));

						string language = entry.Key.Substring(prefix.Length, 3);
						var lines = entry.Value.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

						for (var index = 0; index < lines.Length; index++)
						{
							var line = lines[index];
							Assert(string.Format("{0} should start with {1}", line, string.Format(contentLines[index], language)), line.StartsWith(string.Format(contentLines[index], language)));
						}
					}
				}
			}
		}

		class CustomizableDataTranslationFormForTest : CustomizableDataTranslationForm
		{
			readonly string directory;
			public Dictionary<string, string> SavedFiles = new Dictionary<string, string>();

			public CustomizableDataTranslationFormForTest(CustomizableDataTranslationPage bo, string directory) : base(bo)
			{
				this.directory = directory;
			}

			protected override bool SaveFile(string path, string content)
			{
				SavedFiles.Add(path, content);
				return true;
			}

			protected override string GetDirectory(string description)
			{
				return directory;
			}

			public void Export()
			{
				OnExport(null, EventArgs.Empty);
			}

			public void Import()
			{
				OnImport(null, EventArgs.Empty);
			}
		}

		public void TestRegistryItemWithParameters()
		{
			var registryItem = AccountingConfigurationRegistry.Instance.ProFormaTaxAdjustmentNoteTitle;
			var captionSource = new TranslatableRegistryItemValueCaptionSource(registryItem, registryItem.Value);
			var customizable = new CustomizableDataResourceStrings(captionSource);
			var initialValue = captionSource.GetRuntimeCaptions().FirstOrDefault();
			var page = new CustomizableDataTranslationPage(customizable, (ResourceString)initialValue, null);

			var engText = "";
			var chnText = "";

			using (var form = new CustomizableDataTranslationForm(page))
			{
				form.Show();
				var currencyManager = form.translationsOfCurrentValueGrid.ListManager;

				for (int i = 0; i < currencyManager.Count; i++)
				{
					currencyManager.Position = i;
					var currentTopGrid = (CustomizableDataTranslationEntry)currencyManager.GetCurrent();

					if (currentTopGrid.Language == Core.SharedConstants.Languages.ChineseSimplified)
					{
						engText = currentTopGrid.English;
						chnText = currentTopGrid.Translation;
						break;
					}
				}
			}

			AssertEquals(engText, "PRO FORMA {0}");
			AssertEquals(chnText, "形式 {0}");
		}
	}
}
