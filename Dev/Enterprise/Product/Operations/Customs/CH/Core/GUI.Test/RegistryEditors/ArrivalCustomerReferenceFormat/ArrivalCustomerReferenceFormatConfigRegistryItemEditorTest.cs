using System;
using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ArrivalCustomerReferenceFormatConfigRegistryItemEditor))]
class ArrivalCustomerReferenceFormatConfigRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor()
	{
		var fallback = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
		return new ArrivalCustomerReferenceFormatConfigRegistryItemEditor(RegistryItem.DataType, fallback, Factory);
	}

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		return !((ArrivalCustomerReferenceFormatConfigControl)editorPane).ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType()
	{
		return typeof(ArrivalCustomerReferenceFormatConfigControl);
	}

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
	{
		return new ArrivalCustomerReferenceFormatRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
	}

	protected override object[] GetValidRegistryValues()
	{
		var referenceFormat = new ArrivalCustomerReferenceFormat();
		return new object[] { referenceFormat };
	}

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
	{
		get { return RegistryItemEditor.EditorPaneAnchor.All; }
	}
}
