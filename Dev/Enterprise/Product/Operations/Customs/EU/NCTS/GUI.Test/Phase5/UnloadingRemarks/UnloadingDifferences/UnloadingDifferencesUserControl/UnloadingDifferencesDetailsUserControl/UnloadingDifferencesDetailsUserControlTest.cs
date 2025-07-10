using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class UnloadingDifferencesDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsArrivalMovementHeader), userControl.BindingSource.DataSourceType);
		}

		[RequiresSTA]
		public void TestDeclaredValueLabel()
		{
			AssertZLabel(userControl.DeclaredValueLabel, "Declared Value");
		}

		public void TestUnloadedValueLabel()
		{
			AssertZLabel(userControl.UnloadedValueLabel, "Unloaded Value");
		}

		public void TestTotalGrossMassDeclaredValueCalcEdit()
		{
			AssertZCalcEdit(userControl.TotalGrossMassDeclaredValueCalcEdit, nameof(NctsArrivalMovementHeader.TotalGrossMassInKilograms), 6);
		}

		public void TestEffectiveGrossWeightUnloadedCalcEdit()
		{
			AssertZCalcEdit(userControl.EffectiveGrossWeightUnloadedCalcEdit, nameof(NctsArrivalMovementHeader.EffectiveGrossWeightUnloaded), 6);
		}

		public void TestTotalPackagesDeclaredValueCalcEdit()
		{
			AssertZCalcEdit(userControl.TotalPackagesDeclaredValueCalcEdit, nameof(NctsArrivalMovementHeader.TotalNumberOfPackages), 2);
		}

		public void TestTotalPackagesUnloadedValueCalcEdit()
		{
			AssertZCalcEdit(userControl.TotalPackagesUnloadedValueCalcEdit, nameof(NctsArrivalMovementHeader.TotalUnloadedNumberOfPackages), 2);
		}

		public void TestRecalculateTotalsButton()
		{
			var recalculateTotalsButton = userControl.RecalculateTotalsButton;
			CombineAssertions(() =>
			{
				AssertType<ZButton>("Type", recalculateTotalsButton);
				AssertEquals("Caption", "Recalculate Totals", recalculateTotalsButton.CaptionResourceString.Caption);
			});
		}

		public void TestInlandTransportModeDropEdit()
		{
			var inlandTransportModeDropEdit = userControl.InlandTransportModeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", inlandTransportModeDropEdit);
				AssertEquals("BindTo", nameof(NctsArrivalMovementHeader.BM_InlandTransportMode), inlandTransportModeDropEdit.BindTo);
			});
		}

		[RequiresSTA]
		public void TestSeparatorLabel()
		{
			var control = userControl.SeparatorLabel;
			CombineAssertions(() =>
			{
				AssertZLabel(control, null);
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(control));
			});
		}

		public void TestRecalculateTotalsButton_Click()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_GrossWeightUnloaded = 1;
			var grossWeightUnloadedValueChanged = false;
			var totalGrossMassInKilogramsValueChanged = false;
			var totalNumberOfPackagesValueChanged = false;
			var totalUnloadedNumberOfPackagesValueChanged = false;
			movementHeader.TotalGrossMassInKilogramsInfo.ValueChanged += delegate
			{ totalGrossMassInKilogramsValueChanged = true; };
			movementHeader.TotalNumberOfPackagesInfo.ValueChanged += delegate
			{ totalNumberOfPackagesValueChanged = true; };
			movementHeader.BM_GrossWeightUnloadedInfo.ValueChanged += delegate
			{ grossWeightUnloadedValueChanged = true; };
			movementHeader.TotalUnloadedNumberOfPackagesInfo.ValueChanged += delegate
			{ totalUnloadedNumberOfPackagesValueChanged = true; };

			CombineAssertions(() =>
			{
				using (var form = new ZForm(movementHeader))
				{
					using (var control = new UnloadingDifferencesDetailsUserControl())
					{
						form.Controls.Add(control);
						form.Show();

						control.RecalculateTotalsButton.PerformClick();
						AssertEquals("TotalGrossMassInKilogramsValueChanged fired", expected: true, totalGrossMassInKilogramsValueChanged);
						AssertEquals("TotalNumberOfPackagesValueChanged fired", expected: true, totalNumberOfPackagesValueChanged);
						AssertEquals("GrossWeightUnloadedValueChanged fired", expected: true, grossWeightUnloadedValueChanged);
						AssertEquals("TotalUnloadedNumberOfPackagesValueChanged fired", expected: true, totalUnloadedNumberOfPackagesValueChanged);
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new UnloadingDifferencesDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		UnloadingDifferencesDetailsUserControl userControl;

		void AssertZLabel(ZLabel control, string caption)
		{
			AssertEquals("Caption", caption, control.CaptionResourceString.Caption);
		}

		void AssertZCalcEdit(ZCalcEdit control, string bindTo, int decimalPlaces)
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", bindTo, control.BindTo);
				AssertEquals("TextAlign", HorizontalAlignment.Right, control.TextAlign);
				AssertEquals(decimalPlaces, control.Decimals);
				AssertEquals(decimalPlaces, control.DecimalPlaces);
			});
		}
	}
}
