using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	public class StmTemplateBase : StmTemplate, IDocManagerSupport
	{
		public StmTemplateBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Business Object Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region ExcelTemplateFullPath
#if DEBUG
		public string ExcelTemplateFullPath
		{
			get { return CargoWise.BuildTools.BuildConstants.GetLocalPath(this.SO_ExcelTemplatePath); }
			set
			{
				string localEnterprisePath = CargoWise.BuildTools.BuildConstants.LocalEnterprisePath;
				if (!value.StartsWith(localEnterprisePath, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException("SO_ExcelTemplateFullPath must start with the current source directory (" + localEnterprisePath + ").");
				}
				SO_ExcelTemplatePath = value.Substring(localEnterprisePath.Length);
			}
		}
#endif
		#endregion

		public bool CanModifyExcelTemplate
		{
			get { return !ReadOnly || IsCheckedOutByMe || SO_Name.EqualsIgnoringCase(SectionRepositoryTemplateNames.User); }
		}

		internal MenuEditingMode EditingMode
		{
			get { return fEditingMode; }
			set
			{
				fEditingMode = value;

				switch (value)
				{
					case MenuEditingMode.AllowEditingOfClientSpecificOnly:
						ReadOnly = (SO_IsSystemDefined && !SO_IsClientSpecific);
						break;

					case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
						ReadOnly = (SO_IsSystemDefined && SO_IsClientSpecific);
						break;

					case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
						ReadOnly = (SO_IsSystemDefined || SO_IsClientSpecific);
						break;

					case MenuEditingMode.AllowAll:
					default:
						ReadOnly = false;
						break;
				}
			}
		}

		protected bool SO_IsSystemDefined_ReadOnly
		{
			get
			{
				return EditingMode == MenuEditingMode.AllowEditingOfClientSpecificOnly ||
						EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus ||
						(SO_IsUserConfigurable && SectionRepositoryTemplateNames.NameChecker.IsMatch(SO_Name));
			}
		}

		protected bool SO_IsClientSpecific_ReadOnly
		{
			get
			{
				return EditingMode == MenuEditingMode.AllowEditingOfSystemDefinedOnly ||
						EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			}
		}

		[BusinessObjectTestExclude]
		public override ZBlob SO_Template
		{
			get { return base.SO_Template; }
			set
			{
				base.SO_Template = value;
				if (!value.IsEmpty)
				{
					ExcelTemplate template = new ExcelTemplateReadFromStmTemplateTable(this);
					SectionRepository repository = new SectionRepository(template);
					SO_IsUserConfigurable = repository.IsTemplateCustomisable;
					if (!SO_IsUserConfigurable)
					{
						SO_UDFFieldCache = GetUpToDateUDFFieldCacheValue();
					}
				}

				InvalidateTemplateSections();
			}
		}

		public virtual ZBlob GetUpToDateUDFFieldCacheValue()
		{
			ZBlob uDFCacheBlob = new ZBlob();
			if (SO_Name != "" && !SO_Template.IsEmpty)
			{
				ExcelTemplateReadFromStmTemplateTable exlTemplate = new ExcelTemplateReadFromStmTemplateTable(this);
				using (var pack = new DocumentPack())
				using (Report tempReport = new Report(pack, exlTemplate))
				{
					tempReport.PrepareForRender();
					UserDefinedFieldCollectionBuilder uDFBuilder = new UserDefinedFieldCollectionBuilder(tempReport.UDFSheet, tempReport.Analyser.ValidatorPack, tempReport.ReplaceSingleMacroNotInTemplateBody);
					uDFCacheBlob = StringTreeNode.Serialise(uDFBuilder.RootNode);
				}
			}
			return uDFCacheBlob;
		}

		#region SystemDefined and ClientSpecific

		public override ZBool SO_IsSystemDefined
		{
			get { return base.SO_IsSystemDefined; }
			set
			{
				base.SO_IsSystemDefined = value;

				if (!fIsInAnotherSetter && SO_IsClientSpecific)
				{
					fIsInAnotherSetter = true;

					try
					{
						SO_IsClientSpecific = value;
					}
					finally
					{
						fIsInAnotherSetter = false;
					}
				}
			}
		}

		public override ZBool SO_IsClientSpecific
		{
			get { return base.SO_IsClientSpecific; }
			set
			{
				base.SO_IsClientSpecific = value;

				if (!fIsInAnotherSetter)
				{
					fIsInAnotherSetter = true;

					try
					{
						SO_IsSystemDefined = value;
					}
					finally
					{
						fIsInAnotherSetter = false;
					}
				}
			}
		}

		#endregion

		public static ZBlob GetTemplateBlobFromFile(string filename)
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(filename))
				{
					using (BinaryReader reader = new BinaryReader(fileStream))
					{
						long fileSize = new FileInfo(filename).Length;
						if (fileSize > int.MaxValue)
						{
							throw new TemplateFileReadException("File is too large", null);
						}

						return new ZBlob(reader.ReadBytes((int)fileSize));
					}
				}
			}
			catch (IOException e)
			{
				throw new TemplateFileReadException("Error opening " + filename + "\n\n Please make sure the file is not currently being edited by another application and try again.", e);
			}
		}

		public virtual bool IsCheckedOutByMe
		{
			get
			{
				if (!HasSetIsCheckedOutByMe)
				{
					bool result = false;
#if DEBUG
					if (!SO_ExcelTemplatePath.IsEmpty)
					{
						result = CargoWise.BuildTools.SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(ExcelTemplateFullPath);
					}
#endif
					IsCheckedOutByMe = result;
				}

				return fIsCheckedOutByMe;
			}
			set
			{
				fIsCheckedOutByMe = value;
				HasSetIsCheckedOutByMe = true;
			}
		}

		bool HasSetIsCheckedOutByMe;

		public StmTemplateBase NewCopy()
		{
			StmTemplateBase result = Clone() as StmTemplateBase;
			ZString newName = (NoResString)"Copy of " + SO_Name;
			result.SO_Name = newName.Left(result.SO_NameInfo.MaxLength);
			result.SO_IsSystemDefined = false;
			result.ReadOnly = false;
			return result;
		}

		public override void Delete()
		{
			DeleteStorageMain();
			var pivots = GetPivotsUsingThisTemplate();
			if (pivots.Length == 0)
			{
				base.Delete();
			}
			else
			{
				throw new TemplateInUseException(GetTemplatesInUseMessage(this, pivots));
			}
		}

		static MultilingualString GetTemplatesInUseMessage(StmTemplateBase template, StmMenuTemplatePivot[] pivots)
		{
			var menuItems = new StringBuilder();
			var currentUser = GlbStaff.CurrentUser;
			var hasDeletedMenuItem = false;

			foreach (var pivot in pivots)
			{
				var menuItem = pivot.MenuItem;
				if (menuItem != null)
				{
					menuItems.Append(string.Format("   - {0} ({1})", menuItem.SU_MenuName, menuItem.SU_BusinessContext));
					var isPrivate = !menuItem.SU_GS_NKStaffCode.IsEmpty && menuItem.SU_GS_NKStaffCode != currentUser.GS_Code;
					if (isPrivate)
					{
						menuItems.Append("  " + ResString.GetMultilingualString("2094115f-12b1-401e-83fe-7129ad3ba6ff", "<Private>  <Staff: {0}>", menuItem.SU_GS_NKStaffCode));
					}

					menuItems.AppendLine();
				}
				else
				{
					hasDeletedMenuItem = true;
					break;
				}
			}

			if (hasDeletedMenuItem)
			{
				return ResString.GetMultilingualString("a8701a15-dbcc-45f7-aa1c-46de5512f927", "The template '{0}' is referenced by a document which has just been deleted by another user.\r\nPlease save your current work and retry to delete this template.", template.SO_Name);
			}

			return ResString.GetMultilingualString("e7f8597c-1a08-4b66-9624-96c45620805b",
@"Template '{0}' cannot be deleted because it is in use by the following document menus:
{1}
You need to remove this template from that document first before you can delete this template.

Private document menus may only be seen by the staff member that created it.",
					template.SO_Name, menuItems.ToString());
		}

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				if (IsDefaultLanguageClientSpecificDocBuilderStyle)
				{
					return CanDeleteCustomizedDocBuilderTemplate();
				}
				return !ReadOnly && GetPivotsUsingThisTemplate().Length == 0;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete => ReadOnly
			? ResString.GetMultilingualString("25eadcaa-e150-4850-b116-d78091fdb0f7", "Selected template is System Defined, and cannot be deleted by users.")
			: (IsDefaultLanguageClientSpecificDocBuilderStyle
				? ResString.GetMultilingualString("25C3450D-D367-4346-96E8-3D1198B4DB68", "Cannot delete template {0} because there are customized section(s) being used in documents.\r\n\r\nExamples:{1}", Core.Constants.SectionRepositoryTemplateNames.User, sectionsUsedDetails)
				: GetTemplatesInUseMessage(this, GetPivotsUsingThisTemplate()));

		string sectionsUsedDetails;
		const int NumberOfSectionsToShowInErrorMessage = 5;

		bool CanDeleteCustomizedDocBuilderTemplate()
		{
			sectionsUsedDetails = string.Empty;
			var nonSystemConfigItems = Factory.Load<StmMenuDocumentConfigItem>(new ZQuery(StmMenuDocumentConfigItemSchema.S4_IsSystemDefined, false));
			var nonSystemSections = nonSystemConfigItems.Where(c => TemplateSectionCollection.CacheManager.Get(Factory).SystemTemplateSections.Cast<TemplateSection>().All(s => s.SectionName != c.S4_SectionItemName));

			if (DocBuilderLanguageCode == Enterprise.Core.SharedConstants.Languages.English && nonSystemSections.Any())
			{
				var firstFiveUsedSections = nonSystemSections.Take(NumberOfSectionsToShowInErrorMessage);
				foreach (var section in firstFiveUsedSections)
				{
					sectionsUsedDetails += DocumentMenuCustomisation.ConstructMessageForSection(Factory, section.S4_SectionItemName);
				}

				return false;
			}

			return true;
		}

		#endregion

		#region Implementation

		bool fIsCheckedOutByMe;
		bool fIsInAnotherSetter;
		MenuEditingMode fEditingMode = MenuEditingMode.AllowAll;

		protected override void DeleteForDataRefresh()
		{
			base.Delete(true);
		}

		StmMenuTemplatePivot[] GetPivotsUsingThisTemplate()
		{
			ZQuery filter = new ZQuery(StmMenuTemplatePivotSchema.SI_SO, PK);
			return (StmMenuTemplatePivot[])Factory.Load(typeof(StmMenuTemplatePivot), filter);
		}

		#endregion

		protected override StmTemplateValidation GetNewValidation()
		{
			return new StmTemplateBaseValidation(this);
		}

		public ExcelTemplate GetExcelTemplate()
		{
			return new ExcelTemplateReadFromStmTemplateTable(this);
		}

		public TemplateSectionCollection TemplateSections
		{
			get { return templateSections ?? (templateSections = new TemplateSectionCollection(IsDocBuilderStyle ? GetExcelTemplate() : null)); }
		}
		TemplateSectionCollection templateSections;

		public TemplateSectionCollection TemplateConfigurableOnlySections
		{
			get { return templateConfigurableOnlySections ?? (templateConfigurableOnlySections = new TemplateSectionCollection(IsDocBuilderStyle ? GetExcelTemplate() : null, true)); }
		}
		TemplateSectionCollection templateConfigurableOnlySections;

		internal void InvalidateTemplateSections()
		{
			templateSections = null;
			TemplateSectionCollection.CacheManager.Get(Factory).InvalidateTemplateSectionsCache();
		}

		public static StmTemplateBase GetDocBuilderTemplate(BusinessObjectFactory factory, DocBuilderTemplateType templateType)
		{
			return GetDocBuilderTemplate(factory, templateType, Enterprise.Core.Constants.Languages.English);
		}

		public static StmTemplateBase GetDocBuilderTemplate(BusinessObjectFactory factory, DocBuilderTemplateType templateType, ZString language)
		{
			var name = ZString.Empty;

			switch (templateType)
			{
				case DocBuilderTemplateType.System:
					name = GetDocBuilderTemplateNameWithLanguage(SectionRepositoryTemplateNames.System, language);
					break;

				case DocBuilderTemplateType.Customized:
					name = GetDocBuilderTemplateNameWithLanguage(SectionRepositoryTemplateNames.User, language);
					break;
			}

			var query = new ZQuery(StmTemplateSchema.SO_Name, name);

			return factory.LoadTop1<StmTemplateBase>(query);
		}

		static ZString GetDocBuilderTemplateNameWithLanguage(ZString templateName, ZString language)
		{
			var result = templateName;

			if (!language.IsEmpty && !language.EqualsIgnoringCase(Enterprise.Core.Constants.Languages.English))
			{
				result = SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(result, language);
			}

			return result;
		}

		public ZString DocBuilderLanguageCode
		{
			get
			{
				var result = string.Empty;

				if (IsDocBuilderStyle)
				{
					result = SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName(SO_Name);
				}
				return result;
			}
		}

		[BusinessObjectTestExclude]
		public StmTemplateHistoryCollectionView TemplateHistories
		{
			get
			{
				if (templateHistories == null && DocumentManagerInfo.EDocsView is BusinessObjectCollection eDocsView)
				{
					templateHistories = new StmTemplateHistoryCollectionView(eDocsView);
					templateHistories.Sort(TemplateHistorySortColumn, ListSortDirection.Descending);
				}

				return templateHistories;
			}
		}

		const string TemplateHistorySortColumn = "SC_SystemLastEditTimeUtc";

		StmTemplateHistoryCollectionView templateHistories;

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ??= new DocManagerInfo(this, Core.Constants.DocManagerCodes.DocTemplateRecord);
		DocManagerInfo docManagerInfo;

		public override void OnSaving()
		{
			if (SO_TemplateInfo.IsPersistent && SO_TemplateInfo.HasChanges && SO_TemplateInfo.OriginalValue != null && !SO_TemplateInfo.OriginalValue.IsEmpty)
			{
				DocumentManagerInfo.AddFileOrDocument((ZBlob)SO_TemplateInfo.OriginalValue, GetEDocFileName(), Core.Constants.RefDocTypes.MiscellaneousDocument);
			}
			base.OnSaving();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Excel file extension")]
		string GetEDocFileName()
		{
			var extension = string.IsNullOrEmpty(SO_ExcelTemplatePath) ? ".xls" : Path.GetExtension(SO_ExcelTemplatePath);
			return SO_Name + extension;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				DeleteRedundantDocumentsInEDoc();
				DocumentManagerInfo.Save();
			}
		}

		void DeleteRedundantDocumentsInEDoc()
		{
			var storageFiles = DocumentManagerInfo.EDocsView.OfType<IStorageFile>().ToList();
			var filesCount = storageFiles.Count - DocumentsDataRegistry.Instance.StoredNumberOfSupersededTemplates.Value;
			if (filesCount > 0)
			{
				for (var i = filesCount - 1; i >= 0; i--)
				{
					storageFiles[i].Delete();
				}
			}
		}

		DocManagerInfo DocumentManagerInfo =>  ((IDocManagerSupport)this).DocManagerInfo;

		void DeleteStorageMain()
		{
			var docFactory = DocumentManagerInfo.MasterFactory;
			var storageMain = docFactory.GetStorageMainForPK(PK) as BusinessObject;
			if (storageMain != null)
			{
				storageMain.Delete();
			}
		}
	}
}
