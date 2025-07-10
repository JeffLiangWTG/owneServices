
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CartageDropModeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return DropModeList;
		}

		CodeDescriptionPairList DropModeList
		{
			get
			{
				if (dropModeList == null)
				{
					dropModeList = new LCLAIREquipmentNeededList();
					dropModeList.AddRangeOverwriteIfExists(new FCLEquipmentNeededList());
				}
				return dropModeList;
			}
		}
		CodeDescriptionPairList dropModeList;
	}
}
