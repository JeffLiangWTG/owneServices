using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingPortsMessagingEHubIDCollection))]
	public class ShippingPortsMessagingEHubIDCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ShippingPortsMessagingEHubIDCollection>
	{
		public void TestNewWithDefaultValues()
		{
			var shippingPortsMessagingEHubIDCollection = ShippingPortsMessagingEHubIDCollection.NewWithDefaultValues().Cast<ShippingPortsMessagingEHubID>();

			AssertEquals(5, shippingPortsMessagingEHubIDCollection.Count());
			AssertNotNull(shippingPortsMessagingEHubIDCollection.FirstOrDefault(x => x.Port == "ESBCN" && x.Module == ModuleTypes.Codes.xHub && x.RecipientID == ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub));
			AssertNotNull(shippingPortsMessagingEHubIDCollection.FirstOrDefault(x => x.Port == "ESGAN" && x.Module == ModuleTypes.Codes.xHub && x.RecipientID == ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub));
			AssertNotNull(shippingPortsMessagingEHubIDCollection.FirstOrDefault(x => x.Port == "ESPDS" && x.Module == ModuleTypes.Codes.xHub && x.RecipientID == ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub));
			AssertNotNull(shippingPortsMessagingEHubIDCollection.FirstOrDefault(x => x.Port == "ESVLC" && x.Module == ModuleTypes.Codes.xHub && x.RecipientID == ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub));
			AssertNotNull(shippingPortsMessagingEHubIDCollection.FirstOrDefault(x => x.Port.IsEmpty && x.Module == ModuleTypes.Codes.eHub && x.RecipientID == ShippingPortsMessagingEHubID.ShippingPortMessaging));
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ShippingPortsMessagingEHubIDCollection GetCollectionToTest()
		{
			return new ShippingPortsMessagingEHubIDCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new ShippingPortsMessagingEHubID();
			result.Port = "ESBCN";
			result.Module = ModuleTypes.Codes.eHub;
			result.RecipientID = "Testing";

			return result;
		}
	}
}
