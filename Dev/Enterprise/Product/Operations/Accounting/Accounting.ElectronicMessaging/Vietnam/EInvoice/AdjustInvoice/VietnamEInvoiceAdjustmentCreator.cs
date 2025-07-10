using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Newtonsoft.Json;
using static Enterprise.Accounting.ElectronicMessaging.Vietnam.VietnamEInvoiceHelper;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice
{
	public class VietnamEInvoiceAdjustmentCreator
	{
		public VietnamEInvoiceAdjustmentCreator(UniversalTransactionInfo transaction, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			this.Transaction = transaction;
			this.AdditionalTransactionInfo = additionalTransactionInfo;
		}

		readonly UniversalTransactionInfo Transaction;
		readonly AdditionalTransactionInfoForVietnamEInvoice AdditionalTransactionInfo;

		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoiceAdjustment Create()
		{
			var shipment = VietnamEInvoiceHelper.GetShipment(Transaction, AdditionalTransactionInfo.IsConsolInvoice, AdditionalTransactionInfo.TransactionParentID);

			var companyPK = AdditionalTransactionInfo.CompanyPK.ToGuid();
			var branchPK = AdditionalTransactionInfo.BranchPK.ToGuid();

			var additionalInfo = VietnamEInvoiceHelper.GetAdditionalInfo(AdditionalTransactionInfo, Transaction, shipment);
			var form = VietnamEInvoiceHelper.GetForm(AdditionalTransactionInfo);
			var sequenceNumber = AdditionalTransactionInfo.SequenceNumber;
			var originalSeriesPrefix = AdditionalTransactionInfo.ComplianceSequenceInfo.OriginalSeriesPrefix;
			var originalSequenceNumber = AdditionalTransactionInfo.OriginalSequenceNumber;

			var currencyAffectedHeaderData = new CurrencyAffectedHeaderData(Transaction, Multiplier, UseLocalCurrency);

			var eInvoice = new VietnamEInvoiceAdjustment()
			{
				User = new User() { Username = "", Password = "" },
				Language = AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty),
				Inv =
				{
					Adj = new Adj()
					{
						Rdt = AdditionalTransactionInfo.RectifyDate,
						Rea = AdditionalTransactionInfo.RectifyReason,
						Ref = AdditionalTransactionInfo.RectifySupportingDocumentNumber,
						Seq = form + "-" + originalSeriesPrefix + "-" + originalSequenceNumber,
					},
					Aun = 1,
					Sid = AdditionalTransactionInfo.TransactionPK.ToString(),
					Idt = AdditionalTransactionInfo.ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"),
					Type = VietnamEInvoiceHelper.EInvoiceType,
					Form = form,
					Serial = AdditionalTransactionInfo.ComplianceSequenceInfo.SeriesPrefix,
					Ud = Transaction.TransactionType == TransactionType.CRD ? 0 : 1,
					Seq = sequenceNumber,
					Bname = Transaction.OrganizationAddress.CompanyName,
					Buyer = AdditionalTransactionInfo.ContactInfo?.Name ?? ZString.Empty,
					Btax = AdditionalTransactionInfo.VATRegistrationNum,
					Baddr = AdditionalTransactionInfo.FormattedAddress,
					Btel = AdditionalTransactionInfo.ContactInfo?.Phone ?? ZString.Empty,
					Bmail = VietnamEInvoiceHelper.GetContactEmails(AdditionalTransactionInfo.ContactInfo),
					Paym = VietnamEInvoiceHelper.PaymentMethod,
					Curr = currencyAffectedHeaderData.Curr,
					Exrt = currencyAffectedHeaderData.Exrt,
					Bacc = AdditionalTransactionInfo.AccountNumber,
					Bbank = AdditionalTransactionInfo.BankName,
					Note = Transaction.Description,
					Sumv = Transaction.LocalExVATAmount.Value * Multiplier,
					Sum = currencyAffectedHeaderData.Sum,
					Vatv = Transaction.LocalVATAmount.Value * Multiplier,
					Vat = currencyAffectedHeaderData.Vat,
					Word = VietnamEInvoiceHelper.GetCurrencyToWord(companyPK, branchPK, Transaction.OSCurrency.Code, (double)Transaction.OSTotal.Value * Multiplier, (double)Transaction.LocalTotal.Value * Multiplier),
					Totalv = Transaction.LocalTotal.Value * Multiplier,
					Discount = string.Empty,
					Total = currencyAffectedHeaderData.Total,
					SendType = AccountingConfigurationRegistry.Instance.EInvoicingSendType.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty) ? 2 : 1,
					Stax = VietnamEInvoiceHelper.GetVietnamRegistrationNumber(companyPK, branchPK),
					DueDate = Transaction.DueDate?.ToShortDateString() ?? string.Empty,
					Consignor = VietnamEInvoiceHelper.GetConsignor(AdditionalTransactionInfo.IsDeclarationInvoice, shipment),
					Reference = VietnamEInvoiceHelper.GetReference(Transaction),
					Consignee = VietnamEInvoiceHelper.GetConsignee(AdditionalTransactionInfo.IsDeclarationInvoice, shipment),
					ETD = AdditionalTransactionInfo.ETD,
					VoyageFlightJourneyDetails = AdditionalTransactionInfo.TransportInfo,
					ETA = AdditionalTransactionInfo.ETA,
					MasterBill = AdditionalTransactionInfo.MasterBillNumber,
					HouseBill = VietnamEInvoiceHelper.GetHouseBill(AdditionalTransactionInfo, shipment.WayBillType, shipment.WayBillNumber),
					AdditionalInfoC9 = JsonConvert.SerializeObject(additionalInfo.additionalInfoC9),
					AdditionalInfoC10 = JsonConvert.SerializeObject(additionalInfo.additionalInfoC10),
					AdditionalInfoC11 = JsonConvert.SerializeObject(additionalInfo.additionalInfoC11),
					AdditionalInfoC12 = JsonConvert.SerializeObject(additionalInfo.additionalInfoC12),
					AdditionalInfoC13 = JsonConvert.SerializeObject(additionalInfo.additionalInfoC13),
				}
			};

			var additionalValidation = new VietnamComplianceInfoEInvoicingExtension();

			if (additionalValidation.IsCircular78(AdditionalTransactionInfo.ComplianceSequenceInfo?.ComplianceSequenceMaximumNumberDigits ?? 0))
			{
				eInvoice.Inv.TypeRef = 1;
			}

			eInvoice.Inv.Items.AddRange(GetItem(companyPK, branchPK));

			return eInvoice;
		}

		IEnumerable<AdjustItem> GetItem(Guid companyPK, Guid branchPK)
		{
			var linesWithoutCMTCharge = Transaction.PostingJournalCollection.Where(x => (x.ChargeCode?.ChargeType.Code ?? ZString.Empty) != Constants.ChargeType.Comment).ToList();
			var unit = AccountingConfigurationRegistry.Instance.UnitMeasurementTextOverride.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty).Cast<UnitMeasurementTextOverride>().FirstOrDefault(x => x.UnitMeasurement == "Unit");

			foreach (var line in linesWithoutCMTCharge)
			{
				var currencyAffectedLineData = new CurrencyAffectedLineData(line, Multiplier, UseLocalCurrency);

				var item = new AdjustItem()
				{
					Type = Transaction.TransactionType == TransactionType.CRD ? CreditNoteType : AdjustmentType,
					VRT = VietnamEInvoiceHelper.CalculateTaxRate(line),
					Name = line.Description.Value.Replace("\r\n", " "),
					Status = Transaction.TransactionType == TransactionType.CRD ? 0 : 1,
					Amount = currencyAffectedLineData.Amount,
					VAT = currencyAffectedLineData.Vat,
					Total = currencyAffectedLineData.Total,
					Price = currencyAffectedLineData.Amount,
					LocalTax = line.LocalGSTVATAmount.Value * Multiplier,
					LocalAmount = line.LocalAmount.Value * Multiplier,
					LocalTotal = line.LocalTotalAmount.Value * Multiplier,
					CustomizedLocalTax = ((ZDecimal)(line.LocalGSTVATAmount.Value * Multiplier)).ToString(2),
					CustomizedLocalAmount = ((ZDecimal)(line.LocalAmount.Value * Multiplier)).ToString(2),
					CustomizedLocalTotal = ((ZDecimal)(line.LocalTotalAmount.Value * Multiplier)).ToString(2),
					HouseBill = AdditionalTransactionInfo.TransactionLineHouseBillDictionary != null
						? AdditionalTransactionInfo.TransactionLineHouseBillDictionary.GetValueSafe(line.Job.Key ?? ZString.Empty)
						: null,
					Unit = unit == null ? "Unit" : unit.TextOverride.ToString(), // hard coded value
					Quantity = 1,
				};

				yield return item;
			}
		}

		int Multiplier => Transaction.TransactionType == TransactionType.CRD ? -1 : 1;

		bool UseLocalCurrency => AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.GetFallBackValueAtAllLevels(AdditionalTransactionInfo.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

		#endregion
	}
}
