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
	class VietnamEInvoiceCancellationCircularCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceCancellationCreation()
		{
			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbCompany.CurrentCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Prefix = "PREFIX/";
			sequence.XD_IsActive = true;

			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
			arInvoice.AH_XD_ComplianceBook = sequence.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";

			var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
			arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
			arCreditNote.SupportingDocumentNumber = "SDN";
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var eInvoiceCancellation = CreateVietnamEInvoiceCancellation(arInvoice, arCreditNote);
			AssertNotNull("EInvoice", eInvoiceCancellation);
			AssertCancellationEInvoice(eInvoiceCancellation, arInvoice, arCreditNote);
		}

		VietnamEInvoiceCancellationCircular CreateVietnamEInvoiceCancellation(ARInvoice arInvoice, ARCreditNote arCreditNote)
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

			var eInvoiceCancellationCreator = new VietnamEInvoiceCancellationCircularCreator(additionalTransactionInfo);
			var eInvoice = eInvoiceCancellationCreator.Create();
			return eInvoice;
		}

		void AssertCancellationEInvoice(VietnamEInvoiceCancellationCircular cancellationEInvoice, ARInvoice arInvoice, ARCreditNote arCreditNote)
		{
			CombineAssertions(() =>
			{
				// Lang
				AssertEquals("language", AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.Value, cancellationEInvoice.Language);

				// User
				AssertNullOrEmpty(cancellationEInvoice.User.Username);
				AssertNullOrEmpty(cancellationEInvoice.User.Password);
				// wrongnotice
				AssertEquals("stax", VietnamEInvoiceHelper.GetVietnamRegistrationNumber(arInvoice.Company.PK.ToGuid(), arInvoice.Branch.PK.ToGuid()), cancellationEInvoice.Wrongnotice.Stax);
				AssertEquals("noti_taxtype", "1", cancellationEInvoice.Wrongnotice.Taxtype);
				AssertEquals("noti_taxnum", ZString.Empty, cancellationEInvoice.Wrongnotice.Taxnum);
				AssertEquals("noti_taxdt", ZString.Empty, cancellationEInvoice.Wrongnotice.Taxdt);
				AssertEquals("budget_relationid", ZString.Empty, cancellationEInvoice.Wrongnotice.BudgetRelationid);
				AssertEquals("place", VietnamEInvoiceHelper.GetVietnamProxyOrgCityName(arInvoice.Company.PK.ToGuid(), arInvoice.Branch.PK.ToGuid()), cancellationEInvoice.Wrongnotice.Place);
				// items
				AssertEquals("form", "1", cancellationEInvoice.Wrongnotice.Items[0].Form);
				AssertEquals("serial", arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty, cancellationEInvoice.Wrongnotice.Items[0].Serial);
				AssertEquals("seq", "AP/19E1", cancellationEInvoice.Wrongnotice.Items[0].Seq);
				AssertEquals("idt", arInvoice.AH_ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"), cancellationEInvoice.Wrongnotice.Items[0].Idt);
				AssertEquals("type_ref", 1, cancellationEInvoice.Wrongnotice.Items[0].TypeRef);
				AssertEquals("noti_type", "1", cancellationEInvoice.Wrongnotice.Items[0].NotiType);
				AssertEquals("rea", arCreditNote.AH_Desc, cancellationEInvoice.Wrongnotice.Items[0].Rea);
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
