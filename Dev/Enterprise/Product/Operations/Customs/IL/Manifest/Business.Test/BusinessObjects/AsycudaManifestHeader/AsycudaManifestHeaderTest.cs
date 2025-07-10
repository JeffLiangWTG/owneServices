using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using AsycudaPackPackedItemPivotCollection = Enterprise.Customs.ManifestBase.AsycudaPackPackedItemPivotCollection;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestTransportDocumentIL3UpdatedWhenAMA_ManifestNumberIsUpdated()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			var bill = header.Bills.AddNew();
			var transportDocument = bill.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL3");
			AssertNull("There should no IL3 transport document in the new bill", transportDocument);

			header.AMA_ManifestNumber = "123456";
			transportDocument = bill.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL3");
			AssertEquals("There should an IL3 transport document in the new bill", "123456", transportDocument.CSI_ReferenceNumber);

			header.AMA_ManifestNumber = "1234567";
			AssertEquals("The reference number of IL3 transport document should be updated", "1234567", transportDocument.CSI_ReferenceNumber);

			header.AMA_ManifestNumber = "";
			transportDocument = bill.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL3");
			AssertNull("The IL3 transport document should be deleted", transportDocument);
		}

		public void TestTransportDocumentIL1UpdatedWhenAMA_ManifestNumberIsUpdated()
		{
			var factory = Factory;
			var orgHeaderShippingAgent = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressShippingAgent = orgHeaderShippingAgent.MainAddress;
			orgHeaderShippingAgent.OH_Code = "SA";
			orgHeaderShippingAgent.CustomsCodes.AddNew("CCC", "123", "IL");

			var orgHeaderDeclarant = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressDeclarant = orgHeaderDeclarant.MainAddress;
			orgHeaderDeclarant.OH_Code = "DEC";
			orgHeaderDeclarant.CustomsCodes.AddNew("CMP", "456", "IL");

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			header.AMA_TransportMode = "SEA";
			header.AMA_OA_ShippingAgent = orgAddressShippingAgent.PK;
			header.AMA_OA_Declarant = orgAddressDeclarant.PK;

			var bill1 = header.Bills.AddNew();
			bill1.ABL_SequenceNumber = 1;
			bill1.OnSaving();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_SequenceNumber = 2;
			bill2.OnSaving();
			var transportDocument1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			var transportDocument2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			AssertNull("There should be no IL1 transport document in the new bill1", transportDocument1);
			AssertNull("There should be no IL1 transport document in the new bill2", transportDocument2);

			header.AMA_ManifestNumber = "123456";
			transportDocument1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			transportDocument2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			AssertEquals("There should be an IL1 transport document in the new bill1", "I123456A01", transportDocument1.CSI_ReferenceNumber);
			AssertEquals("There should be an IL1 transport document in the new bill2", "I123456A02", transportDocument2.CSI_ReferenceNumber);

			transportDocument1.CSI_ReferenceNumber = "UU41236";
			transportDocument2.CSI_ReferenceNumber = "VV98745";
			transportDocument2.CSI_Status = "OVR";

			header.AMA_ManifestNumber = "223456";
			transportDocument1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			transportDocument2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL1");
			AssertEquals("The reference number of IL1 transport document should be updated", "I123456A01", transportDocument1.CSI_ReferenceNumber);
			AssertEquals("The reference number of IL1 transport document should be updated", "I123456A02", transportDocument2.CSI_ReferenceNumber);
		}

		public void TestPackedItemRelationship()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(AsycudaPackPackedItemPivotCollection.RelationshipType.Many, temporaryStorageHeader.PackedItemRelationship);
		}

		public void TestIsImport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Assert(header.IsImport);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(!header.IsImport);
		}

		public void TestGetDefaultCountryCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Default Country", "IL", header.AMA_RN_NKCountry);
		}

		public void TestCaptions()
		{
			AssertCaptions("AMA_A_ARV", "Estimate Arrival");
			AssertCaptions("RegistrationNumber", "Customs Status");
			AssertCaptions("RegistrationDate", "Status Date");
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertNotNull(header.Validation);
			AssertType<AsycudaManifestHeaderValidation>(header.Validation);
		}

		public void TestLookups()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertNotNull(header.Lookups);
			AssertType<AsycudaManifestHeaderLookups>(header.Lookups);
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions("default values", () =>
			{
				AssertEquals("Agent Type", "AGT", header.AMA_AgentType);
				AssertEquals("Declarant from login branch", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, header.AMA_OA_Declarant);

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				header = Factory.New<AsycudaManifestHeader>();
				AssertEquals("Declarant from Company's Proxy Org", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, header.AMA_OA_Declarant);
			});
		}

		public void TestAMA_OA_Declarant_List()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var listAttribute = header.AMA_OA_DeclarantInfo.GetAttribute<ListAttribute>();
			AssertEquals("Lookups.DeclarantList", listAttribute.ListDataSourceMember);
		}

		public void TestAMA_OA_Declarant_Caption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_OA_DeclarantInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Declarant", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_RN_NKCountry_Caption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_RN_NKCountryInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Country", resourceStringDataAttribute.Caption);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaManifestHeader.Schema.AMA_OA_Declarant };
		}
		public void TestBills()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaBillCollection>(manifestHeader.Bills);
		}

		public void TestGetBillType()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			AssertEquals(typeof(AsycudaBill), manifestHeader.GetBillType());
		}

		public void TestIsAwaitingResponse()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals("Header should indicate that a response is being awaited", true, header.IsAwaitingResponse());
		}

		public void TestMessageSendingConfiguration()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestMessageSendingConfiguration>(manifestHeader.MessageSendingConfiguration);
		}

		public void TestShowTransportMeansTab()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			AssertEquals("When is not Road", false, manifestHeader.ShowTransportMeansTab);
			manifestHeader.AMA_TransportMode = "ROA";
			AssertEquals("When is Road", true, manifestHeader.ShowTransportMeansTab);
		}

		public void TestTransportMeanCollection()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<TransportMeanCollection>(header.TransportMeans);
		}

		public void TestSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.SetParent(consol);

			AssertType<AsycudaManifestHeaderSynchroniser>(header.Synchroniser);
		}

		public void TestNeedPersonsTabCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_TransportMode = "ROA";
			AssertEquals(false, header.NeedPersonsTabCore_Exposed);
		}

		public void TestAssignForwarderSubDealNumberForBills_WhenShippingAgentOrDeclarantOrManifestNumberChanged()
		{
			var factory = Factory;
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestNumber = "123456";
			var bill1 = header.Bills.AddNew();
			var bill233 = header.Bills.AddNew();

			factory.Save();
			AssertEquals("Prerequisite: Bill1 does not have Transport Documents", 0, bill1.TransportDocuments?.Count);
			AssertEquals("Prerequisite: Bill233 does not have Transport Documents", 0, bill233.TransportDocuments?.Count);

			bill1.ABL_SequenceNumber = 1;
			bill233.ABL_SequenceNumber = 233;

			var shippingAgentHeader = factory.NewWithValidTestData<OrgHeader>();
			var shippingAgentAddress = shippingAgentHeader.MainAddress;
			shippingAgentHeader.OH_Code = "SA";
			shippingAgentHeader.CustomsCodes.AddNew("CCC", "123", "IL");
			header.AMA_OA_ShippingAgent = shippingAgentAddress.PK;
			AssertEquals("When declarant is not valid, Bill1 does not have Transport Documents", 0, bill1.TransportDocuments?.Count);
			AssertEquals("When declarant is not valid, Bill233 does not have Transport Documents", 0, bill233.TransportDocuments?.Count);

			header.AMA_OA_ShippingAgent = ZGuid.Empty;
			var declarantHeader = factory.NewWithValidTestData<OrgHeader>();
			var declarantAddress = declarantHeader.MainAddress;
			declarantHeader.OH_Code = "DEC";
			declarantHeader.CustomsCodes.AddNew("CMP", "456", "IL");
			header.AMA_OA_Declarant = declarantAddress.PK;
			AssertEquals("When shipping agent is not valid, Bill1 does not have Transport Documents", 0, bill1.TransportDocuments?.Count);
			AssertEquals("When shipping agent is not valid, Bill233 does not have Transport Documents", 0, bill233.TransportDocuments?.Count);

			header.AMA_OA_ShippingAgent = shippingAgentAddress.PK;
			var bill1ForwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When Bill1 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A01", bill1ForwarderSubDealNumber);
			var bill233ForwarderSubDealNumber = bill233.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When Bill233 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A02", bill233ForwarderSubDealNumber);

			header.AMA_ManifestNumber = ZString.Empty;
			AssertEquals("the Deal Number For bill1 should delete", 0, bill1.TransportDocuments.Count);
			AssertEquals("the Deal Number For bill233 should delete", 0, bill233.TransportDocuments.Count);

			header.Bills.RemoveAndDeleteAll();
			header.AMA_OA_Declarant = ZGuid.Empty;
			header.AMA_OA_Declarant = declarantAddress.PK;

			AssertEquals("Although ShippingAgent and Declarant are valid, we should not create any bill", 0, header.Bills.Count);
		}

		public void TestAssignForwarderSubDealNumberForBills_RecalculateDealNumber_WhenManifestNumberChanged()
		{
			var factory = Factory;
			var headerA = GetNewBusinessObject() as AsycudaManifestHeader;
			headerA.AMA_TransportMode = "SEA";
			headerA.AMA_ManifestNumber = "123456";
			headerA.AMA_RN_NKCountry = "IL";

			var shippingAgentHeader = factory.NewWithValidTestData<OrgHeader>();
			var shippingAgentAddress = shippingAgentHeader.MainAddress;
			shippingAgentHeader.OH_Code = "SA";
			shippingAgentHeader.CustomsCodes.AddNew("CCC", "123", "IL");
			headerA.AMA_OA_ShippingAgent = shippingAgentAddress.PK;

			var declarantHeader = factory.NewWithValidTestData<OrgHeader>();
			var declarantAddress = declarantHeader.MainAddress;
			declarantHeader.OH_Code = "DEC";
			declarantHeader.CustomsCodes.AddNew("CMP", "456", "IL");
			headerA.AMA_OA_Declarant = declarantAddress.PK;

			var bill1 = headerA.Bills.AddNew();
			bill1.ABL_SequenceNumber = 1;
			factory.Save();
			var bill1ForwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When Bill1 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A01", bill1ForwarderSubDealNumber);

			var headerB = GetNewBusinessObject() as AsycudaManifestHeader;
			headerB.AMA_RN_NKCountry = "IL";
			headerB.AMA_JobReference = "headerB";
			headerB.AMA_TransportMode = "SEA";
			headerB.AMA_ManifestNumber = "123456";
			headerB.AMA_OA_Declarant = declarantAddress.PK;
			headerB.AMA_OA_ShippingAgent = shippingAgentAddress.PK;
			factory.Save();

			var billB1 = headerB.Bills.AddNew();
			billB1.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumber = billB1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When billB1 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A02", forwarderSubDealNumber);

			var billB2 = headerB.Bills.AddNew();
			billB2.ABL_SequenceNumber = 2;
			factory.Save();
			forwarderSubDealNumber = billB2.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When billB2 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A03", forwarderSubDealNumber);

			headerB.AMA_ManifestNumber = "654321";
			CombineAssertions("When ManifestNumber of headerB is changed, prefix should be A", () =>
			{
				forwarderSubDealNumber = billB1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
				AssertEquals("When billB1 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A01", forwarderSubDealNumber);

				forwarderSubDealNumber = billB2.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
				AssertEquals("When billB2 SequenceNumber, ShippingAgent, and Declarant are valid", "I123456A02", forwarderSubDealNumber);
			});
		}

		public void TestTransportDocumentUpdatedWhenMasterBillIsUpdated()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			header.AMA_MasterBill = "ROA123";
			var bill1 = header.Bills.AddNew();

			AssertEquals("When the transport mode is road, no Master Bill related Transport Document should exist", 0, bill1.TransportDocuments.Count);

			header.AMA_TransportMode = "SEA";
			header.AMA_MasterBill = "ABC123";
			var bill2 = header.Bills.AddNew();
			CombineAssertions("When the transport mode is not road", () =>
			{
				var transportDocumentMaster_Bill1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC123");
				var transportDocumentMaster_Bill2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC123");
				AssertNotNull("Master Bill related Transport Document should exist for bill1", transportDocumentMaster_Bill1);
				AssertNotNull("Master Bill related Transport Document should exist for bill2", transportDocumentMaster_Bill2);

				header.AMA_MasterBill = "ABC456";
				transportDocumentMaster_Bill1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC456");
				transportDocumentMaster_Bill2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC456");
				AssertNotNull("Master Bill related Transport Document should exist for bill1 after update", transportDocumentMaster_Bill1);
				AssertNotNull("Master Bill related Transport Document should exist for bill2 after update", transportDocumentMaster_Bill2);
			});

			header.AMA_TransportMode = "ROA";
			CombineAssertions("Changing transport mode to road should not delete existing Master and House Transport Documents", () =>
			{
				var transportDocumentMaster_Bill1 = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC456");
				var transportDocumentMaster_Bill2 = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes._704 && s.CSI_ReferenceNumber == "ABC456");
				AssertNotNull(transportDocumentMaster_Bill1);
				AssertNotNull(transportDocumentMaster_Bill2);
			});
		}

		public void TestPropagateDischargePortToChildBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "";
			var bill1 = header.Bills.AddNew();
			AssertEquals("Bill1 DischargePort", "", bill1.ABL_RL_NKPortOfDischarge);

			header.AMA_RL_NKPortOfDischarge = "ILTLV";
			AssertEquals("Bill1 DischargePort", "ILTLV", bill1.ABL_RL_NKPortOfDischarge);
		}

		public void TestZZValidationHelper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ILDatabaseValidationHelper>(header.ZZValidationHelper);
		}

		public void TestUpdateShippingAgentForSea_WhenAMA_OA_CarrierChanged()
		{
			var factory = Factory;
			var (carrierMainAddress, agencyMainAddressForILASH, agencyMainAddressForIL) = InitializeCarrierAndAgencyAddresses(factory);

			var carrierHeader = carrierMainAddress.Header;
			var agencyForILASH = carrierHeader.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForILASH.O5_PortOrCountry = "ILASH";
			agencyForILASH.O5_OA_AgentOfficeAddress = agencyMainAddressForILASH.PK;
			var agencyForIL = carrierHeader.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForIL.O5_PortOrCountry = "IL";
			agencyForIL.O5_OA_AgentOfficeAddress = agencyMainAddressForIL.PK;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";

			CombineAssertions("When Sea", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_RL_NKPortOfDischarge = "ILASH";
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("Shipping agent should be taken from Discharge Port Carrier Agency", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);

				header.AMA_RL_NKPortOfDischarge = "ILHFA";
				header.AMA_OA_Carrier = ZGuid.Empty;
				AssertEquals("Shipping agent should be empty when carrier is empty", ZGuid.Empty, header.AMA_OA_ShippingAgent_ZAddress.OrgPK);

				header.AMA_RL_NKPortOfDischarge = "ILHFA";
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("Shipping agent should be set based on country when no egency for port of discharge", agencyMainAddressForIL.PK, header.AMA_OA_ShippingAgent);

				header.AMA_RL_NKPortOfDischarge = ZString.Empty;
				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("Shipping agent should be set based on country when port of discharge is empty", agencyMainAddressForIL.PK, header.AMA_OA_ShippingAgent);

				carrierHeader.CarrierAppointedAgentPorts_Agency.RemoveAndDelete(agencyForIL);
				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("Shipping agent should be empty if the carrier has no agency for IL or the discharge port", ZGuid.Empty, header.AMA_OA_ShippingAgent_ZAddress.OrgPK);
			});

			CombineAssertions("When not Sea", () =>
			{
				header.AMA_RL_NKPortOfDischarge = "ILASH";
				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("[Pre-condition]", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				AssertEquals("Shipping agent should not change", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);
			});
		}

		public void TestUpdateShippingAgentForSea_ConsolSynchroniser()
		{
			var factory = Factory;
			var (carrierMainAddress, agencyMainAddressForILASH, agencyMainAddressForIL) = InitializeCarrierAndAgencyAddresses(factory);
			InitializeCarrierAndAgencyAddresses(factory);
			var sourceConsol = factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			sourceConsol.Shipments.AddNew();
			var leg1 = sourceConsol.Transports.AddNew("ITMIL", "ILASH");

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			AssertEquals("Expected ShippingAgent to be empty when sea consol, DischargePort is null and carrier is null.", ZGuid.Empty, manifestHeader.AMA_OA_ShippingAgent);

			leg1.JW_RL_NKDiscPort = sourceConsol.JK_RL_NKDischargePort = "ILASH";
			AssertEquals("Expected ShippingAgent to be empty when sea consol, DischargePort is an Israeli Port and carrier is null.", ZGuid.Empty, manifestHeader.AMA_OA_ShippingAgent);
			leg1.JW_OA_CarrierAddress = carrierMainAddress.PK;
			AssertEquals("Expected ShippingAgent to be empty when sea consol, DischargePort is an Israeli Port and carrier has no Agency.", ZGuid.Empty, manifestHeader.AMA_OA_ShippingAgent);
			var agencyForILASH = carrierMainAddress.Header.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForILASH.O5_PortOrCountry = "ILASH";
			agencyForILASH.O5_OA_AgentOfficeAddress = agencyMainAddressForILASH.PK;
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Expected ShippingAgent to be the Agency's main address when sea consol, carrier has an Agency for the Port.", agencyMainAddressForILASH.PK, manifestHeader.AMA_OA_ShippingAgent);

			manifestHeader.AMA_OA_ShippingAgent = ZGuid.Empty;
			leg1.JW_RL_NKDiscPort = sourceConsol.JK_RL_NKDischargePort = "ILHFA";
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Expected ShippingAgent to be empty when sea consol, carrier has no Agency for the Port or PortCountry.", ZGuid.Empty, manifestHeader.AMA_OA_ShippingAgent);

			var agencyForIL = carrierMainAddress.Header.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForIL.O5_PortOrCountry = "IL";
			agencyForIL.O5_OA_AgentOfficeAddress = agencyMainAddressForIL.PK;
			leg1.JW_RL_NKDiscPort = sourceConsol.JK_RL_NKDischargePort = "ILLOD";
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Expected ShippingAgent to be the Agency's main address when sea consol, carrier has an Agency for the country.", agencyMainAddressForIL.PK, manifestHeader.AMA_OA_ShippingAgent);

			manifestHeader.AMA_OA_ShippingAgent = ZGuid.Empty;
			carrierMainAddress.Header.CarrierAppointedAgentPorts_Agency.RemoveAndDelete(agencyForIL);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Expected ShippingAgent to be empty when sea consol, carrier has not an Agency for the Port either country.", ZGuid.Empty, manifestHeader.AMA_OA_ShippingAgent);
		}

		public void TestUpdateShippingAgentForSea_WhenAMA_RL_NKPortOfDischargeChanged()
		{
			var factory = Factory;
			var (carrierMainAddress, agencyMainAddressForILASH, agencyMainAddressForIL) = InitializeCarrierAndAgencyAddresses(factory);

			var carrierHeader = carrierMainAddress.Header;
			var agencyForILASH = carrierHeader.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForILASH.O5_PortOrCountry = "ILASH";
			agencyForILASH.O5_OA_AgentOfficeAddress = agencyMainAddressForILASH.PK;
			var agencyForIL = carrierHeader.CarrierAppointedAgentPorts_Agency.AddNew();
			agencyForIL.O5_PortOrCountry = "IL";
			agencyForIL.O5_OA_AgentOfficeAddress = agencyMainAddressForIL.PK;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";

			CombineAssertions("When Sea", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				header.AMA_RL_NKPortOfDischarge = "ILASH";
				AssertEquals("Shipping agent should be taken from Discharge Port Carrier Agency", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);

				header.AMA_OA_Carrier = ZGuid.Empty;
				header.AMA_RL_NKPortOfDischarge = "ILHFA";
				AssertEquals("Shipping agent should be empty when carrier is empty", ZGuid.Empty, header.AMA_OA_ShippingAgent_ZAddress.OrgPK);

				header.AMA_OA_Carrier = carrierMainAddress.PK;
				header.AMA_RL_NKPortOfDischarge = "ILHFA";
				AssertEquals("Shipping agent should be set based on country when no egency for port of discharge", agencyMainAddressForIL.PK, header.AMA_OA_ShippingAgent);

				header.AMA_OA_Carrier = carrierMainAddress.PK;
				header.AMA_RL_NKPortOfDischarge = ZString.Empty;
				AssertEquals("Shipping agent should be set based on country when port of discharge is empty", agencyMainAddressForIL.PK, header.AMA_OA_ShippingAgent);
			});

			CombineAssertions("When not Sea", () =>
			{
				header.AMA_OA_Carrier = carrierMainAddress.PK;
				header.AMA_RL_NKPortOfDischarge = "ILASH";
				AssertEquals("[Pre-condition]", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				header.AMA_RL_NKPortOfDischarge = "ILHFA";
				AssertEquals("Shipping agent should not change", agencyMainAddressForILASH.PK, header.AMA_OA_ShippingAgent);
			});
		}

		public void TestMarkAsNeedingValidationForTransportMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transportMean = header.TransportMeans.AddNew();
			AssertEquals("IsValid", false, header.LightValidationIsValid);
			AssertEquals("Should validate on save", true, header.ShouldValidateOnSave);
			transportMean.MarkLightValidationAsValidForTesting();
			Assert(transportMean.LightValidationIsValid);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			Assert(!transportMean.LightValidationIsValid);
		}

		public void TestITransportParentTransportSupporter()
		{
			var header = (ITransportParent)GetNewBusinessObject();
			AssertNotNull(header.TransportSupporter);
			AssertType<AsycudaManifestHeaderTransportSupporter>(header.TransportSupporter);
		}

		public void TestIMessageAttachee()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var messageAttachee = (IMessageAttachee)header;
			AssertNotNull(messageAttachee);
			messageAttachee.MessageStatus = "ERR";
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			header.AMA_MessageStatus = "AWT";
			AssertEquals("MessageStatus", "AWT", messageAttachee.MessageStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(AsycudaManifestHeader), propertyName).Caption);
		}

		static OrgAddress CreateOrgHeader(BusinessObjectFactory factory, string code, string regNo)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.CustomsCodes.AddNew("CCC", regNo, "IL");
			return orgHeader.MainAddress;
		}

		static (OrgAddress carrierMainAddress, OrgAddress agencyMainAddressForILASH, OrgAddress agencyMainAddressForIL) InitializeCarrierAndAgencyAddresses(BusinessObjectFactory factory)
		{
			var carrierMainAddress = CreateOrgHeader(factory, "CRR", "123");
			var agencyMainAddressForILASH = CreateOrgHeader(factory, "SAILASH", "124");
			var agencyMainAddressForIL = CreateOrgHeader(factory, "SAIL", "125");
			return (carrierMainAddress, agencyMainAddressForILASH, agencyMainAddressForIL);
		}
	}

	sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool NeedPersonsTabCore_Exposed => base.NeedPersonsTabCore;
	}
}
