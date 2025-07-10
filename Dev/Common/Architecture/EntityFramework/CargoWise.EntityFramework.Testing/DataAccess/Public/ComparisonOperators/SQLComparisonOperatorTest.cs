using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class SQLComparisonOperatorTest : TestCase
	{
		public void TestIsSerializable()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(EqualComparisonOperator));
			Object newObject = null;

			using (Stream xmldata = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(xmldata, System.Text.Encoding.UTF8);
				serializer.Serialize(writer, SQLComparisonOperator.Equal);

				xmldata.Position = 0;

				XmlTextReader reader = new XmlTextReader(xmldata);
				newObject = serializer.Deserialize(reader);
			}
			AssertNotNull(newObject);
			AssertEquals(SQLComparisonOperator.Equal, newObject);
		}

		public void TestOperatorProperties()
		{
			AssertEquals(typeof(ContainsComparisonOperator), SQLComparisonOperator.Contains.GetType());
			AssertEquals(typeof(DoesNotStartWithComparisonOperator), SQLComparisonOperator.DoesNotStartWith.GetType());
			AssertEquals(typeof(DoesNotEndWithComparisonOperator), SQLComparisonOperator.DoesNotEndWith.GetType());
			AssertEquals(typeof(EndsWithComparisonOperator), SQLComparisonOperator.EndsWith.GetType());
			AssertEquals(typeof(EqualComparisonOperator), SQLComparisonOperator.Equal.GetType());
			AssertEquals(typeof(EqualToDatePartOnlyComparisonOperator), SQLComparisonOperator.EqualToDatePartOnly.GetType());
			AssertEquals(typeof(GreaterThanComparisonOperator), SQLComparisonOperator.GreaterThan.GetType());
			AssertEquals(typeof(GreaterThanOrEqualToComparisonOperator), SQLComparisonOperator.GreaterThanOrEqualTo.GetType());
			AssertEquals(typeof(GreaterThanOrEqualToDatePartComparisonOperator), SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly.GetType());

			AssertEquals(typeof(LessThanComparisonOperator), SQLComparisonOperator.LessThan.GetType());
			AssertEquals(typeof(LessThanOrEqualToComparisonOperator), SQLComparisonOperator.LessThanOrEqualTo.GetType());
			AssertEquals(typeof(LessThanOrEqualToDatePartComparisonOperator), SQLComparisonOperator.LessThanOrEqualToDatePartOnly.GetType());
			AssertEquals(typeof(NotContainsComparisonOperator), SQLComparisonOperator.NotContains.GetType());

			AssertEquals(typeof(NotEqualComparisonOperator), SQLComparisonOperator.NotEqual.GetType());
			AssertEquals(typeof(NotSpecifiedComparisonOperator), SQLComparisonOperator.NotSpecified.GetType());
			AssertEquals(typeof(StartsWithComparisonOperator), SQLComparisonOperator.StartsWith.GetType());
		}

		public void TestGetHashCode()
		{
			AssertEquals(new EqualComparisonOperator().GetHashCode(), new EqualComparisonOperator().GetHashCode());
		}

		public void TestEqualsOverload()
		{
			Assert(new EqualComparisonOperator().Equals(new EqualComparisonOperator()));
		}

		public void TestOperatorOverloadEquals()
		{
			EqualComparisonOperator equalsOp = new EqualComparisonOperator();
			NotEqualComparisonOperator notOp = new NotEqualComparisonOperator();

#pragma warning disable 1718
			Assert(equalsOp == equalsOp);
			Assert(!(equalsOp == null));
			Assert(!(null == equalsOp));
			Assert(!(notOp == equalsOp));
#pragma warning restore 1718
		}

		public void TestOperatorOverloadNotEquals()
		{
			EqualComparisonOperator equalsOp = new EqualComparisonOperator();
			NotEqualComparisonOperator notOp = new NotEqualComparisonOperator();

#pragma warning disable 1718
			Assert(!(equalsOp != equalsOp));
#pragma warning restore 1718
			Assert(equalsOp != null);
			Assert(null != equalsOp);
			Assert(notOp != equalsOp);
		}

		public void TestValueIsNullWithEmptyZDateTime()
		{
			AssertEquals(true, SQLComparisonOperator.ValueIsNull(ZDateTime.Empty));
			AssertEquals(false, SQLComparisonOperator.ValueIsNull(ZDateTime.Now));
			AssertEquals(true, SQLComparisonOperator.ValueIsNull(ZDateTimeOffset.Empty));
			AssertEquals(false, SQLComparisonOperator.ValueIsNull(ZDateTimeOffset.Now));
			AssertEquals(true, SQLComparisonOperator.ValueIsNull(ZDate.Empty));
			AssertEquals(false, SQLComparisonOperator.ValueIsNull(ZDate.Today));
			AssertEquals(true, SQLComparisonOperator.ValueIsNull(ZTime.Empty));
			AssertEquals(false, SQLComparisonOperator.ValueIsNull(new ZTime(1, 2)));
		}

		public void TestEquals()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "ABC");
			AssertEquals("Z0_Code = 'ABC'", filter.LiteralTextADO);
		}

		public void TestEqualsForNull()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, null);
			AssertEquals("Z0_Date is null", filter.LiteralTextADO);
		}

		public void TestEqualsForNullDateOnly()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_DateOnly, SQLComparisonOperator.Equal, null);
			AssertEquals("Z0_DateOnly is null", filter.LiteralTextADO);
		}

		public void TestStaticNotEqual()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "ABC");
			AssertEquals("Z0_Code <> 'ABC'", filter.LiteralTextADO);
		}

		public void TestStaticNotEqualForNull()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, null);
			AssertEquals("Z0_Code is not null", filter.LiteralTextADO);
		}

		public void TestStaticGreaterThan()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThan, "ABC");
			AssertEquals("Z0_Code > 'ABC'", filter.LiteralTextADO);
		}

		public void TestStaticLessThan()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.LessThan, "ABC");
			AssertEquals("Z0_Code < 'ABC'", filter.LiteralTextADO);
		}

		public void TestStaticGreaterThanOrEqualTo()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThanOrEqualTo, "ABC");
			AssertEquals("Z0_Code >= 'ABC'", filter.LiteralTextADO);
		}

		public void TestStaticLessThanOrEqualTo()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.LessThanOrEqualTo, "ABC");
			AssertEquals("Z0_Code <= 'ABC'", filter.LiteralTextADO);
		}

		public void TestStaticEqualToDatePartOnly_ForDateTimeOffset()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTimeOffset(1971, 9, 18, 2, 3, 4, TimeSpan.FromHours(11)));
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #1971-09-18 00:00:00.0000000 +11:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #1971-09-19 00:00:00.0000000 +11:00#", filter.LiteralTextADO);
		}

		public void TestStaticLessThanOrEqualToDatePartOnly_ForDateTimeOffset()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(11)));
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #1971-09-19 00:00:00.0000000 +11:00#", filter.LiteralTextADO);
		}

		public void TestStaticGreaterThanOrEqualToDatePartOnly_ForDateTimeOffset()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(11)));
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #1971-09-18 00:00:00.0000000 +11:00#", filter.LiteralTextADO);
		}

		public void TestStaticEqualToDatePartOnly()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTime(1971, 9, 18, 2, 3, 4));
			AssertEquals("Z0_Date >= #1971-09-18 00:00:00.000# and Z0_Date < #1971-09-19 00:00:00.000#", filter.LiteralTextADO);
		}

		public void TestStaticLessThanOrEqualToDatePartOnly()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDateTime(1971, 9, 18));
			AssertEquals("Z0_Date < #1971-09-19 00:00:00.000#", filter.LiteralTextADO);
		}

		public void TestStaticGreaterThanOrEqualToDatePartOnly()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, new ZDateTime(1971, 9, 18));
			AssertEquals("Z0_Date >= #1971-09-18 00:00:00.000#", filter.LiteralTextADO);
		}

		public void TestStaticStartsWith()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "ABC");
			AssertEquals("Z0_Code like 'ABC%'", filter.LiteralTextADO);
			AssertEquals("ABC", filter.Params[0].Value);
		}

		public void TestStaticEndsWith()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, "ABC");
			AssertEquals("Z0_Code like '%ABC'", filter.LiteralTextADO);
			AssertEquals("ABC", filter.Params[0].Value);
		}

		public void TestStaticContains()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains, "ABC");
			AssertEquals("Z0_Code like '%ABC%'", filter.LiteralTextADO);
		}

		public void TestStaticNotSpecified()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotSpecified, "ABC");
			AssertEquals("", filter.LiteralTextADO);
		}

		public void TestStaticNotContains()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotContains, "ABC");
			AssertEquals("Z0_Code not like '%ABC%'", filter.LiteralTextADO);
		}

		public void TestStaticDoesNotStartWith()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.DoesNotStartWith, "ABC");
			AssertEquals("Z0_Code not like 'ABC%'", filter.LiteralTextADO);
		}

		public void TestStaticDoesNotEndWith()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.DoesNotEndWith, "ABC");
			AssertEquals("Z0_Code not like '%ABC'", filter.LiteralTextADO);
		}

		public void TestReverseOperatorIfItIsNotInOperator()
		{
			var equalOperator = SQLComparisonOperator.Equal;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref equalOperator));
			AssertEquals(SQLComparisonOperator.Equal, equalOperator);

			var greaterThanOperator = SQLComparisonOperator.GreaterThan;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref greaterThanOperator));
			AssertEquals(SQLComparisonOperator.GreaterThan, greaterThanOperator);

			var lessThanOperator = SQLComparisonOperator.LessThan;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref lessThanOperator));
			AssertEquals(SQLComparisonOperator.LessThan, lessThanOperator);

			var greaterThanOrEqualToOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref greaterThanOrEqualToOperator));
			AssertEquals(SQLComparisonOperator.GreaterThanOrEqualTo, greaterThanOrEqualToOperator);

			var lessThanOrEqualToOperator = SQLComparisonOperator.LessThanOrEqualTo;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref lessThanOrEqualToOperator));
			AssertEquals(SQLComparisonOperator.LessThanOrEqualTo, lessThanOrEqualToOperator);

			var equalToDatePartOnlyOperator = SQLComparisonOperator.EqualToDatePartOnly;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref equalToDatePartOnlyOperator));
			AssertEquals(SQLComparisonOperator.EqualToDatePartOnly, equalToDatePartOnlyOperator);

			var lessThanOrEqualToDatePartOnlyOperator = SQLComparisonOperator.LessThanOrEqualToDatePartOnly;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref lessThanOrEqualToDatePartOnlyOperator));
			AssertEquals(SQLComparisonOperator.LessThanOrEqualToDatePartOnly, lessThanOrEqualToDatePartOnlyOperator);

			var greaterThanOrEqualToDatePartOnlyOperator = SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref greaterThanOrEqualToDatePartOnlyOperator));
			AssertEquals(SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, greaterThanOrEqualToDatePartOnlyOperator);

			var startsWithOperator = SQLComparisonOperator.StartsWith;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref startsWithOperator));
			AssertEquals(SQLComparisonOperator.StartsWith, startsWithOperator);

			var endsWithOperator = SQLComparisonOperator.EndsWith;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref endsWithOperator));
			AssertEquals(SQLComparisonOperator.EndsWith, endsWithOperator);

			var containsOperator = SQLComparisonOperator.Contains;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref containsOperator));
			AssertEquals(SQLComparisonOperator.Contains, containsOperator);

			var likeOperator = SQLComparisonOperator.Like;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref likeOperator));
			AssertEquals(SQLComparisonOperator.Like, likeOperator);

			var notSpecifiedOperator = SQLComparisonOperator.NotSpecified;
			Assert(!SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref notSpecifiedOperator));
			AssertEquals(SQLComparisonOperator.NotSpecified, notSpecifiedOperator);

			var notContainsOperator = SQLComparisonOperator.NotContains;
			Assert(SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref notContainsOperator));
			AssertEquals(SQLComparisonOperator.Contains, notContainsOperator);

			var doesNotStartWithOperator = SQLComparisonOperator.DoesNotStartWith;
			Assert(SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref doesNotStartWithOperator));
			AssertEquals(SQLComparisonOperator.StartsWith, doesNotStartWithOperator);

			var doesNotEndWithOperator = SQLComparisonOperator.DoesNotEndWith;
			Assert(SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref doesNotEndWithOperator));
			AssertEquals(SQLComparisonOperator.EndsWith, doesNotEndWithOperator);

			var notEqualOperator = SQLComparisonOperator.NotEqual;
			Assert(SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref notEqualOperator));
			AssertEquals(SQLComparisonOperator.Equal, notEqualOperator);
		}
	}
}
