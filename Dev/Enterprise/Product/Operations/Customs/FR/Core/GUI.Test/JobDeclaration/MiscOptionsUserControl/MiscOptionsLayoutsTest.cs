using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	public void TestItineraryCountriesSeparatorUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
	}

	public void TestItineraryCountriesUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
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
			yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);

			yield return (MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.DefermentAccountNumberDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.CustomsGuaranteeNumberDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.VATDeferTypeDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.VATDeferNumberTextBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.VatCanaDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.ChargePaymentOrDestinationIDsDropEdit, ControlWidthClass.Long);

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
