using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC029B;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC029BProcessor : DTBaseProcessor<Cc029BType>
	{
		public DTCC029BProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC029B processor";

		protected override ZString GetNewMessageStatus(Cc029BType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc029BType messageObject) => EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

		protected override ZString GetMessageInterpretation(Cc029BType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}

		internal override void UpdateGuaranteeTransactionsIfNeeded(Cc029BType messageObject, EDIMessage incomingMessage)
		{
			var header = incomingMessage.EM_LinkedObject as NctsHeader;

			if (header != null)
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeader.GetOutgoingMessage(incomingMessage), EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.France, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		protected override void DoExtraProcessing(Cc029BType messageObject, EDIMessage inboundMessage)
		{
			UpdateTemporaryStorageRegisterIfApplicable(inboundMessage);
		}

		void UpdateTemporaryStorageRegisterIfApplicable(EDIMessage inboundMessage)
		{
			if (NctsHeader?.MovementHeader != null)
			{
				foreach (var goodsItem in NctsHeader.MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>())
				{
					var processor = new CusTempStorageRegisterProcessor<EDIMessage>(goodsItem.TemporaryStorageRegisterTransactionDataProvider, Logger, notifierWhenInsufficient: Notifier);
					try
					{
						processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
						processor.AddTransactionsWhenSaving(inboundMessage);
					}
					finally
					{
						processor.UnlockRegistersMutexes();
					}

					void Notifier(ZString failingRegister)
					{
						ServiceLogger.Log(LogType.Warning, () =>
						{
							return string.Format(CultureInfo.InvariantCulture, (NoResString)"Warning : Processing received message #{0}, type {1} {2}. Register {3} has not been updated : there are not enough remaining packages quantity", inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, inboundMessage.EM_MessageSubType, failingRegister);
						});

						NctsHeader.Logs.AddNew(Events.DeclarationHasErrors, "Warning : Register " + failingRegister + " has not been updated, there are not enough packages remaining", ZDateTimeOffset.UtcNow);

						string body = Res.GetString("ADEA48AB-1BFC-4337-BE68-42FD928979F9", "Warning : Register {0} has not been updated for transit {1}, there are not enough packages remaining", failingRegister, NctsHeader.BH_JobReference);

						Business.MessageProcessors.ProcessorHelper.SendEmail(NctsHeader.Factory, NctsHeader.BH_SystemCreateUser, true, GetEmailBody(body), GetEmailsubject(NctsHeader), GetEmailGroupRegistryItem());
					}
				}
			}
		}

		protected override void GenerateDocuments(EDIMessage inboundMessage)
		{
			if (NctsHeader != null)
			{
				var nctsMessage = inboundMessage.Factory.Load<EU.NCTS.Business.NctsEdiMessage>(inboundMessage.PK);
				new EU.NCTS.Business.NctsTadEdocSaver(NctsHeader).RenderTadAndStoreInEdocs(nctsMessage);
			}
		}
	}
}
