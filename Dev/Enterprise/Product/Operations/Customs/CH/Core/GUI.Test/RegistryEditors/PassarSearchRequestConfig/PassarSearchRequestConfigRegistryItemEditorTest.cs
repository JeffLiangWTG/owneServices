using System;
using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(PassarSearchRequestConfigRegistryItemEditor))]
sealed class PassarSearchRequestConfigRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new PassarSearchRequestConfigRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

	protected override bool GetEditorPaneEnabledState(Control editorPane) => !((PassarSearchRequestConfigRegistryItemUserControl)editorPane).ReadOnly;

	protected override Type GetExpectedEditorPaneType() => typeof(PassarSearchRequestConfigRegistryItemUserControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new PassarSearchRequestConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);

	protected override object[] GetValidRegistryValues()
	{
		return
		[
			new PassarSearchRequestConfig(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)),
		];
	}
}
