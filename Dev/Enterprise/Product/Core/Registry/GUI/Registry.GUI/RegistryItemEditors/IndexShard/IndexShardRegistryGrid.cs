using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	class IndexShardRegistryGrid : ZGrid
	{
		protected override bool CanDeleteCurrentRow(int currentRowIndex)
		{
			return currentRowIndex != -1 && !((IndexShard)List[currentRowIndex]).IndexTableName_ReadOnly;
		}
	}
}
