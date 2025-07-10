using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDEC275MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestPopulateMessages_ShouldIncludeNote_WhenDigitalSignatureDisabled()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var messageBuilder = GetMessageBuilder();
				var messageBuilderResult = messageBuilder.PopulateMessages();
				AssertEquals("IsSuccess = true", true, messageBuilderResult.IsSuccess);
				var buildResults = messageBuilderResult.GetBuilderResults();
				AssertEquals("One build result expected", 1, buildResults.Count());
				var buildResult = buildResults.First();
				var message = buildResult.Message;
				AssertNotNull("Build result still should have message", message);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message have note", 1, notes.Count);
				AssertEquals("The message have note with text", "Digital Signature is disabled by Registry", notes[0].ST_NoteText);
			}
		}

		public void TestPopulateMessages_ShouldFailWithError_WhenDigitalSignatureEnabled()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var messageBuilder = GetMessageBuilder();
				var messageBuilderResult = messageBuilder.PopulateMessages();
				AssertEquals("Populating should fail", false, messageBuilderResult.IsSuccess);
				var buildResults = messageBuilderResult.GetBuilderResults();
				AssertEquals("One build result expected", 1, buildResults.Count());
				var buildResult = buildResults.First();
				AssertContainsExactElementsInAnyOrder("There should be 1 error", new string[] { "No Valid digital sign certificate found for signing this message – please review your staff or company configuration" }, buildResult.Errors);
				AssertNotNull("Build result still should have message", buildResult.Message);
			}
		}

		[TestDate(2024, 09, 09)]
		public void TestPopulateMessages_ShouldReturnExternalPassword_WhenUserHaveValidCertificate()
		{
			var factory = Factory;
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DigitalSignatureFallbackList.Codes.StaffOnly))
			{
				var usrStaffWithCertificate = factory.NewWithValidTestData<GlbStaff>();
				usrStaffWithCertificate.GS_Code = "US1";
				usrStaffWithCertificate.GS_LoginName = "aaa";
				var wrapper = GlbStaffWrapper.Get(usrStaffWithCertificate);
				var usrExternalPassword = wrapper.PasswordCollection.AddNew();
				usrExternalPassword.GP_PasswordType = PasswordTypesList.Codes.ILS;
				usrExternalPassword.GP_CertificateAuthority = CertificateAuthoritiesList.Codes.Comsign;
				usrExternalPassword.GP_UserID = "usr.Name";
				usrExternalPassword.CurrentDecryptedPassword = "usrPass";

				var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany).GlbExternalPassword;
				credential.GP_MailBoxID = "560038416";
				factory.Save();

				var usrCode = GlbStaff.CurrentUser.GS_Code;
				try
				{
					var expectedMessageText = GetExpectedMessageText();
					var messageBuilder = GetMessageBuilder();
					GlbStaff.CurrentUser.GS_Code = "US1";
					var result = messageBuilder.PopulateMessages();

					CombineAssertions("When PopulateMessages", () =>
					{
						AssertNotNull(result);
						Assert("Success", result.IsSuccess);

						var linkedObject = GetLinkedObject();
						var query = new ZQuery();

						if (linkedObject != null)
						{
							query.AddToFilter(EDIMessageSchema.EM_LinkTable, linkedObject.TableName);
							query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkedObject.PK);
						}
						else
						{
							query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ILEDIInterchange.ApplicationCodes.ILCustoms);
							query.AddToFilter(EDIMessageSchema.EM_MessageType, GetExpectedMessageType());
							query.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetExpectedMessageSubType());
						}

						var message = Factory.LoadTop1<EDIMessage>(query);

						AssertNotNull("Message was created", message);
						AssertEquals("EM_ApplicationCode", "ILC", message.EM_ApplicationCode);
						AssertEquals("EM_MessageType", GetExpectedMessageType(), message.EM_MessageType);
						AssertEquals("EM_MessageSubType", GetExpectedMessageSubType(), message.EM_MessageSubType);
						AssertEquals("EM_Status", "QUE", message.EM_Status);
						AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);

						var messageOwnerCollection = GetMessageOwnerCollection();
						if (messageOwnerCollection != null)
						{
							AssertEquals("the message connected to parent", 1, messageOwnerCollection.Count);
						}
						AssertEquals("EM_GP should be same as user US1 ExternalPassword", usrExternalPassword.PK, message.EM_GP);
						AssertEquals("EM_MessageOwner", "ILCOM_REGISTERNO", message.EM_MessageOwner);
					});
				}
				finally
				{
					GlbStaff.CurrentUser.GS_Code = usrCode;
				}
			}
		}

		protected override IMessageBuilder GetMessageBuilder() => new ILDEC275MessageBuilder(entryHeader);

		protected override string GetExpectedMessageSubType() => "275";

		protected override string GetExpectedMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationRequest_275.xml"));

		protected override string GetExpectedMessageType() => "DEC";

		protected override BusinessObject GetLinkedObject() => entryHeader;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => entryHeader.Messages;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			jobDeclaration = factory.New<JobDeclaration>();
			entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
		}

		JobDeclaration jobDeclaration;
		CusEntryHeader entryHeader;
	}
}
