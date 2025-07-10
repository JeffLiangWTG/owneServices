using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckCH_BGMReference()
		{
			var header = Factory.New<CusEntryHeader>();
			header.CH_BGMReference = "";
			AssertHasMessageError(header.CH_BGMReferenceInfo, "The Reference Number of is required for messaging. Please go to menu 'Brokerage - Allocate Entry Reference Number' to allocate references.");
			header.CH_BGMReference = "TTT";
			AssertNoMessageErrors(header.CH_BGMReferenceInfo);
		}

		public void TestCheckEntryNumber()
		{
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_OH_Importer = helper.Importer.PK;
			var entryHeader = impDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.Validation.ValidateEntryNumber();
			AssertHasMessageErrorContaining(entryHeader.EntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			entryHeader.EntryNumber = "1234";
			AssertNoMessageErrorContaining(entryHeader.EntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
