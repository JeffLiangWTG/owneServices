using System;
using System.Threading;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ConsolMessageSender : ChiefDeclarationMessageSender
	{
		public void SendToRecipient(CustomsExportConsolIntegrationWrapper consolWrapper, ISendsMessagesToCustoms sendMessagesToCustoms, GbDes242MessageFunction how, bool isChiefMessage)
		{
			if (CheckTheFunctionCodeIsValidOrNotForSending(isChiefMessage, sendMessagesToCustoms, how))
			{
				GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
				var manager = GetConsolMessageManager(consolWrapper, how, sendMessagesToCustoms);
				manager?.SendToRecipient(CancellationTokenSource.Token);
			}
		}

		protected virtual ConsolMessageManager GetConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return new ConsolMessageManager(consolWrapper, how, sendMessagesToCustoms);
		}

		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction how)
		{
			throw new NotSupportedException();
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new[] { "CUK", "GCI" }; // constants not visible from here
		}

		public CancellationTokenSource CancellationTokenSource => cancellationTokenSource ?? ResetCancellationTokenSource();
		public CancellationTokenSource ResetCancellationTokenSource() => cancellationTokenSource = new CancellationTokenSource();
		CancellationTokenSource cancellationTokenSource;
	}
}
