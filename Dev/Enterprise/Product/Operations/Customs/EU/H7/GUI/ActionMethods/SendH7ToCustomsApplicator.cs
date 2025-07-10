using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.GUI;

public class SendH7ToCustomsApplicator : OperationalActionMethodApplicator
{
	public SendH7ToCustomsApplicator() : base(Res.GetString("548e1d54-b564-4504-98cc-ffb1d72d38ca", "Send H7 To Customs"))
	{
	}

	public SendH7ToCustomsApplicator(string name, BusinessObjectFactory factory) : base(name, factory)
	{
	}

	protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
	{
		foreach (AsycudaManifestHeader header in targets)
		{
			var messageSendingParent = header.ApplicationBusinessProvider.GetNewMessageSendingObjectParent(header);
			var messagesSent = 0;
			foreach (MessageSendingObject sendingObject in messageSendingParent.SendingObjectsCollection)
			{
				sendingObject.Action = sendingObject.ActionForSendingCustomsDeclaration;
				if (sendingObject.CreateSender().Send() != null)
				{
					messagesSent++;
				}
			}

			if (messagesSent > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Success, "{0} : {1}.", GetAsycudaManifestHeaderIdLink(header), SubmitSucceeded);
			}
		}
	}

	public LogControllerLink GetAsycudaManifestHeaderIdLink(AsycudaManifestHeader header)
	{
		return new LogControllerLink(header.AMA_JobReference, ControllerIDs.Customs.EU.EUH7, header.PK);
	}

	protected string SubmitSucceeded => Res.GetString("6ca9872b-c283-4731-8bec-912e5000e1fc", "Submit Succeeded");
}
