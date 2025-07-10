using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.GSSI.Testing
{
	[TestedType(typeof(GSSMessage))]
	public class GSSMessageTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
