using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Testing
{
	[TestedType(typeof(OrganisationUnmatchedNotification))]
	sealed class OrganisationUnmatchedNotificationTest : NotificationTest<OrganisationUnmatchedNotification>
	{
		public void TestMessageAvailableOnlyAfterSaveInterface()
		{
			var bO = Factory.New<OrgHeader>();
			var notification = new OrganisationUnmatchedNotification(bO, null);
			AssertEquals("The message is only available after the factory is saved", true, notification is INotificationWithMessageForAfterSave);
		}

		public void TestDisplayMessage()
		{
			var testOrg = CreateTestOrg();
			var notification = new OrganisationUnmatchedNotification(testOrg, null);
			AssertEquals(Res.GetString("97352186-026d-4085-bdd0-f6faf0d24566", "Failed to matched organization with code/name '") + "AUMEL' / 'Full Name 1'", notification.Message);
			var list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("zzz", "ABC"));
			notification = new OrganisationUnmatchedNotification(testOrg, list);
			AssertEquals(Res.GetString("97352186-026d-4085-bdd0-f6faf0d24566", "Failed to matched organization with code/name '") + "AUMEL' / 'Full Name 1'" + ", zzz: ABC", notification.Message);

			testOrg.Delete();
			notification = new OrganisationUnmatchedNotification(testOrg, list);
			AssertEquals("No Notifications for deleted organization", ZString.Empty, notification.Message);
		}

		public void TestMultiLineDisplayMessage()
		{
			var testOrg = CreateTestOrg();
			var list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("Mapping Org", "XYZ"));
			var notification = new OrganisationUnmatchedNotification(testOrg, list);
			AssertEquals(Expected, notification.MultiLineDisplayMessage);

			testOrg.OH_FullName = "Full Name 2";
			testOrg.OH_Code = "AUMEL";
			var expectedWithUpdatedOhFullName = Expected.Replace("Full Name 1", "Full Name 2");
			AssertEquals(expectedWithUpdatedOhFullName, notification.MultiLineDisplayMessage);

			testOrg.Delete();
			AssertEquals("", notification.MultiLineDisplayMessage);

			AssertEquals("", notification.MultiLineDisplayMessage);
		}

		public void TestLongStringDataGetsTruncated()
		{
			var testOrg = Factory.New<OrgHeader>();
			var builder = new StringBuilder();
			var notification = new OrganisationUnmatchedNotificationForTest(testOrg);

			var name = "Name is going to be longer than 15 char";
			var data = "Data is going to be longer than 30 char, 0123456789 0123456789 0123456789";
			notification.AppendLine(builder, name, data);
			AssertEquals("Name is going t  Data is going to be longer tha\r\n", builder.ToString());
		}

		#region Implementation

		protected override OrganisationUnmatchedNotification NewTestNotification()
		{
			return new OrganisationUnmatchedNotification(Factory.New<OrgHeader>(), null);
		}

		protected override bool IsSerializable
		{
			get { return false; }
		}

		OrgHeader CreateTestOrg()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Full Name 1";
			org.OH_RL_NKClosestPort = "ITTRN";
			org.OH_Code = "AUMEL";
			org.MainAddress.OA_Address1 = "Address Line1";
			org.MainAddress.OA_Address2 = "Address Line 2";
			org.MainAddress.OA_Phone = "9903456";
			org.MainAddress.OA_Fax = "9998888";
			org.MainAddress.OA_Email = "test@edi.com.au";

			return org;
		}

		readonly string Expected =
			"Code             AUMEL                         \r\n" +
			"Full Name        Full Name 1                   \r\n" +
			"Address          Address Line1                 \r\n" +
			"                 Address Line 2                \r\n" +
			"Closest Port     ITTRN                         \r\n" +
			"Phone            9903456                       \r\n" +
			"Fax              9998888                       \r\n" +
			"Email            test@edi.com.au               \r\n" +
			"Mapping Org      XYZ                           \r\n";

		#region OrganisationMatchedNotificationForTest

		class OrganisationUnmatchedNotificationForTest : OrganisationUnmatchedNotification
		{
			public OrganisationUnmatchedNotificationForTest(OrgHeader unmatchedOrg)
				: base(unmatchedOrg, null)
			{
			}

			public new void AppendLine(StringBuilder builder, ZString name, ZString data)
			{
				base.AppendLine(builder, name, data);
			}
		}

		#endregion

		#endregion
	}
}
