using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmTemplateBaseValidationTest : TestCaseWithFactory
	{
		public void TestCannotUseConfigurableSectionsValidation()
		{
			string cannotUseConfigurableSectionsErrorExcerpt = "Cannot use 'Configurable Sections'";
			Template.SO_Name = "Freddy Fights Foo";
			Template.SO_IsUserConfigurable = true;
			AssertHasErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_IsUserConfigurable = false;
			AssertNoErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_IsUserConfigurable = true;
			Template.SO_Name = SectionRepositoryTemplateNames.System;

			AssertNoErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_Name = SectionRepositoryTemplateNames.System + " Copy";
			AssertHasErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_Name = SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.System, Core.SharedConstants.Languages.ChineseTraditional);
			AssertNoErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_Name = SectionRepositoryTemplateNames.User;
			AssertNoErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_Name = SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.User, Core.SharedConstants.Languages.German);
			AssertNoErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);

			Template.SO_Name = "Copy of " + SectionRepositoryTemplateNames.User;
			AssertHasErrorContaining(Template.SO_NameInfo, cannotUseConfigurableSectionsErrorExcerpt);
		}

		public void TestValidateSO_Name()
		{
			Template.SO_Name = "Goo";
			AssertNoErrors(Template.SO_NameInfo);

			Template.SO_Name = "";
			AssertHasError(Template.SO_NameInfo, "Please enter a Template Name.");

			Template.SO_Name = SectionRepositoryTemplateNames.User;
			AssertNoErrors(Template.SO_NameInfo);

			Template.SO_Name = SectionRepositoryTemplateNames.System;
			AssertHasError(Template.SO_NameInfo, "'" + SectionRepositoryTemplateNames.System + "' is a reserved name for section repository templates. Please enter a different name.");

			Template.SO_IsSystemDefined = true;
			Template.Validation.ValidateSO_Name();
			AssertNoErrors(Template.SO_NameInfo);

			Template.SO_Name = "Customized Document Elements";
			Template.SO_IsSystemDefined = true;
			Template.Validation.ValidateSO_Name();
			AssertHasError(Template.SO_NameInfo, "'" + Template.SO_Name + "' can only be saved as Non-System.");

			Template.SO_Name = "Customized Document Elements [ZH-CN]";
			Template.SO_IsSystemDefined = true;
			Template.Validation.ValidateSO_Name();
			AssertHasError(Template.SO_NameInfo, "'" + Template.SO_Name + "' can only be saved as Non-System.");

			Template.SO_Name = "System Document Elements [ZH-CN]";
			Template.SO_IsSystemDefined = false;
			Template.Validation.ValidateSO_Name();
			AssertHasError(Template.SO_NameInfo, "'" + Template.SO_Name + "' can only be saved as System.");

			Template.SO_Name = "System Document Elements";
			Template.SO_IsSystemDefined = false;
			Template.Validation.ValidateSO_Name();
			AssertHasError(Template.SO_NameInfo, "'" + Template.SO_Name + "' can only be saved as System.");
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSO_TemplateLengthIsNotValidated()
		{
			var fullBytes = new byte[1200000]; // Normal limit is 1MB.
			var readBytes = File.ReadAllBytes(UnitTestingConstants.TestDocumentExcelTemplateFilePath);
			readBytes.CopyTo(fullBytes, 0);
			Template.SO_Template = fullBytes;
			AssertNoNotifications(Template.SO_TemplateInfo);
		}

		public void TestSavingTemplateHasContentOutOfPrintArea()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-]DataContext=UnitTest]
{A}-[#SectionBody]
{A}-[#EndOfReport]");
			template.Factory.Save();

			using (var excelInterface = new ExcelInterface())
			using (var stream = template.GetSO_TemplateReader())
			using (var memStream = new MemoryStream())
			{
				excelInterface.LoadExcelFile(stream.ToByteArray());
				var workSheet = excelInterface.WorkSheets[0];

				var printArea = new TXlsNamedRange(((char)InternalNameRange.Print_Area).ToString(), 1, 0, "='" + workSheet.SheetName + "'!$A$1:$AD$1");
				excelInterface.Xls.SetNamedRange(printArea);

				excelInterface.SaveToStream(memStream);
				template.SO_Template = new ZBlob(memStream.GetBuffer());
			}

			AssertHasWarning(template.SO_TemplateInfo, "Some content of this template is out of the print area and will not be printed, you may need to check your modification again.");
		}

		public void TestAddorReloadNewDocBuilderTemplate()
		{
			Template.SO_Name = "System Document Elements";
			AssertNoError(Template.SO_DataContextInfo, "The DocBuilder style template's data context must be GenericFreightJob");
			Template.SO_Name = "Customized Document Elements [ZH-CN]";
			Template.SO_DataContext = "DtbBooking";
			Template.Validation.ValidateSO_DataContext();
			AssertHasError(Template.SO_DataContextInfo, "The DocBuilder style template's data context must be GenericFreightJob");
			Template.SO_DataContext = "GenericFreightJob";
			Template.Validation.ValidateSO_DataContext();
			AssertNoError(Template.SO_DataContextInfo, "The DocBuilder style template's data context must be GenericFreightJob");
		}

		#region Implementation

		StmTemplateBase Template
		{
			get { return fTemplate ?? (fTemplate = Factory.New<StmTemplateBase>()); }
		}
		StmTemplateBase fTemplate;

		#endregion
	}
}
