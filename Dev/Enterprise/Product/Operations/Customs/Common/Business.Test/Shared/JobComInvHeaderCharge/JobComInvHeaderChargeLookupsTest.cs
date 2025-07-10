using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class JobComInvHeaderChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestPrepaidCollectList()
		{
			var invoice = Factory.New<TestInvoice>();
			var charge1 = invoice.Charges.AddNew();
			var prepaidCollectList = charge1.Lookups.PrepaidCollectList;
			NUnit.Framework.Assert.That(prepaidCollectList.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(prepaidCollectList.GetDescriptionFromCode(Core.Constants.PaymentType.Collect), Is.EqualTo("Collect"));
			NUnit.Framework.Assert.That(prepaidCollectList.GetDescriptionFromCode(Core.Constants.PaymentType.Prepaid), Is.EqualTo("Prepaid"));
			var charge2 = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(charge1.Lookups.PrepaidCollectList, Is.EqualTo(prepaidCollectList));
		}

		[ExpectNoExceptions]
		public void TestChargeTypeList()
		{
			var invoice = Factory.New<TestInvoice>();
			var charge = invoice.Charges.AddNew();
			var chargeTypeList = charge.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(chargeTypeList, Is.EqualTo(Factory.GetCachedValue<CustomsChargeTypeList>()).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.CustomsCharges.Codes.AdditionalCharges), Is.EqualTo("Other Additional Charges"));
			NUnit.Framework.Assert.That(chargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.CustomsCharges.Codes.DeductionalCharges), Is.EqualTo("Other Deduction Charges"));
		}

		[ExpectNoExceptions]
		public void TestChargeDistributeBy()
		{
			var invoice = Factory.New<TestInvoice>();
			var charge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(charge.Lookups.ChargeDistributionBy, Is.EqualTo(Factory.GetCachedValue<ChargeDistributeByList>()).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApportionmentTypeList()
		{
			var invoice = Factory.New<TestInvoice>();
			var charge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(charge.Lookups.ApportionmentTypeList, Is.EqualTo(Factory.GetCachedValue<ApportionmentTypeList>()).Using(CustomComparers.TypeComparison));
		}
	}
}
