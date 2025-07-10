using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public static class CodeDescriptionPairListExtension
	{
		public static CodeDescriptionPairList FilterListByCodes(this ICodeDescriptionPairList fullList, IEnumerable<string> codes)
		{
			var result = new CodeDescriptionPairList();
			codes.Where(fullList.ContainsCode).ForEach(code =>
			{
				result.AddPair(code, fullList.GetDescriptionFromCode(code));
			});
			result.Sort();
			return result;
		}
	}
}
