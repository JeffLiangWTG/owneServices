using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(UTBEntryLineFeeWrapper))]
class UTBEntryLineFeeWrapperTest : NonPersistentBusinessObjectTestCase
{
	BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper> fees;
	protected override void SetUp()
	{
		base.SetUp();
		var entryHeader = DocumentWrapperTestHelper.GetEntryHeaderForTest(Factory);
		var entryLineDetails = new UTBDocumentWrapper(entryHeader).CusEntryLineDetails;
		fees = new BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>();
		fees.AddRange(
			entryLineDetails.SelectMany(line => line.OfType<UTBEntryLineDetailWrapper>().SelectMany(lines => lines.FeeSummary))
		);
	}
	public void TestFees()
	{
		AssertEquals("Amount Fees", 7, fees.Count);

		var fee1 = fees[0];
		var fee2 = fees[1];
		var fee3 = fees[2];
		var fee4 = fees[3];

		CombineAssertions(() =>
		{
			AssertEquals("Fees fee1 - Description", "Customs duties on industrial products", fee1.Description);
			AssertEquals("Fees fee1 - BaseAmount", 1200m, fee1.BaseAmount);
			AssertEquals("Fees fee1 - TariffApplied", 8m, fee1.TariffApplied);
			AssertEquals("Fees fee1 - MethodOfCalculation", "%", fee1.MethodOfCalculation);
			AssertEquals("Fees fee1 - UnitOfTariff", "[UCC 4/4] Total amount", fee1.UnitOfTariff);
			AssertEquals("Fees fee1 - TariffCode", EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, fee1.TariffCode);
			AssertEquals("Fees fee1 - TariffAmount", 96m, fee1.TariffAmount);

			AssertEquals("Fees fee2 - Description", "BTW", fee2.Description);
			AssertEquals("Fees fee2 - BaseAmount", 1200m, fee2.BaseAmount);
			AssertEquals("Fees fee2 - TariffApplied", 21m, fee2.TariffApplied);
			AssertEquals("Fees fee2 - MethodOfCalculation", "%", fee2.MethodOfCalculation);
			AssertEquals("Fees fee2 - UnitOfTariff", "[UCC 4/4] Total amount", fee2.UnitOfTariff);
			AssertEquals("Fees fee2 - TariffCode", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, fee2.TariffCode);
			AssertEquals("Fees fee2 - TariffAmount", 252m, fee2.TariffAmount);

			AssertEquals("Fees fee3 - Description", "Customs duties on industrial products", fee3.Description);
			AssertEquals("Fees fee3 - BaseAmount", 1200m, fee3.BaseAmount);
			AssertEquals("Fees fee3 - TariffApplied", 2m, fee3.TariffApplied);
			AssertEquals("Fees fee3 - MethodOfCalculation", "CEN", fee3.MethodOfCalculation);
			AssertEquals("Fees fee3 - UnitOfTariff", "[UCC 4/4] Total amount", fee3.UnitOfTariff);
			AssertEquals("Fees fee3 - TariffCode", EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, fee3.TariffCode);
			AssertEquals("Fees fee3 - TariffAmount", 24m, fee3.TariffAmount);

			AssertEquals("Fees fee4 - Description", "Customs duties on industrial products", fee4.Description);
			AssertEquals("Fees fee4 - BaseAmount", 800m, fee4.BaseAmount);
			AssertEquals("Fees fee4 - TariffApplied", 6m, fee4.TariffApplied);
			AssertEquals("Fees fee4 - MethodOfCalculation", "%", fee4.MethodOfCalculation);
			AssertEquals("Fees fee4 - UnitOfTariff", "[UCC 4/4] Total amount", fee4.UnitOfTariff);
			AssertEquals("Fees fee4 - TariffCode", EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, fee4.TariffCode);
			AssertEquals("Fees fee4 - TariffAmount", 48m, fee4.TariffAmount);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => fees[0];
}
