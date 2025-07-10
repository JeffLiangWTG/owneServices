using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Test;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(GlowDeletePkgPackage))]
	internal sealed class GlowDeletePkgPackageTest : DbCreateScriptTest
	{
		public void TestDeleteRelatedRecords()
		{
			InsertTestData();
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGDataItem WHERE DI_ParentID = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageContainer WHERE K0_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageScreening WHERE KPS_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageTemperature WHERE KPT_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_KP_ParentPackage = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '6862745D-A87E-45B6-AED5-7E9F5746659E'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = 'B04FC182-0515-4F51-A3E3-82609AD25E18'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHold WHERE KHR_PK = 'EBD859A0-62A6-498B-8F0A-6AC41222A1EB'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobServiceLink WHERE ESL_ParentID = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageOrderReference WHERE KPO_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '5CC363B6-B1D6-4882-A944-8F933BAAC429'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KP_Package = '5CC363B6-B1D6-4882-A944-8F933BAAC429'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KPB_OriginalBookedDetail = '41585255-A47E-44C3-81E2-AA4571112394'"));

			RunSP("E56E74D4-1410-4922-9EA7-EB99FD321F3B");

			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGDataItem WHERE DI_ParentID = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageContainer WHERE K0_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageScreening WHERE KPS_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageTemperature WHERE KPT_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_KP_ParentPackage = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '6862745D-A87E-45B6-AED5-7E9F5746659E'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = 'B04FC182-0515-4F51-A3E3-82609AD25E18'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHold WHERE KHR_PK = 'EBD859A0-62A6-498B-8F0A-6AC41222A1EB'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobServiceLink WHERE ESL_ParentID = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageOrderReference WHERE KPO_KP_Package = 'E56E74D4-1410-4922-9EA7-EB99FD321F3B'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '5CC363B6-B1D6-4882-A944-8F933BAAC429'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KP_Package = '5CC363B6-B1D6-4882-A944-8F933BAAC429'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageBookedDetail WHERE KPB_KPB_OriginalBookedDetail = '41585255-A47E-44C3-81E2-AA4571112394'"));
		}

		void InsertTestData()
		{
			GlbGeneratorForTests.NewStaff("S1");
			var packageJobPK = Guid.NewGuid();

			// Outer Package with DirectID, Child Package with Direct ID
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJobPK));
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('6862745D-A87E-45B6-AED5-7E9F5746659E', 'Parent', GetUtcDate(), 'S1', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('E56E74D4-1410-4922-9EA7-EB99FD321F3B', '6862745D-A87E-45B6-AED5-7E9F5746659E', '{0}', 1, 1 , '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJobPK, "PKG"));
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.UNDGDataItem (DI_PK, DI_ParentID, DI_ParentTableCode) VALUES('CA37CC81-5E98-4DD3-AD14-A4D8695D52FE', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', 'APA')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageContainer (K0_PK, K0_KP_Package, K0_RC_ContainerType, K0_SystemCreateTimeUtc, K0_SystemCreateUser, K0_SystemLastEditTimeUtc, K0_SystemLastEditUser) VALUES('DA37CC81-5E98-4DD3-AD14-A4D8695D52FE', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', 'AD809CB7-8691-4534-ADEA-38D0C17D4EC1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageBookedDetail (KPB_PK, KPB_KP_Package, KPB_KPB_OriginalBookedDetail, KPB_SystemCreateTimeUtc, KPB_SystemCreateUser, KPB_SystemLastEditTimeUtc, KPB_SystemLastEditUser) VALUES ('41585255-A47E-44C3-81E2-AA4571112394', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageScreening (KPS_PK, KPS_KP_Package, KPS_Time, KPS_GS_NKScreenedBy, KPS_Method, KPS_SystemCreateTimeUtc, KPS_SystemCreateUser, KPS_SystemLastEditTimeUtc, KPS_SystemLastEditUser) VALUES ('92a674b4-1979-4a62-8f12-d17825298c00', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', '2039-01-01 16:41:33.4730 +10:00', 'AAA', 'AAA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageTemperature (KPT_PK, KPT_KP_Package, KPT_Time, KPT_Unit, KPT_GS_NKMeasuredBy, KPT_SystemCreateTimeUtc, KPT_SystemCreateUser, KPT_SystemLastEditTimeUtc, KPT_SystemLastEditUser) VALUES ('d96965b7-b43d-47b2-a31e-13f6729f188a', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', '2039-01-01 16:41:33.4730 +10:00', 'C', 'AAA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('B04FC182-0515-4F51-A3E3-82609AD25E18', 'Child', GetUtcDate(), 'S1', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageHold (KHR_PK, KHR_KP_Package, KHR_WHC_NKHoldCode, KHR_AddedTime, KHR_GS_NKAddedBy, KHR_RemovedTime, KHR_GS_NKRemovedBy, KHR_SystemCreateUser, KHR_SystemLastEditTimeUtc, KHR_SystemLastEditUser, KHR_SystemCreateTimeUtc) VALUES ('EBD859A0-62A6-498B-8F0A-6AC41222A1EB', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', 'HOLD', GetUtcDate(), 'll4', GetUtcDate(), 'LL4','ll4', GetUtcDate(), 'll4', GetUtcDate())");
			InsertJobServiceLink("E56E74D4-1410-4922-9EA7-EB99FD321F3B");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageOrderReference (KPO_PK, KPO_AutoVersion, KPO_KP_Package, KPO_OrderNumber, KPO_SKUPartNumber, KPO_CommercialInvoiceNumber, KPO_BatchNumber, KPO_SerialNumber, KPO_HarmonizedTariffSchedule, KPO_LineReference, KPO_ExpiryDate, KPO_SystemCreateTimeUtc, KPO_SystemCreateUser, KPO_SystemLastEditTimeUtc, KPO_SystemLastEditUser) VALUES ('A7FE0A6C-D645-4A06-AF73-7AB13406A9B4', 1, 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', '111', '111', '111', '111', '111', '111', '111', GetUtcDate(), GetUtcDate(), 'LL4', GetUtcDate(), 'll4')");
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_KP_ParentPackage, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('4ED93898-A338-44A5-8E5B-45B3EC53D54A', 'B04FC182-0515-4F51-A3E3-82609AD25E18', '{0}', 'E56E74D4-1410-4922-9EA7-EB99FD321F3B', 1, '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJobPK, "PKG"));

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('C7D8726B-740F-4187-9D87-E834C1B962B6', 'Child1', GetUtcDate(), 'S1', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('5CC363B6-B1D6-4882-A944-8F933BAAC429', 'C7D8726B-740F-4187-9D87-E834C1B962B6', '{0}', 1, 1 , '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJobPK, "PKG"));
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageBookedDetail (KPB_PK, KPB_KP_Package, KPB_KPB_OriginalBookedDetail, KPB_SystemCreateTimeUtc, KPB_SystemCreateUser, KPB_SystemLastEditTimeUtc, KPB_SystemLastEditUser) VALUES ('52947AB4-CE64-45B7-821C-153EF78A15BB', '5CC363B6-B1D6-4882-A944-8F933BAAC429', '41585255-A47E-44C3-81E2-AA4571112394', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		public void TestDeletePackageHeaderWhereNotReferencedByPivot()
		{
			var parent1 = Guid.NewGuid();
			// job with loose id
			var packageJob_LoosePK = Guid.NewGuid();
			var packageJobHeaderPivotPK = Guid.NewGuid();
			var packageHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ1', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob_LoosePK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedByAPivotAndAPackage', GetUtcDate(), 'S1', GetUtcDate(), '~BP')", packageHeaderPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJobPackageHeaderPivot (KPJ_PK, KPJ_KJ_PackageJob, KPJ_KPH_PackageHeader, KPJ_SystemCreateTimeUtc, KPJ_SystemCreateUser, KPJ_SystemLastEditTimeUtc, KPJ_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJobHeaderPivotPK, packageJob_LoosePK, packageHeaderPK));

			// job with package with id
			var packageJob_DirectPK = Guid.NewGuid();
			var packagePK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ2', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob_DirectPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packagePK, packageJob_DirectPK, packageHeaderPK, "PKG"));

			RunSP(packagePK.ToString());
			AssertEquals("The Package should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", packagePK)));
			AssertEquals("The Header should remain as another Job is referencing it.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", packageHeaderPK)));
		}

		public void TestDeletePackageHeaderWhereNotReferencedByAnotherPackage()
		{
			// job with package with id
			var packageJob1 = Guid.NewGuid();
			var packageHeaderPK = Guid.NewGuid();
			var package1PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ1', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob1));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedBy2Packages', GetUtcDate(), 'S1', GetUtcDate(), '~BP')", packageHeaderPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", package1PK, packageJob1, packageHeaderPK, "PKG"));

			// job with package with same header as above
			var packageJob2 = Guid.NewGuid();
			var package2PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ2', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob2));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", package2PK, packageJob2, packageHeaderPK, "PKG"));

			RunSP(package2PK.ToString());
			AssertEquals("Package 1 should NOT have been deleted.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", package1PK)));
			AssertEquals("Package 2 should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", package2PK)));
			AssertEquals("Package Header should remain as it's used on another Package.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", packageHeaderPK)));
		}

		public void TestDoNotDeleteUnrelatedFloatingPackageHeader()
		{
			// job with package with id
			var packageJob = Guid.NewGuid();
			var packagePK = Guid.NewGuid();
			var packageHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ1', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedBy2Packages', GetUtcDate(), 'S1', GetUtcDate(), '~BP')", packageHeaderPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packagePK, packageJob, packageHeaderPK, "PKG"));

			var unrelatedPackageHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedBy0Packages', GetUtcDate(), 'S2', GetUtcDate(), '~BP')", unrelatedPackageHeaderPK));

			RunSP(packagePK.ToString());
			AssertEquals("Package should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", packagePK)));
			AssertEquals("Package Header should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", packageHeaderPK)));
			AssertEquals("Unrelated Package Header should remain as it's not related in anyway.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", unrelatedPackageHeaderPK)));
		}

		public void TestDoNotDeleteUnrelatedPackageHeaders()
		{
			// job with package with id
			var packageJob = Guid.NewGuid();
			var packagePK = Guid.NewGuid();
			var packageHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ1', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packageJob));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedBy2Packages', GetUtcDate(), 'S1', GetUtcDate(), '~BP')", packageHeaderPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", packagePK, packageJob, packageHeaderPK, "PKG"));

			// unrealted package job with package with id
			var unrelatedPackageJob = Guid.NewGuid();
			var unrelatedPackagePK = Guid.NewGuid();
			var unrelatedPackageHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_JobID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{0}', NewID(), 'PJ2', 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", unrelatedPackageJob));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{0}', 'ImReferencedBy0Packages', GetUtcDate(), 'S2', GetUtcDate(), '~BP')", unrelatedPackageHeaderPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{0}', '{1}', '{2}', 1, 1, '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", unrelatedPackagePK, unrelatedPackageJob, unrelatedPackageHeaderPK, "PKG"));

			RunSP(packagePK.ToString());
			AssertEquals("Package should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", packagePK)));
			AssertEquals("Package Header should have been deleted.", 0, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", packageHeaderPK)));
			AssertEquals("Unrelated Package should remain as it is not related in any way.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = '{0}'", unrelatedPackagePK)));
			AssertEquals("Unrelated Package Header should remain as it's not related in any way.", 1, TestConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = '{0}'", unrelatedPackageHeaderPK)));
		}

		static void RunSP(string pk)
		{
			short version = (short)Db.Connection.ExecuteScalar($"SELECT KP_AutoVersion FROM dbo.PkgPackage WHERE KP_PK = '{pk}'");
			using (var command = Db.Connection.Command("GlowDeletePkgPackage"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@PkgPackagePK", SqlDbType.UniqueIdentifier, new Guid(pk));
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}

		void InsertJobServiceLink(string pkgPackagePK)
		{
			var companyPK = Guid.NewGuid();
			var branchPK = Guid.NewGuid();
			var warehouseOrgPK = Guid.NewGuid();
			var addressPK = Guid.NewGuid();
			var wltPK = Guid.NewGuid();
			var warehousePK = Guid.NewGuid();
			var rcnPK = Guid.NewGuid();
			var jobServicePK = Guid.NewGuid();
			var jobServiceLinkPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{companyPK}', 'WTG', 'AU company', 'AU', 'AUD')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES ('{branchPK}', '{companyPK}', 'BR1')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) values ('{warehouseOrgPK}', 'WHS001')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) values ('{addressPK}', '{warehouseOrgPK}', 'Test Address')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsLocationType(WLT_PK, WLT_Code, WLT_Description, [WLT_SystemCreateTimeUtc], [WLT_SystemCreateUser], [WLT_SystemLastEditTimeUtc], [WLT_SystemLastEditUser]) VALUES ('{wltPK}', 'WLT', 'WHS Loc Type', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsWarehouse([WW_PK], [WW_GB_RelatedCompanyBranch], [WW_OA_WarehouseAddress], [WW_WLT_DefaultLocationType], [WW_WarehouseCode], [WW_DefaultOutboundDockDoor], [WW_DefaultInboundDockDoor], [WW_SystemCreateTimeUtc], [WW_SystemCreateUser], [WW_SystemLastEditTimeUtc], [WW_SystemLastEditUser]) VALUES ('{warehousePK}','{branchPK}','{addressPK}','{wltPK}','W01', NEWID(), NEWID(), GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsItemReceiveConsignment([WRC_PK], [WRC_ConsignmentID], [WRC_SystemCreateTimeUtc], [WRC_SystemCreateUser], [WRC_SystemLastEditTimeUtc], [WRC_SystemLastEditUser], [WRC_WW_IntendedWarehouse], [WRC_JobID]) VALUES ('{rcnPK}', 'RCN1', GETUTCDATE(), 'E', GETUTCDATE(), 'E', '{warehousePK}', 'RC00000001')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.JobService([ES_PK], [ES_ParentID], [ES_ParentTableCode], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser]) VALUES ('{jobServicePK}', '{rcnPK}', 'WRC', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.JobServiceLink([ESL_PK], [ESL_ParentTableCode], [ESL_ParentID], [ESL_ES_JobService], [ESL_SystemCreateTimeUtc], [ESL_SystemCreateUser], [ESL_SystemLastEditTimeUtc], [ESL_SystemLastEditUser]) VALUES ('{jobServiceLinkPK}', 'KP', '{pkgPackagePK}', '{jobServicePK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
		}
	}
}

