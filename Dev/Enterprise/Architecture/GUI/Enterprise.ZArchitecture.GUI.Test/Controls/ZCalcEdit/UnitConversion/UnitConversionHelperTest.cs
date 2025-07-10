using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UnitConversionHelperTest : TestCaseWithFactory
	{
		public void TestShowMeasureUnitConversion()
		{
			using (var calcEdit = new ZCalcEdit())
			using (var calcEditCore = new ZCalcEditCore(calcEdit))
			{
				var dummy = Factory.New<DummyUnitConversionParent>();

				dummy.WeightAmount1 = 32.2;
				dummy.WeightAmount2 = 45.5;
				dummy.WeightAmount4 = 58.1;
				dummy.WeightUnit = Constants.Weight.Kilograms;

				var helper = new UnitConversionHelper(calcEditCore);
				helper.ShowMeasureUnitConversion(dummy, "WeightAmount1");

				var unitConversion = (UnitConversion)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				object o;

				AssertEquals(new ZDecimal(32.2), unitConversion.UnitAmountContainer["From_WeightAmount1"]);
				AssertEquals(new ZDecimal(45.5), unitConversion.UnitAmountContainer["From_WeightAmount2"]);
				AssertExceptionThrown("WeightAmount4 is not exposed", typeof(ArgumentException), () => o = unitConversion.UnitAmountContainer["From_WeightAmount4"]);
				AssertEquals(Constants.Weight.Kilograms, unitConversion.FromUnit);

				dummy.IsAmount4Exposed = true;

				helper.ShowMeasureUnitConversion(dummy, "WeightAmount1");
				unitConversion = (UnitConversion)ZFormModaliser.LastIBusinessShownOnDialogForTest;

				AssertEquals(new ZDecimal(32.2), unitConversion.UnitAmountContainer["From_WeightAmount1"]);
				AssertEquals(new ZDecimal(45.5), unitConversion.UnitAmountContainer["From_WeightAmount2"]);
				AssertEquals(new ZDecimal(58.1), unitConversion.UnitAmountContainer["From_WeightAmount4"]);
				AssertEquals(Constants.Weight.Kilograms, unitConversion.FromUnit);

				var parentDummy = Factory.New<DummyUnitConversionParent>();
				parentDummy.VolumeAmout2 = 5;
				parentDummy.VolumeUnit2 = Constants.Volume.CubicMetres;
				dummy.DummyParent = parentDummy;

				helper.ShowMeasureUnitConversion(dummy, "DummyParent+VolumeAmout2");
				unitConversion = (UnitConversion)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals(5m, unitConversion.UnitAmountContainer["From_VolumeAmout2"]);
				AssertEquals(Constants.Volume.CubicMetres, unitConversion.FromUnit);
			}
		}

		[ExpectNoExceptions]
		public void TestShowMeasureUnitConversion_AmbigousMatchException()
		{
			using (var calcEdit = new ZCalcEdit())
			using (var calcEditCore = new ZCalcEditCore(calcEdit))
			{
				var dummyBO = Factory.New<TestUnitConversionBusinessObjectParent>();
				var testDummyUnitConversion = Factory.New<TestDummyUnitConversion>();
				testDummyUnitConversion.WeightAmount1 = 1.0m;
				dummyBO.DummyParent = testDummyUnitConversion;

				var helper = new UnitConversionHelper(calcEditCore);
				helper.ShowMeasureUnitConversion(dummyBO, "DummyParent+WeightAmount1");
			}
		}

		[ExpectNoExceptions]
		public void TestShowMeasureUnitConversion_NoException()
		{
			using (var calcEdit = new ZCalcEdit())
			using (var calcEditCore = new ZCalcEditCore(calcEdit))
			{
				var dummyBO = Factory.New<TestUnitConversionBusinessObjectParent>();
				var testDummyUnitConversion = new TestObjectUnitConversion();
				testDummyUnitConversion.WeightAmount1 = 1.0m;
				dummyBO.ObjectParent = testDummyUnitConversion;

				var helper = new UnitConversionHelper(calcEditCore);
				helper.ShowMeasureUnitConversion(dummyBO, "ObjectParent+WeightAmount1");
			}
		}

		void TestInputFieldResponsive(bool isDirectBinding)
		{
			using (var form = new ZForm())
			using (var userControl = new ZUserControl())
			using (var calcDropEdit = new ZCalcDropEdit())
			{
				var dummyBO = Factory.New<TestUnitConversionBusinessObjectParent>();
				dummyBO.DummyParent = Factory.New<TestDummyUnitConversion>();
				dummyBO.DummyParent.Weight = 1.0m;
				dummyBO.DummyParent.Unit = Constants.Weight.Kilograms;

				if (!isDirectBinding)
				{
					calcDropEdit.BindToAmount = "DummyParent+Weight";
					calcDropEdit.BindToUnit = "DummyParent+Unit";
				}
				else
				{
					calcDropEdit.BindToAmount = "DummyParent.Weight";
					calcDropEdit.BindToUnit = "DummyParent.Unit";
				}

				calcDropEdit.Decimals = 3;
				calcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 35, true);
				calcDropEdit.Name = "calcDropEdit";
				calcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
				calcDropEdit.TabIndex = 1;
				calcDropEdit.UnitPreBoundMaxLength = 2;

				userControl.Controls.Add(calcDropEdit);
				userControl.BindingSource.SetBindingMember(calcDropEdit, ".");
				userControl.BindingSource.DataSource = dummyBO;
				form.Controls.Add(userControl);

				form.Show();
				System.Windows.Forms.Application.DoEvents();

				var amountCalcEdit = (ZCalcEdit)calcDropEdit.Controls.Find("AmountCalcEdit", true).First();
				var unitDropEdit = (ZDropEdit)calcDropEdit.Controls.Find("UnitDropEdit", true).First();
				AssertEquals("Precondition: calcDropEdit shows old value", "1.000", amountCalcEdit.Text);
				AssertEquals("Precondition: unitDropEdit shows old value", "KG", unitDropEdit.Text);

				using (ZFormModaliser.SuspendDispose())
				{
					amountCalcEdit.ShowUnitConverterHotkey(null, System.Windows.Forms.Keys.F5);
					System.Windows.Forms.Application.DoEvents();
					if (ZFormModaliser.LastFormShownDialogForTest is UnitConversionForm unitConversionForm)
					{
						unitConversionForm.Show();
						System.Windows.Forms.Application.DoEvents();

						var okButton = (ZButton)unitConversionForm.Controls["okButton"];
						okButton.PerformClick();
					}
				}

				AssertEquals("calcDropEdit shows new value", "2.205", amountCalcEdit.Text);
				AssertEquals("unitDropEdit shows new value", "LB", unitDropEdit.Text);
			}
		}

		public void TestInputFieldResponsiveIndirectBinding()
		{
			TestInputFieldResponsive(false);
		}

		public void TestInputFieldResponsiveDirectBinding()
		{
			TestInputFieldResponsive(true);
		}

		class TestUnitConversionBusinessObjectParent : TestUnitConversionBusinessObject
		{
			public TestUnitConversionBusinessObjectParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new TestDummyUnitConversion DummyParent { get; set; }

			public TestObjectUnitConversion ObjectParent { get; set; }
		}

		class TestUnitConversionBusinessObject : DummyBusinessObject
		{
			public TestUnitConversionBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DummyUnitConversionParent DummyParent { get; set; }
		}

		class TestDummyUnitConversion : DummyUnitConversionParent
		{
			public TestDummyUnitConversion(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

			public ZPropertyInfo UnitInfo => GetZPropertyInfo(nameof(Unit));

			[MeasureUnit("Unit", MeasureUnitType.Weight, null)]
			public ZDecimal Weight
			{
				get { return Z0_AnotherDecimal; }
				set { Z0_AnotherDecimal = value; }
			}

			[List("TestUnits")]
			public ZString Unit
			{
				get { return Z0_Description; }
				set { Z0_Description = value; }
			}
		}

		class TestObjectUnitConversion
		{
			public TestObjectUnitConversion()
			{
			}

			[MeasureUnit("WeightUnit", MeasureUnitType.Weight)]
			public ZDecimal WeightAmount1 { get; set; }
		}
	}
}
