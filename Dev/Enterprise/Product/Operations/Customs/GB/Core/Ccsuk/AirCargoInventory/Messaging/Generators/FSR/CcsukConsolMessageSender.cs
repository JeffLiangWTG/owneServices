using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CcsukConsolMessageSender : ConsolMessageSender
	{
		public void SendToCCSUK(CustomsExportConsolIntegrationWrapper consolWrapper, ISendsMessagesToCustoms sendMessagesToCustoms, CcsukTransmissionMessageFunction messageFunction)
		{
			GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
			var manager = new CcsukConsolMessageManager(consolWrapper, messageFunction, sendMessagesToCustoms);
			manager.SendToRecipient(CancellationTokenSource.Token);
		}
	}

	class CcsukConsolMessageManager : ConsolMessageManager
	{
		public CcsukConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, CcsukTransmissionMessageFunction messageFunction, ISendsMessagesToCustoms sendMessagesToCustoms)
			: base(consolWrapper, null, sendMessagesToCustoms)
		{
			this.messageFunction = messageFunction;
		}

		protected override string SubFunctionCode => messageFunction.MessageSubType;

		protected override IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new CcsukConsolMessageBuilder(consolWrapper, messageFunction);
		}

		readonly CcsukTransmissionMessageFunction messageFunction;
	}

	class CcsukConsolMessageBuilder : ConsolMessageBuilder
	{
		public CcsukConsolMessageBuilder(CustomsExportConsolIntegrationWrapper consolWrapper, CcsukTransmissionMessageFunction messageFunction)
			: base(consolWrapper, null)
		{
			this.messageFunction = messageFunction;
		}

		protected override ZString GetMessageText()
		{
			if (consolWrapper.MawbExportHelper.ME_Profile.IsEmpty)
			{
				errorCollector.AddError("Profile/PIMA may not be empty.");
			}
			else
			{
				switch (messageFunction)
				{
					case CcsukTransmissionMessageFunction.CUKFSR _:
						{
							var fsrProvider = new ExportConsolToFsrProvider(consolWrapper.MawbExportHelper, messageFunction.MessageSubType);
							var awbDisplayName = messageFunction is CcsukTransmissionMessageFunction.CUKFSR.FsaWithoutShed
								? consolWrapper.MawbExportHelper.ME_MasterUCR.ToString()
								: consolWrapper.MawbExportHelper.ME_MasterUCR + " at " + fsrProvider.Airport + fsrProvider.ShedOperatorIdentity;
							creator = new CukFsrCreator(fsrProvider, awbDisplayName, messageFunction, consolWrapper.Factory, CharSet);
							return creator.MakeMessageText();
						}
					case CcsukTransmissionMessageFunction.CUKG2G _:
						creator = new CUKG2GMessageGenerator(consolWrapper, CharSet, errorCollector);
						return creator.MakeMessageText();
				}
			}
			return ZString.Empty;
		}

		protected override void SetMessageType(EDIMessage message)
		{
			message.EM_MessageType = messageFunction.MessageType;
		}

		protected override void SetMessageSubType(EDIMessage message)
		{
			message.EM_MessageSubType = messageFunction.MessageSubType;
		}

		protected override void SetApplicationReference(EDIMessage message)
		{
			message.EM_ApplicationReference = CcsukEdiMessageDiverter.PimaForCommunityDatabase;
		}

		protected override ZString GetInterpretation(EDIMessage message)
		{
			return creator?.MessageInterpretation ?? ZString.Empty;
		}

		CusAwbToInventoryMessageGenerator creator;
		readonly CcsukTransmissionMessageFunction messageFunction;
	}
}
