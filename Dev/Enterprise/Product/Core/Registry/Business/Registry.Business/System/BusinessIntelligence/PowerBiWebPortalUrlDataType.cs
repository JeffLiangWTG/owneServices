using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class PowerBiWebPortalUrlDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue = RemoveSlash(proposedValue);
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			CannotSetThisItemWhenPrerequisiteHasNotBeenSet(proposedValue, companyPK, branchPK, departmentPK);
		}

		void CannotSetThisItemWhenPrerequisiteHasNotBeenSet(string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!string.IsNullOrWhiteSpace(proposedValue) && !SystemDataRegistry.Instance.BiAnalysisServer.HasBeenSetOrChanged(companyPK, branchPK, departmentPK))
			{
				throw new RegistryValidationException(Res.GetString("AEAD7DBE-37B4-4D1B-8B73-EE681A76B7CB", "\"Power BI Web Portal URL\" cannot be set when \"Analysis Server\" has not been set."));
			}
		}

		public static string RemoveSlash(string url)
		{
			return url.TrimEnd(System.IO.Path.AltDirectorySeparatorChar);
		}
	}
}
