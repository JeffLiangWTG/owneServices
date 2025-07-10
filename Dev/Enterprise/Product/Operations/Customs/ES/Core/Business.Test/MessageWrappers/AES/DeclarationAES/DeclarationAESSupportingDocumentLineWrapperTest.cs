using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESSupportingDocumentLineWrapperTest : WrapperHelperTest<DeclarationAESSupportingDocumentLineWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new DeclarationAESSupportingDocumentLineWrapper(null, 0));
		}

		public void TestConstructorWithCodeAndRef()
		{
			var wrapper = new DeclarationAESSupportingDocumentLineWrapper("Code", "Reference", 5);
			AssertEquals("Expected filled Name", "Code", wrapper.Name);
			AssertEquals("Expected filled Number", "Reference", wrapper.Number);
			AssertEquals("Expected filled SequenceNumber", "5", wrapper.SequenceNumber);
			AssertEquals("Expected empty LineNumber", ZString.Empty, wrapper.LineNumber);
			AssertEquals("Expected empty Quantity", ZDecimal.Zero, wrapper.Quantity);
			AssertEquals("Expected false QuantitySpecified", false, wrapper.QuantitySpecified);
			AssertEquals("Expected empty Measurement", ZString.Empty, wrapper.Measurement);
			AssertNull("Expected empty CommonSupportingDocumentExtraFields", wrapper.CommonSupportingDocumentExtraFields);
		}

		public void TestCommonSupportingDocumentExtraFields()
		{
			var commonSupportingDocument = wrapper.CommonSupportingDocumentExtraFields;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled CommonSupportingDocumentExtraFields", commonSupportingDocument);
				AssertSame("Cached CommonSupportingDocumentExtraFields", wrapper.CommonSupportingDocumentExtraFields, commonSupportingDocument);
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
		DeclarationAESSupportingDocumentLineWrapper wrapper;

		DeclarationAESSupportingDocumentLineWrapper GetWrapper(SupportingDocument doc, ZShort seqNum) => new DeclarationAESSupportingDocumentLineWrapper(doc, seqNum);

		protected override DeclarationAESSupportingDocumentLineWrapper GetProvider() => wrapper;
	}
}
