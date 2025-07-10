using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ARTransactionStampDutyExtensions
	{
		public static bool ShouldAddStampDuty(this InvoicingBase invoice)
		{
			var regStampDutyRecharge = AccountingConfigurationRegistry.Instance.StampDutyRecharge.Value;
			switch (regStampDutyRecharge.StampDutyRechargeOrganizationType)
			{
				case Core.Constants.StampDutyRechargeOrganizationType.All:
					break;

				case Core.Constants.StampDutyRechargeOrganizationType.LocalOrganizations:
					if (invoice.Header == null ||
						invoice.Header.UNLOCO == null ||
						invoice.Header.UNLOCO.Country == null ||
						invoice.Header.UNLOCO.Country.Code != Core.Constants.CountryCodes.Italy)
					{
						return false;
					}
					break;

				case Core.Constants.StampDutyRechargeOrganizationType.NotRecharging:
					return false;
			}

			switch (regStampDutyRecharge.StampDutyRechargeTransactionType)
			{
				case Core.Constants.StampDutyRechargeTransactionType.All:
					break;

				case Core.Constants.StampDutyRechargeTransactionType.ARInvoice:
					if (!(invoice is ARInvoice))
					{
						return false;
					}
					break;
			}

			if (invoice.Header == null ||
				invoice.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(ItalyOrgCusCodeInfo.OrgCusCodes.NBO, Core.Constants.CountryCodes.Italy) != null)
			{
				return false;
			}

			return invoice.IsStampDutyApplicable();
		}

		public static bool IsStampDutyAmountGreaterThanCreditNoteTotal(this InvoicingBase invoice)
		{
			return invoice is CreditNote && Math.Abs(invoice.AH_LocalTotal) < AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.Value;
		}

		public static bool IsStampDutyChargeLine(this InvoicingLineBase line)
		{
			if (!(line is ARInvoiceLine || line is ARCreditNoteLine))
			{
				return false;
			}
			else
			{
				var registry = AccountingConfigurationRegistry.Instance;
				var regStampDutyChargeCode = registry.StampDutyChargeCode.Value;
				return (regStampDutyChargeCode != Guid.Empty && line.AL_AC == regStampDutyChargeCode &&
					Math.Abs(line.AL_LocalExTaxAmount) == registry.StampDutyFixedAmount.Value);
			}
		}

		public static InvoicingLineBase AddStampDutyLine(this InvoicingBase invoice, bool useNegativeStampDuty = false)
		{
			var registry = AccountingConfigurationRegistry.Instance;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.SetContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation);
			line.GenericCharge = registry.StampDutyChargeCode.Value;
			line.AL_A9_VATClass = registry.StampDutyInvoiceTaxMessage.Value;

			SetLocalLineDescription(invoice, line);

			int multiplier = useNegativeStampDuty ? -1 : 1;
			line.AL_LocalExTaxAmount = multiplier * registry.StampDutyFixedAmount.Value;

			return line;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		static void SetLocalLineDescription(InvoicingBase invoice, InvoicingLineBase line)
		{
			var accChargeCode = invoice.Factory.Load<AccChargeCode>(line.AL_AC);
			var debtor = invoice.Factory.Load<OrgHeader>(invoice.AH_OH);

			var isDebtorInSameCountry = debtor != null && debtor.IsLocalClosestPort;

			var registry = AccountingConfigurationRegistry.Instance;
			var shouldUseLocalLineDescription = registry.EnableLocalChargeCodeDescriptionDefault.Value &&
					accChargeCode != null &&
					!accChargeCode.AC_LocalLanguageDescription.IsEmpty &&
					(isDebtorInSameCountry || registry.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);

			if (shouldUseLocalLineDescription && line.AL_Desc == accChargeCode.AC_Desc)
			{
				line.AL_Desc = accChargeCode.AC_LocalLanguageDescription;
			}
		}

		public static bool IsStampDutyApplicable(this InvoicingBase invoice)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Italy ||
				invoice.IsReverseTransaction || invoice.IsReversing || invoice.IsInDatabase ||
				!(invoice is ARInvoice || invoice is ARCreditNote))
			{
				return false;
			}

			var associatedInvoices = invoice.GetAllAssocoiatedInvoices();

			return StampDutyNotAppliedAndAmountExceedsThreshold(associatedInvoices);
		}

		#region Implementation

		static List<InvoicingBase> GetAllAssocoiatedInvoices(this InvoicingBase invoice)
		{
			var result = new List<InvoicingBase>() { invoice };

			var amending = invoice as IAmending;
			if (amending != null && amending.IsAmendingTransaction && amending.OriginalTransaction != null)
			{
				var associatedInvoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, amending.OriginalTransaction.PK);
				associatedInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice.AH_GC);
				result = invoice.Factory.Load<InvoicingBase>(associatedInvoicesFilter).ToList();
				result.Add((InvoicingBase)amending.OriginalTransaction);
			}

			return result;
		}

		static bool StampDutyNotAppliedAndAmountExceedsThreshold(List<InvoicingBase> associatedInvoices)
		{
			var result = !IsStampDutyAlreadyApplied(associatedInvoices);

			if (result)
			{
				result = TotalExTaxAmountAttractingStampDuty(associatedInvoices) > AccountingConfigurationRegistry.Instance.StampDutyThreshold.Value;
			}

			return result;
		}

		static bool IsStampDutyAlreadyApplied(List<InvoicingBase> invoices)
		{
			var result = false;

			if (invoices.Any())
			{
				var findEvent = new ZQuery(StmALogSchema.SL_Parent, invoices.Select(x => x.PK));
				findEvent.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StampDutyLiabilityCode);
				result = invoices.First().Factory.LoadTop1<StmALog>(findEvent) != null;

				if (!result)
				{
					foreach (var invoice in invoices)
					{
						if (invoice.Lines.Cast<InvoicingLineBase>().Any(x => x.IsStampDutyChargeLine()))
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		static ZDecimal TotalExTaxAmountAttractingStampDuty(List<InvoicingBase> invoices)
		{
			ZDecimal total = 0m;

			Guid[] taxIDsAttractingStampDuty = AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.GetAsGuidArray();
			if (taxIDsAttractingStampDuty.Any())
			{
				foreach (var invoice in invoices)
				{
					total += invoice.Lines.Cast<InvoicingLineBase>().
						Where(x => x.AL_AT.IsValid && taxIDsAttractingStampDuty.Contains(x.AL_AT.ToGuid())).
						Sum(y => Math.Abs(y.AL_LocalExTaxAmount));
				}
			}

			return total;
		}

		#endregion
	}
}
