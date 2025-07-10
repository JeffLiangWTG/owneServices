using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UserPortal;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(UserPortalDisclaimerBizO))]
	sealed class UserPortalDisclaimerBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldShowDisclaimer()
		{
			Assert(!CachedBusinessObject.DoNotShowAgainNextTime);
			Assert(UserPortalDisclaimerBizO.ShouldShowDisclaimer);

			CachedBusinessObject.DoNotShowAgainNextTime = true;
			Assert(CachedBusinessObject.DoNotShowAgainNextTime);
			Assert("Not yet stored in the registry", UserPortalDisclaimerBizO.ShouldShowDisclaimer);

			CachedBusinessObject.SaveToRegistry();
			Assert(!UserPortalDisclaimerBizO.ShouldShowDisclaimer);

			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert(UserPortalDisclaimerBizO.ShouldShowDisclaimer);
			}
			Assert(!UserPortalDisclaimerBizO.ShouldShowDisclaimer);
		}

		new UserPortalDisclaimerBizO CachedBusinessObject
		{
			get { return (UserPortalDisclaimerBizO)base.CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UserPortalDisclaimerBizO();
		}
	}
}
