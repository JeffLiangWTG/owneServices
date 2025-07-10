using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Medium);
				yield return (MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Medium);
				yield return (CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
				yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongControl);
			}
		}
	}
}
