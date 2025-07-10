using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	public abstract class MenuCustomisationTestCase<T> : NonPersistentBusinessObjectTestCase where T : MenuCustomisation
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadUnSupportedExcelFile()
		{
			string templateFilePath = UnitTestingConstants.TestFilesDir + "TIFPage.tif";
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.None);
			template.SO_IsSystemDefined = false;
			template.SO_ExcelTemplatePath = templateFilePath;
			template.SO_Template = ZBlob.Empty;
			template.IsCheckedOutByMe = true;

			ReturnResult result = Customisation.UpdateTemplateRecord(template, templateFilePath);
			AssertEquals("result.Success should be false.", false, result.Success);
			AssertEquals(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, result.Message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewUnSupportedTemplate()
		{
			string templateFilePath = UnitTestingConstants.TestFilesDir + "TIFPage.tif";
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.None);
			template.SO_IsSystemDefined = false;
			template.SO_ExcelTemplatePath = templateFilePath;
			template.SO_Template = ZBlob.Empty;
			template.IsCheckedOutByMe = true;

			ReturnResult<StmTemplateBase> result = Customisation.AddNewTemplate(templateFilePath);
			AssertEquals("result.Success should be false.", false, result.Success);
			AssertEquals(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, result.Message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewCustomisableTemplateDoesNotCrash()
		{
			AssertNoExceptionThrown(delegate
			{ Customisation.AddNewTemplate(UnitTestingConstants.TestCustomisableTemplateFilePath); });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestAddNewTemplate()
		{
			Customisation.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			int initialTemplatesCount = Customisation.AvailableTemplates.Count;

			ReturnResult<StmTemplateBase> result = Customisation.AddNewTemplate(ValidTemplateFilePath);
			AssertEquals("Success", true, result.Success);

			int newTemplatesCount = Customisation.AvailableTemplates.Count;
			AssertEquals("AvailableTemplates.Count", initialTemplatesCount + 1, newTemplatesCount);

			string localEnterprisePath = CargoWise.BuildTools.BuildConstants.LocalEnterprisePath;

			StmTemplateBase template = result.Value;
			AssertEquals("AvailableTemplates.Contains(AddNewTemplate())", true, Customisation.AvailableTemplates.Contains(template));
			AssertEquals("AddNewTemplate().EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, template.EditingMode);
			AssertEquals("AddNewTemplate().ExcelTemplateFullPath", ValidTemplateFilePath, template.ExcelTemplateFullPath);
			AssertEquals("AddNewTemplate().SO_ExcelTemplatePath", ValidTemplateFilePath.Substring(localEnterprisePath.Length), template.SO_ExcelTemplatePath);
			AssertEquals("AddNewTemplate().IsDeleted", false, template.IsDeleted);
			AssertEquals("AddNewTemplate().SO_DataContext", DataContext.ToString(), template.SO_DataContext);
			AssertEquals("AddNewTemplate().SO_Name", Path.GetFileNameWithoutExtension(ValidTemplateFilePath), template.SO_Name);
			AssertEquals("AddNewTemplate().SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);

			foreach (KeyValuePair<string, string> pair in GetInvalidTemplateFilePathAndValidationErrorMessagePairs())
			{
				result = Customisation.AddNewTemplate(pair.Key);
				AssertEquals("Success", false, result.Success);
				AssertEquals("Message", pair.Value, result.Message);
				AssertNull("AddNewTemplate()", result.Value);
			}

			Customisation.Factory.Save();
			T newCustomisation = (T)GetNewBusinessObject();
			AssertEquals("AvailableTemplates.Count", newTemplatesCount, newCustomisation.AvailableTemplates.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewTemplateReleaseBuild()
		{
			Customisation.IgnoreIsDebugCheckForTest = true;
			Customisation.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			int initialTemplatesCount = Customisation.AvailableTemplates.Count;

			ReturnResult<StmTemplateBase> result = Customisation.AddNewTemplate(ValidTemplateFilePath);
			AssertEquals("Success", true, result.Success);

			int newTemplatesCount = Customisation.AvailableTemplates.Count;
			AssertEquals("AvailableTemplates.Count", initialTemplatesCount + 1, newTemplatesCount);

			StmTemplateBase template = result.Value;
			AssertEquals("AvailableTemplates.Contains(AddNewTemplate())", true, Customisation.AvailableTemplates.Contains(template));
			AssertEquals("AddNewTemplate().EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, template.EditingMode);
			AssertEquals("AddNewTemplate().SO_ExcelTemplatePath", ValidTemplateFilePathReleaseBuild, template.SO_ExcelTemplatePath);
			AssertEquals("AddNewTemplate().IsDeleted", false, template.IsDeleted);
			AssertEquals("AddNewTemplate().SO_DataContext", DataContext.ToString(), template.SO_DataContext);
			AssertEquals("AddNewTemplate().SO_Name", Path.GetFileNameWithoutExtension(ValidTemplateFilePath), template.SO_Name);
			AssertEquals("AddNewTemplate().SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);

			foreach (KeyValuePair<string, string> pair in GetInvalidTemplateFilePathAndValidationErrorMessagePairs())
			{
				result = Customisation.AddNewTemplate(pair.Key);
				AssertEquals("Success", false, result.Success);
				AssertEquals("Message", pair.Value, result.Message);
				AssertNull("AddNewTemplate()", result.Value);
			}

			Customisation.Factory.Save();
			T newCustomisation = (T)GetNewBusinessObject();
			AssertEquals("AvailableTemplates.Count", newTemplatesCount, newCustomisation.AvailableTemplates.Count);
		}

		public void TestAddTemplatePivot()
		{
			StmMenuItemBase menu = Customisation.Menus[2];
			StmTemplateBase template = Customisation.AvailableTemplates[1];
			template.SO_IsSystemDefined = true;
			template.SO_Name = SectionRepositoryTemplateNames.User;
			Customisation.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;

			StmMenuTemplatePivotBase pivot = Customisation.AddTemplatePivot(menu, template);
			AssertEquals("AddTemplatePivot().SI_SO", template.PK, pivot.SI_SO);
			AssertEquals("AddTemplatePivot().SI_SU", menu.PK, pivot.SI_SU);
			AssertEquals("AddTemplatePivot().SI_DocumentTitle", "New " + Customisation.DescriptionForTest, pivot.SI_DocumentTitle);
			AssertEquals("AddTemplatePivot().EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, pivot.EditingMode);
			AssertHasErrors(pivot.SI_IsSystemDefinedInfo);
			AssertHasErrors(pivot.SO_NameInfo);

			AssertNull("AddTemplatePivot()", Customisation.AddTemplatePivot(null, null));
		}

		public void TestCanCheckOutTemplate()
		{
			AssertEquals("CanCheckOutTemplate()", false, Customisation.CanCheckOutTemplate(null));

			var template = Factory.New<StmTemplateBase>();
			template.IsCheckedOutByMe = true;
			AssertEquals("CanCheckOutTemplate()", false, Customisation.CanCheckOutTemplate(template));

			template.IsCheckedOutByMe = false;
			AssertEquals("CanCheckOutTemplate()", true, Customisation.CanCheckOutTemplate(template));
		}

		public void TestCopyTemplate()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Squish";
			StmTemplateBase copy = Customisation.CopyTemplate(template);
			AssertEquals("CopyTemplate().SO_Name", "Copy of Squish", copy.SO_Name);
			AssertEquals("AvailableTemplates.Contains(CopyTemplate())", true, Customisation.AvailableTemplates.Contains(copy));
		}

		public void TestDocumentSupporter()
		{
			AssertEquals("DocumentSupporter.GetType()", SupportedDocumentSupporter.GetType(), ((IDocumentSupportable)Customisation).DocumentSupporter.GetType());
		}

		public virtual void TestEditingMode()
		{
			AssertEquals("EditingMode", MenuEditingMode.AllowAll, Customisation.EditingMode);

			Customisation.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Customisation.EditingMode);
			AssertEquals("AvailableTemplates.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Customisation.AvailableTemplates.EditingMode);
			AssertEquals("Menus.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Customisation.Menus.EditingMode);

			Customisation.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Customisation.EditingMode);
			AssertEquals("AvailableTemplates.EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Customisation.AvailableTemplates.EditingMode);
			AssertEquals("Menus.EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Customisation.Menus.EditingMode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateTemplateRecord()
		{
			ReturnResult result;
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = DataContext.ToString();
			template.SO_Name = "Template A";
			AssertEquals("template.SO_IsSystemDefined", false, template.SO_IsSystemDefined);
			AssertEquals("template.SO_ExcelTemplatePath", string.Empty, template.SO_ExcelTemplatePath);

			using (TempFile tempFile = TempFile.NewFromFile(ValidTemplateFilePath))
			{
				result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				var expectedTemplatePath = template.SO_Name + Path.GetExtension(ValidTemplateFilePath);
				AssertEquals("Success", true, result.Success);
				AssertEquals("template.ExcelTemplateFullPath", expectedTemplatePath, template.SO_ExcelTemplatePath);
				AssertEquals("template.SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);
			}

			result = Customisation.UpdateTemplateRecord(template, ValidTemplateFilePath);
			AssertEquals("Success", true, result.Success);
			AssertEquals("template.ExcelTemplateFullPath", ValidTemplateFilePath, template.ExcelTemplateFullPath);

			template.SO_Template = ZBlob.Empty;
			template.SO_ExcelTemplatePath = "";
			template.SO_DataContext = nameof(Core.Constants.DataContext.AccountingVoucher);
			result = Customisation.UpdateTemplateRecord(template, ValidTemplateFilePath);
			AssertUpdateTemplateRecordCore(result, template);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateTemplateRecordWithInvalidCharactersInSOName()
		{
			ReturnResult result;
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = DataContext.ToString();
			template.SO_Name = "Template <A>";
			AssertEquals("template.SO_IsSystemDefined", false, template.SO_IsSystemDefined);
			AssertEquals("template.SO_ExcelTemplatePath", string.Empty, template.SO_ExcelTemplatePath);

			using (TempFile tempFile = TempFile.NewFromFile(ValidTemplateFilePath))
			{
				result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				var expectedTemplatePath = MakeFilenameSafe.MakeSafe(template.SO_Name + Path.GetExtension(ValidTemplateFilePath), '_');
				AssertEquals("Success", true, result.Success);
				AssertEquals("template.ExcelTemplateFullPath", expectedTemplatePath, template.SO_ExcelTemplatePath);
				AssertEquals("template.SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);
			}
		}

		public void TestUpdateTemplateRecordForCustomisableTemplateDoesNotCrash()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var template = Factory.New<StmTemplateBase>();
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls");
				AssertNoExceptionThrown(() => Customisation.UpdateTemplateRecord(template, tempFileName));
			}
		}

		public abstract void TestAvailableTemplates();

		public abstract void TestGetPrintTask();

		public abstract void TestMenus();

		T customisation;

		protected T Customisation
		{
			get { return customisation ?? (customisation = (T)GetNewBusinessObject()); }
		}

		protected abstract Core.Constants.DataContext DataContext { get; }

		protected abstract string ValidTemplateFilePath { get; }

		protected abstract string ValidTemplateFilePathReleaseBuild { get; }

		protected abstract MenuCustomisationDocumentSupporter SupportedDocumentSupporter { get; }

		protected abstract KeyValuePair<string, string>[] GetInvalidTemplateFilePathAndValidationErrorMessagePairs();

		protected virtual void AssertUpdateTemplateRecordCore(ReturnResult result, StmTemplateBase template)
		{
			AssertEquals("Success", true, result.Success);
			AssertEquals("Message", null, result.Message);
			AssertNotEquals("template.ExcelTemplateFullPath", "", template.SO_ExcelTemplatePath);
			AssertEquals("template.SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);
			AssertEquals("None", template.SO_DataContext);
		}

		protected void AssertCollectionEquals(IBusinessObjectCollection expectedCollection, IBusinessObjectCollection actualCollection, string id)
		{
			Assert("Precondition: There should be at least one element in the collection.", expectedCollection.Count > 0);
			AssertEquals(id + ".Count", expectedCollection.Count, actualCollection.Count);
			foreach (BusinessObject element in expectedCollection)
			{
				AssertEquals(id + ".Contains(" + element.PK + ")", true, actualCollection.Contains(element));
			}
		}
	}
}
