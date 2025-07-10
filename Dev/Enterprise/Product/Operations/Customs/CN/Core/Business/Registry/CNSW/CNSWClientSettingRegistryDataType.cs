using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business;

[RegistryEditor("Enterprise.Customs.CN.GUI.CNSWClientSettingRegistryItemEditor, Enterprise.Customs.CN.GUI")]
public class CNSWClientSettingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CNSWClientSetting>
{
	protected override void ValidateCore(IRegistryItem registryItem, CNSWClientSetting proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		ValidateMachineName(registryItem as IRegistryItemInternals, proposedValue, companyPK, branchPK);
	}

	void ValidateMachineName(IRegistryItemInternals registryItem, CNSWClientSetting proposedValue, Guid currentCompanyPK, Guid currentBranchPK)
	{
		if (!proposedValue.MachineName.IsEmpty && (currentCompanyPK != Guid.Empty || currentBranchPK != Guid.Empty))
		{
			var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.China, RegistryFactory.Instance);

			bool DuplicateFoundOnCompanies() => companies.Select(company => company.PK.ToGuid())
				.Where(companyPK => currentBranchPK != Guid.Empty || companyPK != currentCompanyPK)
				.Any(companyPK => DuplicateMachineNameFound(companyPK, Guid.Empty));

			bool DuplicateFoundOnBranches() => companies.SelectMany(company => company.Branches).Select(branch => branch.PK.ToGuid())
				.Where(branchPK => branchPK != currentBranchPK)
				.Any(branchPK => DuplicateMachineNameFound(Guid.Empty, branchPK));

			if (DuplicateFoundOnCompanies() || DuplicateFoundOnBranches())
			{
				throw new RegistryValidationException(Res.GetString("EC32C475-BBFB-4670-9AFB-95609E79A6E9", "There is already a company or branch which has a same Machine Name."));
			}
		}

		bool DuplicateMachineNameFound(Guid companyPK, Guid branchPK)
		{
			var machineName = (registryItem.GetCurrentValueFromProposedValueAccessor(companyPK, branchPK, Guid.Empty) as CNSWClientSetting).MachineName;
			return proposedValue.MachineName == machineName;
		}
	}
}
