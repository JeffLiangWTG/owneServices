using System;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentCommandValidationTest : TestCaseWithFactory
	{
		#region TestValidateSU_MenuDataContext

		public void TestValidateSU_MenuDataContext()
		{
			var menu = Factory.New<DocumentCommand>();
			menu.Parent = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "BenFred";

			var link = Factory.New<StmMenuTemplatePivotBase>();
			link.SI_SO = template.PK;
			link.SI_SU = menu.PK;

			menu.Documents.Add(link);
			menu.SU_EmailSubjectLine = "Foobar: <Name>Foobar!";

			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoMessageErrors(menu.SU_MenuDataContextInfo);
			AssertNoMessageErrors(menu.SU_EmailSubjectLineInfo);

			menu.Documents.RemoveAll();
			menu.ChildMenus.AddNew();
			menu.SU_EmailSubjectLine = string.Empty;

			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertHasMessageError(menu.SU_MenuDataContextInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");
			AssertHasMessageError(menu.SU_EmailSubjectLineInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");

			menu.SU_EmailSubjectLine = "Foobar: <Name>Foobar!";
			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertHasMessageError(menu.SU_MenuDataContextInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");
			AssertHasMessageError(menu.SU_EmailSubjectLineInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");

			menu.SU_IsDocPack = true;
			AssertHasError(menu.SU_MenuDataContextInfo, "Please select an Email Subject Data Context for your customized Email Subject Line replacements.");

			menu.SU_MenuDataContext = "Blah";
			AssertHasError(menu.SU_MenuDataContextInfo, "Please select a valid Email Subject Data Context for your customized Email Subject Line replacements.");
		}

		#endregion

		#region TestValidateSU_MenuDataContextWhenAnInvalidContextIsEntered

		public void TestValidateSU_MenuDataContextWhenAnInvalidContextIsEntered()
		{
			DocumentCommand menu = Factory.New<DocumentCommand>();
			menu.Parent = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "BenFred";

			var link = Factory.New<StmMenuTemplatePivotBase>();
			link.SI_SO = template.PK;
			link.SI_SU = menu.PK;

			menu.SU_EmailSubjectLine = "SomeEmailSubject";
			menu.SU_MenuDataContext = "SomeInvalidThing";

			AssertHasError(menu.SU_MenuDataContextInfo, "Please select a valid Email Subject Data Context for your customized Email Subject Line replacements.");
		}

		#endregion

		#region TestValidateSU_MenuDataContextWithDifferentSupportedBODataSources

		public void TestValidateSU_MenuDataContextWithDifferentSupportedBODataSources()
		{
			var bo = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			var menu = Factory.New<DocumentCommand>();
			menu.Parent = bo;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "BenFred";

			var link = Factory.New<StmMenuTemplatePivotBase>();
			link.SI_SO = template.PK;
			link.SI_SU = menu.PK;

			menu.SU_MenuDataContext = ".SomeInvalidThing";
			menu.Validation.ValidateSU_MenuDataContext();
			AssertHasError(menu.SU_MenuDataContextInfo, "Please select a valid Email Subject Data Context for your customized Email Subject Line replacements.");

			var documentSupporter = bo.DocumentSupporter as DocumentCommandTest.DummyBusinessObjectDocumentSupporter;

			documentSupporter.AddSupportedBODataSource(".SomeValidThing");
			menu.SU_MenuDataContext = ".SomeValidThing";
			menu.Validation.ValidateSU_MenuDataContext();
			AssertNoErrors(menu.SU_MenuDataContextInfo);

			Factory.Save();

			documentSupporter.RemoveSupportedBODataSource(".SomeValidThing");
			menu.SU_MenuDataContext = ".SomeValidThing";
			menu.Validation.ValidateSU_MenuDataContext();
			AssertNoErrors(menu.SU_MenuDataContextInfo);

			menu.SU_MenuDataContext = ".SomeInvalidThing";
			menu.Validation.ValidateSU_MenuDataContext();
			AssertHasError(menu.SU_MenuDataContextInfo, "Please select a valid Email Subject Data Context for your customized Email Subject Line replacements.");
		}

		#endregion

		#region TestValidateSU_IncludeDocInArchiveWhenDeveloperCustomises

		public void TestValidateSU_IncludeDocInArchiveWhenDeveloperCustomises()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = Guid.NewGuid().ToString();

			//Test list validation
			command.SU_IncludeDocInArchive = "ZZZ";
			AssertHasError(command.SU_IncludeDocInArchiveInfo, "Enter a valid " + command.SU_IncludeDocInArchiveInfo.Description + ".");

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			//Test doc pack
			command.SU_IsDocPack = true;

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertHasError(command.SU_IncludeDocInArchiveInfo, "A Doc Pack cannot be selected for archiving. Please choose individual documents instead.");

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			//Test user defined
			command.SU_IsDocPack = false;

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);
		}

		#endregion

		#region TestValidateSU_IncludeDocInArchiveWhenUserCustomises

		public void TestValidateSU_IncludeDocInArchiveWhenUserCustomises()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = Guid.NewGuid().ToString();

			//Test list validation
			command.SU_IncludeDocInArchive = "ZZZ";
			AssertHasError(command.SU_IncludeDocInArchiveInfo, "Enter a valid " + command.SU_IncludeDocInArchiveInfo.Description + ".");

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			//Test doc pack
			command.SU_IsDocPack = true;

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertHasError(command.SU_IncludeDocInArchiveInfo, "A Doc Pack cannot be selected for archiving. Please choose individual documents instead.");

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			//Test user defined
			command.SU_IsDocPack = false;

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.Yes;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);

			command.SU_IncludeDocInArchive = ArchiveConstants.IncludeDocInArchiveCodes.No;
			AssertNoErrors(command.SU_IncludeDocInArchiveInfo);
		}

		#endregion

		#region TestValidateSU_FilterList_ShouldValidateUsingExpressionEvaluator

		public void TestValidateSU_FilterList_ShouldValidateUsingExpressionEvaluator()
		{
			const string stringExpressionFormat = "The following expression {0} is incorrect. Please make sure:\r\n\u2022 you are using a True/False expression\r\n\u2022 you are not mixing legacy filters(e.g.CTY = AU) with other filters(e.g. \"<PropertyName>\" == \"My value\")\r\n\u2022 if you use a legacy filter, they cannot be combined.";

			var command = Factory.New<DocumentCommand>();
			string filterExpressionFormatError;
			AssertEquals("HasErrors", false, command.SU_FilterListInfo.HasErrors());

			command.SU_FilterList = "SUP=ABC && \"<BowTies>\" == \"Cool\"";
			filterExpressionFormatError = string.Format(stringExpressionFormat, command.SU_FilterList);
			AssertHasError(command.SU_FilterListInfo, filterExpressionFormatError);

			command.SU_FilterList = "<BowTies> == Cool";
			filterExpressionFormatError = string.Format(stringExpressionFormat, command.SU_FilterList);
			AssertHasError(command.SU_FilterListInfo, filterExpressionFormatError);

			command.SU_FilterList = "\"<BowTies>\" == \"Cool\"";
			AssertNoErrors(command.SU_FilterListInfo);

			command.SU_FilterList = "\"<CurrentCompany.Country.Code>\" == \"US\"&&\"<DecForDocMessageTypes>\".Contains(\",INB,\")";
			AssertNoErrors(command.SU_FilterListInfo);
		}
		#endregion

		public void TestValidateEmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup()
		{
			var menu = Factory.New<DocumentCommand>();
			menu.Parent = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "BenFred";

			var link = Factory.New<StmMenuTemplatePivotBase>();
			link.SI_SO = template.PK;
			link.SI_SU = menu.PK;

			menu.Documents.Add(link);
			menu.SU_EmailSubjectLine = "Foobar: <Name>Foobar!";

			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertNoMessageErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoMessageErrors(menu.SU_MenuDataContextInfo);

			menu.Documents.RemoveAll();
			menu.ChildMenus.AddNew();

			menu.SU_IsSystemDefined = true;
			menu.SU_EmailSubjectLine = string.Empty;
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertNoMessageErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoMessageErrors(menu.SU_MenuDataContextInfo);

			menu.SU_IsSystemDefined = true;
			menu.SU_EmailSubjectLine = "Foobar: <Name>Foobar!";
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoMessageErrors(menu.SU_EmailSubjectLineInfo);
			AssertHasError(menu.SU_MenuDataContextInfo, "Please select an Email Subject Data Context for your customized Email Subject Line replacements.");

			menu.SU_IsSystemDefined = false;
			menu.SU_EmailSubjectLine = string.Empty;
			AssertNoErrors(menu.SU_EmailSubjectLineInfo);
			AssertNoErrors(menu.SU_MenuDataContextInfo);
			AssertHasMessageError(menu.SU_EmailSubjectLineInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");
			AssertHasMessageError(menu.SU_MenuDataContextInfo, "This field is only used when the document has at least one template or is flagged as a Doc Pack.");
		}

		public void TestSpotSystemDefinedDocumentsReadonly()
		{
			var query2 = new ZQuery();
			query2.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_EmailSubjectLine, SQLComparisonOperator.NotEqual, "");
			query2.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_MenuDataContext, SQLComparisonOperator.NotEqual, "");
			var query = new ZQuery(StmMenuItemSchema.SU_IsSystemDefined, true);
			query.AddToFilter(query2);

			var errors = new StringBuilder();
			var documentCommands = Factory.Load<DocumentCommand>(query);

			foreach (var item in documentCommands)
			{
				if (item.EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup)
				{
					errors.AppendLine("SU_PK: " + item.PK + " - MenuName: " + item.SU_MenuName + " - BusinessContext: " + item.SU_BusinessContext);
				}
			}

			AssertEquals("The documents spotted by this test should be fixed by cleaning up SU_EmailSubjectLine and SU_MenuDataContext: " + errors, string.Empty, errors.ToString());
		}
	}
}
