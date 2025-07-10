using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACRate : AutoCACRate
	{
		public CACRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CACRate Load(BusinessObject master, ZString treatmentCode)
		{
			var query = new ZQuery(CACRateSchema.ZC_TreatmentCode, treatmentCode);
			query.AddToFilter(CACRateSchema.ZC_Inactive, SQLComparisonOperator.NotEqual, "Y");
			query.AddToFilter(CACRateSchema.ZC_ParentID, master.PK);
			return master.Factory.LoadTop1<CACRate>(query);
		}

		public static CACRate Load(JobComInvoiceLine invoiceLine)
		{
			return Load(invoiceLine.JI_Tariff, invoiceLine.EffectiveTreatmentCode, invoiceLine.EffectiveDateForDutyRate, invoiceLine.Factory);
		}

		public static CACRate Load(ZString classificationTariff, ZString treatmentCode, ZDateTime effectiveDate, BusinessObjectFactory factory, string type = CACRateHeader.RateType.ClassificationRate)
		{
			return factory.GetCachedValue(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationTariff, treatmentCode, effectiveDate.ToShortDateString(), type), () =>
			{
				var ratePK = new DynamicBusinessObjectCollection(factory);
				ratePK.Load(GetCACRatePKLoadSql(), new[]
				{
					ZSqlParameter.New("@ClassificationTariff", classificationTariff, CACClassHeaderSchema.ZA_ClassificationNumber),
					ZSqlParameter.New("@EffectiveDate", effectiveDate, CACClassHeaderSchema.ZA_EffectiveDate),
					ZSqlParameter.New("@RateType", type, CACRateHeaderSchema.ZB_RateType),
					ZSqlParameter.New("@TreatmentCode", treatmentCode, CACRateSchema.ZC_TreatmentCode),
				});
				return factory.Load<CACRate>(ratePK.Select(bo => new ZGuid(bo[CACRateSchema.Constants.PK])).FirstOrDefault());
			});
		}

		static string GetCACRatePKLoadSql()
		{
			var querySql = $@"
SELECT TOP 1 ZC_PK FROM dbo.RefDbEntCA_CACClassHeader
JOIN dbo.RefDbEntCA_CACRateHeader ON ZA_PK = ZB_ZA_ClassHeader 
JOIN dbo.RefDbEntCA_CACRate ON ZB_PK = ZC_ParentID 
WHERE
ZA_ClassificationNumber = @ClassificationTariff
AND ZA_EffectiveDate <= @EffectiveDate
AND ZA_ExpiryDate > @EffectiveDate
AND ZA_InactiveInd <> 'Y'
AND ZB_EffectiveDate <= @EffectiveDate
AND ZB_ExpiryDate > @EffectiveDate
AND ZB_Inactive <> 'Y'
AND ZB_RateType = @RateType
AND ZC_TreatmentCode  = @TreatmentCode
AND ZC_Inactive <> 'Y'
ORDER by ZB_EffectiveDate DESC, ZA_EffectiveDate DESC
";

			return querySql;
		}

		#region Parent

		public BusinessObject Parent
		{
			get { return Factory.Load(GetParentType(ZC_ParentTableCode), ZC_ParentID); }
		}

		Type GetParentType(string typeName)
		{
			Type result = null;
			switch (typeName)
			{
				case CACRateHeaderSchema.Constants.Prefix:
					result = typeof(CACRateHeader);
					break;
				case CACTariffHeaderSchema.Constants.Prefix:
					result = typeof(CACTariffHeader);
					break;
			}
			return result;
		}

		#endregion

		#region RateLines

		[ChildEditable(true)]
		public CACRateLineCollection RateLines
		{
			get
			{
				if (rateLines == null)
				{
					rateLines = new CACRateLineCollection(this);
					rateLines.Load();
					RegisterEditableChildObject(rateLines);
				}
				return rateLines;
			}
		}

		CACRateLineCollection rateLines;

		#endregion

		public ZString EffectiveUnitOfMeasure
		{
			get
			{
				return Parent is CACRateHeader rateHeader ? rateHeader.ZB_UnitOfMeasure : ZString.Empty;
			}
		}
	}
}
