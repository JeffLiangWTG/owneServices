using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManTranHeadMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			var transportHeader = Factory.New<CusSeaManTranHead>();
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, new CusSeaManTranHeadMessageManagerForTest(transportHeader).SendWheneverPossibleOnceMessagingActive);
		}

		public void TestAllMessageManagers()
		{
			var transportHeader = Factory.New<CusSeaManTranHead>();
			var arrivalPort = transportHeader.Arrivals.AddNew();
			arrivalPort.BA_RL_NKArrivalPort = "AUSYD";
			arrivalPort.CargoLines.AddNew();

			var oceanBill1 = transportHeader.OceanBills.AddNew();
			var oceanBill2 = transportHeader.OceanBills.AddNew();
			oceanBill1.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Empty;
			oceanBill1.Details.AddNew();
			oceanBill1.BO_RL_NKDischargePort = "AUSYD";
			oceanBill2.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Import;
			((ICusUnderbondDependentCollectionParent)transportHeader.OceanBills[0].Details.AddNew()).Underbonds.AddNew();

			var allMessageManagers = new CusSeaManTranHeadMessageManagerForTest(transportHeader).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 5, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CusSeaManTranHeadSEAIARManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(CusSeaManArrivalPortSEAAARManager), allMessageManagers[1].GetType());
			AssertEquals("AllMessageManagers[2]", typeof(CusSeaManArrivalPortCARLSTManager), allMessageManagers[2].GetType());
			AssertEquals("AllMessageManagers[3]", typeof(CusSeaManOBLHeaderSEACRManager), allMessageManagers[3].GetType());
			AssertEquals("AllMessageManagers[4]", typeof(CusUnderbondUBMREQManager), allMessageManagers[4].GetType());
		}

		public void TestCanSendMessagesWhenErrorsOnOtherOceanBill()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;

			var tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_LloydsIMO = "123";
			tranHead.BT_VoyageNum = "321";
			tranHead.BT_VesselName = "ADMIRALENGRACHT";
			tranHead.BT_PortOfLastForeignPortATD = ZDateTime.BrettsBirthday;
			tranHead.BT_RL_NKPortOfLastForeignPort = "NZAKL";

			var obl1 = tranHead.OceanBills.AddNew();
			var obl2 = tranHead.OceanBills.AddNew();
			obl2.Details.AddNew();

			obl1.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Import;
			obl1.BO_OH_Consignee = Factory.NewWithValidTestData<OrgHeader>().PK;
			obl1.BO_OH_Consignor = Factory.NewWithValidTestData<OrgHeader>().PK;
			obl1.BO_RL_NKDestinationPort = "AUSYD";
			obl1.BO_RL_NKDischargePort = "AUSYD";
			obl1.BO_RL_NKLoadPort = "NZAKL";
			obl1.BO_RL_NKOriginPort = "NZAKL";
			obl1.BO_PaymentMethod = CMRMethodsOfPayment.Codes.Collect;

			var detail = obl1.Details.AddNew();
			detail.BD_CargoVolume = 100.0;
			detail.BD_ContainerNumber = "AAAA1111113";
			detail.BD_ContainerSizeOrISOCode = CMRContainerSizes.Codes._20X8X8;
			detail.BD_GoodsDescription = "wang";
			detail.BD_GrossWeight = 100.0;
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			detail.BD_MarksAndNumbers = "mks";
			detail.BD_NetWeight = 100.0;
			detail.BD_NoOfPacks = 1;
			detail.BD_PackType = CMRPackageTypes.Codes.BeerCrate;
			detail.BD_TypeOfContainer = CMRContainerTypes.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo;

			obl2.BO_OH_Consignee = Factory.NewWithValidTestData<OrgHeader>().PK;
			obl2.BO_OH_Consignor = Factory.NewWithValidTestData<OrgHeader>().PK;

			tranHead.Validation.ValidateAll();
			obl2.Validation.ValidateAll();
			AssertEquals("precondition", false, obl1.HasNotifications());
			AssertEquals("precondition", true, obl2.HasMessageErrors());
			AssertEquals("precondition", false, obl2.HasErrors());

			var multiManager = new CusSeaManTranHeadMessageManagerForTest(tranHead);
			var allMessageManagers = multiManager.GetAllMessageManagers();

			foreach (var manager in allMessageManagers)
			{
				var crManager = manager as CusSeaManOBLHeaderSEACRManager;
				if (crManager != null && crManager.BusinessObject == obl1)
				{
					multiManager.CanSendOriginal(new SendsMessagesToCustomsShutterUpperer(true), crManager);
					break;
				}
			}
		}

		sealed class CusSeaManTranHeadMessageManagerForTest : CusSeaManTranHeadMessageManager
		{
			public CusSeaManTranHeadMessageManagerForTest(CusSeaManTranHead transportHeader) : base(transportHeader)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();

			internal new bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend) => base.CanSendOriginal(sender, managersToSend);
		}
	}
}
