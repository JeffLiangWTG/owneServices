using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CalculateFreightBizObj : EU.Business.Declaration.CalculateFreightBizObj
	{
		public CalculateFreightBizObj(
			IJobComInvChargeCollection<JobComInvCharge> charges,
			EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory,
			ZString currency,
			ZString country,
			ZDateTime dateOfValuation,
			ZString iataLoadPort
		) : base(
			charges,
			euIncoTermAndChargeFactory,
			currency,
			country,
			dateOfValuation,
			iataLoadPort
		)
		{
		}

		protected override ZString[] GetChargeTypesToDefault() => new[] { (ZString)AISChargeCodeList.Codes.AK };

		protected override bool GetCalculateResult() => base.GetCalculateResult() && SetAfterEUBorderCharge();

		bool SetAfterEUBorderCharge()
		{
			if (!AmountAfterEUBorder.IsEmpty && charges.FirstOrDefault() is JobComInvCharge charge)
			{
				var shouldAddBACharge =
					charge.Parent is JobComInvoiceHeader invoiceHeader && invoiceHeader.JZ_IncoTerm.IsRecommendedAndMandatoryCodesEXW_FCA_FAS_FOB()
					|| charge.Parent is JobComInvoiceGroupHeader groupHeader && groupHeader.JobDeclaration is JobDeclaration declaration && declaration.Invoices.Any(inv => inv.JZ_IncoTerm.IsRecommendedAndMandatoryCodesEXW_FCA_FAS_FOB());
				if (shouldAddBACharge)
				{
					var chargeTypeBA = AISChargeCodeList.Codes.BA;
					var baCharge = charges.FirstOrDefault(charge => charge.J7_ChargeType == chargeTypeBA) ?? charges.AddNew(chargeTypeBA);
					baCharge.J7_Amount = AmountAfterEUBorder;
					baCharge.J7_RX_NKCurrency = Currency;
				}
			}
			return true;
		}
	}
}
