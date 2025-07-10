using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.EMCS.GUI
{
	public class EMCSMessageSendingFormConfiguration : IEMCSMessageSendingFormConfiguration
	{
		bool IEMCSMessageSendingFormConfiguration.IsOKToSend<TSendingAction>(EMCSMessageSendingActionParent<TSendingAction> parent)
		{
			var isOKToSend = true;

			var declaration = parent.JobDeclaration;
			if (declaration.JE_MessageStatus == EDIMessage.Status.Sent)
			{
				using (var form = new ConfirmSendForm())
				{
					if (isOKToSend = ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
					{
						declaration.Logs.AddNew(AutoEvents.Authorised, Constants.SendingMessageEventList.Reference, ZDateTimeOffset.Now, new KeyValuePair<string, string>(Constants.SendingMessageEventList.RES, form.Reason));
					}
				}
			}

			return isOKToSend;
		}
	}
}
