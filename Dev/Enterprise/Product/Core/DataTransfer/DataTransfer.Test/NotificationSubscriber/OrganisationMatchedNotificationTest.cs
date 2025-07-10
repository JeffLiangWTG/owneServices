using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Testing
{
	[TestedType(typeof(OrganisationMatchedNotification))]
	sealed class OrganisationMatchedNotificationTest : NotificationTest<OrganisationMatchedNotification>
	{
		public void TestMessageAvailableOnlyAfterSaveInterface()
		{
			var bO = Factory.New<OrgHeader>();
			var notification = new OrganisationMatchedNotification(new Xsd.Organisation(), bO, null);
			AssertEquals("The message is only available after the factory is saved", true, notification is INotificationWithMessageForAfterSave);
		}

		public void TestDisplayMessage()
		{
			var xsdTestOrg1 = CreateTestOrg1();
			var testOrg2 = CreateTestOrg2();
			testOrg2.FillWithValidTestData();
			testOrg2.Factory.Save();

			OrganisationMatchedNotification notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg2, null);
			AssertEquals(Res.GetString("7682b1d6-90f5-48cf-9f38-271e2d6ed05b", "Successfully matched organization with code '{0}'", "TESTORG2"), notification.Message);

			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "ZZZ");
			notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg2, list);
			AssertEquals("Successfully matched organization with code 'TESTORG2', aaa: ZZZ", notification.Message);

			testOrg2.Delete();
			notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg2, list);
			AssertEquals("No Notifications for deleted organization", ZString.Empty, notification.Message);

			OrgHeader testOrg3 = Factory.Load<OrgHeader>(Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg3, null);
			AssertEquals(Res.GetString("79f06162-5e53-46d5-b035-cb0aaa58e61e", "Organization not found - Assigned to UNMATCHED organization ({0})", "Name: 'Full Name 1'"), notification.Message);

			xsdTestOrg1.OwnerCode = "TESORG1";
			notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg3, null);
			AssertEquals(Res.GetString("79f06162-5e53-46d5-b035-cb0aaa58e61e", "Organization not found - Assigned to UNMATCHED organization ({0})", "Name: 'Full Name 1'  Code: 'TESORG1'"), notification.Message);

			notification = new OrganisationMatchedNotification(xsdTestOrg1, testOrg3, list);
			AssertEquals(Res.GetString("79f06162-5e53-46d5-b035-cb0aaa58e61e", "Organization not found - Assigned to UNMATCHED organization ({0})", "Name: 'Full Name 1'  Code: 'TESORG1'") + ", aaa: ZZZ", notification.Message);
		}

		public void TestMultiLineDisplayMessage()
		{
			Xsd.Organisation testOrg1 = CreateTestOrg1();
			OrgHeader testOrg2 = CreateTestOrg2();
			var list = new CodeDescriptionPairList();
			list.AddPair("Mapping Org", "ZZZZ");
			OrganisationMatchedNotification notification = new OrganisationMatchedNotification(testOrg1, testOrg2, list);
			AssertEquals(Expected, notification.MultiLineDisplayMessage);

			testOrg1.OrganisationDetails.Name = "Full Name 2";
			string expectedWithUpdatedOH_FullName = Expected.Replace("Full Name 1", "Full Name 2");
			AssertEquals(expectedWithUpdatedOH_FullName, notification.MultiLineDisplayMessage);

			testOrg2.Delete();
			AssertEquals("", notification.MultiLineDisplayMessage);
		}

		public void TestLongStringDataGetsTruncated()
		{
			Xsd.Organisation testOrg1 = new Xsd.Organisation();
			OrgHeader testOrg2 = Factory.New<OrgHeader>();
			StringBuilder builder = new StringBuilder();
			OrganisationMatchedNotificationForTest notification = new OrganisationMatchedNotificationForTest(testOrg1, testOrg2);

			string name = "Name is going to be longer than 15 char";
			string input = "Input is going to be longer than 30 char, 0123456789 0123456789 0123456789";
			string matched = "Matched is going to be longer than 30 char, 0123456789 0123456789 0123456789";
			notification.AppendLine(builder, name, input, matched);
			AssertEquals("Name is going t  Input is going to be longer th  Matched is going to be longer \r\n", builder.ToString());
		}

		#region Implementation

		protected override OrganisationMatchedNotification NewTestNotification()
		{
			return new OrganisationMatchedNotification(new Xsd.Organisation(), Factory.New<OrgHeader>(), null);
		}

		protected override bool IsSerializable
		{
			get { return false; }
		}

		Xsd.Organisation CreateTestOrg1()
		{
			Xsd.Organisation org = new Xsd.Organisation();
			org.OrganisationDetails = new Xsd.OrganisationDetail();
			org.OrganisationDetails.Name = "Full Name 1";
			org.OrganisationDetails.Location = new Xsd.UNLOCO();
			org.OrganisationDetails.Location.Value = "IDJKT";
			Xsd.OrgAddress mainAddress = org.OrganisationDetails.Addresses.AddNew();
			Xsd.AddressCapability capability = mainAddress.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			capability.AddressTypeSpecified = true;
			mainAddress.AddressLine1 = "Address Line1";
			mainAddress.AddressLine2 = "Address Line 2";
			mainAddress.TelephoneNumbers = new Xsd.TelephoneNumberCollection();

			Xsd.TelephoneNumber phone = mainAddress.TelephoneNumbers.AddNew();
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			phone.Value = "9903456";

			Xsd.TelephoneNumber fax = mainAddress.TelephoneNumbers.AddNew();
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			fax.Value = "9998888";

			mainAddress.Email = "test@edi.com.au";

			return org;
		}

		OrgHeader CreateTestOrg2()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Organisation 22";
			org.OH_RL_NKClosestPort = "ITTRN";
			org.OH_Code = "TESTORG2";
			org.MainAddress.OA_Address1 = "Sunset Boulevard";
			org.MainAddress.OA_Address2 = "Of A broken dream";
			org.MainAddress.OA_Phone = "8888888";
			org.MainAddress.OA_Fax = "7178987";
			org.MainAddress.OA_Email = "frankfurt@edi.com.au";

			return org;
		}

		const string Expected =
			"                 Input                           Matched                       \r\n" +
			"                 ------------------------------  ------------------------------\r\n" +
			"Code             -                               TESTORG2                      \r\n" +
			"Full Name        Full Name 1                     Test Organisation 22          \r\n" +
			"Address          Address Line1                   Sunset Boulevard              \r\n" +
			"                 Address Line 2                  Of A broken dream             \r\n" +
			"Closest Port     IDJKT                           ITTRN                         \r\n" +
			"Phone            9903456                         8888888                       \r\n" +
			"Fax              9998888                         7178987                       \r\n" +
			"Email            test@edi.com.au                 frankfurt@edi.com.au          \r\n" +
			"Mapping Org      -                               ZZZZ                          \r\n";

		#region OrganisationMatchedNotificationForTest

		class OrganisationMatchedNotificationForTest : OrganisationMatchedNotification
		{
			public OrganisationMatchedNotificationForTest(Xsd.Organisation inputOrg, OrgHeader matchedOrg)
				: base(inputOrg, matchedOrg, null)
			{
			}

			public new void AppendLine(StringBuilder builder, ZString name, ZString input, ZString matched)
			{
				base.AppendLine(builder, name, input, matched);
			}
		}

		#endregion

		#endregion
	}
}
