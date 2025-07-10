using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DbOnlyBusinessObjectQueueTest : TestCaseWithFactory
	{
		public void TestProcessZGuid()
		{
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>();
			List<ZGuid> bizObjPKs = new List<ZGuid>();
			queue.Process((pk, e) => bizObjPKs.Add(pk));
			Assert(bizObjPKs.Count > 0);

			queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A"));
			queue.Process((pk, e) => Assert(bizObjPKs.Contains(pk)));
		}

		class DbOnlyBusinessObjectQueueThatThrows<T> : DbOnlyBusinessObjectQueue<T> where T : BusinessObject
		{
			public DbOnlyBusinessObjectQueueThatThrows(ZNonPersistentDataQuery query)
				: base(query) { }

			public int NumberOfTimesToThrow { get; set; }

			protected override ZGuid[] GetUnprocessedItemPKs_Core()
			{
				if (NumberOfTimesToThrow > 0)
				{
					NumberOfTimesToThrow--;

					throw CreateDeadlockedException();
				}
				return base.GetUnprocessedItemPKs_Core();
			}

			static SqlException CreateDeadlockedException()
			{
				return SqlExceptionBuilder.CreateSqlException<SqlException>(1205, 51, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 1);
			}
		}

		public void TestRetryOnDeadlock()
		{
			Enumerable.Range(0, 3)
				.Select(_ => Factory.NewWithValidTestData<DummyBusinessObject>())
				.ToList()
				.ForEach(dummy => dummy.Z0_Number = 70);

			Factory.Save();

			var query = new ZNonPersistentDataQuery(@"SELECT Z0_PK FROM dbo.DummyBizo WHERE Z0_Number > @num", new ZSqlParameterCollection(ZSqlParameter.New("@num", 69, DummyBizoSchema.Z0_Number)));
			var collection = new DbOnlyBusinessObjectQueueThatThrows<DummyBusinessObject>(query) { NumberOfTimesToThrow = 2 };

			collection.ProcessBatch((stuff, e) => AssertEquals("Should load those 3 elements we made", 3, stuff.Length), 100);

			collection.NumberOfTimesToThrow = int.MaxValue;
			AssertExceptionThrown<SqlException>("After too many attempts it should give up trying to beat the deadlock", () => collection.ProcessBatch((o, e) => { }, 100));
		}

		public void TestProcessZGuidWithNonPersistentDataQuery()
		{
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZNonPersistentDataQuery("SELECT " + DummyBizoSchema.PK.Name + " FROM " + DummyBizoSchema.Constants.TableName));
			var bizObjPKs = new List<ZGuid>();
			queue.Process((pk, e) => bizObjPKs.Add(pk));
			Assert(bizObjPKs.Count > 0);
		}

		public void TestProcessBizObj()
		{
			for (char c = 'A'; c <= 'C'; c++)
			{
				var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, c.ToString()));
				int bizObjCount = 0;
				List<BusinessObjectFactory> factories = new List<BusinessObjectFactory>();
				int maxOrgsPerFactory = 2;

				queue.Process((bizObj, e) =>
				{
					++bizObjCount;
					if (!factories.Contains(bizObj.Factory))
					{
						factories.Add(bizObj.Factory);
					}
				}, maxOrgsPerFactory);

				Assert("'" + c + "' Business Objects", bizObjCount > 0);
				int expectedFactories = bizObjCount / maxOrgsPerFactory + (bizObjCount % maxOrgsPerFactory > 0 ? 1 : 0);
				AssertEquals("'" + c + "' Factories", expectedFactories, factories.Count);
			}
		}

		public void TestProcessWithOrderBy()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A");
			query.OrderBy = DummyBizoSchema.Z0_Code.Name + " DESC";
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(query);

			DummyBusinessObject prevBizObj = null;

			queue.Process((bizObj, e) =>
			{
				Assert(prevBizObj == null || prevBizObj.Z0_Code.CompareTo(bizObj.Z0_Code) >= 0);
				prevBizObj = bizObj;
			}, 2);
		}

		public void TestProcessWithCancel()
		{
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A"));
			int i = 0;
			queue.Process((bizObj, e) =>
			{
				++i;
				if (i == 10)
				{
					e.Cancel = true;
				}
			}, 3);
			AssertEquals(10, i);
		}

		public void TestProcessWithTop()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A");
			query.MaximumRows = 13;
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(query);
			int i = 0;
			queue.Process((bizObj, e) =>
			{
				++i;
			}, 4);
			AssertEquals(13, i);
		}

		public void TestProcessBatch()
		{
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A"));
			queue.ProcessBatch((bizObjs, e) =>
			{
				BusinessObjectFactory factory = bizObjs[0].Factory;
				for (int i = 1; i < bizObjs.Length; i++)
				{
					AssertEquals(factory, bizObjs[i].Factory);
				}
			}, 3);
		}

		public void TestProcessBatchWithCancel()
		{
			var queue = new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A"));
			int i = 0;
			queue.ProcessBatch((bizObjs, e) =>
			{
				i += bizObjs.Length;
				if (i == 10)
				{
					e.Cancel = true;
				}
			}, 5);
			AssertEquals(10, i);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestBusinessObjects();
		}

		public void TestProcessItemsInOriginalOrder()
		{
			var queue =
				new DbOnlyBusinessObjectQueue<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A") { OrderBy = "Z0_Code Desc" });

			string result = "";

			queue.ProcessBatch(
				(bizos, e) =>
				{
					for (int i = 0; i < bizos.Length; i++)
					{
						result += bizos[i].Z0_Code;
					}
				}, 5);

			AssertEquals("A30A25A24A22A19A18A16A15A12A11A10A08A07A06A05A03A02A01", result);
		}

		void CreateTestBusinessObjects()
		{
			CreateDummyBusinessObject("A01", "BizObj01");
			CreateDummyBusinessObject("A02", "BizObj02");
			CreateDummyBusinessObject("A03", "BizObj03");
			CreateDummyBusinessObject("B04", "BizObj04");
			CreateDummyBusinessObject("A05", "BizObj05");
			CreateDummyBusinessObject("A06", "BizObj06");
			CreateDummyBusinessObject("A07", "BizObj07");
			CreateDummyBusinessObject("A08", "BizObj08");
			CreateDummyBusinessObject("C09", "BizObj09");
			CreateDummyBusinessObject("A10", "BizObj10");

			CreateDummyBusinessObject("A11", "BizObj11");
			CreateDummyBusinessObject("A12", "BizObj12");
			CreateDummyBusinessObject("B13", "BizObj13");
			CreateDummyBusinessObject("B14", "BizObj14");
			CreateDummyBusinessObject("A15", "BizObj15");
			CreateDummyBusinessObject("A16", "BizObj16");
			CreateDummyBusinessObject("C17", "BizObj17");
			CreateDummyBusinessObject("A18", "BizObj18");
			CreateDummyBusinessObject("A19", "BizObj19");
			CreateDummyBusinessObject("C20", "BizObj20");

			CreateDummyBusinessObject("C21", "BizObj21");
			CreateDummyBusinessObject("A22", "BizObj22");
			CreateDummyBusinessObject("C23", "BizObj23");
			CreateDummyBusinessObject("A24", "BizObj24");
			CreateDummyBusinessObject("A25", "BizObj25");
			CreateDummyBusinessObject("B26", "BizObj26");
			CreateDummyBusinessObject("B27", "BizObj27");
			CreateDummyBusinessObject("B28", "BizObj28");
			CreateDummyBusinessObject("C29", "BizObj29");
			CreateDummyBusinessObject("A30", "BizObj30");

			Factory.Save();
		}

		void CreateDummyBusinessObject(string code, string desc)
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = code;
			bo.Z0_Description = desc;
		}
	}
}
