using System;
using System.Collections.Generic;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests
{
	public class GlowIndexQueryParamTest : TestCase
	{
		public void TestEmptyUri()
		{
			var emptyQuery = new EmptyQuery();
			var param = new GlowIndexQueryParam(new List<IGlowQuery>() { emptyQuery }, "IOrgHeader");
			AssertEquals("odata/Index/EntityInfos?$top=50&$filter=EntityType eq 'IOrgHeader'", param.GetQueryUri(50));
		}

		public void TestUriQueryParam()
		{
			var equalQuery = new EqualQuery(new Term("Code", "HAY"));
			var param = new GlowIndexQueryParam(new List<IGlowQuery>() { equalQuery }, "IOrgHeader");
			AssertEquals("odata/Index/EntityInfos?$top=50&$filter=(Code eq 'HAY') and EntityType eq 'IOrgHeader'", param.GetQueryUri(50));
		}

		public void TestIncludeCount()
		{
			var equalQuery = new EqualQuery(new Term("Code", "HAY"));
			var param = new GlowIndexQueryParam(new List<IGlowQuery>() { equalQuery }, "IOrgHeader", 60, includeCount: true);
			AssertContains("&$count=true", param.GetQueryUri(0));
		}

		public void TestCountExceed()
		{
			var equalQuery = new EqualQuery(new Term("Code", "HAY"));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new GlowIndexQueryParam(new List<IGlowQuery>() { equalQuery }, "IOrgHeader", 1001, includeCount: true));
		}

		public void TestEmptyQuery()
		{
			var equalQuery = new EmptyQuery();
			var param = new GlowIndexQueryParam(new List<IGlowQuery>() { equalQuery }, "IOrgHeader");
			AssertEquals("odata/Index/EntityInfos?$top=50&$filter=EntityType eq 'IOrgHeader'", param.GetQueryUri(50));
		}

		public void TestMultipleEmptyQuery()
		{
			var emptyTerm = new Term("Code", "");
			var booleanQuery = new BooleanQuery(BooleanOperator.Or, new EmptyQuery(), new EqualQuery(emptyTerm));
			var rangeQuery = new RangeQuery(new Term("Age", null), new Term("Age", null));
			var param = new GlowIndexQueryParam(new List<IGlowQuery> { booleanQuery, rangeQuery }, "IOrgHeader");
			AssertEquals("odata/Index/EntityInfos?$top=50&$filter=EntityType eq 'IOrgHeader'", param.GetQueryUri(50));
		}
	}
}
