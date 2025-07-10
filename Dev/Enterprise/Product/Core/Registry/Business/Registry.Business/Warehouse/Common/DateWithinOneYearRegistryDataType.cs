using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class DateWithinOneYearRegistryDataType : DateTimeRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue > ZDateTime.Now.AddYears(1))
			{
				throw new RegistryValidationException(Res.GetString("bd67a035-5252-4138-ad8a-3a6892575b0f", "You need to enter a date within 12 months", registryItem));
			}

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
