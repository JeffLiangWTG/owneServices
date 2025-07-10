using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfig : AutoStmMenuDocumentConfig, IStmMenuDocumentConfigSource, IDocumentConfig, ICanDelete, IStmMenuDocumentConfig
	{
		public StmMenuDocumentConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S3_PageStyle = DocumentConfigPageStyleList.Codes.Portrait;
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			S3_Description = "notblank";
		}
#endif

		#region OverrideDataContexts

		CodeDescriptionPairList overrideDataContextsList;
		public CodeDescriptionPairList OverrideDataContextsList
		{
			get { return overrideDataContextsList ?? (overrideDataContextsList = GetOverrideDataContextsList()); }
		}

		CodeDescriptionPairList GetOverrideDataContextsList()
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Data context names are not translatable");
			var genericDataContextsList = GetGenericDataContextsList();

			genericDataContextsList.Sort((x, y) => x.CompareTo(y));

			result.AddPair(string.Empty);
			foreach (var dataContext in genericDataContextsList)
			{
				result.AddPair(dataContext);
			}
			return result;
		}

		List<string> GetGenericDataContextsList()
		{
			var result = new List<string>();
			foreach (DataContext dataContext in Enum.GetValues(typeof(DataContext)))
			{
				if (dataContext.ToString().StartsWith((NoResString)"Generic", StringComparison.InvariantCulture))
				{
					result.Add(dataContext.ToString());
				}
			}
			return result;
		}

		#endregion

		#region Related Business Objects

		StmMenuDocumentConfigItemCollection configItems;
		[ChildEditable(true)]
		public StmMenuDocumentConfigItemCollection ConfigItems
		{
			get
			{
				if (configItems == null)
				{
					configItems = new StmMenuDocumentConfigItemCollection(this, Factory);
					configItems.Load();
					RegisterAndSortConfigItems();
					configItems.EditingMode = EditingMode;
				}

				return configItems;
			}
		}

		/// <summary>
		/// Indicates whether this document config is created by ediEnterprise but not specific to a client.
		/// </summary>
		public bool IsSystemDefined
		{
			get { return S3_IsSystem && S3_OH.IsEmpty; }
		}

		/// <summary>
		/// Indicates whether this document config is created by ediEnterprise and specific to a client.
		/// </summary>
		public bool IsClientSpecific
		{
			get { return S3_IsSystem && !S3_OH.IsEmpty; }
		}

		/// <summary>
		/// Indicates whether this document config is created by the user.
		/// </summary>
		public bool IsUserDefined
		{
			get { return !S3_IsSystem; }
		}

		MenuEditingMode editingMode;
		public MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;
				switch (editingMode)
				{
					case MenuEditingMode.AllowEditingOfClientSpecificOnly:
						ReadOnly = !IsUserDefined && !IsClientSpecific;
						break;

					case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
						ReadOnly = !IsUserDefined && !IsSystemDefined;
						break;

					case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
						ReadOnly = !IsUserDefined;
						break;

					default:
						ReadOnly = false;
						break;
				}
			}
		}

		public override StmMenuTemplatePivot MenuTemplatePivot
		{
			get { return Factory.Load<StmMenuTemplatePivotBase>(S3_SI); }
		}

		protected virtual StmMenuDocumentConfigItem GetConfigItemCopy(StmMenuDocumentConfig docConfigToCopyTo, StmMenuDocumentConfigItem configItemToCopyFrom)
		{
			return docConfigToCopyTo.ConfigItems.AddNew();
		}

		public StmMenuDocumentConfig[] GetRelatedDocConfigs()
		{
			return GetRelatedDocConfigsCore();
		}

		protected virtual StmMenuDocumentConfig[] GetRelatedDocConfigsCore()
		{
			List<StmMenuDocumentConfig> result = new List<StmMenuDocumentConfig>();
			foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				if (typeof(StmMenuDocumentConfig).IsAssignableFrom(parentCollection.TypeOfElements))
				{
					StmMenuDocumentConfig[] relatedDocConfigs = (StmMenuDocumentConfig[])parentCollection.Find(new ZQuery(StmMenuDocumentConfigSchema.PK, SQLComparisonOperator.NotEqual, PK));
					result.AddRange(relatedDocConfigs);
				}
			}
			return result.ToArray();
		}

		void RegisterAndSortConfigItems()
		{
			RegisterEditableChildObject(configItems);
			configItems.SortByPrintOrder();
		}

		#endregion

		#region Copy

		public void CopyOver(StmMenuDocumentConfig docConfig)
		{
			docConfig.CopyPersistentValuesFrom(this);
			foreach (StmMenuDocumentConfigItem configItem in ConfigItems)
			{
				StmMenuDocumentConfigItem configItemCopy = GetConfigItemCopy(docConfig, configItem);
				configItemCopy.CopyPersistentValuesFrom(configItem);
				configItemCopy.S4_S3 = docConfig.PK;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			foreach (StmMenuDocumentConfigItem configItem in ConfigItems)
			{
				configItem.Delete();
			}
		}

		#endregion

		#region Properties

		[List("Lookups.AvailablePageStyles")]
		public override ZString S3_PageStyle
		{
			get { return base.S3_PageStyle; }
			set { base.S3_PageStyle = value; }
		}

		[ExcludeMetaDataMember(MetaDataTypeId = MetaDataTypes.ReadOnly)]
		public override ZBool S3_ExcludedFromDocPack
		{
			get { return base.S3_ExcludedFromDocPack; }
			set { base.S3_ExcludedFromDocPack = value; }
		}

		#endregion

		#region IStmMenuDocumentConfigSource Members

		MenuEditingMode IStmMenuDocumentConfigSource.EditingMode
		{
			get { return ((StmMenuTemplatePivotBase)MenuTemplatePivot).EditingMode; }
		}

		StmMenuDocumentConfig IStmMenuDocumentConfigSource.GetPersistentDocConfig()
		{
			return this;
		}

		TemporaryStmMenuDocumentConfig IStmMenuDocumentConfigSource.GetTemporaryDocConfig(BusinessObjectFactory factory)
		{
			TemporaryStmMenuDocumentConfig result = (TemporaryStmMenuDocumentConfig)factory.ImportFromAnotherFactory(this, typeof(TemporaryStmMenuDocumentConfig));
			StmMenuDocumentConfigItemCollection configItemsForResult = new StmMenuDocumentConfigItemCollection(result, factory);
			foreach (StmMenuDocumentConfigItem configItem in ConfigItems)
			{
				configItemsForResult.Add(factory.ImportFromAnotherFactory(configItem));
			}
			result.configItems = configItemsForResult;
			result.RegisterAndSortConfigItems();
			result.ReadOnly = ReadOnly;

			return result;
		}

		#endregion

		internal TemplateSectionCollection TemplateSections
		{
			get { return TemplateSectionCollection.CacheManager.Get(Factory).GetCollection(); }
		}

		#region IDocumentConfig Members

		IDocumentConfigItemCollection IDocumentConfig.ConfigItems
		{
			get { return ConfigItems; }
		}

		string IDocumentConfig.PageStyle
		{
			get { return S3_PageStyle; }
		}

		string IDocumentConfig.OverrideDataContext
		{
			get { return S3_OverrideDataContext; }
		}

		string IDocumentConfig.DocumentTitle
		{
			get
			{
				if (!S3_OverrideEmailSubject.IsEmpty && !S3_IsSystem)
				{
					return S3_OverrideEmailSubject;
				}

				if (MenuTemplatePivot != null)
				{
					return MenuTemplatePivot.SI_DocumentTitle;
				}

				return string.Empty;
			}
		}

		string IDocumentConfig.DocumentType
		{
			get { return MenuTemplatePivot.DocType == null ? "" : MenuTemplatePivot.DocType.RT_DocType.ToString(); }
		}

		bool IDocumentConfig.IsTemplate => S3_IsTemplate;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				switch (EditingMode)
				{
					case MenuEditingMode.AllowAll:
						return true;

					case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
						return IsUserDefined;

					case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
						return IsUserDefined || IsSystemDefined;

					case MenuEditingMode.AllowEditingOfClientSpecificOnly:
						return IsUserDefined || IsClientSpecific;

					default:
						return base.CanDelete;
				}
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;

				switch (EditingMode)
				{
					case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
						result = ResString.GetMultilingualString("e77f4112-525e-4d9e-a446-d2c6832c676e", "You are not allowed to delete this document configuration.");
						break;

					case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
						result = ResString.GetMultilingualString("84540f1e-d264-4f92-96dc-77b6a54252a5", "You are not allowed to delete the selected document configuration, because it is Not System Defined.");
						break;

					case MenuEditingMode.AllowEditingOfClientSpecificOnly:
						result = ResString.GetMultilingualString("c51708fd-aa3c-463b-9424-78b2237e49bf", "You are not allowed to delete the selected document configuration, because it is System Defined.");
						break;
				}

				return result;
			}
		}

		#endregion
	}
}
