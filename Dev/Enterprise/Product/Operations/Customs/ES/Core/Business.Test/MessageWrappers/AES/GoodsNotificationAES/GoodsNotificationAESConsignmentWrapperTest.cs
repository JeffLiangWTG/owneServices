using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class GoodsNotificationAESConsignmentWrapperTest : WrapperHelperTest<GoodsNotificationAESConsignmentWrapper>
	{
		public void TestTransportEquipment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportEquipment", 0, wrapper.TransportEquipment.Count);

				declaration.JE_ContainerMode = "ULD";
				var package1 = declaration.Packages.AddNew();
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "CONT1";
				container1.CO_Seal = "seal11";
				package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
				invoiceLine.PackagesPivot.AddPivotFor(package1);
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				var transportEquipment = wrapper.TransportEquipment;
				AssertEquals("Expected filled TransportEquipment", 1, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);
			});
		}

		public void TestLocationOfGoods()
		{
			var locationOfGoods = wrapper.LocationOfGoods;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled LocationOfGoods", locationOfGoods);
				AssertSame("Cached LocationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
			});
		}

		public void TestDepartureTransportMeans()
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				declaration.JE_TransportMeans = "00";
				declaration.ZG_Box18TransportID = "identification";
				declaration.ZG_Box18TransportNationality = "ES";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;

				AssertEquals("Expected empty DepartureTransportMeans when CustomsOfficeOfExport and CustomsOffice of Exit are the same (empty)", 0, wrapper.DepartureTransportMeans.Count);

				declaration.JE_CustomsOffice = "ES009999";

				var customsOfficeExit = declaration.CustomsOffices.AddNew();
				customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
				customsOfficeExit.CY_Data = "FR008889";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				var departureTransportMeansObject = departureTransportMeans.First();
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				AssertEquals("Expected empty DepartureTransportMeans when EntryInstruction is B", 0, wrapper.DepartureTransportMeans.Count);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				AssertEquals("Expected filled DepartureTransportMeans when EntryInstruction is A", 1, wrapper.DepartureTransportMeans.Count);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				AssertEquals("Expected empty DepartureTransportMeans when EntryInstruction is C", 0, wrapper.DepartureTransportMeans.Count);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				AssertEquals("Expected filled DepartureTransportMeans when EntryInstruction is Y", 1, wrapper.DepartureTransportMeans.Count);

				declaration.JE_CustomsOffice = "FR008889";
				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				AssertEquals("Expected filled DepartureTransportMeans when customsOfficeExit = JE_CustomsOffice", 0, wrapper.DepartureTransportMeans.Count);
			});
		}

		public void TestROADepartureTransportMeans()
		{
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			declaration.ZG_Box18TransportID = "Box18";

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				declaration.JE_TransportMeans = "00";
				declaration.JE_TransportIDInland = "identification";
				declaration.JE_RN_NKTransportNationalityInland = "ES";

				declaration.JE_CustomsOffice = "ES009999";

				var customsOfficeExit = declaration.CustomsOffices.AddNew();
				customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
				customsOfficeExit.CY_Data = "FR008889";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				var departureTransportMeansObject = departureTransportMeans.First();
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

				declaration.JE_Trailer1RegNo = "identification2";
				declaration.JE_RN_NKTrailer1Nationality = "FR";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 2, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				departureTransportMeansObject = departureTransportMeans.ElementAt(1);
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "2", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "31", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification2", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "FR", departureTransportMeansObject.TransportNationality);

				declaration.JE_Trailer2RegNo = "identification3";
				declaration.JE_RN_NKTrailer2Nationality = "IT";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 3, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				departureTransportMeansObject = departureTransportMeans.ElementAt(2);
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "3", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "31", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification3", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "IT", departureTransportMeansObject.TransportNationality);
			});
		}

		public void TestRAIDepartureTransportMeans()
		{
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				declaration.JE_TransportMeans = "00";
				declaration.ZG_Box18TransportID = "identification";
				declaration.ZG_Box18TransportNationality = "ES";

				declaration.JE_CustomsOffice = "ES009999";

				var customsOfficeExit = declaration.CustomsOffices.AddNew();
				customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
				customsOfficeExit.CY_Data = "FR008889";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				var departureTransportMeansObject = departureTransportMeans.First();
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

				declaration.JE_Trailer1RegNo = "identification2";
				declaration.JE_RN_NKTrailer1Nationality = "FR";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 2, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				departureTransportMeansObject = departureTransportMeans.ElementAt(1);
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "2", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification2", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "FR", departureTransportMeansObject.TransportNationality);

				var inlandTransports1 = declaration.InlandTransports.AddNew();
				inlandTransports1.CY_Data = "identification3";
				inlandTransports1.Nationality = "IT";

				var inlandTransports2 = declaration.InlandTransports.AddNew();
				inlandTransports2.CY_Data = "identification4";
				inlandTransports2.Nationality = "DE";

				wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans", 4, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				departureTransportMeansObject = departureTransportMeans.ElementAt(2);
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "3", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification3", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "IT", departureTransportMeansObject.TransportNationality);

				departureTransportMeansObject = departureTransportMeans.ElementAt(3);
				AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "4", departureTransportMeansObject.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification4", departureTransportMeansObject.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "DE", departureTransportMeansObject.TransportNationality);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new GoodsNotificationAESConsignmentWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		GoodsNotificationAESConsignmentWrapper wrapper;

		protected override GoodsNotificationAESConsignmentWrapper GetProvider() => wrapper;
	}
}
