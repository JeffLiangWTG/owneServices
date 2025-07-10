using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Environment;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusOutTurn))]
	public sealed class CusOutTurnTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var basic = Factory.New<CusMAWB>();
			return basic.OutTurns.AddNew();
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestSetDefaultValues()
		{
			var basic = Factory.New<CusMAWB>();
			var ot = basic.OutTurns.AddNew();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 0), ot.C5_CargoReceiptDate);
		}

		public void TestReadOnly()
		{
			var basic = Factory.New<CusMAWB>();
			var ot = basic.OutTurns.AddNew();
			AssertEquals(false, ot.ReadOnly);
			ot.ReadOnly = true;
			AssertEquals("Test that we can set it", true, ot.ReadOnly);
			ot.ReadOnly = false;
			AssertEquals("Test that we can unset it", false, ot.ReadOnly);
			ot.IsDelivered = true;
			AssertEquals(false, ot.ReadOnly);
			Factory.Save();
			AssertEquals(true, ot.ReadOnly);
			ot.IsDelivered = false;
			AssertEquals(false, ot.ReadOnly);
		}

		public void TestDivide()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			RunDivideTest(basic, basic.OutTurns);
			var hawb = Factory.New<CusMAWB>().ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRBAC";
			RunDivideTest(hawb, hawb.OutTurns);
		}

		public void TestLogsUponDelivery()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			var ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 5;
			ot.C5_PackagesUnits = "CTN";
			ot.WarehouseLocationID = locationBac1.PK;
			ot.C5_MarksAndNumbers = "Red, addressed";
			ot.IsReleasedAlready = true;
			Factory.Save();
			ot.IsDelivered = true;
			var log = basic.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull(log);
			AssertEquals(FormattableString.Invariant($"5CTN delivered from [{rowName1}], [Red, addressed]"), log.SL_Reference);

			basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 5;
			ot.C5_PackagesUnits = "CTN";
			ot.WarehouseLocationID = ZGuid.Empty;
			ot.C5_MarksAndNumbers = "Red, addressed";
			ot.IsReleasedAlready = true;
			Factory.Save();
			ot.IsDelivered = true;
			log = basic.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull(log);
			AssertEquals("5CTN delivered from [], [Red, addressed]", log.SL_Reference);

			basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 5;
			ot.C5_PackagesUnits = "CTN";
			ot.WarehouseLocationID = ZGuid.Empty;
			ot.C5_MarksAndNumbers = string.Empty;
			ot.IsReleasedAlready = true;
			Factory.Save();
			ot.IsDelivered = true;
			log = basic.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull(log);
			AssertEquals("5CTN delivered from [], []", log.SL_Reference);

			basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 5;
			ot.C5_PackagesUnits = "CTN";
			ot.WarehouseLocationID = locationBac2.PK;
			ot.C5_MarksAndNumbers = string.Empty;
			ot.IsReleasedAlready = true;
			Factory.Save();
			ot.IsDelivered = true;
			log = basic.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull(log);
			AssertEquals(FormattableString.Invariant($"5CTN delivered from [{rowName2}], []"), log.SL_Reference);
		}

		static void RunDivideTest(ICcsukCusAwb awb, CusOutTurnCollection outturns)
		{
			var ot = outturns.AddNew();
			ot.C5_PackagesOutturned = 10;
			ot.WarehouseLocationID = ZGuid.NewZGuid();
			ot.C5_GoodsDescription = "Stuff";
			ot.C5_MarksAndNumbers = "Red";
			awb.CalculateNprFromReceiptsIfNecessary();
			AssertEquals(10, (int)awb.NumberOfPiecesReceived);
			ot.Divide("1");
			AssertEquals(2, awb.OutTurns.Count);
			AssertEquals(2, outturns.Count);
			var otNew = awb.OutTurns[1];
			AssertEquals(9, ot.C5_PackagesOutturned);
			AssertEquals(1, otNew.C5_PackagesOutturned);
			AssertEquals(ot.WarehouseLocationID, otNew.WarehouseLocationID);
			AssertEquals("Stuff", otNew.C5_GoodsDescription);
			AssertEquals("Red", otNew.C5_MarksAndNumbers);
			awb.CalculateNprFromReceiptsIfNecessary();
			AssertEquals(10, (int)awb.NumberOfPiecesReceived);
		}

		public void TestCanDelete()
		{
			var outTurn = (CusOutTurn)GetNewBusinessObject();
			AssertEquals(true, outTurn.CanDelete);
			AssertEquals(string.Empty, outTurn.ReasonForNotAbleToDelete);
			outTurn.IsBeingReleasedNow = true;
			AssertEquals(false, outTurn.CanDelete);
			AssertContains("released", outTurn.ReasonForNotAbleToDelete);
			outTurn.IsBeingReleasedNow = false;
			outTurn.IsReleasedAlready = true;
			AssertEquals(false, outTurn.CanDelete);
			AssertContains("released", outTurn.ReasonForNotAbleToDelete);
			outTurn.IsReleasedAlready = false;
			AssertEquals(true, outTurn.CanDelete);
			AssertEquals(string.Empty, outTurn.ReasonForNotAbleToDelete);
		}

		public void TestWarehouseLocationID()
		{
			var outTurn = (CusOutTurn)GetNewBusinessObject();
			var id = ZGuid.NewZGuid();
			AssertEquals(false, outTurn.WarehouseLocationIDInfo.ReadOnly);
			outTurn.WarehouseLocationID = id;
			AssertEquals(id, outTurn.WarehouseLocationID);
			AssertEquals(false, outTurn.WarehouseLocationIDInfo.ReadOnly);
			outTurn.IsBeingReleasedNow = true;
			AssertEquals(true, outTurn.WarehouseLocationIDInfo.ReadOnly);
			outTurn.IsBeingReleasedNow = false;
			Factory.Save();
			outTurn = new BusinessObjectFactory().Load<CusOutTurn>(outTurn.PK);
			AssertEquals(id, outTurn.WarehouseLocationID);
			outTurn.WarehouseLocationID = ZGuid.Empty;
			outTurn.Factory.Save();
			AssertEquals("Pivot deleted", 0, new BusinessObjectFactory().Load<GenPivot>(new ZQuery()).Length);
		}

		public void TestWarehouseLocations()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			var basic = Factory.New<CusMAWB>();
			basic.CargoTerminalOperator = "BAC";
			var outTurn = basic.OutTurns.AddNew();
			AssertCollectionContains("Collection has the BAC location", locationBac1, outTurn.WarehouseLocations);
			AssertCollectionContains("Collection has the BAC location", locationBac2, outTurn.WarehouseLocations);
			AssertCollectionNotContains("Collection DOESN'T HAVE the CAX location", locationCax, outTurn.WarehouseLocations);
			basic.CargoTerminalOperator = "CAX";
			AssertCollectionContains("Collection has the CAX location", locationCax, outTurn.WarehouseLocations);
			AssertCollectionNotContains("Collection DOESN'T HAVE the BAC location", locationBac1, outTurn.WarehouseLocations);
			basic.CargoTerminalOperator = "XXX";
			AssertCollectionContains("Collection has the all locations as the warehouse code is not in the CCSUK shed code format - allows users to not have to adhere to the same warehouse naming convention", locationBac2, outTurn.WarehouseLocations);
			AssertCollectionContains("Collection has the all locations as the warehouse code is not in the CCSUK shed code format - allows users to not have to adhere to the same warehouse naming convention", locationCax, outTurn.WarehouseLocations);
			basic.CargoTerminalOperator = "CAX";
			outTurn.WarehouseLocationID = locationCax.PK;
			AssertNoErrorContaining(outTurn.WarehouseLocationIDInfo, "valid");
			outTurn.WarehouseLocationID = locationBac1.PK;
			AssertHasErrorContaining(outTurn.WarehouseLocationIDInfo, "valid");
			outTurn.WarehouseLocationID = ZGuid.NewZGuid();
			AssertHasErrorContaining(outTurn.WarehouseLocationIDInfo, "valid");
		}

		public const string rowName1 = "RowName-1-veryLong";
		public const string rowName2 = "RowName-2";
		public const string rowName3 = "RowName-3";

		public static void CreateWarehouseAreasForTest(BusinessObjectFactory factory, out IWhsLocation locationBac1, out IWhsLocation locationBac2, out IWhsLocation locationCax)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var warehouseBAC = (IWhsWarehouse)helper.CreateWarehouse("BAC");
			var warehouseCAX = (IWhsWarehouse)helper.CreateWarehouse("CAX");
			var bacArea = helper.CreateWhsArea(warehouseBAC.PK, "BAC");
			var caxArea = helper.CreateWhsArea(warehouseBAC.PK, "CAX");
			var warehouseRowBacRow1 = helper.CreateRow(warehouseBAC, rowName1);
			var warehouseRowBacRow2 = helper.CreateRow(warehouseBAC, rowName2);
			var warehouseRowCaxRow = helper.CreateRow(warehouseCAX, rowName3);
			factory.Save();

			locationBac1 = (IWhsLocation)warehouseRowBacRow1.Locations[0];
			locationBac2 = (IWhsLocation)warehouseRowBacRow2.Locations[0];
			locationCax = (IWhsLocation)warehouseRowCaxRow.Locations[0];
			locationBac1.WLV_WA_PickingArea = bacArea.PK;
			locationBac2.WLV_WA_PickingArea = bacArea.PK;
			locationCax.WLV_WA_PickingArea = caxArea.PK;
			factory.Save();
		}

		public void TestNewProperties()
		{
			var outTurn = Factory.New<CusOutTurn>();
			AssertEquals(false, outTurn.ReadOnly);
			AssertEquals(false, outTurn.IsReleasedAlready);
			AssertEquals(true, outTurn.IsReleasedAlreadyInfo.ReadOnly);
			AssertEquals(false, outTurn.IsBeingReleasedNow);
			AssertEquals(false, outTurn.IsBeingReleasedNowInfo.ReadOnly);

			outTurn.IsBeingReleasedNow = true;
			AssertEquals(false, outTurn.ReadOnly);
			AssertEquals(false, outTurn.IsReleasedAlready);
			AssertEquals(false, outTurn.IsDelivered);
			AssertEquals(true, outTurn.IsReleasedAlreadyInfo.ReadOnly);
			AssertEquals(true, outTurn.C5_PackagesOutturnedInfo.ReadOnly);
			AssertEquals(false, outTurn.IsBeingReleasedNowInfo.ReadOnly);

			outTurn.IsReleasedAlready = true;
			AssertEquals(false, outTurn.ReadOnly);
			AssertEquals(true, outTurn.IsReleasedAlready);
			AssertEquals(false, outTurn.IsDelivered);
			AssertEquals(true, outTurn.IsReleasedAlreadyInfo.ReadOnly);
			AssertEquals(true, outTurn.C5_PackagesOutturnedInfo.ReadOnly);
			AssertEquals(true, outTurn.IsBeingReleasedNowInfo.ReadOnly);

			outTurn.IsDelivered = true;
			AssertEquals(true, outTurn.IsDelivered);
			AssertEquals(false, outTurn.ReadOnly);

			Factory.Save();
			outTurn = new BusinessObjectFactory().Load<CusOutTurn>(outTurn.PK);
			AssertEquals(true, outTurn.IsBeingReleasedNow);
			AssertEquals(true, outTurn.IsReleasedAlready);
			AssertEquals(true, outTurn.IsDelivered);
			AssertEquals(true, outTurn.ReadOnly);

			outTurn.IsDeliveredInfo.Value = ZBool.False;
			AssertEquals(false, outTurn.IsDelivered);
			outTurn.IsReleasedAlreadyInfo.Value = ZBool.False;
			AssertEquals(false, outTurn.IsReleasedAlready);
		}

		public void TestValidation()
		{
			var outTurn = Factory.New<CusOutTurn>();
			AssertType(typeof(CusOutTurnValidation), outTurn.Validation);
		}

		public void TestSetSplitNumberBringsInHandlingInfoFromSplit()
		{
			var basic = Factory.New<CusMAWB>();
			var split = basic.Splits.AddNew();
			split.HandlingInformation = "ABC";
			split.SplitReference = "01";
			var outturn = basic.OutTurns.AddNew();
			AssertEquals(ZString.Empty, outturn.C5_MarksAndNumbers);
			outturn.SplitReferenceToWhichThisPertains = "01";
			AssertEquals("ABC", outturn.C5_MarksAndNumbers);
			outturn.C5_MarksAndNumbers = "DEF";
			outturn.SplitReferenceToWhichThisPertains = "";
			outturn.SplitReferenceToWhichThisPertains = "01";
			AssertEquals("Unchanged if already set on outturn", "DEF", outturn.C5_MarksAndNumbers);
		}

		public void TestSplitNumberAndUnderBondSetterAndReadOnly()
		{
			var basic1 = Factory.New<CusMAWB>();
			var basicOutTurn1 = basic1.OutTurns.AddNew();
			AssertEquals(true, basicOutTurn1.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(true, basicOutTurn1.C5_C4_UnderbondInfo.ReadOnly);

			var basic2 = Factory.New<CusMAWB>();
			var splitBasic2 = basic2.Splits.AddNew();
			splitBasic2.SplitReference = "01";
			var splitBasicOutTurn2 = basic2.OutTurns.AddNew();
			AssertEquals(false, splitBasicOutTurn2.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(true, splitBasicOutTurn2.C5_C4_UnderbondInfo.ReadOnly);

			var basic3 = Factory.New<CusMAWB>();
			var underbond3 = basic3.TSRs.AddNew();
			var splitBasic3 = basic3.Splits.AddNew();
			splitBasic3.SplitReference = "01";
			underbond3.SplitReferenceToWhichThisRemovalPertains = splitBasic3.SplitReference;
			var splitBasicOutTurn3 = basic3.OutTurns.AddNew();
			AssertEquals(false, splitBasicOutTurn3.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(false, splitBasicOutTurn3.C5_C4_UnderbondInfo.ReadOnly);
			splitBasicOutTurn3.IsBeingReleasedNow = true;
			AssertEquals(true, splitBasicOutTurn3.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(true, splitBasicOutTurn3.C5_C4_UnderbondInfo.ReadOnly);
			AssertEquals(true, splitBasicOutTurn3.C5_PackagesOutturnedInfo.ReadOnly);
			splitBasicOutTurn3.IsBeingReleasedNow = false;
			AssertEquals(false, splitBasicOutTurn3.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(false, splitBasicOutTurn3.C5_C4_UnderbondInfo.ReadOnly);
			AssertEquals(false, splitBasicOutTurn3.C5_PackagesOutturnedInfo.ReadOnly);
			splitBasicOutTurn3.IsReleasedAlready = true;
			AssertEquals(true, splitBasicOutTurn3.SplitReferenceToWhichThisPertainsInfo.ReadOnly);
			AssertEquals(true, splitBasicOutTurn3.C5_C4_UnderbondInfo.ReadOnly);
			AssertEquals(true, splitBasicOutTurn3.C5_PackagesOutturnedInfo.ReadOnly);

			var basic4 = Factory.New<CusMAWB>();
			var underbond4 = basic4.TSRs.AddNew();
			var splitBasic4 = basic4.Splits.AddNew();
			splitBasic4.SplitReference = "01";
			var splitBasicOutTurn4 = basic4.OutTurns.AddNew();
			splitBasicOutTurn4.C5_C4_Underbond = underbond3.PK;
			AssertEquals("01", splitBasicOutTurn4.SplitReferenceToWhichThisPertains);
		}

		public void TestRemovalsList()
		{
			var basic = Factory.New<CusMAWB>();
			var basicOutTurn = basic.OutTurns.AddNew();
			basicOutTurn.RemovalsList.Load();
			AssertEquals(0, basicOutTurn.RemovalsList.Count);
			var iar = basic.IARs.AddNew();
			basicOutTurn.RemovalsList.Load();
			AssertEquals(1, basicOutTurn.RemovalsList.Count);
			AssertCollectionContains(iar, basicOutTurn.RemovalsList);
			var isr = basic.ISRs.AddNew();
			basicOutTurn.RemovalsList.Load();
			AssertEquals(2, basicOutTurn.RemovalsList.Count);
			AssertCollectionContains(isr, basicOutTurn.RemovalsList);
		}

		public void TestTypes()
		{
			var basic = Factory.New<CusMAWB>();
			var basicOutTurn = basic.OutTurns.AddNew();
			AssertType(typeof(CusOutTurn), basicOutTurn);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var hawbOutTurn = hawb.OutTurns.AddNew();
			AssertType(typeof(CusOutTurn), hawbOutTurn);
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "01";
			var splitBasicOutTurn = basic.OutTurns.AddNew();
			splitBasicOutTurn.SplitReferenceToWhichThisPertains = splitBasic.SplitReference;
			AssertType(typeof(CusOutTurn), splitBasicOutTurn);
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "01";
			var splitHouseOutTurn = hawb.OutTurns.AddNew();
			splitHouseOutTurn.SplitReferenceToWhichThisPertains = splitHouse.SplitReference;
			AssertType(typeof(CusOutTurn), splitHouseOutTurn);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportOutturn_ValueExceededMaxLength()
		{
			var basic = Factory.New<CusMAWB>();
			var underbond = Factory.New<TranshipmentRemoval>();
			var collection = underbond.Outturns;

			ImportCollectionInfoImpl importInfo = new ImportCollectionInfoImpl(underbond.Outturns);
			importInfo.Add(new ImportPropertyInfoImpl<CusOutturn>("ParentStringRepresentation") { HeaderText = "Outturn Line" });
			importInfo.Add(new ImportPropertyInfoImpl<CusOutturn>(CusOutturnSchema.C5_PackagesOutturned.Name) { HeaderText = "Packages Outturned" });
			importInfo.Add(new ImportPropertyInfoImpl<CusOutturn>(CusOutturnSchema.C5_OutturnResultType.Name) { HeaderText = "Outturn Result Type" });
			ImportWizard wizard = new ImportWizard(importInfo, null, new FileMapperForTest());

			string[] values = new string[] { "0199312650999998917bk024425101000930203", "1234", "SH" };
			wizard.FileName = BaseSourcePath + @"Enterprise\Product\Operations\Customs\GB\Core\Ccsuk.Test\AirCargoInventory\BusinessObjectTests\TestFiles\Import Outrurn Test.csv";
			for (int i = 0; i < wizard.Mapping.Count; i++)
			{
				wizard.Mapping[i].AddFileColumnIndex(i);
			}
			wizard.StartingRow = 2;
			AssertNoExceptionThrown(() => wizard.ImportIntoCollection(underbond.Outturns));

			AssertEquals(1, collection.Count);
			var outturn = collection.OfType<CusOutturn>().FirstOrDefault();
			AssertNotEquals(ZGuid.Empty, outturn.PK);
			AssertEquals("0199312650999998917bk02442510100093", outturn.ParentStringRepresentation);
		}
	}

	[TestedType(typeof(CusOutTurnCollection))]
	public class CusOutTurnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusOutTurnCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var basic = Factory.New<CusMAWB>();
			return basic.OutTurns;
		}

		public void TestTotalDelivered()
		{
			var coll = (CusOutTurnCollection)GetCollectionToTest();
			AssertEquals(0, coll.TotalDelivered);
			var receivedOnly = coll.AddNew();
			receivedOnly.C5_PackagesOutturned = 5;
			receivedOnly.IsDelivered = true;
			AssertEquals(5, coll.TotalDelivered);
			var received10 = coll.AddNew();
			received10.C5_PackagesOutturned = 10;
			var received20 = coll.AddNew();
			received20.C5_PackagesOutturned = 20;
			AssertEquals(5, coll.TotalDelivered);
			received10.IsDelivered = true;
			AssertEquals(15, coll.TotalDelivered);
			received20.IsDelivered = true;
			AssertEquals(35, coll.TotalDelivered);
		}

		public void TestCusOutTurnCollectionGetReloadedProperlyByParentHawbOrMawbWhenParentItselfIsReloaded()
		{
			var basic = Factory.New<CusMAWB>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			basic.OutTurns.AddNew();
			hawb.OutTurns.AddNew();
			Factory.Save();
			AssertEquals(1, basic.OutTurns.Count);
			AssertEquals(1, hawb.OutTurns.Count);
			var newFactoryForModifyingBehindTheScenesWithoutDataRefresh = new BusinessObjectFactory();
			newFactoryForModifyingBehindTheScenesWithoutDataRefresh.RefreshEnabled = false; // same as in TcpIpSenderReceiver
			var basicInNewFactory = newFactoryForModifyingBehindTheScenesWithoutDataRefresh.Load<CusMAWB>(basic.PK);
			var hawbInNewFactory = newFactoryForModifyingBehindTheScenesWithoutDataRefresh.Load<CusHAWB>(hawb.PK);
			basicInNewFactory.OutTurns.AddNew();
			hawbInNewFactory.OutTurns.AddNew();
			newFactoryForModifyingBehindTheScenesWithoutDataRefresh.Save();
			AssertEquals(1, basic.OutTurns.Count);
			AssertEquals(1, hawb.OutTurns.Count);
			basic.Reload();
			hawb.Reload();
			AssertEquals(2, basic.OutTurns.Count);
			AssertEquals(2, hawb.OutTurns.Count);
		}
	}

	[TestedType(typeof(CusOutTurnCollectionForSplit))]
	public class CusOutTurnCollectionForSplitTest_Basic : BusinessObjectCollectionViewTestCase<CusOutTurnCollectionForSplit>
	{
		protected override void SetUp()
		{
			base.SetUp();
			basic = Factory.NewWithValidTestData<CusMAWB>();
			var split01 = basic.Splits.AddNew();
			var split02 = basic.Splits.AddNew();
			split01.SplitReference = "01";
			split02.SplitReference = "02";
			ot1a = basic.OutTurns.AddNew();
			ot1b = basic.OutTurns.AddNew();
			ot2a = basic.OutTurns.AddNew();
			ot2b = basic.OutTurns.AddNew();
			ot1a.SplitReferenceToWhichThisPertains = "01";
			ot1b.SplitReferenceToWhichThisPertains = "01";
			ot2a.SplitReferenceToWhichThisPertains = "02";
			ot2b.SplitReferenceToWhichThisPertains = "02";

			split = split02;
		}

		CusMAWB basic;
		SplitConsignment split;
		CusOutTurn ot1a;
		CusOutTurn ot1b;
		CusOutTurn ot2a;
		CusOutTurn ot2b;

		protected override CusOutTurnCollectionForSplit GetCollectionToTest()
		{
			if (split == null)
			{
				SetUp();
			}
			return (CusOutTurnCollectionForSplit)((ICcsukCusAwb)split).OutTurnsCollection;
		}

		public void TestRightOutTurns()
		{
			var coll = GetCollectionToTest();
			AssertEquals(2, coll.Count);
			AssertCollectionContains(ot2a, coll);
			AssertCollectionContains(ot2b, coll);
			AssertCollectionNotContains(ot1a, coll);
			AssertCollectionNotContains(ot1b, coll);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var ot = basic.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = split.SplitReference;
			return ot;
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Collection.HasChanges = false;
			base.TestAddAndCancelOfElementAsThoughBinding();
		}
	}

	[TestedType(typeof(CusOutTurnCollectionForSplit))]
	public class CusOutTurnCollectionForSplitTest_House : BusinessObjectCollectionViewTestCase<CusOutTurnCollectionForSplit>
	{
		protected override void SetUp()
		{
			base.SetUp();
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			house = mawb.ChildBills.AddNew();
			var split01 = house.Splits.AddNew();
			var split02 = house.Splits.AddNew();
			split01.SplitReference = "01";
			split02.SplitReference = "02";
			ot1a = house.OutTurns.AddNew();
			ot1b = house.OutTurns.AddNew();
			ot2a = house.OutTurns.AddNew();
			ot2b = house.OutTurns.AddNew();
			ot1a.SplitReferenceToWhichThisPertains = "01";
			ot1b.SplitReferenceToWhichThisPertains = "01";
			ot2a.SplitReferenceToWhichThisPertains = "02";
			ot2b.SplitReferenceToWhichThisPertains = "02";

			split = split02;
		}

		CusHAWB house;
		SplitConsignment split;
		CusOutTurn ot1a;
		CusOutTurn ot1b;
		CusOutTurn ot2a;
		CusOutTurn ot2b;

		protected override CusOutTurnCollectionForSplit GetCollectionToTest()
		{
			if (split == null)
			{
				SetUp();
			}
			return (CusOutTurnCollectionForSplit)((ICcsukCusAwb)split).OutTurnsCollection;
		}

		public void TestRightOutTurns()
		{
			var coll = GetCollectionToTest();
			AssertEquals(2, coll.Count);
			AssertCollectionContains(ot2a, coll);
			AssertCollectionContains(ot2b, coll);
			AssertCollectionNotContains(ot1a, coll);
			AssertCollectionNotContains(ot1b, coll);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var ot = house.OutTurns.AddNew();
			ot.SplitReferenceToWhichThisPertains = split.SplitReference;
			return ot;
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Collection.HasChanges = false;
			base.TestAddAndCancelOfElementAsThoughBinding();
		}
	}

	public class CusOutTurnValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC5_MarksAndNumbers()
		{
			var basic = Factory.New<CusMAWB>();
			var outturn = basic.OutTurns.AddNew();
			outturn.C5_MarksAndNumbers = "";
			AssertNoErrorContaining(outturn.C5_MarksAndNumbersInfo, "Splits exist");
			outturn.SplitReferenceToWhichThisPertains = "xx"; // anything
			outturn.C5_MarksAndNumbers = "";
			AssertHasErrorContaining(outturn.C5_MarksAndNumbersInfo, "Splits exist");
			outturn.C5_MarksAndNumbers = "x";
			AssertNoErrorContaining(outturn.C5_MarksAndNumbersInfo, "Splits exist");
			outturn.SplitReferenceToWhichThisPertains = "";
			basic.Splits.AddNew();
			outturn.C5_MarksAndNumbers = "";
			AssertHasErrorContaining(outturn.C5_MarksAndNumbersInfo, "Splits exist");
			outturn.C5_MarksAndNumbers = "y";
			AssertNoErrorContaining(outturn.C5_MarksAndNumbersInfo, "Splits exist");
		}

		public void TestCheckC5_ReceiptOnlyIndicator__IsDelivered()
		{
			var basic = Factory.New<CusMAWB>();
			var outturn = basic.OutTurns.AddNew();
			RunTestCheckC5_ReceiptOnlyIndicator__IsDelivered_Seizure(basic, outturn);

			basic = Factory.New<CusMAWB>();
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "01";
			outturn = basic.OutTurns.AddNew();
			outturn.SplitReferenceToWhichThisPertains = "01";
			RunTestCheckC5_ReceiptOnlyIndicator__IsDelivered_Seizure(splitBasic, outturn);

			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			outturn = hawb1.OutTurns.AddNew();
			RunTestCheckC5_ReceiptOnlyIndicator__IsDelivered_Seizure(hawb1, outturn);
			var splitHouse = hawb2.Splits.AddNew();
			splitHouse.SplitReference = "01";
			outturn = hawb2.OutTurns.AddNew();
			outturn.SplitReferenceToWhichThisPertains = "01";
			RunTestCheckC5_ReceiptOnlyIndicator__IsDelivered_Seizure(splitHouse, outturn);
		}

		void RunTestCheckC5_ReceiptOnlyIndicator__IsDelivered_Seizure(ICcsukCusAwb awb, CusOutTurn outturn)
		{
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				outturn.IsDelivered = true;
				AssertHasErrorContaining(outturn.IsDeliveredInfo, "save first");
				AssertHasErrorContaining(outturn.IsDeliveredInfo, "not marked as released,");
				AssertNoWarningContaining(outturn.IsDeliveredInfo, "read only");
				outturn.IsDelivered = false;
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "save first");
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "not marked as released,");
				AssertNoWarningContaining(outturn.IsDeliveredInfo, "read only");
				outturn.IsReleasedAlready = true;
				outturn.IsDelivered = true;
				AssertHasErrorContaining(outturn.IsDeliveredInfo, "save first");
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "not marked as released,");
				AssertNoWarningContaining(outturn.IsDeliveredInfo, "read only");
				outturn.IsDelivered = false;
				Factory.Save();
				outturn.IsDelivered = true;
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "save first");
				AssertHasWarningContaining(outturn.IsDeliveredInfo, "read only");
			}

			// Can deliver without release if seized and controller
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				outturn.IsReleasedAlready = false;
				outturn.IsDelivered = false;
				awb.SetCustomsActionCode(CustomsStatusCodes.Codes.SeizedDestroyedOrRetainedByCustoms, ZDateTime.Now);
				outturn.IsDelivered = true;
				AssertHasErrorContaining(outturn.IsDeliveredInfo, "cannot deliver it without a release document because you are not a Controller");
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "not marked as released,");
				AssertNoWarningContaining(outturn.IsDeliveredInfo, "scan the seizure notice");
			}

			using (Env.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				outturn.IsDelivered = false;
				outturn.IsDelivered = true;
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "cannot deliver it without a release document because you are not a Controller");
				AssertNoErrorContaining(outturn.IsDeliveredInfo, "not marked as released,");
				AssertHasWarningContaining(outturn.IsDeliveredInfo, "scan the seizure notice");
			}
		}

		public void TestParent()
		{
			var basic = Factory.New<CusMAWB>();
			var outturn = basic.OutTurns.AddNew();
			AssertType(typeof(CusOutTurn), outturn.Validation.Parent);
		}

		public void TestCheckC5_MessageStatus()
		{
			var basic = Factory.New<CusMAWB>();
			var outturn = basic.OutTurns.AddNew();
			outturn.Validation.ValidateC5_MessageStatus();
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "select a split reference");
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "valid split reference");
			outturn.C5_MessageStatus = "X";
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "select a split reference");
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "valid split reference");
			var split = basic.Splits.AddNew();
			split.SplitReference = "99";
			outturn.C5_MessageStatus = "Y";
			AssertHasErrorContaining(outturn.C5_MessageStatusInfo, "valid split reference");
			outturn.C5_MessageStatus = "";
			AssertHasErrorContaining(outturn.C5_MessageStatusInfo, "select a split reference");
			outturn.C5_MessageStatus = "99";
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "select a split reference");
			AssertNoErrorContaining(outturn.C5_MessageStatusInfo, "valid split reference");
		}

		public void TestCheckC5_PackagesOutturned()
		{
			// Handled by the CalculationValidation methods
			Assert(true);
		}

		public void TestCheckC5_PillageIndicator__IsReleasedAlready()
		{
			// Handled by the CalculationValidation methods
			Assert(true);
		}

		public void TestCalculationValidation_WholeAwb()
		{
			var basic = Factory.New<CusMAWB>();
			var outTurn1 = basic.OutTurns.AddNew();
			var outTurn2 = basic.OutTurns.AddNew();
			RunCalculationTest(basic, outTurn1, outTurn2);
		}

		public void TestCalculationValidation_SplitAwb()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			var outTurn11 = basic.OutTurns.AddNew();
			var outTurn12 = basic.OutTurns.AddNew();
			var outTurn21 = basic.OutTurns.AddNew();
			var outTurn22 = basic.OutTurns.AddNew();
			outTurn11.SplitReferenceToWhichThisPertains = "01";
			outTurn12.SplitReferenceToWhichThisPertains = "01";
			outTurn21.SplitReferenceToWhichThisPertains = "02";
			outTurn22.SplitReferenceToWhichThisPertains = "02";
			RunCalculationTest(split1, outTurn11, outTurn12);
		}

		public void TestMaxWeightOfWarehouseLocation()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Weight = 200m;
			basic.NumberOfPiecesExpected = 50;
			basic.CargoTerminalOperator = "CAX";
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			locationCax.WLV_MaxWeight = 22.05m;
			locationCax.WLV_MaxWeightUnit = Core.Constants.Weight.Pounds; // 10 kg
			var ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 5;
			ot.WarehouseLocationID = locationCax.PK;
			AssertHasWarningContaining(ot.WarehouseLocationIDInfo, "(200kg x 5/50pcs = 20kg)");
			ot.C5_PackagesOutturned = 4;
			AssertNoWarningContaining(ot.WarehouseLocationIDInfo, "(200kg x 5/50pcs = 20kg)");
			AssertHasWarningContaining(ot.WarehouseLocationIDInfo, "(200kg x 4/50pcs = 16kg)");
			ot.C5_PackagesOutturned = 1;
			AssertNoWarningContaining(ot.WarehouseLocationIDInfo, "(200kg x 5/50pcs = 20kg)");
			AssertNoWarningContaining(ot.WarehouseLocationIDInfo, "(200kg x 4/50pcs = 16kg)");
		}

		void RunCalculationTest(ICcsukCusAwb awb, CusOutTurn ot1, CusOutTurn ot2)
		{
			ot2.C5_PackagesOutturned = 1;
			awb.NumberOfPiecesReceived = 10;
			ot1.IsDelivered = true;
			ot1.C5_PackagesOutturned = 11;
			AssertHasErrorContaining(ot1.C5_PackagesOutturnedInfo, "You are claiming to have delivered more than you have received");
			ot1.C5_PackagesOutturned = 9;
			AssertNoErrorContaining(ot1.C5_PackagesOutturnedInfo, "You are claiming to have delivered more than you have received");
			ot2.C5_PackagesOutturned = 2;
			AssertNoErrorContaining(ot2.C5_PackagesOutturnedInfo, "You are claiming to have delivered more than you have received");
			ot2.IsDelivered = true;
			AssertHasErrorContaining(ot2.C5_PackagesOutturnedInfo, "You are claiming to have delivered more than you have received");
			ot2.C5_PackagesOutturned = 1;
			AssertNoErrorContaining(ot2.C5_PackagesOutturnedInfo, "You are claiming to have delivered more than you have received");

			ot1.IsReleasedAlready = true;
			AssertHasMessageErrorContaining(ot1.IsReleasedAlreadyInfo, "is less than the number of pieces released in this grid");
			awb.ReleaseThisNumberOfPieces(ot1.C5_PackagesOutturned, NumberOfPiecesReleasedHelper.ShedEvent);
			ot1.IsReleasedAlready = false;
			ot1.IsReleasedAlready = true;
			AssertNoMessageErrorContaining(ot1.IsReleasedAlreadyInfo, "is less than the number of pieces released in this grid");
			ot1.C5_PackagesOutturned += 1;
			AssertHasMessageErrorContaining(ot1.C5_PackagesOutturnedInfo, "is less than the number of pieces released in this grid");
			awb.ReleaseThisNumberOfPieces(1, NumberOfPiecesReleasedHelper.ShedEvent);
			ot1.Validation.ValidateC5_PackagesOutturned();
			AssertNoMessageErrorContaining(ot1.C5_PackagesOutturnedInfo, "is less than the number of pieces released in this grid");

			ot2.IsReleasedAlready = true;
			ot1.C5_PackagesOutturned = 8;
			AssertHasWarningContaining(ot1.C5_PackagesOutturnedInfo, "is not equal to the number of pieces released");
			ot1.C5_PackagesOutturned = 9;
			AssertNoWarningContaining(ot1.C5_PackagesOutturnedInfo, "is not equal to the number of pieces released");
			ot1.IsReleasedAlready = false;
			AssertHasWarningContaining(ot1.IsReleasedAlreadyInfo, "is not equal to the number of pieces released");
			ot1.IsReleasedAlready = true;
			AssertNoWarningContaining(ot1.IsReleasedAlreadyInfo, "is not equal to the number of pieces released");
		}
	}
}

