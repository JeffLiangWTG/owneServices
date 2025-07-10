using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CertificateLineWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var wrapper = new CertificateLineWrapper(1, invoiceLine);
			AssertEquals("LineNumber", 1, wrapper.LineNumber);
			AssertEquals("NetQuantity", quarantineLine.QL_NetQuantity, wrapper.NetQuantity);
			AssertEquals("NetQuantityUnit", quarantineLine.QL_NetQuantityUnit, wrapper.NetQuantityUnit);
			AssertEquals("ProductCode", quarantineLine.ProductCode, wrapper.ProductCode);

			AssertEquals("ProductDescription", quarantineLine.InvoiceLine.JI_Description, wrapper.ProductDescription);
			quarantineLine.QL_SendHCDesc = false;
			AssertEquals("ProductDescription", ZString.Empty, wrapper.ProductDescription);

			AssertEquals("AdditionalProductDescription", quarantineLine.QL_AddtionalProductDescription, wrapper.AdditionalProductDescription);
			AssertEquals("ExtraCertificates", quarantineLine.QL_ExtraCertificate, wrapper.ExtraCertificates[0]);
			AssertEquals("PackQuantity", (ZDecimal)quarantineLine.QL_OuterPackCount, wrapper.PackQuantity);
			AssertEquals("PackType", quarantineLine.QL_OuterPackType, wrapper.PackType);
			AssertEquals("RFPNumbers count", 2, RFPNumberWrapperTest.GetCount(wrapper.RFPNumbers));

			int i = 0;
			foreach (var rfpNumber in wrapper.RFPNumbers)
			{
				AssertEquals("RFPNumber", invoiceLine.RFPNumbers[i].ZA_RFPNumber, rfpNumber.RFPNumber);
				AssertEquals("RFPLines count", ++i, RFPNumberWrapperTest.GetCount(rfpNumber.RFPLines));

				int j = 1;
				foreach (var rfpLine in rfpNumber.RFPLines)
				{
					AssertEquals("RFPLineNumber", j++, rfpLine.RFPLineNumber);
					AssertEquals("NetLineQuantity", invoiceLine.RFPNumbers[0].ZA_RFPNetQuantity, rfpLine.NetLineQuantity);
					AssertEquals("NetQuantityUnit", invoiceLine.RFPNumbers[0].ZA_RFPQtyUM, rfpLine.NetQuantityUnit);
					AssertEquals("PackQuantity", invoiceLine.RFPNumbers[0].ZA_RFPPackCount, rfpLine.PackQuantity);
					AssertEquals("PackType", invoiceLine.RFPNumbers[0].ZA_RFPPackType, rfpLine.PackType);

					AssertEquals("Containers count", 1, RFPNumberWrapperTest.GetCount(rfpLine.Containers));

					foreach (var container in rfpLine.Containers)
					{
						AssertEquals("Containers count", invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].ContainerNumber, container.ContainerNumber);
						AssertEquals("Containers count", invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].Container.CO_Seal, container.Seal);
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Declaration.CusContainers.AddNew().CO_ContainerNumber = "FESU1234567";
			var container = helper.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "FESU7654321";
			container.CO_Seal = "1234";

			invoiceLine = helper.Line1;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			quarantineLine = invoiceLine.QuarantineExDocLine;
			FillQuarantineExDocLine(quarantineLine);

			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP4321", 1);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP1234", 1);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP4321", 2);
		}

		internal static void FillQuarantineExDocLine(QuarantineExDocLine quarantineExDocLine)
		{
			quarantineExDocLine.QL_NetQuantity = 2080;
			quarantineExDocLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineExDocLine.QL_SendHCDesc = true;
			quarantineExDocLine.InvoiceLine.JI_Description = "DESCRIPTION LINE 1---------------->DESCRIPTION LINE 2";
			quarantineExDocLine.QL_AddtionalProductDescription = "DESCRIPTION LINE 1---------------->DESCRIPTION LINE 2";
			quarantineExDocLine.QL_ExtraCertificate = "E1234";
			quarantineExDocLine.QL_OuterPackCount = 123;
			quarantineExDocLine.QL_OuterPackType = "VR";
			quarantineExDocLine.QL_ProductType = "WHT";
			quarantineExDocLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			quarantineExDocLine.QL_SupplimentaryCode = "GC";
		}

		JobComInvoiceLine invoiceLine;
		QuarantineExDocLine quarantineLine;
	}
}
