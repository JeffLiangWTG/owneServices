using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.EInvoicing.Egypt.Schemas;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Core.Constants;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.Egypt
{
	public enum DocumentType
	{
		i, // Invoice
		c, // Credit Note
		d, // Debit Note
	}

	public class XUTtoDocumentMapper
	{
		public XUTtoDocumentMapper(UniversalTransactionInfo transactionInfo, DocumentType documentType, string clientOrgCategory, RefTimeZoneSet branchTimeZoneSet = null, string originalTransactionId = null, ForwardingShipment shipment = null)
		{
			TransactionInfo = Argument.NotNull(transactionInfo, nameof(transactionInfo));
			DocumentType = documentType;
			ClientOrgCategory = clientOrgCategory;
			OriginalTransactionId = documentType == DocumentType.d ? Argument.NotNullOrEmpty(originalTransactionId, nameof(originalTransactionId)) : originalTransactionId;
			Shipment = shipment;
			if (!string.IsNullOrEmpty(originalTransactionId))
			{
				Argument.NotNull(transactionInfo.OriginalReference, nameof(transactionInfo.OriginalReference));
			}
			BranchTimeZoneSet = branchTimeZoneSet;
			IsProduction = EnvProxy.Instance.IsProductionSystem;
		}

		public (bool Success, string Xml) MapToXml(INotifications notifications)
		{
			var success = false;
			var xml = string.Empty;

			try
			{
				if (ValidateTransactionInfo(notifications))
				{
					var document = MapToDocument();
					xml = SerializeDocument(document);
					success = true;
				}
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				notifications.AddError(exception.Message);
			}

			return (success, xml);
		}
		UniversalTransactionInfo TransactionInfo { get; }
		ForwardingShipment Shipment { get; }
		DocumentType DocumentType { get; }
		string ClientOrgCategory { get; }
		RefTimeZoneSet BranchTimeZoneSet { get; }
		string OriginalTransactionId { get; }
		bool IsProduction { get; }

		internal bool ValidateTransactionInfo(INotifications notifications)
		{
			var result = true;

			if (string.IsNullOrEmpty((GetIssuerBranchId() ?? ZString.Empty).Trim()))
			{
				result = false;
				notifications.AddError(Res.GetString("c3d9d402-f2f9-46f6-b11a-c1e43ed95ef7", "Missing required Egypt GCR registration number of the {0} branch.", TransactionInfo.Branch?.Code));
			}
			if (string.IsNullOrEmpty((GetIssuerId() ?? string.Empty).Trim()))
			{
				result = false;
				notifications.AddError(Res.GetString("4e26dae7-9971-4b61-a4a2-6c43da230692", "Missing required Egypt COM registration number of the {0} branch.", TransactionInfo.Branch?.Code));
			}
			if (string.IsNullOrEmpty((GetTaxpayerActivityCode() ?? ZString.Empty).Trim()))
			{
				result = false;
				notifications.AddError(Res.GetString("139fb900-fb00-459b-9436-887f07184537", "Missing required Egypt GBR registration number of the {0} branch.", TransactionInfo.Branch?.Code));
			}

			return result;
		}

		internal document MapToDocument()
		{
			var invoiceLines = GetInvoiceLines();
			var result = new document()
			{
				issuer = GetIssuer(),
				receiver = GetReceiver(),
				documentType = DocumentType.ToString(),
				documentTypeVersion = IsProduction ? "1.0" : "0.9",
				dateTimeIssued = GetDateTimeIssuedUtc(),
				taxpayerActivityCode = GetTaxpayerActivityCode(),
				internalID = TransactionInfo.Ledger + " " + TransactionInfo.TransactionType + " " + TransactionInfo.OrganizationAddress?.OrganizationCode + " " + TransactionInfo.Number,
				references = GetReferencesIfApplicable(),
				purchaseOrderReference = GetPurchaseOrderReference(),
				delivery = GetShipmentDelivery(),
				invoiceLines = invoiceLines,
				totalSalesAmount = GetAmount(TransactionInfo.LocalExVATAmount),
				totalDiscountAmount = 0m,
				netAmount = GetAmount(TransactionInfo.LocalExVATAmount),
				taxTotals = GetTaxTotals(invoiceLines),
				extraDiscountAmount = 0m,
				totalItemsDiscountAmount = 0m,
				totalAmount = GetAmount(TransactionInfo.LocalTotal),
			};

			return result;
		}

		protected string SerializeDocument(document document)
		{
			Argument.NotNull(document, nameof(document));

			var xmlWriterSettings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = true,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.UTF8,
				Indent = true
			};

			using (var ms = new MemoryStream())
			{
				using (var xmlWritter = XmlWriter.Create(ms, xmlWriterSettings))
				{
					var serializer = ZXmlSerializer.New(typeof(document));
					serializer.Serialize(xmlWritter, document);
				}
				ms.Position = 0;
				using (var reader = new StreamReader(ms, true))
				{
					return reader.ReadToEnd();
				}
			}
		}

		issuerType GetIssuer() => new issuerType()
		{
			type = issuerPersonaTypeEnum.B,
			id = GetIssuerId(),
			name = TransactionInfo.BranchAddress?.CompanyName,
			address = new addressType()
			{
				branchID = GetIssuerBranchId(),
				country = TransactionInfo.BranchAddress?.Country?.Code,
				governate = MapStateToGovernateIfEgypt(TransactionInfo.BranchAddress?.Country?.Code, TransactionInfo.BranchAddress?.State),
				regionCity = TransactionInfo.BranchAddress?.City,
				street = TransactionInfo.BranchAddress?.Address1,
				buildingNumber = !string.IsNullOrWhiteSpace(TransactionInfo.BranchAddress?.Address2) ? TransactionInfo.BranchAddress?.Address2.ToString() : "-",
				postalCode = TransactionInfo.BranchAddress?.Postcode,
			},
		};

		receiverType GetReceiver() => new receiverType()
		{
			type = GetClientCategory(),
			id = GetClientId(),
			name = TransactionInfo.OrganizationAddress?.CompanyName,
			address = new receiverAddressType()
			{
				country = TransactionInfo.OrganizationAddress?.Country?.Code,
				governate = MapReceiverStateToGovernateIfEgypt(TransactionInfo.OrganizationAddress?.Country?.Code, TransactionInfo.OrganizationAddress?.State),
				regionCity = TransactionInfo.OrganizationAddress?.City,
				street = TransactionInfo.OrganizationAddress?.Address1,
				buildingNumber = !string.IsNullOrWhiteSpace(TransactionInfo.OrganizationAddress?.Address2) ? TransactionInfo.OrganizationAddress.Address2.ToString() : "-",
				postalCode = GetFixedReceiverPostalCode(TransactionInfo.OrganizationAddress?.Country?.Code, TransactionInfo.OrganizationAddress?.Postcode),
			},
		};

		string GetPurchaseOrderReference() => Shipment?.JS_OrderReferences;

		delivery GetShipmentDelivery() => Shipment != null ? new delivery()
		{
			grossWeightSpecified = true,
			grossWeight = RoundAndForceDecimalPlaces(Shipment.JS_ActualWeight, 5),
		} : null;

		invoiceLineType[] GetInvoiceLines()
		{
			var isForeignCurrency = TransactionInfo.OSCurrency != null && TransactionInfo.OSCurrency.Code.ToString().ToUpper() != Constants.CurrencyCodes.Egypt;

			var invoiceLinesList = new List<invoiceLineType>();
			var linesWithoutCMTCharge = TransactionInfo.PostingJournalCollection.Where(x => (x.ChargeCode?.ChargeType?.Code).GetValueOrDefault() != Constants.ChargeType.Comment).ToList();

			foreach (var postingJournal in linesWithoutCMTCharge)
			{
				var exchangeRate = isForeignCurrency
					? RoundAndForceDecimalPlaces(TransactionInfo.ExchangeRate.Value, 5)
					: decimal.Zero;

				var unitValue = new unitValueType()
				{
					currencySold = TransactionInfo.OSCurrency?.Code ?? CurrencyCodes.Egypt,
					amountEGP = GetAmount(postingJournal.LocalAmount),
					amountSold = isForeignCurrency ? GetAmount(postingJournal.OSAmount) : decimal.Zero,
					amountSoldSpecified = isForeignCurrency,
					currencyExchangeRate = exchangeRate,
					currencyExchangeRateSpecified = isForeignCurrency,
				};

				var taxableItemsList = new List<taxableItemType>();
				taxableItemsList.Add(new taxableItemType()
				{
					taxType = MapTaxType((postingJournal.TaxMessageID?.TaxGroupCode?.Code).GetValueOrDefault()),
					amount = GetAmount(postingJournal.LocalGSTVATAmount),
					subType = postingJournal.TaxMessageID?.TaxGroupCode?.GovernmentCode,
					rate = postingJournal.VATTaxID?.TaxRate ?? decimal.Zero,
					rateSpecified = postingJournal.VATTaxID?.TaxRate != null,
				});

				var line = new invoiceLineType()
				{
					description = postingJournal.Description,
					itemType = "EGS",
					itemCode = postingJournal.GovernmentReportingChargeCode,
					unitType = "EA",
					quantity = 1,
					unitValue = unitValue,
					salesTotal = GetAmount(postingJournal.LocalAmount),
					total = GetAmount(postingJournal.LocalTotalAmount),
					valueDifference = 0m,
					totalTaxableFees = 0m,
					netTotal = GetAmount(postingJournal.LocalAmount),
					taxableItems = taxableItemsList.ToArray(),
				};
				invoiceLinesList.Add(line);
			}
			return invoiceLinesList.ToArray();
		}

		string MapTaxType(ZString taxType)
			=> (string)taxType switch
			{
				"T2" => "T2",
				"N01" or "N02" => "T20",
				_ => "T1",
			};

		taxTotalType[] GetTaxTotals(invoiceLineType[] invoiceLines) => invoiceLines
			.GroupBy(il => il.taxableItems[0].taxType)
			.Select(ilg => new taxTotalType()
			{
				taxType = ilg.Key,
				amount = ilg.Sum(l => l.taxableItems[0].amount)
			})
			.OrderBy(t => t.taxType)
			.ToArray();

		string GetIssuerBranchId() => GetBranchRegistrationNumber(OrgCusCode.CodeTypes.CorporationCode);

		string GetIssuerId() => GetBranchRegistrationNumber(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber);

		DateTime GetDateTimeIssuedUtc() => BranchTimeZoneSet != null
			? BranchTimeZoneSet.GetCalculationTimeZone().ToUniversalTime(TransactionInfo.TransactionDate.Value.ToDateTime())
			: Environment.Env.Time.GetUtcFromLocalTime(TransactionInfo.TransactionDate.Value.ToDateTime());

		string GetTaxpayerActivityCode() => GetBranchRegistrationNumber(OrgCusCode.CodeTypes.GovBusinessCode);

		string[] GetReferencesIfApplicable() => DocumentType == DocumentType.i || string.IsNullOrEmpty(OriginalTransactionId) ? null : new string[] { OriginalTransactionId };

		string GetBranchRegistrationNumber(string typeCode, string countryCode = Constants.CountryCodes.Egypt)
			=> GetRegistrationNumberFromCollection(TransactionInfo.BranchAddress?.RegistrationNumberCollection, typeCode, countryCode);

		string GetClientRegistrationNumber(string typeCode, string countryCode = Constants.CountryCodes.Egypt)
			=> GetRegistrationNumberFromCollection(TransactionInfo.OrganizationAddress?.RegistrationNumberCollection, typeCode, countryCode);

		string GetRegistrationNumberFromCollection(List<UniversalRegistrationNumber> registrations, string typeCode, string countryCode)
			=> registrations?.FirstOrDefault(x => (x.CountryOfIssue?.Code ?? string.Empty) == countryCode && (x.Type?.Code ?? string.Empty) == typeCode)?.Value;

		string MapReceiverStateToGovernateIfEgypt(ZString? country, ZString? state)
		{
			var governate = MapStateToGovernateIfEgypt(country, state);
			if (IsForeignCountry(country) && string.IsNullOrWhiteSpace(governate))
			{
				return country;
			}
			else
			{
				return governate;
			}
		}

		string MapStateToGovernateIfEgypt(ZString? country, ZString? state)
		{
			if (country.HasValue && country.Value.ToUpper() == Constants.CountryCodes.Egypt)
			{
				#region SuppressResourceStringsCheckRegion

				switch (state?.ToUpper())
				{
					case "ALX":
						return "Alexandria Governorate";
					case "ASN":
						return "Aswan Governorate";
					case "AST":
						return "Asyut Governorate";
					case "BA":
						return "Red Sea Governorate";
					case "BH":
						return "Beheira Governorate";
					case "BNS":
						return "Beni Suef Governorate";
					case "C":
						return "Cairo Governorate";
					case "DK":
						return "Dakahlia Governorate";
					case "DT":
						return "Damietta Governorate";
					case "FYM":
						return "Faiyum Governorate";
					case "GH":
						return "Gharbia Governorate";
					case "GZ":
						return "Giza Governorate";
					case "HU":
						return "Helwan Governorate";
					case "IS":
						return "Ismailia Governorate";
					case "JS":
						return "South Sinai Governorate";
					case "KB":
						return "Qalyubai Governorate";
					case "KFS":
						return "Kafr el-Sheikh Governorate";
					case "KN":
						return "Qena Governorate";
					case "LX":
						return "Luxor Governorate";
					case "MN":
						return "Minya Governorate";
					case "MNF":
						return "Minufia Governorate";
					case "MT":
						return "Matruh Governorate";
					case "PTS":
						return "Port Said Governorate";
					case "SHG":
						return "Sohag Governorate";
					case "SHR":
						return "Sharqia Governorate";
					case "SIN":
						return "North Sinai Governorate";
					case "SU":
						return "6th of October Governorate";
					case "SUZ":
						return "Suez Governorate";
					case "WAD":
						return "New Valley Governorate";
					default:
						return null;
				}

				#endregion
			}
			else
			{
				return state;
			}
		}

		string GetFixedReceiverPostalCode(ZString? country, ZString? postalCode)
		{
			if (IsForeignCountry(country) && string.IsNullOrWhiteSpace(postalCode))
			{
				return "0000";
			}
			else
			{
				return postalCode;
			}
		}

		receiverPersonaTypeEnum GetClientCategory() =>
			ClientOrgCategory == OrgConstants.Category.NaturalPersonIndividual
			? receiverPersonaTypeEnum.P
			: TransactionInfo.OrganizationAddress?.Country?.Code?.ToString().ToUpper() != Core.Constants.CountryCodes.Egypt
			? receiverPersonaTypeEnum.F
			: receiverPersonaTypeEnum.B;

		string GetClientId()
		{
			switch (GetClientCategory())
			{
				case receiverPersonaTypeEnum.B:
					return GetClientRegistrationNumber(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber);
				case receiverPersonaTypeEnum.F:
					{
						var countryCode = TransactionInfo.OrganizationAddress?.Country?.Code;
						if (countryCode.HasValue)
						{
							var regCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
							return TransactionInfo.OrganizationAddress?.RegistrationNumberCollection?.
								FirstOrDefault(x => x.Type?.Code?.ToString().ToUpper() == regCode && x.CountryOfIssue?.Code == countryCode)?.Value
								?? TransactionInfo.OrganizationAddress?.RegistrationNumberCollection?.
								FirstOrDefault(x => x.CountryOfIssue?.Code == countryCode)?.Value;
						}
						return TransactionInfo.OrganizationAddress?.RegistrationNumberCollection?.FirstOrDefault()?.Value;
					}
				default:
					return null;
			}
		}

		decimal GetAmount(ZDecimal? amount) => Math.Abs(amount ?? ZDecimal.Zero);

		bool IsForeignCountry(string country)
		{
			return !string.Equals(country, CountryCodes.Egypt, StringComparison.OrdinalIgnoreCase);
		}

		protected decimal RoundAndForceDecimalPlaces(decimal d, int places)
			=> decimal.Round(d, places)
			+ new decimal(0, 0, 0, false, (byte)places);
	}
}
