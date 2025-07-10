using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoAddInfoSchema_OLD : AutoSchema_OLD
	{
		public AutoAddInfoSchema_OLD(BusinessObjectInfo info)
			: base(info)
		{
		}

		public override string ToString()
		{
			return LinesOfCode(
				"		#region Schema",
				"",
				"		public new class Schema : " + Info.BaseClassName + ".Schema",
				"		{",
							CodeForProperties,
							!string.IsNullOrEmpty(Info.OldPrefix) ? CodeForOldPrefix : null,
							CodeForChildTable,
				"		}",
				"",
				(HasParentView && !HasParentSchema)
				? "		public new ITableSchema AddInfoTableSchema => AddInfoSchema;"
				: "		public ITableSchema AddInfoTableSchema => " + Info.TableName + "Schema.Instance;",
				"",
				"		#endregion",
				"");
		}

		bool HasParentView => hasParentView ??= !string.IsNullOrEmpty(Info.ParentView);
		bool? hasParentView;

		bool HasParentSchema => hasParentSchema ??= !string.IsNullOrEmpty(Info.ParentSchema);
		bool? hasParentSchema;

		protected override IEnumerable<AutoProperty> Properties => new AutoPropertyList(Info).Properties.Where(property => property.ColumnName.IndexOf(CargoWise.Schema.Schema.ClusterKeyColumnSuffix, StringComparison.InvariantCulture) == -1);

		string CodeForOldPrefix
		{
			get
			{
				var stringBuilder = new StringBuilder();
				if (!string.IsNullOrEmpty(Info.OldPrefix))
				{
					foreach (var property in Properties)
					{
						var fieldName = property.ColumnName.Substring(property.ColumnName.IndexOf("_", StringComparison.InvariantCulture) + 1);
						var fieldNameWithPrefix = $"{Info.OldPrefix}_{fieldName}";
						var columnNameWhiteSpace = "".PadLeft(MaxColumnLength - fieldNameWithPrefix.Length, ' ');
						stringBuilder.Append($@"
			public const string {fieldNameWithPrefix} {columnNameWhiteSpace}= ""{fieldNameWithPrefix}"";");
					}
				}

				return stringBuilder.ToString();
			}
		}

		string CodeForChildTable
		{
			get
			{
				if (Info.ChildTableProperties is null || Info.ChildTableProperties.Length == 0)
				{
					return null;
				}

				var stringBuilder = new StringBuilder();
				foreach (var property in Info.ChildTableProperties)
				{
					var fieldName = property.ColumnName;
					var columnNameWhiteSpace = "".PadLeft(MaxColumnLength - fieldName.Length, ' ');
					stringBuilder.Append($@"
			public const string {fieldName} {columnNameWhiteSpace}= {Info.ChildTableName}Schema.Constants.{fieldName};");
				}

				return stringBuilder.ToString();
			}
		}

		int? maxColumnLength;
		int MaxColumnLength => maxColumnLength ??=
			Info.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName.Length)
				.Concat(Info.ChildTableProperties?.Select(property => property.ColumnName.Length) ?? Enumerable.Empty<int>())
				.Max();

		new AddInfoBusinessObjectInfo Info => (AddInfoBusinessObjectInfo)base.Info;
	}
}
