using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseDataRowTest : TestCase
	{
		public void TestParseRecordID()
		{
			var dataRow = GetNewDataRow("foo");
			AssertEquals("foo", dataRow[0]);
		}

		public void TestParse()
		{
			TestParseCore();
		}

		public void TestGetFieldAsUInt()
		{
			AssertEquals(17, GetNewDataRow("17").GetFieldAsUInt(0));
			AssertEquals(0, GetNewDataRow("-17").GetFieldAsUInt(0));
			AssertEquals(0, GetNewDataRow("foo").GetFieldAsUInt(0));
		}

		public void TestTrimming()
		{
			AssertEquals("A", GetNewDataRow(" A ")[0]);
		}

		protected abstract void TestParseCore();

		protected override void SetUp()
		{
			base.SetUp();
			dataRow = GetNewDataRow(RawRow);
		}

		protected abstract BaseDataRow GetNewDataRow(ZString rawRow);

		protected abstract ZString RawRow { get; }

		protected BaseDataRow dataRow;
	}
}
