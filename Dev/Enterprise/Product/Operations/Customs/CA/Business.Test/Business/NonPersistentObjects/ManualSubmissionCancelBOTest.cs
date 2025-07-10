using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using IManualSubmissionSupport = Enterprise.Integration.Customs.CA.IManualSubmissionSupport;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ManualSubmissionBO))]
	public class ManualSubmissionCancelBOTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2017, 04, 05)]
		public void TestProperties()
		{
			var declaration = GetTestDeclaration();

			AssertNotNull(declaration.B3EntryHeader);
			AssertNotNull(declaration.ReleaseEntryHeader);
			AssertNull("Manual Submission Note is null", declaration.ManualSubmissionNote);
		}

		[TestDate(2017, 04, 05)]
		public void TestUpdateManualSubmissionNote_B3CEntry()
		{
			const string officeCode = "!234";
			PrepareTestCustomOffice(officeCode, Factory);

			var declaration = GetTestDeclaration();
			var manualSubmissionSupport = declaration as IManualSubmissionSupport;
			var manualSubmissionBo = new ManualSubmissionBO(declaration, MessageTypeList.Codes.B3CUSDEC, Factory);

			var entrySubmissionBo = manualSubmissionBo.CurrentEntrySubmissionBO;
			entrySubmissionBo.ManualSubmissionDate = new ZDate(2017, 04, 05);
			entrySubmissionBo.PortOfClearanceOverride = officeCode;
			manualSubmissionSupport.ManualSubmission(manualSubmissionBo.GetCusEntrySubmissionBOs());

			AssertNotNull("Manual Submission Note is not null", declaration.ManualSubmissionNote);

			var noteText = declaration.ManualSubmissionNote.ST_NoteText;
			var relSubmittedDate = entrySubmissionBo.ManualSubmissionDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			Assert("Manual Submission NoteText not empty", !noteText.IsEmpty);
			Assert("Manual Submission B3C MessageType", noteText.Contains(entrySubmissionBo.MessageType));
			Assert("Manual Submission B3C ManualSubmissionDate", noteText.Contains(relSubmittedDate));
			Assert("Manual Submission B3C PortOfClearanceOverride", noteText.Contains(entrySubmissionBo.PortOfClearanceOverride));
			Assert("Manual Submission LoginName", noteText.Contains(Env.CurrentUser.LoginName));

			Assert("Manual Submission contains no REL MessageType", !noteText.Contains(MessageTypeList.Codes.EDIRelease));
		}

		[TestDate(2017, 04, 05)]
		public void TestUpdateManualSubmissionNote_BRELCEntry()
		{
			const string officeCode = "!234";
			PrepareTestCustomOffice(officeCode, Factory);

			var declaration = GetTestDeclaration();
			var manualSubmissionSupport = declaration as IManualSubmissionSupport;
			var manualSubmissionBo = new ManualSubmissionBO(declaration, MessageTypeList.Codes.EDIRelease, Factory);

			var entrySubmissionBo = manualSubmissionBo.CurrentEntrySubmissionBO;
			entrySubmissionBo.ManualSubmissionDate = new ZDate(2017, 04, 05);
			entrySubmissionBo.PortOfClearanceOverride = officeCode;
			manualSubmissionSupport.ManualSubmission(manualSubmissionBo.GetCusEntrySubmissionBOs());

			AssertNotNull("Manual Submission Note is not null", declaration.ManualSubmissionNote);

			var noteText = declaration.ManualSubmissionNote.ST_NoteText;
			var relSubmittedDate = entrySubmissionBo.ManualSubmissionDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			Assert("Manual Submission NoteText not empty", !noteText.IsEmpty);
			Assert("Manual Submission REL MessageType", noteText.Contains(entrySubmissionBo.MessageType));
			Assert("Manual Submission REL ManualSubmissionDate", noteText.Contains(relSubmittedDate));
			Assert("Manual Submission REL PortOfClearanceOverride", noteText.Contains(entrySubmissionBo.PortOfClearanceOverride));
			Assert("Manual Submission LoginName", noteText.Contains(Env.CurrentUser.LoginName));

			Assert("Manual Submission contains no B3C MessageType", !noteText.Contains(MessageTypeList.Codes.B3CUSDEC));
		}

		#region Helpers

		public static void PrepareTestCustomOffice(string officeCode, BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, officeCode, officeCode, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}

		JobDeclaration GetTestDeclaration()
		{
			if (jobDeclaration == null)
			{
				jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var testB3CHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
				testB3CHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

				var testRELHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
				testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			}

			return jobDeclaration;
		}
		JobDeclaration jobDeclaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ManualSubmissionBO(GetTestDeclaration(), MessageTypeList.Codes.EDIRelease, Factory);
		}

		#endregion
	}
}
