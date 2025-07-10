using System.Collections.Generic;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayouts))]
sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestIncoTermFields_IsUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;

				AssertEquals("ShipmentDetailsIncoTermsUserControl visible", true, layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, declaration));
				AssertEquals("ShipmentDetailsIncoTermsPlaceUserControl visible", false, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("AgreedPlaceCodeFindBox visible", true, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
				AssertEquals("ShipmentIncoTermPlaceTextBox visible", true, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));
			}
		});
	}

	public void TestIncoTermFields_IsNotUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;

				AssertEquals("ShipmentDetailsIncoTermsUserControl visible", true, layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, declaration));
				AssertEquals("ShipmentDetailsIncoTermsPlaceUserControl visible", true, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, declaration));
				AssertEquals("AgreedPlaceCodeFindBox visible", false, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, declaration));
				AssertEquals("ShipmentIncoTermPlaceTextBox visible", false, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, declaration));
			}
		});
	}

	public void TestLocationOfGoodsCodeFindBoxVisibility_UCC6()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", true))
		{
			var layout = new ShipmentDetailsLayouts().Layout;
			var jobDeclaration = Factory.New<JobDeclaration>();
			var control = ShipmentDetailsControlBag.Instance.LocationOfGoodsCodeFindBox;

			CombineAssertions(() =>
			{
				AssertEquals("JE_ApplicationCode default to ITF", true, layout.IsVisible(control, jobDeclaration));

				jobDeclaration.JE_ApplicationCode = "XXX";
				AssertEquals("JE_ApplicationCode = 'XXX'", true, layout.IsVisible(control, jobDeclaration));

				jobDeclaration.JE_ApplicationCode = "BLT";
				AssertEquals("JE_ApplicationCode = 'BLT'", false, layout.IsVisible(control, jobDeclaration));
			});
		}
	}

	public void TestLocationOfGoodsCodeFindBoxVisibility_NotUCC6()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
		{
			var layout = new ShipmentDetailsLayouts().Layout;
			var jobDeclaration = Factory.New<JobDeclaration>();
			var control = ShipmentDetailsControlBag.Instance.LocationOfGoodsCodeFindBox;

			CombineAssertions(() =>
			{
				AssertEquals("JE_ApplicationCode empty", true, layout.IsVisible(control, jobDeclaration));

				jobDeclaration.JE_ApplicationCode = "XXX";
				AssertEquals("JE_ApplicationCode = 'XXX'", true, layout.IsVisible(control, jobDeclaration));

				jobDeclaration.JE_ApplicationCode = "BLT";
				AssertEquals("JE_ApplicationCode = 'BLT'", true, layout.IsVisible(control, jobDeclaration));
			});
		}
	}

	public void TestRegionOfDestinationDropEdit_IsUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export RegionOfDestinationDropEdit visible", false, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.RegionOfDestinationDropEdit, declaration));
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import RegionOfDestinationDropEdit visible", true, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.RegionOfDestinationDropEdit, declaration));
			}
		});
	}

	public void TestRegionOfDestinationDropEdit_IsNotUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;
				AssertEquals("RegionOfDestinationDropEdit visible", false, layout.IsVisible(EU.GUI.ShipmentDetailsControlBag.Instance.RegionOfDestinationDropEdit, declaration));
			}
		});
	}

	public void PresentationStartDateEdit_IsNotUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;
				AssertEquals("PresentationStartDateEdit visible", true, layout.IsVisible(ShipmentDetailsControlBag.Instance.PresentationStartDateEdit, declaration));
			}
		});
	}

	public void PresentationStartDateEdit_IsUCC6()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var layout = new ShipmentDetailsLayouts().Layout;
				AssertEquals("PresentationStartDateEdit visible", true, layout.IsVisible(ShipmentDetailsControlBag.Instance.PresentationStartDateEdit, declaration));
			}
		});
	}

	protected override int ControlBagCount => 3;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.RegionOfDestinationDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.LocationOfGoodsCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.AgentsReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.ShipmentDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.DeclarationLanguageDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.PresentationStartDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();
}
