using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(LocalTransportCompanyBrandingRegistryItemEditor))]
	sealed class LocalTransportCompanyBrandingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new LocalTransportCompanyBrandingRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LocalTransportCompanyBrandingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var company = Factory.NewWithValidTestData<OrgHeader>();
			company.OH_Code = "LOC";
			company.OH_IsLocalTransport = true;
			company.OH_IsShippingProvider = true;

			var collection = new LocalTransportCompanyBrandingCollection(NewFallbackLevel(), Factory);

			var branding = collection.AddNew();
			branding.LocalTransportCompanyPK = company.PK;
			branding.Image = new Bitmap(10, 10);

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LocalTransportCompanyBrandingControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LocalTransportCompanyBrandingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		#endregion
	}
}
