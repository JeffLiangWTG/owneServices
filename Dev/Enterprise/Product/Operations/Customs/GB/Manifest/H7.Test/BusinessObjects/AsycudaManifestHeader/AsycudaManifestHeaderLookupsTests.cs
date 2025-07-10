using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class AsycudaManifestHeaderLookupTests : BusinessObjectLookupsTestCase
	{
		public void TestProfileList_WhenHeaderBranchHasNoBadgeCodes_ShouldReturnEmptyList()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch, true);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GB = branch.PK.ToGuid();

			var profileList = header.Lookups.ProfileList;
			AssertEquals(0, profileList.Count);
		}

		public void TestProfileList_WhenPortOfFirstArrivalIsSelected_ShouldReturnProfilesForPortOfFirstArrival()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GB = branch.PK.ToGuid();
			header.AMA_RL_NKPortOfDischarge = "GBLON";

			header.AMA_RL_NKPortOfFirstArrival = "GBBHX";
			var profileList = header.Lookups.ProfileList;

			AssertEquals(2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR3", profileList[1].Code);

			header.AMA_RL_NKPortOfFirstArrival = "GBABD";
			profileList = header.Lookups.ProfileList;

			AssertEquals(2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR4", profileList[1].Code);
		}

		public void TestProfileList_WhenPortOfFirstArrivalAndPortOfDischargeAreEmpty_ShouldReturnAllProfilesForImportOrBothDirection()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GB = branch.PK.ToGuid();
			header.AMA_RL_NKPortOfDischarge = string.Empty;
			header.AMA_RL_NKPortOfFirstArrival = string.Empty;

			var profileList = header.Lookups.ProfileList;

			AssertEquals(3, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR3", profileList[1].Code);
			AssertEquals("PR4", profileList[2].Code);
		}

		public void TestProfileList_WhenPortOfFirstArrivalIsEmptyAndPortOfDischargeIsSelected_ShouldReturnProfilesForPortOfDischarge()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GB = branch.PK.ToGuid();
			header.AMA_RL_NKPortOfFirstArrival = string.Empty;

			header.AMA_RL_NKPortOfDischarge = "GBBHX";
			var profileList = header.Lookups.ProfileList;

			AssertEquals(2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR3", profileList[1].Code);

			header.AMA_RL_NKPortOfDischarge = "GBABD";
			profileList = header.Lookups.ProfileList;

			AssertEquals(2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR4", profileList[1].Code);
		}

		public void TestProfileList_WhenHeaderBranchDoesNotExist_ShouldReturnCurrentBranchProfiles()
		{
			SetupRegistryBadgeCodes(GlbBranch.CurrentBranch);

			var header = Factory.New<AsycudaManifestHeader>();

			var profileList = header.Lookups.ProfileList;
			AssertEquals(3, profileList.Count);
		}

		public void TestProfileList_WhenCachedValueExistsForPortOfFirstArrival_ShouldReturnCachedValue()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch);

			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_GB = branch.PK;
			header1.AMA_RL_NKPortOfFirstArrival = "GBABD";
			header1.AMA_RL_NKPortOfDischarge = "GBLON";
			AssertEquals("Should return correct profiles", 2, header1.Lookups.ProfileList.Count);

			SetupRegistryBadgeCodes(branch, true);

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_GB = branch.PK;
			header2.AMA_RL_NKPortOfFirstArrival = "GBBHX";
			header1.AMA_RL_NKPortOfDischarge = "GBLON";
			AssertEquals("Should not return cached profiles of other ports", 0, header2.Lookups.ProfileList.Count);

			var header3 = Factory.New<AsycudaManifestHeader>();
			header3.AMA_GB = GlbBranch.CurrentBranch.PK;
			header3.AMA_RL_NKPortOfFirstArrival = "GBABD";
			header1.AMA_RL_NKPortOfDischarge = "GBLON";
			AssertEquals("Should not return cached profiles of other branches", 0, header3.Lookups.ProfileList.Count);

			var header4 = Factory.New<AsycudaManifestHeader>();
			header4.AMA_GB = branch.PK;
			header4.AMA_RL_NKPortOfFirstArrival = "GBABD";
			header1.AMA_RL_NKPortOfDischarge = "GBMAN";

			var profileList = header4.Lookups.ProfileList;
			AssertEquals("Should not return cached profiles of the same branch and port", 2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR4", profileList[1].Code);
		}

		public void TestProfileList_WhenCachedValueExistsForPortOfDischarge_ShouldReturnCachedValue()
		{
			var branch = Factory.New<GlbBranch>();
			SetupRegistryBadgeCodes(branch);

			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_GB = branch.PK;
			header1.AMA_RL_NKPortOfDischarge = "GBABD";
			AssertEquals("Should return correct profiles", 2, header1.Lookups.ProfileList.Count);

			SetupRegistryBadgeCodes(branch, true);

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_GB = branch.PK;
			header2.AMA_RL_NKPortOfDischarge = "GBBHX";
			AssertEquals("Should not return cached profiles of other ports", 0, header2.Lookups.ProfileList.Count);

			var header3 = Factory.New<AsycudaManifestHeader>();
			header3.AMA_GB = GlbBranch.CurrentBranch.PK;
			header3.AMA_RL_NKPortOfDischarge = "GBABD";
			AssertEquals("Should not return cached profiles of other branches", 0, header3.Lookups.ProfileList.Count);

			var header4 = Factory.New<AsycudaManifestHeader>();
			header4.AMA_GB = branch.PK;
			header4.AMA_RL_NKPortOfDischarge = "GBABD";

			var profileList = header4.Lookups.ProfileList;
			AssertEquals("Should return cached profiles of the same branch and port", 2, profileList.Count);
			AssertEquals("PR1", profileList[0].Code);
			AssertEquals("PR4", profileList[1].Code);
		}

		void SetupRegistryBadgeCodes(GlbBranch branch, bool clearAll = false)
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			badgeCodeSettings.RemoveAll();

			if (!clearAll)
			{
				var badgeCodeSetting = badgeCodeSettings.AddNew();
				badgeCodeSetting.BadgeCode = "PR4";
				badgeCodeSetting.RL_PortCode = "GBABD";

				badgeCodeSetting = badgeCodeSettings.AddNew();
				badgeCodeSetting.BadgeCode = "PR2";
				badgeCodeSetting.Direction = BadgeDirectionList.Codes.EXP;
				badgeCodeSetting.RL_PortCode = "GBABD";

				badgeCodeSetting = badgeCodeSettings.AddNew();
				badgeCodeSetting.BadgeCode = "PR3";
				badgeCodeSetting.Direction = BadgeDirectionList.Codes.Both;
				badgeCodeSetting.RL_PortCode = "GBBHX";

				badgeCodeSetting = badgeCodeSettings.AddNew();
				badgeCodeSetting.BadgeCode = "PR1";
				badgeCodeSetting.Direction = BadgeDirectionList.Codes.IMP;
			}

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		public void TestRegistrationStatusList_Contents()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var expectedEntryStatusList = new CodeDescriptionPairList(bill.Lookups.CustomsStatusList);
			expectedEntryStatusList.Add(new CodeDescriptionPair("MLT", "Multiple statuses exist"));

			AssertContainsExactElementsInAnyOrder(expectedEntryStatusList, header.Lookups.RegistrationStatusList);
		}

		public void TestSupervisingOfficeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var list = header.Lookups.SupervisingOfficeList;
			AssertType<OrganisationsFindBoxCollection>(list);
		}
	}
}
