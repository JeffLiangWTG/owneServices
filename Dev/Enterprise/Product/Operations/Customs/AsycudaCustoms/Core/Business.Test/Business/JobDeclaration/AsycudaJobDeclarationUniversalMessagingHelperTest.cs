using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class AsycudaJobDeclarationUniversalMessagingHelperTest : JobDeclarationUniversalMessagingHelperTest
	{
		public void TestSendAsycudaDeclarationUniversalMessage()
		{
			using (Factory.AddDisposableService())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "B00001000";
				declaration.JE_DeclarationType = "TTT";
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
				entryHeader.MovementReferenceNumber = "123456";
				var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader2.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday.AddDays(-1);
				entryHeader2.MovementReferenceNumber = "654321";
				Factory.Save();
				var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}

				var helper = new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);
				AssertEquals("2 message sent", 2, helper.SendAsycudaDeclarationUniversalMessage());
				AssertSendingMessage(entryHeader);
				AssertSendingMessage(entryHeader2);
			}
		}

		void AssertSendingMessage(CusEntryHeader entryHeader)
		{
			Assert(entryHeader.MessagesAreLoaded);
			AssertEquals(1, entryHeader.Messages.Count);
			var createdMessage = entryHeader.Messages[0];
			AssertTextContainsDispiteBlanks(expectedAsycudaDataSourceCollection.Replace("<<JOBREFERENCENUMBER>>", entryHeader.CH_BGMReference), createdMessage.EM_MessageText);
			AssertTextContainsDispiteBlanks(expectedActionPurpose, createdMessage.EM_MessageText);
			AssertTextContainsDispiteBlanks(expectedRecipientRole, createdMessage.EM_MessageText);
			AssertContains(entryHeader.MovementReferenceNumber, createdMessage.EM_MessageText);
			AssertEquals("Message Interpretation should be created", 1, createdMessage.Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Description).Length);
			var interchange = createdMessage.Interchange;
			CombineAssertions(() =>
			{
				AssertTextContainsDispiteBlanks(expectedAsycudaDataSourceCollection.Replace("<<JOBREFERENCENUMBER>>", entryHeader.CH_BGMReference), interchange.EI_BodyText);
				AssertTextContainsDispiteBlanks(expectedActionPurpose, interchange.EI_BodyText);
				AssertTextContainsDispiteBlanks(expectedRecipientRole, interchange.EI_BodyText);
			});
		}

		public void TestSendEmail()
		{
			using (var stream = new MemoryStream())
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
				var helper = new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);
				GlbStaff.CurrentUser.GS_EmailAddress = "test@email.com";
				helper.SendEmail(entry, stream, GlbStaff.CurrentUser);
				var subject = "Asycuda World Brokerage File – " + entry.CH_BGMReference;
				var query = new ZQuery(MailDBItemsSchema.MI_Subject, subject);
				var email = Factory.Load<MailItem>(query);
				AssertNotNull(email);
				AssertEquals(1, email.Length);
				var content = @"Dear Client,
	<br />Enclosed please find the mapped ASYCUDA World Brokerage file.
	<br />
	<br />Regards,
	<br />WiseTech Global
	";
				AssertTextContainsDispiteBlanks(content, email[0].MI_Body);
				var emailRecipient = Factory.Load<MailRecipient>(new ZQuery(MailDBRecipientsSchema.MR_MI, email[0].PK));
				AssertNotNull(emailRecipient);
				AssertEquals(1, emailRecipient.Length);
				var address = GlbStaff.CurrentUser.GS_EmailAddress;
				AssertEquals(address, emailRecipient[0].MR_RecipientMailAddress);
				var emailAttachments = Factory.Load<MailAttachment>(new ZQuery(MailDBAttachmentsSchema.MA_MI, email[0].PK));
				AssertNotNull(emailAttachments);
				AssertEquals(3, emailAttachments.Length);
				var expectFileName = helper.GetAttachmentFileName(entry);
				Assert(emailAttachments.Any(x => x.MA_FileName == expectFileName));
			}
		}

		public void TestGetAttachmentFileName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
			var helper = new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);
			AssertEquals(entry.CH_BGMReference + "_" + declaration.JE_AgentsReference + "_Asycuda.xml", helper.GetAttachmentFileName(entry));
		}

		readonly string expectedRecipientRole = @"
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ASY</Code>
          <Description>ASYCUDADeclaration</Description>
        </RecipientRole>
      </RecipientRoleCollection>";
		readonly string expectedActionPurpose = @"
      <ActionPurpose>
        <Code>YBW</Code>
        <Description>ASYCUDADeclaration</Description>
      </ActionPurpose>";
		readonly string expectedAsycudaDataSourceCollection = @"
	<DataSourceCollection>
        <DataSource>
          <Type>AsycudaDeclaration</Type>
          <Key><<JOBREFERENCENUMBER>></Key>
        </DataSource>
    </DataSourceCollection>";
	}
}
