using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class FilterBracketTest : TestCase
	{
		public void TestBlobFilters()
		{
			IFilterPart filterBracket = new FilterBracket(new IFilterPart[] { ZSqlParameter.New("@O", "X", DummyBizoSchema.Z0_VarCharMax) });
			AssertEquals(true, filterBracket.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
			filterBracket = new FilterBracket(new IFilterPart[] { ZSqlParameter.New("@O", "X", DummyBizoSchema.Z0_Code) });
			AssertEquals(false, filterBracket.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
		}

		public void TestSimplifyWithEmpty()
		{
			IFilterPart filterBracket = new FilterBracket(System.Array.Empty<IFilterPart>());
			AssertEquals(1, filterBracket.GetSimplifiedVersion(null).Length);
		}

		public void TestSimplifyWithSingleValue()
		{
			FilterStringBuilder builder = new FilterStringBuilder();
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			query.AddToFilter(new ZQuery());
			builder.Append(query);
			builder.Bracket();

			IFilterPart[] simplifiedPart = ((IFilterPart)builder).GetSimplifiedVersion(null);
			AssertEquals(typeof(FilterBracket), simplifiedPart[0].GetType());
			AssertEquals(1, query.FilterParts.Count);
		}

		public void TestSimplifyWithMultipleValues()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			query.AddToFilter(new ZQuery(DummyBizoSchema.Z0_Code, "124"));
			IFilterPart filterBracket = new FilterBracket(query);
			IFilterPart[] simplified = filterBracket.GetSimplifiedVersion(null);
			AssertEquals(1, simplified.Length);
			AssertEquals(typeof(FilterBracket), simplified[0].GetType());
			AssertEquals("Z0_Code = '123' and Z0_Code = '124'", query.LiteralTextADO);
		}

		public void TestDeepClone()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			FilterBracket fb = new FilterBracket(new IFilterPart[] { filter });
			FilterBracket clone = fb.DeepClone();
			string fbLiteralText = fb.LiteralTextADO;
			AssertEquals(fbLiteralText, clone.LiteralTextADO);
			filter.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			AssertEquals("filter should be disconnected", fbLiteralText, clone.LiteralTextADO);
		}

		public void TestIFilterPartsProvider()
		{
			ZQuery filter = new ZQuery();
			IFilterPartsProvider fb = new FilterBracket(new IFilterPart[] { filter });
			AssertEquals(filter, fb.FilterParts[0]);
		}

		public void TestConstructorAndLiteralTextADO()
		{
			ZQuery innerFilter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFilterPart filterPart = new FilterBracket(new IFilterPart[] { innerFilter });
			AssertEquals("Z0_Code = '123'", filterPart.LiteralTextADO);
		}

		public void TestParameterisedSql()
		{
			ZQuery innerFilter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFilterPart filterPart = new FilterBracket(new IFilterPart[] { innerFilter });
			AssertEquals("Z0_Code = '123'", filterPart.ParameterisedSql(new ParameterNameFactory()).LiteralTextADO);
		}

		public void TestNeedsBrackets()
		{
			ZQuery innerFilter = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFilterPart filterPart = new FilterBracket(new IFilterPart[] { innerFilter });
			Assert(filterPart.NeedsBrackets);
		}

		public void TestHasParametersEmpty()
		{
			IFilterPart filterPart = new FilterBracket(new ZQuery());
			AssertEquals(false, filterPart.HasParameters);
		}

		public void TestHasParametersWithParameters()
		{
			IFilterPart filterPart = new FilterBracket(new ZQuery(DummyBizoSchema.Z0_Code, "CODE"));
			AssertEquals(true, filterPart.HasParameters);
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			ZQuery query1 = new ZQuery();
			ZQuery query2 = new ZQuery();

			AssertNotEquals("Not equals", new FilterBracket(query1), new object());
			AssertEquals("Equals", new FilterBracket(query1), new FilterBracket(query2));

			query2.AddToFilter(DummyBizoSchema.PK, ZGuid.NewZGuid());
			AssertNotEquals("Not equals", new FilterBracket(query1), new FilterBracket(query2));
		}

		#endregion

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var filterBracket = new FilterBracket(new ZQuery());
			AssertEquals(filterBracket.HasComparisonOperatorLike, false);

			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Like, "123");
			query.AddToFilter(new ZQuery(DummyBizoSchema.Z0_Code, "124"));
			filterBracket = new FilterBracket(query);
			AssertEquals(filterBracket.HasComparisonOperatorLike, true);
		}

		#endregion
	}
}
