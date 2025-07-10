using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter
{
	public class UniversalCopyScheduleModuleFilter : ModuleFilter
	{
		public UniversalCopyScheduleModuleFilter(string description)
			: base(description, new BusinessObjectFactory())
		{
			MultilingualDescription = ResString.GetMultilingualString("73C8DDD0-5B25-4BBE-AD56-DD97DE2DF213", "Module");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[List("ComparisonOperator_List")]
		public virtual ZString ComparisonOperator // for binding
		{
			get { return fComparisonOperator; }
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (fComparisonOperator != value)
				{
					fComparisonOperator = value;
					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[List("ModulesPair_List")]
		public virtual ZString ModulesPairList // for binding
		{
			get { return fModulesPairList; }
			set
			{
				if (fModulesPairList != value)
				{
					fModulesPairList = value;
					ModulesPairListInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString fModulesPairList;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public SQLComparisonOperator SqlComparisonOperator
		{
			get
			{
				return ModuleTextFilter.GetSqlComparisonOperator(ComparisonOperator, SQLComparisonOperator.StartsWith);
			}
		}

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		public ZPropertyInfo ModulesPairListInfo
		{
			get { return GetZPropertyInfo(nameof(ModulesPairList)); }
		}

		public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				return ModuleTextFilter.ComparisonConstants.GetAllComparisonOperators();
			}
		}

		protected virtual bool SupportsComparisonOperatorSet
		{
			get { return true; }
		}

		void EnsureSupportsComparisonOperatorSet()
		{
			if (!SupportsComparisonOperatorSet)
			{
				ErrorReporter.ReportOnce("Cannot set SqlComparisonOperator on a module filter",
					"Cannot set SqlComparisonOperator on a module filter of type " + GetType().Name + ".\n" +
					"Filter Description: " + Description);
			}
		}

		ZString fComparisonOperator = ModuleTextFilter.ComparisonConstants.Default;

		#region modulesPairList
		public CodeDescriptionPairList ModulesPair_List
		{
			get
			{
				if (fModulesPair_List == null)
				{
					fModulesPair_List = GetAllModulesPairsList();
				}
				return fModulesPair_List;
			}
		}
		CodeDescriptionPairList fModulesPair_List;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching English text")]
		public static CodeDescriptionPairList GetAllModulesPairsList()
		{
			var modulesPair_List = new CodeDescriptionPairList();

			var loader = ObjectFactory.Get<IModuleTreeLoader>();
			var moduleTree = new ModuleTree();
			loader.Initialise(moduleTree, Env.Security);
			loader.LoadModules();
			foreach (ModuleCategory category in moduleTree.Categories.Values)
			{
				if (category.Name != ModuleTreeLoaderConstant.Category.Jump.Name)
				{
					foreach (ModuleSection section in category.Sections.Values)
					{
						foreach (INamedModule module in section.Modules.Values)
						{
							if (module.Description.GetUnresolvedString() != "Reports")
							{
								modulesPair_List.AddPair(module.ID, MultilingualString.Join(" > ", category.DisplayText, section.DisplayText, module.Description));
							}
						}
					}
				}
			}

			return modulesPair_List;
		}

		string ModulesCodeColumn
		{
			get
			{
				var moduleName = UniversalCopyScheduleModuleFilter.GetAllModulesPairsList().GetCodeFromDescription(ModulesPairList);

				if (moduleName != null)
				{
					var moduleId = ModuleIDs.AllExcludingClientModules.FirstOrDefault(id => id.Name == moduleName);
					if (moduleId != null)
					{
						using (var module = ZModuleFactory.Instance.Create(moduleId))
						{
							var bizoType = (module as ZFilterModule)?.TypeOfTopLevelBusinessObject;
							if (bizoType != null)
							{
								fBizoType = bizoType;
								try
								{
									return CodePropertyAttribute.CodePropertyNameFromType(bizoType);
								}
								catch (NoCodePropertyException)
								{
									return null;
								}
							}
						}
					}
				}
				return null;
			}
		}

		Type fBizoType;
		#endregion

		#region Object

		public ZString CopyObjectCode
		{
			get
			{
				return fCopyObjectCode;
			}
			set
			{
				if (fCopyObjectCode != value)
				{
					fCopyObjectCode = value;
					CopyObjectCodeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString fCopyObjectCode;

		public ZPropertyInfo CopyObjectCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CopyObjectCode)); }
		}

		public bool CopyObjectCode_ReadOnly => ModulesCodeColumn == null;
		public bool ComparisonOperator_ReadOnly => ModulesCodeColumn == null;

		#endregion

		#region XML

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(CopyObjectCodeInfo.Name, CopyObjectCode);
			writer.WriteElementString(ModulesPairListInfo.Name, ModulesPairList);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == CopyObjectCodeInfo.Name)
			{
				CopyObjectCode = reader.ReadElementString(CopyObjectCodeInfo.Name);
			}
			else if (reader.Name == ModulesPairListInfo.Name)
			{
				ModulesPairList = reader.ReadElementString(ModulesPairListInfo.Name);
			}
		}

		#endregion

		#region Query
		SchemaColumn ColumnNameForCopyObject
		{
			get
			{
				return SchemaNameForCopyObject.GetSchemaColumn(ModulesCodeColumn);
			}
		}

		ITableSchema SchemaNameForCopyObject
		{
			get
			{
				var columnTablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(ModulesCodeColumn);
				return EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(columnTablePrefix);
			}
		}

		protected override ZQuery GetQuery()
		{
			var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));

			if (ModulesPair_List.Count != 0 && !ModulesPairList.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(UniversalCopyTemplate), StmUniversalCopySchema.SUC_S9_CopyTemplate);
				subQuery.AddToFilter(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.Equal, ModulesPair_List.GetCodeFromDescription(ModulesPairList) + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			if (!CopyObjectCode.IsEmpty)
			{
				var subQueryObject = new ZDBOnlySubQuery((fBizoType), StmUniversalCopySchema.SUC_CopyObjectId);
				subQueryObject.AddToFilter(ColumnNameForCopyObject, SqlComparisonOperator, fCopyObjectCode);
				query.AddSubQuery(subQueryObject, JoinCondition.And);
			}
			return query;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return GetQuery();
		}

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { fModulesPair_List, CopyObjectCode }; }
		}

		#endregion

		#region Implementation

		protected override void ClearCore()
		{
			ModulesPairList = CopyObjectCode = new ZString();
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override bool IsEmptyCore => string.IsNullOrEmpty(ModulesPairList);

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new UniversalCopyScheduleModuleFilter("Module");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var other = (UniversalCopyScheduleModuleFilter)filterToCopyFrom;
			ModulesPairList = other.ModulesPairList;
			CopyObjectCode = other.CopyObjectCode;
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			CopyObjectCode = RandomString(MaxLength);
			ModulesPairList = RandomString(MaxLength);
		}
#endif

		#endregion

		#region Validation
		public new UniversalCopyScheduleModuleFilterValidation Validation
		{
			get { return (UniversalCopyScheduleModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new UniversalCopyScheduleModuleFilterValidation(this);
		}

		public class UniversalCopyScheduleModuleFilterValidation : ModuleFilterValidation
		{
			public UniversalCopyScheduleModuleFilterValidation(UniversalCopyScheduleModuleFilter parent)
			: base(parent)
			{
				Parent = parent;
			}

			protected readonly UniversalCopyScheduleModuleFilter Parent;

			public override Type AutoValidationType => typeof(object);

			public override void ValidateAll()
			{
				// Nothing to validate.
			}
		}

		#endregion
		#endregion
	}
}
