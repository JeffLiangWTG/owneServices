using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecBordereauConfig))]
sealed class EdecBordereauConfigTest : RegistryBusinessObjectTemplateTestCase<EdecBordereauConfig>
{
	public void TestIsEnabled() => AssertEquals("Default", false, EdecBordereauConfig.IsEnabled);

	public void TestNumberOfDaysIntEdit() => AssertEquals("Default", 3, EdecBordereauConfig.NumberOfDays);

	public void TestNumberOfDaysIntEditValidation() => CombineAssertions(() =>
	{
		const string error = "Please enter a 'Maximum Number of Days' within the range 0 to 10.";
		EdecBordereauConfig.NumberOfDays = 11;
		EdecBordereauConfig.ValidateNumberOfDays();
		AssertHasErrorContaining("NumberOfDays over 10", EdecBordereauConfig.NumberOfDaysInfo, error);

		EdecBordereauConfig.NumberOfDays = -1;
		EdecBordereauConfig.ValidateNumberOfDays();
		AssertHasErrorContaining("NumberOfDays under 0", EdecBordereauConfig.NumberOfDaysInfo, error);

		EdecBordereauConfig.NumberOfDays = 0;
		EdecBordereauConfig.ValidateNumberOfDays();
		AssertNoErrorContaining("NumberOfDays is between 0 and 10", EdecBordereauConfig.NumberOfDaysInfo, error);
	});

	protected override EdecBordereauConfig GetBusinessObjectToClone() => (EdecBordereauConfig)GetNewBusinessObject();

	protected override EdecBordereauConfig GetBusinessObjectToSerialise() => (EdecBordereauConfig)GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => new EdecBordereauConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

	protected override bool RequiresFactory => false;
	protected override bool RequiresFallbackLevel => true;

	EdecBordereauConfig EdecBordereauConfig => edecBordereauConfig ??= (EdecBordereauConfig)GetNewBusinessObject();
	EdecBordereauConfig edecBordereauConfig;
}
