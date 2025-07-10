using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUNDGDataItem))]
	sealed class NctsUNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestName()
		{
			AssertEquals("Dangerous Goods Code", nctsUNDGDataItem.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsUNDGDataItem = Factory.New<NctsUNDGDataItem>();
		}

		NctsUNDGDataItem nctsUNDGDataItem;
	}
}
