using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESSupportingDocumentWrapperTest : WrapperHelperTest<ComplXAESSupportingDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new ComplXAESSupportingDocumentWrapper(null, 0));
		}

		public void TestCommonSupportingDocumentExtraFields()
		{
			var commonSupportingDocument = wrapper.CommonSupportingDocumentExtraFields;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled CommonSupportingDocument", commonSupportingDocument);
				AssertSame("Cached CommonSupportingDocument", wrapper.CommonSupportingDocumentExtraFields, commonSupportingDocument);
			});
		}

		public void TestLineNumber()
		{
			CombineAssertions(() =>
			{
				document.CSI_LineNo = 2;
				document.CSI_ItemNumber = 5;
				AssertEquals("Expected filled LineNumber", "5", wrapper.LineNumber);

				document.CSI_ItemNumber = 0;
				AssertEquals("Expected empty LineNumber when 0", ZString.Empty, wrapper.LineNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = GetWrapper(document, 1);
		}

		SupportingDocument document;
		ComplXAESSupportingDocumentWrapper wrapper;

		ComplXAESSupportingDocumentWrapper GetWrapper(SupportingDocument doc, ZShort seqNum) => new ComplXAESSupportingDocumentWrapper(doc, seqNum);

		protected override ComplXAESSupportingDocumentWrapper GetProvider() => wrapper;
	}
}
