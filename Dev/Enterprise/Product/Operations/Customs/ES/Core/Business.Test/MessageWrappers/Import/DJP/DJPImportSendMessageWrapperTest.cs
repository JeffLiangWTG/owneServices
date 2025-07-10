using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DJPImportSendMessageWrapper))]
	class DJPImportSendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<DJPImportSendMessageWrapper>
	{
		public void TestDocuments()
		{
			AssertEquals("Expected empty Documents", 0, wrapper.Documents.Count);
		}

		public void TestDeclarations()
		{
			AssertEquals("Expected 1 Declaration in Declarations", 1, wrapper.Declarations.Count);
		}

		protected override DJPImportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DJPImportSendMessageWrapper(cusEntryHeader, certificateData);

		protected override ZString ExpectedMRNCode => ZString.Empty;
	}
}
