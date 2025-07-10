using CargoWise.Types;
using static Enterprise.Customs.CA.Business.CusBondDetailCollection;

namespace Enterprise.Customs.CA.Business
{
	public interface IBondDetailsDefault
	{
		OrgImpAddInfo ImporterOfRecord { get; }
		OrgImpAddInfo ImportOrg { get; }
		ZDateTime EffectiveDate { get; }

		ZString CA_BondType { get; set; }
		ZString CA_BondNo { set; }
		ZString CA_SuretyCode { set; }
	}

	public class BondDetailsDefaulter
	{
		public void DefaultWhenBondTypeChanges(IBondDetailsDefault declaration, ZString bondType)
		{
			DefaultCore(declaration, bondType, false);
		}

		public void Default(IBondDetailsDefault declaration, ZString bondType)
		{
			DefaultCore(declaration, bondType, true);
		}

		void DefaultCore(IBondDetailsDefault declaration, ZString bondType, bool defaultBondType)
		{
			var bondData = GetDefaultBondDetails(declaration, bondType);
			if (bondData == null && defaultBondType && (bondType == BondTypeList.Codes.ContinuousBond || bondType == BondTypeList.Codes.SingleTransactionBond))
			{
				bondData = GetDefaultBondDetails(declaration, ZString.Empty);
			}

			if (bondData != null)
			{
				if (defaultBondType)
				{
					declaration.CA_BondType = bondData.PW_BondType;
				}
				declaration.CA_BondNo = bondData.PW_BondNumber;
				declaration.CA_SuretyCode = bondData.PW_SuretyCode;
			}
			else
			{
				declaration.CA_BondType = ZString.Empty;
				declaration.CA_BondNo = ZString.Empty;
				declaration.CA_SuretyCode = ZString.Empty;
			}
		}

		CusBondDetail GetDefaultBondDetails(IBondDetailsDefault declaration, ZString bondType)
		{
			CusBondDetail result = null;

			if (declaration.ImporterOfRecord != null)
			{
				result = declaration.ImporterOfRecord.BondDetails.GetActiveBondDetailData(bondType, declaration.EffectiveDate);
			}

			if (result == null && declaration.ImportOrg != null)
			{
				result = declaration.ImportOrg.BondDetails.GetActiveBondDetailData(bondType, declaration.EffectiveDate);
			}
			return result;
		}

		public BondDetailsStatus GetBondDetailsStatus(IBondDetailsDefault declaration, ZString bondType)
		{
			var result = BondDetailsStatus.NoBond;

			if (declaration.ImporterOfRecord != null)
			{
				result = declaration.ImporterOfRecord.BondDetails.GetBondDetailsStatus(bondType, declaration.EffectiveDate);
			}

			if (result != BondDetailsStatus.BondExist && declaration.ImportOrg != null)
			{
				var bondStatus = declaration.ImportOrg.BondDetails.GetBondDetailsStatus(bondType, declaration.EffectiveDate);
				if (result == BondDetailsStatus.NoBond || bondStatus == BondDetailsStatus.BondExist)
				{
					result = bondStatus;
				}
			}
			return result;
		}
	}
}
