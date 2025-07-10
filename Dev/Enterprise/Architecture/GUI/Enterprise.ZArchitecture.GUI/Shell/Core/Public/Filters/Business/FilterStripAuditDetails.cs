using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public static class FilterStripAuditDetails
	{
		public static void AddAuditDetailsColumns(ZGrid grid, string tableName, Type typeOfElements)
		{
			if (String.IsNullOrWhiteSpace(tableName) || grid == null || typeOfElements == null)
			{
				throw new InvalidOperationException("TableName, grid and the typeOfElements must be provided.");
			}

			var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
			if (tableSchema != null)
			{
				var tablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(tableSchema.PK.Name);

				var createUserColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_" + "SystemCreateUser"];
				var createBranchColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_" + "SystemCreateBranch"];
				var createDepartmentColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_" + "SystemCreateDepartment"];
				var createTimeColumn = (SchemaDateTimeColumn)tableSchema.All[tablePrefix + "_" + "SystemCreateTimeUtc"];
				var lastEditUserColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_" + "SystemLastEditUser"];
				var lastEditTimeColumn = (SchemaDateTimeColumn)tableSchema.All[tablePrefix + "_" + "SystemLastEditTimeUtc"];

				var displayTimeKind = DateTimeKind.Unspecified;
				if (createTimeColumn != null || lastEditTimeColumn != null)
				{
					displayTimeKind = GetDisplayDateTimeKind(typeOfElements);
				}

				if (createUserColumn != null)
				{
					AddStringColumn(grid, createUserColumn.Name, Res.GetString("cb5d6ea9-643a-4702-b4e5-6a2fef08b31a", "Created By"), AuditDetailsGroupText);
				}

				if (createBranchColumn != null)
				{
					AddStringColumn(grid, createBranchColumn.Name, Res.GetString("7F7DD327-5F2F-4BB8-977D-7F799E74B573", "Created Branch"), AuditDetailsGroupText);
				}

				if (createDepartmentColumn != null)
				{
					AddStringColumn(grid, createDepartmentColumn.Name, Res.GetString("5A6DEDD2-3620-498B-809C-7C42E41886F5", "Created Department"), AuditDetailsGroupText);
				}

				if (createTimeColumn != null)
				{
					AddDateTimeColumn(grid, createTimeColumn, Res.GetString("1b929c70-b6d3-42c2-8f6d-125d7e08ac6f", "Created Time"), AuditDetailsGroupText, displayTimeKind);
				}

				if (lastEditUserColumn != null)
				{
					AddStringColumn(grid, lastEditUserColumn.Name, Res.GetString("350bc7aa-5dbc-415c-9391-2fa43c2340da", "Last Edit"), AuditDetailsGroupText);
				}

				if (lastEditTimeColumn != null)
				{
					AddDateTimeColumn(grid, lastEditTimeColumn, Res.GetString("cb5c64a7-cc86-4ce8-baa2-33736e38a883", "Last Edited Time"), AuditDetailsGroupText, displayTimeKind);
				}
			}
		}

		#region Implementation

		public static ResourceStringData AuditDetailsGroupText
		{
			get { return Res.GetData("d97ed595-0df6-47bd-9d1c-af69010a2e0a", "Audit Details"); }
		}

		public static void AddStringColumn(ZGrid grid, string columnName, string columnCaption, ResourceStringData groupName)
		{
			var columnStyleInfo = new ZTextBoxColumnStyleInfo();
			columnStyleInfo.ColumnName = columnName;
			columnStyleInfo.Caption = columnCaption;
			columnStyleInfo.GroupName = groupName;
			columnStyleInfo.IsReadOnly = true;
			columnStyleInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref columnStyleInfo, 66, true);
			grid.ColumnStyles.Add(columnStyleInfo);
		}

		public static void AddDateTimeColumn(ZGrid grid, string columnName, string columnCaption, ResourceStringData groupName, DateTimeKind displayTimeKind)
		{
			var columnStyleInfo = new ZDateEditColumnStyleInfo();
			columnStyleInfo.GroupName = groupName;
			columnStyleInfo.IsReadOnly = true;
			columnStyleInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref columnStyleInfo, 100, true);

			if (displayTimeKind == DateTimeKind.Utc)
			{
				columnStyleInfo.ColumnName = columnName;
				columnStyleInfo.Caption = columnCaption + " (UTC)";
			}
			else
			{
				columnStyleInfo.ColumnName = columnName;
				columnStyleInfo.Caption = columnCaption;
			}

			grid.ColumnStyles.Add(columnStyleInfo);
		}

		public static void AddDateTimeColumn(ZGrid grid, SchemaDateTimeColumn schemaColumn, string columnCaption, ResourceStringData groupName, DateTimeKind displayTimeKind)
		{
			var columnStyleInfo = new ZDateEditColumnStyleInfo();
			columnStyleInfo.GroupName = groupName;
			columnStyleInfo.IsReadOnly = true;
			columnStyleInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref columnStyleInfo, 100, true);

			if (displayTimeKind == DateTimeKind.Local)
			{
				var propertyDescriptor = new LocalAuditTimePropertyDescriptor(schemaColumn);
				((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor = propertyDescriptor;
				columnStyleInfo.ColumnName = propertyDescriptor.Name;
				columnStyleInfo.Caption = columnCaption;
			}
			else if (displayTimeKind == DateTimeKind.Utc)
			{
				columnStyleInfo.ColumnName = schemaColumn.Name;
				columnStyleInfo.Caption = columnCaption + " (UTC)";
			}
			else
			{
				columnStyleInfo.ColumnName = schemaColumn.Name;
				columnStyleInfo.Caption = columnCaption;
			}

			grid.ColumnStyles.Add(columnStyleInfo);
		}

		internal static DateTimeKind GetDisplayDateTimeKind(Type typeOfElement)
		{
			var displayInUtc = ShouldDisplayInUtcTimeForEditAndCreateLogFieldsAttribute.IsApplied(typeOfElement);

			return displayInUtc ? DateTimeKind.Utc : DateTimeKind.Local;
		}

		#endregion
	}
}
