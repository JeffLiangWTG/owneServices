using System.Linq;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class GuaranteeDataProviderTest : BaseDepartureDataProviderTest<GuaranteeDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection()
	{
		var guaranteeDataProvider = GuaranteeDataProvider.NewCollection(null);
		AssertNotNull("input collection is null", guaranteeDataProvider);
		AssertEquals("output count if null", 0, guaranteeDataProvider.Count());

		var validBondTypes = new[] {
			EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver,
			EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee,
			EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor,
			EUNctsGuaranteeTypeList.Codes.FlatRateVoucher,
			EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage
		};

		foreach (var bondType in new EUNctsGuaranteeTypeList().GetAllCodes())
		{
			NctsHeader.MovementHeader.Guarantees.AddNew().PW_BondType = bondType;
		}

		var guaranteeDataProviders = GuaranteeDataProvider.NewCollection(NctsHeader.MovementHeader.Guarantees).ToArray();
		AssertContainsExactElementsInAnyOrder(validBondTypes, guaranteeDataProviders.Select(x => x.GuaranteeType));
	}

	public void TestGuaranteeType()
	{
		const string bondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;

		Guarantee.PW_BondType = bondType;

		AssertEquals(bondType, DataProvider.GuaranteeType);
	}

	public void TestOtherGuaranteeReference()
	{
		const string bondNumber = "123";

		Guarantee.PW_BondNumber2 = bondNumber;

		AssertEquals(bondNumber, DataProvider.OtherGuaranteeReference);
	}

	public void TestSequenceNumber() => CombineAssertions(() =>
	{
		CreateGuarantee();
		CreateGuarantee();
		NctsHeader.MovementHeader.Guarantees.AddNew();
		var dataProviders = GuaranteeDataProvider.NewCollection(NctsHeader.MovementHeader.Guarantees).ToArray();
		AssertEquals(1, dataProviders[0].SequenceNumber);
		AssertEquals(2, dataProviders[1].SequenceNumber);
	});

	public void TestGuaranteeReference() => CombineAssertions(() =>
	{
		_ = Guarantee;
		AssertEquals("count", 1, DataProvider.GuaranteeReferences.Count);
		Assert("type", DataProvider.GuaranteeReferences.All(g => g is GuaranteeReferenceDataProvider));
		AssertSame("cached", DataProvider.GuaranteeReferences, DataProvider.GuaranteeReferences);
	});

	NctsGuarantee Guarantee => guarantee ??= CreateGuarantee();
	NctsGuarantee guarantee;

	NctsGuarantee CreateGuarantee()
	{
		var guarantee = NctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
		return guarantee;
	}

	protected override GuaranteeDataProvider CreateDataProvider() => GuaranteeDataProvider.NewCollection(NctsHeader.MovementHeader.Guarantees).First();
}
