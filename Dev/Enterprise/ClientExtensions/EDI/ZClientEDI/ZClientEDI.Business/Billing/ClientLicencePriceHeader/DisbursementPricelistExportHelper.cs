using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DisbursementPricelistExportHelper
	{
		public DisbursementPricelistExportHelper()
		{
		}

		public SerializePricelistResult SerializePricelist()
		{
			if (PriceItemsForSerialization.Count == 0)
			{
				return new SerializePricelistResult
				{
					Success = false,
					ErrorMessage = Res.GetString("18201630-0a71-4bef-943b-8e7fe0c7b79c", "No price items were available to be serialized.")
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

			foreach (var processingFee in PriceItemsForSerialization)
			{
				writer.PopulateData(processingFee.Value);
			}

			XmlDocument xmlDocument;

			using (var stream = new MemoryStream())
			{
				writer.SaveXml(stream);
				xmlDocument = writer.UniversalXml;
			}

			return new SerializePricelistResult
			{
				Success = true,
				SeralizedValue = xmlDocument
			};
		}

		public class SerializePricelistResult
		{
			public bool Success { get; set; }
			public string ErrorMessage { get; set; }
			public XmlDocument SeralizedValue { get; set; }
		}

		public static string RunValidation(ClientLicencePriceHeader priceHeader)
		{
			if (priceHeader.HasChanges)
			{
				return Res.GetString("cc67b247-7499-48db-ab99-9cbe296a284d", "Please save changes before generating the XML.");
			}

			if (!priceHeader.L6_SystemCode.EqualsIgnoringCase(BillingConstants.PriceHeaderType.CargoWiseNext))
			{
				return Res.GetString("c687136e-8e55-4024-90af-b29e17e1aa8d", "System Code must be CargoWise Next.");
			}

			var itemDirectionalFallbacks = priceHeader.Items.Where(x => x.L7_RN_NKDisbursementCountry.IsEmpty && !x.L7_Price.IsEmpty).Select(x => x.L7_DisbursementDirection).ToHashSet();

			if (
				itemDirectionalFallbacks.Contains(Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All)
				||
				(
					itemDirectionalFallbacks.Contains(Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import)
					&& itemDirectionalFallbacks.Contains(Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Export)
					&& itemDirectionalFallbacks.Contains(Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic)
					&& itemDirectionalFallbacks.Contains(Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Other)
				))
			{
			return string.Empty;
		}

			return Res.GetString("e1527166-499f-48d5-aad7-02e1515afbfe", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).");
		}

		public string AddPriceItems(ClientLicencePriceHeader priceHeader)
		{
			var validationErrorMessage = RunValidation(priceHeader);

			if (!string.IsNullOrEmpty(validationErrorMessage))
			{
				return validationErrorMessage;
			}

			var itemsToAdd = priceHeader.Items.Where(x => x.L7_Price != 0.0m && x.L7_Category.EqualsIgnoringCase(BillingConstants.PriceHeaderType.CargoWiseNext));

			foreach (var priceItem in itemsToAdd)
			{
				var errorMessage = AddPriceItem(priceHeader, priceItem);

				if (!string.IsNullOrEmpty(errorMessage))
				{
					PriceItemsForSerialization.Clear();
					return errorMessage;
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		string AddPriceItem(ClientLicencePriceHeader priceHeader, ClientLicencePriceItem priceItem)
		{
			var processingFee = new RefAccElectronicProcessingFee
			{
				EPF_SystemCode = priceHeader.L6_SystemCode,
				EPF_Category = priceItem.L7_Category,
				EPF_Code = priceItem.L7_Code,
				EPF_Description = priceItem.L7_Description,
				EPF_Currency = priceItem.L7_RX_NKCurrency,
				EPF_Price = priceItem.L7_Price,
				EPF_JobDirection = priceItem.L7_DisbursementDirection,
				EPF_CountryCode = priceItem.L7_RN_NKDisbursementCountry,
				EPF_ValidFrom = priceHeader.L6_ValidFrom.ToDateTime(),
			};

			var uniqueKey = FormattableString.Invariant($"{processingFee.EPF_SystemCode}{processingFee.EPF_Category}{processingFee.EPF_Code}{processingFee.EPF_CountryCode}{processingFee.EPF_JobDirection}{processingFee.EPF_Currency}{processingFee.EPF_ValidFrom}");

			try
			{
				PriceItemsForSerialization.Add(uniqueKey, processingFee);
			}
			catch (ArgumentException e)
			{
				ErrorReporter.ReportOnce("ExchangeRateSerializerHelper Key was not unique", FormattableString.Invariant($"Key was: {uniqueKey}"), e);
				throw;
			}

			return null;
		}

		protected Dictionary<string, RefAccElectronicProcessingFee> PriceItemsForSerialization { get; } = new Dictionary<string, RefAccElectronicProcessingFee>();
	}
}
