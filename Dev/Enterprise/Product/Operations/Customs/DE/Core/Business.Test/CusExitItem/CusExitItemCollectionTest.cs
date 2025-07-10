using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusExitItemCollection))]
	class CusExitItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewChild()
		{
			var item = exitDetail.CusExitItems.AddNew();
			AssertEquals(ZString.Empty, item.CXI_Status);
		}

		protected override Type GetExpectedCollectionType() => typeof(CusExitItemCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusExitItemCollection(exitDetail);

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.New<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
		}
		CusExitDetail exitDetail;
	}
}
