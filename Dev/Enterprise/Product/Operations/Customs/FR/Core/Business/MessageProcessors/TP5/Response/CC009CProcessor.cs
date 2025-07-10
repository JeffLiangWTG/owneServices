using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC009CProcessor : TP5BaseProcessor<Cc009CType>
	{
		public CC009CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.InvalidationDecision;

		protected override ZString GetMRNFromResponseMessage(Cc009CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc009CType messageObject) => IsInvalidationFlagEnabled(messageObject) ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;

		protected override ZString GetNewDepartureStatus(Cc009CType messageObject) => IsInvalidationFlagEnabled(messageObject) ? NCTS5DepartureCustomsStatusList.Codes.Cancelled : ZString.Empty;

		protected override ZString GetNewPhase(Cc009CType messageObject) => IsInvalidationFlagEnabled(messageObject) ? EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Cancellation : EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		ZBool IsInvalidationFlagEnabled(Cc009CType messageObject) => messageObject.Invalidation?.Decision == Flag.Item1;

		protected override void DoExtraProcessing(NctsHeader header, Cc009CType messageObject, EDIMessage inboundMessage)
		{
			base.DoExtraProcessing(header, messageObject, inboundMessage);

			UpdateTemporaryStorageRegisterIfApplicable(header, messageObject, inboundMessage);
		}

		protected void UpdateTemporaryStorageRegisterIfApplicable(NctsHeader header, Cc009CType messageObject, EDIMessage inboundMessage)
		{
			if (header?.MovementHeader != null && IsInvalidationFlagEnabled(messageObject))
			{
				foreach (var bill in header.Bills.Cast<NctsBill>())
				{
					foreach (var goodsItem in bill.GoodsItems.Cast<NctsDepartureCargoDesc>())
					{
						var processor = new CusTempStorageRegisterProcessor<EDIMessage>(goodsItem.TemporaryStorageRegisterTransactionDataProvider, Logger, notifierWhenInsufficient: Notifier);
						try
						{
							processor.CalculateTransactionsAndLockMutexIfNeededForRollingBackTransaction();
							processor.AddTransactionsWhenSaving(inboundMessage);
						}
						finally
						{
							processor.UnlockRegistersMutexes();
						}

						void Notifier(ZString failingRegister)
						{
							Logger.LogWarning(string.Format(CultureInfo.InvariantCulture, (NoResString)"Warning : Processing received message #{0}, type {1} {2}. Register {3} is rolled back: declaration canceled", inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, inboundMessage.EM_MessageSubType, failingRegister));

							header.Logs.AddNew(Events.DeclarationHasErrors, "Warning : Register " + failingRegister + " is rolled back, declaration is cancelled", ZDateTimeOffset.UtcNow);

							var body = Res.GetString("F4066F49-9A41-4F66-8A54-635DC9DA7356", "Warning : Register {0} has been rolled back for transit {1}, declaration is canceled", failingRegister, header.BH_JobReference);

							var jobReferenceNumber = header.MovementHeader?.BM_PaperlessInbondNum;
							Business.MessageProcessors.ProcessorHelper.SendEmail(header.Factory, header.BH_SystemCreateUser, true, GetEmailBody(jobReferenceNumber, body), GetEmailsubject(jobReferenceNumber), GetEmailGroupRegistryItem());
						}
					}
				}
			}
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(NctsHeader header, Cc009CType messageObject, EDIMessage incomingMessage)
		{
			if (IsInvalidationFlagEnabled(messageObject))
			{
				var appId = FRPermitHelper.GetPermitAppIdForMessage(incomingMessage);
				header.MovementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(appId);
				header.MovementHeader.GuaranteeTransactionCoordinator.CounterBalanceTransactionsOfGuaranteesNoLongerInDeclaration(appId);
			}
		}
	}
}
