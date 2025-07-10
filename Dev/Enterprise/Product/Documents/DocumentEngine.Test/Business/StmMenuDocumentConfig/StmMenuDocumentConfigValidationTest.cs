using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuDocumentConfigValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateS3_OverrideDataContext()
		{
			var invalidOverridingDataContextError = "Invalid Overriding Data Context.";
			DocConfig.S3_OverrideDataContext = ZString.Empty;
			AssertNoErrors(DocConfig.S3_OverrideDataContextInfo);

			DocConfig.S3_OverrideDataContext = "GenericCommercialInvoice"; // a valid Data Context
			AssertNoErrors(DocConfig.S3_OverrideDataContextInfo);

			DocConfig.S3_OverrideDataContext = "XXX"; // an invalid Data Context
			AssertHasError(DocConfig.S3_OverrideDataContextInfo, invalidOverridingDataContextError);
		}

		public void TestValidateS3_Description()
		{
			DocConfig.S3_IsSystem = true;
			DocConfig.S3_Description = ZString.Empty;
			AssertHasErrorContaining(DocConfig.S3_DescriptionInfo, MandatoryValidation.MustBeEntered);

			DocConfig.S3_Description = "Not blank";
			AssertNoError(DocConfig.S3_DescriptionInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateS3_PageStyle()
		{
			DocConfig.S3_PageStyle = ZString.Empty;
			AssertHasErrorContaining(DocConfig.S3_PageStyleInfo, MandatoryValidation.MustBeEntered);

			DocConfig.S3_PageStyle = "XXX";
			AssertHasErrorContaining(DocConfig.S3_PageStyleInfo, ListValidation.InvalidCodeError);

			DocConfig.S3_PageStyle = DocumentConfigPageStyleList.Codes.Portrait;
			AssertNoErrors(DocConfig.S3_PageStyleInfo);

			DocConfig.S3_PageStyle = "ABC";
			AssertHasErrorContaining(DocConfig.S3_PageStyleInfo, ListValidation.InvalidCodeError);

			DocConfig.S3_PageStyle = DocumentConfigPageStyleList.Codes.Landscape;
			AssertNoErrors(DocConfig.S3_PageStyleInfo);

			DocConfig.S3_PageStyle = ZString.Empty;
			AssertHasErrorContaining(DocConfig.S3_PageStyleInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateS3_ExcludedFromDocPack()
		{
			DocConfig.Validation.ValidateS3_ExcludedFromDocPack();
			AssertNoErrors(DocConfig.S3_ExcludedFromDocPackInfo);

			DocConfig.S3_ExcludedFromDocPack = true;
			AssertHasErrorContaining(DocConfig.S3_ExcludedFromDocPackInfo, "Can not exclude system defined document configuration from document pack without any other document configurations added in the document pack");

			var systemTemplateConfig = ((StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot).DocConfigs.AddNew();
			systemTemplateConfig.S3_IsSystem = true;
			systemTemplateConfig.S3_IsTemplate = true;
			DocConfig.Validation.ValidateS3_ExcludedFromDocPack();
			AssertNoErrors(DocConfig.S3_ExcludedFromDocPackInfo);

			systemTemplateConfig.S3_ExcludedFromDocPack = true;
			DocConfig.Validation.ValidateS3_ExcludedFromDocPack();
			AssertHasErrorContaining(DocConfig.S3_ExcludedFromDocPackInfo, "Can not exclude system defined document configuration from document pack without any other document configurations added in the document pack");

			var anotherDocConfig = ((StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot).DocConfigs.AddNew();
			anotherDocConfig.S3_SI = DocConfig.S3_SI;
			anotherDocConfig.S3_GC = GlbCompany.CurrentCompany.PK;

			DocConfig.Validation.ValidateS3_ExcludedFromDocPack();
			AssertNoErrors(DocConfig.S3_ExcludedFromDocPackInfo);

			DocConfig.S3_ExcludedFromDocPack = false;
			AssertNoErrors(DocConfig.S3_ExcludedFromDocPackInfo);
		}

		public void TestValidateAll()
		{
			DocConfig.Validation.ValidateAll();
			AssertHasRowError(DocConfig, "Please add at least one section.");

			DocConfig.ConfigItems.AddNew();
			DocConfig.Validation.ValidateAll();
			AssertNoRowErrors(DocConfig);
		}

		public void TestValidateS3_GC()
		{
			string duplicateFallbackError = "There is already another document configuration with the same combination of values for System Defined, Company and Client.";

			DocConfig.Validation.ValidateS3_GC();
			AssertNoErrors(DocConfig.S3_GCInfo);

			DocConfig.S3_GC = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(DocConfig.S3_GCInfo);

			StmMenuDocumentConfig anotherDocConfig = ((StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot).DocConfigs.AddNew();
			anotherDocConfig.S3_SI = DocConfig.S3_SI;
			anotherDocConfig.S3_GC = GlbCompany.CurrentCompany.PK;
			AssertHasError(anotherDocConfig.S3_GCInfo, duplicateFallbackError);

			DocConfig.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertNoErrors(DocConfig.S3_GCInfo);

			anotherDocConfig.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertHasError(anotherDocConfig.S3_GCInfo, duplicateFallbackError);

			anotherDocConfig.S3_OH = ZGuid.Empty;
			anotherDocConfig.S3_IsSystem = true;
			AssertHasError(anotherDocConfig.S3_GCInfo, "Please do not enter a Company.");

			anotherDocConfig.S3_GC = ZGuid.Empty;
			AssertNoErrors(anotherDocConfig.S3_GCInfo);

			DocConfig.S3_OH = ZGuid.Empty;
			DocConfig.S3_IsSystem = true;
			AssertHasError(DocConfig.S3_GCInfo, "Please do not enter a Company.");

			DocConfig.S3_GC = ZGuid.Empty;
			AssertHasError(DocConfig.S3_GCInfo, duplicateFallbackError);

			DocConfig.S3_SI = ZGuid.Empty;
			DocConfig.Validation.ValidateS3_GC();
			AssertNoErrors(DocConfig.S3_GCInfo);

			DocConfig.S3_IsSystem = false;
			AssertNoErrors(DocConfig.S3_GCInfo);

			DocConfig.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertNoErrors(DocConfig.S3_GCInfo);
		}

		public void TestValidateS3_IsSystem()
		{
			DocConfig.S3_IsSystem = true;
			AssertHasError(DocConfig.S3_IsSystemInfo, "This document configuration cannot be marked as system-defined because the containing document is not system-defined.");

			DocConfig.MenuTemplatePivot.SI_IsSystemDefined = true;
			DocConfig.Validation.ValidateS3_IsSystem();
			AssertNoErrors(DocConfig.S3_IsSystemInfo);
		}

		public void TestAllowsDifferentTypesOfConfigs()
		{
			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_IsSystemDefined = true;

			// one system and template config
			var firstConfig = menuTemplatePivot.DocConfigs.AddNew();
			firstConfig.S3_IsTemplate = true;
			firstConfig.S3_IsSystem = true;
			AssertNoErrors("firstConfig should have no validation errors", firstConfig);

			//one system and non template config
			var secondConfig = menuTemplatePivot.DocConfigs.AddNew();
			secondConfig.S3_IsTemplate = false;
			secondConfig.S3_IsSystem = true;
			AssertNoErrors("secondConfig should have no validation errors", secondConfig);

			// second system and template config
			var thirdConfig = menuTemplatePivot.DocConfigs.AddNew();
			thirdConfig.S3_IsTemplate = true;
			thirdConfig.S3_IsSystem = true;
			AssertNoErrors("thirdConfig should have no validation errors", thirdConfig);

			// non system non template with company
			var fourthConfig = menuTemplatePivot.DocConfigs.AddNew();
			fourthConfig.S3_IsSystem = false;
			fourthConfig.S3_IsTemplate = false;
			fourthConfig.S3_GC = GlbCompany.CurrentCompany.PK;
			AssertNoErrors("forthConfig should have no validation errors", fourthConfig);

			// non system non template with same company
			var fifthConfig = menuTemplatePivot.DocConfigs.AddNew();
			fifthConfig.S3_IsSystem = false;
			fifthConfig.S3_IsTemplate = false;
			fifthConfig.S3_GC = GlbCompany.CurrentCompany.PK;
			AssertHasErrors("fifthConfig should have validation error on the company", fifthConfig.S3_GCInfo);

			// checking if it allows save with different company
			var company = Factory.New<GlbCompany>();
			fifthConfig.S3_GC = company.PK;
			AssertNoErrors("fifthConfig should have no validation errors", fifthConfig);
		}

		public void TestValidateS3_IsTemplate()
		{
			DocConfig.S3_IsTemplate = true;
			AssertHasError(DocConfig.S3_IsTemplateInfo, "This document configuration cannot be marked as template because it is not system-defined.");

			DocConfig.S3_IsSystem = true;
			DocConfig.MenuTemplatePivot.SI_IsSystemDefined = true;
			DocConfig.Validation.ValidateS3_IsTemplate();
			AssertNoErrors(DocConfig.S3_IsTemplateInfo);
		}

		public void TestValidateS3_OH()
		{
			DocConfig.S3_IsSystem = true;
			DocConfig.S3_OH = ZGuid.NewZGuid();
			AssertHasError(DocConfig.S3_OHInfo, "Please do not enter a Client.");

			DocConfig.S3_OH = ZGuid.Empty;
			AssertNoErrors(DocConfig.S3_OHInfo);

			DocConfig.S3_IsSystem = false;
			DocConfig.S3_OH = ZGuid.NewZGuid();
			AssertNoErrors(DocConfig.S3_OHInfo);
		}

		#region Implementation

		StmMenuDocumentConfig DocConfig
		{
			get
			{
				if (fDocConfig == null)
				{
					StmMenuTemplatePivotBase menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
					fDocConfig = menuTemplatePivot.DocConfigs.AddNew();
				}
				return fDocConfig;
			}
		}
		StmMenuDocumentConfig fDocConfig;

		#endregion
	}
}
