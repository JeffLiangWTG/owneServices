namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ErrorTypeTest : NotificationSubscriberTypeTest<ErrorType>
	{
		public void TestStaticErrorTypes()
		{
			TestStaticErrorTypesCore();
		}

		protected virtual void TestStaticErrorTypesCore()
		{
			AssertEquals("Error", ErrorType.Error.Name);
			AssertEquals("Validation Error", ErrorType.Error.Message);

			AssertEquals("RequiredFieldEmpty", ErrorType.RequiredFieldEmpty.Name);
			AssertEquals("Required field empty", ErrorType.RequiredFieldEmpty.Message);

			AssertEquals("InvalidFileFormat", ErrorType.InvalidFileFormat.Name);
			AssertEquals("Invalid file format", ErrorType.InvalidFileFormat.Message);

			AssertEquals("MissingMasterRecord", ErrorType.MissingMasterRecord.Name);
			AssertEquals("Could not find master record", ErrorType.MissingMasterRecord.Message);

			AssertEquals("RecordAlreadyExists", ErrorType.RecordAlreadyExists.Name);
			AssertEquals("Existing record already exists", ErrorType.RecordAlreadyExists.Message);

			AssertEquals("MissingHeader", ErrorType.MissingHeader.Name);
			AssertEquals("Missing header record", ErrorType.MissingHeader.Message);

			AssertEquals("MissingTrailer", ErrorType.MissingTrailer.Name);
			AssertEquals("Missing trailer record", ErrorType.MissingTrailer.Message);

			AssertEquals("UnknownRecordType", ErrorType.UnknownRecordType.Name);
			AssertEquals("Unknown record type", ErrorType.UnknownRecordType.Message);

			AssertEquals("TotalRecordCountMismatch", ErrorType.TotalRecordCountMismatch.Name);
			AssertEquals("Total number of records inconsistent with trailer", ErrorType.TotalRecordCountMismatch.Message);

			AssertEquals("NotEnoughColumns", ErrorType.NotEnoughColumns.Name);
			AssertEquals("Column count mismatch", ErrorType.NotEnoughColumns.Message);

			AssertEquals("PostToDatabaseError", ErrorType.PostToDatabaseError.Name);
			AssertEquals("Database or file write error", ErrorType.PostToDatabaseError.Message);

			AssertEquals("DataTypeConversionError", ErrorType.DataTypeConversionError.Name);
			AssertEquals("Data type conversion error", ErrorType.DataTypeConversionError.Message);

			AssertEquals("ValueOverflowError", ErrorType.ValueOverflowError.Name);
			AssertEquals("Value overflow error", ErrorType.ValueOverflowError.Message);

			AssertEquals("MissingDataDirectory", ErrorType.MissingDataDirectory.Name);
			AssertEquals("Could not find directory", ErrorType.MissingDataDirectory.Message);

			AssertEquals("IOError", ErrorType.IOError.Name);
			AssertEquals("File system I/O error", ErrorType.IOError.Message);

			AssertEquals("EmailNotifyGroupNotExist", ErrorType.EmailNotifyGroupNotExist.Name);
			AssertEquals("You must set up the Email Notification Group in order for errors to be sent", ErrorType.EmailNotifyGroupNotExist.Message);

			AssertEquals("ErrorSendingEmail", ErrorType.ErrorSendingEmail.Name);
			AssertEquals("No members of the Orders Notification Group exist", ErrorType.ErrorSendingEmail.Message);

			AssertEquals("ErrorRemovingEmails", ErrorType.ErrorRemovingEmails.Name);
			AssertEquals("Error removing email(s) from database after processing", ErrorType.ErrorRemovingEmails.Message);

			AssertEquals("UnknownCode", ErrorType.UnknownCode.Name);
			AssertEquals("Unknown code", ErrorType.UnknownCode.Message);

			AssertEquals("MissingPKFromNK", ErrorType.MissingPKFromNK.Name);
			AssertEquals("Could not find record from Natural Key", ErrorType.MissingPKFromNK.Message);

			AssertEquals("MoreThan1NKMatch", ErrorType.MoreThan1NKMatch.Name);
			AssertEquals("Duplicate Natural Key matches found", ErrorType.MoreThan1NKMatch.Message);

			AssertEquals("OrgMatchedFuzzy", ErrorType.OrgMatchedFuzzy.Name);
			AssertEquals("Organization matched fuzzy-like, i.e. things like LTD/PTY removed and/or punctuation removed to match", ErrorType.OrgMatchedFuzzy.Message);

			AssertEquals("UnmatchOrgNotSet", ErrorType.UnmatchOrgNotSet.Name);
			AssertEquals("Unmatched Data Items Account not set up. You must set this value in the system registry for this process to function.", ErrorType.UnmatchOrgNotSet.Message);

			AssertEquals("XmlSchemaValidation", ErrorType.XmlSchemaValidation.Name);
			AssertEquals("XML Schema Validation Failed", ErrorType.XmlSchemaValidation.Message);

			AssertEquals("ImportingDataError", ErrorType.ImportingDataError.Name);
			AssertEquals("ERROR MESSAGE:", ErrorType.ImportingDataError.Message);

			AssertEquals("DataErrorPreventSave", ErrorType.DataErrorPreventSave.Name);
			AssertEquals("Data Error Prevent Save", ErrorType.DataErrorPreventSave.Message);
		}

		public void TestGetDisplayMessage()
		{
			AssertEquals("Error: Required field empty (AdditionalInfo)", ErrorType.RequiredFieldEmpty.GetDisplayMessage("AdditionalInfo"));
			AssertEquals("Error: AdditionalInfo", ErrorType.Error.GetDisplayMessage("AdditionalInfo"));
			AssertEquals("Error: AdditionalInfo", ErrorType.XmlSchemaValidation.GetDisplayMessage("AdditionalInfo"));
		}

		#region Implementation

		protected override ErrorType NewNotificationType(string name)
		{
			return new ErrorTypeForTest(name);
		}

		protected override ErrorType NewNotificationType(string name, string message)
		{
			return new ErrorTypeForTest(name, message);
		}

		#endregion
	}
}
