using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.AE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class DocMessageSender
{
	public DocMessageSender(ManifestSupportingDocSendingObjectParent sendingObjectParent)
	{
		SendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
	}
	ManifestSupportingDocSendingObjectParent SendingObjectParent { get; }

	public int SendMessages()
	{
		var messages = new List<AEEDIMessage>();
		foreach (var sendingObject in SendingObjectParent.SelectedSendingObjects.Cast<SupportingDocSendingObject>())
		{
			var builder = new DocMessageBuilder(new MPCIAttachmentMessageBuilder(new MPCIAttachmentMessageProvider(sendingObject)), sendingObject);
			var message = builder.PopulateMessage();
			if (message != null)
			{
				messages.Add(message);
			}
		}

		if (messages.Count > 0)
		{
			try
			{
				SendingObjectParent.Factory.Save();
				return messages.Count;
			}
			catch (ZSaveException ex)
			{
				messages.ForEach(m => m.Delete());
				foreach (var log in SendingObjectParent.ParentManifest.Logs.LogsNotInDB)
				{
					log.Delete();
				}
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		return 0;
	}
}
