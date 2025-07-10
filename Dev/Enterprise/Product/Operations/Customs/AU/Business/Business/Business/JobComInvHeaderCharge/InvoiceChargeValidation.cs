using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceChargeValidation : BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge charge)
			: base(charge)
		{
			this.Charge = charge;

			try
			{
				this.JobDeclaration = charge.Parent != null ? (JobDeclaration)charge.Parent.JobDeclaration : null;
			}
			catch (InvalidCastException ex)
			{
				ErrorReporter.ReportOnce("InvoiceChargeValidation|InitializeJobDeclaration", $"{ex.Message} debug info [ InvoiceCharge PK: {charge.PK}, Parent Type: {charge.Parent.GetType().FullName}, Parent PK: {charge.Parent.PK}, Declaration PK: {charge.Parent.JobDeclaration?.PK} ]");
			}
		}

		protected readonly InvoiceCharge Charge;
		protected readonly JobDeclaration JobDeclaration;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (JobDeclaration != null && JobDeclaration.IsImportEdifice && Charge.J7_ChargeType == AUChargeCodeList.Codes.ExWorks)
			{
				Charge.J7_ChargeTypeInfo.AddWarning("Edifice message doesn't accept ExWorks charge. Message will send this amount as 'Other Charge'");
			}
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			MessageValidation.ValidateDistributeByForEdifice(Parent.J7_DistributeByInfo, JobDeclaration != null && JobDeclaration.IsImportEdifice, JobDeclaration != null && JobDeclaration.IsImport);
		}

		protected override void CheckJ7_IsDutiable()
		{
			base.CheckJ7_IsDutiable();
			ValidateJ7_IsGSTApplicable();
		}

		protected override void CheckJ7_IsGSTApplicable()
		{
			base.CheckJ7_IsGSTApplicable();
			ValidateJ7_IsDutiable();
		}

		MessageValidation fValidationHelper;
		protected MessageValidation ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new MessageValidation(Charge);
				}
				return fValidationHelper;
			}
		}

		public new ExternalMessageValidation MessageValidation
		{
			get { return (ExternalMessageValidation)base.MessageValidation; }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Charge);
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

					var charges = Parent?.Invoice?.Charges;
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
	}
}
