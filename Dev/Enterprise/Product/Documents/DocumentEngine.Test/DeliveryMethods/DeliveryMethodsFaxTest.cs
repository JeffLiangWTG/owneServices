using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class DeliveryMethodsFaxTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEmptyFaxNumber()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);

			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "alexander.korotun@cargowise.com";
			staff.GS_Code = "ZAC";
			Factory.Save();

			AssertEquals("Precondition: No emails in queue", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			DocContact.Fax = ZString.Empty;
			DeliveryMethod faxMethod = new Fax(DocContact);
			AssertEquals("Notification email to PostMasters has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef emailDef = Env.OutgoingMailManager.EmailsCreated[0];

			Assert("Email subject", emailDef.Subject.Contains("Fax Sending Failure"));
			AssertContains("Email body contains Company and Contact name", "Organization: Company Co.\r\n\t\tContact: Lisa Lupin", emailDef.Body);
		}

		[ExpectNoExceptions]
		public void TestValidFaxNumber()
		{
			DocContact.Fax = "324324324234";
			DeliveryMethod faxMethod = new Fax(DocContact);
		}

		public void TestMergeTiffsOnDelivery()
		{
			DocContact.Fax = "324324324234";
			Assert(new FaxForTest(DocContact, Instructions).MergeTiffsOnDelivery_Exposed);
		}

		class FaxForTest : Fax
		{
			public FaxForTest(DocDeliveryContact docContact, DeliveryInstructions instructions)
				: base(docContact)
			{ }

			public bool MergeTiffsOnDelivery_Exposed
			{
				get
				{
					return MergeTiffsOnDelivery;
				}
			}
		}

		DeliveryInstructions Instructions;
		DocDeliveryContact DocContact;

		protected override void SetUp()
		{
			Instructions = new DeliveryInstructions();
			DocContact = new DocDeliveryContact(Factory);
			DocContact.Name = "Lisa Lupin";
			DocContact.CompanyName = "Company Co.";
			DocContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Instructions.Recipients.Add(DocContact);
		}
	}
}
