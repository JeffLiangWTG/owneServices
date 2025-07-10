using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCMRMessageChargeItem))]
	sealed class DocCMRMessageChargeItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocCMRMessageChargeItem("test", 0m);
		}

		public void TestDocCMRMessageChargeItem()
		{
			DocCMRMessageChargeItem item = new DocCMRMessageChargeItem("test", 2000m);
			AssertEquals("Charge Type", "test", item.ChargeType);
			AssertEquals("Description", "TEST", item.Description);
			AssertEquals("Amount", 2000m, item.Amount);
			AssertEquals("EmptyStringIfAmountIsZero", "2000.00", item.EmptyStringIfAmountIsZero);

			item = new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.DutyAmount, 0m);
			AssertEquals("Description", "TOTAL PAYABLE " + CusEntryChargeTypeList.Descriptions.DutyAmount.ToUpper(), item.Description);
			AssertEquals("EmptyStringIfAmountIsZero", "", item.EmptyStringIfAmountIsZero);
		}
	}
}
