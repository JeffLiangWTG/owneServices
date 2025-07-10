using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayout))]
sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestRegionOrTerritoryOfDestinationCodeFindBoxVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for IMP, H1 activate and JE_GoodsDestination = ES", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox Visible for IMP, H1 activate and JE_GoodsDestination = DE", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for IMP, H1 activate and JE_GoodsDestination = XC", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox Visible for IMP, H1 activate and JE_GoodsDestination = IT", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for IMP, H1 activate and JE_GoodsDestination = XL", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox Visible for IMP, H1 activate and JE_GoodsDestination = FR", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
			declaration.JE_GoodsDestination = ZString.Empty;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for IMP, H1 activate and JE_GoodsDestination = Empty", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for EXP and H1 activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for EXP and H1 not activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("RegionOrTerritoryOfDestinationCodeFindBox not Visible for IMP and H1 not activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, declaration));
		}
	});

	public void TestRegionOrTerritoryOfDestinationDropEditVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit Visible for IMP, H1 activate and JE_GoodsDestination = ES", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for IMP, H1 activate and JE_GoodsDestination = DE", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit Visible for IMP, H1 activate and JE_GoodsDestination = XC", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for IMP, H1 activate and JE_GoodsDestination = IT", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit Visible for IMP, H1 activate and JE_GoodsDestination = XL", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for IMP, H1 activate and JE_GoodsDestination = FR", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
			declaration.JE_GoodsDestination = ZString.Empty;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit Visible for IMP, H1 activate and JE_GoodsDestination = Empty", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for EXP and H1 activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for EXP and H1 not activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("RegionOrTerritoryOfDestinationDropEdit not Visible for IMP and H1 not activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, declaration));
		}
	});

	public void TestDestinationStateDropEditVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("DestinationStateDropEdit not Visible for IMP and H1 activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.DestinationStateDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("DestinationStateDropEdit not Visible for EXP and H1 activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.DestinationStateDropEdit, declaration));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("DestinationStateDropEdit not Visible for EXP and H1 not activate", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.DestinationStateDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("DestinationStateDropEdit Visible for IMP and H1 not activate", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.DestinationStateDropEdit, declaration));
		}
	});

	public void TestShipmentDetailsFieldsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration IsUcc6", () =>
			{
				AssertEquals("Agreed Place Code User Control", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("Agreed Place Code Find Box", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
				AssertEquals("Inco Term Place Text Box", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));

				declaration.JE_ShipmentIncoTerm = "XXX";
				AssertEquals("Agreed Place Code Find Box", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("When Declaration Is not Ucc6", () =>
			{
				AssertEquals("Agreed Place Code User Control", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("Agreed Place Code Find Box", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
				AssertEquals("Inco Term Place Text Box", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));
			});
		}
	}

	public void TestPartialWriteoffCheckBoxVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions("When Declaration Import", () =>
		{
			AssertEquals("PartialWriteoffCheckBox", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.PartialWriteoffCheckBox, declaration));
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions("When Declaration Is not Import", () =>
		{
			AssertEquals("PartialWriteoffCheckBox", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.PartialWriteoffCheckBox, declaration));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.PartialWriteoffCheckBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.DestinationStateDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.RegionOrTerritoryOfDestinationCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.AgentsReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		}
	}

	PanelLayout Layout => layout ?? (layout = new ShipmentDetailsLayout().Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();

	PanelLayout layout;
}
