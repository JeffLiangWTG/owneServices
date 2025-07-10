using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.DeclarationStatusUpdater;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.FR.Business.FRConstants;
using Shared = Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class BaseDeltaGResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		protected BaseDeltaGResponseMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override void ProcessMessageCore(EDIMessage message)
		{
			incomingMessage = message as FREDIMessage;

			if (incomingMessage != null && incomingMessage.MessageDataObject is IResponseDataProvider dataProvider)
			{
				var entryHeader = EntryActionHelper.GetEntryHeaderFromMessage(message.Factory, dataProvider, message.GetCountryCodeSafe());
				if (entryHeader != null)
				{
					outgoingMessage = EntryActionHelper.GetOutgoingMessage(entryHeader, incomingMessage);
					entryHeader.Messages.Add(incomingMessage);
					UpdateOrAddEntryNumberIfNeeded(entryHeader, dataProvider);
					UpdateStatus(entryHeader, dataProvider, out var updatedToBAEForTheFirstTime);
					DoExtraProcessing(dataProvider, entryHeader, message, updatedToBAEForTheFirstTime);
				}
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			}
		}

		void UpdatePendingTransaction(CusEntryHeader entryHeader, IResponseDataProvider dataProvider, Action<Shared.SharedCusPermitLineTransaction> updateTransactionAction = null)
		{
			var responseStatus = GetResponseStatus(entryHeader, dataProvider);
			if (EntryActionHelper.TransactionMustBeDeleted(responseStatus, entryHeader, incomingMessage))
			{
				Shared.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, FRPermitHelper.GetPermitAppIdForMessage, entryHeader.CountryCode, false, Shared.PermitTransactionStatusList.Codes.Deleted, updateTransactionAction);
			}
			else if (EntryActionHelper.TransactionMustBeConfirmed(responseStatus, entryHeader, incomingMessage))
			{
				Shared.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, FRPermitHelper.GetPermitAppIdForMessage, entryHeader.CountryCode, false, Shared.PermitTransactionStatusList.Codes.Confirmed, updateTransactionAction);
			}
		}

		void DoExtraProcessing(IResponseDataProvider dataProvider, CusEntryHeader cusEntryHeader, EDIMessage message, ZBool updatedToBAEForTheFirstTime)
		{
			DoExtraProcessingCore(dataProvider, cusEntryHeader);
			UpdatePendingTransaction(cusEntryHeader, dataProvider);
			UpdateFees(cusEntryHeader, dataProvider);
			UpdateConfirmedGuaranteeAmount(cusEntryHeader, dataProvider);
			UpdateBondedWarehouseIfApplicable(cusEntryHeader);
			AddPermitsIfApplicable(cusEntryHeader, message);
			UpdateTemporaryStorageIfApplicable(cusEntryHeader, updatedToBAEForTheFirstTime);
			UpdateAdHocAuthorizationsIfNecessary(cusEntryHeader, updatedToBAEForTheFirstTime);
			ManageEntrySnapshots(cusEntryHeader, message);
			ManageAmendmentEntrySnapshots(cusEntryHeader);
			TriggerT2LDocumentAndAttachToEDocsIfApplicable(cusEntryHeader, updatedToBAEForTheFirstTime);
		}

		void TriggerT2LDocumentAndAttachToEDocsIfApplicable(CusEntryHeader cusEntryHeader, ZBool updatedToBAEForTheFirstTime)
		{
			if (IsT2LApplicableForEntryHeader())
			{
				new T2LEDocsSaver(cusEntryHeader).RenderDocumentAndSaveInEDocs();
				Logger.Log(FormattableString.Invariant($"T2L Document has been generated and attached to the entry with BGMReference {cusEntryHeader.CH_BGMReference}."));
			}
			else if (IsT2LFApplicableForEntryHeader())
			{
				new T2LFEDocsSaver(cusEntryHeader).RenderDocumentAndSaveInEDocs();
				Logger.Log(FormattableString.Invariant($"T2LF Document has been generated and attached to the entry with BGMReference {cusEntryHeader.CH_BGMReference}."));
			}

			bool IsT2LApplicableForEntryHeader()
			{
				return updatedToBAEForTheFirstTime && cusEntryHeader.T2LApplicableEntryLines.Any();
			}

			bool IsT2LFApplicableForEntryHeader()
			{
				return updatedToBAEForTheFirstTime && cusEntryHeader.T2LFApplicableEntryLines.Any();
			}
		}

		void UpdateAdHocAuthorizationsIfNecessary(CusEntryHeader cusEntryHeader, ZBool updatedToBAEForTheFirstTime)
		{
			if (updatedToBAEForTheFirstTime && cusEntryHeader.EntryInstruction is CusEntryInstruction instruction)
			{
				foreach (CusAuthorizationUsage usage in instruction.CusAuthorizationUsages)
				{
					if (usage.AuthorisationHeader is Shared.CusAuthorisationHeader header && header.CPH_IsAdHoc)
					{
						header.CPH_IsActive = false;
					}
				}
			}
		}

		void ManageEntrySnapshots(CusEntryHeader entryHeader, EDIMessage message)
		{
			if (entryHeader.Declaration.IsDeltaD)
			{
				if (DeltaGMessageSender.ApplicableEntryActionCodeForSnapshot.Contains(outgoingMessage.EM_MessageSubType))
				{
					if (EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed((FREDIMessage)message))
					{
						foreach (var snapshot in entryHeader.Snapshots)
						{
							if (snapshot.CES_Status == Shared.AccumulativeAmendment.EntrySnapshotStatus.Current && DeltaGMessageSender.ApplicableEntryActionCodeForSnapshot.Contains(snapshot.CES_MessageType))
							{
								snapshot.CES_Status = Shared.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
							}
						}
					}
					else if (EntryActionHelper.HasError((FREDIMessage)message))
					{
						var snapshotsToRemove = new List<Shared.CusEntrySnapshot>();
						foreach (var snapshot in entryHeader.Snapshots)
						{
							if (snapshot.CES_Status == Shared.AccumulativeAmendment.EntrySnapshotStatus.Current && DeltaGMessageSender.ApplicableEntryActionCodeForSnapshot.Contains(snapshot.CES_MessageType))
							{
								snapshotsToRemove.Add(snapshot);
							}
						}

						foreach (var snapshot in snapshotsToRemove)
						{
							entryHeader.Snapshots.RemoveAndDelete(snapshot);
						}
					}
				}
				else if (EntryActionHelper.EntrySnapshotMustBeUpdated((FREDIMessage)message, entryHeader.CH_Status))
				{
					var snapshotsToUpdate = entryHeader.Snapshots.Cast<Shared.CusEntrySnapshot>().Where(x => x.CES_Status == Shared.AccumulativeAmendment.EntrySnapshotStatus.Lodged && DeltaGMessageSender.ApplicableEntryActionCodeForSnapshot.Contains(x.CES_MessageType));
					foreach (var snapshot in snapshotsToUpdate)
					{
						snapshot.CES_SnapshotXml = entryHeader.GetEntrySnapshotData();
					}
				}
			}
		}

		void ManageAmendmentEntrySnapshots(CusEntryHeader entryHeader)
		{
			var snapShotManager = entryHeader.GetNewAmendmentSnapshotManager();
			var resolver = new DeltaGStatusResolver(entryHeader, incomingMessage);
			if (resolver.CheckIsOriginalClear())
			{
				snapShotManager.CreateNewAndAccept();
			}
			else if (resolver.CheckIsRectificationClear())
			{
				snapShotManager.DeleteLatestLodged();
				snapShotManager.CreateNewAndAccept();
			}
		}

		void AddPermitsIfApplicable(CusEntryHeader entryHeader, EDIMessage message)
		{
			if (entryHeader.HasReleasingGuaranteeProcedure && entryHeader.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES100)
			{
				var guarantee = entryHeader.Declaration.CustomsGuarantee;
				if (guarantee != null)
				{
					var numberReleased = (entryHeader as Shared.IGuaranteeJobParent).PackageCount;
					var liabilityOriginal = entryHeader.ComplementaryJob?.Parent?.AmountToBeGuaranteedInDeclarationCurrency ?? ZDecimal.Zero;
					var numberOriginal = entryHeader.ComplementaryJob?.Parent?.PackageCount ?? ZInt.Zero;

					if (!numberReleased.IsEmpty && !liabilityOriginal.IsEmpty && !numberOriginal.IsEmpty)
					{
						try
						{
							if (guarantee.Mutex.Lock())
							{
								var permitRecord = new Shared.PermitRecord
								{
									PermitHeader = guarantee,
									Value = (ZDecimal)numberReleased * liabilityOriginal / (ZDecimal)numberOriginal,
									Quantity = 0
								};
								guarantee.AddTransaction(Shared.PermitHelper.GetPermitReferenceForEntry(entryHeader), Shared.PermitHelper.GetPermitComment(entryHeader, permitRecord), FRPermitHelper.GetPermitAppIdForMessage(message), permitRecord.Procedure, permitRecord.Value, permitRecord.Quantity, Shared.PermitTransactionStatusList.Codes.Confirmed, Shared.PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader));
							}
						}
						finally
						{
							var mutex = guarantee.Mutex;
							if (mutex.HasLock)
							{
								mutex.Unlock();
							}
						}
					}
				}
			}
		}

		void UpdateBondedWarehouseIfApplicable(CusEntryHeader entry)
		{
			if (entry.SupportsBondedWarehousing)
			{
				var shouldUpdateBondedWarehouse = new DeltaGStatusResolver(entry, incomingMessage).ShouldUpdateBondedWarehouse();
				if (shouldUpdateBondedWarehouse)
				{
					entry.Factory.Saved -= BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
					entry.Factory.Saved += BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				}
			}
		}

		void UpdateTemporaryStorageIfApplicable(CusEntryHeader entryHeader, ZBool updatedToBAEForTheFirstTime)
		{
			var resolver = new DeltaGStatusResolver(entryHeader, incomingMessage);
			var processor = new CusTempStorageRegisterProcessor<EDIMessage>(entryHeader, Logger, notifierWhenInsufficient: Notifier);
			try
			{
				if (updatedToBAEForTheFirstTime || resolver.CheckIsRectificationClear())
				{
					processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
				}
				else if (resolver.CheckHasBeenWithdrawn())
				{
					processor.CalculateTransactionsAndLockMutexIfNeededForRollingBackTransaction();
				}
				processor.AddTransactionsWhenSaving(incomingMessage);
			}
			finally
			{
				processor.UnlockRegistersMutexes();
			}
			void Notifier(ZString failingRegisterReference)
			{
				entryHeader.Logs.AddNew(Events.DeclarationHasErrors, "Warning : Register " + failingRegisterReference + " has not been updated, there are not enough packages remaining", ZDateTimeOffset.UtcNow);
				var message = Res.GetString("A8636F9C-B63B-4F76-B67A-946ED02E5257", "Warning : Register {0} has not been updated for reference {1} , there are not enough packages remaining", failingRegisterReference, entryHeader.EntryNumber);
				Logger.AddWarning(message);
				ProcessorHelper.SendEmail(entryHeader.Factory, entryHeader.CH_SystemCreateUser, true, GetEmailBody(message), GetEmailsubject(entryHeader), GetEmailGroupRegistryItem());
			}
		}

		static ZString GetEmailsubject(CusEntryHeader dataProvider)
		{
			var responseReference = dataProvider.EntryNumber;
			var responseReferenceDec = dataProvider.Declaration?.JobNumber ?? ZString.Empty;
			var readableResponseReference = responseReference != ZString.Empty ? (ZString)Res.GetString("0135E5C9-857C-4A43-90B4-5DA0C90ADD19", " Reference: {0}", responseReference) : ZString.Empty;
			var readableResponseDecReference = responseReferenceDec != ZString.Empty ? (ZString)Res.GetString("1EE49ABE-084D-4F0D-BB83-36F6DB472AA4", " Declaration: {0}", responseReferenceDec) : ZString.Empty;

			return Res.GetString("44715E53-B0CC-476D-AB1E-9D7B897D1731", "New Delta G response received.{0}{1}", readableResponseDecReference, readableResponseReference);
		}

		ZString GetEmailBody(string messageBody)
		{
			return Res.GetString("E6A355CE-5989-49F5-B2B8-6E9F4AAB6FC6", "A Delta G response has been received. {0}.", messageBody);
		}

		Integration.IRegistryItem GetEmailGroupRegistryItem() => FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup;

		protected virtual void DoExtraProcessingCore(IResponseDataProvider dataProvider, CusEntryHeader cusEntryHeader)
		{
			if (CheckIsGvmsAdviceAndStatusES050(cusEntryHeader, dataProvider))
			{
				var declaration = cusEntryHeader.Declaration;
				var goodOrigin = declaration.JE_GoodsOrigin;
				if (goodOrigin == Core.Constants.CountryCodes.UnitedKingdom || goodOrigin == "XU")
				{
					var sendingObject = new DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
					sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAA;
					var errCollector = new EU.Business.ErrorCollector();
					var messageSender = new DeltaGMessageSender(sendingObject, errCollector);
					var result = messageSender.Send(true);

					if (result != DeltaGMessageSender.MessageSendSuccessful)
					{
						Logger.AddWarning($"VAA Message throws an error during the process of Entry {cusEntryHeader.CH_BGMReference} and has not been sent. Errors: {result}");
					}
				}
			}
		}

		protected void SendCINMessageIfApplicable(CusEntryHeader cusEntryHeader, IResponseDataProvider dataProvider)
		{
			if (dataProvider.EtatECS == ExportControlStatusList.Codes.EEC)
			{
				using (DisposableEnvironment.ForBranch(cusEntryHeader.Branch.PK.ToGuid()))
				{
					var declaration = cusEntryHeader.Declaration;

					if (FRCustomsDataRegistry.Instance.CINSenderID.Value != FRCustomsDataRegistry.Instance.CINSenderID.DefaultValue && declaration != null && declaration.IsAir)
					{
						var result = ZString.Empty;
						if (declaration.JE_MasterBill.IsEmpty && !cusEntryHeader.Messages.Cast<EDIMessage>().Any(x => (x.EM_MessageType == MessageTypeList.Codes.CIN || x.EM_MessageType == EntryActionCodeList.Codes.CIN745) && x.EM_MessageSubType == EntryActionCodeList.Codes.CIN745))
						{
							var manager = new CINExportMessageBuilderManager(new EU.Business.ErrorCollector());
							result = manager.SendMessage(declaration, EntryActionCodeList.Codes.CIN745, true).result;
						}
						else if (!declaration.JE_MasterBill.IsEmpty && !cusEntryHeader.Messages.Cast<EDIMessage>().Any(x => (x.EM_MessageType == MessageTypeList.Codes.CIN || x.EM_MessageType == EntryActionCodeList.Codes.CIN755) && x.EM_MessageSubType == EntryActionCodeList.Codes.CIN755))
						{
							var manager = new CINExportMessageBuilderManager(new EU.Business.ErrorCollector());
							result = manager.SendMessage(declaration, EntryActionCodeList.Codes.CIN755, true).result;
						}

						if (result != DeltaGMessageSender.MessageSendSuccessful)
						{
							Logger.AddWarning($"CIN Message throws an error during the process of the Entry {cusEntryHeader.CH_BGMReference} and has not been sent. Errors: {result}");
						}
					}
				}
			}
		}

		protected bool IsDeclarationOwnedByCurrentCompany(Shared.BaseJobDeclaration declaration)
		{
			return declaration.Company != null && declaration.Company.PK == GlbCompany.CurrentCompany.PK;
		}

		void UpdateOrAddEntryNumberIfNeeded(CusEntryHeader entryHeader, IResponseDataProvider dataProvider)
		{
			var entryType = dataProvider.IsImport ? CusEntryNumberTypes.France.Import : CusEntryNumberTypes.France.Export;
			if (CusEntryNumber.Load(entryHeader, entryType, entryHeader.CountryCode) == null)
			{
				var refdec = dataProvider.Refdec;
				var issueDate = GetIssueDate(dataProvider);
				if (!refdec.IsEmpty && issueDate.IsValid && !issueDate.IsEmpty)
				{
					var entryNumber = CusEntryNumber.New(entryHeader, entryType, entryHeader.CountryCode);
					entryNumber.CE_IssueDate = issueDate;
					entryNumber.CE_EntryNum = refdec;
				}
			}
		}

		protected virtual ZString GetResponseStatus(CusEntryHeader entry, IResponseDataProvider dataProvider)
		{
			var result = EntryActionHelper.GetEntryStatus(incomingMessage);

			if (result.IsEmpty && dataProvider.HasErrors)
			{
				return EntryStatusDescriptionCodeList.Codes.ES040;
			}

			return result;
		}

		protected virtual void UpdateStatus(CusEntryHeader entry, IResponseDataProvider dataProvider, out ZBool updatedToBAEForTheFirstTime)
		{
			var currentEntryStatus = entry.CH_EntryStatus;
			var responseStatus = GetResponseStatus(entry, dataProvider);

			if (EntryActionHelper.ShouldRevertEntryStatus(responseStatus) && entry.LastNonIntermediateEntryStatus != ZString.Empty)
			{
				entry.CH_EntryStatus = entry.LastNonIntermediateEntryStatus;
			}
			else if (EntryActionHelper.ShouldUpdateEntryStatus(responseStatus, currentEntryStatus))
			{
				entry.CH_EntryStatus = responseStatus;
				UpdateEntryInstructionSubStyleWhenEntryStatusUpdated(entry);
			}
			updatedToBAEForTheFirstTime = entry.CH_EntryReleaseDate.IsEmpty && EntryActionHelper.IsVariousBAEStatus(entry.CH_EntryStatus);

			var issueDate = GetIssueDate(dataProvider);
			if (issueDate.IsValid && !issueDate.IsEmpty)
			{
				if (CheckIsGvmsAdviceAndStatusES050(entry, dataProvider))
				{
					entry.Logs.AddNew(Events.CustomsEntryStatus, dataProvider.Evenement, issueDate.ToOffset());
				}
				else if (entry.CH_EntryStatusInfo.HasChanges)
				{
					entry.Logs.AddNew(Events.CustomsEntryStatus, entry.CH_EntryStatus, issueDate.ToOffset());
				}

				if (updatedToBAEForTheFirstTime)
				{
					entry.CH_EntryReleaseDate = issueDate;
				}
			}

			entry.CH_Status = dataProvider.HasErrors ? MessageStatusCodeList.Codes.Error : MessageStatusCodeList.Codes.OK;
			DeclarationEntryStatusUpdater.Update(entry.Declaration);
		}

		void UpdateEntryInstructionSubStyleWhenEntryStatusUpdated(CusEntryHeader entry)
		{
			if (EntryActionHelper.IsVariousBAEStatus(entry.CH_EntryStatus))
			{
				var entryInstruction = entry.EntryInstruction;
				if (entryInstruction?.IsPrelodgedSubstyle ?? false)
				{
					entryInstruction.CEI_SubStyle = CusEntryInstruction.GetComplementarySubstyle(entryInstruction.CEI_SubStyle);
				}
			}
		}

		ZDateTime GetIssueDate(IResponseDataProvider dataProvider)
		{
			var etatDateString = dataProvider.EtatDate;
			var etatHeureTimeString = dataProvider.EtatHeure;
			var etatDateTimeString = etatDateString + " " + etatHeureTimeString;
			ZDateTime.TryParseExact(etatDateTimeString, out var result, DateTimeFormat);
			return result;
		}

		protected bool CheckIsGvmsAdviceAndStatusES050(CusEntryHeader entry, IResponseDataProvider dataProvider) => (entry.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES050) && (dataProvider?.IsGvmsAdvice ?? false);

		void UpdateConfirmedGuaranteeAmount(CusEntryHeader entryHeader, IResponseDataProvider dataProvider)
		{
			if (dataProvider.Montantcautionnable != null)
			{
				entryHeader.CH_ConfirmedGuaranteeAmount = (ZDecimal)dataProvider.Montantcautionnable;
			}
		}

		#region Fees
		void UpdateFees(CusEntryHeader entryHeader, IResponseDataProvider dataProvider)
		{
			ZDecimal transactionAmount = 0.0m;
			if (dataProvider.HasLiquidation)
			{
				DeleteConfirmedFees(entryHeader);

				foreach (var reponseLiquidations in dataProvider.ResponseLiquidations)
				{
					foreach (var taxationDetail in reponseLiquidations.TaxDetails)
					{
						if (taxationDetail.typtax == FeeTypeCodeConverter.FlatRate)
						{
							AddEntryHeaderFee(entryHeader, taxationDetail);
						}
						else
						{
							CreateEntryLineFee(entryHeader, reponseLiquidations, taxationDetail);
						}
					}

					transactionAmount -= reponseLiquidations.TaxDetails.Where(x => x.statutLiquidation == MethodOfPayment.AI2).Sum(x => x.montanttax);

					Shared.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, FRPermitHelper.GetPermitAppIdForMessage, entryHeader.CountryCode, false, Customs.Business.PermitTransactionStatusList.Codes.Pending, UpdateTransactionQuantity(transactionAmount));
				}
			}

			if (dataProvider.HasArticleDAU)
			{
				foreach (var articleDAU in dataProvider.ResponseArticleDAUs)
				{
					UpdateConfirmedCustomsValue(entryHeader, articleDAU);
				}
			}

			if (IsProcessingAmendmentConfirmation(entryHeader.CH_EntryStatus))
			{
				ProcessAmendmentConfirmation(entryHeader);
			}
		}

		void ProcessAmendmentConfirmation(CusEntryHeader entryHeader)
		{
			var ai2Permit = entryHeader.Declaration.Ai2Permit;

			if (ai2Permit != null)
			{
				var reference = Shared.PermitHelper.GetPermitReferenceForEntry(entryHeader);
				var referenceForLine = Shared.PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader);
				var currentTransactions = Shared.PermitHelper.GetRelatedPermitTransactions(entryHeader.Factory, entryHeader.CountryCode, reference, referenceForLine, GuaranteeTypeList.Codes.AI2);
				var currentTransactionAmount = currentTransactions.Where(x => x.CPL_TransactionStatus == Shared.PermitTransactionStatusList.Codes.Confirmed).Sum(x => x.CPL_TranValue);
				var newTransactionAmount = currentTransactionAmount < 0
														? (-currentTransactionAmount - entryHeader.Ai2Amount)
														: currentTransactionAmount - entryHeader.Ai2Amount;

				if (newTransactionAmount != 0m)
				{
					try
					{
						if (ai2Permit.Mutex.Lock())
						{
							var permitRecord = new Shared.PermitRecord
							{
								PermitHeader = ai2Permit,
								Value = newTransactionAmount,
								Quantity = 0
							};
							ai2Permit.AddTransaction(reference, Shared.PermitHelper.GetPermitComment(entryHeader, permitRecord), FRPermitHelper.GetPermitAppIdForMessage(incomingMessage), permitRecord.Procedure, permitRecord.Value, permitRecord.Quantity, Shared.PermitTransactionStatusList.Codes.Confirmed, referenceForLine);
						}
					}
					finally
					{
						var mutex = ai2Permit.Mutex;
						if (mutex.HasLock)
						{
							mutex.Unlock();
						}
					}
				}
			}
		}

		public static Action<Shared.SharedCusPermitLineTransaction> UpdateTransactionQuantity(decimal newValue) => (transaction => UpdateTransactionQuantityValue(transaction, newValue));

		static void UpdateTransactionQuantityValue(Shared.SharedCusPermitLineTransaction transaction, decimal newValue)
		{
			var oldvalue = transaction.CPL_TranValue;
			if (oldvalue != newValue && newValue != 0)
			{
				transaction.CPL_TranValue = newValue;
			}
		}

		protected ZDecimal CalculateTotalPayable(CusEntryHeader entryHeader)
		{
			var headerCharges = entryHeader.Charges.Cast<CusEntryHeaderCharges>().Sum(x => x.C1_ChargeAmount);
			var lineCharges = entryHeader.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees).Cast<CusEntryLineFee>().Sum(x => x.CF_ChargeAmount);

			return headerCharges + lineCharges;
		}

		void CreateEntryLineFee(CusEntryHeader entryHeader, IResponseLiquidationWrapper responseArticle, ITaxDetailWrapper taxationDetail)
		{
			var entryLineNumber = responseArticle.numart.ToZInt();
			var entryLine = (CusEntryLine)entryHeader.MergedLines.FindByLineNumber(entryLineNumber);
			if (entryLine != null)
			{
				var entryLineFee = entryLine.ConfirmedFees.AddNew();
				entryLineFee.CF_ChargeType = FeeTypeCodeConverter.GetEUFeeTypeCode(taxationDetail.codtax);
				entryLineFee.CF_MethodOfCalculation = GetFeeMethodOfCalculation(entryLine, taxationDetail);
				entryLineFee.CF_BaseValue = taxationDetail.asstax;
				entryLineFee.CF_Rate = taxationDetail.quotax;
				entryLineFee.CF_ChargeAmount = (ZDecimal)taxationDetail.montanttax;
				entryLineFee.CF_MethodOfPayment = taxationDetail.statutLiquidation;
				entryLineFee.NationalFeeTypeCode = taxationDetail.codtax;
				entryLineFee.G4_RateOverride = ZString.Empty;
			}
		}

		void UpdateConfirmedCustomsValue(CusEntryHeader entryHeader, IResponseArticleDAUWrapper articleDAU)
		{
			var entryLineNumber = articleDAU.numart.ToZInt();
			var entryLine = (CusEntryLine)entryHeader.MergedLines.FindByLineNumber(entryLineNumber);
			if (entryLine != null)
			{
				if (articleDAU.Valstat.HasValue)
				{
					entryLine.CL_ConfirmedStatisticalValue = articleDAU.Valstat.Value;
				}

				if (articleDAU.Valdou.HasValue)
				{
					entryLine.CL_ConfirmedCustomsValue = articleDAU.Valdou.Value;
				}

				if (articleDAU.Asstva.HasValue)
				{
					entryLine.CL_ConfirmedValueForVAT = articleDAU.Asstva.Value;
				}
			}
		}

		protected string GetFeeMethodOfCalculation(CusEntryLine entryLine, ITaxDetailWrapper taxationDetail)
		{
			if (Utilities.Round(taxationDetail.asstax * taxationDetail.quotax, 0) == taxationDetail.montanttax)
			{
				if (taxationDetail.LiquidationItemHasSuppUnits)
				{
					return taxationDetail.SuppUnitsMethodOfCalculation;
				}
				else if (taxationDetail.typtax == FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy)
				{
					return entryLine.InvoiceLines[0].JI_CustomsSecondUnitQty;
				}
				else if (taxationDetail.typtax == FeeTypeCodeConverter.CalculationBasedOnThirdUnits)
				{
					return entryLine.InvoiceLines[0].JI_CustomsThirdUnitQty;
				}
			}

			return Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
		}

		void AddEntryHeaderFee(CusEntryHeader entryHeader, ITaxDetailWrapper liquidationItemTaxationDetail)
		{
			var newEntryheaderCharge = entryHeader.ConfirmedCharges.AddNew();
			newEntryheaderCharge.C1_ChargeType = liquidationItemTaxationDetail.codtax;
			newEntryheaderCharge.C1_ChargeAmount = (ZDecimal)liquidationItemTaxationDetail.montanttax;
			newEntryheaderCharge.C1_MethodOfPayment = liquidationItemTaxationDetail.statutLiquidation;
		}

		void DeleteConfirmedFees(CusEntryHeader entryHeader)
		{
			entryHeader.ConfirmedCharges.RemoveAndDeleteAll();
			foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
			{
				entryLine.ConfirmedFees.RemoveAndDeleteAll();
			}
		}

		ZBool IsProcessingAmendmentConfirmation(ZString newStatus) => EntryActionHelper.IsRectificationAccepted(incomingMessage);
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected override string MessageFriendlyNameCore => "Delta G Response Message";
		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.FRCustomsMessage;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Data Time Format strings")]
		const string DateTimeFormat = "dd/MM/yyyy HH:mm";
		protected abstract ZString MessageType { get; }
		protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new[] { MessageType };

		FREDIMessage outgoingMessage;
		FREDIMessage incomingMessage;
		
		BondedWarehouseMessageProcessorCreator BondedWhsMsgProcessorCreator => bondedWhsMsgProcessorCreator ?? (bondedWhsMsgProcessorCreator = new BondedWarehouseMessageProcessorCreator(Logger, GetNewBondedWarehouseMessageProcessor, GetBondedWarehouseNotificationGroupPK));
		BondedWarehouseMessageProcessorCreator bondedWhsMsgProcessorCreator;

		Shared.MessageProcessors.BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
		{
			return new BondedWarehouseMessageProcessor(incomingMessage.PK, null, sendEmail);
		}

		Guid GetBondedWarehouseNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return FRCustomsDataRegistry.Instance.BondedWarehouseNotificationGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}
	}
}
