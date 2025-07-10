using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class AnalysisServerDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			CannotSetThisItemWhenPrerequisiteHasNotBeenSet(proposedValue, companyPK, branchPK, departmentPK);
			CannotRemoveThisItemWhenPostrequisiteHasValue(proposedValue, companyPK, branchPK, departmentPK);
		}

		void CannotSetThisItemWhenPrerequisiteHasNotBeenSet(string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var prereqHasValue = SystemDataRegistry.Instance.BiReportUserCredential.HasBeenSetOrChanged(companyPK, branchPK, departmentPK);
			if (!string.IsNullOrWhiteSpace(proposedValue) && !prereqHasValue)
			{
				throw new RegistryValidationException(Res.GetString("8AAC3EFB-F8E3-4E4E-BD5B-EB21317ED7BE", "\"Analysis Server\" cannot be set if \"Report User Credentials\" hasn't been set."));
			}
		}

		void CannotRemoveThisItemWhenPostrequisiteHasValue(string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var postrequisiteIsEmpty = SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.IsEmpty(companyPK, branchPK, departmentPK);
			if (string.IsNullOrWhiteSpace(proposedValue) && !postrequisiteIsEmpty)
			{
				throw new RegistryValidationException(Res.GetString("29F9E1D6-30EA-4543-BC74-CD0FB6560AA8", "\"Analysis Server\" value cannot be removed when \"Power Bi Web Portal URL\" has a value specified."));
			}
		}
	}
}
