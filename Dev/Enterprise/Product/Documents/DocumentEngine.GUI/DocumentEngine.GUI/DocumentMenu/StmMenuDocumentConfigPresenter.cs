using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocumentMenu;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class StmMenuDocumentConfigPresenter
	{
		public StmMenuDocumentConfigPresenter(
			IStmMenuDocumentConfigForm view,
			TemporaryStmMenuDocumentConfig config,
			IDocumentSupportable documentSupportable)
		{
			this.view = view;
			this.config = config;
			this.documentSupportable = documentSupportable;

			view.AddButtonClicked += delegate
			{ AddConfigItemsFromSelectedSections(); };
			view.RemoveButtonClicked += delegate
			{ RemoveSelectedConfigItems(); };
			view.MoveDownButtonClicked += delegate
			{ MoveConfigItemDown(); };
			view.MoveUpButtonClicked += delegate
			{ MoveConfigItemUp(); };

			view.TemplateSectionDoubleClicked += delegate
			{ AddConfigItemsFromSelectedSections(); };
			view.ConfigItemDoubleClicked += delegate
			{ RemoveSelectedConfigItems(); };
			view.PreviewButtonClicked += delegate
			{ PreviewDocumentConfig(); };
			view.PreviewSectionButtonClicked += new EventHandler(HandlePreviewSectionButtonClicked);
			view.PreviewConfigItemButtonClicked += new EventHandler(HandlePreviewConfigItemButtonClicked);
		}

		readonly IStmMenuDocumentConfigForm view;
		readonly TemporaryStmMenuDocumentConfig config;
		readonly IDocumentSupportable documentSupportable;

		void HandlePreviewSectionButtonClicked(object sender, EventArgs e)
		{
			var selectedSections = view.GetSelectedSections();
			if (selectedSections.Any())
			{
				var section = selectedSections.First();
				PreviewSection(section);
			}
		}

		void HandlePreviewConfigItemButtonClicked(object sender, EventArgs e)
		{
			var selectedConfigItems = view.GetSelectedConfigItems();
			if (selectedConfigItems.Any())
			{
				var configItem = selectedConfigItems.First();
				var section = config.AvailableSections.CollectionToFilter.Find(configItem.S4_SectionItemName);

				if (section == null)
				{
					var message = Res.GetString("DDA1E2CF-37C4-47AB-89AB-B7516BC00F1A", "Could not preview section because this section is neither a system defined config item nor a customized config item and should be removed.");
					var caption = Res.GetString("53692021-B6BC-4C2F-ADCA-16C6988BB89F", "Cannot Preview Section");
					view.ShowError(message, caption);
				}
				else
				{
					PreviewSection(section);
				}
			}
		}

		void PreviewSection(TemplateSection section)
		{
			if (section != null && documentSupportable != null)
			{
				var documentSupporter = documentSupportable.DocumentSupporter;
				if (documentSupporter != null)
				{
					var dataContext = new DataContextValue(nameof(Core.Constants.DataContext.GenericFreightJob));
					var documentCommand = config.Factory.Load<DocumentCommand>(config.MenuTemplatePivot.MenuItem.PK);
					var bODocDataProviders = documentSupporter.GetBODocDataProviders(dataContext, documentCommand);

					if (bODocDataProviders != null)
					{
						var docDataProviders = new List<IBODocDataProvider>(bODocDataProviders);

						if (docDataProviders.Count > 0)
						{
							if (documentSupporter.BusinessObject != null)
							{
								var topLevelDataProvider = BODocDataProvider.Get(documentSupporter.BusinessObject);

								if (!docDataProviders.Contains(topLevelDataProvider))
								{
									docDataProviders.Add(topLevelDataProvider);
								}
							}

							if (config.MenuTemplatePivot.DocType != null)
							{
								foreach (var docDataProvider in docDataProviders)
								{
									IDocTypeCode docDataTypeCode = docDataProvider as IDocTypeCode;
									if (docDataTypeCode != null)
									{
										docDataTypeCode.DocTypeCode = config.MenuTemplatePivot.DocType.RT_DocType;
									}
								}
							}

							var manager = new SectionPreviewManager(
								config.Factory,
								section.SectionName,
								section.Category,
								config.S3_Calc_DocumentTitle,
								ContactType.Find(documentCommand.SU_ContactType),
								documentCommand.DocumentDirection,
								docDataProviders.ToArray());

							view.ShowSectionPreview(manager);
						}
					}
					else
					{
						var message = Res.GetString("d28cf394-1d1e-4f0d-b95c-37ce9db400c4", "Could not preview section because there was no available data sources.");
						var caption = Res.GetString("4429bb38-f5d1-47b2-939b-bd22a861d966", "Section Preview Unavailable");
						view.ShowError(message, caption);
					}
				}
			}
		}

		void AddConfigItemsFromSelectedSections()
		{
			if (config.ReadOnly)
			{
				ShowCannotEditMessage();
			}
			else
			{
				var selectedSections = view.GetSelectedSections();
				if (selectedSections.Length > 0)
				{
					var addedConfigItems = new List<StmMenuDocumentConfigItem>();
					var invalidSectionNames = new List<string>();

					foreach (var section in selectedSections)
					{
						if (section.SectionName.Length > AutoStmMenuDocumentConfigItem.Schema.S4_SectionItemNameMaxLength)
						{
							invalidSectionNames.Add(section.SectionName);
						}
						else
						{
							var configItem = config.ConfigItems.AddFromTemplateSection(section);
							addedConfigItems.Add(configItem);
						}
					}

					if (invalidSectionNames.Count > 0)
					{
						var message = new StringBuilder();
						message.Append(Res.GetString("78322992-BB32-4F84-A63E-35B33079AA0C", "The following section names are too long. They must not exceed {0} characters:",
								AutoStmMenuDocumentConfigItem.Schema.S4_SectionItemNameMaxLength));
						foreach (var invalidSeciontName in invalidSectionNames)
						{
							message.Append("\r\n\r\n" + invalidSeciontName);
						}
						var caption = Res.GetString("FFE41AA8-E52D-4A8A-A753-F6CB2768CC89", "Template Error");
						view.ShowError(message.ToString(), caption);
						return;
					}

					config.ConfigItems.SortByPrintOrder();
					config.AvailableSections.Rebuild();

					view.SelectConfigItems(addedConfigItems.ToArray());
				}
			}
		}

		void RemoveSelectedConfigItems()
		{
			if (config.ReadOnly)
			{
				ShowCannotEditMessage();
			}
			else
			{
				var itemsToRemove = view.GetSelectedConfigItems();
				if (itemsToRemove.Length > 0)
				{
					config.ConfigItems.DeleteAndReorder(itemsToRemove);
					config.AvailableSections.Rebuild();

					var sortInfo = config.AvailableSections.SortInformation;
					if (sortInfo != null)
					{
						config.AvailableSections.Sort(sortInfo);
					}
				}
			}
		}

		void ShowCannotEditMessage()
		{
			var message = Res.GetString("2732d781-8fff-4578-89e0-bc0263a74d92", "Cannot edit this Document Config - 'System' Configurations are Read Only.");
			var caption = Res.GetString("b43187b2-d3f5-428d-80e5-077f82776655", "Read Only");

			view.ShowError(message, caption);
		}

		void PreviewDocumentConfig()
		{
			config.Validation.ValidateAll();

			if (config.HasErrors)
			{
				var message = Res.GetString("cfc46cbc-3c82-4415-b51c-d856b57e2cba", "There are errors - can't preview.");
				var caption = Res.GetString("5E846068-F94E-41B6-BC2C-14D2C42DC4A2", "Errors");

				view.ShowError(message, caption);
			}
			else
			{
				using (var printTask = new PrintTask())
				{
					try
					{
						var documentPack = new DocumentPack(config.MenuTemplatePivot.MenuItem);
						var loader = new DocumentConfigDocumentPackLoader(documentPack);
						loader.Load(config, documentSupportable);

						if (documentPack.Count > 0)
						{
							printTask.Add(documentPack);
							view.ShowPreview(printTask);
						}
						else
						{
							Globals.Message.ShowWarning(Res.GetString("C082730A-33AC-4F3F-A694-7CB22B9C7AB1", "Cannot generate document with the selected data context."));
						}
					}
					catch (DataContextIsInvalidException ex)
					{
						var caption = Res.GetString("C23DF308-148D-48D2-8AF0-F1C68EE7DE65", "DataContext is invalid");
						view.ShowError(ex.Message, caption);
					}
				}
			}
		}

		bool IsAllowedToEditConfig()
		{
			var result = !config.ReadOnly;

			if (!result)
			{
				ShowCannotEditMessage();
			}

			return result;
		}

		void MoveConfigItemDown()
		{
			if (!IsAllowedToEditConfig())
			{ return; }
			var configItems = view.GetSelectedConfigItems();
			config.ConfigItems.MoveDown(configItems);
			view.SelectConfigItems(configItems);
		}

		void MoveConfigItemUp()
		{
			if (!IsAllowedToEditConfig())
			{ return; }
			var configItems = view.GetSelectedConfigItems();
			config.ConfigItems.MoveUp(configItems);
			view.SelectConfigItems(configItems);
		}
	}
}
