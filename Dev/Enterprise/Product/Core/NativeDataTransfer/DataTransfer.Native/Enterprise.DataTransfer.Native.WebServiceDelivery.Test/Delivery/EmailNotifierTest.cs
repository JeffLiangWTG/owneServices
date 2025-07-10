using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.Testing
{
	sealed class EmailNotifierTest : TestCaseWithFactory
	{
		public void TestKnownExceptionsDontIncludeTheCallStack()
		{
			GlbGroup group = SetupGrouWithDaffyDuckEmail();
			NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();

			IEDICommunicationsMode mode = new LooneyToonsCommunicationsMode(Factory);

			Exception exception = new KnownErrorException("We know about this. We told you once, now go fix it.");
			try
			{ throw exception; }
			catch (Exception) { }

			AssertContains("Precondition: ", "TestKnownExceptionsDontIncludeTheCallStack()", exception.StackTrace);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var notifier = new EmailNotifier();
			notifier.Notify(Factory, exception, mode, null, null);

			AssertEquals("OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients.Count", 1, email.Recipients.Count);
			AssertEquals("email.Recipients[0].Email", "daffy.duck@cargowise.com", email.Recipients[0].Email);
			AssertEquals("email.Subject", "Workflow process delivery failed", email.Subject);

			CombineAssertions(delegate
			{
				AssertContains("email.Body should have the Message from the Exception", "We know about this. We told you once, now go fix it.", email.Body);
				AssertNotContains("email.Body should not have the Type of the Exception", "KnownErrorException", email.Body);
				AssertNotContains("email.Body should not have the Callstack of the Exception", "TestKnownExceptionsDontIncludeTheCallStack()", email.Body);
			});
		}

		public void TestNotifyingAnObjectRefNotSetToAnInstanceExceptionGivesTheCallStack()
		{
			GlbGroup group = SetupGrouWithDaffyDuckEmail();
			NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();

			IEDICommunicationsMode mode = new LooneyToonsCommunicationsMode(Factory);

			Exception exception = new NullReferenceException("Object reference not set to an instance of an object.");
			try
			{ throw exception; }
			catch (Exception) { }

			AssertContains("Precondition: ", "TestNotifyingAnObjectRefNotSetToAnInstanceExceptionGivesTheCallStack()", exception.StackTrace);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var notifier = new EmailNotifier();
			notifier.Notify(Factory, exception, mode, null, null);

			AssertEquals("OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients.Count", 1, email.Recipients.Count);
			AssertEquals("email.Recipients[0].Email", "daffy.duck@cargowise.com", email.Recipients[0].Email);
			AssertEquals("email.Subject", "Workflow process delivery failed", email.Subject);

			CombineAssertions(delegate
			{
				AssertContains("email.Body", "Destination\t\t: " + mode.EK_Destination, email.Body);
				AssertContains("email.Body", "Organization		:  <a href=\"", email.Body);
				AssertContains("email.Body", mode.Organisation.OH_Code + " - " + mode.Organisation.OH_FullName, email.Body);

				AssertContains("email.Body should have the Message from the Exception", "Object reference not set to an instance of an object.", email.Body);
				AssertContains("email.Body should have the Type of the Exception", "NullReferenceException", email.Body);
				AssertContains("email.Body should have the Callstack of the Exception", "TestNotifyingAnObjectRefNotSetToAnInstanceExceptionGivesTheCallStack()", email.Body);

				AssertContains("email.Body file name should be followed by HTML paragraphs so there's a break before the 'You have received this because...'"
					, "File name\t\t\t: " + mode.EK_Filename + "\r\n<p><p>"
					, email.Body);
			});
		}

		GlbGroup SetupGrouWithDaffyDuckEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "daffy.duck@cargowise.com";
			staff.GS_Code = "DDC";
			return group;
		}

		class LooneyToonsCommunicationsMode : IEDICommunicationsMode
		{
			internal LooneyToonsCommunicationsMode(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			ZString IEDICommunicationsMode.EK_CommunicationsTransport
			{
				get { return "XXX"; }
			}

			IOrgHeader IMessageDestinationSource.Organisation
			{
				get
				{
					if (organisation == null)
					{
						organisation = factory.NewWithValidTestData<OrgHeader>();
						organisation.OH_FullName = "Warner Brothers Looney Toons Enterprises";
						organisation.OH_Code = "WARBROLOOT";
					}
					return organisation;
				}
			}
			OrgHeader organisation;

			ZInt IEDICommunicationsMode.EK_PortNumber
			{
				get { return 42; }
			}

			ZString IEDICommunicationsMode.EK_Destination
			{
				get { return "Mars"; }
			}

			ZString IEDICommunicationsMode.EK_Filename
			{
				get { return ""; }
			}

			#region Unimplemented IEDICommunicationsMode Members

			ZGuid IEDICommunicationsMode.EK_ECC_CommunicationPartyConfig
			{
				get
				{
					return ZGuid.Empty;
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			ZString IEDICommunicationsMode.EK_FileFormat
			{
				get { throw new NotImplementedException(); }
			}

			ZDateTime IEDICommunicationsMode.EK_LastFailed
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			ZString IEDICommunicationsMode.EK_LocalPartyVanID
			{
				get { throw new NotImplementedException(); }
			}

			ZString IEDICommunicationsMode.EK_LoginName
			{
				get { throw new NotImplementedException(); }
			}

			ZString IEDICommunicationsMode.EK_MessagePurpose
			{
				get { throw new NotImplementedException(); }
			}

			ZString IEDICommunicationsMode.EK_Password
			{
				get { throw new NotImplementedException(); }
			}

			ZString IEDICommunicationsMode.EK_RelatedPartyVanID
			{
				get { throw new NotImplementedException(); }
			}

			ZString IEDICommunicationsMode.EK_ServerAddressSubject
			{
				get { throw new NotImplementedException(); }
			}

			ZBool IEDICommunicationsMode.EK_PublishInternalMilestones
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}
	}
}
