using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACCasualImpRates : AutoCACCasualImpRates
	{
		public CACCasualImpRates(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		internal static CACCasualImpRates Load(BusinessObjectFactory factory, ZString province, ZString commodity)
		{
			var query = new ZQuery(CACCasualImpRatesSchema.IR_Province, province);
			query.AddToFilter(CACCasualImpRatesSchema.IR_Commodity, commodity);

			return factory.LoadTop1<CACCasualImpRates>(query);
		}

		internal static CACCasualImpRates LoadPSTRates(BusinessObjectFactory factory, ZString provinceOfDest, ZString commodity, ZDate dateForDutyRate)
		{
			CACCasualImpRates result = null;

			var filter1 = new ZQuery(CACCasualImpRatesSchema.IR_Commodity, CasualImportConstants.CasualImpRatesCommodityType.CommodityTypePSTCode);
			if (!string.IsNullOrWhiteSpace(commodity))
			{
				var filter2 = new ZQuery(CACCasualImpRatesSchema.IR_Commodity, CasualImportConstants.CasualImpRatesCommodityType.CommodityTypePSTCode + LookupsHelper.GetCasualImportCommodityType(factory, commodity));
				filter1.AddToFilter(filter2, JoinCondition.Or);
			}
			var query = new ZQuery(CACCasualImpRatesSchema.IR_Province, provinceOfDest);
			query.AddToFilter(CACCasualImpRatesSchema.IR_ProcessingType, CasualImportConstants.CasualImpRatesProcessingType.ProcessingTypePSTCode);
			query.AddToFilter(CACCasualImpRatesSchema.IR_EffectiveDateFrom, SQLComparisonOperator.LessThanOrEqualTo, dateForDutyRate);
			query.AddToFilter(CACCasualImpRatesSchema.IR_EffectiveDateTo, SQLComparisonOperator.GreaterThanOrEqualTo, dateForDutyRate);
			query.AddToFilter(filter1);

			var ratesArray = factory.Load<CACCasualImpRates>(query);
			if (ratesArray != null)
			{
				result = ratesArray.FirstOrDefault(rates => rates.IR_Commodity == CasualImportConstants.CasualImpRatesCommodityType.CommodityTypePSTACode || rates.IR_Commodity == CasualImportConstants.CasualImpRatesCommodityType.CommodityTypePSTTCode);
				if (result == null)
				{
					result = ratesArray.FirstOrDefault();
				}
			}

			return result;
		}

		#endregion

		#region Properties

		public List<CasualImpRateSpec> CasualImpRateSpecList
		{
			get
			{
				if (casualImpRateSpecList == null)
				{
					casualImpRateSpecList = new List<CasualImpRateSpec>();
					if (!string.IsNullOrWhiteSpace(IR_RateType1))
					{
						casualImpRateSpecList.Add(new CasualImpRateSpec(IR_RateType1.Trim(), IR_Units1.Trim(), IR_RegularRate1, IR_MinimumRate1, IR_MaximumRate1));
					}
					if (!string.IsNullOrWhiteSpace(IR_RateType2))
					{
						casualImpRateSpecList.Add(new CasualImpRateSpec(IR_RateType2.Trim(), IR_Units2.Trim(), IR_RegularRate2, IR_MinimumRate2, IR_MaximumRate2));
					}
					if (!string.IsNullOrWhiteSpace(IR_RateType3))
					{
						casualImpRateSpecList.Add(new CasualImpRateSpec(IR_RateType3.Trim(), IR_Units3.Trim(), IR_RegularRate3, IR_MinimumRate3, IR_MaximumRate3));
					}
				}

				return casualImpRateSpecList;
			}
		}
		List<CasualImpRateSpec> casualImpRateSpecList;

		public void ResetCasualImpRateSpecList()
		{
			casualImpRateSpecList = null;
		}

		#endregion

	}
}
