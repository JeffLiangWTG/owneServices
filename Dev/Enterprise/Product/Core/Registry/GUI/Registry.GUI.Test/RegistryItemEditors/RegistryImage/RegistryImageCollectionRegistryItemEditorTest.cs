using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryImageCollectionRegistryItemEditor))]
	sealed class RegistryImageCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new RegistryImageCollectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RegistryImageCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RegistryImageCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RegistryImageCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			RegistryImageCollection collection = new RegistryImageCollection();
			RegistryImage image = collection.AddNew();
			image.Code = "ABC";
			image.Description = (NoResString)"XYZ";
			image.Image = new Bitmap(10, 10);

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			RegistryImageCollection collection1 = (RegistryImageCollection)setValue;
			RegistryImageCollection collection2 = (RegistryImageCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Code", i.ToString()), collection1[i].Code, collection2[i].Code);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Description", i.ToString()), collection1[i].Description, collection2[i].Description);
				Assert(string.Format("GetValueFromEditorPane()[{0}].Image is different from the value what was set", i.ToString()),
				Utilities.IsImageEqual(collection1[i].Image, collection2[i].Image));
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
