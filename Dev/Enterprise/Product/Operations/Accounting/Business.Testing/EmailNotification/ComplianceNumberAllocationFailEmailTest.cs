using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class ComplianceNumberAllocationFailEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get { return typeof(ComplianceNumberAllocationFailureEmail); }
		}

		public void TestEmail()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var invoice = creator.CreateARInvoice<ARInvoice>("1", creator.AUD, 1m, creator.ABIGAS);
			invoice.AH_ComplianceSubType = "TXI";

			ComplianceNumberAllocationFailureEmail mail = new ComplianceNumberAllocationFailureEmail(invoice);

			string expectedSubject = string.Format("Compliance Invoice Book Allocation Failure - Branch {0} / Sub Type TXI", GlbBranch.CurrentBranch.GB_Code);
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));

			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, creator.ABIGAS.CompanyData.PK);

			string expectedBody = string.Format(@"Please review your Compliance Invoice Book setups for Branch {0} and Sub Type {1} in the Maintain > Account > Compliance Sequences module.
Whilst logged in to Branch {0}  User {2} attempted to assign a Compliance Number against transaction {3}.
This assignment could not be made because an Active Compliance Book for Branch {0} and Sub Type {1} did not exist.
If required, please configure / activate a new Compliance Invoice Book.",
				GlbBranch.CurrentBranch.GB_Code,
				"TXI",
				Env.CurrentUser.FullName,
				"1");

			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL), GetBody(mail));
		}
	}
}
