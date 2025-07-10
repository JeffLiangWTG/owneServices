using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	public sealed class TemporaryStmMenuDocumentConfig : StmMenuDocumentConfig
	{
		public TemporaryStmMenuDocumentConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		IStmMenuDocumentConfigSource fSource;
		readonly List<StmMenuDocumentConfig> fRelatedDocConfigs = new List<StmMenuDocumentConfig>();

		public TemplateSectionCollectionView AvailableSections
		{
			get
			{
				if (fAvailableSections == null)
				{
					fAvailableSections = new TemplateSectionCollectionView(TemplateSections);
					fAvailableSections.SetReadOnlyIncludingChildren(true);
				}
				return fAvailableSections;
			}
		}

		TemplateSectionCollectionView fAvailableSections;

		CodeDescriptionPairList categories;
		public CodeDescriptionPairList Categories
		{
			get { return categories ?? (categories = GetCategories()); }
		}

		CodeDescriptionPairList GetCategories()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(string.Empty);

			foreach (TemplateSection section in AvailableSections)
			{
				var category = section.Category;
				if (!category.IsEmpty && !result.ContainsCode(category))
				{
					result.AddPair(category);
				}
			}

			result.Sort();

			return result;
		}

		ZString categoryFilter;
		public ZString CategoryFilter
		{
			get { return categoryFilter; }
			set
			{
				if (value != categoryFilter)
				{
					categoryFilter = value;
					AvailableSections.CategoryFilter = value;
					AvailableSections.Rebuild();
				}
			}
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public IStmMenuDocumentConfigSource Source
		{
			get { return fSource; }
		}

		bool SystemEditable
		{
			get
			{
				MenuEditingMode editingMode = Source.EditingMode;
				return (editingMode == MenuEditingMode.AllowAll) || (editingMode == MenuEditingMode.AllowEditingOfSystemDefinedOnly);
			}
		}

		public StmMenuDocumentConfig Commit()
		{
			StmMenuDocumentConfig result = Source.GetPersistentDocConfig();
			CopyOver(result);

			var configItems = result.ConfigItems.ToArray<StmMenuDocumentConfigItem>();
			Array.Reverse(configItems);

			foreach (var configItem in configItems)
			{
				if (!ConfigItems.Contains(configItem.PK))
				{
					configItem.S4_PrintOrder = result.ConfigItems.Count;
					configItem.Delete();
				}
				//System and client flags can't be edited from GUI and should currently match their parent's config values.
				configItem.S4_IsSystemDefined = result.S3_IsSystem;
				configItem.S4_IsClientSpecific = result.S3_IsClientSpecific;
			}

			result.ConfigItems.SortByPrintOrder();
			result.HasChanges = true;
			return result;
		}

		protected override StmMenuDocumentConfigItem GetConfigItemCopy(StmMenuDocumentConfig docConfigToCopyTo, StmMenuDocumentConfigItem configItemToCopyFrom)
		{
			StmMenuDocumentConfigItem result = (StmMenuDocumentConfigItem)docConfigToCopyTo.ConfigItems.FindByPK(configItemToCopyFrom.PK);
			if (result == null)
			{
				result = (StmMenuDocumentConfigItem)docConfigToCopyTo.Factory.ImportFromAnotherFactory(configItemToCopyFrom);
				docConfigToCopyTo.ConfigItems.Add(result);
			}
			return result;
		}

		protected override StmMenuDocumentConfig[] GetRelatedDocConfigsCore()
		{
			return fRelatedDocConfigs.ToArray();
		}

		public static TemporaryStmMenuDocumentConfig New(IStmMenuDocumentConfigSource source)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TemporaryStmMenuDocumentConfig result = source.GetTemporaryDocConfig(factory);
			result.fSource = source;

			TemplateSectionCollection.CacheManager.Get(source.Factory, factory);

			StmMenuTemplatePivot menuTemplatePivot = source.MenuTemplatePivot;

			result.S3_SI = menuTemplatePivot.PK;

			var query = new ZQuery(StmMenuTemplatePivotSchema.PK, menuTemplatePivot.PK);
			query.FetchOnlyFromLocalCache = true;
			if (factory.Load(menuTemplatePivot.GetType(), query).Length == 0)
			{
				factory.ImportFromAnotherFactory(menuTemplatePivot);
			}

			factory.ImportFromAnotherFactory(menuTemplatePivot.MenuItem);

			ZQuery relatedDocConfigsQuery = new ZQuery();
			relatedDocConfigsQuery.AddToFilter(StmMenuDocumentConfigSchema.PK, SQLComparisonOperator.NotEqual, result.PK);
			relatedDocConfigsQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_SI, menuTemplatePivot.PK);
			StmMenuDocumentConfig[] relatedDocConfigs = source.Factory.Load<StmMenuDocumentConfig>(relatedDocConfigsQuery);
			foreach (StmMenuDocumentConfig relatedDocConfig in relatedDocConfigs)
			{
				result.fRelatedDocConfigs.Add((StmMenuDocumentConfig)factory.ImportFromAnotherFactory(relatedDocConfig));
			}

			foreach (StmMenuDocumentConfigItem configItem in result.ConfigItems)
			{
				if (!result.AvailableSections.CollectionToFilter.Contains(configItem.S4_SectionItemName))
				{
					configItem.AddRowError(Res.GetString("EE188268-9D35-4156-961A-FA4F64D37CB4", "This section is neither a system defined config item nor a customized config item and should be removed."));
				}
			}

			if (result.S3_IsSystem && !result.SystemEditable)
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			return result;
		}

		protected override void OnFactorySaving()
		{
			throw new InvalidOperationException("Factory must not be saved if containing a TemporaryStmMenuDocumentConfig.");
		}

		#region Properties

		#region S3_Calc_DocumentTitle

		public ZString S3_Calc_DocumentTitle
		{
			get { return MenuTemplatePivot.SI_DocumentTitle; }
		}

		public ZPropertyInfo S3_Calc_DocumentTitleInfo
		{
			get { return GetZPropertyInfo(Schema.S3_Calc_DocumentTitle); }
		}

		#endregion

		#region S3_Calc_MenuName

		public ZString S3_Calc_MenuName
		{
			get { return MenuTemplatePivot.MenuItem.SU_MenuName; }
		}

		public ZPropertyInfo S3_Calc_MenuNameInfo
		{
			get { return GetZPropertyInfo(Schema.S3_Calc_MenuName); }
		}

		#endregion

		#region S3_IsSystem

		[ReadOnlyMember(nameof(S3_IsSystem_ReadOnly))]
		public override ZBool S3_IsSystem { get => base.S3_IsSystem; set => base.S3_IsSystem = value; }

		bool S3_IsSystem_ReadOnly => !SystemEditable;

		#endregion

		#region S3_IsTemplate

		[ReadOnlyMember(nameof(S3_IsTemplate_ReadOnly))]
		public override ZBool S3_IsTemplate { get => base.S3_IsTemplate; set => base.S3_IsTemplate = value; }

		bool S3_IsTemplate_ReadOnly => !SystemEditable;

		#endregion

		#endregion

		#region Schema

		public new abstract class Schema : AutoStmMenuDocumentConfig.Schema
		{
			public const string S3_Calc_DocumentTitle = "S3_Calc_DocumentTitle";
			public const string S3_Calc_MenuName = "S3_Calc_MenuName";
			public const string S3_Calc_TemplateName = "S3_Calc_TemplateName";
		}

		#endregion
	}
}
