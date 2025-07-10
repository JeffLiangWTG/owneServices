using System;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class ErrorType : NotificationSubscriberType
	{
		protected ErrorType(string name, string message) : base(name, message)
		{
		}

		protected ErrorType(string message) : this(message, message)
		{
		}

		// general
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static ErrorType Warning { get { return new ErrorType("Warning", Res.GetString("bd425ad0-369b-4d02-b488-9b5ffacebb1e", "Validation Warning")); } }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static ErrorType Error { get { return new ErrorType("Error", Res.GetString("2b1cb99f-439c-4702-90df-dbcd1ebda5a0", "Validation Error")); } }
		public static ErrorType RequiredFieldEmpty { get { return new ErrorType("RequiredFieldEmpty", Res.GetString("0c33da0e-ffef-4fb0-a0ab-3c7436429d72", "Required field empty")); } }
		public static ErrorType InvalidFileFormat { get { return new ErrorType("InvalidFileFormat", Res.GetString("deaea269-08c5-4ce4-9661-dd548ea2e494", "Invalid file format")); } }
		public static ErrorType MissingMasterRecord { get { return new ErrorType("MissingMasterRecord", Res.GetString("3df1adc0-e7be-40b8-84bc-3340866a5ce7", "Could not find master record")); } }
		public static ErrorType RecordAlreadyExists { get { return new ErrorType("RecordAlreadyExists", Res.GetString("68620779-9fd3-4bc1-8bde-a7808ad70989", "Existing record already exists")); } }
		public static ErrorType MissingHeader { get { return new ErrorType("MissingHeader", Res.GetString("d1724160-ec5a-4d7d-8ae1-0a0bebd5f8dd", "Missing header record")); } }
		public static ErrorType MissingTrailer { get { return new ErrorType("MissingTrailer", Res.GetString("368d65e8-59e6-495b-8647-ffba5c2747fd", "Missing trailer record")); } }
		public static ErrorType UnknownRecordType { get { return new ErrorType("UnknownRecordType", Res.GetString("f2e82bd7-688d-4c14-a25f-03f8cec122a6", "Unknown record type")); } }
		public static ErrorType TotalRecordCountMismatch { get { return new ErrorType("TotalRecordCountMismatch", Res.GetString("5b79ed10-da2b-41b7-b7b8-4a5c3e515ab7", "Total number of records inconsistent with trailer")); } }
		public static ErrorType NotEnoughColumns { get { return new ErrorType("NotEnoughColumns", Res.GetString("cb82160a-b8f0-4786-b644-5a360f927f94", "Column count mismatch")); } }
		public static ErrorType PostToDatabaseError { get { return new ErrorType("PostToDatabaseError", Res.GetString("faea9129-6b4c-4dd5-b9b8-2ed7ad5e2f46", "Database or file write error")); } }
		public static ErrorType DataTypeConversionError { get { return new ErrorType("DataTypeConversionError", Res.GetString("ff68600c-310b-4647-a944-de9f328a7764", "Data type conversion error")); } }
		public static ErrorType ValueOverflowError { get { return new ErrorType("ValueOverflowError", Res.GetString("727784d5-8a11-4280-88c9-d922aed68212", "Value overflow error")); } }
		public static ErrorType MissingDataDirectory { get { return new ErrorType("MissingDataDirectory", Res.GetString("0dcebc98-2788-45f7-b21b-d52ed2fc812f", "Could not find directory")); } }
		public static ErrorType IOError { get { return new ErrorType("IOError", Res.GetString("a10d2544-ddd0-4c09-9d64-426086f49e77", "File system I/O error")); } }

		// email issues
		public static ErrorType EmailNotifyGroupNotExist { get { return new ErrorType("EmailNotifyGroupNotExist", Res.GetString("713ca8e1-5356-47a1-a9c3-0f7ecec9041f", "You must set up the Email Notification Group in order for errors to be sent")); } }
		public static ErrorType ErrorSendingEmail { get { return new ErrorType("ErrorSendingEmail", Res.GetString("c951c0e4-26d7-45ac-902e-78d252cef419", "No members of the Orders Notification Group exist")); } }
		public static ErrorType ErrorRemovingEmails { get { return new ErrorType("ErrorRemovingEmails", Res.GetString("a6765955-5857-448e-9443-47d13455184a", "Error removing email(s) from database after processing")); } }
		public static ErrorType UnknownCode { get { return new ErrorType("UnknownCode", Res.GetString("baebdfda-b2c2-4b22-8033-c84e8abe8acb", "Unknown code")); } }

		// matching NK to PK
		public static ErrorType MissingPKFromNK { get { return new ErrorType("MissingPKFromNK", Res.GetString("1bdb9428-6ac5-4ae5-a63e-f51a9fe4a51e", "Could not find record from Natural Key")); } }
		public static ErrorType MoreThan1NKMatch { get { return new ErrorType("MoreThan1NKMatch", Res.GetString("3feb1631-6f0d-4b1a-ab9a-98b0caeeebd9", "Duplicate Natural Key matches found")); } }
		public static ErrorType OrgMatchedFuzzy { get { return new ErrorType("OrgMatchedFuzzy", Res.GetString("9a102eef-ab00-4b58-b342-4a921eccd82b", "Organization matched fuzzy-like, i.e. things like LTD/PTY removed and/or punctuation removed to match")); } }
		public static ErrorType UnmatchOrgNotSet { get { return new ErrorType("UnmatchOrgNotSet", Res.GetString("5db4af32-95eb-4766-a247-255bbc1c94e9", "Unmatched Data Items Account not set up. You must set this value in the system registry for this process to function.")); } }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static ErrorType OrganisationUnmatchedAndTempNotCreated { get { return new ErrorType("Organisation Unmatched And Temporary Not Created", Res.GetString("301feac5-43ce-494c-8cc9-9db0aee04553", "Organization unmatched and temporary could not be created due to lack of information")); } }
		//ignore this comment btw this is not related at all to this document so how ever finds this lone coment please have a sence of humour

		// xml
		public static ErrorType XmlSchemaValidation { get { return new ErrorType("XmlSchemaValidation", Res.GetString("a57dd2c9-5507-4dcc-87f2-57e1f44bf8f8", "XML Schema Validation Failed")); } }
		public static ErrorType DataErrorPreventSave { get { return new ErrorType("DataErrorPreventSave", Res.GetString("dd43ab9e-8d4a-49cf-a13e-9d0a1ae7365f", "Data Error Prevent Save")); } }
		public static ErrorType DataOutOfRangeError { get { return new ErrorType("DataOutOfRangeError", Res.GetString("e9e13bc3-765d-4319-9b6a-271267c41706", "Data out of range error")); } }

		// any business logic error in Data Import
		public static ErrorType ImportingDataError { get { return new ErrorType("ImportingDataError", Res.GetString("8b8813b5-71e5-4fa1-9835-64d4a6718fe7", "ERROR MESSAGE:")); } }

		public override string GetDisplayMessage(string additionalInfo)
		{
			bool showErrorType = (this != Error && this != XmlSchemaValidation && this != DataErrorPreventSave);
			return Res.GetString("20db72d6-de10-48f0-8660-04b650704588", "Error: {0}", base.GetDisplayMessage(additionalInfo, showErrorType));
		}
	}
}
