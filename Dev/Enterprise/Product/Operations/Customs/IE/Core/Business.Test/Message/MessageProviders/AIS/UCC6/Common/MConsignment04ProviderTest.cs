using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MConsignment04ProviderTest : DataProviderTestCase<MConsignment04Provider>
	{
		public void TestIMConsignment04()
		{
			Assert("Should implement IMConsignment04", Provider is IMConsignment04);
		}

		public void TestLocationOfGoods()
		{
			SetUpTestData();
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Type = "A";
			var locationOfGoods = Provider.LocationOfGoods;
			AssertEquals("Has been constructed with entryInstruction", "A", locationOfGoods.TypeOfLocation);
			AssertSame("Cached", locationOfGoods, Provider.LocationOfGoods);
		}

		public void TestArrivalTransportMeans()
		{
			SetUpTestData();
			declaration.JE_TransportMeans = Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			var arrivalTransportMeans = Provider.ArrivalTransportMeans;
			AssertEquals("Type", Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel, arrivalTransportMeans.Type);
			AssertEquals("Id", Customs.Business.TransportTypeList.Codes.Air, arrivalTransportMeans.Id);
			AssertSame("Cached", arrivalTransportMeans, Provider.ArrivalTransportMeans);
		}

		public void TestActiveBorderTransportMeansNationality()
		{
			SetUpTestData();
			declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Ireland;
			AssertEquals(Core.Constants.CountryCodes.Ireland, Provider.ActiveBorderTransportMeansNationality);
		}

		public void TestContainerIndicator()
		{
			SetUpTestData();
			declaration.JE_ContainerMode = "ABC";
			AssertEquals("ContainerIndicator", "ABC", GetProvider().ContainerIndicator);
		}

		public void TestInlandModeOfTransport()
		{
			SetUpTestData();
			AssertInlandModeOfTransport("AIR", "4");
			AssertInlandModeOfTransport("OWN", "9");
		}

		void AssertInlandModeOfTransport(string inputTransportModeInland, string expectedInlandModeOfTransport)
		{
			declaration.JE_TransportModeInland = inputTransportModeInland;
			AssertEquals(
				$"InlandModeOfTransport should output {expectedInlandModeOfTransport} with input JE_TransportModeInland {inputTransportModeInland}",
				expectedInlandModeOfTransport,
				GetProvider().InlandModeOfTransport
			);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			SetUpTestData();
			AssertModeOfTransportAtTheBorder("AIR", "4");
			AssertModeOfTransportAtTheBorder("OWN", "9");
		}

		void AssertModeOfTransportAtTheBorder(string inputTransportMode, string expectedModeOfTransportAtTheBorder)
		{
			declaration.JE_TransportMode = inputTransportMode;
			AssertEquals(
				$"InlandModeOfTransport should output {expectedModeOfTransportAtTheBorder} with input JE_TransportModeInland {inputTransportMode}",
				expectedModeOfTransportAtTheBorder,
				GetProvider().ModeOfTransportAtTheBorder
			);
		}

		public void TestGrossMass()
		{
			SetUpTestData();
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals("GrossMass", 12m, GetProvider().GrossMass);
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 12;
			invoiceLine2.JI_WeightUQ = "KG";
			AssertEquals("GrossMass", 24m, GetProvider().GrossMass);
		}

		public void TestTotalPackageNumber()
		{
			SetUpTestData();
			AssertEquals("TotalPackageNumber", 0, GetProvider().TotalPackageNumber);
			declaration.FilteredInvoiceLines.Add(invoiceLine);
			invoiceLine.PackagesPivot.AddNew();
			var package = invoiceLine.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().FirstOrDefault();
			package.CHC_NumberOfPacks = 23;
			AssertEquals("TotalPackageNumber", 23, GetProvider().TotalPackageNumber);
		}

		public void TestReferenceNumberUCR()
		{
			SetUpTestData();
			declaration.JE_UCR = "ABC";
			AssertEquals("ReferenceNumberUCR", "ABC", GetProvider().ReferenceNumberUCR);
		}

		public void TestTransportEquipments()
		{
			SetUpTestData();

			entryLine.CL_LineNumber = 1;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT1";
			var package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			invoiceLine.PackagesPivot.AddPivotFor(package);

			var transportEquipments = Provider.TransportEquipments;
			AssertEquals("Count", 1, transportEquipments.Count);
			Assert("Element type", transportEquipments.All(x => x is TransportEquipmentProvider));
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			var tra1 = entryInstruction.AdditionalInfos.AddNew();
			tra1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra1.CSI_Code = "TD1";
			tra1.CSI_ReferenceNumber = "REF1";
			tra1.CSI_Description = "Description1";
			var tra2 = entryInstruction.AdditionalInfos.AddNew();
			tra2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra2.CSI_Code = "TD2";
			tra2.CSI_ReferenceNumber = "REF2";
			tra2.CSI_Description = "Description2";

			AssertContainsExactElementsInAnyOrder("TransportDocuments", new[] { "TD1|REF1", "TD2|REF2" }, Provider.TransportDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			Assert("Will be implemented in following work items", true);
		}

		protected override MConsignment04Provider GetProvider()
		{
			SetUpTestData();
			return new MConsignment04Provider(new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
