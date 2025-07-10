using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(GBCustomsDataRegistry))]
	class GBCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<GBCustomsDataRegistry>
	{
		public void TestCcsukAllowCheckinAtMawbLevel()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukAllowCheckinAtMawbLevel,
				"GBCcsukAllowCheckinAtMawbLevel",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
				"Allow check-in at MAWB level?",
				"Option to Bulk Release / Delivery of MAWB & HAWBs in ETSF.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestCdsEntryLocalReferenceNumberCustomisation()
		{
			TestGenericRegistryItem(
				ItemSet.CdsEntryLocalReferenceNumberCustomisationFromCW1,
				"CdsEntryLocalReferenceNumberCustomisationFromCW1",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"CDS Entry Local Reference Number Customization",
				"Override this value to customize how CDS Entry Local Reference Numbers are formatted",
				RegistryStorageFlags.All);

			var dataType = (BillCustomisationRegistryDataType)ItemSet.CdsEntryLocalReferenceNumberCustomisationFromCW1.DataType;
			AssertEquals("GeneratedNumberName", "CDS Entry Local Reference Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", GBCustomsDataRegistry.CDSEntryLocalReferenceNumberMaxLength, dataType.MaxLength);
			AssertEquals("13", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode].Include);
			AssertEquals(new ZByte(1), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode].Order);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include);
			AssertEquals(new ZByte(2), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ServerCode].Include);
			AssertEquals(new ZByte(3), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ServerCode].Order);
		}

		void PrepareNotificationGroupsForTest(ref ZGuid defaultGroupPk, ref ZGuid level1GroupPk, ref ZGuid level2GroupPk, ref ZGuid level3GroupPk)
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUser.GS_EmailAddress = "testing@budwise.com";
			currentUser.Groups.RemoveAll();

			defaultGroupPk = AddGroupForCurrentUser(currentUser, "Def", "Default");
			level1GroupPk = AddGroupForCurrentUser(currentUser, "SG1", "Group1");
			level2GroupPk = AddGroupForCurrentUser(currentUser, "SG2", "Group2");
			level3GroupPk = AddGroupForCurrentUser(currentUser, "SG3", "Group3");

			Factory.Save();
		}

		ZGuid AddGroupForCurrentUser(GlbStaff currentUser, ZString groupCode, ZString groupDecription)
		{
			var group = currentUser.Groups.AddNew();
			group.GG_Code = groupCode;
			group.GG_Desc = groupDecription;
			return group.PK;
		}

		public void TestSpecificNotificationFallbackForChiefE0()
		{
			ZGuid defaultNotificationGroupPk = ZGuid.Empty;
			ZGuid level1NotificationGroupPk = ZGuid.Empty;
			ZGuid level2NotificationGroupPk = ZGuid.Empty;
			ZGuid level3NotificationGroupPk = ZGuid.Empty;
			PrepareNotificationGroupsForTest(ref defaultNotificationGroupPk, ref level1NotificationGroupPk, ref level2NotificationGroupPk, ref level3NotificationGroupPk);
			AssertNotEquals("Pre-req", ZGuid.Empty, defaultNotificationGroupPk);
			AssertNotEquals("Pre-req", ZGuid.Empty, level1NotificationGroupPk);
			AssertNotEquals("Pre-req", ZGuid.Empty, level2NotificationGroupPk);
			AssertNotEquals("Pre-req", ZGuid.Empty, level3NotificationGroupPk);
			AssertEquals("Pre-req", Core.Constants.Groups.PostMastersGroupPK, GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Pre-req", Core.Constants.Groups.PostMastersGroupPK, GBCustomsDataRegistry.Instance.NotificationChief.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Pre-req", Core.Constants.Groups.PostMastersGroupPK, GBCustomsDataRegistry.Instance.NotificationChiefPrints.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Pre-req", Core.Constants.Groups.PostMastersGroupPK, GBCustomsDataRegistry.Instance.NotificationChiefPrintsE0.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultNotificationGroupPk.ToGuid());
			AssertEquals(defaultNotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChiefPrintsE2.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			GBCustomsDataRegistry.Instance.NotificationChief.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, level1NotificationGroupPk.ToGuid());
			AssertEquals(level1NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChiefPrintsE2.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			GBCustomsDataRegistry.Instance.NotificationChiefPrints.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, level2NotificationGroupPk.ToGuid());
			AssertEquals(level2NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChiefPrintsE2.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(level1NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChief.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			GBCustomsDataRegistry.Instance.NotificationChiefPrintsE2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, level3NotificationGroupPk.ToGuid());
			AssertEquals(level3NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChiefPrintsE2.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(level2NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChiefPrints.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(level1NotificationGroupPk, GBCustomsDataRegistry.Instance.NotificationChief.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals(level3NotificationGroupPk, GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPrints, "E2", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(level2NotificationGroupPk, GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPrints, "AA", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestSpecificPrintersFallbackToGenericPrinter()
		{
			var sharedPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sharedPrinter);
			AssertEquals(sharedPrinter, GBCustomsDataRegistry.Instance.PrinterShared.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(sharedPrinter, GBCustomsDataRegistry.Instance.PrinterCcsuk.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(sharedPrinter, GBCustomsDataRegistry.Instance.PrinterChief_E2.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)); //Please Change
			var chiefPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterChief_E2.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, chiefPrinter);
			var ccsukPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, ccsukPrinter);
			AssertEquals(ccsukPrinter, GBCustomsDataRegistry.Instance.PrinterCcsuk.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(chiefPrinter, GBCustomsDataRegistry.Instance.PrinterChief_E2.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(sharedPrinter, GBCustomsDataRegistry.Instance.PrinterShared.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
		}

		public void TestAllChiefPrinters()
		{
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E1, "DTI-E1");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E2, "DTI-E2-CRAP");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E5, "DTI-E5-R");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E7, "DTI-E7-AMD");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E8, "DTI-E8-CRUD");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_E9, "DTI-E9-");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_H2, "DTI-H2-----");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_H3, "DTI-H3XXXXX");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_H5, "DTI-H5");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_H7, "DTI-H7");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_N1, "DTI-N1");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_N3, "DTI-N3");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_N4, "DTI-N4");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_N6, "DTI-N6");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_P2, "DTI-P2");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_P3, "DTI-P3");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_P5, "DTI-P5");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_P7, "DTI-P7");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_P9, "DTI-P9");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S0, "DTI-S0");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S1, "DTI-S1");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S3, "DTI-S3");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S4, "DTI-S4");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S5, "DTI-S5");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S6, "DTI-S6");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S8, "DTI-S8");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_S9, "DTI-S9");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X0, "DTI-X0");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X1, "DTI-X1");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X2, "DTI-X2");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X5, "DTI-X5");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X6, "DTI-X6");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X7, "DTI-X7");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X8, "DTI-X8");
			RunChiefPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief_X9, "DTI-X9");
		}

		void RunChiefPrinterTest(IRegistryItem printerItemBeingTested, string sampleReportCode)
		{
			var sharedPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sharedPrinter);
			AssertEquals("Default value is the shared printer", sharedPrinter, printerItemBeingTested.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)); //Please Change
			var specificPrinter = Guid.NewGuid();
			printerItemBeingTested.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, specificPrinter);
			AssertEquals("Now we have a specific value", specificPrinter, printerItemBeingTested.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("Shared printer unaffected", sharedPrinter, GBCustomsDataRegistry.Instance.PrinterShared.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("We can get this specific value by the report code", specificPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief(sampleReportCode).GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("For a crap report code we fallback to the shared printer", sharedPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief("CRAP-XX-CRAP").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
		}

		public void TestMoreChiefPrinterTests()
		{
			var sharedPrinter = Guid.NewGuid();
			var specificPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sharedPrinter);
			GBCustomsDataRegistry.Instance.PrinterChief_E1.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, specificPrinter);
			AssertEquals("Report code without any prefixes is still found correctly", specificPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief("E1").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("For a totally crap report code we fallback to the shared printer", sharedPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief("").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("For a totally crap report code we fallback to the shared printer", sharedPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief("*").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("For a totally crap report code we fallback to the shared printer", sharedPrinter, GBCustomsDataRegistry.Instance.GetPrinterForChief("******").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
		}

		public void TestAllCcsukPrinters()
		{
			RunCcsukPrinterTest(GBCustomsDataRegistry.Instance.PrinterCcsuk_C1, "C1");
			RunCcsukPrinterTest(GBCustomsDataRegistry.Instance.PrinterCcsuk_RRA, "RRA");
			// Test P5 Printer
			RunCcsukPrinterTest(GBCustomsDataRegistry.Instance.PrinterCcsuk_P5, "P5");
		}

		void RunCcsukPrinterTest(IRegistryItem printerItemBeingTested, string sampleReportCode)
		{
			var sharedPrinter = Guid.NewGuid();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sharedPrinter);
			AssertEquals("Default value is the shared printer", sharedPrinter, printerItemBeingTested.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)); //Please Change
			var specificPrinter = Guid.NewGuid();
			printerItemBeingTested.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, specificPrinter);
			AssertEquals("Now we have a specific value", specificPrinter, printerItemBeingTested.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("Shared printer unaffected", sharedPrinter, GBCustomsDataRegistry.Instance.PrinterShared.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("We can get this specific value by the report code", specificPrinter, GBCustomsDataRegistry.Instance.GetPrinterForCcsuk(sampleReportCode).GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals("For a crap report code we fallback to the shared printer", sharedPrinter, GBCustomsDataRegistry.Instance.GetPrinterForCcsuk("CRAP-XX-CRAP").GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
		}

		public void TestLiveTestFlags()
		{
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CnsPrintsUrl = "CnsPrintsUrltest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CnsPrintsUrl = "CnsPrintsUrllive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CnsPrintsUrltest", GBCustomsDataRegistry.Instance.CnsPrintsUrl);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CnsPrintsUrllive", GBCustomsDataRegistry.Instance.CnsPrintsUrl);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl = "CnsCDSCheckCredentialsUrlTest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl = "CnsCDSCheckCredentialsUrlLive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CnsCDSCheckCredentialsUrlTest", GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CnsCDSCheckCredentialsUrlLive", GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.McpDestin8Url = "https://test";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.McpDestin8Url = "https://live";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("https://test", GBCustomsDataRegistry.Instance.McpDestin8Url);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("https://live", GBCustomsDataRegistry.Instance.McpDestin8Url);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl = "McpCDSCheckCredentialsUrlTest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl = "McpCDSCheckCredentialsUrlLive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("McpCDSCheckCredentialsUrlTest", GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("McpCDSCheckCredentialsUrlLive", GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = "CcsukRemoteIpAddresstest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = "CcsukRemoteIpAddresslive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CcsukRemoteIpAddresstest", GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CcsukRemoteIpAddresslive", GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN = "CcsukRemoteIpAddresstest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN = "CcsukRemoteIpAddresslive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CcsukRemoteIpAddresstest", GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CcsukRemoteIpAddresslive", GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukRemotePort = 10;
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CcsukRemotePort = 20;
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(10, GBCustomsDataRegistry.Instance.CcsukRemotePort);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(20, GBCustomsDataRegistry.Instance.CcsukRemotePort);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CnsUploadUrl = "CnsUploadUrltest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.CnsUploadUrl = "CnsUploadUrllive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CnsUploadUrltest", GBCustomsDataRegistry.Instance.CnsUploadUrl);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CnsUploadUrllive", GBCustomsDataRegistry.Instance.CnsUploadUrl);

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.NesEmailAddress = "NesEmailAddresstest";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.NesEmailAddress = "NesEmailAddresslive";
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("NesEmailAddresstest", GBCustomsDataRegistry.Instance.NesEmailAddress);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("NesEmailAddresslive", GBCustomsDataRegistry.Instance.NesEmailAddress);
		}

		public void TestEnforceCHIEFRulesNotAllowed()
		{
			TestRegistryItem(ItemSet.EnforceCHIEFRulesNotAllowed, "GBEnforceCHIEFRulesNotAllowed", GBCustomsDataRegistry.Categories.Customs_UnitedKingdom, "Enforce CHIEF Rules Not Allowed", "Determines whether a message being sent with fields that violate CHIEF rules for 'Not Allowed' fields will still be sent with their provided values, or have their values overridden.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestReportMessageMissingMandatoryField()
		{
			TestRegistryItem(ItemSet.ReportMessageMissingMandatoryField, "GBReportMessageMissingMandatoryField", GBCustomsDataRegistry.Categories.Customs_UnitedKingdom, "Report Message Missing Mandatory Field", "Determines whether a message being sent with a missing mandatory field will generate an error and disallow sending of the declaration to Customs.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestHmrcExchangeRates()
		{
			// These base tests are flawed and useless.....

			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.HmrcExchangeRateBaseUrlHost,
				GBCustomsDataRegistry.Instance.HmrcExchangeRateBaseUrlHost.Name,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateBaseUrlHost.Category,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateBaseUrlHost.Caption,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateBaseUrlHost.Hint,  // This is not what I am testing
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				"https://www.gov.uk");

			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.HmrcExchangeRateYearlyRelativeUrlWithoutYear,
				GBCustomsDataRegistry.Instance.HmrcExchangeRateYearlyRelativeUrlWithoutYear.Name,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateYearlyRelativeUrlWithoutYear.Category,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateYearlyRelativeUrlWithoutYear.Caption,  // This is not what I am testing
				GBCustomsDataRegistry.Instance.HmrcExchangeRateYearlyRelativeUrlWithoutYear.Hint,  // This is not what I am testing
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				"/government/publications/hmrc-exchange-rates-for-{0}-monthly");
		}

		public void TestCdsCcsukRegistryItems()
		{
			AssertEquals("CUKCTM98CDSUSR", GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdImport.Value);
			AssertEquals("CUKCTM98CDSUSR", GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdExport.Value);
		}

		public void TestGetCDSCCSUKRecipient()
		{
			GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdExport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XYZ");

			var interchangeId = ZGuid.NewZGuid();
			var extCorrelationId = ((ZString)interchangeId.ToString()).KeepAlphanumericCharacters();
			var recipient = GBCustomsDataRegistry.Instance.GetCdsCcsukProcessingInstruction(true, "123", extCorrelationId);
			AssertEquals($@"<?ccsuk senderid=""123"" recipientid=""ABC"" ext-correlation-id=""{extCorrelationId}""?>", recipient);

			recipient = GBCustomsDataRegistry.Instance.GetCdsCcsukProcessingInstruction(false, "123", extCorrelationId);
			AssertEquals($@"<?ccsuk senderid=""123"" recipientid=""XYZ"" ext-correlation-id=""{extCorrelationId}""?>", recipient);
		}

		public void TestBadgeCodes()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.BadgeCodes,
				"GBCustomsBadgeCodes",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Badge Codes",
				"Badge Codes registered against a branch along with the associated Port Code and CSP/Gateway details. N.B. To provide a default badge to be used for all imports, or all exports, regardless of the port through which the job moves, enter a row with a blank port code and an appropriate 'Direction'. You may mix-and-match, so that a given port may use one badge for imports and another for exports.",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue);
		}

		public void TestCredentials()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.Credentials,
				"GbCustomsWebServiceCredentials",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Badge code credentials",
				@"Explanation of columns...

* Badge code (mnemonic): the (friendly) badge code as entered in the Badge Codes section of the registry. After editing the Badge Codes section, ensure changes are saved before editing this column, and ensure you are logged in as the correct company.

* Badge/Company or NES Role: your 3-character badge code, or for NES your role (e.g. THS1ABC). For Pentant this is your full 6-character badge code, e.g. PNTABC. If you are not using friendly/mnemonic names, this column matches the first column.

* Output Device/NES Location/PIMA/CDS Topic:
---- For MCP and CNS under CHIEF this is the EDI 'mailbox' or 'printer' to which CHIEF prints are sent, e.g. ABC9 or ABCFXTMLBX. It is not an email address, mail output device (ABCM) or ink-and-paper printer name.
---- For MCP and CNS under CDS this is the 'topic' allocated to your badge, e.g. ABCX.
---- For NES this is your EDCS location, e.g. LOCEDC1ABC.
---- For CCSUK this is your PIMA, e.g. CUKFFW98000ABC.
---- For Pentant, this is the name of the FTP folder allocated to you.

* Username & password: only needed for Pentant, MCP & CNS, these are the webservice or FTP login credentials. They are not the credentials used to access the Pentant, Destin8 or Compass websites.

* Fallback for shed:  the shed for which this fallback agent is acting (CCSUK only).

* Receiver ID and Sender ID.  These are only needed for Pentant - supply the values allocated to you by Pentant.

* Loader.  Tick this box to give your CCSUK badge the ability to send export arrival and departure messages. Tick this only if Customs have authorised your badge for this role.

* Endpoint. For Pentant, select the code that indicates which endpoint address has been allocated to you, the inventory address or the declaration address.

* Failures and test state.  Values indicating whether these web credentials (for MCP and CNS only) have been proven good, or have had successive login failures. Too many login failures and the use of the badge is suspended. You can remove the suspension by right-clicking the gutter of a row and selecting 'Check Credentials'.",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue | RegistryOptions.NotCached);
		}

		public void TestCDSNotificationGroup()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUser.GS_EmailAddress = "unittesting@testingwise.com";
			currentUser.Groups.RemoveAll();

			var newGroup = AddGroupForCurrentUser(currentUser, "UT1", "UnitTest 1");

			ZGuid pmGroup = Core.Constants.Groups.PostMastersGroupPK;
			var regEntry = GBCustomsDataRegistry.Instance.FindByName("CustomsResponseNotificationsToGroupCDS");

			AssertNotNull(regEntry);

			AssertEquals(pmGroup, (Guid)regEntry.Value);

			regEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroup.ToGuid());

			AssertEquals(newGroup, (Guid)regEntry.Value);
		}

		public void TestGB_CcsukP5ProcessorLinkToConsole()
		{
			var result = GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.Value;
			AssertEquals("Default value should be true", true, result);

			using (GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				result = GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.Value;
				AssertEquals("Manually set value should be false", false, result);
			}

			using (GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				result = GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.Value;
				AssertEquals("Manually set value should be true", true, result);
			}
		}

		public void TestGB_CcsukShowDEPProfiles()
		{
			TestGenericRegistryItem(ItemSet.CcsukShowDEPProfiles,
				"GBCcsukShowDEPProfiles",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
				"Show DEP profiles?",
				"Show DEP profiles for branch",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestGB_N935_AddSupportingDocToInvoiceHeader()
		{
			var result = GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.Value;
			AssertEquals("Default value should be true", true, result);

			using (GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				result = GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.Value;
				AssertEquals("N935 - add supporting document to invoice header", false, result);
			}

			using (GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				result = GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.Value;
				AssertEquals("N935 - add supporting document to invoice header", true, result);
			}
		}

		public void TestGB_N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders()
		{
			var result = GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.Value;
			AssertEquals("Default value should be true", true, result);

			using (GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				result = GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.Value;
				AssertEquals("N935 - add all invoice header numbers to all invoice headers", false, result);
			}

			using (GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				result = GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.Value;
				AssertEquals("N935 - add all invoice header numbers to all invoice headers", true, result);
			}
		}

		public void TestTestCnsPrintsUrlBranding()
		{
			var mock = new Mock<IBranding>();
			mock.Setup(m => m.CompanyName).Returns("Fake Company Name");
			mock.Setup(m => m.ProductName).Returns("Fake Product Name");
			using (BrandingFactory.ConfigureTemporary(() => mock.Object))
			{
				var cnsPrintsUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("CnsPrintsUrl_Live") as StringRegistryItem;
				var hint = cnsPrintsUrl_LiveItem.Hint;
				AssertEndsWith("Branding Should Come from Branding Factory", "Fake Company Name.", hint);
			}
		}

		public void TestCnsPrintsUrl()
		{
			TestStringRegistryItem(ItemSet.CnsPrintsUrl_Live,
				expectedName: "CnsPrintsUrl_Live",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
				expectedCaption: "URL of CNS print mailbox service (live)",
				expectedHint: $"Enter the URL of CNS print mailbox service to which {Core.Constants.ProductName} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to WiseTech Global.",
				RegistryStorageFlags.System, TextEditorType.Url, RegistryOptions.PreserveTestValue,
				expectedDefaultValue: "https://www.cnsonline.co.uk/ws/Mailbox/MailBoxPortImpl",
				CharacterCase.Normal);

			var cnsPrintsUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("CnsPrintsUrl_Live") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsPrintsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_LiveItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			var cnsPrintsUrl_TestItem = GBCustomsDataRegistry.Instance.FindByName("CnsPrintsUrl_Test") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsPrintsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsPrintsUrl_TestItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestCnsUploadUrl()
		{
			TestStringRegistryItem(ItemSet.CnsUploadUrl_Live,
				expectedName: "CnsUploadUrl_Live",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
				expectedCaption: "URL of CNS upload CCMI service (live)",
				expectedHint: $"Enter the URL of CNS upload CCMI service to which {Core.Constants.ProductName} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to WiseTech Global.",
				RegistryStorageFlags.System, TextEditorType.Url, RegistryOptions.PreserveTestValue,
				expectedDefaultValue: "https://www.cnsonline.co.uk/ws/CCMI/ChiefEDI",
				CharacterCase.Normal);

			var cnsUploadUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("CnsUploadUrl_Live") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsUploadUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_LiveItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			var cnsUploadUrl_TestItem = GBCustomsDataRegistry.Instance.FindByName("CnsUploadUrl_Test") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsUploadUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsUploadUrl_TestItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestCnsCdsCheckCredentialsUrl()
		{
			var cnsCdsCheckCredentialsUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("CnsCdsCheckCredentialsUrl_Live") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_LiveItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			var cnsCdsCheckCredentialsUrl_TestItem = GBCustomsDataRegistry.Instance.FindByName("CnsCdsCheckCredentialsUrl_Test") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => cnsCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(cnsCdsCheckCredentialsUrl_TestItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestMcpCdsCheckCredentialsUrl()
		{
			var mcpCdsCheckCredentialsUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("McpCdsCheckCredentialsUrl_Live") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpCdsCheckCredentialsUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_LiveItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			var mcpCdsCheckCredentialsUrl_TestItem = GBCustomsDataRegistry.Instance.FindByName("McpCdsCheckCredentialsUrl_Test") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpCdsCheckCredentialsUrl_TestItem.DataType.ValidateBeforeRegistryFormSave(mcpCdsCheckCredentialsUrl_TestItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestCustomsModuleEnabledForShipmentsAndConsols()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols,
				"GBCustomsModuleEnabledForShipmentsAndConsols",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"CW1 GB customs module is enabled for shipment/consols",
				"Set this to NO to tell CW1 that attachments or detachments of shipments and consols should not cause an associate/disassociate message to be sent to CHIEF, and moreover that failures to send such messages due to insufficient data should be suppressed. There is no other effect.",
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				true);
		}

		public void TestUseBranchEoriForMucrOnConsols()
		{
			TestRegistryItem(ItemSet.GB_UseBranchEoriForMucrOnConsols,
				expectedName: "GBUseBranchEoriForMucrOnConsols",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				expectedCaption: "Use current branch for EORI when calculating MUCR references on Consols",
				expectedHint: "Set this to true to always use the current branch's OrgProxy to provide an EORI when calculating Master UCR values for consols for MUCR modes that require an EORI. The default value of false will use the Sending Agent's EORI instead, if the Sending Agent is set.",
				expectedStorage: RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: false);
		}

		public void TestEnableNorthernIrelandICSManifest()
		{
			TestRegistryItem(
				ItemSet.EnableIcsManifest,
				"EnableIcsManifest",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Enable Northern Ireland ICS Manifest",
				"Enable Northern Ireland ICS Manifest?",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableSSGBManifest()
		{
			TestRegistryItem(
				ItemSet.EnableSSGBManifest,
				expectedName: "EnableSSGBManifest",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				expectedCaption: "Enable S&S GB Manifest",
				expectedHint: "Enable S&S GB Manifest?",
				expectedStorage: RegistryStorageFlags.System,
				expectedDefaultValue: true);
		}

		public void TestCDSPilotMode()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CDSPilotMode,
				"CDSAuthorisationUsesTestEHub",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"CDS authorisation uses test eHub",
				"CDS authorisation uses test eHub?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestCDSEnabledForExports()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CDSEnabledForExports,
				"CDSEnabledForExports",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"CDS is enabled for exports",
				"CDS is enabled for exports?",
				RegistryStorageFlags.Branch,
				false);
		}

		public void TestCDSEnabledForImports()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CDSEnabledForImports,
				"CDSEnabledForImports",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"CDS is enabled for imports",
				"CDS is enabled for imports?",
				RegistryStorageFlags.Branch,
				false);
		}

		public void TestCDSEnableEXRRAutomationForArrivedROROExports()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CDSEnableEXRRAutomationForArrivedROROExports,
				"CDSEnableEXRRAutomationForArrivedROROExports",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				(NoResString)"CDS Enable EXRR automation for arrived RORO exports",
				(NoResString)"Automate the creation of an EXRR authorization for arrived RORO exports?",
				RegistryStorageFlags.Branch,
				expectedDefaultValue: true);
		}

		public void TestICS()
		{
			TestStringRegistryItem(GBCustomsDataRegistry.Instance.ICSUsername,
				"ICSUsername",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ICS,
				"Username",
				"Username for ICS access",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForDevelopers,
				"",
				CharacterCase.Normal);
			AssertEquals(35, ((StringRegistryDataType)GBCustomsDataRegistry.Instance.ICSUsername.DataType).MaxLength);

			TestStringRegistryItem(GBCustomsDataRegistry.Instance.ICSPasssword,
				"ICSPasssword",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ICS,
				"Password",
				"Password for ICS access",
				RegistryStorageFlags.Company,
				TextEditorType.Password,
				RegistryOptions.IsOnlyForDevelopers,
				"",
				CharacterCase.Normal);
			AssertEquals(35, ((StringRegistryDataType)GBCustomsDataRegistry.Instance.ICSPasssword.DataType).MaxLength);
		}

		public void TestGetPentantFtpEndpointDoesNotThrowExceptionWithNullCredential()
		{
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNoExceptionThrown(() => GBCustomsDataRegistry.Instance.GetPentantFtpEndpoint(null));
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNoExceptionThrown(() => GBCustomsDataRegistry.Instance.GetPentantFtpEndpoint(null));
		}

		public void TestPreserveTestValues()
		{
			var registryNodesWeExpectToBePreservedInTest =
				new IRegistryItem[]
					{
						GBCustomsDataRegistry.Instance.IsInTestMode,
						GBCustomsDataRegistry.Instance.CnsPrintsUrl_Live,
						GBCustomsDataRegistry.Instance.CnsUploadUrl_Live,
						GBCustomsDataRegistry.Instance.CnsPrintsUrl_Test,
						GBCustomsDataRegistry.Instance.CnsUploadUrl_Test,
						GBCustomsDataRegistry.Instance.CnsUseCompassForPullingPrints,
						GBCustomsDataRegistry.Instance.McpUseDestin8ForPullingPrints,
						GBCustomsDataRegistry.Instance.McpDestin8Url_Live2023,
						GBCustomsDataRegistry.Instance.McpUseDestin8ForPullingPrints,
						GBCustomsDataRegistry.Instance.McpUseDestin8ForPullingPrints,
						GBCustomsDataRegistry.Instance.McpUseDestin8ForPullingPrints,
						GBCustomsDataRegistry.Instance.McpUseDestin8ForPullingPrints,
						GBCustomsDataRegistry.Instance.BadgeCodes,
						GBCustomsDataRegistry.Instance.Credentials,
						GBCustomsDataRegistry.Instance.CDSPilotMode,
						GBCustomsDataRegistry.Instance.CcsukGoLiveDate,
						GBCustomsDataRegistry.Instance.ChiefFallbackExports,
						GBCustomsDataRegistry.Instance.ChiefFallbackImports,
						GBCustomsDataRegistry.Instance.CcsukNetworkIpToDeclareForCallBack_Legacy,
						GBCustomsDataRegistry.Instance.CcsukPassword,
						GBCustomsDataRegistry.Instance.CcsukPassword_New,
						GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL_Live,
						GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN_Live,
						GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL_Test,
						GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN_Test,
						GBCustomsDataRegistry.Instance.CcsukIpAddresses,
						GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic,
						GBCustomsDataRegistry.Instance.CcsukLocalIpForBindingListener_Legacy,
						GBCustomsDataRegistry.Instance.CcsukLocalPortForBinding,
						GBCustomsDataRegistry.Instance.McpIslWebserviceUrl_Live,
						GBCustomsDataRegistry.Instance.McpIslWebserviceUrl_Test,
						GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet,
					};
			foreach (IRegistryItem o in registryNodesWeExpectToBePreservedInTest)
			{
				AssertEquals(o.Name + " should have PreserveTestValue", RegistryOptions.PreserveTestValue, RegistryOptions.PreserveTestValue & o.Options);
			}
		}

		public void TestSendNetMassForH2Declarations()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SendNetMassForH2Declarations,
				"SendNetMassForH2Declarations",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Send mass (6/1) for H2 declarations?",
				"Send net mass data element 6/1 for H2 CDS declarations.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestSendNetMassForH3Declarations()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SendNetMassForH3Declarations,
				"SendNetMassForH3Declarations",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Send mass (6/1) for H3 declarations?",
				"Send net mass data element 6/1 for H3 CDS declarations.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestSendDTNTaxBaseToCDS()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SendDTNTaxBaseToCDS,
				"SendDTNTaxBaseToCDS",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Send convertible mass tax bases to CDS?",
				"Set to YES to send DTN, TNE and other convertible mass tax bases to CDS.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestCcsukNetworkIpToDeclareForCallBack_LegacyMaxLength()
		{
			StringRegistryDataType dataType = (StringRegistryDataType)ItemSet.CcsukNetworkIpToDeclareForCallBack_Legacy.DataType;
			AssertEquals(0, dataType.MinLength);
			AssertEquals(15, dataType.MaxLength);
		}

		public void TestCcsukNetworkDashboardRegistryItems()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukLastPingDateTime,
					"GBCcsukLastPingDateTime",
					GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
					"Last ping",
					"When did we last ping CCSUK?",
					ZDateTimePickerFormat.LongIncludingSeconds,
					RegistryStorageFlags.System,
					RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
					DateTime.MinValue);

				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukLastLogonDateTime,
					"GBCcsukLastLogonDateTime",
					GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
					"Last logon",
					"When did we last logon to CCSUK?",
					ZDateTimePickerFormat.LongIncludingSeconds,
					RegistryStorageFlags.System,
					RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
					DateTime.MinValue);

				TestStringRegistryItem(GBCustomsDataRegistry.Instance.CcsukLastConnectedProfile,
					"GBCcsukLastConnectedProfile",
					GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
					"Last connected profile",
					"Profile details last used to connect to CCSUK",
					RegistryStorageFlags.System,
					TextEditorType.Memo,
					RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
					string.Empty,
					CharacterCase.Normal);

				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukLastSentMessageDateTime,
					"GBCcsukLastSentMessageDateTime",
					GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
					"Last sent message",
					"When did we last send a message to CCSUK?",
					ZDateTimePickerFormat.LongIncludingSeconds,
					RegistryStorageFlags.System,
					RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
					DateTime.MinValue);

				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukLastReceivedMessageDateTime,
					"GBCcsukLastReceivedMessageDateTime",
					GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
					"Last received message",
					"When did we last receive a message from CCSUK?",
					ZDateTimePickerFormat.LongIncludingSeconds,
					RegistryStorageFlags.System,
					RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
					DateTime.MinValue);

				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukIsConnected,
						"GBCcsukIsConnected",
						GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
						"Is connected",
						"Are we connected to CCSUK?",
						RegistryStorageFlags.System,
						RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
						false);

				TestStringRegistryItem(GBCustomsDataRegistry.Instance.CcsukConnectedProcessController,
						"GBCcsukConnectedProcessController",
						GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
						"Connected process controller",
						"Name of the process controller server that is connected to CCSUK",
						RegistryStorageFlags.System,
						TextEditorType.TextBox,
						RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
						string.Empty,
						CharacterCase.Normal);

				TestRegistryItem(GBCustomsDataRegistry.Instance.CcsukProcessID,
						"GBCcsukProcessID",
						GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
						"Process ID",
						"PID of the connected process on the process controller server",
						RegistryStorageFlags.System,
						RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
						0);
			});
		}

		public void TestCTC_UseDepartureIdOrArrivalId()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CTC_UseDepartureIdOrArrivalId,
				"CTC_UseDepartureIdOrArrivalId",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
				"Use CTC departure ID or arrival ID for message correlation.",
				"Use the departure ID or arrival ID instead of the CAR or Box [7] reference for processing CTC messages,",
				RegistryStorageFlags.System,
				true);
		}

		public void TestNCTS_Fallback_Business_Continuity_Is_Active()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.NCTS_Fallback_Business_Continuity_Is_Active,
				"NCTS_Fallback_Business_Continuity_Is_Active",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
				"NCTS fallback (business continuity) is active",
				"NCTS fallback (business continuity) is active",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedDefaultValue: false
			);
		}

		public void TestSuppressForeignEORI()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SuppressForeignEORI,
				"SuppressForeignEORI",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
				"Suppress Foreign EORI",
				"Suppress Foreign EORI",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestSendDevQueryForNonInventoryImportsUponAcceptance()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SendDevQueryForNonInventoryImportsUponAcceptance,
				"GBSendDevQueryForNonInventoryImportsUponAcceptance",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Auto-DEV",
				"Send DEV query messages automatically when a non-inventory import declaration receives an E2 response?",
				RegistryStorageFlags.System,
				false);
		}

		public void TestSendDesQueryForNonInventoryImportsUponAcceptance()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.SendDesQueryForNonInventoryImportsUponAcceptance,
				"GBSendDesQueryForNonInventoryImportsUponAcceptance",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Auto-DES",
				"Send DES query messages automatically when a non-inventory import declaration receives an E2 response?",
				RegistryStorageFlags.System,
				true);
		}

		public void TestDisableBlueValidationOnMUCR()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForInventoryLinkedPortsImport,
				"GBDisableBlueValidationOnMUCRForInventoryLinkedPortsImport",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Disable blue validation on MUCR for inventory-linked ports (Import)",
				"For a port (box [30]) that is known to be inventory linked, a missing MUCR on an import will show, when set to no (default) a message warning (blue) validation when absent. When set to yes, will show a warning message (yellow).",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);

			TestRegistryItem(GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForExport,
				"GBDisableBlueValidationOnMUCRForExport",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Disable blue validation on MUCR for export",
				"A missing MUCR on an export will show, when set to no (default) a message warning (blue) validation when absent. When set to yes, will show a warning message (yellow).",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestCDSStatisticalValueManualOverride()
		{
			TestRegistryItem(GBCustomsDataRegistry.Instance.CDSStatisticalValueManualOverride,
				"GBCDSStatisticalValueManualOverride",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Set override for statistical value for CDS imports",
				"Set override for statistical value for CDS imports?",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestMcpIslWebserviceUrl_Live()
		{
			TestStringRegistryItem(GBCustomsDataRegistry.Instance.McpIslWebserviceUrl_Live,
				"McpIslWebserviceUrl_Live",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
				"ISL Webservice URL (Live)",
				"Live MCP ISL Webservice URL",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.PreserveTestValue,
				"https://www.destin8.co.uk/ISLInterfaceMCP/ISLInterfaceMCP",
				CharacterCase.Lower);
		}

		public void TestMcpIslWebserviceUrl_Live_Validation()
		{
			var mcpIslWebserviceUrl_LiveItem = GBCustomsDataRegistry.Instance.FindByName("McpIslWebserviceUrl_Live") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => mcpIslWebserviceUrl_LiveItem.DataType.ValidateBeforeRegistryFormSave(mcpIslWebserviceUrl_LiveItem, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestMcpIslWebserviceUrl_Test()
		{
			TestStringRegistryItem(GBCustomsDataRegistry.Instance.McpIslWebserviceUrl_Test,
				"McpIslWebserviceUrl_Test",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
				"ISL Webservice URL (Test)",
				"Test MCP ISL Webservice URL",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.PreserveTestValue,
				"https://uat.destin8.co.uk/ISLInterfaceMCP/ISLInterfaceMCP",
				CharacterCase.Lower);
		}

		public void TestMcpIslWebserviceUrl_Test_Validation()
		{
			var testURL = GBCustomsDataRegistry.Instance.FindByName("McpIslWebserviceUrl_Test") as StringRegistryItem;
			AssertExceptionThrown<RegistryValidationException>(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "ftp://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "mailto://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertExceptionThrown<RegistryValidationException>(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "example", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "http://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => testURL.DataType.ValidateBeforeRegistryFormSave(testURL, "https://www.example.com", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestMcpIslWebserviceUrl()
		{
			string testURL = "https://www.test.com";
			string liveURL = "https://www.live.com";

			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.McpIslWebserviceUrl = testURL;
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.McpIslWebserviceUrl = liveURL;
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(testURL, GBCustomsDataRegistry.Instance.McpIslWebserviceUrl);
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(liveURL, GBCustomsDataRegistry.Instance.McpIslWebserviceUrl);
		}

		public void TestMcpIslWebServiceCredentialsSet()
		{
			var testCreds = GBCustomsDataRegistry.Instance.FindByName("McpIslWebServiceCredentialsSet") as McpIslCredentialsSettingCollectionRegistryItem;
			AssertNotNull(testCreds);

			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet,
				"McpIslWebServiceCredentialsSet",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
				"ISL Webservice Credentials",
				"Enter company code, username, device and password as provided",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue);
		}

		public void TestCDSUCRAutomation()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.CDSDUCRAutomation,
				"CDSDUCRAutomation",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				(NoResString)"CDS DUCR Automation",
				(NoResString)"CDS DUCR automation with three options based on selecting a) Split entry reference into DUCR and Part fields b) Combine.  Send only a single entry reference. c) Do not automatically add DCR/DCS for imports and combine for exports.",
				RegistryStorageFlags.System,
				RegistryOptions.Default
			);

			AssertEquals("DefaultValue", CDSUCRAutomationSettingsList.Codes.NotForImports, GBCustomsDataRegistry.Instance.CDSDUCRAutomation.Value.CDSDUCRAutomation);
		}

		public void TestCDSEnableAutoConvertExciseUnits()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.CDSEnableAutoConvertExciseUnits,
				"CDSEnableAutoConvertExciseUnits",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				(NoResString)"CDS Enable Automatic Conversion of Excise Units",
				(NoResString)"Enable automatic conversion of excise units for CDS?",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: true);
		}

		public void TestExcludeSuspendedAndWaivedFeesFromCalculations()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.ExcludeSuspendedAndWaivedFeesFromCalculations,
				"GBExcludeSuspendedAndWaivedFeesFromCalculations",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				expectedCaption: (NoResString)"Exclude Suspended and Waived Fees from Calculations",
				expectedHint: (NoResString)"Exclude suspended and waived fees from duty and VAT calculations",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedDefaultValue: true);
		}

		public void TestCDSSuppressSendingASVXFor3XXTaxTypes()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.CDSSuppressSendingASVXFor3XXTaxTypes,
				"CDSSuppressSendingASVXFor3XXTaxTypes",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				expectedCaption: (NoResString)"Suppress transmission of ASVX unit to CDS for 3xx tax types",
				expectedHint: (NoResString)"Suppress transmission of ASVX unit to CDS while awaiting fix for incorrect CDS behaviour, for 3xx tax types?",
				expectedStorage: RegistryStorageFlags.Branch,
				expectedDefaultValue: true);
		}

		public void TestNotificationCDSALV()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.NotificationCDSALV,
				"CustomsResponseNotificationsToGroupCDSUnsolicitedUpdatesALV",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_CDS_Unsolicited_Updates,
				(NoResString)"ALV",
				(NoResString)"Group for ALV",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestNotificationCDSQRY()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.NotificationCDSQRY,
				"CustomsResponseNotificationsToGroupCDSUnsolicitedUpdatesQRY",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_CDS_Unsolicited_Updates,
				(NoResString)"QRY",
				(NoResString)"Group for QRY",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
		}

		public void TestDigitalPromptsTrialParticipateInNudgeTrial()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial,
				"DigitalPromptsTrialParticipateInNudgeTrial",
				GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				"Digital prompts trial - participate in nudge trial",
				"Please inform HMRC of the client's desire to be excluded",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestSendCDS317()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.SendCDS317,
				"SendCDS317",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom,
				expectedCaption: (NoResString)"Send CDS 3/17",
				expectedHint: (NoResString)"For supported message types, send the declarant's name and address (3/17) in addition to always sending the declarant ID (3/18).",
				expectedStorage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedDefaultValue: false);
		}

		public void TestCcsukMaximumBatchSize()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.CcsukMaximumBatchSize,
				"CcsukMaximumBatchSize",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
				expectedCaption: (NoResString)"CCS-UK Batch Size for Sending",
				expectedHint: (NoResString)"CCS-UK Batch. Sets the maximum number of interchanges to send in a batch. Set to 0 for no maximum batch size",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: 0);
		}

		public void TestForceRecalculationOfNPR()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.ForceRecalculationOfNPR,
				"ForceRecalculationOfNPR",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
				expectedCaption: (NoResString)"Force recalculation of NPR while sending interchanges",
				expectedHint: (NoResString)"This setting controls whether we allow the recalculation of NPR for Airway Bills while sending interchanges.  Do not edit this setting without consulting WTG",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false);
		}

		public void TestCcsukInterchangePackagerBatchSize()
		{
			TestGenericRegistryItem(GBCustomsDataRegistry.Instance.CcsukInterchangePackagerBatchSize,
				"CcsukInterchangePackagerBatchSize",
				expectedCategory: GBCustomsDataRegistry.Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
				expectedCaption: (NoResString)"CCS-UK Batch Size for Packaging Interchanges",
				expectedHint: (NoResString)"CCS-UK Batch. Sets the maximum number of messages retrieved for packaging into an interchange.  Do not edit this setting without consulting WTG",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: 50);
		}
	}
}
