using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderAgreedLocationOfGoods
{
	ZString AgreedLocationOfGoodsCode { get; }
	ZString AgreedLocationOfGoodsDescription { get; }
	ZString AuthorizedLocationOfGoodsCodeAndCin { get; }
	ZString CustomsSubPlace { get; }
}
