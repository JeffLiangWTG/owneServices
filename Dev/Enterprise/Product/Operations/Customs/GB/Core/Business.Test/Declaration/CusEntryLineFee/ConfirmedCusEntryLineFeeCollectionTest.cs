using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Test
{
	[TestedType(typeof(ConfirmedCusEntryLineFeeCollection))]
	internal class ConfirmedCusEntryLineFeeCollectionTest : Customs.Business.Testing.ConfirmedCusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CreateCollectionForTest();
		}

		ConfirmedCusEntryLineFeeCollection CreateCollectionForTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			return new ConfirmedCusEntryLineFeeCollection(entryLine);
		}

		protected override Type GetTypeOfElement() => typeof(CusEntryLineFee);

		public void TestNewAddOrUpdate()
		{
			var item1 = Collection.AddOrUpdate("XYZ", 123.45);

			var testCollection = CreateCollectionForTest();
			var item2 = testCollection.AddOrUpdate("XYZ", 123.45);

			CombineAssertions("Item created by hiding AddOrUpdate should be identical to that created by base AddOrUpdate.", () =>
			{
				AssertEquals(item1.CF_ChargeType, item2.CF_ChargeType);
				AssertEquals(item1.CF_ChargeAmount, item2.CF_ChargeAmount);
				AssertEquals(item1.CF_Source, item2.CF_Source);
			});
		}
	}
}
