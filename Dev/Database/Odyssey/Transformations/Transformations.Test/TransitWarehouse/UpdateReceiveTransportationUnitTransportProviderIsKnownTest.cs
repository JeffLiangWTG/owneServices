using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(TransitWarehouse.UpdateReceiveTransportationUnitTransportProviderIsKnown))]
	class UpdateReceiveTransportationUnitTransportProviderIsKnown : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new TransitWarehouse.UpdateReceiveTransportationUnitTransportProviderIsKnown();

		protected override string ReasonNotToBeMapped => "This transform must be mapped along with changes to WhsTransitPackageSecureChangeExtender in WI00841840";

		OrgHeader client;
		OrgAddress orgAddress;
		OrgAddress nonApprovedOrgAddress;
		GlbBranch registeredBranch;
		WhsItemReceiveTransportationUnit rtu;
		SqlQueryBuilder sql;
		const string SignedBy = "My Driver";

		const string DriverSecurityCertificationCheckingActivated = "DriverSecurityCertificationCheckingActivated";

		#region Transform Setup

		protected void SetupTestData(string driverName = SignedBy, string warehouseRegion = "AU", DateTimeOffset? gateOutTime = null)
		{
			sql = new SqlQueryBuilder();

			var unloadTime = gateOutTime?.AddHours(-1);

			var data = CreateTestWarehouseData(sql, "CP1", "BR1", warehouseRegion, "WH1");

			registeredBranch = data.Branch;

			client = new OrgHeader("CL1ENT1").AppendInsertAndReturnObject(sql);

			rtu = new WhsItemReceiveTransportationUnit(data.Warehouse, "RTU", data.StagingLocation, "RTU") { WRH_SignedBy = driverName, WRH_GateInTime = unloadTime?.AddHours(-1), WRH_UnloadCompleteTime = unloadTime, WRH_UnloadCompleteNotYetProcessedTime = unloadTime, WRH_GateOutTime = gateOutTime }.AppendInsertAndReturnObject(sql);

			orgAddress = new OrgAddress(client, "Pitt1 Street", "200 Pitt1 Street").AppendInsertAndReturnObject(sql);
			nonApprovedOrgAddress = new OrgAddress(client, "Test Street", "99 Test Street").AppendInsertAndReturnObject(sql);

			new JobDocAddress(rtu.PK, "WRH", "TRA") { E2_OA_Address = orgAddress.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();
		}

		#endregion

		#region Basic Test

		protected override void PrepareTestData()
		{
			SetupTestData();
			SetupKnown();
			SetupDriver();
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(1, WhsItemReceiveTransportationUnit.CountInDB(TestConnection, rtu => rtu.WRH_TransportProviderIsKnown));
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_1] ON [dbo].[OrgContact] ([OC_ContactName]) INCLUDE ([OC_PK]) WHERE ([OC_ContactName]<>'') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_2] ON [dbo].[GenRegCertAccredMaintList] ([XZ_ParentID], [XZ_ExpiryOrDueDate], [XZ_ParentTableCode], [XZ_Type]) WHERE ([XZ_ParentTableCode]='OC' AND [XZ_Type]='BKG') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_3] ON [dbo].[GenRegCertAccredMaintList] ([XZ_ParentID], [XZ_ExpiryOrDueDate], [XZ_ParentTableCode], [XZ_Type]) WHERE ([XZ_ParentTableCode]='OC' AND [XZ_Type]='DTA') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_4] ON [dbo].[JobDocAddress] ([E2_AddressType], [E2_ParentTableCode]) INCLUDE ([E2_OA_Address], [E2_ParentID]) WHERE ([E2_AddressType]='TRA' AND [E2_ParentTableCode]='WRH') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_5] ON [dbo].[WhsItemReceiveTransportationUnit] ([WRH_GateOutTime]) INCLUDE ([WRH_AutoVersion], [WRH_PK], [WRH_SignedBy], [WRH_SystemLastEditTimeUtc], [WRH_SystemLastEditUser], [WRH_WW_Warehouse]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification_6] ON [dbo].[OrgCountryData] ([OV_EXApprovalExpiryDate], [OV_EXApprovedOrMajorExporter], [OV_OH_OrgHeader], [OV_RN_NKClientCountryRelation], [OV_OA_ApprovedLocation]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		#endregion

		#region TestBulk

		public void TestBulk()
		{
			const int ExpectedRTUs = 3000;

			var sql = new SqlQueryBuilder();
			var data = CreateTestWarehouseData(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			var certs = new DriverTestCaseCert[]
			{
				new (DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)),
				new (DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),
			};

			var clients = new List<OrgHeader>();
			var rtus = new List<WhsItemReceiveTransportationUnit>();
			var orgAddresses = new List<OrgAddress>();
			var jobDocAddresses = new List<JobDocAddress>();
			var orgCountryDatas = new List<OrgCountryData>();
			var orgContacts = new List<OrgContact>();
			var newCerts = new List<GenRegCertAccredMaintList>();

			var expireDate = DateTime.Now.AddDays(30);
			for (int i = 0; i < ExpectedRTUs; i++)
			{
				var testcaseCode = Convert.ToString(i, 16).ToUpper().PadLeft(3, '0');

				var client = new OrgHeader(testcaseCode);
				var rtu = new WhsItemReceiveTransportationUnit(data.Warehouse, testcaseCode, data.StagingLocation, testcaseCode) { WRH_SignedBy = SignedBy };

				var orgAddress = new OrgAddress(client, "Pitt Street", "200 Pitt Street");
				var jobDocAddress = new JobDocAddress(rtu.PK, "WRH", "TRA") { E2_OA_Address = orgAddress.PK };

				var orgData = new OrgCountryData(client, TransportCertTypeCodes.CertifiedHaulier, expireDate, nkClientCountryRelation: "AU");

				var contact = new OrgContact(client) { OC_ContactName = SignedBy };

				foreach (var cert in certs)
				{
					newCerts.Add(new GenRegCertAccredMaintList(cert.CertOrAccreditationType, DateTime.Now.Add(cert.ExpireOffset), "OC", contact.PK));
				}

				clients.Add(client);
				rtus.Add(rtu);
				orgAddresses.Add(orgAddress);
				jobDocAddresses.Add(jobDocAddress);
				orgCountryDatas.Add(orgData);
				orgContacts.Add(contact);
			}

			InsertStmDataBool(DriverSecurityCertificationCheckingActivated, true);
			sql.AppendLine(OrgHeader.GetBulkInsertStatement(clients));
			sql.AppendLine(WhsItemReceiveTransportationUnit.GetBulkInsertStatement(rtus));
			sql.AppendLine(OrgAddress.GetBulkInsertStatement(orgAddresses));
			sql.AppendLine(JobDocAddress.GetBulkInsertStatement(jobDocAddresses));
			sql.AppendLine(OrgCountryData.GetBulkInsertStatement(orgCountryDatas));
			sql.AppendLine(OrgContact.GetBulkInsertStatement(orgContacts));
			sql.AppendLine(GenRegCertAccredMaintList.GetBulkInsertStatement(newCerts));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run();

			AssertEquals(ExpectedRTUs, WhsItemReceiveTransportationUnit.CountInDB(TestConnection, rtu => rtu.WRH_TransportProviderIsKnown));
		}

		#endregion

		#region TestIsOfflinePostUpgradeTransform

		public void TestIsOfflinePostUpgradeTransform()
		{
			PrepareTestData();

			var transformation = GetNewTestTransformationInstance();

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals(0, WhsItemReceiveTransportationUnit.CountInDB(TestConnection, rtu => rtu.PK == rtu.PK && rtu.WRH_TransportProviderIsKnown));

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals(1, WhsItemReceiveTransportationUnit.CountInDB(TestConnection, rtu => rtu.PK == rtu.PK && rtu.WRH_TransportProviderIsKnown));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertEquals(1, WhsItemReceiveTransportationUnit.CountInDB(TestConnection, rtu => rtu.PK == rtu.PK && rtu.WRH_TransportProviderIsKnown));
		}

		#endregion

		#region TestOrg

		public void TestOrg_ActiveCertifiedHaulierIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), true);
		public void TestOrg_ActiveCertifiedHaulierAtUnapprovedLocationIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), true, isApprovedLocation: false);
		public void TestOrg_ExpiredCertifiedHaulierIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(-30), false);

		public void TestOrg_ActiveApprovedHaulierIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.ApprovedHaulier, TimeSpan.FromDays(30), true);
		public void TestOrg_ActiveApprovedHaulierAtUnapprovedLocationIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.ApprovedHaulier, TimeSpan.FromDays(30), true, isApprovedLocation: false);
		public void TestOrg_ExpiredApprovedHaulierIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.ApprovedHaulier, TimeSpan.FromDays(-30), false);
		public void TestOrg_ActiveRegulatedAgentWhenAtApprovedLocationIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.RegulatedAgentACENotice, TimeSpan.FromDays(30), true, isApprovedLocation: true);
		public void TestOrg_ActiveRegulatedAgentWhenAtUnapprovedLocationIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.RegulatedAgentACENotice, TimeSpan.FromDays(30), false, isApprovedLocation: false);
		public void TestOrg_ExpiredRegulatedAgentWhenAtApprovedLocationIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.RegulatedAgentACENotice, TimeSpan.FromDays(-30), false, isApprovedLocation: true);
		public void TestOrg_ActiveRegulatedAgentWhenCertifiedLocationIsUnknown() => AssertOrgIsTrusted(TransportCertTypeCodes.RegulatedAgentACENotice, TimeSpan.FromDays(30), false, isNullApprovedLocation: true, isApprovedLocation: false);
		public void TestOrg_ActiveUnrelatedCertificationIsUntrusted() => AssertOrgIsTrusted("ZZZ", TimeSpan.FromDays(30), false);

		public void TestOrg_CountryWithActiveCertificationInSameCountryIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), true, warehouseRegion: "AU", certificationRegion: "AU");
		public void TestOrg_CountryWithActiveCertificationInDifferentCountryIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), false, warehouseRegion: "AU", certificationRegion: "NZ");
		public void TestOrg_CountryInEUWithActiveEUCertificationIsTrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), true, warehouseRegion: "HU", certificationRegion: "EU");
		public void TestOrg_CountryInEUWithActiveNonEUCertificationIsUntrusted() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), false, warehouseRegion: "HU", certificationRegion: "HU");
		public void TestOrg_UnitedKingdowWithActiveGBCertificationIsTrusted_GBIsExcludedFromEU() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), true, warehouseRegion: "GB", certificationRegion: "GB");
		public void TestOrg_UnitedKingdowWithActiveEUCertificationIsUntrusted_GBIsExcludedFromEU() => AssertOrgIsTrusted(TransportCertTypeCodes.CertifiedHaulier, TimeSpan.FromDays(30), false, warehouseRegion: "GB", certificationRegion: "EU");

		public void AssertOrgIsTrusted(string certificationType, TimeSpan expireOffset, bool expectedIsTrusted, bool isNullApprovedLocation = false, bool isApprovedLocation = true, string warehouseRegion = "AU", string certificationRegion = "AU")
		{
			SetupTestData(warehouseRegion: warehouseRegion);
			SetupDriver();
			SetupKnown(certificationType, expireOffset, expectedIsTrusted, isNullApprovedLocation, isApprovedLocation, warehouseRegion, certificationRegion);

			GetNewTestTransformationInstance().Run();
			var expectedRTUTrustedCount = expectedIsTrusted ? 1 : 0;
			var actualRTUTrustedCount = WhsItemReceiveTransportationUnit.CountInDB(TestConnection, row => row.PK == rtu.PK && row.WRH_TransportProviderIsKnown);
			AssertEquals(expectedRTUTrustedCount, actualRTUTrustedCount);
		}

		public void SetupKnown(string certificationType = "CH", TimeSpan? expireOffset = null, bool expectedIsTrusted = true, bool isNullApprovedLocation = false, bool isApprovedLocation = true, string warehouseRegion = "AU", string certificationRegion = "AU")
		{
			expireOffset ??= TimeSpan.FromDays(1);

			var approvedLocation = isApprovedLocation ? orgAddress : isNullApprovedLocation ? null : nonApprovedOrgAddress;

			new OrgCountryData(client, certificationType, DateTime.Now.Add(expireOffset.Value), approvedLocation, certificationRegion).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();
		}

		#endregion

		#region TestDriver

		public void TestDriver_ActiveBKGAndActiveDTAIsTrusted () => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], true);
		public void TestDriver_ExpiredBKGAndActiveDTAIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(-30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], false);
		public void TestDriver_ActiveBKGAndExpiredDTAIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(-30)),], false);
		public void TestDriver_ExpiredBKGAndExpiredDTAIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(-30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(-30)),], false);
		public void TestDriver_OnlyActiveBKGIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30))], false);
		public void TestDriver_OnlyActiveDTAIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30))], false);
		public void TestDriver_OnlyActiveUnrelatedCertIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert("ZZZ", TimeSpan.FromDays(30))], false);
		public void TestDriver_ActiveBKGAndActiveUnrelatedCertIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert("ZZZ", TimeSpan.FromDays(30)),], false);
		public void TestDriver_ActiveDTAAndActiveUnrelatedCertIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)), new DriverTestCaseCert("ZZZ", TimeSpan.FromDays(30)),], false);

		public void TestDriver_WithSystemOnlyRegistryEnabledIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], false, setSystemRegistryValue: true, systemRegistryValue: true);
		public void TestDriver_WithSystemRegistryDisabledIsNotChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], true, setSystemRegistryValue: true, systemRegistryValue: false);
		public void TestDriver_WithSystemRegistrySetToNullIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: true, systemRegistryValue: null);
		public void TestDriver_WithSystemAndBranchRegistrySetToNullIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: true, systemRegistryValue: null, setBranchRegistryValue: true, branchRegistryValue: null);
		public void TestDriver_WithBranchRegistrySetToNullIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: false, setBranchRegistryValue: true, branchRegistryValue: null);
		public void TestDriver_WithNoRegistrySetIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: false);
		public void TestDriver_WithBranchOnlyRegistryEnabledIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], false, setSystemRegistryValue: false, setBranchRegistryValue: true, branchRegistryValue: true);
		public void TestDriver_WithSystemRegistryEnabledAndBranchRegistryDisabledIsNotChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], true, setSystemRegistryValue: true, systemRegistryValue: true, setBranchRegistryValue: true, branchRegistryValue: false);
		public void TestDriver_WithSystemRegistryEnabledAndBranchRegistryEnabledIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: true, systemRegistryValue: true, setBranchRegistryValue: true, branchRegistryValue: true);
		public void TestDriver_WithSystemRegistryDisabledAndBranchRegistryEnabledIsChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], false, setSystemRegistryValue: true, systemRegistryValue: false, setBranchRegistryValue: true, branchRegistryValue: true);
		public void TestDriver_WithSystemRegistryDisabledAndBranchRegistryDisabledIsNotChecked() => AssertDriverIsTrusted([new DriverTestCaseCert("NAN", TimeSpan.FromDays(30))], true, setSystemRegistryValue: true, systemRegistryValue: false, setBranchRegistryValue: true, branchRegistryValue: false);
		public void TestDriver_WithActiveCertsAndMatchingNameIsTrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], true, certContactName: "Cert Driver",  rtuDriverName: "Cert Driver");
		public void TestDriver_WithActiveCertsAndDifferentNameIsUntrusted() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], false, certContactName: "Cert Driver");
		public void TestDriver_WithRegistryDisabledStillChecksKnownWithEmptySignedName() => AssertDriverIsTrusted([new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)), new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),], true, certContactName: "Cert Driver", rtuDriverName: "", setSystemRegistryValue: true, systemRegistryValue: false);

		public void AssertDriverIsTrusted(DriverTestCaseCert[] certs, bool expectedIsTrusted, string rtuDriverName = SignedBy, string certContactName = SignedBy, bool setSystemRegistryValue = true, bool setBranchRegistryValue = false, bool? systemRegistryValue = true, bool? branchRegistryValue = true)
		{
			SetupTestData(rtuDriverName);
			SetupKnown();
			SetupDriver(certs, expectedIsTrusted, certContactName, setSystemRegistryValue, setBranchRegistryValue, systemRegistryValue, branchRegistryValue);

			GetNewTestTransformationInstance().Run();
			var expectedRTUTrustedCount = expectedIsTrusted ? 1 : 0;
			var actualRTUTrustedCount = WhsItemReceiveTransportationUnit.CountInDB(TestConnection, row => row.PK == rtu.PK && row.WRH_TransportProviderIsKnown);
			AssertEquals(expectedRTUTrustedCount, actualRTUTrustedCount);
		}

		public void SetupDriver(DriverTestCaseCert[] certs = null, bool expectedIsTrusted = true, string certContactName = SignedBy, bool setSystemRegistryValue = true, bool setBranchRegistryValue = false, bool? systemRegistryValue = true, bool? branchRegistryValue = true)
		{
			certs ??=
			[
				new DriverTestCaseCert(DriverCertTypeCodes.BKG, TimeSpan.FromDays(30)),
				new DriverTestCaseCert(DriverCertTypeCodes.DTA, TimeSpan.FromDays(30)),
			];

			if (setSystemRegistryValue)
			{
				InsertStmDataBool(DriverSecurityCertificationCheckingActivated, systemRegistryValue);
			}

			if (setBranchRegistryValue)
			{
				InsertStmDataBool(DriverSecurityCertificationCheckingActivated, branchRegistryValue, registeredBranch.PK);
			}

			var contact = new OrgContact(client) { OC_ContactName = certContactName }.AppendInsertAndReturnObject(sql);
			foreach (var cert in certs)
			{
				new GenRegCertAccredMaintList(cert.CertOrAccreditationType, DateTime.Now.Add(cert.ExpireOffset), "OC", contact.PK).AppendInsertAndReturnObject(sql);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();
		}

		#endregion

		#region TestRTU_GateInTime

		public void Test_WithGateOutTime_InclusiveWithin30Days() => AssertGateOutFilter(DateTime.Now.AddDays(-30).Date, true);
		public void Test_WithGateOutTime_Outside30Days() => AssertGateOutFilter(DateTime.Now.AddDays(-30).Date.AddMinutes(-1), false);
		public void Test_WithGateOutTime_Null() => AssertGateOutFilter(null, true);

		public void AssertGateOutFilter(DateTimeOffset? gateOutTime, bool expectedIsTrusted)
		{
			SetupTestData(gateOutTime: gateOutTime);
			SetupKnown();
			SetupDriver();

			GetNewTestTransformationInstance().Run();
			var expectedRTUTrustedCount = expectedIsTrusted ? 1 : 0;
			var actualRTUTrustedCount = WhsItemReceiveTransportationUnit.CountInDB(TestConnection, row => row.PK == rtu.PK && row.WRH_TransportProviderIsKnown);
			AssertEquals(expectedRTUTrustedCount, actualRTUTrustedCount);
		}

		#endregion

		#region TestOrgAndDriver_CertifiedHauler_AndBKGAndDTA

		public void TestOrgAndDriver_CertifiedHauler_AndBKGAndDTA()
		{
			InsertStmDataBool(DriverSecurityCertificationCheckingActivated, true);

			var sql = new SqlQueryBuilder();
			var data = CreateTestWarehouseData(sql, "AU");

			var client = new OrgHeader("CLI").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(data.Warehouse, "RTU", data.StagingLocation, "RTU") { WRH_SignedBy = "My Driver" }.AppendInsertAndReturnObject(sql);

			var orgAddress = new OrgAddress(client, "Pitt Street", "200 Pitt Street").AppendInsertAndReturnObject(sql);
			var jobDocAddress = new JobDocAddress(rtu.PK, "WRH", "TRA") { E2_OA_Address = orgAddress.PK }.AppendInsertAndReturnObject(sql);

			var orgData = new OrgCountryData(client, TransportCertTypeCodes.ApprovedHaulier, DateTime.Now.AddDays(30), nkClientCountryRelation: "AU").AppendInsertAndReturnObject(sql);

			var contact = new OrgContact(client) { OC_ContactName = "My Driver" }.AppendInsertAndReturnObject(sql);
			new GenRegCertAccredMaintList(DriverCertTypeCodes.BKG, DateTime.Now.AddDays(30), "OC", contact.PK).AppendInsertAndReturnObject(sql);
			new GenRegCertAccredMaintList(DriverCertTypeCodes.DTA, DateTime.Now.AddDays(30), "OC", contact.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			GetNewTestTransformationInstance().Run();

			var expectedRTUTrustedCount = 1;
			var actualRTUTrustedCount = WhsItemReceiveTransportationUnit.CountInDB(TestConnection, row => row.PK == rtu.PK && row.WRH_TransportProviderIsKnown);

			AssertEquals(expectedRTUTrustedCount, actualRTUTrustedCount);
		}

		#endregion

		#region Implementation

		(GlbCompany Company, GlbBranch Branch, WhsWarehouse Warehouse, WhsLocation StagingLocation) CreateTestWarehouseData(SqlQueryBuilder sql, string companyCode = "CMP", string branchCode = "BRN", string region = "AU", string warehouseCode = "WHS")
		{
			var company = new GlbCompany(companyCode, region).AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch(branchCode, company.PK).AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse(warehouseCode, "PRW", branch.PK).WithDockDoor(sql);
			var row1 = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var stagingLocation = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			return new(company, branch, whs, stagingLocation);
		}

		void InsertStmDataBool(string key, bool? val, Guid? branch = null)
		{
			var helper = new RegistryTransformationHelper();
			var branchValue = branch ?? Guid.Empty;

			_ = val.HasValue ?
				helper.InsertStmDataRow(key, branchValue, "BOL", Encoding.Unicode.GetBytes(val.Value ? bool.TrueString : bool.FalseString))
				: helper.InsertStmDataRow(key, branchValue,"BOL", null);
		}

		static class DriverCertTypeCodes
		{
			public const string BKG = "BKG";
			public const string DTA = "DTA";
		}

		static class TransportCertTypeCodes
		{
			public const string CertifiedHaulier = "CH";
			public const string ApprovedHaulier = "AH";
			public const string RegulatedAgentACENotice = "RA";
			public const string RegulatedAgentEACENotice = "RE";
			public const string AccreditedAgent = "AA";
			public const string KnownConsignor = "KC";
			public const string AccountConsignor = "AC";
			public const string RegularCustomer = "RC";
			public const string NoCertAssigned = "NO";
		}

		public class DriverTestCaseCert
		{
			public DriverTestCaseCert(string certOrAccreditationType, TimeSpan expiryOffset)
			{
				CertOrAccreditationType = certOrAccreditationType;
				ExpireOffset = expiryOffset;
			}
			public string CertOrAccreditationType;
			public TimeSpan ExpireOffset;
		}

		#endregion
	}
}
