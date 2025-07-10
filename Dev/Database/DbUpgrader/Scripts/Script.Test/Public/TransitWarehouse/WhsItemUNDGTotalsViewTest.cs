using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse.WhsItemUNDGTotalsView))]
	class WhsItemUNDGTotalsViewTest : DbCreateScriptTest
	{
		public void TestView_HaveDGSubstanceAndDGCountryReferenceAndDGClass()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);
			var package2 = new PkgPackage(packageJob, "PLT", 2).AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);

			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var substance3 = new ZZUNDGSubstance("1488", "c", "IMO").AppendInsertAndReturnObject(sql);

			var undgdata1 = CreateUNDGDataItem(sql, package1.PK, substance1, weight: 15);
			var undgdata2 = CreateUNDGDataItem(sql, package1.PK, substance2, weight: 15);
			var undgdata3 = CreateUNDGDataItem(sql, package2.PK, substance1, weight: 15);
			var undgdata4 = CreateUNDGDataItem(sql, package2.PK, substance3, weight: 15);

			var countryReference = new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "AU", DCR_Code = "UNDG1", DCR_Description = "Test Description" }.AppendInsertAndReturnObject(sql);
			var pivot1 = new UNDGCountryReferencePivot(countryReference, "1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var pivot2 = new UNDGCountryReferencePivot(countryReference, "1488", "c", "IMO").AppendInsertAndReturnObject(sql);

			var limit = new WhsUNDGLimit(whs, substance1) { WWD_TotalWeightLimit = 30, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot1 = new UNDGSubstancePivot(limit.PK, "WWD", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard).AppendInsertAndReturnObject(sql);

			var limit2 = new WhsUNDGLimit(whs, substance2) { WWD_TotalWeightLimit = 50000, WWD_TotalWeightLimitUQ = "G", WWD_TotalVolumeLimit = 30000, WWD_TotalVolumeLimitUQ = "D3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot2 = new UNDGSubstancePivot(limit2.PK, "WWD", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard).AppendInsertAndReturnObject(sql);

			var limit3 = new WhsUNDGLimit(whs, countryReference) { WWD_TotalWeightLimit = 60, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 50, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);

			var class1Substance1 = new ZZUNDGSubstance("0005", "a", "IMO") { DG_Class = "1.1F" }.AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", class1Substance1, packageJob, rcn.PK, rtu.PK, locationA1.PK);

			var class1Substance2 = new ZZUNDGSubstance("0006", "a", "IMO") { DG_Class = "1.1E" }.AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", class1Substance2, packageJob, rcn.PK, rtu.PK, locationA1.PK);

			var class1Limit = new WhsUNDGLimit(whs, "1") { WWD_TotalWeightLimit = 40, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 40, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var class3Limit = new WhsUNDGLimit(whs, "3") { WWD_TotalWeightLimit = 40, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 40, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);

			var class2Substance = new ZZUNDGSubstance("1006", "a", "IMO") { DG_Class = "2.2" }.AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", class2Substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);

			var classCombLimit = new WhsUNDGLimit(whs, "Comb") { WWD_TotalWeightLimit = 40, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 40, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var classCombSubstance = new ZZUNDGSubstance("1993", "d", "IMO") { DG_Class = "Comb" }.AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", classCombSubstance, packageJob, rcn.PK, rtu.PK, locationA1.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(12, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance1.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance1.PK, null, 30, 30, 100, "KG", 20, 20, 100, "M3", GetWDT_CodeFromSubstance(substance1), "ETC");

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance2.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, substance2.PK, null, 15000, 50000, 30, "G", 10000, 30000, new decimal(33.33), "D3", GetWDT_CodeFromSubstance(substance2), "ETC");

			var view3 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DCR_UNDGCountryReference.FK == countryReference.PK).First();
			AssertNotNull(view3);
			AssertTotalsView(view3, whs.PK, null, countryReference.PK, 30, 60, 50, "KG", 20, 50, 40, "M3", countryReference.DCR_Code, "ETC");

			var view4 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance3.PK).First();
			AssertNotNull(view4);
			AssertTotalsView(view4, whs.PK, substance3.PK, null, 15, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance3), "NLS", false);

			var view5 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == class1Substance1.PK).First();
			AssertNotNull(view5);
			AssertTotalsView(view5, whs.PK, class1Substance1.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(class1Substance1), "NLS", false);

			var view6 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == class1Substance2.PK).First();
			AssertNotNull(view6);
			AssertTotalsView(view6, whs.PK, class1Substance2.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(class1Substance2), "NLS", false);

			var view7 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == class2Substance.PK).First();
			AssertNotNull(view7);
			AssertTotalsView(view7, whs.PK, class2Substance.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(class2Substance), "NLS", false);

			var class1Views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "1");
			AssertEquals(1, class1Views.Length);
			var view8 = class1Views.Single();
			AssertNotNull(view8);
			AssertTotalsView(view8, whs.PK, null, null, 20, 40, 50, "KG", 20, 40, 50, "M3", "1", "ETC", true);

			var class2Views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "2");
			AssertEquals(1, class2Views.Length);
			var view9 = class2Views.Single();
			AssertNotNull(view9);
			AssertTotalsView(view9, whs.PK, null, null, 10, 0, 0, "KG", 10, 0, 0, "M3", "2", "NLS", false);

			var class3Views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "3");
			AssertEquals(1, class3Views.Length);
			var view10 = class3Views.Single();
			AssertNotNull(view10);
			AssertTotalsView(view10, whs.PK, null, null, 0, 40, 0, "KG", 0, 40, 0, "M3", "3", "WIT");

			var classCombViews = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "Comb");
			AssertEquals(1, classCombViews.Length);
			var view11 = classCombViews.Single();
			AssertNotNull(view11);
			AssertTotalsView(view11, whs.PK, null, null, 10, 40, 25, "KG", 10, 40, 25, "M3", "Comb", "ETC");

			var view12 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == classCombSubstance.PK).First();
			AssertNotNull(view12);
			AssertTotalsView(view12, whs.PK, classCombSubstance.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(classCombSubstance), "NLS", false);
		}

		public void TestView_DifferentPackageStatus()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dll = new WhsItemDispatchLoadList("DLL00001", whs).AppendInsertAndReturnObject(sql);
			var dtu = new WhsItemDispatchTransportationUnit(whs, "TD000001").AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var limit = new WhsUNDGLimit(whs, substance) { WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot = new UNDGSubstancePivot(limit.PK, "WWD", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);

			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PUT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "CTT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PIC", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ADJ", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "STA", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FLO", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "DEP", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FIN", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(1, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance.PK, null, 70, 100, 70, "KG", 70, 100, 70, "M3", GetWDT_CodeFromSubstance(substance), "ETC");
		}

		public void TestView_DifferentPackageStatus_CountryReferenceCase()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dll = new WhsItemDispatchLoadList("DLL00001", whs).AppendInsertAndReturnObject(sql);
			var dtu = new WhsItemDispatchTransportationUnit(whs, "TD000001").AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var countryReference = new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "AU", DCR_Code = "UNDG1", DCR_Description = "Test Description" }.AppendInsertAndReturnObject(sql);
			var pivot = new UNDGCountryReferencePivot(countryReference, "1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var limit = new WhsUNDGLimit(whs, countryReference) { WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);

			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PUT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "CTT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PIC", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ADJ", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "STA", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FLO", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "DEP", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FIN", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DCR_UNDGCountryReference.FK == countryReference.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, null, countryReference.PK, 70, 100, 70, "KG", 70, 100, 70, "M3", countryReference.DCR_Code, "ETC");

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, substance.PK, null, 70, 0, 0, "KG", 70, 0, 0, "M3", GetWDT_CodeFromSubstance(substance), "NLS", false);
		}

		public void TestView_DifferentPackageStatus_Class()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dll = new WhsItemDispatchLoadList("DLL00001", whs).AppendInsertAndReturnObject(sql);
			var dtu = new WhsItemDispatchTransportationUnit(whs, "TD000001").AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO") { DG_Class = "5.1" }.AppendInsertAndReturnObject(sql);
			var limit = new WhsUNDGLimit(whs, "5") { WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);

			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PUT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "CTT", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "PIC", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ADJ", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "STA", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FLO", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "DEP", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "FIN", substance, packageJob, rcn.PK, rtu.PK, locationA1.PK, dll.PK, dcn.PK, dtu.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance.PK, null, 70, 0, 0, "KG", 70, 0, 0, "M3", GetWDT_CodeFromSubstance(substance), "NLS", false);

			var classViews = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "5");
			AssertEquals(1, classViews.Length);
			var view2 = classViews.Single();
			AssertTotalsView(view2, whs.PK, null, null, 70, 100, 70, "KG", 70, 100, 70, "M3", "5", "ETC");
		}

		public void TestView_NoTransitWarehouse()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);

			var substance = new ZZUNDGSubstance("1488", "a", "IMO") { DG_Class = "5.1" }.AppendInsertAndReturnObject(sql);
			var undgdata = new UNDGDataItem(package.PK, substance) { DI_DGWeight = 15, DI_UnitOfWeight = "KG", DI_DGVolume = 10, DI_UnitOfVolume = "M3" }.AppendInsertAndReturnObject(sql);
			var limit = new WhsUNDGLimit(whs, substance) { WWD_TotalWeightLimit = 30, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot = new UNDGSubstancePivot(limit.PK, "WWD", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(0, views.Length);
		}

		public void TestView_HaveHandlingUnit()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);
			var hanlingUnitPackage = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var hanlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, hanlingUnitPackage.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1, isHandlingUnit: true);

			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var undgdata1 = CreateUNDGDataItem(sql, package.PK, substance);
			var undgdata2 = CreateUNDGDataItem(sql, hanlingUnitPackage.PK, substance);
			var limit = new WhsUNDGLimit(whs, substance) { WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot = new UNDGSubstancePivot(limit.PK, "WWD", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(1, views.Length);

			var view = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view);
			AssertTotalsView(view, whs.PK, substance.PK, null, 10, 20, 50, "KG", 10, 20, 50, "M3", GetWDT_CodeFromSubstance(substance), "ETC");
		}

		public void TestView_DefaultUnitOfMeasures()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);

			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var undgdata1 = CreateUNDGDataItem(sql, package.PK, substance1);
			var undgdata2 = CreateUNDGDataItem(sql, package.PK, substance2);
			var limit1 = new WhsUNDGLimit(whs, substance1) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot1 = new UNDGSubstancePivot(limit1.PK, "WWD", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard).AppendInsertAndReturnObject(sql);
			var limit2 = new WhsUNDGLimit(whs, substance2) { WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot2 = new UNDGSubstancePivot(limit2.PK, "WWD", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance1.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance1.PK, null, 10, 0, 0, "KG", 10, 20, 50, "M3", GetWDT_CodeFromSubstance(substance1), "ETC");

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance2.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, substance2.PK, null, 10, 20, 50, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance2), "ETC");
		}

		public void TestView_DGDataWithoutLimits()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);

			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO") { DG_Class = "5.1" }.AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var countryReference = new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "AU", DCR_Code = "UNDG1", DCR_Description = "Test Description" }.AppendInsertAndReturnObject(sql);
			var pivot1 = new UNDGCountryReferencePivot(countryReference, "1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var pivot2 = new UNDGCountryReferencePivot(countryReference, "1488", "b", "IMO").AppendInsertAndReturnObject(sql);

			var undgdata1 = CreateUNDGDataItem(sql, package.PK, substance1);
			var undgdata2 = CreateUNDGDataItem(sql, package.PK, substance2, unitOfWeight: "T", volume: 1000, unitOfVolume: "D3");

			var substanceComb = new ZZUNDGSubstance("1993", "d", "IMO") { DG_Class = "Comb" }.AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "ARV", substanceComb, packageJob, rcn.PK, rtu.PK, locationA1.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(6, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance1.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance1.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance1), "NLS", false);

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance2.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, substance2.PK, null, 10000, 0, 0, "KG", 1, 0, 0, "M3", GetWDT_CodeFromSubstance(substance2), "NLS", false);

			var view3 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DCR_UNDGCountryReference.FK == countryReference.PK).First();
			AssertNotNull(view3);
			AssertTotalsView(view3, whs.PK, null, countryReference.PK, 10010, 0, 0, "KG", 11, 0, 0, "M3", countryReference.DCR_Code, "NLS", false);

			var view4 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "5").Single();
			AssertNotNull(view4);
			AssertTotalsView(view4, whs.PK, null, null, 10, 0, 0, "KG", 10, 0, 0, "M3", "5", "NLS", false);

			var view5 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "Comb").Single();
			AssertNotNull(view5);
			AssertTotalsView(view5, whs.PK, null, null, 10, 0, 0, "KG", 10, 0, 0, "M3", "Comb", "NLS", false);

			var view6 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substanceComb.PK).Single();
			AssertNotNull(view6);
			AssertTotalsView(view6, whs.PK, substanceComb.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substanceComb), "NLS", false);
		}

		public void TestView_HasUNDGLimitsButNoDGData()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var countryReference = new UNDGCountryReference() { DCR_Type = "ICPE", DCR_RN_NKCountry = "AU", DCR_Code = "UNDG1", DCR_Description = "Test Description" }.AppendInsertAndReturnObject(sql);
			var pivot1 = new UNDGCountryReferencePivot(countryReference, "1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var pivot2 = new UNDGCountryReferencePivot(countryReference, "1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var limit1 = new WhsUNDGLimit(whs, countryReference) { WWD_TotalVolumeLimit = 30, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limit2 = new WhsUNDGLimit(whs, substance2) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot = new UNDGSubstancePivot(limit2.PK, "WWD", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard).AppendInsertAndReturnObject(sql);

			var classLimit = new WhsUNDGLimit(whs, "5") { WWD_TotalVolumeLimit = 40, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 40, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var classCombLimit = new WhsUNDGLimit(whs, "Comb") { WWD_TotalVolumeLimit = 40, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 40, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(4, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance2.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance2.PK, null, 0, 0, 0, "KG", 0, 20, 0, "M3", GetWDT_CodeFromSubstance(substance2), "WIT", true);

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DCR_UNDGCountryReference.FK == countryReference.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, null, countryReference.PK, 0, 0, 0, "KG", 0, 30, 0, "M3", countryReference.DCR_Code, "WIT", true);

			var view3 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "5").Single();
			AssertNotNull(view3);
			AssertTotalsView(view3, whs.PK, null, null, 0, 40, 0, "KG", 0, 40, 0, "M3", "5", "WIT", true);

			var view4 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_Source == "CLS" && v.WDT_Code == "Comb").Single();
			AssertNotNull(view4);
			AssertTotalsView(view4, whs.PK, null, null, 0, 40, 0, "KG", 0, 40, 0, "M3", "Comb", "WIT", true);
		}

		public void TestView_AppropriateValueIsSetToCapacityStatusWhenAnyLimitExceeded()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 50 }.WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, rcn, rtu, lastLocation: locationA1);

			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("1488", "b", "IMO").AppendInsertAndReturnObject(sql);
			var substance3 = new ZZUNDGSubstance("1488", "c", "IMO").AppendInsertAndReturnObject(sql);
			var substance4 = new ZZUNDGSubstance("1488", "d", "IMO").AppendInsertAndReturnObject(sql);

			var undgdata1 = CreateUNDGDataItem(sql, package.PK, substance1);
			var undgdata2 = CreateUNDGDataItem(sql, package.PK, substance2, volume: 15);
			var undgdata3 = CreateUNDGDataItem(sql, package.PK, substance3, weight: 25);
			var undgdata4 = CreateUNDGDataItem(sql, package.PK, substance4, weight: 15, volume: 15);

			var limit1 = new WhsUNDGLimit(whs, substance1) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot1 = new UNDGSubstancePivot(limit1.PK, "WWD", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard).AppendInsertAndReturnObject(sql);
			var limit2 = new WhsUNDGLimit(whs, substance2) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot2 = new UNDGSubstancePivot(limit2.PK, "WWD", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard).AppendInsertAndReturnObject(sql);
			var limit3 = new WhsUNDGLimit(whs, substance3) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot3 = new UNDGSubstancePivot(limit3.PK, "WWD", substance3.DG_UNNO, substance3.DG_Variant, substance3.DG_Standard).AppendInsertAndReturnObject(sql);
			var limit4 = new WhsUNDGLimit(whs, substance4) { WWD_TotalVolumeLimit = 20, WWD_TotalVolumeLimitUQ = "M3", WWD_TotalWeightLimit = 20, WWD_TotalWeightLimitUQ = "KG" }.AppendInsertAndReturnObject(sql);
			var limitUNDGSubstancePivot4 = new UNDGSubstancePivot(limit4.PK, "WWD", substance4.DG_UNNO, substance4.DG_Variant, substance4.DG_Standard).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(4, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance1.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance1.PK, null, 10, 20, 50, "KG", 10, 20, 50, "M3", GetWDT_CodeFromSubstance(substance1), "WIT");

			var view2 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance2.PK).First();
			AssertNotNull(view2);
			AssertTotalsView(view2, whs.PK, substance2.PK, null, 10, 20, 50, "KG", 15, 20, 75, "M3", GetWDT_CodeFromSubstance(substance2), "ETC");

			var view3 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance3.PK).First();
			AssertNotNull(view3);
			AssertTotalsView(view3, whs.PK, substance3.PK, null, 25, 20, 125, "KG", 10, 20, 50, "M3", GetWDT_CodeFromSubstance(substance3), "ECP");

			var view4 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance4.PK).First();
			AssertNotNull(view4);
			AssertTotalsView(view4, whs.PK, substance4.PK, null, 15, 20, 75, "KG", 15, 20, 75, "M3", GetWDT_CodeFromSubstance(substance4), "ETC");
		}

		public void TestView_PlannedPackageHasUNDG_Registry_BothNotSet() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.NotSet, RegistryStatus.NotSet);

		public void TestView_PlannedPackageHasUNDG_Registry_BothEnabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Enabled, RegistryStatus.Enabled);

		public void TestView_PlannedPackageHasUNDG_Registry_BothDisabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Disabled, RegistryStatus.Disabled);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemEnabledBranchNotSet() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Enabled, RegistryStatus.NotSet);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemEnabledBranchDisabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Enabled, RegistryStatus.Disabled);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemNotSetBranchEnabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.NotSet, RegistryStatus.Enabled);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemNotSetBranchDisabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.NotSet, RegistryStatus.Disabled);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemDisabledBranchNotSet() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Disabled, RegistryStatus.NotSet);

		public void TestView_PlannedPackageHasUNDG_Registry_SystemDisabledBranchEnabled() => TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus.Disabled, RegistryStatus.Enabled);

		void TestView_PlannedPackageHasUNDG_SystemLevelRegistryCore(RegistryStatus systemLevelRegistry, RegistryStatus branchLevelRegistry)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu") { WRH_GateInTime = new DateTimeOffset(2022, 11, 01, 9, 0, 0, TimeSpan.FromMinutes(480)) }.AppendInsertAndReturnObject(sql);
			var asn = new WhsItemReceiveASN(whs, "asn", "ASN001").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu, asn).AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK, asnID: asn.PK);

			var limit = new WhsUNDGLimit(whs, substance) { WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
			new UNDGSubstancePivot(limit.PK, "WWD", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var disabledRegistryValue = "0x460061006C0073006500";
			var enabledRegistryValue = "0x5400720075006500";

			if (branchLevelRegistry != RegistryStatus.NotSet)
			{
				var registryValue = branchLevelRegistry == RegistryStatus.Enabled ? enabledRegistryValue : disabledRegistryValue;

				var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', {registryValue}, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
				TestConnection.ExecuteNonQuery(registrySQL);
			}

			if (systemLevelRegistry != RegistryStatus.NotSet)
			{
				var registryValue = systemLevelRegistry == RegistryStatus.Enabled ? enabledRegistryValue : disabledRegistryValue;

				var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', {registryValue}, 'EnableUNDGValidationWhenRTUGateIn')
";
				TestConnection.ExecuteNonQuery(registrySQL);
			}

			var view = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).Single();
			AssertNotNull(view);

			bool enabled = branchLevelRegistry == RegistryStatus.Enabled || (branchLevelRegistry == RegistryStatus.NotSet && systemLevelRegistry == RegistryStatus.Enabled);

			if (enabled)
			{
				AssertTotalsView(view, whs.PK, substance.PK, null, 10, 100, 10, "KG", 10, 100, 10, "M3", GetWDT_CodeFromSubstance(substance), "ETC");
			}
			else
			{
				AssertTotalsView(view, whs.PK, substance.PK, null, 0, 100, 0, "KG", 0, 100, 0, "M3", GetWDT_CodeFromSubstance(substance), "WIT");
			}
		}

		public void TestView_PlannedPackageHasUNDG_WithLimit() => TestView_PlannedPackageHasUNDGCore(true);

		public void TestView_PlannedPackageHasUNDG_WithoutLimit() => TestView_PlannedPackageHasUNDGCore(false);

		void TestView_PlannedPackageHasUNDGCore(bool isLimited)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu") { WRH_GateInTime = new DateTimeOffset(2022, 11, 01, 9, 0, 0, TimeSpan.FromMinutes(480)) }.AppendInsertAndReturnObject(sql);
			var asn = new WhsItemReceiveASN(whs, "asn", "ASN001").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu, asn).AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK, asnID: asn.PK);

			if (isLimited)
			{
				var limit = new WhsUNDGLimit(whs, substance) { WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "KG", WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "M3" }.AppendInsertAndReturnObject(sql);
				new UNDGSubstancePivot(limit.PK, "WWD", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);
			}

			TestConnection.ExecuteNonQuery(sql.ToString());
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', 0x5400720075006500, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(1, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view1);

			if (isLimited)
			{
				AssertTotalsView(view1, whs.PK, substance.PK, null, 10, 100, 10, "KG", 10, 100, 10, "M3", GetWDT_CodeFromSubstance(substance), "ETC");
			}
			else
			{
				AssertTotalsView(view1, whs.PK, substance.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance), "NLS", false);
			}
		}

		public void TestView_PlannedPackageHasUNDG_RTUHasCompleted()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var asn = new WhsItemReceiveASN(whs, "asn", "ASN001").AppendInsertAndReturnObject(sql);

			var now = new DateTimeOffset(2022, 11, 01, 9, 0, 0, TimeSpan.FromMinutes(480));
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu") { WRH_GateInTime = now, WRH_UnloadCompleteNotYetProcessedTime = now, WRH_UnloadCompleteTime = now }.AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu, asn).AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK, asnID: asn.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', 0x5400720075006500, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(0, views.Length);
		}

		public void TestView_PlannedPackageHasUNDG_RTUNotGatedIn()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var asn = new WhsItemReceiveASN(whs, "asn", "ASN001").AppendInsertAndReturnObject(sql);

			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu, asn).AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK, asnID: asn.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', 0x5400720075006500, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(0, views.Length);
		}

		public void TestView_PlannedPackageHasUNDG_AttachedToMultipleRTU()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var asn = new WhsItemReceiveASN(whs, "asn", "ASN001").AppendInsertAndReturnObject(sql);

			var now = new DateTimeOffset(2022, 11, 01, 9, 0, 0, TimeSpan.FromMinutes(480));
			var rtu1 = new WhsItemReceiveTransportationUnit(whs, "rtu1", locationA1, "rtu1") { WRH_GateInTime = now }.AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu1, asn).AppendInsertAndReturnObject(sql);

			var rtu2 = new WhsItemReceiveTransportationUnit(whs, "rtu2", locationA1, "rtu2").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu2, asn).AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance, packageJob, rcn.PK, asnID: asn.PK);

			TestConnection.ExecuteNonQuery(sql.ToString());
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', 0x5400720075006500, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(1, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance), "NLS", false);
		}

		public void TestView_PlannedPackageHasUNDG_MultipleRTUAndMultipleASN()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK) { WW_DGThresholdPercentage = 5 }.WithDockDoor(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var substance1 = new ZZUNDGSubstance("1488", "a", "IMO").AppendInsertAndReturnObject(sql);
			var substance2 = new ZZUNDGSubstance("0005", "a", "IMO").AppendInsertAndReturnObject(sql);

			var asn1 = new WhsItemReceiveASN(whs, "asn1", "ASN001").AppendInsertAndReturnObject(sql);
			var asn2 = new WhsItemReceiveASN(whs, "asn2", "ASN002").AppendInsertAndReturnObject(sql);

			var now = new DateTimeOffset(2022, 11, 01, 9, 0, 0, TimeSpan.FromMinutes(480));

			var rtu1 = new WhsItemReceiveTransportationUnit(whs, "rtu1", locationA1, "rtu1").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu1, asn1).AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu1, asn2).AppendInsertAndReturnObject(sql);

			var rtu2 = new WhsItemReceiveTransportationUnit(whs, "rtu2", locationA1, "rtu2") { WRH_GateInTime = now }.AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu2, asn1).AppendInsertAndReturnObject(sql);

			var rtu3 = new WhsItemReceiveTransportationUnit(whs, "rtu3", locationA1, "rtu3").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu3, asn1).AppendInsertAndReturnObject(sql);

			var rtu4 = new WhsItemReceiveTransportationUnit(whs, "rtu4", locationA1, "rtu4").AppendInsertAndReturnObject(sql);
			new WhsItemReceiveASNRTUPivot(rtu4, asn2).AppendInsertAndReturnObject(sql);

			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance1, packageJob, rcn.PK, asnID: asn1.PK);
			CreatePackageStateWithStatusAndDGItem(sql, whs, "BKD", substance2, packageJob, rcn.PK, asnID: asn2.PK);
			TestConnection.ExecuteNonQuery(sql.ToString());
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES ('{Guid.NewGuid()}', 'A', '2023-11-03T08:38:39', 'A', '2023-11-03T08:38:39', 'BOL', 0x5400720075006500, '{whs.WW_GB_RelatedCompanyBranch}', 'EnableUNDGValidationWhenRTUGateIn')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var views = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection);
			AssertEquals(1, views.Length);

			var view1 = WhsItemUNDGTotalsView.ShallowLoadFromDB(TestConnection, v => v.WDT_DG_Substance.FK == substance1.PK).First();
			AssertNotNull(view1);
			AssertTotalsView(view1, whs.PK, substance1.PK, null, 10, 0, 0, "KG", 10, 0, 0, "M3", GetWDT_CodeFromSubstance(substance1), "NLS", false);
		}

		void AssertTotalsView(WhsItemUNDGTotalsView totalsView, Guid warehousePK, Guid? substancePK, Guid? countryReferencePK, decimal totalWeight, decimal weightLimit, decimal weightLimitPercentage, string totalWeightUQ, decimal totalVolume, decimal volumeLimit, decimal volumeLimitPercentage, string totalVolumeUQ, string code, string capacityStatus, bool isLimited = true)
		{
			AssertEquals(warehousePK, totalsView.WDT_WW_Warehouse.FK);

			if (substancePK != null)
			{
				AssertEquals("Precondition: view source should be DG", "DG", totalsView.WDT_Source);
				AssertNull(totalsView.WDT_DCR_UNDGCountryReference);
				AssertEquals(substancePK, totalsView.WDT_DG_Substance.FK);
			}
			else if (countryReferencePK != null)
			{
				AssertEquals("Precondition: view source should be DCR", "DCR", totalsView.WDT_Source);
				AssertNull(totalsView.WDT_DG_Substance);
				AssertEquals(countryReferencePK, totalsView.WDT_DCR_UNDGCountryReference.FK);
			}
			else
			{
				AssertEquals("Precondition: view source should be CLS", "CLS", totalsView.WDT_Source);
			}

			AssertEquals("Total weight:", totalWeight, totalsView.WDT_TotalWeight);
			AssertEquals("Weight limit:", weightLimit, totalsView.WDT_TotalWeightLimit);
			AssertEquals("Total weight UQ:", totalWeightUQ, totalsView.WDT_TotalWeightLimitUQ);
			AssertEquals("Total volume:", totalVolume, totalsView.WDT_TotalVolume);
			AssertEquals("Volume limit:", volumeLimit, totalsView.WDT_TotalVolumeLimit);
			AssertEquals("Total volume UQ:", totalVolumeUQ, totalsView.WDT_TotalVolumeLimitUQ);
			AssertEquals("Code:", code, totalsView.WDT_Code);
			AssertEquals("Volume limit percentage:", volumeLimitPercentage, totalsView.WDT_VolumeLimitPercentage);
			AssertEquals("Weight limit percentage:", weightLimitPercentage, totalsView.WDT_WeightLimitPercentage);
			AssertEquals("Capacity status:", capacityStatus, totalsView.WDT_CapacityStatus);
			AssertEquals("Is limited:", isLimited, totalsView.WDT_IsLimited);
		}

		void CreatePackageStateWithStatusAndDGItem(StringBuilder sql, IWhsWarehouseSQL whs, string status, ZZUNDGSubstance substance, PkgPackageJob packageJob, Guid rcnID, Guid? rtuID = null, Guid? locationID = null, Guid? dllID = null, Guid? dcnID = null, Guid? dtuID = null, Guid? asnID = null)
		{
			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(package.PK, whs, rcnID, status);
			if (status != "BKD")
			{
				packageState.WPS_WL_LastLocation = (Guid)locationID;
				packageState.WPS_WRH_TransitReceiveHeader = (Guid)rtuID;
				packageState.WPS_UnloadedTime = DateTimeOffset.Now;
				packageState.WPS_UnloadedNotYetProcessedTime = DateTimeOffset.Now;
				packageState.WPS_ReceivedAs = "SCN";
			}

			if (status == "STA")
			{
				packageState.WPS_WDL_LoadList = (Guid)dllID;
			}

			if (status == "ADJ")
			{
				packageState.WPS_AdjustedOut = "OTH";
			}

			if (status == "FLO" || status == "DEP" || status == "FIN")
			{
				packageState.WPS_WDC_TransitDispatchConsignment = (Guid)dcnID;
				packageState.WPS_WDH_TransitDispatchHeader = (Guid)dtuID;
				packageState.WPS_WDL_LoadList = (Guid)dllID;
				packageState.WPS_IsSecure = true;
				packageState.WPS_SecurityStatus = "SEC";
				packageState.WPS_LoadedTime = DateTimeOffset.Now;
			}

			if (asnID != null)
			{
				packageState.WPS_WRP_ReceiveExpectedPacking = (Guid)asnID;
			}

			packageState.AppendInsertAndReturnObject(sql);
			CreateUNDGDataItem(sql, package.PK, substance);
		}

		UNDGDataItem CreateUNDGDataItem(StringBuilder sql, Guid packagePK, ZZUNDGSubstance substance, decimal weight = 10, string unitOfWeight = "KG", decimal volume = 10, string unitOfVolume = "M3")
		{
			var undgDataItem = new UNDGDataItem(packagePK, substance) { DI_DGWeight = weight, DI_UnitOfWeight = unitOfWeight, DI_DGVolume = volume, DI_UnitOfVolume = unitOfVolume, DI_IMOClass = substance.DG_Class }.AppendInsertAndReturnObject(sql);
			new UNDGSubstancePivot(undgDataItem.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard).AppendInsertAndReturnObject(sql);
			return undgDataItem;
		}

		static string GetWDT_CodeFromSubstance(ZZUNDGSubstance substance)
		{
			return substance.DG_UNNO + substance.DG_Variant + "-" + substance.DG_Standard;
		}
	}

	enum RegistryStatus
	{
		NotSet,
		Enabled,
		Disabled
	}
}
