using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(DateBasedAccountingPeriodField))]
	sealed class DateBasedAccountingPeriodFieldTest : AccountingPeriodFieldTest
	{
		AccountingPeriodField apf;

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);

			AccountingPeriodTestHelper apth = new AccountingPeriodTestHelper();
			apth.PostPeriodsForEntireYear(2000);
			apth.PostPeriodsForEntireYear(2001);
			apth.PostPeriodsForEntireYear(2002);
			apth.PostPeriodsForEntireYear(2003);
			apf = new DateBasedAccountingPeriodField(new BusinessObjectFactory());
			apf.FieldName = "f";
			apf.DisplayName = "AccountingPeriod";
			apf.SinglePeriod = 200301;
			apf.FromPeriod = 200202;
			apf.ToPeriod = 200206;
			apf.YearToPeriod = 200010;
		}

		public void TestJsonConverter_SinglePeriod()
		{
			apf.DisplayName = "Json Test";
			apf.UseSinglePeriod = true;

			var result = JsonConverterHelper.Serialize(apf);
			var deserialisedField = JsonConverterHelper.Deserialize<DateBasedAccountingPeriodField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);

			var whereClauseMatch = Regex.Match(deserialisedField.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f < @p2 but was: " + deserialisedField.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, deserialisedField.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", new DateTime(2002, 7, 1), deserialisedField.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, deserialisedField.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", new DateTime(2002, 7, 31, 23, 59, 0), deserialisedField.SqlParameters()[1].Value);
		}

		public override void TestSinglePeriod()
		{
			apf.UseSinglePeriod = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f < @p2 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", new DateTime(2002, 7, 1), apf.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, apf.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", new DateTime(2002, 7, 31, 23, 59, 0), apf.SqlParameters()[1].Value);
		}

		public override void TestPeriodRange()
		{
			apf.UsePeriodRange = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f < @p2 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", new DateTime(2001, 8, 1), apf.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, apf.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", new DateTime(2001, 12, 31, 23, 59, 0), apf.SqlParameters()[1].Value);
		}

		public override void TestYearToPeriod()
		{
			apf.UseYearToPeriod = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f < @p2 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", new DateTime(1999, 7, 1), apf.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, apf.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", new DateTime(2000, 4, 30, 23, 59, 0), apf.SqlParameters()[1].Value);
		}

		public void TestIsResponsibleNewWayFromDate()
		{
			apf.UseSinglePeriod = true;
			foreach (ValueProvider provider in apf.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<AccountingPeriod.FromDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTime(2002, 7, 1), provider.GetReplacement("<AccountingPeriod.FromDate>", new Report(null, null)));
				}
			}
		}

		public void TestIsResponsibleNewWayToDate()
		{
			apf.UseSinglePeriod = true;
			foreach (ValueProvider provider in apf.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<AccountingPeriod.ToDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTime(2002, 7, 31, 23, 59, 00), provider.GetReplacement("<AccountingPeriod.ToDate>", new Report(null, null)));//00 seconds for small date time in DB
				}
			}
		}
	}
}
