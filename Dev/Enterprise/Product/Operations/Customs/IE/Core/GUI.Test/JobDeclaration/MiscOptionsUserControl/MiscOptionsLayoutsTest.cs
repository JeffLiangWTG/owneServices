using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
		public void TestRouteFRequestedCheckBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("RouteFRequestedCheckBox Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("RouteFRequestedCheckBox Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, declaration));
		}

		public void TestTrainingCheckBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("TrainingCheckBox Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("TrainingCheckBox Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, declaration));
		}

		public void TestPaymentMethodDropEditVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("PaymentSeparatorUserControl Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));
			AssertEquals("PaymentMethodDropEdit Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("PaymentSeparatorUserControl Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));
			AssertEquals("PaymentMethodDropEdit Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration));
		}

		public void TestItineraryCountriesUserControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("ItineraryCountriesSeparatorUserControl Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
			AssertEquals("ItineraryCountriesUserControl Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("ItineraryCountriesSeparatorUserControl Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
			AssertEquals("ItineraryCountriesUserControl Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
		}

		public void TestDefermentAccountNumberTextBoxVisibility()
		{
			var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.E;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Not visible in export declaration", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.A;
			AssertEquals("Not visible if paymentMethod is not E", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.E;
			AssertEquals("Visible in import declaration and paymentMethod is E", true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));
		}

		protected override int ControlBagCount => 3;

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
				yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Medium);
				yield return (CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, ControlWidthClass.Long);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, ControlWidthClass.Long);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
