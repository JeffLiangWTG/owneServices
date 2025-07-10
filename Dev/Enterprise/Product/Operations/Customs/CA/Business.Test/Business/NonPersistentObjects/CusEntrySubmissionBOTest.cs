using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusEntrySubmissionBO))]
	sealed class CusEntrySubmissionBOTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2017, 04, 10)]
		public void TestValidateSubmissionDate_RELCusEntry()
		{
			RELCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Now;
			AssertNoNotifications(RELCusEntrySubmissionBO.ManualSubmissionDateInfo);

			RELCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Invalid;
			AssertHasNotifications(RELCusEntrySubmissionBO.ManualSubmissionDateInfo);

			RELCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Empty;
			AssertHasErrorContaining(RELCusEntrySubmissionBO.ManualSubmissionDateInfo, "Please enter a value.");
		}

		[TestDate(2017, 04, 10)]
		public void TestValidateSubmissionDate_B3CCusEntry()
		{
			B3CCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Now;
			AssertNoNotifications(B3CCusEntrySubmissionBO.ManualSubmissionDateInfo);

			B3CCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Invalid;
			AssertHasNotifications(B3CCusEntrySubmissionBO.ManualSubmissionDateInfo);

			RELCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Empty;
			AssertHasErrorContaining(RELCusEntrySubmissionBO.ManualSubmissionDateInfo, "Please enter a value.");
		}

		public void TestValidatePortOfClerance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "1111", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");
			Factory.Save();

			RELCusEntrySubmissionBO.PortOfClearanceOverride = "2222";
			AssertHasErrorContaining(RELCusEntrySubmissionBO.PortOfClearanceOverrideInfo, "Enter a valid selection.");

			RELCusEntrySubmissionBO.PortOfClearanceOverride = ZString.Empty;
			AssertNoNotifications(RELCusEntrySubmissionBO.PortOfClearanceOverrideInfo);

			RELCusEntrySubmissionBO.PortOfClearanceOverride = "1111";
			AssertNoNotifications(RELCusEntrySubmissionBO.PortOfClearanceOverrideInfo);
		}

		public void TestBuildNoteText()
		{
			const string officeCode = "!234";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, officeCode, officeCode, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var noteText = RELCusEntrySubmissionBO.NoteText;
			Assert("Manual Submission NoteText empty", noteText.IsEmpty);

			RELCusEntrySubmissionBO.ManualSubmissionDate = ZDateTime.Now;
			RELCusEntrySubmissionBO.PortOfClearanceOverride = officeCode;

			noteText = RELCusEntrySubmissionBO.NoteText;
			var submittedDate = RELCusEntrySubmissionBO.ManualSubmissionDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			Assert("Manual Submission NoteText not empty", !noteText.IsEmpty);
			Assert("Manual Submission MessageType", noteText.Contains(RELCusEntrySubmissionBO.MessageType));
			Assert("Manual Submission ManualSubmissionDate", noteText.Contains(submittedDate));
			Assert("Manual Submission PortOfClearanceOverride", noteText.Contains(RELCusEntrySubmissionBO.PortOfClearanceOverride));
			Assert("Manual Submission LoginName", noteText.Contains(Env.CurrentUser.LoginName));
		}

		#region Helpers

		CusEntrySubmissionBO RELCusEntrySubmissionBO
		{
			get
			{
				if (releaseCusEntrySubmissionBO == null)
				{
					releaseCusEntrySubmissionBO = new CusEntrySubmissionBO(MessageTypeList.Codes.EDIRelease, Array.Empty<ZString>(), Factory);
				}

				return releaseCusEntrySubmissionBO;
			}
		}
		CusEntrySubmissionBO releaseCusEntrySubmissionBO;

		CusEntrySubmissionBO B3CCusEntrySubmissionBO
		{
			get
			{
				if (b3cCusEntrySubmissionBO == null)
				{
					b3cCusEntrySubmissionBO = new CusEntrySubmissionBO(MessageTypeList.Codes.B3CUSDEC, Array.Empty<ZString>(), Factory);
				}

				return b3cCusEntrySubmissionBO;
			}
		}
		CusEntrySubmissionBO b3cCusEntrySubmissionBO;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusEntrySubmissionBO(MessageTypeList.Codes.EDIRelease, Array.Empty<ZString>(), Factory);
		}

		#endregion
	}
}
