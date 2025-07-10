using System;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class HungaryPayloadWriter : TransactionBatchToXmlWriter
	{
		protected override void WriteDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			if (!messageType.EqualsIgnoringCase(HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest))
			{
				throw new ArgumentException($"Invalid Message Type: {messageType}.");
			}

			var invoice = accBatch.TransactionPivots[0].ParentTransactionHeader as InvoicingBase;
			UpdateTransactionInfo(transactionInfo, invoice, out bool shouldIncludeOriginalPostDate);

			var hungaryExtraInfo = GetExtraInfo(invoice, shouldIncludeOriginalPostDate);
			var modelNotifications = new Common.Logger();

			var model = new ManageInvoiceRequestModel(transactionInfo, hungaryExtraInfo, accBatch.AIB_BatchNumber.ToString(), modelNotifications);
			model.LoadData(new BusinessObjectFactory());

			if (!modelNotifications.HasErrors)
			{
				var xmlElements = new ManageInvoiceRequestXmlBuilder(model).GetXML().ToXElements();
				foreach (var xml in xmlElements)
				{
					xml.WriteTo(writer);
				}
			}
		}

		void UpdateTransactionInfo(TransactionInfo transactionInfo, InvoicingBase invoice, out bool shouldIncludeOriginalPostDate)
		{
			shouldIncludeOriginalPostDate = false;
			if (invoice == null)
			{
				return;
			}

			if (transactionInfo.PostingJournalCollection.Count != 0)
			{
				var linesWithTaxDate = invoice.Lines.OfType<InvoicingLineBase>().Where(l => l.AL_TaxDate.IsValid);
				ZDate? latestTaxDate = linesWithTaxDate.Any() ? linesWithTaxDate.Max(l => l.AL_TaxDate) : null;
				transactionInfo.PostingJournalCollection[0].TaxDate = latestTaxDate;
			}

			if (invoice.OriginalReferenceTransaction != null
				&& transactionInfo.OriginalReference != null
				&& transactionInfo.OriginalReference.OriginalTransactionNumber.HasValue
				&& transactionInfo.OriginalReference.OriginalTransactionNumber.Value.EqualsIgnoringCase(invoice.OriginalReferenceTransaction.AH_TransactionNum))
			{
				if (transactionInfo.OriginalReference.OriginalTransactionDate.HasValue
					&& transactionInfo.OriginalReference.OriginalTransactionDate.Value.IsValid)
				{
					transactionInfo.OriginalReference.OriginalTransactionDate = invoice.OriginalReferenceTransaction.AH_InvoiceDate;
				}
				shouldIncludeOriginalPostDate = true;
			}
		}

		HungaryTransactionExtraInfo GetExtraInfo(InvoicingBase invoice, bool shouldIncludeOriginalPostDate)
		{
			var extraInfo = new HungaryTransactionExtraInfo();
			if (invoice != null)
			{
				var idmCusCode = invoice.Header.CustomsCodes.GetCustomsRegNo(HungaryOrgCusCodeInfo.OrgCusCodes.IDM, Constants.CountryCodes.Hungary);

				if (!idmCusCode.IsEmpty)
				{
					InvoiceDeliveryMethod parsedIDM;

					if (!Enum.TryParse(idmCusCode, out parsedIDM) // Becomes true if idmCusCode is an invalid string value 
						|| !Enum.IsDefined(typeof(InvoiceDeliveryMethod), parsedIDM)) // Becomes true if idmCusCode is an invalid integer value
					{
						throw new ArgumentException($"Invalid Invoice Delivery Method {idmCusCode} is specified.");
					}

					extraInfo.InvoiceDeliveryMethod = parsedIDM;
				}

				extraInfo.IsDebtorPrivatePerson = invoice.Header != null && invoice.Header.OH_Category == OrgConstants.Category.NaturalPersonIndividual;
				if (shouldIncludeOriginalPostDate)
				{
					extraInfo.OriginalTransactionPostDate = invoice.OriginalReferenceTransaction?.AH_InvoiceDate;
				}
			}
			return extraInfo;
		}
	}
}
