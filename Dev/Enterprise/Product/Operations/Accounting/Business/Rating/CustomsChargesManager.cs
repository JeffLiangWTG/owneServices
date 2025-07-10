using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class CustomsChargesManager
	{
		readonly IBusiness parent;
		readonly ILogger interactor;

		public CustomsChargesManager(IBusiness parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		public CustomsChargesManager(IBusiness parent, ILogger interactor) : this(parent)
		{
			this.interactor = interactor;
		}

		public AutoRateInfoCollection RateCustomsCharges(ICustomsCharges[] customsJobsToRate)
		{
			if (customsJobsToRate != null && customsJobsToRate.Any())
			{
				var customsCharges = customsJobsToRate.SelectMany(x => x.GetCustomsCharges(interactor)).ToList();
				return GetRateInfosForCustomsCharges(customsCharges);
			}

			return new AutoRateInfoCollection(parent.Factory);
		}

		public void PopulateChargeCodesForEmptyCustomsCharges(ICustomsCharges[] customsJobsToRate)
		{
			foreach (ICustomsCharges customsCharge in customsJobsToRate)
			{
				foreach (CustomsCharge charge in customsCharge.GetCustomsCharges(interactor))
				{
					SetDisbursementOrDeferredChargeCode(charge);
				}
			}
		}

		public IList<ChargeWrapper> DeleteCustomsChargesIfNecessary(Job job, AutoRateInfoCollection autoRatingResults)
		{
			var deletedCustomsCharges = new List<ChargeWrapper>();

			if (job == null)
			{
				return deletedCustomsCharges;
			}

			var parentAsIProvider = parent as ICustomsJobInfoProvider;
			if (parentAsIProvider == null)
			{
				return deletedCustomsCharges;
			}

			var customsJob = parentAsIProvider.GetCustomsJobInfo(job.JH_GC);
			if (customsJob == null || customsJob.Entries.Count <= 0)
			{
				return deletedCustomsCharges;
			}

			// This code looks suspicious. Basically, it deletes customs charges if new matching charges have been autorated. But:
			//
			// 1. When updating/creating charges results on a job, autorating logic assumes a new autorated charge may be matched
			//    with only one existing charge. Check usage of FindBestMatches in AutoRateInvoicingStrategy for more details.
			//	  Logic here assumes that multiple existing charges may be matched with a newly autorated charge and will keep all of them
			//	  which may lead to side-effects.
			//
			// 2. An existing charge being deleted here may be used by a Percentage calculator (percentage of an existing charge). So,
			//	  assume a case if we have some customs charges on a job then we autorate, there are some rates with percentage calculator
			//	  which will be calculated using amounts on existing charges, and then we delete these customs charges here. So, basically
			//	  charges created from rates with percentage calculator will be calculated based on deleted charges.

			var customsCharges = job.Charges
				.Where(charge => charge.IsCustomsCharge)
				.Where(charge => charge.JR_JH == job.PK)
				.Where(charge => !charge.JR_IsApportioned && !charge.IsCostPosted && !charge.IsRevenuePosted)
				.Cast<BaseCharge>()
				.ToArray();

			foreach (var charge in customsCharges)
			{
				var matches = autoRatingResults.FindBestMatches(new[] { charge }, CostSell.Revenue);
				if (matches.Any())
				{
					continue;
				}

				deletedCustomsCharges.Add(new ChargeWrapper(charge, null));
				charge.Delete();
			}

			return deletedCustomsCharges;
		}

		#region Private methods

		int GetLocalCurrencyDecimals(ZString localCurrenyCode)
		{
			if (!localCurrenyCode.IsEmpty)
			{
				var refCurrency = RefCurrency.LoadFromCurrencyCode(parent.Factory, localCurrenyCode);
				if (refCurrency != null)
				{
					return refCurrency.Decimals;
				}
			}
			return GlbCompany.CurrentCompany.LocalCurrency.Decimals;
		}

		AutoRateInfoCollection GetRateInfosForCustomsCharges(List<CustomsCharge> customsCharges)
		{
			var result = new AutoRateInfoCollection(parent.Factory);
			var rateInfoDictionary = new Dictionary<CustomsChargeKey, AutoRateInfo>();

			PopulateChargeCodeOrRemoveCustomsCharges(customsCharges);

			foreach (CustomsCharge charge in customsCharges)
			{
				AddNewOrUpdateExistingRateInfo(charge, rateInfoDictionary);
			}

			if (RatingDataRegistry.Instance.HideValueBreakdownInDescription.Value)
			{
				foreach (CustomsChargeKey key in rateInfoDictionary.Keys)
				{
					AutoRateInfo autoRateInfo;

					if (rateInfoDictionary.TryGetValue(key, out autoRateInfo))
					{
						ZDecimal amount = 0m;

						foreach (CustomsCharge charge in customsCharges)
						{
							if (charge.IsInformationOnly && new CustomsChargeKey(charge).Equals(key))
							{
								amount += charge.Amount;
							}
						}

						if (amount > 0m)
						{
							autoRateInfo.AdditionalInvoiceLineDescription += " " + Res.GetString("fc9b1ddd-1d55-4c49-8969-9b15ece9545c", "(Total:") + " " + amount.ToString(GetLocalCurrencyDecimals(autoRateInfo.Currency)) + ")";
						}
					}
				}
			}

			foreach (AutoRateInfo rateInfo in rateInfoDictionary.Values)
			{
				if (!ShowAdditonalLinesDescription(rateInfo, customsCharges))
				{
					rateInfo.AdditionalInvoiceLineDescription = ZString.Empty;
				}

				result.Add(rateInfo);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		bool ShowAdditonalLinesDescription(AutoRateInfo rateInfo, IList<CustomsCharge> customsCharges)
		{
			AccChargeCode chargeCode = rateInfo.ChargeCode;
			if (chargeCode == null)
			{
				return false;
			}

			if (chargeCode.PK != DisbursementChargeCodePK && chargeCode.PK != DeferredChargeCodePK)
			{
				string onlyChargeCodeDescriptionUsed = "";
				int numberOfAggregatedCharges = 0;

				foreach (CustomsCharge charge in customsCharges)
				{
					if (charge.ChargeCodePK == chargeCode.PK)
					{
						onlyChargeCodeDescriptionUsed = charge.Description;
						numberOfAggregatedCharges++;
					}
				}

				if (numberOfAggregatedCharges == 1)
				{
					if (chargeCode.AC_Desc.Contains(onlyChargeCodeDescriptionUsed))
					{
						return false;
					}
				}
			}

			return true;
		}

		void PopulateChargeCodeOrRemoveCustomsCharges(List<CustomsCharge> customsCharges)
		{
			var chargesToRemoveFromList = new List<CustomsCharge>();

			foreach (CustomsCharge charge in customsCharges)
			{
				SetDisbursementOrDeferredChargeCode(charge);

				if (!charge.ChargeCodePK.IsValid || (charge.IsInformationOnly && !IncludeCustomDeferredChargeInInvoicing))
				{
					chargesToRemoveFromList.Add(charge);
				}
			}

			foreach (CustomsCharge charge in chargesToRemoveFromList)
			{
				customsCharges.Remove(charge);
			}
		}

		void SetDisbursementOrDeferredChargeCode(CustomsCharge charge)
		{
			// SuppressResourceStrings justification: The texts in the region are for logging purpose.
			#region SuppressResourceStringsCheckRegion

			// Default path where charge codes are loaded by charge type.
			var registryPath = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.HumanReadableRegistryPath();

			if (!charge.ChargeCodePK.IsValid && charge.IsPaidByBroker)
			{
				charge.ChargeCodePK = DisbursementChargeCodePK;
				registryPath = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.HumanReadableRegistryPath();
			}
			else if (charge.IsInformationOnly)
			{
				charge.ChargeCodePK = IncludeCustomDeferredChargeInInvoicing ? DeferredChargeCodePK : ZGuid.Empty;
				registryPath = RatingDataRegistry.Instance.CustomDeferredChargeCode.HumanReadableRegistryPath();
			}

			// There were cases that DisbursementChargeCode on the registry was from another company.
			// The disbursement charge was created but there was an error on charge code column saying: "Enter a valid Charge Code." and "This charge code is not valid for the current company."
			// then there were also developer error reports.
			// UI validations do not let user enter the invalid charge code in registry.
			// However, products could see they are invalid in the registry when logging in client system.
			var chargeCode = charge.GetChargeCode(parent.Factory);
			if (chargeCode != null && chargeCode.AC_GC != Env.CurrentCompanyPK)
			{
				charge.ChargeCodePK = ZGuid.Empty;
				// Autorating ASP can also get into here with charge code from `AutoRating > Charge Codes > Customs > Australia > Default Quarantine Charge Code`
				// but there is no interactor (null) in that case.
				interactor?.Warning($"Customs charge filtered. Reason: the charge code '{chargeCode.AC_Code}' in '{registryPath}' is not valid for autorating in the current login company.");
			}

			#endregion
		}

		void AddNewOrUpdateExistingRateInfo(CustomsCharge charge, Dictionary<CustomsChargeKey, AutoRateInfo> rateInfoDictionary)
		{
			ValidateCustomsDisbursementChargeCodeOrCustomsDeferredChargeCode(charge);

			var key = new CustomsChargeKey(charge);

			if (!rateInfoDictionary.TryGetValue(key, out var rateInfo))
			{
				rateInfo = GetNewRateInfo(charge);
				if (charge.DebtorPK.IsValid)
				{
					rateInfo.DebtorOverridePK = charge.DebtorPK;
				}

				if (!charge.OverrideCurrency.IsEmpty)
				{
					rateInfo.Currency = charge.OverrideCurrency;
				}

				if (charge.ChargeCodePK.ToGuid() == RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value)
				{
					AppendStringToCalculationDescription(rateInfo, Res.GetString("2486fe55-f509-49ed-9469-e3a33316d7ef", "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.") + System.Environment.NewLine);
				}

				rateInfoDictionary[key] = rateInfo;
			}

			AddAdditionalDescription(rateInfo, charge);
			AddPaymentBasis(rateInfo, charge);
			rateInfo.ResetDescription = charge.ChargeCodePK == DeferredChargeCodePK;

			if (!charge.IsInformationOnly)
			{
				rateInfo.UseOverriddenGSTAmount = true;
				rateInfo.OverriddenGSTAmount += charge.GST;
			}
			else
			{
				rateInfo.HasExplicitZeroAmount = true;
			}
		}

		void ValidateCustomsDisbursementChargeCodeOrCustomsDeferredChargeCode(CustomsCharge charge)
		{
			var chargeCode = charge.GetChargeCode(parent.Factory);
			if (chargeCode != null)
			{
				return;
			}

			var chargeCodePK = charge.ChargeCodePK;
			if (chargeCodePK == RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value)
			{
				var errorMessage = (NoResString)"<p>"
					+ Res.GetString("d695d77a-b982-43a1-90ac-d4aeaacad0b9", "You must set up the 'Customs Disbursement Charge Code' in the registry before Autorating can be run.")
					+ (NoResString)"</p>";

				throw new CustomsInvoiceRaiseException(errorMessage);
			}

			if (chargeCodePK == RatingDataRegistry.Instance.CustomDeferredChargeCode.Value)
			{
				var errorMessage = (NoResString)"<p>"
					+ Res.GetString("5fc5e18c-6c6a-4db7-ab86-c6f10b12c16d", @"You have enabled 'Include Custom Deferred Charge in Invoicing' in the registry, but the corresponding 'Custom Deferred Charge' is not set up in the registry.
Please set up 'Custom Deferred Charge' or disable 'Include Custom Deferred Charge in Invoicing' in the registry.")
					+ (NoResString)"</p>";

				throw new CustomsInvoiceRaiseException(errorMessage);
			}
		}

		void AddPaymentBasis(AutoRateInfo rateInfo, CustomsCharge charge)
		{
			var reference = ZString.Empty;
			if (parent is IRatingSupporterWithAdapter supporterWithAdapter)
			{
				reference = supporterWithAdapter.RatingAdapter.OperationalJobCode;
			}
			else if (!charge.EntryReference.IsEmpty)
			{
				reference = charge.EntryReference.Split(' ').FirstOrDefault();
			}

			var quantity = Quantity.Empty(charge.EntryReference);
			var flatRateInfo = RateInfo.CreateFLT(charge.IsInformationOnly ? 0 : charge.Amount, rateInfo.Currency);
			var customsChargeBasis = new PaymentBasis(quantity, flatRateInfo, AdapterType.CustomsResponseMessage, reference);
			rateInfo.Bases.Add(customsChargeBasis);
			rateInfo.AgentBases.Add(customsChargeBasis);
		}

		bool IncludeCustomDeferredChargeInInvoicing
		{
			get { return RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.Value; }
		}

		void AddAdditionalDescription(AutoRateInfo rateInfo, CustomsCharge charge)
		{
			if (!RatingDataRegistry.Instance.HideValueBreakdownInDescription.Value)
			{
				ZString description = charge.Description.PadRight(35);
				ZString amount = charge.Amount.ToString(GetLocalCurrencyDecimals(charge.OverrideCurrency)).PadLeft(12);

				if (charge.Amount > 0 && !description.IsEmpty)
				{
					ZString additionalDescription = "  " + description + " " + amount;
					AppendStringToAdditionalDescription(rateInfo, additionalDescription);

					foreach (var info in charge.AdditionalChargeInfos)
					{
						AppendStringToAdditionalDescription(rateInfo, info.ToString());
					}
				}

				if (charge.GST > 0)
				{
					ZString taxDescription = new ZString("+ " + GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription).PadRight(35);
					ZString taxAmount = charge.GST.ToString(GetLocalCurrencyDecimals(charge.OverrideCurrency)).PadLeft(12);

					if (charge.GST > 0m && !taxDescription.IsEmpty)
					{
						ZString additionalTaxDescription = "  " + taxDescription + " " + taxAmount;
						AppendStringToAdditionalDescription(rateInfo, additionalTaxDescription);
					}
				}
			}
		}

		void AppendStringToAdditionalDescription(AutoRateInfo rateInfo, ZString additionalDescription)
		{
			if (!rateInfo.AdditionalInvoiceLineDescription.IsEmpty)
			{
				rateInfo.AdditionalInvoiceLineDescription += System.Environment.NewLine;
			}

			rateInfo.AdditionalInvoiceLineDescription += additionalDescription;
		}

		void AppendStringToCalculationDescription(AutoRateInfo rateInfo, ZString additionalDescription)
		{
			var description = rateInfo.Description;

			if (!description.IsEmpty)
			{
				description += System.Environment.NewLine;
			}

			rateInfo.Description = description + additionalDescription;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		AutoRateInfo GetNewRateInfo(CustomsCharge charge)
		{
			var entryReference = charge.EntryReference;
			var creditorPK = charge.CreditorPK;
			var chargeCodePK = charge.ChargeCodePK;
			var chargeCode = parent.Factory.Load<AccChargeCode>(chargeCodePK);

			CheckAndReportErrorForGetNewAutoRatingInfo(chargeCodePK, chargeCode);

			var rateInfo = new AutoRateInfo(parent.Factory) { ChargeCode = chargeCode };

			if (chargeCodePK == DisbursementChargeCodePK || chargeCodePK == DeferredChargeCodePK)
			{
				rateInfo.RateSource = Res.GetString("c56cda49-58c6-4f31-8f3e-a6aa1a2c91cb", "Customs Response message");
			}

			if (rateInfo.ChargeCode != null)
			{
				rateInfo.InvoiceLineDescription = rateInfo.ChargeCode.AC_Desc + (string.IsNullOrEmpty(entryReference) ? "" : " - " + entryReference);
				rateInfo.InvoiceNumber = entryReference;
			}

			if (GlbCompany.CurrentCompany != null)
			{
				rateInfo.LocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				rateInfo.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}

			if (parent is IRatingSupporterWithAdapter supporter)
			{
				// It is used by autorating engine to match new charges with old ones on the job and update them based
				// on autorating behavior on the existing charge.
				rateInfo.JobRef = supporter.RatingAdapter.JobID;
			}

			rateInfo.ProviderPK = creditorPK;
			return rateInfo;
		}

		void CheckAndReportErrorForGetNewAutoRatingInfo(ZGuid chargeCodePK, AccChargeCode chargeCode)
		{
			if (chargeCode == null || parent == null || GlbCompany.CurrentCompany == null)
			{
				var errorMessageBuilder = new ZStringBuilder();

				#region SuppressResourceStringsCheckRegion

				var parentBO = parent as BusinessObject;
				errorMessageBuilder.Append("Inform IL/Rating Team. Unexpected null object  has been spotted.");
				errorMessageBuilder.Append("Parent: " + (parent == null ? "null" : parent.GetType().ToString())
										  + " Parent AS Bussines Object= " + (parentBO == null ? "null" : parentBO.PK.ToString()));
				errorMessageBuilder.Append("Charge Code: PK= " + chargeCodePK + " Business Object=" + (chargeCode == null ? "null" : chargeCode.AC_Code.ToString()));
				errorMessageBuilder.Append("CurrentCompany: " + (GlbCompany.CurrentCompany == null ? "null" : GlbCompany.CurrentCompany.GC_Code.ToString()));

				#endregion

				ErrorReporter.ReportOnce("AutoRatingRunner.GetNewAutoRatingInfo", errorMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		ZGuid DisbursementChargeCodePK
		{
			get { return RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value; }
		}

		ZGuid DeferredChargeCodePK
		{
			get { return RatingDataRegistry.Instance.CustomDeferredChargeCode.Value; }
		}

		class CustomsChargeKey
		{
			public CustomsChargeKey(CustomsCharge customsCharge)
			{
				chargeCodePK = customsCharge.ChargeCodePK;
				entryRef = customsCharge.EntryReference;
				debtorPk = customsCharge.DebtorPK;
			}

			readonly ZGuid chargeCodePK;
			readonly ZString entryRef;
			readonly ZGuid debtorPk;

			public override bool Equals(object obj)
			{
				var other = obj as CustomsChargeKey;
				return other != null && chargeCodePK == other.chargeCodePK && entryRef == other.entryRef && debtorPk == other.debtorPk;
			}

			public override int GetHashCode()
			{
				return chargeCodePK.GetHashCode() ^ entryRef.GetHashCode() ^ debtorPk.GetHashCode();
			}
		}

		#endregion
	}
}

