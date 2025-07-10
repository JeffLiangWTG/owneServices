using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public sealed class DocumentMenuCustomisation : MenuCustomisation
	{
		DocumentMenuCustomisation(BusinessObjectFactory factory, IDocumentSupportable parent, UserControlProviderList userFieldList)
			: base(factory)
		{
			this.Parent = parent;
			this.userFieldList = userFieldList;
		}

		readonly UserControlProviderList userFieldList;
		public readonly IDocumentSupportable Parent;
		StmMenuItemBaseCollection availableChildMenus;
		RefDocTypeCollection availableDocTypes;

		public StmMenuItemBaseCollection AvailableChildMenus
		{
			get
			{
				if (availableChildMenus == null)
				{
					availableChildMenus = new StmMenuItemBaseCollection(Factory, true);
					availableChildMenus.EditingMode = EditingMode;
					BusinessContext[] supportedContexts = Parent.DocumentSupporter.SupportedChildBusinessContexts;
					BusinessContext[] contextsToSearch;

					if ((supportedContexts != null) && (supportedContexts.Length > 0))
					{
						contextsToSearch = new BusinessContext[supportedContexts.Length + 1];
						supportedContexts.CopyTo(contextsToSearch, 0);
						contextsToSearch[contextsToSearch.Length - 1] = Parent.DocumentSupporter.BusinessContext;
					}
					else
					{
						contextsToSearch = new BusinessContext[] { Parent.DocumentSupporter.BusinessContext };
					}

					availableChildMenus.Load(contextsToSearch, ObjectFactory.Get<IVisualizerMenuCustomisationFormCreator>().GetExcludedClientTemplatePKs());
					availableChildMenus.SetReadOnlyIncludingChildren(true);
				}
				return availableChildMenus;
			}
		}

		public RefDocTypeCollection AvailableDocTypes
		{
			get
			{
				if (availableDocTypes == null)
				{
					IDocManagerSupport docManagerSupport = Parent as IDocManagerSupport;
					if (docManagerSupport != null)
					{
						string referenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManagerSupport.DocManagerInfo.DocManagerCode);

						DocTypeCategoryQuery filter = new DocTypeCategoryQuery(Factory, referenceType);
						filter.AddToFilter(RefDocTypeSchema.RT_IsActive, ZBool.True);
						filter.AddToFilter(RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual, new string[]
						{
							Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument,
							Core.Constants.RefDocTypes.InternallyCreatedPublicDocument
						});

						availableDocTypes = new RefDocTypeCollection(Factory);
						availableDocTypes.AdditionalFilter = filter;
						availableDocTypes.ApplySort(RefDocTypeSchema.Constants.RT_DocType, ListSortDirection.Ascending);
					}
					else
					{
						availableDocTypes = new RefDocTypeCollection(Factory);
					}
					availableDocTypes.SetReadOnlyIncludingChildren(true);
				}
				return availableDocTypes;
			}
		}

		protected override string Description => (NoResString)"Document";

		protected override DocumentSupporter DocumentSupporter
		{
			get
			{
				return new DocumentMenuCustomisationDocumentSupporter(this);
			}
		}

		public override MenuEditingMode EditingMode
		{
			get { return base.EditingMode; }
			set
			{
				base.EditingMode = value;
				if (availableChildMenus != null)
				{
					availableChildMenus.EditingMode = value;
				}
			}
		}

		public new DocumentCommandCollection Menus
		{
			get { return (DocumentCommandCollection)base.Menus; }
		}

#if DEBUG
		internal static event EventHandler SavingEDocsProviderPlaceholders;
#endif

		public StmMenuMenuPivotBase AddChildMenuPivot(DocumentCommand menu, StmMenuItemBase childMenu)
		{
			StmMenuMenuPivotBase result;
			if ((menu != null) && (childMenu != null))
			{
				result = menu.ChildMenus.AddNew();
				result.SF_SU_Inward = menu.PK;

				var mappedBusinessContext = new VirtualBusinessContextsMapping();
				if (mappedBusinessContext.ContainsKey(childMenu.SU_BusinessContext))
				{
					var proxyStmMenuItem = childMenu as ProxyStmMenuItemBase;

					if (proxyStmMenuItem != null)
					{
						result.SF_OverriddenBusinessContext = proxyStmMenuItem.SU_BusinessContext;
						result.SF_SU_Outward = proxyStmMenuItem.ParentMenuItem.PK;
					}
				}
				else
				{
					result.SF_SU_Outward = childMenu.PK;
				}

				result.EditingMode = menu.EditingMode;
				result.Validation.ValidateSF_IsSystemDefined();
			}
			else
			{
				result = null;
			}
			return result;
		}

		public DocumentStmMenuEDocs AddDocTypePivot(DocumentCommand menu, RefDocType docType)
		{
			return (menu != null) && (docType != null) ? menu.AddEDoc(docType) : null;
		}

		static void AddEDocsProviderPlaceholders(BusinessObjectFactory factory, IEDocsProvider eDocsProvider)
		{
			EDocsProviderSupporter eDocsProviderSupporter = eDocsProvider.GetEDocsProviderSupporter();
			MenuItemIdentifier[] consumers = eDocsProviderSupporter.GetConsumers();
			bool saveRequired = false;

			foreach (MenuItemIdentifier identifier in consumers)
			{
				StmMenuItem consumer = identifier.LoadMenuItem<StmMenuItem>(factory);
				if (consumer != null)
				{
					DocumentCommand providerPlaceholder = eDocsProviderSupporter.GetProviderPlaceholder<DocumentCommand>(consumer);
					if (providerPlaceholder == null)
					{
						providerPlaceholder = eDocsProviderSupporter.CreateProviderPlaceholder<DocumentCommand>(consumer);
						saveRequired = true;
					}
					else if (!providerPlaceholder.SU_GS_NKStaffCode.IsEmpty)
					{
						providerPlaceholder.SU_GS_NKStaffCode = ZString.Empty;
						saveRequired = true;
					}
				}
			}

			if (saveRequired)
			{
#if DEBUG
				if (Globals.IsTest && (SavingEDocsProviderPlaceholders != null))
				{
					SavingEDocsProviderPlaceholders.Invoke(null, EventArgs.Empty);
				}
#endif
				factory.Save();
			}
		}

		public StmMenuDocumentConfig CopyDocConfig(StmMenuDocumentConfig docConfig, StmMenuTemplatePivotBase templatePivot)
		{
			StmMenuDocumentConfig result = null;
			if ((docConfig != null) && (templatePivot != null))
			{
				result = templatePivot.DocConfigs.AddNew();
				docConfig.CopyOver(result);

				result.S3_IsSystem = false;
				result.S3_IsTemplate = false;
				result.S3_IsClientSpecific = false;
				foreach (StmMenuDocumentConfigItem configItem in result.ConfigItems)
				{
					configItem.S4_IsSystemDefined = false;
					configItem.S4_IsClientSpecific = false;
				}

				templatePivot.DocConfigs.SortByFallback();
			}
			return result;
		}

		public StmMenuItem CopyConfigToNewDoc(DocumentCommand currentMenuItem, IEnumerable<StmMenuTemplatePivotBase> currentTemplatePivots)
		{
			var newMenu = Menus.AddNew();
			var excludedMenuColumnsFromCopy = new List<string>();

			excludedMenuColumnsFromCopy.Add(StmMenuItemSchema.SU_IsSystemDefined.ObjectName);
			excludedMenuColumnsFromCopy.Add(StmMenuItemSchema.SU_IsClientSpecific.ObjectName);
			excludedMenuColumnsFromCopy.Add(StmMenuItemSchema.SU_IsPublished.ObjectName);
			excludedMenuColumnsFromCopy.Add(StmMenuItemSchema.SU_GS_NKStaffCode.ObjectName);

			newMenu.CopyPersistentValuesFrom(currentMenuItem, new BusinessObjectCloneArgs(excludedMenuColumnsFromCopy));

			var newMenuCopyCount = GetNewMenuCopyCount(newMenu.SU_MenuName);
			ZString newMenuCopyName = newMenu.SU_MenuName + ((newMenuCopyCount == 0) ? (NoResString)" - Copy" : $" - Copy ({newMenuCopyCount + 1})");

			newMenu.SU_MenuName = newMenuCopyName.Truncate(newMenu.SU_MenuNameInfo.MaxLength);

			foreach (var currentTemplatePivot in currentTemplatePivots)
			{
				var newTemplatePivot = AddTemplatePivot(newMenu, currentTemplatePivot.Template);
				newTemplatePivot.SI_DocumentTitle = currentTemplatePivot.SI_DocumentTitle;
				newTemplatePivot.SI_RT_DocType = currentTemplatePivot.SI_RT_DocType;
				newTemplatePivot.SI_PrintByDefault = currentTemplatePivot.SI_PrintByDefault;
				newTemplatePivot.SI_MenuTemplateFilter = currentTemplatePivot.SI_MenuTemplateFilter;
				newTemplatePivot.SI_PrintCopyType = currentTemplatePivot.SI_PrintCopyType;

				foreach (StmMenuDocumentConfig currentDocConfig in currentTemplatePivot.DocConfigs)
				{
					if (currentDocConfig != null)
					{
						var newDocConfig = newTemplatePivot.DocConfigs.AddNew();
						var excludedDocConfigColumnsFromCopy = new List<string>();
						excludedDocConfigColumnsFromCopy.Add(StmMenuDocumentConfigSchema.S3_IsSystem.ObjectName);
						excludedDocConfigColumnsFromCopy.Add(StmMenuDocumentConfigSchema.S3_IsTemplate.ObjectName);
						excludedDocConfigColumnsFromCopy.Add(StmMenuDocumentConfigSchema.S3_IsClientSpecific.ObjectName);
						excludedDocConfigColumnsFromCopy.Add(StmMenuDocumentConfigSchema.S3_OH.ObjectName);
						excludedDocConfigColumnsFromCopy.Add(StmMenuDocumentConfigSchema.S3_GC.ObjectName);

						newDocConfig.CopyPersistentValuesFrom(currentDocConfig, new BusinessObjectCloneArgs(excludedDocConfigColumnsFromCopy));

						newDocConfig.S3_SI = newTemplatePivot.PK;

						var excludedDocConfigitemColumnsFromCopy = new List<string>();
						excludedDocConfigitemColumnsFromCopy.Add(StmMenuDocumentConfigItemSchema.S4_IsSystemDefined.ObjectName);
						excludedDocConfigitemColumnsFromCopy.Add(StmMenuDocumentConfigItemSchema.S4_IsClientSpecific.ObjectName);

						foreach (StmMenuDocumentConfigItem docConfigItem in currentDocConfig.ConfigItems)
						{
							var newDocConfigItem = Factory.New<StmMenuDocumentConfigItem>();
							newDocConfigItem.CopyPersistentValuesFrom(docConfigItem, new BusinessObjectCloneArgs(excludedDocConfigitemColumnsFromCopy));
							newDocConfigItem.S4_S3 = newDocConfig.PK;
						}
					}
				}
			}

			return newMenu;
		}

		int GetNewMenuCopyCount(ZString newMenuName)
		{
			var copiedNameRegex = new Regex(@" - Copy( \([0-9]+\))?$");
			var numberRegex = new Regex(@"[0-9]+");
			var result = 0;
			foreach (StmMenuItem menuItem in Menus)
			{
				if (copiedNameRegex.IsMatch(menuItem.SU_MenuName))
				{
					string originalMenuItemName = copiedNameRegex.Replace(menuItem.SU_MenuName, "");
					if (newMenuName.Equals(originalMenuItemName))
					{
						string copyStringSection = copiedNameRegex.Match(menuItem.SU_MenuName).ToString();
						int menuCopyCount;

						if (copyStringSection != (NoResString)" - Copy")
						{
							menuCopyCount = int.Parse(numberRegex.Match(copyStringSection).ToString());
						}
						else
						{
							menuCopyCount = 1;
						}

						if (menuCopyCount > result)
						{
							result = menuCopyCount;
						}
					}
				}
			}

			return result;
		}

		protected override PrintTask GetPrintTaskCore(StmMenuItemBase menu)
		{
			return new DocumentPrintSet((DocumentCommand)menu, userFieldList);
		}

		protected override StmMenuItemBaseCollection InitialiseMenus()
		{
			return new DocumentCommandCollection(Parent, Factory);
		}

		protected override StmTemplateBaseCollection InitialiseAvailableTemplates()
		{
			return new StmTemplateBaseCollection(Factory, Parent.DocumentSupporter.FilterForSupportedDataContexts);
		}

		public static DocumentMenuCustomisation New(IDocumentSupportable parent, UserControlProviderList userFieldList)
		{
			return New(parent, userFieldList, new BusinessObjectFactory() { NameForDebugging = "DocumentMenuCustomisation.New" });
		}

		public static DocumentMenuCustomisation New(IDocumentSupportable parent, UserControlProviderList userFieldList, BusinessObjectFactory factory)
		{
			var eDocsProvider = parent as IEDocsProvider;
			if (eDocsProvider != null)
			{
				try
				{
					AddEDocsProviderPlaceholders(factory, eDocsProvider);
				}
				catch (ZSaveException)
				{
					AddEDocsProviderPlaceholders(new BusinessObjectFactory(), eDocsProvider);
				}
			}

			return new DocumentMenuCustomisation(factory, parent, userFieldList);
		}

		protected override string ValidateNewTemplate(DataContextValue dataContext)
		{
			string result = null;
			if (dataContext.Equals(DataContextValue.None))
			{
				result = Res.GetString("77a3fe49-1b5a-47ae-be44-9fdad81245ee", "This template cannot be added because it does not have a specified data context.");
			}
			else
			{
				DocumentSupporter documentSupporter = Parent.DocumentSupporter;
				if (!documentSupporter.IsDataContextSupported(dataContext))
				{
					result = Res.GetString("c737b3aa-5ce1-4263-a8a8-a6d19171804b", "This template cannot be added because it requires a data context of '{0}', but the form only supports '{1}'.",
						dataContext.FullDataContext, documentSupporter.CommaSeparatedListOfSupportedDataContexts);
				}
			}
			return result;
		}

		protected override ReturnResult ValidateUpdateTemplateCore(DataContextValue dataContextValue, StmTemplateBase template, ExcelTemplate modifiedTemplate)
		{
			ReturnResult result = new ReturnResult();
			result.Success = true;

			if (template.SO_Name == Core.Constants.SectionRepositoryTemplateNames.User)
			{
				ValidateRemovedCustomizedDocBuilderSections(ref result, modifiedTemplate);
			}

			if (result.Success && dataContextValue.FullDataContext != template.SO_DataContext)
			{
				result.Message = Res.GetString("5765719F-C681-42B8-9843-5C00E63C002F", "The data context of this template is changed from {0} to {1}",
					template.SO_DataContext, dataContextValue.FullDataContext);
			}

			return result;
		}

		void ValidateRemovedCustomizedDocBuilderSections(ref ReturnResult result, ExcelTemplate modifiedTemplate)
		{
			var nonSystemConfigItems = Factory.Load<StmMenuDocumentConfigItem>(new ZQuery(StmMenuDocumentConfigItemSchema.S4_IsSystemDefined, false));
			var systemTemplateSections = TemplateSectionCollection.CacheManager.Get(Factory).SystemTemplateSections.Cast<TemplateSection>().Select(s => s.SectionName).ToHashSet();
			var nonSystemSections = nonSystemConfigItems.Where(c => !systemTemplateSections.Contains(c.S4_SectionItemName)).Select(c => c.S4_SectionItemName).Distinct();

			if (nonSystemSections.Any())
			{
				var newSections = new TemplateSectionCollection(modifiedTemplate).Cast<TemplateSection>().Select(s => s.SectionName);
				var removedSections = nonSystemSections.Where(n => !newSections.Any(s => string.Equals(s, n, StringComparison.OrdinalIgnoreCase))).ToList();

				if (removedSections.Any())
				{
					var firstFiveInvalidRemovedSection = LoadInvalidRemovedSections(removedSections);

					if (firstFiveInvalidRemovedSection.Any())
					{
						result.Success = false;
						var message = Res.GetString("FA1DB7DE-0181-4C03-A889-74AAD984EBD7", "There are Section(s) removed/missing from {0} which are being used in the following DocBuilder document(s). Please remove the section from documents before deleting this section from {0}:", Core.Constants.SectionRepositoryTemplateNames.User);

						foreach (var section in firstFiveInvalidRemovedSection)
						{
							message += ConstructMessageForSection(Factory, section);
						}

						result.Message = message;
					}
				}
			}
		}

		List<string> LoadInvalidRemovedSections(List<ZString> sectionNames)
		{
			var sqlText = FormattableString.Invariant(
$@"SELECT DISTINCT TOP 5 {StmMenuDocumentConfigItemSchema.S4_SectionItemName.Name}
FROM {StmMenuDocumentConfigItemSchema.Constants.SqlSchemaName}.{StmMenuDocumentConfigItemSchema.Constants.TableName}
WHERE {StmMenuDocumentConfigItemSchema.S4_SectionItemName.Name} IN (SELECT Value FROM @SectionNames) ORDER BY {StmMenuDocumentConfigItemSchema.S4_SectionItemName.Name}");

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@SectionNames", sectionNames, StmMenuDocumentConfigItemSchema.S4_SectionItemName, isTableValued: true));
			var firstFiveInvalidRemovedSection = new DynamicBusinessObjectCollection(Factory);
			firstFiveInvalidRemovedSection.Load(sqlText, sqlParams);

			return firstFiveInvalidRemovedSection.Select(s => s[StmMenuDocumentConfigItemSchema.S4_SectionItemName].ToString()).ToList();
		}

		internal static string ConstructMessageForSection(BusinessObjectFactory factory, ZString section)
		{
			ZStringBuilder messageBuilder = new ZStringBuilder();
			ZDBOnlySubQuery stmMenuDocumentConfigItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuDocumentConfigItem), StmMenuDocumentConfigItemSchema.S4_S3);
			stmMenuDocumentConfigItemSubQuery.AddToFilter(StmMenuDocumentConfigItemSchema.S4_SectionItemName, section);

			ZDBOnlySubQuery stmMenuDocumentConfigSubQuery = new ZDBOnlySubQuery(typeof(StmMenuDocumentConfig), StmMenuDocumentConfigSchema.S3_SI);
			stmMenuDocumentConfigSubQuery.AddSubQuery(stmMenuDocumentConfigItemSubQuery, JoinCondition.And);

			ZDBOnlySubQuery stmMenuTemplatePivotSubQuery = new ZDBOnlySubQuery(typeof(StmMenuTemplatePivot), StmMenuTemplatePivotSchema.SI_SU);
			stmMenuTemplatePivotSubQuery.AddSubQuery(stmMenuDocumentConfigSubQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmMenuItem));
			query.AddSubQuery(stmMenuTemplatePivotSubQuery, JoinCondition.And);
			query.MaximumRows = 10;

			var menuItems = factory.Load<StmMenuItem>(query);

			if (menuItems.Any())
			{
				messageBuilder.AppendLine();
				messageBuilder.Append(System.Environment.NewLine + Res.GetString("9A901043-8AA6-4E12-B095-BD8B270437E4", "Documents using section \"{0}\":", section));
				foreach (var menu in menuItems.OrderBy(m => m.SU_MenuName).ThenBy(m => m.SU_MenuPath))
				{
					messageBuilder.Append(System.Environment.NewLine + Res.GetString("292268FA-D794-42FE-9355-30EBE9DF9A2F", "*** Data Context: {0}, Path: {1}, Document Name: \"{2}\"", menu.SU_BusinessContext, menu.SU_MenuPath, menu.SU_MenuName)
						+ (menu.SU_IsPublished ? "" : Res.GetString("2E7E61C3-E5C2-4E71-9672-CB7CB068F134", ", Creating User: {0} (Unpublished)", menu.SU_GS_NKStaffCode)));
				}
			}
			return messageBuilder.ToString();
		}
	}
}
