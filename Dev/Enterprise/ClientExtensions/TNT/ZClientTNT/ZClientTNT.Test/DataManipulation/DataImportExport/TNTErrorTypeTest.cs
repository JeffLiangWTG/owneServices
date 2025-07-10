using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT
{
	[TestedType(typeof(TNTErrorType))]
	class TNTErrorTypeTest : NotificationSubscriberTypeTest<TNTErrorType>
	{
		public void TestTNTErrorType()
		{
			AssertNotNull(TNTErrorType.InvalidFileName);
			string message = String.Format("Invalid file name.  File name should be in the format <Branch>.<File Type>.<Date>.<Time>{0}", QuantumFile.Extension);
			AssertEquals(message, TNTErrorType.InvalidFileName.Name);
			AssertEquals(message, TNTErrorType.InvalidFileName.Message);
			AssertNotNull(TNTErrorType.InvalidRecordDelimiter);
			message = @"Invalid Record Delimiter.  Record Delimiter must contain a dot (""."").";
			AssertEquals(message, TNTErrorType.InvalidRecordDelimiter.Name);
			AssertEquals(message, TNTErrorType.InvalidRecordDelimiter.Message);
		}

		protected override TNTErrorType NewNotificationType(string message)
		{
			return new TNTErrorType(message);
		}

		protected override TNTErrorType NewNotificationType(string name, string message)
		{
			return new TNTErrorType(message);
		}
	}
}
