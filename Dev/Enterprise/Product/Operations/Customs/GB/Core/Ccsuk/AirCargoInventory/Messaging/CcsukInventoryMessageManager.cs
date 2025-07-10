using System;
using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CcsukInventoryMessageManager : MessageManager
	{
		public CcsukInventoryMessageManager(BusinessObject businessObject, CcsukTransmissionMessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
			: base(true)
		{
			this.bizO = businessObject;
			this.how = how;
			this.sendMessagesToCustoms = sendMessagesToCustoms;
		}

		public CcsukInventoryMessageManager(ICcsukCusAwb cusAwb, CcsukTransmissionMessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
			: this((BusinessObject)cusAwb, how, sendMessagesToCustoms)
		{
		}

		protected override string MessageSentNotificationText
		{
			get { return how.MessageType + "/" + how.MessageSubType + " message queued for sending."; }
		}

		bool doNotSaveAutomaticallyAfterwards;
		internal bool SendToCommunity(bool doNotSaveAutomaticallyAfterwards)
		{
			this.doNotSaveAutomaticallyAfterwards = doNotSaveAutomaticallyAfterwards;
			return SendToCommunity();
		}

		internal bool SendToCommunity()
		{
			var reasonsWeCantSend = new StringCollection();
			var warningsAboutSending = new StringCollection();

			var awb = bizO as CusMAWB;

			if (awb != null
				&& LicenceAndPimaHelper.IsFullShed((ICcsukCusAwb)awb)
				&& awb.CM_FlightNo.IsEmpty
				&& (how.MessageSubType == CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode
					|| how.MessageSubType == CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode))
			{
				reasonsWeCantSend.Add("It is not permitted to send this message using your ETSF profile when the bill lacks a flight number. " +
									  "This sending will be aborted.\r\n" +
									  "Supply a flight number and re-try the sending, or if you do not have the flight number you may consider using your agent profile to create a pre-arrival record. \r\n" +
									  "In doing the latter, it should be remembered that agent pre-arrival records are expunged (archived) if not arrived within 4 days.");
			}

			SendMessage(sendMessagesToCustoms,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder },
				how.MessageType + how.MessageSubType,
				CancellationTokenSource.Token);

			return reasonsWeCantSend.Count == 0 && warningsAboutSending.Count == 0;
		}

		public CancellationTokenSource CancellationTokenSource => cancellationTokenSource ?? (cancellationTokenSource = new CancellationTokenSource());
		CancellationTokenSource cancellationTokenSource;

		protected override void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			if (!doNotSaveAutomaticallyAfterwards)
			{
				base.SaveFactoryAfterSendingMessages(sender, token);
			}
		}

		protected virtual IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new CcsukInventoryMessageBuilder(master, how);
		}

		protected override BusinessObject Master
		{
			get { return bizO; }
		}

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get { throw new NotImplementedException(); }
		}

		readonly BusinessObject bizO;
		protected CcsukTransmissionMessageFunction how;
		readonly ISendsMessagesToCustoms sendMessagesToCustoms;
	}
}
