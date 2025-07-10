using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.eRouter.Business.Test
{
	[TestedType(typeof(eRouterEdiEnterpriseCommunication))]
	public class eRouterEdiEnterpriseCommunicationTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}
	}
}
