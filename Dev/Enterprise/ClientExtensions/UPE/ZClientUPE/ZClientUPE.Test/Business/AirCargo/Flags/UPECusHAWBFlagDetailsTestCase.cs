using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBFlagDetails))]
	internal abstract class UPECusHAWBFlagDetailsTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestAuthorisationReceivedBy()
		{
			FlagDetails.AuthorisationReceivedBy = AuthReceivedByCodeDescriptionPairList.Codes.Fax;
			AssertEquals(AuthReceivedByCodeDescriptionPairList.Codes.Fax, FlagDetails.AuthorisationReceivedBy);
			AssertEquals(0, FlagDetails.AuthorisationReceivedByInfo.GetErrors().Count());
			FlagDetails.AuthorisationReceivedBy = "";
			AssertEquals(1, FlagDetails.AuthorisationReceivedByInfo.GetErrors().Count());
			AssertMandatoryValidationError(FlagDetails.AuthorisationReceivedByInfo, true);
			FlagDetails.AuthorisationReceivedBy = "MEH";
			AssertEquals(1, FlagDetails.AuthorisationReceivedByInfo.GetErrors().Count());
			AssertMandatoryValidationError(FlagDetails.AuthorisationReceivedByInfo, false);
			AssertListValidationInvalidCodeError(FlagDetails.AuthorisationReceivedByInfo, true);
		}

		public void TestPersonAuthorised()
		{
			FlagDetails.PersonAuthorised = "JC";
			AssertEquals("JC", FlagDetails.PersonAuthorised);
			AssertEquals(0, FlagDetails.PersonAuthorisedInfo.GetErrors().Count());
			FlagDetails.PersonAuthorised = "";
			AssertEquals(1, FlagDetails.PersonAuthorisedInfo.GetErrors().Count());
			AssertMandatoryValidationError(FlagDetails.PersonAuthorisedInfo, true);
			FlagDetails.PersonAuthorised = "MEH";
			AssertEquals(0, FlagDetails.PersonAuthorisedInfo.GetErrors().Count());
			AssertMandatoryValidationError(FlagDetails.PersonAuthorisedInfo, false);
		}

		public void TestRemarks()
		{
			AssertEquals("", FlagDetails.Remarks);
			FlagDetails.Remarks = "MEH MEH";
			AssertEquals("MEH MEH", FlagDetails.Remarks);
		}

		public void TestLookups()
		{
			AssertNotNull(FlagDetails.Lookups);
		}

		public void TestCreateNote()
		{
			AssertEquals("Pre-condition", 0, UPECusHAWB.Notes.GetAllNotes().Count);
			FlagDetails.AuthorisationReceivedBy = AuthReceivedByCodeDescriptionPairList.Codes.Email;
			FlagDetails.PersonAuthorised = "EDI";
			FlagDetails.Remarks = "This is a looooooooooooooooooooooooooooong remarks";
			FlagDetails.CreateNote();
			StmNoteCollection notes = (StmNoteCollection)UPECusHAWB.Notes.GetAllNotes();
			AssertEquals("Note should be created", 1, notes.Count);
			AssertEquals(true, notes[0].ST_IsCustomDescription);
			AssertEquals(ExpectedNoteDescription, notes[0].ST_Description);
			AssertMultilineEquals("Check the NoteRefence", @"Authorisation Received By: Email
Person Authorised: EDI
Remarks: This is a looooooooooooooooooooooooooooong remarks

" + ExpectedNoteReference, ORtfTextUtil.RtfToText(notes[0].ST_NoteData), '\n');
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override sealed BusinessObject GetNewBusinessObject()
		{
			return GetNewUPECusHAWBFlagDetails(UPECusHAWB);
		}

		protected abstract string ExpectedNoteDescription { get; }

		protected abstract string ExpectedNoteReference { get; }

		protected abstract UPECusHAWBFlagDetails GetNewUPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB);
		protected void AssertMandatoryValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(propertyInfo, isExpectingError);
		}

		protected void AssertListValidationInvalidCodeError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(propertyInfo, isExpectingError);
		}

		protected UPECusHAWBFlagDetails FlagDetails
		{
			get
			{
				if (fFlagDetails == null)
				{
					fFlagDetails = GetNewUPECusHAWBFlagDetails(UPECusHAWB);
				}

				return fFlagDetails;
			}
		}

		protected UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					fUPECusHAWB = Factory.New<UPECusHAWB>();
				}

				return fUPECusHAWB;
			}
		}

		UPECusHAWBFlagDetails fFlagDetails;
		UPECusHAWB fUPECusHAWB;
		#endregion
	}
}
