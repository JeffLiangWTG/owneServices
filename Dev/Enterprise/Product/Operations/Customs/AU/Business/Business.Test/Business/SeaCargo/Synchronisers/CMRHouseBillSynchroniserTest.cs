using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRHouseBillSynchroniserTest : HouseBillSynchroniserTest
	{
		public void TestSynchronisePrepaidCollect()
		{
			ForwardingConsol consol = CreateFCLConsol();
			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CommonShipment shipment = consol.Shipments.AddNew();

			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();

			HouseBillSynchroniser synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(TestPrepaidCollectOther);
			AssertEquals("HouseBill CA_PrepaidCollectOther", CMRMethodsOfPayment.Codes.PrepaidOnly, houseBill.CA_PrepaidCollectOther);

			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("Collect", CMRMethodsOfPayment.Codes.Collect, houseBill.CA_PrepaidCollectOther);

			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("Prepaid", CMRMethodsOfPayment.Codes.PrepaidOnly, houseBill.CA_PrepaidCollectOther);
		}

		public void TestLCLShipmentToBBK_CMR()
		{
			ForwardingConsol consol = CreateGroupageConsol();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 5;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			CommonContainer lCLContainer = consol.Containers.AddNew();
			lCLContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			lCLContainer.JC_ContainerNum = "CHFU0303220";
			shipment.OuterPackLines[0].SetContainer(lCLContainer.PK);
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("LCL SCA Container", Enterprise.Core.Constants.ContainerModes.LCL, houseBill.Pivot[0].Container.CN_ContainerMode);
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			consol.Containers.RemoveAndDelete(lCLContainer);
			synchroniser.SynchroniseOceanBill();
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("LCL SCA Container", CMRImportCargoTypes.Codes.BreakBulk, houseBill.Pivot[0].Container.CN_ContainerMode);
		}

		#region Implementation

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
			expectedUnpackedOrPackedCode = CMRPackageTypes.Codes.UnpackedOrPacked;
		}

		protected override SeaCargoSynchroniser GetSeaCargoSynchroniser(ForwardingConsol consol)
		{
			return new CMRSeaCargoSynchroniser(consol);
		}

		protected override HouseBillSynchroniser GetHouseBillSynchroniser(CusSCAHouse house, CommonShipment shipment)
		{
			return new CMRHouseBillSynchroniser(house, shipment);
		}

		#endregion
	}
}
