using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA.Testing
{
	[TestedType(typeof(VATSummaryForm))]
	public class VATSummaryFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var report = Factory.New<AccComplianceReport>();
			report.FillWithValidTestData();

			return new VATSummaryForm(new LIQSubmissionDataColumns(Factory, report));
		}

		public void TestAllBoxes()
		{
			using (var form = GetFormToBashCore())
			{
				CombineAssertions(() =>
				{
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box2_")).ToArray(), "ComputedByCW1");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box3_")).ToArray(), "Adjustments");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box5_")).ToArray(), "ValuesToSubmit");
				});
			}

			void AssertAllBoxes(ZCalcEdit[] columns, string type)
			{
				AssertEquals("Total Boxes should be 8", 8, columns.Length);

				var expectedListOfBoxes = new[]
				{
					$"{type}.Box1_TotalVatBaseReceivables IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box2_TotalVatReceivables IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box3_TotalVatBasePayables IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box4_TotalVatPayablesRecoverable IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box5_TotalVatPayablesNotRecoverable IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box6_VatBalanceReceivablesAndPayables IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box7_BalancePreviousPeriod IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box8_TotalBalance IsReadOnly:True DecimalPlaces:2"
				};

				var realListOfBoxes = columns.Select(x => $"{x.BindTo} IsReadOnly:{x.ReadOnly} DecimalPlaces:{x.DecimalPlaces}").ToArray();
				AssertContainsExactElementsInAnyOrder(expectedListOfBoxes, realListOfBoxes);
			}
		}
	}
}
