using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface IOrganizationAddressCollectionParent
	{
		List<OrganizationAddress> OrganizationAddressCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetOrganizationAddressCollection(Func<List<OrganizationAddress>> getter);
	}
}
