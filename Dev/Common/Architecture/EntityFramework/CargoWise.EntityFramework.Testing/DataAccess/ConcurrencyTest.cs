using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ConcurrencyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConcurrentQueryParameterisation()
		{
			//Arrange
			var thread1Start = new ManualResetEvent(false);
			var thread2Start = new ManualResetEvent(false);
			var thread1Pause = new ManualResetEvent(false);
			var thread1End = new ManualResetEvent(false);
			var thread2End = new ManualResetEvent(false);
			var thread1Resume = new ManualResetEvent(false);

			Exception threadError = null;

			void StartThread(string name, WaitHandle start, EventWaitHandle end, Action action)
			{
				new Thread(() =>
				{
					try
					{
						start.WaitOne();
						action();
					}
					catch (Exception ex)
					{
						threadError = new Exception($"{name} encountered an error: {ex.Message}", ex);
					}
					finally
					{
						end.Set();
					}
				})
				{
					Name = name
				}.Start();
			}

			(string filterString, HashSet<string> declaredParams) Test(ZQuery query)
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					query = factory.GetFilterIncludingActiveFilter(typeof(MyBusinessObject), query);

					var dataQuery = query.ParameterisedText;
					var paramSet = new HashSet<string>(dataQuery.Parameters.Select(n => n.ParameterName));

					return (dataQuery.ParameterisedQueryText, paramSet);
				}
			}

			// Act
			MyBusinessObject.OnActiveFilterCreated = (query) =>
			{
				var param = (ZSqlParameter)query.FilterParts.FilterParts[0];
				param.OnParameterisedSqlAdded = () =>
				{
					if (Thread.CurrentThread.Name == "Thread1")
					{
						thread1Pause.Set();
						thread1Resume.WaitOne();
					}
				};
			};

			StartThread("Thread2", thread2Start, thread2End, () =>
			{
				Test(new ZQuery(MySchema.Column1, new ZString("some-value")));
			});

			StartThread("Thread1", thread1Start, thread1End, () =>
			{
				var (filterString, declaredParams) = Test(new ZQuery());

				foreach (Match match in Regex.Matches(filterString, @"@\w+"))
				{
					if (!declaredParams.Contains(match.Value))
					{
						throw new AssertionFailedError($"Must declare the scalar variable {match.Value}.\n"
							+ $"Query filter text: '{filterString}'\n"
							+ $"Declared variables: '{string.Join(", ", declaredParams)}'");
					}
				}
			});

			thread1Start.Set();
			thread1Pause.WaitOne();
			thread2Start.Set();
			thread2End.WaitOne();
			thread1Resume.Set();
			thread1End.WaitOne();

			// Assert
			if (threadError != null)
			{
				throw threadError;
			}
		}

		class MyBusinessObject : BusinessObject
		{
			public static Action<ZQuery> OnActiveFilterCreated { get; set; }

			public static ZQuery ActiveFilter
			{
				get
				{
					var query = new ZQuery(MySchema.Column2, true);
					OnActiveFilterCreated?.Invoke(query);

					return query;
				}
			}

			public MyBusinessObject(DataRow row) : base(row) { }
			public override SchemaGuidColumn PKSchemaColumn => MySchema.PKColumn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		class MySchema : ITableSchema
		{
			public static MySchema Instance { get; } = new MySchema();
			public static SchemaPKColumn PKColumn { get; } = new SchemaPKColumn(Instance, "ID", true);
			public static SchemaStringColumn Column1 { get; } = new SchemaStringColumn(Instance, nameof(Column1), 1, SqlDbType.VarChar, "", false, 20, false, false, "dbo.TVP_varchar_250");
			public static SchemaBoolColumn Column2 { get; } = new SchemaBoolColumn(Instance, nameof(Column2), 2, false, true, true, "");

			public string SqlSchemaName { get; } = "Schema1";
			public string TableName { get; } = "Table1";
			public SchemaPKColumn PK { get => PKColumn; }
			public string PkIndexName { get; } = "";
			public SchemaColumnCollection All { get; }

			public SchemaColumn GetSchemaColumn(string columnName)
			{
				throw new NotImplementedException();
			}
		}
	}
}
