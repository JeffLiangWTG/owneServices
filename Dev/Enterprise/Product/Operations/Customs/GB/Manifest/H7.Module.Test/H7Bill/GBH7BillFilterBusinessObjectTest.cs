using Enterprise.Customs.EU.H7.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Module.Testing
{
	[TestedType(typeof(GBH7BillFilterBusinessObject))]
	public class GBH7BillFilterBusinessObjectTest : EUH7BillFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GBH7BillFilterBusinessObject();

		protected override CodeDescriptionPairList GetExpectedMessageStatusList() => Factory.GetCachedValue<Common.Shared.MessageStatusList>();
	}
}
