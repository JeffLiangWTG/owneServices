using System.Text;

namespace CargoWise.EntityFramework
{
	public sealed class LikeComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operator literals")]
		public LikeComparisonOperator() : base("like", "", "") { }

		protected override string EscapedADOValue(string value)
		{
			return value;
		}

		protected override object ValueForLiteralADOCore(object value)
		{
			string str = base.ValueForLiteralADOCore(value).ToString();
			StringBuilder sb = new StringBuilder(str.Length);
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				switch (c)
				{
					case ']':
					case '[':
					case '*':
						sb.Append("[").Append(c).Append("]");
						break;
					default:
						sb.Append(c);
						break;
				}
			}
			return sb.ToString();
		}
	}
}
