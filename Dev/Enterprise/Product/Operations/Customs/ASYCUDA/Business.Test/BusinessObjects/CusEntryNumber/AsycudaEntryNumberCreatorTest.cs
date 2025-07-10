using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaEntryNumberCreatorTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdateRegistrationNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var number1 = AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<CusEntryNumber>(header, "ENT1", header.AMA_RN_NKCountry);
			AssertEquals("number1.CE_EntryType", CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, number1.CE_EntryType);
			AssertEquals("number1.CE_EntryNum", "ENT1", number1.CE_EntryNum);
			AssertEquals("number1.CE_ParentID", header.PK, number1.CE_ParentID);
			var number2 = AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<CusEntryNumber>(header, "ENT4", header.AMA_RN_NKCountry);
			AssertEquals(number1, number2);
			AssertEquals("number1.CE_EntryType", CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, number1.CE_EntryType);
			AssertEquals("number1.CE_EntryNum", "ENT4", number1.CE_EntryNum);
			AssertEquals("number1.CE_ParentID", header.PK, number1.CE_ParentID);
		}

		public void TestCreateOrUpdateTradenetPermitNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var number1 = AsycudaEntryNumberCreator.CreateOrUpdateCustomsEntryNumber<CusEntryNumber>(header, "ENT1", header.AMA_RN_NKCountry);
			AssertEquals("number1.CE_EntryType", Constants.CustomsEntryType.TradeNetPermit, number1.CE_EntryType);
			AssertEquals("number1.CE_EntryNum", "ENT1", number1.CE_EntryNum);
			AssertEquals("number1.CE_ParentID", header.PK, number1.CE_ParentID);
			var number2 = AsycudaEntryNumberCreator.CreateOrUpdateCustomsEntryNumber<CusEntryNumber>(header, "ENT4", header.AMA_RN_NKCountry);
			AssertEquals(number1, number2);
			AssertEquals("number1.CE_EntryType", Constants.CustomsEntryType.TradeNetPermit, number1.CE_EntryType);
			AssertEquals("number1.CE_EntryNum", "ENT4", number1.CE_EntryNum);
			AssertEquals("number1.CE_ParentID", header.PK, number1.CE_ParentID);
		}
	}
}
