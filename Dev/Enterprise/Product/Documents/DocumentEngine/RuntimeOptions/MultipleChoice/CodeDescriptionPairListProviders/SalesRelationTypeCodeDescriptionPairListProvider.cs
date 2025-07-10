using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class SalesRelationTypeCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ANY", ResString.GetMultilingualString("4fe191f6-451c-4264-8596-773cc9b79cf2", "Any Sales Relation"));
			list.AddRange(SalesRelationTypeList.New());

			return list;
		}
	}
}
