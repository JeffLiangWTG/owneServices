using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString", Justification = "HTML strings")]
	public class CC025CMessagePrettier : NCTSMessagePrettier<Cc025CType>
	{
		public CC025CMessagePrettier(NCTSMessageDataObject<Cc025CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationStyle => "<style> body, p, span { font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px; } ul, #tree { list-style-type: none; line-height: 1.4; } #tree { margin: 0; padding: 0; }</style>";

		protected override ZString GetMessageInterpretationCore(Cc025CType messageObject)
		{
			var result = GenerateMessageDetails(messageObject.TransitOperation) + GenerateHouseConsignments(messageObject.Consignment);
			return result;
		}

		ZString GenerateMessageDetails(TransitOperationType10 transitOperation)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", TP5ResponseMessageSubTypeList.Descriptions.GoodsReleaseNotification),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				("Release date", transitOperation?.ReleaseDate.ToString("dd/MM/yyyy") ?? ZString.Empty),
				("Release type", $"{transitOperation?.ReleaseIndicator} - {GetFRCodeDescription(transitOperation?.ReleaseIndicator, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL164)}"),
			});
		}

		ZString GenerateHouseConsignments(IEnumerable<HouseConsignmentType02> houseConsignments)
		{
			ZString result = @"<ul id=""tree""><li><span><b>Consignment</b></span><ul>";
			result = houseConsignments.Aggregate(result, (current, houseConsignment) => current + GenerateHouseConsignment(houseConsignment));
			result += "</ul></li></ul>";

			return result;
		}

		ZString GenerateHouseConsignment(HouseConsignmentType02 houseConsignment)
		{
			var result = OpenUnsortedList("House Consignment");

			result += AddListItems(new List<(ZString Key, ZString Value)>
			{
				new("Sequence number", houseConsignment.SequenceNumber),
				new ("Release type", houseConsignment.ReleaseType),
				new ("Release description", GetFRCodeDescription(houseConsignment.ReleaseType, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL163))
			});
			result += GenerateConsignmentItems(houseConsignment.ConsignmentItem);
			result += CloseUnsortedList();

			return result;
		}

		ZString GenerateConsignmentItems(IEnumerable<ConsignmentItemType02> consignmentItems)
		{
			return consignmentItems.Aggregate(ZString.Empty, (current, consignmentItem) => (ZString)(current + GenerateConsignmentItem(consignmentItem)));
		}

		ZString GenerateConsignmentItem(ConsignmentItemType02 consignmentItem)
		{
			var result = OpenUnsortedList("Consignment Item");

			result += AddListItems(new List<(ZString Key, ZString Value)>
			{
				new ("Goods item number", consignmentItem.GoodsItemNumber),
				new ("Declaration Goods item number", consignmentItem.DeclarationGoodsItemNumber),
				new ("Release type", consignmentItem.ReleaseType),
				new ("Release description", GetFRCodeDescription(consignmentItem.ReleaseType, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL163))
			});

			result += GenerateCommodity(consignmentItem.Commodity);
			result += GeneratePackagingItems(consignmentItem.Packaging);
			result += CloseUnsortedList();

			return result;
		}

		ZString GenerateCommodity(CommodityType08 commodity)
		{
			var result = OpenUnsortedList("Commodity");

			result += AddListItems(new List<(ZString Key, ZString Value)>
			{
				new ("Harmonised system sub-heading code", commodity.CommodityCode.HarmonizedSystemSubHeadingCode),
				new ("Combined nomenclature code", commodity.CommodityCode.CombinedNomenclatureCode),
				new ("Description of goods", commodity.DescriptionOfGoods)
			});
			result += CloseUnsortedList();

			return result;
		}

		ZString GeneratePackagingItems(IEnumerable<PackagingType02> packagingItems)
		{
			return packagingItems.Aggregate(ZString.Empty, (current, packagingItem) => (ZString)(current + GeneratePackagingItem(packagingItem)));
		}

		ZString GeneratePackagingItem(PackagingType02 packaging)
		{
			var result = OpenUnsortedList("Packaging");

			result += AddListItems(new List<(ZString Key, ZString Value)>
			{
				new ("Sequence number", packaging.SequenceNumber),
				new ("Number of packages", packaging.NumberOfPackages),
				new ("Type of packages", packaging.TypeOfPackages),
				new ("Shipping marks", packaging.ShippingMarks)
			});
			result += CloseUnsortedList();

			return result;
		}

		ZString OpenUnsortedList(string title)
		{
			return $"<li><span><b>{title}</b></span><ul>";
		}

		ZString CloseUnsortedList()
		{
			return "</ul></li>";
		}

		ZString AddListItems(List<(ZString Key, ZString Value)> items)
		{
			return items.Aggregate(ZString.Empty, (current, item) => (ZString)(current + AddListItem(item.Key, item.Value)));
		}

		string AddListItem(string label, string value)
		{
			return $"<li><strong>{label}</strong>: {value}</li>";
		}

		ZString GetFRCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.CountryCodes.France, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}
	}
}

