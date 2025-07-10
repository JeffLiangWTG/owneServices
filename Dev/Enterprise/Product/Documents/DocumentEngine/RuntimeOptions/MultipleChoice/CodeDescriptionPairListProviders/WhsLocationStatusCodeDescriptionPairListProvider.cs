using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WhsLocationStatusCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (whsLocationStatusPairList == null)
			{
				var list = (CodeDescriptionPairList)ObjectFactory.Get<ILocationStatus>();
				list.RemoveCode("VOI");
				whsLocationStatusPairList = list;
			}
			return whsLocationStatusPairList;
		}

		ReadOnlyCodeDescriptionPairList whsLocationStatusPairList;
	}
}
