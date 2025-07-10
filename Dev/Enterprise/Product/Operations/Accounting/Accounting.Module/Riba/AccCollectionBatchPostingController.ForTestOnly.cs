#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchPostingController
	{
		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}

		public SecurityCheckpoint CheckPointForNew_ForTestOnly => CheckPointForNew;
	}
}

#endif
