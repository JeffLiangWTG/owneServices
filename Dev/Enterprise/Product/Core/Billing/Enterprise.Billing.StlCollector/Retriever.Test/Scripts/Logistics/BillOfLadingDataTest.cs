using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(BillOfLadingData))]
	sealed class BillOfLadingDataTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2024, 12, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
--RefShippingLine
INSERT INTO dbo.RefShippingLine (RSL_PK, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_CarrierName)
VALUES
	('4710E73B-06DB-4FDD-A02C-B17280858509', 'SCAC', 'C1AC', 'DHL01');

--OrgHeader
INSERT INTO [dbo].[OrgHeader] ([OH_PK], [OH_Code], [OH_FullName], [OH_RSL_ShippingLine], [OH_IsShippingLine])
VALUES
	('1E52F9A6-3C16-41A3-980B-B8BE991B55D4', 'OH1', 'Sending Forwarder', NULL, 0),
	('24B52ED5-A84A-4375-9F2D-0682C2A06B3F', 'OH2', 'Receiving Forwarder', NULL, 0),
	('74B748C5-56D9-48FF-A31B-1D693D244267', 'OH3', 'Carrier', '4710E73B-06DB-4FDD-A02C-B17280858509', 1);

--OrgAddress
INSERT INTO [dbo].[OrgAddress] ([OA_PK], [OA_OH], [OA_Address1])
VALUES
	('F5D4E252-5B04-417D-9A3F-59B3554D9E5D', '1E52F9A6-3C16-41A3-980B-B8BE991B55D4', 'Address1'),
	('B002AB72-C325-4BCA-A533-F862C1B9F41A', '24B52ED5-A84A-4375-9F2D-0682C2A06B3F', 'Address2'),
	('84B4C000-DDFE-4B44-B7F6-1A6144F18BE1', '74B748C5-56D9-48FF-A31B-1D693D244267', 'Address3');

--GlbCompany
INSERT INTO [dbo].[GlbCompany] ([GC_PK], [GC_Code], [GC_Name], [GC_OH_OrgProxy])
VALUES
	('98FBB8DB-A6EA-4CF2-8E0F-36B3390BAD13', 'SFC', 'Sending Forwarder Compnay', '1E52F9A6-3C16-41A3-980B-B8BE991B55D4'),
	('8A1411EF-0505-4F04-81AF-8C2C311A3C6E', 'RFC', 'Receiving Forwarder Company', '24B52ED5-A84A-4375-9F2D-0682C2A06B3F');

--GlbBranch
INSERT INTO [dbo].[GlbBranch] ([GB_PK], [GB_Code], [GB_BranchName], [GB_GC], [GB_OH_OrgProxy])
VALUES
	('EB97984A-72E6-4108-B3AE-C0334141C2DC', 'SFB', 'Sending Forwarder Branch', '98FBB8DB-A6EA-4CF2-8E0F-36B3390BAD13', '1E52F9A6-3C16-41A3-980B-B8BE991B55D4'),
	('F14DCA78-EE06-46C8-BA54-BC8382DE4B81', 'RFB', 'Receiving Forwarder Branch', '8A1411EF-0505-4F04-81AF-8C2C311A3C6E', '24B52ED5-A84A-4375-9F2D-0682C2A06B3F');

--JobConsol
INSERT INTO [dbo].[JobConsol] ([JK_PK], [JK_UniqueConsignRef], [JK_AgentType], [JK_TransportMode], [JK_ConsolMode], [JK_OA_SendingForwarderAddress], [JK_OA_ReceivingForwarderAddress], [JK_OA_ShippingLineAddress], [JK_RL_NKLoadPort], [JK_RL_NKDischargePort], [JK_MasterBillNum], [JK_CoLoadMasterBill], [JK_BookingReference], [JK_CoLoadBookingReference], [JK_ReleaseType], [JK_NoOriginalBills], [JK_NoCopyBills], [JK_SystemCreateTimeUtc])
VALUES
	('28882099-5818-4A57-A7A0-11446CDDBD3E', 'C00001234', 'CLD', 'SEA', 'FCL', 'F5D4E252-5B04-417D-9A3F-59B3554D9E5D', 'B002AB72-C325-4BCA-A533-F862C1B9F41A', '84B4C000-DDFE-4B44-B7F6-1A6144F18BE1', 'CNSHA', 'USNYC', 'MBL', 'CLD MBL', 'BOOK REF', 'CLD BOOK REF', 'CAD', 5, 3, '2024-12-01 00:00:00');

--JobConsolTransport
INSERT INTO [dbo].[JobConsolTransport] ([JW_PK], [JW_ParentGUID], [JW_ETD], [JW_ETA])
VALUES
	('7498334E-18C1-4F15-868C-D74737A04B12', '28882099-5818-4A57-A7A0-11446CDDBD3E', '2024-12-01 00:00:00', '2024-12-03 00:00:00'),
	('8F05590E-2712-490F-8E47-2BA6E3D8BF94', '28882099-5818-4A57-A7A0-11446CDDBD3E', '2024-12-03 12:00:00', '2024-12-04 12:00:00');

--JobShipment
INSERT INTO [dbo].[JobShipment] ([JS_PK], [JS_UniqueConsignRef], [JS_TransportMode], [JS_PackingMode], [JS_ShipmentType], [JS_HouseBill], [JS_RL_NKOrigin], [JS_RL_NKDestination], [JS_CarrierContractNumber], [JS_ReleaseType], [JS_HouseBillOfLadingType], [JS_NoOriginalBills], [JS_NoCopyBills], [JS_E_DEP], [JS_E_ARV])
VALUES
	('286D283B-1B96-444E-8F4C-6CCAF99C042C', 'S00001234', 'SEA', 'FCL', 'STD', 'HB123456', 'CNABC', 'USA4V', 'AAAH', 'EBL', 'INZ', 5, 3, '2024-12-01 00:00:00', '2024-12-04 12:00:00');

--JobConShipLink
INSERT INTO [dbo].[JobConShipLink] ([JN_PK], [JN_JK], [JN_JS])
VALUES
	('9DAD28A1-99D6-4658-B93D-B798F56523D6', '28882099-5818-4A57-A7A0-11446CDDBD3E', '286D283B-1B96-444E-8F4C-6CCAF99C042C');

--RefUNLOCO
INSERT INTO [dbo].[RefUNLOCO] ([RL_PK], [RL_Code], [RL_PortName])
VALUES
	('11709B8B-E658-49C2-BC78-DBD655D19FFA', 'CNABC', 'China Port'),
	('0BA28094-0224-4282-8699-ADD555882EFD', 'USA4V', 'US Port');
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction = transactions.Single();

			AssertEquals("Company Code", "SFC", transaction.GetCompanyCode());
			AssertEquals("Branch Code", "SFB", transaction.GetBranchCode());
			AssertEquals("Transaction Date Utc", new DateTime(2024, 12, 1, 0, 0, 0), transaction.ServiceOccuredUTC);
			AssertEquals("Transaction Reference 1", "C00001234", transaction.Reference1);
			AssertEquals("Transaction Reference 2", "S00001234", transaction.Reference2);
			AssertEquals("Additioanl Reference",
				"{\"SendingForwarderCode\":\"OH1\"," +
				"\"SendingForwarderName\":\"Sending Forwarder\"," +
				"\"SendingForwarderBranchCode\":\"SFB\"," +
				"\"SendingForwarderBranchName\":\"Sending Forwarder Branch\"," +
				"\"SendingForwarderCompanyCode\":\"SFC\"," +
				"\"SendingForwarderCompanyName\":\"Sending Forwarder Compnay\"," +
				"\"ReceivingForwarderCode\":\"OH2\"," +
				"\"ReceivingForwarderName\":\"Receiving Forwarder\"," +
				"\"ReceivingForwarderBranchCode\":\"RFB\"," +
				"\"ReceivingForwarderBranchName\":\"Receiving Forwarder Branch\"," +
				"\"ReceivingForwarderCompanyCode\":\"RFC\"," +
				"\"ReceivingForwarderCompanyName\":\"Receiving Forwarder Company\"," +
				"\"ConsolNumber\":\"C00001234\"," +
				"\"ConsolCreatedUTC\":\"2024-12-01T00:00:00\"," +
				"\"ConsolType\":\"CLD\"," +
				"\"ConsolTransportMode\":\"SEA\"," +
				"\"ConsolContainerMode\":\"FCL\"," +
				"\"Carrier\":\"Carrier\"," +
				"\"CarrierC1C\":\"C1AC\"," +
				"\"CarrierSCAC\":\"SCAC\"," +
				"\"CBR\":\"BOOK REF\"," +
				"\"CoLoadCBR\":\"CLD BOOK REF\"," +
				"\"ConsolLoadPort\":\"CNSHA\"," +
				"\"ConsolLoadPortName\":\"Shanghai Hongqiao International Apt\"," +
				"\"ConsolDischargePort\":\"USNYC\"," +
				"\"ConsolDischargePortName\":\"New York\"," +
				"\"ConsolETD\":\"2024-12-01T00:00:00\"," +
				"\"ConsolETA\":\"2024-12-04T12:00:00\"," +
				"\"MBL\":\"MBL\"," +
				"\"CoLoadMBL\":\"CLD MBL\"," +
				"\"MBLReleaseType\":\"CAD\"," +
				"\"MBLNumberOfOriginalBills\":5," +
				"\"MBLNumberOfCopyBills\":3," +
				"\"ShipmentETD\":\"2024-12-01T00:00:00\"," +
				"\"ShipmentETA\":\"2024-12-04T12:00:00\"," +
				"\"ShipmentTransportMode\":\"SEA\"," +
				"\"ShipmentContainerMode\":\"FCL\"," +
				"\"ShipmentType\":\"STD\"," +
				"\"ShipmentNumber\":\"S00001234\"," +
				"\"HBL\":\"HB123456\"," +
				"\"HBLReleaseType\":\"EBL\"," +
				"\"HBLType\":\"INZ\"," +
				"\"HBLNumberOfOriginalBills\":5," +
				"\"HBLNumberOfCopyBills\":3," +
				"\"ShipmentOriginCode\":\"CNABC\"," +
				"\"ShipmentOriginName\":\"China Port\"," +
				"\"ShipmentDestinationCode\":\"USA4V\"," +
				"\"ShipmentDestinationName\":\"US Port\"," +
				"\"ShipmentCarrierContractNumber\":\"AAAH\"}",
				transaction.AdditionalRefs);
		}
	}
}
