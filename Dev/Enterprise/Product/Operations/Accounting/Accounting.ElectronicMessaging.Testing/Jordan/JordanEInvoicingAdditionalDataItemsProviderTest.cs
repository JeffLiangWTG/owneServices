using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Jordan
{
	public class JordanEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDataItemsOfInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Jordan))
			{
				var debtor = TestObjectCreator.Debtor;
				debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("JO-AR-1", Currency, 10m, debtor);
				Factory.Save();

				var expectedAdditionalDataItems = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection
				{
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "UUID", Value = invoice.PK.ToString(),
					},
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "DebtorCategory", Value = debtor.OH_Category,
					},
				};

				AssertAdditionalDataItems(invoice.PK, expectedAdditionalDataItems);
			}
		}

		public void TestAdditionalDataItemsOfCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Jordan))
			{
				var debtor = TestObjectCreator.Debtor;
				debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("JO-AR-1", Currency, 10m, debtor);
				var creditNote = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice);
				creditNote.AH_OH = debtor.PK;
				Factory.Save();

				var expectedAdditionalDataItems = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection
				{
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "UUID", Value = creditNote.PK.ToString(),
					},
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "DebtorCategory", Value = debtor.OH_Category,
					},
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigUUID", Value = invoice.PK.ToString(),
					},
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigTotalOSAmt", Value = invoice.AH_OSTotalAmount.ToStringTrimZeros(),
					},
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigTotalLocalAmt", Value = invoice.AH_LocalTotalAmount.ToStringTrimZeros(),
					}
				};

				AssertAdditionalDataItems(creditNote.PK, expectedAdditionalDataItems);
			}
		}

		void AssertAdditionalDataItems(ZGuid transactionPK, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection expectedAdditionalDataItems)
		{
			var (branch, batch, countryFactory, accEInvoiceBatch, warnings) = SetDataForTest();
			var pivot = accEInvoiceBatch.TransactionPivots.AddNew();
			pivot.SetCompanyAndCountryCode(accEInvoiceBatch.Company);
			pivot.AIP_ParentID = transactionPK;

			var additionalDataItemProvider = new JordanEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var actualAdditionalDataItems = additionalDataItemProvider.GetAdditionalHeaderDataItems(accEInvoiceBatch, branch, batch, countryFactory, warnings);

			AssertEquals("Actual number of AdditionalDataItem doesn't match expectation", expectedAdditionalDataItems.Count, actualAdditionalDataItems.Count);
			CombineAssertions(() =>
			{
				for (var i = 0; i < expectedAdditionalDataItems.Count; i++)
				{
					AssertEquals("AdditionalDataItem.Key", expectedAdditionalDataItems[i].Key, actualAdditionalDataItems[i].Key);
					AssertEquals("AdditionalDataItem.Value", expectedAdditionalDataItems[i].Value, actualAdditionalDataItems[i].Value);
				}
			});
		}

		(GlbBranch branch, UniversalTransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory, AccEInvoicingBatch accEInvoiceBatch, INotifications warnings) SetDataForTest()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch { Code = branch.GB_Code }
			});

			return (branch, batch, GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Jordan), accBatch, new Logger());
		}

		RefCurrency Currency => currency ??= RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Jordan);
		RefCurrency currency;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
