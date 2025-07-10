using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportHeaderToEntryInstruction()
		{
			PrepareCusCodeDataForTesting();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			header.AMA_ManifestType = "MGI";
			header.RegistrationNumber = "REG001";
			header.RegistrationDate = new ZDateTime(2017, 1, 1);
			header.RegistrationStatus = "NOT";
			header.AMA_CustomsOffice = "CUO";
			header.AMA_DateAtCustomsOffice = new ZDate(2017, 1, 1);
			header.AMA_RL_NKPortOfFirstArrival = "VUVLI";
			header.AMA_Nature = "NA";
			var shippingAgentHeader = Factory.New<OrgHeader>();
			shippingAgentHeader.OH_FullName = "Shipping Agent";
			shippingAgentHeader.OH_Code = "SHA";
			var shippingAgentAddress = Factory.New<OrgAddress>();
			shippingAgentAddress.OA_OH = shippingAgentHeader.PK;
			shippingAgentAddress.OA_Address1 = "S Address1";
			shippingAgentAddress.OA_Address2 = "S Address2";
			shippingAgentAddress.OA_City = "Sydney";
			shippingAgentAddress.OA_State = "NSW";
			shippingAgentAddress.OA_RN_NKCountryCode = "AU";
			header.AMA_OA_ShippingAgent = shippingAgentAddress.PK;
			header.AMA_CarrierCode = "CC0001";

			Factory.SaveForTesting();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);

			var writer = new AsycudaManifestHeaderEntryInstructionDataObjectWriter<AsycudaManifestHeader>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var manifestHeaderEntryInstructionData = writer.GetDataObject(header);

			AssertEquals("MGI", manifestHeaderEntryInstructionData.Style);
			AssertEquals(new ZDateTime(2017, 1, 1), manifestHeaderEntryInstructionData.DateAtCustomsOffice);
			AssertEquals("VUVLI", manifestHeaderEntryInstructionData.FirstArrival.Code);
			var nature = manifestHeaderEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeaderEntryInstructionDataObjectReader.AHC_Nature);
			var placeOfExitCode = manifestHeaderEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == GenAddOnHelper.PlaceOfExitCode);
			AssertNotNull(nature);
			AssertEquals("NA", nature.Value);
			AssertNull(placeOfExitCode);
			AssertEquals(2, manifestHeaderEntryInstructionData.OrganizationAddressCollection.Count);
			var shippingAgent = manifestHeaderEntryInstructionData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ControllingAgent));
			AssertEquals("Shipping Agent", shippingAgent.CompanyName);
			AssertEquals("S Address1", shippingAgent.Address1);
			AssertEquals("S Address2", shippingAgent.Address2);
			AssertEquals("Sydney", shippingAgent.City);
			AssertEquals("NSW", shippingAgent.State);
			AssertEquals("AU", shippingAgent.Country.Code);

			var carrier = manifestHeaderEntryInstructionData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.Carrier));
			AssertEquals(1, carrier.RegistrationNumberCollection.Count);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, carrier.RegistrationNumberCollection[0].Type.Code);
			AssertEquals("CC0001", carrier.RegistrationNumberCollection[0].Value);
		}
	}
}
