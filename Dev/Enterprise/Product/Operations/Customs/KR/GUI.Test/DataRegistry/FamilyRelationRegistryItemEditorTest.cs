using System;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FamilyRelationRegistryItemEditor))]
	sealed class FamilyRelationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new FamilyRelationRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((FamilyRelationUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(FamilyRelationUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new FamilyRelationRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System, FamilyRelationCollection.GetDefaultFamilyRelationCollection());

		protected override object[] GetValidRegistryValues()
		{
			var collection = new FamilyRelationCollection();
			var cycleNo = collection.AddNew();
			cycleNo.Code = "01";
			cycleNo.Description = "아버지";
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
