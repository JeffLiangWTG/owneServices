using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusExitItem))]
	class CusExitItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var exitItem = Factory.New<CusExitItem>();
			AssertType<CusExitItemLookups>(exitItem.Lookups);
		}

		public void TestReferenceNumberUCR_Caption()
		{
			AssertEquals("Reference Number UCR", DataBoundResourceStrings.GetDataForProperty(exitItem.ReferenceNumberUCRInfo).Caption);
		}

		public void TestReferenceNumberUCR_MaxLength()
		{
			AssertEquals("MaxLength", 35, exitItem.ReferenceNumberUCRInfo.MaxLength);
		}

		public void TestReferenceNumberUCR_Getter()
		{
			var cusEntryNumber = CusEntryNumber.New(exitItem, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "DEUCR123";
			AssertEquals("DEUCR123", exitItem.ReferenceNumberUCR);
		}

		public void TestReferenceNumberUCR_Setter()
		{
			exitItem.ReferenceNumberUCR = "DEUCR123";
			var cusEntryNumber = CusEntryNumber.Load(exitItem, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Germany);
			AssertEquals("DEUCR123", cusEntryNumber.CE_EntryNum);
		}

		public void TestRegistrationNumberAWB_Caption()
		{
			AssertEquals("Registration Number (ext.)", DataBoundResourceStrings.GetDataForProperty(exitItem.RegistrationNumberAWBInfo).Caption);
		}

		public void TestRegistrationNumberAWB_MaxLength()
		{
			AssertEquals("MaxLength", 35, exitItem.RegistrationNumberAWBInfo.MaxLength);
		}

		public void TestRegistrationNumberAWB_Getter()
		{
			var cusEntryNumber = CusEntryNumber.New(exitItem, CusEntryNumberTypes.Germany.AirWaybillEntryNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "DEAWB123";
			AssertEquals("DEAWB123", exitItem.RegistrationNumberAWB);
		}

		public void TestRegistrationNumberAWB_Setter()
		{
			exitItem.RegistrationNumberAWB = "DEAWB123";
			var cusEntryNumber = CusEntryNumber.Load(exitItem, CusEntryNumberTypes.Germany.AirWaybillEntryNumber, Core.Constants.CountryCodes.Germany);
			AssertEquals("DEAWB123", cusEntryNumber.CE_EntryNum);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var exitHeader = factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			exitItem.CXI_Status = "XXX";
			return exitItem;
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitItem = Factory.New<CusExitItem>();
		}
		CusExitItem exitItem;
	}
}
