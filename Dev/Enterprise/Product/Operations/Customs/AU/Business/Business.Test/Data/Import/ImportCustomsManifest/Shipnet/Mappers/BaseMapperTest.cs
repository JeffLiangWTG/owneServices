using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class BaseMapperTest : TestCase
	{
		public void TestIsRowType()
		{
			FlatFileDataRow row = new FlatFileDataRow(1);
			row[0] = "FOO";
			TestHelper mapper = new TestHelper();
			AssertEquals(true, mapper.IsRowType(row, "FOO"));
			AssertEquals(false, mapper.IsRowType(row, "BAR"));
		}

		sealed class TestHelper : BaseMapper
		{
			public override void Map(FlatFileDataRowCollection rows, IValueObject value)
			{
			}
		}
	}
}
