using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SimpleZQueryDataRowFilterTest : TestCaseWithFactory
	{
		DummyBusinessObject dummyIn, dummyOut;
		DummyBusinessObject DummyIn => dummyIn ?? (dummyIn = Factory.New<DummyBusinessObject>());
		DummyBusinessObject DummyOut => dummyOut ?? (dummyOut = Factory.New<DummyBusinessObject>());

		public void TestOr()
		{
			var query = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Description, "bob"), JoinCondition.Or, new ZQuery(DummyBizoSchema.Z0_Code, "bob"));
			query.Simplify();
			AssertExceptionThrown<InvalidOperationException>(() => new SimpleZQueryDataRowFilter(query));
		}

		void AssertMatch(ZQuery query) => AssertMatch("", query);

		void AssertMatch(string message, ZQuery query)
		{
			CombineAssertions(message + " for sql " + query.LiteralTextSqlFormatted, () =>
			{
				query.Simplify();
				var result = Factory.Load<DummyBusinessObject>(query);
				AssertCollectionContains("Precondition: Can the query load the bizo", DummyIn, result);
				AssertCollectionNotContains("Precondition: Query excludes the other bizo", DummyOut, result);
				AssertEquals("Precondition: Match filter works", true, DummyIn.MatchesFilter(query));
				AssertEquals("Precondition: Match filter excludes", false, DummyOut.MatchesFilter(query));
				var filter = new SimpleZQueryDataRowFilter(query);
				AssertEquals(true, filter.IsMatch(DummyIn.Row));
				AssertEquals(false, filter.IsMatch(DummyOut.Row));
			});
		}

		[TestDate(2019, 07, 03, 08, 04, 02)]
		public void TestComparitors()
		{
			DummyIn.Z0_Code = "Bob";
			DummyIn.Z0_Date = ZDateTime.Now;
			DummyIn.Z0_DateTimeOffset = ZDateTimeOffset.Now;
			DummyIn.Z0_Description = "I walked into a wall oops!";
			DummyIn.Z0_Bool = true;
			DummyIn.Z0_Decimal = 10.0;
			DummyIn.Z0_Guid = ZGuid.NewZGuid();
			DummyIn.Z0_Number = 10;

			DummyOut.Z0_Code = "Tim";
			DummyOut.Z0_Date = ZDateTime.Now.AddDays(10);
			DummyOut.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddDays(10);
			DummyOut.Z0_Description = "I looked for a cow.";
			DummyOut.Z0_Bool = false;
			DummyOut.Z0_Decimal = 20.0;
			DummyOut.Z0_Guid = ZGuid.NewZGuid();
			DummyOut.Z0_Number = 20;

			Factory.Save();
			// String
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "Bob"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, new[] { "Bob", "cob", "nob" }));
			AssertMatch("Case sensitivity", new ZQuery(DummyBizoSchema.Z0_Code, new[] { "Bob", "cob", "nob" }));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "Tim"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "Bo"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.DoesNotStartWith, "Ti"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, "ob"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.DoesNotEndWith, "im"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains, "o"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotContains, "i"));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "%walked%"));
			AssertMatch("Case sensitivity", new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "%WALKED%"));

			DummyIn.Z0_Description = "Japan";
			DummyOut.Z0_Description = "";
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.IsNotBlank, ""));

			DummyIn.Z0_Description = "";
			DummyOut.Z0_Description = "Japan";
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.IsBlank, ""));

			// Date
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, ZDateTime.Now));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.NotEqual, ZDateTime.Now.AddDays(10)));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, ZDateTime.Now.AddMinutes(10)));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, ZDateTimeOffset.Now.AddMinutes(10)));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThan, ZDateTime.Now.AddMinutes(10)));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Now));
			DummyOut.Z0_Date = ZDateTime.Now.AddDays(-10);
			Factory.Save();
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMinutes(-10)));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now));

			// bool
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, true));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, ZBool.True));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual, false));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual, ZBool.False));

			// decimal
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 10));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.NotEqual, 20));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.LessThan, 15));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.LessThanOrEqualTo, 10));
			DummyOut.Z0_Decimal = -10;
			Factory.Save();
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.GreaterThan, 0));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.GreaterThanOrEqualTo, 10));

			// guid
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, DummyIn.Z0_Guid));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.NotEqual, DummyOut.Z0_Guid));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Guid, new[] { DummyIn.Z0_Guid, ZGuid.NewZGuid() }));

			// number
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 10));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.NotEqual, 20));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan, 11));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThanOrEqualTo, 10));
			DummyOut.Z0_Number = -10;
			Factory.Save();
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 0));
			AssertMatch(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 10));
		}

		[ExpectNoExceptions]
		public void TestDataContainsEmptyOrInvalidZDateTime()
		{
			DummyIn.Z0_Date = ZDateTime.Empty;
			DummyOut.Z0_Date = ZDateTime.Invalid;
			Factory.Save();

			var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, ZDateTime.Now);
			query.Simplify();
			var filter = new SimpleZQueryDataRowFilter(query);
			AssertEquals(false, filter.IsMatch(DummyIn.Row));
			AssertEquals(false, filter.IsMatch(DummyOut.Row));
		}

		[ExpectNoExceptions]
		public void TestRealBizoWithEmptyZDateField()
		{
			var staff = Factory.LoadTop1(ObjectFactory.GetType<IGlbStaff>(), new ZQuery(GlbStaffSchema.GS_IsActive, true));

			var query = new ZQuery(GlbStaffSchema.GS_DepartureDate, SQLComparisonOperator.EqualToDatePartOnly, ZDateTime.Now);
			query.Simplify();
			var filter = new SimpleZQueryDataRowFilter(query);
			AssertEquals(false, filter.IsMatch(((INeedRow)staff).Row));
		}
	}
}
