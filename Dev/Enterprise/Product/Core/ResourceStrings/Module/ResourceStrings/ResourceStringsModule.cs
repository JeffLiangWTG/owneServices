using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ResourceStrings.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	[SuppressFormsLocalizedTest]
	public class ResourceStringsModule : ZFilterGridModule
	{
		public ResourceStringsModule()
		{
#if DEBUG

			AddExportDataMenuItem("DocBuilder Strings",
				delegate
				{
					using (var folderBrowser = new ZFolderBrowserDialog())
					using (var tradosFileExporter = ObjectFactory.Get<ITradosFileExporter>())
					{
						folderBrowser.RequireMappablePath = true;
						if (folderBrowser.ShowDialog() == DialogResult.OK)
						{
							using (var progressForm = new ProgressForm())
							{
								progressForm.ShowCancelButton = false;
								progressForm.ShowProgressBar = false;
								progressForm.Show();

								tradosFileExporter.Run(new TradosProjectCreator() { ProjectType = TradosProjectCreator.ProjectTypeCodes.DocBuilder }, folderBrowser.MappedSelectedPath);
							}
						}
					}
				});

			AddExportDataMenuItem("All Strings",
				delegate
				{
					using (var folderBrowser = new ZFolderBrowserDialog())
					using (var tradosFileExporter = ObjectFactory.Get<ITradosFileExporter>())
					{
						folderBrowser.RequireMappablePath = true;
						if (folderBrowser.ShowDialog() == DialogResult.OK)
						{
							using (var progressForm = new ProgressForm())
							{
								progressForm.ShowCancelButton = false;
								progressForm.ShowProgressBar = false;
								progressForm.Show();

								tradosFileExporter.Run(new TradosProjectCreator() { ProjectType = TradosProjectCreator.ProjectTypeCodes.GUI }, folderBrowser.MappedSelectedPath);
							}
						}
					}
				});

			AddImportDataMenuItem("Translations",
				delegate
				{
					using (var folderBrowser = new ZFolderBrowserDialog())
					{
						folderBrowser.RequireMappablePath = true;
						if (folderBrowser.ShowDialog() == DialogResult.OK)
						{
							var langauge = Globals.Message.QueryUserResponse(new UserResponseArgument()
							{
								Caption = "Import Translations",
								Message = "Langauge",
								Buttons = ZMessageBoxButtons.OK,
								MinimumResponseLength = 3,
								AnswerList = new CodeDescriptionPairList(OLookUpEditType.Language)
							});
							if (!string.IsNullOrEmpty(langauge))
							{
								using (var logger = new ImportLogger())
								using (var progressForm = new ProgressForm())
								{
									progressForm.ShowCancelButton = false;
									progressForm.ShowProgressBar = false;
									progressForm.Show();

									try
									{
										using (var impoter = new XmlFileImporter(logger))
										{
											impoter.EditReason = EditReasons.Codes.TradosImport;
											impoter.Import(folderBrowser.MappedSelectedPath, langauge);
										}
									}
									catch (InvalidOperationException ex)
									{
										Globals.Message.ShowError("Failed to import: " + ex.Message);
									}
									catch (XmlException ex)
									{
										Globals.Message.ShowError("Failed to import " + ex.SourceUri + " because of an Xml error:\r\n" + ex.Message + "\r\n\r\nImport is incomplete.");
									}

									progressForm.Close();
									ShowImportResults(logger);
								}
							}
						}
					}
				});
#endif
		}

		protected override int MaxRowsToLoad => 100_000;
		#if DEBUG
		internal int MaxRowsToLoadInternal => MaxRowsToLoad;
		#endif

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result =
				new List<MenuItem>(base.GetNewActionMenuItems())
					{
						new ZMenuItem(
							"My Check Outs",
							delegate { CheckOutsRoutines.FindMyCheckOuts((ResourceStringsFilterControl)EmbeddedControl); }),
#if DEBUG
						new ZMenuItem((NoResString)"Save Selected Strings Locally", new EventHandler(SaveSelectedStringsLocally)),
						new ZMenuItem(
							"Undo unchanged strings",
							delegate { ResourceStringsFactory.UndoUnchanged(); }),
#endif
					};

			return result.ToArray();
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is the default dev dirctory, developer feature only")]
		void SaveSelectedStringsLocally(object sender, EventArgs e)
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				dialog.SelectedPath = @"C:\Dev";
				dialog.RequireMappablePath = true;
				dialog.Description = "Choose Source Code Root Path";
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var originalLocalPath = BuildConstants.LocalEnterprisePath;
					BuildConstants.LocalEnterprisePath = dialog.MappedSelectedPath;
					try
					{
						var dataByLanguage = new Dictionary<string, List<ResourceStringData>>();
						var sourceCache = ResourceStringsFactory.GetResourceStringCache(Res.DefaultLanguage);
						using (var hashCalculator = new ResourceStringHashCalculator())
						{
							foreach (HelpDataString item in this.SelectedBusinessObjects)
							{
								List<ResourceStringData> dataForLanguage;
								if (!dataByLanguage.TryGetValue(item.HD_Language, out dataForLanguage))
								{
									dataByLanguage[item.HD_Language] = dataForLanguage = new List<ResourceStringData>();
								}
								var sourceData = sourceCache.Get(item.HD_Code);
								if (sourceData != null)
								{
									var data = item.ToResourceStringData(sourceHash: hashCalculator.GetHash(sourceData), editReason: item.HD_EditReason);
									dataForLanguage.Add(data);
								}
							}
						}
						foreach (var dictionaryEntry in dataByLanguage)
						{
							new ResourcesDeltaSource(ResourcesDeltaSource.GetSourceControlLanguageFile(dialog.MappedSelectedPath, dictionaryEntry.Key), ResourcesDeltaSource.DeltaSourceLocationType.SourceControl, dictionaryEntry.Key).WriteAll(dictionaryEntry.Value);
						}
					}
					finally
					{
						BuildConstants.LocalEnterprisePath = originalLocalPath;
					}
				}
			}
		}
#endif

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ResourceStrings; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ResourceStrings);
		}

#if DEBUG
		internal IBusinessObjectCollection GetNewGridCollectionInternal() => GetNewGridCollection();
		internal FilterBusinessObject GetNewFilterBusinessObjectInternal() => GetNewFilterBusinessObject();
		internal IFilterControl GetNewFilterControlInternal() => GetNewFilterControl();
		internal ZController GetNewControllerInternal(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);
#endif
		protected override IFilterControl GetNewFilterControl()
		{
			return new ResourceStringsFilterControl((HelpDataStringCollection)GridCollection, (ResourceStringsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HelpDataStringCollection();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ResourceStringsFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return PerformSearchResult.Success(factory, query, ResourceStringsFactory.Load(query), true);
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("E06EA75B-8D4B-48A1-A080-E5ECD875EAA3", "Undo Checkout", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		public void ShowImportResults(ImportLogger logger)
		{
			string message =
				logger.Statistics.Values.Sum() + " resource strings processed\r\n" +
				logger.Statistics[ImportStatus.Import] + " imported\r\n" +
				logger.Statistics[ImportStatus.NotChanged] + " not changed\r\n" +
				logger.Statistics[ImportStatus.NotTranslated] + " not translated\r\n" +
				logger.Statistics[ImportStatus.NoMatchingSource] + " no matching source\r\n" +
				logger.Statistics[ImportStatus.SourceChanged] + " source changed\r\n" +
				"Would you like to save the log file?"
			;

			if (Globals.Message.Show(message, "Import Resource Strings", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				using (var saveFileDialog = new ZSaveFileDialog())
				{
					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						using (var stream = saveFileDialog.OpenFile())
						{
							logger.Save(stream);
						}
					}
				}
			}
		}
	}
}
