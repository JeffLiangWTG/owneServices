using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public static class TransactionHeaderHelper
	{
		public static TransactionHeader[] ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(TransactionHeader[] gridSelections)
		{
			List<TransactionHeader> transactionList = new List<TransactionHeader>(gridSelections);
			var secondTransfers = from x in gridSelections
								  where x.AH_TransactionType == TransactionTypes.Transfer && x.AH_TransactionCount == 2
								  && (x.AH_Ledger == LedgerTypes.AccountsPayable || x.AH_Ledger == LedgerTypes.AccountsReceivable)
								  select x;
			foreach (var secondTransfer in secondTransfers)
			{
				if (secondTransfer.RelatedTransactions.Count > 0)
				{
					int index = transactionList.IndexOf(secondTransfer);
					TransactionHeader firstTransfer = secondTransfer.RelatedTransactions[0];
					transactionList.Remove(secondTransfer);
					if (!transactionList.Contains(firstTransfer))
					{
						transactionList.Insert(index, firstTransfer);
					}
				}
			}
			TransactionHeader[] transactions = transactionList.ToArray<TransactionHeader>();
			return transactions;
		}

		public static MultilingualString GetCreditTerms(string transactionType, string companyCountryCode, string invoiceTerm, string invoiceTermDays, ZDateTime invoiceDate, ZDateTime invoiceDueDate, bool isExport = false, BaseJobDeclaration declaration = null, CommonCartage cartage = null)
		{
			var result = (NoResString)string.Empty;

			if (transactionType != TransactionTypes.CreditNote)
			{
				var termList = new InvoiceTermsListWithShortDescription();

				if (!string.IsNullOrEmpty(invoiceTerm))
				{
					if (invoiceTerm == Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate
						&& int.TryParse(invoiceTermDays, out var termDays)
						&& companyCountryCode != Core.Constants.CountryCodes.Portugal)
					{
						if (invoiceDate.AddDays(termDays) == invoiceDueDate)
						{
							result = (NoResString)(invoiceTermDays + " " + Res.GetString("ec96c03d-c7ac-477e-9a3e-ea52e7ee167e", "days from Inv. Date"));
						}
						else
						{
							result = (NoResString)(invoiceTermDays + " " + Res.GetString("4bbf7459-bdc2-4b41-9162-75339337c2ce", "days from shipment"));
						}
					}
					else if (invoiceTerm == Constants.InvoiceTerms.FromDeliveryOrPickupDate)
					{
						if (declaration != null)
						{
							if (declaration.IsExportOrNonTransport)
							{
								if (companyCountryCode == Core.Constants.CountryCodes.Brazil && declaration.IsNonTransportDeclarationType)
								{
									result = (NoResString)(invoiceTermDays + " " + Res.GetString("D45F2AB7-01FD-47D6-8D40-F57B85A54DE2", "days from delivery date"));
								}
								else
								{
									result = (NoResString)(invoiceTermDays + " " + Res.GetString("C30C0855-9289-434E-BAB2-E060447A457B", "days from pickup date"));
								}
							}
							else
							{
								result = (NoResString)(invoiceTermDays + " " + Res.GetString("1210B375-B133-4282-838E-3E335C75BBC7", "days from delivery date"));
							}
						}
						else
						{
							if (cartage != null && cartage.Job != null)
							{
								var job = cartage.Job as Job;
								if (job.JobType.Code == JobInvoicingConsumerTypes.LocalCartageCode)
								{
									if (cartage.JJ_A_JCL.IsValid)
									{
										result = (NoResString)(invoiceTermDays + " " + Res.GetString("F3984219-711E-4596-9087-144BA461DBA6", "days from completion date"));
									}
									else
									{
										result = (NoResString)(invoiceTermDays + " " + Res.GetString("B45EC8B1-8EDC-49F5-8B55-D14D6FB577B4", "days from delivery date"));
									}
								}
							}
							else
							{
								if (isExport)
								{
									result = (NoResString)(invoiceTermDays + " " + Res.GetString("A37DFA12-B871-48D5-9851-66D2DA0F9EC6", "days from pickup date"));
								}
								else
								{
									result = (NoResString)(invoiceTermDays + " " + Res.GetString("174D2122-72DF-4A66-9619-5EB88C997A39", "days from delivery date"));
								}
							}
						}
					}
					else
					{
						var invoiceTermDescription = companyCountryCode == Core.Constants.CountryCodes.Portugal
						? (NoResString)termList.GetMultilingualDescriptionFromCode(invoiceTerm).GetUnresolvedString()
						: termList.GetMultilingualDescriptionFromCode(invoiceTerm);

						if (invoiceTermDescription != null)
						{
							result = (NoResString)string.Format(invoiceTermDescription.ToString().Trim(), invoiceTermDays);
						}
						else
						{
							result = (NoResString)(invoiceTermDays + " " + Res.GetString("b1c7e262-41a1-4e27-872c-70b8b6b05301", "days") + " ");
							ErrorReporter.ReportOnce(Res.GetString("C9499A51-60B6-4ef2-89AE-67CC7DCDEBFB", "Can not determine description from invoice term code '{0}'.", invoiceTerm));
						}
					}
				}
				else
				{
					var invoiceTermDescription = companyCountryCode == Core.Constants.CountryCodes.Portugal
						? (NoResString)termList.GetMultilingualDescriptionFromCode(InvoiceTermWithShortDescription.CashOnDelivery.Code).GetUnresolvedString()
						: termList.GetMultilingualDescriptionFromCode(InvoiceTermWithShortDescription.CashOnDelivery.Code);

					if (invoiceTermDescription != null)
					{
						result = (NoResString)invoiceTermDescription.ToString().Trim();
					}
				}
			}
			return result;
		}

		public static bool CheckHasGeneratedComplianceDocumentBeforeReverse(IReversing reversingTransaction) => reversingTransaction != null
			&& reversingTransaction is InvoicingBase invoice
			&& (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote)
			&& !(invoice is IBadDebtWritingOff badDebtWritingOff && badDebtWritingOff.IsWritingOff)
			&& invoice.GetTransactionGeneratedComplianceDocument() != null;

		public static ZString HasGeneratedComplianceDocumentErrorMessage => Res.GetString("AB3B4B29-8452-4A53-9D57-2A929EB8D012", "You cannot reverse an INV or CRD that is linked to a compliance document record. You need to void all compliance document records related to the transaction before proceeding to reverse.");
	}
}
