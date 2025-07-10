using System;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Testing;

public static class FallbackConfigurationTestHelper
{
	public static FallbackConfiguration GetFallbackConfiguration(ZDateTime start)
	{
		var config = new FallbackConfiguration();
		config.Start = start;
		config.End = config.Start.AddHours(6);
		config.Regularisation = config.End.AddHours(1);
		config.RegularisationPeriod = 60;
		config.RegularisationCount = 1;
		config.InvocationReason = "Invocation reason";
		config.RevocationReason = "Revocation reason";
		config.RegularisationBatchSize = 10;
		return config;
	}

	public static FallbackConfiguration GetFallbackConfigurationWithDVASetValue(ZDateTime start)
	{
		var config = GetFallbackConfiguration(start);
		NLCustomsRegistry.Instance.FallbackConfiguration_DVA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
		return config;
	}
}
