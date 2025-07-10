using System;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class ResourceStringsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNoExceptions()
		{
			ResourceStrings.Instance.GetData(1, "abcde");
			ResourceStrings.Instance.GetString(1, "abcde", "Abcde");
		}

		public void TestCurrentLanguage()
		{
			const string german = Core.SharedConstants.Languages.German;

			bool currentLanguageChangedFired = false;
			EventHandler handler = delegate
			{ currentLanguageChangedFired = true; };
			Res.Changed += handler;
			try
			{
				GlbStaff grmStaff = Factory.NewWithValidTestData<GlbStaff>();
				grmStaff.GS_WorkingLanguage = german;
				Factory.Save();

				var originalLanguage = ResourceStrings.Instance.CurrentLanguage;
				AssertEquals("germanLicence.IsLoggedIn", false, Env.Licence.LanguagePackLookup[german].GUILanguageCheckpoint.IsLoggedIn);
				AssertNotEquals(german, ResourceStrings.Instance.CurrentLanguage);
				AssertEquals(false, currentLanguageChangedFired);

				using (Env.SetTemporaryUserContext(grmStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("germanLicence.IsLoggedIn", true, Env.Licence.LanguagePackLookup[german].GUILanguageCheckpoint.IsLoggedIn);
					AssertEquals(german, ResourceStrings.Instance.CurrentLanguage);
					AssertEquals(true, currentLanguageChangedFired);
					currentLanguageChangedFired = false;
				}
				AssertEquals("germanLicence.IsLoggedIn", false, Env.Licence.LanguagePackLookup[german].GUILanguageCheckpoint.IsLoggedIn);
				AssertEquals(originalLanguage, ResourceStrings.Instance.CurrentLanguage);
				AssertEquals(true, currentLanguageChangedFired);
			}
			finally
			{
				Res.Changed -= handler;
			}
		}

		public void TestEnglishLanguage()
		{
			AssertEquals(Res.DefaultLanguage, ResourceStrings.Instance.CurrentLanguage);

			var defaultStaff = Factory.NewWithValidTestData<GlbStaff>();

			var gbStaff = Factory.NewWithValidTestData<GlbStaff>();
			gbStaff.GS_WorkingLanguage = Constants.Languages.EnglishBritish;

			var usStaff = Factory.NewWithValidTestData<GlbStaff>();
			usStaff.GS_WorkingLanguage = Constants.Languages.EnglishAmerican;

			var ukCompany = Factory.NewWithValidTestData<GlbCompany>();
			ukCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "UKB";

			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var auBranch = ukCompany.Branches.AddNew();
			auBranch.GB_Code = "AUB";

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			var usBranch = ukCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";

			var gbCompany = Factory.NewWithValidTestData<GlbCompany>();
			var gbBranch = gbCompany.Branches.AddNew();
			gbBranch.GB_Code = "GBB";
			RawDataRegistry.Instance.EnglishSpelling.SetValue(gbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.Languages.EnglishBritish);

			var deCompany = Factory.NewWithValidTestData<GlbCompany>();
			var deBranch = usCompany.Branches.AddNew();
			deBranch.GB_Code = "DEB";
			RawDataRegistry.Instance.EnglishSpelling.SetValue(deCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.Languages.EnglishAmerican);

			var companyWithProxy = Factory.NewWithValidTestData<GlbCompany>();
			var branchWithProxy = companyWithProxy.Branches.AddNew();
			branchWithProxy.GB_Code = "PRB";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Constants.Languages.EnglishBritish;
			companyWithProxy.GC_OH_OrgProxy = orgProxy.PK;

			Factory.Save();

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, ukBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, auBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, usBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, gbBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, deBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Res.DefaultLanguage, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(usStaff.GS_LoginName, gbBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Res.DefaultLanguage, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(gbStaff.GS_LoginName, usBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			using (Environment.Env.SetTemporaryUserContext(defaultStaff.GS_LoginName, branchWithProxy.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(Constants.Languages.EnglishBritish, ResourceStrings.Instance.CurrentLanguage);
			}

			AssertEquals(Res.DefaultLanguage, ResourceStrings.Instance.CurrentLanguage);
		}

		public void TestUseMockData()
		{
			using (IMockResourceStringCache mockCache = ResourceStrings.Instance.UseMockData())
			{
				mockCache.Put("abcde", new ResourceStringData("abcde", "Abcde"));

				AssertEquals("Abcde", ResourceStrings.Instance.GetData(0, "abcde").Caption);
				AssertNull(ResourceStrings.Instance.GetData(0, "vwxyz"));
			}
		}
	}
}
