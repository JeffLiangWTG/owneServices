using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmTemplateBase))]
	sealed class StmTemplateBaseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveTemplateChangeToEdoc()
		{
			var templateData1 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
			var templateData2 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			var templateData3 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3.]
{A}-[#EndOfReport]");

			if (Factory != null)
			{
				var template = Factory.NewWithValidTestData<StmTemplateBase>();
				template.SO_Template = templateData1;
				template.SO_Name = "ATest";
				template.SO_ExcelTemplatePath = "Test.xls";
				Factory.Save();
				AssertEquals("The template should not be saved to eDoc when creating new template",0, template.DocManagerInfo().Files.Count);

				var storageMain = template.DocManagerInfo().MasterFactory.GetStorageMainForPK(template.PK);
				template.SO_Template = templateData2;
				Factory.Save();
				AssertEquals("The template should be saved to eDoc when editing template", 1, template.DocManagerInfo().Files.Count);
				AssertEquals("ATest.xls", template.DocManagerInfo().Files[0].FileName);
				using (var reader = template.DocManagerInfo().Files[0].GetImageDataReader())
				{
					AssertArrayEqualsByElements("The old template should be saved to eDoc", templateData1, reader.ConvertToByteArrayAndCloseStream());
				}

				template.SO_Template = templateData3;
				Factory.Save();

				AssertEquals("The template should be saved to eDoc when editing template", 2, template.DocManagerInfo().Files.Count);
				using (var reader = template.DocManagerInfo().Files[1].GetImageDataReader())
				{
					AssertArrayEqualsByElements("The old template should be saved to eDoc", templateData2, reader.ConvertToByteArrayAndCloseStream());
				}

				template.SO_Name = "456";
				Factory.Save();
				AssertEquals("The template should not be saved to eDoc if SO_Template not been changed", 2, template.DocManagerInfo().Files.Count);

				template.Delete();
				Assert(((BusinessObject)storageMain).IsDeleted);
			}
		}

		public void TestSaveTemplateChangeToEdocIfStoredNumberOfSupersededTemplatesExceedLimit()
		{
			DocumentsDataRegistry.Instance.StoredNumberOfSupersededTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var templateData1 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
			var templateData2 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			var templateData3 =  DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3.]
{A}-[#EndOfReport]");
			if (Factory != null)
			{
				var template = Factory.NewWithValidTestData<StmTemplateBase>();
				template.SO_Template = templateData1;
				template.SO_ExcelTemplatePath = "Test.xsl";
				Factory.Save();
				AssertEquals(0, template.DocManagerInfo().Files.Count);

				template.SO_Template = templateData2;
				Factory.Save();
				AssertEquals(1, template.DocManagerInfo().Files.Count);

				using (var reader = template.DocManagerInfo().Files[0].GetImageDataReader())
				{
					AssertArrayEqualsByElements("Should save old template", templateData1, reader.ConvertToByteArrayAndCloseStream());
				}
				template.SO_Template = templateData3;
				Factory.Save();

				AssertEquals(1, template.DocManagerInfo().Files.Count);
				using (var reader = template.DocManagerInfo().Files[0].GetImageDataReader())
				{
					AssertArrayEqualsByElements("The oldest record should be removed", templateData2, reader.ConvertToByteArrayAndCloseStream());
				}
			}
		}

		public void TestDeleteTemplateThatIsBeingUsedByAPrivateMenuItem()
		{
			var businessContext = nameof(BusinessContext.Test);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();
			var otherStaff = Factory.New<GlbStaff>();
			otherStaff.GS_Code = "XXX";

			var documentCommand1 = Factory.New<DocumentCommand>();
			documentCommand1.SU_BusinessContext = businessContext;
			documentCommand1.SU_MenuIndex = 1;
			documentCommand1.SU_MenuName = "Document A";
			documentCommand1.SU_GS_NKStaffCode = ZString.Empty;

			var documentCommand2 = Factory.New<DocumentCommand>();
			documentCommand2.SU_BusinessContext = businessContext;
			documentCommand2.SU_MenuIndex = 2;
			documentCommand2.SU_MenuName = "Document B";
			documentCommand2.SU_GS_NKStaffCode = staff.GS_Code;

			var documentCommand3 = Factory.New<DocumentCommand>();
			documentCommand3.SU_BusinessContext = businessContext;
			documentCommand3.SU_MenuIndex = 3;
			documentCommand3.SU_MenuName = "Document C";
			documentCommand3.SU_GS_NKStaffCode = otherStaff.GS_Code;

			var documentCommand4 = Factory.New<DocumentCommand>();
			documentCommand4.SU_BusinessContext = businessContext;
			documentCommand4.SU_MenuIndex = 4;
			documentCommand4.SU_MenuName = "Document D";
			documentCommand4.SU_GS_NKStaffCode = ZString.Empty;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				var menuItems = new StmMenuItemBaseCollection(Factory);
				menuItems.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting(businessContext, true));

				AssertMultilineASCIIEquals("Pre-condition: Document C should not be visible.",
@"Document A
Document B
Document D", GetMenuItemsString(menuItems));

				var template = Factory.New<StmTemplateBase>();
				template.SO_Name = "My Template";

				var document = documentCommand3.Documents.AddNew();
				document.SI_SO = template.PK;
				document.SI_SU = documentCommand3.PK;

				AssertEquals("Should not be able to delete because Document C is holding it up.", false, template.CanDelete);
				AssertExceptionThrown("Error message should advise that owner because Document C is hidden.", typeof(TemplateInUseException),
@"Template 'My Template' cannot be deleted because it is in use by the following document menus:
   - Document C (Test)  <Private>  <Staff: XXX>

You need to remove this template from that document first before you can delete this template.

Private document menus may only be seen by the staff member that created it.",
				delegate
				{
					template.Delete();
				});
			}
		}

		public void TestGetSystemDocumentElements()
		{
			var template = Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, "System Document Elements"));
			AssertEquals("GetSystemDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System));
		}

		public void TestGetSystemDocumentElementsForEnglishLanguage()
		{
			var template = Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, "System Document Elements"));
			AssertEquals("GetSystemDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.SharedConstants.Languages.English));
		}

		public void TestGetSystemDocumentElementsForOtherLanguage()
		{
			var template = Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, "System Document Elements [GRM]"));
			AssertEquals("GetSystemDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.SharedConstants.Languages.German));
		}

		public void TestGetCustomizedDocumentElements()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Customized Document Elements";
			AssertEquals("GetCustomizedDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized));
		}

		public void TestGetCustomizedDocumentElementsForEnglishLanguage()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Customized Document Elements";
			AssertEquals("GetCustomizedDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.English));
		}

		public void TestGetCustomizedDocumentElementsForOtherLanguage()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Customized Document Elements [NL-NL]";
			AssertEquals("GetCustomizedDocumentElements", template, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.Dutch));
		}

		public void TestLanguageCode()
		{
			var template = Factory.New<StmTemplateBase>();

			Action<string, StmTemplateBase> assertLanguageCode = (expected, actual) =>
			{
				var message = string.Format("The template [{0}] language code should be [{1}].", actual.SO_Name, expected);
				AssertEquals(message, expected, actual.DocBuilderLanguageCode);
			};

			template.SO_Name = "System Document Elements";
			assertLanguageCode(Enterprise.Core.SharedConstants.Languages.English, template);

			template.SO_Name = "System Document Elements [DE-DE]";
			assertLanguageCode(Enterprise.Core.SharedConstants.Languages.German, template);

			template.SO_Name = "System Document Elements [NL-NL]";
			assertLanguageCode(Enterprise.Core.SharedConstants.Languages.Dutch, template);

			template.SO_Name = SectionRepositoryTemplateNames.User + " [NL-NL]";
			assertLanguageCode(Enterprise.Core.SharedConstants.Languages.Dutch, template);

			template.SO_Name = "System Document Elements [EN]";
			assertLanguageCode(Enterprise.Core.SharedConstants.Languages.English, template);

			template.SO_Name = "Pre Alert";
			assertLanguageCode(string.Empty, template);
		}

		public void TestInvalidateTemplateSections()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var templateSection = systemTemplate.TemplateSections[0];
			systemTemplate.InvalidateTemplateSections();
			AssertNotEquals("systemTemplate.TemplateSections[0]", templateSection, systemTemplate.TemplateSections[0]);
		}

		public void TestTemplateSystemAndUserSections()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var userTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, SectionRepositoryTemplateNames.User, ConfigurableTemplateTestHelper.ConfigurableStripsForTesting);
			AssertNotNull("User section should be found.", userTemplate.TemplateSections.Find("Generic Section 1"));
		}

		public void TestTemplateSections()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			AssertNotNull(template);
			AssertNotNull(template.TemplateSections);
			AssertEquals("template.TemplateSections.Count", 12, template.TemplateSections.Count);
		}

		public void TestTemplateConfigurableOnlySections()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			AssertNotNull(template);
			AssertNotNull(template.TemplateConfigurableOnlySections);
			AssertEquals("template.TemplateConfigurableOnlySections.Count", 10, template.TemplateConfigurableOnlySections.Count);
		}

		public void TestTemplateSectionsWithNonConfigurableTemplate()
		{
			var template = ConfigurableTemplateTestHelper.CreateNonConfigurableTemplate(Factory);
			AssertNotNull(template);
			AssertNotNull(template.TemplateSections);
			AssertEquals("template.TemplateSections.Count", 0, template.TemplateSections.Count);
		}

		public void TestIsAutoLogged()
		{
			TemplateTest template = Factory.New<TemplateTest>();
			AssertEquals("Template should be auto logged", true, template.IsAutoLoggedForTest);
		}

		public void TestCanModifyExcelTemplate()
		{
			testTemplate.SO_Name = "Template";
			testTemplate.ReadOnly = true;
			AssertEquals("CanModifyExcelTemplate", false, testTemplate.CanModifyExcelTemplate);

			testTemplate.IsCheckedOutByMe = true;
			AssertEquals("CanModifyExcelTemplate", true, testTemplate.CanModifyExcelTemplate);

			testTemplate.IsCheckedOutByMe = false;
			AssertEquals("CanModifyExcelTemplate", false, testTemplate.CanModifyExcelTemplate);

			testTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			AssertEquals("CanModifyExcelTemplate", true, testTemplate.CanModifyExcelTemplate);
		}

		public void TestIsCheckedOutByMeOnInvalidFileName()
		{
			var mock = new Mock<ISourceControl>();
			SourceControl.SetEnterpriseInstanceForTesting(mock.Object);
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_ExcelTemplatePath = @"\I\Test\Firt\";

			Exception expectedException = new SourceControlException("BLAH");
			mock.Setup(m => m.IsFileCheckedOutByMe(It.IsAny<string>())).Throws(expectedException);
			try
			{
				bool result = template.IsCheckedOutByMe;
				Fail("Exception not thrown");
			}
			catch (SourceControlException ex)
			{
				AssertEquals(expectedException, ex);
			}
		}

		[ExpectNoExceptions]
		public void TestSettingTemplateInDotNet2Order()
		{
			testTemplate.SO_Name = "B";
			testTemplate.SO_Template = ZBlob.Empty;
		}

		public void TestClone()
		{
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "test";

			StmTemplateBase templateCopy = (StmTemplateBase)template.Clone();
			AssertEquals("DataContext", template.SO_DataContext, templateCopy.SO_DataContext);
			Assert("Reference not the same", template != templateCopy);
		}

		public void TestGetTemplateBlobFromFile()
		{
			var filename = UDFWithoutTabs.FullTemplateSourceLocation;

			AssertEquals("Is Empty", true, testTemplate.SO_Template.IsEmpty);
			var templateData = StmTemplateBase.GetTemplateBlobFromFile(filename);
			AssertEquals("Is Emptry", false, templateData.IsEmpty);
		}

		public void TestNewCopy()
		{
			const string Copyof = "Copy of ";
			var templateBytes = UDFWithoutTabs.GetAsByteArray();
			testTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			testTemplate.SO_IsSystemDefined = true;
			testTemplate.SO_Name = "Shipment Template";
			testTemplate.SO_Template = new ZBlob(templateBytes);

			var templateCopy = testTemplate.NewCopy();
			AssertEquals("Name", "Copy of Shipment Template", templateCopy.SO_Name);
			AssertEquals("IsSystemDefined", false, templateCopy.SO_IsSystemDefined);
			AssertEquals("DataContext", nameof(Core.Constants.DataContext.Shipment), templateCopy.SO_DataContext);
			Assert("Template value the same", templateCopy.SO_Template == testTemplate.SO_Template);

			//Truncating SO_Names longer than Permitted Maximum Length
			var longName = "";
			longName = longName.PadLeft(templateCopy.SO_NameInfo.MaxLength, 'A');
			templateCopy.SO_Name = longName;
			templateCopy = templateCopy.NewCopy();
			var newName = (Copyof + longName).Substring(0, templateCopy.SO_NameInfo.MaxLength);
			AssertEquals("NewCopy does not truncate longnames properly. ", templateCopy.SO_Name, newName);
		}

		public void TestDeletingTemplateWhenInUse()
		{
			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmMenuItem pubSysShipmentMenu1 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu1.SU_IsPublished = true;
			pubSysShipmentMenu1.SU_IsSystemDefined = true;
			pubSysShipmentMenu1.SU_MenuName = "Pub System Shipment Document1";

			StmMenuTemplatePivot pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivot>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			StmMenuItem pubSysShipmentMenu2 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu2.SU_IsPublished = true;
			pubSysShipmentMenu2.SU_IsSystemDefined = true;
			pubSysShipmentMenu2.SU_MenuName = "Pub System Shipment Document2";

			StmMenuTemplatePivot pubSysShipmentPivot2 = Factory.New<StmMenuTemplatePivot>();
			pubSysShipmentPivot2.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot2.SI_SU = pubSysShipmentMenu2.PK;
			pubSysShipmentPivot2.SI_DocumentTitle = "Pub System Ship Doc2 Pivot";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			StmTemplateBase copyOfTemplate = factory2.Load<StmTemplateBase>(sysShipmentTemplate.PK);

			try
			{
				sysShipmentTemplate.Delete();
				Fail("Delete should fail because template is in use");
			}
			catch (TemplateInUseException e)
			{
				Assert("Exception Message", e.Message.IndexOf("Pub System Shipment Document1") > 0);
				Assert("Exception Message", e.Message.IndexOf("Pub System Shipment Document2") > 0);
			}

			pubSysShipmentPivot1.Delete();

			try
			{
				sysShipmentTemplate.Delete();
				Fail("Delete should fail because template is in use");
			}
			catch (TemplateInUseException e)
			{
				Assert("Exception Message", e.Message.IndexOf("Pub System Shipment Document1") == -1);
				Assert("Exception Message", e.Message.IndexOf("Pub System Shipment Document2") > 0);
			}

			AssertEquals("CanDelete", false, sysShipmentTemplate.CanDelete);
			AssertMultilineASCIIEquals("ReasonForNotAbleToDelete",
@"Template 'System Shipment Template' cannot be deleted because it is in use by the following document menus:
   - Pub System Shipment Document2 (Shipment)

You need to remove this template from that document first before you can delete this template.

Private document menus may only be seen by the staff member that created it.",
				((ICanDelete)sysShipmentTemplate).ReasonForNotAbleToDelete);

			pubSysShipmentPivot2.Delete();
			AssertEquals("CanDelete", true, sysShipmentTemplate.CanDelete);
			sysShipmentTemplate.Delete();

			Factory.Save();

			Assert("Copy should be deleted by DataRefreshBus without throwing exceptions", copyOfTemplate.IsDeleted);
		}

		public void TestDeletingTemplateWhenInUseByNewPivots()
		{
			StmMenuItem pubSysShipmentMenu1 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu1.SU_IsPublished = true;
			pubSysShipmentMenu1.SU_IsSystemDefined = true;
			pubSysShipmentMenu1.SU_MenuName = "Pub System Shipment Document1";

			StmMenuItem pubSysShipmentMenu2 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu2.SU_IsPublished = true;
			pubSysShipmentMenu2.SU_IsSystemDefined = true;
			pubSysShipmentMenu2.SU_MenuName = "Pub System Shipment Document2";

			StmMenuItem pubSysShipmentMenu3 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu3.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu3.SU_IsPublished = true;
			pubSysShipmentMenu3.SU_IsSystemDefined = true;
			pubSysShipmentMenu3.SU_MenuName = "Pub System Shipment Document3";

			StmMenuItem pubSysShipmentMenu4 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu4.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu4.SU_IsPublished = true;
			pubSysShipmentMenu4.SU_IsSystemDefined = true;
			pubSysShipmentMenu4.SU_MenuName = "Pub System Shipment Document4";

			Factory.Save();

			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmMenuTemplatePivotCollection pivotCollection = new StmMenuTemplatePivotCollection(Factory);

			var pubSysShipmentPivot1 = pivotCollection.AddNew();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			var pubSysShipmentPivot2 = pivotCollection.AddNew();
			pubSysShipmentPivot2.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot2.SI_SU = pubSysShipmentMenu2.PK;
			pubSysShipmentPivot2.SI_DocumentTitle = "Pub System Ship Doc2 Pivot";

			var pubSysShipmentPivot3 = pivotCollection.AddNew();
			pubSysShipmentPivot3.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot3.SI_SU = pubSysShipmentMenu3.PK;
			pubSysShipmentPivot3.SI_DocumentTitle = "Pub System Ship Doc3 Pivot";

			var pubSysShipmentPivot4 = pivotCollection.AddNew();
			pubSysShipmentPivot4.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot4.SI_SU = pubSysShipmentMenu4.PK;
			pubSysShipmentPivot4.SI_DocumentTitle = "Pub System Ship Doc4 Pivot";

			try
			{
				AssertEquals("CanDelete", false, sysShipmentTemplate.CanDelete);
				sysShipmentTemplate.Delete();
				Fail("Delete should fail because template is in use");
			}
			catch (TemplateInUseException e)
			{
				Assert("Exception Message should contain Document1", e.Message.IndexOf("Pub System Shipment Document1") > 0);
				Assert("Exception Message should contain Document2", e.Message.IndexOf("Pub System Shipment Document2") > 0);
				Assert("Exception Message should contain Document3", e.Message.IndexOf("Pub System Shipment Document3") > 0);
				Assert("Exception Message should contain Document4", e.Message.IndexOf("Pub System Shipment Document4") > 0);

				AssertMultilineASCIIEquals("GetTemplateInUseErrorMessage",
@"Template 'System Shipment Template' cannot be deleted because it is in use by the following document menus:
   - Pub System Shipment Document1 (Shipment)
   - Pub System Shipment Document2 (Shipment)
   - Pub System Shipment Document3 (Shipment)
   - Pub System Shipment Document4 (Shipment)

You need to remove this template from that document first before you can delete this template.

Private document menus may only be seen by the staff member that created it.",
				e.Message);
			}
		}

		public void TestGetTemplateWhenInUseMessage()
		{
			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmMenuItem pubSysShipmentMenu1 = Factory.New<StmMenuItem>();
			pubSysShipmentMenu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu1.SU_IsPublished = true;
			pubSysShipmentMenu1.SU_IsSystemDefined = true;
			pubSysShipmentMenu1.SU_MenuName = "Pub System Shipment Document1";

			StmMenuTemplatePivot pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivot>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			StmMenuTemplatePivot pubSysShipmentPivot2 = Factory.New<StmMenuTemplatePivot>();
			pubSysShipmentPivot2.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot2.SI_SU = ZGuid.Empty;
			pubSysShipmentPivot2.SI_DocumentTitle = "Pub System Ship Doc2 Pivot";
			AssertNull(pubSysShipmentPivot2.MenuItem);

			StmMenuTemplatePivot pubSysShipmentPivot3 = Factory.New<StmMenuTemplatePivot>();
			pubSysShipmentPivot3.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot3.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot3.SI_DocumentTitle = "Pub System Ship Doc3 Pivot";

			AssertEquals(string.Format("The template '{0}' is referenced by a document which has just been deleted by another user.{1}Please save your current work and retry to delete this template.", sysShipmentTemplate.SO_Name, System.Environment.NewLine), ((ICanDelete)sysShipmentTemplate).ReasonForNotAbleToDelete);
		}

		public void TestInsertingALargeTemplate()
		{
			testTemplate = Factory.New<NonCachingTemplate>();

			var length = 2000000;
			var filename = Env.GetTempFileName();
			try
			{
				using (var tempFile = File.OpenWrite(filename))
				{
					for (var index = 0; index < length; index++)
					{
						tempFile.WriteByte(0xD);
					}
				}

				Assert("Should be empty", testTemplate.SO_Template.IsEmpty);

				var templateData = StmTemplateBase.GetTemplateBlobFromFile(filename);
				AssertEquals("Length", length, templateData.Length);

				AssertEquals("HasErrors", false, testTemplate.SO_TemplateInfo.HasErrors());
			}
			finally
			{
				File.Delete(filename);
			}
		}

		public void TestEditingMode()
		{
			var template = Factory.New<StmTemplateBase>();

			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, template.EditingMode);

			// User Defined
			template.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", false, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", false, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", false, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", false, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			// Client specific
			template.SO_IsClientSpecific = true;
			template.SO_IsSystemDefined = true;

			template.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", false, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", false, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", false, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", true, template.ReadOnly);
			AssertEquals("CanDelete", false, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", true, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, template.ReadOnly);
			AssertEquals("CanDelete", false, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", true, template.SO_IsPasswordProtectedInfo.ReadOnly);

			// System Defined
			template.SO_IsClientSpecific = false;
			template.SO_IsSystemDefined = true;

			template.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", false, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", false, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", true, template.ReadOnly);
			AssertEquals("CanDelete", false, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", true, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, template.ReadOnly);
			AssertEquals("CanDelete", true, template.CanDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", false, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", false, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, template.ReadOnly);
			AssertEquals("CanDelete", false, template.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "Selected template is System Defined, and cannot be deleted by users.", ((ICanDelete)template).ReasonForNotAbleToDelete);
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SO_IsClientSpecificInfo.ReadOnly", true, template.SO_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SO_IsPasswordProtected.ReadOnly", true, template.SO_IsPasswordProtectedInfo.ReadOnly);

			template.EditingMode = MenuEditingMode.AllowAll;
			template.SO_Name = "Customized Document Elements";
			template.SO_IsUserConfigurable = true;
			AssertEquals("SO_IsSystemDefinedInfo.ReadOnly", true, template.SO_IsSystemDefinedInfo.ReadOnly);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomizedDocumentElementsCannotBeDeletedWhenSectionIsBeingUsed()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();

			var excelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements Four Sections.xls", TestFilesSubFolder.DocumentTestFiles);
			var template = excelTemplate.GetAsByteArray();
			var customizedTemplate = TemplateTestHelper.CreateTemplate(Factory, Core.Constants.SectionRepositoryTemplateNames.User, template, "GenericFreightJob");
			var customizedTemplateForCHS = TemplateTestHelper.CreateTemplate(Factory, Core.Constants.SectionRepositoryTemplateNames.User + " [ZH-CN]", template, "GenericFreightJob");

			var excelTemplate1 = new ExcelTemplateForUnitTesting("Customized Document Elements One Section.xls", TestFilesSubFolder.DocumentTestFiles);
			var systemTemplate = TemplateTestHelper.CreateTemplate(Factory, Core.Constants.SectionRepositoryTemplateNames.System, excelTemplate1.GetAsByteArray(), "GenericFreightJob");

			Assert(!customizedTemplate.ReadOnly);
			Assert(!customizedTemplateForCHS.ReadOnly);
			Assert(customizedTemplate.CanDelete);
			Assert(customizedTemplateForCHS.CanDelete);

			customizedTemplate.Delete();
			Assert(customizedTemplateForCHS.CanDelete);

			customizedTemplate = TemplateTestHelper.CreateTemplate(Factory, Core.Constants.SectionRepositoryTemplateNames.User, template, "GenericFreightJob");

			var docBuilderDocumentCommand = Factory.New<DocumentCommand>();
			docBuilderDocumentCommand.Parent = dummy;
			docBuilderDocumentCommand.SU_MenuName = "Test Document";
			docBuilderDocumentCommand.SU_BusinessContext = "Dummy";
			var docBuilderTemplate1 = docBuilderDocumentCommand.Documents.AddNew();
			docBuilderTemplate1.SI_SU = docBuilderDocumentCommand.PK;
			docBuilderTemplate1.SI_SO = systemTemplate.PK;
			var aDocConfig = docBuilderTemplate1.DocConfigs.AddNew();
			aDocConfig.S3_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			aDocConfig.ConfigItems.AddNew().S4_SectionItemName = "A Happy Section";

			var docBuilderDocumentCommandCHS = Factory.New<DocumentCommand>();
			docBuilderDocumentCommandCHS.Parent = dummy;
			docBuilderDocumentCommandCHS.SU_MenuName = "Test Document CHS";
			docBuilderDocumentCommandCHS.SU_BusinessContext = "Dummy";
			var docBuilderTemplateCHS = docBuilderDocumentCommandCHS.Documents.AddNew();
			docBuilderTemplateCHS.SI_SU = docBuilderDocumentCommandCHS.PK;
			docBuilderTemplateCHS.SI_SO = customizedTemplateForCHS.PK;

			Factory.Save();

			Assert(!customizedTemplateForCHS.CanDelete);
			Assert(!customizedTemplate.CanDelete);

			var cannotDeleteReason = $@"Cannot delete template {Core.Constants.SectionRepositoryTemplateNames.User} because there are customized section(s) being used in documents.

Examples:

Documents using section ""A Happy Section"":
*** Data Context: Dummy, Path: , Document Name: ""Test Document"", Creating User: E (Unpublished)";

			AssertEquals("ReasonForNotAbleToDelete", cannotDeleteReason, ((ICanDelete)customizedTemplate).ReasonForNotAbleToDelete);

			var cannotDeleteReasonForCHS = $@"Template '{Core.Constants.SectionRepositoryTemplateNames.User + " [ZH-CN]"}' cannot be deleted because it is in use by the following document menus:
   - Test Document CHS (Dummy)
You need to remove this template from that document first before you can delete this template.

Private document menus may only be seen by the staff member that created it.";

			AssertMultilineASCIIEquals(cannotDeleteReasonForCHS, customizedTemplateForCHS.ReasonForNotAbleToDelete);

			customizedTemplate.SO_Name = Core.Constants.SectionRepositoryTemplateNames.User + " Test";
			Assert($"Can not delete the {Core.Constants.SectionRepositoryTemplateNames.User} even when the template name is changed", !customizedTemplate.CanDelete);

			customizedTemplate.SO_Name = Core.Constants.SectionRepositoryTemplateNames.User;
			customizedTemplate.SO_Template = null;
			Assert(!customizedTemplate.CanDelete);
			Assert(!customizedTemplateForCHS.CanDelete);
		}

		public void TestSetIsClientSpecific()
		{
			StmTemplateBase template = Factory.New<StmTemplateBase>();

			template.SO_IsSystemDefined = true;
			AssertEquals("SO_IsSystemDefined", true, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", false, template.SO_IsClientSpecific);

			template.SO_IsSystemDefined = false;
			AssertEquals("SO_IsSystemDefined", false, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", false, template.SO_IsClientSpecific);

			template.SO_IsClientSpecific = true;
			AssertEquals("SO_IsSystemDefined", true, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", true, template.SO_IsClientSpecific);

			template.SO_IsClientSpecific = false;
			AssertEquals("SO_IsSystemDefined", false, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", false, template.SO_IsClientSpecific);

			template.SO_IsClientSpecific = true;
			AssertEquals("SO_IsSystemDefined", true, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", true, template.SO_IsClientSpecific);

			template.SO_IsSystemDefined = false;
			AssertEquals("SO_IsSystemDefined", false, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", false, template.SO_IsClientSpecific);

			template.SO_IsSystemDefined = true;
			AssertEquals("SO_IsSystemDefined", true, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", false, template.SO_IsClientSpecific);

			template.SO_IsClientSpecific = true;
			AssertEquals("SO_IsSystemDefined", true, template.SO_IsSystemDefined);
			AssertEquals("SO_IsClientSpecific", true, template.SO_IsClientSpecific);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExcelTemplateFullPath()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_ExcelTemplatePath = "Documents\\Template.xls";
			AssertEquals("ExcelTemplateFullPath", Path.Combine(BaseSourcePath, "Documents\\Template.xls"), template.ExcelTemplateFullPath);
			template.ExcelTemplateFullPath = Path.Combine(BaseSourcePath, "Templates\\File.xls");
			AssertEquals("SO_ExcelTemplatePath", "Templates\\File.xls", template.SO_ExcelTemplatePath);
			AssertEquals("ExcelTemplateFullPath", Path.Combine(BaseSourcePath, "Templates\\File.xls"), template.ExcelTemplateFullPath);
		}

		public void TestGetExcelTemplate()
		{
			var bytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");
			testTemplate.SO_Name = "Oink";
			testTemplate.SO_Template = bytes;

			ExcelTemplate template = testTemplate.GetExcelTemplate();
			AssertEquals("GetExcelTemplate().TemplateName", "Oink", template.TemplateName);
			AssertEquals("GetExcelTemplate().GetAsByteArray()", bytes, template.GetAsByteArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUdfFieldCacheIsOnlySetIfTemplateIsForNonCustomisableDocument()
		{
			testTemplate.SO_Name = "Document";
			testTemplate.SO_Template = ZBlob.Empty;
			AssertEquals("SO_UDFFieldCache.IsEmpty", true, testTemplate.SO_UDFFieldCache.IsEmpty);

			testTemplate.SO_Name = "System Document Elements";
			testTemplate.SO_Template = File.ReadAllBytes(UnitTestingConstants.TestCustomisableTemplateFilePath);
			AssertEquals("SO_UDFFieldCache.IsEmpty", true, testTemplate.SO_UDFFieldCache.IsEmpty);

			testTemplate.SO_Name = "Document";
			testTemplate.SO_Template = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\ErrorsFiltersSortsGroupBysUDFsOptionsNNNNNN.xls"));
			AssertEquals("SO_UDFFieldCache.IsEmpty", false, testTemplate.SO_UDFFieldCache.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New(GetExpectedBusinessObjectType());

		StmTemplateBase testTemplate;

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[StmTemplateSchema.Constants.SO_Template] = (ZBlob)resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.DummyBusinessObjectAsDataSource.xls");
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			testTemplate = Factory.New<StmTemplateBase>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting udfWithoutTabs;
		ExcelTemplateForUnitTesting UDFWithoutTabs
		{
			get
			{
				if (udfWithoutTabs == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF without tabs.xls", "UDF without tabs.xls");
					udfWithoutTabs = new ExcelTemplateForUnitTesting("UDF without tabs.xls", Path.GetFullPath(tempFileName));
				}
				return udfWithoutTabs;
			}
		}

		string GetMenuItemsString(StmMenuItemBaseCollection menuItems)
		{
			var result = new StringBuilder();

			foreach (StmMenuItem menuItem in menuItems)
			{
				result.AppendLine(menuItem.SU_MenuName);
			}

			return result.ToString();
		}

		sealed class TemplateTest : StmTemplateBase
		{
			public TemplateTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal bool IsAutoLoggedForTest => IsAutoLogged;
		}

		sealed class NonCachingTemplate : StmTemplateBase
		{
			public NonCachingTemplate(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZBlob GetUpToDateUDFFieldCacheValue() => null;
		}
	}
}
