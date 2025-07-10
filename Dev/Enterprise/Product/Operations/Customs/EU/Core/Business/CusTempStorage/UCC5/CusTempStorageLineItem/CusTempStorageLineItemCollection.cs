using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineItemCollection<TCusTempStorageLineItem> : ActiveBusinessObjectCollection<TCusTempStorageLineItem>,
		ICusTempStorageLineItemCollection<TCusTempStorageLineItem>
		where TCusTempStorageLineItem : CusTempStorageLineItem
	{
		public CusTempStorageLineItemCollection(CusTempStorageLine line)
			: base(line)
		{
		}
	}
}
