using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ExitControlSendToCustomsMenuCreator : EU.ExitControl.GUI.ExitControlSendToCustomsMenuCreator
	{
		public ExitControlSendToCustomsMenuCreator(CusExitHeader header)
			: base(header)
		{
		}

		protected override bool HasValidSystemSettings() => header.Company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired() && HasExitReports();

		bool HasExitReports()
		{
			bool hasExitReports = true;
			if (header.CusExitReports.Count == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("{142F4273-FB13-45F8-A5FD-0CCADD7730BD}", "No exit reports exist – Please add exit report before attempting to send a message to customs."));
				hasExitReports = false;
			}
			return hasExitReports;
		}

		protected override void OnMessageSendingFormOk(EU.ExitControl.Business.ExitControlMessageSendingObjectParent sendingParent)
		{
			var messageSent = 0;
			sendingParent.SendingObjectsCollection.Cast<ExitControlMessageSendingObject>()
				.Where(action => action.ShouldSend)
				.Select(action => action.CreateSender())
				.ForEach(sender =>
				{
					sender.Send();
					messageSent++;
				});
			if (messageSent > 0)
			{
				TrySaveAndShowMessage(sendingParent.Factory, messageSent);
			}
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				Globals.Message.ShowInformation(GetMessageSentText(messagesCreated));
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		string GetMessageSentText(int count)
		{
			return Res.GetString("389F62E7-2CB6-47FB-A4AC-008C973B986F", "{0} message(s) queued for sending.", count);
		}

		protected new CusExitHeader header => (CusExitHeader)base.header;

		protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetMessageSendingParent() => new ExitControlMessageSendingObjectParent(header);
	}
}
