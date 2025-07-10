using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.AMS.Testing
{
	class AMSConsolBillCustomsStatusListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			var list = new AMSConsolBillCustomsStatusList();
			NUnit.Framework.Assert.That(list.Count, Is.EqualTo(4));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(AMSConsolBillCustomsStatusList.Codes.Multiple), Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.Multiple).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(AMSBillCustomsStatusList.Codes.NotOnFile), Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(AMSBillCustomsStatusList.Codes.OnFile), Is.EqualTo(AMSBillCustomsStatusList.Descriptions.OnFile).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(AMSConsolBillCustomsStatusList.Codes.OutOfSync), Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.OutOfSync).Using(CustomComparers.TypeComparison));
		}
	}
}
