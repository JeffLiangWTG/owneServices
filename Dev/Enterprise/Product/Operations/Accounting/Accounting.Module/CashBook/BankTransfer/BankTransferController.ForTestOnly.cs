#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class BankTransferController
	{
		public IBusiness GetTopLevelBusinessObject_ForTestOnly(IBusiness sourceEntity)
		{
			return GetTopLevelBusinessObject(sourceEntity);
		}
	}
}

#endif