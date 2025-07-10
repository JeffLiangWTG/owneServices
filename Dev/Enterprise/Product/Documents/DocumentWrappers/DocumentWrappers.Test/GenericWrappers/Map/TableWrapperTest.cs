using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(TableWrapper))]
	sealed class TableWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			TableWrapper wrapperEmpty = new TableWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.LeftColumnText", ZString.Empty, wrapperEmpty.LeftColumnText);
			AssertEquals("wrapperEmpty.LeftColumnTitle", ZString.Empty, wrapperEmpty.LeftColumnTitle);
			AssertEquals("wrapperEmpty.RightColumnText", ZString.Empty, wrapperEmpty.RightColumnText);
			AssertEquals("wrapperEmpty.RightColumnTitle", ZString.Empty, wrapperEmpty.RightColumnTitle);
			AssertEquals("wrapperEmpty.TitleText", ZString.Empty, wrapperEmpty.TitleText);
			AssertEquals("wrapperEmpty.TitleSupplement", ZString.Empty, wrapperEmpty.TitleSupplement);
			AssertEquals("wrapperEmpty.DataSourceType", "Generic", wrapperEmpty.DataSourceType);
		}

		public void TestWrapperMappingsFull()
		{
			MapTable mapTable = new MapTable("Roitter", "Tickers", "Fred", 30, "Flintstone", 25, true);
			mapTable.AddLine("4", "Barney", "Rubble");
			mapTable.AddLine("1", "Hot", "Nuts");
			mapTable.AddLine("3", "", "");
			mapTable.AddLine("2", "Another", "Toblerone");

			TableWrapper wrapper = new TableWrapper(mapTable, Factory);
			AssertEquals("wrapper.ToString()", "Roitter", wrapper.ToString());
			AssertEquals("wrapper.TitleText", "Roitter", wrapper.TitleText);
			AssertEquals("wrapper.TitleSupplement", "(Default Field: Tickers)", wrapper.TitleSupplement);
			AssertEquals("wrapper.LeftColumnTitle", "Fred", wrapper.LeftColumnTitle);
			AssertEquals("wrapper.RightColumnTitle", "Flintstone", wrapper.RightColumnTitle);
			AssertEquals("wrapper.DataSourceType", "NonGeneric", wrapper.DataSourceType);

			AssertEquals("wrapper.LeftColumnText",
@"Hot
Another

Barney", wrapper.LeftColumnText);

			AssertEquals("wrapper.RightColumnText",
@"Nuts
Toblerone

Rubble", wrapper.RightColumnText);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Table                                       (Default Field: TitleText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DataSourceType                          String
LeftColumnText                          String
LeftColumnTitle                         String
RightColumnText                         String
RightColumnTitle                        String
TitleSupplement                         String
TitleText                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			MapTable mapTable = new MapTable("Roitter", "Tickers", "Fred", 30, "Flintstone", 25, true);
			return new TableWrapper(mapTable, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new TableWrapper(null, Factory);
		}
	}
}
