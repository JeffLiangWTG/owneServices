using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IColumnValueSetterInfo
	{
		SchemaColumn Column { get; }
		IColumnIndexer Row { get; }
		object Value { get; }
	}

	public abstract class ValueSetter
	{
		protected ValueSetter(Func<object> getValue, IXmlImportLogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.getValue = getValue;
		}

		public void SetValue()
		{
			SetValueCore();
		}
		protected abstract void SetValueCore();

		public ZString MatchingKey
		{
			get { return MatchingKeyCore; }
		}
		protected abstract ZString MatchingKeyCore { get; }

		protected readonly IXmlImportLogger logger;

		protected object value
		{
			get { return getValue(); }
		}

		readonly Func<object> getValue;
	}

	public abstract class ColumnValueSetter : ValueSetter, IColumnValueSetterInfo
	{
		protected ColumnValueSetter(IColumnIndexer row, SchemaColumn column, Func<object> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(getValue, logger)
		{
			this.row = Argument.NotNull(row, "row");
			this.column = Argument.NotNull(column, "column");
			this.rowPKSchema = rowPKSchema;
		}

		public static ZString GetKey(ZGuid pk, SchemaColumn column)
		{
			return pk.ToStringKey() + column.Name;
		}

		protected sealed override ZString MatchingKeyCore
		{
			get { return GetKey(row.GetValue(rowPKSchema ?? column.TableSchema.PK), column); }
		}

		protected readonly IColumnIndexer row;
		protected readonly SchemaColumn column;
		readonly SchemaPKColumn rowPKSchema;

		SchemaColumn IColumnValueSetterInfo.Column
		{
			get { return column; }
		}

		IColumnIndexer IColumnValueSetterInfo.Row
		{
			get { return row; }
		}

		object IColumnValueSetterInfo.Value
		{
			get { return value; }
		}
	}

	public class BinaryColumnValueSetter : ColumnValueSetter
	{
		public BinaryColumnValueSetter(IColumnIndexer row, SchemaBinaryColumn column, Func<ZBlob?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaBinaryColumn)column, (ZBlob?)value, logger);
		}
	}

	public class BooleanColumnValueSetter : ColumnValueSetter
	{
		public BooleanColumnValueSetter(IColumnIndexer row, SchemaBoolColumn column, Func<ZBool?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaBoolColumn)column, (ZBool?)value, logger);
		}
	}

	public class ByteColumnValueSetter : ColumnValueSetter
	{
		public ByteColumnValueSetter(IColumnIndexer row, SchemaByteColumn column, Func<ZByte?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaByteColumn)column, (ZByte?)value, logger);
		}
	}

	public class CodeDataObjectColumnValueSetter : ColumnValueSetter
	{
		public CodeDataObjectColumnValueSetter(IColumnIndexer row, SchemaStringColumn column, Func<ICodeDataObject> getValue, IXmlImportLogger logger, CharacterCase characterCase, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
			this.characterCase = characterCase;
		}

		readonly CharacterCase characterCase;

		protected sealed override void SetValueCore()
		{
			var codeDataObject = (ICodeDataObject)value;

			var code = codeDataObject?.Code;
			if (code.HasValue)
			{
				var valueToSet = characterCase switch
				{
					CharacterCase.Lower => code.Value.ToLower(),
					CharacterCase.Upper => code.Value.ToUpper(),
					_ => code.Value,
				};
				row.SetValue((SchemaStringColumn)column, valueToSet, logger);
			}
		}
	}

	public class DateColumnValueSetter : ColumnValueSetter
	{
		public DateColumnValueSetter(IColumnIndexer row, SchemaDateColumn column, Func<ZDate?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaDateColumn)column, (ZDate?)value, logger);
		}
	}

	public class DateTimeColumnValueSetter : ColumnValueSetter
	{
		public DateTimeColumnValueSetter(IColumnIndexer row, SchemaDateTimeColumn column, Func<ZDateTime?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaDateTimeColumn)column, (ZDateTime?)value, logger);
		}
	}

	public class DateTimeOffsetColumnValueSetter : ColumnValueSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public DateTimeOffsetColumnValueSetter(IColumnIndexer row, SchemaDateTimeOffsetColumn column, Func<ZDateTimeOffset?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaDateTimeOffsetColumn)column, (ZDateTimeOffset?)value, logger);
		}
	}

	public class DecimalColumnValueSetter : ColumnValueSetter
	{
		public DecimalColumnValueSetter(IColumnIndexer row, SchemaDecimalColumn column, Func<ZDecimal?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaDecimalColumn)column, (ZDecimal?)value, logger);
		}
	}

	public class GeographyColumnValueSetter : ColumnValueSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public GeographyColumnValueSetter(IColumnIndexer row, SchemaGeographyColumn column, Func<ZGeography?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaGeographyColumn)column, (ZGeography?)value, logger);
		}
	}

	public class GuidColumnValueSetter : ColumnValueSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public GuidColumnValueSetter(IColumnIndexer row, SchemaGuidColumn column, Func<ZGuid?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null, Func<SchemaColumn, string> getColumnName = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
			this.getColumnName = getColumnName;
		}
		readonly Func<SchemaColumn, string> getColumnName;

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaGuidColumn)column, (ZGuid?)value, logger, getColumnName);
		}
	}

	public class IntColumnValueSetter : ColumnValueSetter
	{
		public IntColumnValueSetter(IColumnIndexer row, SchemaIntColumn column, Func<ZInt?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaIntColumn)column, (ZInt?)value, logger);
		}
	}

	public class IntColumnWithLongValueSetter : ColumnValueSetter
	{
		public IntColumnWithLongValueSetter(IColumnIndexer row, SchemaIntColumn column, Func<ZLong?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaIntColumn)column, (ZLong?)value, logger);
		}
	}

	public class LongColumnValueSetter : ColumnValueSetter
	{
		public LongColumnValueSetter(IColumnIndexer row, SchemaLongColumn column, Func<ZLong?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaLongColumn)column, (ZLong?)value, logger);
		}
	}

	public class LongColumnWithIntValueSetter : ColumnValueSetter
	{
		public LongColumnWithIntValueSetter(IColumnIndexer row, SchemaLongColumn column, Func<ZInt?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaLongColumn)column, (ZInt?)value, logger);
		}
	}

	public class ShortColumnWithLongValueSetter : ColumnValueSetter
	{
		public ShortColumnWithLongValueSetter(IColumnIndexer row, SchemaShortColumn column, Func<ZLong?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaShortColumn)column, (ZLong?)value, logger);
		}
	}

	public class ShortColumnWithIntValueSetter : ColumnValueSetter
	{
		public ShortColumnWithIntValueSetter(IColumnIndexer row, SchemaShortColumn column, Func<ZInt?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaShortColumn)column, (ZInt?)value, logger);
		}
	}

	public class ShortColumnValueSetter : ColumnValueSetter
	{
		public ShortColumnValueSetter(IColumnIndexer row, SchemaShortColumn column, Func<ZShort?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaShortColumn)column, (ZShort?)value, logger);
		}
	}

	public class StringColumnValueSetter : ColumnValueSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public StringColumnValueSetter(IColumnIndexer row, SchemaStringColumn column, Func<ZString?> getValue, IXmlImportLogger logger, CharacterCase characterCase = CharacterCase.Normal, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
			this.characterCase = characterCase;
		}

		readonly CharacterCase characterCase;

		protected sealed override void SetValueCore()
		{
			var valueToSet = (ZString?)value;
			if (!string.IsNullOrEmpty(valueToSet) && characterCase != CharacterCase.Normal)
			{
				switch (characterCase)
				{
					case CharacterCase.Lower:
						valueToSet = valueToSet.Value.ToLower();
						break;
					case CharacterCase.Upper:
						valueToSet = valueToSet.Value.ToUpper();
						break;
					default:
						break;
				}
			}

			row.SetValue((SchemaStringColumn)column, valueToSet, logger);
		}
	}

	public class TimeColumnValueSetter : ColumnValueSetter
	{
		public TimeColumnValueSetter(IColumnIndexer row, SchemaTimeColumn column, Func<ZTime?> getValue, IXmlImportLogger logger, SchemaPKColumn rowPKSchema = null)
			: base(row, column, () => getValue(), logger, rowPKSchema)
		{
		}

		protected sealed override void SetValueCore()
		{
			row.SetValue((SchemaTimeColumn)column, (ZTime?)value, logger);
		}
	}
}
