using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAClassification))]
	sealed class CusCAClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertType<CusCAClassificationValidation>(details.Validation);
			pivot.CI_ChildType = "#@2";
			AssertType<CusCAClassificationValidation>(details.Validation);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertType<ImportCusCAClassificationValidation>(details.Validation);
		}

		public void TestCompartibleMaxLength()
		{
			// This test ensures that transformation will not fall. Any changes in CA AddInfo Schema must be reflected in CusCAClassification and vice versa.
			AssertEquals(CAAddInfoSchema.CA_NRCanInd.MaxLength, CusCAClassificationSchema.CCA_NRCanIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_OA_Manufacturer.MaxLength, CusCAClassificationSchema.CCA_OA_Manufacturer.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_PHACInd.MaxLength, CusCAClassificationSchema.CCA_PHACIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ProvinceOfOrigin.MaxLength, CusCAClassificationSchema.CCA_ProvinceOfOrigin.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_RequirementID.MaxLength, CusCAClassificationSchema.CCA_RequirementID.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_RequirementVer.MaxLength, CusCAClassificationSchema.CCA_RequirementVersion.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_RN_NKCFIAOrigin.MaxLength, CusCAClassificationSchema.CCA_RN_NKCFIAOrigin.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ModelNumber.MaxLength, CusCAClassificationSchema.CCA_ModelNumber.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_RN_NKOrigin.MaxLength, CusCAClassificationSchema.CCA_RN_NKOrigin.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_TCInd.MaxLength, CusCAClassificationSchema.CCA_TCIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_TIIN.MaxLength, CusCAClassificationSchema.CCA_TIIN.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_TreatmentCode.MaxLength, CusCAClassificationSchema.CCA_TreatmentCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_TRSNumber.MaxLength, CusCAClassificationSchema.CCA_TRSNumber.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_TypeSize.MaxLength, CusCAClassificationSchema.CCA_TypeSize.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ValueForDutyCode.MaxLength, CusCAClassificationSchema.CCA_ValueForDutyCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_SIMADumpingNum.MaxLength, CusCAClassificationSchema.CCA_SIMADumpingNumber.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_MiscID.MaxLength, CusCAClassificationSchema.CCA_MiscID.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ImportReasonCode.MaxLength, CusCAClassificationSchema.CCA_ImportReasonCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_HCInd.MaxLength, CusCAClassificationSchema.CCA_HCIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_99TariffCode.MaxLength, CusCAClassificationSchema.CCA_99TariffCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_AirsCode.MaxLength, CusCAClassificationSchema.CCA_AirsCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_AuthorityNumber.MaxLength, CusCAClassificationSchema.CCA_AuthorityNumber.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_CFIAInd.MaxLength, CusCAClassificationSchema.CCA_CFIAIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_CFIAUSStateOfOrigin.MaxLength, CusCAClassificationSchema.CCA_CFIAUSStateOfOrigin.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_CNSCInd.MaxLength, CusCAClassificationSchema.CCA_CNSCIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_CompliantCompletion.MaxLength, CusCAClassificationSchema.CCA_CompliantCompletion.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_CompliantImportDate.MaxLength, CusCAClassificationSchema.CCA_CompliantImportDateIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_DestinationProvince.MaxLength, CusCAClassificationSchema.CCA_DestinationProvince.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_DFOInd.MaxLength, CusCAClassificationSchema.CCA_DFOIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ECCCInd.MaxLength, CusCAClassificationSchema.CCA_ECCCIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_EndUse.MaxLength, CusCAClassificationSchema.CCA_EndUse.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ETExemption.MaxLength, CusCAClassificationSchema.CCA_ETExemption.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_ETRateCode.MaxLength, CusCAClassificationSchema.CCA_ETRateCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_GACInd.MaxLength, CusCAClassificationSchema.CCA_GACIndicator.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_GSTStatusCode.MaxLength, CusCAClassificationSchema.CCA_GSTStatusCode.MaxLength);
			AssertEquals(CAAddInfoSchema.CA_Model.MaxLength, CusCAClassificationSchema.CCA_Model.MaxLength);
		}

		public void TestMarkAsNeedingValidation()
		{
			var classification = Factory.New<CusClassification>();
			classification.MarkLightValidationAsValidForTesting();
			Assert(classification.LightValidationIsValid);
			var caClassification = classification.Details;
			Assert(!classification.LightValidationIsValid);

			classification.MarkLightValidationAsValidForTesting();
			Assert(classification.LightValidationIsValid);
			caClassification.CCA_RN_NKOrigin = "CN";
			Assert(!classification.LightValidationIsValid);

			classification.MarkLightValidationAsValidForTesting();
			Assert(classification.LightValidationIsValid);
			caClassification.CCA_SIMADumpingNumber = "123";
			Assert(!classification.LightValidationIsValid);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			var details = pivot.Details;
			details.FillWithValidTestData();
			return details;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
