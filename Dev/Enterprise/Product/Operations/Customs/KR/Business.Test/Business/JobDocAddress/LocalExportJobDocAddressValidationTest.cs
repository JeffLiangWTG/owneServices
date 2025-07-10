using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAddressLine1()
		{
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var stevedore = Declaration.StevedoreCompany;
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertNoMessageErrorContaining(stevedore.E2_OA_AddressInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			stevedore.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(stevedore.E2_OA_AddressInfo, MandatoryValidation.YouHaveNotEntered);
			stevedore.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNoMessageErrors(stevedore.E2_OA_AddressInfo);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
