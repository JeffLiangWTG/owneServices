using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	public class SaudiArabiaEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDataItems()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = branch.PK;
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_AIB = accBatch.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = EInvoicingPivotState.Queued;
			pivot.SetCompanyAndCountryCode(accBatch.Company);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			invoice.AH_OH = debtor.PK;
			debtor.OH_RL_NKClosestPort = "AUSYD";
			var expectedCode = debtor.CustomsCodes.AddNew("ABN", "primarycode", "AU");

			var expectedAdditionalDataItems = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection()
			{
				new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = "UUID",
					Value = invoice.PK.ToString(),
				},
				new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
				{
					Key = "InvoiceTaxDate",
					Value = invoice.InvoiceTaxDate.ToString("yyyy-MM-dd"),
				},
				new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
				{
					Key = "OtherBuyerID",
					Value = expectedCode.SecuredCustomsRegNo,
				}
			};

			var additionalDataItemProvider = new SaudiArabiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("Number Of Elements", expectedAdditionalDataItems.Count, actualAdditionalDataItems.Count);
			for (var i = 0; i < expectedAdditionalDataItems.Count; i++)
			{
				AssertEquals("AdditionalDataItem.Key", expectedAdditionalDataItems[i].Key, actualAdditionalDataItems[i].Key);
				AssertEquals("AdditionalDataItem.Value", expectedAdditionalDataItems[i].Value, actualAdditionalDataItems[i].Value);
			}
		}

		public void TestOtherBuyerID()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = branch.PK;
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_AIB = accBatch.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = EInvoicingPivotState.Queued;
			pivot.SetCompanyAndCountryCode(accBatch.Company);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			invoice.AH_OH = debtor.PK;
			debtor.OH_RL_NKClosestPort = "AUSYD";
			var codeGCR = debtor.CustomsCodes.AddNew("GCR", "subcode", "AU");

			var additionalDataItemProvider = new SaudiArabiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("GCR should be used if only GCR appear", codeGCR.SecuredCustomsRegNo, actualAdditionalDataItems[2].Value);
			AssertEquals("If valid code is used, AddtionalItems List has 3 items", 3, actualAdditionalDataItems.Count);

			var codeABN = debtor.CustomsCodes.AddNew("ABN", "primarycode", "AU");
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("ABN must be choosen over GCR", codeABN.SecuredCustomsRegNo, actualAdditionalDataItems[2].Value);
			AssertEquals("If valid code is used, AddtionalItems List has 3 items", 3, actualAdditionalDataItems.Count);

			debtor.CustomsCodes.RemoveAll();
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("When no code added, AddtionalItems List has only 2 items", 2, actualAdditionalDataItems.Count);

			debtor.CustomsCodes.AddNew("VAT", "BRCode", "BR");
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("If config country code different from debtor country code, AddtionalItems List has only 2 items", 2, actualAdditionalDataItems.Count);

			debtor.OH_RL_NKClosestPort = "SA111";
			debtor.CustomsCodes.AddNew("ABN", "validcode", "AU");
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("If debtor country is domestic in SA, AddtionalItems List has only 2 items", 2, actualAdditionalDataItems.Count);
		}

		public void TestOtherBuyerIDWithSpecialCases()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = branch.PK;
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_AIB = accBatch.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = EInvoicingPivotState.Queued;
			pivot.SetCompanyAndCountryCode(accBatch.Company);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			invoice.AH_OH = debtor.PK;
			debtor.OH_RL_NKClosestPort = "AUSYD";
			debtor.CustomsCodes.AddNew("GCR", "34.367.714/0001-54", "AU");

			var additionalDataItemProvider = new SaudiArabiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("Non alpha-numeric characters removed", "34367714000154", actualAdditionalDataItems[2].Value);
			AssertEquals("AddtionalItems List has 3 items", 3, actualAdditionalDataItems.Count);

			debtor.CustomsCodes.AddNew("ABN", "!primardéjàvu", "AU");
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("Diacritics to be normalized", "primardejavu", actualAdditionalDataItems[2].Value);
			AssertEquals("AddtionalItems List has 3 items", 3, actualAdditionalDataItems.Count);

			debtor.OH_RL_NKClosestPort = "US123";
			debtor.OH_Code = "YOUORG_US";
			actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accBatch, branch, batch, countryFactoryMock.Object, new Common.Logger());
			AssertEquals("Org Proxy must be choosen if the country is not registered", "YOUORGUS", actualAdditionalDataItems[2].Value);
			AssertEquals("When no registration for the country, the OrgProxy added, AddtionalItems List has 3 items", 3, actualAdditionalDataItems.Count);
		}
	}
}
