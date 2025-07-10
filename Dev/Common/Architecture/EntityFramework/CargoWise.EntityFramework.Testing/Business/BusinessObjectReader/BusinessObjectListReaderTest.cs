using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectListReaderTest : TestCaseWithFactory
	{
		public void TestReadElements()
		{
			Factory.New<DummyBusinessObject>().Z0_Code = "XX1";
			Factory.New<DummyBusinessObject>().Z0_Code = "XX2";
			Factory.New<DummyBusinessObject>().Z0_Code = "XX3";
			Factory.New<DummyBusinessObject>().Z0_Code = "XX4";
			Factory.New<DummyBusinessObject>().Z0_Code = "XX5";
			Factory.New<DummyBusinessObject>().Z0_Code = "YY1";
			Factory.New<DummyBusinessObject>().Z0_Code = "YY2";
			Factory.Save();

			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "XX") { OrderBy = "Z0_Code" };
			IEnumerable collection = new BusinessObjectListReader(query, typeof(DummyBusinessObject));
			List<DummyBusinessObject> list = collection.Cast<object>().Cast<DummyBusinessObject>().ToList();

			AssertEquals(5, list.Count);
			AssertEquals("XX1", list[0].Z0_Code);
			AssertEquals("XX2", list[1].Z0_Code);
			AssertEquals("XX3", list[2].Z0_Code);
			AssertEquals("XX4", list[3].Z0_Code);
			AssertEquals("XX5", list[4].Z0_Code);
		}

		public void TestIsReadonlyCollection()
		{
			IList collection = new BusinessObjectListReader(new ZQuery(), typeof(DummyBusinessObject));
			Assert(collection.IsReadOnly);
			Assert(collection.IsFixedSize);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestNoChangesAllowed()
		{
			IList collection = new BusinessObjectListReader(new ZQuery(), typeof(DummyBusinessObject));
			collection.Add(Factory.New<DummyBusinessObject>());
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestNoDirrectAccessAllowed()
		{
			IList collection = new BusinessObjectListReader(new ZQuery(), typeof(DummyBusinessObject));
			AssertEquals("Whatever value", collection[10]);
		}

		[ExpectNoExceptions]
		public void TestCanGetFirstElement()
		{
			IList collection = new BusinessObjectListReader(new ZQuery(), typeof(DummyBusinessObject));
			AssertNull(collection[0]);

			Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertNotNull(collection[0]);
		}
	}
}
