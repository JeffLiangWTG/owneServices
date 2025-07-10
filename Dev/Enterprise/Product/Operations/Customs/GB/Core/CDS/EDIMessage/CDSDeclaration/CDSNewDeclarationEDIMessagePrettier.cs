using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSNewDeclarationEDIMessagePrettier : CDSEDIMessagePrettier<CDSNewDeclarationEDIMessage>
	{
		public CDSNewDeclarationEDIMessagePrettier(CDSNewDeclarationEDIMessage message) : base(message)
		{
		}

		public override ZString MakeHumanReadable()
		{
			var declaration = Message.MessageDataObject?.GetDeclaration();
			if (declaration == null)
			{
				return ZString.Empty;
			}

			var headerTableDisplay = GetTableDisplay(GetHeaderDetails(declaration));
			var lineTableDisplays = declaration.GoodsShipment?.GovernmentAgencyGoodsItem?
						.Select(item => GetTableDisplay(GetLineDetails(item)))
						.ToArray()
						?? Array.Empty<HtmlTableCreator>();

			var linesDisplay = lineTableDisplays.IsNullOrEmpty()
				? ZString.Empty
				: new ZString(ToUlIfNotEmpty(lineTableDisplays.Select((lineTableDisplay, i) => ToLiIfNotEmpty(Invariant($"Entry line {i + 1}:{lineTableDisplay.ToHtml()}"))).JoinAsString()));

			return MessagePrettierCss.CSS +
					ToH1IfNotEmpty("New Declaration Request") +
					ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("Declaration Type", Entry?.EntryInstruction?.CEI_Style ?? ZString.Empty),
						("Declaration Sub Type", Declaration?.JE_EntrySubStyle ?? ZString.Empty),
						("Entry Type", Declaration?.JE_MessageType ?? ZString.Empty),
						("Profile", Declaration?.JE_CustomsProfile ?? ZString.Empty),
						("CSP", Declaration?.ZG_Gateway ?? ZString.Empty),
					}) +
					ToTableSection("Entry Header", headerTableDisplay) +
					linesDisplay;
		}

		HtmlTableCreator GetTableDisplay(IEnumerable<(ZString Name, ZString Value, ZString Path)> details)
		{
			var tableCreator = GetHtmlTableCreator();
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("Name", "width", "20%"),
				new CellWithFormatting("Value", "width", "20%"),
				new CellWithFormatting("Path", "width", "60%")
			);

			foreach (var (name, value, path) in details.Where(x => !x.Value.IsEmpty))
			{
				tableCreator.WriteRow(name, value, path);
			}

			return tableCreator;
		}

		static IEnumerable<(ZString Name, ZString Value, ZString Path)> GetHeaderDetails(CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration declaration)
		{
			yield return ("Entry Reference", declaration.FunctionalReferenceID?.Value, "FunctionalReferenceID");
			yield return ("Type Code", declaration.TypeCode?.Value, "TypeCode");
			yield return ("UCR", declaration.GoodsShipment?.UCR?.TraderAssignedReferenceID?.Value, "GoodsShipment/UCR/TraderAssignedReferenceID");
			yield return ("Goods Item Quantity", GetDisplayFor(declaration.GoodsItemQuantity?.Value), "GoodsItemQuantity");
			yield return ("Total Gross Mass Measure", GetDisplayFor(declaration.TotalGrossMassMeasure?.Value), "TotalGrossMassMeasure");
			yield return ("Total Package Quantity", GetDisplayFor(declaration.TotalPackageQuantity?.Value), "TotalPackageQuantity");
			yield return ("Transport Nationality", declaration.BorderTransportMeans?.RegistrationNationalityCode?.Value, "BorderTransportMeans/RegistrationNationalityCode");
			yield return ("Transport Mode", declaration.BorderTransportMeans?.ModeCode?.Value, "BorderTransportMeans/ModeCode");
			yield return ("Container Mode", declaration.GoodsShipment?.Consignment?.ContainerCode?.Value, "GoodsShipment/Consignment/ContainerCode");
			yield return ("Container Numbers", GetContainerNumbers(declaration).JoinAsString(","), "GoodsShipment/Consignment/TransportEquipment/ID");
			yield return ("Destination Country", declaration.GoodsShipment?.Destination?.CountryCode?.Value, "GoodsShipment/Destination/CountryCode");
			yield return ("Export Country", declaration.GoodsShipment?.ExportCountry?.ID?.Value, "GoodsShipment/ExportCountry/ID");
			yield return ("Transaction Nature", declaration.GoodsShipment?.TransactionNatureCode?.Value, "GoodsShipment/TransactionNatureCode");
			yield return ("Agent", declaration.Agent?.Name?.Value, "Agent/Name");
			yield return ("Declarant", declaration.Declarant?.ID?.Value, "Declarant/ID");
			yield return ("Exporter", declaration.Exporter?.Name?.Value, "Exporter/Name");
			yield return ("Buyer", declaration.GoodsShipment?.Buyer?.Name?.Value, "GoodsShipment/Buyer/Name");
			yield return ("Buyer", declaration.GoodsShipment?.Buyer?.ID?.Value, "GoodsShipment/Buyer/ID");
			yield return ("Importer ID", declaration.GoodsShipment?.Importer?.ID?.Value, "GoodsShipment/Importer/ID");
			yield return ("Seller", declaration.GoodsShipment?.Seller?.Name?.Value, "GoodsShipment/Seller/Name");
			yield return ("Seller", declaration.GoodsShipment?.Seller?.ID?.Value, "GoodsShipment/Seller/ID");
			yield return ("Warehouse ID", declaration.GoodsShipment?.Warehouse?.ID?.Value, "GoodsShipment/Warehouse/ID");
			yield return ("Presentation Office", declaration.PresentationOffice?.ID?.Value, "PresentationOffice/ID");
			yield return ("Supervising Office", declaration.SupervisingOffice?.ID?.Value, "SupervisingOffice/ID");
		}

		static ZString[] GetContainerNumbers(CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration declaration)
		{
			return declaration.GoodsShipment?.Consignment?.TransportEquipment?
					.Select(x => new ZString(x.ID?.Value))
					.Where(x => !x.IsEmpty).ToArray()
					?? Array.Empty<ZString>();
		}

		static IEnumerable<(ZString Name, ZString Value, ZString Path)> GetLineDetails(DeclarationGoodsShipmentGovernmentAgencyGoodsItem item)
		{
			yield return ("Net Weight", GetDisplayFor(item.Commodity?.GoodsMeasure?.NetNetWeightMeasure?.Value), "Commodity/GoodsMeasure/NetNetWeightMeasure");
			yield return ("Quantity", GetDisplayFor(item.Packaging?.Sum(x => x.QuantityQuantity?.Value ?? decimal.Zero)), "Packaging/QuantityQuantity");
			yield return ("Origin Country", item.Origin?.FirstOrDefault()?.CountryCode?.Value, "Origin/CountryCode");
			yield return ("Export Country", item.ExportCountry?.ID?.Value, "GoodsShipment/ExportCountry/ID");
			yield return ("Consignor", item.Consignor?.Name?.Value, "Consignor/Name");

			var classifications = item.Commodity?.Classification;
			if (classifications != null)
			{
				yield return ("Commodity Code", GetCommodityCode(classifications), "Commodity/Classification[Contains('TSP,TRC,TRA',IdentificationTypeCode)]/ID");
				yield return ("Tax Rates", GetTaxRates(classifications), "Commodity/Classification[IdentificationTypeCode='GN']/ID");
			}

			yield return ("Procedures", GetProcedures(item).JoinAsString(","), "GovernmentProcedure/CurrentCode+GovernmentProcedure/PreviousCode");
		}

		static ZString GetDisplayFor(decimal? value)
		{
			return !value.HasValue || value.Value == decimal.Zero
				? ZString.Empty
				: (ZString)value.Value.ToString(CultureInfo.InvariantCulture);
		}

		static ZString GetTaxRates(IEnumerable<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> classifications)
		{
			return classifications.Select(classification => new
			{
				TypeCode = new ZString(classification.IdentificationTypeCode?.Value),
				ID = new ZString(classification.ID?.Value)
			})
			.Where(x => x.TypeCode == Constants.Classification.IdentificationTypeCodes.GN && !x.ID.IsEmpty)
			.Select(x => x.ID)
			.ToArray()
			.JoinAsString(",");
		}

		static ZString GetCommodityCode(IEnumerable<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> classifications)
		{
			var (tsp, trc, tra1, tra2) = GetCommodityCodeDetails(classifications);
			var result = tsp + trc + tra1;
			if (!tra2.IsEmpty)
			{
				result += " " + tra2;
			}
			return result;
		}

		static (ZString Tsp, ZString Trc, ZString Tra1, ZString Tra2) GetCommodityCodeDetails(IEnumerable<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> classifications)
		{
			ZString tsp, trc, tra1, tra2;
			tsp = trc = tra1 = tra2 = ZString.Empty;
			foreach (var classification in classifications)
			{
				var typeCode = classification.IdentificationTypeCode?.Value ?? ZString.Empty;
				var id = classification.ID?.Value ?? ZString.Empty;
				switch (typeCode)
				{
					case Constants.Classification.IdentificationTypeCodes.TSP:
						tsp = id;
						break;
					case Constants.Classification.IdentificationTypeCodes.TRC:
						trc = id;
						break;
					case Constants.Classification.IdentificationTypeCodes.TRA when tra1.IsEmpty:
						tra1 = id;
						break;
					case Constants.Classification.IdentificationTypeCodes.TRA when !tra1.IsEmpty:
						tra2 = id;
						break;
				}
			}

			return (tsp, trc, tra1, tra2);
		}

		static ZString[] GetProcedures(DeclarationGoodsShipmentGovernmentAgencyGoodsItem item)
		{
			return item.GovernmentProcedure?.Select(x =>
			{
				var currentCode = x.CurrentCode?.Value ?? ZString.Empty;
				var previousCode = x.PreviousCode?.Value ?? ZString.Empty;
				return new ZString(currentCode + previousCode);
			}).Where(x => !x.IsEmpty).ToArray()
			?? Array.Empty<ZString>();
		}
	}
}
