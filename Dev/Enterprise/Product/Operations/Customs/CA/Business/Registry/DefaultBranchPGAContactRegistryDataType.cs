using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Registry;

public class DefaultBranchPGAContactRegistryDataType : GuidRegistryDataType
{
	protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

		var staff = new BusinessObjectFactory().Load<GlbStaff>(proposedValue);

		if (staff != null)
		{
			var messageError = ContactNameValidataionHelper.ValidationContactName(staff.GS_FullName, true);
			if (!messageError.IsEmpty)
			{
				throw new RegistryValidationException(messageError);
			}
		}
	}
}
