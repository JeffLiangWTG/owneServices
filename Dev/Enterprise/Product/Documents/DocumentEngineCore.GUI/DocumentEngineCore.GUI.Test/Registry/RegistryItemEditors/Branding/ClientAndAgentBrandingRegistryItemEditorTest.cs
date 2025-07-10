using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(ClientAndAgentBrandingRegistryItemEditor))]
	internal class ClientAndAgentBrandingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ClientAndAgentBrandingRegistryItemEditor(RegistryItem.DataType,
				new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ClientAndAgentBrandingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ClientAndAgentBrandingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ClientTariffAndLevelCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ClientTariffAndLevelCollection collection = new ClientTariffAndLevelCollection();
			ClientTariffAndLevel element = collection.AddNew();
			element.CodeList.AddPair("ABC", "");
			element.Code = "ABC";
			element.Description = (NoResString)"Desc";
			element.BrandName = "blah";
			element.BrandEmailAddress = "blah@blah.com";
			element.Image = new Bitmap(1, 1);

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			ClientTariffAndLevelCollection expectedCollection = (ClientTariffAndLevelCollection)setValue;
			ClientTariffAndLevelCollection actualCollection = (ClientTariffAndLevelCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", expectedCollection.Count, actualCollection.Count);

			for (int i = 0; i < expectedCollection.Count; ++i)
			{
				AssertEquals(string.Format("ActualCollection[{0}].Code", i), expectedCollection[i].Code, actualCollection[i].Code);
				AssertEquals(string.Format("ActualCollection[{0}].Description", i), expectedCollection[i].Description, actualCollection[i].Description);
				AssertEquals(string.Format("ActualCollection[{0}].Image was different from ExpectedCollection[{0}].Image", i), true, Utilities.IsImageEqual(expectedCollection[i].Image, actualCollection[i].Image));
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
