using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EdecBordereauConfigRegistryItemUserControl))]
sealed class EdecBordereauConfigRegistryItemUserControlTest : RegistryZUserControlTestCase
{
	public void TestIsEnabledCheckBox() => CombineAssertions(() =>
	{
		using (var control = new EdecBordereauConfigRegistryItemUserControl())
		{
			var enabledCheckBox = control.EnabledCheckBox;
			AssertEquals("Visible", true, enabledCheckBox.Visible);
			AssertEquals("BindTo", nameof(EdecBordereauConfig.IsEnabled), enabledCheckBox.BindTo);
		}
	});

	public void TestNumberOfDaysIntEdit() => CombineAssertions(() =>
	{
		using (var control = new EdecBordereauConfigRegistryItemUserControl())
		{
			var numberOfDays = control.NumberOfDaysIntEdit;
			AssertEquals("Visible", true, numberOfDays.Visible);
			AssertEquals("BindTo", nameof(EdecBordereauConfig.NumberOfDays), numberOfDays.BindTo);
		}
	});

	protected override IBusiness GetNewBusinessEntity() => new EdecBordereauConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
}
