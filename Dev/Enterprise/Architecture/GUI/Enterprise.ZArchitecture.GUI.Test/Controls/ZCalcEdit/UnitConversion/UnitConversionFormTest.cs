using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class UnitConversionFormTest : TestCaseWithFactory
	{
		public void TestDefaults()
		{
			using (var calcEdit = new ZCalcEdit())
			using (var calcEditCore = new ZCalcEditCore(calcEdit))
			{
				calcEditCore.Decimals = 1;
				calcEditCore.MaxValue = 99999.9m;

				var dummy = Factory.New<DummyUnitConversionParent>();
				var amountDescriptor = TypeDescriptor.GetProperties(dummy)["WeightAmount1"];
				var unitDescriptor = TypeDescriptor.GetProperties(dummy)["WeightUnit"];

				var unitConversion = UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, MeasureUnitType.Weight);

				using (var form = new UnitConversionForm(unitConversion, calcEditCore))
				{
					var fromCalcEdit = form.Controls["fromGroupBox"].Controls.OfType<ZCalcEdit>().First();
					var toCalcEdit = form.Controls["toGroupBox"].Controls.OfType<ZCalcEdit>().First();

					AssertEquals(1, fromCalcEdit.Decimals);
					AssertEquals(1, toCalcEdit.Decimals);
					AssertEquals(99999.9m, fromCalcEdit.MaxValue);
					AssertEquals(99999.9m, toCalcEdit.MaxValue);
				}
			}
		}

		public void TestCommitChanges()
		{
			using (var calcEdit = new ZCalcEdit())
			using (var calcEditCore = new ZCalcEditCore(calcEdit))
			{
				var dummy = Factory.New<DummyUnitConversionParent>();
				var descriptors = TypeDescriptor.GetProperties(dummy);
				var amount1Descriptor = descriptors["WeightAmount1"];
				var amount2Descriptor = descriptors["WeightAmount2"];
				var unitDescriptor = descriptors["WeightUnit"];

				var unitConversion = UnitConversion.Create(dummy, new[] { amount1Descriptor, amount2Descriptor }, unitDescriptor, MeasureUnitType.Weight);

				using (var form = new UnitConversionForm(unitConversion, calcEditCore))
				{
					form.Show();

					var okButton = (ZButton)form.Controls["okButton"];

					okButton.PerformClick();
					Assert(unitConversion.HasErrors);

					unitConversion.FromUnit = Constants.Weight.Grams;
					unitConversion.UnitAmountContainer["From_WeightAmount1"] = 1250;
					unitConversion.UnitAmountContainer["From_WeightAmount2"] = 1500;
					unitConversion.ToUnit = Constants.Weight.Kilograms;

					okButton.PerformClick();
				}

				AssertEquals(Constants.Weight.Kilograms, dummy.WeightUnit);
				AssertEquals(new ZDecimal(1.25), dummy.WeightAmount1);
				AssertEquals(new ZDecimal(1.5), dummy.WeightAmount2);
			}
		}
	}

	[TestedType(typeof(UnitConversionForm))]
	public class UnitConversionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var caltEdit = new ZCalcEdit();
			var calcEditCore = new ZCalcEditCore(caltEdit)
			{
				Decimals = 1,
				MaxValue = 99999.9m
			};

			var dummy = Factory.New<DummyUnitConversionParent>();
			var amountDescriptor = TypeDescriptor.GetProperties(dummy)["WeightAmount1"];
			var unitDescriptor = TypeDescriptor.GetProperties(dummy)["WeightUnit"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, MeasureUnitType.Weight);

			var form = new UnitConversionForm(unitConversion, calcEditCore);
			form.Disposed += (sender, args) =>
			{
				calcEditCore.Dispose();
				caltEdit.Dispose();
			};

			return form;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
