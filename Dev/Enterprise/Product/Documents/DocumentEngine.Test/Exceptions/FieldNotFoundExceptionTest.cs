using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class FieldNotFoundExceptionTest : ExceptionTestCase<FieldNotFoundException>
	{
		public void TestYouCanReportAMissingFieldExceptionWithoutHavingATemplateSoThatThingsLikeUDFsCanBeFilledIn()
		{
			using (Report report = new Report(null, null))
			{
				try
				{
					FieldNotFoundException.ReportFieldNotFound("MissingField", new DataProviderList(BODocDataProvider.Get(Factory.New<DummyBusinessObject>())));
				}
				catch (FieldNotFoundException exception)
				{
					exception.AddAsWarningToReport(report);
					AssertEquals("Report Errors", "Severity: [Warning (without error report)] Message: [Field <MissingField> not found on DataSource Type [DummyBusinessObject].] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString());
				}
			}
		}

		public void TestReportFieldNotFoundWithDataSource()
		{
			AssertExceptionThrown(typeof(FieldNotFoundException), @"Field <Woopie Goldberg> not found on DataSource Type [DummyBusinessObjectWithActiveFilter].", delegate
			{
				FieldNotFoundException.ReportFieldNotFound("Woopie Goldberg", new DataProviderList(BODocDataProvider.Get(Factory.New<DummyBusinessObjectWithActiveFilter>())));
			});
		}

		public void TestReportFieldNotFoundWithNoDataSource()
		{
			AssertExceptionThrown(typeof(FieldNotFoundException), @"Field <RachelHunter> not found on DataSource.", delegate
			{
				DataProviderList dataProviderList = null;
				FieldNotFoundException.ReportFieldNotFound("<RachelHunter>", dataProviderList);
			});
		}

		protected override FieldNotFoundException GetNewExceptionToTest(string message)
		{
			ConstructorInfo constructor = typeof(FieldNotFoundException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(DataProviderList) }, null);
			return (FieldNotFoundException)constructor.Invoke(new object[] { message, new DataProviderList(BODocDataProvider.Get(Factory.New<DummyBusinessObject>())) });
		}
	}
}
