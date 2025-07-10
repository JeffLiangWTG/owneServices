using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public sealed class JoinCondition : AbstractFilterPart, IFilterPart
	{
		public override int GetHashCode()
		{
			return Text.GetHashCode();
		}

		public static bool operator !=(JoinCondition lHS, JoinCondition rHS)
		{
			return !(lHS == rHS);
		}

		public static bool operator ==(JoinCondition lHS, JoinCondition rHS)
		{
			bool lHSIsNull = object.ReferenceEquals(lHS, null);
			bool rHSIsNull = object.ReferenceEquals(rHS, null);

			if (lHSIsNull)
			{
				return rHSIsNull;
			}
			else if (rHSIsNull)
			{
				return lHSIsNull;
			}
			else
			{
				return lHS.Text.Equals(rHS.Text);
			}
		}

		public override bool Equals(object obj)
		{
			JoinCondition otherCondition = obj as JoinCondition;
			return (otherCondition != null) && (Text == otherCondition.Text);
		}

		JoinCondition(string text)
		{
			this.Text = text;
		}

		public override string ToString()
		{
			return this.Text;
		}

		public readonly string Text;

		[SuppressThreadStaticFieldMessage]
		public static JoinCondition Or = new JoinCondition((NoResString)"or");
		[SuppressThreadStaticFieldMessage]
		public static JoinCondition And = new JoinCondition((NoResString)"and");
		[SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operator")]
		public static JoinCondition Union = new JoinCondition((NoResString)"union");
		[SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operator")]
		public static JoinCondition UnionAll = new JoinCondition((NoResString)"union all");

		#region IFilterPart Members

		void IFilterPart.DisableModifications()
		{
			// Object is immutable so no work to do
		}

		ZNonPersistentDataQuery IFilterPart.ParameterisedSql(ParameterNameFactory factory)
		{
			return new ZNonPersistentDataQuery(Text, ZSqlParameter.EmptyArray);
		}

		void IFilterPart.ParameterisedSql(SqlBuilder sqlBuilder)
		{
			sqlBuilder.Append(Text);
		}

		IEnumerable<SchemaColumn> IFilterPart.BlobFilters
		{
			get { return Enumerable.Empty<SchemaColumn>(); }
		}

		string IFilterPart.LiteralTextADO
		{
			get { return Text; }
		}

		void IFilterPart.AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			sqlBuilder.Append(Text);
		}

		bool IFilterPart.NeedsBrackets
		{
			get { return true; }
		}

		bool IFilterPart.FilterIsEmpty
		{
			get { return true; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get { return this == JoinCondition.Or; }
		}

		#endregion

		public IFilterPart DeepClone()
		{
			return new JoinCondition(this.Text);
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			return new IFilterPart[] { this };
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => Enumerable.Empty<IFilterPart>();
	}
}
