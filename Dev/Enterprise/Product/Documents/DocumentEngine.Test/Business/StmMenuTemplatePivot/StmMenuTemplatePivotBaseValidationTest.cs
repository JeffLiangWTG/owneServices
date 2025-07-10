using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngineCore;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuTemplatePivotBaseValidationTest : TestCaseWithFactory
	{
		public void TestSI_RT_DocType()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_BusinessContext = "RepCustomsReports";
			var template = Factory.New<StmTemplate>();
			pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menu.PK;
			pivot.SI_SO = template.PK;

			pivot.Validation.ValidateSI_RT_DocType();
			AssertHasWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);
			AssertNoError(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);

			pivot.SI_IsSystemDefined = true;
			AssertHasWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);
			AssertNoError(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);

			menu.SU_BusinessContext = "Customs";
			pivot.SI_IsSystemDefined = false;
			AssertHasWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);
			AssertNoError(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);

			pivot.SI_IsSystemDefined = true;
			AssertNoWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);
			AssertHasError(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);

			pivot.SI_IsSystemDefined = false;
			AssertHasWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);
			AssertNoError(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveDocumentType);

			pivot.SI_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;
			AssertNoNotifications(pivot.SI_RT_DocTypeInfo);

			pivot.SI_IsSystemDefined = true;
			AssertNoNotifications(pivot.SI_RT_DocTypeInfo);
		}

		public void TestPrintCopyType()
		{
			AssertEquals("PrintCopyTypeList.Count", 5, Pivot.PrintCopyTypeList.Count);
			Assert("Contains ''", Pivot.PrintCopyTypeList.ContainsCode(""));

			AssertEquals("PrintCopyTypeList[0].Code", "", Pivot.PrintCopyTypeList[0].Code);
			AssertEquals("PrintCopyTypeList[1].Code", "PRN", Pivot.PrintCopyTypeList[1].Code);
			AssertEquals("PrintCopyTypeList[2].Code", "FAX", Pivot.PrintCopyTypeList[2].Code);
			AssertEquals("PrintCopyTypeList[3].Code", "EML", Pivot.PrintCopyTypeList[3].Code);
			AssertEquals("PrintCopyTypeList[4].Code", "ALL", Pivot.PrintCopyTypeList[4].Code);

			AssertEquals("HasErrors", false, Pivot.SI_PrintCopyTypeInfo.HasErrors());

			Pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			AssertEquals("HasErrors", false, Pivot.SI_PrintCopyTypeInfo.HasErrors());

			Pivot.SI_PrintCopyType = "ZZZ";
			AssertEquals("HasErrors", true, Pivot.SI_PrintCopyTypeInfo.HasErrors());

			Pivot.SI_PrintCopyType = nameof(PrintCopyType.EML);
			AssertEquals("HasErrors", false, Pivot.SI_PrintCopyTypeInfo.HasErrors());
		}

		public void TestMenuTemplateFilter()
		{
			AssertEquals("HasErrors", false, Pivot.SI_MenuTemplateFilterInfo.HasErrors());

			Pivot.SI_MenuTemplateFilter = "HBLFFF";
			AssertEquals("HasErrors", true, Pivot.SI_MenuTemplateFilterInfo.HasErrors());
			AssertStartsWith("Starts with 'Filter format'", "Filter format", Pivot.SI_MenuTemplateFilterInfo.GetErrors().GetFirstMessage());

			Pivot.SI_MenuTemplateFilter = "HCC=FFF";
			AssertEquals("HasErrors", true, Pivot.SI_MenuTemplateFilterInfo.HasErrors());
			AssertStartsWith("Starts with ''HCC' code'", "'HCC' code", Pivot.SI_MenuTemplateFilterInfo.GetErrors().GetFirstMessage());
			AssertContains("contains 'HBL'", nameof(MenuTemplateFilterType.HBL), Pivot.SI_MenuTemplateFilterInfo.GetErrors().GetFirstMessage());

			Pivot.SI_MenuTemplateFilter = "HBL=FFF";
			AssertEquals("HasErrors", false, Pivot.SI_MenuTemplateFilterInfo.HasErrors());
		}

		public void TestMenuTemplateFilter_ShouldValidateUsingExpressionEvaluator()
		{
			const string stringExpressionFormat = "The following expression {0} is incorrect. Please make sure:\r\n\u2022 you are using a True/False expression\r\n\u2022 you are not mixing legacy filters(e.g.CTY = AU) with other filters(e.g. \"<PropertyName>\" == \"My value\")\r\n\u2022 if you use a legacy filter, they cannot be combined.";

			AssertEquals("HasErrors", false, Pivot.SI_MenuTemplateFilterInfo.HasErrors());

			Pivot.SI_MenuTemplateFilter = "HBL=ABC && \"<BowTies>\" == \"Cool\"";
			var filterExpressionFormatError = string.Format(stringExpressionFormat, Pivot.SI_MenuTemplateFilter);
			AssertHasError(Pivot.SI_MenuTemplateFilterInfo, filterExpressionFormatError);

			Pivot.SI_MenuTemplateFilter = "<BowTies> == Cool";
			filterExpressionFormatError = string.Format(stringExpressionFormat, Pivot.SI_MenuTemplateFilter);
			AssertHasError(Pivot.SI_MenuTemplateFilterInfo, filterExpressionFormatError);

			Pivot.SI_MenuTemplateFilter = "\"<BowTies>\" == \"Cool\"";
			AssertNoErrors(Pivot.SI_MenuTemplateFilterInfo);
		}

		public void TestSystemOnlyPivotCannotUseClientMenu()
		{
			Pivot.SI_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;
			Pivot.Menu.SU_IsClientSpecific = true;
			Pivot.Menu.SU_IsSystemDefined = true;

			Pivot.Template.SO_IsClientSpecific = false;
			Pivot.Template.SO_IsSystemDefined = true;

			Pivot.SI_IsClientSpecific = false;
			Pivot.SI_IsSystemDefined = true;

			Pivot.Validation.ValidateSI_IsSystemDefined();
			AssertEquals("Errors", 1, Pivot.Notifications.GetErrors().Count());
			Assert("Row Error message", Pivot.Notifications.GetErrors().GetFirstMessage().IndexOf("System defined joining relationship cannot be added to a client specific document / report.") >= 0);
		}

		public void TestSystemOnlyPivotCannotUseUserMenu()
		{
			Pivot.SI_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;
			Pivot.Menu.SU_IsClientSpecific = false;
			Pivot.Menu.SU_IsSystemDefined = false;

			Pivot.Template.SO_IsClientSpecific = false;
			Pivot.Template.SO_IsSystemDefined = true;

			Pivot.SI_IsClientSpecific = false;
			Pivot.SI_IsSystemDefined = true;

			Pivot.Validation.ValidateSI_IsSystemDefined();
			AssertEquals("Errors", 1, Pivot.Notifications.GetErrors().Count());
			Assert("Row Error message", Pivot.Notifications.GetErrors().GetFirstMessage().IndexOf("System defined joining relationship cannot be added to a user defined document / report.") >= 0);
		}

		public void TestSystemDefinedPivotAllowsUserDefinedTemplatesForSystemBillOfLading()
		{
			Pivot.SI_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;
			Pivot.Menu.SU_IsClientSpecific = false;
			Pivot.Menu.SU_IsSystemDefined = false;
			Assert("Precondition.", Pivot.SI_SU != ForwardingConstants.SystemFormMenuItems.BillOfLadingPK);

			Pivot.Template.SO_IsClientSpecific = false;
			Pivot.Template.SO_IsSystemDefined = true;

			Pivot.SI_IsClientSpecific = false;
			Pivot.SI_IsSystemDefined = true;

			Pivot.Validation.ValidateSI_IsSystemDefined();
			AssertHasErrors("Menu item is not system bill of lading, so validation should fail.", Pivot.SI_IsSystemDefinedInfo);

			Pivot.SI_SU = ForwardingConstants.SystemFormMenuItems.BillOfLadingPK;
			Pivot.Validation.ValidateSI_IsSystemDefined();
			AssertNoErrors("Menu item is system bill of lading, so validation should pass.", Pivot.SI_IsSystemDefinedInfo);
		}

		public void TestValidateSO_Name()
		{
			Pivot.Template.SO_Name = SectionRepositoryTemplateNames.User;
			Pivot.Validation.ValidateSO_Name();
			AssertHasError(Pivot.SO_NameInfo, "The '" + SectionRepositoryTemplateNames.User + "' template is only meant for adding new template sections. Please link this document to the '" + SectionRepositoryTemplateNames.System + "' template instead.");

			var languageSpecificName = SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.User, Core.SharedConstants.Languages.ChineseSimplified);
			Pivot.Template.SO_Name = languageSpecificName;
			Pivot.Validation.ValidateSO_Name();
			AssertHasError(Pivot.SO_NameInfo, $"The '{languageSpecificName}' template is only meant for adding new template sections. Please link this document to the '{SectionRepositoryTemplateNames.System}' template instead.");

			Pivot.Template.SO_Name = SectionRepositoryTemplateNames.System;
			Pivot.Validation.ValidateAll();
			AssertNoErrors(Pivot.SO_NameInfo);
		}

		public void TestValidateArchiveNoDocType()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_BusinessContext = "RepCustomsReports";
			var template = Factory.New<StmTemplate>();
			pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menu.PK;
			pivot.SI_SO = template.PK;

			pivot.Validation.ValidateSI_RT_DocType();
			AssertNoWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveArchivableDocumentType);

			pivot.Menu.SU_IncludeDocInArchive = "YES";
			pivot.Validation.ValidateSI_RT_DocType();
			AssertHasWarning(pivot.SI_RT_DocTypeInfo, StmMenuTemplatePivotBaseValidation.ErrorMustHaveArchivableDocumentType);
		}

		#region Implementation

		StmMenuTemplatePivotBase Pivot
		{
			get
			{
				if (pivot == null)
				{
					var menu = Factory.New<StmMenuItem>();
					var template = Factory.New<StmTemplate>();
					pivot = Factory.New<StmMenuTemplatePivotBase>();
					pivot.SI_SU = menu.PK;
					pivot.SI_SO = template.PK;
				}

				return pivot;
			}
		}

		StmMenuTemplatePivotBase pivot;

		#endregion
	}
}
