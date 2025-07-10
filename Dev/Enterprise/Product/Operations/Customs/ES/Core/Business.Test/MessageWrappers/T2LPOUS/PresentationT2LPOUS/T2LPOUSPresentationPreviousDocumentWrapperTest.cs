using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSPresentationPreviousDocumentWrapperTest : WrapperHelperTest<T2LPOUSPresentationPreviousDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new T2LPOUSPresentationPreviousDocumentWrapper(null, ZDecimal.Zero, null, ZInt.Zero));
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("Expected fixed value for MeasurementUnitAndQualifier", "KGMG", wrapper.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected Quantity when CSI_Quantity is not 0 and totalGrossWeightInKGFromInvoiceLine is not 0", 2.3m, wrapper.Quantity);

				document.CSI_Quantity = 0m;
				wrapper = GetWrapper(document, totalGrossWeightInKGFromInvoiceLine: 0);
				AssertEquals("Expected Quantity when CSI_Quantity = 0 and totalGrossWeightInKGFromInvoiceLine = 0", ZDecimal.Zero, wrapper.Quantity);

				document.CSI_PackType = ZString.Empty;
				document.CSI_PackQty = 0;
				wrapper = GetWrapper(document);
				AssertEquals("Expected Quantity when CSI_Quantity = 0 and totalGrossWeightInKGFromInvoiceLine is not 0", 4.5m, wrapper.Quantity);
			});
		}

		public void TestQuantityValueSpecified()
		{
			CombineAssertions(() =>
			{
				document.CSI_Quantity = 2.3m;
				AssertEquals("Expected true QuantityValueSpecified when CSI_Quantity is not 0 and totalGrossWeightInKGFromInvoiceLine is not 0", true, wrapper.QuantityValueSpecified);

				document.CSI_Quantity = 0m;
				wrapper = GetWrapper(document, totalGrossWeightInKGFromInvoiceLine: 0);
				AssertEquals("Expected false QuantityValueSpecified when quantity is 0 and totalGrossWeightInKGFromInvoiceLine is 0", false, wrapper.QuantityValueSpecified);

				document.CSI_PackType = ZString.Empty;
				document.CSI_PackQty = 0;
				wrapper = GetWrapper(document);
				AssertEquals("Expected false QuantityValueSpecified when CSI_Quantity is 0 and totalGrossWeightInKGFromInvoiceLine is not 0", true, wrapper.QuantityValueSpecified);
			});
		}

		public void TestGoodsItemIdentifier()
		{
			document.CSI_LineNo = 1;
			AssertEquals("Expected filled GoodsItemIdentifier", 1, wrapper.GoodsItemIdentifier);
		}

		public void TestPackagingWhenEmptyCSI_PackQtyAndType()
		{
			document.CSI_PackType = ZString.Empty;
			document.CSI_PackQty = 0;
			wrapper = GetWrapper(document);
			var packaging = wrapper.Packaging;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Packaging", packaging);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages when document.CSI_PackType is empty", "KG", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackageswhen document.CSI_PackQty is empty", 10, packaging.NumberOfPackages);
				AssertEquals("Expected true Packaging.NumberOfPackagesValueSpecified", true, packaging.NumberOfPackagesValueSpecified);
			});
		}

		public void TestPackagingWhenEmptyCSI_PackQtyAndBulkType()
		{
			document.CSI_PackType = "VG";
			wrapper = GetWrapper(document);
			var packaging = wrapper.Packaging;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Packaging", packaging);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);
				AssertEquals("Expected TypeOfPackages", "VG", packaging.TypeOfPackages);
				AssertEquals("Expected NumberOfPackages when document.CSI_PackQty is empty and bulk", 0, packaging.NumberOfPackages);
				AssertEquals("Expected false NumberOfPackagesValueSpecified", false, packaging.NumberOfPackagesValueSpecified);
			});
		}

		public void TestPackagingWhenNotEmptyCSI_PackQtyAndType()
		{
			document.CSI_PackQty = 2;
			document.CSI_PackType = "BB";

			wrapper = GetWrapper(document);
			var packaging = wrapper.Packaging;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Packaging", packaging);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages", "BB", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackages", 2, packaging.NumberOfPackages);
				AssertEquals("Expected true Packaging.NumberOfPackagesValueSpecified", true, packaging.NumberOfPackagesValueSpecified);
			});
		}

		public void TestPackaging()
		{
			CombineAssertions(() =>
			{
				document.CSI_PackQty = 20;
				document.CSI_PackType = "CT";
				wrapper = GetWrapper(document);
				var packaging = wrapper.Packaging;
				AssertNotNull("Expected filled Packaging for Qty 20 and type CT", packaging);
				AssertSame("Cached Packaging for Qty 20 and type CT", wrapper.Packaging, packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages for Qty 20 and type CT", "CT", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackages for Qty 20 and type CT", 20, packaging.NumberOfPackages);
				AssertEquals("Expected true Packaging.NumberOfPackagesValueSpecified for Qty 20 and type CT", true, packaging.NumberOfPackagesValueSpecified);

				document.CSI_PackType = "VG";
				wrapper = GetWrapper(document);
				packaging = wrapper.Packaging;
				AssertNotNull("Expected filled Packaging for Qty 20 and type VG", packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages for Qty 20 and type VG", "VG", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackages for Qty 20 and type VG", 20, packaging.NumberOfPackages);
				AssertEquals("Expected false Packaging.NumberOfPackagesValueSpecified when not 0 and type is bulk for Qty 20 and type VG", false, packaging.NumberOfPackagesValueSpecified);

				document.CSI_PackQty = 0;
				wrapper = GetWrapper(document, firstType: "VG", firstQty: 0);
				packaging = wrapper.Packaging;
				AssertNotNull("Expected filled Packaging for Qty 0 and type VG", packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages for Qty 0 and type VG", "VG", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackages for Qty 0 and type VG", 0, packaging.NumberOfPackages);
				AssertEquals("Expected false Packaging.NumberOfPackagesValueSpecified when 0 and bulk for Qty 0 and type VG", false, packaging.NumberOfPackagesValueSpecified);

				document.CSI_PackQty = 0;
				document.CSI_PackType = "CT";
				wrapper = GetWrapper(document, firstQty: 0);
				packaging = wrapper.Packaging;
				AssertNotNull("Expected filled Packaging for Qty 0 and type CT", packaging);
				AssertEquals("Expected filled Packaging.TypeOfPackages for Qty 0 and type CT", "CT", packaging.TypeOfPackages);
				AssertEquals("Expected filled Packaging.NumberOfPackages for Qty 0 and type CT", 0, packaging.NumberOfPackages);
				AssertEquals("Expected true Packaging.NumberOfPackagesValueSpecified when 0 and not bulk for Qty 0 and type CT", true, packaging.NumberOfPackagesValueSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory.SetBulkTypeHelper();

			document = Factory.New<JobDeclaration>().PreviousDocuments.AddNew();
			wrapper = GetWrapper(document);
		}

		PreviousDocument document;
		T2LPOUSPresentationPreviousDocumentWrapper wrapper;

		T2LPOUSPresentationPreviousDocumentWrapper GetWrapper(PreviousDocument doc, decimal totalGrossWeightInKGFromInvoiceLine = 4.5m, string firstType = "KG", int firstQty = 10) => new T2LPOUSPresentationPreviousDocumentWrapper(doc, totalGrossWeightInKGFromInvoiceLine: totalGrossWeightInKGFromInvoiceLine, firstType: firstType, firstQty: firstQty);

		protected override T2LPOUSPresentationPreviousDocumentWrapper GetProvider() => wrapper;
	}
}
