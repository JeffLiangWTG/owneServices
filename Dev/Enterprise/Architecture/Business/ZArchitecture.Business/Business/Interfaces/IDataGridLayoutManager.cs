using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.Business
{
	public interface IDataGridLayoutManager
	{
		StmModuleFilter SavePreconfiguredLayout(IModifyModuleAndGridLayout layoutManageable, ZString layoutName, ZBool publish, ZBool publishGlobal, SaveColumnLayout saveColumnLayout, SaveGridColourLayout saveGridColourLayout = SaveGridColourLayout.No, bool isUserDefinedFilter = false);
	}
}
