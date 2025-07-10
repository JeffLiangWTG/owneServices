using CargoWise.EntityFramework.Testing;
using Enterprise.ErrorReporting.Business;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.GUI.Test
{
	sealed class ErrorDetailsFormTest : TestCaseWithFactory
	{
		public void TestFormCaptionIsErrorReportID()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = "<ErrorReportID>E1234</ErrorReportID>";

			using (var form = new ErrorDetailsForm(report))
			{
				AssertEquals(report.ErrorReportID, form.FormCaption);
			}
		}

		[RequiresSTA]
		public void TestTreeViewPopulation()
		{
			var xml = "<a><b><c>d</c><d /></b><e f=\"g\">h</e><i j=\"k\" /></a>";
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = xml;

			using (var form = new ErrorDetailsForm(report))
			using (form.ShowForTest())
			{
				AssertEquals(1, form.TreeView.Nodes.Count);
				var a = form.TreeView.Nodes[0];

				AssertEquals("a", a.Text);
				AssertEquals(1, a.Nodes.Count);

				var elementsOfA = a.Nodes[0];
				AssertEquals("Elements", elementsOfA.Text);
				AssertEquals(3, elementsOfA.Nodes.Count);

				var b = elementsOfA.Nodes[0];
				var e = elementsOfA.Nodes[1];
				var i = elementsOfA.Nodes[2];

				AssertEquals("b", b.Text);
				AssertEquals(1, b.Nodes.Count);

				var elementsOfB = b.Nodes[0];
				AssertEquals("Elements", elementsOfB.Text);
				AssertEquals(2, elementsOfB.Nodes.Count);

				var c = elementsOfB.Nodes[0];
				var d = elementsOfB.Nodes[1];

				AssertEquals("c: d", c.Text);
				AssertEquals(0, c.Nodes.Count);

				AssertEquals("d", d.Text);
				AssertEquals(0, d.Nodes.Count);

				AssertEquals("e: h", e.Text);
				AssertEquals(1, e.Nodes.Count);

				var attributesOfE = e.Nodes[0];
				AssertEquals("Attributes", attributesOfE.Text);
				AssertEquals(1, attributesOfE.Nodes.Count);

				var f = attributesOfE.Nodes[0];
				AssertEquals("f: g", f.Text);
				AssertEquals(0, f.Nodes.Count);

				AssertEquals("i", i.Text);
				AssertEquals(1, i.Nodes.Count);

				var attributesOfI = i.Nodes[0];
				AssertEquals("Attributes", attributesOfI.Text);
				AssertEquals(1, attributesOfI.Nodes.Count);

				var j = attributesOfI.Nodes[0];
				AssertEquals("j: k", j.Text);
				AssertEquals(0, j.Nodes.Count);
			}
		}

		public void TestTreeViewPopulationWithError()
		{
			var xml = " haha not xml ";
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = xml;

			using (var form = new ErrorDetailsForm(report))
			using (form.ShowForTest())
			{
				AssertEquals(1, form.TreeView.Nodes.Count);
				AssertEquals("An error occurred while parsing XML: Data at the root level is invalid. Line 1, position 2.", form.TreeView.Nodes[0].Text);
			}
		}
	}
}
