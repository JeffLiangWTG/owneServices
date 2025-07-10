using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public class EVVDutyAmountWrapper : DocumentWrapper
{
	public static EVVDutyAmountWrapper New(IEvvDutyAmount duty, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVDutyAmountWrapper(Argument.NotNull(duty, nameof(duty)), Argument.NotNull(factory, nameof(factory)), documentLanguage);

	EVVDutyAmountWrapper(IEvvDutyAmount duty, BusinessObjectFactory factory, ZString documentLanguage) : base(duty, factory)
	{
		this.documentLanguage = documentLanguage;
	}
	readonly ZString documentLanguage;

	IEvvDutyAmount DutyAmount => (IEvvDutyAmount)WrappedObject;

	public ZDecimal Amount => DutyAmount.Amount;

	public ZString Description => description ??= Factory.GetTaxesAndFeesCodeList(documentLanguage).GetDescriptionFromCode(DutyAmount.Type) ?? DutyAmount.Type;
	ZString? description;
}
