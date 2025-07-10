using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DirectSqlLoaderTest : TestCaseWithFactory
	{
		public void TestLoadPKs_Filter()
		{
			var dummies = CreateDummies();
			var loader = new DirectSqlLoader();

			var emptyResultQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			emptyResultQuery.AddToFilter(DummyBizoSchema.Z0_Description, "don't exist");

			var singleResultQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			singleResultQuery.AddToFilter(DummyBizoSchema.Z0_Description, dummies[1].Z0_Description);

			AssertEquals(0, loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { IsNoResultQuery = true }).Count);
			AssertEquals(0, loader.LoadPKs(emptyResultQuery).Count);
			AssertEquals(dummies[1].PK, loader.LoadPKs(singleResultQuery).SingleOrDefault());
			AssertContainsExactElementsInAnyOrder(dummies.Select(d => d.PK), loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject))));
		}

		public void TestLoadPKs_OrderBy()
		{
			var dummies = CreateDummies();
			var loader = new DirectSqlLoader();

			Assert("(pre-condition)", dummies.Select(d => d.PK).Distinct().Count() > 1);

			AssertEquals
			(
				string.Join("\r\n", dummies.OrderBy(d => d.Z0_Description).Select(d => d.PK)),
				string.Join("\r\n", loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { OrderBy = "Z0_Description ASC" }))
			);

			AssertEquals
			(
				string.Join("\r\n", dummies.OrderByDescending(d => d.Z0_Description).Select(d => d.PK)),
				string.Join("\r\n", loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { OrderBy = "Z0_Description DESC" }))
			);
		}

		public void TestLoadPKs_MaximumRows()
		{
			var dummies = CreateDummies();
			var loader = new DirectSqlLoader();

			AssertEquals(2, loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { MaximumRows = 2 }).Count);
			AssertEquals(3, loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { MaximumRows = 3 }).Count);

			AssertEquals
			(
				"interaction of OrderBy and MaximumRows (ASC)",
				string.Join("\r\n", dummies.OrderBy(d => d.Z0_Description).Take(2).Select(d => d.PK)),
				string.Join("\r\n", loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject))
				{
					OrderBy = "Z0_Description ASC",
					MaximumRows = 2
				}))
			);

			AssertEquals
			(
				"interaction of OrderBy and MaximumRows (DESC)",
				string.Join("\r\n", dummies.OrderByDescending(d => d.Z0_Description).Take(2).Select(d => d.PK)),
				string.Join("\r\n", loader.LoadPKs(new ZDBOnlyQuery(typeof(DummyBusinessObject))
				{
					OrderBy = "Z0_Description DESC",
					MaximumRows = 2
				}))
			);
		}

		public void TestLoadPKs_TVP()
		{
			var dummies = CreateDummies();
			var loader = new DirectSqlLoader();

			var manyGuids = new List<ZGuid>();
			for (var i = 0; i < 1000; i++)
			{
				manyGuids.Add(ZGuid.NewZGuid());
			}

			manyGuids[manyGuids.Count / 2] = dummies[1].PK;

			var tvpQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject)) { AllowTableValuedParameters = true };
			tvpQuery.AddToFilter(DummyBizoSchema.PK, manyGuids);

			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals
				(
					string.Join("\r\n", new[] { dummies[1].PK }),
					string.Join("\r\n", loader.LoadPKs(tvpQuery))
				);

				var sql = Db.Connection.ExecutedCommands.Single(s => s.Contains("DummyBizo"));
				AssertLessThan("query is expected not to have each value as a parameter", sql.Length, manyGuids.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestZQueryUsage()
		{
			var type = typeof(DirectSqlLoader);
			foreach (var method in type.GetMethods())
			{
				foreach (var methodParameter in method.GetParameters())
				{
					var parameterType = methodParameter.ParameterType;
					if (typeof(ZQuery).IsAssignableFrom(parameterType) && !typeof(ZDBOnlyQuery).IsAssignableFrom(parameterType))
					{
						var query = new ZQuery();
						query.OrderBy = DummyBizoSchema.Z0_Description.Name;
						var sql = query.GetAsCompleteSQLStatement(DummyBizoSchema.PK.TableName, false, new SchemaColumn[] { DummyBizoSchema.PK });

						Fail($@"
Method {method.Name} uses {nameof(ZQuery)} instead of {nameof(ZDBOnlyQuery)} as a parameter.
Make sure that parameters of {nameof(ZQuery)}, such as {nameof(ZQuery.OrderBy)} and {nameof(ZQuery.MaximumRows)}, are handled correctly and have appropriate tests.
In some cases {nameof(ZQuery.OrderBy)} is ignored on {nameof(ZQuery)} that is not {nameof(ZDBOnlyQuery)} during SQL generation.
Example:
{sql}
						".Trim());
					}
				}
			}
		}

		DummyBusinessObject[] CreateDummies()
		{
			const int samplesCount = 3;
			var dummies = new DummyBusinessObject[samplesCount];
			for (var i = 0; i < samplesCount; i++)
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummies[i] = dummy;
				dummy.Z0_Description = $"Test-Dummy-{i}";
			}

			Factory.Save();

			return dummies;
		}
	}
}
