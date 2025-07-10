using System;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	abstract class ColumnCreatorWithDefaultValue : IColumnCreatorWithValue
	{
		public ColumnCreatorWithDefaultValue(IUpgradeManager manager, ColumnChangeMetadata columnMetadata, string dbBeingUpgraded)
		{
			this.columnMetadata = columnMetadata;
			this.dbBeingUpgraded = dbBeingUpgraded;
			this.defaultValue = columnMetadata.GetDefaultClause();
		}

		protected readonly ColumnChangeMetadata columnMetadata;
		protected readonly string dbBeingUpgraded;
		protected readonly string defaultValue;

		#region IColumnCreatorWithValue Members

		string IColumnCreatorWithValue.PopulateExpression
		{
			get { return GetPopulateExpression(); }
		}

		protected virtual string GetPopulateExpression()
		{
			return defaultValue;
		}

		string IColumnCreatorWithValue.PopulateSource
		{
			get { return GetPopulateSource(); }
		}

		protected virtual string GetPopulateSource()
		{
			return String.Empty;
		}

		string IColumnCreatorWithValue.PopulateTriggerExpression
		{
			get { return GetPopulateTriggerExpression(); }
		}

		protected virtual string GetPopulateTriggerExpression()
		{
			return String.Empty;
		}

		string IColumnCreatorWithValue.PopulateTriggerSource
		{
			get { return GetPopulateTriggerSourceFormat(); }
		}

		protected virtual string GetPopulateTriggerSourceFormat()
		{
			return String.Empty;
		}

		string IColumnCreatorWithValue.OrderByExpression
		{
			get { return GetOrderByExpression(); }
		}

		protected virtual string GetOrderByExpression()
		{
			return String.Empty;
		}

		#endregion
	}
}
