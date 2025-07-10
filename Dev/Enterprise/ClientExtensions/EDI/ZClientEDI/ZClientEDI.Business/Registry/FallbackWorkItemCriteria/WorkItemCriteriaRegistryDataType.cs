using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Registry
{
	public class WorkItemCriteriaRegistryDataType : StringRegistryDataType
	{
		protected override bool AllowNullCore => true;

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.IsNullOrEmpty())
			{
				return;
			}

			var split = proposedValue.Split('/');
			if (split.Length != 3 || ExistInvalidCode(split))
			{
				throw new RegistryValidationException("Please enter a valid work item criteria. Must be in the format 'ABC/DEF/HIJ'.");
			}
		}

		bool ExistInvalidCode(string[] items)
		{
			return items.Any(item => item.IsNullOrEmpty() || item.Length > 3 || item.ToUpper() != item);
		}
	}
}
