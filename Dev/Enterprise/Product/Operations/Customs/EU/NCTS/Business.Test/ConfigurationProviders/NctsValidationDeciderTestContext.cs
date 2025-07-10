using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Moq;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public abstract class NctsValidationDeciderTestContext<TValidationDecider>(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	: ValidationDeciderTestContext<TValidationDecider, NctsConfiguration>(factory, ruleDeciderInterfaceTypes)
	where TValidationDecider : class
{
	protected sealed override IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<NctsConfiguration> configuration)
	{
		return NctsConfigurationTestHelper.UpdateObjectFactory(factory, configuration);
	}

	public NctsValidationDeciderTestContext<TValidationDecider> ClearCachedValidationDecider(BusinessObject bizObj)
	{
		bizObj?.ClearAllCachedValues();
		return this;
	}
}
