using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarSearchRequestConfigRegistryDataType))]
sealed class PassarSearchRequestConfigRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PassarSearchRequestConfigRegistryDataType>
{
	protected override PassarSearchRequestConfigRegistryDataType GetNewDataType() => new PassarSearchRequestConfigRegistryDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var config1 = new PassarSearchRequestConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)) { IsEnabled = false };
		var config2 = new PassarSearchRequestConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)) { IsEnabled = true, TimeLimit = 48 };

		return
		[
			new ValidSampleAndBinaryValueInDB(config1, new PassarSearchRequestConfigRegistryDataType().Serialise(config1)),
			new ValidSampleAndBinaryValueInDB(config2, new PassarSearchRequestConfigRegistryDataType().Serialise(config2)),
		];
	}

	protected override string ExpectedEditorName => "PassarSearchRequestConfigRegistryItemEditor";
}
