using System;
using System.Data;
using System.Data.Common;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	public sealed class HttpDbParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter
	{
		public const int EmptySqlDbType = -1;

		public override DbType DbType { get; set; }

		public int SqlDbType { get; set; } = EmptySqlDbType;

		public string TypeName { get; set; }

		public string UdtTypeName { get; set; }

		public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
		public override bool IsNullable { get; set; }
		public override string ParameterName { get; set; }

		int size;
		public override int Size
		{
			get
			{
				int num = size;
				if (num == 0)
				{
					num = ValueSize(Value);
				}
				return num;
			}
			set
			{
				if (size != value)
				{
					size = value;
				}
			}
		}

		public override string SourceColumn { get; set; }
		public override bool SourceColumnNullMapping { get; set; }
		public override object Value { get; set; }

		int ValueSize(object value)
		{
			return ValueSizeCore(value);
		}

		static bool IsNull(object value)
		{
			if (value == null || DBNull.Value == value)
			{
				return true;
			}
			INullable nullable = value as INullable;
			if (nullable == null)
			{
				return false;
			}
			return nullable.IsNull;
		}

		int ValueSizeCore(object value)
		{
			if (!IsNull(value))
			{
				if (value is string str)
				{
					return str.Length;
				}

				if (value is byte[] numArray)
				{
					return numArray.Length;
				}

				if (value is char[] chrArray)
				{
					return chrArray.Length;
				}

				if (value is byte or char)
				{
					return 1;
				}
			}
			return 0;
		}

		object ICloneable.Clone()
		{
			return new HttpDbParameter(this);
		}

		public HttpDbParameter()
		{
		}

		HttpDbParameter(HttpDbParameter source) : this()
		{
			Argument.NotNull(source, nameof(source));
			source.CopyTo(this);

			if (Value is ICloneable cloneable)
			{
				Value = cloneable.Clone();
			}
		}

		void CopyTo(HttpDbParameter destination)
		{
			Argument.NotNull(destination, nameof(destination));
			CopyToCore(destination);
			destination.ParameterName = ParameterName;
			destination.Precision = Precision;
			destination.Scale = Scale;
		}

		void CopyToCore(HttpDbParameter destination)
		{
			Argument.NotNull(destination, nameof(destination));
			destination.Value = Value;
			destination.Direction = Direction;
			destination.Size = Size;
			destination.SourceColumn = SourceColumn;
			destination.SourceVersion = SourceVersion;
			destination.SourceColumnNullMapping = SourceColumnNullMapping;
			destination.IsNullable = IsNullable;
			destination.SqlDbType = SqlDbType;
			destination.TypeName = TypeName;
			destination.UdtTypeName = UdtTypeName;
		}

		public override void ResetDbType()
		{
		}

		public SqlParameterDTO ToSqlParameterDTO()
		{
			return SqlParameterDTO.FromDbParameter(this);
		}
	}
}
