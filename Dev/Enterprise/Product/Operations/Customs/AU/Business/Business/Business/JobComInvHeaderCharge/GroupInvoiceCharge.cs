using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class GroupInvoiceCharge : BaseGroupInvoiceCharge
		, Integration.Customs.AU.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobDeclaration JobDeclaration
		{
			get { return Parent != null ? Parent.JobDeclaration as JobDeclaration : null; }
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(this);
		}

		[ReadOnlyMember(nameof(J7_IsCalculated))]
		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				var oldValue = J7_ChargeType;
				base.J7_ChargeType = value;
				var newValue = J7_ChargeType;
				if (oldValue != newValue && newValue == AUChargeCodeList.Codes.OverseasInsurance && !IsInDatabase)
				{
					ApplyApplicableInsuranceRate();
				}
				MarkAllGroupChargesAsNeedingValidation();
			}
		}

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.GroupInvoiceCharge|J7_IsCalculated", Caption = "Calculated")]
		[ReadOnlyMember(nameof(IsNotImportInsuranceCharge))]
		public override ZBool J7_IsCalculated
		{
			get => base.J7_IsCalculated;
			set => base.J7_IsCalculated = value;
		}

		public bool IsNotImportInsuranceCharge => !(J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance && JobDeclaration.IsImport);

		public void ApplyApplicableInsuranceRate()
		{
			if (GroupInvoice is JobComInvoiceGroupHeader groupInvoice)
			{
				var insurance = groupInvoice.GetApplicableInsurance();
				if (insurance != null)
				{
					var insuranceCalculator = new UniversalRateCalculator(insurance.CCR_Formula, groupInvoice);
					var insuranceAmount = insuranceCalculator.Calculate();
					insuranceAmount = ZArchitecture.Core.Utilities.Round(insuranceAmount, insurance.Currency.Decimals);
					J7_Percentage = 0;
					J7_Amount = insuranceAmount;
					J7_RX_NKCurrency = insurance.CCR_RX_NKCurrency;
					J7_IsCalculated = true;
				}
			}
		}

		public override ZBool J7_IsGSTApplicable
		{
			get { return base.J7_IsGSTApplicable; }
			set
			{
				base.J7_IsGSTApplicable = value;
				MarkAllGroupChargesAsNeedingValidation();
				MarkInvoicesOfGroupInvoiceAsNeedingValidation();
			}
		}

		public override ZString J7_DistributeBy
		{
			get { return base.J7_DistributeBy; }
			set
			{
				base.J7_DistributeBy = value;
				MarkAllGroupChargesAsNeedingValidation();
			}
		}

		public override ZBool J7_IsDutiable
		{
			get { return base.J7_IsDutiable; }
			set
			{
				base.J7_IsDutiable = value;
				MarkAllGroupChargesAsNeedingValidation();
				MarkInvoicesOfGroupInvoiceAsNeedingValidation();
			}
		}

		void MarkInvoicesOfGroupInvoiceAsNeedingValidation()
		{
			if (GroupInvoice != null)
			{
				GroupInvoice.JobComInvoiceHeaders.MarkAsNeedingValidation();
			}
		}

		public override ZString J7_FullOrPartialApportionment
		{
			get { return base.J7_FullOrPartialApportionment; }
			set
			{
				base.J7_FullOrPartialApportionment = value;
				MarkAllGroupChargesAsNeedingValidation();
			}
		}

		[ReadOnlyMember(nameof(J7_IsCalculated))]
		public override ZDecimal J7_Amount
		{
			get { return base.J7_Amount; }
			set
			{
				base.J7_Amount = value;
				MarkAllGroupChargesAsNeedingValidation();
				JobDeclaration.InvoiceLines.MarkAsNeedingValidationIncludingChildren();
			}
		}

		[ReadOnlyMember(nameof(J7_IsCalculated))]
		public override ZDecimal J7_Percentage
		{
			get => base.J7_Percentage;
			set => base.J7_Percentage = value;
		}

		[ReadOnlyMember(nameof(J7_IsCalculated))]
		public override ZString J7_RX_NKCurrency
		{
			get => base.J7_RX_NKCurrency;
			set => base.J7_RX_NKCurrency = value;
		}

		void MarkAllGroupChargesAsNeedingValidation()
		{
			if (HasChanges && JobDeclaration != null)
			{
				JobDeclaration.MarkAllGroupChargesAsNeedingValidation();
			}
		}
	}
}
