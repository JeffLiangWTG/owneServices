using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using DeliveryFormats = Enterprise.DocumentEngine.DeliveryMethods.DeliveryInfo.DeliveryFormats;
using EDICore = Enterprise.Core;
using StmPrintJob = Enterprise.DocumentEngine.Scheduler.Business.StmPrintJob;

namespace Enterprise.DocumentEngine.Testing
{
	public sealed class PrintTaskTest : TestCaseWithFactory
	{
		public void TestDocumentsUseDefaultPrinters()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
	@"{A}-[#Config]
{A}-[#EndOfReport]");
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = documentSupportable;

			var pivot1 = command.Documents.AddNew();
			pivot1.SI_SU = command.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = command.Documents.AddNew();
			pivot2.SI_SU = command.PK;
			pivot2.SI_SO = template2.PK;

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";

			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";

			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				AssertEquals("should be 2 documents to be delivered.", 2, deliveryInstructions.DocumentsToBeDelivered.Count);
				var document1 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK);
				document1.PrinterDetails.PrintQueuePK = stmPrintQueue1.PK;
				document1.PrinterDetails.NumberOfCopies = 2;
				var document2 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK);
				document2.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
				document2.PrinterDetails.NumberOfCopies = 3;

				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);
			}

			var query1 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)2);

			var stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query1);

			AssertEquals("there should be a default printer for pivot1 and \"printer 1\".", 1, stmDefaultPrinters.Length);

			var query2 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)3);

			stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query2);
			AssertEquals("there should be a default printer for pivot2 and \"printer 2\".", 1, stmDefaultPrinters.Length);

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();

				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAll();
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = "PRN";
				printTask.LoadPrinterDeliveryDefaults(deliveryInstructions);

				AssertEquals("", 2, deliveryInstructions.DocumentsToBeDelivered.Count);

				var document1 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 1\"", "Printer 1", document1.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 2.", (ZShort)2, document1.PrinterDetails.NumberOfCopies);

				var document2 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 2\"", "Printer 2", document2.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 3.", (ZShort)3, document2.PrinterDetails.NumberOfCopies);

				printTask.Run(deliveryInstructions);

				var stmPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(2, stmPrintJobs.Length);
				AssertContainsExactElementsInAnyOrder("there should be 2 and 3.", new ZShort[2] { 2, 3 }, stmPrintJobs.Select(printJob => printJob.SP_Copies));
				AssertContainsExactElementsInAnyOrder("there should be printer1 and printer2.", new ZGuid[2] { stmPrintQueue1.PK, stmPrintQueue2.PK }, stmPrintJobs.Select(printJob => printJob.SP_SQ));
			}
		}

		public void TestDocumentsUseDefaultPrintersWithNonPersistentCommand()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
	@"{A}-[#Config]
{A}-[#EndOfReport]");
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = documentSupportable;

			var pivot1 = command.Documents.AddNew();
			pivot1.SI_SU = command.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = command.Documents.AddNew();
			pivot2.SI_SU = command.PK;
			pivot2.SI_SO = template2.PK;

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";

			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";

			var stmPrintQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue3.SQ_DisplayName = "Printer 3";
			stmPrintQueue3.SQ_ServerName = "TEST3";

			Factory.Save();

			command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "NewTestMenu";
			command.Parent = documentSupportable;

			command.Documents.Add(pivot1);
			command.Documents.Add(pivot2);

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.PrinterDelivery.PrintQueuePK = stmPrintQueue3.PK;
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				AssertEquals("should be 2 documents to be delivered.", 2, deliveryInstructions.DocumentsToBeDelivered.Count);
				var document1 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK);
				document1.PrinterDetails.PrintQueuePK = stmPrintQueue1.PK;
				document1.PrinterDetails.NumberOfCopies = 2;
				var document2 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK);
				document2.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
				document2.PrinterDetails.NumberOfCopies = 3;

				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);
			}

			var query1 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)2);

			var stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query1);

			AssertEquals("there should be no default printer for pivot1 and NewTestMenu.", 0, stmDefaultPrinters.Length);

			var query2 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)3);

			stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query2);
			AssertEquals("there should be no default printer for pivot2 and NewTestMenu.", 0, stmDefaultPrinters.Length);

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();

				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAll();
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				printTask.LoadPrinterDeliveryDefaults(deliveryInstructions);

				AssertEquals("the delivery instructions should have the default printer \"Printer 3\".", deliveryInstructions.PrinterDelivery.PrintQueuePK, stmPrintQueue3.PK);
				AssertEquals("there should be 2 documents to be delivered.", 2, deliveryInstructions.DocumentsToBeDelivered.Count);

				var document1 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 1\"", "Printer 1", document1.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 2.", (ZShort)2, document1.PrinterDetails.NumberOfCopies);

				var document2 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 2\"", "Printer 2", document2.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 3.", (ZShort)3, document2.PrinterDetails.NumberOfCopies);

				printTask.Run(deliveryInstructions);

				var stmPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(2, stmPrintJobs.Length);
				AssertContainsExactElementsInAnyOrder("there should be 2 and 3.", new ZShort[2] { 2, 3 }, stmPrintJobs.Select(printJob => printJob.SP_Copies));
				AssertContainsExactElementsInAnyOrder("there should be printer1 and printer2.", new ZGuid[2] { stmPrintQueue1.PK, stmPrintQueue2.PK }, stmPrintJobs.Select(printJob => printJob.SP_SQ));
			}
		}

		public void TestDocumentPacks_WhenAllPacksHaveSameForcedLanguage_ShouldSetLanguage()
		{
			using var printTask = new PrintTask();
			var pack1 = new DocumentPack();
			pack1.ForcedLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			var pack2 = new DocumentPack();
			pack2.ForcedLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			var packs = new List<DocumentPack>();
			packs.Add(pack1);
			packs.Add(pack2);
			printTask.AddRange(packs);
			printTask.Run(Env.Security.None, true);

			AssertEquals(printTask.DeliveryInstructionsForDeliveryForm.Language, Core.SharedConstants.Languages.ChineseSimplified);
			AssertEquals(printTask.DeliveryInstructionsForDeliveryForm.Language_ReadOnly, true);
		}

		public void TestDocumentPack_WhenTemplatesHaveForcedLanguage()
		{
			var templateWithNonTranslate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[#EndOfReport]");

			var templateWithNonLang = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]");

			var templateWithZHCN1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test3",
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=ZH-CN]
{A}-[#EndOfReport]");

			var templateWithZHCN2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test4",
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=zh-cn]
{A}-[#EndOfReport]");

			var templateWithZHTW = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test5",
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=ZH-TW]
{A}-[#EndOfReport]");

			AssertDocumentPackWithForcedLanguage([templateWithNonLang, templateWithZHCN1, templateWithZHTW], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithNonLang, templateWithZHTW, templateWithZHCN1], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1, templateWithNonLang, templateWithZHTW], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1, templateWithZHTW, templateWithNonLang], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHTW, templateWithNonLang, templateWithZHCN1,], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHTW, templateWithZHCN1, templateWithNonLang], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1, templateWithZHCN2], true, true, Core.SharedConstants.Languages.ChineseSimplified);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN2, templateWithZHCN1], true, true, Core.SharedConstants.Languages.ChineseSimplified);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1, templateWithZHTW], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHTW, templateWithZHCN1], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1, templateWithNonLang], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithZHCN1], true, true, Core.SharedConstants.Languages.ChineseSimplified);
			AssertDocumentPackWithForcedLanguage([templateWithNonTranslate, templateWithNonLang], true, false, DataRegistry.Instance.EnglishSpelling);
			AssertDocumentPackWithForcedLanguage([templateWithNonTranslate, templateWithZHCN1], true, true, Core.SharedConstants.Languages.ChineseSimplified);
			AssertDocumentPackWithForcedLanguage([templateWithNonTranslate], false, false, DataRegistry.Instance.EnglishSpelling);
		}

		void AssertDocumentPackWithForcedLanguage(
			IEnumerable<StmTemplateBase> templates,
			bool expectedSupportLanguageSelection,
			bool expectedLanguageSelectionReadonly,
			string expectedLanguage)
		{
			var documentSupportable = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			command.Parent = documentSupportable;
			templates.ForEach(template =>
			{
				var pivot = command.Documents.AddNew();
				pivot.SI_SU = command.PK;
				pivot.SI_SO = template.PK;
			});
			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();

				printTask.Run(Env.Security.None, true);
				var documentPack = printTask.GetDocumentPacks().FirstOrDefault();
				AssertEquals($"SupportsLanguageSelection should be {expectedSupportLanguageSelection}", expectedSupportLanguageSelection, documentPack.SupportsLanguageSelection);
				AssertEquals($"LanguageSelectionReadonly should be {expectedLanguageSelectionReadonly}", expectedLanguageSelectionReadonly, printTask.DeliveryInstructionsForDeliveryForm.Language_ReadOnly);
				AssertEquals($"ExpectedLanguage should be {expectedLanguage}", expectedLanguage, printTask.DeliveryInstructionsForDeliveryForm.Language);
			}
		}

		public void TestDocumentsDeletePrinterOverride()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
	@"{A}-[#Config]
{A}-[#EndOfReport]");
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = documentSupportable;

			var pivot1 = command.Documents.AddNew();
			pivot1.SI_SU = command.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = command.Documents.AddNew();
			pivot2.SI_SU = command.PK;
			pivot2.SI_SO = template2.PK;

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";

			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";

			var stmPrintQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue3.SQ_DisplayName = "Printer 3";
			stmPrintQueue3.SQ_ServerName = "TEST3";

			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.PrinterDelivery.PrintQueuePK = stmPrintQueue3.PK;
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				AssertEquals("should be 2 documents to be delivered.", 2, deliveryInstructions.DocumentsToBeDelivered.Count);
				var document1 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK);
				document1.PrinterDetails.PrintQueuePK = stmPrintQueue1.PK;
				document1.PrinterDetails.NumberOfCopies = 2;
				var document2 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK);
				document2.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
				document2.PrinterDetails.NumberOfCopies = 3;

				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);
			}

			var query1 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)2);

			var stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query1);

			AssertEquals("there should be a default printer for pivot1 and \"printer 1\".", 1, stmDefaultPrinters.Length);

			var query2 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)3);

			stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query2);
			AssertEquals("there should be a default printer for pivot2 and \"printer 2\".", 1, stmDefaultPrinters.Length);

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();

				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAll();
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				printTask.LoadPrinterDeliveryDefaults(deliveryInstructions);

				AssertEquals("", 2, deliveryInstructions.DocumentsToBeDelivered.Count);

				var document1 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 1\"", "Printer 1", document1.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 2.", (ZShort)2, document1.PrinterDetails.NumberOfCopies);

				var document2 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 2\"", "Printer 2", document2.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 3.", (ZShort)3, document2.PrinterDetails.NumberOfCopies);

				document2.PrinterDetails.PrintQueuePK = ZGuid.Empty;

				printTask.Run(deliveryInstructions);
				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);

				var stmPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(2, stmPrintJobs.Length);
				AssertContainsExactElementsInAnyOrder("there should be 2 and 3.", new ZShort[2] { 2, 3 }, stmPrintJobs.Select(printJob => printJob.SP_Copies));
				AssertContainsExactElementsInAnyOrder("there should be printer1 and printer3.", new ZGuid[2] { stmPrintQueue1.PK, stmPrintQueue3.PK }, stmPrintJobs.Select(printJob => printJob.SP_SQ));

				stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query2);
				AssertEquals("there should be no default printer for pivot2.", 0, stmDefaultPrinters.Length);
			}
		}

		public void TestResetDeliveryInstructionsEDocsOnEndDocProcessing()
		{
			AssertResetDeliveryInstructionsEDocsOnEndDocProcessing(DeliveryInstructionDestination.Print, false);
			AssertResetDeliveryInstructionsEDocsOnEndDocProcessing(DeliveryInstructionDestination.Preview, true);
			AssertResetDeliveryInstructionsEDocsOnEndDocProcessing(DeliveryInstructionDestination.DocConfigPreview, true);
		}

		void AssertResetDeliveryInstructionsEDocsOnEndDocProcessing(DeliveryInstructionDestination destination, bool expectedShouldPrintByDefault)
		{
			var docPack = new DocumentPack();
			var dumy = Factory.New<DocumentPrintSetTest.DummyBizoStorageDocs>();
			docPack.Add(dumy);
			var report = new Report(new DocumentPack(), TestReport);
			docPack.Add(report);
			var deliveryInstruction = new DeliveryInstructions(docPack);
			deliveryInstruction.Destination = destination;

			AssertEquals("EDoc's IncludedInPrint should be false", false, dumy.IncludedInPrint);

			using (var printTask = new PrintTask())
			{
				dumy.IncludedInPrint = true;
				dumy.ShouldPrintByDefault = true;
				AssertEquals("EDoc's IncludedInPrint should be true", true, dumy.IncludedInPrint);
				AssertEquals("EDoc's ShouldPrintByDefault should be true", true, dumy.ShouldPrintByDefault);

				printTask.Run(deliveryInstruction, null);

				AssertEquals("EDoc's IncludedInPrint should be true", true, dumy.IncludedInPrint);
				AssertEquals("EDoc's ShouldPrintByDefault", expectedShouldPrintByDefault, dumy.ShouldPrintByDefault);
			}
		}

		public void TestRunWithEmailSubjectBeOverriddenByContact()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			using (var mockReport = new MockReport(TestReport))
			{
				var testTask = new MockPrintTask();
				var testPack = new DocumentPack { mockReport };

				testTask.Add(testPack);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Test Company";

				testPack.DocumentSupporter = new AutoDeliveryBizO(org);

				var taskSettings = new PrintTaskSettings(testTask) { Destination = DeliveryInstructionDestination.TakenFromContact };
				var instruction = taskSettings.DocPacksDeliveryInstructions[0];
				instruction.Recipients.RemoveAndDeleteAll();

				var contact = instruction.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.EmailSubjectMacro = "Hello <OH_FullName>";
				contact.Email = "unit.test@cw1.com";

				var oldGroup = instruction.DeliveryGroups[0];
				testTask.Run(taskSettings);

				var newGroup = instruction.DeliveryGroups[0];
				AssertEquals(1, instruction.DeliveryGroups.Count);
				AssertNotEquals(oldGroup, newGroup);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("One print job should be there", 1, printJobs.Length);
				AssertEquals("Print job should be Processed", ZBool.True, printJobs[0].DeliveryGroup.SB_IsProcessed);

				var groups = Factory.Load<StmDeliveryGroup>(new ZQuery());
				AssertEquals("One delivery group should be there", 1, groups.Length);
				AssertEquals("Hello Test Company", groups[0].SB_EmailSubjectLine);
				AssertEquals(printJobs[0].DeliveryGroup, groups[0]);
			}
		}

		public void TestDeliveryInfoProcessedWithinGivenRun()
		{
			DeliveryInfo deliveryInfo1 = new DeliveryInfo(DeliveryFormats.Document);
			deliveryInfo1.Name = "damn you, doc engine!!!";
			deliveryInfo1.DeliveryGroupID = ZGuid.NewZGuid();

			PrintTask printTask = new PrintTask();
			AssertNoExceptionThrown("Methods are expected to check for null references and stay cool", () =>
			{
				AssertEquals(false, printTask.IsSimilarDeliveryAlreadyProcessed(null, null));
				printTask.NotifyDeliveryInfoCreated(null, null);
				AssertEquals(false, printTask.IsSimilarDeliveryAlreadyProcessed(null, null));
			});

			AssertEquals("No similar delivery already processed", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, null));

			printTask.NotifyDeliveryInfoCreated(deliveryInfo1, null);
			AssertEquals("Similar delivery already processed", true, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, null));

			DeliveryInfo deliveryInfo2 = new DeliveryInfo(DeliveryFormats.Document);
			deliveryInfo2.Name = "burn in hell, foul incident";
			deliveryInfo2.DeliveryGroupID = ZGuid.NewZGuid();
			AssertEquals("No similar delivery already processed", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, null));

			printTask.NotifyDeliveryInfoCreated(deliveryInfo2, null);
			AssertEquals("Similar delivery already processed", true, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, null));

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.DeliveryAddress = "hello@cruelworld.com";

			AssertEquals("Contact not matched", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, contact));
			AssertEquals("Contact not matched", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, contact));

			printTask.NotifyDeliveryInfoCreated(deliveryInfo1, contact);
			AssertEquals("Both DeliveryInfo and DeliveryContact matched", true, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, contact));
			AssertEquals("Contact not matched", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, contact));

			DocDeliveryContact anotherContact = new DocDeliveryContact(Factory);
			anotherContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			anotherContact.DeliveryAddress = "docengine@debug.com";

			AssertEquals("Contact not matched", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, anotherContact));
			AssertEquals("Contact not matched", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, anotherContact));

			AssertEquals("Precondition: similar delivery found", true, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, contact));
			AssertEquals("Precondition: similar delivery found", true, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, null));

			printTask.Run(new DeliveryInstructions());
			AssertEquals("Already delivered list is initialized on every run", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo1, contact));
			AssertEquals("Already delivered list is initialized on every run", false, printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo2, null));
		}

		public void TestDeliveryInfoSnapshotShouldContainAttachmentType()
		{
			var deliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = "Little Apple!!",
				DeliveryGroupID = ZGuid.NewZGuid()
			};

			var contact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
				DeliveryAddress = "hello@cruelworld.com",
				AttachmentType = "PDF"
			};

			var anotherContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
				DeliveryAddress = "hello@cruelworld.com",
				AttachmentType = "HTMF"
			};

			var printTask = new PrintTask();
			printTask.NotifyDeliveryInfoCreated(deliveryInfo, contact);

			Assert("Should be false if 2 contacts have different AttachmentType", !printTask.IsSimilarDeliveryAlreadyProcessed(deliveryInfo, anotherContact));
		}

		public void TestTwoDeliveryInfosWithDifferentBusinessObjectPkAreNotSimilar()
		{
			// Arrange
			var deliveryContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = EDICore.Constants.ContactNotifyModes.Email,
				DeliveryAddress = "stone@stone.com"
			};
			var similarDeliveryContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = EDICore.Constants.ContactNotifyModes.Email,
				DeliveryAddress = deliveryContact.DeliveryAddress
			};
			var unsimilarDeliveryContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = EDICore.Constants.ContactNotifyModes.Email,
				DeliveryAddress = deliveryContact.DeliveryAddress
			};
			var deliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = "Stone1",
				AttachedFilename = "Stone2",
				EmailSubjectLine = "Stone3",
				EmailSignature = "Stone4",
				RelatedBusinessContext = "Stone5",
				ShowDraftWatermark = true,
				FileFormat = "Stone6",
				DeliveryGroupID = ZGuid.NewZGuid(),
				DeliveryFormat = DeliveryFormats.Document,
				BusinessObjectPk = ZGuid.NewZGuid(),
				ParentGuid = ZGuid.NewZGuid(),
			};
			var similarDeliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = deliveryInfo.Name,
				AttachedFilename = deliveryInfo.AttachedFilename,
				EmailSubjectLine = deliveryInfo.EmailSubjectLine,
				EmailSignature = deliveryInfo.EmailSignature,
				RelatedBusinessContext = deliveryInfo.RelatedBusinessContext,
				ShowDraftWatermark = deliveryInfo.ShowDraftWatermark,
				FileFormat = deliveryInfo.FileFormat,
				DeliveryGroupID = deliveryInfo.DeliveryGroupID,
				DeliveryFormat = deliveryInfo.DeliveryFormat,
				BusinessObjectPk = deliveryInfo.BusinessObjectPk,
				ParentGuid = deliveryInfo.ParentGuid,
			};
			var unsimilarDeliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = deliveryInfo.Name,
				AttachedFilename = deliveryInfo.AttachedFilename,
				EmailSubjectLine = deliveryInfo.EmailSubjectLine,
				EmailSignature = deliveryInfo.EmailSignature,
				RelatedBusinessContext = deliveryInfo.RelatedBusinessContext,
				ShowDraftWatermark = deliveryInfo.ShowDraftWatermark,
				FileFormat = deliveryInfo.FileFormat,
				DeliveryGroupID = deliveryInfo.DeliveryGroupID,
				DeliveryFormat = deliveryInfo.DeliveryFormat,
				BusinessObjectPk = ZGuid.NewZGuid(),
				ParentGuid = deliveryInfo.ParentGuid,
			};
			var printTask = new PrintTask();
			printTask.NotifyDeliveryInfoCreated(deliveryInfo, deliveryContact);
			// Act
			var resultForSimilarDeliveryInfo = printTask.IsSimilarDeliveryAlreadyProcessed(similarDeliveryInfo, similarDeliveryContact);
			var resultForUnsimilarDeliveryInfo = printTask.IsSimilarDeliveryAlreadyProcessed(unsimilarDeliveryInfo, unsimilarDeliveryContact);
			// Assert
			Assert(resultForSimilarDeliveryInfo);
			Assert(!resultForUnsimilarDeliveryInfo);
		}

		public void TestTwoDeliveryInfoWithDifferentFileContentsAreNotSimilar()
		{
			// Arrange
			var deliveryContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = EDICore.Constants.ContactNotifyModes.Email,
				DeliveryAddress = "stone@stone.com"
			};
			var deliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = "Stone1",
				AttachedFilename = "Stone2",
				EmailSubjectLine = "Stone3",
				EmailSignature = "Stone4",
				RelatedBusinessContext = "Stone5",
				ShowDraftWatermark = true,
				FileFormat = "Stone6",
				DeliveryGroupID = ZGuid.NewZGuid(),
				DeliveryFormat = DeliveryFormats.Document,
				BusinessObjectPk = ZGuid.NewZGuid(),
				ParentGuid = ZGuid.NewZGuid(),
			};
			deliveryInfo.FileContents.Write(new byte[] { 1, 2, 3 }, 0, 3);
			var similarDeliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = deliveryInfo.Name,
				AttachedFilename = deliveryInfo.AttachedFilename,
				EmailSubjectLine = deliveryInfo.EmailSubjectLine,
				EmailSignature = deliveryInfo.EmailSignature,
				RelatedBusinessContext = deliveryInfo.RelatedBusinessContext,
				ShowDraftWatermark = deliveryInfo.ShowDraftWatermark,
				FileFormat = deliveryInfo.FileFormat,
				DeliveryGroupID = deliveryInfo.DeliveryGroupID,
				DeliveryFormat = deliveryInfo.DeliveryFormat,
				BusinessObjectPk = deliveryInfo.BusinessObjectPk,
				ParentGuid = deliveryInfo.ParentGuid,
			};
			similarDeliveryInfo.FileContents.Write(new byte[] { 1, 2, 3 }, 0, 3);
			var unsimilarDeliveryInfo = new DeliveryInfo(DeliveryFormats.Document)
			{
				Name = deliveryInfo.Name,
				AttachedFilename = deliveryInfo.AttachedFilename,
				EmailSubjectLine = deliveryInfo.EmailSubjectLine,
				EmailSignature = deliveryInfo.EmailSignature,
				RelatedBusinessContext = deliveryInfo.RelatedBusinessContext,
				ShowDraftWatermark = deliveryInfo.ShowDraftWatermark,
				FileFormat = deliveryInfo.FileFormat,
				DeliveryGroupID = deliveryInfo.DeliveryGroupID,
				DeliveryFormat = deliveryInfo.DeliveryFormat,
				BusinessObjectPk = deliveryInfo.BusinessObjectPk,
				ParentGuid = deliveryInfo.ParentGuid,
			};
			unsimilarDeliveryInfo.FileContents.Write(new byte[] { 3, 2, 1 }, 0, 3);
			var printTask = new PrintTask();
			printTask.NotifyDeliveryInfoCreated(deliveryInfo, deliveryContact);
			// Act
			var resultForSimilarDeliveryInfo = printTask.IsSimilarDeliveryAlreadyProcessed(similarDeliveryInfo, deliveryContact);
			var resultForUnsimilarDeliveryInfo = printTask.IsSimilarDeliveryAlreadyProcessed(unsimilarDeliveryInfo, deliveryContact);
			// Assert
			Assert(resultForSimilarDeliveryInfo);
			Assert(!resultForUnsimilarDeliveryInfo);
		}

		public void TestExcludeDocumentsThatHaveSectionsWhichContainNoDataRows()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=System Document Elements]

{A}-[#ConfigurableSection:BEX, Section Body 1]
{A}-[#SectionHeader]
{B}-[<ReportName>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>]    {C}-[<Collection.Z0_VarCharMax>]

{A}-[#ConfigurableSection:BEX, Section Body 2]
{A}-[#SectionHeader]
{B}-[<ReportName>]
{A}-[#SectionBody:Data=FilteredCollection]
{B}-[<FilteredCollection.Z0_Number>]    {C}-[<FilteredCollection.Z0_VarCharMax>]

{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_VarCharMax = "One";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document1 = documentCommand.Documents.AddNew();
			document1.SI_SU = documentCommand.PK;
			document1.SI_SO = template.PK;
			document1.SI_DocumentTitle = "Document 1";
			var config1 = document1.DocConfigs.AddNew();
			config1.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Section Body 1"));

			var document2 = documentCommand.Documents.AddNew();
			document2.SI_SU = documentCommand.PK;
			document2.SI_SO = template.PK;
			document2.SI_DocumentTitle = "Document 2";
			var config2 = document2.DocConfigs.AddNew();
			config2.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Section Body 2"));

			var document3 = documentCommand.Documents.AddNew();
			document3.SI_SU = documentCommand.PK;
			document3.SI_SO = template.PK;
			document3.SI_DocumentTitle = "Document 3";
			var config3 = document3.DocConfigs.AddNew();
			config3.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Section Body 1"));

			Factory.Save();

			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows = true;

			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.AttachmentType = AttachmentTypeList.Codes.Xls;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand, null, deliveryInstructions);
			AssertEquals(1, printJobs.Length);

			var result = printJobs.First();

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("Document 1 should be added because there are data rows.",
@"{B}-[Document 1]
{B}-[1]   {C}-[One]",
					excelInterface.WorkSheets[0].ToString());

				AssertMultilineASCIIEquals("Document 2 should not be added to the output because there are are no data rows.",
@"{B}-[Document 3]
{B}-[1]   {C}-[One]",
					excelInterface.WorkSheets[1].ToString());
			}
		}

		public void TestDocumentDeliveryMenuUnableToBeShownWhilstValidationErrorsExistForADocumentMenuItem()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_DocumentDirection = "NOT";

			PrintTask task = new PrintTask(command);
			AssertEquals("The destination for document delivery should be none when validation fails", task.Run(Env.Security.None, true), DeliveryInstructionDestination.None);

			var dialogBox = UnitTestUserNotification.Instance;
			AssertEquals("A popup dialog box appears telling the user that they have errors in validation", "Please resolve errors before trying to run the document", dialogBox.LastMessage.Text);
		}

		public void TestIsDisposed()
		{
			var printTask = new PrintTask();
			AssertEquals("printTask.IsDisposed", false, printTask.IsDisposed);
			printTask.Dispose();
			AssertEquals("printTask.IsDisposed", true, printTask.IsDisposed);
		}

		public void TestDeliveryMethodAfterCloneInstructions()
		{
			var menuItem = Factory.New<StmMenuItem>();
			DeliveryInstructions originalInstructions = new DeliveryInstructions();

			DeliveryInstructions instructionsclone1 = (DeliveryInstructions)originalInstructions.Clone();
			DeliveryInstructions instructionsclone2 = (DeliveryInstructions)originalInstructions.Clone();

			menuItem.SU_PreventAutoDelivery = false;
			var testTask1 = new MockPrintTaskAutoDeliveryPermission(menuItem);
			testTask1.RunWithPartialInstructions(instructionsclone1.DeliveryOptions, instructionsclone1, Env.Security.None);
			AssertEquals("PermissionForAutoDelivery should be true", true, testTask1.AllowedToPerformAutoDelivery);
			AssertEquals("DeliveryMethod should be email", Core.Constants.ContactNotifyModes.Email, instructionsclone1.Recipients[0].DeliveryMethod);

			menuItem.SU_PreventAutoDelivery = true;
			var testTask2 = new MockPrintTaskAutoDeliveryPermission(menuItem);
			testTask2.RunWithPartialInstructions(instructionsclone2.DeliveryOptions, instructionsclone2, Env.Security.None);
			AssertEquals("PermissionForAutoDelivery should be false", false, testTask2.AllowedToPerformAutoDelivery);
			AssertEquals("DeliveryMethod should be print", Core.Constants.ContactNotifyModes.Print, instructionsclone2.Recipients[0].DeliveryMethod);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesBeDefaultedCorrectlyWhenRunningPrintTask()
		{
			RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Core.SharedConstants.Languages.EnglishBritish);
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var command = Factory.New<DocumentCommand>();
			var documentSupportable = Factory.New<DummyBODocSupportable>();
			command.Parent = documentSupportable;
			command.SU_IsDocPack = true;
			command.SU_PreventAutoDelivery = true;
			command.SU_ContactType = ContactType.FreightAgent.Code;
			var template1 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 1");
			var template2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2");
			var pivot1 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template1, command);
			pivot1.SI_PrintByDefault = true;
			pivot1.SI_PrintCopyType = "PRN";
			var pivot2 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 2", template2, command);
			pivot2.SI_PrintByDefault = false;
			pivot2.SI_PrintCopyType = "PRN";

			using (var testTask = new PrintTask(command))
			{
				var pack = new DocumentPack(command, documentSupportable, null, null);
				testTask.Add(pack);
				testTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, null, null);

				AssertEquals(1, testTask.DeliveryInstructionsForDeliveryForm.DeliverablesToBePrinted.OfType<Report>().Count(r => r.IncludedInPrint));
			}

			RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Core.SharedConstants.Languages.EnglishAmerican);
		}

		public void TestDocumentDeliveryMethod_PreventAutoDeliveryOrNot()
		{
			var documentCommand = Factory.New<DocumentCommand>();

			CombineAssertions(() =>
			{
				AssertDocumentDeliveryMethodWhenPreventAutoDeliveryOrNot(documentCommand, true);
				AssertDocumentDeliveryMethodWhenPreventAutoDeliveryOrNot(documentCommand, false);
			});
		}

		void AssertDocumentDeliveryMethodWhenPreventAutoDeliveryOrNot(DocumentCommand documentCommand, bool allowAutoDelivery)
		{
			if (allowAutoDelivery)
			{
				documentCommand.SU_PreventAutoDelivery = false;
				documentCommand.SU_ContactType = ContactType.CTO.Code;
			}
			else
			{
				documentCommand.SU_PreventAutoDelivery = true;
				documentCommand.SU_ContactType = ContactType.NoContactType.Code;
			}

			var documentSupportable = new MockDocSupportBizO();
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack(documentCommand, documentSupportable, null, null, false))
			using (var printTask = new MockPrintTaskAutoDeliveryPermission(documentCommand))
			{
				var report = new Report(documentPack, excelTemplate);
				documentPack.Add(report);
				printTask.Add(documentPack);
				printTask.Run(AllowedDeliveryOptions.All, Env.Security.None);

				AssertEquals($"should {(allowAutoDelivery ? null : "not")} allow auto delivery", allowAutoDelivery, printTask.AllowedToPerformAutoDelivery);

				var deliveryMethod = printTask.DeliveryInstructionsForDeliveryForm.Recipients[0].DeliveryMethod;

				if (allowAutoDelivery)
				{
					AssertEquals("Delivery method should be email", Core.Constants.ContactNotifyModes.Email, deliveryMethod);
				}
				else
				{
					AssertNotEquals("Delivery method should not be email", Core.Constants.ContactNotifyModes.Email, deliveryMethod);
					AssertEquals("Delivery method should be print", Core.Constants.ContactNotifyModes.Print, deliveryMethod);
				}
			}
		}

		public void TestDeliveryInstuctionsIsDraftOnDeliveryRequested()
		{
			bool initialIsUserInteractive = Globals.IsUserInteractive;

			try
			{
				Globals.IsUserInteractive = false;

				var menuItem = Factory.New<StmMenuItem>();

				using (var documentPack = new TestableDocumentPackWithContacts(menuItem))
				using (var printTask = new PrintTask(menuItem))
				{
					var deliveryInstructions = new DeliveryInstructions(documentPack);

					menuItem.SU_DraftOption = DraftOptionsList.Codes.Both;
					menuItem.SU_ContactType = ContactType.NoContactType.Code;
					printTask.Form_DeliveryRequested(this, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
					AssertEquals("deliveryInstructions.IsDraft", ZBool.False, deliveryInstructions.IsDraft);
					AssertEquals("deliveryInstructions.IsDraft_ReadOnly", false, deliveryInstructions.IsDraft_ReadOnly);

					menuItem.SU_DraftOption = DraftOptionsList.Codes.Draft;
					printTask.Form_DeliveryRequested(this, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
					AssertEquals("deliveryInstructions.IsDraft", ZBool.True, deliveryInstructions.IsDraft);
					AssertEquals("deliveryInstructions.IsDraft_ReadOnly", true, deliveryInstructions.IsDraft_ReadOnly);

					menuItem.SU_DraftOption = DraftOptionsList.Codes.Final;
					printTask.Form_DeliveryRequested(this, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
					AssertEquals("deliveryInstructions.IsDraft", ZBool.False, deliveryInstructions.IsDraft);
					AssertEquals("deliveryInstructions.IsDraft_ReadOnly", true, deliveryInstructions.IsDraft_ReadOnly);
				}
			}
			finally
			{
				Globals.IsUserInteractive = initialIsUserInteractive;
			}
		}

		public void TestRunTaskFromMenusCustomisationFormUpdatesPacks()
		{
			PrintTask printTask = new PrintTask();
			DocumentPack pack1 = new DocumentPack();
			DocumentPack pack2 = new DocumentPack();
			List<DocumentPack> packs = new List<DocumentPack>();
			packs.Add(pack1);
			packs.Add(pack2);
			printTask.AddRange(packs);
			AssertEquals("Pack1 IsRunFromMenusCustomisationForm should be false", false, pack1.IsRunFromMenusCustomisationForm);
			AssertEquals("Pack2 IsRunFromMenusCustomisationForm should be false", false, pack2.IsRunFromMenusCustomisationForm);
			printTask.Run(Env.Security.None, true);
			AssertEquals("Pack1 IsRunFromMenusCustomisationForm should be true", true, pack1.IsRunFromMenusCustomisationForm);
			AssertEquals("Pack2 IsRunFromMenusCustomisationForm should be true", true, pack2.IsRunFromMenusCustomisationForm);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunWithTaskSettingsTwice()
		{
			using (PrintTask printTask = new PrintTask())
			{
				ReportCommand reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), Factory);
				DocumentPack documentPack = new DocumentPack(reportCommand);
				printTask.Add(documentPack);

				PrintTaskSettings taskSettings = new PrintTaskSettings(printTask);
				taskSettings.Destination = DeliveryInstructionDestination.TakenFromContact;
				foreach (DeliveryInstructions deliveryInstruction in taskSettings.DocPacksDeliveryInstructions)
				{
					deliveryInstruction.Recipients.RemoveAndDeleteAll();
					DocDeliveryContact contact = deliveryInstruction.Recipients.AddNew();
					contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					contact.AttachmentType = OrgConstants.AttachmentType.PDF;
					contact.Email = "unit.test@cargowise.com";
				}

				printTask.Run(taskSettings);
				AssertMultilineASCIIEquals("",
	@"{A}-[#config]
{A}-[data:test=select top 2 name from sys.objects order by object_id]
{A}-[#SectionBody:data=test]
{B}-[<test.name>]
{A}-[#endofreport]",
					((Report)documentPack[0]).WorkSheetCurrentlyBeingProcessed.ToString());

				printTask.Run(taskSettings);
				AssertMultilineASCIIEquals("",
	@"{A}-[#config]
{A}-[data:test=select top 2 name from sys.objects order by object_id]
{A}-[#SectionBody:data=test]
{B}-[<test.name>]
{A}-[#endofreport]",
					((Report)documentPack[0]).WorkSheetCurrentlyBeingProcessed.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunDeliveryInstructionsWithExceptionsInSecondPack()
		{
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), Factory);
			var deliveryInstructions = GetDeliveryInstrunctions();

			using (var printTask = new PrintTask(reportCommand))
			{
				var documentPack1 = new DocumentPack(reportCommand);
				var documentPack2 = new DocumentPack(reportCommand);
				documentPack2.OnAfterReportRun += (sender, item) =>
				{
					throw new InvalidOperationException();
				};

				printTask.Add(documentPack1);
				printTask.Add(documentPack2);
				AssertExceptionThrown<InvalidOperationException>(() => printTask.Run(deliveryInstructions));
				AssertEquals("DeliveryGroup should have been deleted.", true, deliveryInstructions.DeliveryGroups[0].IsDeleted);
				AssertEquals("Print jobs should have been purged.", 0, deliveryInstructions.DeliveryGroups[0].PrintJobs.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunPrintTaskWithLoginException()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report";
			reportCommand.SU_IsSystemDefined = true;

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			template.SO_IsSystemDefined = true;

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = reportCommand.SU_MenuName;
			menuTemplatePivot.SI_SU = reportCommand.PK;
			menuTemplatePivot.SI_SO = template.PK;
			menuTemplatePivot.SI_IsSystemDefined = true;

			var deliveryInstructions = GetDeliveryInstrunctions();

			var hasExceptionOccured = false;
			var exCatched = new Exception();
			using (var printTask = new PrintTask(reportCommand))
			{
				try
				{
					var documentPack1 = new DocumentPack(reportCommand);
					var documentPack2 = new DocumentPack(reportCommand);
					documentPack2.OnAfterReportRun += (sender, item) =>
					{
						throw new LoginException("LoginException", null);
					};

					printTask.Add(documentPack1);
					printTask.Add(documentPack2);
					printTask.Run(deliveryInstructions);
				}
				catch (Exception ex)
				{
					exCatched = ex;
					hasExceptionOccured = true;
				}

				Assert("Exception occurred", hasExceptionOccured);
				AssertEquals("CargoWise.Data.LoginException", exCatched.GetType().ToString());
				Assert("should not in ErrorReporter", ErrorReporter.LastExceptionReported == null);
			}
		}

		DeliveryInstructions GetDeliveryInstrunctions()
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Destination = DeliveryInstructionDestination.Auto;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var contact = deliveryInstructions.Recipients.AddNew();
			contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.Email = "unit.test@cargowise.com";

			return deliveryInstructions;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunWithDeliveryInstructionsTwice()
		{
			ReportCommand reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), Factory);

			DeliveryInstructions deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			DocDeliveryContact contact = deliveryInstructions.Recipients.AddNew();
			contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.Email = "unit.test@cargowise.com";

			using (PrintTask printTask = new PrintTask(reportCommand))
			{
				DocumentPack documentPack = new DocumentPack(reportCommand);

				printTask.Add(documentPack);
				printTask.Run(deliveryInstructions);

				AssertMultilineASCIIEquals("",
	@"{A}-[#config]
{A}-[data:test=select top 2 name from sys.objects order by object_id]
{A}-[#SectionBody:data=test]
{B}-[<test.name>]
{A}-[#endofreport]",
					((Report)documentPack[0]).WorkSheetCurrentlyBeingProcessed.ToString());

				printTask.Run(deliveryInstructions);

				AssertMultilineASCIIEquals("",
	@"{A}-[#config]
{A}-[data:test=select top 2 name from sys.objects order by object_id]
{A}-[#SectionBody:data=test]
{B}-[<test.name>]
{A}-[#endofreport]",
					((Report)documentPack[0]).WorkSheetCurrentlyBeingProcessed.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowRuntimeOptionsDoesNotSetReportRendererNull()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			command.SU_MenuName = "Test Report";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";
			template.SO_Template = excelTemplate.GetAsByteArray();

			StmMenuTemplatePivotBase menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = "Test Document";
			menuTemplatePivot.SI_SU = command.PK;
			menuTemplatePivot.SI_SO = template.PK;
			Factory.Save();

			using (ReportPrintSet printSet = new ReportPrintSet(command))
			{
				DeliveryInstructions deliveryInstruction = new DeliveryInstructions(printSet[0]);
				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				using (PrintTask printTask = new PrintTask())
				{
					printTask.Add(printSet[0]);
					printTask.IsReportPrintSet = true;

					Report report = printSet[0][0] as Report;
					AssertNotNull("Pre-condition: Report should not be null.", report);
					report.PrepareForRender();
					AssertNotNull("Pre-condition: Report renderer should not be null.", report.Renderer);

					mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<Enterprise.ZArchitecture.Modules.ISecurityCheckpoint>())).Returns(true);

					printTask.RunWithPartialInstructions(deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None);
					AssertNotNull("Report renderer should not be null.", report.Renderer);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRuntimeOptionsFormIsShownWhenNeeded()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			command.SU_MenuName = "Test Report";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";
			template.SO_Template = excelTemplate.GetAsByteArray();

			StmMenuTemplatePivotBase menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = "Test Document";
			menuTemplatePivot.SI_SU = command.PK;
			menuTemplatePivot.SI_SO = template.PK;
			Factory.Save();

			using (ReportPrintSet printSet = new ReportPrintSet(command))
			{
				DeliveryInstructions deliveryInstruction = new DeliveryInstructions(printSet[0]);
				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				using (PrintTask printTask = new PrintTask())
				{
					printTask.Add(printSet[0]);
					printTask.IsReportPrintSet = true;

					bool isRuntimeOptionsUIShown = false;
					mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(printTask, deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None)).Returns(() =>
					{
						isRuntimeOptionsUIShown = true;
						return true;
					});

					printTask.RunWithPartialInstructions(deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None);
					AssertEquals("isRuntimeOptionsUIShown", true, isRuntimeOptionsUIShown);
					AssertEquals("DeliveryInstructionsForDeliveryForm is set.", deliveryInstruction, printTask.DeliveryInstructionsForDeliveryForm);
				}
			}
		}

		public void TestDocumentCommandDoNotCallRuntimeOptionsForm()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			using (var documentPrintSet = new DocumentPrintSet(documentCommand))
			{
				var deliveryInstruction = new DeliveryInstructions();
				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				{
					bool isRuntimeOptionsFormShown = false;
					mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
					mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(documentPrintSet, deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None)).Returns(() =>
					{
						isRuntimeOptionsFormShown = true;
						return true;
					});

					documentPrintSet.RunWithPartialInstructions(deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None);
					Assert("isRuntimeOptionsFormShown", !isRuntimeOptionsFormShown);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestDoNotPrintWhenRunTimeOptionsFormDoesNotResolveTheIsssue_All()
		{
			AssertDoNotPrintWhenRunTimeOptionsFormDoesNoResolveTheIsssue(AllowedDeliveryOptions.All);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestDoNotPrintWhenRunTimeOptionsFormDoesNotResolveTheIsssue_HardCopy()
		{
			AssertDoNotPrintWhenRunTimeOptionsFormDoesNoResolveTheIsssue(AllowedDeliveryOptions.HardCopyOnly);
		}

		void AssertDoNotPrintWhenRunTimeOptionsFormDoesNoResolveTheIsssue(AllowedDeliveryOptions deliveryOptions)
		{
			var command = Factory.New<ReportCommand>();
			command.SU_MenuName = "Test Report";

			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = "Test Document";
			menuTemplatePivot.SI_SU = command.PK;
			menuTemplatePivot.SI_SO = template.PK;

			Factory.Save();

			using (var printSet = new ReportPrintSet(command))
			{
				var deliveryInstruction = new DeliveryInstructions(printSet[0]);
				deliveryInstruction.Destination = DeliveryInstructionDestination.Print;
				deliveryInstruction.DeliveryOptions = deliveryOptions;

				var printTaskUIProvider = new Mock<IPrintTaskUIProvider>();

				using (new PrintTaskUIProviderFactory.OverriderForTesting(printTaskUIProvider.Object))
				using (var printTask = new PrintTask())
				{
					printTask.Add(printSet[0]);
					printTask.IsReportPrintSet = true;

					printTask.RunWithPartialInstructions(deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None);

					printTaskUIProvider.Verify(p => p.ShowRuntimeOptionsUI(printTask, deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None));

					printTaskUIProvider.Verify(p => p.GetNewProgressNotificationUI(
						It.Is<DeliveryInstructions>(i => i.Destination == DeliveryInstructionDestination.UserCancelled),
						It.IsAny<int>()));
				}
			}
		}

		public void TestRuntimeOptionsFormShownIsDependantOnTypeProvided()
		{
			using (var printTask1 = new PrintTask())
			{
				AssertEquals(PrintTaskUIProviderTypes.None, printTask1.PrintTaskUIProviderType);
				AssertEquals("PrintTaskFormsProvider", printTask1.PrintTaskUIProvider.GetType().Name);
			}

			using (var printTask2 = new PrintTask())
			{
				printTask2.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;
				AssertEquals(typeof(UnattendedPrintTaskUIProvider), printTask2.PrintTaskUIProvider.GetType());
			}
		}

		public void TestDocPackLanguageSetByUI()
		{
			var printTask = new PrintTask();
			var pack1 = new DocumentPack();
			pack1.Language = EDICore.Constants.Languages.French;
			var pack2 = new DocumentPack();
			var packs = new List<DocumentPack>();
			packs.Add(pack1);
			packs.Add(pack2);
			printTask.AddRange(packs);

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				printTask.Run(Env.Security.None);
				foreach (var documentPack in printTask.GetDocumentPacks())
				{
					AssertEquals(EDICore.Constants.Languages.French, documentPack.Language);
				}
			}
		}

		public void TestDocumentPackLanguage_WhenForcedLanguageExists_ShouldBeForcedLanguage()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pack = new DocumentPack(documentCommand);
			pack.Language = EDICore.Constants.Languages.French;
			var packs = new List<DocumentPack>();
			packs.Add(pack);

			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_AddInfo = Core.SharedConstants.Languages.ChineseSimplified;

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=<Z0_AddInfo>]
{A}-[#EndOfReport]");
			var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(dataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
			pack.Add(report);

			var printTask = new PrintTask();
			printTask.AddRange(packs);

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				printTask.Run(Env.Security.None);
				foreach (var documentPack in printTask.GetDocumentPacks())
				{
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, documentPack.Language);
				}
			}
		}

		public void TestDocPackLanguages_DocumentDeliveryDefaultLanguages()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.Constants.Languages.German;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			branch.GB_RL_NKHomePort = "AUSYD";
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.OH_Language = Core.Constants.Languages.Dutch;
			branch.GB_OH_OrgProxy = branchProxy.PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var resultsCompany = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 1 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsCompany))
				{
					var menuItem = Factory.New<StmMenuItem>();
					menuItem.SU_ContactType = ContactType.All.Code;
					menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;

					var pack = new DocumentPack(menuItem) { DocumentSupporter = new AutoDeliveryBizO(org) };

					var printTask = new PrintTask();
					printTask.Add(pack);

					var deliveryInstructions = new DeliveryInstructions(pack) { Destination = DeliveryInstructionDestination.Auto };
					printTask.RunDocumentPack(deliveryInstructions, pack);
					var documentPacks = printTask.GetDocumentPacks().ToList();
					AssertEquals(1, documentPacks.Count);
					AssertEquals(Core.Constants.Languages.German, documentPacks[0].Language);
				}
			}
		}

		public void TestDocPackLanguageIsMaintainedThroughAutoDelivery()
		{
			var printTask = new PrintTask();
			var pack = new DocumentPack();
			pack.Language = EDICore.Constants.Languages.French;
			printTask.Add(pack);

			printTask.RunDocumentPack(new DeliveryInstructions(pack), pack);

			foreach (var documentPack in printTask.GetDocumentPacks())
			{
				AssertEquals(EDICore.Constants.Languages.French, documentPack.Language);
			}
		}

		public void TestTaskSettings()
		{
			PrintTask task = new PrintTask();
			AssertNotNull(task.TaskSettings);

			PrintTaskSettings taskSettings = task.TaskSettings;
			AssertEquals(taskSettings, task.TaskSettings);
		}

		public void TestRunWithDeliveryOptions_PreviewOnly()
		{
			var task = new PrintTask();
			var destination = task.Run(AllowedDeliveryOptions.PreviewOnly, Env.Security.None);
			AssertEquals("Preview immediately - no docdeliveryform", DeliveryInstructionDestination.Preview, destination);

			destination = task.Run(AllowedDeliveryOptions.All, Env.Security.None);
			AssertEquals("Default when showing DocDeliveryForm is cancelled", DeliveryInstructionDestination.UserCancelled, destination);
		}

		public void TestRunWithDeliveryFormWithDeliveryOptionsOtherThanHardCopyOnly()
		{
			MockPrintTask testTask = new MockPrintTask();
			testTask.RunWithDeliveryForm(AllowedDeliveryOptions.All, Env.Security.None);

			AssertEquals("Delivery form should have been shown", 1, testTask.ShowDeliveryFormCount);
		}

		public void TestRunWithDeliveryFormWithDeliveryOptionsHardCopyOnly()
		{
			MockPrintTask testTask = new MockPrintTask();
			testTask.RunWithDeliveryForm(AllowedDeliveryOptions.HardCopyOnly, Env.Security.None);

			AssertEquals("HardCopyOnly form should have been shown", 1, testTask.ShowHardCopyOnlyFormCount);
		}

		public void TestDraftOptionsDefaults()
		{
			StmMenuItem testItem = Factory.New<StmMenuItem>();
			MockPrintTask testTask = new MockPrintTask(testItem);
			DeliveryInstructions instructions = new DeliveryInstructions();

			testItem.SU_DraftOption = DraftOptionsList.Codes.Both;

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(!instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(!instructions.IsDraft_ReadOnly);

			testItem.SU_DraftOption = DraftOptionsList.Codes.Final;

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(!instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			testItem.SU_DraftOption = DraftOptionsList.Codes.Draft;

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = true;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = true;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);

			instructions.IsDraft = false;
			instructions.IsDraft_ReadOnly = false;
			testTask.SetDraftOptions_ExposedForTest(instructions);
			Assert(instructions.IsDraft);
			Assert(instructions.IsDraft_ReadOnly);
		}

		public void TestAddRange()
		{
			PrintTask task = new PrintTask();
			DocumentPack pack1 = null;
			DocumentPack pack2 = new DocumentPack();
			DocumentPack pack3 = new DocumentPack();
			List<DocumentPack> packs = new List<DocumentPack>();
			packs.Add(pack1);
			packs.Add(pack2);
			packs.Add(pack3);

			AssertExceptionThrown(typeof(ArgumentNullException), () => { task.AddRange(packs); });
			packs.Remove(pack1);
			AssertNoExceptionThrown(() => { task.AddRange(packs); });
			AssertEquals(2, task.Count);
		}

		public void TestThatWeCantNullTheDocumentPacksInTheList()
		{
			PrintTask task = new PrintTask();
			DocumentPack pack1 = new DocumentPack();
			DocumentPack pack2 = new DocumentPack();
			DocumentPack pack3 = new DocumentPack();

			task.Add(pack1);
			task.Add(pack2);
			task.Add(pack3);

			List<DocumentPack> packs = new List<DocumentPack>(task.GetDocumentPacks());
			packs[1] = null;

			foreach (DocumentPack pack in task.GetDocumentPacks())
			{
				AssertNotNull(pack);
			}

			AssertExceptionThrown(typeof(ArgumentNullException), () => { task[0] = null; });
			AssertExceptionThrown(typeof(ArgumentNullException), () => { task[2] = null; });
			DocumentPack packPointerToNull = task[1];
			packPointerToNull = null;

			foreach (DocumentPack pack in task.GetDocumentPacks())
			{
				AssertNotNull(pack);
			}
		}

		public void TestDocPacksWithIncompleteRecipientDetailsNotDelivered()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var instructions = new DeliveryInstructions();
			instructions.Recipients.RemoveAndDeleteAll();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGCODE1";
			var orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_Email = "email@test.com";
			orgContact1.OC_ContactName = "Justin";
			var orgContact2 = org1.Contacts.AddNew();
			orgContact2.OC_Email = "";
			orgContact2.OC_ContactName = "Jeff";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORGCODE2";
			var orgContact3 = org2.Contacts.AddNew();
			orgContact3.OC_Email = "";
			orgContact3.OC_ContactName = "James";

			Factory.Save();

			var task = new PrintTask();
			var pack1 = new DocumentPack();
			var pack2 = new DocumentPack();
			var pack3 = new DocumentPack();

			using (var mockReport = new MockReport(TestReport))
			{
				pack1.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact1 = pack1.DeliveryInstructions.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.OrgHeaderPK = org1.PK;
				contact1.Name = "Justin";
				contact1.Email = orgContact1.Email;
				pack1.Add(mockReport);

				pack2.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact2 = pack2.DeliveryInstructions.Recipients.AddNew();
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact2.OrgHeaderPK = org1.PK;
				contact2.Email = orgContact2.Email;
				contact2.Name = "Jeff";
				pack2.Add(mockReport);

				pack3.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact3 = pack3.DeliveryInstructions.Recipients.AddNew();
				contact3.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact3.OrgHeaderPK = org2.PK;
				contact3.Email = orgContact3.Email;
				contact3.Name = "James";
				pack3.Add(mockReport);

				task.Add(pack1);
				task.Add(pack2);
				task.Add(pack3);

				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				{
					mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
					mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);

					instructions.DocPack = pack2;
					instructions.Recipients.Add(contact2);
					task.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);

					var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

					AssertEquals("Only 1 StmPrintJob should have been created", 1, printJobs.Length);
					AssertEquals("Warning message should have been shown to the user.", @"These documents could not be delivered as the following Contacts do not have an email address:

Eagle Datamation International - BN - AUBNE
  Organisation: ORGCODE1
    Contact: Jeff
Eagle Datamation International - BN - AUBNE
  Organisation: ORGCODE2
    Contact: James

Please add an email address to these Contacts and deliver the documents again."
							, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertContains("An Email was created without any recipients", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestDocPacksWithIncompleteRecipientDetailsNotDeliveredWhenContactIsInactive()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var instructions = new DeliveryInstructions();
			instructions.Recipients.RemoveAndDeleteAll();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGCODE1";
			var orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_Email = "email@test.com";
			orgContact1.OC_ContactName = "Justin";
			var orgContact2 = org1.Contacts.AddNew();
			orgContact2.OC_Email = "";
			orgContact2.OC_ContactName = "Jeff";
			orgContact2.OC_IsActive = false;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORGCODE2";
			var orgContact3 = org2.Contacts.AddNew();
			orgContact3.OC_Email = "";
			orgContact3.OC_ContactName = "James";

			Factory.Save();

			var task = new PrintTask();
			var pack1 = new DocumentPack();
			var pack2 = new DocumentPack();
			var pack3 = new DocumentPack();

			using (var mockReport = new MockReport(TestReport))
			{
				pack1.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact1 = pack1.DeliveryInstructions.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.OrgHeaderPK = org1.PK;
				contact1.Name = "Justin";
				contact1.Email = orgContact1.Email;
				pack1.Add(mockReport);

				pack2.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact2 = pack2.DeliveryInstructions.Recipients.AddNew();
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact2.OrgHeaderPK = org1.PK;
				contact2.Email = orgContact2.Email;
				contact2.Name = "Jeff";
				pack2.Add(mockReport);

				pack3.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
				var contact3 = pack3.DeliveryInstructions.Recipients.AddNew();
				contact3.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact3.OrgHeaderPK = org2.PK;
				contact3.Email = orgContact3.Email;
				contact3.Name = "James";
				pack3.Add(mockReport);

				task.Add(pack1);
				task.Add(pack2);
				task.Add(pack3);

				var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
				{
					mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
					mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);

					instructions.DocPack = pack2;
					instructions.Recipients.Add(contact2);
					task.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);

					var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

					AssertEquals("Only 1 StmPrintJob should have been created", 1, printJobs.Length);
					AssertEquals("Warning message should have been shown to the user.", @"These documents could not be delivered as the following Contacts do not have an email address:

Eagle Datamation International - BN - AUBNE
  Organisation: ORGCODE1
    Contact: Jeff
Eagle Datamation International - BN - AUBNE
  Organisation: ORGCODE2
    Contact: James

Please add an email address to these Contacts and deliver the documents again."
							, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertContains("An Email was created without any recipients", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestRunWithTaskSettingsIfUserCancelled()
		{
			MockPrintTask testTask = new MockPrintTask();
			MockDocumentPack testPack1 = new MockDocumentPack();
			MockDocumentPack testPack2 = new MockDocumentPack();
			PrintTaskSettings taskSettings = new PrintTaskSettings(testTask);
			testTask.Add(testPack1);
			testTask.Add(testPack2);
			taskSettings.Destination = DeliveryInstructionDestination.UserCancelled;
			testTask.Run(taskSettings);

			AssertEquals("Pack1 should not be run", 0, testPack1.RunCount);
			AssertEquals("Pack2 should not be run", 0, testPack2.RunCount);
		}

		public void TestAddAndRun()
		{
			MockPrintTask testTask = new MockPrintTask();
			MockDocumentPack testPack1 = new MockDocumentPack();
			MockDocumentPack testPack2 = new MockDocumentPack();
			testTask.Add(testPack1);
			testTask.Add(testPack2);
			testTask.Run(Env.Security.None);

			AssertEquals("There were no instructions, so the form should have been shown", 1, testTask.ShowFormCount);
			AssertEquals("Pack1 should have been run once", 1, testPack1.RunCount);
			AssertEquals("Pack2 should have been run once", 1, testPack2.RunCount);
		}

		public void TestRunWithPartialInstructions()
		{
			DeliveryInstructions instruction = new DeliveryInstructions(new DocumentPack());
			instruction.TIFAttachmentsOnly = true;
			DocDeliveryContact contact = instruction.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			MockPrintTask testTask = new MockPrintTask();
			testTask.RunWithPartialInstructions(instruction.DeliveryOptions, instruction, Env.Security.None);

			AssertEquals("Partial instructions specified, therefore form should be shown", 1, testTask.ShowFormCount);
			AssertEquals("Passed in instructions are the same as the instructions used on the last form", instruction, testTask.LastShownInstructions);
		}

		void AfterReportRunCallBackForTest(object sender, IDeliverable itemToDeliver)
		{
			AfterReportRunCallBackCount++;
		}

		int AfterReportRunCallBackCount;

		public void TestAfterReportRunCallBackWhenRunWithTaskSettings()
		{
			AfterReportRunCallBackCount = 0;
			MockPrintTask testTask = new MockPrintTask();
			MockDocumentPack testPack1 = new MockDocumentPack();
			MockDocumentPack testPack2 = new MockDocumentPack();
			testTask.Add(testPack1);
			testTask.Add(testPack2);
			testTask.AfterReportRun += new PrintTask.AfterReportRunEventHandler(AfterReportRunCallBackForTest);
			PrintTaskSettings taskSettings = new PrintTaskSettings(testTask);
			testTask.Run(taskSettings);

			AssertEquals(2, AfterReportRunCallBackCount);
		}

		public void TestAfterReportRunCallBack()
		{
			AfterReportRunCallBackCount = 0;
			MockPrintTask testTask = new MockPrintTask();
			MockDocumentPack testPack1 = new MockDocumentPack();
			MockDocumentPack testPack2 = new MockDocumentPack();
			testTask.Add(testPack1);
			testTask.Add(testPack2);
			testTask.AfterReportRun += new PrintTask.AfterReportRunEventHandler(AfterReportRunCallBackForTest);
			testTask.Run(Env.Security.None);

			AssertEquals(2, AfterReportRunCallBackCount);
		}

		public void TestAddAndRunWithInstructions()
		{
			MockPrintTask testTask = new MockPrintTask();
			DeliveryInstructions instruction = new DeliveryInstructions();

			MockDocumentPack testPack1 = new MockDocumentPack();
			testPack1.ExpectedDeliveryInstructions = instruction;

			MockDocumentPack testPack2 = new MockDocumentPack();
			testPack2.ExpectedDeliveryInstructions = instruction;

			testTask.Add(testPack1);
			testTask.Add(testPack2);
			testTask.Run(instruction);

			AssertEquals("There were instructions, so the form should not have been shown", 0, testTask.ShowFormCount);
			AssertEquals("Pack1 should have been run once", 1, testPack1.RunCount);
			AssertEquals("Pack2 should have been run once", 1, testPack2.RunCount);
		}

		public void TestRunWithInstructionsHooksUpUserNotification()
		{
			MockPrintTask testTask = new MockPrintTask();
			MockDeliveryInstructions instruction = new MockDeliveryInstructions();

			MockDocumentPack testPack1 = new MockDocumentPack();
			testPack1.ExpectedDeliveryInstructions = instruction;

			MockDocumentPack testPack2 = new MockDocumentPack();
			testPack2.ExpectedDeliveryInstructions = instruction;

			AssertEquals("PreCondition: Instruction Notification count", 0, instruction.NotificationCount);
			AssertEquals("PreCondition: Instruction LastHandler", "", instruction.LastHandler);
			testTask.Add(testPack1);
			testTask.Add(testPack2);
			testTask.Run(instruction);

			AssertEquals("There were instructions, so the form should not have been shown", 0, testTask.ShowFormCount);
			Assert("The user notification should report progress", instruction.NotificationCount > 0);
			AssertEquals("User notification should have run at least one handler", "EndDocProcessing", instruction.LastHandler);
			AssertEquals("Pack1 should have been run once", 1, testPack1.RunCount);
			AssertEquals("Pack2 should have been run once", 1, testPack2.RunCount);
		}

		public void TestRunWithoutUserNotificationOnWeb()
		{
			try
			{
				Globals.IsWeb = true;
				MockPrintTask testTask = new MockPrintTask();
				MockDeliveryInstructions instruction = new MockDeliveryInstructions();

				MockDocumentPack testPack1 = new MockDocumentPack();
				testPack1.ExpectedDeliveryInstructions = instruction;

				MockDocumentPack testPack2 = new MockDocumentPack();
				testPack2.ExpectedDeliveryInstructions = instruction;

				AssertEquals("PreCondition: Instruction Notification count", 0, instruction.NotificationCount);
				AssertEquals("PreCondition: Instruction LastHandler", "", instruction.LastHandler);
				testTask.Add(testPack1);
				testTask.Add(testPack2);
				testTask.Run(instruction);

				AssertEquals("There were instructions, so the form should not have been shown", 0, testTask.ShowFormCount);
				Assert("The user notification should not be hooked up", instruction.NotificationCount == 0);
				Assert("No User Notification handler should have been run", instruction.LastHandler.IsEmpty);
				AssertEquals("Pack1 should have been run once", 1, testPack1.RunCount);
				AssertEquals("Pack2 should have been run once", 1, testPack2.RunCount);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestPreview()
		{
			MockPrintTask testTask = new MockPrintTask();
			DeliveryInstructions instruction = new DeliveryInstructions();
			instruction.Destination = DeliveryInstructionDestination.Print;

			testTask.Add(new MockDocumentPack());
			testTask.Preview(instruction);

			AssertEquals("Destination on Instructions still original value", DeliveryInstructionDestination.Print, instruction.Destination);
		}

		public void TestPreviewWithTaskSettings()
		{
			MockPrintTask testTask = new MockPrintTask();
			testTask.Add(new MockDocumentPack());
			PrintTaskSettings taskSettings = new PrintTaskSettings(testTask);
			taskSettings.Destination = DeliveryInstructionDestination.Print;
			testTask.Preview(taskSettings);

			AssertEquals("Destination on TaskSettings has original value", DeliveryInstructionDestination.Print, taskSettings.Destination);
			foreach (DeliveryInstructions instruction in taskSettings.DocPacksDeliveryInstructions)
			{
				AssertEquals("Destination on Instructions has original value", DeliveryInstructionDestination.Print, instruction.Destination);
			}
		}

		public void TestPreviewWithTaskSettingsDoesNotNullDocPacks()
		{
			MockPrintTask testTask = new MockPrintTask();
			testTask.Add(new MockDocumentPack());
			testTask.Add(new MockDocumentPack());

			PrintTaskSettings taskSettings = new PrintTaskSettings(testTask);
			taskSettings.Destination = DeliveryInstructionDestination.Preview;

			AssertEquals("Precondition: Two document packs to be printed", 2, testTask.Count);
			AssertNotNull("Precondition: DocPacks are not null", testTask[0]);
			AssertNotNull("Precondition: DocPacks are not null", testTask[1]);

			testTask.Run(taskSettings);

			AssertNotNull("DocPacks are not null as task is previewed", testTask[0]);
			AssertNotNull("DocPacks are not null as task is previewed", testTask[1]);
		}

		public void TestPreviewDoesNotNullDocPacks()
		{
			MockPrintTask testTask = new MockPrintTask();
			DeliveryInstructions instruction = new DeliveryInstructions();
			instruction.Destination = DeliveryInstructionDestination.Preview;

			testTask.Add(new MockDocumentPack());
			testTask.Add(new MockDocumentPack());

			AssertEquals("Precondition: Two document packs to be printed", 2, testTask.Count);
			AssertNotNull("Precondition: DocPacks are not null", testTask[0]);
			AssertNotNull("Precondition: DocPacks are not null", testTask[1]);

			testTask.Run(instruction);

			AssertNotNull("DocPacks are not null as task is previewed", testTask[0]);
			AssertNotNull("DocPacks are not null as task is previewed", testTask[1]);
		}

		public void TestRunWithTaskSettingsMarksJobsAsProcessed()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			using (var mockReport = new MockReport(TestReport))
			using (var mockReport2 = new MockReport(TestReport))
			{
				var testTask = new MockPrintTask();

				var testPack1 = new DocumentPack();
				testPack1.Add(mockReport);

				var testPack2 = new DocumentPack();
				testPack2.Add(mockReport2);

				testTask.Add(testPack1);
				testTask.Add(testPack2);

				var taskSettings = new PrintTaskSettings(testTask);
				taskSettings.Destination = DeliveryInstructionDestination.TakenFromContact;

				foreach (DeliveryInstructions instruction in taskSettings.DocPacksDeliveryInstructions)
				{
					instruction.Recipients.RemoveAndDeleteAll();

					var contact1 = instruction.Recipients.AddNew();
					contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					contact1.Email = "unit.test@cw1.com";
				}

				testTask.Run(taskSettings);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Two print jobs should be there", 2, printJobs.Length);
				AssertEquals("Print job should be Processed", ZBool.True, printJobs[0].DeliveryGroup.SB_IsProcessed);
				AssertEquals("Print job should be Processed", ZBool.True, printJobs[1].DeliveryGroup.SB_IsProcessed);
			}
		}

		public void TestRunMarksJobsAsProcessed()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			using (var mockReport = new MockReport(TestReport))
			using (var mockReport2 = new MockReport(TestReport))
			{
				mockReport.EmailSubject = "111";
				mockReport2.EmailSubject = "222";

				var testTask = new MockPrintTask();

				var instruction = new DeliveryInstructions();
				instruction.Destination = DeliveryInstructionDestination.TakenFromContact;
				instruction.Recipients.RemoveAndDeleteAll();

				var contact1 = instruction.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.Email = "unit.test@cw1.com";

				var testPack1 = new DocumentPack();
				testPack1.Add(mockReport);

				var testPack2 = new DocumentPack();
				testPack2.Add(mockReport2);

				testTask.Add(testPack1);
				testTask.Add(testPack2);
				testTask.Run(instruction);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Two print jobs should be there", 2, printJobs.Length);
				AssertEquals("Print job should be Processed", ZBool.True, printJobs[0].DeliveryGroup.SB_IsProcessed);
				AssertEquals("Print job should be Processed", ZBool.True, printJobs[1].DeliveryGroup.SB_IsProcessed);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunMarkGroupAsZipped_Zipped()
		{
			AssertRunMarkGroupAsZipped(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunMarkGroupAsZipped_NotZipped()
		{
			AssertRunMarkGroupAsZipped(false);
		}

		void AssertRunMarkGroupAsZipped(bool isZippedDocPack)
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();
			((DummyDocManagerTestBizODocumentSupporter)dummy.DocumentSupporter).BusinessContextOverride = BusinessContext.Shipment;
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");

			var command = helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;
			command.SU_IsZippedDocPack = isZippedDocPack;
			helper.CreateMenuTemplatePivot("Pub System Shipment Document", template, command);

			var childCommand = helper.CreateDocCommand("Child Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand.Parent = dummy;
			helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2"), childCommand);
			helper.CreateMenuMenuPivot(command, childCommand);

			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				printTask.DeliveryInstructionsDefaultPK = command.PK;
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				loader.LoadAll();

				AssertEquals(1, printTask.GetDocumentPacks().Count());

				var docPack = printTask.GetFirstDocumentPack();
				var includeCount = docPack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("DocPack should contain 2 documents", 2, docPack.Count);
				AssertEquals("DocPack should contain 2 documents include in print", 2, includeCount);
				AssertEquals("DocPack should contain 3 other documents which is not print by default", 3, docPack.OtherEDocsToAttach.Count);

				var instructions = new DeliveryInstructions(docPack);
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.Recipients.RemoveAndDeleteAll();
				var contact1 = instructions.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.Email = "unit.test1@edi.com";

				var contact2 = instructions.Recipients.AddNew();
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact2.Email = "unit.test2@edi.com";

				printTask.Run(instructions);
			}

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Two print jobs should be there", 2, printJobs.Length);
			AssertEquals("Both PrintJob should have the same delivery group", printJobs[0].SP_SB_DeliveryGroup, printJobs[1].SP_SB_DeliveryGroup);
			AssertEquals(string.Format("Group should {0}be flagged as SB_IsZippedDocPack", isZippedDocPack ? "" : "not "), isZippedDocPack, printJobs[0].DeliveryGroup.SB_IsZippedDocPack);
		}

		public void TestNumCopiesReadOnly()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			Assert("Multiple copies allowed by default", menuItem.AllowMultipleCopies);

			MockDestinationPrintTask testTask = new MockDestinationPrintTask(menuItem);
			OrgHeader org1 = Factory.New<OrgHeader>();
			TestableDocumentPack testPack1 = new TestableDocumentPack(menuItem);
			testPack1.DocumentSupporter = new MockDocSupportBizO(org1).DocumentSupporter;

			testTask.Add(testPack1);
			testTask.Run(Env.Security.None);

			AssertEquals("Number of copies can be specified", false, testTask.Instructions.PrinterDelivery.NumberOfCopiesInfo.ReadOnly);

			menuItem.AllowMultipleCopies = false;

			testTask = new MockDestinationPrintTask(menuItem);
			testTask.Add(testPack1);
			testTask.Run(Env.Security.None);

			AssertEquals("Number of copies can NOT be specified", true, testTask.Instructions.PrinterDelivery.NumberOfCopiesInfo.ReadOnly);
		}

		public void TestMultiDocPackEmailFaxPrint()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;

			PrintTask testTask = new PrintTask();

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			TestableDocumentPack testPack1 = new TestableDocumentPack(menuItem);
			testPack1.DocumentSupporter = new MockDocSupportBizO(org1).DocumentSupporter;

			TestableDocumentPack testPack2 = new TestableDocumentPack(menuItem);
			testPack2.DocumentSupporter = new MockDocSupportBizO(org2).DocumentSupporter;

			testTask.Add(testPack1);
			testTask.Add(testPack2);

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				testTask.Run(Env.Security.None);

				AssertEquals("Each doc pack should be run once", 1, testPack1.LastRunCount);
				AssertEquals("Each doc pack should be run once", 1, testPack2.LastRunCount);
			}
		}

		public void TestRunReturnsSelectedDeliveryMethod()
		{
			var task = new MockDestinationPrintTask(DeliveryInstructionDestination.TakenFromContact);
			AssertEquals("Run should return selected delivery method", DeliveryInstructionDestination.TakenFromContact, task.Run(Env.Security.None));

			task.DestinationToReturn = DeliveryInstructionDestination.Print;
			AssertEquals("Run should return UserCancelled when printer cannot be set up", DeliveryInstructionDestination.UserCancelled, task.Run(Env.Security.None));
			AssertEquals("prerequisite - no printer delivery details", null, task.Instructions.PrinterDelivery.PrintQueue);

			task.DestinationToReturn = DeliveryInstructionDestination.UserCancelled;
			AssertEquals("Run should return selected delivery method", DeliveryInstructionDestination.UserCancelled, task.Run(Env.Security.None));
		}

		[ExpectNoExceptions]
		public void TestImmediatePrint()
		{
			MockDestinationPrintTask mockImmediatePrintTask = new MockDestinationPrintTask();
		}

		public void TestShowRunTimeOptionsForm()
		{
			MockPrintTask testTask = new MockPrintTask();
			MockDocumentPack testPack = new MockDocumentPack();
			testTask.Add(testPack);
			AssertEquals("Should not check to show RunTimeOptionsForm", false, testTask.IsReportPrintSet);

			ReportCommand reportCommand = Factory.New<ReportCommand>();
			ReportPrintSet reportSet = new ReportPrintSet(reportCommand);
			AssertEquals("Should check to show RunTimeOptionsForm", true, reportSet.IsReportPrintSet);

			DocumentCommand documentCommand = Factory.New<DocumentCommand>();
			DocumentPrintSet documentSet = new DocumentPrintSet(documentCommand, new UserControlProviderList());
			AssertEquals("No need to check to show RunTimeOptionsForm", false, documentSet.IsReportPrintSet);
		}

		[ExpectNoExceptions]
		public void TestItemsOtherThanReportOrDocumentAreDelivered()
		{
			MockPrintTask testTask = new MockPrintTask();
			DocumentPack pack = new DocumentPack();
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			BusinessObjectFactory factoryOne = ((IDocumentFactory)documentFactory).GetFactory(1);
			IDeliverable storageDoc = (IDeliverable)factoryOne.New<IStorageDocs>();
			pack.Add(storageDoc);
			testTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None);
			AssertEquals("Delivery form should have been shown", 1, testTask.ShowFormCount);
		}

		public void TestGetReportFromPrintSet()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			command.SU_MenuName = "Test Report";

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (ReportPrintSet set = new ReportPrintSet(command))
			{
				AssertEquals("Should have report", true, set[0].GetFirstReport() != null);
			}
		}

		public void TestGetReportWhenReportIsNotFirstIDeliverableInDocPack()
		{
			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack();
			Report report = new Report(pack, null);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			BusinessObjectFactory factoryOne = ((IDocumentFactory)documentFactory).GetFactory(1);
			IDeliverable mockIDeliverable = (IDeliverable)factoryOne.New<IStorageDocs>();
			pack.Add(mockIDeliverable);
			pack.Add(report);

			AssertEquals(report, pack.GetFirstReport());
		}

		#region Save and Load Delivery Defaults

		public void TestSaveAndLoadPrintDeliveryDefaultsForTaskSettings()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			Factory.Save();

			var printerPK = printer.PK;
			var defaultPK = ZGuid.NewZGuid();

			var testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			var taskSettings = new PrintTaskSettings(testTask);
			taskSettings.PrinterDelivery.PrintQueuePK = printerPK;
			taskSettings.PrinterDelivery.NumberOfCopies = 13;
			testTask.SavePrinterDeliveryDefaults(taskSettings);

			var loadedTaskSettings = new PrintTaskSettings(new PrintTask());
			testTask.LoadPrinterDeliveryDefaults(loadedTaskSettings);
			AssertEquals("Printer Name", "Test Printer", loadedTaskSettings.PrinterDelivery.PrintQueue.SQ_DisplayName);
			AssertEquals("PrinterPK", printerPK, loadedTaskSettings.PrinterDelivery.PrintQueuePK);
			AssertEquals("Number of Copies", 13, loadedTaskSettings.PrinterDelivery.NumberOfCopies);

			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(new BusinessObjectFactory(), GlbStaff.CurrentUser, false);
			AssertEquals("Default Printer should NOT be registered child-editable.", false, GlbStaff.CurrentUser.IsRegisteredEditableChildObject(defaultPrinter));

			GC.Collect();
			AssertEquals("None of the temp factories are leaked after garbage collection", 0, PersistentFactoryCacheManager.Instance.persistentFactoryWeakReferences.Count(reference => reference.Target is BusinessObjectFactory factory && (factory.NameForDebugging.StartsWith("PrintTask.SavePrinterDeliveryDefaults") || factory.NameForDebugging.StartsWith("PrintTask.LoadPrinterDeliveryDefaults"))));
		}

		public void TestSaveAndLoadPrintDeliveryDefaults()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			Factory.Save();

			var printerPK = printer.PK;
			var defaultPK = ZGuid.NewZGuid();

			var testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			var instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printerPK;
			instructions.PrinterDelivery.NumberOfCopies = 13;
			testTask.SavePrinterDeliveryDefaults(instructions);

			var loadedInstructions = new DeliveryInstructions();
			testTask.LoadPrinterDeliveryDefaults(loadedInstructions);
			AssertEquals("Printer Name", "Test Printer", loadedInstructions.PrinterDelivery.PrintQueue.SQ_DisplayName);
			AssertEquals("PrinterPK", printerPK, loadedInstructions.PrinterDelivery.PrintQueuePK);
			AssertEquals("Number of Copies", 13, loadedInstructions.PrinterDelivery.NumberOfCopies);

			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(new BusinessObjectFactory(), GlbStaff.CurrentUser, false);
			AssertEquals("Default Printer should NOT be registered child-editable.", false, GlbStaff.CurrentUser.IsRegisteredEditableChildObject(defaultPrinter));

			GC.Collect();
			AssertEquals("None of the temp factories are leaked after garbage collection", 0, PersistentFactoryCacheManager.Instance.persistentFactoryWeakReferences.Count(reference => reference.Target is BusinessObjectFactory factory && (factory.NameForDebugging.StartsWith("PrintTask.SavePrinterDeliveryDefaults") || factory.NameForDebugging.StartsWith("PrintTask.LoadPrinterDeliveryDefaults"))));
		}

		public void TestSaveAndLoadPrintDeliveryDefaultsWhenInactive()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			printer.SQ_AllowPrinting = false;
			Factory.Save();

			ZGuid defaultPK = ZGuid.NewZGuid();

			PrintTask testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			instructions.PrinterDelivery.NumberOfCopies = 13;
			testTask.SavePrinterDeliveryDefaults(instructions);

			DeliveryInstructions instructionsToPopulate = new DeliveryInstructions();
			testTask.LoadPrinterDeliveryDefaults(instructionsToPopulate);
			AssertNull("Printer should be empty because the queue is not active", instructionsToPopulate.PrinterDelivery.PrintQueue);
			Assert("The printer pk should be blank", instructionsToPopulate.PrinterDelivery.PrintQueuePK.IsEmpty);
			AssertEquals("Number of copies should still work", 13, instructionsToPopulate.PrinterDelivery.NumberOfCopies);
		}

		public void TestSaveAndLoadPrintDeliveryDefaultsWhenDeleted()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			printer.SQ_QueueDeleted = ZDateTime.Today.AddDays(-5);
			Factory.Save();

			ZGuid defaultPK = ZGuid.NewZGuid();

			PrintTask testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			instructions.PrinterDelivery.NumberOfCopies = 13;
			testTask.SavePrinterDeliveryDefaults(instructions);

			DeliveryInstructions instructionsToPopulate = new DeliveryInstructions();
			testTask.LoadPrinterDeliveryDefaults(instructionsToPopulate);
			AssertNull("Printer should be empty because the queue is deleted", instructionsToPopulate.PrinterDelivery.PrintQueue);
			Assert("The printer pk should be blank", instructionsToPopulate.PrinterDelivery.PrintQueuePK.IsEmpty);
			AssertEquals("Number of copies should still work", 13, instructionsToPopulate.PrinterDelivery.NumberOfCopies);
		}

		public void TestSaveAndLoadPrintDeliveryDefaultsWhenSecurityDenied()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			Env.Security.GetPrintQueueCheckPoint(printer.PK.ToGuid(), printer.SQ_DisplayName).IsAllowed = false;
			Factory.Save();

			ZGuid defaultPK = ZGuid.NewZGuid();

			PrintTask testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			instructions.PrinterDelivery.NumberOfCopies = 13;
			testTask.SavePrinterDeliveryDefaults(instructions);

			DeliveryInstructions instructionsToPopulate = new DeliveryInstructions();
			testTask.LoadPrinterDeliveryDefaults(instructionsToPopulate);
			AssertNull("Printer should be empty because the current user does not have security to print to this printer", instructionsToPopulate.PrinterDelivery.PrintQueue);
			Assert("The printer pk should be blank", instructionsToPopulate.PrinterDelivery.PrintQueuePK.IsEmpty);
			AssertEquals("Number of copies should still work", 13, instructionsToPopulate.PrinterDelivery.NumberOfCopies);
		}

		public void TestSaveAndLoadPrintDeliveryDefaultsWhenPrinterPKInvalidWillThrowAnException()
		{
			ZGuid defaultPK = ZGuid.NewZGuid();

			PrintTask testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = defaultPK;

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = ZGuid.NewZGuid();
			instructions.PrinterDelivery.NumberOfCopies = 13;

			AssertExceptionThrown(typeof(ZSaveException), () => testTask.SavePrinterDeliveryDefaults(instructions));

			DeliveryInstructions instructionsToPopulate = new DeliveryInstructions();
			testTask.LoadPrinterDeliveryDefaults(instructionsToPopulate);
			AssertNull("Printer should be empty because the saved printer PK was invalid", instructionsToPopulate.PrinterDelivery.PrintQueue);
			Assert("The printer pk should be blank", instructionsToPopulate.PrinterDelivery.PrintQueuePK.IsEmpty);
		}

		#endregion

		public void TestDeliveryOptions()
		{
			MockPrintTask task = new MockPrintTask();
			task.Run(Env.Security.None);
			AssertEquals("Should not do hard copy only", AllowedDeliveryOptions.All, task.LastShownInstructions.DeliveryOptions);

			task.Run(AllowedDeliveryOptions.All, Env.Security.None);
			AssertEquals("Should not do hard copy only", AllowedDeliveryOptions.All, task.LastShownInstructions.DeliveryOptions);

			task.Run(AllowedDeliveryOptions.AllExceptPreview, Env.Security.None);
			AssertEquals("Should not allow preview", AllowedDeliveryOptions.AllExceptPreview, task.LastShownInstructions.DeliveryOptions);

			task.Run(AllowedDeliveryOptions.HardCopyOnly, Env.Security.None);
			AssertEquals("Should do hard copy only", AllowedDeliveryOptions.HardCopyOnly, task.LastShownInstructions.DeliveryOptions);
		}

		public void TestPermissionForAutoDelivery()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_PreventAutoDelivery = false;

			var task = new MockPrintTaskAutoDeliveryPermission(menuItem);
			var pack1 = new TestableDocumentPack();
			var pack2 = new TestableDocumentPack();
			var pack3 = new TestableDocumentPack();
			task.Add(pack1);
			task.Add(pack2);
			task.Add(pack3);
			AssertEquals("PermissionForAutoDelivery - neither DocPack has IDocumentAutoDelivery DeliveryFilter", false, task.AllowedToPerformAutoDelivery);

			var org = Factory.New<OrgHeader>();
			var bizO = new MockDocSupportBizO(org);
			pack1.DocumentSupporter = bizO.DocumentSupporter;
			AssertEquals("PermissionForAutoDelivery - only one DocPack has IDocumentAutoDelivery DeliveryFilter", false, task.AllowedToPerformAutoDelivery);

			pack3.DocumentSupporter = bizO.DocumentSupporter;
			AssertEquals("PermissionForAutoDelivery - only two DocPacks have IDocumentAutoDelivery DeliveryFilter", false, task.AllowedToPerformAutoDelivery);

			pack2.DocumentSupporter = bizO.DocumentSupporter;
			AssertEquals("PermissionForAutoDelivery - all DocPacks have IDocumentAutoDelivery DeliveryFilter", true, task.AllowedToPerformAutoDelivery);

			menuItem.SU_PreventAutoDelivery = true;
			AssertEquals("PreventAutoDelivery from ParentMenuCommand", false, task.AllowedToPerformAutoDelivery);
		}

		public void TestPermissionForAutoDelivery_WithStreaming()
		{
			int nbOfEnumerationHits = 0;

			var command = Factory.New<DocumentCommand>();
			command.SU_PreventAutoDelivery = false;

			var packs = new List<DocumentPack>();
			var pack1 = new TestableDocumentPack();
			var pack2 = new TestableDocumentPack();
			packs.Add(pack1);
			packs.Add(pack2);

			var task = new DocumentPrintSetWithStreamingTest.DocumentPrintSetWithStreamingForTest(command, 2,
				GetDocumentPacks_TestPermissionForAutoDelivery_WithStreaming(packs, nbOfEnumerationHits));

			AssertEquals("PermissionForAutoDelivery - NoContactType", false, task.AllowedToPerformAutoDelivery);

			command.SU_ContactType = ContactType.All.Code;
			AssertEquals("PermissionForAutoDelivery", true, task.AllowedToPerformAutoDelivery);

			command.SU_PreventAutoDelivery = true;
			AssertEquals("PreventAutoDelivery from ParentMenuCommand", false, task.AllowedToPerformAutoDelivery);

			AssertEquals("Should not have enumerated DocumentPacks", 0, nbOfEnumerationHits);
		}
		IEnumerable<DocumentPack> GetDocumentPacks_TestPermissionForAutoDelivery_WithStreaming(List<DocumentPack> packs, int nbOfEnumerationHits)
		{
			foreach (var pack in packs)
			{
				++nbOfEnumerationHits;
				yield return pack;
			}
		}

		public void TestGenerateSubjectLineForPrintTask()
		{
			var printTask = new MockPrintTask();
			AssertEquals("Should be the EmptyMapping on the base class, doesn't apply except for DocumentCommands yet", PrintTask.ReportSubjectLineMapping.EmptyMapping, printTask.GenerateSubjectLineForPrintTaskForTesting(null));
		}

		public void TestNeedPrinterForAutoDelivery()
		{
			var printTask = new MockPrintTask();
			var docPack = new TestableDocumentPackWithContacts();
			var organisation = Factory.New<OrgHeader>();
			docPack.OrgHeaderContact = new OrgHeaderContact(organisation, null);
			printTask.Add(docPack);

			var deliveryWithContacts = (MockAutoDocumentDeliveryWithContacts)docPack.AutoDocumentDelivery;
			deliveryWithContacts.Contacts.RemoveAndDeleteAll();
			var contact = deliveryWithContacts.Contacts.AddNew();

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("When notify type is Fax", false, printTask.NeedPrinterForAutoDelivery_ExposedForTest);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("When notify type is Email", false, printTask.NeedPrinterForAutoDelivery_ExposedForTest);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("When notify type is Print", true, printTask.NeedPrinterForAutoDelivery_ExposedForTest);
		}

		public void TestNeedPrinterForAutoDelivery_WithStreaming()
		{
			int nbOfEnumerationHits = 0;

			var docPack = new TestableDocumentPackWithContacts();
			var organisation = Factory.New<OrgHeader>();
			docPack.OrgHeaderContact = new OrgHeaderContact(organisation, null);

			var printTask = new DocumentPrintSetWithStreamingTest.DocumentPrintSetWithStreamingForTest(Factory.New<DocumentCommand>(), 1,
				GetDocumentPacks_TestNeedPrinterForAutoDelivery_WithStreaming(docPack, nbOfEnumerationHits));

			var deliveryWithContacts = (MockAutoDocumentDeliveryWithContacts)docPack.AutoDocumentDelivery;
			deliveryWithContacts.Contacts.RemoveAndDeleteAll();
			var contact = deliveryWithContacts.Contacts.AddNew();

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("When notify type is Fax", true, printTask.NeedPrinterForAutoDelivery_ExposedForTest);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("When notify type is Email", true, printTask.NeedPrinterForAutoDelivery_ExposedForTest);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("When notify type is Print", true, printTask.NeedPrinterForAutoDelivery_ExposedForTest);

			AssertEquals("Should not have enumerated DocumentPacks", 0, nbOfEnumerationHits);
		}
		IEnumerable<DocumentPack> GetDocumentPacks_TestNeedPrinterForAutoDelivery_WithStreaming(DocumentPack pack, int nbOfEnumerationHits)
		{
			++nbOfEnumerationHits;
			yield return pack;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreviewDoesNotSaveDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.Preview, 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCancelDoesNotSaveDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.UserCancelled, 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoneDoesNotSaveDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.None, 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDiskDoesNotSaveDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.Disk, 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrintSavesDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.Print, 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerSavesDeliveryGroup()
		{
			TestDeliveryGroupSaving(DeliveryInstructionDestination.DocManager, 1);
		}

		void TestDeliveryGroupSaving(DeliveryInstructionDestination destination, int expectedDeliveryGroupCount)
		{
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), Factory);
			AssertNotNull("precondition: The Organisation Notes menu item was not found.", reportCommand);

			using (var task = new PrintTask(reportCommand))
			{
				var docPack = new DocumentPack(reportCommand);
				var instructions = new DeliveryInstructions { Destination = destination };
				if (destination == DeliveryInstructionDestination.DocManager || destination == DeliveryInstructionDestination.Print)
				{
					var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
					instructions.PrinterDelivery.PrintQueuePK = stmPrintQueue1.PK;
					Factory.Save();

					var contact = instructions.Recipients.AddNew();
					contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					instructions.DeliverablesToBePrinted.Add(docPack.FirstOrDefault());
					task.Add(docPack);
				}
				task.Run(instructions);
				AssertEquals("Count of Delivery groups", expectedDeliveryGroupCount, Factory.GetDatabaseCount(typeof(StmDeliveryGroup)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsDraft_EmailSubjectMacro()
		{
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), Factory);
			using (var task = new PrintTask(reportCommand))
			{
				var docPack = new DocumentPack(reportCommand);
				var instructions = new DeliveryInstructions { Destination = DeliveryInstructionDestination.TakenFromContact };
				instructions.IsDraft = true;
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "a@b.com";
				contact.EmailSubjectMacro = "<If(\"<IsDraft>\"==\"Y\",\"DRAFT Arrival Notice\",\"<ReportName>\")>";
				instructions.DeliverablesToBePrinted.Add(docPack.FirstOrDefault());
				task.Add(docPack);
				task.Run(instructions);
				AssertEquals("DRAFT Arrival Notice", instructions.DeliveryGroups[0].SB_EmailSubjectLine);
			}
		}

		public void TestIsPreviewAllowed()
		{
			var maxCount = PrintTask.MaxPreviewCount;

			using (var printTask = new MockPrintTask())
			{
				for (int i = 0; i < maxCount; i++)
				{
					printTask.Add(new DocumentPack());
				}
				Assert(printTask.Count <= maxCount);
				Assert("PrintTask contains no more than " + maxCount + " documentPacks. It can be previewed", printTask.IsPreviewAllowed_ExposedForTest);

				printTask.Add(new DocumentPack());
				Assert("PrintTask contains more than " + maxCount + " documentPacks. It cannot be previewed", !printTask.IsPreviewAllowed_ExposedForTest);
			}

			using (var printTask = new DocumentPrintSetWithStreamingTest.DocumentPrintSetWithStreamingForTest(Factory.New<DocumentCommand>(), maxCount, GetDocumentPacks_TestIsPreviewAllowed(maxCount)))
			{
				Assert(printTask.Count <= maxCount);
				Assert("PrintTask with streaming. Preview is not allowed.", !printTask.IsPreviewAllowed_ExposedForTest);
			}

			using (var printTask = new DocumentPrintSetWithStreamingTest.DocumentPrintSetWithStreamingForTest(Factory.New<DocumentCommand>(), maxCount + 1, GetDocumentPacks_TestIsPreviewAllowed(maxCount + 1)))
			{
				Assert(printTask.Count > maxCount);
				Assert("PrintTask with streaming. Preview is not allowed.", !printTask.IsPreviewAllowed_ExposedForTest);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPurgeStmPrintJobsAndStmDeliveryGroup_WhenMaxConcurrentReportConnectionsExceededIsCaught()
		{
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report";
			reportCommand.SU_IsSystemDefined = true;

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Report Template";
			template.SO_Template = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			template.SO_IsSystemDefined = true;

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = reportCommand.SU_MenuName;
			menuTemplatePivot.SI_SU = reportCommand.PK;
			menuTemplatePivot.SI_SO = template.PK;
			menuTemplatePivot.SI_IsSystemDefined = true;

			var instructions = GetDeliveryInstrunctions();
			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.Email = "test@test.com";

			using (var printTask = new PrintTask(reportCommand))
			{
				var documentPack = new MaxConcurrentReportConnectionsExceededDocumentPack(reportCommand);
				printTask.Add(documentPack);

				var deliveryGroupPK = instructions.DeliveryGroups[0].PK;
				var factory = new BusinessObjectFactory();

				AssertExceptionThrown<MaxConcurrentReportConnectionsExceeded>(() => printTask.Run(instructions));
				AssertNull(factory.Load<StmDeliveryGroup>(deliveryGroupPK));
				AssertEquals(0, factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroupPK)).Length);
			}
		}

		IEnumerable<DocumentPack> GetDocumentPacks_TestIsPreviewAllowed(int nbOfDocumentPacks)
		{
			for (int i = 0; i < nbOfDocumentPacks; i++)
			{
				yield return new DocumentPack();
			}
		}

		public void TestBackgroundDelivery()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
			@"{A}-[#Config]
{A}-[#EndOfReport]");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
	@"{A}-[#Config]
{A}-[#EndOfReport]");
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = documentSupportable;
			command.ControllerId = new ClientControllerID("DummyBODoc");

			var pivot1 = command.Documents.AddNew();
			pivot1.SI_SU = command.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = command.Documents.AddNew();
			pivot2.SI_SU = command.PK;
			pivot2.SI_SO = template2.PK;

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";

			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";

			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				AssertEquals("should be 2 documents to be delivered.", 2, deliveryInstructions.DocumentsToBeDelivered.Count);
				var document1 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK);
				document1.PrinterDetails.PrintQueuePK = stmPrintQueue1.PK;
				document1.PrinterDetails.NumberOfCopies = 2;
				var document2 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK);
				document2.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
				document2.PrinterDetails.NumberOfCopies = 3;

				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);
			}

			var query1 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue1.PK);
			query1.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)2);

			var stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query1);

			AssertEquals("there should be a default printer for pivot1 and \"printer 1\".", 1, stmDefaultPrinters.Length);

			var query2 = new ZQuery(StmDefaultPrinterSchema.SDP_SU_Document, command.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SI, pivot2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_SQ_Printer, stmPrintQueue2.PK);
			query2.AddToFilter(StmDefaultPrinterSchema.SDP_NumberOfCopies, (ZByte)3);

			stmDefaultPrinters = Factory.Load<StmDefaultPrinter>(query2);
			AssertEquals("there should be a default printer for pivot2 and \"printer 2\".", 1, stmDefaultPrinters.Length);

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();

				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAll();
				deliveryInstructions.BackgroundDelivery = true;
				var contact = deliveryInstructions.Recipients.AddNew();
				contact.DeliveryMethod = "PRN";
				deliveryInstructions.PrinterDelivery.PrintQueuePK = stmPrintQueue1.PK;
				printTask.LoadPrinterDeliveryDefaults(deliveryInstructions);

				AssertEquals("", 2, deliveryInstructions.DocumentsToBeDelivered.Count);

				var document1 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 1\"", "Printer 1", document1.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 2.", (ZShort)2, document1.PrinterDetails.NumberOfCopies);

				var document2 = deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK) as IDeliverable;
				AssertEquals("the override printer should be default with \"Printer 2\"", "Printer 2", document2.PrinterDetails.PrintQueue.SQ_DisplayName);
				AssertEquals("the copies should be 3.", (ZShort)3, document2.PrinterDetails.NumberOfCopies);

				printTask.CreateStmDocumentDelivery(deliveryInstructions);

				var stmPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("when ScheduleReportForLater is true, no stmPrintJobs will be create", 0, stmPrintJobs.Length);

				var documentDeliveries = Factory.Load<StmDocumentDelivery>(new ZQuery());
				AssertEquals("when ScheduleReportForLater is true, a StmDocumentDelivery will be create", 1, documentDeliveries.Length);

				var stmDocDelivery = documentDeliveries[0];
				AssertEquals(false, stmDocDelivery.SDL_IsProcessed);
				AssertEquals((ZByte)0, stmDocDelivery.SDL_RetryAttempts);
				AssertEquals(command.PK, stmDocDelivery.SDL_SU);
				AssertEquals(Env.CurrentBranchPK, stmDocDelivery.SDL_GB);
				AssertEquals(Env.CurrentUserPK, stmDocDelivery.SDL_GS);
				AssertEquals(Env.CurrentDepartmentPK, stmDocDelivery.SDL_GE);
				AssertEquals("DummyBODoc", stmDocDelivery.SDL_ParentControllerIdOrTableCode);
				AssertEquals(documentSupportable.PK, stmDocDelivery.SDL_ParentId);
			}
		}

		public void TestDocumentUsageEDIMessageCollectedAfterPrintTaskRun()
		{
			var helper = new UsageCollectorTestHelper(Factory);
			Assert("No DocumentGenerated in the beginning.", helper.AssertUsageSummmaryMessagesCount(UsageFeatures.Codes.DocumentGenerated, 0));

			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			using (var mockReport = new MockReport(TestReport))
			{
				var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
				menuItem.SU_MenuName = "TestMenuTitle";

				var testTask = new MockPrintTask(menuItem);

				var instruction = new DeliveryInstructions();
				instruction.Destination = DeliveryInstructionDestination.TakenFromContact;
				instruction.Recipients.RemoveAndDeleteAll();

				var contact1 = instruction.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.Email = "test@test.com";
				contact1.AttachmentType = "PDF";

				var contact2 = instruction.Recipients.AddNew();
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				contact2.Email = "";
				contact2.AttachmentType = "";

				var testPack = new DocumentPack(menuItem);
				testPack.Add(mockReport);
				testTask.Add(testPack);
				testTask.Run(instruction);

				var properties1 = new List<(string name, object value)>();
				var properties2 = new List<(string name, object value)>();
				properties1.Add(("MenuTitle", "TestMenuTitle"));
				properties1.Add(("DeliveryMethod", "EML"));
				properties1.Add(("AttachmentType", "PDF"));
				properties1.Add(("IsPreview", false));
				properties1.Add(("IsTriggeredViaWorkflow", false));
				properties1.Add(("IsUserSignatureUsed", false));

				properties2.Add(("MenuTitle", "TestMenuTitle"));
				properties2.Add(("DeliveryMethod", "PRN"));
				properties2.Add(("AttachmentType", ""));
				properties2.Add(("IsPreview", false));
				properties2.Add(("IsTriggeredViaWorkflow", false));
				properties2.Add(("IsUserSignatureUsed", false));

				Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties1));
				Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties2));
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting testReport;
		ExcelTemplateForUnitTesting TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testReport = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testReport;
			}
		}

		sealed class TestableDocumentPackWithContacts : DocumentPack
		{
			public TestableDocumentPackWithContacts()
				: base() { }

			public TestableDocumentPackWithContacts(StmMenuItem menuItem)
				: base(menuItem) { }

			DocAutoDelivery fAutoDocumentDelivery;
			public override DocAutoDelivery AutoDocumentDelivery
			{
				get
				{
					if (fAutoDocumentDelivery == null)
					{
						fAutoDocumentDelivery = new MockAutoDocumentDeliveryWithContacts();
					}
					return fAutoDocumentDelivery;
				}
			}

			public void SetValidContacts()
			{
				DocDeliveryContactCollection contactCollection = fAutoDocumentDelivery.GetDeliveryContacts(StmMenuCommand, DocumentSupporter);
				foreach (DocDeliveryContact contact in contactCollection)
				{
					if (contact.Name != "Craig")
					{
						contact.Fax = "+61 (29) 888-8888";
					}
				}
			}
		}

		sealed class MockAutoDocumentDeliveryWithContacts : DocAutoDelivery
		{
			public DocDeliveryContact Tim;
			public DocDeliveryContact Dawn;
			public DocDeliveryContact David;
			public DocDeliveryContact Gareth;
			public DocDeliveryContact Craig;

			public override DocDeliveryContactCollection GetDeliveryContacts(IStmMenuItem menuItem, DocumentSupporter deliveryFilter)
			{
				return Contacts;
			}

			public DocDeliveryContactCollection Contacts
			{
				get
				{
					if (fContacts == null)
					{
						fContacts = new DocDeliveryContactCollection(Factory);
						Tim = Contacts.AddNew();
						Tim.Name = "Tim";
						Tim.CompanyName = "Timbo Pty Ltd.";
						Tim.Fax = "9888gg";
						Tim.DeliveryMethod = "FAX";

						Dawn = Contacts.AddNew();
						Dawn.Name = "Dawn";
						Dawn.Fax = "9999";

						Dawn.DeliveryMethod = "FAX";

						Gareth = Contacts.AddNew();
						Gareth.Name = "Gareth";
						Gareth.CompanyName = "Gazza Inc.";
						Gareth.Fax = "(02)}}}9876 5434";
						Gareth.DeliveryMethod = "FAX";

						David = Contacts.AddNew();
						David.Name = "David Brent";
						David.Fax = "fsdfsdf";
						David.DeliveryMethod = "PRN";

						Craig = Contacts.AddNew();
						Craig.Name = "Craig";
						Craig.CompanyName = "Craig's Company";
						Craig.DeliveryMethod = "FAX";
						Craig.Fax = "";
					}
					return fContacts;
				}
			}
			DocDeliveryContactCollection fContacts;
		}

		sealed class MockDestinationPrintTask : PrintTask
		{
			public MockDestinationPrintTask(StmMenuItem menuItem)
				: base(menuItem)
			{
			}

			public MockDestinationPrintTask()
				: this(DeliveryInstructionDestination.Print)
			{
			}

			public MockDestinationPrintTask(DeliveryInstructionDestination destinationToReturn)
			{
				this.DestinationToReturn = destinationToReturn;
			}

			public DeliveryInstructionDestination DestinationToReturn;

			public DeliveryInstructions Instructions;

			protected override void ShowDeliveryInstructionsForm(DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				Instructions = deliveryInstructions;
				deliveryInstructions.Destination = DestinationToReturn;
				SetNumberOfCopies(deliveryInstructions);
			}
		}

		public sealed class MockPrintTask : PrintTask
		{
			public int ShowFormCount;
			public int ShowDeliveryFormCount;
			public int ShowHardCopyOnlyFormCount;
			public DeliveryInstructions LastShownInstructions;

			public MockPrintTask() { }
			public MockPrintTask(IStmMenuItem item) : base(item) { }

			protected override void ShowHardCopyOnlyForm(PrintTaskSettings taskSettings)
			{
				ShowHardCopyOnlyFormCount++;
			}

			protected override void ShowDeliveryInstructionsForm(DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				ShowFormCount++;
				LastShownInstructions = deliveryInstructions;
			}

			protected override void ShowDeliveryForm(PrintTaskSettings taskSettings)
			{
				ShowDeliveryFormCount++;
			}

			public ReportSubjectLineMapping GenerateSubjectLineForPrintTaskForTesting(DocumentPack docPack)
			{
				return base.GenerateSubjectLineMappingForPrintTask(docPack);
			}

			public bool NeedPrinterForAutoDelivery_ExposedForTest
			{
				get { return base.NeedPrinterForAutoDelivery; }
			}

			public void SetDraftOptions_ExposedForTest(DeliveryInstructions instructions)
			{
				base.SetDraftOptions(instructions);
			}

			public bool IsPreviewAllowed_ExposedForTest
			{
				get { return base.IsPreviewAllowed; }
			}
		}

		sealed class MaxConcurrentReportConnectionsExceededDocumentPack : DocumentPack
		{
			public MaxConcurrentReportConnectionsExceededDocumentPack(ReportCommand reportCommand) : base(reportCommand)
			{
			}

			// Specific for TestPurgeStmPrintJobsAndStmDeliveryGroup_WhenMaxConcurrentReportConnectionsExceededIsCaught
			protected override void RunForContact(DocDeliveryContact contact, DeliveryInstructions instructions, INotifications notifications = null)
			{
				if (contact.Email == "test@test.com")
				{
					throw new MaxConcurrentReportConnectionsExceeded("MaxConcurrentReportConnectionsExceeded","ReportInfo");
				}

				base.RunForContact(contact, instructions);
			}
		}

		sealed class MockDocumentPack : DocumentPack
		{
			public DeliveryInstructions ExpectedDeliveryInstructions;
			public int RunCount;

			protected internal override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				base.Run(deliveryInstructions);
				RunCount++;
				CallOnAfterReportRun(null);
			}
		}

		sealed class MockPrintTaskAutoDeliveryPermission : PrintTask
		{
			public MockPrintTaskAutoDeliveryPermission(StmMenuItem menuItem)
				: base(menuItem)
			{
			}

			public bool AllowedToPerformAutoDelivery
			{
				get { return base.PermissionForAutoDelivery; }
			}
		}

		sealed class TestableDocumentPack : DocumentPack
		{
			public TestableDocumentPack()
				: base()
			{
			}

			public TestableDocumentPack(StmMenuItem menuItem)
				: base(menuItem)
			{
			}

			public override DocAutoDelivery AutoDocumentDelivery
			{
				get { return new MockAutoDocumentDelivery(); }
			}

			internal protected override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				LastRunCount++;
				base.Run(deliveryInstructions);
			}

			public int LastRunCount;
		}

		sealed class MockAutoDocumentDelivery : DocAutoDelivery
		{
			public override DocDeliveryContactCollection GetDeliveryContacts(IStmMenuItem menuItem, DocumentSupporter deliveryFilter)
			{
				DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
				DocDeliveryContact lorenzo = contacts.AddNew();
				lorenzo.Name = "Lorenzo";
				lorenzo.CompanyName = "Lorenzo Inc";
				lorenzo.Fax = "121212";
				lorenzo.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

				DocDeliveryContact enrico = contacts.AddNew();
				enrico.Name = "Enrico";
				enrico.CompanyName = "Enrico Co.";
				enrico.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

				DocDeliveryContact stefano = contacts.AddNew();
				stefano.Name = "Stefano";
				stefano.Fax = "121212";
				stefano.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

				return contacts;
			}
		}
	}
}
