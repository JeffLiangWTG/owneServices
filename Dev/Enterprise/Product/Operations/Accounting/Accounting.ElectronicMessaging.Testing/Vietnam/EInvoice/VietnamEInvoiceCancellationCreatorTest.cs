using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class VietnamEInvoiceCancellationCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceCancellationCreation()
		{
			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbCompany.CurrentCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Prefix = prefix;
			sequence.XD_IsActive = true;

			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
			arInvoice.AH_XD_ComplianceBook = sequence.PK;

			var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
			arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
			arCreditNote.SupportingDocumentNumber = "SDN";
			Factory.Save();

			var eInvoiceCancellation = CreateVietnamEInvoiceCancellation(arInvoice, arCreditNote);
			AssertNotNull("EInvoice", eInvoiceCancellation);
			AssertCancellationEInvoice(eInvoiceCancellation, arInvoice, arCreditNote);
		}

		VietnamEInvoiceCancellation CreateVietnamEInvoiceCancellation(ARInvoice arInvoice, ARCreditNote arCreditNote)
		{
			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arCreditNote.AH_TransactionReference,
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					OriginalSeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					OriginalIsComplianceNumberFormatDefault = true,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				RectifyReason = arCreditNote.AH_Desc,
				RectifyDate = arCreditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
				RectifySupportingDocumentNumber = arCreditNote.SupportingDocumentNumber
			};

			var eInvoiceCancellationCreator = new VietnamEInvoiceCancellationCreator(additionalTransactionInfo);
			var eInvoice = eInvoiceCancellationCreator.Create();
			return eInvoice;
		}

		void AssertCancellationEInvoice(VietnamEInvoiceCancellation cancellationEInvoice, ARInvoice arInvoice, ARCreditNote arCreditNote)
		{
			var seq = $"{form}-{prefix}-{arInvoice.AH_TransactionReference}";

			CombineAssertions(() =>
			{
				// User
				AssertNullOrEmpty(cancellationEInvoice.User.Username);
				AssertNullOrEmpty(cancellationEInvoice.User.Password);
				// Inv
				AssertEquals("seq", seq, cancellationEInvoice.Inv.Seq);
				// Adj
				AssertEquals("rdt", arCreditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"), cancellationEInvoice.Inv.Adj.Rdt);
				AssertEquals("rea", arCreditNote.AH_Desc, cancellationEInvoice.Inv.Adj.Rea);
				AssertEquals("ref", arCreditNote.SupportingDocumentNumber, cancellationEInvoice.Inv.Adj.Ref);
			});
		}

		readonly string prefix = "xyz";
		readonly string form = "world_peace";

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, form);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
