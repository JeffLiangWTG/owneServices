using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DVDQuerySendMessageWrapperCanaryIslands))]
	class DVDQuerySendMessageWrapperCanaryIslandsTest : DVDCommonSendMessageWrapperAbstractTest<DVDQuerySendMessageWrapperCanaryIslands>
	{
		public void TestRequestATCData()
		{
			AssertEquals("Expected filled RequestATCData", true, wrapper.RequestATCData);
		}

		protected override DVDQuerySendMessageWrapperCanaryIslands GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DVDQuerySendMessageWrapperCanaryIslands(cusEntryHeader, certificateData);
	}
}
