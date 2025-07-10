using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonLineNumberDocumentWrapperTest : WrapperHelperTest<AESCommonLineNumberDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new AESCommonLineNumberDocumentWrapper(null, 0));
		}

		public void TestConstructorWithCodeAndRef()
		{
			var wrapper = new AESCommonLineNumberDocumentWrapper("Code", "Reference", 5);
			AssertEquals("Expected filled Name", "Code", wrapper.Name);
			AssertEquals("Expected filled Number", "Reference", wrapper.Number);
			AssertEquals("Expected filled SequenceNumber", "5", wrapper.SequenceNumber);
			AssertEquals("Expected empty LineNumber", ZString.Empty, wrapper.LineNumber);
		}

		public void TestLineNumber()
		{
			CombineAssertions(() =>
			{
				document.CSI_LineNo = 2;
				AssertEquals("Expected filled LineNumber", "2", wrapper.LineNumber);

				document.CSI_LineNo = 0;
				AssertEquals("Expected empty LineNumber when 0", ZString.Empty, wrapper.LineNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<CusSupportingInfo>();
			wrapper = new AESCommonLineNumberDocumentWrapper(document, 1);
		}

		CusSupportingInfo document;
		AESCommonLineNumberDocumentWrapper wrapper;

		protected override AESCommonLineNumberDocumentWrapper GetProvider() => wrapper;
	}
}
