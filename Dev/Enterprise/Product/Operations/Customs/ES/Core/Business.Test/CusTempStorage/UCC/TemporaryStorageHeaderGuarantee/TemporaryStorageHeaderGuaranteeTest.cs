using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageHeaderGuarantee))]
class TemporaryStorageHeaderGuaranteeTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var (_, tsGuarantee) = CreateHeaderAndGuarantee();
		return tsGuarantee;
	}

	public void TestPW_BondAmountInfoIsReadOnly()
	{
		var (tsHeader, tsGuarantee) = CreateHeaderAndGuarantee();

		CombineAssertions(() =>
		{
			tsHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			tsGuarantee.PW_Override = false;
			AssertEquals("When AMA_MessageType is G5P and PW_Override is not checked", true, tsGuarantee.PW_BondAmountInfo.ReadOnly);
			tsGuarantee.PW_Override = true;
			AssertEquals("When AMA_MessageType is G5P and PW_Override is checked", false, tsGuarantee.PW_BondAmountInfo.ReadOnly);

			tsHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tsGuarantee.PW_Override = false;
			AssertEquals("When AMA_MessageType is G5X and PW_Override is not checked", true, tsGuarantee.PW_BondAmountInfo.ReadOnly);
			tsGuarantee.PW_Override = true;
			AssertEquals("When AMA_MessageType is G5X and PW_Override is checked", false, tsGuarantee.PW_BondAmountInfo.ReadOnly);

			tsHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tsGuarantee.PW_Override = false;
			AssertEquals("When AMA_MessageType is TSM and PW_Override is not checked", true, tsGuarantee.PW_BondAmountInfo.ReadOnly);
			tsGuarantee.PW_Override = true;
			AssertEquals("When AMA_MessageType is TSM and PW_Override is checked", false, tsGuarantee.PW_BondAmountInfo.ReadOnly);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	public void TestValidation()
	{
		AssertType<TemporaryStorageHeaderGuaranteeValidation>(Factory.New<TemporaryStorageHeaderGuarantee>().Validation);
	}

	(TemporaryStorageHeader Header, TemporaryStorageHeaderGuarantee Guarantee) CreateHeaderAndGuarantee()
	{
		var tsHeader = Factory.New<TemporaryStorageHeader>();
		var tsGuarantee = tsHeader.Guarantee;

		return (tsHeader, tsGuarantee);
	}
}
