using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonDocumentWrapperTest : WrapperHelperTest<AESCommonDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new AESCommonDocumentWrapper(null, 0));
		}

		public void TestConstructorWithCodeAndRef()
		{
			var wrapper = new AESCommonDocumentWrapper("Code", "Reference", 5);
			AssertEquals("Expected filled Name", "Code", wrapper.Name);
			AssertEquals("Expected filled Number", "Reference", wrapper.Number);
			AssertEquals("Expected filled SequenceNumber", "5", wrapper.SequenceNumber);
			AssertEquals("Expected empty LineNumber", ZString.Empty, wrapper.LineNumber);
			AssertEquals("Expected empty Quantity", ZDecimal.Zero, wrapper.Quantity);
			AssertEquals("Expected false QuantitySpecified", false, wrapper.QuantitySpecified);
			AssertEquals("Expected empty Measurement", ZString.Empty, wrapper.Measurement);
		}

		public void TestMeasurement()
		{
			CombineAssertions(() =>
			{
				document.CSI_UnitOfQuantity = "KGM";
				AssertEquals("Expected filled Measurement", "KGM", wrapper.Measurement);

				wrapper = new AESCommonDocumentWrapper(document, 1, calculatedUOM: "NAR");
				AssertEquals("Expected filled Measurement with the one calculated", "NAR", wrapper.Measurement);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected filled Quantity", 2.3m, wrapper.Quantity);

				wrapper = new AESCommonDocumentWrapper(document, 1, calculatedQuantity: 5.5m);
				AssertEquals("Expected filled Quantity with the one calculated", 5.5m, wrapper.Quantity);
			});
		}

		public void TestQuantitySpecified()
		{
			CombineAssertions(() =>
			{
				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected true QuantitySpecified when quantity is not 0", true, wrapper.QuantitySpecified);

				document.CSI_Quantity = 0m;
				AssertEquals("Expected false QuantitySpecified when quantity is 0", false, wrapper.QuantitySpecified);

				wrapper = new AESCommonDocumentWrapper(document, 1, calculatedQuantity: 5.5m);
				AssertEquals("Expected true QuantitySpecified when quantity and calculated quantity is 0", true, wrapper.QuantitySpecified);
			});
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
			wrapper = new AESCommonDocumentWrapper(document, 1);
		}

		CusSupportingInfo document;
		AESCommonDocumentWrapper wrapper;

		protected override AESCommonDocumentWrapper GetProvider() => wrapper;
	}
}
