using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSBilling : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		public CASSBilling(BusinessObjectFactory factory)
			: base(factory)
		{
			Tracer = ObjectFactory.Get<ITracer>();
		}

		#region Methods

		public void Initialize(CASSCostHeader header)
		{
			if (header != null)
			{
				using (GetValidationSuspender())
				{
					SetCostHeader(header);
				}
			}
		}

		public void ForceRecalculateData()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			foreach (CASSBillingLine line in Lines)
			{
				line.ForceRecalculateData();
			}
			foreach (CASSBillingLine line in HiddenLines)
			{
				line.ForceRecalculateData();
			}

			ClearAPTransactions();

			RunPreSaveValidation();
			Lines.RunPreSaveValidation();

			HideLinesWithNotAllowedDiscrepancy();
		}

		public void ClearAPTransactions()
		{
			if (APTransactions != null && APTransactions.Count > 0)
			{
				var aPTransactionsArray = APTransactions.ToArray<InvoicingBase>();
				APTransactions.RemoveAll();
				foreach (var transaction in aPTransactionsArray.Where(x => !x.IsPostedToCASSOrSaved))
				{
					transaction.Delete();
				}

				SerializedTransactions.Clear();
				UnpostedConsolCostsToDeleteIndexedByInvoice.Clear();
				LinesWithInvoicePK.Clear();

				apTransactionsFactory = null;
				apTransactions.SwapFactoryAndRemoveAll(APTransactionsFactory);

				ResetGSTTaxRate();
				ResetFREEGSTTaxRate();
			}
		}

		void HideLinesWithNotAllowedDiscrepancy()
		{
			decimal discrepancy = AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.Value;
			bool isDiscrepancyFilterActivated = discrepancy != AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.DefaultValue;
			List<ZGuid> linesToRemove = new List<ZGuid>();

			foreach (CASSBillingLine line in Lines)
			{
				if (Math.Abs(line.CostDifference) < discrepancy && isDiscrepancyFilterActivated)
				{
					HiddenLines.Add(line);
					linesToRemove.Add(line.PK);
				}
			}
			foreach (ZGuid guid in linesToRemove)
			{
				Lines.Remove(guid);
			}

			linesToRemove.Clear();

			foreach (CASSBillingLine line in HiddenLines)
			{
				if (Math.Abs(line.CostDifference) >= discrepancy || !isDiscrepancyFilterActivated ||
						line.HasNotifications() || line.HasRowNotifications)
				{
					Lines.Add(line);
					linesToRemove.Add(line.PK);
				}
			}
			foreach (ZGuid guid in linesToRemove)
			{
				HiddenLines.Remove(guid);
			}

			ForceRecalculateTotals();
		}

		void ForceRecalculateTotals()
		{
			fTotalCASSCostAdjustedValue = null;
			fTotalCASSCostValue = null;
			fTotalCASSRejectedClaimValue = null;
			fTotalCostDifferenceValue = null;
			fTotalSystemCostAccrualValue = null;
			fTotalNetCASSCostValue = null;

			fTotalHiddenCASSCostAdjustedValue = null;
			fTotalHiddenCASSCostValue = null;
			fTotalHiddenCASSRejectedClaimValue = null;
			fTotalHiddenCostDifferenceValue = null;
			fTotalHiddenSystemCostAccrualValue = null;
			fTotalHiddenNetCASSCostValue = null;

			TotalCASSCostAdjustedValueInfo.RefreshBinding();
			TotalCASSCostValueInfo.RefreshBinding();
			TotalCASSRejectedClaimValueInfo.RefreshBinding();
			TotalCostDifferenceValueInfo.RefreshBinding();
			TotalSystemCostAccrualValueInfo.RefreshBinding();
			TotalNetCASSCostValueInfo.RefreshBinding();

			TotalHiddenCASSCostAdjustedValueInfo.RefreshBinding();
			TotalHiddenCASSCostValueInfo.RefreshBinding();
			TotalHiddenCASSRejectedClaimValueInfo.RefreshBinding();
			TotalHiddenCostDifferenceValueInfo.RefreshBinding();
			TotalHiddenSystemCostAccrualValueInfo.RefreshBinding();
			TotalHiddenNetCASSCostValueInfo.RefreshBinding();

			TotalAllCASSCostAdjustedValueInfo.RefreshBinding();
			TotalAllCASSCostValueInfo.RefreshBinding();
			TotalAllCASSRejectedClaimValueInfo.RefreshBinding();
			TotalAllCostDifferenceValueInfo.RefreshBinding();
			TotalAllSystemCostAccrualValueInfo.RefreshBinding();
			TotalAllNetCASSCostValueInfo.RefreshBinding();
		}

		public void CreateInvoices()
		{
			CreateInvoices(false);
		}

		bool CreateInvoices(bool doPostingForEachInvoice)
		{
			if (ShouldInvoicesBeCreated)
			{
				ClearAPTransactions();
			}
			else
			{
				return false;
			}

			var dictionary = GroupLinesByInvoices();
			if (dictionary.Keys.Count == 0)
			{
				return false;
			}

			bool allTransactionsSaved = true;
			int counter = 0;
			var processStatus = doPostingForEachInvoice ? Res.GetString("E49A9418-458D-4CE6-8FAD-7EA04D32D07C", "Posting of AP Invoices") :
														Res.GetString("0FCE01BD-FC59-4D1B-A914-0CD828EF1E91", "Calculation of AP Invoices");
			using (var longProcessEventHelper = GetLongTimeProcessEventHelper(processStatus, dictionary.Keys.Count))
			using (APTransactions.SuspendListChanged())
			{
				longProcessEventHelper.RiseProsessStartEvent();

				foreach (InvoiceKey key in dictionary.Keys)
				{
					allTransactionsSaved &= CreateInvoice(key, dictionary[key], doPostingForEachInvoice);

					longProcessEventHelper.RiseProcessProgressEvent(++counter);
				}
				APTransactions.SetReadOnlyIncludingChildren(true);
			}

			Lines.ForEach(x => ((CASSBillingLine)x).IsInvoiceNumberChanged = false);

			return allTransactionsSaved;
		}

		public InvoicingBase GenerateCompleteInvoice(InvoicingBase invoiceHeader)
		{
			InvoicingBase completeInvoice = null;
			var success = false;
			try
			{
				if (invoiceHeader.IsPostedToCASSOrSaved)
				{
					var currentFactory = CreateNewFactory();
					var invoiceQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, invoiceHeader.AH_Ledger);
					invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, invoiceHeader.AH_TransactionType);
					invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, invoiceHeader.AH_TransactionNum);
					invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, invoiceHeader.AH_OH);
					invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB, invoiceHeader.AH_GB);
					invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoiceHeader.AH_GC);
					completeInvoice = currentFactory.LoadTop1<InvoicingBase>(invoiceQuery);

					if (completeInvoice == null)
					{
						invoiceHeader.IsPostedToCASSOrSaved = false;
						throw new TransactionNotFoundException("Posted invoice was not found in database.");
					}
				}
				else
				{
					completeInvoice = DeserializeCompleteInvoice(invoiceHeader);
				}
				completeInvoice.SetReadOnlyIncludingChildren(true);
				success = true;
				return completeInvoice;
			}
			finally
			{
				if (!success)
				{
					completeInvoice?.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public bool PostAPTransactions()
		{
			bool allTransactionsSaved = true;

			if (!ShouldInvoicesBeCreatedForPosting)
			{
				LongTimeProcessEventHelper longProcessEventHelper;
				using (longProcessEventHelper = GetLongTimeProcessEventHelper(Res.GetString("19a72033-90de-4ae2-962e-2e396e60efa3", "Posting of AP Invoices"), APTransactions.Count))
				{
					longProcessEventHelper.RiseProsessStartEvent();

					for (int i = 0; i < APTransactions.Count; i++)
					{
						var serializedInvoice = APTransactions[i];
						if (serializedInvoice.IsPostedToCASSOrSaved)
						{ continue; }

						InvoicingBase completeInvoice = DeserializeCompleteInvoice(serializedInvoice);
						try
						{
							allTransactionsSaved &= SaveOriginalInvoiceIfNoErrors(completeInvoice, serializedInvoice);
							longProcessEventHelper.RiseProcessProgressEvent(i + 1);
						}
						finally
						{
							completeInvoice.ReleaseAllMutexOnInvoice();
							GCWrapper.ReclaimMemory(ref completeInvoice);
						}
					}
				}
			}
			else
			{
				allTransactionsSaved = CreateInvoices(true);
			}

			return allTransactionsSaved;
		}

		public bool AreAnyTransactionsPosted => IsPostedToCASSDictionary.Values.FirstOrDefault(x => x);

		Dictionary<InvoiceKey, List<CASSBillingLine>> GroupLinesByInvoices()
		{
			var dictionary = new Dictionary<InvoiceKey, List<CASSBillingLine>>();

			PopulateInvoiceKeyDictionary(Lines);
			PopulateInvoiceKeyDictionary(HiddenLines);

			return dictionary;

			void PopulateInvoiceKeyDictionary(CASSBillingLineCollection billingLines)
			{
				foreach (CASSBillingLine line in billingLines)
				{
					if (line.Creditor != null && line.CASSCostCurrency != null)
					{
						var key = new InvoiceKey(line.Creditor.PK, line.CASSCostCurrency.RX_Code, line.InvoiceNumber);
						if (!dictionary.ContainsKey(key))
						{
							var valueLines = new List<CASSBillingLine>();
							valueLines.Add(line);
							dictionary.Add(key, valueLines);
						}
						else
						{
							if (!dictionary[key].Contains(line))
							{
								dictionary[key].Add(line);
							}
						}
					}
				}
			}
		}

		bool CreateInvoice(InvoiceKey key, IEnumerable<CASSBillingLine> cassBillingLinesForInvoice, bool doPosting)
		{
			var allTransactionsSaved = false;
			var invoice = IsInvoiceTotalPositive(cassBillingLinesForInvoice) ? CreateNewFactory().New<APInvoice>() :
													(InvoicingBase)CreateNewFactory().New<APCreditNote>();

			try
			{
				using (new DisposableAction(() => invoice.Factory.SuspendValidation(), () => invoice.Factory.ResumeValidation()))
				using (GetHookInvoiceEventsHelper(invoice))
				using (invoice.GetValidationSuspender())
				using (invoice.GetConsolCostImportPopupSuspender())
				using (invoice.GetSetGSTOnLinesSuspender())
				using (invoice.ConsolCosting.ConsolCosts.SuspendListChanged())
				using (invoice.ConsolCosting.ConsolSummary.UpdateSuspender.GetSuspender())
				using (invoice.Lines.SuspendListChanged())
				{
					invoice.AH_OH = key.CreditorPK;
					invoice.ExchangeRate.Currency = key.CurrencyNK;

					if (key.InvoiceNumber.IsEmpty)
					{
						var baseInvoiceNumber = InvoiceLiteralNumberGenerator.GetAutoAPInvoiceNumberForCASS(invoice.AH_RX_NKTransactionCurrency);
						invoice.AH_TransactionNum = InvoiceLiteralNumberGenerator.GetNextLiteralAPInvoiceNumberForCASS(APTransactionsFactory, invoice, baseInvoiceNumber);
					}
					else
					{
						invoice.AH_TransactionNum = key.InvoiceNumber;
					}

					if (!BillingDate.IsEmpty)
					{
						invoice.AH_InvoiceDate = BillingDate;
					}
					invoice.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.CASS, Res.GetString("1434ef5f-dc1f-4dc4-b006-cc57bf808def", "CASS discrepancy clearing"));
					invoice.AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.CASS, 0);
					invoice.SubmittedFromInvoicingForm = true;

					using (invoice.Lines.SuspendListChanged())
					{
						foreach (CASSBillingLine billingLine in cassBillingLinesForInvoice)
						{
							var lineCreator = GetInvoiceLineCreator(invoice, billingLine);
							lineCreator.CreateInvoiceLines();
						}
					}
					invoice.ImportAllApportionmentsFromCosting();
					Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, BuildTraceMessage);
				}

				if (invoice.Lines.Count != 0)
				{
					var invoiceHeaderForCollection = SerializeInvoiceToProvideViewFunctionalityWithoutKeepingAllStuffInMemory(invoice);
					APTransactions.Add(invoiceHeaderForCollection);

					LinesWithInvoicePK.Add(invoiceHeaderForCollection.PK, cassBillingLinesForInvoice);

					if (doPosting)
					{
						allTransactionsSaved = SaveOriginalInvoiceIfNoErrors(invoice, invoiceHeaderForCollection);
					}
				}
				else
				{
					if (BillingDate.IsValid && cassBillingLinesForInvoice.First().ConsolID.IsValid)
					{
						var exchangeRate = ExchangeRateCalculator.GetRate(key.CurrencyNK, ExchangeRateType.Buy, BillingDate.ToDateTime());
						if (exchangeRate == 0M)
						{
							Globals.Message.ShowError(Res.GetString("4D4C51A4-AA9A-4D1B-9415-7A9FBA0070A2", "BUY Exchange rate for invoice date {0} is missing", BillingDate.ToShortDateString()));
						}
					}
				}
			}
			finally
			{
				invoice.ReleaseAllMutexOnInvoice();
				GCWrapper.ReclaimMemory(ref invoice);
			}

			return allTransactionsSaved;

			#region Trace Message
			#region SuppressResourceStringsCheckRegion

			string BuildTraceMessage()
			{
				if (invoice?.ConsolCosting.ConsolCosts.Any() ?? false)
				{
					var consolCostInfo = string.Join("\r\n", invoice?.ConsolCosting.ConsolCosts.Select(c => c.GetJobConsolCostInfo()).ToArray());
					return FormattableString.Invariant($"Traced @{nameof(CreateInvoice)} after calling invoice.{nameof(invoice.ImportAllApportionmentsFromCosting)}-->Invoice Number: {invoice.AH_TransactionNum}.\r\nList of imported consol costs\r\n{consolCostInfo}");
				}
				return string.Empty;
			}

			#endregion
			#endregion
		}

		InvoicingBase SerializeInvoiceToProvideViewFunctionalityWithoutKeepingAllStuffInMemory(InvoicingBase invoice)
		{
			var invoiceHeaderForCollection = (InvoicingBase)APTransactionsFactory.New(invoice.GetType());
			using (new DisposableAction(() => invoiceHeaderForCollection.SuspendValidation(), () => invoiceHeaderForCollection.ResumeValidation()))
			{
				invoiceHeaderForCollection.AH_OH = invoice.AH_OH;
				invoiceHeaderForCollection.ExchangeRate.Currency = invoice.ExchangeRate.Currency;
				invoiceHeaderForCollection.AH_TransactionNum = invoice.AH_TransactionNum;
				invoiceHeaderForCollection.AH_InvoiceDate = invoice.AH_InvoiceDate;
				invoiceHeaderForCollection.AH_Desc = invoice.AH_Desc;
				invoiceHeaderForCollection.SubmittedFromInvoicingForm = true;
				invoiceHeaderForCollection.AH_OSTotalAmount = invoice.AH_OSTotalAmount;
				invoiceHeaderForCollection.AH_LocalExTaxAmount = invoice.AH_LocalExTaxAmount;

				invoiceHeaderForCollection.AH_LocalTaxAmount = invoice.AH_LocalTaxAmount;

				var lineForCollection = (InvoicingLineBase)invoiceHeaderForCollection.Lines.AddNew();
				var firstCharge = Factory.LoadTop1<GenericCharge.GenericCharge>(lineForCollection.ChargeList.CompleteFilter);
				lineForCollection.GenericCharge = firstCharge != null ? firstCharge.PK : ZGuid.Empty;
				lineForCollection.AL_OSAmount = invoice.AH_OSTotalAmount;
				lineForCollection.AL_LocalExTaxAmount = invoice.AH_LocalExTaxAmount;
				lineForCollection.AL_LocalTaxAmount = invoice.AH_LocalTaxAmount;

				ValidateAndCopyErrorsToSerializedInvoice(invoice, invoiceHeaderForCollection);

				//set "IsPostedToCASS" property on invoice based on any previous postings
				var isPostedPreviouslyKey = new InvoiceKey(invoiceHeaderForCollection.AH_OH, invoiceHeaderForCollection.AH_RX_NKTransactionCurrency, invoiceHeaderForCollection.AH_TransactionNum);
				if (IsPostedToCASSDictionary.ContainsKey(isPostedPreviouslyKey))
				{
					if (IsPostedToCASSDictionary[isPostedPreviouslyKey])
					{
						invoiceHeaderForCollection.IsPostedToCASSOrSaved = true;
					}
				}
				else
				{
					IsPostedToCASSDictionary.Add(isPostedPreviouslyKey, false);
					invoiceHeaderForCollection.IsPostedToCASSOrSaved = false;
				}

				SerializedTransactions.Add(invoiceHeaderForCollection.PK, invoice.Serialize());
			}
			return invoiceHeaderForCollection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		InvoicingBase DeserializeCompleteInvoice(InvoicingBase invoiceHeader)
		{
			var currentFactory = CreateNewFactory();
			var completeInvoice = (InvoicingBase)currentFactory.New(invoiceHeader.GetType());
			try
			{
				using (GetHookInvoiceEventsHelper(completeInvoice))
				using (completeInvoice.GetValidationSuspender())
				using (completeInvoice.GetConsolCostImportPopupSuspender())
				using (completeInvoice.GetSetGSTOnLinesSuspender())
				{
					completeInvoice.AH_OH = invoiceHeader.AH_OH;
					completeInvoice.ExchangeRate.Currency = invoiceHeader.ExchangeRate.Currency;
					completeInvoice.AH_TransactionNum = invoiceHeader.AH_TransactionNum;
					completeInvoice.AH_InvoiceDate = invoiceHeader.AH_InvoiceDate;
					completeInvoice.AH_Desc = invoiceHeader.AH_Desc;
					completeInvoice.SubmittedFromInvoicingForm = true;
					completeInvoice.ShowError = invoiceHeader.ShowError;

					var error = completeInvoice.Deserialize(SerializedTransactions[invoiceHeader.PK]);

					if (!string.IsNullOrEmpty(error))
					{
						completeInvoice.RaiseShowError(error, Res.GetString("1012fffe-0aa6-478e-82a8-34c29d250492", "Error reading saved transaction data"));
					}

					completeInvoice.ImportAllApportionmentsFromCosting();
				}

				ValidateAndCopyErrorsToSerializedInvoice(completeInvoice, invoiceHeader);
				return completeInvoice;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (completeInvoice != null)
				{
					completeInvoice.ReleaseAllMutexOnInvoice();
				}
				throw;
			}
		}

		void DeleteConsolCostsForInvoice(InvoicingBase originalInvoice)
		{
			var invoiceNumber = originalInvoice.AH_TransactionNum;

			if (UnpostedConsolCostsToDeleteIndexedByInvoice.Keys.Contains(invoiceNumber))
			{
				var costsToLookupForDeletion = UnpostedConsolCostsToDeleteIndexedByInvoice[invoiceNumber];
				var chargeCodesToQuery = costsToLookupForDeletion.Select(c => c.ChargeCodeId).ToList();
				var consolCostsToDelete = new HashSet<JobConsolCost>();

				var consolCostsToSearch = new ConsolCostListWithStrategy(ConsolCostStrategy.ConsolCostCalculationStrategyWithoutCalculations);
				using (var chargesToSearch = new TransactionLineJobChargeTransformer.ChargesByPK())
				{
					TransactionLineJobChargeTransformer.PopulateCostsAndChargesToSearch(
						originalInvoice.Factory, originalInvoice, chargesToSearch, consolCostsToSearch, chargeCodesToQuery);
				}

				foreach (var costToDelete in costsToLookupForDeletion)
				{
					foreach (JobConsolCost existingCost in consolCostsToSearch)
					{
						if (TransactionLineJobChargeTransformer.CanImportNewCostIntoExistingCost(existingCost,
							costToDelete.ChargeCodeId, costToDelete.ConsolId))
						{
							consolCostsToDelete.Add(existingCost);
							break;
						}
					}
				}

				foreach (var costToDelete in consolCostsToDelete)
				{
					costToDelete.DeleteCostAndCharges();
				}
			}
		}

		static void ValidateAndCopyErrorsToSerializedInvoice(InvoicingBase originalInvoice, InvoicingBase serializedInvoice)
		{
			originalInvoice.RegisterEditableChildObject(originalInvoice.ConsolCosting);
			try
			{
				originalInvoice.SetComplianceSubTypeIfIsNecessary();
				originalInvoice.RunPreSaveValidation();

				serializedInvoice.ClearAllNotifications();
				foreach (INotification notification in originalInvoice.NotificationsIncludingChildren)
				{
					serializedInvoice.AddRowNotification(notification);
				}
			}
			finally
			{
				originalInvoice.UnRegisterEditableChildObject(originalInvoice.ConsolCosting);
			}
		}

		bool SaveOriginalInvoiceIfNoErrors(InvoicingBase originalInvoice, InvoicingBase serializedInvoice)
		{
			bool success = false;
			if (!originalInvoice.HasErrors)
			{
				var allClaimsAreCreated = CreateClaims(originalInvoice, serializedInvoice);
				if (allClaimsAreCreated)
				{
					DeleteConsolCostsForInvoice(originalInvoice);
					originalInvoice.Factory.RefreshEnabled = false;
					originalInvoice.Factory.Save();
					serializedInvoice.IsPostedToCASSOrSaved = true;
					IsPostedToCASSDictionary[new InvoiceKey(serializedInvoice.AH_OH, serializedInvoice.AH_RX_NKTransactionCurrency, serializedInvoice.AH_TransactionNum)] = true;
					success = true;
				}
			}

			return success;
		}

		bool CreateClaims(InvoicingBase originalInvoice, InvoicingBase serializedInvoice)
		{
			var result = true;

			foreach (var cassBillingLineForInvoice in LinesWithInvoicePK[serializedInvoice.PK])
			{
				var (claimCreationResult, errorMessages) = cassBillingLineForInvoice.ClaimCreator.CreateOrUpdateClaim(originalInvoice, OverrideAutoClosureOfCASSBillingClaim);
				result &= errorMessages.IsNullOrEmpty();

				if (!errorMessages.IsNullOrEmpty())
				{
					serializedInvoice.AddRowError(Res.GetString("223c2ab3-1fb7-4283-91a8-3bf1d502a961", "Could not create/update claim with MAWB: {0} because of following validation error: \r\n{1}", LinesWithInvoicePK[serializedInvoice.PK].First().MAWBNumber, string.Join("\r\n", errorMessages)));
				}
				else if (!claimCreationResult && errorMessages.IsNullOrEmpty())
				{
					serializedInvoice.AddRowWarning(Res.GetString("9657c714-128b-43ac-9f58-0393538fbded", "Could not find any claim for MAWB: {0}. Its status or amount might be modified.", LinesWithInvoicePK[serializedInvoice.PK].First().MAWBNumber));
				}

				if (!result)
				{
					break;
				}
			}

			return result;
		}

		InvoiceLineCreatorBase GetInvoiceLineCreator(InvoicingBase invoice, CASSBillingLine billingLine)
		{
			InvoiceLineCreatorBase lineCreator = null;

			if (!billingLine.ConsolPK.IsValid)
			{
				lineCreator = new InvoiceLineCreatorForGLAccount(invoice, billingLine, GSTTaxRate, FREEGSTTaxRate);
			}
			else if (billingLine.IsForGatewayBilling)
			{
				lineCreator = new InvoiceLineCreatorForJob(invoice, billingLine, GSTTaxRate, FREEGSTTaxRate, () => { return TotalCostCorrector<InvoicingLineBase>.CreateForInvoiceLines(invoice.Lines); });
			}
			else
			{
				lineCreator = new InvoiceLineCreatorForConsolCost(invoice, billingLine, GSTTaxRate, FREEGSTTaxRate, () => { return TotalCostCorrector<JobConsolCost>.CreateForConsolCosts(invoice.AH_TransactionNum, invoice.ConsolCosting.ConsolCosts, UnpostedConsolCostsToDeleteIndexedByInvoice); });
			}

			return lineCreator;
		}

		void ValidateCriticalPropertiesForInvoiceAndClaimGeneration()
		{
			Lines.Cast<CASSBillingLine>().ForEach(x => x.ValidateInvoiceAndClaimRelatedCriticalProperties());
			ValidateCASSBillingLineInvoiceNumbers();
		}

		void ValidateCASSBillingLineInvoiceNumbers()
		{
			Lines.Cast<CASSBillingLine>().ForEach(x => x.ClearCriticalError(CASSBillingLine.Schema.InvoiceNumber));

			var invoiceNumbers = GroupLinesByInvoices();
			if (!AreThereInvoiceNumberDuplicatesWithSameCreditorButDifferentCurrency(invoiceNumbers))
			{
				foreach (InvoiceKey invoiceNumberCreditorKey in invoiceNumbers.Keys.Where(x => !x.InvoiceNumber.IsEmpty))
				{
					var transactionType = IsInvoiceTotalPositive(invoiceNumbers[invoiceNumberCreditorKey]) ? TransactionTypes.Invoice : TransactionTypes.CreditNote;
					var invoiceDate = !BillingDate.IsEmpty ? BillingDate : ZDateTime.Today;
					var invoiceNumberCheck = AccountingUtils.APTransactionNumberExists(transactionType, invoiceNumberCreditorKey.InvoiceNumber, invoiceNumberCreditorKey.CreditorPK, invoiceDate);
					if (invoiceNumberCheck.HasNotification)
					{
						if (invoiceNumberCheck.NotificationType == CargoWise.ComponentModel.NotificationType.Error)
						{
							invoiceNumbers[invoiceNumberCreditorKey].ForEach(x => x.AddCriticalError(CASSBillingLine.Schema.InvoiceNumber, invoiceNumberCheck.NotificationMessage));
						}
						else if (invoiceNumberCheck.NotificationType == CargoWise.ComponentModel.NotificationType.Warning)
						{
							invoiceNumbers[invoiceNumberCreditorKey].ForEach(x => x.InvoiceNumberInfo.AddWarning(invoiceNumberCheck.NotificationMessage));
						}
					}
				}
			}

			Lines.Cast<CASSBillingLine>().ForEach(x => x.ValidateInvoiceNumber());
		}

		bool AreThereInvoiceNumberDuplicatesWithSameCreditorButDifferentCurrency(Dictionary<InvoiceKey, List<CASSBillingLine>> billingLines)
		{
			bool result = false;

			var duplicatedInvoicNumbers = from key in billingLines.Keys
										  where !key.InvoiceNumber.IsEmpty
										  group key by new { key.InvoiceNumber, key.CreditorPK } into g
										  where g.GroupBy(x => x.CurrencyNK).Count() > 1
										  select g;
			foreach (var key in duplicatedInvoicNumbers.SelectMany(x => x))
			{
				result = true;
				billingLines[key].ForEach(x => x.AddCriticalError(CASSBillingLine.Schema.InvoiceNumber, CASSBillingLine.GetInvoiceNumberDuplicateInCollectionErrorMessage()));
			}

			return result;
		}

		bool IsInvoiceTotalPositive(IEnumerable<CASSBillingLine> cassBillingLines)
		{
			ZDecimal invoiceTotal = cassBillingLines.Sum(line => line.CASSCostValue + line.CASSCostAdjustedValue + line.CASSCostTaxValue + line.CASSCostTaxAdjustedValue);
			return invoiceTotal > 0;
		}

		#endregion

		readonly ITracer Tracer;

		#region Events

		public class LongTimeEventArgs : EventArgs
		{
			public readonly string Status;
			public readonly int PercentComplete;

			public LongTimeEventArgs(string status, int percentComplete)
			{
				this.Status = status;
				this.PercentComplete = percentComplete;
			}
		}

		public event EventHandler<LongTimeEventArgs> OnStartLongTimeProcess;
		public event EventHandler<LongTimeEventArgs> OnProgressLongTimeProcess;
		public event EventHandler OnEndLongTimeProcess;
		public event EventHandler<JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs> JobCreationError;

		LongTimeProcessEventHelper GetLongTimeProcessEventHelper(string processStatus, int itemsToProcessAmount)
		{
			return new LongTimeProcessEventHelper(this, processStatus, itemsToProcessAmount);
		}

		class LongTimeProcessEventHelper : IDisposable
		{
			readonly CASSBilling Parent;
			readonly ZDateTime StartTime = ZDateTime.Now;
			readonly int ItemsToProcessAmount;
			readonly string ProcessStatus;
			readonly Stopwatch timer = new Stopwatch();

			public LongTimeProcessEventHelper(CASSBilling parent, string processStatus, int itemsToProcessAmount)
			{
				this.Parent = parent;
				this.ItemsToProcessAmount = itemsToProcessAmount;
				this.ProcessStatus = processStatus.TrimEnd('.').Trim();
			}

			public void RiseProsessStartEvent() => Parent.OnStartLongTimeProcess?.Invoke(this, new LongTimeEventArgs(ProcessStatus + "...", 0));

			public void RiseProcessProgressEvent(int itemsProcessed)
			{
				if (Parent.OnProgressLongTimeProcess != null &&
					(!timer.IsRunning || timer.ElapsedMilliseconds >= 500))
				{
					timer.Restart();
					TimeSpan timeElapsed = ZDateTime.Now - StartTime;
					Parent.OnProgressLongTimeProcess(this, new LongTimeEventArgs(
						ProcessStatus + " " +
						Res.GetString("BD631D29-5DF4-43F2-AFA2-F1C1ABDC3FA2", @"{0} of {1}", itemsProcessed, ItemsToProcessAmount) +
						System.Environment.NewLine +
						Res.GetString("22833CD3-85E4-4311-A3B5-2C3C4996AB5B", "time elapsed: {0:00}:{1:00}:{2:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds),
						100 * itemsProcessed / ItemsToProcessAmount));
				}
			}

			public void Dispose()
			{
				Parent.OnEndLongTimeProcess?.Invoke(this, EventArgs.Empty);
			}
		}

		void JobCreationErrorHandler(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
		{
			if (JobCreationError != null)
			{
				JobCreationError(sender, e);
			}
		}

		IDisposable GetHookInvoiceEventsHelper(InvoicingBase invoice)
		{
			return new HookInvoiceEventsHelper(this, invoice);
		}

		class HookInvoiceEventsHelper : IDisposable
		{
			public HookInvoiceEventsHelper(CASSBilling parent, InvoicingBase invoice)
			{
				this.invoice = invoice;
				this.parent = parent;
				JobCreationError += parent.JobCreationErrorHandler;
				invoice.ConsolCosting.ConsolCosts.OnJobCreationError += JobCreationErrorHandler;
			}

			public void Dispose()
			{
				invoice.ConsolCosting.ConsolCosts.OnJobCreationError -= JobCreationErrorHandler;
				JobCreationError -= parent.JobCreationErrorHandler;
			}

			readonly InvoicingBase invoice;
			readonly CASSBilling parent;
			event EventHandler<JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs> JobCreationError;

			void JobCreationErrorHandler(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
			{
				JobCreationError?.Invoke(sender, e);
			}
		}

		#endregion

		#region Implementation

		new BusinessObjectFactory CreateNewFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.SetContext(BusinessContext.CASS);

			return factory;
		}

		class InvoiceKey
		{
			public InvoiceKey(ZGuid creditorPK, ZString currencyNK, ZString invoiceNumber)
			{
				CreditorPK = creditorPK;
				CurrencyNK = currencyNK;
				InvoiceNumber = invoiceNumber;
			}

			public readonly ZGuid CreditorPK;
			public readonly ZString CurrencyNK;
			public readonly ZString InvoiceNumber;

			public override bool Equals(object obj)
			{
				var key = (InvoiceKey)obj;
				return key.CurrencyNK == this.CurrencyNK && key.CreditorPK == this.CreditorPK && key.InvoiceNumber == this.InvoiceNumber;
			}

			public override int GetHashCode()
			{
				return InvoiceNumber.GetHashCode() ^ CreditorPK.GetHashCode() ^ CurrencyNK.GetHashCode();
			}
		}

		#region InvoiceLineCreators

		internal delegate TotalCostCorrector<T> CostCorrector<T>() where T : BusinessObject;

		internal abstract class InvoiceLineCreatorBase
		{
			protected InvoiceLineCreatorBase(InvoicingBase invoice, CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate)
			{
				this.billingLine = billingLine;
				this.invoice = invoice;
				this.gstTaxRate = gstTaxRate;
				this.freeGSTTaxRate = freeGSTTaxRate;
			}
			readonly CASSBillingLine billingLine;
			readonly InvoicingBase invoice;
			readonly AccTaxRate gstTaxRate;
			readonly AccTaxRate freeGSTTaxRate;

			public void CreateInvoiceLines()
			{
				var allLineInfo = IsCostAndTaxDistributionRequired ? CostDistributor.GetInvoiceLineInfoWithDistribution(IsTaxApplicable) : CostDistributor.GetInvoiceLineInfoWithoutDistribution(IsTaxApplicable);
				foreach (var lineInfo in allLineInfo)
				{
					AddLineToInvoice(lineInfo);
				}
				MarkInvoiceWhereTaxIsRecalculated();
			}

			protected abstract void AddLineToInvoice(CASSCostDistributor.InvoiceLineInfo lineInfo);

			protected abstract bool IsCostAndTaxDistributionRequired { get; }

			protected CASSBillingLine BillingLine { get { return billingLine; } }

			protected InvoicingBase Invoice { get { return invoice; } }

			protected ZDecimal GetEffectiveExTaxAmount(AccTaxRate taxRate, ZDecimal exTaxAmount, ZDecimal taxAmount)
			{
				return AccountingUtils.Round(taxRate != null ? exTaxAmount : new ZDecimal(exTaxAmount + taxAmount), BillingLine.CASSCostCurrency);
			}

			protected ZDecimal GetEffectiveTaxAmount(AccTaxRate taxRate, ZDecimal taxAmount)
			{
				return AccountingUtils.Round(taxRate != null ? taxAmount : ZDecimal.Zero, BillingLine.CASSCostCurrency);
			}

			bool IsTaxApplicable
			{
				get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered && (Invoice?.Header.CompanyData.OB_APVATConfig ?? AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code) != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code; }
			}

			void MarkInvoiceWhereTaxIsRecalculated()
			{
				var isTaxRecalculated = !IsTaxApplicable && billingLine.IsGSTApplicable && (BillingLine.CASSCostTaxValue != 0 || BillingLine.CASSCostTaxAdjustedValue != 0);
				if (isTaxRecalculated && !Invoice.Notifications.Any(x => x.Message.Equals(InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage())))
				{
					Invoice.AddRowWarning(InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());
				}
			}

			CASSCostDistributor CostDistributor
			{
				get { return costDistributor = costDistributor ?? new CASSCostDistributor(BillingLine, gstTaxRate, freeGSTTaxRate); }
			}
			CASSCostDistributor costDistributor;
		}

		internal abstract class InvoiceLineCreator<T> : InvoiceLineCreatorBase
			where T : BusinessObject
		{
			protected InvoiceLineCreator(InvoicingBase invoice, CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate, CostCorrector<T> getCostCorrector)
				: base(invoice, billingLine, gstTaxRate, freeGSTTaxRate)
			{
				this.getCostCorrector = getCostCorrector;
			}
			readonly CostCorrector<T> getCostCorrector;

			protected TotalCostCorrector<T> GetNewCostCorrector()
			{
				return getCostCorrector();
			}
		}

		internal class InvoiceLineCreatorForGLAccount : InvoiceLineCreatorBase
		{
			public InvoiceLineCreatorForGLAccount(InvoicingBase invoice, CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate)
				: base(invoice, billingLine, gstTaxRate, freeGSTTaxRate)
			{
			}

			protected override void AddLineToInvoice(CASSCostDistributor.InvoiceLineInfo lineInfo)
			{
				int multiplier = Invoice is CreditNote ? -1 : 1;
				InvoicingLineBase invoiceLine = (InvoicingLineBase)Invoice.Lines.AddNew();
				invoiceLine.AL_JH = ZGuid.Empty;
				invoiceLine.GenericCharge = AccountingConfigurationRegistry.Instance.CASSGLAccount.Value;
				invoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
				invoiceLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
				invoiceLine.AL_AT = lineInfo.TaxRate?.PK ?? ZGuid.Empty;

				invoiceLine.AL_OSExTaxAmount = GetEffectiveExTaxAmount(lineInfo.TaxRate, lineInfo.ExTaxAmount * multiplier, lineInfo.TaxAmount * multiplier);
				invoiceLine.AL_OSTaxAmount = GetEffectiveTaxAmount(lineInfo.TaxRate, lineInfo.TaxAmount * multiplier);
			}

			protected override bool IsCostAndTaxDistributionRequired { get { return false; } }
		}

		public static bool GetUseExistingConsolCostTaxRate() => GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Italy;

		internal class InvoiceLineCreatorForConsolCost : InvoiceLineCreator<JobConsolCost>
		{
			public InvoiceLineCreatorForConsolCost(InvoicingBase invoice, CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate, CostCorrector<JobConsolCost> getCostCorrector)
				: base(invoice, billingLine, gstTaxRate, freeGSTTaxRate, getCostCorrector)
			{
				Tracer = ObjectFactory.Get<ITracer>();
			}

			protected override void AddLineToInvoice(CASSCostDistributor.InvoiceLineInfo lineInfo)
			{
				int multiplier = Invoice is CreditNote ? -1 : 1;
				var chargeCodeWithDistributedAmounts = lineInfo.DistributionInfo;
				var costCorrector = GetNewCostCorrector();

				foreach (CASSCostDistributor.CASSCostDistributorInfo chargeCodeWithDistributedAmount in chargeCodeWithDistributedAmounts)
				{
					JobConsolCost newCost = Invoice.ConsolCosting.ConsolCosts.AddNew();
					using (newCost.GetValidationSuspender())
					{
						using (var suspender = newCost.GetSuspenderForConsolCostImporter())
						{
							newCost.E6_AC_ChargeCode = chargeCodeWithDistributedAmount.ChargeCodePK;
							Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, () => FormattableString.Invariant($"{GetTraceMessagePrefix(newCost)}:Started Apportioning"));
						}

						newCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(BillingLine.ConsolPK, JobConsolSchema.Constants.Prefix);

						foreach (ApportionSplitCharge charge in newCost.ApportionmentCharges)
						{
							Tuple<ZGuid, ZGuid> key = new Tuple<ZGuid, ZGuid>(charge.JR_JH, charge.JR_AC);
							if (BillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(key))
							{
								charge.JR_GB = BillingLine.SystemChargeDataByJobAndChargeCode[key].JR_GB;
								charge.JR_GE = BillingLine.SystemChargeDataByJobAndChargeCode[key].JR_GE;
								Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, () => FormattableString.Invariant($"{GetTraceMessagePrefix(newCost)}:A job charge found in database for charge code {charge.ChargeCode.AC_Code} in job: Number={charge.Job.JH_JobNum} JobPK={charge.JR_JH}"));
							}
						}

						if (BillingLine.SystemCostAccrualValue == 0)
						{
							AddConsolCostsWithEquivalentApportionmentByChargeCodes(newCost, chargeCodeWithDistributedAmount, lineInfo.TaxRate, multiplier);
							Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, () => FormattableString.Invariant($"{GetTraceMessagePrefix(newCost)}:There is no matching consol cost in database"));
						}
						else
						{
							AddConsolCostsWithProportionalApportionmentByChargeCodes(newCost, chargeCodeWithDistributedAmount, lineInfo.TaxRate, multiplier);
						}

						costCorrector.MarkForAdjustment(newCost);
						Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, () => FormattableString.Invariant($"{GetTraceMessagePrefix(newCost)}:After completing Apportioning. Job cost details-->\r\n{newCost.GetJobConsolCostInfo()}"));
					}
				}

				var totalExTaxAmount = GetEffectiveExTaxAmount(lineInfo.TaxRate, lineInfo.ExTaxAmount * multiplier, lineInfo.TaxAmount * multiplier);
				var totalTaxAmount = GetEffectiveTaxAmount(lineInfo.TaxRate, lineInfo.TaxAmount * multiplier);
				costCorrector.AdjustValuesToSumCorrectlyAfterApportionment(totalExTaxAmount, totalTaxAmount);

				#region Trace Message
				#region SuppressResourceStringsCheckRegion
				string GetTraceMessagePrefix(JobConsolCost newCost) => FormattableString.Invariant($"{BillingLine.ConsolID}|{BillingLine.AWBNumber}|{BillingLine.CreditorCode}|{newCost.ChargeCode.AC_Code}");
				#endregion
				#endregion
			}

			protected override bool IsCostAndTaxDistributionRequired
			{
				get { return true; }
			}

			void AddConsolCostsWithEquivalentApportionmentByChargeCodes(JobConsolCost newCost, CASSCostDistributor.CASSCostDistributorInfo chargeCodeWithDistributedAmount, AccTaxRate taxRate, int multiplier)
			{
				using (newCost.UpdateE6_A9_VATClassSuspender.GetSuspender())
				{
					newCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
					newCost.E6_AT_TaxRate = taxRate?.PK ?? ZGuid.Empty;
					newCost.E6_A9_VATClass = taxRate?.AT_A9_DefaultVatClass ?? ZGuid.Empty;
					newCost.E6_OSCostAmount = GetEffectiveExTaxAmount(taxRate, chargeCodeWithDistributedAmount.Cost * multiplier, chargeCodeWithDistributedAmount.Tax * multiplier);
					newCost.E6_OSGSTAmount_Calc = GetEffectiveTaxAmount(taxRate, chargeCodeWithDistributedAmount.Tax * multiplier);
				}
			}

			void AddConsolCostsWithProportionalApportionmentByChargeCodes(JobConsolCost newCost, CASSCostDistributor.CASSCostDistributorInfo chargeCodeWithDistributedAmount, AccTaxRate taxRate, int multiplier)
			{
				var exTaxAmount = GetEffectiveExTaxAmount(taxRate, chargeCodeWithDistributedAmount.Cost * multiplier, chargeCodeWithDistributedAmount.Tax * multiplier);
				var taxRateFromConsolCost = false;

				var sysCostByChargeCodes = BillingLine.SystemCostValueByChargeCodes;
				var chargeCodeKey = chargeCodeWithDistributedAmount.ChargeCodePK;
				if (sysCostByChargeCodes.ContainsKey(chargeCodeKey))
				{
					var appSelectInfo = ConsolCostSelector.GetBestMatchedApportionMethod(ExtractApportionMethodSelectionInfo(sysCostByChargeCodes[chargeCodeKey].Item2)
																	 , newCost.E6_OH_Creditor
																	 , newCost.E6_RX_NKCurrency
																	 , exTaxAmount);

					var appMethod = appSelectInfo?.ApportionMethod ?? ZString.Empty;
					if (appMethod.IsEmpty || appMethod == AllocationMethod.Manual)
					{
						appMethod = AllocationMethod.ChargeableUnits;
					}
					newCost.E6_ApportionmentMethod = appMethod;

					if (appSelectInfo != null && GetUseExistingConsolCostTaxRate())
					{
						using (newCost.UpdateE6_A9_VATClassSuspender.GetSuspender())
						{
							var taxRateFromBilling = appSelectInfo.TaxRatePK;
							if (!taxRateFromBilling.IsEmpty)
							{
								newCost.E6_AT_TaxRate = taxRateFromBilling;
								taxRateFromConsolCost = true;
							}
							newCost.E6_A9_VATClass = appSelectInfo.InvTaxMsgPK;
						}
					}
				}
				else
				{
					newCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				}

				if (!taxRateFromConsolCost)
				{
					using (newCost.UpdateE6_A9_VATClassSuspender.GetSuspender())
					{
						newCost.E6_AT_TaxRate = taxRate?.PK ?? ZGuid.Empty;
						newCost.E6_A9_VATClass = taxRate?.AT_A9_DefaultVatClass ?? ZGuid.Empty;
					}
				}
				newCost.E6_OSCostAmount = exTaxAmount;
				newCost.E6_OSGSTAmount_Calc = GetEffectiveTaxAmount(taxRate, chargeCodeWithDistributedAmount.Tax * multiplier);

				var apportionmentChargesToImport = from item in BillingLine.SystemChargeDataByJobAndChargeCode
												   where item.Value.JR_AC == newCost.E6_AC_ChargeCode && item.Value.IsApportioned
												   select item.Value;

				Tracer.TraceInformation(AccountingTraceSourceCodes.CASS, BuildTraceMessage);

				ConsolCostImporter.ImportChargeIntoCosting(newCost, apportionmentChargesToImport, multiplier == -1);

				#region Trace Message
				#region SuppressResourceStringsCheckRegion

				string BuildTraceMessage()
				{
					var prefix = FormattableString.Invariant($"{BillingLine.ConsolID}|{BillingLine.AWBNumber}|{BillingLine.CreditorCode}|{newCost.ChargeCode.AC_Code}");

					int counter = 0;
					var messageBuilder = new ZStringBuilder();
					foreach (var apportionmentChargeToImport in BillingLine.SystemChargeDataByJobAndChargeCode.Select(x => x.Value))
					{
						counter++;
						var isImported = apportionmentChargeToImport.JR_AC == newCost.E6_AC_ChargeCode && apportionmentChargeToImport.IsApportioned;
						messageBuilder.AppendLine(FormattableString.Invariant($"{nameof(apportionmentChargesToImport)}_{counter}: {nameof(apportionmentChargeToImport.JR_AC)}={apportionmentChargeToImport.JR_AC}, {nameof(apportionmentChargeToImport.JR_GB)}={apportionmentChargeToImport.JR_GB}, {nameof(apportionmentChargeToImport.JR_GE)}={apportionmentChargeToImport.JR_GE}, {nameof(apportionmentChargeToImport.JR_IsRevenuePosted)}={apportionmentChargeToImport.JR_IsRevenuePosted}, {nameof(apportionmentChargeToImport.JR_JH)}={apportionmentChargeToImport.JR_JH}, {nameof(apportionmentChargeToImport.JR_OH_SellAccount)}={apportionmentChargeToImport.JR_OH_SellAccount}, {nameof(apportionmentChargeToImport.JR_OSCostAmt)}={apportionmentChargeToImport.JR_OSCostAmt}, {nameof(apportionmentChargeToImport.JR_OSCostExRate)}={apportionmentChargeToImport.JR_OSCostExRate}, {nameof(apportionmentChargeToImport.JR_RX_NKCostCurrency)}={apportionmentChargeToImport.JR_RX_NKCostCurrency}, {nameof(isImported)}={isImported.ToYesNoString()}"));
					}

					return FormattableString.Invariant($"{prefix}: Accrual in database\r\n{messageBuilder}");
				}

				#endregion
				#endregion
			}

			List<ConsolCostSelector.ApportionMethodSelectionInfo> ExtractApportionMethodSelectionInfo(string apportionMethodSelectionInfoText)
			{
				var info = new List<ConsolCostSelector.ApportionMethodSelectionInfo>();

				if (!string.IsNullOrEmpty(apportionMethodSelectionInfoText))
				{
					var lines = apportionMethodSelectionInfoText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string line in lines)
					{
						var elements = line.Trim().Split(new[] { "|" }, StringSplitOptions.None);
						info.Add(new ConsolCostSelector.ApportionMethodSelectionInfo()
						{
							CreditorPK = new ZGuid(elements[0]),
							Currency = new ZString(elements[1]),
							Amount = new ZDecimal(elements[2]),
							ApportionMethod = elements[3],
							TaxRatePK = new ZGuid(elements[4]),
							InvTaxMsgPK = new ZGuid(elements[5])
						});
					}
				}

				return info;
			}

			readonly ITracer Tracer;

			IConsolCostImporter ConsolCostImporter => consolCostImporter ??= ObjectFactory.Get<IConsolCostImporter>();
			IConsolCostImporter consolCostImporter;
		}

		internal class InvoiceLineCreatorForJob : InvoiceLineCreator<InvoicingLineBase>
		{
			public InvoiceLineCreatorForJob(InvoicingBase invoice, CASSBillingLine billingLine, AccTaxRate gstTaxRate, AccTaxRate freeGSTTaxRate, CostCorrector<InvoicingLineBase> getCostCorrector)
				: base(invoice, billingLine, gstTaxRate, freeGSTTaxRate, getCostCorrector)
			{
			}

			protected override void AddLineToInvoice(CASSCostDistributor.InvoiceLineInfo lineInfo)
			{
				int multiplier = Invoice is CreditNote ? -1 : 1;
				var chargeCodeWithDistributedAmounts = lineInfo.DistributionInfo;
				var costCorrector = GetNewCostCorrector();

				foreach (CASSCostDistributor.CASSCostDistributorInfo chargeCodeWithDistributedAmount in chargeCodeWithDistributedAmounts)
				{
					InvoicingLineBase invoiceLine = (InvoicingLineBase)Invoice.Lines.AddNew();
					invoiceLine.AL_JH = BillingLine.GatewayBillingJob != null ? BillingLine.GatewayBillingJob.PK : ZGuid.Empty;
					invoiceLine.GenericCharge = chargeCodeWithDistributedAmount.ChargeCodePK;

					Tuple<ZGuid, ZGuid> key = new Tuple<ZGuid, ZGuid>(invoiceLine.AL_JH, chargeCodeWithDistributedAmount.ChargeCodePK);
					if (BillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(key))
					{
						invoiceLine.AL_GB = BillingLine.SystemChargeDataByJobAndChargeCode[key].JR_GB;
						invoiceLine.AL_GE = BillingLine.SystemChargeDataByJobAndChargeCode[key].JR_GE;
					}

					invoiceLine.AL_AT = lineInfo.TaxRate?.PK ?? ZGuid.Empty;
					invoiceLine.AL_OSExTaxAmount = GetEffectiveExTaxAmount(lineInfo.TaxRate, chargeCodeWithDistributedAmount.Cost * multiplier, chargeCodeWithDistributedAmount.Tax * multiplier);
					invoiceLine.AL_OSTaxAmount = GetEffectiveTaxAmount(lineInfo.TaxRate, chargeCodeWithDistributedAmount.Tax * multiplier);
					costCorrector.MarkForAdjustment(invoiceLine);
				}

				var totalExTaxAmount = GetEffectiveExTaxAmount(lineInfo.TaxRate, lineInfo.ExTaxAmount * multiplier, lineInfo.TaxAmount * multiplier);
				var totalTaxAmount = GetEffectiveTaxAmount(lineInfo.TaxRate, lineInfo.TaxAmount * multiplier);
				costCorrector.AdjustValuesToSumCorrectlyAfterApportionment(totalExTaxAmount, totalTaxAmount);
			}

			protected override bool IsCostAndTaxDistributionRequired
			{
				get { return true; }
			}
		}

		#endregion

		internal class TotalCostCorrector<T> where T : BusinessObject
		{
			internal static TotalCostCorrector<JobConsolCost> CreateForConsolCosts(ZString invoiceNumber, IList costsToAdd, Dictionary<ZString, List<UnpostedConsolCostIdentificationKey>> consolCostsToDelete)
			{
				return new TotalCostCorrector<JobConsolCost>(
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_OSCostAmountInfo,
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_OSGSTAmount_CalcInfo,
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_LocalCostAmountInfo,
					cst =>
					{
						costsToAdd.Remove(cst);
						if (!consolCostsToDelete.ContainsKey(invoiceNumber))
						{ consolCostsToDelete.Add(invoiceNumber, new List<UnpostedConsolCostIdentificationKey>()); }
						consolCostsToDelete[invoiceNumber].Add(new UnpostedConsolCostIdentificationKey(cst.E6_ParentID, cst.E6_AC_ChargeCode));
					});
			}

			internal static TotalCostCorrector<InvoicingLineBase> CreateForInvoiceLines(InvoicingLineBaseCollection lineCollection)
			{
				return new TotalCostCorrector<InvoicingLineBase>(
					line => (ZPropertyInfo<ZDecimal>)line.AL_OSExTaxAmountInfo,
					line => (ZPropertyInfo<ZDecimal>)line.AL_OSTaxAmountInfo,
					line => (ZPropertyInfo<ZDecimal>)line.AL_LocalExTaxAmountInfo,
					line => lineCollection.Remove(line));
			}

			readonly Func<T, ZPropertyInfo<ZDecimal>> getOSCostAmountProperty;
			readonly Func<T, ZPropertyInfo<ZDecimal>> getOSTaxAmountProperty;
			readonly Func<T, ZPropertyInfo<ZDecimal>> getLocalCostAmountProperty;
			readonly Action<T> zeroValueAction;
			readonly List<T> itemsForCorrection;

			public TotalCostCorrector(
				Func<T, ZPropertyInfo<ZDecimal>> getOSCostAmountProperty,
				Func<T, ZPropertyInfo<ZDecimal>> getOSTaxAmountProperty,
				Func<T, ZPropertyInfo<ZDecimal>> getLocalCostAmountProperty,
				Action<T> zeroValueAction)
			{
				this.getOSCostAmountProperty = getOSCostAmountProperty;
				this.getOSTaxAmountProperty = getOSTaxAmountProperty;
				this.getLocalCostAmountProperty = getLocalCostAmountProperty;
				this.zeroValueAction = zeroValueAction;
				this.itemsForCorrection = new List<T>();
			}

			public void MarkForAdjustment(T item)
			{
				itemsForCorrection.Add(item);
			}

			public void AdjustValuesToSumCorrectlyAfterApportionment(ZDecimal originalTotalCostAmount, ZDecimal originalTotalTaxAmount)
			{
				ZDecimal totalCostAmount = 0M;
				ZDecimal totalTaxAmount = 0M;
				ZDecimal maxAbsCostAmount = 0M;
				ZPropertyInfo<ZDecimal> maxAmountProperty = null;
				ZPropertyInfo<ZDecimal> maxTaxAmountProperty = null;

				if (itemsForCorrection.Count == 0)
				{
					throw new InvalidOperationException("You need to mark items for correction before calling this method");
				}

				ZeroOffItemsThatAreZeroInLocalCurrency();

				totalCostAmount = totalTaxAmount = maxAbsCostAmount = 0M;
				maxAmountProperty = maxTaxAmountProperty = null;

				foreach (T item in itemsForCorrection)
				{
					totalCostAmount += getOSCostAmountProperty(item).Value;
					totalTaxAmount += getOSTaxAmountProperty(item).Value;

					ZDecimal absCostAmount = Math.Abs(getOSCostAmountProperty(item).Value);

					if (absCostAmount > maxAbsCostAmount || maxAmountProperty == null)
					{
						maxAbsCostAmount = absCostAmount;
						maxAmountProperty = getOSCostAmountProperty(item);
						maxTaxAmountProperty = getOSTaxAmountProperty(item);
					}
				}

				ZDecimal costAmountDifference = originalTotalCostAmount - totalCostAmount;
				ZDecimal taxAmountDifference = originalTotalTaxAmount - totalTaxAmount;

				if (costAmountDifference != 0)
				{
					decimal maxTaxAmountOriginal = maxTaxAmountProperty.Value;
					maxAmountProperty.Value += costAmountDifference;
					maxTaxAmountProperty.Value = maxTaxAmountOriginal; // This reset is needed because updating cost amount could have triggered an update to tax value
				}

				if (taxAmountDifference != 0)
				{
					maxTaxAmountProperty.Value += taxAmountDifference;
				}

				ZeroOffItemsThatAreZeroInLocalCurrency();
				ProcessZeroValues();
			}

			void ProcessZeroValues()
			{
				foreach (T item in itemsForCorrection)
				{
					if (getOSCostAmountProperty(item).Value == 0M)
					{
						zeroValueAction(item);
					}
				}
			}

			void ZeroOffItemsThatAreZeroInLocalCurrency()
			{
				foreach (T item in itemsForCorrection)
				{
					if (getLocalCostAmountProperty(item).Value == 0M)
					{
						getOSCostAmountProperty(item).Value = 0M;
					}
				}
			}
		}

		#region Tax Rates

		AccTaxRate GetRate(ZGuid taxPK)
		{
			ZQuery taxFilter = new ZQuery(AccTaxRateSchema.PK, taxPK);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			return APTransactionsFactory.LoadTop1<AccTaxRate>(taxFilter);
		}

		AccTaxRate GSTTaxRate
		{
			get { return gstTaxRate ?? (gstTaxRate = GetRate(AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.StandardRatedTaxID)); }
		}
		AccTaxRate gstTaxRate;

		void ResetGSTTaxRate() => gstTaxRate = null;

		AccTaxRate FREEGSTTaxRate
		{
			get { return freeGSTTaxRate ?? (freeGSTTaxRate = GetRate(AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID)); }
		}
		AccTaxRate freeGSTTaxRate;

		void ResetFREEGSTTaxRate() => freeGSTTaxRate = null;

		#endregion

		#endregion

		#region Print Document

		public void PrintDiscrepanciesDocument()
		{
			AccPrintingUtility utility = new AccPrintingUtility(Factory, Constants.DataContext.CASSBilling);
			utility.PrintDocument(this, (NoResString)"CASS Discrepancies Report", AllowedDeliveryOptions.All, ZGuid.Empty, true);
		}

		#endregion

		#region Properties

		public static bool IsRejectedClaimLinesExpected
		{
			get
			{
				return GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.UnitedStates;
			}
		}

		[XmlIgnore]
		public ZBool ClaimExistsAndNeedsToBeClosed => Lines.Any(x => ((CASSBillingLine)x).ClaimCreator.ClaimExistsAndNeedsToBeClosed);

		[XmlIgnore]
		[BusinessObjectTestExclude]
		public ZBool OverrideAutoClosureOfCASSBillingClaim
		{
			get { return overrideAutoClosureOfCASSBillingClaim; }
			set { SetNonPersistentPropertyValue(OverrideAutoClosureOfCASSBillingClaimInfo, ref overrideAutoClosureOfCASSBillingClaim, value); }
		}
		ZBool overrideAutoClosureOfCASSBillingClaim;

		public ZPropertyInfo OverrideAutoClosureOfCASSBillingClaimInfo => GetZPropertyInfo(nameof(OverrideAutoClosureOfCASSBillingClaim));

		[XmlIgnore]
		public bool IsImportBilling
		{
			get { return CostHeader.Lines is CASSCostImportLineCollection; }
		}

		[XmlIgnore]
		public bool IsExportBilling
		{
			get { return CostHeader.Lines is CASSCostExportLineCollection; }
		}

		[XmlIgnore]
		[ReadOnly(true)]
		public ZString HOTFileName { get { return CostHeader.HOTFileName; } }

		public ZPropertyInfo HOTFileNameInfo => GetZPropertyInfo(nameof(HOTFileName));

		[XmlIgnore]
		[ReadOnly(true)]
		public ZDateTime BillingPeriodStart { get { return CostHeader.DatePeriodStart; } }

		public ZPropertyInfo BillingPeriodStartInfo => GetZPropertyInfo(nameof(BillingPeriodStart));

		[XmlIgnore]
		[ReadOnly(true)]
		public ZDateTime BillingPeriodEnd { get { return CostHeader.DatePeriodEnd; } }

		public ZPropertyInfo BillingPeriodEndInfo => GetZPropertyInfo(nameof(BillingPeriodEnd));

		[XmlIgnore]
		[ReadOnly(true)]
		public ZDateTime BillingDate { get { return CostHeader.DateOfBilling; } }

		public ZPropertyInfo BillingDateInfo => GetZPropertyInfo(nameof(BillingDate));

		[XmlIgnore]
		public CASSBillingLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new CASSBillingLineCollection(Factory);
				}
				return fLines;
			}
		}
		CASSBillingLineCollection fLines;

		[XmlIgnore]
		public CASSBillingLineCollection HiddenLines
		{
			get
			{
				if (fHiddenLines == null)
				{
					fHiddenLines = new CASSBillingLineCollection(Factory);
				}
				return fHiddenLines;
			}
		}
		CASSBillingLineCollection fHiddenLines;

		public void RemoveAllLines()
		{
			Lines.RemoveAndDeleteAll();
			HiddenLines.RemoveAndDeleteAll();
		}

		#region Cost Totals

		[XmlIgnore]
		public ZDecimal TotalCASSCostValue
		{
			get
			{
				if (fTotalCASSCostValue == null)
				{
					fTotalCASSCostValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalCASSCostValue += line.CASSCostValueInLocalCurrencyForDisplay;
					}
				}
				return fTotalCASSCostValue.Value;
			}
		}
		ZDecimal? fTotalCASSCostValue;

		public ZPropertyInfo TotalCASSCostValueInfo => GetZPropertyInfo(nameof(TotalCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalCASSRejectedClaimValue
		{
			get
			{
				if (fTotalCASSRejectedClaimValue == null)
				{
					fTotalCASSRejectedClaimValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalCASSRejectedClaimValue += line.CASSRejectedClaimValueInLocalCurrencyForDisplay;
					}
				}
				return fTotalCASSRejectedClaimValue.Value;
			}
		}
		ZDecimal? fTotalCASSRejectedClaimValue;

		public ZPropertyInfo TotalCASSRejectedClaimValueInfo => GetZPropertyInfo(nameof(TotalCASSRejectedClaimValue));

		[XmlIgnore]
		public ZDecimal TotalSystemCostAccrualValue
		{
			get
			{
				if (fTotalSystemCostAccrualValue == null)
				{
					fTotalSystemCostAccrualValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalSystemCostAccrualValue += line.SystemCostAccrualValue;
					}
				}
				return fTotalSystemCostAccrualValue.Value;
			}
		}
		ZDecimal? fTotalSystemCostAccrualValue;

		public ZPropertyInfo TotalSystemCostAccrualValueInfo => GetZPropertyInfo(nameof(TotalSystemCostAccrualValue));

		[XmlIgnore]
		public ZDecimal TotalCostDifferenceValue
		{
			get
			{
				if (fTotalCostDifferenceValue == null)
				{
					fTotalCostDifferenceValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalCostDifferenceValue += line.CostDifference;
					}
				}
				return fTotalCostDifferenceValue.Value;
			}
		}
		ZDecimal? fTotalCostDifferenceValue;

		public ZPropertyInfo TotalCostDifferenceValueInfo => GetZPropertyInfo(nameof(TotalCostDifferenceValue));

		[XmlIgnore]
		public ZDecimal TotalNetCASSCostValue
		{
			get
			{
				if (fTotalNetCASSCostValue == null)
				{
					fTotalNetCASSCostValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalNetCASSCostValue += line.NetCASSCost;
					}
				}
				return fTotalNetCASSCostValue.Value;
			}
		}
		ZDecimal? fTotalNetCASSCostValue;

		public ZPropertyInfo TotalNetCASSCostValueInfo => GetZPropertyInfo(nameof(TotalNetCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalCASSCostAdjustedValue
		{
			get
			{
				if (fTotalCASSCostAdjustedValue == null)
				{
					fTotalCASSCostAdjustedValue = 0;
					foreach (CASSBillingLine line in Lines)
					{
						fTotalCASSCostAdjustedValue += line.CASSCostAdjustedValue;
					}
				}
				return fTotalCASSCostAdjustedValue.Value;
			}
		}
		ZDecimal? fTotalCASSCostAdjustedValue;

		public ZPropertyInfo TotalCASSCostAdjustedValueInfo => GetZPropertyInfo(nameof(TotalCASSCostAdjustedValue));

		#endregion

		#region Hidden Cost Totals

		[XmlIgnore]
		public ZDecimal TotalHiddenCASSCostValue
		{
			get
			{
				if (fTotalHiddenCASSCostValue == null)
				{
					fTotalHiddenCASSCostValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenCASSCostValue += line.CASSCostValueInLocalCurrencyForDisplay;
					}
				}
				return fTotalHiddenCASSCostValue.Value;
			}
		}
		ZDecimal? fTotalHiddenCASSCostValue;

		public ZPropertyInfo TotalHiddenCASSCostValueInfo => GetZPropertyInfo(nameof(TotalHiddenCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalHiddenCASSRejectedClaimValue
		{
			get
			{
				if (fTotalHiddenCASSRejectedClaimValue == null)
				{
					fTotalHiddenCASSRejectedClaimValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenCASSRejectedClaimValue += line.CASSRejectedClaimValueInLocalCurrencyForDisplay;
					}
				}
				return fTotalHiddenCASSRejectedClaimValue.Value;
			}
		}
		ZDecimal? fTotalHiddenCASSRejectedClaimValue;

		public ZPropertyInfo TotalHiddenCASSRejectedClaimValueInfo => GetZPropertyInfo(nameof(TotalHiddenCASSRejectedClaimValue));

		[XmlIgnore]
		public ZDecimal TotalHiddenSystemCostAccrualValue
		{
			get
			{
				if (fTotalHiddenSystemCostAccrualValue == null)
				{
					fTotalHiddenSystemCostAccrualValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenSystemCostAccrualValue += line.SystemCostAccrualValue;
					}
				}
				return fTotalHiddenSystemCostAccrualValue.Value;
			}
		}
		ZDecimal? fTotalHiddenSystemCostAccrualValue;

		public ZPropertyInfo TotalHiddenSystemCostAccrualValueInfo => GetZPropertyInfo(nameof(TotalHiddenSystemCostAccrualValue));

		[XmlIgnore]
		public ZDecimal TotalHiddenCostDifferenceValue
		{
			get
			{
				if (fTotalHiddenCostDifferenceValue == null)
				{
					fTotalHiddenCostDifferenceValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenCostDifferenceValue += line.CostDifference;
					}
				}
				return fTotalHiddenCostDifferenceValue.Value;
			}
		}
		ZDecimal? fTotalHiddenCostDifferenceValue;

		public ZPropertyInfo TotalHiddenCostDifferenceValueInfo => GetZPropertyInfo(nameof(TotalHiddenCostDifferenceValue));

		[XmlIgnore]
		public ZDecimal TotalHiddenNetCASSCostValue
		{
			get
			{
				if (fTotalHiddenNetCASSCostValue == null)
				{
					fTotalHiddenNetCASSCostValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenNetCASSCostValue += line.NetCASSCost;
					}
				}
				return fTotalHiddenNetCASSCostValue.Value;
			}
		}
		ZDecimal? fTotalHiddenNetCASSCostValue;

		public ZPropertyInfo TotalHiddenNetCASSCostValueInfo => GetZPropertyInfo(nameof(TotalHiddenNetCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalHiddenCASSCostAdjustedValue
		{
			get
			{
				if (fTotalHiddenCASSCostAdjustedValue == null)
				{
					fTotalHiddenCASSCostAdjustedValue = 0;
					foreach (CASSBillingLine line in HiddenLines)
					{
						fTotalHiddenCASSCostAdjustedValue += line.CASSCostAdjustedValue;
					}
				}
				return fTotalHiddenCASSCostAdjustedValue.Value;
			}
		}
		ZDecimal? fTotalHiddenCASSCostAdjustedValue;

		public ZPropertyInfo TotalHiddenCASSCostAdjustedValueInfo => GetZPropertyInfo(nameof(TotalHiddenCASSCostAdjustedValue));

		#endregion

		#region All Cost Totals

		[XmlIgnore]
		public ZDecimal TotalAllCASSCostValue
		{
			get { return TotalCASSCostValue + TotalHiddenCASSCostValue; }
		}

		public ZPropertyInfo TotalAllCASSCostValueInfo => GetZPropertyInfo(nameof(TotalAllCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalAllCASSRejectedClaimValue
		{
			get { return TotalCASSRejectedClaimValue + TotalHiddenCASSRejectedClaimValue; }
		}

		public ZPropertyInfo TotalAllCASSRejectedClaimValueInfo => GetZPropertyInfo(nameof(TotalAllCASSRejectedClaimValue));

		[XmlIgnore]
		public ZDecimal TotalAllSystemCostAccrualValue
		{
			get { return TotalSystemCostAccrualValue + TotalHiddenSystemCostAccrualValue; }
		}

		public ZPropertyInfo TotalAllSystemCostAccrualValueInfo => GetZPropertyInfo(nameof(TotalAllSystemCostAccrualValue));

		[XmlIgnore]
		public ZDecimal TotalAllCostDifferenceValue
		{
			get { return TotalCostDifferenceValue + TotalHiddenCostDifferenceValue; }
		}

		public ZPropertyInfo TotalAllCostDifferenceValueInfo => GetZPropertyInfo(nameof(TotalAllCostDifferenceValue));

		[XmlIgnore]
		public ZDecimal TotalAllNetCASSCostValue
		{
			get { return TotalNetCASSCostValue + TotalHiddenNetCASSCostValue; }
		}

		public ZPropertyInfo TotalAllNetCASSCostValueInfo => GetZPropertyInfo(nameof(TotalAllNetCASSCostValue));

		[XmlIgnore]
		public ZDecimal TotalAllCASSCostAdjustedValue
		{
			get { return TotalCASSCostAdjustedValue + TotalHiddenCASSCostAdjustedValue; }
		}

		public ZPropertyInfo TotalAllCASSCostAdjustedValueInfo => GetZPropertyInfo(nameof(TotalAllCASSCostAdjustedValue));

		#endregion

		[XmlIgnore]
		public InvoicingBaseCollection APTransactions
		{
			get
			{
				if (apTransactions == null)
				{
					apTransactions = new InvoicingBaseCollection(APTransactionsFactory);
					apTransactions.SetAllowNew(false);
				}

				return apTransactions;
			}
		}
		InvoicingBaseCollection apTransactions;

		BusinessObjectFactory APTransactionsFactory => apTransactionsFactory ?? (apTransactionsFactory = CreateNewFactory());

		BusinessObjectFactory apTransactionsFactory;

		readonly Dictionary<ZGuid, string> SerializedTransactions = new Dictionary<ZGuid, string>();

		Dictionary<InvoiceKey, bool> IsPostedToCASSDictionary
		{
			get
			{
				if (fIsPostedToCASSDictionary == null)
				{
					fIsPostedToCASSDictionary = new Dictionary<InvoiceKey, bool>();
				}
				return fIsPostedToCASSDictionary;
			}
		}

		Dictionary<InvoiceKey, bool> fIsPostedToCASSDictionary;

		internal class UnpostedConsolCostIdentificationKey
		{
			internal UnpostedConsolCostIdentificationKey(ZGuid consolId, ZGuid chargeCodeId)
			{
				ConsolId = consolId;
				ChargeCodeId = chargeCodeId;
			}

			public ZGuid ConsolId { get; }
			public ZGuid ChargeCodeId { get; }
		}

		Dictionary<ZString, List<UnpostedConsolCostIdentificationKey>> UnpostedConsolCostsToDeleteIndexedByInvoice
		{
			get
			{
				if (consolCostsToDeleteIfUnposted == null)
				{
					consolCostsToDeleteIfUnposted = new Dictionary<ZString, List<UnpostedConsolCostIdentificationKey>>();
				}
				return consolCostsToDeleteIfUnposted;
			}
		}

		Dictionary<ZString, List<UnpostedConsolCostIdentificationKey>> consolCostsToDeleteIfUnposted;

		#region CASSCostHeader

		[XmlIgnore]
		public CASSCostHeader CostHeader
		{
			get
			{
				if (costHeader == null)
				{
					SetCostHeader(new CASSCostHeader());
				}
				return costHeader;
			}
		}
		void SetCostHeader(CASSCostHeader value)
		{
			ImportLines.RemoveAll();
			ExportLines.RemoveAll();
			costHeader?.Delete();

			costHeader = value;
			ImportLines.AddRange(costHeader.ImportLines);
			ExportLines.AddRange(costHeader.ExportLines);
			RaiseCASSCostHeaderChanged(costHeader);

			HOTFileNameInfo.RefreshBinding();
			BillingPeriodStartInfo.RefreshBinding();
			BillingPeriodEndInfo.RefreshBinding();
			BillingDateInfo.RefreshBinding();
		}
		CASSCostHeader costHeader;

		[XmlIgnore]
		[ChildEditable(true)]
		public CASSCostExportLineCollection ExportLines
		{
			get
			{
				if (exportLines == null)
				{
					exportLines = new CASSCostExportLineCollection();
				}
				return exportLines;
			}
		}
		CASSCostExportLineCollection exportLines;

		[XmlIgnore]
		[ChildEditable(true)]
		public CASSCostImportLineCollection ImportLines
		{
			get
			{
				if (importLines == null)
				{
					importLines = new CASSCostImportLineCollection();
				}
				return importLines;
			}
		}
		CASSCostImportLineCollection importLines;

		[XmlIgnore]
		public bool NeedToBeRefreshed
		{
			get
			{
				return (ImportLines.HasChanges || ExportLines.HasChanges);
			}
		}

		public event EventHandler OnCASSCostHeaderChanged;

		void RaiseCASSCostHeaderChanged(object sender)
		{
			if (OnCASSCostHeaderChanged != null)
			{
				OnCASSCostHeaderChanged(sender, null);
			}
		}

		#endregion

		[XmlIgnore]
		public bool AllowCASSCostAdjustment
		{
			get
			{
				return (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Canada);
			}
		}

		[XmlIgnore]
		public bool HasAnyCASSCostAdjustmentBeenMade
		{
			get
			{
				return CostHeader.Lines != null && CostHeader.Lines.Cast<CASSCostLine>().Any(x => x.HasAnyAmountChanged());
			}
		}

		bool HasInvoiceNumberChanges
		{
			get { return Lines.Any(x => ((CASSBillingLine)x).IsInvoiceNumberChanged); }
		}

		public bool HasCriticalErrorsForInvoiceCreation
		{
			get
			{
				bool result = false;
				if (ShouldInvoicesBeCreated)
				{
					ValidateCriticalPropertiesForInvoiceAndClaimGeneration();
					result = Lines.Any(x => !((CASSBillingLine)x).CriticalErrors.IsNullOrEmpty());
				}
				return result;
			}
		}

		public bool NeedToRecalculateInvoices => APTransactions.Count > 0 && HasInvoiceNumberChanges;

		public bool HasCriticalErrorsForInvoicePosting => ShouldInvoicesBeCreatedForPosting && HasCriticalErrorsForInvoiceCreation;

		bool ShouldInvoicesBeCreated => APTransactions.Count == 0 || NeedToRecalculateInvoices;

		bool ShouldInvoicesBeCreatedForPosting => APTransactions.Count == 0;

		Dictionary<ZGuid, IEnumerable<CASSBillingLine>> LinesWithInvoicePK => linesWithInvoicePK ?? (linesWithInvoicePK = new Dictionary<ZGuid, IEnumerable<CASSBillingLine>>());
		Dictionary<ZGuid, IEnumerable<CASSBillingLine>> linesWithInvoicePK;

		#endregion

		#region IDocumentSupportable Members

		[XmlIgnore]
		public DocumentSupporter DocumentSupporter
		{
			get { return new CASSBillingDocumentSupporter(this); }
		}

		#endregion

		public static string CASSGstNotSetErrorMessage()
		{
			return Res.GetString("1ba46cbb-251a-432e-966f-42c838162bf3", "CASS file Import Default Tax ID is not set, CASS import cannot be processed at the moment.\r\nPlease contact support for assistance with this.");
		}
	}

	public class CASSBillingDocumentSupporter : DocumentSupporter
	{
		public CASSBillingDocumentSupporter(CASSBilling cASSBilling)
			: base(cASSBilling)
		{
		}

		protected CASSBilling CASSBilling
		{
			get { return (CASSBilling)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override CargoWise.Definitions.BusinessContext BusinessContext
		{
			get { return CargoWise.Definitions.BusinessContext.CASSBilling; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.CASSBilling };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CASSBilling, CASSBilling) };
		}

		#endregion
	}
}

//CASSBillingTest class has been moved to Accounting.Business.Testing project.
