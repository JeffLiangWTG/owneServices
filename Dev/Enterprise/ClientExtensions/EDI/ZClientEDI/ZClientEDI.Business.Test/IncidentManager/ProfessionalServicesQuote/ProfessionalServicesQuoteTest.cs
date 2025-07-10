using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.EDI.IncidentManager.Testing;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProfessionalServicesQuote))]
	class ProfessionalServicesQuoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetLocalClientFromSettlementGroupOfBranchOrg()
		{
			OrgHeader clientWithoutSettlementGroup = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader clientWithSettlementGroup = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementGroup = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			clientWithSettlementGroup.ARSettlementGroupPK = settlementGroup.PK;
			Job job = new Job.Loader(BizObj).TryCreate();

			BizObj.IM_OA_BranchAddress = clientWithoutSettlementGroup.MainAddress.PK;
			AssertEquals(ZGuid.Empty, job.JH_OA_LocalChargesAddr);

			BizObj.IM_OA_BranchAddress = clientWithSettlementGroup.MainAddress.PK;
			AssertEquals(settlementGroup, job.LocalChargesAddr.Header);
		}

		public void TestIM_IncidentType()
		{
			AssertEquals("IM_IncidentType", "PSQ", BizObj.IM_IncidentType);
		}

		public void TestIM_ProgramArea()
		{
			AssertEquals("IM_ProgramArea", IncidentConstants.ProgramArea.General, BizObj.IM_ProgramArea);
		}

		public void TestGetNewValidation()
		{
			AssertEquals("Validation.GetType()", typeof(ProfessionalServicesQuoteValidation), BizObj.Validation.GetType());
		}

		public void TestGetNewLookups()
		{
			AssertEquals("Lookups.GetType()", typeof(ProfessionalServicesQuoteLookups), BizObj.Lookups.GetType());
		}

		public void TestIsAssignedStaffChangedNotificationEnabled()
		{
			PropertyInfo propertyInfo = typeof(ProfessionalServicesQuote).GetProperty("IsAssignedStaffChangedNotificationEnabled", BindingFlags.NonPublic | BindingFlags.Instance);

			EnableAssignedStaffChangedEmailNotification(false);
			AssertEquals("IsAssignedStaffChangedNotificationEnabled", false, propertyInfo.GetValue(BizObj, null));

			EnableAssignedStaffChangedEmailNotification(true);
			AssertEquals("IsAssignedStaffChangedNotificationEnabled", true, propertyInfo.GetValue(BizObj, null));
		}

		public void TestIM_IncidentNumber()
		{
			long currentIncidentNumber = Modules.ClientNumberFountainRegistration.GetInstance().ProfessionalServicesQuoteNo.PeekPreliminary(Factory);
			AssertEquals("Precondition: IM_IncidentNumber should be empty.", "", BizObj.IM_IncidentNumber);

			BizObj.FillWithValidTestData();
			Factory.Save();

			AssertEquals("IM_IncidentNumber", "PSQ" + currentIncidentNumber.ToString().PadLeft(8, '0'), BizObj.IM_IncidentNumber);
		}

		public void TestIM_GS_CurrentlyAssignedTo_Defaulted()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "nondeveloper";
			staff.GS_IsDeveloper = false;

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("nondeveloper"))
			{
				ProfessionalServicesQuote bizObj = (ProfessionalServicesQuote)Factory.New(ExpectedBusinessObjectType);

				Assert("Precondition: CurrentUser should not be a developer.", !Env.CurrentUser.IsDeveloper);
				AssertEquals("IM_GS_NKCustServiceContact should be empty if the current user is not a developer.", ZString.Empty,
					bizObj.IM_GS_NKAssignedToCurrent);
			}

			ProfessionalServicesQuote newBizObj = (ProfessionalServicesQuote)Factory.New(ExpectedBusinessObjectType);

			Assert("Precondition: CurrentUser should be a developer.", Env.CurrentUser.IsDeveloper);
			AssertEquals("IM_GS_NKCustServiceContact should default to the current user if it is a developer account.",
									 Factory.Load<GlbStaff>(Env.CurrentUser.PK).GS_Code,
									 newBizObj.IM_GS_NKAssignedToCurrent);
		}

		public void TestSetTeamFromLastQuoteAddedByCurrentUser()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();

			BizObj.IM_GG_Team = group1.PK;
			Factory.Save();

			BizObj.IM_GG_Team = group2.PK;
			Factory.Save();

			ProfessionalServicesQuote newBizObj = (ProfessionalServicesQuote)Factory.New(ExpectedBusinessObjectType);
			newBizObj.SetTeamFromLastQuoteAddedByCurrentUser();

			AssertEquals("IM_GG_Team should default to the team of the last quote user created.", group2.PK, newBizObj.IM_GG_Team);
		}

		#region Outlook Mail Item

		[ExpectExceptionMessage(typeof(ApplicationException), "Your user account currently has no Email Address specified. Please edit your account and specify an Email Address before sending an Email.")]
		public void TestGetNewOutlookMailItemThrowsExceptionWhenEmailAddressIsEmpty()
		{
			BizObj.FillWithValidTestData();
			BizObj.IM_Status = IncidentConstants.IncidentStatus.Assigned;
			BizObj.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "";
			BizObj.GetNewOutlookMailItem();
		}

		public void TestGetNewOutlookMailItem()
		{
			var mock = Factory.NewMoq<ProfessionalServicesQuote>();
			mock.Protected()
				.Setup<Interop.OutlookIntegration.IOutlookMailItem>("NewOutlookMailItem")
				.Returns(new DummyOutlookMailItem());

			var bizObj = mock.Object;

			InsertDataForMailItemRecipients(bizObj);

			string originalName = GlbStaff.CurrentUser.GS_FullName;
			string originalTitle = GlbStaff.CurrentUser.GS_Title;
			string originalEmail = GlbStaff.CurrentUser.GS_EmailAddress;

			try
			{
				GlbCompany.CurrentCompany.GC_Phone = "+61 2 8001 2200";
				GlbCompany.CurrentCompany.GC_Fax = "+61 2 9025 1199";
				GlbStaff.CurrentUser.GS_FullName = "Current Guy";
				GlbStaff.CurrentUser.GS_Title = "Some Title";
				GlbStaff.CurrentUser.GS_EmailAddress = "currentguy@edi.com.au";

				bizObj.FillWithValidTestData();
				bizObj.IM_Description = "Some Description";
				bizObj.IM_Status = IncidentConstants.IncidentStatus.Assigned;
				bizObj.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

				Factory.Save();

				var mailItem = (DummyOutlookMailItem)bizObj.GetNewOutlookMailItem();
				mock.VerifyAll();

				AssertNotNull("MailItem should not be null.", mailItem);
				AssertMailItemRecipientsAreCorrect(mailItem);
				AssertEquals("MailItem.Subject", "Quotation #" + bizObj.IM_IncidentNumber + " (Some Description)", mailItem.Subject);

				string expectedBody =
					"Dear Bow Wow," + System.Environment.NewLine + System.Environment.NewLine +
					"<Put detailed description here>" + System.Environment.NewLine + System.Environment.NewLine +
					"Best regards," + System.Environment.NewLine + System.Environment.NewLine +
					"Current Guy" + System.Environment.NewLine +
					"Some Title" + System.Environment.NewLine + System.Environment.NewLine +
					"CargoWise Pty Ltd" + System.Environment.NewLine +
					"Tel: +61 2 8001 2200   Fax: +61 2 9025 1199" + System.Environment.NewLine +
					"Email: currentguy@edi.com.au   Web: www.cargowise.com";

				AssertEquals("MailItem.Body", expectedBody, mailItem.Body);
				AssertEquals("MailItem.ReplyRecipient", "currentguy@edi.com.au", mailItem.ReplyRecipient);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_FullName = originalName;
				GlbStaff.CurrentUser.GS_Title = originalTitle;
				GlbStaff.CurrentUser.GS_EmailAddress = originalEmail;
			}
		}

		void InsertDataForMailItemRecipients(ProfessionalServicesQuote bizObj)
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "BOW WOW";
			contact.OC_Email = "bow@wow.com";
			contact.OC_Salutation = "Dear Bow Wow";
			bizObj.IM_OC_Contact = contact.PK;
		}

		void AssertMailItemRecipientsAreCorrect(DummyOutlookMailItem mailItem)
		{
			AssertEquals("There should only be 1 recipient.", 1, mailItem.Recipients.Length);
			AssertEquals("MailItem.Recipients[0]", "bow@wow.com", mailItem.Recipients[0]);
		}

		public void TestGetEmailInEmlFormat()
		{
			ProfessionalServicesQuote bizObj = Factory.New<ProfessionalServicesQuote>();

			InsertDataForMailItemRecipients(bizObj);

			string originalName = GlbStaff.CurrentUser.GS_FullName;
			string originalTitle = GlbStaff.CurrentUser.GS_Title;
			string originalEmail = GlbStaff.CurrentUser.GS_EmailAddress;

			try
			{
				GlbCompany.CurrentCompany.GC_Phone = "+61 2 8001 2200";
				GlbCompany.CurrentCompany.GC_Fax = "+61 2 9025 1199";
				GlbStaff.CurrentUser.GS_FullName = "Current Guy";
				GlbStaff.CurrentUser.GS_Title = "Some Title";
				GlbStaff.CurrentUser.GS_EmailAddress = "currentguy@edi.com.au";

				bizObj.FillWithValidTestData();
				bizObj.IM_Description = "Some Description";
				bizObj.IM_Status = IncidentConstants.IncidentStatus.Assigned;
				bizObj.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

				Factory.Save();

				var email = bizObj.GetEmailInEmlFormat();

				string expectedHeader =
@"MIME-Version: 1.0
Content-type: text/html; Charset=utf-8
To: <bow@wow.com>
X-Unsent: 1
Subject: Quotation #PSQ00000001 (Some Description)";

				string expectedBody =
					"<Body style='font-family:Calibri, Arial;'>"
					+ "Dear Bow Wow,<br /><br />"
					+ "&lt;Put detailed description here&gt;<br /><br />"
					+ "Best regards,<br /><br />Current Guy<br />"
					+ "Some Title<br /><br />"
					+ "CargoWise Pty Ltd<br />"
					+ "Tel: +61 2 8001 2200   Fax: +61 2 9025 1199<br />"
					+ "Email: currentguy@edi.com.au   Web: www.cargowise.com"
					+ "</Body>";

				string expectedEml = expectedHeader + System.Environment.NewLine + System.Environment.NewLine + expectedBody;

				AssertEquals(expectedEml, bizObj.GetEmailInEmlFormat());
			}
			finally
			{
				GlbStaff.CurrentUser.GS_FullName = originalName;
				GlbStaff.CurrentUser.GS_Title = originalTitle;
				GlbStaff.CurrentUser.GS_EmailAddress = originalEmail;
			}
		}

		#endregion

		public void TestReadOnlyForClosedQuote()
		{
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote.CloseIncident();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ProfessionalServicesQuote reloadedQuote = newFactory.Load<ProfessionalServicesQuote>(quote.PK);
			AssertEquals(false, reloadedQuote.ReadOnly);
		}

		public void TestRelatedWorkItems()
		{
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();

			NewWorkItem workItem1 = Factory.New<NewWorkItem>();
			NewWorkItem workItem2 = Factory.New<NewWorkItem>();
			quote.RelatedWorkItems.Add(workItem1);
			quote.RelatedWorkItems.Add(workItem2);

			AssertEquals(2, quote.RelatedWorkItems.Count);
			AssertCollectionContains(workItem1, quote.RelatedWorkItems);
			AssertCollectionContains(workItem2, quote.RelatedWorkItems);
		}

		public void TestIWorkItemRelatedItemImplementation()
		{
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			quote.IM_IncidentNumber = "Z123";
			quote.IM_Description = "Test work ok";
			quote.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			quote.IM_Status = ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WaitForOrder;

			var item = (IWorkItemRelatedItem)quote;

			AssertEquals("Quote", item.Type);
			AssertEquals("Z123", item.Number);
			AssertEquals("Test work ok", item.ItemDescription);
			AssertEquals("W_O - Wait for Order", item.StatusDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, item.AssignedStaffCode);
			AssertEquals(ClientControllerRegistration.ProfessionalServicesQuote, item.ControllerID);
			AssertEquals(quote.IM_Priority, item.Criticality);
			AssertEquals(quote.IM_PriorityInfo, item.CriticalityInfo);

			item.OnRelatedWorkItemReOpened(null);
			AssertEquals(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, quote.IM_Status);
		}

		public void TestBillingNumber()
		{
			BizObj.IM_IncidentNumber = "123456";
			AssertEquals("123456", ((IJobInvoicingPlugIn)BizObj).JobNumber);
		}

		public void TestSettingBranchAddressSetsCorrectARSettlementGroupAddressToJob()
		{
			OrgHeader mainOrg = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("mainOrg.MainAddress", mainOrg.MainAddress);
			OrgAddress arAddress = mainOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("org.MainAddress", org.MainAddress);

			org.ARSettlementGroupPK = mainOrg.PK;
			Job.Loader loader = new Job.Loader(BizObj);
			Job job = loader.Load();
			AssertNull("Precondition: Job", job);
			BizObj.IM_OA_BranchAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNull("Setting IM_OA_BranchAddress does not create Job", job);

			loader.TryCreate();
			BizObj.IM_OA_BranchAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNotNull("Job", job);
			AssertEquals("Should be AR adress of the Settlement Group", arAddress.PK, job.JH_OA_LocalChargesAddr);
		}

		public void TestConsumerType()
		{
			AssertEquals(EDIJobInvoicingConsumerTypes.PSQuote.Code, ((IJobInvoicingPlugIn)BizObj).InvoicingSupporter.ConsumerType.Code);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", EDISecurityCheckpoints.ProfessionalServicesQuoteAuditBilling, ((IJobInvoicingPlugIn)BizObj).InvoicingSupporter.AuditSecurity);
		}

		public void TestIM_GS_CustomerServiceContact()
		{
			AssertEquals("IM_GS_NKCustServiceContact", GlbStaff.CurrentUser.GS_Code, BizObj.IM_GS_NKCustServiceContact);
		}

		public void TestIM_Product()
		{
			AssertEquals("IM_Product", EnterpriseModuleList.Codes.ClientProjects, BizObj.IM_Product);
		}

		public void TestIM_ChargableWork()
		{
			AssertEquals("IM_ChargableWork", true, BizObj.IM_ChargableWork);
		}

		public void TestIM_Status()
		{
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote, BizObj.IM_Status);
		}

		public void TestValidation()
		{
			AssertEquals("Validation should contain BranchAddressAndContactValidation.", true, BizObj.Validation.ContainsPiggybackedValidation(typeof(BranchAddressAndContactValidation)));
		}

		public void TestReopenIncident()
		{
			Assert("Precondition: IM_Status should not be WorkInProgress.", BizObj.IM_Status != ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress);

			BizObj.ReopenIncident();
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, BizObj.IM_Status);
		}

		public void TestSettingClientUpdatesQuoteCurrency()
		{
			RefCurrency currency1 = Factory.New<RefCurrency>();
			RefCurrency currency2 = Factory.New<RefCurrency>();

			OrgHeader client1 = Factory.New<OrgHeader>();
			OrgHeader client2 = Factory.New<OrgHeader>();

			OrgMiscServ miscServ1 = Factory.New<OrgMiscServ>();
			OrgMiscServ miscServ2 = Factory.New<OrgMiscServ>();

			currency1.RX_Code = "TC1";
			currency2.RX_Code = "TC2";

			miscServ1.OM_OH = client1.PK;
			miscServ2.OM_OH = client2.PK;

			miscServ1.Header.CompanyData.OB_RX_NKARDDefltCurrency = currency1.RX_Code;
			miscServ2.Header.CompanyData.OB_RX_NKARDDefltCurrency = currency2.RX_Code;

			BizObj.IM_OH_Client = client1.PK;
			AssertEquals("IM_RX_NKQuoteCurrency", currency1.RX_Code, BizObj.IM_RX_NKQuoteCurrency);

			BizObj.IM_OH_Client = client2.PK;
			AssertEquals("IM_RX_NKQuoteCurrency", currency2.RX_Code, BizObj.IM_RX_NKQuoteCurrency);

			BizObj.IM_OH_Client = ZGuid.Invalid;
			AssertEquals("IM_RX_NKQuoteCurrency", "", BizObj.IM_RX_NKQuoteCurrency);
		}

		public void TestSettingQuoteAmountSetsDepositAmount()
		{
			BizObj.IM_DepositAmountRequired = 0;
			BizObj.IM_QuoteAmount = 123.45m;

			AssertEquals("IM_QuoteAmount", 123.45m, BizObj.IM_QuoteAmount);
			AssertEquals("IM_DepositAmountRequired", 123.45m, BizObj.IM_DepositAmountRequired);

			BizObj.IM_QuoteAmount = 987.65m;

			AssertEquals("IM_QuoteAmount", 987.65m, BizObj.IM_QuoteAmount);
			AssertEquals("IM_DepositAmountRequired", 987.65m, BizObj.IM_DepositAmountRequired);

			BizObj.IM_DepositAmountRequired = 234.56m;

			AssertEquals("IM_QuoteAmount", 987.65m, BizObj.IM_QuoteAmount);
			AssertEquals("IM_DepositAmountRequired", 234.56m, BizObj.IM_DepositAmountRequired);

			BizObj.IM_QuoteAmount = 987.65m;

			AssertEquals("IM_QuoteAmount", 987.65m, BizObj.IM_QuoteAmount);
			AssertEquals("IM_DepositAmountRequired", 234.56m, BizObj.IM_DepositAmountRequired);
		}

		public void TestNoteTypes()
		{
			System.Collections.IList noteTypes = BizObj.NoteTypes;

			AssertEquals("NoteTypes.Count", 1, noteTypes.Count);
			AssertEquals("NoteTypes should contain InvoiceDetails.", true, noteTypes.Contains(PredefinedNoteTypes.Instance.InvoiceDetails));
		}

		public void TestNotes()
		{
			AssertEquals("Notes.GetType()", typeof(IncidentNotes), BizObj.Notes.GetType());
		}

		public void TestInvoiceDetails()
		{
			BizObj.FillWithValidTestData();
			Factory.Save();

			AssertEquals("Precondition: InvoiceDetails should be empty.", "", BizObj.InvoiceDetails);
			AssertEquals("Precondition: HasChanges should be false.", false, BizObj.HasChanges);
			AssertEquals("Precondition: There shouldn't be any Notes yet.", 0, BizObj.Notes.GetAllNotes().Count);

			BizObj.InvoiceDetails = "Some Details.";
			AssertEquals("Some Details.", BizObj.InvoiceDetails);
			AssertEquals("HasChanges", true, BizObj.HasChanges);

			StmNoteCollection notes = (StmNoteCollection)BizObj.Notes.GetAllNotes();

			AssertEquals("1 Note should have been created.", 1, notes.Count);
			AssertEquals("Note.ST_Description", PredefinedNoteTypes.Instance.InvoiceDetails.Description, notes[0].ST_Description);
			AssertEquals("Note.ST_NoteText", "Some Details.", notes[0].ST_NoteText);
			AssertEquals("Note.ST_NoteType", StmNoteDescription.Pub, notes[0].ST_NoteType);

			Factory.Save();

			BizObj.InvoiceDetails = "New Details.";
			AssertEquals("New Details.", BizObj.InvoiceDetails);
			AssertEquals("HasChanges", true, BizObj.HasChanges);

			notes = (StmNoteCollection)BizObj.Notes.GetAllNotes();

			AssertEquals("The previously created Note should be used.", 1, notes.Count);
			AssertEquals("Note.ST_Description", PredefinedNoteTypes.Instance.InvoiceDetails.Description, notes[0].ST_Description);
			AssertEquals("Note.ST_NoteText", "New Details.", notes[0].ST_NoteText);
			AssertEquals("Note.ST_NoteType", StmNoteDescription.Pub, notes[0].ST_NoteType);

			BizObj.InvoiceDetails = "";
			notes = (StmNoteCollection)BizObj.Notes.GetAllNotes();
			AssertEquals("The Note should have been deleted.", 0, notes.Count);
		}

		public void TestRelatedIncidentNumbersCommaDelimited()
		{
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			NewWorkItem relatedItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			relatedItem1.WKI_WorkItemNumber = "WI12345678";
			quote.RelatedWorkItems.Add(relatedItem1);
			AssertEquals("RelatedItems.Count", 1, quote.RelatedWorkItems.Count);
			AssertEquals("RelatedClientCode", "WI12345678", quote.RelatedIncidentNumbersCommaDelimited);

			NewWorkItem relatedItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			relatedItem2.WKI_WorkItemNumber = "WI87654321";
			quote.RelatedWorkItems.Add(relatedItem2);
			AssertEquals("RelatedItems.Count", 2, quote.RelatedWorkItems.Count);
			AssertEquals("RelatedClientCode", "WI12345678, WI87654321", quote.RelatedIncidentNumbersCommaDelimited);
		}

		public void TestPopulateWorkItem()
		{
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			NewWorkItem workItem = Factory.New<NewWorkItem>();

			quote.IM_Product = "P01";
			quote.IM_Description = "DESCR001";
			quote.IM_WorkItemType = "ENH";
			quote.IM_ProgramArea = "UDF";
			ZDateTime requiredBy = ZDateTime.Now;
			quote.IM_RequiredBy = requiredBy;
			quote.IM_EstimatedHours = 5;
			quote.PopulateWorkItem(workItem);

			AssertEquals(ProductTypes.Codes.Enterprise, workItem.WKI_WorkItemType);
			AssertEquals("P01", workItem.WKI_ActivityType);
			AssertEquals("DESCR001", workItem.WKI_Summary);
			AssertEquals(NewWorkItemLookups.WorkItemTypeConstants.EnhanceCoPayment, workItem.WKI_ActivitySubtype);
			AssertEquals(ReleaseRings.Codes.ALP, workItem.WKI_Priority);
			AssertEquals("Refer to related Professional Service Quotations for description information.", workItem.WKI_Details.ToUTF8());
			AssertEquals((ZDateTime)TimeSpan.FromHours(5),
				new ZDateTime(workItem.WorkflowItems[0].P9_EstDuration.Year, workItem.WorkflowItems[0].P9_EstDuration.Month, workItem.WorkflowItems[0].P9_EstDuration.Day, workItem.WorkflowItems[0].P9_EstDuration.Hour, 0, 0));

			quote.IM_WorkItemType = "CTM";
			quote.IM_ProgramArea = "XXX";
			quote.PopulateWorkItem(workItem);

			AssertEquals(NewWorkItemLookups.WorkItemTypeConstants.EnhanceClientSpecific, workItem.WKI_ActivitySubtype);
			AssertEquals(ReleaseRings.Codes.DPR, workItem.WKI_Priority);

			quote.IM_WorkItemType = "CTW";
			quote.PopulateWorkItem(workItem);
			AssertEquals(ReleaseRings.Codes.DPR, workItem.WKI_Priority);

			quote.IM_WorkItemType = "XXX";
			quote.PopulateWorkItem(workItem);
			AssertEquals(NewWorkItemLookups.PatchToConstants.None, workItem.WKI_Priority);

			quote.IM_WorkItemType = "GRA";
			quote.PopulateWorkItem(workItem);
			AssertEquals(quote.IM_Details, workItem.WKI_Details);
		}

		public void TestRelatedItems()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			psq.RelatedItems.Add(workItem);
			project.RelatedItems.Add(psq);
			featureRequest.RelatedItems.Add(psq);

			Factory.Save();

			AssertEquals(3, psq.RelatedItems.Count);
			Assert(psq.RelatedItems.Contains(workItem));
			Assert(psq.RelatedItems.Contains(project));
			Assert(psq.RelatedItems.Contains(featureRequest));
		}

		public void TestIsClosedOrCancelled()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			Assert(!psq.IsClosed);
			Assert(!((IWorkTaskRelatedItem)psq).IsClosedOrCancelled);

			psq.CloseIncident();
			Assert(psq.IsClosed);
			Assert(((IWorkTaskRelatedItem)psq).IsClosedOrCancelled);
		}

		public void TestFilteredRelatedItems()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			EdiHelpErrorLog issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();

			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.RelatedItems.Add(incident);
			psq.RelatedItems.Add(issue);
			psq.RelatedItems.Add(workitem);
			psq.RelatedItems.Add(project);

			AssertEquals(4, psq.FilteredRelatedItems.Count);
			AssertCollectionContains(incident, psq.FilteredRelatedItems);
			AssertCollectionContains(issue, psq.FilteredRelatedItems);
			AssertCollectionContains(workitem, psq.FilteredRelatedItems);
			AssertCollectionContains(project, psq.FilteredRelatedItems);

			psq.ShowOnlyNonClosedItems = true;
			AssertEquals(2, psq.FilteredRelatedItems.Count);
			AssertCollectionNotContains(incident, psq.FilteredRelatedItems);
			AssertCollectionContains(issue, psq.FilteredRelatedItems);
			AssertCollectionNotContains(workitem, psq.FilteredRelatedItems);
			AssertCollectionContains(project, psq.FilteredRelatedItems);
		}

		public void TestIWorkTaskTreeNodeMembers()
		{
			var psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			psq.RelatedItems.AddNew(typeof(SupportIncident));
			psq.RelatedItems.AddNew(typeof(SupportIncident));

			AssertEquals(ZDateTime.Empty, psq.AgreedDeliveryDate);
			AssertEquals(ZString.Empty, psq.CurrentTaskStatus);
			AssertEquals(ZString.Empty, psq.CurrentTaskDescription);
			AssertEquals(ZString.Empty, psq.CurrentTaskCapabilityCodeDescription);
			AssertEquals(ZString.Empty, psq.CurrentTaskAssigned);
			AssertNull(psq.ParentsOnlyRelatedItems);
			AssertNotNull(psq.ChildrenOnlyRelatedItems);
			AssertEquals(2, psq.ChildrenOnlyRelatedItems.Count);
		}

		#region PSQ Opportunity Pivot

		public void TestRelatedOpportunities()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIOrgOpportunity opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();

			psq.RelatedOpportunities.Add(opportunity1);
			AssertEquals(1, psq.RelatedOpportunities.Count);
			Factory.Save();

			AssertEquals(false, psq.HasChanges);

			psq.RelatedOpportunities.Add(opportunity2);
			AssertEquals(2, psq.RelatedOpportunities.Count);
			AssertEquals(true, psq.HasChanges);

			Factory.Save();
			AssertEquals(false, psq.HasChanges);

			psq.RelatedOpportunities.Remove(opportunity1);
			AssertEquals(1, psq.RelatedOpportunities.Count);
			AssertEquals(true, psq.HasChanges);
		}

		public void TestCreateNewPSQOpportunityPivot()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIOrgOpportunity opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();

			psq.CreateNewPSQOpportunityPivot(opportunity1);
			psq.CreateNewPSQOpportunityPivot(opportunity2);

			Factory.Save();

			AssertEquals("Should have 2 related opportunities", 2, psq.RelatedOpportunities.Count);
			Assert("Should have opportunity 1", psq.RelatedOpportunities.Contains(opportunity1));
			Assert("Should have opportunity 2", psq.RelatedOpportunities.Contains(opportunity2));
		}

		public void TestLoadPSQOpportunityPivot()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			GenPivot pivot1 = psq.CreateNewPSQOpportunityPivot(opportunity);

			Factory.Save();

			GenPivot pivot2 = psq.LoadPSQOpportunityPivot(opportunity);

			AssertEquals(pivot1.XX_Relation1ID, pivot2.XX_Relation1ID);
			AssertEquals(pivot1.XX_Relation2ID, pivot2.XX_Relation2ID);
			AssertEquals(pivot1.XX_Relation1TableCode, pivot2.XX_Relation1TableCode);
			AssertEquals(pivot1.XX_Relation2TableCode, pivot2.XX_Relation2TableCode);
		}

		public void TestDeletePSQOpportunityPivot()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();

			GenPivot pivot1 = psq.CreateNewPSQOpportunityPivot(opportunity);

			Factory.Save();

			GenPivot pivot2 = psq.LoadPSQOpportunityPivot(opportunity);
			AssertNotNull(pivot2);

			psq.RemovePSQOpportunityPivot(opportunity);
			GenPivot pivot3 = psq.LoadPSQOpportunityPivot(opportunity);
			AssertEquals(null, pivot3);
		}

		public void TestRemoveAllPSQOpportunityPivots()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIOrgOpportunity opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();

			GenPivot pivot = psq.CreateNewPSQOpportunityPivot(opportunity1);
			GenPivot pivot2 = psq.CreateNewPSQOpportunityPivot(opportunity2);

			Factory.Save();

			psq.Delete();

			Assert("Pivot 1 should be deleted", pivot.IsDeleted);
			Assert("Pivot 2 should be deleted", pivot2.IsDeleted);
		}

		public void TestAddandRemoveRelatedOpportunities()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();

			psq.RelatedOpportunities.Add(opportunity1);
			Factory.Save();

			GenPivot pivot1 = psq.LoadPSQOpportunityPivot(opportunity1);
			AssertNotNull(pivot1);

			AssertEquals("Should have 1 related opportunity", 1, psq.RelatedOpportunities.Count);
			Assert("Should have opportunity 1", psq.RelatedOpportunities.Contains(opportunity1));

			psq.RelatedOpportunities.Remove(opportunity1);
			Factory.Save();

			GenPivot pivot2 = psq.LoadPSQOpportunityPivot(opportunity1);
			AssertEquals(null, pivot2);

			AssertEquals("Should have no related opportunity", 0, psq.RelatedOpportunities.Count);
			AssertEquals("Should not have opportunity 1", false, psq.RelatedOpportunities.Contains(opportunity1));
		}

		public void TestAddandRemoveOpportunityWithoutSaving()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			psq.RelatedOpportunities.Add(opportunity1);

			GenPivot pivot1 = psq.LoadPSQOpportunityPivot(opportunity1);
			AssertNotNull(pivot1);

			psq.RelatedOpportunities.Remove(opportunity1);
			GenPivot pivot2 = psq.LoadPSQOpportunityPivot(opportunity1);
			AssertEquals(null, pivot2);
		}

		public void TestHasChangesIsFalseWhenLoaded()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();

			psq.RelatedOpportunities.Add(opportunity);
			AssertEquals(1, psq.RelatedOpportunities.Count);
			Factory.Save();

			AssertEquals(false, psq.HasChanges);

			psq.RelatedOpportunities.Load();
			AssertEquals(false, psq.HasChanges);
		}

		public void TestInitializeFromOpportunity()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();

			opportunity.P8_OpportunityDescription = "Description";
			opportunity.P8_OH = client.PK;
			opportunity.P8_EstimatedValue = 1000;
			opportunity.P8_OC = contact.PK;

			psq.InitializeFromOpportunity(opportunity);

			AssertEquals(opportunity.Client, psq.Client);
			AssertEquals(opportunity.P8_OpportunityDescription, psq.IM_Description);
			AssertEquals(opportunity.Contact, psq.Contact);
			AssertEquals(opportunity.P8_EstimatedValue, psq.IM_QuoteAmount);
		}

		#endregion

		#region File -> Validate All does not load children of related opportunities

		public void TestValidateAllDoesNotLoadRelatedOpportunities()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIOrgOpportunity opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			psq.RelatedOpportunities.Add(opportunity1);

			AssertEquals("Precondition: PSQ has related opportunity", 1, psq.RelatedOpportunities.Count);
			Factory.Save();

			bool registeredEditableChild = psq.IsRegisteredEditableChildObject(psq.RelatedOpportunities);
			psq.RelatedOpportunities.Add(opportunity2);
			AssertEquals("Precondition: PSQ has related opportunities", 2, psq.RelatedOpportunities.Count);
			registeredEditableChild |= psq.IsRegisteredEditableChildObject(psq.RelatedOpportunities);

			string message = "Failure of this test means that a stack overflow exception will happen whenever a user does a File -> Validate All" +
				" on a form of a Professional Services Quote that fits the conditions specified in the test.";

			AssertEquals(message, false, registeredEditableChild);
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = BizObj;
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IARInvoiceSavingNotificationSubscriber

		public void TestGetRecipientsForNotification()
		{
			GlbGroup incidentGroupManager = GetGroupWithStaffWithEmailAndTitle("MYGROUP", "manager@test.com", "MGR");
			EDIDataRegistry.Instance.IncidentCustomisationGroupENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, incidentGroupManager.PK.ToGuid());
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@test.com";
			BizObj.IM_GS_NKCustServiceContact = staff.GS_Code;
			GlbGroup assignedToGroup = GetGroupWithStaffWithEmailAndTitle("PSQ", "captn@test.com", "MGR");
			BizObj.IM_GG_Team = assignedToGroup.PK;
			Factory.Save();

			IARInvoiceSavingNotificationSubscriber subscriber = BizObj;

			AssertEquals("Three recipients: ", 3, subscriber.GetRecipientsForNotification().Length);
			AssertEquals("Should contain contact email address: ", "test@test.com", subscriber.GetRecipientsForNotification()[0]);
			AssertEquals("Should contain group manager email address: ", "manager@test.com", subscriber.GetRecipientsForNotification()[1]);
			AssertEquals("Should contain assigned group manager email addresss: ", "captn@test.com", subscriber.GetRecipientsForNotification()[2]);

			incidentGroupManager = GetGroupWithStaffWithEmailAndTitle("Dummy1", "test@test.com", "MGR");
			EDIDataRegistry.Instance.IncidentCustomisationGroupENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, incidentGroupManager.PK.ToGuid());
			assignedToGroup = GetGroupWithStaffWithEmailAndTitle("Dummy2", "test@test.com", "MGR");
			BizObj.IM_GG_Team = assignedToGroup.PK;

			Factory.Save();
			AssertEquals("Should have only one recipient: ", 1, subscriber.GetRecipientsForNotification().Length);
			AssertEquals("Should contain contact email address: ", "test@test.com", subscriber.GetRecipientsForNotification()[0]);
		}

		GlbGroup GetGroupWithStaffWithEmailAndTitle(ZString groupCode, ZString staffEmail, ZString staffTitle)
		{
			GlbGroup incidentGroupManager = Factory.NewWithValidTestData<GlbGroup>();
			incidentGroupManager.GG_Code = groupCode;
			incidentGroupManager.GG_Desc = "Some Group iN the WOrld";
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Maaaah baah haaah Lola";
			staff.GS_EmailAddress = staffEmail;

			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = incidentGroupManager.PK;
			link.GK_GS = staff.PK;
			link.GK_MembershipType = staffTitle;
			return incidentGroupManager;
		}

		#endregion

		#region Implementation

		void EnableAssignedStaffChangedEmailNotification(bool enabled)
		{
			EDIDataRegistry.Instance.EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
		}

		ProfessionalServicesQuote BizObj
		{
			get { return fBizObj ?? (fBizObj = Factory.New<ProfessionalServicesQuote>()); }
		}

		ProfessionalServicesQuote fBizObj;

		#endregion
	}

	#region Task Provider Test

	[TestedType(typeof(ProfessionalServicesQuote))]
	class ProfessionalServicesQuoteWorkflowProviderTest : WorkflowProviderTest<ProfessionalServicesQuote, PSQuoteProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria_ForIM_WorkItemType()
		{
			PSQuote.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(PSQuote.IM_WorkItemTypeInfo, ProcessTaskTemplate.P0_SubType1Info, "CTM", "ENH", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIM_ProgramArea()
		{
			PSQuote.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(PSQuote.IM_ProgramAreaInfo, ProcessTaskTemplate.P0_SubType2Info, IncidentConstants.ProgramArea.DataFix, IncidentConstants.ProgramArea.General, ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIM_Product()
		{
			PSQuote.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(PSQuote.IM_ProductInfo, ProcessTaskTemplate.P0_SubType3Info, AutoEnterpriseModuleList.Codes.AirCargoAutomation, AutoEnterpriseModuleList.Codes.ClientProjects, ZString.Empty);
		}

		ProfessionalServicesQuote PSQuote
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote.Code; }
		}
	}

	#endregion

	#region RelatableActivity Test

	[TestedType(typeof(ProfessionalServicesQuote))]
	class ProfessionalServicesQuoteRelatableActivityTest : RelatableActivityTestCase<ProfessionalServicesQuote>
	{
		protected override ProfessionalServicesQuote GetNewActivity()
		{
			return Factory.NewWithValidTestData<ProfessionalServicesQuote>();
		}
	}

	#endregion

	[TestedType(typeof(ProfessionalServicesQuote))]
	sealed class ProfessionalServicesQuoteRelatedItemTest : IncidentMainBaseRelatedItemTest
	{
		protected override string ExpectedSelectionCriterion3 => "GRA - Graphics";

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var quote = (IncidentMainBase)base.GetItemForSelectionCriteriaTest();

			quote.IM_Module = "GRA";

			return (IWorkTaskRelatedItem)quote;
		}
	}

	[TestedType(typeof(ProfessionalServicesQuote))]
	sealed class ProfessionalServicesQuoteRelatedItemSourceTest : IncidentMainBaseRelatedItemSourceTest
	{
	}
}
