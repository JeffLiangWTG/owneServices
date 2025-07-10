using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class MessageProcessorWithEmailNotificationTest<TMessageProcessor, T> : MessageProcessorTest<TMessageProcessor, T>
		where TMessageProcessor : MessageProcessor<T>
	{
		protected abstract IRegistryItem EmailGroupNotificationRegistryItem { get; }
		protected abstract string ExpectedEmailSubject { get; }
		protected abstract string[] ExpectedEmailBody { get; }

		public void TestGetEmailGroupRegistryItem()
		{
			(_, var incomingMessage) = PrepareForEmailTesting();

			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, EmailRegistryItemBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.NoEmails, notificationGroupPK)))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);
					AssertEquals("SendMode being NoEmails, no email should be created.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				}

				incomingMessage.EM_Status = "QUE";
				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, EmailRegistryItemBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMember, notificationGroupPK)))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
					AssertEquals("SendMode being StaffMember, should have created an email for Staff.", StaffEmail, email.Recipients.Cast<RecipientDef>().Single().Email);
				}

				incomingMessage.EM_Status = "QUE";
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, EmailRegistryItemBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, notificationGroupPK)))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
					CombineAssertions("SendMode being Staff&Group, should have created an email for both Staff and Group.", () =>
					{
						var recipients = email.Recipients.Cast<RecipientDef>().Select(r => r.Email).ToArray();
						AssertEquals("Recipients Count", 2, recipients.Length);
						Assert("Recipients should contain Staff email.", recipients.Contains(StaffEmail));
						Assert("Recipients should contain Group email.", recipients.Contains(GroupEmail));
					});
				}
			}
		}

		Guid EmailRegistryItemBranchPK => EmailGroupNotificationRegistryItem.Storage.HasFlag(RegistryStorageFlags.Branch)
			? Env.CurrentBranchPK
			: Guid.Empty;

		public void TestGetEmailGroupRegistryItem_UseManifestHeaderBranchToDetermineEmailRecipientConfiguration()
		{
			if (!EmailGroupNotificationRegistryItem.Storage.HasFlag(RegistryStorageFlags.Branch))
			{
				Assert(true);
			}
			else
			{
				(var manifestHeader, var incomingMessage) = PrepareForEmailTesting();

				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				manifestHeader.AMA_GB = branch1.PK;

				Factory.Save();

				using (Factory.AddDisposableService())
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, new GroupNotification(Core.Constants.EmailTo.NoEmails, notificationGroupPK)))
					{
						Processor.PreProcessMessage(incomingMessage);
						Processor.ProcessMessage(incomingMessage);
						AssertEquals("Still create email when registry branch is different from manifest header branch", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					}

					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, new GroupNotification(Core.Constants.EmailTo.NoEmails, notificationGroupPK)))
					{
						Processor.PreProcessMessage(incomingMessage);
						Processor.ProcessMessage(incomingMessage);
						AssertEquals("No email created when registry branch is same as manifest header branch", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					}
				}
			}
		}

		public void TestProcessMessageCoreWithNoRegistrySetup_AssertCorrectEmailContent()
		{
			(var manifestHeader, var incomingMessage) = PrepareForEmailTesting();

			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				EUICS2MessageTestHelper.AssertEmail(ExpectedEmailSubject, ExpectedEmailBody, new[] { new ZString(StaffEmail) });

				TestProcessMessageCore_MessageProcessedStatus(incomingMessage);
				TestProcessMessageCore_AdditionalAssertion(manifestHeader);
			}
		}

		public void TestMessageInterpretation()
		{
			(var manifestHeader, var incomingMessage) = PrepareForEmailTesting();

			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				foreach (var expectedBodyText in ExpectedEmailBody)
				{
					Assertion.AssertContains("Email body should contain", expectedBodyText, incomingMessage.EM_MessageInterpretation);
				}
			}
		}

		protected virtual void TestProcessMessageCore_MessageProcessedStatus(TestEdiMessage incomingMessage)
		{
			AssertEquals("The message status should be 'PRS'.\r\n" +
				"If you overrided SetMessageProcessedStatus to set a different message status then please also override me to make the correct assertion.",
				EDIMessageStatusList.Codes.ProcessedOK,
				incomingMessage.EM_Status);
		}

		protected virtual void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
		}

		#region Setup

		protected virtual (AsycudaManifestHeader Header, TestEdiMessage Message) PrepareForEmailTesting()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = CommonManifestJobReference;
			manifestHeader.AMA_MasterBill = CommonMasterBill;

			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, EntryNumberTypeToCreate, CommonReferenceNumber);

			CreateOutgoingMessage(manifestHeader);

			var incomingMessage = GetIncomingMessage(CommonReferenceNumber);

			Factory.Save();

			return (manifestHeader, incomingMessage);
		}

		protected virtual void CreateOutgoingMessage(AsycudaManifestHeader manifestHeader)
		{
			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_MessageText = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><IE3F10 xmlns=\"urn:wco:datamodel:eu:ics2:2\"><MRN>{CommonReferenceNumber}</MRN></IE3F10>";
			outgoingMessage.EM_SystemCreateUser = staffCode;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupUserAndNotificationGroup();
		}

		protected virtual string EntryNumberTypeToCreate => CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;

		protected const string StaffEmail = "DefaultStaff@email.com";
		const string GroupEmail = "Defaultgroup@email.com";
		protected const string CommonReferenceNumber = "EmailTestReferenceNumber";
		protected const string CommonManifestJobReference = "MAN0009999";
		protected const string CommonMasterBill = "CommonMasterBill";

		protected ZGuid notificationGroupPK;
		protected ZString staffCode;

		void SetupUserAndNotificationGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DT";
			staff.GS_FullName = "Default Staff";
			staff.GS_LoginName = "Default Staff";
			staff.GS_EmailAddress = StaffEmail;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DG";
			group.GG_Desc = "Default Group";

			var groupStaff = group.Staff.AddNew();
			groupStaff.GS_Code = "GS";
			groupStaff.GS_FullName = "Group Staff";
			groupStaff.GS_LoginName = "Group Staff";
			groupStaff.GS_EmailAddress = GroupEmail;

			staffCode = staff.GS_Code;
			notificationGroupPK = group.PK;

			Factory.Save();
		}

		#endregion
	}
}
