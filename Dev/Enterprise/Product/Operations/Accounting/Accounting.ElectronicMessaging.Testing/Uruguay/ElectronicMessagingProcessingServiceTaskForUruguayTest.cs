using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForUruguay))]
	public class ElectronicMessagingProcessingServiceTaskForUruguayTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForUruguay>
	{
		protected override ZString CountryCode => CountryCodes.Uruguay;
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => "REQ";
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };
		protected override DateTime TestDate => new DateTime(2021, 02, 12);
		protected override ElectronicMessagingProcessingServiceTaskForUruguay GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForUruguay();

		protected override void AddAdditionalInformationForCompany(GlbCompany glbCompany)
		{
			glbCompany.FirstActiveBranch.OrgProxy.OH_RL_NKClosestPort = "UYMVD";
			glbCompany.FirstActiveBranch.OrgProxy.MainAddressCollection[0].OA_State = "MO";
		}

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "999999999999");
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, UruguayOrgCusCodeInfo.OrgCusCodes.BRC, "1");
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "IVA10";

			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "A101";

				var arInvoiceLine = (AccTransactionLines)arInvoice.Lines[0];
				arInvoiceLine.AL_AT = taxRate.PK;
			}
			if (arCreditNote != null)
			{
				arCreditNote.AH_ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR;
				arCreditNote.AH_TransactionReference = "B101";

				var line = (ARCreditNoteLine)arCreditNote.Lines[0];
				line.AL_AT = taxRate.PK;
				if (line.RelatedJobCharge != null)
				{
					line.RelatedJobCharge.JR_AT_SellGSTRate = taxRate.PK;
				}
			}
			if (arAdjustmentNote != null)
			{
				arAdjustmentNote.AH_ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD;
				arAdjustmentNote.AH_TransactionReference = "C101";
			}
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNotNullOrEmpty(nameof(geiMessage.Transaction), geiMessage.Transaction);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, nameof(geiMessage.Transaction));
		}

		protected override void AssertCountrySpecificGEIMessageContentForReversal(GlobalElectronicInvoicing geiMessage)
		{
			AssertNotNullOrEmpty(nameof(geiMessage.Transaction), geiMessage.Transaction);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, nameof(geiMessage.Transaction));
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
	}
}
