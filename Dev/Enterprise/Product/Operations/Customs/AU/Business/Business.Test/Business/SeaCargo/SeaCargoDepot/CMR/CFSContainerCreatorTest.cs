using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CFSContainerCreatorTest : CFSRecordCreatorTest
	{
		[ExpectNoExceptions()]
		public void TestMatchExistingContainer()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			DepotCusOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = TestContainerNumber1;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport.JW_VoyageFlight = TestVoyageNum;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = TestContainerNumber1;

			CFSContainerCreator creator = new CFSContainerCreator(outturn, Factory);
			AssertEquals(container, creator.Container);
		}

		public void TestCreateNewContainerFromOutturn()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddContainerLine(header, underbond, TestContainerNumber1);

			CFSContainerCreator creator = new CFSContainerCreator(outturn, Factory);
			AssertNotNull("Failed to create Consol", creator.Consol);
			AssertNotNull("Failed to create Consol", creator.Container);

			AssertEquals("Container Number", TestContainerNumber1, creator.Container.JC_ContainerNum);
		}

		public void TestCreatingNewContainerOnExistingLoadList()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddContainerLine(header, underbond, TestContainerNumber1);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport.JW_VoyageFlight = TestVoyageNum;

			CFSContainer container = new CFSContainerCreator(outturn, consol, Factory).Container;
			AssertEquals("Containers consol", consol, container.Consol);
		}

		public void TestCreateFCXContainerFromOutturn()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddFCXContainerLine(header, underbond, TestContainerNumber1, TestOceanBill1);

			CFSContainerCreator creator = new CFSContainerCreator(outturn, Factory);
			AssertNotNull("Failed to create Consol", creator.Consol);
			AssertEquals("Consol Mode", Enterprise.Core.Constants.ContainerModes.BuyersConsol, creator.Consol.JK_ConsolMode);
			AssertNotNull("Failed to create Consol", creator.Container);
			AssertEquals("Container Mode", Enterprise.Core.Constants.ContainerModes.BuyersConsol, creator.Container.JC_ContainerMode);

			AssertEquals("Container Number", TestContainerNumber1, creator.Container.JC_ContainerNum);
		}
	}
}
