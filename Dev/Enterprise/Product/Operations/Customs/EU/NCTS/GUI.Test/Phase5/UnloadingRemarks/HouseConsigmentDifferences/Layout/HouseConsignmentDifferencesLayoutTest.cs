using System;
using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDifferencesLayout))]
	sealed class HouseConsignmentDifferencesLayoutTest : LayoutsAbstractTest
	{
		public void TestHouseDetailsVisibility()
		{
			CombineAssertions(() =>
			{
				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				AssertEquals("Field Visibility, if Unloaded State = DIF", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));

				nctsBill.MovementDetail.B9_UnloadedState = "DEC";
				AssertEquals("Field Visibility, if Unloaded State = DEC", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DEC", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DEC", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));

				nctsBill.MovementDetail.B9_UnloadedState = "NEW";
				AssertEquals("Field Visibility, if Unloaded State = NEW", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = NEW", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = NEW", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));

				nctsBill.MovementDetail.B9_UnloadedState = "MIS";
				AssertEquals("Field Visibility, if Unloaded State = MIS", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = MIS", false, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = MIS", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));

				nctsBill.MovementDetail.B9_UnloadedState = "DIF";
				nctsBill.Header.ArrivalMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = ROAD", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = ROAD", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = ROAD", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));

				nctsBill.Header.ArrivalMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = AIR", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = AIR", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, nctsBill));
				AssertEquals("Field Visibility, if Unloaded State = DIF & Transport Mode = AIR", true, Layout.IsVisible(HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, nctsBill));
			});
		}

		public void TestDeclaredValueLabelCaption()
		{
			var control = HouseConsignmentDifferencesControlBag.Instance.DeclaredValueLabel;

			CombineAssertions(() =>
			{
				nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				Layout.TryGetCaption(control, nctsBill, out var resourceStringData);
				AssertEquals("CSI_Status = 'DEC'", "Declared Value", resourceStringData.Caption);

				nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				Layout.TryGetCaption(control, nctsBill, out resourceStringData);
				AssertEquals("CSI_Status = 'NEW'", "New Value", resourceStringData.Caption);
			});
		}

		public void TestGetStatusLabelCaption() => CombineAssertions(() =>
		{
			nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals($"B9_UnloadedState = '{nctsBill.MovementDetail.B9_UnloadedState}'", "Declared Value", HouseConsignmentDifferencesLayout.GetStatusLabelCaption(nctsBill).Caption);

			nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals($"B9_UnloadedState = '{nctsBill.MovementDetail.B9_UnloadedState}'", "New Value", HouseConsignmentDifferencesLayout.GetStatusLabelCaption(nctsBill).Caption);
		});

		protected override int ControlBagCount => 1;

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentDifferencesGridUserControl);

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (HouseConsignmentDifferencesControlBag.Instance.SequenceNumberTextBox, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.SecurityCheckBox, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.HouseConsignmentTextBox, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.UnloadedStateDropEdit, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, ControlWidthClass.Auto);
			}
		}
		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Auto);
				yield return (HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, ControlWidthClass.Auto);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (HouseConsignmentDifferencesControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongControl);
				yield return (HouseConsignmentDifferencesControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsBill = nctsHeader.Bills.AddNew();
		}
		NctsBill nctsBill;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new HouseConsignmentDifferencesLayout()).Layout);
		PanelLayout layout;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentDifferencesLayoutBuilder<NctsBill>();
	}
}
