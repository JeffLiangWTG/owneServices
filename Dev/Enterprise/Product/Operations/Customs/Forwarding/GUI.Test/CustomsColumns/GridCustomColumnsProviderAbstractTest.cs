using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Forwarding.GUI.Testing
{
	public abstract class GridCustomColumnsProviderAbstractTest<T> : TestCaseWithFactory
		where T : BusinessObject
	{
		public void TestNoNullParameterIssue()
		{
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				var collection = new DummyBizObjectCollection(Factory);
				grid.SetDataBinding(collection, "");
				var columnsProvider = GetGridCustomColumnsProvider();
				var gridControl = grid.GetGridControl();
				gridControl.IsListManagerNotNull = false;
				AssertNoExceptionThrown(() => columnsProvider.AddColumns(grid));
			}
		}

		public void TestMissingColumnForTestFetchHint()
		{
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				var collection = new DummyBizObjectCollection(Factory);
				grid.SetDataBinding(collection, "");
				var columnsProvider = GetGridCustomColumnsProvider();
				columnsProvider.AddColumns(grid);
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				var excludedColumnNames = GetExcludedColumnNamesForTestFetchHint();
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName).Where(c => excludedColumnNames.All(d => d != c)).ToArray();
				var methodNames = GetType().GetMethods().Select(c => c.Name).Where(c => c.StartsWith(PrefixForMethodName, true, CultureInfo.InvariantCulture)).ToArray();
				var list = new List<string>();
				foreach (var columnName in columnNames)
				{
					var methodName = string.Concat(PrefixForMethodName, columnName.Replace('.', '_').Replace('+', '_'));
					if (methodNames.All(c => c != methodName))
					{
						var propertyAndMethod = string.Concat(columnName, " - ", methodName);
						list.Add(propertyAndMethod);
					}
				}

				var message = string.Concat("Following columns miss matching test cases. For performance reason please check their max db hits and add corresponding test cases.", System.Environment.NewLine, string.Join(System.Environment.NewLine, list));
				AssertEquals(message, false, list.Any());
			}
		}

		protected void BashFetchForView(string columnName, int maxDbHits)
		{
			var factory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var query = new ZQuery(PkColumn, keysForTest);
			var objs = factory.Load<T>(query);
			Assert("Should setup business objects to run test.", objs.Any());
			factory.ResetDatabaseLoadCount();
			foreach (var obj in objs)
			{
				obj.FetchStrategy.FetchForView(new[] { new TableColumn(string.Empty, columnName) });
			}

			foreach (var obj in objs)
			{
				HitColumn(obj, columnName);
			}

			var message = string.Format("The {0}'s max db hit should be {1}", columnName, maxDbHits);
			AssertMaxDbHits(message, maxDbHits, factory);
		}

		void HitColumn(T obj, string columnName)
		{
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				var collection = new DummyBizObjectCollection(obj.Factory)
				{
					obj
				};
				grid.SetDataBinding(collection, "");
				var columnsProvider = GetGridCustomColumnsProvider();
				columnsProvider.AddColumns(grid);
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
				AssertNotNull($"Column: {columnName} should be added from the GridCustomColumnsProvider.", column);
				column.IsVisible = true;
				Application.DoEvents();
			}
		}

		ZGuid[] keysForTest;
		protected override void SetUp()
		{
			base.SetUp();
			keysForTest = keysForTest ?? (keysForTest = CreateKeysForTest());
		}

		const string PrefixForMethodName = "TestBashFetchForView_";
		protected abstract SchemaPKColumn PkColumn { get; }

		protected abstract GridCustomColumnsProvider GetGridCustomColumnsProvider();
		protected abstract ZGuid[] CreateKeysForTest();
		protected virtual string[] GetExcludedColumnNamesForTestFetchHint()
		{
			return System.Array.Empty<string>();
		}

		class DummyBizObjectCollection : BusinessObjectCollection<T>
		{
			public DummyBizObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}
}
