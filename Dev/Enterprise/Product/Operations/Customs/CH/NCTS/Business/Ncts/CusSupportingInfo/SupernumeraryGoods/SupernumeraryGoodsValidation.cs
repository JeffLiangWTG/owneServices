using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class SupernumeraryGoodsValidation : CusSupportingInfoValidation
{
	public SupernumeraryGoodsValidation(SupernumeraryGoods parent) : base(parent)
	{
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_PackType()
	{
		base.CheckCSI_PackType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_PackTypeInfo);
	}

	protected override void CheckCSI_PackQty()
	{
		base.CheckCSI_PackQty();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_PackQtyInfo);
	}

	protected override void CheckCSI_Tariff()
	{
		base.CheckCSI_Tariff();
		if (!Parent.CSI_Tariff.IsEmpty && Parent.CSI_Tariff.Length != 6)
		{
			Parent.CSI_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_TariffInfo);
		}
	}
}
