using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HouseBillOfLadingTypeCollectionRegistryItemEditor))]
	sealed class HouseBillOfLadingTypeCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HouseBillOfLadingTypeCollectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((HouseBillOfLadingTypeCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HouseBillOfLadingTypeCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new HouseBillOfLadingTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new HouseBillOfLadingTypeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			RegistryImageCollection imageCollection = new RegistryImageCollection();
			RegistryImage image = imageCollection.AddNew();
			image.Code = "OOO";
			image.Description = (NoResString)"XYZ";
			image.Image = new Bitmap(10, 10);

			FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, imageCollection);

			HouseBillOfLadingTermsAndConditionsCollection termsCollection = new HouseBillOfLadingTermsAndConditionsCollection();
			HouseBillOfLadingTermsAndConditions terms = termsCollection.AddNew();
			terms.Code = "WWW";
			terms.Description = (NoResString)"XYZ";
			terms.DeliveryMode = nameof(PrintCopyType.ALL);
			terms.Image = new Bitmap(10, 10);

			FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, termsCollection);

			HouseBillOfLadingTypeCollection collection = new HouseBillOfLadingTypeCollection();
			HouseBillOfLadingType type = collection.AddNew();
			type.Code = "ABC";
			type.Description = (NoResString)"XYZ";
			type.LogoCode = "OOO";
			type.TermsAndConditionsCode = "WWW";
			type.PrePrinted = true;
			type.PrintLogo = true;
			type.PrintLogoInFormBuilder = PrintLogoOptions.Codes.All;

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			HouseBillOfLadingTypeCollection collection1 = (HouseBillOfLadingTypeCollection)setValue;
			HouseBillOfLadingTypeCollection collection2 = (HouseBillOfLadingTypeCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Code", i.ToString()), collection1[i].Code, collection2[i].Code);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Description", i.ToString()), collection1[i].Description, collection2[i].Description);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].LogoCode", i.ToString()), collection1[i].LogoCode, collection2[i].LogoCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].TermsAndConditionsCode", i.ToString()), collection1[i].TermsAndConditionsCode, collection2[i].TermsAndConditionsCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PrePrinted", i.ToString()), collection1[i].PrePrinted, collection2[i].PrePrinted);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PrintLogo", i.ToString()), collection1[i].PrintLogo, collection2[i].PrintLogo);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PrintLogoInFormBuilder", i.ToString()), collection1[i].PrintLogoInFormBuilder, collection2[i].PrintLogoInFormBuilder);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
