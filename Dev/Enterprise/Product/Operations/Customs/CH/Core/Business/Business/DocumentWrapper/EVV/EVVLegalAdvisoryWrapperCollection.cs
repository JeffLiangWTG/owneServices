using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVLegalAdvisoryWrapperCollection : DocumentWrapperCollection<EVVLegalAdvisoryWrapper>
{
	public static EVVLegalAdvisoryWrapperCollection New(IEnumerable<IEvvLegalAdvisory> legalAdvisories, BusinessObjectFactory factory)
		=> new EVVLegalAdvisoryWrapperCollection(legalAdvisories.EmptyIfNull(), factory);

	EVVLegalAdvisoryWrapperCollection(IEnumerable<IEvvLegalAdvisory> legalAdvisory, BusinessObjectFactory factory) : base(legalAdvisory, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVLegalAdvisoryWrapper.New((IEvvLegalAdvisory)objectToWrap, Factory);
	}
}
