using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class DataWarehouseServerDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			CannotRemoveThisItemWhenPostrequisiteHasValue(proposedValue, companyPK, branchPK, departmentPK);
		}

		void CannotRemoveThisItemWhenPostrequisiteHasValue(string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (string.IsNullOrWhiteSpace(proposedValue) && !SystemDataRegistry.Instance.BiReportUserCredential.IsEmpty(companyPK, branchPK, departmentPK))
			{
				throw new RegistryValidationException(Res.GetString("3916E3FF-86A6-4732-9966-C7F2640A1D06", "\"Data Warehouse Server\" value cannot be removed when \"Report User Credentials\" has a value specified."));
			}
		}
	}
}
