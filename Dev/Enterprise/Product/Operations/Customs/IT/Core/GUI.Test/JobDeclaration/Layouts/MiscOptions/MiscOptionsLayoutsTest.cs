using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	public void TestSubscriberDropEditVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("Visible when IsUCC6 = false", true, layout.IsVisible(MiscOptionsControlBag.Instance.SubscriberDropEdit, declaration));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("Not Visible when IsUCC6 = true", false, layout.IsVisible(MiscOptionsControlBag.Instance.SubscriberDropEdit, declaration));
		}
	});

	public void TestPreClearingCheckBoxVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		AssertEquals("Visible when Import and TransportMode=Sea", true, layout.IsVisible(MiscOptionsControlBag.Instance.PreClearingCheckBox, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Not Visible when Export and TransportMode=Sea", false, layout.IsVisible(MiscOptionsControlBag.Instance.PreClearingCheckBox, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals("Not Visible when Import and TransportMode=Air", false, layout.IsVisible(MiscOptionsControlBag.Instance.PreClearingCheckBox, declaration));
	});

	public void TestItineraryCountriesSeparatorUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
	}

	public void TestItineraryCountriesUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
	}

	protected override int ControlBagCount => 3;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.PreClearingCheckBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.BadgeCodeDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.SubscriberDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Long);

			yield return (EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.DefermentAccountNumberDropEdit, ControlWidthClass.Long);

			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
			yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongControl);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();
}
