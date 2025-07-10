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

[TestedType(typeof(EdecBordereauConfigRegistryItemEditor))]
sealed class EdecBordereauConfigRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new EdecBordereauConfigRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

	protected override bool GetEditorPaneEnabledState(Control editorPane) => !((EdecBordereauConfigRegistryItemUserControl)editorPane).ReadOnly;

	protected override Type GetExpectedEditorPaneType() => typeof(EdecBordereauConfigRegistryItemUserControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new EdecBordereauConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);

	protected override object[] GetValidRegistryValues()
	{
		return
		[
			new EdecBordereauConfig(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)),
		];
	}
}

