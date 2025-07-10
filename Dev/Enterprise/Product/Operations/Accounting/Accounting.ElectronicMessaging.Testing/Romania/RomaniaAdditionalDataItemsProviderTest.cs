using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	public sealed class RomaniaAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestGetRegistrationNumberForForeignBuyers_Romania()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_RN_NKCountryCode = "RO";
			port.RL_Code = "RODEM";

			var debtor = TestObjectCreator.CreateOrgHeader("DEMORO", true, true);
			debtor.OH_RL_NKClosestPort = port.RL_Code;

			var address = TestObjectCreator.CreateAddress(debtor);
			address.OA_RN_NKCountryCode = "RO";

			AddCustomsCodeForTest(debtor, "RO", "123456789", "TVA");
			AddCustomsCodeForTest(debtor, "RO", "111222333", "GCR");

			Factory.Save();

			AssertRegistrationNumber(debtor.PK, address.PK, null);
		}

		public void TestGetRegistrationNumberForForeignBuyers_NotRomania()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_RN_NKCountryCode = "AU";
			port.RL_Code = "AUDEM";

			var debtor = TestObjectCreator.CreateOrgHeader("DEMOAU", true, true);
			debtor.OH_RL_NKClosestPort = port.RL_Code;

			var address = TestObjectCreator.CreateAddress(debtor);
			address.OA_RN_NKCountryCode = "AU";

			AddCustomsCodeForTest(debtor, "AU", "111222333", "GCR");
			AddCustomsCodeForTest(debtor, "AU", "321654", "ABN");

			Factory.Save();

			AssertRegistrationNumber(debtor.PK, address.PK, "321654");
		}

		public void TestGetRegistrationNumberForForeignBuyers_FallbackToOrgCode()
		{
			var debtor = TestObjectCreator.CreateOrgHeader("DEMOUS", true, true);

			var address = TestObjectCreator.CreateAddress(debtor);
			address.OA_RN_NKCountryCode = "US";
			AssertRegistrationNumber(debtor.PK, address.PK, debtor.OH_Code);
		}

		public void TestGetEInvoicingNumber_WithoutAuthorisationRecord()
		{
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			var invoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var additionalInfoProvider = new RomaniaAdditionalDataItemsProvider();
			var additionalInfos = additionalInfoProvider.GetAdditionalHeaderDataItems(invoicingBatch, TestObjectCreator.NonCurrentBranch, transactionBatch, new RomaniaEInvoicingObjectFactory(), new Logger());
			var eInvoicingNumber = additionalInfos.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().FirstOrDefault(x => x.Key == "EINV_Number")?.Value;
			AssertNull(eInvoicingNumber);
		}

		public void TestGetEInvoicingNumber_WithAuthorisationRecord_GEN()
		{
			AssertEInvoicingNumber(EInvoicingPivotState.Delivered, "1122334455", "1122334455");
		}

		public void TestGetEInvoicingNumber_WithAuthorisationRecord_GEQ()
		{
			AssertEInvoicingNumber(EInvoicingPivotState.Sent, "1122334455", null);
		}

		void AssertEInvoicingNumber(string pivotStatus, string authorisationNumber, string expectEInvoicingNumber)
		{
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			var invoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.Company.GC_RN_NKCountryCode = "RO";

			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = "AH";
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_AIB = invoicingBatch.PK;
			pivot.AIP_Status = pivotStatus;

			var authorisationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorisationRecord.AHF_ParentTableCode = "AH";
			authorisationRecord.AHF_ParentId = invoice.PK;
			authorisationRecord.AHF_Number = authorisationNumber;
			authorisationRecord.AHF_RecordType = "ROA";

			var additionalInfoProvider = new RomaniaAdditionalDataItemsProvider();
			var additionalInfos = additionalInfoProvider.GetAdditionalHeaderDataItems(invoicingBatch, TestObjectCreator.NonCurrentBranch, transactionBatch, new RomaniaEInvoicingObjectFactory(), new Logger());
			var eInvoicingNumber = additionalInfos.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().FirstOrDefault(x => x.Key == "EINV_Number")?.Value;
			AssertEquals(expectEInvoicingNumber, eInvoicingNumber);
		}

		void AssertRegistrationNumber(ZGuid debtorPK, ZGuid invoiceAddressOverridePK, string expectRegistrationNumber)
		{
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = debtorPK;
			invoice.AH_OA_InvoiceAddressOverride = invoiceAddressOverridePK;

			var invoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			var transactionPivot = Factory.New<AccEInvoicingTransactionPivot>();
			transactionPivot.AIP_ParentID = invoice.PK;
			transactionPivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			transactionPivot.AIP_AIB = invoicingBatch.PK;

			var additionalInfoProvider = new RomaniaAdditionalDataItemsProvider();
			var additionalInfos = additionalInfoProvider.GetAdditionalHeaderDataItems(invoicingBatch, TestObjectCreator.NonCurrentBranch, transactionBatch, new RomaniaEInvoicingObjectFactory(), new Logger());
			var registrationNumber = additionalInfos.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().FirstOrDefault(x => x.Key == "OtherBuyerID")?.Value;
			AssertEquals(expectRegistrationNumber, registrationNumber);
		}

		OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType)
		{
			var taxCode = organisation.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = country;
			taxCode.OK_CodeType = codeType;
			taxCode.OK_CustomsRegNo = customsRegNo;
			return taxCode;
		}

		TestObjectCreator testObjectCreator;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
	}
}
