using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZPKCollectionTest : TestCase
	{
		ZGuid GetNewGuid()
		{
			return ZGuid.NewZGuid();
		}

		public void TestConstructionAndIndexer()
		{
			var guidCol = new List<ZGuid>();
			guidCol.Add(GetNewGuid());
			guidCol.Add(GetNewGuid());

			var col = new ZPKCollection(guidCol);

			AssertEquals("Should be same", guidCol.Count, col.Count);
			AssertEquals("Should be same", guidCol[0], col[0].PK);
			AssertEquals("Should be same", guidCol[1], col[1].PK);

			var pkDataList = new List<PKData>();
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });

			col = new ZPKCollection(pkDataList);

			AssertEquals(pkDataList.Count, col.Count);
			AssertEquals(pkDataList[0], col[0]);
			AssertEquals(pkDataList[1], col[1]);
		}

		public void TestRebuild()
		{
			var guidCol = new List<ZGuid>();
			guidCol.Add(GetNewGuid());
			guidCol.Add(GetNewGuid());

			var col = new ZPKCollection(new List<ZGuid>());
			col.Rebuild(guidCol);

			AssertEquals("Should be same", guidCol.Count, col.Count);
			AssertEquals("Should be same", guidCol[0], col[0].PK);
			AssertEquals("Should be same", guidCol[1], col[1].PK);

			var pkDataList = new List<PKData>();
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });

			col = new ZPKCollection(pkDataList);
			col.Rebuild(pkDataList);

			AssertEquals(pkDataList.Count, col.Count);
			AssertEquals(pkDataList[0], col[0]);
			AssertEquals(pkDataList[1], col[1]);
		}

		public void TestCount()
		{
			var guidCol = new List<ZGuid>();
			guidCol.Add(GetNewGuid());
			guidCol.Add(GetNewGuid());
			var col = new ZPKCollection(guidCol);
			AssertEquals("Should be 2", 2, col.Count);
		}

		public void TestIndexOf()
		{
			var guidCol = new List<ZGuid>();
			guidCol.Add(GetNewGuid());
			guidCol.Add(GetNewGuid());
			var col = new ZPKCollection(guidCol);

			AssertEquals("Should be same", 0, col.IndexOf(guidCol[0]));
			AssertEquals("Should be same", 1, col.IndexOf(guidCol[1]));

			var pkDataList = new List<PKData>();
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });
			col = new ZPKCollection(pkDataList);

			AssertEquals(0, col.IndexOf(pkDataList[0]));
			AssertEquals(1, col.IndexOf(pkDataList[1]));
		}

		public void TestListChanged()
		{
			var col = new ZPKCollection(new List<ZGuid>());

			col.ListChanging += new EventHandler(Col_ListChanging);
			col.ListChanged += new EventHandler(Col_ListChanged);

			ListChangingCount = 0;

			var guidCol = new List<ZGuid>();
			guidCol.Add(GetNewGuid());
			guidCol.Add(GetNewGuid());

			col.Rebuild(guidCol);

			AssertEquals("ListChanging", 1, ListChangingCount);
			AssertEquals("ListChanged", 1, ListChangingCount);

			col.Rebuild(new List<ZGuid>());

			AssertEquals("ListChanging", 2, ListChangingCount);
			AssertEquals("ListChanged", 2, ListChangingCount);

			col = new ZPKCollection(new List<PKData>());

			col.ListChanging += new EventHandler(Col_ListChanging);
			col.ListChanged += new EventHandler(Col_ListChanged);

			ListChangingCount = 0;

			var pkDataList = new List<PKData>();
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });
			pkDataList.Add(new PKData() { PK = GetNewGuid(), Data = GetNewGuid() });

			col.Rebuild(pkDataList);

			AssertEquals("ListChanging", 1, ListChangingCount);
			AssertEquals("ListChanged", 1, ListChangingCount);

			col.Rebuild(new List<PKData>());

			AssertEquals("ListChanging", 2, ListChangingCount);
			AssertEquals("ListChanged", 2, ListChangingCount);
		}

		void Col_ListChanging(object sender, EventArgs e)
		{
			ListChangingCount++;
			HadListChanging = true;
		}
		int ListChangingCount;
		bool HadListChanging;

		void Col_ListChanged(object sender, EventArgs e)
		{
			if (!HadListChanging)
			{
				throw new Exception("Have to have Changing before Changed");
			}

			HadListChanging = false;
		}
	}
}
