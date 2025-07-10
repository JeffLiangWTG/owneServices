using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	public class GenWrapperTest : TestCaseWithFactory
	{
		public void TestEntryNumber()
		{
			AssertEquals("PREV_ENT_0001", wrapper.EntryNumber);
		}

		public void TestDirection()
		{
			AssertEquals("IMP", wrapper.Direction);
		}

		public void TestNumcod()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "0123", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "AKHO", Core.Constants.CountryCodes.France);
			guarantee.CPH_StartDate = ZDate.Today.AddDays(-2);
			guarantee.CPH_EndDate = ZDate.Today.AddDays(-1);
			guarantee.CPH_Number = "GUAA";
			Factory.Save();

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_CustomsGuaranteeNumber = "GUAA";
			AssertEquals("AKHO", wrapper.Numcod);
		}

		public void TestOpecod()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "12345678", Core.Constants.CountryCodes.France);
			entryHeader.Importer.E2_OA_Address = orgHeader.MainAddress.PK;

			wrapper = new GenWrapper(itemApplicator);
			AssertEquals("FR12345678", wrapper.Opecod);
		}

		protected override void SetUp()
		{
			base.SetUp();
			headerApplicator = new FrCreditCODApplicator(Factory);
			itemApplicator = headerApplicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.PreviousEntryReference = "PREV_BGM_0001";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsGuaranteeNumber = "GUAA";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "PREV_BGM_0001";
			entryHeader.EntryNumber = "PREV_ENT_0001";
			wrapper = new GenWrapper(itemApplicator);
		}
		FrCreditCODApplicator headerApplicator;
		CreditCODDataObject itemApplicator;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		GenWrapper wrapper;
	}
}
