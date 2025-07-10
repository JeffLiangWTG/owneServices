using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_GB))]
	public class GlbExternalPasswordValidation_GBTests : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_GB, GlbExternalPasswordValidation_GB>
	{
		public void TestCheckUserIDAndMailBoxIDAreUnique()
		{
			// Blue errors 
			var password = Factory.New<GlbExternalPassword_GBForTest>();
			password.Badge = "ABC";
			password.EORI = "123";
			AssertHasMessageErrorContaining(password.EORIInfo, "CK1");

			password.Badge = "DEF";
			AssertNoMessageErrors(password.BadgeInfo);
			AssertHasMessageErrorContaining(password.EORIInfo, "CK1");

			password.Badge = "ABC";
			AssertHasMessageErrorContaining(password.EORIInfo, "CK1");
			password.EORI = "456";
			AssertNoMessageErrors(password.EORIInfo);

			password.EORI = "123";
			AssertHasMessageErrorContaining(password.EORIInfo, "CK1");
			AssertHasMessageErrorContaining(password.BadgeInfo, "CK1");

			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			var originalController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				// Red Error for non controller user
				GlbStaff.CurrentUser.GS_LoginName = "MTB";
				GlbStaff.CurrentUser.GS_IsController = false;
				password.Badge = "ABC";
				password.EORI = "123";
				AssertHasErrorContaining("Non-controller user", password.BadgeInfo, "CK1");
				AssertHasErrorContaining("Non-controller user", password.EORIInfo, "CK1");

				password.Badge = "DEF";
				password.EORI = "456";
				AssertNoErrors(password.BadgeInfo);
				AssertNoErrors(password.EORIInfo);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
				GlbStaff.CurrentUser.GS_IsController = originalController;
			}
		}

		public void TestCheckEORI()
		{
			var password = Factory.New<GlbExternalPassword_GBForTest>();
			password.EORI = "123";
			AssertNoMessageErrors(password.EORIInfo);
			password.EORI = string.Empty;
			AssertHasMessageErrors(password.EORIInfo);
			password.EORI = "789";
			AssertHasMessageErrors(password.EORIInfo);
			password.EORI = "123";
			AssertNoMessageErrors(password.EORIInfo);
		}

		public void TestCheckBadge()
		{
			var password = Factory.New<GlbExternalPassword_GBForTest>();
			password.Badge = "ABC";
			AssertNoWarnings(password.BadgeInfo);
			password.Badge = string.Empty;
			AssertHasWarnings(password.BadgeInfo);
			password.Badge = "XYZ";
			AssertHasWarnings(password.BadgeInfo);
			password.Badge = "ABC";
			AssertNoWarnings(password.BadgeInfo);
		}

		void SetupPasswordsInDatabase()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CK1";
			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = company.PK;
			password.Badge = "ABC";
			password.EORI = "123";
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupPasswordsInDatabase();
		}
	}

	class GlbExternalPassword_GBForTest : GlbExternalPassword_GB
	{
		public GlbExternalPassword_GBForTest(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordLookups_GBForTest(this);
		}
	}

	class GlbExternalPasswordLookups_GBForTest : GlbExternalPasswordLookups_GB
	{
		public GlbExternalPasswordLookups_GBForTest(GlbExternalPassword_GBForTest parent)
		: base(parent)
		{
		}

		public override CodeDescriptionPairList BadgeCodes => new CodeDescriptionPairList { new CodeDescriptionPair("ABC", "ABC"), new CodeDescriptionPair("DEF", "DEF") };

		public override CodeDescriptionPairList EORIs => new CodeDescriptionPairList { new CodeDescriptionPair("123", "123"), new CodeDescriptionPair("456", "456") };
	}
}
