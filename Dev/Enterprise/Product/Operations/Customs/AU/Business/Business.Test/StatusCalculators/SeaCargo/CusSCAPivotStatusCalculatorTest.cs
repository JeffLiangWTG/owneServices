using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAPivotStatusCalculator))]
	sealed class CusSCAPivotStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "123";
			oceanBill.CB_LloydsIMO = "12345";
			oceanBill.CB_OceanBill = "OC";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN123";
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";

			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "House1";
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			pivot.CV_AssociatedContainer = container.CN_ContainerNumber;
			pivot.CV_CargoStatus = "HLD";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";

			var line = outturnHeader.Outturns.AddNew();
			line.C5_ContainerNumber = "CN123";
			line.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			line.C5_CargoReceiptDate = ZDateTime.Today;
			line.C5_HouseBill = "House1";
			line.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			Factory.Save();

			pivot.StatusCalculator.DeriveStatusNow();
			AssertEquals("HLD", pivot.CV_CargoStatus);
		}

		public void TestDefaultValueShouldNotLoadOceanBillAllUnderbonds()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "123";
			oceanBill.CB_LloydsIMO = "12345";
			oceanBill.CB_OceanBill = "OC";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN123";
			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CN123";
			var underbond = container2.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";

			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "House1";
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			pivot.CV_AssociatedContainer = container.CN_ContainerNumber;
			pivot.CV_CargoStatus = "HLD";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";

			var line = outturnHeader.Outturns.AddNew();
			line.C5_ContainerNumber = "CN123";
			line.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			line.C5_CargoReceiptDate = ZDateTime.Today;
			line.C5_HouseBill = "House1";
			line.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			Factory.Save();

			pivot.StatusCalculator.DeriveStatusNow();
			AssertEquals("NOT", pivot.CV_CargoStatus);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusSCAPivotStatusCalculator(Factory.New<CusSCAPivot>());
	}
}
