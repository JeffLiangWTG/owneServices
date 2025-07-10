using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class PrintTypeNameTest : TestCase
	{
		public void TestJobTypeDisplayNameNotEmpty()
		{
			AssertEquals("Expected user-friendly name", "Email", PrintTypeName.Name("EML"));
			AssertEquals("Expected user-friendly name", "Print", PrintTypeName.Name("PRN"));
			AssertEquals("Expected user-friendly name", "Print sent to remote printer - success", PrintTypeName.Name("PRS"));
			AssertEquals("Expected user-friendly name", "Document Delivery Success", PrintTypeName.Name("DDS"));
			AssertEquals("Expected user-friendly name", "Fax", PrintTypeName.Name("FAX"));
			AssertEquals("Expected user-friendly name", "Fax awaiting acknowledgement", PrintTypeName.Name("FAA"));
			AssertEquals("Expected user-friendly name", "Upload to FTP folder", PrintTypeName.Name("FTP"));

			Assertion.AssertExceptionThrown(typeof(ArgumentException), delegate
			{ PrintTypeName.Name("ZZZ"); });
		}
	}
}
