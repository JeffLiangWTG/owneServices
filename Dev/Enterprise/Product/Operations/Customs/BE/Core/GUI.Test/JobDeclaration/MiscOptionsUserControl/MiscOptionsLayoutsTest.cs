using System.Collections.Generic;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	public void TestPaidByDropEditVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertEquals("Visible when JE_ApplicationCode = 'ITF'", true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaidByDropEdit, declaration));

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertEquals("Visible when JE_ApplicationCode != 'ITF'", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaidByDropEdit, declaration));
	});

	public void TestVATDeferTypeDropEditVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertEquals("Visible when JE_ApplicationCode = 'ITF'", true, layout.IsVisible(MiscOptionsControlBag.Instance.VATDeferTypeDropEdit, declaration));

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertEquals("Visible when JE_ApplicationCode != 'ITF'", false, layout.IsVisible(MiscOptionsControlBag.Instance.VATDeferTypeDropEdit, declaration));
	});

	public void TestItineraryCountriesSeparatorUserControlVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
	});

	public void TestItineraryCountriesUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
	}

	public void TestDefermentAccountNumberTextBoxVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.Deferral;
			AssertEquals("Not visible in export declaration", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.Cheque;
			AssertEquals("Not visible if paymentMethod is not E", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.Deferral;
			AssertEquals("Visible in import declaration and paymentMethod is E", true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.AgentCashAccount;
			AssertEquals("Visible in import declaration and paymentMethod is P", true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));
		});
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

			yield return (EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.VATDeferTypeDropEdit, ControlWidthClass.Long);

			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();
}
