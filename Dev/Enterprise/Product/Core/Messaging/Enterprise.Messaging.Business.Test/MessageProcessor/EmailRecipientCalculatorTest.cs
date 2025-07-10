using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Business.Testing
{
	class EmailRecipientCalculatorTest : TestCaseWithFactory
	{
		public void TestThereIsNoEmailHasNoFromAddressExceptionWhenFromAddressIsEmpty()
		{
			using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var sMTPDefaultReturnEmailAddressRegistryItem = typeof(RawDataRegistry).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "SMTPDefaultReturnEmailAddress");
				var sMTPDefaultReturnEmailAddressRegistryItemValue = (IRegistryItem)sMTPDefaultReturnEmailAddressRegistryItem.GetValue(RawDataRegistry.Instance);
				using (sMTPDefaultReturnEmailAddressRegistryItemValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					var group = Factory.NewWithValidTestData<GlbGroup>();
					var staff1 = Factory.NewWithValidTestData<GlbStaff>();
					staff1.GS_EmailAddress = "staff1@cargowise.com";
					group.Staff.Add(staff1);
					Factory.Save();

					var registryItem = new GuidRegistryItem("Dummy", (NoResString)"Dummy Group", (NoResString)"Dummy Caption", (NoResString)"DummyHint",
					RegistryStorageFlags.Company, RegistryOptions.Default, Core.Constants.Groups.PostMastersGroupPK);

					registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
					var email = new EmailDef() { Subject = "Dummy Email", Body = "Dummy Body" };
					AssertEquals("Should Be Empty", ZString.Empty, email.FromAddress);

					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					email.Recipients.Clear();
					var emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMember, group.PK, "staff@cargowise.com", group.PK);
					AssertNoExceptionThrown(() =>
					{
						emailRepCal.SendNotifications(Factory, email, registryItem);
					});
				}
			}
		}

		public void TestSendNotifications()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@cargowise.com";
			group.Staff.Add(staff1);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@cargowise.com";
			group.Staff.Add(staff2);

			Factory.Save();

			var registryItem = new GuidRegistryItem("Dummy", (NoResString)"Dummy Group", (NoResString)"Dummy Caption", (NoResString)"DummyHint",
				RegistryStorageFlags.Company, RegistryOptions.Default, Core.Constants.Groups.PostMastersGroupPK);

			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var email = new EmailDef() { Subject = "Dummy Email", Body = "Dummy Body" };

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			var emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMember, group.PK, "staff@cargowise.com", group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMember, group.PK, new ZString[] { "staff@cargowise.com", "staff10@cargowise.com" }, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff10@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.NoEmails, group.PK, "staff@cargowise.com", group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.NominatedGroup, group.PK, "staff@cargowise.com", group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.NominatedGroup, group.PK, new ZString[] { "staff@cargowise.com", "staff10@cargowise.com" }, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, "staff@cargowise.com", group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(3, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, new ZString[] { "staff@cargowise.com", "staff10@cargowise.com" }, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(4, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff10@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(GroupNotification.StaffMemberOrNominatedGroup, group.PK, "staff@cargowise.com", group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(GroupNotification.StaffMemberOrNominatedGroup, group.PK, ZString.Empty, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(GroupNotification.StaffMemberOrNominatedGroup, ZGuid.Empty, ZString.Empty, Guid.Empty);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMember, ZGuid.Empty, ZString.Empty, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff1@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff2@cargowise.com"));
		}

		public void TestNotToSendNotificationsWhenNoRecipients()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(staff1);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "";
			group.Staff.Add(staff2);

			Factory.Save();

			var registryItem = new GuidRegistryItem("Dummy", (NoResString)"Dummy Group", (NoResString)"Dummy Caption", (NoResString)"DummyHint",
				RegistryStorageFlags.Company, RegistryOptions.Default, Core.Constants.Groups.PostMastersGroupPK);

			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var email = new EmailDef() { Subject = "Dummy Email", Body = "Dummy Body" };

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			email.Recipients.Clear();
			var emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, ZString.Empty, ZGuid.Empty);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			staff1.GS_EmailAddress = "staff@cargowise.com";
			Factory.Save();
			email.Recipients.Clear();
			emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, new ZString[] { "staff10@cargowise.com" }, group.PK);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Count);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff@cargowise.com"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("staff10@cargowise.com"));
		}

		public void TestEmailSavedAfterCreated()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff@cargowise.com";
			group.Staff.Add(staff1);
			Factory.Save();

			var registryItem = new GuidRegistryItem("Dummy", (NoResString)"Dummy Group", (NoResString)"Dummy Caption", (NoResString)"DummyHint",
				RegistryStorageFlags.Company, RegistryOptions.Default, Core.Constants.Groups.PostMastersGroupPK);

			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var email = new EmailDef() { Subject = "Dummy Email", Body = "Dummy Body" };

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var emailRepCal = new EmailRecipientCalculator(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, ZString.Empty, ZGuid.Empty);
			emailRepCal.SendNotifications(Factory, email, registryItem);
			AssertEquals("Email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var mailQueryText = $"SELECT * FROM dbo.MailDBItems WHERE MI_Subject = 'Dummy Email'";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Email created but not saved into database when factory is not null", 0, collection.Count);

			Factory.Save();
			collection.Load(mailQueryText);
			AssertEquals("Email saved into database after factory is saved", 1, collection.Count);

			var email2 = new EmailDef() { Subject = "Dummy Email 2", Body = "Dummy Body 2" };
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			emailRepCal.SendNotifications(null, email2, registryItem);
			AssertEquals("Email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			mailQueryText = $"SELECT * FROM dbo.MailDBItems WHERE MI_Subject = 'Dummy Email 2'";
			collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Email created and saved into database when factory is null", 1, collection.Count);
		}
	}
}
