using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(PrincipalBrandingRegistryItemEditor))]
	sealed class PrincipalBrandingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PrincipalBrandingRegistryItemEditor(RegistryItem.DataType,
				NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PrincipalBrandingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			PrincipalBrandingCollection collection = new PrincipalBrandingCollection(Factory, NewFallbackLevel());

			PrincipalBranding branding = collection.AddNew();
			branding.Code = "COD";
			branding.BrandName = "Brand";
			branding.BrandEmailAddress = "blah@brand.com";
			branding.Image = new Bitmap(10, 10);

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PrincipalBrandingControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PrincipalBrandingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
