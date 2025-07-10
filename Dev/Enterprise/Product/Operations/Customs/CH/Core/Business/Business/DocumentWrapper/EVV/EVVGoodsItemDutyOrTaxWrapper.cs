using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemDutyOrTaxWrapper : DocumentWrapper
{
	public static EVVGoodsItemDutyOrTaxWrapper New(IEvvGoodsItemDutyOrTax dutyOrTax, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVGoodsItemDutyOrTaxWrapper(Argument.NotNull(dutyOrTax, nameof(dutyOrTax)), Argument.NotNull(factory, nameof(factory)), documentLanguage);

	EVVGoodsItemDutyOrTaxWrapper(IEvvGoodsItemDutyOrTax dutyOrTax, BusinessObjectFactory factory, ZString documentLanguage) : base(dutyOrTax, factory)
	{
		this.documentLanguage = documentLanguage;
	}
	readonly ZString documentLanguage;

	IEvvGoodsItemDutyOrTax DutyOrTax => (IEvvGoodsItemDutyOrTax)WrappedObject;

	public ZString Description => description ??= Factory.GetTaxesAndFeesCodeList(documentLanguage).GetDescriptionFromCode(DutyOrTax.Type) ?? DutyOrTax.Type;
	ZString? description;

	public ZString BasisForAssessment => DutyOrTax.BasisForAssessment;

	public ZDecimal AlcoholLevel => new ZDecimal(DutyOrTax.AlcoholLevel);

	public ZString Rate => DutyOrTax.Rate;

	public ZDecimal Amount => DutyOrTax.Amount;

	public ZDecimal RefundAmount => DutyOrTax.RoundedAmount ? ZDecimal.Zero : Amount;
}
