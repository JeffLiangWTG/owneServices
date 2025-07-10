using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class GroupInvoiceChargeValidation : BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge charge)
			: base(charge)
		{
			GroupCharge = charge;
			this.jobDeclaration = GroupCharge.JobDeclaration;
		}

		#region Implementation

		readonly JobDeclaration jobDeclaration;
		protected readonly GroupInvoiceCharge GroupCharge;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (GroupCharge.J7_ChargeType == AUChargeCodeList.Codes.ExWorks)
			{
				if (jobDeclaration != null && jobDeclaration.IsImport)
				{
					GroupCharge.J7_ChargeTypeInfo.AddWarning("AU Customs doesn't accept ExWorks charge. Message will send this amount as 'Other Charge'");
				}
			}
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			MessageValidation.ValidateDistributeByForEdifice(Parent.J7_DistributeByInfo, jobDeclaration != null && jobDeclaration.IsImportEdifice, jobDeclaration != null && jobDeclaration.IsImport);
		}

		//J7_IsIncludedInITOT and Invoice flags are now copied down to apportioned charges
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
					fValidationHelper = new MessageValidation(GroupCharge);
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
			return new ExternalMessageValidation(GroupCharge);
		}

		protected override void CheckJ7_Percentage()
		{
			var totUnkFreightInsurancePercentage = AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value;

			if (totUnkFreightInsurancePercentage == 0 || !(jobDeclaration != null && jobDeclaration.IsImport))
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

					var charges = GroupCharge?.GroupInvoice?.Charges;
					decimal totalPercent = 0;
					foreach (GroupInvoiceCharge item in charges)
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
		#endregion
	}
}
