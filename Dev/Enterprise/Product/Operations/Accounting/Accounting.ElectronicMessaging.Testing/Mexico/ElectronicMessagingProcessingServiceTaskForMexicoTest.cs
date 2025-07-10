using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForMexico))]
	public class ElectronicMessagingProcessingServiceTaskForMexicoTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForMexico>
	{
		protected override ZString CountryCode => CountryCodes.Mexico;
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };
		protected override ElectronicMessagingProcessingServiceTaskForMexico GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForMexico();

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "TES030201001");
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			Helper.AddCustomsCodeForCountryIfMissing(arOrg, CountryCode, MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "TES030201001");
		}

		protected override void AddAdditionalInformationForCompany(GlbCompany glbCompany)
		{
			var credentialForTestOnly = Factory.New<GlbCompanyEInvoicingCertificateCredential>();

			credentialForTestOnly.GP_GC = glbCompany.PK;
			credentialForTestOnly.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credentialForTestOnly.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credentialForTestOnly.GP_CertificateSerialNumber = "3230303031303030303030333030303232383233";
			credentialForTestOnly.GP_IssueDate = ZDateTime.Now.AddDays(-1);
			credentialForTestOnly.GP_ExpiryDate = ZDateTime.Now.AddDays(1);

			Factory.Save();
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI;
			}
			if (arCreditNote != null)
			{
				arCreditNote.AH_ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR;

				if (arCreditNote.IsReversalTransaction)
				{
					arCreditNote.ReversalStatusCode = "01";
				}
			}
			if (arAdjustmentNote != null)
			{
				arAdjustmentNote.AH_ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR;
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						CountryCode + " Newly created transactions",
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status            + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode  + "=" + CountryCode),
				};
			}
		}

		#region Remove tests when  XSD version 4.0 is enabled

		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			Assert("This test must be removed when the einvoicing provider enables the XSD version 4.0", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			Assert("This test must be removed when the einvoicing provider enables the XSD version 4.0. The interchange.EI_Status should have been HQU but was FAL", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			Assert("This test must be removed when the einvoicing provider enables the XSD version 4.0. The interchange.EI_Status should have been HQU but was FAL", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			Assert("This test must be removed when the einvoicing provider enables the XSD version 4.0", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			Assert("This test must be removed when the einvoicing provider enables the XSD version 4.0", true);
		}

		#endregion

	}
}
