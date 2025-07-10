using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateExpiryNotificationGroup))]
	class EInvoicingCertificateExpiryNotificationGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new EInvoicingCertificateExpiryNotificationGroup();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var result = new EInvoicingCertificateExpiryNotificationGroup();

			return result;
		}

		public void TestNotificationGroup()
		{
			var osmgGroup = Factory.NewWithValidTestData<GlbGroup>();
			osmgGroup.GG_Code = "OG1";
			osmgGroup.GG_Desc = "osmgGroup1";
			Factory.Save();

			BizObj.AlertDays = 0;
			BizObj.NotificationGroup = ZGuid.Invalid;
			AssertHasError(BizObj.NotificationGroupInfo, "Enter a valid selection.");
			BizObj.NotificationGroup = ZGuid.Empty;
			AssertNoErrors("NotificationGroup could be empty when AlertDays is 0.", BizObj.NotificationGroupInfo);
			BizObj.NotificationGroup = osmgGroup.PK;
			AssertNoErrors(BizObj.NotificationGroupInfo);

			BizObj.AlertDays = 1;
			BizObj.NotificationGroup = ZGuid.Invalid;
			AssertHasError(BizObj.NotificationGroupInfo, "Enter a valid selection.");
			BizObj.NotificationGroup = ZGuid.Empty;
			AssertHasError("NotificationGroup should not be empty when AlertDays is greater than 0.", BizObj.NotificationGroupInfo, "Please enter a value.");
			BizObj.NotificationGroup = osmgGroup.PK;
			AssertNoErrors(BizObj.NotificationGroupInfo);
		}

		public void TestAlertDays()
		{
			BizObj.AlertDays = 366;
			AssertHasError(BizObj.AlertDaysInfo, "The day value must be less than or equal to the maximum 365.");

			BizObj.AlertDays = 365;
			AssertNoErrors("AlertDays should have no error when value is less than 365.", BizObj.AlertDaysInfo);
		}

		public void TestNotificationGroupList()
		{
			var groupA = Factory.NewWithValidTestData<GlbGroup>();
			groupA.GG_Desc = "groupA1";
			var groupB = Factory.NewWithValidTestData<GlbGroup>();
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			groupA.Organisation.Add(orgA);
			Factory.Save();

			BizObj.NotificationGroupList.Load();
			Assert(BizObj.NotificationGroupList.Contains(groupA.PK));
			Assert(BizObj.NotificationGroupList.Contains(groupB.PK));
			AssertEquals("groupA1", (BizObj.NotificationGroupList as IFindBoxListProvider).DescriptionFromPrimaryKey(groupA.PK));
			AssertEquals(null, (BizObj.NotificationGroupList as IFindBoxListProvider).DescriptionFromPrimaryKey(ZGuid.Empty));
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new EInvoicingCertificateExpiryNotificationGroup BizObj
		{
			get { return (EInvoicingCertificateExpiryNotificationGroup)base.BizObj; }
		}

		#endregion Implementation
	}
}