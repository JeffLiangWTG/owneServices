using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCImportLicenseStatusInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCImportLicenseStatusInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool UseSequenceAsMessageNumber => true;

		protected override IEnumerable<ZString> GetMessageTexts(ZString messageType, ZString messageSubType, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			var liResponse = XmlObjectSerializer.Deserialize<respostaconsultali>(responseMessage);

			if (liResponse.Item is listalicompletatype listalicompletatype)
			{
				foreach (var li in listalicompletatype.licompleta)
				{
					yield return XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(li);
				}
			}
		}
	}
}
