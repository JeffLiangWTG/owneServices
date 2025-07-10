using System.Collections.Generic;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class SeaMessageSender
	{
		public SeaMessageSender(AsycudaManifestHeader header, ZString sendingAction, List<CusTransactionNumber> documentIDs)
		{
			this.sendingAction = sendingAction;
			this.header = header;
			this.documentIDs = documentIDs;
		}

		readonly ZString sendingAction;
		readonly AsycudaManifestHeader header;
		readonly List<CusTransactionNumber> documentIDs;

		public ZString CreateMessage() =>
			(new BLMessageBuilder(new ManifestWrapper(header, sendingAction, documentIDs)) as IXmlMessageBuilder)
			.GenerateXmlMessage().GetSerializedString();
	}
}
