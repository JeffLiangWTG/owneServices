using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	public static class CodeDescriptionPairListExtensions
	{
		public static ZString ToCodeDescription<T>(this ZString code) where T : ICodeDescriptionPairList, new()
		{
			return ToCodeDescription<T>(code, code);
		}

		public static ZString ToCodeDescription<T>(this ZString codeToGetDescriptionFrom, string code) where T : ICodeDescriptionPairList, new()
		{
			return new T().GetCodeDescription(codeToGetDescriptionFrom, code);
		}

		public static ZString GetCodeDescription(this ICodeDescriptionPairList list, ZString code)
		{
			return GetCodeDescription(list, code, code);
		}

		public static ZString GetCodeDescription(this ICodeDescriptionPairList list, ZString codeToGetDescriptionFrom, string code)
		{
			var builder = new ZStringBuilder();
			if (!string.IsNullOrEmpty(code))
			{
				builder.Append(code);
				builder.AppendIfNotEmpty(list.GetDescriptionFromCode(codeToGetDescriptionFrom));
			}
			return builder.ToStringWithDelimiterBetweenAppends(" - ");
		}
	}
}
