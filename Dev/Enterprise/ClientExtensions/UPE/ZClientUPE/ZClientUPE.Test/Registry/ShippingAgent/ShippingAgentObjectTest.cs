using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.Business.Testing
{
	[TestedType(typeof(ShippingAgentObject))]
	public class ShippingAgentObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestWrapperIsRegistedEditableChild()
		{
			var shippingAgentObject = (ShippingAgentObject)GetNewBusinessObject();
			Assert(shippingAgentObject.IsRegisteredEditableChildObject(shippingAgentObject.Wrapper));
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected override BusinessObject GetNewBusinessObject() => new ShippingAgentObject();
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => (ShippingAgentObject)GetNewBusinessObject();
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => (ShippingAgentObject)GetNewBusinessObject();
	}
}
