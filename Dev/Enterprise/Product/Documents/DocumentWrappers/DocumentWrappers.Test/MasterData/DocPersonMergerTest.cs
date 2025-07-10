using System;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterData.Testing
{
	[TestedType(typeof(DocPersonMergeEmailSender))]
	sealed class DocPersonMergerTest : DocumentWrapperTestCase
	{
		public void TestPersonName()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Alex";
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "TRex";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson }));
			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should get name from retained person", retainedPerson.PER_FullName, docWrapper.PersonName);
		}

		public void TestMergedAccounts()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertContains("Should return retained contacts grouped by email", FormattableString.Invariant($"<b>other@email.com<b><br>{contact1.OrgCode} - {contact1.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			AssertNotContains("Should not dissolved person contacts until they're successfully dissolved", FormattableString.Invariant($"<b>another@one.com<b><br>{contact2.OrgCode} - {contact2.WorkingAddressCompanyName}<br>{contact3.OrgCode} - {contact3.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			Assert("Should remove trailing <br>", !docWrapper.MergedAccounts.EndsWith("<br>"));

			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			AssertContains("Should return retained contacts", FormattableString.Invariant($"<b>other@email.com<b><br>{contact1.OrgCode} - {contact1.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			AssertContains("Should include dissolvedPerson1's contacts now", FormattableString.Invariant($"<b>another@one.com<b><br>{contact2.OrgCode} - {contact2.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			Assert("Should remove trailing <br>", !docWrapper.MergedAccounts.EndsWith("<br>"));

			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			AssertContains("Should return retained contacts", FormattableString.Invariant($"<b>other@email.com<b><br>{contact1.OrgCode} - {contact1.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			AssertContains("Should include dissolvedPerson2's contacts now. Grouped by email", FormattableString.Invariant($"<b>another@one.com<b><br>{contact2.OrgCode} - {contact2.WorkingAddressCompanyName}<br>{contact3.OrgCode} - {contact3.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			AssertContains("Should include dissolvedPerson2's contacts now. Grouped by email", FormattableString.Invariant($"<b>yetanother@one.com<b><br>{contact4.OrgCode} - {contact4.WorkingAddressCompanyName}"), docWrapper.MergedAccounts);
			Assert("Should remove trailing <br>", !docWrapper.MergedAccounts.EndsWith("<br>"));
		}

		public void TestMergedAccountsTable()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			var retainedContactsString = FormattableString.Invariant($"<tr><td><b>other@email.com</b></td><td>{contact1.OrgCode}</td><td>{contact1.WorkingAddressCompanyName}</td></tr>");

			var mergedAccountsTable = docWrapper.MergedAccountsTable;
			AssertStartsWith("Should start with table tags", "<table cellspacing=\"20\"><tr><td><b>", mergedAccountsTable);
			AssertContains("Should return retained contacts grouped by email", retainedContactsString, mergedAccountsTable);
			AssertNotContains("Should not dissolved person contacts until they're successfully dissolved", FormattableString.Invariant($"<tr><td><b>another@one.com</b></td><td>{contact2.OrgCode}</td><td>{contact2.WorkingAddressCompanyName}</td></tr><tr><td></td><td>{contact3.OrgCode}</td><td>{contact3.WorkingAddressCompanyName}</td></tr>"), mergedAccountsTable);
			AssertEndsWith("Should include table", "</td></tr></table>", mergedAccountsTable);

			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			mergedAccountsTable = docWrapper.MergedAccountsTable;
			AssertStartsWith("Should start with table tags", "<table cellspacing=\"20\"><tr><td><b>", mergedAccountsTable);
			AssertContains("Should return retained contacts", retainedContactsString, mergedAccountsTable);
			AssertContains("Should include dissolvedPerson1's contacts now", FormattableString.Invariant($"<tr><td><b>another@one.com</b></td><td>{contact2.OrgCode}</td><td>{contact2.WorkingAddressCompanyName}</td></tr>"), mergedAccountsTable);
			AssertEndsWith("Should include table", "</td></tr></table>", mergedAccountsTable);

			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			mergedAccountsTable = docWrapper.MergedAccountsTable;
			AssertStartsWith("Should start with table tags", "<table cellspacing=\"20\"><tr><td><b>", mergedAccountsTable);
			AssertContains("Should return retained contacts", retainedContactsString, mergedAccountsTable);
			AssertContains("Should include dissolvedPerson2's contacts now. Grouped by email", FormattableString.Invariant($"<tr><td><b>another@one.com</b></td><td>{contact2.OrgCode}</td><td>{contact2.WorkingAddressCompanyName}</td></tr><tr><td></td><td>{contact3.OrgCode}</td><td>{contact3.WorkingAddressCompanyName}</td></tr>"), mergedAccountsTable);
			AssertContains("Should include dissolvedPerson2's contacts now. Grouped by email", FormattableString.Invariant($"<tr><td><b>yetanother@one.com</b></td><td>{contact4.OrgCode}</td><td>{contact4.WorkingAddressCompanyName}</td></tr>"), mergedAccountsTable);
			AssertEndsWith("Should include table", "</td></tr></table>", mergedAccountsTable);
		}

		public void TestAccountsWithRetainedPassword_PasswordOnDissolvedPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertContains("Should return dissolvedPerson2's contacts grouped by email", FormattableString.Invariant($"<b>another@one.com<b><br>{contact3.OrgCode} - {contact3.WorkingAddressCompanyName}"), docWrapper.AccountsWithRetainedPassword);
			Assert("Should remove trailing <br>", !docWrapper.MergedAccounts.EndsWith("<br>"));
		}

		public void TestAccountsWithRetainedPassword_PasswordOnRetainedPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", FormattableString.Invariant($"<b>other@email.com<b><br>{contact1.OrgCode} - {contact1.WorkingAddressCompanyName}"), docWrapper.AccountsWithRetainedPassword);
		}

		public void TestAccountsWithRetainedPassword_NoContactOnRetainedPersonWithNoPassword()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", retainedPerson.PER_FullName, docWrapper.AccountsWithRetainedPassword);
		}

		public void TestAccountsWithRetainedPassword_PasswordOnDissolvedPersonWithNoContact()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", dissolvedPerson2.PER_FullName, docWrapper.AccountsWithRetainedPassword);
		}

		public void TestAccountsWithRetainedPassword_NoPasswordsShouldReportError()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", string.Empty, docWrapper.AccountsWithRetainedPassword);
			Assert(ErrorReporter.HasBeenReported("No Password was present when one was expected"));
			ErrorReporter.Clear();
		}

		public void TestAccountsWithRetainedPasswordTable_PasswordOnDissolvedPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			var accountsWithRetainedPasswordTable = docWrapper.AccountsWithRetainedPasswordTable;
			AssertStartsWith("Should start with table tags", "<table cellspacing=\"20\"><tr><td><b>", docWrapper.AccountsWithRetainedPasswordTable);
			AssertContains("Should return dissolvedPerson2's contacts grouped by email", FormattableString.Invariant($"<tr><td><b>another@one.com</b></td><td>{contact3.OrgCode}</td><td>{contact3.WorkingAddressCompanyName}</td></tr>"), accountsWithRetainedPasswordTable);
			AssertEndsWith("Should include table", "</td></tr></table>", accountsWithRetainedPasswordTable);
		}

		public void TestAccountsWithRetainedPasswordTable_PasswordOnRetainedPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			var accountsWithRetainedPasswordTable = docWrapper.AccountsWithRetainedPasswordTable;
			AssertEquals("Should return retainedPerson's contacts grouped by email", FormattableString.Invariant($"<table cellspacing=\"20\"><tr><td><b>other@email.com</b></td><td>{contact1.OrgCode}</td><td>{contact1.WorkingAddressCompanyName}</td></tr></table>"), accountsWithRetainedPasswordTable);
		}

		public void TestAccountsWithRetainedPasswordTable_NoContactOnRetainedPersonWithNoPassword()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = dissolvedPerson2.PK;
			contact3.OC_Email = "another@one.com";
			contact3.OC_ContactName = dissolvedPerson2.PER_FullName;
			var contact4 = org3.Contacts.AddNew();
			contact4.OC_PER = dissolvedPerson2.PK;
			contact4.OC_Email = "yetanother@one.com";
			contact4.OC_ContactName = dissolvedPerson2.PER_FullName + "ara";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", retainedPerson.PER_FullName, docWrapper.AccountsWithRetainedPasswordTable);
		}

		public void TestAccountsWithRetainedPasswordTable_PasswordOnDissolvedPersonWithNoContact()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Blue world";
			dissolvedPerson2.PER_EmailAddress = "ran@run.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson2.PER_PasswordHashIterations = 9239;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1, dissolvedPerson2 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			emailSender.MarkPersonAsDissolved(dissolvedPerson2.PK);
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			retainedPerson.PER_PasswordHashIterations = 9239;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", dissolvedPerson2.PER_FullName, docWrapper.AccountsWithRetainedPasswordTable);
		}

		public void TestAccountsWithRetainedPasswordTable_NoPasswordsShouldReportError()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Rand Al'Thor";
			retainedPerson.PER_EmailAddress = "rand@al.com";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Lews Therin Telamon";
			dissolvedPerson1.PER_EmailAddress = "lanfear@theways.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = dissolvedPerson1.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = dissolvedPerson1.PER_FullName;
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson1 }));
			emailSender.MarkPersonAsDissolved(dissolvedPerson1.PK);
			Factory.Save();

			var docWrapper = DocPersonMergeEmailSender.New(emailSender, Factory);
			AssertEquals("Should return retainedPerson's contacts grouped by email", string.Empty, docWrapper.AccountsWithRetainedPasswordTable);
			Assert(ErrorReporter.HasBeenReported("No Password was present when one was expected"));
			ErrorReporter.Clear();
		}

		#region Implementation

		PersonMergeEmailSender EmailSender;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = retainedPerson.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = retainedPerson.PER_FullName;
			retainedPerson.SetHashedPassword("1234");
			Factory.Save();

			EmailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson }));

			return new DocumentWrapper[] { DocPersonMergeEmailSender.New(EmailSender, Factory) };
		}

		#endregion
	}
}
