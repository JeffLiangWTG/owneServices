using System.Collections.Generic;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(TableWrapperCollection))]
	sealed class TableWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<TableWrapperCollection>
	{
		public void TestConstructorWithTableList()
		{
			List<MapTable> list = new List<MapTable>();
			list.Add(new MapTable("1 Potato", "", "", 0, "", 0, false));
			list.Add(new MapTable("2 Potato", "", "", 0, "", 0, false));
			TableWrapperCollection collection = new TableWrapperCollection(list, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].TitleText", "1 Potato", collection[0].TitleText);
			AssertEquals("collection[1].TitleText", "2 Potato", collection[1].TitleText);
		}

		protected override TableWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new TableWrapperCollection(null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new TableWrapper(null, Factory);
		}
	}
}
