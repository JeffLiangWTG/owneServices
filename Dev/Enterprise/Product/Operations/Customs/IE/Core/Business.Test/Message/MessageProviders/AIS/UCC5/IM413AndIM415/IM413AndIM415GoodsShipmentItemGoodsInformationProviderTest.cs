using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemGoodsInformationProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemGoodsInformationProvider>
	{
		public void TestNetMass()
		{
			SetUpTestData();
			invoiceLine.JI_CustomsQuantity = 12;
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("NetMass", 12m, Provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			SetUpTestData();
			invoiceLine.JI_CustomsSecondQuantity = 12;
			AssertEquals("SupplementaryUnits", 12m, Provider.SupplementaryUnits);
		}

		public void TestGrossMass()
		{
			SetUpTestData();
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals("GrossMass", 12m, Provider.GrossMass);
		}

		public void TestGoodsDescription()
		{
			SetUpTestData();
			invoiceLine.JI_Description = "Description of Goods";
			AssertEquals("Description of Goods", Provider.GoodsDescription);
		}

		public void TestPackaging()
		{
			SetUpTestData();
			declaration.JE_MasterBill = "MB1";
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			var packing = Provider.Packaging;
			AssertType<CargoWise.Customs.IE.MessageContracts.Interfaces.IPackaging[]>(packing);
			AssertEquals("Count", 1, packing.Count);
			AssertSame("Cached", packing, Provider.Packaging);
		}

		public void TestCusCode()
		{
			SetUpTestData();
			invoiceLine.ZG_CusNumber = "61012010";
			AssertEquals("CusCode", "61012010", GetProvider().CusCode);
		}

		public void TestCommodityCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("CommodityCode.CombinedNomenclatureCode", "12345678", Provider.CommodityCode.CombinedNomenclatureCode);
		}

		protected override IM413AndIM415GoodsShipmentItemGoodsInformationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemGoodsInformationProvider(entryLine);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
