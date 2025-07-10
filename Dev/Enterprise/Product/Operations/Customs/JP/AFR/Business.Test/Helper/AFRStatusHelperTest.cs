using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class AFRStatusHelperTest : TestCaseWithFactory
	{
		AFRStatusHelper StatusHelper
		{
			get { return statusHelper ?? (statusHelper = new AFRStatusHelper()); }
		}
		AFRStatusHelper statusHelper;

		public void TestGetAFRBillStatusForAFRHeader()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals("", StatusHelper.GetAFRBillStatus(header));
			var bill1 = header.Bills.AddNew();
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));

			var bill2 = header.Bills.AddNew();
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));

			bill1.JPB_ReleaseStatus = ZString.Empty;
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));

			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			bill2.JPB_ReleaseStatus = ZString.Empty;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));

			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(header));

			var bill3 = header.Bills.AddNew();
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(header));
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(header));
		}

		public void TestGetAFRBillStatusForConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTTAHB1";
			AssertEquals("There is no AFR done for this consol", string.Empty, StatusHelper.GetAFRBillStatus(consol));

			var header = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill1.JPB_JPH_Header = header.PK;
			bill1.JPB_BillNumber = "HB1";
			AssertEquals("There is no AFR done for this consol", string.Empty, StatusHelper.GetAFRBillStatus(consol));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "JPTKY";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(consol));

			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "OTTAHB2";
			var customsInformation = new ForwardingConsolCustomsInformation(consol2);

			AssertEquals("There is no AFR done for this consol", string.Empty, customsInformation.AFRBillStatus);
			AssertEquals("There is no AFR done for this consol", string.Empty, customsInformation.AFRBillStatusDescription);

			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(consol));

			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, StatusHelper.GetAFRBillStatus(consol));

			var bill2 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill2.JPB_JPH_Header = header.PK;
			bill2.JPB_BillNumber = "HB2";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, StatusHelper.GetAFRBillStatus(consol));

			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(consol));

			bill2.JPB_ReleaseStatus = string.Empty;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(consol));

			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(consol));

			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			AssertEquals(AFRBillCustomsStatusList.Codes.HLD, StatusHelper.GetAFRBillStatus(consol));
		}

		public void TestGetAFRBillStatusForShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";
			AssertEquals("There is no AFR done for this shipment", string.Empty, StatusHelper.GetAFRBillStatus(shipment));

			var header = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill1.JPB_JPH_Header = header.PK;
			bill1.JPB_BillNumber = "XXX";
			AssertEquals(string.Empty, StatusHelper.GetAFRBillStatus(shipment));

			bill1.JPB_BillNumber = "HB1";
			AssertEquals(string.Empty, StatusHelper.GetAFRBillStatus(shipment));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "JPTKY";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill1.JPB_BillNumber = "xxHB1";
			AssertEquals(string.Empty, StatusHelper.GetAFRBillStatus(shipment));

			bill1.JPB_BillNumber = "xxxxHB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill1.JPB_BillNumber = "xxxxHB1";
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, StatusHelper.GetAFRBillStatus(shipment));

			var bill2 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill2.JPB_JPH_Header = header.PK;
			bill2.JPB_BillNumber = "HB1";
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(shipment));

			bill2.JPB_BillNumber = "xxHB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, StatusHelper.GetAFRBillStatus(shipment));

			bill2.JPB_BillNumber = "ttttHB1";
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(shipment));

			bill2.JPB_BillNumber = "ttttHB1";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, StatusHelper.GetAFRBillStatus(shipment));

			bill1.JPB_ReleaseStatus = string.Empty;
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = "JPTKY";
			consol2.Shipments.Add(shipment);
			var header2 = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			header2.JPH_ParentId = consol2.PK;
			header2.JPH_ParentTableCode = consol.TablePrefix;
			var bill3 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill3.JPB_JPH_Header = header2.PK;
			bill3.JPB_BillNumber = "HB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(shipment));

			bill3.JPB_BillNumber = "xxxxHB1";
			AssertEquals(AFRStatusHelper.MultipleStatusCode, StatusHelper.GetAFRBillStatus(shipment));

			bill3.JPB_BillNumber = "xxxHB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			var header3 = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			var bill4 = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill4.JPB_JPH_Header = header3.PK;
			bill4.JPB_BillNumber = "HB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));

			bill4.JPB_BillNumber = "xxxHB1";
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, StatusHelper.GetAFRBillStatus(shipment));
		}

		public void TestGetAFRBillStatusDescription()
		{
			var sourceList = new[] {
				AFRBillCustomsStatusList.Codes.DoNotLoad,
				AFRBillCustomsStatusList.Codes.DoNotUnload,
				AFRBillCustomsStatusList.Codes.HLD,
				AFRBillCustomsStatusList.Codes.NotRegistered,
				AFRBillCustomsStatusList.Codes.Registered,
				AFRBillCustomsStatusList.Codes.NL1,
				AFRBillCustomsStatusList.Codes.NL2,
				AFRBillCustomsStatusList.Codes.NL3,
				AFRBillCustomsStatusList.Codes.NL4,
				AFRBillCustomsStatusList.Codes.NL5,
				AFRBillCustomsStatusList.Codes.ReleasedHold,
				AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad,
				AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload,
				AFRStatusHelper.MultipleStatusCode
			};

			var resultList = new[] {
				AFRBillCustomsStatusList.Descriptions.DoNotLoad,
				AFRBillCustomsStatusList.Descriptions.DoNotUnload,
				AFRBillCustomsStatusList.Descriptions.HLD,
				AFRBillCustomsStatusList.Descriptions.NotRegistered,
				AFRBillCustomsStatusList.Descriptions.Registered,
				AFRBillCustomsStatusList.Descriptions.NL1,
				AFRBillCustomsStatusList.Descriptions.NL2,
				AFRBillCustomsStatusList.Descriptions.NL3,
				AFRBillCustomsStatusList.Descriptions.NL4,
				AFRBillCustomsStatusList.Descriptions.NL5,
				AFRBillCustomsStatusList.Descriptions.ReleasedHold,
				AFRBillCustomsStatusList.Descriptions.ReleasedDoNotLoad,
				AFRBillCustomsStatusList.Descriptions.ReleasedDoNotUnload,
				AFRStatusHelper.MultipleBillsWithDifferentStatuses
			};

			AssertEquals(sourceList.Length, resultList.Length);
			AssertEquals(Factory.GetCachedValue<AFRBillCustomsStatusList>().ToArray().Length + 1, sourceList.Length);

			for (int i = 0; i < sourceList.Length; i++)
			{
				AssertEquals(resultList[i], StatusHelper.GetAFRBillStatusDescription(Factory, sourceList[i]));
			}
		}
	}
}
