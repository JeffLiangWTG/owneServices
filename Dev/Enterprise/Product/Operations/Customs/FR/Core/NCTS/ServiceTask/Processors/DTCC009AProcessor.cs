using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC009A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC009AProcessor : DTBaseProcessor<Cc009AType>
	{
		public DTCC009AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC009A processor";

		protected override ZString GetNewMessageStatus(Cc009AType messageObject)
		{
			return EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;
		}

		protected override ZString GetNewDepartureStatus(Cc009AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				var canIniByCusHEA94 = messageObject.Heahea.CanIniByCusHea94;
				var canDecHEA93 = messageObject.Heahea.CanDecHea93;
				if (canDecHEA93 == Flag.Item0 && canIniByCusHEA94 == Flag.Item0)
				{
					return NctsHeader.MovementHeader.LastNonIntermediateStatus;
				}
				else if (canDecHEA93 == Flag.Item1 || canIniByCusHEA94 == Flag.Item1)
				{
					return FR.Business.NctsTransitStatusList.Codes.DeclarationCancelled;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetNewDetailedDepartureStatus(Cc009AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				var canIniByCusHEA94 = messageObject.Heahea.CanIniByCusHea94;
				var canDecHEA93 = messageObject.Heahea.CanDecHea93;
				if (canDecHEA93 == Flag.Item0 && canIniByCusHEA94 == Flag.Item0)
				{
					return NctsDetailedStatusList.Codes.CancellationRefused;
				}
				else if (canDecHEA93 == Flag.Item1 || canIniByCusHEA94 == Flag.Item1)
				{
					return NctsDetailedStatusList.Codes.CancellationAccepted;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetMessageInterpretation(Cc009AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}

		protected override void DoExtraProcessing(Cc009AType messageObject, EDIMessage inboundMessage)
		{
			UpdateTemporaryStorageRegisterIfApplicable(messageObject, inboundMessage);
		}

		void UpdateTemporaryStorageRegisterIfApplicable(Cc009AType messageObject, EDIMessage inboundMessage)
		{
			var canIniByCusHEA94 = messageObject.Heahea.CanIniByCusHea94;
			var canDecHEA93 = messageObject.Heahea.CanDecHea93;
			if (NctsHeader?.MovementHeader != null && (canDecHEA93 == Flag.Item1 || canIniByCusHEA94 == Flag.Item1))
			{
				foreach (var goodsItem in NctsHeader.MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>())
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
				}
			}

			void Notifier(ZString failingRegister)
			{
				ServiceLogger.Log(LogType.Warning, () =>
				{
					return string.Format(CultureInfo.InvariantCulture, (NoResString)"Warning : Processing received message #{0}, type {1} {2}. Register {3} is rolled back: declaration canceled", inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, inboundMessage.EM_MessageSubType, failingRegister);
				});

				NctsHeader.Logs.AddNew(Events.DeclarationHasErrors, "Warning : Register " + failingRegister + " is rolled back, declaration is cancelled", ZDateTimeOffset.UtcNow);

				string body = Res.GetString("7A3B738A-8F53-466A-920C-D13DCFAEA7B0", "Warning : Register {0} has been rolled back for transit {1}, declaration is canceled", failingRegister, NctsHeader.BH_JobReference);

				Business.MessageProcessors.ProcessorHelper.SendEmail(NctsHeader.Factory, NctsHeader.BH_SystemCreateUser, true, GetEmailBody(body), GetEmailsubject(NctsHeader), GetEmailGroupRegistryItem());
			}
		}
	}
}
