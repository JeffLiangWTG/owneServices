using System.Collections.Generic;

namespace CargoWise.Integration
{
	public interface IDomainCredentialsProvider
	{
		IEnumerable<IDomainCredentials> DomainCredentialsCollection
		{
			get;
		}
	}
}
