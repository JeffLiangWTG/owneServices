using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CustomsDSBChargePostValidator
	{
		public CustomsDSBChargePostValidator(ChargePosterBehaviours behaviours, ZGuid[] customsDSBChargeCodes)
		{
			this.behaviours = behaviours;
			this.customsDSBChargeCodes = customsDSBChargeCodes;
		}

		readonly ChargePosterBehaviours behaviours;
		readonly ZGuid[] customsDSBChargeCodes;

		CUSDSBPostingValidationResult ValidateRegistryItems(ZGuid creditorPK, GlbBranch branch, AutoPostingNotification autoRatingNotification)
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult(ZString.Empty, autoRatingNotification);

			if (branch != null)
			{
				Guid companyPK = branch.GB_GC.ToGuid();
				ZString companyCode = branch.Company.GC_Code;

				ValidateCustomsDisbursementCreditor(creditorPK, branch, result);
				ValidateChargeCode(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty), branch, result, GetCustomsDisbursementChargeCodeNotSet(companyCode), GetCustomsDisbursementChargeCodeNotValid(companyCode));
				if (RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty))
				{
					ValidateChargeCode(RatingDataRegistry.Instance.CustomDeferredChargeCode.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty), branch, result, GetCustomDeferredChargeCodeNotSet(companyCode), GetCustomDeferredChargeCodeNotValid(companyCode));
				}
			}

			return result;
		}

		void ValidateChargeCode(Guid chargeCodePK, GlbBranch branch, CUSDSBPostingValidationResult result, ZString errorMessageNotSet, ZString errorMessageNotValid)
		{
			if (chargeCodePK == Guid.Empty)
			{
				result.AddError(errorMessageNotSet);
			}
			else
			{
				AccChargeCode chargeCode = branch.Factory.Load<AccChargeCode>(chargeCodePK);
				if (chargeCode == null || chargeCode.IsDeleted)
				{
					result.AddError(errorMessageNotValid);
				}
			}
		}

		void ValidateCustomsDisbursementCreditor(ZGuid creditorPK, GlbBranch branch, CUSDSBPostingValidationResult result)
		{
			var error = GetCustomsDisbursementCreditorValidationError(creditorPK, branch.Company);

			if (!string.IsNullOrEmpty(error))
			{
				result.AddError(error);
			}
		}

		internal static string GetCustomsDisbursementCreditorValidationError(ZGuid creditorPK, GlbCompany company)
		{
			var result = "";
			if (!creditorPK.IsValid)
			{
				result = GetCustomsDisbursementCreditorNotSet(company.GC_Code);
			}
			else
			{
				OrgHeader org = company.Factory.Load<OrgHeader>(creditorPK);
				if (org == null || org.IsDeleted)
				{
					result = GetCustomsDisbursementCreditorNotValid(company.GC_Code);
				}
				else if (org != null)
				{
					var companyDataQuery = new ZQuery(OrgCompanyDataSchema.OB_GC, company.PK);
					companyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, creditorPK);

					var companyData = company.Factory.LoadTop1<OrgCompanyData>(companyDataQuery);
					if (companyData != null && companyData.OB_APCostsSelfBilled)
					{
						result = Res.GetString("549cfc2f-ee0b-42e0-919b-d8e9fc21e503", "The Customs Disbursement Creditor '{0}' should not be configured to issue self-billing invoices. The configuration to enable self-billing invoices is on the A/P tab for the Customs Disbursement Creditor.", org.OH_Code);
					}
				}
			}

			return result;
		}

		public static string GetCustomsDisbursementChargeCodeNotSet(ZString companyCode)
		{
			return Res.GetString("4b1dd33c-bbe7-4430-89c5-86e725c6b79b", "Customs Disbursement Charge Code is not set in the registry for the company '{0}'.", companyCode);
		}

		public static string GetCustomsDisbursementChargeCodeNotValid(ZString companyCode)
		{
			return Res.GetString("53DF6082-43B4-4081-8D07-7FCF5A853B83", "The Customs Disbursement Charge Code set up in the registry for the company '{0}' is not valid.", companyCode);
		}

		public static string GetCustomsDisbursementCreditorNotSet(ZString companyCode)
		{
			return Res.GetString("94e585f5-e3bf-4eef-800e-a8ce692cea90", "Customs Disbursement Creditor is not set in the registry for the company '{0}'.", companyCode);
		}

		public static string GetCustomsDisbursementCreditorNotValid(ZString companyCode)
		{
			return Res.GetString("DC4FDF10-F908-4e67-AA49-81E509217FF2", "The Customs Disbursement Creditor set up in the registry for the company '{0}' is not valid.", companyCode);
		}

		public static string GetCustomDeferredChargeCodeNotSet(ZString companyCode)
		{
			return Res.GetString("78c82651-0049-434d-9e90-8c42ad5739f9", "'Include Custom Deferred Charge in Invoicing' is enabled in the registry, but the corresponding Custom Deferred Charge Code is not set up in the registry for '{0}'.", companyCode);
		}

		public static string GetCustomDeferredChargeCodeNotValid(ZString companyCode)
		{
			return Res.GetString("8B72A049-0D5E-481c-BAA7-46152AF932A3", "'Include Custom Deferred Charge in Invoicing' is enabled in the registry, but the corresponding Custom Deferred Charge Code set up in the registry for '{0}' is not valid.", companyCode);
		}

		public CUSDSBPostingValidationResult ValidateFor(IAccInvoiceDataProvider dataProvider, AutoRateInfoCollection autoRateInfos)
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult(dataProvider.UniqueNumber, dataProvider.AutoPostingNotification);

			ICustomsJobInfo customsJob = dataProvider.CustomsJob;
			var creditorPK = customsJob?.CreditorPK ?? ZGuid.Empty;
			if (customsJob == null || !dataProvider.IsBillable)
			{
				result.AddError(dataProvider.ReasonForUnbillability);
			}
			else
			{
				result.Merge(ValidateRegistryItems(creditorPK, customsJob.Branch, dataProvider.AutoPostingNotification));
			}

			if (!result.HasErrors && customsJob != null)
			{
				Job invoicingJob = LoadJobFromDatabaseIfExists(customsJob);

				if (invoicingJob != null)
				{
					var charges = invoicingJob.Charges;
					if (dataProvider.HasBeenWithdrawn && charges.HasPostedAROrAP(customsDSBChargeCodes))
					{
						string withdrawnStatusTerm = dataProvider.EntryWithdrawnStatusTerm;

						result.AddError(Res.GetString("9a312976-3789-47aa-8dee-19fba7443e34", "The entry has been {0}, however there is at least one posted AR or AP for disbursement charges. Please reverse the invoice.", withdrawnStatusTerm));
					}
					else
					{
						APInvoice[] apInvoices = new APInvoice.Loader(invoicingJob.Factory).LoadNotReversed(dataProvider.UniqueNumber, creditorPK, invoicingJob.PK);

						if (apInvoices.Length > 0)
						{
							CheckIfChargeDetailsAreSame(apInvoices, autoRateInfos, result, invoicingJob);
						}

						CheckExistingJobCharge(autoRateInfos, charges, result, dataProvider);
					}
				}
			}

			return result;
		}

		public CUSDSBPostingValidationResult ValidateFor(ICustomsJobInfo customsJob, AutoRateInfoCollection autoRateInfos)
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult(customsJob.JobNumber, customsJob.AutoPostingNotification);

			Job invoicingJob = LoadJobFromDatabaseIfExists(customsJob);

			if (invoicingJob != null)
			{
				var uniqueChargeCodePKs = autoRateInfos.Cast<AutoRateInfo>().Where(x => x.ChargeCode != null).Select(x => x.ChargeCode.PK).Distinct().ToArray();
				ARInvoice[] arInvoices = new ARInvoice.Loader(invoicingJob.Factory).LoadNotReversed(invoicingJob.PK, uniqueChargeCodePKs);

				if (arInvoices.Length > 0 && autoRateInfos != null)
				{
					CheckIfChargeDetailsAreSame(arInvoices, autoRateInfos, result, invoicingJob);
				}
			}

			return result;
		}

		bool ShouldAPPostDSB
		{
			get { return (behaviours & ChargePosterBehaviours.APPostDSB) == ChargePosterBehaviours.APPostDSB; }
		}

		bool ShouldARPostDSB
		{
			get { return (behaviours & ChargePosterBehaviours.ARPostDSB) == ChargePosterBehaviours.ARPostDSB; }
		}

		bool ShouldARAPPostNonDSB
		{
			get { return (behaviours & ChargePosterBehaviours.ARAPPostNonDSB) == ChargePosterBehaviours.ARAPPostNonDSB; }
		}

		Job LoadJobFromDatabaseIfExists(ICustomsJobInfo customsJob)
		{
			return new Job.Loader(customsJob.TopLevelObjectForJobToReference).Load();
		}

		void CheckExistingJobCharge(AutoRateInfoCollection autoRateInfos, ChargeCollection charges, CUSDSBPostingValidationResult validationResult, IAccInvoiceDataProvider provider)
		{
			ZStringBuilder duplicateChargeCodes = new ZStringBuilder();

			List<ZGuid> notAutoRatedCustomsDSBChargeCodes = new List<ZGuid>(this.customsDSBChargeCodes);

			foreach (AutoRateInfo rateInfo in autoRateInfos)
			{
				notAutoRatedCustomsDSBChargeCodes.Remove(rateInfo.ChargeCode.PK);

				List<Charge> existingCharges = charges.GetMatchingUnapportionedChargesFromThisJobOnly(rateInfo.ChargeCode.PK, provider.UniqueNumber, provider.PreviousUniqueNumber);

				if (existingCharges.Count(x => !x.IsCostPosted && !x.IsRevenuePosted) > 1)
				{
					if (ShouldAPPostDSB && rateInfo.LocalAmount != existingCharges.Sum(charge => charge.JR_LocalCostAmt)
						|| ShouldARPostDSB && rateInfo.LocalAmount != existingCharges.Sum(charge => charge.JR_LocalSellAmt))
					{
						duplicateChargeCodes.Append(rateInfo.ChargeCode.AC_Code);
					}
				}

				CheckIfCustomsDSBChargeAmountsMatch(existingCharges, rateInfo.LocalAmount, validationResult);
			}

			if (notAutoRatedCustomsDSBChargeCodes.Count > 0)
			{
				foreach (ZGuid chargeCode in notAutoRatedCustomsDSBChargeCodes)
				{
					List<Charge> existingCharges = charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCode, provider.UniqueNumber, provider.PreviousUniqueNumber);

					CheckIfAutoRateAmountsMatchExistingCustomsDSBCharge(existingCharges, 0m, validationResult);
				}
			}

			if (!duplicateChargeCodes.IsEmpty)
			{
				validationResult.AddError(GetMoreThanOneCustomsDSBCharges(duplicateChargeCodes.ToStringWithDelimiterBetweenAppends(", ")));
			}
		}

		void CheckIfCustomsDSBChargeAmountsMatch(List<Charge> existingCharges, ZDecimal autoRatedAmount, CUSDSBPostingValidationResult validationResult)
		{
			if (existingCharges.Count > 0 && ShouldARAPPostNonDSB)
			{
				ZDecimal existingAmount = 0m;

				foreach (Charge customsCharge in existingCharges)
				{
					existingAmount += customsCharge.JR_LocalCostAmt;
				}

				if (existingAmount != autoRatedAmount)
				{
					Money existingAmountMoney = new Money(existingAmount, GlbCompany.CurrentCompany.LocalCurrency);
					Money ratedAmountMoney = new Money(autoRatedAmount, GlbCompany.CurrentCompany.LocalCurrency);
					validationResult.AddError(Res.GetString("d9e2b46b-d8ae-4b1c-987f-0b152a78b538", "The existing Customs Disbursement amount({0}) differs from a newly auto-rated amount({1}) and the job potentially needs another review. The system will auto-rate for Customs Disbursement. However you should auto-rate for other charges manually.", existingAmountMoney, ratedAmountMoney));
				}
			}
		}

		void CheckIfAutoRateAmountsMatchExistingCustomsDSBCharge(List<Charge> existingCharges, ZDecimal autoRatedAmount, CUSDSBPostingValidationResult validationResult)
		{
			if (existingCharges.Count > 0 && ShouldARAPPostNonDSB && autoRatedAmount == 0)
			{
				ZStringBuilder charges = new ZStringBuilder();
				existingCharges.ForEach(x => charges.Append(x.JR_DisplaySequence.ToString()));

				validationResult.AddError(Res.GetString("d9e2b46b-d8ae-4b1c-987f-0d152a78b540", "The Customs DSB charges lines with the following sequence numbers, '{0}' are not relevant any more. Please remove them. The job potentially needs another review and auto-rate for other charges manually.", charges.ToStringWithDelimiterBetweenAppends(", ")));
			}
		}

		public static string GetMoreThanOneCustomsDSBCharges(ZString chargeCodes)
		{
			return Res.GetString("dbf57b2e-7394-4362-a831-362db8dee43e", "This Job has more than one charge line for each of the following Charge Codes: {0}. The sum of the amounts of these charge lines (grouped per charge code) differ from the actual Customs Disbursement amount represented by the charge code. Automatically processing failed.", chargeCodes);
		}

		void PopulateExistingAmountFromInvoice(Dictionary<ZString, ChargesSummary> summary, Invoice[] invoices, List<ZGuid> chargeCodesToCheck, Job job)
		{
			foreach (Invoice invoice in invoices)
			{
				foreach (InvoiceLine line in invoice.Lines)
				{
					if (chargeCodesToCheck.Contains(line.AL_AC) && line.AL_JH == job.PK)
					{
						if (!summary.ContainsKey(line.ChargeCode.AC_Code))
						{
							summary.Add(line.ChargeCode.AC_Code, new ChargesSummary());
						}

						ChargesSummary chargeSummary = summary[line.ChargeCode.AC_Code];
						chargeSummary.ChargeCode = line.ChargeCode.AC_Code;

						ZString[] desc = line.AL_Desc.Split(System.Environment.NewLine.ToCharArray(), 3);
						chargeSummary.Description = desc[0];
						if (desc.Length > 2)
						{
							chargeSummary.AdditionalDescription = desc[2];
						}

						chargeSummary.ExistingExTaxAmount += line.AL_LocalExTaxAmount;
						chargeSummary.ExistingTaxAmount += line.AL_LocalTaxAmount;
					}
				}
			}
		}

		void PopulateNewAmountFromAutoRatedAmount(Dictionary<ZString, ChargesSummary> summary, AutoRateInfoCollection autoRateInfos)
		{
			foreach (AutoRateInfo info in autoRateInfos)
			{
				if (!summary.ContainsKey(info.ChargeCode.AC_Code))
				{
					summary.Add(info.ChargeCode.AC_Code, new ChargesSummary());
				}

				ChargesSummary chargeSummary = summary[info.ChargeCode.AC_Code];
				chargeSummary.ChargeCode = info.ChargeCode.AC_Code;

				if (chargeSummary.Description.IsEmpty)
				{
					chargeSummary.Description = info.Description;
				}

				if (chargeSummary.AdditionalDescription.IsEmpty)
				{
					chargeSummary.AdditionalDescription = info.AdditionalInvoiceLineDescription;
				}

				chargeSummary.NewExTaxAmount += info.Amount;
				chargeSummary.NewTaxAmount += info.OverriddenGSTAmount;
			}
		}

		void CheckIfChargeDetailsAreSame(Invoice[] invoices, AutoRateInfoCollection autoRateInfos, CUSDSBPostingValidationResult validationResult, Job job)
		{
			Dictionary<ZString, ChargesSummary> summary = new Dictionary<ZString, ChargesSummary>();

			var uniqueChargeCodePKs = autoRateInfos.Cast<AutoRateInfo>().Where(x => x.ChargeCode != null).Select(x => x.ChargeCode.PK).Distinct().ToList();
			PopulateExistingAmountFromInvoice(summary, invoices, uniqueChargeCodePKs, job);

			PopulateNewAmountFromAutoRatedAmount(summary, autoRateInfos);

			bool areChargeDetailsSame = true;

			foreach (ChargesSummary chargeSummary in summary.Values)
			{
				if ((chargeSummary.ExistingExTaxAmount != chargeSummary.NewExTaxAmount) ||
					(!chargeSummary.NewTaxAmount.IsEmpty && chargeSummary.ExistingTaxAmount != chargeSummary.NewTaxAmount))
				{
					areChargeDetailsSame = false;
					break;
				}
			}

			if (!areChargeDetailsSame)
			{
				ZString invoiceDetail = GetInvoiceCreationDetails(invoices);
				if (invoices.Any(invoice => invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable))
				{
					validationResult.APInvoiceDetail = invoiceDetail;
					validationResult.AddAPDiscrepancyCharge(new List<ChargesSummary>(summary.Values).ToArray());
				}
				else
				{
					validationResult.ARInvoiceDetail = invoiceDetail;
					validationResult.AddARDiscrepancyCharge(new List<ChargesSummary>(summary.Values).ToArray());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		string GetInvoiceCreationDetails(Invoice[] invoices)
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (Invoice invoice in invoices)
			{
				if (!result.IsEmpty)
				{
					result.Append("/");
				}

				result.Append(" " + Res.GetString("3a6d98f4-ee6a-48f3-958f-37bb983b5d76", "(Created {0}, by {1}", invoice.CreatedDate.ToShortDateString(), invoice.CreatingUser));

				result.Append(Res.GetString("5e3e355c-e22c-49a6-ad15-91844a940164", ", Invoice Num.: {0}", invoice.AH_TransactionNum));

				if (invoice.Creator != null && invoice.Creator.GS_Code != "~BP")
				{
					result.Append(Res.GetString("67e53dcd-efdc-4dfb-ba64-68b7c4a49845", ", Tel: {0}, Email: {1}", invoice.Creator.GS_WorkPhone, invoice.Creator.GS_EmailAddress));
				}

				result.Append(")");
			}
			return result.ToString();
		}
	}
}
