using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class VietnamEInvoiceDocumentCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceDocumentRequestCreation()
		{
			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbCompany.CurrentCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Prefix = "xyz";
			sequence.XD_IsActive = true;

			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
			arInvoice.AH_XD_ComplianceBook = sequence.PK;
			Factory.Save();

			var eInvoiceCancellation = CreateVietnamEInvoiceDocument(arInvoice);
			AssertNotNull("EInvoice", eInvoiceCancellation);
			AssertEInvoiceDocument(eInvoiceCancellation, arInvoice);
		}

		VietnamEInvoiceDocument CreateVietnamEInvoiceDocument(ARInvoice arInvoice)
		{
			var eInvoiceDocumentCreator = new VietnamEInvoiceDocumentCreator();
			var eInvoice = eInvoiceDocumentCreator.Create(new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK });
			return eInvoice;
		}

		void AssertEInvoiceDocument(VietnamEInvoiceDocument eInvoiceDocument, ARInvoice arInvoice)
		{
			CombineAssertions(() =>
			{
				// User
				AssertNullOrEmpty(eInvoiceDocument.User.Username);
				AssertNullOrEmpty(eInvoiceDocument.User.Password);
				// Inv
				AssertEquals("sid", arInvoice.PK.ToString(), eInvoiceDocument.Inv.Sid);
				AssertEquals("type", "pdf", eInvoiceDocument.Inv.Type);
				AssertEquals("stax", VietnamEInvoiceHelper.GetVietnamRegistrationNumber(arInvoice.Company.PK.ToGuid(), arInvoice.Branch.PK.ToGuid()), eInvoiceDocument.Inv.Stax);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456");
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
