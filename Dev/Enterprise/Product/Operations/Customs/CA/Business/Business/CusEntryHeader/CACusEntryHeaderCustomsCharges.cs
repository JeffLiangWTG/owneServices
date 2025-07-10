using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
namespace Enterprise.Customs.CA.Business
{
	class CACusEntryHeaderCustomsCharges : Customs.Business.InterfaceImplementations.CusEntryHeaderCustomsCharges
	{
		public CACusEntryHeaderCustomsCharges(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)entryHeader; }
		}

		protected override Dictionary<OrganizationReferenceKey, ZDecimal> GetTotalChargeValueByDebtorFor(EntryChargeType chargeType, ZString methodOfPayment, ILogger logger)
		{
			var declaration = EntryHeader.Declaration;
			if (declaration.IsConsolidatedLVS)
			{
				return EntryHeader.GetLVSTotalChargeValueByDebtorFor(chargeType);
			}
			else if (declaration.IsIM2)
			{
				return EntryHeader.GetIM2TotalChargeValueByDebtorFor(chargeType);
			}
			else if (declaration.IsB2Adjustments || declaration.IsB3X)
			{
				return EntryHeader.GetB2TotalChargeValueByDebtorFor(chargeType);
			}
			else
			{
				var result = base.GetTotalChargeValueByDebtorFor(chargeType, methodOfPayment, logger);
				if (chargeType.Code == Registry.EntryChargeTypeList.Codes.TotalGSTAmount && EntryHeader.Declaration.IsExistingEffectiveCasualImport)
				{
					result[result.First().Key] += EntryHeader.GetTotalPSTForCasualImport();
				}
				return result;
			}
		}

		protected override bool IsFeePaidByBroker(string feeCode, ZGuid importerPK, ZString methodOfPaymentCode, ILogger logger)
		{
			return EntryHeader.Declaration.IsLVSTotalConsolidation ? EntryHeader.IsFeePaidByBroker(feeCode, importerPK) : base.IsFeePaidByBroker(feeCode, importerPK, methodOfPaymentCode, logger);
		}
	}
}
