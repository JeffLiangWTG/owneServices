using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class GLJournalFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertCSVWithMultipleSubAccounts()
		{
			var valueObject = new Xsd.GLJournal();
			var notifications = new NotificationBuffer();
			var converter = new GLJournalFlatFileConverter(notifications, Factory);
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\GJLJournalWithMultipleSubAccounts.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(4, valueObject.JournalLines.Count);

			//CSV Line was imported in sequence. Hence we fetch line and sub account with collection index directly.
			AssertEquals("2010.00.00", valueObject.JournalLines[0].Account.ToString());
			AssertEquals(2, valueObject.JournalLines[0].SubAccounts.Count);
			var importedSubAccount1 = valueObject.JournalLines[0].SubAccounts[0];
			AssertEquals("ABIGAS", importedSubAccount1.Code);
			AssertEquals(Constants.SubAccountType.Organization, importedSubAccount1.Type.Code);
			var importedSubAccount2 = valueObject.JournalLines[0].SubAccounts[1];
			AssertEquals("TST", importedSubAccount2.Code);
			AssertEquals(Constants.SubAccountType.StaffAndResources, importedSubAccount2.Type.Code);

			AssertEquals("2020.00.00", valueObject.JournalLines[1].Account.ToString());
			AssertEquals(1, valueObject.JournalLines[1].SubAccounts.Count);
			var importedSubAccount3 = valueObject.JournalLines[1].SubAccounts[0];
			Assert(string.IsNullOrEmpty(importedSubAccount3.Code));
			AssertEquals(Constants.SubAccountType.SalesGroup, importedSubAccount3.Type.Code);

			AssertEquals("2020.10.00", valueObject.JournalLines[2].Account.ToString());
			AssertEquals(1, valueObject.JournalLines[2].SubAccounts.Count);
			var importedSubAccount4 = valueObject.JournalLines[2].SubAccounts[0];
			Assert(string.IsNullOrEmpty(importedSubAccount4.Code));
			AssertEquals(Constants.SubAccountType.StaffGroup, importedSubAccount4.Type.Code);

			AssertEquals("2020.20.00", valueObject.JournalLines[3].Account.ToString());
			AssertEquals(0, valueObject.JournalLines[3].SubAccounts.Count);

			AssertEquals(false, notifications.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertCSVWithInvalidSubAccount()
		{
			var valueObject = new Xsd.GLJournal();
			var notifications = new NotificationBuffer();
			var converter = new GLJournalFlatFileConverter(notifications, Factory);
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\GJLJournalWithInvalidSubAccount.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(2, valueObject.JournalLines.Count);
			AssertEquals(0, valueObject.JournalLines[0].SubAccounts.Count);
			AssertEquals(0, valueObject.JournalLines[1].SubAccounts.Count);

			AssertEquals(true, notifications.HasErrors);
			AssertEquals("2 sub accounts does not have relative line", FormattableString.Invariant($@"Error: {GLJournalFlatFileConverter.InvalidLineSubAccountErrorMessage}
Error: {GLJournalFlatFileConverter.InvalidLineSubAccountErrorMessage}"), notifications.AsString.Trim());
		}
	}
}
