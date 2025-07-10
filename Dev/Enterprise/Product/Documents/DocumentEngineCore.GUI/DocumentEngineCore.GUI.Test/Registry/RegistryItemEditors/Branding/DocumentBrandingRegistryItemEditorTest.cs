using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocumentBrandingRegistryItemEditor))]
	sealed class DocumentBrandingRegistryItemEditorTest : ClientAndAgentBrandingRegistryItemEditorTest
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DocumentBrandingRegistryItemEditor(RegistryItem.DataType,
				new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocumentBrandingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocumentBrandingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			ClientTariffAndLevelCollection collection = new ClientTariffAndLevelCollection();
			var element = (DocumentBrandingBusinessObject)collection.AddNew();
			element.CodeList.AddPair("ABC", "");
			element.Code = "ABC";
			element.Description = (NoResString)"Desc";
			element.Image = new Bitmap(1, 1);
			element.BrandEmailAddress = "brandEmailAddress@edi.com.au";
			element.BrandName = "Brand Name";

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			base.AssertSetAndGetValuesEqual(setValue, getValue);
			ClientTariffAndLevelCollection expectedCollection = (ClientTariffAndLevelCollection)setValue;
			ClientTariffAndLevelCollection actualCollection = (ClientTariffAndLevelCollection)getValue;

			for (int i = 0; i < expectedCollection.Count; ++i)
			{
				AssertEquals(string.Format("ActualCollection[{0}].BrandName", i), expectedCollection[i].BrandName, actualCollection[i].BrandName);
				AssertEquals(string.Format("ActualCollection[{0}].BrandEmailAddress", i), expectedCollection[i].BrandEmailAddress, actualCollection[i].BrandEmailAddress);
			}
		}
	}
}
