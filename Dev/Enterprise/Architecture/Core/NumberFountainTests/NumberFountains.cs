using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.NumberFountain.Internal;

#region Test
#if DEBUG

namespace Enterprise.NumberFountain.Testing
{
	using System.Data;
	using System.Text.RegularExpressions;
	using NUnit.Framework;

	[UseSnapshotProtection]
	public class NumberFountainsTest : TransactionedTestCase
	{
		public void TestFountain()
		{
			AssertNotNull(Fountains.JobShipmentNumber);
			AssertNotNull(Fountains.GetJobSupplierBookingNumberGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetForwardingGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetForwardingConsolGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetAgencyGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetCarrierShipmentReferenceGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetShippingInstructionReferenceGeneratorFountain("Generator"));
			AssertNotNull(Fountains.GetCAEntryNumberGeneratorFountain("Generator"));
			AssertNotNull(Fountains.JobShipmentNumberCFS);
			AssertNotNull(Fountains.JobShipmentNumberAgency);
			AssertNotNull(Fountains.CarrierShipmentReference);
			AssertNotNull(Fountains.ShippingInstructionReference);
			AssertNotNull(Fountains.JobCartageRunSheetNumber);
			AssertNotNull(Fountains.JobConsolidatedTransportBookingNumber);
			AssertNotNull(Fountains.StorageNumber);
			AssertNotNull(Fountains.GetStorageNumberGeneratorFountain("Generator"));

			AssertNotNull(Fountains.DescartesMessageNumberFountain);

			AssertNotNull(Fountains.ProcessID);

			AssertNotNull(Fountains.DDRBatchNo);
			AssertNotNull(Fountains.ARInvoiceNo);
			AssertNotNull(Fountains.APInvoiceNo);
			AssertNotNull(Fountains.JCJournalNo);
			AssertNotNull(Fountains.JRJournalNo);

			AssertNotNull(Fountains.ARCreditNoteNo);
			AssertNotNull(Fountains.APCreditNoteNo);

			AssertNotNull(Fountains.ARAdjustmentNoteNo);
			AssertNotNull(Fountains.APAdjustmentNoteNo);

			AssertNotNull(Fountains.ContraNo);
			AssertNotNull(Fountains.DirectReceiptNo);
			AssertNotNull(Fountains.DirectPaymentNo);

			AssertNotNull(Fountains.ARTransferNo);
			AssertNotNull(Fountains.APTransferNo);

			AssertNotNull(Fountains.TransferNo);
			AssertNotNull(Fountains.BatchReceiptNo);
			AssertNotNull(Fountains.MatchNo);
			AssertNotNull(Fountains.PaymentMatchNo);

			AssertNotNull(Fountains.CFSReference);
			AssertNotNull(Fountains.JobNumber);
			AssertNotNull(Fountains.QuoteNumber);
			AssertNotNull(Fountains.QuoteCustomisedNumber("DEJ"));
			AssertNotNull(Fountains.InterimNo);
			AssertNotNull(Fountains.ErrorReporting);

			AssertNotNull(Fountains.GLJournal);
			AssertNotNull(Fountains.WIPAccrualsJournal);

			AssertNotNull(Fountains.CustomsJobNo);
			AssertNotNull(Fountains.GetCustomsJobNoGeneratorFountain("Generator"));
			AssertNotNull(Fountains.JobContainerJobID);
			AssertNotNull(Fountains.SGMessageNumberSequence);

			AssertNotNull(Fountains.EDIFACTNumberFountain("M", "123", "456"));
			AssertNotNull(Fountains.EDIFACTNumberFountain("I", "123", "456"));
			AssertNotNull(Fountains.EDIFACTNumberFountain("G", "123", "456"));
			AssertNotNull(Fountains.CMRMessageNumberSequence);

			AssertNotNull(Fountains.ZACustomsEDIFACTNumberFountain("M", "456"));
			AssertNotNull(Fountains.ZACustomsEDIFACTNumberFountain("I", "456"));
			AssertNotNull(Fountains.ZACustomsEDIFACTNumberFountain("G", "456"));

			AssertNotNull(Fountains.AUOneStopInterchangeNumber);

			AssertNotNull(Fountains.NZCustomsInterchangeNumber);
			AssertNotNull(Fountains.NZMessageReferenceNumber);
			AssertNotNull(Fountains.NZOCRReferenceNumber);
			AssertNotNull(Fountains.ECIWriteOffManifestReference);
			AssertNotNull(Fountains.ExpressECIWriteOffReference);

			AssertNotNull(Fountains.SGCustomsInterchangeNumber);

			AssertNotNull(Fountains.JobConsolNumber);
			AssertNotNull(Fountains.JobConsolNumberCFS);

			AssertNotNull(Fountains.JobCartageNumber);
			AssertNotNull(Fountains.Payment);
			AssertNotNull(Fountains.Receipt);
			AssertNotNull(Fountains.FinancialTransactionsExportBatchNo);
			AssertNotNull(Fountains.TransportInterchangeNumber);

			AssertNotNull(Fountains.TransportJobMessageNo);
			AssertNotNull(Fountains.RepairEstimateNo);
			AssertNotNull(Fountains.CodecoNumber);
			AssertNotNull(Fountains.CMSInterchangeNumber);

			AssertNotNull(Fountains.ARDiscountNo);
			AssertNotNull(Fountains.APDiscountNo);
			AssertNotNull(Fountains.APExchangeDifferenceNo);
			AssertNotNull(Fountains.ARExchangeDifferenceNo);
			AssertNotNull(Fountains.ExchangeDifferenceNo);
			AssertNotNull(Fountains.AROverpaymentsNo);
			AssertNotNull(Fountains.APOverpaymentsNo);
			AssertNotNull(Fountains.BGMNo);
			AssertNotNull(Fountains.ZA_DA63Number);
			AssertNotNull(Fountains.ZA_EntryHeaderNo);

			AssertNotNull(Fountains.AUAirCargoJobNumber);
			AssertNotNull(Fountains.AUAirCargoPartShipment);
			AssertNotNull(Fountains.ManifestJobNo);
			AssertNotNull(Fountains.ManifestJobLineNo);
			AssertNotNull(Fountains.EUH7ManifestJobReference);

			AssertNotNull(Fountains.CartageLegNotificationSequence);

			AssertNotNull(Fountains.GetDtbTransportGeneratorFountain("Generator"));
			AssertNotNull(Fountains.DtbBookingConsolidationID);
			AssertNotNull(Fountains.DtbBookingConsolidationMultiJobID);
			AssertNotNull(Fountains.DtbBookingID);
			AssertNotNull(Fountains.DtbConsignmentConsolidationID);
			AssertNotNull(Fountains.DtbConsignmentID);
			AssertNotNull(Fountains.DtbConsignmentRunSheetID);

			AssertNotNull(Fountains.WarehouseDocketID);
			AssertNotNull(Fountains.WarehouseInvoiceNumber);
			AssertNotNull(Fountains.WarehouseLoadID);
			AssertNotNull(Fountains.WarehousePickNo);
			AssertNotNull(Fountains.WarehouseStocktakeNumber);
			AssertNotNull(Fountains.WarehouseVASOrderJobID);
			AssertNotNull(Fountains.WhsCycleCountLocationID);

			AssertNotNull(Fountains.PackageID);
			AssertNotNull(Fountains.PackingID);
			AssertNotNull(Fountains.PackingListID);

			AssertNotNull(Fountains.QueryClaimNo);
			AssertNotNull(Fountains.APQueryClaimNo);

			AssertNotNull(Fountains.SeaCargoMessageNumberFountain);
			AssertNotNull(Fountains.CusUnderbondNumberFountain);
			AssertNotNull(Fountains.CusMAWBMessageReferenceNumberFountain);

			AssertNotNull(Fountains.JobVoyageNumber(20));
			AssertNotNull(Fountains.VoyageDestinationNumber(20));
			AssertNotNull(Fountains.VoyageOriginNumber(20));

			AssertNotNull(Fountains.AUPartShipConRef);
			AssertNotNull(Fountains.CusSeaManTranHeaderNumber);
			AssertNotNull(Fountains.CusSeaManArrivalPortNumber);
			AssertNotNull(Fountains.CusSeaManOBLHeaderNumber);
			AssertNotNull(Fountains.CTOCusHAWBNumber);
			AssertNotNull(Fountains.CusSCAHouseNumber);
			AssertNotNull(Fountains.CusOutturnHeader);

			AssertNotNull(Fountains.SalesOpportunityID);
			AssertNotNull(Fountains.CrmOpportunityID);
			AssertNotNull(Fountains.GlbCompanyCampaignID);
			AssertNotNull(Fountains.ProcessTaskID);

			AssertNotNull(Fountains.InvoiceBatchNo);
			AssertNotNull(Fountains.SelfBillingInvoiceNo);
			AssertNotNull(Fountains.AccountingExportWebServiceBatchNo(Guid.NewGuid()));
			AssertNotNull(Fountains.GenExportBatchSequenceBatchNo);
			AssertNotNull(Fountains.AccCollectionBatchNo);
			AssertNotNull(Fountains.GeneralLedgerConsolidationBatchNo);

			AssertNotNull(Fountains.ContainerLogicalNumber);

			AssertNotNull(Fountains.ImporterSecurityFilingReference);
			AssertNotNull(Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("BLAH"));

			AssertNotNull(Fountains.USInBondJobReference);
			AssertNotNull(Fountains.USeManifestTripReference);
			AssertNotNull(Fountains.GetUSeManifestTripReferenceGeneratorFountain("BLAH"));

			AssertNotNull(Fountains.CommissionApprovalRequestBatchNo);

			AssertNotNull(Fountains.CommunicationID);

			AssertNotNull(Fountains.SupplierBookingNumber);
			AssertNotNull(Fountains.NctsLocalReferenceNumber);
			AssertNotNull(Fountains.GetNctsLocalReferenceNumberFountain("XXX"));
			AssertNotNull(Fountains.EULocalReferenceNumber(Guid.NewGuid()));
			AssertNotNull(Fountains.PNTSLocalReferenceNumber(Guid.NewGuid()));
			AssertNotNull(Fountains.G3LocalReferenceNumber(Guid.NewGuid()));
			AssertNotNull(Fountains.CHLocalReferenceNumber(Guid.NewGuid()));
			AssertNotNull(Fountains.CHDeclarationActivationJobNumber());

			AssertNotNull(Fountains.EUICS2LocalReferenceNumber(Guid.NewGuid()));

			AssertNotNull(Fountains.TelematicsEDIMessageNumber);
			AssertNotNull(Fountains.TelematicsRimEnrolmentReportNumber);
			AssertNotNull(Fountains.TelematicsRimDataBatchNumber);
			AssertNotNull(Fountains.TelematicsRimRegistrationNumber);

			AssertNotNull(Fountains.HVLVBookingHeaderJobNumber);
			AssertNotNull(Fountains.HVLVConsignmentHeaderJobNumber);
			AssertNotNull(Fountains.HVLVConsignmentClusterKey);
			AssertNotNull(Fountains.HVLVConsignmentId);
			AssertNotNull(Fountains.HVLVItemId);
			AssertNotNull(Fountains.HVLVOriginLoadListReference);

			AssertNotNull(Fountains.AccEInvoicingBatchNo);
			AssertNotNull(Fountains.CreditControlApproval);
			AssertNotNull(Fountains.EPaymentQuoteInternalRef);
			AssertNotNull(Fountains.EPaymentDealInternalReference);
			AssertNotNull(Fountains.EPaymentBeneficiaryRequestInternalRef);
			AssertNotNull(Fountains.AccBillingHeaderInternalReference);
			AssertNotNull(Fountains.ARCashAdvanceRequestReference);

			AssertNotNull(Fountains.OrgARClientNumber(Guid.NewGuid()));
			AssertNotNull(Fountains.InvoiceTransactionReference(Guid.NewGuid()));

			AssertNotNull(Fountains.ComplianceDocumentInternalReference("AR", "INV", Guid.NewGuid()));

			AssertNotNull(Fountains.EMCSLocalReferenceNumber);
			AssertNotNull(Fountains.GetEMCSLocalReferenceNumberFountain("XXX"));
			AssertNotNull(Fountains.GetConsolidatedDeclarationNumberFountain("XXX"));

			AssertNotNull(Fountains.ESCustomsEDIFACTNumberFountain("M", "456"));
			AssertNotNull(Fountains.ESCustomsEDIFACTNumberFountain("I", "456"));
			AssertNotNull(Fountains.ESCustomsEDIFACTNumberFountain("G", "456"));
			AssertNotNull(Fountains.ESBGMReference("2021", "WTL", "JPB"));
			AssertNotNull(Fountains.ESBGMLocalReferenceSuffix("ref"));

			AssertNotNull(Fountains.APPaymentApprovalReference);
			AssertNotNull(Fountains.ARPaymentApprovalReference);

			AssertNotNull(Fountains.LimitedFiscalRepresentationNo);
			AssertNotNull(Fountains.GateBookingNumber);
			AssertNotNull(Fountains.GteGateActionNumber);
			AssertNotNull(Fountains.GteMovementBookingNumber);

			AssertNotNull(Fountains.ProfitShareRedistributionNumber);
			AssertNotNull(Fountains.JobComInvoiceLineMatchingKey("EDIDAT", 35)); // prefix of numberfountain

			AssertNotNull(Fountains.ExitControlJobNumber);
			AssertNotNull(Fountains.GetExitControlJobNumberGeneratorFountain("Generator"));

			AssertNotNull(Fountains.CYDDeliveryHeaderJobNumber);
			AssertNotNull(Fountains.CYDPickupHeaderJobNumber);
			AssertNotNull(Fountains.CYDMovementHeaderJobNumber);

			AssertNotNull(Fountains.EdiIdentityCertificateSequenceNumber);

			AssertNotNull(Fountains.KREntryNumberFountain("KREXP", "00000", "23", 999999, false));
			AssertNotNull(Fountains.KRNumberFountain("KRCMR", 99999999, true));

			AssertNotNull(Fountains.ClusterKeyNumber);

			AssertNotNull(Fountains.ExternalRequestID);
			AssertNotNull(Fountains.JobSupplierBookingLineID);
		}

		public void TestFountainNumbersStartsWithTheirPrefix()
		{
			Assert("Customs Number starts with NumberFountains.CustomsJobNumberFountainPrefix", Fountains.CustomsJobNo.GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.CustomsJobNumberFountainPrefix));
			Assert("Transport Number starts with NumberFountains.JobCartageFountainCFSPrefix", Fountains.JobCartageNumber.GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.JobCartageFountainCFSPrefix));
			Assert("Self Billing Invoice Number starts with NumberFountains.SelfBillingInvoiceFountainPrefix", Fountains.SelfBillingInvoiceNo.GetTodaysPeriodFountain(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.Code, Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK).GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.SelfBillingInvoiceFountainPrefix));
			Assert("AR QueryClaim Reference starts with NumberFountains.ARQueryClaimFountainPrefix", Fountains.QueryClaimNo.GetNextFormatted(Connection, Transaction).StartsWith("RQ"));
			Assert("AP QueryClaim Reference starts with NumberFountains.APQueryClaimFountainPrefix", Fountains.APQueryClaimNo.GetNextFormatted(Connection, Transaction).StartsWith("PQ"));
			Assert("Supplier Booking Number starts with NumberFountains.SupplierBookingNumberFountainPrefix", Fountains.SupplierBookingNumber.GetNextFormatted(Connection, Transaction).StartsWith("M"));
			Assert("Job Supplier Booking Number starts with NumberFountains.SupplierBookingNumberFountainPrefix", Fountains.JobSupplierBookingNumber.GetNextFormatted(Connection, Transaction).StartsWith("SB"));
			Assert("Limited Fiscal Representation Number starts with NumberFountains.LimitedFiscalRepresentationNumberFountainPrefix", Fountains.LimitedFiscalRepresentationNo.GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.LimitedFiscalRepresentationNumberFountainPrefix));
			Assert("ProfitShare Redistribution Number starts with NumberFountains.ProfitShareRedistributionNumber", Fountains.ProfitShareRedistributionNumber.GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.ProfitShareRedistributionNumberFountainPrefix));
			Assert("Exit Control Job Number starts with NumberFountains.ExitControlJobNumberFountainPrefix", Fountains.ExitControlJobNumber.GetNextFormatted(Connection, Transaction).StartsWith(NumberFountains.ExitControlJobNumberFountainPrefix));
		}

		public void TestZACustomsEDIFACTNumberFountain()
		{
			var mFountain = Fountains.ZACustomsEDIFACTNumberFountain("M", "ReCEV");
			mFountain.SetValues(Connection, Transaction, minValue: 1, nextValue: 999898, maxValue: 999999);

			while (mFountain.GetNext(Connection, Transaction) != 999999)
			{ }

			AssertEquals("1", mFountain.GetNextFormatted(Connection, Transaction));
		}

		public void TestSSCCBarCodeNumberFountain()
		{
			AssertEquals("012345670000000015", Fountains.SSCCBarCodeNumber("1234567").GetNextFormatted(Connection, Transaction));
			// detailed testing is in the SSCC fountain
		}

		public void TestImporterJobReference()
		{
			Guid importerGuid1 = Guid.NewGuid();
			Guid importerGuid2 = Guid.NewGuid();
			AssertEquals("Number Fountain Number", "1", Fountains.ImporterJobReference(importerGuid1).GetNextFormatted(Connection, Transaction));
			AssertEquals("Number Fountain Number", "2", Fountains.ImporterJobReference(importerGuid1).GetNextFormatted(Connection, Transaction));
			AssertEquals("Next Number Fountain Number", "1", Fountains.ImporterJobReference(importerGuid2).GetNextFormatted(Connection, Transaction));
			AssertEquals("Number Fountain Number", "3", Fountains.ImporterJobReference(importerGuid1).GetNextFormatted(Connection, Transaction));
		}

		public void TestUSInBondNumberFountain()
		{
			var ownerPK = Guid.NewGuid();
			var fountain = Fountains.USInBondNumberFountain(ownerPK);
			AssertEquals(8, fountain.GetNextFormatted(Connection, Transaction).Length);
			fountain.SetNext(Connection, Transaction, FountainUtils.MaxNumber - FountainUtils.CacheSize - 1);
			while (fountain.GetNext(Connection, Transaction) != FountainUtils.MaxNumber)
			{
			}
			AssertEquals("PeekPreliminary", FountainUtils.MaxNumber + 1, fountain.PeekPreliminary(Connection, Transaction));
			AssertExceptionThrown(
				typeof(NumberFountainMaximumValueReachedException),
				"Fountain has reached its maximum value of 9220000000000000000. Cannot generate any more numbers. [Fountain (" + NumberFountains.USInBondNumber + ", Owner PK " + ownerPK.ToString().ToUpperInvariant() + ")]",
				delegate
				{ fountain.GetNextFormatted(Connection, Transaction); });
		}

		public void TestAMSManifestSequenceNumber()
		{
			AssertNotNull(Fountains.AMSManifestSequenceNumberFountain("OTT1"));
			AssertMatch(new Regex(@"\d{6}"), Fountains.AMSManifestSequenceNumberFountain("OTT1").GetNextFormatted(Connection, Transaction));
		}

		public void TestIncidentApprovalClientReference()
		{
			AssertEquals("Number Fountain Number", "SR00001000", Fountains.IncidentApprovalClientRef.GetNextFormatted(Connection, Transaction));
		}

		public void TestCustomerServiceIncidentNo()
		{
			AssertEquals("Number Fountain Number", "CS00000001", Fountains.CustomerServiceIncidentNo.GetNextFormatted(Connection, Transaction));
		}

		public void TestCertificateExamCampaignID()
		{
			AssertEquals("Number Fountain Number", "CRT00000001", Fountains.CertificateExamCampaignID.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetGeneratorFountain()
		{
			AssertEquals("GeneratorFountain-V-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAgencyGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-C-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetCAEntryNumberGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-SB-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-S-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-FC-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingConsolGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-T-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetStorageNumberGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-B-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetCustomsJobNoGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-SC-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetSundryChargesGeneratorFountain("KEY1", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-ISF-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain--KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAccountingNumberGeneratorFountain("KEY1", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-MAN-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetUSeManifestTripReferenceGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-DTB-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetDtbTransportGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-PKGID-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountain("KEY1")));
			AssertEquals("GeneratorFountain-PKGID-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY1", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-NCT-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetNctsLocalReferenceNumberFountain("KEY1")));
			AssertEquals("GeneratorFountain-E-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetEMCSLocalReferenceNumberFountain("KEY1")));
			AssertEquals("GeneratorFountain-CE-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetConsolidatedDeclarationNumberFountain("KEY1")));
			AssertEquals("GeneratorFountain-E-KEY1", GetFactoryName((NonFormattedNumberFountain)Fountains.GetExitControlJobNumberGeneratorFountain("KEY1")));

			AssertEquals("GeneratorFountain-V-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAgencyGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-C-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetCAEntryNumberGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-SB-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-S-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-FC-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingConsolGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-T-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetStorageNumberGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-B-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetCustomsJobNoGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-SC-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetSundryChargesGeneratorFountain("KEY2", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-ISF-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain--KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAccountingNumberGeneratorFountain("KEY2", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-MAN-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetUSeManifestTripReferenceGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-PKGID-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-PKGID-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY2", Guid.NewGuid())));
			AssertEquals("GeneratorFountain-DTB-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetDtbTransportGeneratorFountain("KEY2")));
			AssertEquals("GeneratorFountain-NCT-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetNctsLocalReferenceNumberFountain("KEY2")));
			AssertEquals("GeneratorFountain-E-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetEMCSLocalReferenceNumberFountain("KEY2")));
			AssertEquals("GeneratorFountain-CE-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetConsolidatedDeclarationNumberFountain("KEY2")));
			AssertEquals("GeneratorFountain-E-KEY2", GetFactoryName((NonFormattedNumberFountain)Fountains.GetExitControlJobNumberGeneratorFountain("KEY2")));

			AssertEquals("GeneratorFountain-V-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAgencyGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-C-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetCAEntryNumberGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-SB-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetJobSupplierBookingNumberGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-S-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-FC-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetForwardingConsolGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-SC-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetSundryChargesGeneratorFountain(new string('x', 50), Guid.NewGuid())));
			AssertEquals("GeneratorFountain-T-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetStorageNumberGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain--6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetAccountingNumberGeneratorFountain(new string('x', 50), Guid.NewGuid())));
			AssertEquals("GeneratorFountain-PKGID-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountain(new string('x', 50))));
			AssertEquals("GeneratorFountain-PKGID-6D696968", GetFactoryName((NonFormattedNumberFountain)Fountains.GetPackageIDGeneratorFountainWithPackingParent(new string('x', 50), Guid.NewGuid())));

			AssertEquals("00000001", Fountains.GetAgencyGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetCAEntryNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetForwardingGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetForwardingConsolGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetStorageNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetAgencyGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetCAEntryNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetForwardingGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetForwardingConsolGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetStorageNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetCustomsJobNoGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			var companyPK = Guid.NewGuid();
			AssertEquals("00000001", Fountains.GetAccountingNumberGeneratorFountain("KEY1", companyPK).GetNextFormatted(Connection, Transaction));
			var company2PK = Guid.NewGuid();
			AssertEquals("00000001", Fountains.GetAccountingNumberGeneratorFountain("KEY1", company2PK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetUSeManifestTripReferenceGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetPackageIDGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetPackageIDGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			var parent1PK = Guid.NewGuid();
			AssertEquals("00000001", Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY1", parent1PK).GetNextFormatted(Connection, Transaction));
			var parent2PK = Guid.NewGuid();
			AssertEquals("00000001", Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY2", parent2PK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetDtbTransportGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetDtbTransportGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetNctsLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetEMCSLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetConsolidatedDeclarationNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000001", Fountains.GetExitControlJobNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));

			AssertEquals("00000002", Fountains.GetAgencyGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetCAEntryNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetForwardingGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetForwardingConsolGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetStorageNumberGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetAgencyGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetCAEntryNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetForwardingGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetForwardingConsolGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetStorageNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetCustomsJobNoGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetAccountingNumberGeneratorFountain("KEY1", companyPK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetAccountingNumberGeneratorFountain("KEY1", company2PK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetUSeManifestTripReferenceGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetPackageIDGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetPackageIDGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY1", parent1PK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY2", parent2PK).GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetDtbTransportGeneratorFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetDtbTransportGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetNctsLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetEMCSLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetConsolidatedDeclarationNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
			AssertEquals("00000002", Fountains.GetExitControlJobNumberGeneratorFountain("KEY2").GetNextFormatted(Connection, Transaction));
		}

		public void TestMinMaxValues()
		{
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetAgencyGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 99_999_999, Fountains.GetCAEntryNumberGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetJobSupplierBookingNumberGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetForwardingGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetForwardingConsolGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetStorageNumberGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetCustomsJobNoGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetSundryChargesGeneratorFountain("KEY1", Guid.NewGuid()));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetImporterSecurityFilingReferenceGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetAccountingNumberGeneratorFountain("KEY1", Guid.NewGuid()));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetUSeManifestTripReferenceGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetDtbTransportGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetPackageIDGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetPackageIDGeneratorFountainWithPackingParent("KEY1", Guid.NewGuid()));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetNctsLocalReferenceNumberFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetAgencyGeneratorFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetEMCSLocalReferenceNumberFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetConsolidatedDeclarationNumberFountain("KEY1"));
			AssertMinMaxValues(1, 9_220_000_000_000_000_000, Fountains.GetExitControlJobNumberGeneratorFountain("KEY1"));
		}

		void AssertMinMaxValues(long expectedMinValue, long expectedMaxValue, INumberFountain numberFountain)
		{
			long maxValue, minValue;
			numberFountain.GetMinAndMaxValues(Connection, Transaction, out minValue, out maxValue);
			AssertEquals($"{GetFactoryName((NonFormattedNumberFountain)numberFountain)}.MinValue ", expectedMinValue, minValue);
			AssertEquals($"{GetFactoryName((NonFormattedNumberFountain)numberFountain)}.MaxValue ", expectedMaxValue, maxValue);
		}

		public void TestQuoteCustomisedNumber()
		{
			string name1 = GetFactoryName((NonFormattedNumberFountain)Fountains.QuoteCustomisedNumber("DEJ"));
			AssertEquals("QuoteCustomisedNumber:DEJ", name1);
			AssertEquals("QDEJ00001000", Fountains.QuoteCustomisedNumber("DEJ").GetNextFormatted(Connection, Transaction));
			AssertEquals("QDEJ00001001", Fountains.QuoteCustomisedNumber("DEJ").GetNextFormatted(Connection, Transaction));

			string name2 = GetFactoryName((NonFormattedNumberFountain)Fountains.QuoteCustomisedNumber("JAB"));
			AssertEquals("QuoteCustomisedNumber:JAB", name2);
			AssertEquals("QJAB00001000", Fountains.QuoteCustomisedNumber("JAB").GetNextFormatted(Connection, Transaction));
			AssertEquals("QJAB00001001", Fountains.QuoteCustomisedNumber("JAB").GetNextFormatted(Connection, Transaction));
		}

		public void TestUAENumberFountain()
		{
			AssertNotNull(Fountains.UAEDeliveryOrderNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{8}"), Fountains.UAEDeliveryOrderNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestCanadaControlNumberFountain()
		{
			AssertNotNull(Fountains.CanadaCargoControlNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{8}"), Fountains.CanadaCargoControlNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestContainerLogicalNumber()
		{
			AssertNotNull(Fountains.ContainerLogicalNumber);
			AssertMatch(new Regex(@"\d{7}"), Fountains.ContainerLogicalNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestXmlEDIInterchangeNumber()
		{
			AssertNotNull(Fountains.XmlEDIInterchangeNumber);
			AssertEquals(20, Fountains.XmlEDIInterchangeNumber.GetNextFormatted(Connection, Transaction).Length);
		}

		public void TestXmlEDIMessageNumber()
		{
			AssertNotNull(Fountains.XmlEDIMessageNumber);
			AssertEquals(20, Fountains.XmlEDIMessageNumber.GetNextFormatted(Connection, Transaction).Length);
		}
		public void TestUsageEDIMessageNumber()
		{
			AssertNotNull(Fountains.UsageEDIMessageNumber);
			string number = Fountains.UsageEDIMessageNumber.GetNextFormatted(Connection, Transaction);
			AssertEquals("length", 20, number.Length);
			Assert("Number fountain for UsageEDIMessageNumber should be lossy", !Fountains.UsageEDIMessageNumber.EnsureConsistentSequence);
		}

		public void TestSystemEDIInterchangeNumber()
		{
			AssertNotNull(Fountains.SystemEDIInterchangeNumber);
			string number = Fountains.SystemEDIInterchangeNumber.GetNextFormatted(Connection, Transaction);
			AssertEquals("length", 20, number.Length);
			Assert("prefix " + NumberFountains.SystemEDINumberPrefix, number.StartsWith(NumberFountains.SystemEDINumberPrefix));
			Assert("Number fountain for SystemEDIInterchangeNumber should be lossy", !Fountains.SystemEDIInterchangeNumber.EnsureConsistentSequence);
		}

		public void TestSystemEDIMessageNumber()
		{
			AssertNotNull(Fountains.SystemEDIMessageNumber);
			string number = Fountains.SystemEDIMessageNumber.GetNextFormatted(Connection, Transaction);
			AssertEquals("length", 20, number.Length);
			Assert("prefix " + NumberFountains.SystemEDINumberPrefix, number.StartsWith(NumberFountains.SystemEDINumberPrefix));
			Assert("Number fountain for SystemEDIMessageNumber should be lossy", !Fountains.SystemEDIMessageNumber.EnsureConsistentSequence);
		}

		public void TestNZTSWSenderReferenceNumberFountain()
		{
			AssertExceptionThrown(typeof(ArgumentException), delegate
			{ Fountains.NZTSWSenderReferenceNumberFountain(""); });

			var fountainCompany = Fountains.NZTSWSenderReferenceNumberFountain("I10");
			AssertEquals(8, fountainCompany.GetNextFormatted(Connection, Transaction).Length);

			fountainCompany.SetNext(Connection, Transaction, FountainUtils.MaxNumber - FountainUtils.CacheSize - 1);
			while (fountainCompany.GetNext(Connection, Transaction) != FountainUtils.MaxNumber)
			{
			}
			AssertEquals("PeekPreliminary", FountainUtils.MaxNumber + 1, fountainCompany.PeekPreliminary(Connection, Transaction));
			AssertExceptionThrown(
				typeof(NumberFountainMaximumValueReachedException),
				"Fountain has reached its maximum value of 9220000000000000000. Cannot generate any more numbers. [Fountain (I10:" + NumberFountains.NZTSWEntryRef + ")]",
				delegate
				{ fountainCompany.GetNextFormatted(Connection, Transaction); });
		}

		public void TestTelematicsEDIMessageNumberFountaion()
		{
			AssertNotNull(Fountains.ContainerLogicalNumber);
			AssertMatch(new Regex(@"^TEL[0-9]{10}$"), Fountains.TelematicsEDIMessageNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestOrgARClientNumber()
		{
			AssertNotNull(Fountains.OrgARClientNumber(Guid.NewGuid()));
			AssertEquals("Number Fountain Number", "00001000", Fountains.OrgARClientNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestInvoiceTransactionReference()
		{
			AssertNotNull(Fountains.InvoiceTransactionReference(Guid.NewGuid()));
			AssertEquals("Number Fountain Number", "00001000", Fountains.InvoiceTransactionReference(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestComplianceDocumentInternalReference()
		{
			var companyPk = Guid.NewGuid();
			AssertNotNull(Fountains.ComplianceDocumentInternalReference("AR", "INV", companyPk));
			AssertEquals("Number Fountain Number", "00001000", Fountains.ComplianceDocumentInternalReference("AR", "INV", companyPk).GetNextFormatted(Connection, Transaction));
			AssertEquals("Number Fountain Number", "00001001", Fountains.ComplianceDocumentInternalReference("AR", "INV", companyPk).GetNextFormatted(Connection, Transaction));
			AssertEquals("Number Fountain Number", "00001000", Fountains.ComplianceDocumentInternalReference("AR", "CRD", companyPk).GetNextFormatted(Connection, Transaction));
		}

		public void TestITMessageLocalReferenceNumber()
		{
			AssertEquals("000000000001", Fountains.ITMessageLocalReferenceNumberFountain("2022").GetNextFormatted(Connection, Transaction));
			AssertEquals("000000000002", Fountains.ITMessageLocalReferenceNumberFountain("2022").GetNextFormatted(Connection, Transaction));
			AssertEquals("000000000001", Fountains.ITMessageLocalReferenceNumberFountain("2021").GetNextFormatted(Connection, Transaction));
		}

		public void TestNOCustomsWarehouseGoodsNumber()
		{
			var date1 = new DateTime(2024, 08, 23);
			var date2 = new DateTime(2024, 08, 24);
			AssertEquals("001", Fountains.NOCustomsWarehouseGoodsNumberFountain(date1, "12345").GetNextFormatted(Connection, Transaction));
			AssertEquals("002", Fountains.NOCustomsWarehouseGoodsNumberFountain(date1, "12345").GetNextFormatted(Connection, Transaction));
			AssertEquals("001", Fountains.NOCustomsWarehouseGoodsNumberFountain(date2, "12345").GetNextFormatted(Connection, Transaction));
			AssertEquals("001", Fountains.NOCustomsWarehouseGoodsNumberFountain(date1, "12346").GetNextFormatted(Connection, Transaction));
			AssertMinMaxValues(1, 999, Fountains.NOCustomsWarehouseGoodsNumberFountain(date1, "55555"));
		}

		public void TestESBGMReference() => CombineAssertions(() =>
		{
			AssertEquals("ES220000001WTLJPB", Fountains.ESBGMReference("2022", "WTL", "JPB").GetNextFormatted(Connection, Transaction));
			AssertEquals("ES220000002WTLJPB", Fountains.ESBGMReference("2022", "WTL", "JPB").GetNextFormatted(Connection, Transaction));
			AssertEquals("ES210000001WTLJPB", Fountains.ESBGMReference("2021", "WTL", "JPB").GetNextFormatted(Connection, Transaction));

			AssertEquals("ES220000001WTLAZM", Fountains.ESBGMReference("2022", "WTL", "AZM").GetNextFormatted(Connection, Transaction));
			AssertEquals("ES220000001TARJPB", Fountains.ESBGMReference("2022", "TAR", "JPB").GetNextFormatted(Connection, Transaction));
			AssertEquals("ES220000001BBBAAA", Fountains.ESBGMReference("2022", "BBB", "AAA").GetNextFormatted(Connection, Transaction));
		});

		public void TestDETempStorageJobReference()
		{
			AssertEquals("TS00000001", Fountains.DETempStorageJobReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestDEMessageControlNumberFountain()
		{
			CombineAssertions(() =>
			{
				var fountain1 = Fountains.DEMessageControlNumber(Guid.NewGuid(), "", 14);
				var number1 = fountain1.GetNextFormatted(Connection, Transaction);
				AssertNotNull("fountain1", fountain1);
				AssertEquals("number1 length", 14, number1.Length);
				AssertEquals("number1 no prefix", 0, Array.FindIndex(number1.ToArray(), char.IsNumber));

				var fountain2 = Fountains.DEMessageControlNumber(Guid.NewGuid(), "EDI", 11);
				var number2 = fountain2.GetNextFormatted(Connection, Transaction);
				AssertNotNull("fountain2", fountain2);
				AssertEquals("number2 length", 14, number2.Length);
				AssertEquals("number2 prefix", number2.Substring(0, 3), "EDI");
			});
		}

		public void TestDEInterchangeControlReference()
		{
			AssertEquals("00000000000001", Fountains.DEInterchangeControlReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestDEEMCSInterchangeControlReference()
		{
			AssertEquals("00000000000001", Fountains.DEEMCSInterchangeControlReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestMonthlyClosingJobReference()
		{
			AssertEquals("MON0000001", Fountains.MonthlyClosingJobReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetMonthlyClosingJobReferenceGeneratorFountain()
		{
			var fountain1 = Fountains.GetMonthlyClosingJobReferenceGeneratorFountain("KEY1");
			var fountain2 = Fountains.GetMonthlyClosingJobReferenceGeneratorFountain("KEY2");

			CombineAssertions(() =>
			{
				AssertEquals("Name of fountain1", "GeneratorFountain-MON-KEY1", GetFactoryName((NonFormattedNumberFountain)fountain1));
				AssertEquals("Name of fountain2", "GeneratorFountain-MON-KEY2", GetFactoryName((NonFormattedNumberFountain)fountain2));
				AssertEquals("Number #1 of fountain1", "00000001", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain1", "00000002", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #1 of fountain2", "00000001", fountain2.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain2", "00000002", fountain2.GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestFRTempStorageJobReference()
		{
			AssertEquals("FRJ00000001", Fountains.FRTempStorageJobReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestIEMessageControlNumber()
		{
			AssertEquals("AES00000000000001", Fountains.GetIEMessageControlNumber("AES").GetNextFormatted(Connection, Transaction));
			AssertEquals("AIS00000000000002", Fountains.GetIEMessageControlNumber("AIS").GetNextFormatted(Connection, Transaction));
		}

		public void TestFRStatementNumber()
		{
			AssertEquals("L00000001", Fountains.FRStatementNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetFRStatementNumberGeneratorFountain()
		{
			var fountain1 = Fountains.GetFRStatementNumberGeneratorFountain("KEY1");
			var fountain2 = Fountains.GetFRStatementNumberGeneratorFountain("KEY2");

			CombineAssertions(() =>
			{
				AssertEquals("Name of fountain1", "GeneratorFountain-L-KEY1", GetFactoryName((NonFormattedNumberFountain)fountain1));
				AssertEquals("Name of fountain2", "GeneratorFountain-L-KEY2", GetFactoryName((NonFormattedNumberFountain)fountain2));
				AssertEquals("Number #1 of fountain1", "00000001", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain1", "00000002", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #1 of fountain2", "00000001", fountain2.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain2", "00000002", fountain2.GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestFRMessageBatchNumber()
		{
			AssertEquals("0000000000000001", Fountains.FRMessageBatchNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetFRMessageBatchNumberGeneratorFountain()
		{
			var fountain1 = Fountains.GetFRMessageBatchNumberGeneratorFountain("KEY1");
			var fountain2 = Fountains.GetFRMessageBatchNumberGeneratorFountain("KEY2");

			CombineAssertions(() =>
			{
				AssertEquals("Name of fountain1", "GeneratorFountain--KEY1", GetFactoryName((NonFormattedNumberFountain)fountain1));
				AssertEquals("Name of fountain2", "GeneratorFountain--KEY2", GetFactoryName((NonFormattedNumberFountain)fountain2));
				AssertEquals("Number #1 of fountain1", "00000001", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain1", "00000002", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #1 of fountain2", "00000001", fountain2.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain2", "00000002", fountain2.GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestEUMessageControlNumber()
		{
			AssertEquals("IC200000000000001", Fountains.EUMessageControlNumber("IC2").GetNextFormatted(Connection, Transaction));
			AssertEquals("IC200000000000002", Fountains.EUMessageControlNumber("IC2").GetNextFormatted(Connection, Transaction));
			AssertEquals(17, Fountains.EUMessageControlNumber("IC2").GetNextFormatted(Connection, Transaction).Length);
		}

		public void TestTRMessageControlNumber()
		{
			AssertNotNull(Fountains.TRMessageControlNumber(Guid.NewGuid()));
			AssertEquals(14, Fountains.TRMessageControlNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction).Length);
		}

		public void TestTRManifestStatementNumberFountain()
		{
			AssertEquals("Fountain Name", "TRManifestStatementULU2022", Fountains.TRStampDutyLedgerNumberFountain("ULU", "2022", 1).Name);
			AssertEquals("Fountain Number", "WTG00003456", Fountains.TRStampDutyLedgerNumberFountain("WTG", "2023", 3456).GetNextFormatted(Connection, Transaction));
		}

		public void TestNLMessageControlNumber()
		{
			AssertNotNull(Fountains.NLMessageControlNumber(Guid.NewGuid()));
			AssertEquals(14, Fountains.NLMessageControlNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction).Length);
		}

		public void TestILMessageControlNumber()
		{
			AssertNotNull(Fountains.ILMessageControlNumber(Guid.NewGuid()));
			AssertEquals(14, Fountains.ILMessageControlNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction).Length);
		}

		public void TestILDeliveryOrderNumber()
		{
			AssertNotNull(Fountains.ILDeliveryOrderNumber);
			var number = Fountains.ILDeliveryOrderNumber.GetNextFormatted(Connection, Transaction);
			AssertEquals(8, number.Length);
			AssertEquals("10000000", number);
		}

		public void TestILGatePassMovementNumber()
		{
			AssertNotNull(Fountains.ILGatePassMovementNumber);
			var number = Fountains.ILGatePassMovementNumber.GetNextFormatted(Connection, Transaction);
			AssertEquals(8, number.Length);
			AssertEquals("10000000", number);
		}

		public void TestPLMessageControlNumber_AcceptableInput() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Attempt to get PLMessageControlNumber with a NULL application code", () => Fountains.PLMessageControlNumber(applicationCode: null, year2Digits: "25"));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLMessageControlNumber with a wrong application code", () => Fountains.PLMessageControlNumber(applicationCode: "PL", year2Digits: "25"));
			AssertExceptionThrown<ArgumentNullException>("Attempt to get PLMessageControlNumber with a NULL year", () => Fountains.PLMessageControlNumber(applicationCode: "PLC", year2Digits: null));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLMessageControlNumber with a wrong year", () => Fountains.PLMessageControlNumber(applicationCode: "PLC", year2Digits: "BC"));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLMessageControlNumber with a wrong message code", () => Fountains.PLMessageControlNumber(applicationCode: "PLC", year2Digits: "25", optionalMessageCode5Symbols: "PL"));
			AssertNotNull("Attempt to get PLMessageControlNumber with correct data", Fountains.PLMessageControlNumber(applicationCode: "PLC", year2Digits: "25"));
			AssertNotNull("Attempt to get PLMessageControlNumber with correct data and message code", Fountains.PLMessageControlNumber(applicationCode: "PLC", year2Digits: "25", optionalMessageCode5Symbols: "IE001"));
		});

		public void TestPLMessageControlNumber_ResultNumbers() => CombineAssertions(() =>
		{
			AssertEquals("PLC, year=21 => 1st number", "210000001", Fountains.PLMessageControlNumber("PLC", "21").GetNextFormatted(Connection, Transaction));
			AssertEquals("PLC, year=21 => 2nd number", "210000002", Fountains.PLMessageControlNumber("PLC", "21").GetNextFormatted(Connection, Transaction));
			AssertEquals("PLC, year=22 => 1st number", "220000001", Fountains.PLMessageControlNumber("PLC", "22").GetNextFormatted(Connection, Transaction));
			AssertEquals("PLN, year=21 => 1st number", "210000001", Fountains.PLMessageControlNumber("PLN", "21").GetNextFormatted(Connection, Transaction));
			AssertEquals("PLN, year=21 => 2st number, with message code", "21IE0510000002", Fountains.PLMessageControlNumber("PLN", "21", "IE051").GetNextFormatted(Connection, Transaction));
		});

		public void TestPLBGMReferenceNumber() => CombineAssertions(() =>
		{
				AssertEquals("branch= XXX, year=22 => 1st number", "22XXX000000001", Fountains.PLBGMReferenceNumber("XXX", "22").GetNextFormatted(Connection, Transaction));
				AssertEquals("branch= XXX, year=23 => 1st number", "23XXX000000001", Fountains.PLBGMReferenceNumber("XXX", "23").GetNextFormatted(Connection, Transaction));
				AssertEquals("branch= XXX, year=23 => 2nd number", "23XXX000000002", Fountains.PLBGMReferenceNumber("XXX", "23").GetNextFormatted(Connection, Transaction));
				AssertEquals("branch= YYY, year=23 => 1st number", "23YYY000000001", Fountains.PLBGMReferenceNumber("YYY", "23").GetNextFormatted(Connection, Transaction));
		});

		public void TestPLBGMReferenceNumberThrowsExceptionWithNullBranch()
		{
			AssertExceptionThrown<ArgumentNullException>("Attempt to get PLBGMReferenceNumber with a NULL branch", () => Fountains.PLBGMReferenceNumber(branch: null, "23"));
		}

		public void TestPLBGMReferenceNumberThrowsExceptionWithNullYear()
		{
			AssertExceptionThrown<ArgumentNullException>("Attempt to get PLBGMReferenceNumber with a NULL year", () => Fountains.PLBGMReferenceNumber("XXX", null));
		}

		public void TestPLBGMReferenceNumberThrowsExceptionWithInvalidYearFormat() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("Attempt to get PLBGMReferenceNumber with a BLANK year", () => Fountains.PLBGMReferenceNumber("XXX", ""));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLBGMReferenceNumber with a 1-digit year", () => Fountains.PLBGMReferenceNumber("XXX", "2"));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLBGMReferenceNumber with a non-numeric year", () => Fountains.PLBGMReferenceNumber("XXX", "XX"));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLBGMReferenceNumber with a 3-digit year", () => Fountains.PLBGMReferenceNumber("XXX", "202"));
			AssertExceptionThrown<ArgumentException>("Attempt to get PLBGMReferenceNumber with a 4-digit year", () => Fountains.PLBGMReferenceNumber("XXX", "2023"));
		});

		public void TestGetKey()
		{
			AssertEquals("GeneratorFountain-A-KEY1", Fountains.GetKey("A", "KEY1"));
			AssertEquals("GeneratorFountain-B-KEY2", Fountains.GetKey("B", "KEY2"));
			AssertEquals("GeneratorFountain-A-KEY1", Fountains.GetKey("A", "KEY1"));
			AssertEquals("GeneratorFountain-V-6D696968", Fountains.GetKey("V", new string('x', 50)));
			AssertExceptionThrown<ArgumentNullException>(() => Fountains.GetKey("V", null));
		}

		public void TestBRLicenseMessageBatchNumber()
		{
			AssertEquals("000001", Fountains.BRLicenseMessageBatchNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("000002", Fountains.BRLicenseMessageBatchNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("000003", Fountains.BRLicenseMessageBatchNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestBRLicenseEntryNumber()
		{
			AssertEquals("LI0000001", Fountains.BRLicenseEntryNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("LI0000002", Fountains.BRLicenseEntryNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("LI0000003", Fountains.BRLicenseEntryNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestBELocalReferenceNumber()
		{
			AssertNotNull(Fountains.BELocalReferenceNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{9}"), Fountains.BELocalReferenceNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestNLLocalReferenceNumber()
		{
			AssertNotNull(Fountains.NLLocalReferenceNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{9}"), Fountains.NLLocalReferenceNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestEULocalReferenceNumber()
		{
			AssertNotNull(Fountains.EULocalReferenceNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{9}"), Fountains.EULocalReferenceNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestPNTSLocalReferenceNumber()
		{
			AssertNotNull(Fountains.PNTSLocalReferenceNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{9}"), Fountains.PNTSLocalReferenceNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}
		public void TestG3LocalReferenceNumber()
		{
			AssertNotNull(Fountains.G3LocalReferenceNumber(Guid.NewGuid()));
			AssertMatch(new Regex(@"\d{9}"), Fountains.G3LocalReferenceNumber(Guid.NewGuid()).GetNextFormatted(Connection, Transaction));
		}

		public void TestCHTraderDeclarationNumber()
		{
			AssertNotNull(Fountains.GetCHTraderDeclarationNumber(new Guid(), "prefix"));
			var companyPK = new Guid();
			AssertNotNull(Fountains.GetCHTraderDeclarationNumber(companyPK, "prefix"));
			AssertEquals("prefix000000001", Fountains.GetCHTraderDeclarationNumber(companyPK, "prefix").GetNextFormatted(Connection, Transaction));
			AssertEquals("prefix000000002", Fountains.GetCHTraderDeclarationNumber(companyPK, "prefix").GetNextFormatted(Connection, Transaction));
			AssertEquals("prefix000000001", Fountains.GetCHTraderDeclarationNumber(Guid.NewGuid(), "prefix").GetNextFormatted(Connection, Transaction));
		}

		public void TestProfitShareRedistributionNumber()
		{
			AssertEquals("PSR00000001", Fountains.ProfitShareRedistributionNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("PSR00000002", Fountains.ProfitShareRedistributionNumber.GetNextFormatted(Connection, Transaction));
			AssertEquals("PSR00000003", Fountains.ProfitShareRedistributionNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestTSDJobReference()
		{
			AssertEquals("TSD0000001", Fountains.TSDJobReference.GetNextFormatted(Connection, Transaction));
		}

		public void TestBECustomsRegistryNumberFountain()
		{
			AssertNotNull(Fountains.BECustomsRegistryNumberFountain(Guid.NewGuid(), Guid.NewGuid(), string.Empty, string.Empty, 1));

			var fountain = Fountains.BECustomsRegistryNumberFountain(Guid.NewGuid(), Guid.NewGuid(), string.Empty, string.Empty, 20);
			AssertEquals("20", fountain.GetNextFormatted(Connection, Transaction).TrimStart('0'));
			AssertEquals("21", fountain.GetNextFormatted(Connection, Transaction).TrimStart('0'));
		}

		public void TestBRCatalogNumber()
		{
			AssertEquals("00000001", Fountains.BRCatalogCode.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetBRCatalogNumberGeneratorFountain()
		{
			var fountain1 = Fountains.GetBRCatalogCodeGeneratorFountain("KEY1");
			var fountain2 = Fountains.GetBRCatalogCodeGeneratorFountain("KEY2");

			CombineAssertions(() =>
			{
				AssertEquals("Name of fountain1", "GeneratorFountain-CGC-KEY1", GetFactoryName((NonFormattedNumberFountain)fountain1));
				AssertEquals("Name of fountain2", "GeneratorFountain-CGC-KEY2", GetFactoryName((NonFormattedNumberFountain)fountain2));
				AssertEquals("Number #1 of fountain1", "00000001", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain1", "00000002", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #1 of fountain2", "00000001", fountain2.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain2", "00000002", fountain2.GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestBRLPCOJobNumber()
		{
			AssertEquals("00000001", Fountains.BRLPCOJobNumber.GetNextFormatted(Connection, Transaction));
		}

		public void TestGetBRLPCOJobNumberGeneratorFountain()
		{
			var fountain1 = Fountains.GetBRLPCOJobNumberGeneratorFountain("KEY1");
			var fountain2 = Fountains.GetBRLPCOJobNumberGeneratorFountain("KEY2");

			CombineAssertions(() =>
			{
				AssertEquals("Name of fountain1", "GeneratorFountain-LPC-KEY1", GetFactoryName((NonFormattedNumberFountain)fountain1));
				AssertEquals("Name of fountain2", "GeneratorFountain-LPC-KEY2", GetFactoryName((NonFormattedNumberFountain)fountain2));
				AssertEquals("Number #1 of fountain1", "00000001", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain1", "00000002", fountain1.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #1 of fountain2", "00000001", fountain2.GetNextFormatted(Connection, Transaction));
				AssertEquals("Number #2 of fountain2", "00000002", fountain2.GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestGetINCustomsMessageNumberFountain()
		{
			CombineAssertions(() =>
			{
				AssertEquals("KEY1, Number #1", "0000001", Fountains.INCustomsMessageNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY1, Number #2", "0000002", Fountains.INCustomsMessageNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY2, Number #1", "0000001", Fountains.INCustomsMessageNumberFountain("KEY2").GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestGetINLocalReferenceNumberFountain()
		{
			CombineAssertions(() =>
			{
				AssertEquals("KEY1, Number #1", "0000001", Fountains.INLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY1, Number #2", "0000002", Fountains.INLocalReferenceNumberFountain("KEY1").GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY2, Number #1", "0000001", Fountains.INLocalReferenceNumberFountain("KEY2").GetNextFormatted(Connection, Transaction));
			});
		}

		public void TestGetAECustomsNumberFountain()
		{
			CombineAssertions(() =>
			{
				AssertEquals("KEY1, Number #1", "0000001", Fountains.AECustomsNumberFountain("KEY1", 7).GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY1, Number #2", "00000002", Fountains.AECustomsNumberFountain("KEY1", 8).GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY2, Number #1", "000000001", Fountains.AECustomsNumberFountain("KEY2", 9).GetNextFormatted(Connection, Transaction));
				AssertEquals("KEY3, Number #1", "000000003", Fountains.AECustomsNumberFountain("KEY3", 9, 3).GetNextFormatted(Connection, Transaction));
			});
		}

		IDbConnection Connection
		{
			get { return ((IDbConnectionInternals)Db.Connection).ADOConnection; }
		}

		IDbTransaction Transaction
		{
			get { return ((IDbConnectionInternals)Db.Connection).ADOTransaction; }
		}

		#region Implementation

		string GetFactoryName(NonFormattedNumberFountain fountain)
		{
			return fountain.Name;
		}

		protected NumberFountains Fountains
		{
			get { return new NumberFountains(); }
		}

		#endregion
	}
}

#endif
#endregion
