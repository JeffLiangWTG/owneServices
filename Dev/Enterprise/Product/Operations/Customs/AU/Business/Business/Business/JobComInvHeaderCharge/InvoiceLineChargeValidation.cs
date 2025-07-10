using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		#region Implementation

		new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}

		JobDeclaration JobDeclaration
		{
			get { return Parent.InvoiceLine != null ? Parent.InvoiceLine.Declaration as JobDeclaration : null; }
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			JobDeclaration jobDeclaration = this.JobDeclaration;    // Caching
			if (jobDeclaration != null && jobDeclaration.IsImportEdifice
				&& Parent.J7_ChargeType != CustomsChargeTypeList.Codes.OverseasFreight
				&& Parent.J7_ChargeType != CustomsChargeTypeList.Codes.OverseasInsurance)
			{
				Parent.J7_ChargeTypeInfo.AddMessageError("For Edifice, you can only enter OFT and ONS in this grid.");
			}
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			MessageValidation.ValidateDistributeByForEdifice(Parent.J7_DistributeByInfo, JobDeclaration != null && JobDeclaration.IsImportEdifice, JobDeclaration != null && JobDeclaration.IsImport);
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

					var charges = Parent?.InvoiceLine.Charges;
					decimal totalPercent = 0;
					foreach (var item in charges)
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

		public new ExternalMessageValidation MessageValidation
		{
			get { return (ExternalMessageValidation)base.MessageValidation; }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		#endregion
	}
}
