using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
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
	[TestedType(typeof(DeliveryOrderRegistryItemEditor))]
	sealed class DeliveryOrderRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DeliveryOrderRegistryItemEditor(RegistryItem.DataType,
				NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DeliveryOrderUserControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallbackLevel(), Factory);

			DeliveryOrder deliveryOrder = collection.AddNew();
			deliveryOrder.PrincipalPK = Principal.PK;
			deliveryOrder.PrintParameter = DeliveryOrder.PrintConstants.Code.PCT;
			deliveryOrder.Image = new Bitmap(10, 10);

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DeliveryOrderUserControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DeliveryOrderCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = CreatePrincipal("Principal1");
				}

				return principal;
			}
		}

		OrgHeader principal;

		OrgHeader CreatePrincipal(ZString code)
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = code;
			principal.OH_IsShippingProvider = true;

			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			return Factory.Load<OrgHeader>(principal.PK);
		}

		#endregion
	}
}
