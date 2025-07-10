using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayout))]
sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestShipmentDetailsFieldsVisibility_WhenDeclarationIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions("When Declaration Is Import", () =>
		{
			AssertEquals("Agreed Place Code", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
			AssertEquals("Unloco Agreed Place Code", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, declaration));
			AssertEquals("Additional Delivery Terms", false, layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDeliveryTermsTextBox, declaration));
		});
	}

	public void TestShipmentDetailsFieldsVisibility_WhenDeclarationIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("When Declaration Is Export and MessageVersion is TXT", () =>
			{
				AssertEquals("Agreed Place Code", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("Unloco Agreed Place Code", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, declaration));
				AssertEquals("Additional Delivery Terms", false, layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDeliveryTermsTextBox, declaration));
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_ShipmentIncoTerm = "EXW";
			CombineAssertions("When Declaration Is Export and MessageVersion is XML", () =>
			{
				AssertEquals("Agreed Place Code", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("Unloco Agreed Place Code", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, declaration));
				AssertEquals("Additional Delivery Terms", false, layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDeliveryTermsTextBox, declaration));
			});

			declaration.JE_ShipmentIncoTerm = "XXX";
			CombineAssertions("When Declaration Is Export and MessageVersion is XML and IncoTerm is XXX", () =>
			{
				AssertEquals("Agreed Place Code", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("Unloco Agreed Place Code", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, declaration));
				AssertEquals("Additional Delivery Terms", true, layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDeliveryTermsTextBox, declaration));
			});
		}
	}

	public void TestLocationQualifierDropEditVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export, LocationQualifierDropEdit", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));

		CombineAssertions("When Declaration Is Import", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_LocationQualifier = GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection;
			AssertEquals("LocationQualifier is in lookups", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));

			declaration.JE_LocationQualifier = "X";
			AssertEquals("LocationQualifier is not in lookups", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));

			declaration.JE_LocationQualifier = "";
			AssertEquals("LocationQualifier is Empty", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));

			declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
			AssertEquals("Authorization not set up and LocationQualifier is in Qualifiers With Authorisation List", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));

			declaration.ZG_AuthorisationNumber = "1111CWP";
			declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
			AssertEquals("Authorization is set up and LocationQualifier is in Qualifiers With Authorisation List", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, declaration));
		});
	}

	public void TestGoodsLocationDUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export, GoodsLocationDUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationDUserControl, declaration));

		CombineAssertions("When Declaration Is Import", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("LocationQualifier is not D, GoodsLocationDUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationDUserControl, declaration));

			declaration.JE_LocationQualifier = "D";
			AssertEquals("LocationQualifier is D, GoodsLocationDUserControl", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationDUserControl, declaration));

			declaration.ZG_AuthorisationNumber = "1111CWP";
			AssertEquals("LocationQualifier is D, but authorization is set", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationDUserControl, declaration));
		});
	}

	public void TestGoodsLocationFUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export, GoodsLocationFUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFUserControl, declaration));

		CombineAssertions("When Declaration Is Import", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("LocationQualifier is not F, GoodsLocationFUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFUserControl, declaration));

			declaration.JE_LocationQualifier = "F";
			AssertEquals("LocationQualifier is F, GoodsLocationFUserControl", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFUserControl, declaration));

			declaration.ZG_AuthorisationNumber = "1111CWP";
			AssertEquals("LocationQualifier is F, but authorization is set", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFUserControl, declaration));
		});
	}

	public void TestGoodsLocationFCUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export, GoodsLocationFCUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFCUserControl, declaration));

		CombineAssertions("When Declaration Is Import", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("LocationQualifier is not FC, GoodsLocationFCUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFCUserControl, declaration));

			declaration.JE_LocationQualifier = "FC";
			AssertEquals("LocationQualifier is FC, GoodsLocationFCUserControl", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFCUserControl, declaration));

			declaration.ZG_AuthorisationNumber = "1111CWP";
			AssertEquals("LocationQualifier is FC, but authorization is set", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationFCUserControl, declaration));
		});
	}

	public void TestGoodsLocationLBLCUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export, GoodsLocatinLBUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When Declaration Is Import and LocationQualifier is LB but authorization is not set, GoodsLocatinLBUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, declaration));

		CombineAssertions("When Declaration is import and Authorization is set", () =>
		{
			declaration.ZG_AuthorisationNumber = "1111CWP";

			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("LocationQualifier is not LB or LC", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, declaration));

			declaration.JE_LocationQualifier = "LB";
			AssertEquals("LocationQualifier is LB", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, declaration));

			declaration.JE_LocationQualifier = "LC";
			AssertEquals("LocationQualifier is LC", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, declaration));
		});
	}

	public void TestSubLocationTextBoxVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("When Declaration Is Import, SubLocationTextBox", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.SubLocationTextBox, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Non Ucc6 Export, SubLocationTextBox", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.SubLocationTextBox, declaration));

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("When Declaration Is Ucc6 Export, SubLocationTextBox", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.SubLocationTextBox, declaration));
		}
	}

	public void TestGoodsLocationDropEditVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("When Declaration Is Import, GoodsLocationDropEdit", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.GoodsLocationDropEdit, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export but not UCC6, GoodsLocationDropEdit", true, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.GoodsLocationDropEdit, declaration));

		declaration.MessageVersion = "XML";
		AssertEquals("When Declaration Is Export and UCC6, GoodsLocationDropEdit", false, Layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.GoodsLocationDropEdit, declaration));
	}

	public void TestLocationOfGoodsUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("When Declaration Is Import, LocationOfGoodsUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationOfGoodsUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Is Export but not UCC6, LocationOfGoodsUserControl", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationOfGoodsUserControl, declaration));

		declaration.MessageVersion = "XML";
		AssertEquals("When Declaration Is Export and UCC6, LocationOfGoodsUserControl", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.LocationOfGoodsUserControl, declaration));
	}

	public void TestShipmentDetailsIncoTermPlaceUserControl_HasJE_ShipmentIncoTermAsVisibilityDependency()
		=> AssertControlHasJE_ShipmentIncoTermAsVisibilityDependency(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl);

	public void TestShipmentDetailsUnlocoIncoTermsPlaceUserControl_HasJE_ShipmentIncoTermAsVisibilityDependency()
		=> AssertControlHasJE_ShipmentIncoTermAsVisibilityDependency(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl);

	void AssertControlHasJE_ShipmentIncoTermAsVisibilityDependency(ControlReference controlReference)
	{
		var declaration = Factory.New<JobDeclaration>();
		var visibilityDependencies = Layout.GetVisibilityDependencies(controlReference, declaration);
		Assert("JE_ShipmentIncoTerm Visibility Dependency is present", visibilityDependencies.Any(v => v.Name == nameof(JobDeclaration.JE_ShipmentIncoTerm)));
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.GoodsLocationDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.LocationQualifierDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.GoodsLocationDUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.GoodsLocationFUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.GoodsLocationFCUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.GoodsLocationLBLCUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.SubLocationTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.AdditionalDeliveryTermsTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.AgentsReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 3;

	PanelLayout Layout => layout ?? (layout = new ShipmentDetailsLayout().Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();

	PanelLayout layout;
}
