using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AnnexSendMessageWrapperTest : WrapperHelperTest<AnnexSendMessageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Document", () => new AnnexSendMessageWrapper(entryHeader, Certificate, null, "A"));
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var header = wrapper.Header;

				AssertNotNull("Expected not null Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		public void TestDocument()
		{
			CombineAssertions(() =>
			{
				var document = wrapper.Document;

				AssertNotNull("Expected not null Document", document);
				AssertSame("Cached Document", wrapper.Document, document);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			document = docFactory.New<StorageDocs>();

			wrapper = new AnnexSendMessageWrapper(entryHeader, Certificate, document, "A");
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		StorageDocs document;
		AnnexSendMessageWrapper wrapper;

		protected override AnnexSendMessageWrapper GetProvider() => wrapper;
	}
}
