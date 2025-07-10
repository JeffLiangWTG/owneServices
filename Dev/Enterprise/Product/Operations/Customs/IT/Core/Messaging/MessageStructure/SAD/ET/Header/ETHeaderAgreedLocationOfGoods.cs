using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderAgreedLocationOfGoods
{
	readonly IETHeaderAgreedLocationOfGoods iETHeaderAgreedLocationOfGoods;

	public ETHeaderAgreedLocationOfGoods(IETHeaderAgreedLocationOfGoods iETHeaderAgreedLocationOfGoods)
	{
		this.iETHeaderAgreedLocationOfGoods = Argument.NotNull(iETHeaderAgreedLocationOfGoods, "iETHeaderAgreedLocationOfGoods");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportRules("D", "CN92")]
	[MessageFieldExportWithTransitRules("D", "CN92")]
	[MessageFieldTransitRules("D", "CN92")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN92")]
	public ZString AgreedLocationOfGoodsCode => iETHeaderAgreedLocationOfGoods.AgreedLocationOfGoodsCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("D", "CN92")]
	[MessageFieldExportWithTransitRules("D", "CN92")]
	[MessageFieldTransitRules("D", "CN92")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN92")]
	public ZString AgreedLocationOfGoods => iETHeaderAgreedLocationOfGoods.AgreedLocationOfGoodsDescription;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportRules("D", "CN92")]
	[MessageFieldExportWithTransitRules("D", "CN92")]
	[MessageFieldTransitRules("D", "CN92")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN92")]
	public ZString AuthorisedLocationOfGoodsCode => iETHeaderAgreedLocationOfGoods.AuthorizedLocationOfGoodsCodeAndCin;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportRules("D", "CN92")]
	[MessageFieldExportWithTransitRules("D", "CN92")]
	[MessageFieldTransitRules("D", "CN92")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN92")]
	public ZString CustomsSubPlace => iETHeaderAgreedLocationOfGoods.CustomsSubPlace;
}
