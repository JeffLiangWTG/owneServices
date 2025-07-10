using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplicationPermission.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.EdiIdentityApplicationPermission
{
	[TestedType(typeof(EdiIdentityApplicationPermissionCollection))]
	public class EdiIdentityApplicationPermissionCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiIdentityApplicationPermissionCollection>
	{
	}
}
