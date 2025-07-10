using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(MTDSubmissionForm))]
	public class MTDSubmissionFormBasherTest : ZFormBasherTest
	{
		public void TestAllBoxes()
		{
			using (var form = GetFormToBashCore())
			{
				CombineAssertions(() =>
				{
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box1_")).ToArray(), "ComputedByCW1");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box2_")).ToArray(), "UnsubmitedPreviousValues");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box3_")).ToArray(), "Adjustments");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box4_")).ToArray(), "GroupMemberTotal");
					AssertAllBoxes(form.Controls.OfType<ZCalcEdit>().Where(x => x.Name.StartsWith("box5_")).ToArray(), "ValuesToSubmitToHMRC");
				});
			}

			void AssertAllBoxes(ZCalcEdit[] columns, string type)
			{
				AssertEquals("Total Boxes should be 9", 9, columns.Length);

				var expectedListOfBoxes = new[]
				{
					$"{type}.Box1_VATDue IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box2_VATDueReverseChg IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box3_TotalVATDue IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box4_VATReclaimed IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box5_NetVAT IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box6_TotalSalesExVAT IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box7_TotalPurchaseExVAT IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box8_GoodsSalesECMembersExVAT IsReadOnly:True DecimalPlaces:2",
					$"{type}.Box9_GoodsPurchaseECMembersExVAT IsReadOnly:True DecimalPlaces:2"
				};

				var realListOfBoxes = columns.Select(x => $"{x.BindTo} IsReadOnly:{x.ReadOnly} DecimalPlaces:{x.DecimalPlaces}").ToArray();
				AssertContainsExactElementsInAnyOrder(expectedListOfBoxes, realListOfBoxes);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var report = Factory.New<AccComplianceReport>();
			report.FillWithValidTestData();
			return new MTDSubmissionForm(new MTDSubmissionDataColumns(Factory, report));
		}
	}
}
