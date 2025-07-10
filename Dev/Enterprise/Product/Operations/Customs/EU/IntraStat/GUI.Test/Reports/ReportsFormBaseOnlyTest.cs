using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(ReportsForm))]
	sealed class ReportsFormBaseOnlyTest : ReportsFormAbstractTest<CusIntrastatGroup>
	{
		public void TestFormCaption()
		{
			using (var form = new ReportsForm(report))
			{
				form.Show();
				AssertEquals("Intrastat - Report", form.FormCaption);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
		}

		CusIntrastatGroup report;
	}
}
