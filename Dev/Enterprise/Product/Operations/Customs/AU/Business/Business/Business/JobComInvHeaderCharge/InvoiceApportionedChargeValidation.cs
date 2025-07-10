using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceApportionedChargeValidation : BaseApportionedChargeValidation
	{
		public InvoiceApportionedChargeValidation(InvoiceApportionedCharge apportionedCharge) : base(apportionedCharge)
		{
			this.apportionedCharge = apportionedCharge;
		}

		protected JobDeclaration JobDeclaration
		{
			get
			{
				ICommonInvoice parentParent = Parent.Parent;
				return parentParent != null ? parentParent.JobDeclaration as JobDeclaration : null;
			}
		}

		protected override void CheckJ7_Percentage()
		{
			var totUnkFreightInsurancePercentage = AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value;

			if (totUnkFreightInsurancePercentage == 0 || !(JobDeclaration != null && JobDeclaration.IsImport))
			{
				base.CheckJ7_Percentage();
			}
			else
			{
				if (Parent.J7_ChargeType.In((ZString)CustomsChargeTypeList.Codes.OverseasFreight, (ZString)CustomsChargeTypeList.Codes.OverseasInsurance))
				{
					if (Parent.J7_Percentage > totUnkFreightInsurancePercentage)
					{
						Parent.J7_PercentageInfo.AddError(Res.GetString("9D134F93-146B-4E1C-895E-94BB33748904", "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage"));
					}
					var charges = (Parent.Parent as JobComInvoiceHeader).GroupCharges;
					decimal totalPercent = 0;
					foreach (InvoiceApportionedCharge item in charges)
					{
						if (totalPercent > totUnkFreightInsurancePercentage)
						{
							break;
						}
						if (item.J7_ChargeType.In((ZString)CustomsChargeTypeList.Codes.OverseasFreight, (ZString)CustomsChargeTypeList.Codes.OverseasInsurance))
						{
							totalPercent += item.J7_Percentage;
						}
					}
					if (totalPercent > 0 && totalPercent > totUnkFreightInsurancePercentage)
					{
						Parent.J7_PercentageInfo.AddMessageError(Res.GetString("2F9642AB-90A3-457A-8293-1FE545CBD618", "Total combined percentages of OFT and ONS must be no greater than {0} percentage.", totUnkFreightInsurancePercentage));
					}
				}
			}
		}
	}
}
