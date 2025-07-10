using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(PassarSearchRequestConfigRegistryItemUserControl))]
sealed class PassarSearchRequestConfigRegistryItemUserControlTest : RegistryZUserControlTestCase
{
	public void TestEnabledCheckBox() => CombineAssertions(() =>
	{
		using (var control = new PassarSearchRequestConfigRegistryItemUserControl())
		{
			var enabledCheckBox = control.EnabledCheckBox;
			AssertEquals("Visible", true, enabledCheckBox.Visible);
			AssertEquals("BindTo", nameof(PassarSearchRequestConfig.IsEnabled), enabledCheckBox.BindTo);
		}
	});

	public void TestTimeLimitIntEdit() => CombineAssertions(() =>
	{
		using (var control = new PassarSearchRequestConfigRegistryItemUserControl())
		{
			var timeLimitIntEdit = control.TimeLimitIntEdit;
			AssertEquals("Visible", true, timeLimitIntEdit.Visible);
			AssertEquals("BindTo", nameof(PassarSearchRequestConfig.TimeLimit), timeLimitIntEdit.BindTo);
		}
	});

	protected override IBusiness GetNewBusinessEntity() => new PassarSearchRequestConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
}
