using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(RefLocalLanguage))]
	sealed class RefLocalLanguageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFullLanguageCode()
		{
			LocalLanguage.RA_Code = "KKK";
			LocalLanguage.RA_RN_NKCountryCode = "CN";
			AssertEquals("Full language code should be combined with Code and Country code", "KKK-CN", LocalLanguage.FullLanguageCode);
		}

		public void TestDefaultParentLanguage()
		{
			LocalLanguage.RA_Code = "DE";
			AssertEquals("ParentLanguageCode should be defaulted", Core.SharedConstants.Languages.German, LocalLanguage.ParentLanguageCode);
		}

		#region System Defined Languages

		public void TestIsSystemReadOnly()
		{
			Assert(LocalLanguage.RA_IsSystem_ReadOnly);
		}

		public void TestRACodeReadOnly()
		{
			LocalLanguage.RA_IsSystem = false;
			LocalLanguage.OnLoaded();
			Assert("RN_Code should not be read only", !LocalLanguage.RA_CodeInfo.ReadOnly);
			LocalLanguage.RA_IsSystem = true;
			LocalLanguage.OnLoaded();
			Assert("RN_Code should be read only", LocalLanguage.RA_CodeInfo.ReadOnly);
		}

		public void TestRADescReadOnly()
		{
			var originalIsController = GlbStaff.CurrentUser.GS_IsController;

			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				LocalLanguage.RA_IsSystem = false;
				AssertEquals("Precondition: Description is not readonly", false, LocalLanguage.RA_DescriptionInfo.ReadOnly);

				LocalLanguage.RA_Description = "My Language";
				LocalLanguage.RA_IsSystem = true;

				AssertEquals("Description is readonly", true, LocalLanguage.RA_DescriptionInfo.ReadOnly);

				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals("Description is not readonly", false, LocalLanguage.RA_DescriptionInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = originalIsController;
			}
		}

		public void TestRAParentLanguageReadOnly()
		{
			LocalLanguage.RA_IsSystem = false;
			LocalLanguage.OnLoaded();
			Assert("RA_RA_ParentLanguage should not be read only", !LocalLanguage.RA_RA_ParentLanguageInfo.ReadOnly);
			Assert("ParentLanguageCode should not be read only", !LocalLanguage.ParentLanguageCodeInfo.ReadOnly);
			LocalLanguage.RA_IsSystem = true;
			LocalLanguage.OnLoaded();
			Assert("RA_RA_ParentLanguage should be read only", LocalLanguage.RA_RA_ParentLanguageInfo.ReadOnly);
			Assert("ParentLanguageCode should be read only", LocalLanguage.ParentLanguageCodeInfo.ReadOnly);
		}

		public void TestRACountryCodeReadOnly()
		{
			LocalLanguage.RA_IsSystem = false;
			LocalLanguage.OnLoaded();
			Assert("RA_RN_NKCountryCode should not be read only", !LocalLanguage.RA_RN_NKCountryCodeInfo.ReadOnly);
			LocalLanguage.RA_IsSystem = true;
			LocalLanguage.OnLoaded();
			Assert("RA_RN_NKCountryCode should be read only", LocalLanguage.RA_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestIsActiveReadOnly()
		{
			LocalLanguage.RA_IsSystem = false;
			LocalLanguage.OnLoaded();
			Assert("RA_IsActive should not be read only", !LocalLanguage.RA_IsActiveInfo.ReadOnly);
			LocalLanguage.RA_IsSystem = true;
			LocalLanguage.OnLoaded();
			Assert("RA_IsActive should be read only", LocalLanguage.RA_IsActiveInfo.ReadOnly);
		}

		#endregion

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		RefLocalLanguage LocalLanguage;

		protected override void SetUp()
		{
			base.SetUp();
			LocalLanguage = Factory.New<RefLocalLanguage>();
			LocalLanguage.RA_Code = "CN";
		}

		#endregion
	}
}
