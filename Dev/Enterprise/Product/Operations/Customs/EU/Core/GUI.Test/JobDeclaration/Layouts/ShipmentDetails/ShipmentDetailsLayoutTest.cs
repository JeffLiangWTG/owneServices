using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayouts))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestShipmentDetailsFieldsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions("When Declaration IsUcc6", () =>
				{
					AssertEquals("Agreed Place Code User Control", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
					AssertEquals("Agreed Place Code Find Box", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
					AssertEquals("Inco Term Place Text Box", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				CombineAssertions("When Declaration Is not Ucc6", () =>
				{
					AssertEquals("Agreed Place Code User Control", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
					AssertEquals("Agreed Place Code Find Box", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
					AssertEquals("Inco Term Place Text Box", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));
				});
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.GoodsLocationDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsCountUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.AgentsReferenceTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = new ShipmentDetailsLayouts().Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
