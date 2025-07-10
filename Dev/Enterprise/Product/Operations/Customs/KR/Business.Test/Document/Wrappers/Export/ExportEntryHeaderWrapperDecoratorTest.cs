using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExportEntryHeaderWrapperDecoratorTest : TestCaseWithFactory
	{
		public void TestNewClassFieldValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = new ZDateTime("2022-02-01");
			entry.CH_CustomsMessageRemarks = "ABCD";

			entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "AAAA", "BBB", ZDateTime.Empty);

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_IssueDate = new ZDateTime("2022-03-01");
			entryNumber.CE_ExpiryDate = new ZDateTime("2022-03-03");

			ExportEntryHeaderWrapper exportEntryWrapper = new ExportEntryHeaderWrapper(entry.PK, new ExportEntryHeaderCreator().Create(entry), Factory);
			exportEntryWrapper.Decorate(entry);

			var entryHeader = exportEntryWrapper;
			AssertEquals(new ZDateTime("2022-03-01"), entryHeader.DeclarationDate);
			AssertEquals(new ZDateTime("2022-03-03"), entryHeader.ExpectedLoadingDate);
			AssertEquals(new ZDateTime("2022-02-01"), entryHeader.EntryReleaseDateTime);
			AssertEquals("ABCD", entryHeader.CustomsMessageRemarks);
			AssertEquals("AAAA-BBB", entryHeader.ResponsibleCustomsOfficer);
		}
	}
}
