using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.Business.Testing
{
	[TestedType(typeof(ShippingAgentRegistryDataType))]
	class ShippingAgentRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ShippingAgentRegistryDataType>
	{
		protected override ShippingAgentRegistryDataType GetNewDataType() => new ShippingAgentRegistryDataType();
		protected override string ExpectedEditorName => "ShippingAgentRegistryItemEditor";
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.Load<OrgHeader>(new ZGuid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29")); //Test OrgHeader PK already in DAT DB.
			var shippingAgent1 = new ShippingAgentObject { ShippingAgentAddress = org1.MainAddress.PK };
			var org2 = factory.Load<OrgHeader>(new ZGuid("13A2CD7D-848D-4481-9F30-004D6291A045")); //Test OrgHeader PK already in DAT DB.
			var shippingAgent2 = new ShippingAgentObject { ShippingAgentAddress = org2.MainAddress.PK };
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(shippingAgent1, new byte[] { 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0, 60, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 65, 0, 100, 0, 100, 0, 114, 0, 101, 0, 115, 0, 115, 0, 62, 0, 52, 0, 51, 0, 101, 0, 52, 0, 52, 0, 102, 0, 97, 0, 99, 0, 45, 0, 102, 0, 49, 0, 101, 0, 52, 0, 45, 0, 52, 0, 98, 0, 52, 0, 48, 0, 45, 0, 56, 0, 100, 0, 97, 0, 56, 0, 45, 0, 100, 0, 51, 0, 52, 0, 49, 0, 97, 0, 53, 0, 102, 0, 99, 0, 54, 0, 50, 0, 49, 0, 51, 0, 60, 0, 47, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 65, 0, 100, 0, 100, 0, 114, 0, 101, 0, 115, 0, 115, 0, 62, 0, 60, 0, 47, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0 }), new ValidSampleAndBinaryValueInDB(shippingAgent2, new byte[] { 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0, 60, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 65, 0, 100, 0, 100, 0, 114, 0, 101, 0, 115, 0, 115, 0, 62, 0, 97, 0, 101, 0, 52, 0, 101, 0, 53, 0, 55, 0, 53, 0, 57, 0, 45, 0, 53, 0, 97, 0, 52, 0, 48, 0, 45, 0, 52, 0, 52, 0, 97, 0, 48, 0, 45, 0, 57, 0, 98, 0, 50, 0, 49, 0, 45, 0, 98, 0, 54, 0, 98, 0, 56, 0, 100, 0, 100, 0, 100, 0, 100, 0, 100, 0, 50, 0, 52, 0, 50, 0, 60, 0, 47, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 65, 0, 100, 0, 100, 0, 114, 0, 101, 0, 115, 0, 115, 0, 62, 0, 60, 0, 47, 0, 83, 0, 104, 0, 105, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 65, 0, 103, 0, 101, 0, 110, 0, 116, 0, 79, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 0, 62, 0 }) };
		}
	}
}
