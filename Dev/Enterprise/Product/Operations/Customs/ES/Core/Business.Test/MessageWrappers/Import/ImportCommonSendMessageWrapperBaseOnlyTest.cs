using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ImportCommonSendMessageWrapper))]
	class ImportCommonSendMessageWrapperBaseOnlyTest : ImportCommonSendMessageWrapperAbstractTest<ImportCommonSendMessageWrapper>
	{
		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MovementReferenceNumber, ZDateTime.Today);
			AssertEquals("Expected filled MRN", ExpectedMRNCode, wrapper.MRN);
		}

		protected override ImportCommonSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ImportCommonSendMessageWrapperForTest(cusEntryHeader, certificateData);

		class ImportCommonSendMessageWrapperForTest : ImportCommonSendMessageWrapper
		{
			public ImportCommonSendMessageWrapperForTest(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
			{
			}
		}
	}
}
