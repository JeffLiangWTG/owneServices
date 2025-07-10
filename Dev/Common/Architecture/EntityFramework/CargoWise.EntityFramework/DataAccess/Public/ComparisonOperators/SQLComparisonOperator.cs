using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	[TypeConverter(typeof(SQLComparisonOperatorTypeConverter))]
	public abstract partial class SQLComparisonOperator : IXmlSerializable
	{
		protected SQLComparisonOperator()
		{
		}

		internal SQLComparisonOperator(string comparisonText, bool supportsNullComparison, string prefix, string suffix)
		{
			this.comparisonText = comparisonText;
			this.SupportsNullComparison = supportsNullComparison;
			this.prefix = prefix;
			this.suffix = suffix;
		}

		#region Operators

		public static SQLComparisonOperator Equal { get; } = new EqualComparisonOperator();

		public static SQLComparisonOperator NotEqual { get; } = new NotEqualComparisonOperator();

		public static SQLComparisonOperator GreaterThan { get; } = new GreaterThanComparisonOperator();

		public static SQLComparisonOperator LessThan { get; } = new LessThanComparisonOperator();

		public static SQLComparisonOperator GreaterThanOrEqualTo { get; } = new GreaterThanOrEqualToComparisonOperator();

		public static SQLComparisonOperator LessThanOrEqualTo { get; } = new LessThanOrEqualToComparisonOperator();

		public static SQLComparisonOperator EqualToDatePartOnly { get; } = new EqualToDatePartOnlyComparisonOperator();

		public static SQLComparisonOperator LessThanOrEqualToDatePartOnly { get; } = new LessThanOrEqualToDatePartComparisonOperator();

		public static SQLComparisonOperator GreaterThanOrEqualToDatePartOnly { get; } = new GreaterThanOrEqualToDatePartComparisonOperator();

		public static SQLComparisonOperator StartsWith { get; } = new StartsWithComparisonOperator();

		public static SQLComparisonOperator EndsWith { get; } = new EndsWithComparisonOperator();

		public static SQLComparisonOperator Contains { get; } = new ContainsComparisonOperator();

		public static SQLComparisonOperator Like { get; } = new LikeComparisonOperator();

		public static SQLComparisonOperator NotSpecified { get; } = new NotSpecifiedComparisonOperator();

		public static SQLComparisonOperator NotContains { get; } = new NotContainsComparisonOperator();

		public static SQLComparisonOperator DoesNotStartWith { get; } = new DoesNotStartWithComparisonOperator();

		public static SQLComparisonOperator DoesNotEndWith { get; } = new DoesNotEndWithComparisonOperator();

		public static SQLComparisonOperator IsBlank { get; } = new IsBlankComparisonOperator();

		public static SQLComparisonOperator IsNotBlank { get; } = new IsNotBlankComparisonOperator();

		#endregion

		#region Operator Overloading (Language)

		public static bool operator ==(SQLComparisonOperator op1, SQLComparisonOperator op2)
		{
			if (ReferenceEquals(op1, null) || ReferenceEquals(op2, null))
			{
				return ReferenceEquals(op1, op2);
			}

			return op1.GetType() == op2.GetType();
		}

		public static bool operator !=(SQLComparisonOperator op1, SQLComparisonOperator op2)
		{
			return !(op1 == op2);
		}

		#endregion

		#region HashStaticPropertyNames

		Hashtable StaticPropertyNames
		{
			get
			{
				if (staticPropertyNames == null)
				{
					staticPropertyNames = new Hashtable();
					foreach (PropertyInfo info in typeof(SQLComparisonOperator).GetProperties(BindingFlags.Public | BindingFlags.Static))
					{
						object value = info.GetValue(null, null);
						if (value != null && value.GetType().IsSubclassOf(typeof(SQLComparisonOperator)) && staticPropertyNames[value.GetType().Name] == null)
						{
							staticPropertyNames.Add(value.GetType().Name, info);
						}
					}
				}
				return staticPropertyNames;
			}
		}
		[ThreadStatic]
		static Hashtable staticPropertyNames;

		#endregion

		public InstanceDescriptor InstanceDescriptor
		{
			get { return MemberInfo != null ? new InstanceDescriptor(MemberInfo, Array.Empty<object>()) : null; }
		}

		public override string ToString()
		{
			return MemberInfo != null ? MemberInfo.Name : GetType().ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj is SQLComparisonOperator)
			{
				return (this == (SQLComparisonOperator)obj);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return MemberInfo != null ? MemberInfo.GetHashCode() : GetType().Name.GetHashCode();
		}

		MemberInfo MemberInfo
		{
			get { return (MemberInfo)StaticPropertyNames[GetType().Name]; }
		}

		protected virtual string EscapedADOValue(string value)
		{
			return value;
		}

		public static bool ValueIsNull(object value)
		{
			bool result = value == null || value == DBNull.Value;
			if (!result)
			{
				if (value is ZDateTime)
				{
					result = ((ZDateTime)value).IsEmpty;
				}
				else if (value is ZDate)
				{
					result = ((ZDate)value).IsEmpty;
				}
				else if (value is ZDateTimeOffset)
				{
					result = ((ZDateTimeOffset)value).IsEmpty;
				}
				else if (value is ZTime)
				{
					result = ((ZTime)value).IsEmpty;
				}
			}
			return result;
		}

		/// <summary>
		/// Returns the value modified in respect of the current comparison operator.
		/// This will adjust date values for DatePart comparison operators
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public object ValueAdjustedForComparisonOperator(object value)
		{
			return ValueAdjustedForComparisonOperatorCore(value);
		}

		protected virtual object ValueAdjustedForComparisonOperatorCore(object value)
		{
			return value;
		}

		/// <summary>
		/// Generates the escape clause required for escaping Like statements in SQL
		/// </summary>
		public string SqlEscapeClause
		{
			get { return SqlEscapeClauseCore; }
		}

		public string SqlEscapeCharacter
		{
			get { return SqlEscapeCharacterCore; }
		}

		protected virtual string SqlEscapeCharacterCore
		{
			get { return ""; }
		}

		protected virtual string SqlEscapeClauseCore
		{
			get { return ""; }
		}

		public object EscapedSqlValue(object value)
		{
			return EscapedSqlValueCore(value);
		}

		protected virtual object EscapedSqlValueCore(object value)
		{
			return ValueAdjustedForComparisonOperator(value);
		}

		public object ValueForLiteralADO(object value)
		{
			return ValueForLiteralADOCore(value);
		}

		protected virtual object ValueForLiteralADOCore(object value)
		{
			object result = ValueAdjustedForComparisonOperator(value);
			if ((prefix != null && prefix.Length > 0) || (suffix != null && suffix.Length > 0))
			{
				result = prefix + EscapedADOValue(result.ToString()) + suffix;
			}
			return result;
		}

		protected virtual string ComparisonTextCore(object value)
		{
			return comparisonText;
		}

		public void CheckArgumentIsValid(object value)
		{
			this.CheckArgumentIsValid((NoResString)"Value", value);
		}

		public void CheckArgumentIsValid(string valueName, object value)
		{
			if (!SupportsNullComparison && ValueIsNull(value))
			{
				throw new ArgumentOutOfRangeException(valueName, "This comparison operator does not support null");
			}
		}

		public string ComparisonText(object value)
		{
			CheckArgumentIsValid(value);

			return ComparisonTextCore(value);
		}

		public static bool ReverseOperatorIfItIsNotInOperator(ref SQLComparisonOperator comparisonOperator)
		{
			if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				comparisonOperator = SQLComparisonOperator.StartsWith;
				return true;
			}

			if (comparisonOperator == SQLComparisonOperator.DoesNotEndWith)
			{
				comparisonOperator = SQLComparisonOperator.EndsWith;
				return true;
			}

			if (comparisonOperator == SQLComparisonOperator.NotContains)
			{
				comparisonOperator = SQLComparisonOperator.Contains;
				return true;
			}

			if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				comparisonOperator = SQLComparisonOperator.Equal;
				return true;
			}

			return false;
		}

		protected internal readonly string comparisonText;
		protected internal readonly string prefix;
		protected internal readonly string suffix;

		public abstract bool IsLike { get; }
		public readonly bool SupportsNullComparison;

		public virtual Func<T, bool> GetPredicate<T>(T value)
		{
			throw new NotImplementedException();
		}

		protected IComparable AsIComparable(object value)
		{
			return value as IComparable ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type {0} does not implement IComparable", value.GetType()));
		}

		#region IXmlSerializable Members

		public void WriteXml(XmlWriter writer)
		{
			//no state to the object but its type
		}

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			//no state to the object but its type
		}

		#endregion
	}
}
