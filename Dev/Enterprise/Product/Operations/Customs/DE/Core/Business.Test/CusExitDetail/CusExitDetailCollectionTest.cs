using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusExitDetailCollection))]
	class CusExitDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChild()
		{
			exitHeader.CEH_LocationOfGoods = "DEA";
			var collection = GetCollectionToTest() as CusExitDetailCollection;
			var exitDetail = collection.AddNew();
			NUnit.Framework.Assert.That(exitDetail.CED_LocationOfGoods, Is.EqualTo("DEA").Using(CustomComparers.TypeComparison));
		}

		protected override Type GetExpectedCollectionType() => typeof(CusExitDetailCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusExitDetailCollection(exitHeader);

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;
	}
}
