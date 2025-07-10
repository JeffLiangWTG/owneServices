using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class InexactComparisonOperator : SQLComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public InexactComparisonOperator(string comparisonText, string prefix, string suffix) : base(comparisonText, false, prefix, suffix)
		{
		}

		/// <summary>
		/// See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003APR.1033/cpref/html/frlrfsystemdatadatacolumnclassexpressiontopic.htm
		/// for more information.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Escaped value</returns>
		protected override string EscapedADOValue(string value)
		{
			string result = value.Replace("[", "[[]");
			result = result.Replace("%", "[%]");
			result = result.Replace("*", "[*]");
			return result;
		}

		protected override object EscapedSqlValueCore(object value)
		{
			value = base.EscapedSqlValueCore(value);
			if ((prefix != null && prefix.Length > 0) || (suffix != null && suffix.Length > 0))
			{
				string sqlEscapeCharacter = this.SqlEscapeCharacter;    // Caching
				string escapedValue = value.ToString().Replace(sqlEscapeCharacter, sqlEscapeCharacter + sqlEscapeCharacter);
				escapedValue = escapedValue.Replace("%", sqlEscapeCharacter + "%");
				escapedValue = escapedValue.Replace("[", sqlEscapeCharacter + "[");
				escapedValue = escapedValue.Replace("]", sqlEscapeCharacter + "]");
				escapedValue = escapedValue.Replace("_", sqlEscapeCharacter + "_");
				value = prefix + escapedValue + suffix;
			}
			return value;
		}

		public override bool IsLike
		{
			get { return true; }
		}

		protected override string SqlEscapeCharacterCore
		{
			get { return "~"; }
		}

		protected override string SqlEscapeClauseCore
		{
			get { return (NoResString)"escape '" + SqlEscapeCharacter + (NoResString)"'"; }
		}
	}
}
