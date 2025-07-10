using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CcsukInventoryRemovalMessageSender : CcsukInventoryICusAwbMessageSender
	{
		public CcsukInventoryRemovalMessageSender(ZStringBuilder confirmations)
		{
			this.confirmations = confirmations;
		}

		public void Send(CusUnderbond cusUnderbond, ICcsukCusAwb awb, ISendsMessagesToCustoms sendMessagesToCustoms, CcsukTransmissionMessageFunction how)
		{
			if (ValidateAndShowUserAnyWarningsOrErrors(cusUnderbond, sendMessagesToCustoms, how))
			{
				if (!hasWarnedAboutServiceTasksAlready)
				{
					GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
					hasWarnedAboutServiceTasksAlready = true;
				}
				var manager = new CcsukInventoryRemovalMessageManager(cusUnderbond, awb, how, sendMessagesToCustoms, confirmations);
				manager.SendToCommunity();
			}
		}
		readonly ZStringBuilder confirmations;
		bool hasWarnedAboutServiceTasksAlready;
	}

	class CcsukInventoryRemovalMessageManager : CcsukInventoryMessageManager
	{
		public CcsukInventoryRemovalMessageManager(CusUnderbond cusUnderbond, ICcsukCusAwb cusAwb, CcsukTransmissionMessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms, ZStringBuilder confirmations)
			: base(cusAwb, how, sendMessagesToCustoms)
		{
			this.cusUnderbond = cusUnderbond;
			this.confirmations = confirmations;
		}

		protected override IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new CcsukInventoryRemovalMessageBuilder(master, how, cusUnderbond);
		}

		protected override void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			try
			{
				cusUnderbond.C4_Status = EDIMessage.Status.Pending;
				Master.Factory.Save();
				OnMessageSent(sender);
				confirmations.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} message for removal {1} created and queued for transmission", how.MessageSubType, cusUnderbond.C4_SendersMessageReference));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
		readonly CusUnderbond cusUnderbond;
		readonly ZStringBuilder confirmations;
	}

	class CcsukInventoryRemovalMessageBuilder : CcsukInventoryMessageBuilder
	{
		public CcsukInventoryRemovalMessageBuilder(BusinessObject master, CcsukTransmissionMessageFunction how, CusUnderbond cusUnderbond)
			: base(master, how)
		{
			this.cusUnderbond = cusUnderbond;
		}

		public override void AfterFullSuccess(Enterprise.Messaging.Business.MessageBuilders.IBuilderResult builderResult)
		{
			// We need a more specific CAR for removals in case a request is rejected
			var commonAccessReference = string.Format("{0}/{1}", EDIMessage.MessageNumberPlaceHolder, cusUnderbond.C4_SendersMessageReference);
			builderResult.Message.EM_MessageText = builderResult.Message.EM_MessageText.Replace(GbTransmissionMessageGenerator.SysCarPlaceHolder, commonAccessReference);
		}

		readonly CusUnderbond cusUnderbond;
	}
}
