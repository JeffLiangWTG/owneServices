#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class TransferController
	{
		public IBusiness GetTopLevelBusinessObject_ForTestOnly(IBusiness sourceEntity)
		{
			return GetTopLevelBusinessObject(sourceEntity);
		}
	}
}

#endif