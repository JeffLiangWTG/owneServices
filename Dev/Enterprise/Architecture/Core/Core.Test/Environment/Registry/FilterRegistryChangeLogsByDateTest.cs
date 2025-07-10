using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing;

[TestedType(typeof(FilterRegistryChangeLogsByDate))]
public class FilterRegistryChangeLogsByDateTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidationForEmptyFromDate()
	{
		DateValidationHelper(ZDateTime.Empty, ZDateTime.Today, new[] { "Error - FromDate: From date cannot be empty." });
	}

	public void TestValidationForFutureFromDate()
	{
		DateValidationHelper(ZDateTime.Today.AddDays(5), ZDateTime.Today, new[] { "Error - FromDate: From date cannot be later than today.", "Error - FromDate: From date cannot be later than To date.", "Error - ToDate: From date cannot be later than To date." });
	}

	public void TestValidationForEmptyToDate()
	{
		DateValidationHelper(ZDateTime.Today, ZDateTime.Empty, new[] { "Error - ToDate: To date cannot be empty." });
	}

	public void TestValidationForFutureToDate()
	{
		DateValidationHelper(ZDateTime.Today, ZDateTime.Today.AddDays(10), new[] { "Error - ToDate: To date cannot be later than today." });
	}

	public void TestValidationForFromDateAfterToDate()
	{
		DateValidationHelper(ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-10), new[] { "Error - FromDate: From date cannot be later than To date.", "Error - ToDate: From date cannot be later than To date." });
	}

	public void TestFilterGeneratesValidSubQuery()
	{
		var testFromDate = ZDateTime.Today.AddDays(-5);
		var testToDate = ZDateTime.Today;
		var filterParameters = new FilterRegistryChangeLogsByDate { FromDate = testFromDate, ToDate = testToDate };
		var subQuery = filterParameters.GenerateSubQueryFilterFromFilterParameters(new ZDBOnlySubQuery(typeof(TestBusinessObject), StmALogSchema.SL_Parent));
		AssertNotNull(subQuery);
		AssertEquals(3, subQuery.Params.Length);

		var testParam = subQuery.Params[0];
		AssertEquals(StmALogSchema.SL_EventTime, testParam.SchemaColumn);
		AssertEquals(SQLComparisonOperator.GreaterThanOrEqualTo, testParam.ComparisonOperator);
		AssertEquals(testFromDate, testParam.Value);

		testParam = subQuery.Params[1];
		AssertEquals(StmALogSchema.SL_EventTime, testParam.SchemaColumn);
		AssertEquals(SQLComparisonOperator.LessThanOrEqualTo, testParam.ComparisonOperator);
		AssertEquals(testToDate.AddDays(1), testParam.Value);

		testParam = subQuery.Params[2];
		AssertEquals(StmALogSchema.SL_Table, testParam.SchemaColumn);
		AssertEquals(SQLComparisonOperator.Equal, testParam.ComparisonOperator);
		AssertEquals("StmData", testParam.Value);
	}

	void DateValidationHelper(ZDateTime fromDate, ZDateTime toDate, string[] expectedErrors)
	{
		var filterParameters = new FilterRegistryChangeLogsByDate { FromDate = fromDate, ToDate = toDate };
		filterParameters.Validation.ValidateAll();
		var errors = filterParameters.GetErrors().Select(e => e.Message).ToList();
		AssertEquals(expectedErrors.Length, errors.Count);
		foreach (var expectedError in expectedErrors)
		{
			Assert(errors.Contains(expectedError));
		}
	}

	[CodeProperty("Code")]
	class TestBusinessObject : DummyBusinessObject
	{
		public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(99)]
		public ZString Code
		{
			get { return null; }
		}
	}
}
