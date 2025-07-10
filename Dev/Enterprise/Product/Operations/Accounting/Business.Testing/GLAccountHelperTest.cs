using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class GLAccountHelperTest : TestCaseWithFactory
	{
		[TestDate(2006, 10, 21)]
		public void TestAPLineCheckLocalAccountDescriptorHasMapping()
		{
			var errorMsg = "Please select a valid GL Account. You cannot select a GL Account without Mapping as you do not have following security right: " + Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.DisplayTextPathToSecurityRight;
			var creator = new TestObjectCreator(Factory);

			var accGlHeader = creator.CreateGLHeader();
			var accGlHeaderWithMapping = creator.CreateGLHeader();
			var accGlAccountDescriptor = creator.CreateAccountDesriptorLight("1000.00.00", AccGLAccountDescriptor.ReportTypeCOA, GlbStaff.CurrentUser.GS_WorkingLanguage, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			creator.CreateGLDescriptorPivotLight(accGlAccountDescriptor, accGlHeaderWithMapping);

			Factory.Save();

			var invoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "001", creator.AUD, 1m, 10m, 0m, 10m, 0m);
			var line = invoice.Lines[0];

			// With security right and no mapping.
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			// With security right and has mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.IsAllowed = false;

			// Without security right and no mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Attempt to change validation on a property info outside of its Check method", ErrorReporter.LastMessageReported);
			AssertContains("Field: Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceLine.AL_AG", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertHasError(line.AL_AGInfo, errorMsg);

			// Without security right and has mapping.
			line.AL_AG = accGlHeaderWithMapping.PK;
			AssertNoErrors(line.AL_AGInfo);
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);
		}

		[TestDate(2006, 10, 21)]
		public void TestARLineCheckLocalAccountDescriptorHasMapping()
		{
			var errorMsg = "Please select a valid GL Account. You cannot select a GL Account without Mapping as you do not have following security right: " + Environment.Env.Security.NewReceivablesAllowGLAccountWithoutMapping.DisplayTextPathToSecurityRight;
			var creator = new TestObjectCreator(Factory);

			var accGlHeader = creator.CreateGLHeader();
			var accGlHeaderWithMapping = creator.CreateGLHeader();
			var accGlAccountDescriptor = creator.CreateAccountDesriptorLight("1000.00.00", AccGLAccountDescriptor.ReportTypeCOA, GlbStaff.CurrentUser.GS_WorkingLanguage, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			creator.CreateGLDescriptorPivotLight(accGlAccountDescriptor, accGlHeaderWithMapping);

			Factory.Save();

			var invoice = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "001", creator.AUD, 1m, 10m, 0m, 10m, 0m);
			var line = invoice.Lines[0];

			// With security right and no mapping.
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			// With security right and has mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			Environment.Env.Security.NewReceivablesAllowGLAccountWithoutMapping.IsAllowed = false;

			// Without security right and no mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Attempt to change validation on a property info outside of its Check method", ErrorReporter.LastMessageReported);
			AssertContains("Field: Enterprise.Accounting.Business.ARAP.Invoicing.ARCreditNoteLine.AL_AG", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertHasError(line.AL_AGInfo, errorMsg);

			// Without security right and has mapping.
			line.AL_AG = accGlHeaderWithMapping.PK;
			AssertNoErrors(line.AL_AGInfo);
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);
		}

		[TestDate(2006, 10, 21)]
		public void TestCashBookLineCheckLocalAccountDescriptorHasMapping()
		{
			var errorMsg = "Please select a valid GL Account. You cannot select a GL Account without Mapping as you do not have following security right: " + Environment.Env.Security.GeneralLedgerJournalAllowGLAccountWithoutMapping.DisplayTextPathToSecurityRight;
			var creator = new TestObjectCreator(Factory);

			var accGlHeader = creator.CreateGLHeader();
			var accGlHeaderWithMapping = creator.CreateGLHeader();
			var accGlAccountDescriptor = creator.CreateAccountDesriptorLight("1000.00.00", AccGLAccountDescriptor.ReportTypeCOA, GlbStaff.CurrentUser.GS_WorkingLanguage, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			creator.CreateGLDescriptorPivotLight(accGlAccountDescriptor, accGlHeaderWithMapping);

			Factory.Save();

			var gLJournal = creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDate.Today);

			// With security right and no mapping.
			var line = creator.CreateGLJournalLine(gLJournal, 10m, DebitCredit.CR, accGlHeader.PK);
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			// With security right and has mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);

			Environment.Env.Security.GeneralLedgerJournalAllowGLAccountWithoutMapping.IsAllowed = false;

			// Without security right and no mapping.
			line.AL_AG = ZGuid.Empty;
			line.AL_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Attempt to change validation on a property info outside of its Check method", ErrorReporter.LastMessageReported);
			AssertContains("Field: Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalLine.AL_AG", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertHasError(line.AL_AGInfo, errorMsg);

			// Without security right and has mapping.
			line.AL_AG = accGlHeaderWithMapping.PK;
			AssertNoErrors(line.AL_AGInfo);
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(line);
			AssertEquals(false, line.AL_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(line.AL_AGInfo);
		}

		[TestDate(2006, 10, 21)]
		public void TestHeaderCheckLocalAccountDescriptorHasMapping()
		{
			var errorMsg = "Please select a valid GL Account. You cannot select a GL Account without Mapping as you do not have following security right: " + Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.DisplayTextPathToSecurityRight;
			var creator = new TestObjectCreator(Factory);

			var accGlHeader = creator.CreateGLHeader();
			var accGlHeaderWithMapping = creator.CreateGLHeader();
			var accGlAccountDescriptor = creator.CreateAccountDesriptorLight("1000.00.00", AccGLAccountDescriptor.ReportTypeCOA, GlbStaff.CurrentUser.GS_WorkingLanguage, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			creator.CreateGLDescriptorPivotLight(accGlAccountDescriptor, accGlHeaderWithMapping);

			Factory.Save();

			var journal = creator.CreateJournal<APJournal>(100m, ZDateTime.Now, creator.Creditor1.PK);

			// With security right and no mapping.
			journal.AH_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(journal);
			AssertEquals(false, journal.AH_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(journal.AH_AGInfo);

			// With security right and has mapping.
			journal.AH_AG = ZGuid.Empty;
			journal.AH_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(journal);
			AssertEquals(false, journal.AH_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(journal.AH_AGInfo);

			Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.IsAllowed = false;

			// Without security right and no mapping.
			journal.AH_AG = ZGuid.Empty;
			journal.AH_AG = accGlHeader.PK;
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(journal);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Attempt to change validation on a property info outside of its Check method", ErrorReporter.LastMessageReported);
			AssertContains("Field: Enterprise.Accounting.Business.ARAP.Journal.APJournal.AH_AG", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertHasError(journal.AH_AGInfo, errorMsg);

			// Without security right and has mapping.
			journal.AH_AG = accGlHeaderWithMapping.PK;
			AssertNoErrors(journal.AH_AGInfo);
			GLAccountHelper.CheckLocalAccountDescriptorHasMapping(journal);
			AssertEquals(false, journal.AH_AGInfo.HasMessageError(errorMsg));
			AssertNoErrors(journal.AH_AGInfo);
		}
	}
}
