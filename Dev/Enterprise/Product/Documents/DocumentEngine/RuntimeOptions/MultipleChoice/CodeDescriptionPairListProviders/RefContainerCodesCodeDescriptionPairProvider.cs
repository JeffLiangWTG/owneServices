using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class RefContainerCodesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			var containerTypes = new RefContainerCollection(factory).Where(x => (x.IsSeaContainer || x.IsRoadTruckContainer) && x.RC_TEU > 0);

			var result = new CodeDescriptionPairList();
			foreach (var containertype in containerTypes)
			{
				result.InsertInSortOrder(new CodeDescriptionPair(containertype.RC_Code.ToString(), containertype.RC_DescriptionMultilingual));
			}
			return result;
		}
	}
}
