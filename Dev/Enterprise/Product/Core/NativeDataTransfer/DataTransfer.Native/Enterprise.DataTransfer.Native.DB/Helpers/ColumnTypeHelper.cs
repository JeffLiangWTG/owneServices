namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	class ColumnTypeHelper
	{
		internal ColumnDef ColumnDef { get; set; }

		internal ColumnType GetColumnType()
		{
			if (IsPolyMorphicKey())
			{
				return ColumnType.PolymorphicKey;
			}

			if (IsNaturalKey())
			{
				return ColumnType.NaturalKey;
			}

			if (IsPrimaryKey())
			{
				return ColumnType.PrimaryKey;
			}

			if (IsForeignKey())
			{
				return ColumnType.ForeignKey;
			}

			if (IsTableCode())
			{
				return ColumnType.TableCode;
			}

			if (IsTableName())
			{
				return ColumnType.TableName;
			}
			return ColumnType.NormalColumn;
		}

		internal bool IsPolyMorphicKey()
		{
			return HasParentIdName() && IsKey();
		}

		internal bool IsForeignKey()
		{
			return (HasKeyName() || HasParentIdName()) && IsKey() && !IsPrimaryKey();
		}

		internal bool IsPrimaryKey()
		{
			return HasKeyName() && IsKey() && ColumnDef.Name.Contains("_PK");
		}

		internal bool IsNaturalKey()
		{
			return HasKeyName() && ColumnDef.Name.Contains("_NK");
		}

		internal bool IsTableCode()
		{
			return ColumnDef.Name.Contains("ParentTableCode");
		}

		internal bool IsTableName()
		{
			return ColumnDef.Name.EndsWith("ParentTable") || ColumnDef.Name.EndsWith("_Table");
		}

		internal bool IsKey()
		{
			return ColumnDef.DataType == DbDataType.UniqueIdentifier;
		}

		internal bool HasKeyName()
		{
			string[] colNameParts = ColumnDef.Name.Split('_');
			return (colNameParts.Length >= 2 && colNameParts[0].Length >= 2 && colNameParts[0].Length <= 3 && colNameParts[1].Length >= 2 && colNameParts[1].Length <= 3);
		}

		internal bool HasParentIdName()
		{
			string[] colNameParts = ColumnDef.Name.Split('_');
			return (colNameParts.Length == 2 && colNameParts[0].Length >= 2 && colNameParts[0].Length <= 3 && (colNameParts[1] == "ForeignKey" || colNameParts[1] == "ParentID"));
		}
	}
}