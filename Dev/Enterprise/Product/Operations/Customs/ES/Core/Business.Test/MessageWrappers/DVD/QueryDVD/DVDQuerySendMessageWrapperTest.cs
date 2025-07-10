using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DVDQuerySendMessageWrapper))]
	class DVDQuerySendMessageWrapperTest : DVDCommonSendMessageWrapperAbstractTest<DVDQuerySendMessageWrapper>
	{
		public void TestRequestATCData()
		{
			AssertEquals("Expected empty RequestATCData", false, wrapper.RequestATCData);
		}

		protected override DVDQuerySendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DVDQuerySendMessageWrapper(cusEntryHeader, certificateData);
	}
}
