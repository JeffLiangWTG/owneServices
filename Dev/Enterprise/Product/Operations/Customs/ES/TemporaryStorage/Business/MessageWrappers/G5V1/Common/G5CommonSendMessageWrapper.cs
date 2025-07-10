using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5CommonSendMessageWrapper : G5GenericSendMessageWrapper, IG5CommonMessageDataProvider
	{
		public G5CommonSendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData) : base(tempHeader, certificateData)
		{
		}

		public IG5CommonHeader Header => header ??= new G5CommonHeaderWrapper(tempHeader);
		G5CommonHeaderWrapper header;

		public IReadOnlyCollection<IG5CommonLine> Lines => lines ??= tempHeader.Bills.FirstOrDefault()?.PackedItems.Cast<TemporaryStoragePackedItem>().Where(x => !x.IsMissing).Select(x => new G5CommonLineWrapper(x)).ToList().AsReadOnly();
		IReadOnlyCollection<G5CommonLineWrapper> lines;
	}
}
