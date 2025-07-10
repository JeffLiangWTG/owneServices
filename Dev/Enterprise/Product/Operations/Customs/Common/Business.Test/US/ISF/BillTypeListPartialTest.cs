using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.ISF.Testing
{
	class BillTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestICodeDescriptionPairListProviderMembers()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new BillTypeList();
			NUnit.Framework.Assert.That(list.GetCodeDescriptionPairList(), Is.EqualTo(list).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetReferenceBillTypes()
		{
			CodeDescriptionPairList list = BillTypeList.GetReferenceBillTypes();
			NUnit.Framework.Assert.That(list.Count, Is.EqualTo(3), "reference bill types should have 3 codes");
			NUnit.Framework.Assert.That(list.ContainsCode(BillTypeList.Codes.HouseBillOfLading), Is.True);
			NUnit.Framework.Assert.That(list.ContainsCode(BillTypeList.Codes.MasterBillOfLading), Is.True);
			NUnit.Framework.Assert.That(list.ContainsCode(BillTypeList.Codes.OceanBillOfLading), Is.True);
		}
	}
}
