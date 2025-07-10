using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IE.Business
{
	public abstract class OutboundMessageDataProviderEDIInterchange : EDIInterchange, IMessageDataProvider
	{
		protected OutboundMessageDataProviderEDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ReceiveTransmit = Direction.Transmit;
		}

		#region IMessageDataProvider

		BinaryReader IMessageDataProvider.GetMessageData() => GetEI_BodyWithMessageAttachments();

		BinaryReader GetEI_BodyWithMessageAttachments()
		{
			var messageDataBuilder = new StringBuilder(EI_BodyText);

			foreach (var attachment in ContainedMessages.Cast<EDIMessage>().SelectMany(x => x.MessageAttachments.Cast<EDIMessageAttach>()).Where(x => x.EG_StorageDocsGuid.IsValid))
			{
				var storageIdInBase64 = Convert.ToBase64String(attachment.EG_StorageDocsGuid.ToGuid().ToByteArray());
				var imageDataInBase64 = Convert.ToBase64String(attachment.GetAttachment()?.ImageData ?? ZBlob.Empty);

				messageDataBuilder.Replace(storageIdInBase64, imageDataInBase64);
			}

			return new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(messageDataBuilder.ToString())));
		}

		#endregion
	}
}
