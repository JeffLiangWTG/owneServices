using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public static class BRPairListHelper
	{
		public static ZString GetDescriptionOfPairListInBR(CodeDescriptionPairList codeDescriptionPairList, ZString code)
		{
			if (!code.IsEmpty)
			{
				return codeDescriptionPairList.GetMultilingualDescriptionFromCode(code)?.ToString(Core.Constants.Languages.PortugueseBrazil);
			}
			return ZString.Empty;
		}

		public static CodeAndDescriptionWrapper CreateCodeAndDescriptionWrapper(CodeDescriptionPairList codeDescriptionPairList, ZString code, BusinessObjectFactory factory)
		{
			return new CodeAndDescriptionWrapper(code, GetDescriptionOfPairListInBR(codeDescriptionPairList, code), factory);
		}
	}
}
