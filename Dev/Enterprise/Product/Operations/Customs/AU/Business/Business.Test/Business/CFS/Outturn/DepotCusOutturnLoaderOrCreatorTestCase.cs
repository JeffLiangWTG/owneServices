using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DepotCusOutturnLoaderOrCreatorTestCase : TestCaseWithFactory
	{
		public void TestFindMatchingOutturn()
		{
			DepotCusOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn.C5_ContainerNumber = "AAAA1111113";
			outturn.C5_MasterBill = "OBL348938";
			outturn.C5_HouseBill = "HOUSE9394";
			{
				DepotCusOutturn outturn2 = header.Outturns.AddNew();
				outturn2.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
				outturn2.C5_ContainerNumber = "BBBB2222227";
				outturn2.C5_MasterBill = "OBL348938";
				outturn2.C5_HouseBill = "HOUSE9394";
			}
			{
				DepotCusOutturn outturn3 = header.Outturns.AddNew();
				outturn3.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
				outturn3.C5_ContainerNumber = "AAAA1111113";
				outturn3.C5_MasterBill = "OBL348938";
				outturn3.C5_HouseBill = "HOUSE9394";
			}
			{
				DepotCusOutturn outturn4 = header.Outturns.AddNew();
				outturn4.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
				outturn4.C5_ContainerNumber = "AAAA1111113";
				outturn4.C5_MasterBill = "OBL999999";
				outturn4.C5_HouseBill = "HOUSE9394";
			}
			{
				DepotCusOutturn outturn5 = header.Outturns.AddNew();
				outturn5.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
				outturn5.C5_ContainerNumber = "AAAA1111113";
				outturn5.C5_MasterBill = "OBL348938";
				outturn5.C5_HouseBill = "HARBL3949";
			}
			{
				DepotCusOutturn outturn6 = header.Outturns.AddNew();
			}

			AssertEquals(outturn, creator.FindMatchingOutturn());
		}

		[ExpectException(typeof(DepotCusOutturnLoaderOrCreator.FoundLineAlreadyLinked))]
		public void TestFindMatchingOutturnIsLinkedToSomethingElse()
		{
			DepotCusOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn.C5_ContainerNumber = "AAAA1111113";
			outturn.C5_MasterBill = "OBL348938";
			outturn.C5_HouseBill = "HOUSE9394";

			PackUnpackShipment otherShipment = Factory.New<PackUnpackShipment>();
			CFSShipmentWrapper.Load(otherShipment).Outturns.Add(outturn);

			AssertNull(creator.FindMatchingOutturn());
		}

		public void TestCreateOutturn()
		{
			DepotCusOutturn outturn = creator.CreateOutturn();
			AssertEquals("AAAA1111113", outturn.C5_ContainerNumber);
			AssertEquals("OBL348938", outturn.C5_MasterBill);
			AssertEquals("HOUSE9394", outturn.C5_HouseBill);
			AssertEquals(CMRImportCargoTypes.Codes.LessThanContainerLoad, outturn.C5_CargoType);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<CFSLoadListConsol>();
			container = Factory.New<TallyContainer>();
			consol.Containers.Add(container);
			shipment = container.PackUnpackShipments.AddNew();

			CFSTallyContainerWrapper wrapper = CFSTallyContainerWrapper.Load(container);

			header = Factory.New<CusOutturnHeader>();
			//DepotCusOutturn outturn = header.Outturns.AddNew();

			//wrapper.Outturns.Add(outturn);

			creator = new DepotCusOutturnLoaderOrCreator(shipment, header);

			container.JC_ContainerNum = "AAAA1111113";
			consol.JK_MasterBillNum = "OBL348938";
			shipment.JS_HouseBill = "HOUSE9394";
		}

		DepotCusOutturnLoaderOrCreator creator;
		PackUnpackShipment shipment;
		TallyContainer container;
		CFSLoadListConsol consol;
		CusOutturnHeader header;

		#endregion
	}
}
