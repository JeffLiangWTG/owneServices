using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Test.EDICommunicationParty
{
	[TestedType(typeof(EDICommunicationPartyFilterBusinessObject))]
	public class EDICommunicationPartyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDICommunicationPartyFilterBusinessObject();
		}
	}
}
