using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5ULHeaderWrapperDecoratorTest : TestCaseWithFactory
	{
		public void TestNewClassFieldValues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";

			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "신청인 상호");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "신청인 대표자명", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			branch.GB_OH_OrgProxy = broker.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = new ZDateTime("2000-01-01");
			var entryNumber5UL = entry.EntryNumbers.AddNew();
			entryNumber5UL.CE_EntryType = "5UL";
			entryNumber5UL.CE_EntryStatus = "OAC";
			entryNumber5UL.CE_IssueDate = new ZDateTime("2000-12-31");

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("신청인 상호", wrapper.Declarant.CompanyName);
			AssertEquals("신청인 대표자명", wrapper.Declarant.RepresentativeName);

			AssertEquals(new ZDateTime("2000-01-01"), wrapper.EntryReleaseDate);
			AssertEquals("OAC", wrapper.EntryStatus);
			AssertEquals(new ZDateTime("2000-12-31"), wrapper.IssueDateTo5UL);
		}
	}
}
