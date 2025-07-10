using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentNote))]
	sealed class DocumentNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateUserDefinedFieldWithDefaultExpressionIsItself()
		{
			var dummyBo = Factory.New<DummyBizOForTest>();
			var note = Factory.New<DocumentNote>();
			note.MainBusinessObject = dummyBo;

			var field1 = new TextField(Factory);
			field1.DisplayName = "Test Field1";
			field1.DefaultExpression = "<Test Field1>";
			field1.DataContextValue = new DataContextValueForTesting(Core.Constants.DataContext.UnitTest);

			var field2 = new TextField(Factory);
			field2.DisplayName = "Test Field2";
			field2.DefaultExpression = "<Z0_NVarChar><Test Field2>";
			field2.DataContextValue = new DataContextValueForTesting(Core.Constants.DataContext.UnitTest);

			note.fUserDefinedFieldList = new UserControlProviderList(field1, field2);

			AssertNoExceptionThrown(() => note.UpdateUDFsFromMainBusinessObjectInternal());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadedNoteUDFIsReloadedWhenItsChangedInAnotherFactory()
		{
			DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var exlTemplate = new ExcelTemplateForUnitTesting("UDF without tabs.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Consol);
			template.SO_IsSystemDefined = true;
			template.SO_Name = "Template 1";
			template.SO_Template = exlTemplate.GetAsByteArray();
			var pubDocumentCommand = Factory.New<DocumentCommand>();
			pubDocumentCommand.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubDocumentCommand.SU_IsPublished = true;
			pubDocumentCommand.SU_IsSystemDefined = true;
			pubDocumentCommand.SU_MenuName = "Document 1";
			var pubDocumentPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubDocumentPivot.SI_SO = template.PK;
			pubDocumentPivot.SI_SU = pubDocumentCommand.PK;
			pubDocumentPivot.SI_DocumentTitle = "Pub System Document 1";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var note1 = factory1.New<DocumentNoteForTest>();
			var businessObject1 = factory1.New<DummyConsolBusinessObject>();
			note1.MainBusinessObject = businessObject1;
			note1.ST_ParentID = businessObject1.PK;
			note1.SetFieldValue("Field1", "Value1");
			factory1.Save();

			AssertEquals("Value1", note1.GetFieldValueAsString("Field1"));

			var factory2 = new BusinessObjectFactory();
			var businessObject2 = factory2.Load<DummyConsolBusinessObject>(businessObject1.PK);
			var note2 = DocumentNote.LoadNote(businessObject2);
			AssertEquals("Value1", note2.GetFieldValueAsString("Field1"));

			note1.SetFieldValue("Field1", "Value2");
			factory1.Save();
			AssertEquals("Value2", note1.GetFieldValueAsString("Field1"));

			var note3 = DocumentNote.LoadNote(businessObject2);
			AssertEquals("Value2", note3.GetFieldValueAsString("Field1"));
		}

		public void TestLoadUDFFromInvalidXMLCatchAndReportErrorGracefully()
		{
			var note = Factory.New<DocumentNoteForTest>();
			((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20 This is Test Note.\par}");
			AssertNull("Precondition - ErrorReporter.LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
			note.LoadUDFsFromXML(null);
			AssertNotNull("ErrorReporter.LastExceptionReported should not be null", ErrorReporter.LastExceptionReported);
			AssertType("LastExceptionReported should be an XmlException", typeof(XmlException), ErrorReporter.LastExceptionReported);
			AssertEquals("ErrorReporter.LastMessageReported", string.Format(
				@"Read XML failed when load UDFs from XML for document note: PK = [{0}], Note Data = [{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fnil\fcharset0 Microsoft Sans Serif;}}}}\viewkind4\uc1\pard\f0\fs20 This is Test Note.\par}}]",
						note.PK.ToString()).Trim(), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadSDFFromInvalidXMLCatchAndReportErrorGracefully()
		{
			var note = Factory.New<DocumentNoteForTest>();
			((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20 This is Test Note.\par}");
			AssertNull("Precondition - ErrorReporter.LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
			note.LoadSDFsFromXML(null);
			AssertNotNull("ErrorReporter.LastExceptionReported should not be null", ErrorReporter.LastExceptionReported);
			AssertType("LastExceptionReported should be an XmlException", typeof(XmlException), ErrorReporter.LastExceptionReported);
			AssertEquals("ErrorReporter.LastMessageReported", string.Format(
				@"Read XML failed when load SDFs from XML for document note: PK = [{0}], Note Data = [{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fnil\fcharset0 Microsoft Sans Serif;}}}}\viewkind4\uc1\pard\f0\fs20 This is Test Note.\par}}]",
						note.PK.ToString()).Trim(), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetCompleteList()
		{
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var factory = new BusinessObjectFactory();
			TextField a = new TextField(factory), b = new TextField(factory), c = new TextField(factory), d = new TextField(factory);

			a.Value = "I've changed";
			a.DisplayName = "alpha";

			b.Value = b.Value;
			b.DisplayName = "bravo";

			c.DisplayName = "charlie";

			d.Value = "I've also changed";
			d.DisplayName = "delta";

			var dummyBO = factory.New<DummyBizOForTest>();
			var documentNote = factory.New<DocumentNoteForTest>();
			documentNote.MainBusinessObject = dummyBO;
			documentNote.fUserDefinedFieldList = new UserControlProviderList(a, b);
			documentNote.fSystemDefinedFieldList = new UserControlProviderList(c, d);

			AssertCollectionContains(a, documentNote.GetCompleteFieldList());
			AssertCollectionContains(b, documentNote.GetCompleteFieldList());
			AssertCollectionContains(c, documentNote.GetCompleteFieldList());
			AssertCollectionContains(d, documentNote.GetCompleteFieldList());
		}

		public void TestErrorReporterForLoadedDataTableWithoutColumnName()
		{
			var note = Factory.New<DocumentNote>();
			var dummyBo = Factory.New<DummyBizOForTest>();
			note.MainBusinessObject = dummyBo;

			using (var data = new DataSet())
			{
				var table = new DataTable("Doc");
				var nameColumn = new DataColumn("WhatEver", typeof(string));
				var valueColumn = new DataColumn("Value", typeof(string));
				table.Columns.Add(nameColumn);
				table.Columns.Add(valueColumn);
				table.PrimaryKey = new[] { nameColumn };
				data.Tables.Add(table);

				table.Rows.Add("WhatEverName", "WhatEverValue");

				using (var xmlStream = new MemoryStream())
				{
					data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
					note.ST_NoteData = new ZBlob(xmlStream.ToArray());
				}
			}
			TextField a = new TextField(Factory), b = new TextField(Factory);
			a.Value = "I've changed";
			a.DisplayName = "alpha";

			b.Value = b.Value;
			b.DisplayName = "bravo";

			var userDefinedFieldList = new UserControlProviderList(a, b);
			note.LoadUDFsFromXML(userDefinedFieldList);

			AssertNotNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals("NoColumnNameInLoadedDataTableForDocumentNote", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestSetIsOverrideInDocDataForFilterField()
		{
			var textField = new TextField(Factory)
			{
				DisplayName = "Alpha"
			};

			var userDefinedFieldList = new UserControlProviderList(textField);
			Assert(!textField.IsOverriddenInDocData);

			var note = Factory.New<DocumentNote>();
			var dummyBo = Factory.New<DummyBizOForTest>();
			note.MainBusinessObject = dummyBo;

			using (var data = new DataSet())
			{
				var table = new DataTable("Doc");
				var nameColumn = new DataColumn("Name", typeof(string));
				var valueColumn = new DataColumn("Value", typeof(string));
				table.Columns.Add(nameColumn);
				table.Columns.Add(valueColumn);
				table.PrimaryKey = new[] { nameColumn };
				data.Tables.Add(table);

				table.Rows.Add("Alpha", "New Value");

				using (var xmlStream = new MemoryStream())
				{
					data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
					note.ST_NoteData = new ZBlob(xmlStream.ToArray());
				}
			}

			note.LoadUDFsFromXML(userDefinedFieldList);
			Assert(textField.IsOverriddenInDocData);

			note = Factory.New<DocumentNote>();
			note.MainBusinessObject = dummyBo;
			textField = new TextField(Factory)
			{
				DisplayName = "Alpha",
				Value = "I have changed"
			};
			userDefinedFieldList = new UserControlProviderList(textField);
			Assert(!textField.IsOverriddenInDocData);

			note.fUserDefinedFieldList = userDefinedFieldList;
			note.SaveSDFsAndUDFsToXML();
			Assert(textField.IsOverriddenInDocData);
		}

		public void TestDocumentNoteShouldNotStayChangedWhenCallFactorySave()
		{
			var factory = new BusinessObjectFactory();
			var field = new TextField(factory) { DisplayName = "Field1", Value = "Value1" };

			var note = Factory.New<DocumentNoteForTest>();
			note.MainBusinessObject = Factory.New<DummyBizOForTest>();
			note.fUserDefinedFieldList = new UserControlProviderList(field);
			note.RegisterEditableChildObject(field);
			Factory.Save();

			field.Value = "Changed";
			field.Value = "Value1";
			Assert("note should have been changed", note.HasChanges);
			Factory.Save();
			Assert("note should not stay changed", !note.HasChanges);
		}

		public void TestOnlyChangedValuesAreSerialised()
		{
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var factory = new BusinessObjectFactory();
			TextField a = new TextField(factory), b = new TextField(factory), c = new TextField(factory), d = new TextField(factory);

			a.Value = "I've changed";
			a.DisplayName = "alpha";

			b.Value = b.Value;
			b.DisplayName = "bravo";

			c.DisplayName = "charlie";

			d.Value = "I've also changed";
			d.DisplayName = "delta";

			AssertEquals("HasChanges a", true, a.HasSerialisableValueChanged);
			AssertEquals("HasChanges b as it is assigned to", true, b.HasSerialisableValueChanged);
			AssertEquals("HasChanges c", false, c.HasSerialisableValueChanged);
			AssertEquals("HasChanges d", true, d.HasSerialisableValueChanged);

			var dummyBO = factory.New<DummyBizOForTest>();
			var documentNote = factory.New<DocumentNoteForTest>();
			documentNote.MainBusinessObject = dummyBO;
			documentNote.fUserDefinedFieldList = new UserControlProviderList(a, b, c, d);

			AssertEquals("Note data", "", documentNote.ST_NoteData.ToUTF8());
			documentNote.TestSerialise();
			AssertEquals("Note data should not have serialised non-changed fields", @"<NewDataSet>
  <Doc>
    <Name>alpha</Name>
    <Value>I've changed</Value>
  </Doc>
  <Doc>
    <Name>bravo</Name>
    <Value />
  </Doc>
  <Doc>
    <Name>delta</Name>
    <Value>I've also changed</Value>
  </Doc>
</NewDataSet>", documentNote.ST_NoteData.ToUTF8());
		}

		public void TestIsSavedByFactoryUdf()
		{
			var field = new TextField(Factory) { DisplayName = "Field1" };

			var note = Factory.New<DocumentNoteForTest>();
			note.MainBusinessObject = Factory.New<DummyBizOForTest>();
			note.fUserDefinedFieldList = new UserControlProviderList(field);

			AssertIsSavedByFactory(note, () => field.Value = "changed", () => { field.Value = ""; field.ResetHasChangesForTesting(); });
		}

		public void TestDocumentNotesArentOrphaned()
		{
			var systemDefinedField = Factory.NewWithValidTestData<StmSystemDefinedField>();
			systemDefinedField.S1_Name = "Foo";
			systemDefinedField.S1_BusinessContext = nameof(BusinessContext.Organisation);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var note = DocumentNote.LoadNote(org);

			note.SetSystemDefinedFieldValue("Foo", "Blah");
			Factory.Save();

			Assert("PRE: Note was saved", note.IsInDatabase);

			org.Delete();

			Factory.Save();

			AssertNull("The should have been deleted with the parent", new BusinessObjectFactory().Load(note.GetType(), note.PK));
		}

		public void TestGetApplicableDocumentCommandsIsCalled_WhenGetListOfUDFListsFromAllDocumentCommands()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var note = DocumentNote.LoadNote(org);
			var userDefinedFieldList = note.UserDefinedFieldList;
			Assert("Should call GetApplicalbeDocumentsCommands as the filter list would be evaluated only once within this method", note.IsGetApplicalbeDocumentsCommandsCalled);
		}

		public void TestIsSavedByFactorySdf()
		{
			var systemDefinedFields = new StmSystemDefinedFieldCollection(Factory);
			var field = systemDefinedFields.AddNew();
			field.S1_BusinessContext = nameof(BusinessContext.Test);
			field.S1_Name = "Field1";
			field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;

			var note = Factory.New<DocumentNoteForTest>();
			note.MainBusinessObject = Factory.New<DummyBizOForTest>();
			note.systemDefinedFieldWrappers = note.GetSystemDefinedFieldWrappers(systemDefinedFields);

			AssertIsSavedByFactory(note, () => note.systemDefinedFieldWrappers[0].S1_Value = "changed", () => note.systemDefinedFieldWrappers[0].S1_Value = "");
		}

		public void TestUpdateUDFsShouldNotCareAboutContext()
		{
			var field = new TextField(Factory)
			{
				DisplayName = "Field1",
				DataContextValue = new DataContextValueForTesting(Core.Constants.DataContext.UnitTest),
				DefaultExpression = "<Text>"
			};
			var note = Factory.New<DocumentNoteForTest>();
			note.MainBusinessObject = Factory.New<DummyBizOForTest>();
			note.fUserDefinedFieldList = new UserControlProviderList(field);

			var manager = Factory.GetDocWrapperContextManager();
			manager.UpdateDocWrapperContextFromReportConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Test Title" } });

			note.UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate();
			AssertEquals("CORRECT", field.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemoveUnnecessaryUDFValidators()
		{
			DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var exlTemplate = new ExcelTemplateForUnitTesting("UDF with same name in different case 1.xls", TestFilesSubFolder.DocumentTestFiles);
			var consolTemplate = Factory.New<StmTemplateBase>();
			consolTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Consol);
			consolTemplate.SO_IsSystemDefined = true;
			consolTemplate.SO_Name = "Consol Template 1";
			consolTemplate.SO_Template = exlTemplate.GetAsByteArray();
			var pubDocumentCommand = Factory.New<DocumentCommand>();
			pubDocumentCommand.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubDocumentCommand.SU_IsPublished = true;
			pubDocumentCommand.SU_IsSystemDefined = true;
			pubDocumentCommand.SU_MenuName = "Consol Document 1";
			var pubDocumentPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubDocumentPivot.SI_SO = consolTemplate.PK;
			pubDocumentPivot.SI_SU = pubDocumentCommand.PK;
			pubDocumentPivot.SI_DocumentTitle = "Pub System Consol Document 1";
			Factory.Save();

			var bizObject = Factory.New<DummyConsolBusinessObject>();
			using (var note = DocumentNote.LoadNoteWithExclusiveMutex(bizObject))
			{
				AssertEquals(3, note.UserDefinedFieldList.Count);

				var fieldNameCaseValidator = note.GetType().GetProperty("FieldNameCaseValidator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(note) as FieldNameCaseValidator;

				AssertCollectionContains(fieldNameCaseValidator, note.UserDefinedFieldList["Textfield"].Validators);
				AssertCollectionContains(fieldNameCaseValidator, note.UserDefinedFieldList["TexTField"].Validators);
				AssertCollectionContains(fieldNameCaseValidator, note.UserDefinedFieldList["Yet Another Field"].Validators);

				note.RunPreSaveValidation();
				note.RemoveUnnecessaryUDFValidators();

				AssertCollectionContains(fieldNameCaseValidator, note.UserDefinedFieldList["Textfield"].Validators);
				AssertCollectionContains(fieldNameCaseValidator, note.UserDefinedFieldList["TexTField"].Validators);
				AssertCollectionNotContains(fieldNameCaseValidator, note.UserDefinedFieldList["Yet Another Field"].Validators);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateOfCachedProperty()
		{
			var testNote = Factory.New<DocumentNote>();
			testNote.UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate();
		}

		public void TestCreatingANewNoteWithDescriptionAndThenAccessingFilteredSystemDefinedFieldWrappersShouldNotThrowExceptions()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestCode";

			var setupNote = orgHeader.Notes.AddNew(true, "a note description!", "This is a custom note that is just text.");
			setupNote.ST_NoteType = "DOC";
			setupNote.ST_NoteData = ZBlob.FromUTF8("This is a custom note that is just text.");

			using (var documentNote = DocumentNote.LoadNoteWithExclusiveMutex(orgHeader))
			{
				documentNote.Factory.Save();
				AssertEquals("Should not throw any exceptions", 0, documentNote.FilteredSystemDefinedFieldWrappers.Count);
			}
		}

		public void TestRetrieveNoteDoesNotRetrieveNoteWithRichTextData()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestCode";
			Factory.Save();

			var documentNote = Factory.New<DocumentNote>();
			documentNote.ST_ParentID = orgHeader.PK;
			documentNote.ST_Description = "";
			((INeedRow)documentNote).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20 Test Note Data\par}"); // Mock dirt record

			AssertNull("DocumentNote.RetrieveNote(orgHeader)", DocumentNote.RetrieveNote(orgHeader));
		}

		public void TestRetrieveNoteDoesNotRetrieveNoteWithDescription()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestCode";
			Factory.Save();

			using (var documentNote = Factory.New<DocumentNote>())
			{
				documentNote.ST_ParentID = orgHeader.PK;
				documentNote.ST_Description = "This is the best description ever!";
				documentNote.ST_NoteData = ZBlob.FromUTF8("This is a custom note that is just text.");
			}

			AssertNull("DocumentNote.RetrieveNote(orgHeader)", DocumentNote.RetrieveNote(orgHeader));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRetrieveNote()
		{
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			var exlTemplate1 = new ExcelTemplateForUnitTesting("UDF with tabs.xls", TestFilesSubFolder.ReportTestFiles);
			var sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Consol);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";
			sysConsolTemplate.SO_Template = exlTemplate1.GetAsByteArray();

			using (var tempReport = new Report(new DocumentPack(), exlTemplate1))
			{
				tempReport.PrepareForRender();
				var uDFBuilder = new UserDefinedFieldCollectionBuilder(tempReport.UDFSheet, tempReport.Analyser.ValidatorPack, tempReport.ReplaceSingleMacroNotInTemplateBody);
				var uDFCacheBlob = StringTreeNode.Serialise(uDFBuilder.RootNode);

				sysConsolTemplate.SO_UDFFieldCache = uDFCacheBlob;
			}

			var pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			var pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			Factory.Save();

			var bizObject = Factory.New<DummyConsolBusinessObject>();

			using (var note = DocumentNote.RetrieveNote(bizObject))
			{
				AssertNull("Note", note);
			}

			using (var note = DocumentNote.LoadNoteWithExclusiveMutex(bizObject))
			{
				AssertEquals("Note is new", true, !note.IsInDatabase);

				var field = (TextField)note.UserDefinedFieldList["I'm on beta too"];
				field.Value = "TestBeta";

				note.Factory.Save();
			}

			using (var retrievedNote = DocumentNote.RetrieveNote(bizObject))
			{
				AssertEquals("User Defined fields returned", 6, retrievedNote.UserDefinedFieldList.Count);
				AssertEquals("Field value", "TestBeta", ((TextField)retrievedNote.UserDefinedFieldList["I'm on beta too"]).ValueAsStringForSerialisation);
			}
		}

		public void TestAlwaysShowCommonDocuments()
		{
			var note = Factory.New<DocumentNote>();
			note.HasChanges = false;
			note.AlwaysShowCommonDocuments = true;
			Assert(note.AlwaysShowCommonDocuments);
			note.AlwaysShowCommonDocuments = false;
			Assert(!note.HasChanges);
			Assert(!note.AlwaysShowCommonDocuments);
		}

		public void TestDocumentType()
		{
			using (var note = Factory.New<DocumentNote>())
			{
				note.HasChanges = false;
				note.DocumentType = "Cuckoo Squeaker";
				Assert(!note.HasChanges);
				AssertEquals("Cuckoo Squeaker", note.DocumentType);
				note.DocumentType = "Gibbiceps";
				AssertEquals("Gibbiceps", note.DocumentType);
				AssertEquals(StmSystemDefinedFieldSchema.S1_Category.MaxLength, note.DocumentTypeInfo.MaxLength);
			}
		}

		public void TestDocumentTypes()
		{
			using (var note = helper.GetNewDocumentNote())
			{
				Assert(note.DocumentTypes.ContainsCode("ALL"));
				Assert(note.DocumentTypes.ContainsCode("CuckooSqueaker"));
				Assert(note.DocumentTypes.ContainsCode("Internet"));
				Assert(!note.DocumentTypes.ContainsCode("COMMON"));
				Assert(note.DocumentTypes.ContainsCode("aids"));
				Assert(note.DocumentTypes.ContainsCode("HOW2BINDFORMLOL"));
			}
		}

		public void TestPopulateSystemDefinedFieldWrappers()
		{
			using (var note = helper.GetNewDocumentNote())
			{
				note.DocumentType = "Internet";
				AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
				note.AlwaysShowCommonDocuments = false;
				AssertEquals(1, note.FilteredSystemDefinedFieldWrappers.Count);
			}
		}

		public void TestSystemDefinedFieldWrappers()
		{
			using (var note = helper.GetNewDocumentNote())
			{
				note.DocumentType = "";
				note.AlwaysShowCommonDocuments = false;
				AssertEquals(5, note.FilteredSystemDefinedFieldWrappers.Count);
			}
		}

		public void TestGetAndSetSystemDefinedFieldValue()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			var collection = new StmSystemDefinedFieldCollection(Factory);
			var field1 = collection.AddNew();
			field1.S1_Name = "Field1";
			field1.S1_BusinessContext = nameof(BusinessContext.Consol);

			Factory.Save();

			var dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			var note = DocumentNote.LoadNote(dummyConsol1);
			note.SetSystemDefinedFieldValue("Field1", "Field1Value");

			AssertEquals("Value should have been set and retrieved properly", "Field1Value", note.GetSystemDefinedFieldValue("Field1"));
		}

		[StressTest]
		public void TestAllTemplatesHaveUserDefinedFieldsCachedUpToDate()
		{
			var collection = new StmTemplateBaseCollection(Factory);
			collection.Load();

			foreach (StmTemplateBase template in collection)
			{
				if (template.SO_DataContext == nameof(Core.Constants.DataContext.None) || template.SO_Name.StartsWith("System Document Elements"))
				{
					continue;
				}

				var exlTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				using (var tempReport = new Report(new DocumentPack(), exlTemplate))
				{
					tempReport.PrepareForRender();
					var uDFBuilder = new UserDefinedFieldCollectionBuilder(tempReport.UDFSheet, tempReport.Analyser.ValidatorPack, tempReport.ReplaceSingleMacroNotInTemplateBody);
					CompareNodes(StringTreeNode.Deserialise(template.SO_UDFFieldCache), uDFBuilder.RootNode);
				}
			}
		}

		public void TestDocumentNoteDescriptionIsNotRequired()
		{
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var note = Factory.New<DocumentNote>();
			note.ST_Description = "";
			AssertEquals("A DocumentNote with no ST_Description should not have any notification.", false, note.ST_DescriptionInfo.HasNotifications());
		}

		public void TestNullMainObjectBlowsUpInGetSystemDefinedFieldList()
		{
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var note = Factory.New<DocumentNote>();
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ note.GetSystemDefinedFieldList(); });
		}

		public void TestSavingAndLoadingFieldCollectionThroughNote()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			var collection = new StmSystemDefinedFieldCollection(Factory);
			var field1 = collection.AddNew();
			field1.S1_Name = "Field1";
			field1.S1_BusinessContext = nameof(BusinessContext.Consol);

			var field2 = collection.AddNew();
			field2.S1_Name = "Field2";
			field2.S1_BusinessContext = nameof(BusinessContext.Consol);

			var field3 = collection.AddNew();
			field3.S1_Name = "Field3";
			field3.S1_BusinessContext = nameof(BusinessContext.Consol);

			Factory.Save();

			var dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.DocumentSupporter.ReturnNullWrappers = true;
			AssertEquals("Dummy BusinessContext", BusinessContext.Consol, dummyConsol1.DocumentSupporter.BusinessContext);
			using (var note = DocumentNote.LoadNoteWithExclusiveMutex(dummyConsol1))
			{
				AssertEquals("SDF Count", 3, note.FilteredSystemDefinedFieldWrappers.Count);
				var fields = note.FilteredSystemDefinedFieldWrappers.Cast<StmSystemDefinedFieldWrapper>().OrderBy(x => x.S1_NameFromDatabase).ToArray();

				AssertEquals("Field Name", "Field1", fields[0].S1_NameFromDatabase);
				AssertEquals("Field Value", "", fields[0].S1_Value);

				AssertEquals("Field Name", "Field2", fields[1].S1_NameFromDatabase);
				AssertEquals("Field Value", "", fields[1].S1_Value);
				fields[1].S1_Value = "Field2 Value";

				AssertEquals("Field Name", "Field3", fields[2].S1_NameFromDatabase);
				AssertEquals("Field Value", "", fields[2].S1_Value);
				fields[2].S1_Value = "Field3 Value";

				Factory.Save();
			}

			var factory2 = new BusinessObjectFactory();
			var dummyConsol1Copy = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			dummyConsol1Copy.DocumentSupporter.ReturnNullWrappers = true;
			using (var noteCopy = DocumentNote.LoadNoteWithExclusiveMutex(dummyConsol1Copy))
			{
				AssertEquals("SDF Count", 3, noteCopy.FilteredSystemDefinedFieldWrappers.Count);
				var fields = noteCopy.FilteredSystemDefinedFieldWrappers.Cast<StmSystemDefinedFieldWrapper>().OrderBy(x => x.S1_NameFromDatabase).ToArray();

				AssertEquals("Field Name", "Field1", fields[0].S1_NameFromDatabase);
				AssertEquals("Field Value", "", fields[0].S1_Value);

				AssertEquals("Field Name", "Field2", fields[1].S1_NameFromDatabase);
				AssertEquals("Field Value", "Field2 Value", fields[1].S1_Value);

				AssertEquals("Field Name", "Field3", fields[2].S1_NameFromDatabase);
				AssertEquals("Field Value", "Field3 Value", fields[2].S1_Value);
			}
		}

		public void TestFilteredSystemDefinedFieldWrappers()
		{
			using (var note = helper.GetNewDocumentNote())
			{
				var commonField = note.AllSystemDefinedFields.OfType<StmSystemDefinedField>().FirstOrDefault(x => x.S1_Category.EqualsIgnoringCase("COMMON"));
				var resKeyCategory = commonField.S1_CategoryInfo.CustomizableDataResourceStrings.GetMultilingualString(commonField, commonField.S1_Category).ResourceKey;
				var resKeyName = commonField.S1_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(commonField, commonField.S1_Name).ResourceKey;
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				using (var mockRes = Res.UseMockData())
				{
					mockRes.Put(resKeyCategory, new ResourceStringData(resKeyCategory, "常用"));
					AssertEquals("常用", commonField.S1_CategoryMultilingual);

					mockRes.Put(resKeyName, new ResourceStringData(resKeyName, "整数字段"));
					AssertEquals("整数字段", commonField.S1_NameMultilingual);

					AssertEquals(5, note.FilteredSystemDefinedFieldWrappers.Count);

					note.DocumentType = "comMon";
					AssertEquals(1, note.FilteredSystemDefinedFieldWrappers.Count);
					AssertEquals("IntegerField1", note.FilteredSystemDefinedFieldWrappers[0].S1_NameFromDatabase);
					AssertEquals("整数字段", note.FilteredSystemDefinedFieldWrappers[0].S1_Name);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.IntegerField1);

					note.DocumentType = "aids";
					AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.IntegerField1);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.DateTimeField1);

					note.DocumentType = "HOW2BINDFORMLOL";
					AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.IntegerField1);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.DecimalField1);

					note.DocumentType = "internet";
					AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.IntegerField1);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.DateOnlyField1);

					note.DocumentType = "CUCKOOSQUEAKER";
					AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.IntegerField1);
					AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, helper.TextField1);
				}
			}
		}

		public void TestFilteredSystemDefinedFieldWrappers_HideLetterOfCreditFieldsWhenValueIsEmpty()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			var systemDefinedFields = new StmSystemDefinedFieldCollection(Factory);

			var field1 = systemDefinedFields.AddNew();
			field1.S1_BusinessContext = nameof(BusinessContext.Customs);
			field1.S1_Name = "Letter of Credit Number";
			field1.S1_Category = "COMMON";

			var field2 = systemDefinedFields.AddNew();
			field2.S1_BusinessContext = nameof(BusinessContext.Customs);
			field2.S1_Name = "Letter of Credit Date";
			field2.S1_Category = "COMMON";

			var field3 = systemDefinedFields.AddNew();
			field3.S1_BusinessContext = nameof(BusinessContext.Customs);
			field3.S1_Name = "Field 3";
			field3.S1_Category = "COMMON";

			var field4 = systemDefinedFields.AddNew();
			field4.S1_BusinessContext = nameof(BusinessContext.Customs);
			field4.S1_Name = "Filed 4";
			field4.S1_Category = "aids";

			var field5 = systemDefinedFields.AddNew();
			field5.S1_BusinessContext = nameof(BusinessContext.Customs);
			field5.S1_Name = "Field 5";
			field5.S1_Category = "HOW2BINDFORMLOL";

			Factory.Save();

			var customs = Factory.New<DummyCustomsBusinessObject>();
			customs.Z0_Code = "DC2";
			using (var note = DocumentNote.LoadNoteWithExclusiveMutex(customs))
			{
				AssertEquals(3, note.FilteredSystemDefinedFieldWrappers.Count);

				note.DocumentType = "comMon";
				AssertEquals(1, note.FilteredSystemDefinedFieldWrappers.Count);
				AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, field3);

				note.DocumentType = "aids";
				AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
				AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, field3);
				AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, field4);

				note.DocumentType = "HOW2BINDFORMLOL";
				AssertEquals(2, note.FilteredSystemDefinedFieldWrappers.Count);
				AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, field3);
				AssertContainsWrapper(note.FilteredSystemDefinedFieldWrappers, field5);

				note.SetSystemDefinedFieldValue("Letter of Credit Number", "111111");
				note.SetSystemDefinedFieldValue("Letter of Credit Date", "20220121");

				Factory.Save();
			}

			var factory2 = new BusinessObjectFactory();
			var dummyCustomsCopy1 = factory2.Load<DummyCustomsBusinessObject>(customs.PK);
			using (var noteCopy = DocumentNote.LoadNoteWithExclusiveMutex(dummyCustomsCopy1))
			{
				AssertEquals(5, noteCopy.FilteredSystemDefinedFieldWrappers.Count);
				AssertContainsWrapper(noteCopy.FilteredSystemDefinedFieldWrappers, field1);
				AssertContainsWrapper(noteCopy.FilteredSystemDefinedFieldWrappers, field2);
			}
		}

		public void TestNoteLoadedWithoutMutexCannotBeSaved()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var note1 = DocumentNote.LoadNote(orgHeader);

			try
			{
				note1.ST_NoteDataAsText = "whatever";
				note1.Factory.Save();
				AssertEquals("Two XML exceptions should be thrown. Refer to LoadSDFsFromXML and LoadUDFsFromXML.", 2, ExceptionReporterTestListener.Instance.Count);
				Assert(!note1.IsInDatabase);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		protected override BusinessObject GetNewBusinessObject() => DocumentNote.LoadNote(Factory.New<DummyConsolBusinessObject>());

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var note = helper.GetNewDocumentNote(factory, useExclusiveMutex: false);
			note.SystemDefinedFieldWrappers[0].S1_Value = "changed";
			return note;
		}

		protected override bool IsDeleteSupported() => true;

		DocumentNoteTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new DocumentNoteTestHelper(Factory);
		}

		void CompareNodes(StringTreeNode node1, StringTreeNode node2)
		{
			AssertEquals("testing Values to be the same", node1.Value, node2.Value);
			AssertEquals("testing children count on " + node1.Value, node1.Children.Count, node2.Children.Count);
			for (var i = 0; i < node1.Children.Count; i++)
			{
				CompareNodes(node1.Children[i], node2.Children[i]);
			}
		}

		void AssertContainsWrapper(StmSystemDefinedFieldWrapperCollection collection, StmSystemDefinedField expectedField)
		{
			Assert(string.Format("FieldWrapper for '{0}' not found", expectedField.S1_Name),
				collection.Cast<StmSystemDefinedFieldWrapper>().Any(wrapper => wrapper.S1_NameFromDatabase == expectedField.S1_Name));
		}

		void AssertIsSavedByFactory(DocumentNote note, Action doChange, Action doClear)
		{
			doChange();
			Assert("Note should not be empty with changes", note.IsNotEmpty);
			Assert("Not-empty note should not be saveable", note.IsSavedByFactory);

			doClear();
			Assert("Note should be empty with defaults", !note.IsNotEmpty);
			Assert("Empty note should not be saveable", !note.IsSavedByFactory);

			Factory.Save();
			Assert("Empty note should not be saved to db", !note.IsInDatabase);

			doChange();
			Assert("Note should not be empty with changes", note.IsNotEmpty);
			Factory.Save();
			Assert("Non-empty note should be saved to db", note.IsInDatabase);

			doClear();
			note.ST_NoteContext = "XYZ"; // To mark note as has changes
			Assert("Note should be empty with defaults", !note.IsNotEmpty);
			Assert("Empty note already in db should be saveable", note.IsSavedByFactory);
		}

		sealed class DummyBizOForTest : Enterprise.ZArchitecture.Business.Testing.DummyEnterpriseBusinessObject, IDocumentSupportable
		{
			public DummyBizOForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return new DummyBizODocSupporterForTest(this); }
			}
		}

		sealed class DummyBizODocSupporterForTest : DocumentSupporter
		{
			public DummyBizODocSupporterForTest(DummyBizOForTest parent)
				: base(parent)
			{
				parentDummyBO = parent;
			}

			readonly DummyBizOForTest parentDummyBO;

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.None; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return dataContext == Core.Constants.DataContext.UnitTest ? new DocumentWrapper[] { new DocumentWrapperForTest(parentDummyBO) } : null;
			}

			protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Enterprise.Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.UnitTest };
			}
		}

		sealed class DocumentWrapperForTest : DocumentWrapper
		{
			public DocumentWrapperForTest(DummyBizOForTest parentBusinessObject)
			: base(parentBusinessObject, parentBusinessObject.Factory)
			{
			}

			public ZString Text
			{
				get
				{
					IDocWrapperContext context = Factory.GetDocWrapperContextManager();
					return context.MenuTitle == "Test Title" ? "INCORRECT" : "CORRECT";
				}
			}
		}

		sealed class DocumentNoteForTest : DocumentNote
		{
			public DocumentNoteForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void TestSerialise()
			{
				base.SaveSDFsAndUDFsToXML();
			}
		}
	}
}
