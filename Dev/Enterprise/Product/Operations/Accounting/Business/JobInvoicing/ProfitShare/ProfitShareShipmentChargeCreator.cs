using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareShipmentChargeCreator : ProfitShareChargeCreator
	{
		public ProfitShareShipmentChargeCreator(
			ProfitShareDetailCollection profitShareDetails,
			Job job,
			bool saveCharges = true,
			IProfitShareShipmentChargePoster chargePoster = null,
			IProfitShareShipmentValidator validator = null)
		{
			this.Job = job;
			this.CalculatedProfitShares = profitShareDetails;
			adjustPostedInvoiceHelper_constructorInitializedOnly = new AdjustPostedInvoiceHelper();
			this.saveCharges = saveCharges;
			ChargePoster = chargePoster ?? new ProfitShareShipmentChargePoster();
			Validator = validator ?? new ProfitShareShipmentValidator();
		}

		public readonly Job Job;
		public readonly ProfitShareDetailCollection CalculatedProfitShares;
		readonly bool saveCharges;

		IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper => adjustPostedInvoiceHelper_constructorInitializedOnly;
		IAdjustPostedInvoiceHelper adjustPostedInvoiceHelper_constructorInitializedOnly;

		public IProfitShareShipmentChargePoster ChargePoster { get; }
		public IProfitShareShipmentValidator Validator { get; }

		bool JobHasOtherChangesBeforeCreateCharges;

		protected override bool CreateChargesCore()
		{
			JobHasOtherChangesBeforeCreateCharges = Job.HasChanges;
			CreatedCharges.Clear();
			CreateOrUpdateCharges();
			PostAndUpdateJob();
			CreatedChargesCount += CreatedCharges.Count;

			return CreatedCharges.Count > 0;
		}

		protected override BusinessObject ParentObject
		{
			get { return Job != null ? Job.Parent as BusinessObject : null; }
		}

		void CreateOrUpdateCharges()
		{
			foreach (ProfitShareDetail profitShare in CalculatedProfitShares)
			{
				foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
				{
					var profitShareChargesWithProfitShare = shipmentDetail.ProfitShareCharges.Where(x => x.ProfitShare.HasValue);
					foreach (var profitShareCharge in profitShareChargesWithProfitShare)
					{
						var (chargeExists, chargeKey) = GetOrCreateCharge(profitShareCharge);
						var charge = CreatedCharges[chargeKey];
						SetAdditionalChargeDetails(charge, profitShareCharge, updateExistingCharge: chargeExists);

						var createProfitShareAsAR = AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value;
						if ((!createProfitShareAsAR && charge.JR_OSCostAmt.IsEmpty) ||
							(createProfitShareAsAR && charge.JR_OSSellAmt.IsEmpty))
						{
							charge.Delete();
							CreatedCharges.Remove(chargeKey);
						}
					}
				}
			}
		}

		void PostAndUpdateJob()
		{
			if (AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.Value)
			{
				var allChargesArePosted = true;
				foreach (Charge createdCharge in CreatedCharges.Values)
				{
					if (Validator.IsChargeValidToBePosted(createdCharge))
					{
						ChargePoster.PostCharge(createdCharge, Job, AdjustPostedInvoiceHelper);
					}
					else
					{
						allChargesArePosted = false;
					}
				}

				if (!allChargesArePosted)
				{
					ValidationErrorAccumulator.AppendIfNotEmpty(Res.GetString("39ab5103-02b3-47ba-9cbc-0d4cf4287d84", @"Profit Share charge(s) were created but not posted because there are validation error(s).
The Profit Share charge(s) should be posted manually after fixing the validation error(s)."));
				}
				else
				{
					ChargePoster.UpdateJobStatus(Job);
				}
			}
		}

		protected override ZBool SaveCharges()
		{
			var success = false;
			if (CreatedCharges.Count > 0 && Job.HasChanges && !Job.HasErrors && ValidationErrors.IsEmpty)
			{
				if (saveCharges)
				{
					using (Job.SuspendCreatingProfitShareOnSave())
					{
						try
						{
							Job.Factory.Save();
							success = true;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (Env.Instance.ServiceTaskCode == "JCS")
							{
								throw;
							}
							else
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
					}
				}
				else
				{
					success = true;
				}
			}

			if (!JobHasOtherChangesBeforeCreateCharges && saveCharges)
			{
				Job.HasChanges = false;
			}

			return success;
		}

		#region Implementation

		void SetAdditionalChargeDetails(Charge charge, ProfitShareCharge profitShareCharge, bool updateExistingCharge)
		{
			var shipmentDetail = profitShareCharge.ShipmentDetail;
			var isReceivable = AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value;

			void UpdateChargeDetails(string calculationDescriptionPropertyName, string amountPropertyName, ZDecimal newProfitShareAmount,
				Func<Charge, decimal> funcCalculateExistingShares, string sharedPartyPropertyName, string clearedOrgPropertyName)
			{
				// Important: setting OS Amount clears calculation description, so store it first.
				var existingCalculationDescription = ((ZBlob)charge[calculationDescriptionPropertyName]).ToUTF8();

				var chargeAmount = (ZDecimal)charge[amountPropertyName] + newProfitShareAmount;
				var existingProfitShareCharges = ProfitShareChargesExcludeNewCreatedOnes(profitShareCharge);
				var alreadySharedAmount = (ZDecimal)existingProfitShareCharges.Sum(funcCalculateExistingShares);

				if (!updateExistingCharge && alreadySharedAmount != ZDecimal.Zero)
				{
					charge[amountPropertyName] = chargeAmount - alreadySharedAmount;
				}
				else
				{
					charge[amountPropertyName] = chargeAmount;
				}

				var lookups = new OrgProfitSharePartyLookups(null);
				var newCalculationDescription = lookups.PartyTypes.GetDescriptionFromCode(shipmentDetail.PartyType) + " - " + profitShareCharge.Description;
				UpdateChargeDescriptions(charge, calculationDescriptionPropertyName, existingCalculationDescription, alreadySharedAmount, newCalculationDescription, updateExistingCharge);

				charge[sharedPartyPropertyName] = shipmentDetail.ProfitShareParty.PK;
				charge[clearedOrgPropertyName] = ZGuid.Empty;
			}

			using (charge.GetSuspenderForCreateProfitShareCharges())
			{
				if (isReceivable)
				{
					UpdateChargeDetails(
						calculationDescriptionPropertyName: nameof(charge.RevenueCalculationDescription),
						amountPropertyName: nameof(charge.JR_OSSellAmt),
						newProfitShareAmount: AccountingUtils.Round(-profitShareCharge.ProfitShare ?? 0m, profitShareCharge.Currency.RX_Code),
						funcCalculateExistingShares: psCharge => psCharge.JR_OSSellAmt,
						sharedPartyPropertyName: nameof(charge.JR_OH_SellAccount),
						clearedOrgPropertyName: nameof(charge.JR_OH_CostAccount));
				}
				else
				{
					UpdateChargeDetails(
						calculationDescriptionPropertyName: nameof(charge.CostCalculationDescription),
						amountPropertyName: nameof(charge.JR_OSCostAmt),
						newProfitShareAmount: AccountingUtils.Round(profitShareCharge.ProfitShare ?? 0m, profitShareCharge.Currency.RX_Code),
						funcCalculateExistingShares: psCharge => psCharge.JR_OSCostAmt - psCharge.JR_OSSellAmt,
						sharedPartyPropertyName: nameof(charge.JR_OH_CostAccount),
						clearedOrgPropertyName: nameof(charge.JR_OH_SellAccount));
				}
			}
			charge.JR_IsIncludedInProfitShare = false;
			PrepareInvoicingPropertiesForPostingProfitShareCharge(charge, isReceivable, profitShareCharge);

			charge.Validation.ValidateAll();
		}

		/// <summary>
		/// Update charge description and calculation description
		/// </summary>
		static void UpdateChargeDescriptions(Charge charge, string calculationDescriptionPropertyName, ZString existingCalculationDescription, ZDecimal alreadySharedAmount, string newCalculationDescription, bool updateExistingCharge)
		{
			var fullCalculationDescription = string.Empty;
			if (!existingCalculationDescription.IsEmpty)
			{
				fullCalculationDescription = existingCalculationDescription + ", " + newCalculationDescription;
			}
			else
			{
				if (alreadySharedAmount == ZDecimal.Zero)
				{
					fullCalculationDescription = newCalculationDescription;
				}
				else
				{
					fullCalculationDescription = Res.GetString("08ef3a0d-4efc-4bdb-b8bc-78100a66492f", "Adjustment: less {0}", alreadySharedAmount.ToString(2)) + " - " + newCalculationDescription;
				}
			}
			charge[calculationDescriptionPropertyName] = ZBlob.FromUTF8(fullCalculationDescription);

			var chargeDescription = charge.JR_Desc;
			if (updateExistingCharge)
			{
				chargeDescription += ", " + newCalculationDescription;
			}
			else
			{
				chargeDescription += " - " + fullCalculationDescription;
			}
			charge.JR_Desc = chargeDescription.SubstringSafe(0, JobChargeSchema.JR_Desc.MaxLength);
		}

		void PrepareInvoicingPropertiesForPostingProfitShareCharge(Charge charge, bool isReceivable, ProfitShareCharge profitShareCharge)
		{
			if (isReceivable || !AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.Value)
			{
				return;
			}

			if (charge.JR_APInvoiceDate.IsEmpty)
			{
				charge.JR_APInvoiceDate = ZDateTime.Now;
			}

			if (charge.JR_APInvoiceNum.IsEmpty)
			{
				var invoiceNum = InvoiceLiteralNumberGenerator.GetNextProfitShareAPInvoiceNumberForConsol(Job.Factory, Job.JH_JobNum);
				if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value)
				{
					invoiceNum += "-" + profitShareCharge.CurrencyCode;
				}

				charge.JR_APInvoiceNum = invoiceNum.SubstringSafe(0, JobChargeSchema.JR_APInvoiceNum.MaxLength);
			}
		}

		(bool chargeExists, CreatedChargesKey key) GetOrCreateCharge(ProfitShareCharge profitShareCharge)
		{
			var shipmentDetail = profitShareCharge.ShipmentDetail;
			var chargeExists = true;

			var key = new CreatedChargesKey(shipmentDetail.ProfitShareParty, profitShareCharge.CurrencyCode);
			if (!CreatedCharges.TryGetValue(key, out var charge))
			{
				chargeExists = false;
				charge = CreateNewCharge(profitShareCharge);
				CreatedCharges.Add(key, charge);
			}

			return (chargeExists, key);
		}

		Charge CreateNewCharge(ProfitShareCharge profitShareCharge)
		{
			var profitShareChargeCode = GetProfitShareChargeCode(profitShareCharge);
			Charge charge = Job.Charges.AddNew();
			charge.JR_AC = profitShareChargeCode;
			var currencyCode = profitShareCharge.Currency.RX_Code;

			if (AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value)
			{
				charge.JR_RX_NKSellCurrency = currencyCode;
			}
			else
			{
				charge.JR_RX_NKCostCurrency = currencyCode;
			}
			return charge;
		}

		ZGuid GetProfitShareChargeCode(ProfitShareCharge profitShareCharge)
		{
			ZGuid profitShareChargeCode;
			IRegistryItemInternals registryItem;
			var existingCharges = ProfitShareChargesExcludeNewCreatedOnes(profitShareCharge);

			if (existingCharges.Length > 0)
			{
				var registry = AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode;
				profitShareChargeCode = registry.Value;
				registryItem = registry;

				// Hm, this code is defective. Why do we use default ProfitShareChargeCode instead of ProfitShareChargeCodesPerParty?
				// What if the existing PS charge code is from overridden ProfitShareChargeCodesPerParty?
				// The goal here is adjusting the PS charge but we may end up creating a new charge with default PS charge code
				// instead of new charge with ProfitShareChargeCodesPerParty.
				// [WI00020731 - CHECKIN - ALP - PS Adjustment] added the code.
				// Since it was from a way old date: 19/11/2009, I won't change its behavior. Should review and create a defect WI if needed.
				if (profitShareChargeCode.IsEmpty)
				{
					registry = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode;
					profitShareChargeCode = registry.Value;
					registryItem = registry;
				}
			}
			else
			{
				(registryItem, profitShareChargeCode) = GetProfitShareChargeCodePerPartyRegistryAndChargeCodePK(profitShareCharge.ShipmentDetail);
			}

			if (!AccountingUtils.ChargeCodeExistsAndBelongsToCompany(Job.Factory, profitShareChargeCode, Job.JH_GC))
			{
				throw new ZCannotSaveException(AccountingConstants.ProfitShareErrorMessages.InvalidRegistry(registryItem), Res.GetString("7b516466-2522-49ca-a4a8-3356050eb114", "Cannot Create Profit Share Charge"));
			}

			return profitShareChargeCode;
		}

		(IRegistryItemInternals, ZGuid) GetProfitShareChargeCodePerPartyRegistryAndChargeCodePK(ProfitShareShipmentDetail shipmentDetail)
		{
			var jobType = shipmentDetail.ProfitShareAgreement?.O4_JobType ?? ZString.Empty;
			var partyType = jobType == JobTypesList.Codes.GCN
				? OrgProfitSharePartyLookups.PartyTypeCodes.GatewayAgent
				: shipmentDetail.PartyType;
			var registry = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty;
			var profitShareChargeCodePK = registry.Value.GetCode(partyType);

			return (registry, profitShareChargeCodePK);
		}

		readonly Dictionary<CreatedChargesKey, Charge> CreatedCharges = new Dictionary<CreatedChargesKey, Charge>();

		struct CreatedChargesKey
		{
			public CreatedChargesKey(OrgHeader org, string currency)
			{
				this.Org = org;
				this.Currency = currency;
			}

			public readonly OrgHeader Org;
			public readonly string Currency;
		}

		Charge[] ProfitShareChargesExcludeNewCreatedOnes(ProfitShareCharge profitShareCharge)
		{
			var shipmentDetail = profitShareCharge.ShipmentDetail;
			var currency = profitShareCharge.Currency;

			ZQuery query1 = new ZQuery(JobChargeSchema.JR_JH, Job.PK);
			foreach (var chargeToExclude in CreatedCharges)
			{
				query1.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, chargeToExclude.Value.PK);
			}

			var (_, chargeCodePK) = GetProfitShareChargeCodePerPartyRegistryAndChargeCodePK(shipmentDetail);
			ZQuery query2 = new ZQuery(JobChargeSchema.JR_AC, chargeCodePK);
			query2.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);
			query2.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);

			var query = new ZQuery(query1, query2);

			if (AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value)
			{
				query.AddToFilter(JobChargeSchema.JR_OH_SellAccount, shipmentDetail.ProfitShareParty.PK);
				query.AddToFilter(JobChargeSchema.JR_RX_NKSellCurrency, currency.RX_Code);
			}
			else
			{
				query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, shipmentDetail.ProfitShareParty.PK);
				query.AddToFilter(JobChargeSchema.JR_RX_NKCostCurrency, currency.RX_Code);
			}

			return Job.Factory.Load<Charge>(query);
		}

		#endregion

		protected override void RunPreCreationValidationCore()
		{
			ValidationErrorAccumulator.AppendIfNotEmpty(ValidateOrganisationDetails());
		}

		ZString ValidateOrganisationDetails()
		{
			bool profitShareIsAR = AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value;
			ZString message = ZString.Empty;
			foreach (ProfitShareDetail profitShareDetail in CalculatedProfitShares)
			{
				if (profitShareIsAR && !profitShareDetail.ProfitShareParty.OH_IsDebtor)
				{
					message = Enterprise.Accounting.Business.AccountingConstants.ProfitShareErrorMessages.InvalidDebtor(profitShareDetail.ProfitShareParty, profitShareDetail.PartyType);
					break;
				}
				else if (!profitShareIsAR && !profitShareDetail.ProfitShareParty.OH_IsCreditor)
				{
					message = Enterprise.Accounting.Business.AccountingConstants.ProfitShareErrorMessages.InvalidCreditor(profitShareDetail.ProfitShareParty.OH_Code);
					break;
				}
			}

			return message;
		}

#if DEBUG

		public void SubstituteAdjustPostedInvoiceHelper_ForTestOnly(IAdjustPostedInvoiceHelper replacement) => adjustPostedInvoiceHelper_constructorInitializedOnly = replacement;
		public IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper_ExposedForTestOnly => AdjustPostedInvoiceHelper;

#endif

	}
}