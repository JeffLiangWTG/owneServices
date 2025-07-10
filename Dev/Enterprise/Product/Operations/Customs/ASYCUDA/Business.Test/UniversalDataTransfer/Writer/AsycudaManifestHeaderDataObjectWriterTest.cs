using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	public partial class AsycudaWriterTest : AsycudaWriterTestHelper
	{
		public void TestExportHeader()
		{
			PrepareCusCodeDataForTesting();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_MasterBill = "MKS23432";
			manifestHeader.AMA_ManifestType = "MGI";
			manifestHeader.RegistrationNumber = "REG001";
			manifestHeader.RegistrationDate = new ZDateTime(2017, 1, 1);
			manifestHeader.RegistrationStatus = "NOT";
			manifestHeader.AMA_CustomsOffice = "CUO";
			manifestHeader.AMA_DateAtCustomsOffice = new ZDate(2017, 1, 1);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "VUVLI";
			manifestHeader.AMA_Nature = "NA";
			manifestHeader.AMA_Trailer1RegNo = "TRAILER001";
			manifestHeader.AMA_Trailer2RegNo = "TRAILER002";
			manifestHeader.AMA_RN_NKTrailer1RegCountry = "TR";
			manifestHeader.AMA_RN_NKTrailer2RegCountry = "ZA";
			manifestHeader.AMA_IsBuyersConsolidation = ZBool.True;
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			manifestHeader.AMA_MasterBillIssueDate = new ZDate(2017, 1, 1);

			var deConsolidatorOrganizationAddress = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsContainerYardAddress);
			var deConsolidator = new OrganisationDataObjectReader(deConsolidatorOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			deConsolidator.OA_Address1 = "1 ATLAS ROAD";
			deConsolidator.OA_PostCode = "1619";
			deConsolidator.OA_Address2 = "JOHANNESBURG INTERNATIONAL AIRPORT";
			deConsolidator.OA_City = "KEMPTON PARK";
			deConsolidator.CompanyName = "ZA DECONSOLIDATOR";

			var dischargeTerminalOrganizationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsContainerTerminalOperatorAddress);
			var dischargeTerminal = new OrganisationDataObjectReader(dischargeTerminalOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			dischargeTerminal.OA_Address1 = "2 ATLAS ROAD";
			dischargeTerminal.OA_PostCode = "1620";
			dischargeTerminal.OA_Address2 = "DURBAN INTERNATIONAL AIRPORT";
			dischargeTerminal.OA_City = "KING SHAKA";
			dischargeTerminal.CompanyName = "ZA TERMINAL";
			manifestHeader.AMA_OA_DeconsolidateAddress = deConsolidator.PK;
			manifestHeader.AMA_OA_DischargeTerminalAddress = dischargeTerminal.PK;
			Factory.SaveForTesting();

			var writer = new AsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals(ApplicationCodeTypeList.Codes.TRETrade, headerData.MessagingApplicationCode.Code);
			AssertEquals("MKS23432", headerData.WayBillNumber);
			AssertEquals(Core.Constants.TransportModes.Air, headerData.TransportMode.Code);
			AssertEquals("AGT", headerData.DeclarantType.Code);
			AssertEquals("Agent", headerData.DeclarantType.Description);
			AssertEquals("CNT", headerData.ContainerMode.Code);
			AssertEquals("Containerized", headerData.ContainerMode.Description);
			AssertEquals(new ZDate(2017, 1, 1), headerData.DateCollection.FirstOrDefault(x => x.Type == DateType.BillIssued).Value);
			AssertEquals("If ticked then code should be true", ZBool.True, headerData.IsBuyersConsol);

			var headerEntryHeaderData1 = headerData.EntryHeaderCollection.FirstOrDefault(x => x.Type.Code.Value == manifestHeader.AMA_RN_NKCountry);
			AssertEquals(1, headerEntryHeaderData1.EntryNumberCollection.Count);
			AssertEquals(CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, headerEntryHeaderData1.EntryNumberCollection[0].Type.Code);
			AssertEquals("REG001", headerEntryHeaderData1.EntryNumberCollection[0].Number);
			AssertEquals(new ZDateTime(2017, 1, 1), headerEntryHeaderData1.EntryNumberCollection[0].IssueDate);
			AssertEquals("NOT", headerEntryHeaderData1.EntryNumberCollection[0].EntryStatus.Code);

			var link1 = headerEntryHeaderData1.EntryInstructionLink;
			var headerEntryInstructionData1 = headerData.EntryInstructionCollection.FirstOrDefault(x => x.Link == link1);
			AssertEquals("MGI", headerEntryInstructionData1.Style);
			AssertEquals(new ZDateTime(2017, 1, 1), headerEntryInstructionData1.DateAtCustomsOffice);
			AssertEquals("VUVLI", headerEntryInstructionData1.FirstArrival.Code);

			var nature = headerEntryInstructionData1.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeaderEntryInstructionDataObjectReader.AHC_Nature);
			var placeOfExitCode = headerEntryInstructionData1.AddInfoCollection.FirstOrDefault(x => x.Key.Value == GenAddOnHelper.PlaceOfExitCode);
			AssertNotNull(nature);
			AssertEquals("NA", nature.Value);
			AssertNull(placeOfExitCode);
			AssertAddressData(headerData.OrganizationAddressCollection, nameof(DocAddressType.CustomsContainerYardAddress), "ZA DECONSOLIDATOR", deConsolidator);
			AssertAddressData(headerData.OrganizationAddressCollection, nameof(DocAddressType.CustomsContainerTerminalOperatorAddress), "ZA TERMINAL", dischargeTerminal);
		}

		public void TestExportVoyageFlightNo_For_Road()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new AsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("REG001", headerData.VoyageFlightNo);
		}

		public void TestExportVoyageFlightNo_For_Sea()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new AsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("VOY001", headerData.VoyageFlightNo);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			setupCreator?.Dispose();
		}
	}
}
