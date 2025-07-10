using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using RefAccElectronicProcessingFee = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefAccElectronicProcessingFee;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ExchangeRateSerializerHelper
	{
		public ExchangeRateSerializerHelper()
		{
		}

		public SerializeRatesResult SerializeRates()
		{
			if (ElectronicProcessingFees.Count == 0)
			{
				return new SerializeRatesResult
				{
					Success = false,
					ErrorMessage = Res.GetString("ec81d9c2-03b2-4c60-9800-5c730e20565f", "No rates were available to be serialized.")
				};
			}

			var processingFeeConfiguration = new EntityTypeConfiguration<RefAccElectronicProcessingFee>(true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_SystemCode, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_Category, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_Code, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_Description, isKeyColumn: false);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_CountryCode, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_JobDirection, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_Currency, isKeyColumn: true);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_Price, isKeyColumn: false);
			processingFeeConfiguration.IncludeColumn(x => x.EPF_ValidFrom, isKeyColumn: true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(processingFeeConfiguration);

			var writer = new CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter(writerConfiguration);
			writer.SetDataSource("Initialize Data for RefAccElectronicProcessingFee Table");
			writer.SetPublicationTime(ZDateTime.UtcNow.ToDateTime());
			writer.SetUpdateType(UpdateType.Full);

			foreach (var processingFee in ElectronicProcessingFees)
			{
				writer.PopulateData(processingFee.Value);
			}

			XmlDocument xmlDocument;

			using (var stream = new MemoryStream())
			{
				writer.SaveXml(stream);
				xmlDocument = writer.UniversalXml;
			}

			return new SerializeRatesResult
			{
				Success = true,
				SeralizedValue = xmlDocument
			};
		}

		public class SerializeRatesResult
		{
			public bool Success { get; set; }
			public string ErrorMessage { get; set; }
			public XmlDocument SeralizedValue { get; set; }
		}

		public static string RunValidation(IEnumerable<ClientLicencePriceItem> priceItems)
		{
			if (!priceItems.Any())
			{
				return Res.GetString("77f92083-df68-4292-a6f9-985381d0acab", "Please select at least one Price Item.");
			}

			var priceHeader = priceItems.First().Parent;

			if (priceHeader.HasChanges)
			{
				return Res.GetString("cc67b247-7499-48db-ab99-9cbe296a284d", "Please save changes before generating the XML.");
			}

			if (!priceHeader.L6_SystemCode.EqualsIgnoringCase(BillingConstants.PriceHeaderType.CargoWiseNext))
			{
				return Res.GetString("8bff9f72-87c9-463c-8aa1-96b21b7662f2", "System Code must be CargoWise Next.");
			}

			foreach (var priceItem in priceItems)
			{
				if (priceItem.HasChanges)
				{
					return Res.GetString("cc67b247-7499-48db-ab99-9cbe296a284d", "Please save changes before generating the XML.");
				}

				if (!string.IsNullOrEmpty(priceItem.L7_RX_NKCurrency))
				{
					return Res.GetString("165ea821-21ac-40fe-83fc-a22e019f1581", "Exchange rates cannot be exported for Price Items with Currency overrides.");
				}

				if (string.IsNullOrEmpty(priceItem.L7_Category))
				{
					return Res.GetString("647bf71f-3618-42bc-8778-c0672fea0f24", "Price Item Category must be entered.");
				}

				if (string.IsNullOrEmpty(priceItem.L7_Code))
				{
					return Res.GetString("727d9ca4-f83b-4909-862f-a4fb5ff407dc", "Price Item Code must be entered.");
				}

				if (!string.IsNullOrEmpty(priceItem.L7_RN_NKDisbursementCountry))
				{
					return Res.GetString("517bddb9-a870-4d71-8fdd-9c5215d93855", "Disbursement country must be empty for Price Items.");
				}

				if (!priceItem.L7_DisbursementDirection.EqualsIgnoringCase(OrgDocumentLookups.FilterDirectionConstants.Codes.All))
				{
					return Res.GetString("20e13c67-6ba7-47a8-a53e-47063d379e2b", "Disbursement direction must be ALL for Price Items.");
				}

				var exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_StartDate, priceHeader.L6_ValidFrom);
				exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.SellRate);
				exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, priceHeader.L6_RX_NKCurrency);
				var priceHeaderCurrencyExists = priceHeader.Factory.ExistsInDatabase(RefExchangeRateSchema.Constants.TableName, exchangeRateQuery);

				if (!priceHeaderCurrencyExists)
				{
					return Res.GetString("cd431ed8-88b5-4fe2-85e6-c42326467887", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.");
				}
			}

			return string.Empty;
		}

		public string AddRates(IEnumerable<ClientLicencePriceItem> priceItems)
		{
			var priceHeader = priceItems.FirstOrDefault().Parent;
			var validationErrorMessage = RunValidation(priceItems);

			if (!string.IsNullOrEmpty(validationErrorMessage))
			{
				return validationErrorMessage;
			}

			var exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_StartDate, priceHeader.L6_ValidFrom);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.SellRate);
			var exchangeRatesToAdd = priceHeader.Factory.Load<RefExchangeRate>(exchangeRateQuery);
			var priceHeaderCurrencyExchangeRate = exchangeRatesToAdd.FirstOrDefault(x => x.RE_RX_NKExCurrency.EqualsIgnoringCase(priceHeader.L6_RX_NKCurrency));

			if (priceHeaderCurrencyExchangeRate == null)
			{
				return Res.GetString("c433b0f0-e992-4a65-a419-1307b841e8e6", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.");
			}

			foreach (var priceItem in priceItems)
			{
				foreach (var rate in exchangeRatesToAdd)
				{
					var errorMessage = AddRate(priceHeader, priceItem, priceHeaderCurrencyExchangeRate, rate);

					if (!string.IsNullOrEmpty(errorMessage))
					{
						ElectronicProcessingFees.Clear();
						return errorMessage;
					}
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		string AddRate(ClientLicencePriceHeader priceHeader, ClientLicencePriceItem priceItem, RefExchangeRate priceHeaderCurrencyExchangeRate, RefExchangeRate exchangeRate)
		{
			var roundingDecimalPlaces = 0;
			var quotient = exchangeRate.ExCurrency.RX_ISOSubUnitRatio;
			while (quotient > 1)
			{
				roundingDecimalPlaces++;
				quotient = quotient / 10;
			}

			var convertedPrice = priceHeaderCurrencyExchangeRate == exchangeRate
				? (decimal)priceItem.L7_Price
				: (priceItem.L7_Price / priceHeaderCurrencyExchangeRate.RE_SellRate) * exchangeRate.RE_SellRate;
			var roundedPrice = Enterprise.ZArchitecture.Core.Utilities.Round(convertedPrice, roundingDecimalPlaces);

			var processingFee = new RefAccElectronicProcessingFee
			{
				EPF_SystemCode = priceHeader.L6_SystemCode,
				EPF_Category = priceItem.L7_Category,
				EPF_Code = priceItem.L7_Code,
				EPF_Description = priceItem.L7_Description,
				EPF_Currency = exchangeRate.RE_RX_NKExCurrency,
				EPF_JobDirection = priceItem.L7_DisbursementDirection,
				EPF_CountryCode = priceItem.L7_RN_NKDisbursementCountry,
				EPF_Price = roundedPrice,
				EPF_ValidFrom = priceHeader.L6_ValidFrom.ToDateTime(),
			};

			var uniqueKey = FormattableString.Invariant($"{processingFee.EPF_SystemCode}{processingFee.EPF_Category}{processingFee.EPF_Code}{processingFee.EPF_CountryCode}{processingFee.EPF_JobDirection}{processingFee.EPF_Currency}{processingFee.EPF_ValidFrom}");

			try
			{
				ElectronicProcessingFees.Add(uniqueKey, processingFee);
			}
			catch (ArgumentException e)
			{
				ErrorReporter.ReportOnce("ExchangeRateSerializerHelper Key was not unique", FormattableString.Invariant($"Key was: {uniqueKey}"), e);
				throw;
			}

			return null;
		}

		protected Dictionary<string, RefAccElectronicProcessingFee> ElectronicProcessingFees { get; } = new Dictionary<string, RefAccElectronicProcessingFee>();
	}
}
