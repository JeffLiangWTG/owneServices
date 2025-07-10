using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationByServiceLevelRegistryItemEditor))]
	sealed class BillOfLadingNumberCustomisationByServiceLevelRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			RegistryItemEditor editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BillOfLadingNumberCustomisationByServiceLevelControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillOfLadingNumberCustomisationByServiceLevelControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillCustomisationByServiceLevelRegistryItem("", null, null, null, RegistryStorageFlags.System, new BillCustomisationByServiceLevelRegistryDataType());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BillOfLadingNumberCustomisationByServiceLevelRegistryItemEditor(new BillCustomisationByServiceLevelRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override object[] GetValidRegistryValues()
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation customisation = customisations.BillOfLadingNumberCustomisations[0];
			customisation.Elements.Sort(BillOfLadingNumberCustomisationElement.Schema.Key);

			for (int i = 0; i < customisation.Elements.Count; i++)
			{
				BillOfLadingNumberCustomisationElement element = customisation.Elements[i];
				element.Include = true;
				element.Order = (byte)i;
			}

			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "WWWWW";

			return new object[] { customisations };
		}

		#endregion
	}
}
