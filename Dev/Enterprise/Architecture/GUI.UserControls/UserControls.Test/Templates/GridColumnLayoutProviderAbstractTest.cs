using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class GridColumnLayoutProviderAbstractTest<TGridColumnLayoutProvider> : TestCaseWithFactory
		where TGridColumnLayoutProvider : class, IGridColumnLayoutProvider
	{
		public void TestGridColumns()
		{
			var layout = CreateGridColumnLayoutProvider().Layout;
			AssertNotNull("Layout", layout);

			var allColumns = ExpectedColumns ?? Array.Empty<(string, Type, int)>();
			var columns = layout.Columns;
			var expectedColumnsCount = ExpectedColumns.Count;

			AssertEquals("Column Count", expectedColumnsCount, columns.Count);

			var propertyColumnDictionary = columns.ToDictionary(c => c.ColumnName, c => c);

			CombineAssertions("Columns", () =>
			{
				foreach (var (columnName, columnType, columnWidth) in allColumns)
				{
					var columnInfo = propertyColumnDictionary[columnName];
					AssertNotNull($"Column with name: {columnName}", columnInfo);
					AssertEquals($"Column Type for {columnName}", columnType, columnInfo?.GetType());
					AssertEquals($"Column Width {columnName}", columnWidth, columnInfo?.Width);
				}
			});
		}

		public void TestGridColumnBinding()
		{
			var layout = CreateGridColumnLayoutProvider().Layout;
			AssertNotNull("Layout", layout);

			var columns = layout.Columns;
			CombineAssertions("Column Binding", () =>
			{
				foreach (var columnInfo in columns)
				{
					var boundProperty = GetBoundProperty(GridBoundEntityType, columnInfo.ColumnName);
					AssertNotNull($"Bound Property: {columnInfo.ColumnName}", boundProperty);
				}
			});
		}

		static string GetBoundProperty(Type boundType, string propertyName)
		{
			var entityPropertyArray = boundType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
			var propertyParts = propertyName.Split(new[] { '.', '+' }, 2);
			var propertyInfo = entityPropertyArray.FirstOrDefault(p => string.Equals(p.Name, propertyParts[0], StringComparison.OrdinalIgnoreCase));
			if (propertyInfo == null)
			{
				return null;
			}

			return propertyParts.Length == 1
				? propertyInfo.Name
				: GetBoundProperty(propertyInfo.PropertyType, propertyParts[1]);
		}

		protected abstract IReadOnlyCollection<(string, Type, int)> ExpectedColumns { get; }

		protected virtual TGridColumnLayoutProvider CreateGridColumnLayoutProvider() => Activator.CreateInstance<TGridColumnLayoutProvider>();

		protected abstract Type GridBoundEntityType { get; }
	}
}
