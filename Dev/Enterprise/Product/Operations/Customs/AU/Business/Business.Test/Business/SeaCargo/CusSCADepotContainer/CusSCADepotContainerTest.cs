using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCADepotContainer))]
	sealed class CusSCADepotContainerTest : EnterpriseBusinessObjectTestCase
	{
		#region Static Loaders

		public void TestLoad()
		{
			#region Create Test Objects

			var container1 = Factory.New<CusSCADepotContainer>();
			container1.CJ_ContainerNumber = "CONT1111117";
			container1.CJ_LloydsNumber = "12345";
			container1.CJ_Voyage = "123";
			container1.CJ_Status = "FOO";

			var container2 = Factory.New<CusSCADepotContainer>();
			container2.CJ_ContainerNumber = "CONT9999991";
			container2.CJ_LloydsNumber = "12345";
			container2.CJ_Voyage = "123";
			container2.CJ_Status = "FOO";

			var container3 = Factory.New<CusSCADepotContainer>();
			container3.CJ_ContainerNumber = "CONT1111117";
			container3.CJ_LloydsNumber = "54321";
			container3.CJ_Voyage = "123";
			container3.CJ_Status = "FOO";

			var container4 = Factory.New<CusSCADepotContainer>();
			container4.CJ_ContainerNumber = "CONT1111117";
			container4.CJ_LloydsNumber = "12345";
			container4.CJ_Voyage = "321";
			container4.CJ_Status = "FOO";

			var container5 = Factory.New<CusSCADepotContainer>();
			container5.CJ_ContainerNumber = "CONT1111117";
			container5.CJ_LloydsNumber = "12345";
			container5.CJ_Voyage = "123";
			container5.CJ_Status = "BAR";

			#endregion

			AssertEquals(container1, CusSCADepotContainer.Load(Factory, "CONT1111117", "123", "FOO", "12345"));
			AssertEquals(container2, CusSCADepotContainer.Load(Factory, "CONT9999991", "123", "FOO", "12345"));
			AssertEquals(container3, CusSCADepotContainer.Load(Factory, "CONT1111117", "123", "FOO", "54321"));
			AssertEquals(container4, CusSCADepotContainer.Load(Factory, "CONT1111117", "321", "FOO", "12345"));
			AssertEquals(container5, CusSCADepotContainer.Load(Factory, "CONT1111117", "123", "BAR", "12345"));
			AssertNull(CusSCADepotContainer.Load(Factory, "CONT2222222", "123", "FOO", "12345"));
			AssertNull(CusSCADepotContainer.Load(Factory, "CONT1111117", "999", "FOO", "12345"));
			AssertNull(CusSCADepotContainer.Load(Factory, "CONT1111117", "123", "FAP", "12345"));
			AssertNull(CusSCADepotContainer.Load(Factory, "CONT1111117", "123", "FOO", "99999"));
		}

		#endregion

		public void TestOutturnableLines()
		{
			var container = Factory.New<CusSCADepotContainer>();
			AssertEquals(0, container.OutturnableLines.Length);

			var underbond = container.Underbonds.AddNew();
			AssertEquals(0, container.OutturnableLines.Length);

			var outturn = underbond.Outturns.AddNew();
			AssertEquals(0, container.OutturnableLines.Length);

			outturn.Parent = container;
			AssertEquals(1, container.OutturnableLines.Length);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var container = Factory.New<CusSCADepotContainer>();
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)container).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)container).DefaultTranshipmentPort);
		}

		public void TestEquals()
		{
			var container1 = Factory.New<CusSCADepotContainer>();
			var container2 = Factory.New<CusSCADepotContainer>();
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container1.CJ_ContainerNumber = TestContainerNumber1;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_ContainerNumber = TestContainerNumber1;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_ContainerNumber = TestContainerNumber2;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_ContainerNumber = TestContainerNumber1;

			container1.CJ_IsSealOk = false;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_IsSealOk = false;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_IsSealOk = true;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_IsSealOk = false;

			container1.CJ_LloydsNumber = TestLloydsNumber1;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_LloydsNumber = TestLloydsNumber1;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_LloydsNumber = TestLloydsNumber2;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_LloydsNumber = TestLloydsNumber1;

			container1.CJ_PackageCount = 10;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_PackageCount = 10;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_PackageCount = 20;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_PackageCount = 10;

			container1.CJ_SealNumber = TestSealNumber1;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_SealNumber = TestSealNumber1;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_SealNumber = TestSealNumber2;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_SealNumber = TestSealNumber1;

			container1.CJ_Voyage = TestVoyageNumber1;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_Voyage = TestVoyageNumber1;
			AssertEquals("Containers Equal", true, container1.MessageDataEqual(container2));
			container2.CJ_Voyage = TestVoyageNumber2;
			AssertEquals("Containers Not Equal", false, container1.MessageDataEqual(container2));
			container2.CJ_Voyage = TestVoyageNumber1;
		}

		public void TestCanSendWithoutDelay()
		{
			var container = Factory.New<CusSCADepotContainer>();
			Assert(((ICusUnderbondDependentCollectionParent)container).CanSendWithoutDelay);
		}

		public void TestFromContainer()
		{
			var testConsol = Factory.New<CFSLoadListConsol>();
			var testSailing = Factory.New<JobSailing>();
			var testVoyage = Factory.New<JobVoyage>();
			testVoyage.JV_VoyageFlight = TestVoyageNumber1;
			testVoyage.JV_RV_NKVessel = GetOrCreateRefVessel(TestLloydsNumber1).RV_FK;
			var testOrigin = Factory.New<VoyageOrigin>();
			testOrigin.JA_RL_NKPortOfLoading = "USLAX";
			var testDestination = Factory.New<VoyageDestination>();
			testDestination.JB_RL_NKPortOfDischarge = "AUSYD";
			testOrigin.JA_JV = testVoyage.PK;
			testDestination.JB_JV = testVoyage.PK;
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			var transport = testConsol.Transports[0];
			transport.JW_JX = testSailing.PK;

			var testShipment = GetShipment();
			PackLine testPackLine = testShipment.OuterPackLines.AddNew();
			testPackLine.JL_PackageCount = 11;
			CommonContainer testContainer = testConsol.Containers.AddNew();
			testContainer.JC_ContainerNum = TestContainerNumber1;
			testConsol.Shipments.Add(testShipment);
			testPackLine.SetContainer(testSailing, testContainer);
			testContainer.JC_IsSealOk = true;
			testContainer.JC_SealNum = TestSealNumber1;

			var sCDContainer = Factory.New<CusSCADepotContainer>();
			sCDContainer.FromContainer(testContainer);

			AssertEquals("CJ_ContainerNumber ", TestContainerNumber1, sCDContainer.CJ_ContainerNumber);
			AssertEquals("CJ_IsSealOk", true, sCDContainer.CJ_IsSealOk);
			AssertEquals("CJ_LloydsNumber", TestLloydsNumber1, sCDContainer.CJ_LloydsNumber.ToString());
			AssertEquals("CJ_PackageCount", 11, sCDContainer.CJ_PackageCount);
			AssertEquals("CJ_SealNumber", TestSealNumber1, sCDContainer.CJ_SealNumber);
			AssertEquals("CJ_Voyage", TestVoyageNumber1, sCDContainer.CJ_Voyage);
		}

		public void TestIsAutoLogged()
		{
			CusSCADepotContainer container = Factory.New<CusSCADepotContainer>();
			Factory.Save();
			StmALog addedEvent = container.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
			AssertNotNull("Failed to create Added event", addedEvent);
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			CusSCADepotContainer container = Factory.New<CusSCADepotContainer>();
			container.CJ_PackageCount = 48;
			AssertEquals(48, ((IOutturnableLine)container).PackagesManifested);
		}

		#region Implementation

		const string TestContainerNumber1 = "FRDU0000298";
		const string TestContainerNumber2 = "DGSU0022939";
		const string TestLloydsNumber1 = "8610033";
		const string TestLloydsNumber2 = "4650021";
		const string TestSealNumber1 = "465824";
		const string TestSealNumber2 = "975452";
		const string TestVoyageNumber1 = "611";
		const string TestVoyageNumber2 = "2012";

		RefVessel GetOrCreateRefVessel(ZString lloydsNumber)
		{
			var result = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			if (result == null)
			{
				result = RefVessel.New(Factory);
				result.RV_Code = "TEST VESSEL " + lloydsNumber;
				result.RV_LloydsNumber = lloydsNumber;
			}
			return result;
		}

		CFSShipment GetShipment()
		{
			CFSShipment result = Factory.New<CFSShipment>();
			return result;
		}

		#endregion
	}
}
