using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class EVVDutyAmountWrapperCollection : DocumentWrapperCollectionWithLanguage<IEvvDutyAmount, EVVDutyAmountWrapper>
{
	public static EVVDutyAmountWrapperCollection New(IEnumerable<IEvvDutyAmount> duties, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVDutyAmountWrapperCollection(duties.EmptyIfNull().OrderBy(d => d.Type == AdditionalTaxesTypes.Duty ? 1 : 2), factory, documentLanguage);

	EVVDutyAmountWrapperCollection(IEnumerable<IEvvDutyAmount> dutyAmounts, BusinessObjectFactory factory, ZString documentLanguage) : base(dutyAmounts, factory, documentLanguage)
	{
	}

	protected override DocumentWrapper WrapObject(IEvvDutyAmount objectToWrap, ZString documentLanguage)
	{
		return EVVDutyAmountWrapper.New(objectToWrap, Factory, documentLanguage);
	}
}
