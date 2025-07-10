using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public sealed class CHEDIInterchange : EDIInterchange, IMessageDataProvider, Integration.Customs.CH.IEDIInterchange
{
	public CHEDIInterchange(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EI_ApplicationCode = ApplicationCodes.CHCustomsEdec;
	}

	#region IMessageDataProvider

	public BinaryReader GetMessageData()
	{
		return EI_InterchangeType == MessageTypeCodeList.Codes.EBD ? GetEI_BodyWithMessageAttachment() : new BinaryReader(GetEI_BodyTextReader().CopyAndDispose());
	}

	BinaryReader GetEI_BodyWithMessageAttachment()
	{
		var messageData = EI_BodyText;

		foreach (var attachment in ContainedMessages.Cast<CHEDIMessage>()
		.SelectMany(x => x.MessageAttachments.Cast<EDIMessageAttach>())
		.Where(x => x.EG_StorageDocsGuid.IsValid))
		{
			var storageIdInBase64 = Convert.ToBase64String(attachment.EG_StorageDocsGuid.ToGuid().ToByteArray());
			var imageDataInBase64 = Convert.ToBase64String(attachment.GetAttachment()?.ImageData ?? ZBlob.Empty);
			messageData = messageData.Replace(storageIdInBase64, imageDataInBase64);
		}

		return new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(messageData)));
	}

	#endregion
}
