using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public static class ZStringBuilderExtensions
	{
		public static ZStringBuilder AppendIfBuilderIsNotEmpty(this ZStringBuilder sb, string value)
		{
			Argument.NotNull(sb, nameof(sb));

			if (!sb.IsEmpty)
			{
				sb.Append(value);
			}
			return sb;
		}

		public static ZStringBuilder AppendIfBuilderIsEmpty(this ZStringBuilder sb, string value)
		{
			Argument.NotNull(sb, nameof(sb));
			if (sb.IsEmpty)
			{
				sb.Append(value);
			}
			return sb;
		}
	}
}
