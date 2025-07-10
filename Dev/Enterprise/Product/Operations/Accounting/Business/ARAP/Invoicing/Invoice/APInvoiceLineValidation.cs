using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceLineValidation : InvoiceLineValidation
	{
		public APInvoiceLineValidation(APInvoiceLine parent)
			: base(parent)
		{
		}

		protected new APInvoiceLine Parent
		{
			get { return base.Parent as APInvoiceLine; }
		}

		#region Overrides

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckApportionSplitChargeCostAmount();
		}

		void CheckApportionSplitChargeCostAmount()
		{
			if (Parent.ApportionmentChargeImportedFrom != null)
			{
				var charge = Parent.ApportionmentChargeImportedFrom;
				if (charge.JR_OSCostAmt.IsEmpty || charge.JR_LocalCostAmt.IsEmpty)
				{
					Parent.AddRowError(Res.GetString("195B2479-03A7-43C8-BC43-F7C76D5BFCA7", "Linked apportion charge should not have zero oversea or local cost amount. Please remove this line and other associated lines then import the apportioned charges again."));
				}
			}
		}

		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();
			ValidateGenericCharge();
		}

		protected override void CheckAL_AC()
		{
			base.CheckAL_AC();
			ValidateGenericCharge();
		}

		protected override void CheckGenericCharge()
		{
			base.CheckGenericCharge();
			if (!Parent.IsPopulatedFromImportedApportionment)
			{
				CheckUnpostedApportionment(Parent.AL_AC, Parent.GenericChargeInfo);
			}

			ZString chargeType = Parent.ChargeTypeWithOverride;
			bool isJobEntered = Parent.InvoicingJob != null;

			if (chargeType != ZString.Empty && !AccChargeCode.IsValidInAP(chargeType, isJobEntered))
			{
				Parent.GenericChargeInfo.AddError(Res.GetString("26d939a3-5eb6-41ec-a340-ea7e5068af4d", "This charge code cannot be chosen here."));
			}
		}

		void CheckUnpostedApportionment(ZGuid aL_ACAG, ZPropertyInfo aL_ACAGInfo)
		{
			if (aL_ACAG != ZGuid.Empty && aL_ACAG != ZGuid.Invalid)
			{
				if (Parent.AL_JH != ZGuid.Empty && Parent.AL_JH != ZGuid.Invalid)
				{
					Charge charge = JobContainsUnpostedApportionments(Parent.AL_JH, aL_ACAG);
					if (charge != null && charge.ParentConsolCost != null)
					{
						IJobCostingPlugIn consol = GenericConsol.GenericConsol.GetIJobCostingPlugInByPK(Parent.Factory, charge.ParentConsolCost.E6_ParentID, charge.ParentConsolCost.E6_ParentTableCode);
						ZString consolNumber = (consol == null) ? ZString.Empty : consol.JK_UniqueConsignRef;

						bool bringForwardAgainstCreditor = AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
						bool shouldAddWarningInsteadOfError = false;
						if (bringForwardAgainstCreditor)
						{
							AccTransactionHeader transactionHeader = Parent.Factory.Load<AccTransactionHeader>(Parent.AL_AH);
							shouldAddWarningInsteadOfError = !charge.ParentConsolCost.E6_OH_Creditor.IsEmpty && (transactionHeader.AH_OH != charge.ParentConsolCost.E6_OH_Creditor);
						}
						if (shouldAddWarningInsteadOfError)
						{
							OrgHeader orgHeader = Parent.Factory.Load<OrgHeader>(charge.ParentConsolCost.E6_OH_Creditor);
							if (orgHeader != null)
							{
								aL_ACAGInfo.AddWarning(Res.GetString("5b319d95-d35a-4f8f-a1cf-41d8bb5bfaf3", "There is an unposted apportionment relating to this charge code for the job number you have selected. The unposted apportionment is on Consol {0} for creditor {1}. Please review the creditor on the consol cost to ensure it is correct.", consolNumber, orgHeader.OH_Code));
							}
						}
						else
						{
							if (Parent.IsPopulatedFromImportedJobCharge)
							{
								aL_ACAGInfo.AddWarning(Res.GetString("26608e59-8fe9-48c2-86cc-f39fcd1d6d46", "There is an unposted apportionment relating to this charge code for the job number that you have selected. If you are intending to post that apportionment, please cancel this line and click on the Apportion To Consols button to import the charge."));
							}
							else
							{
								aL_ACAGInfo.AddError(Res.GetString("47a554fe-703a-4d03-9e0e-fb179a7a9e8f", "There is an unposted apportionment relating to this charge code for the job number that you have selected. Please cancel this line and click on the Apportion To Consols button to import the charge."));
							}
						}
					}
				}
			}
		}

		protected override void CheckAL_OSExTaxAmount()
		{
			base.CheckAL_OSExTaxAmount();
			if (!Parent.AL_OSExTaxAmountInfo.HasErrors())
			{
				var costVarianceAuthorisationRequired = Parent.CostVarianceAuthorisationRequired;
				if (costVarianceAuthorisationRequired == APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired)
				{
					Parent.AL_OSExTaxAmountInfo.AddWarning(Res.GetString("1e83aa30-3693-4b34-aae0-9198fda00353", "This cost requires approval on posting because it exceeds the registry defined accrual variance threshold."));
				}
				else if (costVarianceAuthorisationRequired == APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights)
				{
					Parent.AL_OSExTaxAmountInfo.AddWarning(Res.GetString("d60cc574-c524-47b3-afbc-168d715e7155", "This cost will be automatically approved when you post because you already have the necessary authorization security right"));
				}
			}

			if (!Parent.AL_OSExTaxAmountInfo.HasErrors() && Parent.Factory.HasContext(BusinessContext.APInvoiceForm)
				 && !Parent.AL_JH.IsEmpty && !Parent.AL_AC.IsEmpty && Parent.APInvoice != null)
			{
				var lineChargeAmountSum = Parent.APInvoice.GetLineChargeAmountSumFromCache(Parent);
				var billedRevenue = lineChargeAmountSum.RevenueAmountSum;
				var unbilledRevenue = -1m * lineChargeAmountSum.WIPAmountSum;
				var totalRevenue = billedRevenue + unbilledRevenue;
				var billedCost = -1m * lineChargeAmountSum.CSTAmountSum;
				var costEntered = Parent.AL_LocalExTaxAmount;
				var totalCost = costEntered + billedCost;

				var currencySymbolFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;
				currencySymbolFormat.CurrencySymbol = GlbCompany.CurrentCompany.LocalCurrency.RX_Symbol;
				currencySymbolFormat.CurrencyDecimalDigits = GlbCompany.CurrentCompany.LocalCurrency.Decimals;

				var warningMessage = string.Empty;

				if (totalCost > billedRevenue && totalCost <= totalRevenue )
				{
					warningMessage = GetExceedsBilledRevenueMessage(billedCost, billedRevenue, unbilledRevenue, currencySymbolFormat);
				}
				else if (totalCost > billedRevenue && totalCost > totalRevenue )
				{
					warningMessage = GetExceedsTotalRevenueMessage(billedCost, billedRevenue, unbilledRevenue, currencySymbolFormat);
				}

				if (!string.IsNullOrWhiteSpace(warningMessage))
				{
					Parent.AL_OSExTaxAmountInfo.AddWarning(warningMessage);
				}
			}
		}

		string GetExceedsBilledRevenueMessage(ZDecimal billedCost, ZDecimal billedRevenue, ZDecimal unbilledRevenue, NumberFormatInfo currencySymbolFormat)
		{
			return Res.GetString("22480210-ac20-4d3d-849c-4d8a125c5a25", "The cost on this line") + " " +
				(billedCost != 0 ? (Res.GetString("a5cca62c-83d3-4d12-abf6-8702745ce8d0", "together with billed cost of {0}", billedCost.ToString("C", currencySymbolFormat))) + " " : "") +
				Res.GetString("e9ef254d-3b72-43c9-87c1-ac78eff3ac74", "is greater than the corresponding billed revenue of {0}.", billedRevenue.ToString("C", currencySymbolFormat)) + " " +
				(unbilledRevenue != 0 ? Res.GetString("ab483a88-04cd-4540-832b-a9441fe948ef", "There is still un-billed revenue of {0}.", unbilledRevenue.ToString("C", currencySymbolFormat)) : "");
		}

		string GetExceedsTotalRevenueMessage(ZDecimal billedCost, ZDecimal billedRevenue, ZDecimal unbilledRevenue, NumberFormatInfo currencySymbolFormat)
		{
			return Res.GetString("22480210-ac20-4d3d-849c-4d8a125c5a25", "The cost on this line") + " " +
				(billedCost != 0 ? (Res.GetString("a5cca62c-83d3-4d12-abf6-8702745ce8d0", "together with billed cost of {0}", billedCost.ToString("C", currencySymbolFormat))) + " " : "") +
				Res.GetString("fdf9315a-cd15-492e-86fa-2667a188fd9c", "is greater than the corresponding billed revenue of {0}", billedRevenue.ToString("C", currencySymbolFormat)) +
				(unbilledRevenue != 0 ? " " + (Res.GetString("3de51fa3-14a0-4b4b-9a78-88f7f8f1909a", "and un-billed revenue of {0}.", unbilledRevenue.ToString("C", currencySymbolFormat))) : ".");
		}

		#region AL_IsFinalCharge

		protected override void CheckAL_IsFinalCharge()
		{
			base.CheckAL_IsFinalCharge();

			if (!Parent.IsPopulatedFromImportedApportionment)
			{
				if ((!Parent.IsInDatabase || Parent is UAInvoiceLine)
					&& Parent.AL_IsFinalCharge
					&& !Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed)
				{
					Parent.AL_IsFinalChargeInfo.AddError(Res.GetString("CBBDCD0E-4EE1-412A-9047-F80B1F1E208D", @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice"));
				}

				if (!Parent.IsInDatabase && Parent.APInvoice != null && Parent.APInvoice.CostVarianceApprovalHelper.AutoTickFinalFlag
					&& !Parent.AL_IsFinalCharge && Parent.AL_IsFinalChargeDefault
					&& !Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed)
				{
					Parent.AL_IsFinalChargeInfo.AddError(Res.GetString("447FD59F-5FE5-46C3-95E1-921D3978CB0C", @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag"));
				}
			}
		}

		#endregion
		#endregion
	}
}