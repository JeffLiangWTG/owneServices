using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitItemCollection))]
	class CusExitItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusExitItemCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusExitItemCollection(Factory.New<CusExitDetail>());

		[ExpectNoExceptions]
		public void TestDefaultCXI_LineNumber()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			NUnit.Framework.Assert.That(exitItem.CXI_LineNumber.ToString(), NUnit.Framework.Is.EqualTo("1"));
			var exitItem2 = exitDetail.CusExitItems.AddNew();
			NUnit.Framework.Assert.That(exitItem2.CXI_LineNumber.ToString(), NUnit.Framework.Is.EqualTo("2"));
		}

		[ExpectNoExceptions]
		public void TestNewChild()
		{
			var cusExitDetail = Factory.New<CusExitDetail>();
			var item = cusExitDetail.CusExitItems.AddNew();
			NUnit.Framework.Assert.That(item.CXI_GrossMassUQ, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.CXI_NetMassUQ, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.CXI_LineNumber.ToString(), NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(item.CXI_Status.ToString(), NUnit.Framework.Is.EqualTo("UNK"));
			NUnit.Framework.Assert.That(item.CXI_CED, NUnit.Framework.Is.EqualTo(cusExitDetail.PK));
		}
	}
}
