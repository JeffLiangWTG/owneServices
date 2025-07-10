using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public static class RestoreTaxTransactionInIncompleteInvoiceHelper
	{
		public static void RestoreTaxTransactionFromTaxRecordData(InvoicingBase invoice)
		{
			if (invoice.Factory.TryGetValueFromCacheOnly(GetKeyForCachingTaxRecordsData(invoice.PK), out List<TaxRecordData> taxRecordDataList))
			{
				Rectify_TaxRecordData_TransactionLinePKs(invoice, taxRecordDataList);

				var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
				ObjectFactory.Get<ITaxProcessor>().RestoreFromTaxRecordData(taxRecordParent, taxRecordDataList);
				DeleteTaxRecordsDataFromInvoiceFactory(invoice);
			}
			else
			{
				throw new InvalidOperationException("There was no taxRecordData for the invoice in the factory cache for restoring tax transactions.");
			}
		}

		static void Rectify_TaxRecordData_TransactionLinePKs(InvoicingBase invoice, IReadOnlyList<TaxRecordData> taxRecordDataList)
		{
			if (taxRecordDataList != null)
			{
				var importedLinePKByApportionmentSplitChargePK = new Dictionary<ZGuid, ZGuid>();

				foreach (InvoicingLineBase line in invoice.Lines)
				{
					if (line.IsPopulatedFromImportedApportionment)
					{
						importedLinePKByApportionmentSplitChargePK.Add(line.ApportionmentChargeImportedFrom.PK, line.PK);
					}
				}

				var invoiceLinePKs = invoice.Lines.GetPKs();

				foreach (var taxRecordData in taxRecordDataList)
				{
					var rectifiedLinePKs = new List<ZGuid>();

					var lineOrAportionmentChargePKs = taxRecordData.TransactionLinePKs;
					foreach (var lineOrAportionmentChargePK in lineOrAportionmentChargePKs)
					{
						ZGuid rectifiedLinePK;
						if (importedLinePKByApportionmentSplitChargePK.TryGetValue(lineOrAportionmentChargePK, out var linePK))
						{
							rectifiedLinePK = linePK;
						}
						else if (invoiceLinePKs.Contains(lineOrAportionmentChargePK))
						{
							rectifiedLinePK = lineOrAportionmentChargePK;
						}
						else
						{
							throw new InvalidOperationException($"Could not find a line or apportionment split charge while trying to rectify TransactionLinePKs in taxRecordData during restore.");
						}

						rectifiedLinePKs.Add(rectifiedLinePK);
					}

					taxRecordData.TransactionLinePKs = rectifiedLinePKs;
				}
			}
		}

		public static void SaveTaxRecordsDataInInvoiceFactoryCache(InvoicingBase invoice, List<TaxRecordData> taxRecordsData)
		{
			if (!invoice.Factory.TryGetValueFromCacheOnly(GetKeyForCachingTaxRecordsData(invoice.PK), out List<TaxRecordData> existingTaxRecordsDataList))
			{
				invoice.Factory.GetCachedValue(GetKeyForCachingTaxRecordsData(invoice.PK), () => taxRecordsData);
			}
			else
			{
				throw new InvalidOperationException("Import incomplete invoice operation may have been attempted twice, which is not valid. TaxRecordData for the same key already exists in the invoice factory cache.");
			}
		}

		static void DeleteTaxRecordsDataFromInvoiceFactory(InvoicingBase invoice)
		{
			invoice.Factory.ClearCachedValue<List<TaxRecordData>>(GetKeyForCachingTaxRecordsData(invoice.PK));	
		}

		static ZString GetKeyForCachingTaxRecordsData(ZGuid pk) => "RestoredIncompleteInvoiceTaxRecordsData" + pk;
	}
}
