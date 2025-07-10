namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CFSLoadListConsolCreatorTest : CFSRecordCreatorTest
	{
		public void TestCreateNewConsolFromContainer()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddContainerLine(header, underbond, TestContainerNumber1);

			CFSLoadListConsolCreator creator = new CFSLoadListConsolCreator(outturn, Factory);
			AssertNotNull("Failed to create Consol", creator.Consol);

			AssertEquals("Consols LloydsNumber", VesselFromLloyds(TestLloydsNum), creator.Consol.JK_JX_JV_NKVessel);
			AssertEquals("Consols Voyage", TestVoyageNum, creator.Consol.JK_JX_JV_VoyageFlight);
		}

		public void TestCreateNewConsolFromShipmentLine()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			CFSLoadListConsolCreator creator = new CFSLoadListConsolCreator(outturn, Factory);
			AssertNotNull("Failed to create Consol", creator.Consol);

			AssertEquals("Consols LloydsNumber", VesselFromLloyds(TestLloydsNum), creator.Consol.JK_JX_JV_NKVessel);
			AssertEquals("Consols Voyage", TestVoyageNum, creator.Consol.JK_JX_JV_VoyageFlight);
		}
	}
}
