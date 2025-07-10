using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public interface ISupportSubAccount
	{
		ZString SubAccountTypeParentTableCode { get; set; }
		ZGuid SubAccountParentId { get; set; }
		ZPropertyInfo SubAccountParentIdInfo { get; }
		ZString SubAccountTypeDisplayCode { get; set; }
		ZPropertyInfo SubAccountTypeDisplayCodeInfo { get; }
		ISupportMultiSubAccounts SubAccountParent { get; }
	}
}
