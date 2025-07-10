using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.Testing
{
	public class GbTurnCodeValidatorTest : TestCaseWithFactory
	{
		public void TestTurnCodeValidationOnPartyIsOkWhenHaveEitherTrnOrVatCodeDefined()
		{
			var dec = Factory.New<JobDeclaration>();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("ABC", "123");
			dec.JE_OH_Supplier = org.PK;
			AssertHasMessageError(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, CDS.MessagingRules.EoriCodeValidationHelper.ErrorMessageNoEoriCode);
			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew("VAT", "123");
			dec.JE_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = org.PK;
			AssertNoMessageError(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, CDS.MessagingRules.EoriCodeValidationHelper.ErrorMessageNoEoriCode);
			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456");
			dec.JE_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = org.PK;
			AssertNoMessageError(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, CDS.MessagingRules.EoriCodeValidationHelper.ErrorMessageNoEoriCode);
		}
	}
}
