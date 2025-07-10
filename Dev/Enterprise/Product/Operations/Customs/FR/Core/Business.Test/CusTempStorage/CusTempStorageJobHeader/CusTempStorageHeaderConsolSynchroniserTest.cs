using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageHeaderConsolSynchroniserTest : TestCaseWithFactory
	{
		void SetupSourceConsolAndShipments()
		{
			sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C4321";
			sourceConsol.JK_RL_NKDischargePort = "FR123";
			sourceConsol.JK_RL_NKLoadPort = "GBFXT";
			sourceConsol.JK_TransportMode = "AIR";
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_MasterBillNum = "BOL123";
			sourceConsol.Transports[0].JW_VoyageFlight = "FR123";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);
			sourceConsol.Transports[0].JW_VoyageFlightForBinding = "FRFE";
			sourceConsol.Transports[0].JW_RL_NKLoadPortForBinding = "GBFXT";
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			localClient = org.Addresses.AddNew();
			localClient.OA_Address1 = "A";
			refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "20DC";
			containerA = sourceConsol.Containers.AddNew();
			containerA.JC_ContainerNum = "AAAA1234567";
			containerA.JC_RC = refContainer.PK;
			containerA.JC_SealNum = "S1";
			containerA.JC_AdditionalSealNum = "S2";
			containerB = sourceConsol.Containers.AddNew();
			containerB.JC_ContainerNum = "BBBB1234567";
			containerC = sourceConsol.Containers.AddNew();
			containerC.JC_ContainerNum = "CCCC1234567";

			consignee = org.Addresses.AddNew();
			consignor = org.Addresses.AddNew();
			consignee.OA_Address1 = "B";
			consignor.OA_Address1 = "C";

			principal = org.Addresses.AddNew();
			principal.OA_Address1 = "P";
			sourceConsol.JK_OA_SendingForwarderAddress = principal.PK;

			AddShipment("S0001", "BOOKS", "GBDTE", "VUAUY", consignor, consignee, "T1", "111");
		}

		void AddShipment(ZString shipmentNo, ZString description, ZString origin, ZString destination, OrgAddress shipmentConsignor, OrgAddress shipmentConsignee, ZString ctStatus, ZString houseBill)
		{
			shipment = sourceConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.PK;
			shipment.CreateJobHeaderWithMutex();
			shipment.JobHeader.JH_OA_LocalChargesAddr = localClient.PK;
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_OuterPacks = 26;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_ActualWeight = 35m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = description;
			shipment.JS_MarksAndNumbers = "MARKS";
			shipment.JS_ActualVolume = 38m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_GoodsValue = 1m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 2m;
			shipment.JS_RX_NKInsuranceCurrency = "NZD";
			shipment.JS_CommunityTransitStatus = ctStatus;
			shipment.JS_HouseBill = houseBill;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var pivot = shipment.OuterPackLines.AddNew();
			pivot.JL_JC = containerA.PK;
			pivot.JL_PackageCount = 69;
			pivot.JL_F3_NKPackType = "BOX";
			pivot.JL_MarksAndNumbers = "Marks and numbers";
		}
		protected override void TearDown()
		{
			base.TearDown();
			if (sourceConsol != null)
			{
				foreach (ForwardingShipment shipment in sourceConsol.Shipments)
				{
					if (shipment != null && shipment.Job != null)
					{
						shipment.Job.Dispose();
					}
				}
			}
		}

		public void TestConsolSynchronisationAtHeaderLevel()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "EN-US";

			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSourceConsolAndShipments();

				var cusTempStorageJobHeader = CreateCusTempStorageJobHeader(sourceConsol);
				Factory.Save();

				cusTempStorageJobHeader.ConsolSynchroniser.Synchronise(true);

				AssertEquals("AIR", cusTempStorageJobHeader.SJH_TransportMode);
				AssertEquals("FRFE", cusTempStorageJobHeader.SJH_TransportRegNo);
				AssertEquals("GBFXT", cusTempStorageJobHeader.SJH_RL_NKLoading);
				AssertEquals("C4321", cusTempStorageJobHeader.SJH_ReferenceNumber);
			}
		}
		CusTempStorageJobHeader CreateCusTempStorageJobHeader(ForwardingConsol sourceConsol)
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.SetRelatedBusinessObject(sourceConsol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			return jobHeader;
		}

		ForwardingConsol sourceConsol;
		OrgAddress principal;
		OrgAddress consignee;
		OrgAddress consignor;
		OrgAddress localClient;
		RefContainer refContainer;
		ForwardingContainer containerA;
		ForwardingContainer containerB;
		ForwardingContainer containerC;
		ForwardingShipment shipment;
	}
}
