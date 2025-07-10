using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.SWT
{
	class ReportFiltersSetterTest : TestCaseWithFactory
	{
		public void TestUpdateReportFilters()
		{
			using (DocumentPack pack = new DocumentPack(Factory.New<StmMenuItem>()))
			using (Report report = (Report)pack.AddNew())
			{
				LookupField filter1 = new LookupField(Factory);
				filter1.DisplayName = "Importer";
				report.FilterCollection.Add(filter1);
				OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				ReportFiltersSetter setter = new ReportFiltersSetter("Importer", org);
				setter.UpdateReportFilters(report.FilterCollection);
				AssertEquals("Filter field value:", org.PK, filter1.Value);
			}
		}
	}
}
