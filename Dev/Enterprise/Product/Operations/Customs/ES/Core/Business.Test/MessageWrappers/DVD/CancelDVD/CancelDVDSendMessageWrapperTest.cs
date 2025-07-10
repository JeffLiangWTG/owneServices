using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CancelDVDSendMessageWrapper))]
	public class CancelDVDSendMessageWrapperTest : DVDCommonSendMessageWrapperAbstractTest<CancelDVDSendMessageWrapper>
	{
		protected override CancelDVDSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new CancelDVDSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
