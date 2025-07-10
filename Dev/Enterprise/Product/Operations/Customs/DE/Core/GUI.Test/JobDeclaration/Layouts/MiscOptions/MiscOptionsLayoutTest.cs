using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayout))]
	sealed class MiscOptionsLayoutTest : LayoutsAbstractTest
	{
		public void TestStatisticStatusDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(MiscOptionsLayoutControlBag.Instance.StatisticStatusDropEdit);
		}

		public void TestVATClaimBackDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(MiscOptionsLayoutControlBag.Instance.VATClaimBackDropEdit);
		}

		public void TestDeferralSeparatorUserControlVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl);
		}

		public void TestPaymentMethodDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit);
		}

		public void TestDutyAccountNumberDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(MiscOptionsLayoutControlBag.Instance.DutyAccountNumberDropEdit);
		}

		public void TestVatPaymentPartyDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(MiscOptionsLayoutControlBag.Instance.VatPaymentPartyDropEdit);
		}

		public void TestVATAccountNumberDropEditVisibility()
		{
			AssertControlIsVisibleForImportDeclaration(MiscOptionsLayoutControlBag.Instance.VATAccountNumberDropEdit);
		}

		public void TestItineraryCountriesSeparatorUserControlVisibility()
		{
			var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Visible in export declaration", true, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Not visible in import declaration", false, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
		}

		public void TestItineraryCountriesUserControlVisibility()
		{
			var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Visible in export declaration", true, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Not visible in import declaration", false, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
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
				yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsLayoutControlBag.Instance.StatisticStatusDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsLayoutControlBag.Instance.VATClaimBackDropEdit, ControlWidthClass.Long);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsLayoutControlBag.Instance.DutyAccountNumberDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsLayoutControlBag.Instance.VatPaymentPartyDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsLayoutControlBag.Instance.VATAccountNumberDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<EU.Business.Declaration.JobDeclaration>();

		void AssertControlIsVisibleForImportDeclaration(ControlReference control)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new MiscOptionsLayout()).Layout;

			CombineAssertions(() =>
			{
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import declaration", true, layout.IsVisible(control, jobDeclaration));

				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export declaration", false, layout.IsVisible(control, jobDeclaration));
			});
		}
	}
}
