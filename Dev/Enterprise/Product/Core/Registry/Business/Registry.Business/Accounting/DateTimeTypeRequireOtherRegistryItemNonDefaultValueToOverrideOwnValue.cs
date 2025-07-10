using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue : DateTimeRegistryDataType
	{
		public DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue(IRegistryItem relatedRegistryItem)
		{
			RelatedRegistryItem = Argument.NotNull(relatedRegistryItem, nameof(relatedRegistryItem));
		}

		readonly IRegistryItem RelatedRegistryItem;

		[SuppressMessage("CargoWiseOne", "CW1048")]
		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != (DateTime)registryItem.DefaultValue)
			{
				var relatedRegistryItemValue = RelatedRegistryItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK) as IComparable;
				var relatedRegistryItemDefaultValue = RelatedRegistryItem.DefaultValue as IComparable;
				if (relatedRegistryItemValue?.Equals(relatedRegistryItemDefaultValue) ?? relatedRegistryItemDefaultValue == null)
				{
					throw new RegistryValidationException(Res.GetString("cfa5c0bf-a888-4403-b42b-7b0bdbe37eda", "This '{0}' registry item value can be overridden only after the '{1}' registry item value overridden at the same fallback level.",
						registryItem.Caption, RelatedRegistryItem.Caption));
				}
			}
		}
	}
}
