using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public static class VietnamEInvoiceHelper
	{
		public static User GetUser(Guid companyPK, Guid branchPK)
		{
			var userName = AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			var passWord = AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);

			return new User { Username = userName, Password = passWord };
		}

		public static ZString GetForm(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			var result = AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.GetFallBackValueAtAllLevels(additionalTransactionInfo.CompanyPK.ToGuid(), additionalTransactionInfo.BranchPK.ToGuid(), Guid.Empty);

			if (new VietnamComplianceInfoEInvoicingExtension().IsCircular78(additionalTransactionInfo.ComplianceSequenceInfo?.ComplianceSequenceMaximumNumberDigits ?? 0))
			{
				result = "1";
			}

			return result;
		}

		public static string GetVietnamProxyOrgCityName(Guid companyPK, Guid branchPK)
		{
			var cityName = ZString.Empty;
			var factory = new ReadOnlyBusinessObjectFactory();
			var branch = factory.Load<GlbBranch>(branchPK);
			cityName = branch.OrgProxy?.CityName ?? ZString.Empty;

			if (string.IsNullOrEmpty(cityName))
			{
				var company = factory.Load<GlbCompany>(companyPK);
				cityName = company.OrgProxy?.CityName ?? ZString.Empty;
			}

			return cityName;
		}

		public static string GetVietnamRegistrationNumber(Guid companyPK, Guid branchPK)
		{
			var registrationNumber = "";
			var factory = new ReadOnlyBusinessObjectFactory();
			var branch = factory.Load<GlbBranch>(branchPK);
			registrationNumber = branch.OrgProxy?.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.VietNam).FirstOrDefault()?.OK_CustomsRegNo;

			if (string.IsNullOrEmpty(registrationNumber))
			{
				var company = factory.Load<GlbCompany>(companyPK);
				registrationNumber = company.OrgProxy.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.VietNam).FirstOrDefault()?.OK_CustomsRegNo;
			}

			return registrationNumber;
		}

		public static string GetCurrencyToWord(Guid companyPK, Guid branchPK, string osCurrencyCode, double osTotal, double localTotal)
		{
			var alwaysUseLocalCurrency = AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			var useOSCurrency = !alwaysUseLocalCurrency && AccountingConfigurationRegistry.Instance.VietnamExportAmountInWordsBasedOnInvoicedCurrency.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			var total = useOSCurrency ? osTotal : localTotal;
			var currencyCode = useOSCurrency ? osCurrencyCode ?? string.Empty : Constants.CurrencyCodes.VietNam;

			var currencyToWord = new CurrencyToWords_VI_VN().ConvertToWords(total, currencyCode);
			currencyToWord = currencyToWord.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + currencyToWord.Substring(1, currencyToWord.Length - 1);

			return currencyToWord;
		}

		public static AdditionalInfo GetAdditionalInfo(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo, UniversalTransactionInfo transaction, Shipment shipment)
		{
			var additionalInfo = new AdditionalInfo();

			additionalInfo.additionalInfoC9 = new AdditionalInfoC9()
			{
				Origin = additionalTransactionInfo.OriginPort,
				Destination = additionalTransactionInfo.DestinationPort,
				Incoterms = GetINCO(shipment.ShipmentIncoTerm),
				CreditTerms = GetCreditTerm(transaction.InvoiceTerm?.ToString() ?? string.Empty, transaction.InvoiceTermDays ?? 0),
				SellReference = transaction.CheckNumberOrPaymentRef ?? ZString.Empty,
				TransactionNumber = AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value
					? string.IsNullOrEmpty(transaction.JobInvoiceNumber)
						? transaction.Job?.Key ?? string.Empty
						: transaction.JobInvoiceNumber
					: additionalTransactionInfo.TransactionNumber,
				TransportMode = additionalTransactionInfo.TransportMode,
			};

			additionalInfo.additionalInfoC10 = new AdditionalInfoC10()
			{
				OwnerReference = shipment.OwnerRef ?? string.Empty,
				OrderReference = additionalTransactionInfo.OrderReference.SubstringSafe(0, 100),
				InvoiceDepartmentDesc = transaction.Department.Name ?? ZString.Empty,
				JobDepartmentDesc = additionalTransactionInfo.JobInfo?.JobDepartmentDesc ?? ZString.Empty,
				ExternalSystemDebtorCode = transaction.ExternalDebtorCode ?? ZString.Empty,
				GoodDescription = shipment.GoodsDescription ?? string.Empty,
				ConsolID = additionalTransactionInfo.ConsolID,
				Package = additionalTransactionInfo.Package,
			};

			additionalInfo.additionalInfoC11 = new AdditionalInfoC11()
			{
				Weight = additionalTransactionInfo.Weight,
				ChargeableWeight = additionalTransactionInfo.ChargeableWeight,
				Volume = additionalTransactionInfo.Volume,
				SendingAgent = additionalTransactionInfo.SendingAgent,
				ReceivingAgent = additionalTransactionInfo.ReceivingAgent,
				LineCarrier = additionalTransactionInfo.Carrier,
			};

			additionalInfo.additionalInfoC12 = new AdditionalInfoC12()
			{
				JobOperationStaff = (additionalTransactionInfo.JobInfo?.JobOperationStaff ?? ZString.Empty).SubstringSafe(0, 50),
				InvoiceCreatingStaff = (additionalTransactionInfo.InvoiceCreatingStaff).SubstringSafe(0, 50),
			};

			additionalInfo.additionalInfoC13 = new AdditionalInfoC13()
			{
				JobSalesStaff = (additionalTransactionInfo.JobInfo?.JobSalesStaff ?? ZString.Empty).SubstringSafe(0, 50),
			};

			return additionalInfo;
		}

		static string GetINCO(UniversalDataBuss.DataObjects.Universal.IncoTerm incoTerm)
		{
			if (incoTerm == null)
			{
				return string.Empty;
			}

			return incoTerm.Code + " - " + incoTerm.Description;
		}

		static string GetCreditTerm(string invoiceTerm, ZInt invoiceTermDays)
		{
			var result = (NoResString)ZString.Empty;
			var termList = new InvoiceTermsListWithShortDescription();

			if (string.IsNullOrEmpty(invoiceTerm))
			{
				var invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(InvoiceTermWithShortDescription.CashOnDelivery.Code);
				if (invoiceTermDescription != null)
				{
					result = (NoResString)invoiceTermDescription.ToString().Trim();
				}
			}
			else
			{
				var invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(invoiceTerm);
				if (string.IsNullOrEmpty(invoiceTermDescription))
				{
					result = (NoResString)(invoiceTermDays.ToString() + " " + Res.GetString("2C29CC73-AA85-4878-9C71-93B320363255", "days") + " "); // False alarm - no constants here
					ErrorReporter.ReportOnce(Res.GetString("5EEE4E26-58B6-4CC9-B2FA-D9DA1CE82D29", "Can not determine description from invoice term code '{0}'.", invoiceTerm));
				}
				else
				{
					result = (NoResString)string.Format(invoiceTermDescription.ToString().Trim(), invoiceTermDays.ToString());
				}
			}

			return result;
		}

		public static string GetReference(UniversalTransactionInfo transaction)
		{
			return string.IsNullOrEmpty(transaction.JobInvoiceNumber)
				? GetJobID(transaction.Job?.Key) ?? string.Empty
				: GetJobID(transaction.JobInvoiceNumber) ?? string.Empty;
		}

		static string GetJobID(string jobInvoiceNumber)
		{
			var result = ZString.Empty;

			if (jobInvoiceNumber != null)
			{
				var indexOfSuffix = jobInvoiceNumber.IndexOf("/");
				result = indexOfSuffix == -1 ? jobInvoiceNumber : jobInvoiceNumber.Substring(0, indexOfSuffix);
			}

			return result;
		}

		public static string GetHouseBill(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo, WayBillType wayBillType, string wayBillNumber)
		{
			var result = wayBillNumber ?? string.Empty;

			if (additionalTransactionInfo.IsDeclarationInvoice)
			{
				if (wayBillType == null || wayBillType.Code.GetValueOrDefault() != WayBillTypeList.Codes.House)
				{
					result = string.Empty;
				}
			}

			return result;
		}

		public static string CalculateTaxRate(PostingJournal journal)
		{
			var rate = journal.VATTaxID?.TaxRate?.ToString() ?? "-1";
			var code = journal.VATTaxID?.TaxCode?.ToString();

			if (code == NOTREPORT)
			{
				rate = "-1";
			}

			return rate;
		}

		public static RatingItem GetRatingBasis(List<RatingBasis> ratingBases, decimal amount, Currency transactionCurrency)
		{
			var ratingItem = new RatingItem();
			ratingItem.Unit = unit;
			ratingItem.Price = amount;
			ratingItem.Quantity = 1;

			if (!ratingBases.IsNullOrEmpty())
			{
				if (ratingBases.Count == 1 && ratingBases.First().MinimumRate <= 0)
				{
					var ratingBasis = ratingBases.First();
					if (ratingBasis.FlatRate > 0)
					{
						ratingItem.Unit = unt;
					}
					else if (ratingBasis.RateUnit != null)
					{
						switch (ratingBasis.RateUnit.Code.Value)
						{
							case percentage:
							case lowestBill:
							case one:
								break;
							case custom:
								ratingItem.Price = ratingBasis.PerUnitRate.Value;
								ratingItem.Quantity = ratingBasis.OriginQuantity.Value;
								break;
							default:
								if (ratingBasis.Currency.Code == transactionCurrency.Code)
								{
									ratingItem.Unit = ratingBasis.RateUnit.Code;
									ratingItem.Price = ratingBasis.PerUnitRate.Value;
									ratingItem.Quantity = ratingBasis.OriginQuantity.Value;
								}
								break;
						}
					}
				}
				else if (ratingBases.Count > 1)
				{
					if (ratingBases.First().RateUnit != null && ratingBases.AllSame(x => x.RateUnit?.Code) && ratingBases.AllSame(x => x.OriginQuantityUnit?.Code) && ratingBases.All(x => x.Currency.Code == transactionCurrency.Code))
					{
						ratingItem.Unit = ratingBases.First().RateUnit.Code;
						ratingItem.Price = ratingBases.First().PerUnitRate.Value;
						ratingItem.Quantity = ratingBases.Sum(x => x.OriginQuantity.Value);
					}
				}
			}

			return ratingItem;
		}

		public static Shipment GetShipment(UniversalTransactionInfo transaction, bool isConsolInvoice, ZString transactionParentID)
		{
			var result = (transaction.ShipmentCollection != null && transaction.ShipmentCollection.Count == 1) ? transaction.ShipmentCollection.First() : new Shipment();

			if (isConsolInvoice)
			{
				result = new Shipment();
			}
			else
			{
				var shipmentCollection = result.SubShipmentCollection;
				while (shipmentCollection?.Any() ?? false)
				{
					var shipment = shipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Any(y => (y.Key ?? ZString.Empty) == transactionParentID));
					if (shipment != null)
					{
						result = shipment;
						break;
					}
					else
					{
						var data = shipmentCollection.SelectMany(x => x.SubShipmentCollection ?? new DataObjectList<Shipment>());
						shipmentCollection = new DataObjectList<Shipment>(data);
					}
				}
			}

			return result;
		}

		public static string GetConsignor(bool isDeclarationInvoice, Shipment shipment)
		{
			var addressType = isDeclarationInvoice ? "SupplierDocumentaryAddress" : "ConsignorDocumentaryAddress";

			return GetCompanyName(shipment, addressType);
		}

		public static string GetConsignee(bool isDeclarationInvoice, Shipment shipment)
		{
			var addressType = isDeclarationInvoice ? "ImporterDocumentaryAddress" : "ConsigneeDocumentaryAddress";

			return GetCompanyName(shipment, addressType);
		}

		public static ZString GetContactEmails(AdditionalContactInfo additionalContactInfo)
		{
			return string.Join(";", additionalContactInfo?.Mails ?? new List<ZString>()) ?? ZString.Empty;
		}

		static string GetCompanyName(Shipment shipment, string addressType)
		{
			return shipment.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType?.ToString() == addressType)?.CompanyName ?? string.Empty;
		}

		public struct RatingItem
		{
			public string Unit { get; set; }
			public decimal Price { get; set; }
			public decimal Quantity { get; set; }
		}

		public struct CurrencyAffectedHeaderData
		{
			public CurrencyAffectedHeaderData(UniversalTransactionInfo transaction, int multiplier, bool useLocalCurrency)
			{
				Curr = useLocalCurrency ? transaction.LocalCurrency.Code : transaction.OSCurrency.Code;
				Exrt = useLocalCurrency ? new ZDecimal(1) : transaction.ExchangeRate.Value.Round(2);
				Sum = useLocalCurrency ? transaction.LocalExVATAmount.Value * multiplier : transaction.OSExGSTVATAmount.Value * multiplier;
				Total = useLocalCurrency ? transaction.LocalTotal.Value * multiplier : transaction.OSTotal.Value * multiplier;
				Vat = useLocalCurrency ? transaction.LocalVATAmount.Value * multiplier : transaction.OSGSTVATAmount.Value * multiplier;
			}

			public string Curr { get; }
			public decimal Exrt { get; }
			public decimal Sum { get; }
			public decimal Total { get; }
			public decimal Vat { get; }
		}

		public struct CurrencyAffectedLineData
		{
			public CurrencyAffectedLineData(PostingJournal line, int multiplier, bool useLocalCurrency)
			{
				Amount = useLocalCurrency ? line.LocalAmount.Value * multiplier : line.OSAmount.Value * multiplier;
				Vat = useLocalCurrency ? line.LocalGSTVATAmount.Value * multiplier : line.OSGSTVATAmount.Value * multiplier;
				Total = useLocalCurrency ? line.LocalTotalAmount.Value * multiplier : line.OSTotalAmount.Value * multiplier;
			}

			public decimal Vat { get; }
			public decimal Total { get; }
			public decimal Amount { get; }
		}

		#region SuppressResourceStringsCheckRegion

		public const string EInvoiceType = "01GTKT";
		public const string PaymentMethod = "TM/CK";
		public const string AdjustmentType = "DCT";
		public const string CreditNoteType = "DCG";
		const string NOTREPORT = "NOTREPORT";
		public const string unit = "Unit";
		const string percentage = "100";
		const string lowestBill = "Lowest Bill";
		const string custom = "Custom";
		const string one = "1";
		const string unt = "UNT";

		#endregion
	}
}
