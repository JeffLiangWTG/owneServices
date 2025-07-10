using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportBorderLayout))]
	sealed class TransportBorderLayoutTest : LayoutsAbstractTest
	{
		public void TestBorderTransportTypeOfIdDropEditVisibility()
		{
			AssertControlVisibilityForModes(TransportBorderControlBag.Instance.BorderTransportTypeOfIdDropEdit);
		}

		public void TestBorderTransportIdAndNationalityUserControlVisibility()
		{
			AssertControlVisibilityForModes(TransportBorderControlBag.Instance.BorderTransportIdAndNationalityUserControl);
		}

		public void TestBorderConveyanceNumberTextBoxVisibility()
		{
			AssertControlVisibilityForModes(TransportBorderControlBag.Instance.BorderConveyanceNumberTextBox);
		}

		public void TestBorderOfficeDropEditVisibility()
		{
			AssertControlVisibilityForModes(TransportBorderControlBag.Instance.BorderOfficeDropEdit);
		}

		public void TestAdditionalTransportBorderUserControlVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			var moveHeader = nctsHeader.MovementHeader;
			var layout = ((IPanelLayoutProvider)new TransportBorderLayout()).Layout;
			var control = TransportBorderControlBag.Instance.AdditionalTransportBorderUserControl;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals($"AdditionalTransportBorderUserControl in transition period", false, layout.IsVisible(control, moveHeader));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals($"AdditionalTransportBorderUserControl off transition period", true, layout.IsVisible(control, moveHeader));
			}
		}

		public void TestAdditionalTransportBorderUserControlVisibilityForModes()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var layout = ((IPanelLayoutProvider)new TransportBorderLayout()).Layout;
			var control = TransportBorderControlBag.Instance.AdditionalTransportBorderUserControl;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertControlVisibilityForModes(control);
			}
		}

		public void TestBorderTransportIdAndNationalityUserControlBehaviour()
		{
			var layout = ((IPanelLayoutProvider)new TransportBorderLayout()).Layout;
			var borderTransportIdAndNationalityUserControlReference = TransportBorderControlBag.Instance.BorderTransportIdAndNationalityUserControl;
			AssertEquals("Has BorderTransportIdAndNationalityUserControlCharacterCasingBehaviour",
				true,
				layout.HasBehaviourByBehaviourType(borderTransportIdAndNationalityUserControlReference,
				typeof(BorderTransportIdAndNationalityUserControlBehaviour)));
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportBorderLayoutBuilder<NctsDepartureMovementHeader>();

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
				yield return (TransportBorderControlBag.Instance.BorderTransportModeDropEdit, ControlWidthClass.Long);
				yield return (TransportBorderControlBag.Instance.BorderTransportTypeOfIdDropEdit, ControlWidthClass.Long);
				yield return (TransportBorderControlBag.Instance.BorderTransportIdAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (TransportBorderControlBag.Instance.BorderConveyanceNumberTextBox, ControlWidthClass.Long);
				yield return (TransportBorderControlBag.Instance.BorderOfficeDropEdit, ControlWidthClass.Long);
				yield return (TransportBorderControlBag.Instance.AdditionalTransportBorderUserControl, ControlWidthClass.Long);
			}
		}

		void AssertControlVisibilityForModes(ControlReference control)
		{
			AssertControlVisibility(control,
				new[] {
					ModeOfTransportList.Codes._1_SeaTransport,
					ModeOfTransportList.Codes._2_RailTransport,
					ModeOfTransportList.Codes._3_RoadTransport,
					ModeOfTransportList.Codes._4_AirTransport,
					ModeOfTransportList.Codes._7_FixedTransportInstallations,
					ModeOfTransportList.Codes._8_InlandWaterwayTransport,
					ModeOfTransportList.Codes._9_OwnPropulsion
				},
				new[] {
					string.Empty,
					ModeOfTransportList.Codes._5_PostalConsignment
				});
		}

		void AssertControlVisibility(ControlReference control, string[] visibleModes, string[] hiddenModes)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			var layout = ((IPanelLayoutProvider)new TransportBorderLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var transportMode in visibleModes)
				{
					movementHeader.BM_ExportTransportMode = transportMode;
					AssertEquals($"BM_ExportTransportMode = '{transportMode}'", true, layout.IsVisible(control, movementHeader));
				}

				foreach (var transportMode in hiddenModes)
				{
					movementHeader.BM_ExportTransportMode = transportMode;
					AssertEquals($"BM_ExportTransportMode = '{transportMode}'", false, layout.IsVisible(control, movementHeader));
				}
			});
		}
	}
}
