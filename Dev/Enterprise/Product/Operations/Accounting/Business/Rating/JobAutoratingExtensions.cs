using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class JobAutoratingExtensions
	{
		public static Charge SetAmountsOnCharge(this Job job, Charge chargeToUpdate, AutoRateInfo rateInfo, CostSell costOrSell, bool canUpdateCreditor = true)
		{
			Charge result = null;
			if (chargeToUpdate != null && rateInfo != null)
			{
				using (job.SuppressAutoRatingOverride())
				{
					if (costOrSell == CostSell.Revenue && !chargeToUpdate.JR_SellRatingOverride)
					{
						result = job.AddAutorateRevenue(rateInfo, chargeToUpdate);
						if (rateInfo.HasExplicitZeroAmount)
						{
							ClearSellFields(chargeToUpdate);
						}
					}

					if (costOrSell == CostSell.Cost && !chargeToUpdate.JR_CostRatingOverride)
					{
						result = job.AddAutorateCost(rateInfo, chargeToUpdate, canUpdateCreditor: canUpdateCreditor);
						if (rateInfo.HasExplicitZeroAmount)
						{
							ClearCostFields(chargeToUpdate);
						}
					}

					if (result != null)
					{
						AddAdditionalDescription(rateInfo, chargeToUpdate);
						UpdateInvoiceNumberAndDateForCustomsCharge(job, chargeToUpdate, rateInfo.InvoiceNumber);
						chargeToUpdate.AddAttributes(rateInfo.Attributes);

						if (chargeToUpdate.IsDisbursementCharge)
						{
							chargeToUpdate.JR_SellRated = true;
							chargeToUpdate.JR_CostRated = true;
						}
					}

					bool shouldSaveLogs = !rateInfo.CalculationLogs.IsEmpty
											&& ((rateInfo.CalculationLogs.IsCosting && !chargeToUpdate.JR_CostRatingOverride)
												|| (!rateInfo.CalculationLogs.IsCosting && !chargeToUpdate.JR_SellRatingOverride));

					if (shouldSaveLogs)
					{
						CalculationLogsLoader.Save(chargeToUpdate, rateInfo.CalculationLogs);
					}
				}
			}

			return result;
		}

		static void UpdateInvoiceNumberAndDateForCustomsCharge(Job job, Charge chargeToUpdate, ZString invoiceNumber)
		{
			var companyPK = job.Branch != null ? job.Branch.GB_GC.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid();
			var populateEntryRefAsInvoiceNo = RatingDataRegistry.Instance.PopulateEntryRefAsInvoiceNo.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			if (populateEntryRefAsInvoiceNo == RateEntryReferenceTypeList.Codes.FullReference && chargeToUpdate.IsCustomsCharge)
			{
				chargeToUpdate.JR_APInvoiceNum = invoiceNumber;
				chargeToUpdate.JR_APInvoiceDate = ZDateTime.Now;
			}
			else if (populateEntryRefAsInvoiceNo == RateEntryReferenceTypeList.Codes.ShortenedReference && chargeToUpdate.IsCustomsCharge)
			{
				chargeToUpdate.JR_APInvoiceNum = invoiceNumber.SubstringSafe(3);
				chargeToUpdate.JR_APInvoiceDate = ZDateTime.Now;
			}
		}

		static void ClearSellFields(Charge charge)
		{
			if (!charge.JR_IsRevenuePosted && !charge.JR_SellRatingOverride)
			{
				charge.JR_LocalSellAmt = 0;
				charge.JR_OSSellAmt = 0;
				charge.JR_EstimatedRevenue = 0;

				if (!charge.IsDisbursementCharge)
				{
					charge.JR_SellRated = true;
				}
			}
		}

		static void ClearCostFields(Charge charge)
		{
			if (!charge.JR_IsCostPosted && !charge.JR_IsApportioned && !charge.JR_CostRatingOverride)
			{
				charge.JR_LocalCostAmt = 0;
				charge.JR_OSCostAmt = 0;
				charge.JR_CostRated = true;
			}
		}

		static void AddAdditionalDescription(AutoRateInfo rateInfo, Charge chargeToUpdate)
		{
			chargeToUpdate.JR_Desc = chargeToUpdate.GetCombinedDescription(rateInfo);
		}

		#region Cost

		/// <param name="fromAutorating">
		/// True when called during autorating,
		/// False when called from QuickCalculate
		/// </param>
		public static Charge AddAutorateCost(
			this Job job,
			AutoRateInfo rateInfo,
			Charge charge,
			bool recalculateOverriden = false,
			bool canUpdateCreditor = true,
			bool fromAutorating = true)
		{
			using (job.SuppressAutoRatingOverride())
			{
				if (!charge.IsRevenueCharge && !charge.JR_IsApportioned)
				{
					if (charge.JR_IsCostPosted || (charge.IsDisbursementCharge && charge.JR_IsRevenuePosted))
					{
						if (rateInfo.ProviderPK.IsValid && charge.JR_OH_CostAccount != rateInfo.ProviderPK)
						{
							charge = job.Charges.AddNew();
							charge.JR_AC = rateInfo.ChargeCode.PK;
						}
						else
						{
							return null;
						}
					}

					if (!charge.JR_CostRatingOverride || recalculateOverriden)
					{
						if (canUpdateCreditor)
						{
							SetCreditor(job, rateInfo, charge);
						}

						if (rateInfo.IsDisbursement && rateInfo.DebtorOverridePK.IsValid)
						{
							charge.JR_OH_SellAccount = rateInfo.DebtorOverridePK;
						}

						if (!string.IsNullOrWhiteSpace(rateInfo.Currency))
						{
							charge.JR_RX_NKCostCurrency = rateInfo.Currency;
						}

						if (rateInfo.HasExplicitZeroAmount || !rateInfo.Amount.IsEmpty)
						{
							var costAmountFromRateInfo = rateInfo.Amount;
							var agentCostAmountFromRateInfo = !rateInfo.AgentAmount.IsEmpty ? rateInfo.AgentAmount : ChoseThisCostRateOrZero(rateInfo.Amount);

							if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered || charge.CostGSTRate == null || charge.CostGSTRate.GetRate(charge.JR_CostTaxDate).IsEmpty)
							{
								costAmountFromRateInfo += rateInfo.OverriddenGSTAmount;
								agentCostAmountFromRateInfo += rateInfo.OverriddenGSTAmount;
							}

							if (charge.IsCostForeign)
							{
								charge.JR_OSCostAmt = costAmountFromRateInfo;
							}
							else
							{
								charge.JR_LocalCostAmt = costAmountFromRateInfo;
							}

							// Despite the charge itself already containing logic to update the estimate,
							// the charge does not know who is updating it. It could be a manual update,
							// an update from quick-calculate, or from autorating.
							// By setting the estimate here, we ensure that the below can only execute
							// when the charge is NOT manually set, and also when the QuickCalculate
							// functionality did not call this code
							if (!charge.JR_CostRated && fromAutorating)
							{
								charge.SetEstimatedCost(charge.JR_OSCostAmt);
							}

							charge.JR_AgentDeclaredCostAmt = Utilities.Round(agentCostAmountFromRateInfo, charge.OSCostCurrencyDecimals);
							charge.JR_CostRated = true;
						}

						if (rateInfo.ResetDescription)
						{
							charge.SetInitialChargeDescription();
						}

						if (charge.HasChanges || !charge.IsInDatabase)
						{
							SetChargeCostDescription(job, rateInfo, charge);
							charge.SetCostCalculationDescription(rateInfo);
						}
					}
				}

				charge.AddPaymentBases(rateInfo.Bases, true);
				return charge.HasChanges || !charge.IsInDatabase ? charge : null;
			}
		}

		static void SetCreditor(Job job, AutoRateInfo rateInfo, Charge charge)
		{
			var creditorPK = rateInfo.CreditorPK;
			if (creditorPK.IsValid && charge.JR_OH_CostAccount != creditorPK)
			{
				var creditorOrg = job.Factory.Load<OrgHeader>(creditorPK);
				if (creditorOrg != null && creditorOrg.OH_IsCreditor)
				{
					charge.ClearDebtorIfBothCostAndSellAccountsAreOrgProxies(creditorPK);
					charge.JR_OH_CostAccount = creditorPK;
				}
			}
		}

		#endregion

		#region Revenue

		/// <param name="fromAutorating">
		/// True when called during autorating,
		/// False when called from QuickCalculate
		/// </param>
		public static Charge AddAutorateRevenue(
			this Job job,
			AutoRateInfo rateInfo,
			Charge charge,
			bool recalculateOverriden = false,
			bool fromAutorating = true)
		{
			using (job.SuppressAutoRatingOverride())
			{
				if (charge.IsDisbursementCharge)
				{
					return GetUpdatedCostAsDisbursementCharge(job, rateInfo, charge, recalculateOverriden);
				}

				var isIntercompanyTariff = rateInfo.Line?.IsIntercompanyTariff() ?? false;

				if (rateInfo.HasExplicitZeroAmount || !rateInfo.Amount.IsEmpty)
				{
					if (charge.JR_IsRevenuePosted)
					{
						var newCharge = job.Charges.AddNew();
						newCharge.JR_AC = rateInfo.ChargeCode.PK;

						if (newCharge.JR_OH_SellAccount == charge.JR_OH_SellAccount)
						{
							job.Charges.RemoveAndDelete(newCharge);
							return null;
						}
						else if (!charge.JR_OH_SellAccount.IsValid)
						{
							charge.JR_OH_SellAccount = newCharge.JR_OH_SellAccount;
							job.Charges.RemoveAndDelete(newCharge);
						}
						else
						{
							charge = newCharge;
						}
					}

					if (!charge.JR_SellRatingOverride || recalculateOverriden)
					{
						var sellAmountFromRateInfo = rateInfo.Amount;
						var agentSellAmountFromRateInfo = !rateInfo.AgentAmount.IsEmpty ? rateInfo.AgentAmount : ChoseThisSellRateOrZero(rateInfo.Amount);

						if (!string.IsNullOrWhiteSpace(rateInfo.Currency))
						{
							charge.JR_RX_NKSellCurrency = rateInfo.Currency;
						}

						if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered || charge.SellGSTRate == null || charge.SellGSTRate.GetRate(charge.JR_SellTaxDate).IsEmpty)
						{
							sellAmountFromRateInfo += rateInfo.OverriddenGSTAmount;
							agentSellAmountFromRateInfo += rateInfo.OverriddenGSTAmount;
						}

						if (charge.IsSellForeign)
						{
							charge.JR_OSSellAmt = sellAmountFromRateInfo;
						}
						else
						{
							charge.JR_LocalSellAmt = sellAmountFromRateInfo;
						}

						// Despite the charge itself already containing logic to update the estimate,
						// the charge does not know who is updating it. It could be a manual update,
						// an update from quick-calculate, or from autorating.
						// By setting the estimate here, we ensure that the below can only execute
						// when the charge is NOT manually set, and also when the QuickCalculate
						// functionality did not call this code
						if (!charge.JR_SellRated && fromAutorating)
						{
							charge.JR_EstimatedRevenue = sellAmountFromRateInfo;
						}

						charge.JR_AgentDeclaredSellAmt = Utilities.Round(agentSellAmountFromRateInfo, charge.OSSellCurrencyDecimals);

						if (charge.JR_OH_CostAccount.IsEmpty && rateInfo.CreditorPK.IsValid && !charge.IsCostPosted)
						{
							var creditorOrg = job.Factory.Load<OrgHeader>(rateInfo.CreditorPK);
							if (creditorOrg.OH_IsCreditor)
							{
								if (!isIntercompanyTariff || charge.JR_LocalCostAmt > 0 || charge.JR_OSCostAmt > 0 || rateInfo.HasExplicitZeroAmount)
								{
									charge.JR_OH_CostAccount = creditorOrg.PK;
								}
							}
						}

						if (rateInfo.DebtorOverridePK.IsValid)
						{
							charge.JR_OH_SellAccount = rateInfo.DebtorOverridePK;
						}

						job.SetChargeRevenueDescription(rateInfo, charge);

						charge.SetRevenueCalculationDescription(rateInfo);
						charge.JR_OrderReference = rateInfo.JobRef;
						charge.JR_SellRated = true;
					}
				}
				else if (rateInfo.DebtorOverridePK.IsValid)
				{
					charge.JR_OH_SellAccount = rateInfo.DebtorOverridePK;
				}
			}

			charge.AddPaymentBases(rateInfo.Bases, false);
			return charge.HasChanges || !charge.IsInDatabase ? charge : null;
		}

		static Charge GetUpdatedCostAsDisbursementCharge(Job job, AutoRateInfo rateInfo, Charge charge, bool recalculateOverriden = false)
		{
			var updatedCharge = job.AddAutorateCost(rateInfo, charge, recalculateOverriden);
			if (updatedCharge != null && !updatedCharge.IsRevenuePosted)
			{
				if (!string.IsNullOrWhiteSpace(rateInfo.Currency) && updatedCharge.JR_RX_NKSellCurrency != rateInfo.Currency)
				{
					updatedCharge.JR_RX_NKSellCurrency = rateInfo.Currency;
				}

				job.SetChargeRevenueDescription(rateInfo, updatedCharge);
				updatedCharge.SetRevenueCalculationDescription(rateInfo);
			}

			return updatedCharge;
		}

		static ZDecimal ChoseThisSellRateOrZero(ZDecimal sellRate)
		{
			return RatingDataRegistry.Instance.UnspecifiedSellShouldForceZeroToBePulledThrough.Value ? ZDecimal.Zero : sellRate;
		}

		static ZDecimal ChoseThisCostRateOrZero(ZDecimal costRate)
		{
			return RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.Value ? ZDecimal.Zero : costRate;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		internal static void SetChargeRevenueDescription(this Job job, AutoRateInfo rateInfo, Charge chargeToUpdate)
		{
			var (description, replaceChargeCodeDescWithItsLocalDescInDescription) = rateInfo.GetInvoiceLineDescriptionOrLocalDescription(job.JobType?.Code, chargeToUpdate.SellAccount, job.LocalChargesAddr?.Header);

			if (chargeToUpdate.Job != null)
			{
				OrgHeader chargeParty = chargeToUpdate.SellAccount;
				if (chargeParty != null && chargeToUpdate.ChargeCode != null)
				{
					if (replaceChargeCodeDescWithItsLocalDescInDescription && chargeToUpdate.ShouldDefaultLocalChargeDescription)
					{
						description = description.Replace(chargeToUpdate.ChargeCode.AC_Desc, chargeToUpdate.ChargeCode.AC_LocalLanguageDescription);
					}

					if (chargeToUpdate.IsCalculationDescriptionRelevant && !rateInfo.CalculationDescription.IsEmpty)
					{
						var containerNumber = chargeToUpdate.JR_SellReference;
						if (description.Contains(containerNumber) && rateInfo.CalculationDescription.Contains(containerNumber) && !containerNumber.IsEmpty)
						{
							description += " - " + rateInfo.CalculationDescription.Substring(rateInfo.CalculationDescription.IndexOf("-") + 2);
						}
						else
						{
							description += " - " + rateInfo.CalculationDescription;
						}
					}
				}

				description += GetAutoRatedForInformation(job, rateInfo, chargeToUpdate.IsCalculationDescriptionRelevant);
			}

			if (!description.IsEmpty)
			{
				chargeToUpdate.JR_Desc = description.Substring(0, JobChargeSchema.JR_Desc.MaxLength);
			}
		}

		static void SetChargeCostDescription(Job job, AutoRateInfo rateInfo, Charge chargeToUpdate)
		{
			var description = chargeToUpdate.JR_Desc;
			description += GetAutoRatedForInformation(job, rateInfo);

			if (!description.IsEmpty)
			{
				chargeToUpdate.JR_Desc = description.Substring(0, JobChargeSchema.JR_Desc.MaxLength);
			}
		}

		static ZString GetAutoRatedForInformation(Job job, AutoRateInfo rateInfo, bool shouldDescribeForContainerYard = false)
		{
			var result = ZString.Empty;

			if (!rateInfo.AutoRatedForString.IsEmpty)
			{
				if (job.IsGatewayBillingJob())
				{
					result = " - " + rateInfo.AutoRatedForString;
				}
				else if (rateInfo.RateTypeToUse == RateType.TransportBookings)
				{
					result = " {" + rateInfo.OperationalJobRef + "}";
				}
				else if (rateInfo.RateTypeToUse == RateType.ContainerYard && shouldDescribeForContainerYard)
				{
					// ContainerYard has introduced an "adapter human readable name" and this is presented in AutoRatedForString.
					// This is because ContainerYard has one adapter per container and this needs to be described as it is for whom
					// it is being autorated. In the future if anyone else needs similar functionality, consider introducing an
					// additional field into AutoRateInfo to supplement AutoRatedFor and to make this code simpler.
					result = ": " + rateInfo.AutoRatedForString;
				}

				if (!rateInfo.AdditionalJobRef.IsEmpty)
				{
					result = " {" + rateInfo.AdditionalJobRef + "}";
				}
			}

			return result;
		}

		#endregion
	}
}
