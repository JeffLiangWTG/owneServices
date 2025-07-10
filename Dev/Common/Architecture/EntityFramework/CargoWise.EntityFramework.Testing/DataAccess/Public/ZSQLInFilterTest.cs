using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSQLInFilterTest : TestCaseWithFactory
	{
		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				"Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Code),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
			AssertNotEquals(
				"Not Equals",
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThan, DummyBizoSchema.Z0_Description),
				new ZSQLColumnComparer(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_Description));
		}

		#endregion

		#region IFilterPart Members

		public void TestLiteralTextADOForDecimal()
		{
			Array values = new decimal[] { 1 };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(DummyBizoSchema.Z0_Decimal.Name + " = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestToCSharpCode()
		{
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new string[] { "A", "B", "C" }, ComparisonOptions.Default);
			AssertEquals(
@"query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new object[] { ""A"", ""B"", ""C"" } );",
filter.ToCSharpCode());
		}

		public void TestLiteralTextADOForGuid()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			Array values = new ZGuid[] { guid1 };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(inFilter.LiteralTextADO, DummyBizoSchema.Z0_Guid.Name + " = CONVERT('" + guid1 + "', 'System.Guid')");
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestLiteralTextADOForGuidsAndNotEqualsComparisonOperator()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			Array values = new ZGuid[] { guid1, guid2 };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);

			//because the ZSQLInFilter will sort the values so we need to do the same here
			var sortedValues = values.Cast<object>().OrderBy(x => x);
			var firstGuid = sortedValues.First();
			var secondGuid = sortedValues.Last();
			string expectedLiteralTextADO = "(" + DummyBizoSchema.Z0_Guid.Name + " not in (CONVERT('" + firstGuid + "', 'System.Guid'), CONVERT('" + secondGuid + "', 'System.Guid')))";
			AssertEquals(expectedLiteralTextADO, inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestLiteralTextADOForDate()
		{
			ZDateTime now = ZDateTime.Now;
			Array values = new ZDateTime[] { now };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(DummyBizoSchema.Z0_Date.Name + " = #" + now.SqlFormat + "#", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestLiteralTextADOForDateTimeOffset()
		{
			ZDateTimeOffset now = ZDateTimeOffset.Now;
			Array values = new ZDateTimeOffset[] { now };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) = #" + now.SqlFormat + "#", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestLiteralTextADOForString()
		{
			Array values = new string[] { "A" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(DummyBizoSchema.Z0_Code.Name + " = 'A'", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestLiteralTextADOForBool()
		{
			var values = new object[] { true, false };
			var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("1 = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { ZBool.True, false };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("1 = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { ZBool.True, ZBool.False };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("1 = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { true, false };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);
			AssertEquals("1 = 0", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { true, true };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("Z0_Bool = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { ZBool.True, true };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("Z0_Bool = 1", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception

			values = new object[] { ZBool.False, ZBool.False };
			inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("Z0_Bool = 0", inFilter.LiteralTextADO);
			Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); // expect no exception
		}

		public void TestNeedsBrackets()
		{
			Array values = new string[] { "A" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.NeedsBrackets);
		}

		public void TestFilterIsEmptyWhenEmpty_ForIn()
		{
			Array values = Array.Empty<string>();
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(true, inFilter.FilterIsEmpty);
		}

		public void TestFilterIsEmptyWhenNotEmpty_ForIn()
		{
			Array values = new string[] { "A" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.FilterIsEmpty);
		}

		public void TestFilterIsEmptyWhenEmpty_ForNotIn()
		{
			Array values = Array.Empty<string>();
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.FilterIsEmpty);
		}

		public void TestFilterIsEmptyWhenNotEmpty_ForNotIn()
		{
			Array values = new string[] { "A" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.FilterIsEmpty);
		}

		public void TestSingleValue()
		{
			Array values = new[] { "A" };
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(DummyBizoSchema.Z0_Code.Name + " = 'A'", filter.LiteralTextADO);

			values = new[] { "A", "B" };
			filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " in ('A', 'B'))", filter.LiteralTextADO);
		}

		public void TestParameterisedSql()
		{
			var values = new[] { "A'", "B" };
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default, allowTableValuedParameter: true);
				var dataQuery = inFilter.ParameterisedSql(new ParameterNameFactory());
				AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " in (SELECT Value FROM @CWO1_))", dataQuery.ParameterisedQueryText);
				AssertEquals(1, dataQuery.Parameters.Length);

				var collection = dataQuery.Parameters[0].Value as ICollection;
				var items = new List<object>();

				foreach (var param in collection)
				{
					items.Add(param);
				}

				AssertEquals("A'", items[0]);
				AssertEquals("B", items[1]);

				var notInFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default, allowTableValuedParameter: true);
				dataQuery = notInFilter.ParameterisedSql(new ParameterNameFactory());
				AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " not in (SELECT Value FROM @CWO1_))", dataQuery.ParameterisedQueryText);
				AssertEquals(1, dataQuery.Parameters.Length);

				collection = dataQuery.Parameters[0].Value as ICollection;
				items = new List<object>();

				foreach (var param in collection)
				{
					items.Add(param);
				}

				AssertEquals("A'", items[0]);
				AssertEquals("B", items[1]);
			}
		}

		public void TestParameterisedSql_ForMaximumElementsForParameterisationEdgeCases()
		{
			var values = Enumerable.Range(0, ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION).Select(i => i.ToString()).ToArray();
			var filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			var query = filter.ParameterisedSql(new ParameterNameFactory());

			AssertEquals(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION, query.Parameters.Length);

			values = Enumerable.Range(0, ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 1).Select(i => i.ToString()).ToArray();
			filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			query = filter.ParameterisedSql(new ParameterNameFactory());

			AssertEquals(0, query.Parameters.Length);
		}

		public void TestApplySuffixRulesToTVP()
		{
			var decimalValues = new[] { 0m, 100m };

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Money, SQLComparisonOperator.Equal, decimalValues, ComparisonOptions.Default, allowTableValuedParameter: true);
				var dataQuery = inFilter.ParameterisedSql(new ParameterNameFactory());

				AssertEquals("(" + DummyBizoSchema.Z0_Money.Name + " in (SELECT Value FROM @CWO1_))", dataQuery.ParameterisedQueryText);
				AssertEquals(1, dataQuery.Parameters.Length);

				var collection = dataQuery.Parameters[0].Value as ICollection;
				var items = new List<object>();
				foreach (var param in collection)
				{
					items.Add(param);
				}

				AssertEquals(0m, items[0]);
				AssertEquals(100m, items[1]);
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Money, SQLComparisonOperator.Equal, decimalValues, ComparisonOptions.Default);
				var dataQuery = inFilter.ParameterisedSql(new ParameterNameFactory());
				AssertEquals("(" + DummyBizoSchema.Z0_Money.Name + " in (@CWO1_, @CWO2_))", dataQuery.ParameterisedQueryText);
				AssertEquals(2, dataQuery.Parameters.Length);
				AssertEquals(0m, dataQuery.Parameters[0].Value);
				AssertEquals(100m, dataQuery.Parameters[1].Value);
			}

			var stringValues = new[] { "A'", "B" };

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, stringValues, ComparisonOptions.Default, true);
				var dataQuery = inFilter.ParameterisedSql(new ParameterNameFactory());
				AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " in (SELECT Value FROM @CWO1_))", dataQuery.ParameterisedQueryText);
				AssertEquals(1, dataQuery.Parameters.Length);

				var collection = dataQuery.Parameters[0].Value as ICollection;
				var items = new List<object>();
				foreach (var param in collection)
				{
					items.Add(param);
				}

				AssertEquals("A'", items[0]);
				AssertEquals("B", items[1]);
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, stringValues, ComparisonOptions.Default, true);
				var dataQuery = inFilter.ParameterisedSql(new ParameterNameFactory());
				AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " in (SELECT Value FROM @CWO1_))", dataQuery.ParameterisedQueryText);
				AssertEquals(1, dataQuery.Parameters.Length);

				var collection = dataQuery.Parameters[0].Value as ICollection;
				var items = new List<object>();
				foreach (var param in collection)
				{
					items.Add(param);
				}

				AssertEquals("A'", items[0]);
				AssertEquals("B", items[1]);
			}
		}

		public void TestLiteralTextADO()
		{
			Array values = new string[] { "A'", "B" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " in ('A''', 'B'))", inFilter.LiteralTextADO);

			ZSQLInFilter notInFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);
			AssertEquals("(" + DummyBizoSchema.Z0_Code.Name + " not in ('A''', 'B'))", notInFilter.LiteralTextADO);
		}

		public void TestFunctionalBehaviourForString()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Code = "A'";
			ZQuery inFilter = new ZQuery();
			inFilter.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "A'", "B", "C" });
			BusinessObject[] bizOs = Factory.Load(typeof(DummyBusinessObject), inFilter);
			AssertEquals(1, bizOs.Length);
			AssertEquals(bizO, bizOs[0]);
		}

		public void TestFunctionalBehaviourForGuid()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			bizO.Z0_Guid = ZGuid.NewZGuid();
			ZQuery inFilter = new ZQuery();
			ZGuid[] guids = new ZGuid[300];
			for (int i = 0; i < guids.Length; i++)
			{
				guids[i] = ZGuid.NewZGuid();
			}
			guids[0] = bizO.Z0_Guid;

			inFilter.AddToFilter(DummyBizoSchema.Z0_Guid, guids);
			BusinessObject[] bizOs = factory.Load(typeof(DummyBusinessObject), inFilter);
			AssertEquals(1, bizOs.Length);
			AssertEquals(bizO, bizOs[0]);
		}

		public void TestFunctionalBehaviourForDate()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			bizO.Z0_Date = ZDateTime.BrettsBirthday;
			DummyBusinessObject bizO2 = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			bizO2.Z0_Date = ZDateTime.BrettsBirthday.AddDays(1);
			ZQuery inFilter = new ZQuery();
			ZDate[] dates = { ZDateTime.BrettsBirthday.Date };

			inFilter.AddToFilter(DummyBizoSchema.Z0_Date, dates);
			BusinessObject[] bizOs = factory.Load(typeof(DummyBusinessObject), inFilter);
			AssertEquals(1, bizOs.Length);
			AssertEquals(bizO, bizOs[0]);
		}

		public void TestFunctionalBehaviourForDateTimeOffset()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			bizO.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 1, 2, 0, 0, 0, TimeSpan.FromHours(11));
			DummyBusinessObject bizO2 = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			bizO2.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 1, 3, 0, 0, 0, TimeSpan.FromHours(11));
			ZQuery inFilter = new ZQuery();
			ZDateTimeOffset[] dates = { bizO.Z0_DateTimeOffset.DateAndOffset };

			inFilter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, dates);
			BusinessObject[] bizOs = factory.Load(typeof(DummyBusinessObject), inFilter);
			AssertEquals(1, bizOs.Length);
			AssertEquals(bizO, bizOs[0]);
		}

		public void TestContainsOrOperatorForOneItem()
		{
			Array values = new string[] { "A" };
			IFilterPart inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.ContainsOrOperator);
		}

		// the logic here is that it doesn't really contain an or statement (as no bracketing is required)
		public void TestContainsOrOperatorForTwoItemsWithEqual()
		{
			Array values = new string[] { "A", "B" };
			IFilterPart inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.ContainsOrOperator);
		}

		public void TestContainsOrOperatorForTwoItemsWithStartsWith()
		{
			Array values = new string[] { "A", "B" };
			IFilterPart inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, values, ComparisonOptions.Default);
			AssertEquals(true, inFilter.ContainsOrOperator);
		}

		public void TestContainsOrOperatorForNotIn()
		{
			Array values = new string[] { "A", "B" };
			IFilterPart inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);
			AssertEquals(false, inFilter.ContainsOrOperator);
		}

		public void TestFilterWithTrailingCondition()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var values = new string[] { "A", "B" };
				var query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values);
				query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 123);
				AssertEquals("\r\n\tWHERE (Z0_Code in ('A', 'B')) and Z0_Number = 123", query.GetAsWhereClause(true));
				AssertEquals("\r\n\tWHERE (Z0_Code in (SELECT Value FROM @CWO1_)) and Z0_Number = @CWO2_", query.GetAsWhereClause(false));
			}
		}

		public void TestGetOrParts()
		{
			Array values = new string[] { "A", "B" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			ZQuery[] orParts = inFilter.GetOrParts();
			AssertEquals(2, orParts.Length);

			ZSqlParameter param0 = orParts[0].GetMostUniqueSingleEqualParameter();
			AssertEquals("A", param0.Value);
			AssertEquals(DummyBizoSchema.Z0_Code, param0.SchemaColumn);
			AssertEquals(SQLComparisonOperator.Equal, param0.ComparisonOperator);

			ZSqlParameter param1 = orParts[1].GetMostUniqueSingleEqualParameter();
			AssertEquals("B", param1.Value);
			AssertEquals(DummyBizoSchema.Z0_Code, param1.SchemaColumn);
			AssertEquals(SQLComparisonOperator.Equal, param1.ComparisonOperator);
		}

		public void TestGetOrParts_ForNotIn()
		{
			Array values = new string[] { "A", "B" };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, values, ComparisonOptions.Default);
			ZQuery[] orParts = inFilter.GetOrParts();
			AssertEquals(0, orParts.Length);
		}

		public void TestBlobFilters()
		{
			Array values = new string[] { "A", "B" };
			IFilterPart blobbyFilter = new ZSQLInFilter(DummyBizoSchema.Z0_VarBinaryMax, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(true, blobbyFilter.BlobFilters.Contains(DummyBizoSchema.Z0_VarBinaryMax));
			IFilterPart nonBlobbyFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertEquals(false, nonBlobbyFilter.BlobFilters.Contains(DummyBizoSchema.Z0_VarBinaryMax));
		}

		public void TestHasParametersEmpty()
		{
			IFilterPart filterPart = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new List<string>(), ComparisonOptions.Default);
			AssertEquals(false, filterPart.HasParameters);
		}

		public void TestHasParametersNotEmpty()
		{
			List<string> list = new List<string>();
			list.Add("CODE");
			IFilterPart filterPart = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, list, ComparisonOptions.Default);
			AssertEquals(true, filterPart.HasParameters);
		}

		public void TestWithZeroValuesForNotIn()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			DummyBusinessObject[] dummiesNotInEmptyList = Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, Array.Empty<object>()));
			AssertEquals("There is 1 object not in empty list", 1, dummiesNotInEmptyList.Length);

			DummyBusinessObject[] dummiesInEmptyList = Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, SQLComparisonOperator.Equal, Array.Empty<object>()));
			AssertEquals("No objects in empty list", 0, dummiesInEmptyList.Length);
		}

		[ExpectNoExceptions]
		public void TestWithLargeNumber_In()
		{
			TestWithLargeNumber(SQLComparisonOperator.Equal);
		}

		[ExpectNoExceptions]
		public void TestWithLargeNumber_NotIn()
		{
			TestWithLargeNumber(SQLComparisonOperator.NotEqual);
		}

		void TestWithLargeNumber(SQLComparisonOperator op)
		{
			List<DummyBusinessObject> dummies = new List<DummyBusinessObject>();
			for (int i = 0; i < 10000; i++)
			{
				dummies.Add(Factory.New<DummyBusinessObject>());
			}
			Factory.Save();

			ZQuery query = new ZQuery(DummyBizoSchema.PK, op, dummies.ConvertAll(x => x.PK));
			Factory.LoadTop1<DummyBusinessObject>(query);
			new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(query);
		}

		public void TestStartsWith()
		{
			Array values = new[] { "A01", "A02", "A031", "A032", "B", "C", "D", "Qwer" };
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, values, ComparisonOptions.Default);

			var expected = "(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('B', 'B%'), ('C', 'C%'), ('D', 'D%'), ('A01', 'A01%'), ('A02', 'A02%'), ('A031', 'A031%'), ('A032', 'A032%'), ('Qwer', 'Qwer%')) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))";

			AssertEquals(expected, filter.LiteralTextADO);
			AssertNoExceptionThrown(() => Factory.Load<DummyBusinessObject>(new ZQuery(filter))); // Expects no exceptions
		}

		public void TestEndsWith()
		{
			Array values = new[] { "A01", "A02", "A031", "A032", "B", "C", "D", "Qwer" };
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, values, ComparisonOptions.Default);

			var expected = "(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%B'), ('%C'), ('%D'), ('%A01'), ('%A02'), ('%A031'), ('%A032'), ('%Qwer')) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))";

			AssertEquals(expected, filter.LiteralTextADO);
			AssertNoExceptionThrown(() => Factory.Load<DummyBusinessObject>(new ZQuery(filter))); // Expects no exceptions
		}

		public void TestGuidColumnWhichIsNotNullable()
		{
			ZGuid guid1 = new ZGuid("11111111-1111-1111-1111-111111111111");
			ZGuid guid2 = new ZGuid("22222222-2222-2222-2222-222222222222");
			ZGuid guid3 = ZGuid.Empty;
			Array values = new ZGuid[] { guid1, guid2, guid3 };
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.PK, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			string expectedLiteralTextADO = "(" + DummyBizoSchema.PK.Name + " in (CONVERT('" + guid3 + "', 'System.Guid'), CONVERT('" + guid1 + "', 'System.Guid'), CONVERT('" + guid2 + "', 'System.Guid')) or Z0_PK is null)";
			AssertEquals(expectedLiteralTextADO, inFilter.LiteralTextADO);
			AssertNoExceptionThrown(() => { Factory.Load<DummyBusinessObject>(new ZQuery(inFilter)); });
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "comparisonOperator SQLComparisonOperator.StartsWith and EndsWith can be used only with string values.")]
		public void TestStartsWithNotForString()
		{
			Array values = new object[] { "A", "B", 3 };
			new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, values, ComparisonOptions.Default);
		}

		public void TestEndsWithLongAndShortValues()
		{
			DummyBusinessObject element1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			element1.Z0_Description = "1234567890";

			DummyBusinessObject element2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			element2.Z0_Description = "34567890";

			DummyBusinessObject element3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			element3.Z0_Description = "890";

			Factory.Save();

			Array values = new[] { "34567890" };
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.EndsWith, values, ComparisonOptions.Default);

			AssertEquals("(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%34567890')) AS Con(escapedValue) ) ConTempTable0 ON Z0_Description LIKE ConTempTable0.escapedValue ESCAPE '~' ))", filter.LiteralTextADO);

			DummyBusinessObject[] result = Factory.Load<DummyBusinessObject>(new ZQuery(filter));
			AssertEquals(2, result.Length);

			DummyBusinessObject el1 = null;
			if (result[0].PK == element1.PK)
			{
				el1 = result[0];
			}
			else if (result[1].PK == element1.PK)
			{
				el1 = result[1];
			}
			AssertNotNull(el1);

			DummyBusinessObject el2 = null;
			if (result[0].PK == element2.PK)
			{
				el2 = result[0];
			}
			else if (result[1].PK == element2.PK)
			{
				el2 = result[1];
			}
			AssertNotNull(el2);
		}

		#endregion

		public void TestElementsAreSortedToReducePlansInDbAndMemory()
		{
			var values = new[] { "C", "A", "B" };
			var filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertArrayEqualsByElements(new[] { "A", "B", "C" }, filter.GetValues().Cast<string>().ToArray());
		}

		public void TestElementsOfDifferingTypesAreSorted()
		{
			var values = new object[] { 1, 2.1m };
			var filter = new ZSQLInFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			var sorted = values.OrderBy(x => x.GetHashCode()).ToArray();
			AssertArrayEqualsByElements(sorted, filter.GetValues().Cast<object>().ToArray());
		}

		public void TestElementsAreDeduplicated()
		{
			var values = new[] { "C", "B", "B" };
			var filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			AssertArrayEqualsByElements(new[] { "B", "C" }, filter.GetValues().Cast<string>().ToArray());
		}

		public void TestIsDbOnlyFilter()
		{
			Array values = new[] { "A01", "A02", "A031", "A032", "B", "C", "D", "Qwertyuiop" };
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, values, ComparisonOptions.Default);
			Assert("Filter should be DB-only.", ((IDbOnlyFilter)filter).IsDbOnlyFilter);

			filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, values, ComparisonOptions.Default);
			Assert("Filter should not be DB-only.", !((IDbOnlyFilter)filter).IsDbOnlyFilter);
		}

		public void TestDeepClone()
		{
			ZSQLInFilter filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new string[] { "A", "B", "C" }, ComparisonOptions.Default);
			ZSQLInFilter clonedFilter = (ZSQLInFilter)filter.DeepClone();
			Assert(!object.ReferenceEquals(filter, clonedFilter));
			AssertEquals(filter.LiteralTextADO, clonedFilter.LiteralTextADO);
		}

		public void TestGetSimplifiedVersionWithElements()
		{
			IFilterPart filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new string[] { "A", "B", "C" }, ComparisonOptions.Default);
			AssertEquals(filter, filter.GetSimplifiedVersion(null)[0]);
		}

		public void TestGetSimplifiedVersionWithNoElements()
		{
			IFilterPart filter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, Array.Empty<string>(), ComparisonOptions.Default);
			AssertNull(filter.GetSimplifiedVersion(null));
		}

		public void TestQueryToNonNullableColumnHasFilterWithNullValue()
		{
			var filter = new ZSQLInFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, new string[] { "Test String", null }, ComparisonOptions.Default);
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = "Test String";
			Factory.Save();

			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			typeof(SchemaStringColumn)
				.GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
				.SetValue(filter.Column, false);

			dummies.Load(filter.GetOrParts()[0]);
			AssertCollectionContains(dummy, dummies);
		}

		public void TestGenerateConstValuesSQL()
		{
			var values = new List<string>();
			var zSQLInFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, values, ComparisonOptions.Default);
			var builder = new SqlBuilder();
			AssertExceptionThrown(typeof(ArgumentException), "Input values cannot be null or empty.", () => zSQLInFilter.GenerateConstValuesSQL(builder, false));

			values = new List<string>() { "A", "A", "AB", "BA", "ABC" };
			var zSQLInFilter1 = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, values, ComparisonOptions.Default);
			var builder1 = new SqlBuilder();

			zSQLInFilter1.GenerateConstValuesSQL(builder1, false);

			var startWithOperatorExpectedMessage = "SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('BA', 'BA%')) AS Con(value, escapedValue)";
			AssertEquals(startWithOperatorExpectedMessage, builder1.ToString());

			var zSQLInFilter2 = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, values, ComparisonOptions.Default);
			var builder2 = new SqlBuilder();

			zSQLInFilter2.GenerateConstValuesSQL(builder2, false);

			var endWithOperatorExpectedMessage = "SELECT escapedValue FROM (VALUES ('%A'), ('%AB'), ('%ABC')) AS Con(escapedValue)";
			AssertEquals(endWithOperatorExpectedMessage, builder2.ToString());
		}

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var sqlInFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new string[] { "A", "B", "C" }, ComparisonOptions.Default);
			AssertEquals(sqlInFilter.HasComparisonOperatorLike, false);
		}

		#endregion
	}
}
