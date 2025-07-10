using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.IdentityApplication
{
	[TestedType(typeof(EdiIdentityApplicationCollection))]
	internal class EdiIdentityApplicationCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiIdentityApplicationCollection>
	{
	}
}
