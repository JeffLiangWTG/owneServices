using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Journal
{
	public static class AutoJobRevenueJournalCreator
	{
		public static void CreateJournalFromCostCharge(ChargeWithCost costCharge)
		{
			CreateJournalCore(costCharge, true);
		}

		public static void CreateJournalFromSellCharge(ChargeWithCost sellCharge)
		{
			CreateJournalCore(sellCharge, false);
		}

		static void CreateJournalCore(ChargeWithCost firstCharge, bool useCostValues)
		{
			Argument.NotNull(firstCharge, "firstCharge");

			CheckIsAllowPost(firstCharge);

			using (new DisposableAction(
				() => { firstCharge.Factory.SetContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule); },
				() => { firstCharge.Factory.RemoveContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule); }
			))
			{
				if (useCostValues && firstCharge.TryFindGatewaySellAndCostsFromApportionedCost(out var gatewaySellAndCosts))
				{
					CheckIsAllowPost(gatewaySellAndCosts.gatewayBillingSellCharge);
					foreach (var charge in gatewaySellAndCosts.costCharges)
					{
						CheckIsAllowPost(charge);
					}

					CreateOneToManyJournal(gatewaySellAndCosts);
					return;
				}

				Charge secondCharge;
				bool foundMatchedAccrual = false;
				if (useCostValues)
				{
					secondCharge = CreateNewChargeForOtherSideOfJRJ(firstCharge);
				}
				else
				{
					var isGatewayBillingJob = firstCharge.InternalJob?.IsGatewayBillingJob() ?? false;
					var matchingCharges = GetMatchingCharges(firstCharge).Where(x => !isGatewayBillingJob || x.RelatedJobID == firstCharge.RelatedJobID)
																		 .OrderByDescending(x => x.JR_OH_CostAccount);
					secondCharge = matchingCharges.FirstOrDefault(x => x.IsInDatabase && !x.IsCostPosted && !x.IsRevenuePostedWithManualJobRevenueJournal);
					if (secondCharge == null)
					{
						secondCharge = matchingCharges.FirstOrDefault(x => !x.IsCostPosted && !x.IsRevenuePostedWithManualJobRevenueJournal);
					}
					if (secondCharge != null)
					{
						foundMatchedAccrual = true;
					}
					else
					{
						secondCharge = CreateNewChargeForOtherSideOfJRJ(firstCharge);
					}
				}

				try
				{
					secondCharge.SetContext(BusinessContext.AutoJobRevenueJournal);
					if (foundMatchedAccrual)
					{
						UpdateValuesToMatchedAccrualCharge(firstCharge, secondCharge);
					}
					else
					{
						CopyValuesToSecondCharge(firstCharge, useCostValues, secondCharge);
					}
					if (secondCharge.CostRecognition.IsEmpty)
					{
						secondCharge.Delete();
						return;
					}

					var journal = CreateJournal(firstCharge.Factory);

					using (journal.GetValidationSuspender())
					using (journal.JournalLines.SuspendListChanged())
					{
						AddJournalLineSetValuesAndLinkToCharge(firstCharge, journal, useCostValues);
						AddJournalLineSetValuesAndLinkToCharge(secondCharge, journal, !useCostValues);
					}

					firstCharge.AddGSHBillingEvent(AccBillingEvents.Codes.JRJPosted);
				}
				finally
				{
					secondCharge.RemoveContext(BusinessContext.AutoJobRevenueJournal);
				}
			}
		}

		static void CheckIsAllowPost(ChargeWithCost charge)
		{
			if (charge == null)
			{
				return;
			}

			var isNotAllowPost = charge.Job?.IsReadyForFinancialClosureWithoutPostSecurity ?? false;
			if (!isNotAllowPost && charge.JR_JH != charge.JR_JH_InternalJob)
			{
				isNotAllowPost = charge.InternalJob?.IsReadyForFinancialClosureWithoutPostSecurity ?? false;
			}

			if (isNotAllowPost)
			{
				var message = Res.GetString("C283F4BA-0D51-4c02-8A22-A72FD3C02E02", "Auto Job Revenue Journal cannot be created as its Invoicing or Internal Job has Ready For Financial Closure status.");
				throw new CannotSaveAfterCriticalErrorException(message);
			}
		}

		static Charge[] GetMatchingCharges(ChargeWithCost charge)
		{
			var matchingCharges = charge.Factory.Load<Charge>(GetMatchingChargesQuery(charge));
			return matchingCharges;
		}

		static ZQuery GetMatchingChargesQuery(ChargeWithCost charge)
		{
			var result = new ZQuery(JobChargeSchema.JR_AC, charge.JR_AC);
			result.AddToFilter(JobChargeSchema.JR_GB, charge.JR_GB_InternalBranch);
			result.AddToFilter(JobChargeSchema.JR_GE, charge.JR_GE_InternalDept);
			result.AddToFilter(JobChargeSchema.JR_JH, charge.JR_JH_InternalJob);
			result.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, charge.PK);

			var jobConsolCostQuery = new ZQuery(JobChargeSchema.JR_E6, SQLComparisonOperator.Equal, null);
			result.AddToFilter(jobConsolCostQuery);

			var creditorQuery = new ZQuery(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, null);
			if (charge.Branch != null)
			{
				creditorQuery.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_OH_CostAccount, charge.Branch.GB_OH_OrgProxy);
			}
			result.AddToFilter(creditorQuery);

			return result;
		}

		static void CreateOneToManyJournal((ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewaySellAndCosts)
		{
			var validationError = EnsureOneToManyJournalCanBeCreated(gatewaySellAndCosts, true);
			if (string.IsNullOrWhiteSpace(validationError))
			{
				var journal = FindOrCreateGatewayBillingChargeLineAndJournal(gatewaySellAndCosts.gatewayBillingSellCharge);

				using (journal.GetValidationSuspender())
				using (journal.JournalLines.SuspendListChanged())
				{
					foreach (var costCharge in gatewaySellAndCosts.costCharges)
					{
						AddJournalLineSetValuesAndLinkToCharge(costCharge, journal, true);
					}
				}
			}
			else
			{
				//even this may cause trouble translating, prefix is required to that ChargeValidation could identify these errors
				throw new CannotSaveAfterCriticalErrorException(JRJCreationErrorPrefix + " " + validationError);
			}
		}

		public static ZString JRJCreationErrorPrefix => Res.GetString("E6C81855-0BE7-421C-9DEC-AA4E6A1B5D90", "One to many job revenue journal cannot be created due to");

		public static string EnsureOneToManyJournalCanBeCreated((ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewayBatch, bool isCreatingJRJ = false)
		{
			string chargeDesc(JobCharge charge, string genDesc) => string.Format("{0} {1} {2}", charge.Job?.JH_JobNum, charge.ChargeCode?.AC_Code, genDesc).Trim();

			var sellCharge = gatewayBatch.gatewayBillingSellCharge;
			var sellChargeGenDesc = Res.GetString("B38E92E9-92E0-45BE-ACBD-A5DD4488263F", "gateway billing sell charge");
			var sellChargeDesc = chargeDesc(sellCharge, sellChargeGenDesc);

			if (sellCharge.ShouldCreateSellJRJ)
			{
				return Res.GetString("234B7D9C-46AF-40D6-9FBA-4E287FDDB157", "{0} setup for standard job revenue journal", sellChargeDesc);
			}

			var sellChargeJob = sellCharge.Job;
			var sellChargeBranch = sellCharge.Branch;
			var sellChargeDept = sellCharge.Department;

			var costChargeGenDesc = Res.GetString("0DDEB3D1-B7A9-4140-9231-78F0ABE0BA19", "cost charge");
			foreach (var costCharge in gatewayBatch.costCharges)
			{
				var costChargeDesc = chargeDesc(costCharge, costChargeGenDesc);
				if (costCharge.Job == null)
				{
					return Res.GetString("3C0F9635-E29F-40C8-B577-B41F295841FC", "{0} does not have a Job", costChargeDesc);
				}

				if (!costCharge.InternalFieldsPointTo(sellChargeJob, sellChargeBranch, sellChargeDept))
				{
					var costFieldsStr = costCharge.InternalFieldsStr();
					var sellFieldsStr = sellCharge.InternalFieldsStr();
					return Res.GetString("8D3E65EA-F320-4CB0-AD8C-6C62878C2C20", "{0} internal job, branch, department fields ({1} {2} {3}) don't match gateway billing sell charge ({4} {5} {6}). This can occur if the system tries to set Debtor and Creditor of the charge both organization proxies",
						costChargeDesc,
						costFieldsStr.intJobNumber,
						costFieldsStr.intBranch,
						costFieldsStr.intDept,
						sellFieldsStr.intJobNumber,
						sellFieldsStr.intBranch,
						sellFieldsStr.intDept);
				}

				var suspendInternalFieldsPointToDifferentEntityCheck = costCharge.Job.PK == sellChargeJob.PK
					? costCharge.SetTempContext(BusinessContext.AutoJobRevenueJournal)
					: null;

				using (suspendInternalFieldsPointToDifferentEntityCheck)
				{
					if (!costCharge.ShouldCreateCostJRJ)
					{
						return Res.GetString("AF483364-3497-488C-A47D-9EE494BB0C7E", "{0} has incorrect job revenue journal setup", costChargeDesc);
					}
				}
			}

			var balance = sellCharge.JR_LocalSellAmt - gatewayBatch.costCharges.Sum(x => x.JR_LocalCostAmt);
			if (balance != 0)
			{
				return Res.GetString("E4FC6C4E-6595-4C0D-B86B-C57C8BFB90B6", "journal total line amount is not zero");
			}

			if (sellCharge.CostRecognition.IsEmpty)
			{
				return Res.GetString("490564ED-4F8E-489B-816E-6FA0866E40AC", "gateway billing charge cost recognition is empty");
			}

			if (isCreatingJRJ)
			{
				foreach (var costCharge in gatewayBatch.costCharges)
				{
					ChargeWithCost.ApplyRevenueRecognitionDateForCostPart(costCharge);
					if (costCharge.CostRecognition.IsEmpty)
					{
						return Res.GetString("9779BBA0-6727-44EC-A299-FA9444E49F98", "{0} charge cost recognition is empty", costCharge.Job.JH_JobNum);
					}
				}
			}

			return string.Empty;
		}

		static Charge CreateNewChargeForOtherSideOfJRJ(ChargeWithCost firstCharge)
		{
			var secondChargeJob = firstCharge.Factory.Load<Job>(firstCharge.JR_JH_InternalJob);
			if (secondChargeJob != null)
			{
				secondChargeJob.InitializeParentFromGenericJobWithSettingDefaults();
				// TODO: Add exchange rate to job if it doesn't already exist? See line 25-37 in JobRevenueJournalJobChargeTransformer.
				using (secondChargeJob.ChargesLoadSuspender.GetSuspender())
				{
					return secondChargeJob.Charges.AddNew();
				}
			}

			throw new CannotSaveAfterCriticalErrorException("Job Revenue Journal could not be created");
		}

		static JobRevenueJournal CreateJournal(BusinessObjectFactory factory)
		{
			var journal = factory.New<JobRevenueJournal>();

			using (journal.GetValidationSuspender())
			{
#if DEBUG

				var autoJRJCreatorCacheService_ForTestOnly = FactoryLevelPropertyStorageForTests.GetInstance(journal.Factory);
				if (autoJRJCreatorCacheService_ForTestOnly != null)
				{
					autoJRJCreatorCacheService_ForTestOnly.AddNewElement(journal.PK, "CreateJournal_JournalIsValidationSuspended", journal.IsValidationSuspended);
				}

#endif
				journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
				journal.AH_InvoiceDate = ZDateTime.Now;
				journal.AH_PostDate = ZDateTime.Now;
				journal.MarkAsAlreadyTransformed();
			}
			return journal;
		}

		static void UpdateValuesToMatchedAccrualCharge(ChargeWithCost firstCharge, ChargeWithCost secondCharge)
		{
			using (secondCharge.GetValidationSuspender())
			{
				IPopulateAJRJCharges chargePopulator = new AJRJCostPopulatorFromCharge(firstCharge, secondCharge);
				chargePopulator.PopulateJobExchangeRates(secondCharge.InvoicingJob);
				chargePopulator.PopulateCostExchangeRate();
				if (secondCharge.JR_OH_CostAccount.IsEmpty)
				{
					chargePopulator.PopulateAccount();
				}
				chargePopulator.PopulateCostCurrency();
				chargePopulator.PopulateCostAmount();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		static void CopyValuesToSecondCharge(ChargeWithCost firstCharge, bool useCostValues, ChargeWithCost secondCharge)
		{
			DisposableAction temporaryChargePostingContext = null;
			if (!useCostValues)
			{
				temporaryChargePostingContext = new DisposableAction(
					() => secondCharge.Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting), //secondCharge Cost will be posted later in this class that is why it can be also treated as APTransacation posting to skip unnecessary calculations
					() => secondCharge.Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting)
				);
			}

			using (temporaryChargePostingContext)
			using (secondCharge.GetValidationSuspender())
			{
				secondCharge.JR_AC = firstCharge.JR_AC;
				secondCharge.JR_GB = firstCharge.JR_GB_InternalBranch;
				secondCharge.JR_GE = firstCharge.JR_GE_InternalDept;

				var job = secondCharge.InvoicingJob;

				var chargePopulator = useCostValues
					? new AJRJChargePopulatorFromCost(firstCharge, secondCharge)
					: new AJRJCostPopulatorFromCharge(firstCharge, secondCharge) as IPopulateAJRJCharges;

				using (secondCharge.Calculations.SuspendCalculations())
				{
					chargePopulator.PopulateJobExchangeRates(job); //create specific job billing exchange rate in advance to force zero cfx
					chargePopulator.PopulateDescription();
					chargePopulator.PopulateAccount();

					chargePopulator.PopulateSellCurrency();
					chargePopulator.PopulateCostCurrency();

					chargePopulator.PopulateSellExchangeRate();
					chargePopulator.PopulateCostExchangeRate();

					if (useCostValues) //Setting Sell amounts might trigger changing Cost amounts and vice versa, so should preserve the order of setting Sell and Cost amounts
					{
						chargePopulator.PopulateSellAmount();
						chargePopulator.PopulateCostAmount();
					}
					else
					{
						chargePopulator.PopulateCostAmount();
						chargePopulator.PopulateSellAmount();
					}

					chargePopulator.PopulateSellRatingOverride();
					chargePopulator.PopulateCostRatingOverride();
				}
				chargePopulator.PopulateSellCurrencyDependingOnDefaults();
				chargePopulator.PopulateCostCurrencyDependingOnDefaults();

				chargePopulator.PopulateRelatedJobNumber(job);

				secondCharge.JR_E6_GatewaySellHeader = firstCharge.JR_E6_GatewaySellHeader;

				secondCharge.JR_GB_InternalBranch = ZGuid.Empty;
				secondCharge.JR_GE_InternalDept = ZGuid.Empty;
				secondCharge.JR_JH_InternalJob = ZGuid.Empty;

				if (firstCharge.Job.IsGatewayBillingJob() && !useCostValues)
				{
					// Revenue on consol will be cost on shipment
					var costCalculationDescriptionHeader = Res.GetString("f027f9e9-8e18-4c71-ac38-e8cef50a2a8d", "Cascaded from Consol {0} > Gateway Billing charge {1} > Revenue Rate Audit:", firstCharge.JR_JobNumber, firstCharge.ChargeCode.AC_Code);
					secondCharge.CostCalculationDescription = ZBlob.FromUTF8(costCalculationDescriptionHeader + "\r\n\r\n" + firstCharge.RevenueCalculationDescription.ToUTF8());
				}
			}

			secondCharge.Validation.ValidateJR_GB_InternalBranch();
			secondCharge.Validation.ValidateJR_GE_InternalDept();
			secondCharge.Validation.ValidateJR_JH_InternalJob();
		}

		static JobRevenueJournal FindOrCreateGatewayBillingChargeLineAndJournal(ChargeWithCost gatewayBillingCharge)
		{
			var gatetewayBillingJournalLine = gatewayBillingCharge.Factory.Load<JobRevenueJournalLine>(gatewayBillingCharge.JR_AL_ARLine);
			if (gatetewayBillingJournalLine != null && gatetewayBillingJournalLine.ParentJournal != null)
			{
				return gatetewayBillingJournalLine.ParentJournal;
			}

			var journal = CreateJournal(gatewayBillingCharge.Factory);

			using (journal.GetValidationSuspender())
			{
				var consolRef = gatewayBillingCharge.Factory.Load<JobConsolCost>(gatewayBillingCharge.JR_E6_GatewaySellHeader)?.Consol.JK_UniqueConsignRef;

				journal.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(Constants.TransactionCategory.Codes.AutoJobRevenueJournal, Res.GetString("8952AEE2-249E-4D33-9A73-0F48FC8A0E2E", "GATEWAY SELL APPORTIONMENT"));
				if (!string.IsNullOrWhiteSpace(consolRef))
				{
					journal.AH_Desc += " " + consolRef;
				}
#if DEBUG

				var autoJRJCreatorCacheService_ForTestOnly = FactoryLevelPropertyStorageForTests.GetInstance(journal.Factory);
				if (autoJRJCreatorCacheService_ForTestOnly != null)
				{
					autoJRJCreatorCacheService_ForTestOnly.AddNewElement(journal.PK, "FindOrCreateGatewayBillingChargeLineAndJournal_JournalIsValidationSuspended", journal.IsValidationSuspended);
				}

#endif

				AddJournalLineSetValuesAndLinkToCharge(gatewayBillingCharge, journal, false);
			}

			gatewayBillingCharge.AddGSHBillingEvent(AccBillingEvents.Codes.GWSellApportionmentPosted);

			return journal;
		}

		static void AddJournalLineSetValuesAndLinkToCharge(ChargeWithCost charge, JobRevenueJournal journal, bool useCostValues)
		{
			var isBoth = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(journal.AH_GC.ToGuid(), Guid.Empty, Guid.Empty) == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			var osAmount = useCostValues ? charge.JR_OSCostAmt : charge.JR_OSSellAmt;
			var localAmount = useCostValues ? charge.JR_LocalCostAmt : charge.JR_LocalSellAmt;

			var line = journal.JournalLines.AddNew();
#if DEBUG

			var autoJRJCreatorCacheService_ForTestOnly = FactoryLevelPropertyStorageForTests.GetInstance(journal.Factory);
			if (autoJRJCreatorCacheService_ForTestOnly != null)
			{
				autoJRJCreatorCacheService_ForTestOnly.AddNewElement(line.PK, "AddJournalLineSetValuesAndLinkToCharge_LineIsValidationSuspended", line.IsValidationSuspended);
				autoJRJCreatorCacheService_ForTestOnly.AddNewElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalIsValidationSuspended", journal.IsValidationSuspended);
				autoJRJCreatorCacheService_ForTestOnly.AddNewElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalLinesIsListChangedSuspended", ((IBusinessObjectCollectionInternals)journal.JournalLines).IsListChangedSuspended);
			}

#endif
			if (isBoth)
			{
				line.SetLocalAmountDebitCredit(localAmount < 0);
				line.SetOSAmountDebitCredit(osAmount < 0);
				line.CostRevenueType = useCostValues ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue;
			}
			line.AL_AC = charge.JR_AC;
			line.AL_JH = charge.JR_JH;
			line.AL_Desc = charge.JR_Desc;
			line.AL_GB = charge.JR_GB;
			line.AL_GE = charge.JR_GE;
			line.AL_RX_NKTransactionCurrency = useCostValues ? charge.JR_RX_NKCostCurrency : charge.JR_RX_NKSellCurrency;

			line.DebitCreditSign = isBoth
				? useCostValues ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR
				: (useCostValues ^ localAmount < 0) ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;

			line.AL_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, osAmount);

			line.OSUnsignedLineAmount = Math.Abs(osAmount);

			using (line.GetOSAmountCalculationSuspender())
			{
				line.LocalUnsignedLineAmount = Math.Abs(localAmount);
			}

			using (charge.Calculations.SuspendCalculations())
			{
				if (useCostValues)
				{
					charge.ReverseAccrual(ZDateTime.Now);
					charge.JR_AL_APLine = line.PK;
				}
				else
				{
					charge.ReverseWIP(ZDateTime.Now);
					charge.JR_AL_ARLine = line.PK;
				}
			}
		}
	}
}
