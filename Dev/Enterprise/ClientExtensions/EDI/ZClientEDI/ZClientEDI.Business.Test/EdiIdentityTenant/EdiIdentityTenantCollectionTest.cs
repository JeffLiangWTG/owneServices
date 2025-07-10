using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Business.Testing
{
	[TestedType(typeof(EdiIdentityTenantCollection))]
	internal class EdiIdentityTenantCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiIdentityTenantCollection>
	{
	}
}
