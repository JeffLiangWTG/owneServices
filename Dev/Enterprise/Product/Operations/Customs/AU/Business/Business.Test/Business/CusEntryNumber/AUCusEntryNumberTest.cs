using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUCusEntryNumber))]
	sealed class AUCusEntryNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetBusinessObject();
		}

		BusinessObject GetBusinessObject()
		{
			var entryNum = Factory.NewWithValidTestData<AUCusEntryNumber>(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			entryNum.CE_ParentTable = "CusEntryHeader";

			return entryNum;
		}

		public void TestIsValidExemptionForExit2Exemption()
		{
			AUCusEntryNumber entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			Assert(entryNum.IsValidExemption);
			entryNum.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			Assert(!entryNum.IsValidExemption);
		}

		public void TestIsValidExemptionForCMRExemption()
		{
			AUCusEntryNumber entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			Assert(entryNum.IsValidExemption);
			entryNum.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			Assert(!entryNum.IsValidExemption);
		}

		public void TestGettingAndSettingKnownCode()
		{
			AUCusEntryNumber entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			AssertEquals(CMRExportExemptionCodes.EXLV.Code, entryNum.CE_EntryType);
		}

		public void TestGettingAndSettingUnknownCode()
		{
			AUCusEntryNumber entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = "ABC";
			AssertEquals("ABC", entryNum.CE_EntryType);
		}
	}
}
