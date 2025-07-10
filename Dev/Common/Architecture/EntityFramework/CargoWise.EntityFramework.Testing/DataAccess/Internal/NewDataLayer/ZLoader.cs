using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZLoaderTest : TestCase
	{
		public void TestGetTable_ThreadSafe()
		{
			var tasksAndCts = new List<(Task, CancellationTokenSource)>();
			for (int i = 0; i < 10; i++)
			{
				var cts = new CancellationTokenSource();
				var task = Task.Run(() => HammerGetTable(cts.Token),cts.Token);
				tasksAndCts.Add((task,cts));
			}

			Task.Delay(100).Wait();

			foreach (var pair in tasksAndCts)
			{
				pair.Item2.Cancel();
				pair.Item1.Wait();
			}
		}

		public void TestGetTable_Cache()
		{
			for (int i = 0; i < 10; i++)
			{
				Loader.GetTable("Bung", true);
				Loader.GetTable("BuNg", true);
				Loader.GetTable("Chumpo", true);
			}
			var cache = Data.ExtendedProperties.Values.OfType<Dictionary<string, ZDataTable>>().Single();
			AssertEquals(2, cache.Count);
		}

		void HammerGetTable(CancellationToken token)
		{
			try
			{
				int i = 0;
				while (!token.IsCancellationRequested)
				{
					AssertNotNull(Loader.GetTable("Test" + i++.ToString()));
				}
				token.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException)
			{
				// ignored...
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected override void SetUp()
		{
			Data = new DataSet();
			Loader = new TestZLoader(Data);
			base.SetUp();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataSet Data;
		ZLoader Loader;

		#region TestZLoader

		class TestZLoader : ZLoader
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public TestZLoader(DataSet data) : base(data)
			{
			}

			public override int GetCount(ZCountDataQuery query)
			{
				return 0;
			}

			public override Stream GetBinaryFieldStream(DataRow row, string tableName, string columnName)
			{
				return null;
			}

			public override TextReader GetTextFieldReader(DataRow row, string tableName, string columnName, bool closeReaderBetweenReads)
			{
				return null;
			}

			public override void LoadBlobField(DataRow row, SchemaColumn column)
			{
			}

			public override void LoadBlobFieldsForTable(string tableName, ZDataRowDictionary rows, IEnumerable<SchemaColumn> columns)
			{
			}

			public override NonPersistentLoaderResponse LoadNonPersistentRows(ZNonPersistentDataQuery filter)
			{
				return null;
			}

			public override LoaderResponse LoadPersistentRowsIntoDataSet(IList<ZDataQuery> query)
			{
				return new LoaderResponse(0, System.Array.Empty<DataRowLoadResponse>());
			}

			protected override ZDataTable AddTableToDataSet(string tableName)
			{
				ZDataTable table = new ZDataTable(tableName);
				Data.Tables.Add(table);
				return table;
			}

			public override Stream GetStream(DataRow row, string tableName, string columnName)
			{
				return null;
			}
		}

		#endregion
	}
}
