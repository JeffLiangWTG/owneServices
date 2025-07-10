using System;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB.Keys
{
	public class ForeignKey : Key
	{
		public ForeignKey(ColumnDef columnDef)
			: base(columnDef)
		{
		}

		public override Table ReferenceTable
		{
			get
			{
				if (referenceTable == null)
				{
					// this is a temporary code that will be resolved in one of the next WIs
					if (Name == ProcessTasksSchema.Constants.P9_SE_NKExceptionEvent)
					{
						referenceTable = Table.Get(ProcessWorkflowExceptionTypeSchema.Constants.TableName);
					}
					else
					{
						string[] nameParts = Name.Split('_');
						var tableName = TableNameHelper.GetTableNameFromPrefix(nameParts[1]);
						if (string.IsNullOrEmpty(tableName))
						{
							throw new InvalidOperationException(FormattableString.Invariant($"The column name [{Name}] does not have a valid foreign key prefix"));
						}
						referenceTable = Table.Get(tableName);
					}
				}
				return referenceTable;
			}
			set
			{
				referenceTable = value;
			}
		}
		Table referenceTable;

		public override ColumnDef Discriminator
		{
			get
			{
				// TODO: Should be able to use method in Polymorphic Key
				if (Type == ColumnType.PolymorphicKey)
				{
					return Table.Columns.TableCodes.First();
				}
				return null;
			}
		}

		public static ForeignKey Build(ColumnDef fk, Table refTable)
		{
			ForeignKey fromKey;
			if (fk is ForeignKey)
			{
				fromKey = (ForeignKey)fk;
				fromKey.ReferenceTable = refTable;
			}
			else
			{
				fromKey = new ForeignKey(fk)
				{
					ReferenceTable = refTable
				};
			}
			return fromKey;
		}

		public static ForeignKey Build(ColumnDef fk, ColumnDef referenceKey)
		{
			var refTable = referenceKey.Table;
			ForeignKey fromKey;
			if (fk is ForeignKey)
			{
				fromKey = (ForeignKey)fk;
				fromKey.ReferenceTable = refTable;
				fromKey.ReferenceColumnDef = referenceKey;
			}
			else
			{
				fromKey = new ForeignKey(fk)
				{
					ReferenceTable = refTable,
					ReferenceColumnDef = referenceKey
				};
			}
			return fromKey;
		}
	}
}
