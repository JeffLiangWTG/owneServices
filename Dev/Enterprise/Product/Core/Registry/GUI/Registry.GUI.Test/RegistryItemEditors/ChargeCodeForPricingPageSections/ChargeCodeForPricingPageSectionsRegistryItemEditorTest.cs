using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsRegistryItemEditor))]
	sealed class ChargeCodeForPricingPageSectionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ChargeCodeForPricingPageSectionsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ChargeCodeForPricingPageSectionsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ChargeCodeForPricingPageSectionsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ChargeCodeForPricingPageSectionsRegistryItem("", null, null, null, RegistryStorageFlags.System, new ChargeCodeForPricingPageSectionsConfigurationCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ChargeCodeForPricingPageSectionsConfigurationCollection();

			var chargeCodeForPricingPageSectionsConfiguration = collection.AddNew();
			chargeCodeForPricingPageSectionsConfiguration.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			chargeCodeForPricingPageSectionsConfiguration.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			var charge = chargeCodeForPricingPageSectionsConfiguration.Charges.AddNew();
			charge.ChargeCodePK = new BusinessObjectFactory().LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
