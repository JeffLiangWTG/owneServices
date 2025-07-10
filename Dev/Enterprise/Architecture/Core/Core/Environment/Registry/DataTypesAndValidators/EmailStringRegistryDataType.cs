using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailStringRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("e38548ec-46ca-445e-bf98-60549ae14688", "Please enter a valid email address. E.g. email_name@domain_name.com"));
			}
		}
	}
}
