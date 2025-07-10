using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCImportLicenseAcceptInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCImportLicenseAcceptInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool UseSequenceAsMessageNumber => true;

		protected override IEnumerable<ZString> GetMessageTexts(ZString messageType, ZString messageSubType, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			var liList = XmlObjectSerializer.Deserialize<loteli>(responseMessage);
			foreach (var li in liList.listaLIVORetorno)
			{
				var loteli = liList.ShallowClone();
				loteli.idLote = BRMessageHelper.FormatIdLote(loteli.idLote);
				loteli.listaLIVORetorno = new[] { li };
				yield return XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(loteli);
			}
		}
	}
}
