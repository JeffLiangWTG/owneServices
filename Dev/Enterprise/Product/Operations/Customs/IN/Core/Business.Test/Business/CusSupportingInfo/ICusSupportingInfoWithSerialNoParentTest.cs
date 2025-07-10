using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing
{
	[TestedType(typeof(ICusSupportingInfoWithSerialNoParent))]
	sealed class CusSupportingInfoWithSerialNoParentTest : TestCaseWithFactory
	{
		public void TestGetLineNumberSuspenders()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var expectedSuspenderList = new[]
			{
				invoiceLine.SWConstituentLineNumberGenerator.GetLineNumberSuspender(),
				invoiceLine.DfiaExportItemDetailsLineNumberGenerator.GetLineNumberSuspender(),
				invoiceLine.JobWorkLineNumberGenerator.GetLineNumberSuspender(),
				invoiceLine.SWControlsLineNumberGenerator.GetLineNumberSuspender(),
				invoiceLine.SupportingDocumentLineNumberGenerator.GetLineNumberSuspender(),
				invoiceLine.SWProductionsLineNumberGenerator.GetLineNumberSuspender(),
			};

			using var suspenderDisposable = invoiceLine.GetLineNumberSuspenders();

			var suspenderList = (suspenderDisposable as DisposableList)?.ToList();

			AssertEquals("Count", expectedSuspenderList.Length, suspenderList.Count);

			var expectedValues = expectedSuspenderList.Select(s => s.ToString());
			var actualValues = suspenderList.Select(s => s.ToString());

			AssertContainsExactElementsInAnyOrder("Suspender List", expectedValues, actualValues);
		}
	}
}
