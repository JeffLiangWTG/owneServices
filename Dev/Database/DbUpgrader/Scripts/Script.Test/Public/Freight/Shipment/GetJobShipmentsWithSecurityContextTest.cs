using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment.Testing
{
	[TestedType(typeof(GetJobShipmentsWithSecurityContext))]
	class GetJobShipmentsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobShipmentColumns()
		{
			var fromJobShipment = GetColumnNames("SELECT * FROM dbo.JobShipment");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobShipmentsWithSecurityContext(0, 0, 0, 0, '', '')");

			var missingColumns = fromJobShipment.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestShipmentBookings() => AssertShipmentsWithSingleFlag("JS_IsBooking");

		public void TestCFSShipments() => AssertShipmentsWithSingleFlag("JS_IsCFSRegistered");

		public void TestLinerAndAgencyBookings() => AssertShipmentsWithSingleFlag("JS_IsShipping");

		public void TestForwardRegisteredShipments() => AssertShipmentsWithSingleFlag("JS_IsForwardRegistered");

		void AssertShipmentsWithSingleFlag(string flagFieldName)
		{
			var orgPK = TestDataCreator.CreateOrganisation("ORG", "Test Org");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "Address", "Test Address");

			var shipment1PK = TestDataCreator.CreateShipment("Shipment1");
			TestDataCreator.CreateDocAddress(addressPK, "Shipment1 Address", shipment1PK, "JS", "CED");

			var shipment2PK = TestDataCreator.CreateShipment("Shipment2");
			TestDataCreator.CreateDocAddress(addressPK, "Shipment2 Address", shipment2PK, "JS", "CED");

			var relatedAddressesList = new[] { addressPK };
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET {flagFieldName}=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipment1PK}'");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET {flagFieldName}=0, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipment2PK}'");
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList);
			AssertEquals(0, shipments.Count());

			shipments = GetJobShipmentsWithSecurityContext(
				relatedAddressesList: relatedAddressesList,
				hasBookingsRight: flagFieldName == "JS_IsBooking",
				hasCFSShipmentsRight: flagFieldName == "JS_IsCFSRegistered",
				hasShipmentsRight: flagFieldName == "JS_IsForwardRegistered",
				hasLinerAndAgencyBooking: flagFieldName == "JS_IsShipping");
			Assert(shipments.Contains(shipment1PK));
			AssertEquals(1, shipments.Count());

			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET {flagFieldName}=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipment2PK}'");
			shipments = GetJobShipmentsWithSecurityContext(
				relatedAddressesList: relatedAddressesList,
				hasBookingsRight: flagFieldName == "JS_IsBooking",
				hasCFSShipmentsRight: flagFieldName == "JS_IsCFSRegistered",
				hasShipmentsRight: flagFieldName == "JS_IsForwardRegistered",
				hasLinerAndAgencyBooking: flagFieldName == "JS_IsShipping");
			Assert(shipments.Contains(shipment1PK));
			Assert(shipments.Contains(shipment2PK));
			AssertEquals(2, shipments.Count());
		}

		public void TestShipmentsWithAllowedAddresses()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var shipmentWithCEDPK = CreateShipmentWithDocAddress(address1PK, "CED");
			var shipmentWithCRDPK = CreateShipmentWithDocAddress(address2PK, "CRD");
			var shipmentWithNPPPK = CreateShipmentWithDocAddress(address1PK, "NPP");
			var shipmentWithN2DPK = CreateShipmentWithDocAddress(address2PK, "N2D");
			var shipmentWithN3DPK = CreateShipmentWithDocAddress(address1PK, "N3D");
			var shipmentWithBKDPK = CreateShipmentWithDocAddress(address2PK, "BKD");
			var shipmentWithSCPPK = CreateShipmentWithDocAddress(address1PK, "SCP");
			var shipmentWithPAGPK = CreateShipmentWithDocAddress(address1PK, "PAG");

			var shipmentOtherPK = TestDataCreator.CreateShipment("Shipment 1");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "CED");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "CRD");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "NPP");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "N2D");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "N3D");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "BKD");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "SCP");
			TestDataCreator.CreateDocAddress(address3PK, "Shipment Address", shipmentOtherPK, "JS", "PAG");
			TestDataCreator.CreateDocAddress(address1PK, "Shipment Address", shipmentOtherPK, "JS", "OT1");
			TestDataCreator.CreateDocAddress(address2PK, "Shipment Address", shipmentOtherPK, "JS", "OT2");

			TestConnection.ExecuteNonQuery("UPDATE dbo.JobShipment SET JS_IsForwardRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP'");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, hasShipmentsRight: true);
			Assert(shipments.Contains(shipmentWithCEDPK));
			Assert(shipments.Contains(shipmentWithCRDPK));
			Assert(shipments.Contains(shipmentWithNPPPK));
			Assert(shipments.Contains(shipmentWithN2DPK));
			Assert(shipments.Contains(shipmentWithN3DPK));
			Assert(shipments.Contains(shipmentWithBKDPK));
			Assert(shipments.Contains(shipmentWithSCPPK));
			Assert(shipments.Contains(shipmentWithPAGPK));
			AssertEquals(8, shipments.Count());
		}

		Guid CreateShipmentWithDocAddress(Guid addressPK, string addressType)
		{
			var shipmentPK = TestDataCreator.CreateShipment($"Shipment {addressType}{++shipmentCounter}");
			TestDataCreator.CreateDocAddress(addressPK, $"Shipment {addressType} Address", shipmentPK, "JS", addressType);

			return shipmentPK;
		}

		public void TestShipmentsWithJobHeaders()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var shipmentAgentCollect1PK = CreateShipmentWithJob("JH_OA_AgentCollectAddr", address1PK, branchPK, companyPK, departmentPK);
			var shipmentAgentCollect2PK = CreateShipmentWithJob("JH_OA_AgentCollectAddr", address2PK, branchPK, companyPK, departmentPK);
			CreateShipmentWithJob("JH_OA_AgentCollectAddr", address3PK, branchPK, companyPK, departmentPK);

			var shipmentLocalCharges1PK = CreateShipmentWithJob("JH_OA_LocalChargesAddr", address1PK, branchPK, companyPK, departmentPK);
			var shipmentLocalCharges2PK = CreateShipmentWithJob("JH_OA_LocalChargesAddr", address2PK, branchPK, companyPK, departmentPK);
			CreateShipmentWithJob("JH_OA_LocalChargesAddr", address3PK, branchPK, companyPK, departmentPK);

			var shipmentPK = TestDataCreator.CreateShipment($"Shipment{++shipmentCounter}");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, shipmentPK, departmentPK, "JS", "Parent", "Reference", "WRK");

			TestConnection.ExecuteNonQuery("UPDATE dbo.JobShipment SET JS_IsForwardRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP'");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, hasShipmentsRight: true);
			Assert(shipments.Contains(shipmentAgentCollect1PK));
			Assert(shipments.Contains(shipmentAgentCollect2PK));
			Assert(shipments.Contains(shipmentLocalCharges1PK));
			Assert(shipments.Contains(shipmentLocalCharges2PK));
			AssertEquals(4, shipments.Count());
		}

		Guid CreateShipmentWithJob(string addressFieldName, Guid addressPK, Guid branchPK, Guid companyPK, Guid departmentPK)
		{
			var shipmentPK = TestDataCreator.CreateShipment($"Shipment{++shipmentCounter}");
			var jobPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, shipmentPK, departmentPK, "JS", $"Parent{shipmentCounter}", $"Reference{shipmentCounter}", "WRK");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobHeader SET {addressFieldName}='{addressPK}', JH_SystemLastEditTimeUtc = GETUTCDATE(), JH_SystemLastEditUser = '~BP' WHERE JH_PK='{jobPK}'");

			return shipmentPK;
		}

		public void TestShipmentsWithJobConsols()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var shipmentSendingForwarder1PK = CreateShipmentWithJobConsolWithAddress("JK_OA_SendingForwarderAddress", address1PK);
			var shipmentSendingForwarder2PK = CreateShipmentWithJobConsolWithAddress("JK_OA_SendingForwarderAddress", address2PK);
			CreateShipmentWithJobConsolWithAddress("JK_OA_SendingForwarderAddress", address3PK);

			var shipmentReceivingForwarder1PK = CreateShipmentWithJobConsolWithAddress("JK_OA_ReceivingForwarderAddress", address1PK);
			var shipmentReceivingForwarder2PK = CreateShipmentWithJobConsolWithAddress("JK_OA_ReceivingForwarderAddress", address2PK);
			CreateShipmentWithJobConsolWithAddress("JK_OA_ReceivingForwarderAddress", address3PK);

			var shipmentUnpackDepot1PK = CreateShipmentWithJobConsolWithAddress("JK_OA_UnpackDepotAddress", address1PK);
			var shipmentUnpackDepot2PK = CreateShipmentWithJobConsolWithAddress("JK_OA_UnpackDepotAddress", address2PK);
			CreateShipmentWithJobConsolWithAddress("JK_OA_UnpackDepotAddress", address3PK);

			var shipmentPackDepot1PK = CreateShipmentWithJobConsolWithAddress("JK_OA_PackDepotAddress", address1PK);
			var shipmentPackDepot2PK = CreateShipmentWithJobConsolWithAddress("JK_OA_PackDepotAddress", address2PK);
			CreateShipmentWithJobConsolWithAddress("JK_OA_PackDepotAddress", address3PK);

			var shipmentPK = TestDataCreator.CreateShipment("Shipment");
			var consolPK = TestDataCreator.CreateJobConsol("Consol");
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);

			TestConnection.ExecuteNonQuery("UPDATE dbo.JobShipment SET JS_IsForwardRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP'");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, hasShipmentsRight: true);
			Assert(shipments.Contains(shipmentSendingForwarder1PK));
			Assert(shipments.Contains(shipmentSendingForwarder2PK));
			Assert(shipments.Contains(shipmentReceivingForwarder1PK));
			Assert(shipments.Contains(shipmentReceivingForwarder2PK));
			Assert(shipments.Contains(shipmentUnpackDepot1PK));
			Assert(shipments.Contains(shipmentUnpackDepot2PK));
			Assert(shipments.Contains(shipmentPackDepot1PK));
			Assert(shipments.Contains(shipmentPackDepot2PK));
			AssertEquals(8, shipments.Count());
		}

		Guid CreateShipmentWithJobConsolWithAddress(string addressFieldName, Guid addressPK)
		{
			var shipmentPK = TestDataCreator.CreateShipment($"Shipment{++shipmentCounter}");
			var consolPK = TestDataCreator.CreateJobConsol($"Consol{shipmentCounter}");
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobConsol SET {addressFieldName}= '{addressPK}', JK_SystemLastEditTimeUtc = GETUTCDATE(), JK_SystemLastEditUser = '~BP' WHERE JK_PK='{consolPK}'");

			return shipmentPK;
		}

		public void TestBookingWithRatingHeader()
		{
			var orgPK = TestDataCreator.CreateOrganisation("ORG", "Test Org");
			var addrPK = TestDataCreator.CreateAddress(orgPK, "Address", "Test Address");
			var shipmentPK = CreateShipmentWithRatingHeadersWithAddress(addrPK, false);

			var relatedAddressesList = new[] { addrPK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, hasBookingsRight: true).ToList();
			Assert(shipments.Contains(shipmentPK));
			AssertEquals(1, shipments.Count);
		}

		public void TestShipmentWithRatingHeader()
		{
			var orgPK = TestDataCreator.CreateOrganisation("ORG", "Test Org");
			var addrPK = TestDataCreator.CreateAddress(orgPK, "Address", "Test Address");
			CreateShipmentWithRatingHeadersWithAddress(addrPK, true);

			var relatedAddressesList = new[] { addrPK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, hasBookingsRight: true).ToList();
			AssertEquals(0, shipments.Count);
		}

		Guid CreateShipmentWithRatingHeadersWithAddress(Guid addressPk, bool isConvertedToShipment)
		{
			var companyPk = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("QTE00001", companyPk);
			var shipmentPk = TestDataCreator.CreateShipmentWithRatingHeader($"Shipment{++shipmentCounter}", ratingHeaderPk, isConvertedToShipment);
			TestDataCreator.CreateDocAddress(addressPk, "RatingHeader Address", ratingHeaderPk, "TH", "LCA");

			return shipmentPk;
		}

		public void TestShipmentsWithOrgsAndAddresses()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var org3PK = TestDataCreator.CreateOrganisation("ORG3", "Test Org3");

			var shipmentDeliveryAgent1PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_DeliveryAgent", org1PK);
			var shipmentDeliveryAgent2PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_DeliveryAgent", org2PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OH_DeliveryAgent", org3PK);

			var shipmentExportBroker1PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_ExportBroker", org1PK);
			var shipmentExportBroker2PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_ExportBroker", org2PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OH_ExportBroker", org3PK);

			var shipmentImportReleaseDepot1PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OA_ImportReleaseDepot", address1PK);
			var shipmentImportReleaseDepot2PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OA_ImportReleaseDepot", address2PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OA_ImportReleaseDepot", address3PK);

			var shipmentHandledOnBehalfOfForwarder1PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org1PK);
			var shipmentHandledOnBehalfOfForwarder2PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org2PK);
			var shipmentHandledOnBehalfOfForwarder3PK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org3PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org1PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org2PK);
			CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", org3PK);
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_IsCFSRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK IN ('{shipmentHandledOnBehalfOfForwarder1PK}','{shipmentHandledOnBehalfOfForwarder2PK}','{shipmentHandledOnBehalfOfForwarder3PK}')");

			TestConnection.ExecuteNonQuery("UPDATE dbo.JobShipment SET JS_IsBooking=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP'");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var relatedOrgsList = new[] { org1PK, org2PK };
			var shipments = GetJobShipmentsWithSecurityContext(relatedAddressesList: relatedAddressesList, relatedOrgsList: relatedOrgsList, hasBookingsRight: true);
			Assert(shipments.Contains(shipmentDeliveryAgent1PK));
			Assert(shipments.Contains(shipmentDeliveryAgent2PK));
			Assert(shipments.Contains(shipmentExportBroker1PK));
			Assert(shipments.Contains(shipmentExportBroker2PK));
			Assert(shipments.Contains(shipmentImportReleaseDepot1PK));
			Assert(shipments.Contains(shipmentImportReleaseDepot2PK));
			Assert(shipments.Contains(shipmentHandledOnBehalfOfForwarder1PK));
			Assert(shipments.Contains(shipmentHandledOnBehalfOfForwarder2PK));
			AssertEquals(8, shipments.Count());
		}

		Guid CreateShipmentWIthRelatedOrgOrAddress(string fieldName, Guid fieldValue)
		{
			var shipmentPK = TestDataCreator.CreateShipment($"Shipment{++shipmentCounter}");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET {fieldName}='{fieldValue}', JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipmentPK}'");

			return shipmentPK;
		}

		public void TestAllAllowedShipments()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var orgPK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "Address1", "Test Address1");

			var shipmentDeliveryAgentPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_DeliveryAgent", orgPK);
			var shipmentExportBrokerPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_ExportBroker", orgPK);
			var shipmentImportReleaseDepotPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OA_ImportReleaseDepot", addressPK);

			var shipmentHandledOnBehalfOfForwarderPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", orgPK);
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_IsCFSRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipmentHandledOnBehalfOfForwarderPK}'");

			var shipmentSendingForwarderPK = CreateShipmentWithJobConsolWithAddress("JK_OA_SendingForwarderAddress", addressPK);
			var shipmentReceivingForwarderPK = CreateShipmentWithJobConsolWithAddress("JK_OA_ReceivingForwarderAddress", addressPK);
			var shipmentUnpackDepotPK = CreateShipmentWithJobConsolWithAddress("JK_OA_UnpackDepotAddress", addressPK);
			var shipmentPackDepotPK = CreateShipmentWithJobConsolWithAddress("JK_OA_PackDepotAddress", addressPK);

			var shipmentAgentCollectPK = CreateShipmentWithJob("JH_OA_AgentCollectAddr", addressPK, branchPK, companyPK, departmentPK);
			var shipmentLocalChargesPK = CreateShipmentWithJob("JH_OA_LocalChargesAddr", addressPK, branchPK, companyPK, departmentPK);

			var shipmentWithCEDPK = CreateShipmentWithDocAddress(addressPK, "CED");
			var shipmentWithCRDPK = CreateShipmentWithDocAddress(addressPK, "CRD");
			var shipmentWithNPPPK = CreateShipmentWithDocAddress(addressPK, "NPP");
			var shipmentWithN2DPK = CreateShipmentWithDocAddress(addressPK, "N2D");
			var shipmentWithN3DPK = CreateShipmentWithDocAddress(addressPK, "N3D");
			var shipmentWithBKDPK = CreateShipmentWithDocAddress(addressPK, "BKD");
			var shipmentWithSCPPK = CreateShipmentWithDocAddress(addressPK, "SCP");
			var shipmentWithPAGPK = CreateShipmentWithDocAddress(addressPK, "PAG");

			var shipmentForwardRegisteredPK = CreatetShipmentWithSingleFlag("JS_IsForwardRegistered", addressPK);
			var shipmentBookingPK = CreatetShipmentWithSingleFlag("JS_IsBooking", addressPK);
			var shipmentCFSRegisteredPK = CreatetShipmentWithSingleFlag("JS_IsCFSRegistered", addressPK);
			var shipmenShippingPK = CreatetShipmentWithSingleFlag("JS_IsShipping", addressPK);

			var relatedAddressesList = new[] { addressPK };
			var relatedOrgsList = new[] { orgPK };
			var shipments = GetJobShipmentsWithSecurityContext(
				hasShipmentsRight: true,
				hasBookingsRight: true,
				hasCFSShipmentsRight: true,
				hasLinerAndAgencyBooking: true,
				relatedAddressesList: relatedAddressesList,
				relatedOrgsList: relatedOrgsList);

			Assert(shipments.Contains(shipmentDeliveryAgentPK));
			Assert(shipments.Contains(shipmentExportBrokerPK));
			Assert(shipments.Contains(shipmentImportReleaseDepotPK));
			Assert(shipments.Contains(shipmentHandledOnBehalfOfForwarderPK));
			Assert(shipments.Contains(shipmentSendingForwarderPK));
			Assert(shipments.Contains(shipmentReceivingForwarderPK));
			Assert(shipments.Contains(shipmentUnpackDepotPK));
			Assert(shipments.Contains(shipmentPackDepotPK));
			Assert(shipments.Contains(shipmentAgentCollectPK));
			Assert(shipments.Contains(shipmentLocalChargesPK));
			Assert(shipments.Contains(shipmentWithCEDPK));
			Assert(shipments.Contains(shipmentWithCRDPK));
			Assert(shipments.Contains(shipmentWithNPPPK));
			Assert(shipments.Contains(shipmentWithN2DPK));
			Assert(shipments.Contains(shipmentWithN3DPK));
			Assert(shipments.Contains(shipmentWithBKDPK));
			Assert(shipments.Contains(shipmentWithSCPPK));
			Assert(shipments.Contains(shipmentWithPAGPK));
			Assert(shipments.Contains(shipmentForwardRegisteredPK));
			Assert(shipments.Contains(shipmentBookingPK));
			Assert(shipments.Contains(shipmentCFSRegisteredPK));
			Assert(shipments.Contains(shipmenShippingPK));
			AssertEquals(22, shipments.Count());
		}

		public IEnumerable<Guid> CreateAllowedShipments(Guid orgPK, Guid addressPK, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var shipmentDeliveryAgentPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_DeliveryAgent", orgPK);
			var shipmentExportBrokerPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_ExportBroker", orgPK);
			var shipmentImportReleaseDepotPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OA_ImportReleaseDepot", addressPK);

			var shipmentHandledOnBehalfOfForwarderPK = CreateShipmentWIthRelatedOrgOrAddress("JS_OH_HandledOnBehalfOfForwarder", orgPK);
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_IsCFSRegistered=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipmentHandledOnBehalfOfForwarderPK}'");

			var shipmentSendingForwarderPK = CreateShipmentWithJobConsolWithAddress("JK_OA_SendingForwarderAddress", addressPK);
			var shipmentReceivingForwarderPK = CreateShipmentWithJobConsolWithAddress("JK_OA_ReceivingForwarderAddress", addressPK);
			var shipmentUnpackDepotPK = CreateShipmentWithJobConsolWithAddress("JK_OA_UnpackDepotAddress", addressPK);
			var shipmentPackDepotPK = CreateShipmentWithJobConsolWithAddress("JK_OA_PackDepotAddress", addressPK);

			var shipmentAgentCollectPK = CreateShipmentWithJob("JH_OA_AgentCollectAddr", addressPK, branchPK, companyPK, departmentPK);
			var shipmentLocalChargesPK = CreateShipmentWithJob("JH_OA_LocalChargesAddr", addressPK, branchPK, companyPK, departmentPK);

			var shipmentWithDocAddressPK = CreateShipmentWithDocAddress(addressPK, "CED");

			var shipmentForwardRegisteredPK = CreatetShipmentWithSingleFlag("JS_IsForwardRegistered", addressPK);
			var shipmentBookingPK = CreatetShipmentWithSingleFlag("JS_IsBooking", addressPK);
			var shipmentCFSRegisteredPK = CreatetShipmentWithSingleFlag("JS_IsCFSRegistered", addressPK);
			var shipmenShippingPK = CreatetShipmentWithSingleFlag("JS_IsShipping", addressPK);

			return new[]
			{
				shipmentDeliveryAgentPK,
				shipmentExportBrokerPK,
				shipmentImportReleaseDepotPK,
				shipmentSendingForwarderPK,
				shipmentReceivingForwarderPK,
				shipmentUnpackDepotPK,
				shipmentPackDepotPK,
				shipmentAgentCollectPK,
				shipmentLocalChargesPK,
				shipmentWithDocAddressPK,
				shipmentForwardRegisteredPK,
				shipmentBookingPK,
				shipmentCFSRegisteredPK,
				shipmenShippingPK
			};
		}

		Guid CreatetShipmentWithSingleFlag(string flagFieldName, Guid addressPK)
		{
			var shipmentPK = TestDataCreator.CreateShipment($"Shipment{++shipmentCounter}");
			TestDataCreator.CreateDocAddress(addressPK, $"Shipment {shipmentCounter} Address", shipmentPK, "JS", "CED");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET {flagFieldName}=1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK='{shipmentPK}'");

			return shipmentPK;
		}

		IEnumerable<Guid> GetJobShipmentsWithSecurityContext(
			bool hasShipmentsRight = false,
			bool hasBookingsRight = false,
			bool hasCFSShipmentsRight = false,
			bool hasLinerAndAgencyBooking = false,
			IEnumerable<Guid> relatedAddressesList = null,
			IEnumerable<Guid> relatedOrgsList = null)
		{
			var relatedAddresses = relatedAddressesList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();
			var relatedOrgs = relatedOrgsList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();

			using (var command = TestConnection.Command("SELECT * FROM GetJobShipmentsWithSecurityContext(@hasShipmentsRightForShipments, @hasBookingsRightForShipments, @hasCFSShipmentsRightForShipments, @hasLinerAndAgencyBookingForShipments, @addressesListForShipments, @orgsListForShipments)"))
			{
				command.AddParameter("@hasShipmentsRightForShipments", SqlDbType.Bit, hasShipmentsRight);
				command.AddParameter("@hasBookingsRightForShipments", SqlDbType.Bit, hasBookingsRight);
				command.AddParameter("@hasCFSShipmentsRightForShipments", SqlDbType.Bit, hasCFSShipmentsRight);
				command.AddParameter("@hasLinerAndAgencyBookingForShipments", SqlDbType.Bit, hasLinerAndAgencyBooking);

				command.AddParameter("@addressesListForShipments", SqlDbType.NVarChar, string.Join(",", relatedAddresses));
				command.AddParameter("@orgsListForShipments", SqlDbType.NVarChar, string.Join(",", relatedOrgs));

				var data = DataUtils.GetDataTableFromCommand(command);

				return data.AsEnumerable().Select(row => (Guid)row[JobShipmentSchema.Constants.PK]);
			}
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader
					.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipmentCounter = 0;
		}

		int shipmentCounter;
	}
}
