namespace Enterprise.Customs.JP.Common
{
	public static class ValueTypeHandler
	{
		public static bool IsAn(this char @this, bool isIVA = false, bool isInBound = false)
		{
			return
				char.IsDigit(@this) ||
				char.IsUpper(@this) ||
				@this == '\r' ||
				@this == '\n' ||
				@this == ' ' ||
				@this == '!' ||
				@this == '"' ||
				@this == '#' ||
				(@this == '$' && isInBound) ||
				@this == '%' ||
				@this == '&' ||
				@this == '\'' ||
				@this == '(' ||
				@this == ')' ||
				@this == '*' ||
				@this == '+' ||
				@this == ',' ||
				@this == '-' ||
				@this == '.' ||
				@this == '/' ||
				@this == ':' ||
				@this == ';' ||
				@this == '<' ||
				@this == '=' ||
				@this == '>' ||
				@this == '?' ||
				@this == '@' ||
				(@this == '[' && isInBound) ||
				(@this == '¥' && (isIVA || isInBound)) ||
				(@this == ']' && isInBound) ||
				(@this == '^' && isInBound) ||
				(@this == '_' && isInBound);
		}

		public static bool IsSn(this char @this)
		{
			return
				char.IsDigit(@this) ||
				char.IsUpper(@this) ||
				char.IsLower(@this) ||
				@this == '\r' ||
				@this == '\n' ||
				@this == ' ' ||
				@this == '!' ||
				@this == '"' ||
				@this == '#' ||
				@this == '$' ||
				@this == '%' ||
				@this == '&' ||
				@this == '\'' ||
				@this == '(' ||
				@this == ')' ||
				@this == '*' ||
				@this == '+' ||
				@this == ',' ||
				@this == '-' ||
				@this == '.' ||
				@this == '/' ||
				@this == ':' ||
				@this == ';' ||
				@this == '<' ||
				@this == '=' ||
				@this == '>' ||
				@this == '?' ||
				@this == '@' ||
				@this == '[' ||
				@this == '¥' ||
				@this == ']' ||
				@this == '^' ||
				@this == '`' ||
				@this == '_' ||
				@this == '{' ||
				@this == '|' ||
				@this == '}' ||
				@this == '~';
		}

		public static bool IsJ(this char @this)
		{
			return IsSn(@this) || JisUtility.IsJISX0208(@this);
		}
	}
}
