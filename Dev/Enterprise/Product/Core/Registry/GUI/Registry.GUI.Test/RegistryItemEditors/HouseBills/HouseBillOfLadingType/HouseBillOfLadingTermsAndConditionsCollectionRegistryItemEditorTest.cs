using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor))]
	sealed class HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((HouseBillOfLadingTermsAndConditionsCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HouseBillOfLadingTermsAndConditionsCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			HouseBillOfLadingTermsAndConditionsCollection collection = new HouseBillOfLadingTermsAndConditionsCollection();
			HouseBillOfLadingTermsAndConditions terms = collection.AddNew();
			terms.Code = "ABC";
			terms.Description = (NoResString)"XYZ";
			terms.DeliveryMode = nameof(PrintCopyType.ALL);
			terms.Image = new Bitmap(10, 10);

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			HouseBillOfLadingTermsAndConditionsCollection collection1 = (HouseBillOfLadingTermsAndConditionsCollection)setValue;
			HouseBillOfLadingTermsAndConditionsCollection collection2 = (HouseBillOfLadingTermsAndConditionsCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Code", i.ToString()), collection1[i].Code, collection2[i].Code);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Description", i.ToString()), collection1[i].Description, collection2[i].Description);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].DeliveryMode", i.ToString()), collection1[i].DeliveryMode, collection2[i].DeliveryMode);
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
