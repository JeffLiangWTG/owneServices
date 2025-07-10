using System;
using System.Linq;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	internal class RegionNumberFormatStringRegistryDataType : StringRegistryDataType
	{
		public RegionNumberFormatStringRegistryDataType(Func<StringRegistryItem> conflictingRegistryItem) : base() 
		{
			this.conflictingRegistryItem = conflictingRegistryItem;
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var registryItemWithOtherChangedItems = registryItem as IRegistryItemWithOtherChangedItems;

			if (registryItemWithOtherChangedItems != null && IsRegistryItemValueConflicting(registryItemWithOtherChangedItems, proposedValue, companyPK, branchPK, departmentPK))
			{
				throw new RegistryValidationException(Res.GetString("FC24B74F-2437-4E7A-ADD4-3DE4AEB5F85E", "The {0} cannot be set as the same value as {1}.", registryItem.Caption, conflictingRegistryItem().Caption));
			}
		}

		bool IsRegistryItemValueConflicting(IRegistryItemWithOtherChangedItems currentRegistryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var conflictingChangedItem = currentRegistryItem.OtherChangedItems?.FirstOrDefault(otherChangedItem => otherChangedItem.Name.Equals(conflictingRegistryItem().Name));
			// If ChangedItem is of belong to other company, GetProposedValue() will return null.
			// RegistryItem should be obtained from GetValueWithoutFallback().
			string conflictValue = (string)(conflictingChangedItem?.GetProposedValue(companyPK, branchPK, departmentPK))
				?? conflictingRegistryItem().GetValueWithoutFallback(companyPK, branchPK, departmentPK);

			return conflictValue.Equals(proposedValue);
		}

		readonly Func<StringRegistryItem> conflictingRegistryItem;

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
