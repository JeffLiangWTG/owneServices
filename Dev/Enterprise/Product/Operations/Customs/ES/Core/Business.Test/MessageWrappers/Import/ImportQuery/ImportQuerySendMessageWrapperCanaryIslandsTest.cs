using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ImportQuerySendMessageWrapperCanaryIslands))]
	class ImportQuerySendMessageWrapperCanaryIslandsTest : ImportCommonSendMessageWrapperAbstractTest<ImportQuerySendMessageWrapperCanaryIslands>
	{
		public void TestRequestATCData()
		{
			AssertEquals("Expected filled RequestATCData", "S", wrapper.RequestATCData);
		}

		protected override ImportQuerySendMessageWrapperCanaryIslands GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ImportQuerySendMessageWrapperCanaryIslands(cusEntryHeader, certificateData);
	}
}
