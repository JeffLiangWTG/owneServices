using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	/// <summary>
	/// Moved from Main project
	/// </summary>
	sealed partial class ZQueryTest
	{
		public void TestQueryWith10ParametersGeneratesCorrectParameterisedText()
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.Or;
			for (int i = 0; i <= 9; i++)
			{
				query.AddToFilter(DummyBizoSchema.Z0_Number, i);
			}
			ZNonPersistentDataQuery nonPersistentQuery = query.ParameterisedText;
			string parameterisedText = nonPersistentQuery.LiteralTextSql;
			AssertEquals("Z0_Number = 0 or Z0_Number = 1 or Z0_Number = 2 or Z0_Number = 3 or Z0_Number = 4 or Z0_Number = 5 or Z0_Number = 6 or Z0_Number = 7 or Z0_Number = 8 or Z0_Number = 9", parameterisedText);
		}

		public void TestSimplifyWithEmptyQueryAndParameterQuery()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(new ZQuery());
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			query.Simplify();
			AssertEquals(1, query.FilterParts.Count);
			AssertEquals("Z0_Code = '123'", query.LiteralTextADO);
		}

		public void TestSimplifyWithSingleParameter()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			query.Simplify();
			AssertEquals(1, query.FilterParts.Count);
			AssertEquals(typeof(ZSqlParameter), query.FilterParts.FilterParts[0].GetType());
			AssertEquals("Z0_Code = '1'", query.FilterParts.LiteralTextADO);
		}

		public void TestGetSimplifiedVersionWithSingleParameter()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			IFilterPart filterPart = query;
			IFilterPart[] filterParts = filterPart.GetSimplifiedVersion(null);
			AssertEquals(1, filterParts.Length);
			AssertEquals(typeof(ZSqlParameter), filterParts[0].GetType());
			AssertEquals("Z0_Code = '1'", filterParts[0].LiteralTextADO);
		}

		public void TestGetSimplifiedVersionWithMultipleParametersNullJoinCondition()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			query.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			IFilterPart filterPart = query;
			IFilterPart[] filterParts = filterPart.GetSimplifiedVersion(null);
			AssertEquals(3, filterParts.Length);
			AssertEquals(typeof(ZSqlParameter), filterParts[0].GetType());
			AssertEquals(typeof(JoinCondition), filterParts[1].GetType());
			AssertEquals(typeof(ZSqlParameter), filterParts[2].GetType());
		}

		public void TestGetSimplifiedVersionWithMultipleParametersSameJoinCondition()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			query.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			IFilterPart filterPart = query;
			IFilterPart[] filterParts = filterPart.GetSimplifiedVersion(query.DefaultJoinCondition);
			AssertEquals(3, filterParts.Length);
			AssertEquals(typeof(ZSqlParameter), filterParts[0].GetType());
			AssertEquals(typeof(JoinCondition), filterParts[1].GetType());
			AssertEquals(typeof(ZSqlParameter), filterParts[2].GetType());
		}

		public void TestGetSimplifiedVersionWithMultipleParametersAndDifferentJoinCondition()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			query.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			IFilterPart filterPart = query;
			IFilterPart[] filterParts = filterPart.GetSimplifiedVersion(JoinCondition.Or);
			AssertEquals(1, filterParts.Length);
			AssertEquals(typeof(ZQuery), filterParts[0].GetType());
		}

		public void TestEqualsArray()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();

			var expectedFilterStringFormat = "(Z0_Guid in (@CWO1_, @CWO2_, @CWO3_))";
			Filter.AddToFilter(DummyBizoSchema.Z0_Guid, new Guid[] { guid1, guid2, guid3 });
			AssertEquals(expectedFilterStringFormat, Filter.FilterString);
		}

		public void TestEqualsArrayForMoreThen10Elements1()
		{
			ZInt[] numbers = new ZInt[30];
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummyBizO;
			for (int i = 0; i < 30; i++)
			{
				dummyBizO = factory.New<DummyBusinessObject>();
				dummyBizO.Z0_Number = i;
				if (i == 0 || i == 9 || i == 17 || i == 24 || i == 29)
				{
					dummyBizO.Z0_Code = "X";
				}
				else
				{
					dummyBizO.Z0_Code = "Y";
				}
				numbers[i] = i;
			}
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "X");
			query.AddToFilter(DummyBizoSchema.Z0_Number, numbers);
			DummyBusinessObject[] bizOs = factory.Load<DummyBusinessObject>(query);
			AssertEquals(5, bizOs.Length);
			AssertEquals("Z0_Code = 'X' and (Z0_Number in (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29))",
				query.FilterParts.LiteralTextADO);
		}

		public void TestEqualsArrayForMoreThen10Elements2()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummyBizO = factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Number = 0;
			ZQuery query;
			for (int i = 0; i < 30; i++)
			{
				ZInt[] numbers = new ZInt[i + 1];
				query = new ZQuery();
				query.AddToFilter(DummyBizoSchema.Z0_Code, "Z");
				query.AddToFilter(DummyBizoSchema.Z0_Number, numbers);
				DummyBusinessObject[] bizOs = factory.Load<DummyBusinessObject>(query);
				AssertEquals(0, bizOs.Length);
			}
		}

		public void TestMaxLengthOfUsedColumns()
		{
			var query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			query.AddToFilter(DummyBizoSchema.Z0_Description, "Description");
			AssertEquals(100, query.GetMaxLengthOfUsedColumns());
		}

		#region Constructors

		public void TestLiteralTextADOWithSingleParameter()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 123);
			AssertEquals("Z0_Number = 123", filter1.LiteralTextADO);
		}

		[ExpectNoExceptions]
		public void TestZQuery_ForXMLColumn_NoException()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummyBizO = factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Xml = "<Root>0123456789 ღ 0123456789 ღ 0123456789 </Root>";
			factory.Save();

			AssertEquals(1, factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, "<Root>0123456789 ღ 0123456789 ღ 0123456789 </Root>")).Length);
			AssertEquals(1, factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.Contains, "0")).Length);
			AssertEquals(1, factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.StartsWith, "<")).Length);
			AssertEquals(0, factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.EndsWith, "0")).Length);

			factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.NotEqual, ""));
			factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.NotContains, "0"));
			factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Xml, SQLComparisonOperator.DoesNotStartWith, "<"));
		}

		public void TestParameterisedTextWithSingleParameter()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 123);
			ZNonPersistentDataQuery query = filter1.ParameterisedText;
			AssertEquals("Z0_Number = " + ParameterNameFactory.GetParameterName(1), query.ParameterisedQueryText);
		}

		public void TestConstructorFilterPart()
		{
			ZQuery filterInternal = new ZQuery();
			filterInternal.AddToFilter(DummyBizoSchema.Z0_Code, "H");
			ZQuery filter = new ZQuery();
			filter.AddToFilter(filterInternal);
			AssertEquals("Z0_Code = 'H'", filter.LiteralTextADO);
		}

		public void TestConstructorTwoFilter()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 123);
			ZQuery filter2 = new ZQuery(DummyBizoSchema.Z0_AnotherNumber, 124);
			ZQuery filter3 = new ZQuery(filter1, filter2);
			AssertEquals("Z0_Number = 123 and Z0_AnotherNumber = 124", filter3.LiteralTextADO);
		}

		public void TestConstructorTwoFilterPlusJoinCondition()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 123);
			ZQuery filter2 = new ZQuery(DummyBizoSchema.Z0_AnotherNumber, 124);
			ZQuery filter3 = new ZQuery(filter1, JoinCondition.Or, filter2);
			AssertEquals("Z0_Number = 123 or Z0_AnotherNumber = 124", filter3.LiteralTextADO);
		}

		public void TestNullComparisonForGuid()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Guid, null);
			AssertEquals("Z0_Guid is null", Filter.LiteralTextADO);
		}

		public void TestNullComparisonForEmptyZGeography()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Geography, ZGeography.Empty);
			AssertEquals("CONVERT(Z0_Geography, System.String) = 'POINT EMPTY'", Filter.LiteralTextADO);
		}

		public void TestADOComparisonForZGeography()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Geography, new ZGeography("POINT (-121 48)"));
			AssertEquals("CONVERT(Z0_Geography, System.String) = 'POINT (-121 48)'", Filter.LiteralTextADO);
		}

		public void TestNullComparisonForEmptyZDateTimeOffset()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, ZDateTimeOffset.Empty);
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) is null", Filter.LiteralTextADO);
		}

		public void TestNullComparisonForEmptyZDateTime()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, ZDateTime.Empty);
			AssertEquals("Z0_Date is null", Filter.LiteralTextADO);
		}

		public void TestConstructorColumnPlusValue()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 12345);

			AssertEquals("Z0_Number = " + ParameterNameFactory.GetParameterName(1), filter1.FilterString);
			AssertEquals("Z0_Number = 12345", filter1.LiteralTextADO);
		}

		public void TestConstructorSchemaColumnPlusValue()
		{
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Number, 123456);

			AssertEquals(DummyBizoSchema.Z0_Number.Name + " = " + ParameterNameFactory.GetParameterName(1), filter1.FilterString);
			AssertEquals(DummyBizoSchema.Z0_Number.Name + " = 123456", filter1.LiteralTextADO);
		}

		#endregion

		public void TestAndOrConditionWithNestedFilter()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 456);
			filter.AddToFilter(innerFilter, JoinCondition.And);

			AssertEquals("Z0_Code = '123' and Z0_Decimal = 456", filter.LiteralTextADO);
		}

		#region GetSingleEqualParameter

		public void TestNullOnEmptyQuery()
		{
			ZQuery filter = new ZQuery();
			AssertNull(filter.GetMostUniqueSingleEqualParameter());
		}

		public void TestPassesOnSingleQuery()
		{
			ZQuery filter = new ZQuery();
			filter.AddParameter("V", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, JoinCondition.Or, ComparisonOptions.Default);
			AssertEquals(filter.Params[0], filter.GetMostUniqueSingleEqualParameter());
		}

		public void TestNullOnMultiQuery()
		{
			ZQuery filter = new ZQuery();
			filter.AddParameter("V", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, JoinCondition.Or, ComparisonOptions.Default);
			filter.AddParameter("V", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, JoinCondition.Or, ComparisonOptions.Default);
			AssertNull(filter.GetMostUniqueSingleEqualParameter());
		}

		#endregion
		#region Test NoLockComesFromEnv

		public void TestNoLockComesFromEnv()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.RunSelectStatementsWithLocking = true;
				Assert("Should not have nolock", !(new ZQuery()).IsNoLock);
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.RunSelectStatementsWithLocking = false;
				Assert("Should not have nolock", !(new ZQuery()).IsNoLock);//the default value of ZQuery.IsNoLock is now static readonly. Its value will be same until Enterprise runs for the next time.
				ZQuery query = new ZQuery();
				query.IsNoLock = true;
				Assert("Should have nolock", query.IsNoLock);
			}
		}

		#endregion

		#region Test AddToFilter

		public void TestAddToFilterKeepsParametersFromOriginalFilters()
		{
			ZQuery relationshipFilter = new ZQuery();
			relationshipFilter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			ZQuery lastLoadedAdditionalFilter = new ZQuery();
			lastLoadedAdditionalFilter.AddToFilter(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18));

			ZGuid pK = ZGuid.Empty;
			ZQuery sQLFilter = new ZQuery(DummyBizoSchema.PK, pK);
			sQLFilter.AddToFilter(relationshipFilter, JoinCondition.And);
			sQLFilter.AddToFilter(lastLoadedAdditionalFilter, JoinCondition.And);
			AssertEquals("Z0_PK = CONVERT('00000000-0000-0000-0000-000000000000', 'System.Guid') and Z0_Code = '123' and Z0_Date = #1971-09-18 00:00:00.000#", sQLFilter.LiteralTextADO);
		}

		public void TestClonedFilterUpdatesParameters()
		{
			ZQuery relationshipFilter = new ZQuery();
			relationshipFilter.AddToFilter(DummyBizoSchema.Z0_Code, "Relationship");
			ZQuery relationshipHolderFilter = new ZQuery();
			relationshipHolderFilter.AddToFilter(relationshipFilter);

			ZQuery clonedRelationshipFilter = relationshipFilter.ShallowClone();
			relationshipHolderFilter.AddToFilter(clonedRelationshipFilter);

			AssertEquals("Parameter Count", 1, relationshipHolderFilter.Params.Length);
			AssertEquals("@CWO1_", relationshipHolderFilter.Params[0].ParameterName);
			AssertEquals("FilterString", "Z0_Code = @CWO1_ and Z0_Code = @CWO1_", relationshipHolderFilter.FilterString);
			new BusinessObjectFactory().Load(typeof(DummyBusinessObject), relationshipHolderFilter);
		}

		public void TestAddingSameSQLFilterTwiceDoesNotError()
		{
			ZQuery relationshipFilter = new ZQuery();
			relationshipFilter.AddToFilter(DummyBizoSchema.Z0_Code, "123");

			ZGuid pK = ZGuid.Empty;
			ZQuery sQLFilter = new ZQuery(DummyBizoSchema.PK, pK);
			sQLFilter.AddToFilter(relationshipFilter, JoinCondition.And);
			sQLFilter.AddToFilter(relationshipFilter, JoinCondition.And);
			AssertEquals("LiteralTextADO", "Z0_PK = CONVERT('00000000-0000-0000-0000-000000000000', 'System.Guid') and Z0_Code = '123' and Z0_Code = '123'", sQLFilter.LiteralTextADO);
			AssertEquals("FilterString", "Z0_PK = @CWO1_ and Z0_Code = @CWO2_ and Z0_Code = @CWO2_", sQLFilter.FilterString);
			AssertEquals("Parameter Count", 2, sQLFilter.Params.Length);
			// Just testing it does not error
			new BusinessObjectFactory().Load(typeof(DummyBusinessObject), sQLFilter);
		}

		public void TestUnion()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Union, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 2, 3, 4));
			filter.AddToFilter(JoinCondition.Union, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 0, 0, 0));

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Date = new ZDateTime(1971, 9, 18, 2, 3, 4);
			bizO1.Z0_Code = "AAA";
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Date = new ZDateTime(1971, 9, 18, 0, 0, 0);
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Date = new ZDateTime(1971, 9, 18, 23, 59, 59);
			DummyBusinessObject bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_Date = new ZDateTime(1971, 9, 17, 23, 59, 59);
			DummyBusinessObject bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_Date = new ZDateTime(1971, 9, 19, 0, 0, 0);
			factory.Save(); //union queries are DB-only
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK });
			filter.AddToFilter(JoinCondition.Union, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 23, 59, 59));
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK, bizO3.PK });
		}

		public void TestUnion_2()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(new ZQuery().AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 2, 3, 4)), JoinCondition.Union);
			filter.AddToFilter(new ZQuery().AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 0, 0, 0)), JoinCondition.Union);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Date = new ZDateTime(1971, 9, 18, 2, 3, 4);
			bizO1.Z0_Code = "AAA";
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Date = new ZDateTime(1971, 9, 18, 0, 0, 0);
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Date = new ZDateTime(1971, 9, 18, 23, 59, 59);
			DummyBusinessObject bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_Date = new ZDateTime(1971, 9, 17, 23, 59, 59);
			DummyBusinessObject bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_Date = new ZDateTime(1971, 9, 19, 0, 0, 0);
			factory.Save(); //union queries are DB-only
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK });
			filter.AddToFilter(new ZQuery().AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 23, 59, 59)), JoinCondition.Union);
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK, bizO3.PK });
			//'hoisting' mechanic - if you add an always-on filter to a query of union queries later, then it gets 'hoisted' into all the child queries, so behaviour is the same as if it were ORs
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK });
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "BBB");
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), Array.Empty<ZGuid>());
		}

		public void TestDeeplyNestedUnion()
		{
			var factory = new BusinessObjectFactory();
			var bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Date = new ZDateTime(1971, 9, 18, 2, 3, 4);
			bizO1.Z0_Code = "AAA";
			var bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Date = new ZDateTime(1971, 9, 18, 0, 0, 0);
			var bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Date = new ZDateTime(1971, 9, 18, 23, 59, 59);
			var bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_Date = new ZDateTime(1971, 9, 17, 23, 59, 59);
			var bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_Date = new ZDateTime(1971, 9, 19, 0, 0, 0);
			factory.Save(); //union queries are DB-only

			var filter = new ZQuery();
			filter.AddToFilter(new ZQuery().AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 2, 3, 4)), JoinCondition.Union);
			filter.AddToFilter(new ZQuery().AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 0, 0, 0)), JoinCondition.Union);

			var collection = new DummyBusinessObjectCollection(factory);

			var unionFirstFilter = new ZQuery();
			unionFirstFilter.AddToFilter(filter);
			unionFirstFilter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			collection.Load(unionFirstFilter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK });

			var doubleUnionFirstFilter = new ZQuery();
			doubleUnionFirstFilter.AddToFilter(unionFirstFilter);
			doubleUnionFirstFilter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			collection.Load(doubleUnionFirstFilter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK });
		}

		public void TestUnionAll()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.UnionAll, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 2, 3, 4));
			filter.AddToFilter(JoinCondition.UnionAll, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 0, 0, 0));

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Date = new ZDateTime(1971, 9, 18, 2, 3, 4);
			bizO1.Z0_Code = "AAA";
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Date = new ZDateTime(1971, 9, 18, 0, 0, 0);
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Date = new ZDateTime(1971, 9, 18, 23, 59, 59);
			DummyBusinessObject bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_Date = new ZDateTime(1971, 9, 17, 23, 59, 59);
			DummyBusinessObject bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_Date = new ZDateTime(1971, 9, 19, 0, 0, 0);
			factory.Save(); //union queries are DB-only
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK });
			filter.AddToFilter(JoinCondition.UnionAll, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 18, 23, 59, 59));
			collection.Load(filter);
			AssertContainsExactElementsInAnyOrder(collection.Select(x => x.PK), new ZGuid[] { bizO1.PK, bizO2.PK, bizO3.PK });
		}
		public void TestAddEqualToDatePartOnlyFilter()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTime(1971, 9, 18, 2, 3, 4));
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Date = new ZDateTime(1971, 9, 18, 2, 3, 4);
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Date = new ZDateTime(1971, 9, 18, 0, 0, 0);
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Date = new ZDateTime(1971, 9, 18, 23, 59, 59);
			DummyBusinessObject bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_Date = new ZDateTime(1971, 9, 17, 23, 59, 59);
			DummyBusinessObject bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_Date = new ZDateTime(1971, 9, 19, 0, 0, 0);
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			Assert(collection.FindByPK(bizO1.PK) != null);
			Assert(collection.FindByPK(bizO2.PK) != null);
			Assert(collection.FindByPK(bizO3.PK) != null);
			Assert(collection.FindByPK(bizO4.PK) == null);
			Assert(collection.FindByPK(bizO5.PK) == null);
		}

		public void TestAddEqualToDatePartOnlyFilter_ForDateTimeOffset()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTimeOffset(1971, 9, 18, 2, 3, 4, TimeSpan.FromHours(11)));
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 18, 2, 3, 4, TimeSpan.FromHours(11));
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(11));
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 18, 23, 59, 59, TimeSpan.FromHours(11));
			DummyBusinessObject bizO4 = DummyBusinessObject.New(factory);
			bizO4.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 17, 23, 59, 59, TimeSpan.FromHours(11));
			DummyBusinessObject bizO5 = DummyBusinessObject.New(factory);
			bizO5.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 19, 0, 0, 0, TimeSpan.FromHours(11));
			DummyBusinessObject bizO6 = DummyBusinessObject.New(factory);
			bizO6.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(8));
			DummyBusinessObject bizO7 = DummyBusinessObject.New(factory);
			bizO7.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(14));
			DummyBusinessObject bizO8 = DummyBusinessObject.New(factory);
			bizO8.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 19, 0, 0, 0, TimeSpan.FromHours(9));
			DummyBusinessObject bizO9 = DummyBusinessObject.New(factory);
			bizO9.Z0_DateTimeOffset = new ZDateTimeOffset(1971, 9, 19, 0, 0, 0, TimeSpan.FromHours(12));
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			Assert(collection.FindByPK(bizO1.PK) != null);
			Assert(collection.FindByPK(bizO2.PK) != null);
			Assert(collection.FindByPK(bizO3.PK) != null);
			Assert(collection.FindByPK(bizO4.PK) == null);
			Assert(collection.FindByPK(bizO5.PK) == null);
			Assert(collection.FindByPK(bizO6.PK) != null);
			Assert(collection.FindByPK(bizO7.PK) == null);
			Assert(collection.FindByPK(bizO8.PK) == null);
			Assert(collection.FindByPK(bizO9.PK) != null);
		}

		public void TestGeographyQuery()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Geography, SQLComparisonOperator.Equal, new ZGeography("POINT (-121 48)"));
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO1 = DummyBusinessObject.New(factory);
			bizO1.Z0_Geography = ZGeography.Empty;
			DummyBusinessObject bizO2 = DummyBusinessObject.New(factory);
			bizO2.Z0_Geography = new ZGeography("POINT (-121 48)");
			DummyBusinessObject bizO3 = DummyBusinessObject.New(factory);
			bizO3.Z0_Geography = new ZGeography("POINT (-121.1 48)");
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			collection.Load(filter);
			Assert(collection.FindByPK(bizO1.PK) == null);
			Assert(collection.FindByPK(bizO2.PK) != null);
			Assert(collection.FindByPK(bizO3.PK) == null);

			factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObjectCollection collection2 = new DummyBusinessObjectCollection(factory2);
			collection2.Load(filter);
			Assert(collection2.FindByPK(bizO1.PK) == null);
			Assert(collection2.FindByPK(bizO2.PK) != null);
			Assert(collection2.FindByPK(bizO3.PK) == null);

			ZQuery filter2 = new ZQuery();
			filter2.AddToFilter(DummyBizoSchema.Z0_Geography, SQLComparisonOperator.Equal, ZGeography.Empty);
			collection2.Load(filter2);
			Assert(collection2.FindByPK(bizO1.PK) != null);
			Assert(collection2.FindByPK(bizO2.PK) == null);
			Assert(collection2.FindByPK(bizO3.PK) == null);
		}

		public void TestGeographyQueryCastsAsVarbinary()
		{
			ZQuery filter = new ZQuery();
			var builder = new SqlBuilder();
			filter.AddAsCompleteSQLStatement(builder, DummyBizoSchema.Constants.TableName, true, new[] { DummyBizoSchema.Z0_Geography });
			AssertContains("CAST(Z0_Geography AS varbinary(max)) AS Z0_Geography", builder.ToString());
		}

		public void TestTableHintPropagation()
		{
			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) => query.IsNoLock = value,
				tableHintGetter: (query) => query.IsNoLock);

			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) => query.IsForceSeek = value,
				tableHintGetter: (query) => query.IsForceSeek);

			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.READPAST : query.TableHints & ~TableHints.READPAST,
				tableHintGetter: (query) => query.TableHints.HasFlag(TableHints.READPAST));

			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.UPDLOCK : query.TableHints & ~TableHints.UPDLOCK,
				tableHintGetter: (query) => query.TableHints.HasFlag(TableHints.UPDLOCK));

			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.ROWLOCK : query.TableHints & ~TableHints.ROWLOCK,
				tableHintGetter: (query) => query.TableHints.HasFlag(TableHints.ROWLOCK));

			AssertOneTableHintMeansEverythingHasIt(
				tableHintSetter: (query, value) =>
				{
					if (value)
					{
						query.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
					}
					else
					{
						query.TableIndexHints.Clear();
					}
				},
				tableHintGetter: (query) => query.TableIndexHints.Select(indexHint => indexHint.IndexName).Contains("SomeNDX"));
		}

		void AssertOneTableHintMeansEverythingHasIt(Action<ZQuery, bool> tableHintSetter, Func<ZQuery, bool> tableHintGetter, bool invertExpectation = false)
		{
			var filterWithLock = new ZQuery();
			tableHintSetter(filterWithLock, false);

			var filterWithNoLock = new ZQuery();
			tableHintSetter(filterWithNoLock, true);

			var testFilter1 = new ZQuery();
			tableHintSetter(testFilter1, false);
			testFilter1.AddToFilter(filterWithNoLock);

			Assert("Should have hint", tableHintGetter(testFilter1) == !invertExpectation);

			Assert("Should have hint", tableHintGetter(new ZQuery(filterWithLock, filterWithNoLock)) == !invertExpectation);

			var testFilter2 = new ZQuery();
			tableHintSetter(testFilter2, false);
			testFilter2.AddToFilter(filterWithLock);
			Assert("Should not have hint", !tableHintGetter(testFilter2));

			testFilter2.AddToFilter(filterWithNoLock);
			Assert("Should have hint", tableHintGetter(testFilter2) == !invertExpectation);
		}

		public void TestAddToFilter()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, 123);
			AssertEquals("Equals filter string", "Z0_Number = " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithEquals()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code = " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithEqualsAndJoinCondition()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, "123");
			AssertEquals("Does not equal filter string", "Z0_Code = " + ParameterNameFactory.GetParameterName(1) + " or Z0_Code = " + ParameterNameFactory.GetParameterName(2), Filter.FilterString);
		}

		public void TestAddToFilterWithDoesNotEqual()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code <> " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithGreaterThan()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThan, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code > " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithLessThan()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.LessThan, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code < " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithGreaterThanOrEqualTo()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThanOrEqualTo, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code >= " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithLessThanOrEqualTo()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.LessThanOrEqualTo, "XYZ");
			AssertEquals("Does not equal filter string", "Z0_Code <= " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestLiteralTextADOEscapingAsteriskForEquals()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "A*BC");
			AssertEquals("Z0_Description = 'A*BC'", Filter.LiteralTextADO);
		}

		public void TestLiteralTextADOEscapingAsteriskForLike()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, "A*BC");
			AssertEquals("Z0_Description like '%A[*]BC%'", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithLessThanOrEqualToDatePartOnly()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDateTime(2003, 10, 10, 9, 9, 9));

			AssertEquals("Does not equal filter string", "Z0_Date < @CWO1_", Filter.FilterString);
			AssertEquals("Does not equal filter string", "Z0_Date < #2003-10-11 00:00:00.000#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithLessThanOrEqualToDatePartOnly_DateTimeOffsetVersion()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDateTimeOffset(2003, 10, 10, 9, 9, 9, TimeSpan.FromHours(11)));

			AssertEquals("Does not equal filter string", "Z0_DateTimeOffset < @CWO1_", Filter.FilterString);
			AssertEquals("Does not equal filter string", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #2003-10-11 00:00:00.0000000 +11:00#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithGreaterThanOrEqualToDatePartOnly()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, new ZDateTime(2003, 10, 10, 9, 9, 9));

			AssertEquals("Does not equal filter string", "Z0_Date >= @CWO1_", Filter.FilterString);
			AssertEquals("Does not equal filter string", "Z0_Date >= #2003-10-10 00:00:00.000#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithGreaterThanOrEqualToDatePartOnly_DateTimeOffsetVersion()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, new ZDateTimeOffset(2003, 10, 10, 9, 9, 9, TimeSpan.FromHours(11)));

			AssertEquals("Does not equal filter string", "Z0_DateTimeOffset >= @CWO1_", Filter.FilterString);
			AssertEquals("Does not equal filter string", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #2003-10-10 00:00:00.0000000 +11:00#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithEqualToDatePartOnly()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTime(2003, 10, 10, 9, 9, 9));

			AssertEquals("Does not equal filter string", "Z0_Date >= @CWO1_ and Z0_Date < @CWO2_", Filter.FilterString);
			AssertEquals("Does not equal Plain Filter", "Z0_Date >= #2003-10-10 00:00:00.000# and Z0_Date < #2003-10-11 00:00:00.000#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithEqualToDatePartOnly_DateTimeOffsetVersion()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTimeOffset(2003, 10, 10, 9, 9, 9, TimeSpan.FromHours(11)));

			AssertEquals("Does not equal filter string", "Z0_DateTimeOffset >= @CWO1_ and Z0_DateTimeOffset < @CWO2_", Filter.FilterString);
			AssertEquals("Does not equal Plain Filter", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #2003-10-10 00:00:00.0000000 +11:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #2003-10-11 00:00:00.0000000 +11:00#", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithEqualAndEqualToDatePartOnly()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, 123);
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTime(2003, 10, 10, 9, 9, 9));
			AssertEquals("Plain Filter", "Z0_Number = 123 and (Z0_Date >= #2003-10-10 00:00:00.000# and Z0_Date < #2003-10-11 00:00:00.000#)", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithEqualAndEqualToDatePartOnly_DateTimeOffsetVersion()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, 123);
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTimeOffset(2003, 10, 10, 9, 9, 9, TimeSpan.FromHours(11)));
			AssertEquals("Plain Filter", "Z0_Number = 123 and (CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #2003-10-10 00:00:00.0000000 +11:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #2003-10-11 00:00:00.0000000 +11:00#)", Filter.LiteralTextADO);
		}

		public void TestAddToFilter_WithNationalLanguage()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_NVarCharMax = (char)33337 + " won ton soup";
			DummyBusinessObject dummy2 = factory.New<DummyBusinessObject>();
			dummy2.Z0_NVarCharMax = "English";

			Filter.AddToFilter(DummyBizoSchema.Z0_NVarCharMax, SQLComparisonOperator.StartsWith, "" + (char)33337, ComparisonOptions.NationalLanguage);

			AssertEquals("Loading objects in memory", 1, factory.Load<DummyBusinessObject>(Filter).Length);
			factory.Save();
			AssertEquals("Loading from the database", 1, newFactory.Load<DummyBusinessObject>(Filter).Length);

			Filter = new ZQuery();
			Filter.AddToFilter(DummyBizoSchema.Z0_NVarCharMax, SQLComparisonOperator.StartsWith, "" + (char)33337);
			newFactory = new BusinessObjectFactory();
			AssertEquals("Loading from the database without NationalLanguage", 1, newFactory.Load<DummyBusinessObject>(Filter).Length);
		}

		public void TestAddToFilter_WithNationalLanguageAndInFilter()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_NVarChar = (char)33337 + " won ton soup";
			DummyBusinessObject dummy2 = factory.New<DummyBusinessObject>();
			dummy2.Z0_NVarChar = "English";

			ZString[] criteria = new ZString[] { "Shrimp on the barby", "" + (char)33337 + " won ton soup" };
			Filter.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Equal, criteria, ComparisonOptions.NationalLanguage);

			AssertEquals("Loading objects in memory", 1, factory.Load<DummyBusinessObject>(Filter).Length);
			factory.Save();
			AssertEquals("Loading from the database", 1, newFactory.Load<DummyBusinessObject>(Filter).Length);

			Filter = new ZQuery();
			Filter.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Equal, criteria);
			newFactory = new BusinessObjectFactory();
			AssertEquals("Loading from the database without NationalLanguage", 1, newFactory.Load<DummyBusinessObject>(Filter).Length);
		}

		[ExpectNoExceptions]
		public void TestAddToFilter_WithIEnumerable()
		{
			IEnumerable enumerable = new EnumerableForTest();

			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, enumerable);
			AssertEquals("IEnumerable added correctly", "(Z0_Code in ('1one', '2two', '3three'))", query.LiteralTextADO);

			string stringParameter = "hello";
			query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, stringParameter);
			AssertEquals("String added correctly", "Z0_Code = 'hello'", query.LiteralTextADO);
		}

		class EnumerableForTest : IEnumerable
		{
			public IEnumerator GetEnumerator()
			{
				return (new string[] { "1one", "2two", "3three" }).GetEnumerator();
			}
		}

		public void TestTwoParameters()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			Filter.AddToFilter(DummyBizoSchema.Z0_AnotherNumber, 2);
			AssertEquals("Filter String", "Z0_Number = " + ParameterNameFactory.GetParameterName(1) + " and Z0_AnotherNumber = " + ParameterNameFactory.GetParameterName(2), Filter.FilterString);
		}

		public void TestEqualToNull()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Guid, null);
			AssertEquals("Null value column equals", "Z0_Guid is NULL", Filter.FilterString);
			AssertEquals("Parameter Count", 0, Filter.Params.Length);
		}

		public void TestIsNull()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.NotEqual, null);
			AssertEquals("Null value column DoesNotEqual", "Z0_Guid is not NULL", Filter.FilterString);
			AssertEquals("Parameter Count", 0, Filter.Params.Length);
		}

		public void TestAddToFilterWithZQuery()
		{
			ZQuery tempFilter = new ZQuery(DummyBizoSchema.Z0_Description, "Value");
			Filter.AddToFilter(tempFilter);
			AssertEquals("Z0_Description = " + ParameterNameFactory.GetParameterName(1), Filter.FilterString);
		}

		public void TestAddToFilterWithIgnoreBlobFieldsCheck()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, "Value");
			filter.IgnoreBlobFieldsCheck = true;
			var anotherFilter = new ZQuery();
			anotherFilter.AddToFilter(filter);
			AssertEquals(true, anotherFilter.IgnoreBlobFieldsCheck);

			filter = new ZQuery(DummyBizoSchema.Z0_Description, "Value");
			filter.IgnoreBlobFieldsCheck = false;
			anotherFilter = new ZQuery();
			anotherFilter.AddToFilter(filter);
			AssertEquals(false, anotherFilter.IgnoreBlobFieldsCheck);
		}

		public void TestAddWithAnd()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_AnotherNumber, SQLComparisonOperator.Equal, 2);

			ZQuery tempFilter = new ZQuery(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 3);
			tempFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_AnotherDecimal, SQLComparisonOperator.Equal, 4);

			ZQuery topLevelFilter = new ZQuery(Filter, JoinCondition.And, tempFilter);
			string expectedFilter = "(Z0_Number = @CWO1_ or Z0_AnotherNumber = @CWO2_) and (Z0_Decimal = @CWO3_ or Z0_AnotherDecimal = @CWO4_)";
			AssertEquals("Filters should be joined with AND, with brackets around both filters", expectedFilter, topLevelFilter.FilterString);
		}

		/// <summary>
		/// Inserts a few rows in the DummyBizo table to test if ORs and ANDs are properly enclosed by brackets.
		///   Z0_Code        = Player Code (ID)
		///   Z0_Description = Player Field Position
		///   Z0_Number      = Player T-shirt Number
		/// </summary>
		public void TestAddWithOrAnd()
		{
			string sqlText = @"
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PF09', 'forward', 9)
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PF10', 'forward', 10)
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PM08', 'midfield', 8)
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PM10', 'midfield', 10)
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PG01', 'goalkeeper', 1)
		INSERT dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description, Z0_Number) VALUES (newid(), 'PG10', 'goalkeeper', 10)";
			Db.Connection.ExecuteNonQuery(sqlText);

			ZQuery descFilter = new ZQuery();
			descFilter.AddToFilter(DummyBizoSchema.Z0_Description, "forward");
			descFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "midfield");

			ZQuery testFilter = new ZQuery();
			testFilter.AddToFilter(descFilter);
			testFilter.AddToFilter(DummyBizoSchema.Z0_Number, 10);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			DummyBusinessObject[] loadedPlayers = (DummyBusinessObject[])testFactory.Load(typeof(DummyBusinessObject), testFilter);
			AssertEquals("Loaded Players", 2, loadedPlayers.Length);

			bool forward10Loaded = (loadedPlayers[0].Z0_Code == "PF10" || loadedPlayers[1].Z0_Code == "PF10");
			AssertEquals("Player Forward-10 should have been loaded", true, forward10Loaded);

			bool midfield10Loaded = (loadedPlayers[0].Z0_Code == "PM10" || loadedPlayers[1].Z0_Code == "PM10");
			AssertEquals("Player Midfield-10 should have been loaded", true, midfield10Loaded);

			bool forward09Loaded = (loadedPlayers[0].Z0_Code == "PF09" || loadedPlayers[1].Z0_Code == "PF09");
			AssertEquals("Player Forward-09 should NOT have been loaded", false, forward09Loaded);

			string expectedFilter = "(Z0_Description = " + ParameterNameFactory.GetParameterName(1) + " or Z0_Description = " + ParameterNameFactory.GetParameterName(2);
			expectedFilter += ") and Z0_Number = " + ParameterNameFactory.GetParameterName(3);
			AssertEquals("Filters should be joined like '(Condition1 or Condition2) and Condition3'", expectedFilter, testFilter.FilterString);
		}

		public void TestAddWithParameterNumberOverlap()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
			Filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 2);

			ZQuery tempFilter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "3");
			tempFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 4);

			ZQuery topLevelFilter = new ZQuery(Filter, JoinCondition.Or, tempFilter);
			string expectedFilter =
				"(Z0_Code = " + ParameterNameFactory.GetParameterName(1) +
				" and Z0_Number = " + ParameterNameFactory.GetParameterName(2) + ")" +
				" or (Z0_Description = " + ParameterNameFactory.GetParameterName(3) +
				" or Z0_Number = " + ParameterNameFactory.GetParameterName(4) + ")";
			AssertEquals("Brackets should be applied to nested ZQuerys", expectedFilter, topLevelFilter.FilterString);

			AssertEquals("First parameter value", "1", topLevelFilter.Params[0].Value);
			AssertEquals("Second parameter value", 2, topLevelFilter.Params[1].Value);
			AssertEquals("Third parameter value", "3", topLevelFilter.Params[2].Value);
			AssertEquals("Fourth parameter value", 4, topLevelFilter.Params[3].Value);

			string expectedPlainFilter = "(Z0_Code = '1' and Z0_Number = 2) or (Z0_Description = '3' or Z0_Number = 4)";
			AssertEquals("Plain filter", expectedPlainFilter, topLevelFilter.LiteralTextADO);
		}

		public void TestParameterOrderRenaming()
		{
			ZQuery sQLFilter1 = new ZQuery();
			sQLFilter1.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.NotEqual, 0);

			ZQuery sQLFilter2 = new ZQuery();
			sQLFilter2.AddToFilter(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, "Y");
			sQLFilter2.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThan, ZDateTime.Today);

			sQLFilter1.AddToFilter(sQLFilter2, JoinCondition.And);

			ZQuery testFilter = new ZQuery();
			testFilter.AddToFilter(sQLFilter1);

			string expectedFilter = "Z0_Number <> 0 and (Z0_Bool = 1 or Z0_Date < #" + ZDateTime.Today.SqlFormat + "#)";
			AssertEquals("Plain filter", expectedFilter, testFilter.LiteralTextADO);
		}

		public void TestAddWithMaxRows()
		{
			Filter.MaximumRows = 5;
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "5");
			AssertEquals("Plain filter", "Z0_Code = '5'", Filter.LiteralTextADO);
		}

		public void TestNineParametersDirectly()
		{
			ZString expectedFilter = "";

			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
			expectedFilter += "Z0_Code = '1' or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "2");
			expectedFilter += "Z0_Description = '2' or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_FK_Code, SQLComparisonOperator.Equal, "3");
			expectedFilter += "Z0_FK_Code = '3' or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, false);
			expectedFilter += "Z0_Bool = 0 or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 5);
			expectedFilter += "Z0_Number = 5 or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_AnotherNumber, SQLComparisonOperator.Equal, 6);
			expectedFilter += "Z0_AnotherNumber = 6 or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 7);
			expectedFilter += "Z0_Decimal = 7 or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_AnotherDecimal, SQLComparisonOperator.Equal, 8);
			expectedFilter += "Z0_AnotherDecimal = 8 or ";
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Short, SQLComparisonOperator.Equal, (short)9);
			expectedFilter += "Z0_Short = 9";

			AssertEquals("Filter should match", expectedFilter, Filter.LiteralTextADO);
		}

		public void TestReloadExistingRows_PropagatesOnAddToFilter()
		{
			ZQuery filter = new ZQuery();
			filter.ReLoadExistingRows = true;

			ZQuery filter2 = new ZQuery();
			filter2.AddToFilter(filter);
			AssertEquals("ReLoadExistingRows gets copied on AddToFilter", true, filter2.ReLoadExistingRows);
		}

		public void TestIgnoreDbQueryCache_PropagatesOnAddToFilter()
		{
			ZQuery filter = new ZQuery();
			filter.IgnoreDbQueryCache = true;

			ZQuery filter2 = new ZQuery();
			filter2.AddToFilter(filter);
			AssertEquals("IgnoreDbQueryCache gets copied on AddToFilter", true, filter2.IgnoreDbQueryCache);
		}

		#endregion

		#region Test BlobFilters

		public void TestBlobFilters()
		{
			foreach (SchemaColumn schemaColumn in DummyBizoSchema.All)
			{
				if (schemaColumn is SchemaGuidColumn)
				{
					AssertEquals(schemaColumn.IsLargeBinaryOrText, new ZQuery(schemaColumn, ZGuid.NewZGuid()).BlobFilters.Contains(schemaColumn));
				}
				else
				{
					AssertEquals(schemaColumn.IsLargeBinaryOrText, new ZQuery(schemaColumn, schemaColumn.SqlDbDefault).BlobFilters.Contains(schemaColumn));
				}
			}

			Filter.AddToFilter(DummyBizoSchema.Z0_Description, "Z0_Description search string");
			AssertEquals(0, Filter.BlobFilters.Count());

			Filter.AddToFilter(new ZQuery(DummyBizoSchema.Z0_Number, 0)); // ensure recursion continues looking for blobs after non-blob subfilter
			Filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "Z0_VarCharMax search string");
			AssertEquals(true, Filter.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));

			AssertEquals(true, new ZQuery(DummyBizoSchema.Z0_VarCharMax, "Z0_VarCharMax search string").BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
		}

		#endregion

		#region Test Parameters

		public void TestAndAndOrParameterCausesBracketing()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			Filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
			Filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
			AssertEquals("LiteralTextADO", "Z0_Code = '1' and Z0_Code = '1' or Z0_Code = '1'", Filter.LiteralTextADO);
			//				AssertEquals("ErrorReporter.LastKeyReported", "And and Or appearing in same SQL", ErrorReporter.LastKeyReported);
			//				ErrorReporter.Clear();
		}

		public void TestParameterCount()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			AssertEquals("Parameter Count after 1st add", 1, Filter.Params.Length);
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, "2");
			AssertEquals("Parameter Count after 2nd add", 2, Filter.Params.Length);
		}

		public void TestParameterContents()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			Filter.AddToFilter(DummyBizoSchema.Z0_Number, 2);
			AssertEquals(ParameterNameFactory.GetParameterName(1), Filter.Params[0].ParameterName);
			AssertEquals(ParameterNameFactory.GetParameterName(2), Filter.Params[1].ParameterName);
			AssertEquals("1", Filter.Params[0].Value);
			AssertEquals(2, Filter.Params[1].Value);
		}

		#endregion

		public void TestDatalength()
		{
			ZQuery query = new ZQuery();
			string sql = query.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, true);
			Assert(sql.Contains("datalength(Z0_NVarCharMax)"));
			Assert(sql.Contains("datalength(Z0_VarBinaryMax)"));
			Assert(sql.Contains("datalength(Z0_VarCharMax)"));
			Assert(sql.Contains("datalength(Z0_Xml)"));
			Assert(!sql.Contains("len(Z0_"));
		}

		public void TestDBQueryWithEmptyZDateTime()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, ZDateTime.Empty);
			AssertEquals("Z0_Date is NULL", Filter.ParameterisedText.ParameterisedQueryText);
		}

		public void TestDBQueryWithEmptyZTime()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Time, ZTime.Empty);
			AssertEquals("Z0_Time is NULL", Filter.ParameterisedText.ParameterisedQueryText);
		}

		public void TestDBQueryWithEmptyZDateTimeOffset()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, ZDateTimeOffset.Empty);
			AssertEquals("Z0_DateTimeOffset is NULL", Filter.ParameterisedText.ParameterisedQueryText);
		}

		public void TestIsDataViewOptimisableDBOnly()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(true, Filter.IsDataViewOptimisable);
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			Filter.AddToFilter(dbOnlyQuery);
			AssertEquals(false, Filter.IsDataViewOptimisable);
		}

		public void TestDataViewTopNFunctional()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.IndexingEnabled = true;
			for (int i = 0; i < 1000; i++)
			{
				factory.New<DummyBusinessObject>();
			}
			ZQuery query = new ZQuery();
			query.MaximumRows = 12;
			AssertEquals("Precondition", true, query.IsDataViewOptimisable);
			AssertEquals(12, factory.Load<DummyBusinessObject>(query).Length);
		}

		public void TestDataViewTopNWithOrderByFunctional()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.IndexingEnabled = true;
			for (int i = 0; i < 1000; i++)
			{
				factory.New<DummyBusinessObject>().Z0_Number = 1000 - i;
			}
			ZQuery query = new ZQuery();
			query.MaximumRows = 12;
			query.OrderBy = DummyBizoSchema.Z0_Number.Name + " desc";
			AssertEquals("Precondition", true, query.IsDataViewOptimisable);
			DummyBusinessObject[] bizOs = factory.Load<DummyBusinessObject>(query);
			AssertEquals(12, bizOs.Length);
			for (int i = 0; i < bizOs.Length; i++)
			{
				AssertEquals(1000 - i, bizOs[i].Z0_Number);
			}
		}

		public void TestIsDataViewOptimisableChildObjects()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(true, Filter.IsDataViewOptimisable);
			Filter.AddFilterAndZSQLParameterCollection("string", null);
			AssertEquals(true, Filter.IsDataViewOptimisable);
		}

		public void TestDBQueryWithNull()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Guid, null);
			AssertEquals("Z0_Guid is NULL", Filter.ParameterisedText.ParameterisedQueryText);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestNullWithGreaterThan()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.GreaterThan, null);
			string sqlADO = Filter.LiteralTextADO;
		}

		public void TestAddToFilterWithZDecimalZero()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Decimal, ZDecimal.Zero);
			AssertEquals("Z0_Decimal = 0", Filter.LiteralTextADO);
		}

		public void TestAddToFilterGreaterThanOrEqualToDecimalZero()
		{
			Filter.AddToFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.GreaterThanOrEqualTo, 0m);
			AssertEquals("Z0_Decimal >= 0", Filter.LiteralTextADO);
		}
		public void TestDateFormatIsPresentedInSQLFormat()
		{
			DateTime currentTime = DateTime.Now;
			string sQLFormatCurrentTime = new ZDateTime(currentTime).SqlFormat;
			Filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThan, currentTime);
			AssertEquals("PlainFilter", "Z0_Date > #" + sQLFormatCurrentTime + "#", Filter.LiteralTextADO);
		}

		public void TestDateTimeOffsetFormatIsPresentedInSQLFormat()
		{
			DateTimeOffset currentTime = DateTimeOffset.Now;
			string sQLFormatCurrentTime = new ZDateTimeOffset(currentTime).SqlFormat;
			Filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.GreaterThan, currentTime);
			AssertEquals("PlainFilter", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) > #" + sQLFormatCurrentTime + "#", Filter.LiteralTextADO);
		}

		public void TestIsEmpty()
		{
			Assert("Filter is empty", Filter.IsEmpty);
			Filter.AddToFilter(DummyBizoSchema.Z0_Code, "val1");
			Assert("Filter is not empty", !Filter.IsEmpty);

			Filter = new ZQuery();
			Filter.MaximumRows = 1;
			Assert("Filter is not empty", !Filter.IsEmpty);

			Filter = new ZQuery();
			Filter.OrderBy = "test";
			Assert("Filter is not empty", !Filter.IsEmpty);

			Filter = new ZQuery();
			Filter.IsNoResultQuery = true;
			Assert("Filter should not be empty if IsNoResultQuery flag is set", !Filter.IsEmpty);
		}

		public void TestAddToFilterWithSingleAddToFilter()
		{
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.NotEqual, null);

			ZQuery outerFilter = new ZQuery();
			outerFilter.MaximumRows = 100;
			outerFilter.DefaultJoinCondition = JoinCondition.And;
			outerFilter.AddToFilter(innerFilter);
			Assert("No dodgey () in filter string", outerFilter.FilterString.IndexOf("()") == -1);
		}

		public void TestAddToFilter_WithNoResultQuery()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Value");
			Assert("There should be a filter initially", !string.IsNullOrEmpty(filter.LiteralTextADO));

			filter.AddToFilter(ZQuery.NoResultQuery, JoinCondition.Or);
			Assert("There should still be a filter on logical OR", !string.IsNullOrEmpty(filter.LiteralTextADO));

			filter.AddToFilter(ZQuery.NoResultQuery, JoinCondition.And);
			Assert("There should no longer be a filter on logical AND", string.IsNullOrEmpty(filter.LiteralTextADO));
		}

		public void TestIgnoreActiveFilter_GetsCopiedWhenAddToFilter()
		{
			ZQuery ignoreActiveFilterFilter = new ZQuery();
			ignoreActiveFilterFilter.IgnoreActiveFilter = true;
			ZQuery filter = new ZQuery();

			AssertEquals("Should not touch the IgnoreActiveFilter at first", false, filter.IgnoreActiveFilter);
			filter.AddToFilter(ignoreActiveFilterFilter);
			AssertEquals("Should copy the 'IgnoreActiveFilter'", true, filter.IgnoreActiveFilter);
			filter.AddToFilter(new ZQuery());
			AssertEquals("Should still have the 'IgnoreActiveFilter' flag set", true, filter.IgnoreActiveFilter);
		}

		public void TestNestedSQLFilterAddBracketsToStartingCondition()
		{
			ZQuery initialFilter = new ZQuery();
			initialFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "A");
			initialFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "B");
			initialFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "C");
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Code, "D");
			initialFilter.AddToFilter(innerFilter, JoinCondition.And);
			AssertEquals("(Z0_Code = 'A' or Z0_Code = 'B' or Z0_Code = 'C') and Z0_Code = 'D'", initialFilter.LiteralTextADO);
		}

		public void TestTwoFiltersNestedInAnotherFilterWithOr()
		{
			ZQuery initialFilter = new ZQuery();
			ZQuery nested1Filter = new ZQuery(DummyBizoSchema.Z0_Code, "a");
			ZQuery nested2Filter = new ZQuery(DummyBizoSchema.Z0_Code, "b");
			initialFilter.AddToFilter(nested1Filter);
			initialFilter.AddToFilter(nested2Filter, JoinCondition.Or);
			AssertEquals("Z0_Code = 'a' or Z0_Code = 'b'", initialFilter.LiteralTextADO);
		}

		public void TestTwoFiltersNestedInAnotherFilterWithOrInBrackets()
		{
			ZQuery initialFilter = new ZQuery();
			ZQuery nested1Filter = new ZQuery(DummyBizoSchema.Z0_Code, "a");
			nested1Filter.DefaultJoinCondition = JoinCondition.Or;
			nested1Filter.AddToFilter(DummyBizoSchema.Z0_Code, "b");
			ZQuery nested2Filter = new ZQuery(DummyBizoSchema.Z0_Code, "b");
			nested2Filter.DefaultJoinCondition = JoinCondition.Or;
			nested2Filter.AddToFilter(DummyBizoSchema.Z0_Code, "c");
			initialFilter.AddToFilter(nested1Filter);
			initialFilter.AddToFilter(nested2Filter, JoinCondition.Or);
			AssertEquals("(Z0_Code = 'a' or Z0_Code = 'b') or (Z0_Code = 'b' or Z0_Code = 'c')", initialFilter.LiteralTextADO);
		}

		public void TestTwoFiltersUsingAndNestedInAnotherFilterWithOrInBrackets()
		{
			ZQuery initialFilter = new ZQuery();
			ZQuery nested1Filter = new ZQuery(DummyBizoSchema.Z0_Code, "1");
			nested1Filter.DefaultJoinCondition = JoinCondition.Or;
			nested1Filter.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			ZQuery nested2Filter = new ZQuery(DummyBizoSchema.Z0_Code, "3");
			nested2Filter.DefaultJoinCondition = JoinCondition.Or;
			nested2Filter.AddToFilter(DummyBizoSchema.Z0_Code, "4");
			initialFilter.AddToFilter(nested1Filter);
			initialFilter.AddToFilter(nested2Filter, JoinCondition.And);
			AssertEquals("(Z0_Code = '1' or Z0_Code = '2') and (Z0_Code = '3' or Z0_Code = '4')", initialFilter.LiteralTextADO);
		}

		public void TestInFilterBracketing()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "1");
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "2");
			ZSQLInFilter inFilter = new ZSQLInFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, new decimal[] { 1, 2, 3 }, ComparisonOptions.Default);
			filter.AddToFilter(inFilter, JoinCondition.And);
			AssertEquals("(Z0_Code = '1' or Z0_Code = '2') and (Z0_Decimal in (1, 2, 3))", filter.LiteralTextADO);
		}

		public void TestTwoFiltersNestedInAnotherFilterWithAndInBrackets()
		{
			ZQuery initialFilter = new ZQuery();
			ZQuery nested1Filter = new ZQuery(DummyBizoSchema.Z0_Code, "a");
			nested1Filter.DefaultJoinCondition = JoinCondition.And;
			nested1Filter.AddToFilter(DummyBizoSchema.Z0_Code, "b");
			ZQuery nested2Filter = new ZQuery(DummyBizoSchema.Z0_Code, "b");
			nested2Filter.DefaultJoinCondition = JoinCondition.And;
			nested2Filter.AddToFilter(DummyBizoSchema.Z0_Code, "c");
			initialFilter.AddToFilter(nested1Filter);
			initialFilter.AddToFilter(nested2Filter, JoinCondition.Or);
			AssertEquals("(Z0_Code = 'a' and Z0_Code = 'b') or (Z0_Code = 'b' and Z0_Code = 'c')", initialFilter.LiteralTextADO);
		}

		public void TestTableHintIsPresentInSqlOutput()
		{
			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) => { query.IsNoLock = value; },
				expectedTableHint: "NOLOCK");

			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) => { query.IsForceSeek = value; },
				expectedTableHint: "FORCESEEK");

			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.READPAST : query.TableHints & ~TableHints.READPAST,
				expectedTableHint: TableHintNames.Name(TableHints.READPAST));

			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.UPDLOCK : query.TableHints & ~TableHints.UPDLOCK,
				expectedTableHint: TableHintNames.Name(TableHints.UPDLOCK));

			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) => query.TableHints = value ? query.TableHints | TableHints.ROWLOCK : query.TableHints & ~TableHints.ROWLOCK,
				expectedTableHint: TableHintNames.Name(TableHints.ROWLOCK));

			AssertTableHintIsPresentInSqlOutput(
				tableHintSetter: (query, value) =>
				{
					if (value)
					{
						query.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
					}
					else
					{
						query.TableIndexHints.Clear();
					}
				},
				expectedTableHint: "INDEX");
		}

		void AssertTableHintIsPresentInSqlOutput(Action<ZQuery, bool> tableHintSetter, string expectedTableHint)
		{
			var filter = GetNewSQLFilter();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, "teapot");
			tableHintSetter(filter, true);
			Assert($"{expectedTableHint} present", filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, true).ToUpper().IndexOf(expectedTableHint) != -1);

			tableHintSetter(filter, false);
			Assert($"{expectedTableHint} no present", filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, true).IndexOf(expectedTableHint) == -1);
		}

		public void TestClearTableIndexHintsIncludingSubQueries()
		{
			var filter = GetNewSQLFilterWithTableIndexHints();
			AssertEquals(2, filter.TableIndexHints.Count);

			CombineAssertions(() =>
			{
				var sql = filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, false);
				AssertContains("WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered, NR_RX__Z0_BitFiltered))", sql);
				AssertContains("WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered2))", sql);

				filter.ClearTableIndexHintsIncludingSubQueries();
				sql = filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, false);
				AssertNotContains("NR_RX__Z0_BitFiltered", sql);
				AssertNotContains("NR_RX__Z0_BitFiltered2", sql);
				AssertEquals(0, filter.TableIndexHints.Count);
			});
		}

		public void TestDisableForceSeekIncludingSubQueries()
		{
			var filter = GetNewSQLFilterWithTableIndexHints();
			AssertEquals(true, filter.IsForceSeek);

			CombineAssertions(() =>
			{
				var sql = filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, false);
				AssertContains("WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered, NR_RX__Z0_BitFiltered))", sql);
				AssertContains("WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered2))", sql);

				filter.DisableForceSeekIncludingSubQueries();
				sql = filter.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, false);
				AssertNotContains("FORCESEEK", sql);
				AssertEquals(false, filter.IsForceSeek);
			});
		}

		ZQuery GetNewSQLFilterWithTableIndexHints()
		{
			var filter = GetNewSQLFilter();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, "teapot");
			filter.TableIndexHints.Add(new TableIndexHint("NR_RX__Z0_BitFiltered"));
			filter.IsForceSeek = true;
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, "teapot");
			var hint = new TableIndexHint("NR_RX__Z0_BitFiltered");
			dbOnlyQuery.TableIndexHints.Add(hint);
			dbOnlyQuery.IsForceSeek = true;
			var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "teapot2");
			subQuery.TableIndexHints.Add(new TableIndexHint("NR_RX__Z0_BitFiltered2"));
			subQuery.IsForceSeek = true;
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			filter.AddToFilter(dbOnlyQuery);
			return filter;
		}

		public void TestNewFieldsAsClassesAreTracked()
		{
			Assert(true);
			foreach (FieldInfo field in typeof(ZQuery).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.FieldType != typeof(string) &&
					field.FieldType.IsClass &&
					field.Name != "JoinCondition" &&
					field.Name != "defaultJoinCondition" &&
					field.Name != "filterParts" &&
					field.Name != "tableList" &&
					field.Name != "loadWithBlobs" &&
					field.Name != "tableIndexHints")
				{
					Fail("Field " + field.Name +
						" probably isn't cloned properly. Please clone this field properly and suppress that property in this test case.");
				}
			}
		}

		public void TestDeepClone()
		{
			AssertEquals(false, Filter.IsNoResultQuery);
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, "teapot");
			Filter.OrderBy = "rubbish";
			Filter.IsNoResultQuery = true;

			ZQuery filter2 = Filter.DeepClone();

			AssertEquals(filter2.GetType(), Filter.GetType());
			AssertEquals(filter2.LiteralTextADO, Filter.LiteralTextADO);
			AssertEquals("rubbish", filter2.OrderBy);
			AssertEquals(true, filter2.IsNoResultQuery);
			Assert(!object.ReferenceEquals(Filter.FilterParts.filterParts, filter2.FilterParts.filterParts[0]));
		}

		public void TestShallowClone()
		{
			AssertEquals(false, Filter.IsNoResultQuery);
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, "teapot");
			Filter.OrderBy = "rubbish";
			Filter.IsNoResultQuery = true;

			ZQuery filter2 = Filter.ShallowClone();

			AssertEquals(filter2.GetType(), Filter.GetType());
			AssertEquals(filter2.LiteralTextADO, Filter.LiteralTextADO);
			AssertEquals("rubbish", filter2.OrderBy);
			AssertEquals(true, filter2.IsNoResultQuery);
		}

		public void TestClear()
		{
			AssertEquals("", Filter.LiteralTextADO);
			Filter.AddToFilter(DummyBizoSchema.Z0_Description, "teapot");
			Filter.OrderBy = "test";
			Filter.IsNoResultQuery = true;
			Assert(!string.IsNullOrEmpty(Filter.LiteralTextADO));
			AssertEquals("test", Filter.OrderBy);
			AssertEquals(true, Filter.IsNoResultQuery);
			Assert(!Filter.FilterIsEmpty);

			Filter.Clear();
			AssertEquals("", Filter.LiteralTextADO);
			AssertEquals("", Filter.OrderBy);
			AssertEquals(false, Filter.IsNoResultQuery);
			Assert(Filter.FilterIsEmpty);
		}

		public void TestAddToFilterWithValuesList()
		{
			Filter.IsDBOnlyQuery = false;
			Filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.StartsWith, new[] { "A", "B", "CD", "DE" });
			Assert("Filter should not be DB-only.", !Filter.IsDBOnlyQuery);
			AssertEquals(
				"(Z0_VarCharMax like @CWO1_ AND Z0_VarCharMax >= @CWO2_ AND Z0_VarCharMax <= @CWO3_)" +
				" or (Z0_VarCharMax like @CWO4_ AND Z0_VarCharMax >= @CWO5_ AND Z0_VarCharMax <= @CWO3_)" +
				" or (Z0_VarCharMax like @CWO7_ AND Z0_VarCharMax >= @CWO8_ AND Z0_VarCharMax <= @CWO9_)" +
				" or (Z0_VarCharMax like @CWO10_ AND Z0_VarCharMax >= @CWO11_ AND Z0_VarCharMax <= @CWO12_)",
				Filter.FilterString);

			Filter.Clear();
			Filter.IsDBOnlyQuery = false;
			Filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" });
			Assert("Filter should not be DB-only.", !Filter.IsDBOnlyQuery);
			AssertEquals(
				"Z0_VarCharMax like @CWO1_" +
				" or Z0_VarCharMax like @CWO2_" +
				" or Z0_VarCharMax like @CWO3_" +
				" or Z0_VarCharMax like @CWO4_" +
				" or Z0_VarCharMax like @CWO5_" +
				" or Z0_VarCharMax like @CWO6_" +
				" or Z0_VarCharMax like @CWO7_" +
				" or Z0_VarCharMax like @CWO8_" +
				" or Z0_VarCharMax like @CWO9_" +
				" or Z0_VarCharMax like @CWO10_" +
				" or Z0_VarCharMax like @CWO11_",
				Filter.FilterString);

			Filter.Clear();
			Filter.IsDBOnlyQuery = false;
			Filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.StartsWith, new[] { "A", "B", "CD", "DE", "EF", "FGH", "IJK", "QWER", "ABCDE", "FGHIJ" });
			Assert("Filter should be DB-only.", Filter.IsDBOnlyQuery);
			AssertEquals(
				"(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('B', 'B%'), ('CD', 'CD%'), ('DE', 'DE%'), ('EF', 'EF%'), ('FGH', 'FGH%'), ('IJK', 'IJK%'), ('QWER', 'QWER%')) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_VarCharMax LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_VarCharMax >= ConTempTable0.value AND Z0_VarCharMax <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))",
				Filter.LiteralTextADO);
			AssertEquals(
				"(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('B', 'B%'), ('CD', 'CD%'), ('DE', 'DE%'), ('EF', 'EF%'), ('FGH', 'FGH%'), ('IJK', 'IJK%'), ('QWER', 'QWER%')) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_VarCharMax LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_VarCharMax >= ConTempTable0.value AND Z0_VarCharMax <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))",
				Filter.LiteralTextADO);
		}

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var query1 = new ZQuery();
			AssertEquals(query1.HasComparisonOperatorLike, false);

			var query2 = new ZQuery();
			query2.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.NotContains, "A");
			AssertEquals(query2.HasComparisonOperatorLike, true);

			var query3 = new ZQuery();
			query3.AddToFilter(query2);
			AssertEquals(query3.HasComparisonOperatorLike, true);
		}

		#endregion
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Filter = GetNewSQLFilter();
		}

		ZQuery GetNewSQLFilter()
		{
			return new ZQuery();
		}

		ZQuery Filter;

		#endregion
	}
}
