using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingPortsMessagingEHubID))]
	public class ShippingPortsMessagingEHubIDTest : RegistryBusinessObjectTemplateTestCase<ShippingPortsMessagingEHubID>
	{
		#region Properties

		public void TestDefaultRecipientIDWhenSettingModule()
		{
			var shippingPortsMessagingEHubID = new ShippingPortsMessagingEHubID();
			shippingPortsMessagingEHubID.Port = "ESBCN";
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;

			AssertEquals(ShippingPortsMessagingEHubID.ShippingPortMessaging, shippingPortsMessagingEHubID.RecipientID);

			shippingPortsMessagingEHubID.RecipientID = ZString.Empty;
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.xHub;

			AssertEquals(ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub, shippingPortsMessagingEHubID.RecipientID);

			shippingPortsMessagingEHubID.RecipientID = "Forwarding";

			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;
			AssertEquals("Forwarding", shippingPortsMessagingEHubID.RecipientID);

			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.xHub;
			AssertEquals("Forwarding", shippingPortsMessagingEHubID.RecipientID);
		}

		#endregion

		#region Validation

		public void TestValidatePort()
		{
			var shippingPortsMessagingEHubIDs = new ShippingPortsMessagingEHubIDCollection();
			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDs.AddNew();
			AssertNoError(shippingPortsMessagingEHubID.PortInfo, "Enter a valid Port.");

			shippingPortsMessagingEHubID.Port = "BEANR";
			AssertHasError(shippingPortsMessagingEHubID.PortInfo, "Enter a valid Port.");

			shippingPortsMessagingEHubID.Port = "ESBCN";
			AssertNoError(shippingPortsMessagingEHubID.PortInfo, "Enter a valid Port.");
		}

		public void TestValidateModule()
		{
			var shippingPortsMessagingEHubIDs = new ShippingPortsMessagingEHubIDCollection();
			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDs.AddNew();

			shippingPortsMessagingEHubID.Module = string.Empty;
			AssertHasError(shippingPortsMessagingEHubID.ModuleInfo, "Please enter a Module.");
			AssertNoError(shippingPortsMessagingEHubID.ModuleInfo, "Enter a valid Module.");

			shippingPortsMessagingEHubID.Module = "zHub";
			AssertNoError(shippingPortsMessagingEHubID.ModuleInfo, "Please enter a Module.");
			AssertHasError(shippingPortsMessagingEHubID.ModuleInfo, "Enter a valid Module.");

			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;
			AssertNoError(shippingPortsMessagingEHubID.ModuleInfo, "Please enter a Module.");
			AssertNoError(shippingPortsMessagingEHubID.ModuleInfo, "Enter a valid Module.");
		}

		public void TestValidateRecipientID()
		{
			var shippingPortsMessagingEHubIDs = new ShippingPortsMessagingEHubIDCollection();
			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDs.AddNew();

			shippingPortsMessagingEHubID.RecipientID = "ShippingPortMessaging";
			AssertNoError(shippingPortsMessagingEHubID.RecipientIDInfo, "Please enter a Recipient ID.");

			shippingPortsMessagingEHubID.RecipientID = ZString.Empty;
			AssertHasError(shippingPortsMessagingEHubID.RecipientIDInfo, "Please enter a Recipient ID.");
		}

		public void TestCheckUniqueness()
		{
			var shippingPortsMessagingEHubIDs = new ShippingPortsMessagingEHubIDCollection();

			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDs.AddNew();
			shippingPortsMessagingEHubID.Port = "ESBCN";
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;

			var shippingPortsMessagingEHubID1 = shippingPortsMessagingEHubIDs.AddNew();
			shippingPortsMessagingEHubID1.Port = "ESPDS";
			shippingPortsMessagingEHubID1.Module = ModuleTypes.Codes.eHub;

			AssertNoErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.PortInfo);
			AssertNoErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.ModuleInfo);

			shippingPortsMessagingEHubID1.Port = "ESBCN";
			AssertHasErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.PortInfo);

			shippingPortsMessagingEHubID1.Module = ModuleTypes.Codes.xHub;
			AssertHasErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.ModuleInfo);

			shippingPortsMessagingEHubID1.Module = ModuleTypes.Codes.eHub;
			AssertHasErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.ModuleInfo);

			shippingPortsMessagingEHubID1.Port = "ESPDS";
			AssertNoErrors("The same Port cannot be duplicated.", shippingPortsMessagingEHubID1.PortInfo);
		}

		#endregion

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ShippingPortsMessagingEHubID GetBusinessObjectToClone()
		{
			var result = new ShippingPortsMessagingEHubID();
			result.Port = "ESBCN";
			result.Module = ModuleTypes.Codes.eHub;
			result.RecipientID = "Testing";

			return result;
		}

		protected override ShippingPortsMessagingEHubID GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
