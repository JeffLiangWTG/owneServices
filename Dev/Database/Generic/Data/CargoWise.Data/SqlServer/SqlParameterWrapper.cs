using System;
using System.Data;

namespace CargoWise.Data;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "We do not want simplify name here")]
public class SqlParameterWrapper : System.Data.Common.DbParameter
{
	readonly System.Data.Common.DbParameter @base;
	public System.Data.Common.DbParameter Base { get => @base; }
	public SqlParameterWrapper(System.Data.Common.DbParameter dbParameter)
	{
		if (dbParameter == null)
		{
			throw new ArgumentNullException(nameof(dbParameter));
		}
		else if (dbParameter is SqlParameterWrapper)
		{
			throw new ArgumentException("Cannot wrap a SqlParameterWrapper with another SqlParameterWrapper.", nameof(dbParameter));
		}

		this.@base = dbParameter;

		this.InvalidAsThisIsNotSqlParameter = IsInvalid();

		bool IsInvalid()
		{
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				return false;
			}
#if NET
			else if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				return false;
			}
#endif
			else
			{
				return true;
			}
		}
	}

	public SqlParameterWrapper(IDataParameter dataParameter)
		: this(dataParameter as System.Data.Common.DbParameter)
	{
	}

	/// <summary>
	/// Indicates whether the parameter type is a SqlParameter.
	/// </summary>
	public bool InvalidAsThisIsNotSqlParameter { get; }

	public override DbType DbType
	{
		get => @base.DbType;
		set => @base.DbType = value;
	}

	public override ParameterDirection Direction
	{
		get => @base.Direction;
		set => @base.Direction = value;
	}

	public override bool IsNullable
	{
		get => @base.IsNullable;
		set => @base.IsNullable = value;
	}

	public override string ParameterName
	{
		get => @base.ParameterName;
		set => @base.ParameterName = value;
	}

	public override int Size
	{
		get => @base.Size;
		set => @base.Size = value;
	}

	public override string SourceColumn
	{
		get => @base.SourceColumn;
		set => @base.SourceColumn = value;
	}

	public override bool SourceColumnNullMapping
	{
		get => @base.SourceColumnNullMapping;
		set => @base.SourceColumnNullMapping = value;
	}

	public override object Value
	{
		get => @base.Value;
		set => @base.Value = value;
	}

	public override void ResetDbType()
	{
		@base.ResetDbType();
	}

	public override byte Precision { get => @base.Precision; set => @base.Precision = value; }
	public override byte Scale { get => @base.Scale; set => @base.Scale = value; }
	public override DataRowVersion SourceVersion { get => @base.SourceVersion; set => @base.SourceVersion = value; }
	public override string ToString() => @base.ToString();

	public string UdtTypeName
	{
		get
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				return sqlParameterMS.UdtTypeName;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				return sqlParameter.UdtTypeName;
			}
			else
			{
				throw new DataException($"UdtTypeName is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}

		set
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				sqlParameterMS.UdtTypeName = value;
				return;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				sqlParameter.UdtTypeName = value;
				return;
			}
			else
			{
				throw new DataException($"UdtTypeName is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}
	}

	public System.Data.SqlDbType SqlDbType
	{
		get
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				return sqlParameterMS.SqlDbType;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				return sqlParameter.SqlDbType;
			}
			else
			{
				throw new DataException($"SqlDbType is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}
		set
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				sqlParameterMS.SqlDbType = value;
				return;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				sqlParameter.SqlDbType = value;
				return;
			}
			else
			{
				throw new DataException($"SqlDbType is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}
	}

	public string TypeName
	{
		get
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				return sqlParameterMS.TypeName;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				return sqlParameter.TypeName;
			}
			else
			{
				throw new DataException($"SqlDbType is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}
		set
		{
#if NET
			if (@base is Microsoft.Data.SqlClient.SqlParameter sqlParameterMS)
			{
				sqlParameterMS.TypeName = value;
				return;
			}
#endif
			if (@base is System.Data.SqlClient.SqlParameter sqlParameter)
			{
				sqlParameter.TypeName = value;
				return;
			}
			else
			{
				throw new DataException($"SqlDbType is not supported for this parameter type ({@base.GetType().FullName}).");
			}
		}
	}

	#region Overload the equality operator
	public static bool operator == (SqlParameterWrapper @this, object that)
	{
		if (that is null)
		{
			return @this?.InvalidAsThisIsNotSqlParameter == true;
		}

		return ReferenceEquals(@this, that) || ReferenceEquals(@this.@base, that);
	}

	public static bool operator != (SqlParameterWrapper @this, object that)
	{
		if (that is null)
		{
			if (@this == null)
			{
				return false;
			}

			return !@this.InvalidAsThisIsNotSqlParameter;
		}
		return !ReferenceEquals(@this, that) || !ReferenceEquals(@this.@base, that);
	}

	public override bool Equals(object that)
	{
		if (that is null)
		{
			return this.InvalidAsThisIsNotSqlParameter;
		}
		return ReferenceEquals(this, that) || ReferenceEquals(this.@base, that);
	}

	public override int GetHashCode()
	{
		if (this.InvalidAsThisIsNotSqlParameter)
		{
			return 0;
		}
		return @base.GetHashCode();
	}
	#endregion
}
