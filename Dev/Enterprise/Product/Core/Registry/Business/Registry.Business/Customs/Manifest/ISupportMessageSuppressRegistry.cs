using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public interface ISupportMessageSuppressRegistry
	{
		ZBool ShouldSEndErrorsOnly(ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK);
	}
}