using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationDeclarantProviderTest : TestCaseWithFactory
	{
		public void TestID()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_Address1 = "TEST1";

			var declarant = Factory.New<OrgHeader>();
			declarant.PrimaryRegistrationNumber.Number = "26.456.247/0001-00";
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.OA_Address1 = "TEST2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var dataProvider = new DeclarationDeclarantProvider(entryInstruction);
			AssertEquals("ID should be empty", string.Empty, dataProvider.ID);

			declaration.JE_OH_Supplier = supplier.PK;

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			dataProvider = new DeclarationDeclarantProvider(entryInstruction);
			AssertEquals("ID should be Supplier ID", "58500398000105", dataProvider.ID);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertEquals("ID should be Declarant ID", "26456247000100", dataProvider.ID);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			AssertEquals("ID should be Declarant ID", "26456247000100", dataProvider.ID);
		}

		public void TestContactProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.JustificationContactDetailAddress.E2_Contact = "Justify Name Contact";
			entryInstruction.JustificationContactDetailAddress.E2_Email = "justifynamecontact@company.com";
			entryInstruction.JustificationContactDetailAddress.E2_Mobile = "+5511999999999";

			var dataProvider = new DeclarationDeclarantProvider(entryInstruction);
			AssertEquals("Declarant ContactName should be", "Justify Name Contact", dataProvider.ContactName);
			AssertEquals("Declarant ContactEmail should be", "justifynamecontact@company.com", dataProvider.ContactEmail);
			AssertEquals("Declarant ContactPhone should be", "11999999999", dataProvider.ContactPhone);

			entryInstruction.JustificationContactDetailAddress.E2_Phone = "+551135856000";
			entryInstruction.JustificationContactDetailAddress.E2_Mobile = CargoWise.Types.ZString.Empty;

			AssertEquals("Declarant ContactPhone should be", "1135856000", dataProvider.ContactPhone);
		}
	}
}
