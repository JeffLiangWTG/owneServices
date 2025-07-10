using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderTemplateFilterBusinessObject))]
	public class ClientDeviceHeaderTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ClientDeviceHeaderTemplateFilterBusinessObject();
	}
}
