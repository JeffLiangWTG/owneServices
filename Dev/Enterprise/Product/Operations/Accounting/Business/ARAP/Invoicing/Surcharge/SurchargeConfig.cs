using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface ISurchargeConfig
	{
		(ZDecimal rate, ZGuid chargeCodePK) GetSurcharge(ZString surchargeCode, ZGuid chargeCodePK);
	}

	class SurchargeConfig : ISurchargeConfig
	{
		(ZDecimal rate, ZGuid chargeCodePK) ISurchargeConfig.GetSurcharge(ZString surchargeCode, ZGuid chargeCodePK)
		{
			var rate = 0m;
			var matchChargeCodePK = ZGuid.Empty;
			var factory = new BusinessObjectFactory();
			var chargeCode = factory.Load<AccChargeCode>(chargeCodePK);
			var query = new ZQuery(AccSurchargeConfigurationSchema.ASC_Code, surchargeCode);
			query.AddToFilter(new ZQuery(AccSurchargeConfigurationSchema.ASC_GC_Company, chargeCode.AC_GC));
			var surchargeConfig = factory.LoadTop1<AccSurchargeConfiguration>(query);

			if (surchargeConfig != null)
			{
				query = new ZQuery(AccSurchargeBasisSchema.ASB_ASC_SurchargeConfiguration, surchargeConfig.PK);
				var surchargeFeeBase = factory.Load<AccSurchargeBasis>(query).ToList();

				var hasSurchargeFee = true;

				switch (surchargeConfig.ASC_BasisType)
				{
					case SurchargeBasisTypeList.Codes.ALL:
						break;
					case SurchargeBasisTypeList.Codes.EXC:
						hasSurchargeFee = !surchargeFeeBase.Any(x => x.ASB_AC_ChargeCode == chargeCode.PK || x.ASB_ChargeGroup == chargeCode.AC_ChargeGroup);
						break;
					case SurchargeBasisTypeList.Codes.INC:
						hasSurchargeFee = surchargeFeeBase.Any(x => x.ASB_AC_ChargeCode == chargeCode.PK || x.ASB_ChargeGroup == chargeCode.AC_ChargeGroup);
						break;
				}
				if (hasSurchargeFee && surchargeConfig.ASC_Type == SurchargeTypeList.Codes.PER)
				{
					rate = surchargeConfig.ASC_Rate;
					matchChargeCodePK = surchargeConfig.ASC_AC_ChargeCode;
				}
			}
			return (rate, matchChargeCodePK);
		}
	}
}
