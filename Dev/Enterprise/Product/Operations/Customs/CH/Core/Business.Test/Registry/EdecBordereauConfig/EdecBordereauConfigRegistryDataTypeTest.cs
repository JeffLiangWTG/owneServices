using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecBordereauConfigRegistryDataType))]
sealed class EdecBordereauConfigRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EdecBordereauConfigRegistryDataType>
{
	protected override EdecBordereauConfigRegistryDataType GetNewDataType() => new EdecBordereauConfigRegistryDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var config1 = new EdecBordereauConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)) { IsEnabled = false };
		var config2 = new EdecBordereauConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)) { IsEnabled = true, NumberOfDays = 8 };

		return
		[
			new ValidSampleAndBinaryValueInDB(config1, new EdecBordereauConfigRegistryDataType().Serialise(config1)),
				new ValidSampleAndBinaryValueInDB(config2, new EdecBordereauConfigRegistryDataType().Serialise(config2))
		];
	}

	protected override string ExpectedEditorName => "EdecBordereauConfigRegistryItemEditor";
}
