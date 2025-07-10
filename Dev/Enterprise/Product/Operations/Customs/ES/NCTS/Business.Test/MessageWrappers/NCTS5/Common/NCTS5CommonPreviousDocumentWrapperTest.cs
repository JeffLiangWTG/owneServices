using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonPreviousDocumentWrapperTest : WrapperHelperTest<NCTS5CommonPreviousDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new NCTS5CommonPreviousDocumentWrapper(null, 0));
		}

		public void TestMeasurementUnitAndQualifier()
		{
			CombineAssertions(() =>
			{
				document.CSI_UnitOfQuantity = "KGM";
				AssertEquals("Expected empty MeasurementUnitAndQualifier when quantity is 0", "KGM", wrapper.MeasurementUnitAndQualifier);

				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected filled MeasurementUnitAndQualifier when quantity is declared", "KGM", wrapper.MeasurementUnitAndQualifier);

				wrapper = new NCTS5CommonPreviousDocumentWrapper(document, 1, calculatedUOM: "NAR");
				AssertEquals("Expected filled MeasurementUnitAndQualifier with the one calculated", "NAR", wrapper.MeasurementUnitAndQualifier);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected filled Quantity", 2.3m, wrapper.Quantity);

				wrapper = new NCTS5CommonPreviousDocumentWrapper(document, 1, calculatedQuantity: 5.5m);
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

				wrapper = new NCTS5CommonPreviousDocumentWrapper(document, 1, calculatedQuantity: 5.5m);
				AssertEquals("Expected true QuantitySpecified when quantity and calculated quantity is 0", true, wrapper.QuantitySpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<CusSupportingInfo>();
			wrapper = new NCTS5CommonPreviousDocumentWrapper(document, 1);
		}

		CusSupportingInfo document;
		NCTS5CommonPreviousDocumentWrapper wrapper;

		protected override NCTS5CommonPreviousDocumentWrapper GetProvider() => wrapper;
	}
}
