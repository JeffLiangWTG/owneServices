using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class FilterStringBuilderTest : TestCase
	{
		public void TestDeepClone()
		{
			ZQuery query = new ZQuery();
			FilterStringBuilder builder = new FilterStringBuilder();
			builder.Append(query);
			FilterStringBuilder clonedBuilder = builder.DeepClone();
			Assert(builder != clonedBuilder);
			Assert(builder.filterParts[0] != clonedBuilder.filterParts[0]);
		}

		public void TestShallowClone()
		{
			ZQuery query = new ZQuery();
			FilterStringBuilder builder = new FilterStringBuilder();
			builder.Append(query);
			FilterStringBuilder clonedBuilder = builder.ShallowClone();
			Assert(builder != clonedBuilder);
			AssertEquals(builder.filterParts[0], clonedBuilder.filterParts[0]);
		}

		public void TestSimplifyRemovesNestedEmptyQuery()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery nestedNestedQuery = new ZQuery();
			ZQuery nestedQuery = new ZQuery();
			nestedQuery.AddToFilter(nestedNestedQuery);
			builder.Append(nestedQuery);
			AssertEquals("Precondition", 1, builder.filterParts.Count);
			builder.Simplify();
			AssertEquals(0, builder.filterParts.Count);
		}

		public void TestSimplifyMovesQueryThroughNestedEmptyQuery()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery nestedNestedQuery = new ZQuery();
			nestedNestedQuery.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			ZQuery nestedQuery = new ZQuery();
			nestedQuery.AddToFilter(nestedNestedQuery);
			builder.Append(nestedQuery);
			AssertEquals("Precondition", 1, builder.filterParts.Count);
			builder.Simplify();
			AssertEquals(1, builder.filterParts.Count);
			AssertEquals(builder.filterParts[0].GetType(), typeof(ZSqlParameter));
		}

		public void TestSimplifyMovesQueryThroughDoubleNestedEmptyQuery()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery nestedNestedNestedQuery = new ZQuery();
			nestedNestedNestedQuery.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			ZQuery nestedNestedQuery = new ZQuery();
			nestedNestedQuery.AddToFilter(nestedNestedNestedQuery);
			ZQuery nestedQuery = new ZQuery();
			nestedQuery.AddToFilter(nestedNestedQuery);
			builder.Append(nestedQuery);
			builder.Simplify();
			AssertEquals(1, builder.filterParts.Count);
			AssertEquals(builder.filterParts[0].GetType(), typeof(ZSqlParameter));
		}

		public void TestSimplifyLeavesNestedUsefulQuery()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery nestedQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			builder.Append(nestedQuery);
			AssertEquals("Precondition", 1, builder.filterParts.Count);
			builder.Simplify();
			AssertEquals(1, builder.filterParts.Count);
		}

		public void TestSimplifyRemovesPrecedingJoinCondition()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			builder.Append(ZSqlParameter.New("@P1", "123", DummyBizoSchema.Z0_Code));
			builder.Append(JoinCondition.Or, new ZQuery());
			AssertEquals("Precondition", 3, builder.Count);
			builder.Simplify();
			AssertEquals(1, builder.Count);
		}

		public void TestSimplifySimplifiesMultipleLevels()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery nestedQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery nestedNestedQuery = new ZQuery();
			nestedQuery.AddToFilter(nestedNestedQuery);
			builder.Append(nestedQuery);
			AssertEquals("Precondition", 3, nestedQuery.FilterParts.Count);
			builder.Simplify();
			AssertEquals(1, nestedQuery.FilterParts.Count);
		}

		public void TestGetOrPartsWithSingleOr()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CODE"));
			filter.Append(JoinCondition.Or);
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CODE2"));
			ZQuery[] compositeParts = filter.GetOrParts();
			AssertEquals(2, compositeParts.Length);
			AssertEquals("Z0_Code = 'CODE'", compositeParts[0].LiteralTextADO);
			AssertEquals("Z0_Code = 'CODE2'", compositeParts[1].LiteralTextADO);
		}

		public void TestGetOrPartsWithSingleOrAndInternalAnds()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			ZQuery q1 = NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CODE");
			q1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, 1);
			filter.Append(q1);
			filter.Append(JoinCondition.Or);
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CODE2"));
			ZQuery[] compositeParts = filter.GetOrParts();
			AssertEquals(2, compositeParts.Length);
			AssertEquals("Z0_Code = 'CODE' and Z0_Number = 1", compositeParts[0].LiteralTextADO);
			AssertEquals("Z0_Code = 'CODE2'", compositeParts[1].LiteralTextADO);
		}

		public void TestGetOrPartsWithAllOrs()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1"));
			filter.Append(JoinCondition.Or);
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "2"));
			ZQuery[] compositeParts = filter.GetOrParts();
			AssertEquals(2, compositeParts.Length);
			AssertEquals("Z0_Code = '1'", compositeParts[0].LiteralTextADO);
			AssertEquals("Z0_Code = '2'", compositeParts[1].LiteralTextADO);
		}

		public void TestGetOrPartsWithMixedOrAndAnds()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1"));
			filter.Append(JoinCondition.And);
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "2"));
			ZQuery[] compositeParts = filter.GetOrParts();
			AssertEquals(0, compositeParts.Length);
		}

		public void TestBracketsOrWithNestedQuery()
		{
			ZQuery innerQuery = new ZQuery();
			innerQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
			ZQuery middleQuery = new ZQuery(innerQuery);
			middleQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "2");
			ZQuery middle2Query = new ZQuery(middleQuery.ShallowClone());
			ZQuery middle3Query = new ZQuery(middle2Query);
			ZQuery outerQuery = new ZQuery(middle2Query);

			ZQuery fullQuery = new ZQuery();
			fullQuery.AddToFilter(DummyBizoSchema.Z0_Code, "3");
			fullQuery.AddToFilter(outerQuery, JoinCondition.And);
			AssertEquals("Z0_Code = '3' and (Z0_Code = '1' or Z0_Code = '2')", fullQuery.LiteralTextADO);
		}

		public void TestGetCompositePartsWhenEmptyFilter()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			AssertEquals(0, filter.GetCompositeParts().Length);
		}

		public void TestGetCompositePartsWhenSingleFilter()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CODE"));
			ZQuery[] compositeParts = filter.GetCompositeParts();
			AssertEquals(1, compositeParts.Length);
			AssertEquals("Z0_Code = 'CODE'", compositeParts[0].LiteralTextADO);
		}

		public void TestGetFirstSingleEqualParameterForNestedQueries()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Bool, true);
			List<string> list = new List<string>();
			list.Add("A");
			list.Add("B");
			list.Add("C");
			list.Add("D");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, list);
			AssertNull(new ZQuery(query).GetMostUniqueSingleEqualParameter());
		}

		public void TestGetFirstSingleEqualParameterForMultipleThenSingle()
		{
			ZQuery firstquery = new ZQuery();
			firstquery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "A");
			firstquery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "B");
			ZQuery query = new ZQuery();
			query.AddToFilter(firstquery);
			query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal, true);
			ZSqlParameter parameter = query.GetMostUniqueSingleEqualParameter();
			AssertNotNull(parameter);
			AssertEquals(true, parameter.Value);
		}

		public void TestHasParametersEmpty()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			AssertEquals(false, filter.HasParameters);
		}

		public void TestHasParametersWithParameters()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(ZSqlParameter.New("@P1", "Code", DummyBizoSchema.Z0_Code));
			AssertEquals(true, filter.HasParameters);
		}

		public void TestLeadingJoinConditionIsIgnored()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			AssertEquals("Precondition", 0, filter.Count);
			filter.Append(JoinCondition.Or);
			AssertEquals("Adding JoinCondition when filter is empty", 0, filter.Count);
			filter.Append(new ZQuery());
			AssertEquals("Precondition", 1, filter.Count);
			filter.Append(JoinCondition.Or);
			AssertEquals("Adding JoinCondition when filter is not empty", 2, filter.Count);
		}

		public void TestCount()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			AssertEquals(0, filter.Count);
			filter.Append(new ZQuery());
			AssertEquals(1, filter.Count);
			filter.Reset();
			AssertEquals(0, filter.Count);
		}

		public void TestGetFilterPart()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			IFilterPart filterPart = new ZQuery();
			filter.Append(filterPart);
			AssertEquals(filterPart, filter.GetFilterPart(0));
		}

		public void TestGetCompositePartsForNestedOrQuery()
		{
			ZQuery queryInternal = new ZQuery();
			queryInternal.AddToFilter(JobComInvHeaderChargeSchema.J7_ParentID, ZGuid.NewZGuid());
			queryInternal.AddToFilter(JoinCondition.Or, JobComInvHeaderChargeSchema.J7_IsApportionedCharge, SQLComparisonOperator.Equal, "N");
			ZQuery queryExternal = new ZQuery();
			queryExternal.AddToFilter(queryInternal);
			ZQuery[] compositeParts = queryExternal.GetCompositeParts();
			AssertEquals(1, compositeParts.Length);
		}

		public void TestGetCompositePartsForMultipleOrQueriesThatHaveNoCompositePartsRecursively()
		{
			ZQuery mainQuery = new ZQuery();

			ZQuery query1 = new ZQuery();
			query1.AddToFilter(JoinCondition.And, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "NOOSL");
			query1.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, new object[] { "SCAN", "EURO" });
			query1.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, new object[] { "SCAN", "EURO" });
			query1.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "NO");

			mainQuery.AddToFilter(query1, JoinCondition.And);
			ZQuery query2 = new ZQuery();
			query2.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "AUSYD");
			query2.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, new object[] { "AUEC" });
			query2.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, new object[] { "AUEC" });
			query2.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "AU");
			mainQuery.AddToFilter(query2, JoinCondition.Or);

			ZQuery query3 = new ZQuery();
			query3.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "");
			query3.AddToFilter(JoinCondition.And, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "");
			query3.AddToFilter(JoinCondition.And, RateEntrySchema.TI_IsCrossTrade, SQLComparisonOperator.Equal, false);
			mainQuery.AddToFilter(query3, JoinCondition.Or);

			ZQuery[] compositeParts = mainQuery.GetCompositeParts();
			AssertEquals("No parts returned as ors existed", 0, compositeParts.Length);
		}

		public void TestGetCompositePartsForNestedAndQuery()
		{
			ZGuid parentID = ZGuid.NewZGuid();
			ZQuery queryInternal = new ZQuery();
			queryInternal.AddToFilter(JobComInvHeaderChargeSchema.J7_ParentID, parentID);
			queryInternal.AddToFilter(JobComInvHeaderChargeSchema.J7_IsApportionedCharge, "N");
			queryInternal.AddToFilter(JobComInvHeaderChargeSchema.J7_IsApportionedCharge, "N");
			ZQuery queryExternal = new ZQuery();
			queryExternal.AddToFilter(queryInternal);
			AssertEquals("J7_ParentID = CONVERT('" + parentID.ToString() + "', 'System.Guid') and J7_IsApportionedCharge = 0 and J7_IsApportionedCharge = 0", queryExternal.LiteralTextADO);
			ZQuery[] compositeParts = queryExternal.GetCompositeParts();
			AssertEquals(3, compositeParts.Length);
			AssertEquals("J7_ParentID = CONVERT('" + parentID.ToString() + "', 'System.Guid')", compositeParts[0].LiteralTextADO);
			AssertEquals("J7_IsApportionedCharge = 0", compositeParts[1].LiteralTextADO);
			AssertEquals("J7_IsApportionedCharge = 0", compositeParts[2].LiteralTextADO);
		}

		public void TestGetCompositePartsWhenMultipleFilterWithAnd()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "A"));
			filter.Append(JoinCondition.And, NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "B"));
			ZQuery[] compositeParts = filter.GetCompositeParts();
			AssertEquals(2, compositeParts.Length);
			AssertEquals("Z0_Code = 'A'", compositeParts[0].LiteralTextADO);
			AssertEquals("Z0_Code = 'B'", compositeParts[1].LiteralTextADO);
		}

		public void TestGetCompositePartsWhenMultipleFilterWithOr()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "A"));
			filter.Append(JoinCondition.Or, NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "B"));
			ZQuery[] compositeParts = filter.GetCompositeParts();
			AssertEquals(0, compositeParts.Length);
		}

		public void TestGetCompositePartsWhenSubquery()
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "XXX");
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Number, 2);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

			ZQuery[] compositeParts = dbOnlyQuery.GetCompositeParts();
			AssertEquals(1, compositeParts.Length);
		}

		public void TestGetCompositePartsWhenMultipleFilterWithMixedAndOr()
		{
			FilterStringBuilder filter = new FilterStringBuilder();
			filter.Append(NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "A"));
			filter.Append(JoinCondition.And, NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "B"));
			filter.Append(JoinCondition.Or, NewFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "C"));
			ZQuery[] compositeParts = filter.GetCompositeParts();
			AssertEquals(0, compositeParts.Length);
		}

		public void TestContainsNonBracketedOr()
		{
			FilterStringBuilder testBuilder = new FilterStringBuilder();
			AssertEquals("Should be initialised as FALSE", false, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (1)", "", testBuilder.ToString());

			testBuilder.Append(JoinCondition.And, ZSqlParameter.New("@P7", 7, DummyBizoSchema.Z0_Number));
			AssertEquals("No OR operator added - Should be FALSE", false, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (2)", "Z0_Number = 7", testBuilder.ToString());

			testBuilder.Append(JoinCondition.Or, ZSqlParameter.New("@P8", 8, DummyBizoSchema.Z0_Number));
			AssertEquals("OR operator added - Should be TRUE", true, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (3)", "Z0_Number = 7 or Z0_Number = 8", testBuilder.ToString());

			testBuilder.Append(JoinCondition.Or, ZSqlParameter.New("@P9", 9, DummyBizoSchema.Z0_Number));
			AssertEquals("Another OR operator added - Should be TRUE", true, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (4)", "Z0_Number = 7 or Z0_Number = 8 or Z0_Number = 9", testBuilder.ToString());

			testBuilder.Bracket();
			AssertEquals("Brackets added - Should be FALSE", false, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (5)", "Z0_Number = 7 or Z0_Number = 8 or Z0_Number = 9", testBuilder.ToString());

			testBuilder.Append(JoinCondition.Or, ZSqlParameter.New("@PA", "A", DummyBizoSchema.Z0_Code));
			AssertEquals("One more OR operator added - Should be TRUE", true, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (6)", "(Z0_Number = 7 or Z0_Number = 8 or Z0_Number = 9) or Z0_Code = 'A'", testBuilder.ToString());

			testBuilder.Reset();
			AssertEquals("Filter cleared - Should be FALSE", false, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (7)", "", testBuilder.ToString());

			testBuilder.Append(JoinCondition.And, new ZNonPersistentDataQuery("(Code = 'A' or Code = 'B')"));
			AssertEquals("OR operator added - Should be TRUE", true, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (8)", "(Code = 'A' or Code = 'B')", testBuilder.ToString());

			testBuilder.Append(JoinCondition.And, new ZNonPersistentDataQuery("Number = 1 or Number = 2"));
			AssertEquals("Non-bracketed OR operator added - Should be TRUE", true, testBuilder.ContainsNonBracketedOr);
			AssertEquals("Filter String (9)", "((Code = 'A' or Code = 'B')) and (Number = 1 or Number = 2)", testBuilder.ToString());
		}

		public void TestNewFieldsAsClassesAreTracked()
		{
			Assert(true);
			foreach (FieldInfo field in typeof(FilterStringBuilder).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.FieldType != typeof(string) &&
						field.FieldType.IsClass &&
						field.Name != "filterParts" && field.Name != "compositePartsCache")
				{
					Fail("Field " + field.Name + " probably isn't cloned properly. Please clone this field properly and suppress that property in this test case.");
				}
			}
		}

		public void TestAppendFilterPart()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			filterStringBuilder.Append(new ZQuery(DummyBizoSchema.Z0_Code, "Hi"));
			AssertEquals("Z0_Code = 'Hi'", filterStringBuilder.LiteralTextADO);
		}

		public void TestEnumerationWithNoElements()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			IEnumerator enumerator = ((IEnumerable)filterStringBuilder).GetEnumerator();
			AssertEquals(false, enumerator.MoveNext());
		}

		public void TestEnumerationWithThreeElements()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Code, "Hi");
			filterStringBuilder.Append(filter1);
			filterStringBuilder.Append(JoinCondition.Or);
			ZQuery filter2 = new ZQuery(DummyBizoSchema.Z0_Code, "Bye");
			filterStringBuilder.Append(filter2);
			int i = 1;
			foreach (IFilterPart filterPart in filterStringBuilder)
			{
				switch (i)
				{
					case 1:
						AssertEquals(filter1, filterPart);
						break;
					case 2:
						break;
					case 3:
						AssertEquals(filter2, filterPart);
						break;
					default:
						Fail("Too many values");
						break;
				}
				i++;
			}
		}

		public void TestContainsOrOperator()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			filterStringBuilder.Append(filter);
			AssertEquals(false, filterStringBuilder.ContainsOrOperator);
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "124");
			AssertEquals(true, filterStringBuilder.ContainsOrOperator);
		}

		public void TestCompositePartsWithNestedOr()
		{
			ZQuery query1 = new ZQuery();
			query1.MaximumRows = 1;
			query1.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_Code, SQLComparisonOperator.Equal, "FRT");
			ZQuery query2 = new ZQuery();
			ZQuery filterQuery3 = new ZQuery();
			ZQuery query4 = new ZQuery();
			query4.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			ZQuery query5 = new ZQuery();
			query5.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_GC, SQLComparisonOperator.Equal, null);
			query5.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, SQLComparisonOperator.Equal, new Guid("b651eae1-d86c-4715-b0e5-a748def329df"));
			query4.AddToFilter(query5, JoinCondition.And);
			filterQuery3.AddToFilter(query4, JoinCondition.And);

			FilterBracket filterBracket6 = new FilterBracket(filterQuery3);
			query2.AddToFilterForTesting(JoinCondition.And, filterBracket6);
			ZQuery query7 = new ZQuery();
			ZQuery query8 = new ZQuery();
			ZQuery filterQuery9 = new ZQuery();
			ZQuery queryPAndL = new ZQuery();
			queryPAndL.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "P&L");
			ZQuery query11 = new ZQuery();
			query11.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "BSH");
			query11.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsControlAccount, SQLComparisonOperator.Equal, "N");
			queryPAndL.AddToFilter(query11, JoinCondition.Or);
			filterQuery9.AddToFilter(queryPAndL, JoinCondition.And);
			ZQuery query12 = new ZQuery();
			query12.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "CMT");
			query12.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "MRG");
			query12.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "DSB");
			query12.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "NON");
			query12.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, "OVR");
			filterQuery9.AddToFilter(query12, JoinCondition.Or);

			FilterBracket filterBracket13 = new FilterBracket(filterQuery9);
			query8.AddToFilterForTesting(JoinCondition.And, filterBracket13);
			query8.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_DisallowDirectPosting, SQLComparisonOperator.Equal, false);
			query7.AddToFilter(query8, JoinCondition.And);
			query2.AddToFilter(query7, JoinCondition.And);
			query1.AddToFilter(query2, JoinCondition.And);

			ZQuery[] compositeParts = query1.GetCompositeParts();
			AssertEquals("Composite part count", 4, compositeParts.Length);
		}

		public void TestEquals()
		{
			ZSqlParameter param1 = ZSqlParameter.New("@ParamName1", "123", DummyBizoSchema.Z0_Code);
			ZSqlParameter param2 = ZSqlParameter.New("@ParamName1", "123", DummyBizoSchema.Z0_Description);
			ZSqlParameter param3 = ZSqlParameter.New("@ParamName2", "123", DummyBizoSchema.Z0_Code);

			FilterStringBuilder filters1 = new FilterStringBuilder();
			filters1.Append(param1);
			FilterStringBuilder filters2 = new FilterStringBuilder();
			filters2.Append(param2);
			AssertNotEquals(filters1, filters2);

			filters2.Reset();
			filters2.Append(param3);
			AssertEquals(filters1, filters2);

			ZSQLColumnComparer columnComparer = new ZSQLColumnComparer(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, DummyBizoSchema.Z0_AnotherNumber);
			filters1.Append(columnComparer);
			AssertNotEquals(filters1, filters2);

			filters2.Append(columnComparer);
			AssertEquals(filters1, filters2);
		}

		ZQuery NewFilter(SchemaColumn column, SQLComparisonOperator @operator, object code)
		{
			return new ZQuery(column, @operator, code);
		}

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var filterStringBuilder = new FilterStringBuilder();
			AssertEquals(filterStringBuilder.HasComparisonOperatorLike, false);

			var param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.NotContains);
			AssertEquals(param.HasComparisonOperatorLike, true);

			filterStringBuilder = new FilterStringBuilder();
			filterStringBuilder.Append(param);
			AssertEquals(filterStringBuilder.HasComparisonOperatorLike, true);
		}

		#endregion
	}
}
