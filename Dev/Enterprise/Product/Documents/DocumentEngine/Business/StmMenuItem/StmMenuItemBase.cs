using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	[ProvideMetaDataProperty("PropertyInfosReadOnly", MetaDataTypes.ReadOnly)]
	public class StmMenuItemBase : StmMenuItem, ICanDelete
	{
		#region Schema
		public abstract new class Schema : StmMenuItem.Schema
		{
			public const string SU_Calc_IsWebSupportable = "SU_Calc_IsWebSupportable";
		}
		#endregion

		public StmMenuItemBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SU_FilterList = "";
			SU_IsPublished = false;
			SU_MenuShortcut = ObjectFactory.Get<IShortcuts>().NoneShortcut;
			SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;

			EditingMode = EditingMode; // Make sure it's in sync
			SU_DocumentDirection = nameof(DocumentDirection.ANY);
			SU_ContactType = ContactType.NoContactType.Code;
		}

		protected override StmMenuItemValidation GetNewValidation()
		{
			return new StmMenuItemBaseValidation(this);
		}

		#region Properties

		public override ZBool SU_IsSystemDefined
		{
			get { return base.SU_IsSystemDefined; }
			set
			{
				base.SU_IsSystemDefined = value;
				UpdateSU_GS_NKStaffCode();
				SetIsClientSpecificOffIfValueNotTrue(value);
			}
		}

		void SetIsSystemDefined(ZBool value)
		{
			if (!isInAnotherSetter)
			{
				isInAnotherSetter = true;

				try
				{
					SU_IsSystemDefined = value;
				}
				finally
				{
					isInAnotherSetter = false;
				}
			}
		}

		public ZBool SU_Calc_IsWebSupportable
		{
			get
			{
				return SU_MenuType == Core.Constants.StmMenuItemTypes.WebReports;
			}
			set
			{
				bool oldValue = SU_Calc_IsWebSupportable;

				if (value)
				{
					SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
				}
				else
				{
					SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
					SU_IsVisibleOnWeb = false;
				}

				if (!IsValidationSuspended && oldValue != value)
				{
					((StmMenuItemBaseValidation)Validation).ValidateSU_Calc_IsWebSupportable();
				}
				SU_Calc_IsWebSupportableInfo.RefreshBinding();
				SU_MenuTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SU_Calc_IsWebSupportableInfo
		{
			get { return GetZPropertyInfo(Schema.SU_Calc_IsWebSupportable); }
		}

		public override ZBool SU_IsClientSpecific
		{
			get { return base.SU_IsClientSpecific; }
			set
			{
				base.SU_IsClientSpecific = value;
				SetIsSystemDefined(value);
			}
		}

		void SetIsClientSpecificOffIfValueNotTrue(ZBool value)
		{
			if (!isInAnotherSetter && SU_IsClientSpecific)
			{
				isInAnotherSetter = true;

				try
				{
					SU_IsClientSpecific = value;
				}
				finally
				{
					isInAnotherSetter = false;
				}
			}
		}
		bool isInAnotherSetter;

		public override ZBool SU_IsPublished
		{
			get { return base.SU_IsPublished; }
			set
			{
				base.SU_IsPublished = value;
				UpdateSU_GS_NKStaffCode();
			}
		}

		void UpdateSU_GS_NKStaffCode()
		{
			SU_GS_NKStaffCode = (!SU_IsPublished && !SU_IsSystemDefined) ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
		}

		[List("ContactTypeList")]
		public override ZString SU_ContactType
		{
			get { return base.SU_ContactType; }
			set
			{
				base.SU_ContactType = value;
				if (SU_ContactType == ContactType.NoContactType.Code)
				{
					SU_PreventAutoDelivery = true;
				}
			}
		}

		[List("SignByList")]
		public override ZString SU_SignBy
		{
			get => base.SU_SignBy;
			set => base.SU_SignBy = value;
		}

		[List("IncludedDocumentsList")]
		public override ZGuid SU_PrimaryDocPackItemId
		{
			get { return base.SU_PrimaryDocPackItemId; }
			set { base.SU_PrimaryDocPackItemId = value; }
		}

		public bool SU_PrimaryDocPackItem_ReadOnly
		{
			get { return SU_IsSystemDefined || !(SU_IsDocPack && IncludedDocumentsList.Count > 0); }
		}

		public override ZBool SU_IsDocPack
		{
			get { return base.SU_IsDocPack; }
			set
			{
				base.SU_IsDocPack = value;
				if (!value)
				{
					SU_IsZippedDocPack = false;
				}
			}
		}

		public bool IsApplicable
		{
			get
			{
				try
				{
					return IsApplicableCore();
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					if (SU_IsSystemDefined && !ExceptionReporter.Instance.TryHandleWithoutReporting(exception.GetInnermostException()))
					{
						var menuNameWithPath = SU_MenuPath + (SU_MenuPath.IsEmpty || SU_MenuPath.EndsWith("/") ? "" : "/") + SU_MenuName;
						var exMessageWithMoreInfo = exception.Message + "\r\n" + string.Format((NoResString)"BusinessContext=[{0}], MenuName=[{1}], FilterList=[{2}]", SU_BusinessContext, menuNameWithPath, SU_FilterList);

						//Explanation for the bizarre flag switching:
						//I need this to be false during unit testing so that it logs it via the UnitTestUserNotification path.
						//BUT I need the HandleUnhandledException check to happen with it set to true (if the unit test checks it), so that when it fails to handle a specific exception, false is returned.
						//Since I need two contradictory things simultaneously, either I split the flag in two (but based on what criteria? I'm not sure) or change it mid-function. I chose the latter.
#if DEBUG
						var cachedValue = Globals.GetIsUnitTestingProductionFunctionality();
						Globals.SetIsUnitTestingProductionFunctionality(false);
#endif
						Globals.Message.ShowDeveloperExceptionOnce(exMessageWithMoreInfo, exMessageWithMoreInfo, exception);
#if DEBUG
						Globals.SetIsUnitTestingProductionFunctionality(cachedValue);
#endif
					}

					return false;
				}
			}
		}

		public virtual bool IsApplicableCore()
		{
			return true;
		}

		#endregion

		public event EventHandler OnDeleted;

		public override void Delete()
		{
			var message = string.Empty;
			if (IsDocumentMenuUsedInWorkflow(out message) || IsDocumentMenuUsedInOrgDocuments(out message))
			{
				throw new CannotDeleteException(message);
			}
			else
			{
				base.Delete();

				if (IsDeleted)
				{
					while (Documents.Count > 0)
					{
						Documents[0].Delete();
					}

					var pivotFilter = new ZQuery(StmMenuMenuPivotSchema.SF_SU_Outward, PK);
					pivotFilter.AddToFilter(JoinCondition.Or, StmMenuMenuPivotSchema.SF_SU_Inward, SQLComparisonOperator.Equal, PK);
					var menuPivots = Factory.Load<StmMenuMenuPivotBase>(pivotFilter);
					if (menuPivots != null && menuPivots.Length > 0)
					{
						foreach (StmMenuMenuPivotBase pivotBase in menuPivots)
						{
							pivotBase.Delete();
						}
					}

					var securityFilter = new ZQuery(OrgSecuritySchema.OX_SU, PK);
					var securityList = Factory.Load<OrgSecurity>(securityFilter);
					if (securityList != null && securityList.Length > 0)
					{
						foreach (var item in securityList)
						{
							item.Delete();
						}
					}

					var glbSecurityFilter = new ZQuery(GlbSecuritySchema.GU_ItemGUID, PK);
					var glbSecurityList = Factory.Load<GlbSecurity>(glbSecurityFilter);
					if (glbSecurityList != null && glbSecurityList.Length > 0)
					{
						foreach (var item in glbSecurityList)
						{
							item.Delete();
						}
					}

					var overrideFilter = new ZQuery(StmDocDataOverrideSchema.DD_SU, PK);
					var overrideList = Factory.Load<VisualizerNote>(overrideFilter);
					if (overrideList != null && overrideList.Length > 0)
					{
						foreach (var item in overrideList)
						{
							item.Delete();
						}
					}

					EDocs.RemoveAndDeleteAll();

					if (OnDeleted != null)
					{
						OnDeleted(this, EventArgs.Empty);
					}
				}
			}
		}

		bool IsDocumentMenuUsedInOrgDocuments(out string message)
		{
			message = string.Empty;
			var orgDocumentFilter = new ZQuery(OrgDocumentSchema.OD_SU_MenuItem, PK);
			orgDocumentFilter.MaximumRows = 10;
			var orgDocuments = Factory.Load<OrgDocument>(orgDocumentFilter);

			foreach (var orgDocument in orgDocuments)
			{
				var orgContact = Factory.Load<OrgContact>(orgDocument.OD_OC);
				if (orgContact != null)
				{
					message += Res.GetString("2E87BFB9-C456-4D75-BBA9-7035A216E79D", "The Menu Item '{0}' is used in the Organization '{1}' >> Contact '{2}' >> Documents To Receive", SU_MenuName, orgContact.OrganisationCode, orgContact.OC_ContactName) + "\r\n\r\n";
				}
			}

			return !string.IsNullOrEmpty(message);
		}

		bool IsDocumentMenuUsedInWorkflow(out string message)
		{
			message = string.Empty;

			var processTaskNotificationFilter = new ZQuery(ProcessTaskNotificationSchema.PQ_SU_Document, PK);
			processTaskNotificationFilter.MaximumRows = 10;
			var processTaskNotifications = Factory.Load<ProcessTaskNotification>(processTaskNotificationFilter);
			foreach (var processTaskNotification in processTaskNotifications)
			{
				BusinessObject parent = null;
				ZGuid parentPK = ZGuid.Empty;
				var parentTableCode = string.Empty;
				var taskDescription = processTaskNotification.Description;
				if (processTaskNotification.Parent is ProcessTask)
				{
					var processTask = processTaskNotification.Parent as ProcessTask;
					parentPK = processTask.P9_ParentID;
					parentTableCode = processTask.P9_ParentTableCode;
				}
				else if (!processTaskNotification.PQ_P9T_Trigger.IsEmpty)
				{
					var universalTemplateTrigger = Factory.Load<IUniversalTemplateTrigger>(processTaskNotification.PQ_P9T_Trigger);
					parentPK = universalTemplateTrigger.SourceTemplatePK;
					parentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
					taskDescription = Res.GetString("CFDC50B1-EA15-49A5-83CE-99EDFA2B1238", "Universal ") + taskDescription;
				}

				parent = Factory.Load(parentTableCode, parentPK);
				message += Res.GetString("65398A85-A9F6-4662-A0C3-2BD3BD0EA986", "The Menu Item '{0}' is used in '{1}' of {2}", SU_MenuName, taskDescription, parent.HumanReadableName) + "\r\n\r\n";
			}

			return !string.IsNullOrEmpty(message);
		}

		#region Lookups

		public CodeDescriptionPairList MenuShortcutList
		{
			get
			{
				if (menuShortcutList == null)
				{
					menuShortcutList = new UntranslatableCodeDescriptionPairList((NoResString)"Keyboard shortcut descriptions cannot be translated");

					foreach (string name in ObjectFactory.Get<IShortcuts>().AllShortcuts)
					{
						if (name.StartsWith("CtrlShift") && name != "CtrlShiftS" && name != "CtrlShiftD" && name != "CtrlShiftR")
						{
							menuShortcutList.AddPair(name, name);
						}
					}
				}

				return menuShortcutList;
			}
		}
		CodeDescriptionPairList menuShortcutList;

		public CodeDescriptionPairList SignByList => DocumentsSignBy.GetSignByList();

		public CodeDescriptionPairList DocumentDirectionList
		{
			get
			{
				var documentDirectionList = new CodeDescriptionPairList();
				documentDirectionList.AddPair("ARV", ResString.GetMultilingualString("a6e8d6fe-aea9-47c0-a04f-8580ef569df0", "Arrival"));
				documentDirectionList.AddPair("DEP", ResString.GetMultilingualString("620aace0-7afd-4d89-8c03-1f9dbe08a216", "Departure"));
				documentDirectionList.AddPair("ANY", ResString.GetMultilingualString("877b0414-94ac-4feb-a863-092ab52b9815", "Any Direction"));
				return documentDirectionList;
			}
		}

		public CodeDescriptionPairList DeliveryRestrictionTypeList => deliveryRestrictionTypeList ?? (deliveryRestrictionTypeList = DeliveryRestrictionTypeHelper.DeliveryRestrictionTypeList());
		CodeDescriptionPairList deliveryRestrictionTypeList;

		public CodeDescriptionPairList ContactTypeList
		{
			get
			{
				if (contactTypeList == null)
				{
					contactTypeList = OrgCodeLists.ContactType_List;
					contactTypeList.Insert(0, new CodeDescriptionPair(ContactType.NoContactType.Code, Res.GetString("98194225-9ede-4c83-8e8f-b298883a9bf1", "No Doc Group")));
				}
				return contactTypeList;
			}
		}
		CodeDescriptionPairList contactTypeList;

		public CodeDescriptionPairList AddressCategoryList
		{
			get
			{
				if (addressCategoryList == null)
				{
					addressCategoryList = new OrgAddressCategory();
				}
				return addressCategoryList;
			}
		}
		CodeDescriptionPairList addressCategoryList;

		public virtual CodeDescriptionPairList MenuTypeList
		{
			get
			{
				if (menuTypeList == null)
				{
					menuTypeList = new CodeDescriptionPairList();
					menuTypeList.AddPair(Core.Constants.StmMenuItemTypes.Documents, Res.GetString("1d6d1a25-e9f5-48a5-b37e-37fe2abc5394", "{0} Document / Report", Core.Constants.ProductName));
					menuTypeList.AddPair(Core.Constants.StmMenuItemTypes.WebReports, Res.GetString("6fb9a9ec-920a-4998-8b2c-9e7ca9bc8284", "Web Report"));
					menuTypeList.AddPair(Core.Constants.StmMenuItemTypes.Forms, Res.GetString("6ce436b5-dea0-4cdb-8c21-2b1965e5ecd7", "Visualizer Form"));
				}

				return menuTypeList;
			}
		}
		CodeDescriptionPairList menuTypeList;

		public virtual CodeDescriptionPairList SU_MenuDataContextList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList IncludedDocumentsList
		{
			get { return GetIncludedDocumentsList(); }
		}

		protected virtual CodeDescriptionPairList GetIncludedDocumentsList()
		{
			var list = new CodeDescriptionPairList();
			if (SU_IsDocPack)
			{
				list.AddRange(Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, PK)).OrderBy(p => p.SI_Index).ToArray());
				list.AddRange(Factory.Load<StmMenuMenuPivot>(new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, PK)).OrderBy(p => p.SF_Index).ToArray());
			}

			return list;
		}

		#endregion

		#region Related Objects

		[ChildEditable(false)]
		public virtual StmMenuTemplatePivotBaseCollection Documents
		{
			get
			{
				if (documents == null)
				{
					documents = GetNewDocuments();

					if (documents != null)
					{
						documents.Load();

						RegisterEditableChildObject(documents);
						documents.SetReadOnlyIncludingChildren(false);

						documents.EditingMode = EditingMode;

						documents.CollectionChanged += (s, e) =>
						{
							RefreshEmailSubjectAndMenuDataContextBinding();
						};
					}
				}

				if (documents != null)
				{
					documents.Sort(new SortInfo(StmMenuTemplatePivotBase.Schema.SI_Index, System.ComponentModel.ListSortDirection.Ascending));
				}

				return documents;
			}
		}

		protected virtual StmMenuTemplatePivotBaseCollection GetNewDocuments()
		{
			return new StmMenuTemplatePivotBaseCollection(Factory,
				new ZQuery(StmMenuTemplatePivotSchema.SI_SU, PK));
		}

		protected StmMenuTemplatePivotBaseCollection documents;

		protected override ICollection DocumentsCore => Documents;

		[ChildEditable(false)]
		public StmMenuMenuPivotBaseCollection ChildMenus
		{
			get
			{
				if (childMenus == null)
				{
					var lchildMenus = new StmMenuMenuPivotBaseCollection(Factory);
					lchildMenus.Load(new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, PK));
					childMenus = lchildMenus;
					childMenus.EditingMode = EditingMode;
					childMenus.Sort(new SortInfo(StmMenuMenuPivot.Schema.SF_Index, System.ComponentModel.ListSortDirection.Ascending));
					RegisterEditableChildObject(childMenus);
					childMenus.CollectionChanged += (s, e) =>
					{
						RefreshEmailSubjectAndMenuDataContextBinding();
					};

					Factory.AddFetchHint(typeof(DocumentCommand), new ZQuery(StmMenuItemSchema.PK, childMenus.Cast<StmMenuMenuPivotBase>().Select(menu => menu.SF_SU_Outward)));
					Factory.AddFetchHint(typeof(StmMenuTemplatePivot), new ZQuery(StmMenuTemplatePivotSchema.SI_SU, childMenus.Cast<StmMenuMenuPivotBase>().Select(menu => menu.SF_SU_Outward)));
				}

				return childMenus;
			}
		}
		StmMenuMenuPivotBaseCollection childMenus;

		protected override ICollection ChildMenusCore => ChildMenus;

		void RefreshEmailSubjectAndMenuDataContextBinding()
		{
			SU_MenuDataContextInfo.RefreshBinding();
			SU_EmailSubjectLineInfo.RefreshBinding();
		}

		[ChildEditable(true)]
		public DocumentStmMenuEDocsDependentCollection EDocs
		{
			get
			{
				if (eDocs == null)
				{
					eDocs = new DocumentStmMenuEDocsDependentCollection(this, Factory);
					ZQuery query = new ZQuery(StmMenuEDocsSchema.SX_SU, PK);
					eDocs.Load(query);
					RegisterEditableChildObject(eDocs);
				}
				return eDocs;
			}
		}
		DocumentStmMenuEDocsDependentCollection eDocs;

		public DocumentStmMenuEDocsDependentCollectionView EDocsView
		{
			get
			{
				if (eDocsView == null)
				{
					eDocsView = new DocumentStmMenuEDocsDependentCollectionView(EDocs);
				}
				eDocsView?.Sort(new SortInfo(StmMenuEDocsSchema.Constants.SX_Index, ListSortDirection.Ascending));
				return eDocsView;
			}
		}
		DocumentStmMenuEDocsDependentCollectionView eDocsView;

		#endregion

		#region Editing Mode / ReadOnly

		public virtual MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;
				if (documents != null)
				{
					Documents.EditingMode = value;
				}
				RefreshBinding();
			}
		}
		MenuEditingMode editingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		protected virtual bool GetPropertyInfosReadOnly(PropertyDescriptor property)
		{
			bool result = false;
			string propertyName = property != null ? property.Name : string.Empty;

			switch (propertyName)
			{
				case Schema.SU_PreventAutoDelivery:
				case Schema.SU_IsVisibleOnWeb:
				case Schema.SU_IsLocalDocument:
				case Schema.SU_IsPublished:
				case Schema.SU_SignBy:
				case Schema.SU_IsZippedDocPack:
				case Schema.SU_MenuIndex:
				case Schema.SU_DefaultAttachmentType:
				case Schema.SU_MustRunOnline:
					break;
				default:
					result = GetReadOnlynessDependingOnEditingMode(propertyName);
					break;
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		bool GetReadOnlynessDependingOnEditingMode(string propertyName)
		{
			bool result = false;

			switch (EditingMode)
			{
				case MenuEditingMode.AllowEditingOfClientSpecificOnly:
					result =
						SU_IsSystemDefinedInfo.Name == propertyName ||
						SU_IsSystemDefined && !SU_IsClientSpecific;
					break;

				case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
					result =
						SU_IsClientSpecificInfo.Name == propertyName ||
						SU_IsSystemDefined && SU_IsClientSpecific;
					break;

				case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
					result =
						SU_IsSystemDefinedInfo.Name == propertyName ||
						SU_IsClientSpecificInfo.Name == propertyName ||
						SU_MenuTypeInfo.Name == propertyName ||
						SU_IsSystemDefined || SU_IsClientSpecific;
					break;
			}
			return result;
		}

		protected bool SU_IsPublished_ReadOnly
		{
			get { return !GlbStaff.CurrentUser.GS_IsController && GetPropertyInfosReadOnly(null); }
		}

		protected bool SU_PreventAutoDelivery_ReadOnly
		{
			get { return SU_ContactType == "NCT"; }
		}

		protected bool SU_IsZippedDocPack_ReadOnly
		{
			get { return !SU_IsDocPack; }
		}

		#endregion
	}
}
