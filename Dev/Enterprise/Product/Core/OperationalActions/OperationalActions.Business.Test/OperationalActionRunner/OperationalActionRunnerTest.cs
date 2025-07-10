using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionRunner))]
	internal sealed class OperationalActionRunnerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeliverDocumentsInOneEmail()
		{
			Action.DocumentPivots.RemoveAll();
			var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			AssertEquals(true, runner.DeliverDocumentsInOneEmailInfo.ReadOnly);
			AssertEquals(true, runner.AttachmentOptionsInfo.ReadOnly);
			runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
			var pivot1 = Action.DocumentPivots.AddNew();
			var command1 = Factory.New<DocumentCommand>();
			command1.SU_ContactType = "CNR";
			pivot1.SF_SU_Outward = command1.PK;
			AssertEquals(false, runner.DeliverDocumentsInOneEmailInfo.ReadOnly);
			AssertEquals(true, runner.AttachmentOptionsInfo.ReadOnly);
			var pivot2 = Action.DocumentPivots.AddNew();
			var command2 = Factory.New<DocumentCommand>();
			command2.SU_ContactType = "CNE";
			pivot2.SF_SU_Outward = command2.PK;
			AssertEquals(true, runner.DeliverDocumentsInOneEmailInfo.ReadOnly);
			AssertEquals(true, runner.AttachmentOptionsInfo.ReadOnly);
			Action.DocumentPivots.RemoveAll();
			Action.DocumentPivots.AddNew();
			runner.DeliverDocumentsInOneEmail = true;
			AssertEquals(false, runner.AttachmentOptionsInfo.ReadOnly);
			AssertEquals("Single Attachment should be Default value", "SAT", runner.AttachmentOptions);
		}

		public void TestTargets()
		{
			var target1 = NewTarget("Target1");
			var target2 = NewTarget("Target2");
			var target3 = NewTarget("Target3");
			var target4 = NewTarget("Target4");
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			var records = new SelectedRecords()
			{ PrimaryKeys = new ZGuid[] { target1.PK, target2.PK, target3.PK, }, AutoSelectedAllKeys = false, };
			var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), records);
			var targets = runner.GetSelectedPrimaryKeys().ToList();
			AssertEquals("Doesn't contain correct number of targets", 3, targets.Count);
			Assert("Doesn't contain one of the targets supplied", targets.Contains(target1.PK));
			Assert("Doesn't contain one of the targets supplied", targets.Contains(target2.PK));
			Assert("Doesn't contain one of the targets supplied", targets.Contains(target3.PK));
		}

		public void TestModifyingCustomFieldCreatesEDTLog()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
			template.P0_Name = "Template";
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "customFieldString";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			using (var staffModule = ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var context = new OperationalActionContext(((IOperationalActionSupportable)staffModule).OperationalActionSupporter, "whatever", WorkflowDescriptors.GlbStaffDescriptorCode);
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				var descriptor = action.FieldDescriptors.AddNew();
				descriptor.FieldCaption = "customFieldString";
				descriptor.FieldName = "customFieldString";
				descriptor.DefaultingStrategy = "FXD";
				descriptor.DefaultValue = "B";
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();
				var records = new SelectedRecords()
				{ PrimaryKeys = new ZGuid[] { staff.PK }, AutoSelectedAllKeys = false, };
				AssertEquals(0, staff.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.EditedARecord).Count());
				var runner = new OperationalActionRunner(action, typeof(GlbStaff), records);
				var businessObjectFactory = new BusinessObjectFactory();
				runner.Run(new DummyOperationalActionLog(), businessObjectFactory);
				businessObjectFactory.Save(); //would happen after runner.Run in OperationalActionRunnerForm.RunAction
				staff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				AssertEquals("B", staff.GetPossiblyCustomProperty("__CUSTOMFIELDSTRING__prop__ZString"));
				AssertEquals(1, staff.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.EditedARecord).Count());
			}
		}

		public void TestGlbStaffFieldCanBeModifiedByOperationalAction()
		{
			using (var staffModule = ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var context = new OperationalActionContext(((IOperationalActionSupportable)staffModule).OperationalActionSupporter, "whatever");
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				var descriptor = action.FieldDescriptors.AddNew();
				descriptor.FieldCaption = "Branch";
				descriptor.FieldName = "GS_UserAddress2";
				descriptor.DefaultingStrategy = "FXD";
				descriptor.DefaultValue = "Broome";
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();
				var records = new SelectedRecords()
				{ PrimaryKeys = new ZGuid[] { staff.PK }, AutoSelectedAllKeys = false, };
				var runner = new OperationalActionRunner(action, typeof(GlbStaff), records);
				var businessObjectFactory = new BusinessObjectFactory();
				runner.Run(new DummyOperationalActionLog(), businessObjectFactory);
				CombineAssertions(delegate
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					AssertNull(ErrorReporter.LastExceptionReported);
				});
			}
		}

		public void TestSequenceOfOperation()
		{
			DummyBusinessObjectWithDocumentSupport target1 = NewTarget("Target1");
			DummyBusinessObjectWithDocumentSupport target4 = NewTarget("Target4");
			DummyBusinessObjectWithDocumentSupport target3 = NewTarget("Target3");
			DummyBusinessObjectWithDocumentSupport target2 = NewTarget("Target2");
			DummyBusinessObjectWithDocumentSupport target5 = NewTarget("Target5");
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			OperationalActionMethodDescriptor descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			SelectedRecords records = new SelectedRecords()
			{
				PrimaryKeys = new ZGuid[] { target2.PK, target5.PK, target3.PK, ZGuid.NewZGuid(), // just pretend this row has been deleted.
 target1.PK, target4.PK, },
				AutoSelectedAllKeys = false,
			};
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), records);
			const string expected = @"
INFO: Starting Section: Dummy Action Method Without GUI ...
DEBUG: Updating Target2
DEBUG: Updating Target5
DEBUG: Updating Target3
DEBUG: Updating Target1
DEBUG: Updating Target4
INFO: Dummy Action Method Without GUI Summary:
INFO: 5 event(s) added.
";
			RunRunner("Targets are not updated in correct order", runner, expected.Trim(), true);
		}

		public void TestBulkDeliveryMethodMaxLength()
		{
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals(new CodeDescriptionPairList(new BulkDeliveryMethodList()).MaxCodeLength, runner.BulkDeliveryMethodInfo.MaxLength);
		}

		public void TestRunAction_ZeroTargets()
		{
			AssertNotNull(Dummy1);
			Factory.Save();
			AssertEquals("precondition:", 0, CountEvents(Dummy1, Events.Delivered));
			Action.SU_SE_NKDocumentEvent = Events.Delivered.Code;
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			runner.Printer = ZGuid.NewZGuid();
			RunRunner("Precondition: start runner", runner, "INFO: Starting Section: Events ...");
			AssertEquals("no targets were selected, do not apply", 0, CountEvents(Dummy1, Events.Delivered));
		}

		[SetSavedRunnerSettings("")]
		public void TestRememberLastUsedPrinter()
		{
			OperationalActionRunner runner1 = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals("Garbage is treated as the empty guid", ZGuid.Empty, runner1.Printer);
			AssertEquals("Garbage is treated as dont CloseOnCompletion", false, runner1.CloseOnCompletion);
			runner1.Printer = ZGuid.NewZGuid();
			runner1.CloseOnCompletion = true;
			runner1.RunPreSaveValidation();
			OperationalActionRunner runner2 = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals("Should have remembered the previous printer", runner1.Printer, runner2.Printer);
			AssertEquals("Should have remembered not to close on completion", true, runner2.CloseOnCompletion);
		}

		public void TestRowCountAndNoun_Singular()
		{
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			action.SU_MenuName = "Do Random Stuff";
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			Factory.Save();
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				dummyModule.SelectedBusinessObjectsOverride = new[] { dummy1 };
				var runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				AssertEquals("1 element: SelectedGridRowCount", 1, runner.SelectedGridRowCount);
				AssertEquals("1 element: SelectedGridRowNoun", "record", runner.SelectedGridRowNoun);
				dummyModule.SelectedBusinessObjectsOverride = null;
				runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				runner.RunOnAllMatchingRecords = true;
				AssertEquals("1 element: FilterRowCount", 1, runner.FilterRowCount);
				AssertEquals("1 element: FilterRowNoun", "record", runner.FilterRowNoun);
			}
		}

		public void TestRowCountAndNoun_Plural()
		{
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			action.SU_MenuName = "Do Random Stuff";
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			Factory.Save();
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				dummyModule.SelectedBusinessObjectsOverride = new[] { dummy1, dummy2 };
				var runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				AssertEquals("2 elements: SelectedGridRowCount", 2, runner.SelectedGridRowCount);
				AssertEquals("2 elements: SelectedGridRowNoun", "records", runner.SelectedGridRowNoun);
				dummyModule.SelectedBusinessObjectsOverride = null;
				runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				runner.RunOnAllMatchingRecords = true;
				AssertEquals("2 elements: FilterRowCount", 2, runner.FilterRowCount);
				AssertEquals("2 elements: FilterRowNoun", "records", runner.FilterRowNoun);
			}
		}

		public void TestRowCountOverride()
		{
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			action.SU_MenuName = "Do Random Stuff";
			Factory.Save();
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);

				var overrideSelectionCount = new Mock<IOverrideSelectionCount>();
				overrideSelectionCount.SetupGet(x => x.SelectionCount).Returns(3);
				runner.OverrideSelectionCount = overrideSelectionCount.Object;

				AssertEquals("Overridden: SelectedGridRowCount", 3, runner.SelectedGridRowCount);
				AssertEquals("Overridden: FilterRowCount", 3, runner.FilterRowCount);
			}
		}

		[SetSavedRunnerSettings("")]
		public void TestUsesPrinter()
		{
			var stmPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();
			ZGuid invalidPrinter = ZGuid.NewZGuid();
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			AssertEquals("precondition:", ZGuid.Empty, runner.Printer);
			Action.DocumentPivots.AddNew();
			runner.Printer = invalidPrinter;
			AssertEquals(invalidPrinter, runner.Printer);
			AssertEquals(false, runner.PrinterInfo.ReadOnly);
			Assert(runner.PrinterInfo.HasError("Please enter a valid printer"));
			runner.Printer = stmPrintQueue.PK;
			runner.Validation.ValidatePrinter();
			Assert(!runner.PrinterInfo.HasError("Please enter a valid printer"));
			runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
			AssertEquals(ZGuid.Empty, runner.Printer);
			AssertEquals(true, runner.PrinterInfo.ReadOnly);
			runner.BulkDeliveryMethod = ForcePrinterBulkDeliveryMethod.CodeText;
			AssertEquals(stmPrintQueue.PK, runner.Printer);
			AssertEquals(false, runner.PrinterInfo.ReadOnly);
		}

		public void TestIncludeCoverNote()
		{
			CombineAssertions(delegate
			{
				OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
				runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
				runner.IncludeCoverNote = true;
				AssertEquals("1) IncludeCoverNoteInfo.ReadOnly", false, runner.IncludeCoverNoteInfo.ReadOnly);
				AssertEquals("1) IncludeCoverNote", true, runner.IncludeCoverNote);
				runner.BulkDeliveryMethod = OnlyPrinterBulkDeliveryMethod.CodeText;
				AssertEquals("2) IncludeCoverNoteInfo.ReadOnly", true, runner.IncludeCoverNoteInfo.ReadOnly);
				AssertEquals("2) IncludeCoverNote", false, runner.IncludeCoverNote);
			});
		}

		public void TestCoverNoteText()
		{
			CombineAssertions(delegate
			{
				OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
				runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
				runner.IncludeCoverNote = true;
				AssertEquals(0, runner.GetErrors().Count());
				runner.CoverNoteText = "Cover 載 Note";
				AssertEquals("1) CoverNoteTextInfo.ReadOnly", false, runner.CoverNoteTextInfo.ReadOnly);
				AssertEquals("1) CoverNoteText", "Cover 載 Note", runner.CoverNoteText);
				AssertEquals(0, runner.GetErrors().Count());
				runner.IncludeCoverNote = false;
				AssertEquals("2) CoverNoteTextInfo.ReadOnly", true, runner.CoverNoteTextInfo.ReadOnly);
				AssertEquals("2) CoverNoteText", "", runner.CoverNoteText);
			});
		}

		public void TestExtractUniqueLicenceCheckpoints()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			AssertContainsExactElementsInAnyOrder(Array.Empty<LicenceCheckpoint>(), new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueLicenceCheckpoints());
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			AssertContainsExactElementsInAnyOrder(new LicenceCheckpoint[] { Env.Licence.ShippingManagerBookings, Env.Licence.Forwarder, }, new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueLicenceCheckpoints());
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);
			AssertContainsExactElementsInAnyOrder(new LicenceCheckpoint[] { Env.Licence.ShippingManagerBookings, Env.Licence.ShippingManagerBillOfLading, Env.Licence.Forwarder, }, new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueLicenceCheckpoints());
		}

		public void TestExtractUniqueSecurityCheckpoints()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			AssertContainsExactElementsInAnyOrder(Array.Empty<SecurityCheckpoint>(), new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueSecurityCheckpoints());
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			AssertContainsExactElementsInAnyOrder(new SecurityCheckpoint[] { Env.Security.Schedules, Env.Security.Forwarding, }, new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueSecurityCheckpoints());
			action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);
			AssertContainsExactElementsInAnyOrder(new SecurityCheckpoint[] { Env.Security.Schedules, Env.Security.Forwarding, Env.Security.AgencyBillOfLading, }, new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()).ExtractUniqueSecurityCheckpoints());
		}

		public void TestSummaryLog()
		{
			AssertNotNull(Dummy1);
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			OperationalActionMethodDescriptor descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy1.PK));
			BusinessObjectFactory factoryForChanges;
			DummyOperationalActionLog dummyLog;
			const string expectedLogNotRun = "INFO: Dummy Action Method Without GUI Summary:\r\n" + "INFO: 0 event(s) added.";
			const string expectedLogRun = "INFO: Dummy Action Method Without GUI Summary:\r\n" + "INFO: 1 event(s) added.";
			dummyLog = new DummyOperationalActionLog();
			runner.SummaryLog(dummyLog);
			AssertMultilineASCIIEquals("", expectedLogNotRun, dummyLog.MessagesString());
			dummyLog = new DummyOperationalActionLog();
			factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			dummyLog.Verify();
			dummyLog = new DummyOperationalActionLog();
			runner.SummaryLog(dummyLog);
			AssertMultilineASCIIEquals("running the action once", expectedLogRun, dummyLog.MessagesString());
			dummyLog = new DummyOperationalActionLog();
			factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			dummyLog.Verify();
			dummyLog = new DummyOperationalActionLog();
			runner.SummaryLog(dummyLog);
			AssertMultilineASCIIEquals("running the action a second time", expectedLogRun, dummyLog.MessagesString());
		}

		public void TestFields()
		{
			OperationalActionFieldDescriptor descriptor1 = Action.FieldDescriptors.AddNew();
			descriptor1.FieldCaption = "Caption1";
			descriptor1.FieldName = "Z0_VarCharMax";
			OperationalActionFieldDescriptor descriptor2 = Action.FieldDescriptors.AddNew();
			descriptor2.FieldCaption = "Caption2";
			descriptor2.FieldName = "Z0_ThisFieldDoesNotExist";
			OperationalActionFieldDescriptor descriptor3 = Action.FieldDescriptors.AddNew();
			descriptor3.FieldCaption = "Caption3";
			descriptor3.FieldName = "Z0_Calculated";
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals("should have the correct number of fields", 3, runner.Fields.Count);
			AssertSame("Fields[0] should have the correct descriptor", descriptor1, runner.Fields[0].Descriptor);
			AssertEquals("Fields[0] should have the correct type", typeof(RunnerTextField), runner.Fields[0].GetType());
			AssertEquals("Fields[0] should have the correct field", "Z0_VarCharMax", ((RunnerTextField)runner.Fields[0]).FieldSupporter.Field);
			AssertSame("Fields[1] should have the correct descriptor", descriptor2, runner.Fields[1].Descriptor);
			AssertEquals("Fields[1] should have the correct type", typeof(RunnerErrorField), runner.Fields[1].GetType());
			AssertEquals("Fields[1] should have the correct text", "The field 'Z0_ThisFieldDoesNotExist' does not exist, this field may have been removed or renamed.\r\nTo resolve this you will need to edit this action and correct the fields name.", ((RunnerErrorField)runner.Fields[1]).ErrorText);
			AssertSame("Fields[2] should have the correct descriptor", descriptor3, runner.Fields[2].Descriptor);
			AssertEquals("Field[2] should have the correct type", typeof(RunnerErrorField), runner.Fields[2].GetType());
			AssertEquals("Fields[2] should have the correct text", "The field 'Z0_Calculated' cannot be bulk updated.", ((RunnerErrorField)runner.Fields[2]).ErrorText);
		}

		public void TestInvalidLanguageCannotBeSet()
		{
			var originalSecurityStatus = new Dictionary<string, bool>();
			foreach (var item in Env.Security.DocBuilderLanguagesLookup)
			{
				originalSecurityStatus[item.Key] = item.Value.IsAllowed;
				if (item.Key == Enterprise.Core.Constants.Languages.German || item.Key == Enterprise.Core.Constants.Languages.French)
				{
					item.Value.IsAllowed = true;
				}
				else
				{
					item.Value.IsAllowed = false;
				}
			}

			try
			{
				OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.Spanish;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = "XXX";
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.German;
				AssertEquals(Enterprise.Core.Constants.Languages.German, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.French;
				AssertEquals(Enterprise.Core.Constants.Languages.French, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.EnglishBritish;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishBritish, runner.DocumentPrintLanguage);
				runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.English;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, runner.DocumentPrintLanguage);
			}
			finally
			{
				foreach (var item in Env.Security.DocBuilderLanguagesLookup)
				{
					item.Value.IsAllowed = originalSecurityStatus[item.Key];
				}
			}
		}

		public void TestLanguageWarnings()
		{
			var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, runner.DocumentPrintLanguage);
			AssertNoWarnings(runner.DocumentPrintLanguageInfo);
			runner.DocumentPrintLanguage = Enterprise.Core.Constants.Languages.German;
			AssertHasWarning(runner.DocumentPrintLanguageInfo, "You have selected to issue this document translated into German.");
			runner.DocumentPrintLanguage = Res.DefaultLanguage;
			AssertNoWarnings(runner.DocumentPrintLanguageInfo);
		}

		public void TestBatchRunWhileModifyingFilterRecordsByAddingNew()
		{
			var sales1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var sales2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var sales3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var sales4 = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.SalesEnquiry))
			{
				dummyModule.PerformSearch_ForTest();
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(Action, sales1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				_ = runner.FilterRowCount;
				var otherFactory = new BusinessObjectFactory();
				var other = otherFactory.NewWithValidTestData<SalesEnquiry>();
				otherFactory.Save();
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				AssertMultilineASCIIEquals(@"INFO: Starting Batch 1 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 2 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 3 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
WARNING: There were 4 item(s) selected but 5 item(s) were found to process.", dummyLog.MessagesString());
				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals(1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestBatchRunWhileModifyingFilterRecordsRemoveExisting()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3DummyMissingPk = ZGuid.NewZGuid();
			var staff4DummyMissingPk = ZGuid.NewZGuid();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var selectionAfterDeletion = new List<ISelectedRecords>();
				selectionAfterDeletion.Add(new SelectedRecords(staff1.PK, staff2.PK));
				var moduleSelection = new MockTargetRecordSelectionForTest(new SelectedRecords(staff1.PK, staff2.PK, staff3DummyMissingPk, staff4DummyMissingPk), selectionAfterDeletion);
				var runner = new OperationalActionRunner(Action, staff1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				_ = runner.FilterRowCount;
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				AssertMultilineASCIIEquals($@"INFO: Starting Batch 1 of 1: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
WARNING: There were 4 item(s) selected but 2 item(s) were found to process.", dummyLog.MessagesString());
				AssertEquals("Jumps to end when finishing", dummyLog.LastMasterMaxExposedForTest, dummyLog.LastMasterCountExposedForTest);
				AssertEquals("Gives warning", OperationalActionLogErrorLevel.Warning, dummyLog.HighestErrorLevelEncountered);
				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals(1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestBatchRunWhileModifyingFilterRecordsRemoveTwoAndAddTwo()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var dummyMissingPk1 = ZGuid.NewZGuid();
			var dummyMissingPk2 = ZGuid.NewZGuid();
			var extraStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var extraStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var selectionAfterModification = new List<ISelectedRecords>();
				selectionAfterModification.Add(new SelectedRecords(staff1.PK, staff2.PK));
				selectionAfterModification.Add(new SelectedRecords(extraStaff1.PK, extraStaff2.PK));
				var moduleSelection = new MockTargetRecordSelectionForTest(new SelectedRecords(staff1.PK, staff2.PK, dummyMissingPk1, dummyMissingPk2), selectionAfterModification);
				var runner = new OperationalActionRunner(Action, staff1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				_ = runner.FilterRowCount;
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				AssertMultilineASCIIEquals($@"INFO: Starting Batch 1 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 2 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
", dummyLog.MessagesString());

				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals(1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestBatchRunWhileModifyingFilterRecordsAddTwoAndDeleteOne()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var dummyMissingPk1 = ZGuid.NewZGuid();
			var extraStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var extraStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var selectionAfterModification = new List<ISelectedRecords>();
				selectionAfterModification.Add(new SelectedRecords(staff1.PK, staff2.PK));
				selectionAfterModification.Add(new SelectedRecords(extraStaff1.PK, extraStaff2.PK));
				var moduleSelection = new MockTargetRecordSelectionForTest(new SelectedRecords(staff1.PK, staff2.PK, dummyMissingPk1), selectionAfterModification);
				var runner = new OperationalActionRunner(Action, staff1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				_ = runner.FilterRowCount;
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				AssertMultilineASCIIEquals($@"INFO: Starting Batch 1 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 2 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
WARNING: There were 3 item(s) selected but 4 item(s) were found to process.", dummyLog.MessagesString());
				AssertEquals("Jumps to end when finishing", dummyLog.LastMasterMaxExposedForTest, dummyLog.LastMasterCountExposedForTest);
				AssertEquals("Gives warning", OperationalActionLogErrorLevel.Warning, dummyLog.HighestErrorLevelEncountered);
				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals(1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestBatchRunWhileModifyingFilterRecordsRemoveTwoAndAddOne()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var dummyMissingPk1 = ZGuid.NewZGuid();
			var dummyMissingPk2 = ZGuid.NewZGuid();
			var extraStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				var selectionAfterModification = new List<ISelectedRecords>();
				selectionAfterModification.Add(new SelectedRecords(staff1.PK, staff2.PK));
				selectionAfterModification.Add(new SelectedRecords(extraStaff1.PK));
				var moduleSelection = new MockTargetRecordSelectionForTest(new SelectedRecords(staff1.PK, staff2.PK, dummyMissingPk1, dummyMissingPk2), selectionAfterModification);
				var runner = new OperationalActionRunner(Action, staff1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				_ = runner.FilterRowCount;
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				AssertMultilineASCIIEquals($@"INFO: Starting Batch 1 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 2 of 2: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
WARNING: There were 4 item(s) selected but 3 item(s) were found to process.", dummyLog.MessagesString());
				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals(1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestBatchRun_InitialiseBeforeAllBatchesRun_Batch()
		{
			var expectedLog = @"INFO: Starting Batch 1 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 2 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...
INFO: Starting Batch 3 of 3: ...
INFO: Starting Section: Dummy Action Method Without GUI ...";
			TestBatchRun_InitialiseBeforeAllBatchesRunCore((r, log, factory) => r.BatchRun(log, (l, f) => { }), expectedLog);
		}

		public void TestBatchRun_InitialiseBeforeAllBatchesRun_NonBatch()
		{
			var expectedLog = "INFO: Starting Section: Dummy Action Method Without GUI ...";
			TestBatchRun_InitialiseBeforeAllBatchesRunCore((r, log, factory) => r.Run(log, factory), expectedLog);
		}

		void TestBatchRun_InitialiseBeforeAllBatchesRunCore(Action<OperationalActionRunner, DummyOperationalActionLog, BusinessObjectFactory> runRunner, string expectedLog)
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.NewWithValidTestData<GlbStaff>();
			Factory.NewWithValidTestData<GlbStaff>();
			Factory.NewWithValidTestData<GlbStaff>();
			Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			var descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			{
				dummyModule.PerformSearch_ForTest();
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(Action, staff1.GetType(), moduleSelection)
				{ RunOnAllMatchingRecords = true };
				var dummyLog = new DummyOperationalActionLog();
				runRunner(runner, dummyLog, Factory);

				AssertMultilineASCIIEquals(expectedLog, dummyLog.MessagesString());
				var applicator = ((DummyOperationalActionMethodApplicator)runner.MethodApplicators.Single());
				AssertEquals("For Batch and non-Batch it should call once.", 1, applicator.InitialiseBeforeAllBatchesRunCalled);
			}
		}

		public void TestDisposableServiceIsAttachedBatchRun()
		{
			// Create hook
			var disposed = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "OperationalActions factory for changes")
				{
					f.SubscribeForDispose(new DisposableAction(() => disposed = true));
				}
			});

			AssertEquals("Factory should not have performed disposable action yet", false, disposed);
			var dummyLog = new DummyOperationalActionLog();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var selectionAfterAddition = new List<ISelectedRecords>();
			selectionAfterAddition.Add(new SelectedRecords(staff1.PK, staff2.PK));
			var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new MockTargetRecordSelectionForTest(new SelectedRecords(staff1.PK), selectionAfterAddition));
			runner.RunOnAllMatchingRecords = true;
			runner.BatchRun(dummyLog, (log, factory) => { factory.Save(); });
			AssertEquals("Factory should have a disposable service attached and have performed disposable action", true, disposed);
		}

		public void TestOverrideRecipientEmailEnabled()
		{
			var selectedRecord = Factory.New<SupportOverrideRecipientEmailForTest>();

			var runner = new OperationalActionRunner(Action, typeof(SupportOverrideRecipientEmailForTest), new SelectedRecords(selectedRecord.PK));

			runner.DeliverDocumentsInOneEmail = true;
			selectedRecord.IsOverrideRecipientEmailEnabledForTest = true;
			AssertEquals(ZBool.True, runner.OverrideRecipientEmailEnabled);

			runner.DeliverDocumentsInOneEmail = true;
			selectedRecord.IsOverrideRecipientEmailEnabledForTest = false;
			AssertEquals(ZBool.False, runner.OverrideRecipientEmailEnabled);

			runner.DeliverDocumentsInOneEmail = false;
			selectedRecord.IsOverrideRecipientEmailEnabledForTest = true;
			AssertEquals(ZBool.False, runner.OverrideRecipientEmailEnabled);

			var selectedRecordNotImplementsOverrideRecipientEmailDecider = Dummy1;
			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(selectedRecordNotImplementsOverrideRecipientEmailDecider.PK));
			runner.DeliverDocumentsInOneEmail = true;
			AssertEquals(ZBool.False, runner.OverrideRecipientEmailEnabled);
		}

		public void TestRecipientEmail_ReadOnly()
		{
			var runner = (OperationalActionRunner)GetNewBusinessObject();

			AssertEquals(ZBool.True, runner.RecipientEmailInfo.ReadOnly);

			runner.OverrideRecipientEmail = true;
			AssertEquals(ZBool.False, runner.RecipientEmailInfo.ReadOnly);
		}

		class MockTargetRecordSelectionForTest : IFilterRecordsSelection
		{
			public MockTargetRecordSelectionForTest(ISelectedRecords records, IEnumerable<ISelectedRecords> modifiedRecords)
			{
				this.records = records;
				this.modifiedRecords = modifiedRecords;
			}

			readonly ISelectedRecords records;
			readonly IEnumerable<ISelectedRecords> modifiedRecords;
			public IList<string> ExclusionReasons => throw new NotImplementedException();
			bool modified;
			public int FilterRowCount
			{
				get
				{
					var result = modified ? modifiedRecords.Sum(x => x.PrimaryKeys.Length) : records.PrimaryKeys.Length;
					if (!modified)
					{
						modified = true;
					}

					return result;
				}
			}

			public ISelectedRecords GetSelectedRecords()
			{
				return records;
			}

			public IEnumerable<ISelectedRecords> GetAllFilterRecords(Type bizObjType)
			{
				return modifiedRecords;
			}
		}

		#region Implementation
		static int CountEvents(BusinessObject bizObj, Event eventType)
		{
			StmALog[] logs = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code));
			return logs.Length;
		}

		static void RunRunner(string errorMessage, OperationalActionRunner runner, string expectedLog)
		{
			RunRunner(errorMessage, runner, expectedLog, false);
		}

		static void RunRunner(string errorMessage, OperationalActionRunner runner, string expectedLog, bool includeDebug)
		{
			DummyOperationalActionLog dummyLog = new DummyOperationalActionLog();
			dummyLog.IncludeDebug = includeDebug;
			BusinessObjectFactory factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			dummyLog.Verify();
			factoryForChanges.Save();
			runner.SummaryLog(dummyLog);
			AssertMultilineASCIIEquals(errorMessage, expectedLog, dummyLog.MessagesString());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
		}

		protected override void SetUp()
		{
			Env.Registry.SetFilterCriteria(nameof(OperationalActionRunner), "");
			base.SetUp();
		}

		DummyBusinessObjectWithDocumentSupport NewTarget(string description)
		{
			DummyBusinessObjectWithDocumentSupport target = Factory.New<DummyBusinessObjectWithDocumentSupport>();
			target.Z0_Description = description;
			return target;
		}

		MockOperationalActionSupportable ActionSupportable
		{
			get
			{
				return actionSupportable ?? (actionSupportable = new MockOperationalActionSupportable());
			}
		}

		MockOperationalActionSupportable actionSupportable;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = ActionSupportable.OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		DummyBusinessObjectWithDocumentSupport Dummy1
		{
			get
			{
				return dummy1 ?? (dummy1 = Factory.New<DummyBusinessObjectWithDocumentSupport>());
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy1;

		class SupportOverrideRecipientEmailForTest : DummyBusinessObjectWithDocumentSupport, ISupportOverrideOperationalActionRecipientEmail
		{
			public SupportOverrideRecipientEmailForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsOverrideRecipientEmailEnabledForTest { get; set; }

			public bool IsOverrideRecipientEmailEnabled(Type bizObjType, ZGuid[] targets)
			{
				return IsOverrideRecipientEmailEnabledForTest;
			}
		}
		#endregion
	}
}
