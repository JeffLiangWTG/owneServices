using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class GridColumnLayoutBuilder
	{
		GridColumnLayoutBuilder()
		{
			columnReferences = new Dictionary<string, IGridColumnReference>();
		}

		public static GridColumnLayoutBuilder Create() => new GridColumnLayoutBuilder();

		public IGridColumnLayout Build()
		{
			var columnLayout = new GridColumnLayout();
			columnReferences.Values.ForEach(c => columnLayout.AddColumn(c.CreateGridColumnInfo()));
			return columnLayout;
		}

		public void AddColumn<TColumnInfo>(string columnName, int width, Action<TColumnInfo> configureColumnAction = null) where TColumnInfo : ZGridColumnInfo, new()
		{
			Argument.NotNullOrEmpty(columnName, nameof(columnName));
			var columnReference = new GridColumnReference<TColumnInfo>(columnName, width, configureColumnAction);
			AddColumn(columnReference);
		}

		public void AddColumn(IGridColumnReference columnReference)
		{
			Argument.NotNull(columnReference, nameof(columnReference));

			var columnName = columnReference.ColumnName;
			if (columnReferences.ContainsKey(columnName))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Column with the same name already exists: {columnName}"));
			}

			columnReferences[columnName] = columnReference;
		}

		readonly Dictionary<string, IGridColumnReference> columnReferences;
	}
}
