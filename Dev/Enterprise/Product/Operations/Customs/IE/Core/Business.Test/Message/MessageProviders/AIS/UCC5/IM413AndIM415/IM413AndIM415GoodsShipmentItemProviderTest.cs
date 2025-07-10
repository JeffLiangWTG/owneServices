using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemProvider>
	{
		public void TestIGoodsShipmentItemType()
		{
			Assert("Should implement IGoodsShipmentItemType", Provider is IGoodsShipmentItemType);
		}

		public void TestGoodsItemNumber()
		{
			SetUpTestData();
			entryLine.CL_LineNumber = 1;
			AssertEquals("1", Provider.GoodsItemNumber);

			entryLine.CL_LineNumber = 4;
			AssertEquals("4", Provider.GoodsItemNumber);
		}

		public void TestProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "0456000";
			var procedure = Provider.Procedure;
			CombineAssertions(() =>
			{
				AssertType<ProcedureProvider>(procedure);
				AssertEquals("Requested Procedure", "04", procedure.RequestedProcedure);
				AssertEquals("Previous Procedure", "56", procedure.PreviousProcedure);
			});
		}

		public void TestParties()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var orgAddress = orgHeader.MainAddress;
			invoiceLine.JI_OA_ExporterAddress = orgAddress.PK;
			invoiceLine.SellerDocAddress.E2_AddressOverride = true;
			invoiceLine.SellerDocAddress.E2_GovRegNum = "IETEST";
			invoiceLine.BuyerDocAddress.E2_AddressOverride = true;
			invoiceLine.BuyerDocAddress.E2_GovRegNum = "IETEST";
			var party = GetProvider().Parties;
			CombineAssertions(() =>
			{
				AssertType<IM413AndIM415GoodsShipmentItemTypePartiesProvider>(party);
				AssertEquals("Exporter", "IEE007", party.Exporter.ID);
				AssertEquals("Seller", "IETEST", party.Seller.ID);
				AssertEquals("Buyer", "IETEST", party.Buyer.ID);
			});
		}

		public void TestAdditionalProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "9999999";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "1234567";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "8901234";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "2334333";

			var additionalProcedures = GetProvider().AdditionalProcedure.ToArray();
			CombineAssertions("AdditionalProcedure test.", () =>
			{
				AssertEquals("Additional Procedure count", 4, additionalProcedures.Length);
				AssertEquals("Should have Default procedure", true, additionalProcedures.Any(code => code == "999"));
				AssertEquals("Should have First additional procedure", true, additionalProcedures.Any(code => code == "567"));
				AssertEquals("Should have Second additional procedure", true, additionalProcedures.Any(code => code == "234"));
				AssertEquals("Should have Third additional procedure", true, additionalProcedures.Any(code => code == "234"));
			});
		}

		public void TestDocumentsAuthorisations()
		{
			AssertType<IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider>(Provider.DocumentsAuthorisations);
		}

		public void TestValuationInformation()
		{
			AssertType<IM413AndIM415GoodsShipmentItemValuationInformationProvider>(Provider.ValuationInformation);
		}

		public void TestGoodsInformation()
		{
			AssertType<IM413AndIM415GoodsShipmentItemGoodsInformationProvider>(Provider.GoodsInformation);
		}

		public void TestContainerIdentificationNumbers()
		{
			SetUpTestData();

			declaration.CusContainers.AddNew().CO_ContainerNumber = "1234";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "5678";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().ForEach(pack => pack.IsForInvoiceLine = true);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var containers = Provider.ContainerIdentificationNumbers.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, containers.Count);
				AssertEquals("Container 1", "1234", containers[0]);
				AssertEquals("Container 2", "5678", containers[1]);
			});
		}

		public void TestQuotaOrderNumber()
		{
			SetUpTestData();
			invoiceLine.JI_ConcessionOrder = "Qa1";
			AssertEquals("QuotaOrderNumber", "Qa1", Provider.QuotaOrderNumber);
		}

		public void TestTransactionNature()
		{
			AssertNull("Do not send it at Goods Shipment Item level.", Provider.TransactionNature);
		}

		public void TestStatisticalValue()
		{
			SetUpTestData();
			entryLine.CL_StatisticalValue = 22m;
			AssertEquals("StatisticalValue", 22m, Provider.StatisticalValue);
		}

		public void TestDatesPlaces()
		{
			AssertType<IM413AndIM415GoodsShipmentItemDatesPlacesProvider>(Provider.DatesPlaces);
		}

		protected override IM413AndIM415GoodsShipmentItemProvider GetProvider()
		{
			SetUpTestData();

			var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			var headerProvider = new IM413AndIM415GoodsShipmentProvider(entryHeaderWrapper);

			return (IM413AndIM415GoodsShipmentItemProvider)headerProvider.GoodsShipmentItem.First();
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
