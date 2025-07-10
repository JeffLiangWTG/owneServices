using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CANPreDUAImportSendMessageWrapper))]
	class CANPreDUAImportSendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<CANPreDUAImportSendMessageWrapper>
	{
		protected override CANPreDUAImportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new CANPreDUAImportSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
