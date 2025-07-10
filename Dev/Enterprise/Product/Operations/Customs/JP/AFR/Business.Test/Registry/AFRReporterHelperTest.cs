using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	sealed class AFRReporterHelperTest : TestCaseWithFactory
	{
		public void TestGetReporter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "VWG";
			Factory.Save();
			using (JPAFRRegistry.Instance.AFRReporterIDForDocument.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new AFRReporterID { ReporterID = "12345", Password = "111" }))
			{
				var reporter = company.GetReporter();
				AssertEquals("12345", reporter.ReporterID);
				AssertEquals("111", reporter.Password);

				CombineAssertions(() =>
				{
					AssertEquals("ReporterID", "12345", reporter.ReporterID);
					AssertEquals("Password", "111", reporter.Password);
				});
			}
		}

		public void TestSetReporter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "VWG";
			Factory.Save();
			company.SetReporter(new AFRReporterID { ReporterID = "12345", Password = "111" });
			var reporter = company.GetReporter();

			CombineAssertions(() =>
			{
				AssertEquals("ReporterID", "12345", reporter.ReporterID);
				AssertEquals("Password", "111", reporter.Password);
			});
		}
	}
}
