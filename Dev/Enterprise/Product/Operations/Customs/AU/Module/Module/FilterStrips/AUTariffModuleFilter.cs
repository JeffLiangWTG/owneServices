using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	public enum TariffModuleFilterType
	{
		Export, Import
	}

	public class AUTariffModuleFilter : ModuleFilter
	{
		public AUTariffModuleFilter(ZString filterDescription, TariffModuleFilterType filterType, TariffFormatter tariffFormatter)
			: base(filterDescription)
		{
			fTariffFormatter = tariffFormatter;
			fFilterType = filterType;
		}

		TariffFormatter fTariffFormatter;

		#region Filter Type

		public TariffModuleFilterType FilterType
		{
			get { return fFilterType; }
		}

		TariffModuleFilterType fFilterType;

		#endregion

		#region CC_TariffNum

		[BusinessObjectTestExclude]
		public ZString CC_TariffNum
		{
			get { return fCC_TariffNum; }
			set
			{
				if (CC_TariffNum != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(CC_TariffNumInfo, ref fCC_TariffNum, fTariffFormatter.Format(value));
			}
		}

		public virtual ZPropertyInfo CC_TariffNumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CC_TariffNum)); }
		}

		ZString fCC_TariffNum;

		#endregion

		#region GetNewCommonModuleFilter / CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			AUTariffModuleFilter filter = filterToCopyFrom as AUTariffModuleFilter;
			fTariffFormatter = filter.fTariffFormatter;
			fFilterType = filter.FilterType;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			CC_TariffNum = ZString.Empty;
		}

		protected override bool IsEmptyCore => CC_TariffNum.IsEmpty;

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new AUTariffModuleFilterValidation(this);
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			ZQuery result = new ZQuery();

			if (!CC_TariffNum.IsEmpty)
			{
				result.AddToFilter(Enterprise.ZArchitecture.Schema.CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, CC_TariffNum);
			}

			return result;
		}

		protected override object[] QueryDelegateParameters
		{
			get { throw new NotSupportedException(GetType().Name + " does not support filtering via a query delegate."); }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException(GetType().Name + " does not support filtering via filter columns.");
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			// currently not serialised
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			// currently not serialised
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			CC_TariffNum = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}
