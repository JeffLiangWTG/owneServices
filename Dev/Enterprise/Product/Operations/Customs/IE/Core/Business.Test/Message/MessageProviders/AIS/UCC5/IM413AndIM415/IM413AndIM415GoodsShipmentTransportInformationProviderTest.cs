using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentTransportInformationProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentTransportInformationProvider>
	{
		public void TestIGoodsShipmentTypeTransportInformation()
		{
			Assert("Should implement IGoodsShipmentTypeTransportInformation", Provider is IGoodsShipmentTypeTransportInformation);
		}

		public void TestContainer()
		{
			AssertEquals("No Containers added", "0", Provider.Container);
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT001";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<Customs.Business.NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT001")).IsForInvoiceLine = true;
			AssertEquals("Container added", "1", Provider.Container);
		}

		public void TestInlandBorderTransportMode()
		{
			SetUpTestData();
			CombineAssertions(() =>
			{
				declaration.JE_TransportModeInland = "AIR";
				AssertEquals("JE_TransportModeInland = AIR", "4", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "FIX";
				AssertEquals("JE_TransportModeInland = FIX", "7", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "IWT";
				AssertEquals("JE_TransportModeInland = IWT", "8", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "OWN";
				AssertEquals("JE_TransportModeInland = OWN", "9", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "MAI";
				AssertEquals("JE_TransportModeInland = MAI", "5", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "RAI";
				AssertEquals("JE_TransportModeInland = RAI", "2", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "ROA";
				AssertEquals("JE_TransportModeInland = ROA", "3", Provider.InlandBorderTransportMode);

				declaration.JE_TransportModeInland = "SEA";
				AssertEquals("JE_TransportModeInland = SEA", "1", Provider.InlandBorderTransportMode);
			});
		}

		public void TestArrivalTransportMeansId()
		{
			AssertType<ArrivalTransportMeansProvider>(Provider.ArrivalTransportMeansId);
		}

		public void TestContainerIdentificationNumbers()
		{
			SetUpTestData();

			declaration.CusContainers.AddNew().CO_ContainerNumber = "1234";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "5678";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<Customs.Business.NonPersistentCusContainer>().ForEach(pack => pack.IsForInvoiceLine = true);

			var containers = Provider.ContainerIdentificationNumbers;
			CombineAssertions(() =>
			{
				AssertEquals("Should be 2 containers", 2, containers.Count);
				AssertEquals("First container", "1234", containers.First());
				AssertEquals("Last container", "5678", containers.Last());
			});
		}

		protected override IM413AndIM415GoodsShipmentTransportInformationProvider GetProvider()
		{
			SetUpTestData();

			var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			var headerProvider = new IM413AndIM415GoodsShipmentProvider(entryHeaderWrapper);
			return (IM413AndIM415GoodsShipmentTransportInformationProvider)headerProvider.TransportInformation;
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
