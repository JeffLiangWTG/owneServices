using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public abstract class CommonConsolCostValidation : JobConsolCostValidation
	{
		public CommonConsolCostValidation(JobConsolCost parent)
			: base(parent)
		{
		}

		#region PrepaidCollect

		protected override void CheckE6_PPDCLT()
		{
			base.CheckE6_PPDCLT();

			MandatoryValidation.CheckEntered(Parent.E6_PPDCLTInfo);

			var apportionmentCharges = Parent.ApportionmentCharges?.Cast<ApportionSplitCharge>() ?? Enumerable.Empty<ApportionSplitCharge>();
			var homeCountry = GlbCompany.CurrentCompany?.Country;

			if ((Parent.E6_PPDCLT == PrepaidCollectCodes.PPD || Parent.E6_PPDCLT == PrepaidCollectCodes.CCX) && !apportionmentCharges.Any(c => c.JR_PrepaidCollect == Parent.E6_PPDCLT))
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("830e9ae0-6f7b-41f3-a6a4-c1c97dcbc5a2", "Apportionment Filter - There are no jobs matching '{0}'. Select another filter to apply apportionment to.", Parent.E6_PPDCLT));
			}
			else if (Parent.E6_PPDCLT == PrepaidCollectCodes.CTS && !Parent.IsContainerService)
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("59da5ccb-c3cd-4cbd-b2a4-ec0203d3b769", "{0} Apportionment filter cannot be applied on this cost, as selected charge code cannot be mapped to any container service.", PrepaidCollectList.Codes.CTS));
			}
			else if (Parent.E6_PPDCLT == PrepaidCollectCodes.LOG && !apportionmentCharges.Any(c => c.IsLocalOrigin(homeCountry)))
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("659c2357-7cb0-4b6b-9c24-6a92d29cf788", "Apportionment Filter - There are no shipments with local origin. Select another filter to apply apportionment to."));
			}
			else if (Parent.E6_PPDCLT == PrepaidCollectCodes.FOG && !apportionmentCharges.Any(c => c.IsForeignOrigin(homeCountry)))
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("a0dcf915-7c43-4d0d-93dd-a3829a676695", "Apportionment Filter - There are no shipments with foreign origin. Select another filter to apply apportionment to."));
			}
			else if (Parent.E6_PPDCLT == PrepaidCollectCodes.LDT && !apportionmentCharges.Any(c => c.IsLocalDestination(homeCountry)))
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("75c0bcb6-5764-4c37-a3db-6444fc45dd0d", "Apportionment Filter - There are no shipments with local destination. Select another filter to apply apportionment to."));
			}
			else if (Parent.E6_PPDCLT == PrepaidCollectCodes.FDT && !apportionmentCharges.Any(c => c.IsForeignDestination(homeCountry)))
			{
				Parent.E6_PPDCLTInfo.AddError(Res.GetString("ecec4405-4a24-4f14-98db-4779cd55e5e5", "Apportionment Filter - There are no shipments with foreign destination. Select another filter to apply apportionment to."));
			}
		}

		#endregion

		#region Bank Account Validation

		protected override void CheckE6_AB_BankAccount()
		{
			base.CheckE6_AB_BankAccount();
			ListValidation.ErrorIfInvalidPK(Parent.E6_AB_BankAccountInfo);
		}

		#endregion

		#region GST Validation

		protected override void CheckE6_AT_TaxRate()
		{
			base.CheckE6_AT_TaxRate();
			if (Parent.IsCostGSTApplicable && (!Parent.IsPosted || Parent.IsApprovingPosting))
			{
				MandatoryValidation.CheckEntered(Parent.E6_AT_TaxRateInfo);
			}
		}

		#endregion

		#region ChargeCode Validation

		protected override void CheckE6_AC_ChargeCode()
		{
			base.CheckE6_AC_ChargeCode();
			ListValidation.ErrorIfInvalidPK(Parent.E6_AC_ChargeCodeInfo);
		}

		#endregion

		#region Cost Amount Validation

		protected override void CheckE6_OSCostAmount()
		{
			base.CheckE6_OSCostAmount();
			MandatoryValidation.CheckEntered(Parent.E6_OSCostAmountInfo);
			if (!AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value && Parent.E6_OSCostAmount < 0)
			{
				if (!Parent.E6_OH_Creditor.IsValid ||
				Parent.E6_InvoiceNum.IsEmpty ||
				Parent.E6_InvoiceDate.IsEmpty ||
				Parent.E6_PaymentDate.IsEmpty)
				{
					Parent.E6_OSCostAmountInfo.AddError(Res.GetString("f1125539-feb0-48e5-82f2-c39ab5da14ba", "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date. Also note that {0} does not create accruals for negative values", Core.Constants.ProductName));
				}
			}

			if (Parent.Currency != null && Parent.Currency.Decimals == 0 &&
				(Math.Abs(Parent.E6_OSCostAmount) - (decimal)Math.Abs(Math.Floor((double)Parent.E6_OSCostAmount)) != 0))
			{
				Parent.E6_OSCostAmountInfo.AddError(Res.GetString("d3b3db3c-4a47-4b87-a047-0aeabcba327b", "Decimal places are not valid for this currency. Enter amount without decimal places."));
			}
		}

		protected override void CheckE6_LocalCostAmount()
		{
			base.CheckE6_LocalCostAmount();
			MandatoryValidation.CheckEntered(Parent.E6_LocalCostAmountInfo);
		}

		#endregion

		#region Implementation

		protected new JobConsolCost Parent
		{
			get { return (JobConsolCost)base.Parent; }
		}

		#endregion
	}
}
