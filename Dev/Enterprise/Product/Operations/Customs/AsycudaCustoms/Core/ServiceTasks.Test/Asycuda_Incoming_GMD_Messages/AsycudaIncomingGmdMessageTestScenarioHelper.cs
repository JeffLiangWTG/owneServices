using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing
{
	class AsycudaIncomingGmdMessageTestScenarioHelper : TestCaseWithFactory
	{
		public void CheckScenario(AsycudaIncomingGmdMessageTestScenario scenario)
		{
			#region Prerequisites:

			var factory = Factory.CreateNewFactory();

			var query = new ZQuery();
			query.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, scenario.Declaration_Reference);
			var entry = factory.LoadTop1<Customs.Business.CusEntryHeader>(query);
			AssertNotNull("Prerequisite: The CusEntryHeader must be read from the database.", entry);
			var declaration = entry.Declaration;
			AssertNotNull("Prerequisite: The JobDeclaration must be read from the database.", declaration);

			query = new ZQuery();
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, scenario.MessageNo);
			var interchange = factory.LoadTop1<EDIInterchange>(query);
			AssertNotNull("Prerequisite: The EDIInterchange record must exist in the database.", interchange);

			#endregion

			AssertEquals("Check status of EDIInterchange after processing.", scenario.Expected_Interchange_Status, interchange.EI_Status);

			if (scenario.Expected_Interchange_Status == EDIInterchange.Status.Received)
			{
				#region Check EDIMessage:

				AssertEquals("Number of messages linked to CusEntryHeader", 1, entry.Messages.Count);

				var linkedMessage = entry.Messages[0];
				AssertEquals("Direction of EDIMessage must be RCV (Receive).", EDIInterchange.Direction.Receive, linkedMessage.EM_ReceiveTransmit);
				AssertEquals("Status of EDIMessage must be RCV (Received).", EDIInterchange.Status.Received, linkedMessage.EM_Status);
				AssertTextContainsDespiteBlanksAndBreaks("Message text of linked EDIMessage", scenario.Expected_AsycudaMessageText, linkedMessage.EM_MessageText);
				if (scenario.Type == AsycudaIncomingGmdMessageTestScenario.SUCCESS_TYPE_NO_IDENTATION_BODY)
				{
					AssertContains("Message text of linked EDIMessage", scenario.Expected_AsycudaMessageText, linkedMessage.EM_MessageText);
				}
				AssertEquals("EDIMessage must be linked to the correct EDIInterchange.", scenario.InterchangePK, linkedMessage.EM_EI);
				AssertEquals("EDIMessage.EM_ApplicationCode must be ZZA.", "ZZA", linkedMessage.EM_ApplicationCode);

				#endregion

				#region Check E-mail:

				query = new ZQuery();
				query.AddToFilter(MailDBItemsSchema.MI_Subject, scenario.Expected_EmailSubject);
				query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
				var email = factory.LoadTop1<MailItem>(query);

				AssertNotNull("An e-mail must have been created.", email);
				AssertEquals("Send e-mail to the correct recipient.", scenario.User_ToReceiveEmail.GS_EmailAddress, email.AllRecipients);
				AssertTextContainsDespiteBlanksAndBreaks("Text of the e-mail body", scenario.Expected_EmailBody, email.MI_Body);
				AssertNotNull(email.MailAttachments);
				AssertEquals("Three e-mail attachments are expected.", 3, email.MailAttachments.Count);

				var attachments = new List<MailAttachment>(email.MailAttachments.ToArray<MailAttachment>());
				var attachment = attachments.Find(x => x.MA_FileName == scenario.Expected_EmailAttachmentFileName);
				AssertNotNull("Find the e-mail attachment with the expected attachment filename.", attachment);

				var dataAsText = ExtractDataFromEmailAttachment(attachment);
				AssertTextContainsDespiteBlanksAndBreaks("Check attachment data content.", scenario.Expected_AsycudaMessageText, dataAsText);

				AssertEquals(1, declaration.DocManagerInfo.AllEDocs.Count);
				var validFileName = string.Concat(scenario.Expected_EmailAttachmentFileName.Split(Path.GetInvalidFileNameChars()));
				AssertEquals(validFileName, declaration.DocManagerInfo.AllEDocs[0].FileName);
				AssertEquals(Core.Constants.RefDocTypes.Manifest, declaration.DocManagerInfo.AllEDocs[0].DocType);
				AssertTextContainsDespiteBlanksAndBreaks("Check edoc data content.", scenario.Expected_AsycudaMessageText, declaration.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream());

				#endregion

				#region Notes on Success:

				AssertEquals("Total Notes if processed successfully.", 1, interchange.Notes.DatabaseCount);
				var note = (StmNote)interchange.Notes.GetAllNotes().ToList()[0];
				AssertEquals("Note description if processed successfully.", "Data Import Log Text", note.ST_Description);
				var expectedNoteText = $"Linked Asycuda message to Declaration {scenario.Declaration_Reference}.";
				AssertEquals("Note text if processed successfully.", expectedNoteText, note.ST_NoteText);

				#endregion
			}
			else
			{
				AssertEquals("No messages linked to CusEntryHeader", 0, entry.Messages.Count);

				#region Notes on Failure:

				AssertEquals("Total Notes if processing failed.", 1, interchange.Notes.DatabaseCount);
				var note = (StmNote)interchange.Notes.GetAllNotes().ToList()[0];
				AssertEquals("Note description if processing failed.", "Data Import Log Text", note.ST_Description);
				AssertEquals("Note text if processing failed.", scenario.Failure_Message, note.ST_NoteText);

				#endregion
			}
		}

		protected void AssertTextContainsDespiteBlanksAndBreaks(string msg, ZString expected, ZString actual)
		{
			var expectedNoBlanks = Regex.Replace(expected, @"\s", string.Empty);
			var actualNoBlanks = Regex.Replace(actual, @"\s", string.Empty);
			var actualNoBlanksOrBreaks = Regex.Replace(actualNoBlanks, "<br/>", string.Empty);
			AssertContains(msg, expectedNoBlanks, actualNoBlanksOrBreaks);
		}

		static string ExtractDataFromEmailAttachment(MailAttachment attachment)
		{
			byte[] arr = new byte[4096];
			var stream = attachment.GetMA_DataReader();
			int iTotal = stream.Read(arr, 0, 4096);
			byte[] arr2 = new byte[iTotal];
			Array.Copy(arr, 0, arr2, 0, iTotal);
			return Encoding.ASCII.GetString(arr2);
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_Empty_Body()
		{
			var dec = CreateJobDeclaration();

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec, AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_EMPTY_BODY)
			{
				MessageNo = "51"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_No_Declaration_Reference()
		{
			var dec = CreateJobDeclaration();

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec, AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_NO_DEC_REF)
			{
				MessageNo = "53"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_Declaration_Not_Found()
		{
			var dec = CreateJobDeclaration();

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec, AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_DEC_NOT_FOUND)
			{
				MessageNo = "54"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_Success_with_CusAgent()
		{
			var dec = CreateJobDeclaration();
			dec.JE_GS_NKCusAgent = cusAgent.GS_Code;

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec)
			{
				User_ToReceiveEmail = cusAgent,
				MessageNo = "55"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_Success_without_CusAgent()
		{
			var dec = CreateJobDeclaration();

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec)
			{
				User_ToReceiveEmail = userWhoCreatesDeclarations,
				MessageNo = "56"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		public AsycudaIncomingGmdMessageTestScenario CreateTestScenario_Success_No_Identation_Body()
		{
			var dec = CreateJobDeclaration();

			var scenario = new AsycudaIncomingGmdMessageTestScenario(dec, AsycudaIncomingGmdMessageTestScenario.SUCCESS_TYPE_NO_IDENTATION_BODY)
			{
				User_ToReceiveEmail = userWhoCreatesDeclarations,
				MessageNo = "57"
			};

			CreateEdiInterchange(scenario);
			return scenario;
		}

		void CreateEdiInterchange(AsycudaIncomingGmdMessageTestScenario scenario)
		{
			string xmlGeneric = null;

			if (scenario.Type == null)
			{
				var xmlAsycuda = Create_Asycuda_XMLDocument(scenario.Declaration_Reference);
				scenario.Expected_AsycudaMessageText = xmlAsycuda;
				xmlGeneric = xmlAsycuda;
			}
			else if (scenario.Type == AsycudaIncomingGmdMessageTestScenario.SUCCESS_TYPE_NO_IDENTATION_BODY)
			{
				var xmlAsycuda = $"<ASYCUDA><Declarant><Declarant_name>DEMO COMPANY</Declarant_name><Reference><Number>{scenario.Declaration_Reference}</Number></Reference></Declarant></ASYCUDA>";
				scenario.Expected_AsycudaMessageText = $@"<ASYCUDA>
  <Declarant>
    <Declarant_name>DEMO COMPANY</Declarant_name>
    <Reference>
      <Number>{scenario.Declaration_Reference}</Number>
    </Reference>
  </Declarant>
</ASYCUDA>";
				xmlGeneric = xmlAsycuda;
			}
			else if (scenario.Type == AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_EMPTY_BODY)
			{
				xmlGeneric = "";
			}
			else if (scenario.Type == AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_NO_DEC_REF)
			{
				var xmlAsycuda = "<Invalid><Asycuda><Message>__Dummy_Data_Contains_No_Declaration_Reference__</Message></Asycuda></Invalid>";
				xmlGeneric = xmlAsycuda;
			}
			else if (scenario.Type == AsycudaIncomingGmdMessageTestScenario.FAILURE_TYPE_DEC_NOT_FOUND)
			{
				var xmlAsycuda = "<ASYCUDA><Declarant><Reference><Number>An_Invalid_Declaration_Reference</Number></Reference></Declarant></ASYCUDA>";
				xmlGeneric = xmlAsycuda;
			}

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "";
			interchange.EI_BodyText = xmlGeneric;           // EI_BodyText should contain only the ASYCUDA or ESAD xml
			interchange.EI_ApplicationCode = "GMD";         // ("Generic Message Delivery")
			interchange.EI_InterchangeType = "ZZA";         // ("ZZ" for all countries, "A" for Asycuda.)
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_InterchangeNum = scenario.MessageNo;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_SystemCreateTimeUtc = ZDate.Today.AddDays(-2);

			scenario.InterchangePK = interchange.PK;
		}

		string Create_Asycuda_XMLDocument(ZString declarationReference)
		{
			var path = @"Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing.Asycuda_Incoming_GMD_Messages.XML_Template_Asycuda.xml";

			using (var inStream = typeof(AsycudaIncomingGMDMessageProcessorTest).Assembly.GetManifestResourceStream(path))
			{
				var text = new StreamReader(inStream).ReadToEnd();
				text = text.Replace("~~DeclarantReference~~", declarationReference);
				return text;
			}
		}

		JobDeclaration CreateJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();

			dec.JE_MessageType = "EXP";
			dec.JE_MessageSubType = "EX";
			dec.JE_SystemCreateUser = userWhoCreatesDeclarations.GS_Code;
			dec.PopulateJE_DeclarationReferenceIfNeeded();

			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = dec.JE_DeclarationReference + "/1";
			Factory.Save();
			return dec;
		}

		public void SaveAll()
		{
			Factory.Save();
		}

		public void CreateUserAccounts()
		{
			userWhoCreatesDeclarations = Factory.New<GlbStaff>();
			userWhoCreatesDeclarations.GS_Code = "DEC";
			userWhoCreatesDeclarations.GS_LoginName = "Declaration User";
			userWhoCreatesDeclarations.GS_EmailAddress = "declarations@acme.com";

			cusAgent = Factory.New<GlbStaff>();
			cusAgent.GS_Code = "CUS";
			cusAgent.GS_LoginName = "CusAgent User";
			cusAgent.GS_EmailAddress = "cusagent@acme.com";

			wrongUser = Factory.New<GlbStaff>();
			wrongUser.GS_Code = "NOT";
			wrongUser.GS_LoginName = "Wrong User";
			wrongUser.GS_EmailAddress = "wrong@acme.com";
		}

		GlbStaff userWhoCreatesDeclarations;
		GlbStaff cusAgent;
		GlbStaff wrongUser;
	}
}
