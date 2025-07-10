using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(CusCalculationRuleForm))]
	sealed class CusCalculationRuleFormTest : ZFormBasherTest
	{
		public void TestFomCaption()
		{
			using (var form = GetFormToBashCore() as CusCalculationRuleForm)
			{
				form.Show();
				AssertEquals("Customs Calculation Rule", form.FormCaption);
			}
		}

		public void TestRatesGrid() => CombineAssertions(() =>
		{
			using (var form = new CusCalculationRuleForm())
			{
				var grid = form.RatesGrid;
				AssertEquals("BindTo", nameof(CusCalculationRule.CalculationRuleRateCollection), grid.BindTo);
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertSequencesEqual("Columns", new[]
					{
						nameof(CusCalculationRuleRate.ValueFrom),
						nameof(CusCalculationRuleRate.FlatRate),
						nameof(CusCalculationRuleRate.Uplift)
					}, columnNames);

				var valueFromColumnInfo = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(CusCalculationRuleRate.ValueFrom));
				AssertEquals("Column not sortable", false, valueFromColumnInfo.IsSortable);

				var flatRateColumnInfo = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(CusCalculationRuleRate.FlatRate));
				AssertEquals("Column not sortable", false, flatRateColumnInfo.IsSortable);

				var upliftColumnInfo = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(CusCalculationRuleRate.Uplift));
				AssertEquals("Column not sortable", false, upliftColumnInfo.IsSortable);
			}
		});

		protected override Form GetFormToBashCore()
		{
			var rule = Factory.NewWithValidTestData<CusCalculationRule>();
			Factory.Save();
			var form = new CusCalculationRuleForm(rule);
			form.ControllerID = ControllerIDs.Customs.CusCalculationRules;
			return form;
		}
	}
}
