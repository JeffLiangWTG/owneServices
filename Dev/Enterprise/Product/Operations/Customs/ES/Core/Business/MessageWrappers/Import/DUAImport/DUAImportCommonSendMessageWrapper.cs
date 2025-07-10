using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class DUAImportCommonSendMessageWrapper : ImportCommonSendMessageWrapper, IDUAImportDataProvider
	{
		public DUAImportCommonSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public ZBool IsCeutaOrMelilla => declaration.ZG_PartialWriteoff;

		public ZBool IsCanary => declaration.DestinationStateIsCanaryIsland;
	}
}
