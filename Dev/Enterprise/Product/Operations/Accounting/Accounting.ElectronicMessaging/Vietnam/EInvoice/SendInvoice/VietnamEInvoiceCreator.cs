using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Newtonsoft.Json;
using static Enterprise.Accounting.ElectronicMessaging.Vietnam.VietnamEInvoiceHelper;
using static Enterprise.Core.Constants;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class VietnamEInvoiceCreator
	{
		public VietnamEInvoiceCreator(UniversalTransactionInfo transaction, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			this.Transaction = transaction;
			this.AdditionalTransactionInfo = additionalTransactionInfo;
		}

		readonly UniversalTransactionInfo Transaction;

		readonly AdditionalTransactionInfoForVietnamEInvoice AdditionalTransactionInfo;

		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoice Create()
		{
			if (Validate())
			{
				var shipment = VietnamEInvoiceHelper.GetShipment(Transaction, AdditionalTransactionInfo.IsConsolInvoice, AdditionalTransactionInfo.TransactionParentID);

				var companyPK = AdditionalTransactionInfo.CompanyPK.ToGuid();
				var branchPK = AdditionalTransactionInfo.BranchPK.ToGuid();

				var additionalInfo = VietnamEInvoiceHelper.GetAdditionalInfo(AdditionalTransactionInfo, Transaction, shipment);

				var currencyAffectedHeaderData = new CurrencyAffectedHeaderData(Transaction, Multiplier, UseLocalCurrency);

				var eInvoice = new VietnamEInvoice()
				{
					User = new User() { Username = "", Password = "" },
					Language = AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty),
					Inv =
					{
						Sid = AdditionalTransactionInfo.OriginalTransactionPK.ToString(),
						Idt = AdditionalTransactionInfo.ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"),
						Type = VietnamEInvoiceHelper.EInvoiceType,
						Form = VietnamEInvoiceHelper.GetForm(AdditionalTransactionInfo),
						Serial = AdditionalTransactionInfo.ComplianceSequenceInfo?.SeriesPrefix ?? ZString.Empty,
						Seq = AdditionalTransactionInfo.SequenceNumber,
						Aun = 1,
						Bcode = Transaction.OrganizationAddress.OrganizationCode.Value,
						Bname = Transaction.OrganizationAddress.CompanyName,
						Buyer = AdditionalTransactionInfo.ContactInfo?.Name ?? ZString.Empty,
						Btax = AdditionalTransactionInfo.VATRegistrationNum,
						Baddr = AdditionalTransactionInfo.FormattedAddress,
						Btel = AdditionalTransactionInfo.ContactInfo?.Phone ?? ZString.Empty,
						Bmail = VietnamEInvoiceHelper.GetContactEmails(AdditionalTransactionInfo.ContactInfo),
						Sendfile = 1,
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
					},
				};

				var additionalValidation = new VietnamComplianceInfoEInvoicingExtension();

				if (additionalValidation.IsCircular78(AdditionalTransactionInfo.ComplianceSequenceInfo?.ComplianceSequenceMaximumNumberDigits ?? 0))
				{
					eInvoice.Inv.TypeRef = 1;
				}

				eInvoice.Inv.Items.AddRange(GetItem(companyPK, branchPK));
				return eInvoice;
			}
			return null;
		}

		int Multiplier => (Transaction.TransactionType == TransactionType.CRD ? -1 : 1);

		bool UseLocalCurrency => AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.GetFallBackValueAtAllLevels(AdditionalTransactionInfo.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

		bool Validate()
		{
			return true;
		}

		IEnumerable<Item> GetItem(Guid companyPK, Guid branchPK)
		{
			var linesWithoutCMTCharge = Transaction.PostingJournalCollection.Where(x => (x.ChargeCode?.ChargeType.Code ?? ZString.Empty) != ChargeType.Comment).ToList();
			foreach (var line in linesWithoutCMTCharge)
			{
				var currencyAffectedLineData = new CurrencyAffectedLineData(line, Multiplier, UseLocalCurrency);

				var item = new Item()
				{
					Type = string.Empty,
					VRT = VietnamEInvoiceHelper.CalculateTaxRate(line),
					Name = line.Description.Value.Replace("\r\n", " "),
					Line = line.Sequence.Value,
					Amount = currencyAffectedLineData.Amount,
					VAT = currencyAffectedLineData.Vat,
					Total = currencyAffectedLineData.Total,
					LocalTax = line.LocalGSTVATAmount.Value * Multiplier,
					LocalAmount = line.LocalAmount.Value * Multiplier,
					LocalTotal = line.LocalTotalAmount.Value * Multiplier,
					CustomizedLocalTax = ((ZDecimal)(line.LocalGSTVATAmount.Value * Multiplier)).ToString(2),
					CustomizedLocalAmount = ((ZDecimal)(line.LocalAmount.Value * Multiplier)).ToString(2),
					CustomizedLocalTotal = ((ZDecimal)(line.LocalTotalAmount.Value * Multiplier)).ToString(2),
					HouseBill = AdditionalTransactionInfo.TransactionLineHouseBillDictionary != null
						? AdditionalTransactionInfo.TransactionLineHouseBillDictionary.GetValueSafe(line.Job.Key ?? ZString.Empty)
						: null,
				};

				var unit = VietnamEInvoiceHelper.unit;
				if (UseLocalCurrency && line.ChargeCurrency != null && line.LocalCurrency != null && line.ChargeCurrency.Code != line.LocalCurrency.Code)
				{
					item.Price = currencyAffectedLineData.Amount;
					item.Quantity = 1;
				}
				else
				{
					var ratingItem = VietnamEInvoiceHelper.GetRatingBasis(line.RatingBasisCollection, item.Amount, line.OSCurrency);
					unit = ratingItem.Unit;
					item.Price = ratingItem.Price;
					item.Quantity = ratingItem.Quantity;
				}
				item.Unit = GetOverrideUnitText(companyPK, branchPK, unit);

				yield return item;
			}
		}

		string GetOverrideUnitText(Guid companyPK, Guid branchPK, string unit)
		{
			var matchingOverride = AccountingConfigurationRegistry.Instance.UnitMeasurementTextOverride.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty).Cast<UnitMeasurementTextOverride>().FirstOrDefault(x => x.UnitMeasurement == unit);

			return matchingOverride == null ? unit : matchingOverride.TextOverride;
		}

		#endregion
	}
}
