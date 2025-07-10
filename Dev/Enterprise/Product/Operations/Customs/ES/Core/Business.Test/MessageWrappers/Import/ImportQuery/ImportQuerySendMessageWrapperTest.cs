using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ImportQuerySendMessageWrapper))]
	class ImportQuerySendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<ImportQuerySendMessageWrapper>
	{
		public void TestRequestATCData()
		{
			AssertEquals("Expected empty RequestATCData", ZString.Empty, wrapper.RequestATCData);
		}

		protected override ImportQuerySendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ImportQuerySendMessageWrapper(cusEntryHeader, certificateData);
	}
}
