using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public static class CodeDescriptionExtensions
	{
		public static MultilingualString GetMultilingualCode(this ICodeDescription codeDescription)
		{
			var codeDescriptionPair = codeDescription as CodeDescriptionPair;
			return codeDescriptionPair != null ? codeDescriptionPair.MultilingualCode : (NoResString)codeDescription.Code;
		}

		public static MultilingualString GetMultilingualDescription(this ICodeDescription codeDescription)
		{
			var codeDescriptionPair = codeDescription as CodeDescriptionPair;
			return codeDescriptionPair != null ? codeDescriptionPair.MultilingualDescription : (NoResString)codeDescription.Description;
		}
	}
}
