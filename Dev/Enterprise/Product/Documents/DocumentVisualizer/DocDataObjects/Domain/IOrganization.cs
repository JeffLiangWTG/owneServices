using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IOrganization
	{
		ZString Code { get; }
		ZString Name { get; }
		ZGuid PK { get; }
		IUnloco Unloco { get; }
		IAddress MainAddress { get; }
		IReadOnlyCollection<IRegistrationNumber> RegistrationNumbers { get; }
	}
}
