using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Business
{
	[ProvideMetaDataProperty("PropertyInfosReadOnly", MetaDataTypes.ReadOnly)]
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class StmMenuTemplatePivotBase : StmMenuTemplatePivot, IStmMenuDocumentConfigSource
	{
		public new class Schema : StmMenuTemplatePivot.Schema
		{
			public const string SO_Name = "SO_Name";
		}

		public StmMenuTemplatePivotBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		#region DocManagerFilter

		public ZString DocManagerFilter
		{
			get { return fDocManagerFilter; }
			set
			{
				fDocManagerFilter = value;
				fDocTypeList = null;
			}
		}

		ZString fDocManagerFilter;

		#endregion

		#region Editing Mode

		public MenuEditingMode EditingMode
		{
			get;
			set;
		}

		#region ReadOnly

		protected bool GetPropertyInfosReadOnly(PropertyDescriptor property)
		{
			bool result;
			string propertyName = property != null ? property.Name : string.Empty;

			switch (propertyName)
			{
				case Schema.SI_IsClientSpecific:
					result = GetDefaultPropertyInfosReadOnly() || EditingMode == MenuEditingMode.AllowEditingOfSystemDefinedOnly || EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
					break;
				case Schema.SI_IsSystemDefined:
					result = GetDefaultPropertyInfosReadOnly() || EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus || EditingMode == MenuEditingMode.AllowEditingOfClientSpecificOnly;
					break;
				case Schema.SI_PrintByDefault:
				case Schema.SI_IsPasswordProtected:
				case Schema.SI_IsPasswordProtectedForOpening:
					result = false;
					break;
				default:
					result = GetDefaultPropertyInfosReadOnly();
					break;
			}

			return result;
		}

		bool GetDefaultPropertyInfosReadOnly()
		{
			bool result;

			switch (EditingMode)
			{
				case MenuEditingMode.AllowEditingOfClientSpecificOnly:
					result = (SI_IsSystemDefined && !SI_IsClientSpecific);
					break;

				case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
					result = (SI_IsSystemDefined && SI_IsClientSpecific);
					break;

				case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
					result = (SI_IsSystemDefined || SI_IsClientSpecific);
					break;

				case MenuEditingMode.AllowAll:
				default:
					result = false;
					break;
			}

			return result;
		}

		#endregion

		#endregion

		#region SystemDefined and ClientSpecific

		public override ZBool SI_IsSystemDefined
		{
			get
			{
				return base.SI_IsSystemDefined;
			}
			set
			{
				base.SI_IsSystemDefined = value;

				if (!fIsInAnotherSetter && SI_IsClientSpecific)
				{
					fIsInAnotherSetter = true;

					try
					{
						SI_IsClientSpecific = value;
					}
					finally
					{
						fIsInAnotherSetter = false;
					}
				}
			}
		}

		public override ZBool SI_IsClientSpecific
		{
			get
			{
				return base.SI_IsClientSpecific;
			}
			set
			{
				base.SI_IsClientSpecific = value;

				if (!fIsInAnotherSetter)
				{
					fIsInAnotherSetter = true;

					try
					{
						SI_IsSystemDefined = value;
					}
					finally
					{
						fIsInAnotherSetter = false;
					}
				}
			}
		}

		bool fIsInAnotherSetter;
		#endregion

		#region Template

		StmTemplateBase fTemplate;
		public new StmTemplateBase Template
		{
			get
			{
				if (!SI_SO.IsValid)
				{
					fTemplate = null;
				}
				else if (fTemplate == null || SI_SO != fTemplate.PK)
				{
					fTemplate = (StmTemplateBase)Factory.Load(typeof(StmTemplateBase), SI_SO);
				}

				return fTemplate;
			}
		}

		#endregion

		#region Menu

		StmMenuItemBase fMenu;
		public StmMenuItemBase Menu
		{
			get
			{
				if (!SI_SU.IsValid)
				{
					fMenu = null;
				}
				else if (fMenu == null || SI_SU != fMenu.PK)
				{
					fMenu = (StmMenuItemBase)Factory.Load(typeof(StmMenuItemBase), SI_SU);
				}

				return fMenu;
			}
		}

		#endregion

		#region Validation

		public new StmMenuTemplatePivotBaseValidation Validation
		{
			get { return (StmMenuTemplatePivotBaseValidation)base.Validation; }
		}

		protected override StmMenuTemplatePivotValidation GetNewValidation()
		{
			return new StmMenuTemplatePivotBaseValidation(this);
		}

		#endregion

		[ChildEditable(true)]
		public StmMenuDocumentConfigDependentCollection DocConfigs
		{
			get
			{
				if (fDocConfigs == null)
				{
					fDocConfigs = new StmMenuDocumentConfigDependentCollection(this);
					fDocConfigs.Load();
					fDocConfigs.SortByFallback();
					RegisterEditableChildObject(fDocConfigs);
					fDocConfigs.SetReadOnlyIncludingChildren(false);
					fDocConfigs.EditingMode = EditingMode;
				}
				return fDocConfigs;
			}
		}
		StmMenuDocumentConfigDependentCollection fDocConfigs;

		public RefDocTypeCollection DocTypeList
		{
			get
			{
				if (fDocTypeList == null)
				{
					if (!DocManagerFilter.IsEmpty)
					{
						fDocTypeList = new RefDocTypeFindboxCollection(Factory, DocManagerFilter);
					}
					else
					{
						fDocTypeList = new RefDocTypeCollection(Factory);
					}
				}
				return fDocTypeList;
			}
		}

		RefDocTypeCollection fDocTypeList;

		public CodeDescriptionPairList PrintCopyTypeList
		{
			get
			{
				if (fPrintCopyTypeList == null)
				{
					fPrintCopyTypeList = new CodeDescriptionPairList(OLookUpEditType.PrintCopyType);
					fPrintCopyTypeList.Insert(0, new CodeDescriptionPair("", ""));
				}

				return fPrintCopyTypeList;
			}
		}
		CodeDescriptionPairList fPrintCopyTypeList;

		public override void Delete()
		{
			base.Delete();
			foreach (StmMenuDocumentConfig docConfig in DocConfigs)
			{
				docConfig.Delete();
			}
		}

		#region SO_Name

		public event EventHandler SO_NameChanged
		{
			add { Template.SO_NameInfo.ValueChanged += value; }
			remove { Template.SO_NameInfo.ValueChanged -= value; }
		}

		public ZString SO_Name
		{
			get { return (Template != null ? Template.SO_Name : ZString.Empty); }
		}

		public ZPropertyInfo SO_NameInfo
		{
			get { return GetZPropertyInfo(Schema.SO_Name); }
		}

		#endregion

		#region ICanDelete Members

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("4d4721ce-18c4-4ebb-be3a-da3d481d5199", "Selected item belongs to a System Defined document or report, and cannot be deleted by users.");
			}
		}

		public override bool CanDelete
		{
			get
			{
				return !GetDefaultPropertyInfosReadOnly();
			}
		}

		#endregion

		#region IStmMenuDocumentConfigSource Members

		StmMenuTemplatePivot IStmMenuDocumentConfigSource.MenuTemplatePivot
		{
			get { return this; }
		}

		StmMenuDocumentConfig IStmMenuDocumentConfigSource.GetPersistentDocConfig()
		{
			return DocConfigs.AddNew();
		}

		TemporaryStmMenuDocumentConfig IStmMenuDocumentConfigSource.GetTemporaryDocConfig(BusinessObjectFactory factory)
		{
			return factory.New<TemporaryStmMenuDocumentConfig>();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new StmMenuTemplatePivotBaseStrategy(this);
		}

		#endregion
	}
}
