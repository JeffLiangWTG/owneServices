using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrdersFilterBusinessObject))]
	public class WoolworthsOrdersFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WoolworthsOrdersFilterBusinessObject();
		}
		#endregion
	}
}
