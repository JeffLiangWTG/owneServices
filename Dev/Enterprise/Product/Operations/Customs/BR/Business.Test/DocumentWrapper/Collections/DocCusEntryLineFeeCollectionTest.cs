using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryLineFeeCollection))]
	class DocCusEntryLineFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineFeeCollection>
	{
		public void TestChargeTypeIndex()
		{
			var feeIPI = Factory.New<CusEntryLineFee>();
			feeIPI.CF_ChargeType = "IPI";
			feeIPI.CF_ChargeAmount = 100m;
			feeIPI.CF_MethodOfCalculation = "QPU";

			var feePIS = Factory.New<CusEntryLineFee>();
			feePIS.CF_ChargeType = "PIS";
			feePIS.CF_ChargeAmount = 200m;
			feePIS.CF_MethodOfCalculation = "OTH";

			var collection = new DocCusEntryLineFeeCollection(Factory);
			collection.Add(DocCusEntryLineFee.New(feePIS, Factory));
			collection.Add(DocCusEntryLineFee.New(feeIPI, Factory));

			CombineAssertions(() =>
			{
				AssertEquals("IPI ChargeAmount", 100m, collection["IPI"].ChargeAmount);
				AssertEquals("IPI MethodOfCalculation", "QPU", collection["IPI"].MethodOfCalculation);
				AssertEquals("PIS ChargeAmount", 200m, collection["PIS"].ChargeAmount);
				AssertEquals("PIS MethodOfCalculation", "OTH", collection["PIS"].MethodOfCalculation);
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryLineFee = Factory.New<CusEntryLineFee>();
			return DocCusEntryLineFee.New(cusEntryLineFee, Factory);
		}

		protected override DocCusEntryLineFeeCollection GetCollectionToTest()
		{
			return new DocCusEntryLineFeeCollection(Factory);
		}
	}
}
