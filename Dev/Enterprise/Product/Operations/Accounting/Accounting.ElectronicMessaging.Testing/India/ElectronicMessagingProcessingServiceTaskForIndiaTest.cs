using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing.eInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForIndia))]
	public class ElectronicMessagingProcessingServiceTaskForIndiaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForIndia>
	{
		protected override ZString CountryCode => CountryCodes.India;

		protected override DateTime TestDate => new DateTime(2024, 11, 14);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => IndiaEInvoiceAPICommandList.Codes.GenerateIRN;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => IndiaEInvoiceAPICommandList.Codes.GenerateIRN;

		protected override ElectronicMessagingProcessingServiceTaskForIndia GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForIndia();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new[] { 1, 1 };

		protected override EInvoicingFeatureSettingsBuilder FeatureControlSettings
			=> new EInvoicingFeatureSettingsBuilder(CountryCode)
				.WithTransportDelivery(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface)
				.WithTransportDestination("XHUB_IN_EINVOICING2");

		protected override string ExpectedFeatureControlSettingsAsJson =>
"""
{
  "IN": {
    "Features": [],
    "Transport": {
      "Delivery": "XTT",
      "MessageType": null,
      "Destination": "XHUB_IN_EINVOICING2"
    },
    "CountrySpecific": {}
  }
}
""";

		protected override string ExpectedServicePointForFeatureControl => "XHUB_IN_EINVOICING2";
		protected override string ExpectedTransportTypeForFeatureControl => EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

		protected override void AddAdditionalInformationForCompany(GlbCompany glbCompany)
		{
			AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestDate);
			TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(glbCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation());
		}

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			branchWithCompany.BranchCredentialsIndia.Username = "User38205";
			branchWithCompany.BranchCredentialsIndia.Password = "SuperSecretPassword";
			branchWithCompany.BranchCredentialsIndia.PasswordConfirmation = "SuperSecretPassword";
		}

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCodes.India, OrgCusCode.CodeTypes.GSTCode);
			TestObjectCreator.CreateAddress(orgProxy,
				OrgAddressType.Office,
				isMain: true,
				streetAddress1: "Central Secretariat",
				streetAddress2: string.Empty,
				city: "New Delhi",
				stateCode: "DL",
				countryCode: CountryCodes.India,
				postCode: "110011",
				phone: "+9123016857",
				email: "address@b.com"
			);
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			Helper.AddCustomsCodeForCountryIfMissing(arOrg, CountryCodes.India, OrgCusCode.CodeTypes.GSTCode, "234567890123456");
			TestObjectCreator.PopulateOrgAddress(arOrg.MainAddress,
				OrgAddressType.Office,
				isMain: true,
				streetAddress1: "South Block",
				streetAddress2: "Raisina Hill",
				city: "New Delhi",
				stateCode: "DL",
				countryCode: "IN",
				postCode: "110011",
				phone: "023012312",
				email: ""
			);
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "ABC0000001";
			}
			if (arCreditNote != null)
			{
				arCreditNote.AH_ComplianceSubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
				arCreditNote.AH_TransactionReference = "ABC0000002";
			}
			if (arAdjustmentNote != null)
			{
				arAdjustmentNote.AH_ComplianceSubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXD;
				arAdjustmentNote.AH_TransactionReference = "ABC0000003";
			}
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			AssertNotNull(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertNull(nameof(geiMessage.TransactionBatch), geiMessage.TransactionBatch.Transactions);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, "Transaction");

			var credentials = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials;
			AssertEquals(2, credentials.Count);

			var taxpayerUsername = credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "Username").Value;
			AssertEquals(false, taxpayerUsername.Encrypted);
			AssertEquals("User38205", taxpayerUsername.Value);

			var taxpayerPassword = credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "Password").Value;
			AssertEquals(true, taxpayerPassword.Encrypted);
			AssertNoExceptionThrown(
				"EHub encryption is not deterministic; value should be a valid base64 string",
				() => Convert.FromBase64String(taxpayerPassword.Value)
			);

			var additionalItems = geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems;
			AssertEquals(1, additionalItems.Count);

			var regItem = additionalItems.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().Single(x => x.Key == "Reg_ComplianceNumFrom").Value;
			AssertEquals("2024-11-14", regItem);
		}

		protected override void AssertCountrySpecificGEIMessageContentForReversal(GlobalElectronicInvoicing geiMessage)
		{
			Assert("Not applicable; India does not support cancellation messages.", true);
		}
	}
}
