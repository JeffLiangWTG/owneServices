using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DeliveryInstructions))]
	sealed class DeliveryInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEdocsToBeDEliveredInOrder()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			((Forwarding.IForwardingConsol)consol).JK_UniqueConsignRef = "CTEST00001";
			var consolCommand = CreateTemplateForTest(helper, "ConsolTemplate", consol, "ConsolCommand", nameof(Core.Constants.DataContext.Consol));
			consolCommand.SU_ContactType = ContactType.NoContactType.Code;
			CreateCommandDocumentsAndEDocs(helper, consol, consolCommand);

			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, consolCommand, null);
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				using (deliveryInstructions.ResetEDocsToBeDeliveredIfNeeded())
				{
					var deliverables = deliveryInstructions.EDocsToBeDelivered;
					AssertEquals("Total Count ", 12, deliverables.Count);

					deliverables[0].Index = 2;
					deliverables[1].Index = 5;
					deliverables[2].Index = 8;
					deliverables[3].Index = 1;
					deliverables[4].Index = 11;
					deliverables[5].Index = 6;
					deliverables[6].Index = 12;
					deliverables[7].Index = 3;
					deliverables[8].Index = 4;
					deliverables[9].Index = 7;
					deliverables[10].Index = 10;
					deliverables[11].Index = 9;

					deliveryInstructions.EDocsToBeDelivered.ResetEDocs(); // this will sort the view

					AssertEquals(deliverables[0].Index.ToZInt(), 1);
					AssertEquals(deliverables[1].Index.ToZInt(), 2);
					AssertEquals(deliverables[2].Index.ToZInt(), 3);
					AssertEquals(deliverables[3].Index.ToZInt(), 4);
					AssertEquals(deliverables[4].Index.ToZInt(), 5);
					AssertEquals(deliverables[5].Index.ToZInt(), 6);
					AssertEquals(deliverables[6].Index.ToZInt(), 7);
					AssertEquals(deliverables[7].Index.ToZInt(), 8);
					AssertEquals(deliverables[8].Index.ToZInt(), 9);
					AssertEquals(deliverables[9].Index.ToZInt(), 10);
					AssertEquals(deliverables[10].Index.ToZInt(), 11);
					AssertEquals(deliverables[11].Index.ToZInt(), 12);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestResetEDocsToBeDeliveredIfAllEDocsAreNotIncluded()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			((Forwarding.IForwardingShipment)shipment).JS_UniqueConsignRef = "STEST00001";

			var eDocTypeA = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			var shipmentCommand = CreateTemplateForTest(helper, "ShipmentTemplate", shipment, "ShipmentCommand", nameof(Core.Constants.DataContext.Shipment));
			shipmentCommand.SU_ContactType = ContactType.NoContactType.Code;
			shipmentCommand.AddEDoc(eDocTypeA).SX_Index = 0;

			Factory.Save();

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, shipment);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = $"Test Shipment eDoc AAA";
			}

			documentFactory.Save();

			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, shipmentCommand, null);
				loader.LoadAll();
				var instructions = new DeliveryInstructions(printTask[0]);
				using (instructions.ResetEDocsToBeDeliveredIfNeeded())
				{
					AssertEquals("EDocsToBeDelivered should have 1 eDoc file", 1, instructions.EDocsToBeDelivered.Count);

					AssertEquals("IncludedInPrint should be true", true, instructions.EDocsToBeDelivered[0].IncludedInPrint);
					AssertEquals("ShouldPrintByDefault should be true", true, instructions.EDocsToBeDelivered[0].ShouldPrintByDefault);

					instructions.EDocsToBeDelivered[0].IncludedInPrint = false;
					AssertEquals("IncludedInPrint should be false", false, instructions.EDocsToBeDelivered[0].IncludedInPrint);
					AssertEquals("ShouldPrintByDefault should be true", true, instructions.EDocsToBeDelivered[0].ShouldPrintByDefault);
				}

				AssertEquals("IncludedInPrint should be true", true, instructions.EDocsToBeDelivered[0].IncludedInPrint);
				AssertEquals("ShouldPrintByDefault should be false", false, instructions.EDocsToBeDelivered[0].ShouldPrintByDefault);
			}
		}

		#region TestAddOtherEDocsToAttach

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddOtherEDocsToAttach()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			((Forwarding.IForwardingConsol)consol).JK_UniqueConsignRef = "CTEST00001";
			var consolCommand = CreateTemplateForTest(helper, "ConsolTemplate", consol, "ConsolCommand", nameof(Core.Constants.DataContext.Consol));
			consolCommand.SU_ContactType = ContactType.NoContactType.Code;
			CreateCommandDocumentsAndEDocs(helper, consol, consolCommand);

			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, consolCommand, null);
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				IDeliverable othereDoc = null;
				using (deliveryInstructions.ResetEDocsToBeDeliveredIfNeeded())
				{
					var deliverables = deliveryInstructions.DeliverablesToBePrinted;
					AssertEquals("Total Count ", 18, deliverables.Count);

					AssertEquals("Test Consol eDoc BBB", deliverables[0].Name);
					AssertEquals("Test Consol eDoc AAA", deliverables[1].Name);
					AssertEquals("ConsolCommand", deliverables[2].Name);
					AssertEquals("Test Shipment1 eDoc AAA", deliverables[3].Name);
					AssertEquals("Test Shipment1 eDoc BBB", deliverables[4].Name);
					AssertEquals("ShipmentCommand1", deliverables[5].Name);
					AssertEquals("Test Shipment2 eDoc AAA", deliverables[6].Name);
					AssertEquals("Test Shipment2 eDoc BBB", deliverables[7].Name);
					AssertEquals("ShipmentCommand1", deliverables[8].Name);
					AssertEquals("ShipmentCommand2", deliverables[9].Name);
					AssertEquals("ShipmentCommand2", deliverables[10].Name);
					AssertEquals("ChildConsolCommand", deliverables[11].Name);

					//Other eDocs
					AssertEquals("Test Consol eDoc CCC", deliverables[12].Name);
					AssertEquals("Test Consol eDoc DDD", deliverables[13].Name);
					AssertEquals("Test Shipment1 eDoc CCC", deliverables[14].Name);
					AssertEquals("Test Shipment1 eDoc DDD", deliverables[15].Name);
					AssertEquals("Test Shipment2 eDoc CCC", deliverables[16].Name);
					AssertEquals("Test Shipment2 eDoc DDD", deliverables[17].Name);

					var eDocs = deliveryInstructions.EDocsToBeDelivered.OfType<IDeliverable>().ToList();
					AssertEquals("EDocs Count", 12, eDocs.Count);

					AssertEquals("Consol CTEST00001", eDocs[0].JobNumber);
					AssertEquals("Consol CTEST00001", eDocs[1].JobNumber);
					AssertEquals("Shipment STEST00001", eDocs[2].JobNumber);
					AssertEquals("Shipment STEST00001", eDocs[3].JobNumber);
					AssertEquals("Shipment STEST00002", eDocs[4].JobNumber);
					AssertEquals("Shipment STEST00002", eDocs[5].JobNumber);

					//Other eDocs
					AssertEquals("Consol CTEST00001", eDocs[6].JobNumber);
					AssertEquals("Consol CTEST00001", eDocs[7].JobNumber);
					AssertEquals("Shipment STEST00001", eDocs[8].JobNumber);
					AssertEquals("Shipment STEST00001", eDocs[9].JobNumber);
					AssertEquals("Shipment STEST00002", eDocs[10].JobNumber);
					AssertEquals("Shipment STEST00002", eDocs[11].JobNumber);

					var documents = deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>().ToList();
					AssertEquals("Documents Count", 6, documents.Count);

					AssertEquals("Consol CTEST00001", documents[0].JobNumber);
					AssertEquals("Shipment STEST00001", documents[1].JobNumber);
					AssertEquals("Shipment STEST00002", documents[2].JobNumber);
					AssertEquals("Shipment STEST00001", documents[3].JobNumber);
					AssertEquals("Shipment STEST00002", documents[4].JobNumber);
					AssertEquals("Consol CTEST00001", documents[5].JobNumber);

					//OtherEDocsToAttach
					AssertEquals("OtherEDocsToAttach count", 12, printTask[0].OtherEDocsToAttach.Count);
					foreach (var eDoc in printTask[0].OtherEDocsToAttach)
					{
						var docType = ((IeDoc)eDoc).DocType;
						if (docType == "CCC" || docType == "DDD")
						{
							AssertEquals(false, eDoc.ShouldPrintByDefault);
							AssertEquals(false, eDoc.IncludedInPrint);
						}
						else
						{
							AssertEquals(true, eDoc.ShouldPrintByDefault);
							AssertEquals(true, eDoc.IncludedInPrint);
						}
					}

					//Warning message
					othereDoc = deliverables[12];
					othereDoc.IncludedInPrint = true;
					AssertHasWarning(othereDoc.IncludedInPrintInfo, "The eDoc file  was not published.");

					deliveryInstructions.EDocsToBeDelivered.OfType<IDeliverable>().ForEach(d => d.IncludedInPrint = false);
				}

				//Warning message
				AssertNoWarning(othereDoc.IncludedInPrintInfo, "The eDoc file  was not published.");
			}
		}

		void CreateCommandDocumentsAndEDocs(PrintTaskDocumentPackTestHelper helper, BusinessObject consol, DocumentCommand consoleCommand)
		{
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			((Forwarding.IForwardingShipment)shipment1).JS_UniqueConsignRef = "STEST00001";
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			((Forwarding.IForwardingShipment)shipment2).JS_UniqueConsignRef = "STEST00002";
			var shipments = consol.GetType().BaseType.GetField("fShipments", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(consol) as BusinessObjectCollection;
			shipments.Add(shipment1);
			shipments.Add(shipment2);

			var eDocTypeA = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			var eDocTypeB = EDocsTestHelper.CreateDocType(Factory, "BBB", "UNL", "B Test Document BBB");
			EDocsTestHelper.CreateDocType(Factory, "CCC", "UNL", "C Test Document CCC");
			EDocsTestHelper.CreateDocType(Factory, "DDD", "UNL", "D Test Document DDD");

			consoleCommand.AddEDoc(eDocTypeA).SX_Index = 1;
			consoleCommand.AddEDoc(eDocTypeB).SX_Index = 0;

			var shipmentCommand1 = CreateTemplateForTest(helper, "ShipmentTemplate1", shipment1, "ShipmentCommand1", nameof(Core.Constants.DataContext.Shipment));
			shipmentCommand1.AddEDoc(eDocTypeA).SX_Index = 0;
			shipmentCommand1.AddEDoc(eDocTypeB).SX_Index = 1;

			var shipmentCommand2 = CreateTemplateForTest(helper, "ShipmentTemplate2", shipment2, "ShipmentCommand2", nameof(Core.Constants.DataContext.Shipment));
			shipmentCommand2.AddEDoc(eDocTypeA).SX_Index = 1;
			shipmentCommand2.AddEDoc(eDocTypeB).SX_Index = 0;

			var childConsolCommand = CreateTemplateForTest(helper, "ChildConsolTemplate", consol, "ChildConsolCommand", nameof(Core.Constants.DataContext.Consol));
			childConsolCommand.AddEDoc(eDocTypeA).SX_Index = 0;
			childConsolCommand.AddEDoc(eDocTypeB).SX_Index = 1;

			helper.CreateMenuMenuPivot(consoleCommand, shipmentCommand1).SF_Index = 0;
			helper.CreateMenuMenuPivot(consoleCommand, shipmentCommand2).SF_Index = 1;
			helper.CreateMenuMenuPivot(consoleCommand, childConsolCommand).SF_Index = 2;

			Factory.Save();

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			AddStorageDocs(documentFactory, consol, "Consol");
			AddStorageDocs(documentFactory, shipment1, "Shipment1");
			AddStorageDocs(documentFactory, shipment2, "Shipment2");
			documentFactory.Save();
		}

		DocumentCommand CreateTemplateForTest(PrintTaskDocumentPackTestHelper helper, string templateName, BusinessObject parent, string title, string businessContext)
		{
			var template = helper.CreateTemplate(businessContext, templateName);
			var command = helper.CreateDocCommand(title);
			command.Parent = (IDocumentSupportable)parent;
			command.SU_IsDocPack = true;
			helper.CreateMenuTemplatePivot(title, template, command);
			command.SU_BusinessContext = businessContext;
			return command;
		}

		void AddStorageDocs(IDocumentFactory documentFactory, BusinessObject bizO, string description)
		{
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, bizO);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = $"Test {description} eDoc AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = $"Test {description} eDoc BBB";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "CCC", false).Description = $"Test {description} eDoc CCC";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "DDD", false).Description = $"Test {description} eDoc DDD";
			}
		}

		#endregion

		public void TestInstructionWithReportToSpecifyPageRanges()
		{
			var instruction = new DeliveryInstructions();
			AssertNull("should be null if doc pack is empty.", instruction.ReportToSpecifyPageRanges);
			instruction.DocPack = null;
			AssertNull("should be null if doc pack is null.", instruction.ReportToSpecifyPageRanges);

			Assert("should be false if ReportToSpecifyPageRanges is null.", !instruction.PageRangesSpecified);
			instruction.PageRangesSpecified = true;
			Assert("should not can set with value if ReportToSpecifyPageRanges is null.", !instruction.PageRangesSpecified);
		}

		public void TestSpecificPageRanges()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestSpecificPageRangesDoesNotCacheResult";
			menuItem.SU_BusinessContext = ".DummyBusinessObject";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");
			dummy.Collection.AddNew("CCC", "CCC Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				documentPack.Add(report);
				AssertEquals(true, report.CanSpecifyPageRanges);

				var instructions = new DeliveryInstructions(documentPack);
				AssertEquals(3, instructions.DataSourceRowCountIfPageRangesSpecifiable);
				AssertNull(instructions.ReportToSpecifyPageRanges.SpecifiedDataRowSource);

				instructions.PageRangesSpecified = true;
				instructions.SpecifiedPageRangesText = "1,3";
				AssertEquals(2, instructions.ReportToSpecifyPageRanges.SpecifiedDataRowSource.RowCount);

				instructions.SpecifiedPageRangesText = "1,3,4";
				AssertNull(instructions.ReportToSpecifyPageRanges.SpecifiedDataRowSource);
				Assert(instructions.SpecifiedPageRangesTextInfo.HasError(@"Please enter valid page ranges.
To print a range of pages, e.g pages 2 through to 9 enter as follows 2 - 9. To print specific pages only, enter as follows 2, 5, 9."));
			}
		}

		public void TestDeliverablesToBePrintedHasDocumentsAndEDocs()
		{
			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.AddRelatedObjectsForTest = true;
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;

			var pivot = command.Documents.AddNew();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			var docType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			command.AddEDoc(docType).SX_Filter = "\"<RL_Code>\" == \"AUSYD\"";

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, bizo);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
			}

			documentFactory.Save();

			using (var pack = new DocumentPack(command, bizo, null, null))
			{
				var instructions = new DeliveryInstructions(pack);
				var includeCount = instructions.DeliverablesToBePrinted.Cast<IDeliverable>().Count(x => x.IncludeInPrint);
				var notPrintByDefault = instructions.EDocsToBeDelivered.Cast<IDeliverable>().Count(x => !x.ShouldPrintByDefault);

				AssertEquals("DeliveryInstructions should have 1 report", 1, instructions.DocumentsToBeDelivered.Count);
				AssertEquals("DeliveryInstructions should have 3 eDocs", 3, instructions.EDocsToBeDelivered.Count);
				AssertEquals("DeliveryInstructions should have 1 report and 1 eDoc included in printed", 2, includeCount);
				AssertEquals("DeliveryInstructions should have 2 eDoc is not print by default", 2, notPrintByDefault);
			}
		}

		public void TestEmailSubjectMacroCouldGetMostTopBusinessObjectField()
		{
			OrgHeader documentSupportable = Factory.New<OrgHeader>();
			var menuItem = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery());

			var task = new PrintTask();
			task.MostTopLevelBusinessObject = documentSupportable;
			var pack = new DocumentPack(menuItem);
			task.Add(pack);

			var instructions = new DeliveryInstructions(pack);

			var relatedObjects = instructions.GetRelatedBusinessObjectsForEmailSubject(new DocDeliveryContact(new BusinessObjectFactory()));

			Assert(relatedObjects.Contains(documentSupportable));
		}

		public void TestLanguageMaxlength()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			AssertEquals(5, instructions.LanguageInfo.MaxLength);
		}

		public void TestNoExtraDbHitsOnGettingRecipientsWhenJobDoesNotSupportJobDocumentRecipient()
		{
			var instructionsWithJobSpecificRecipient = GetDeliveryInstructions(out _, out _, out _, out _, false);
			Factory.Save();

			var recipients = instructionsWithJobSpecificRecipient.Recipients;
			var tableHitsOnJobDocumentDelivery = Factory.GetTableHitCount(JobDocumentDelivery.Schema.TableName);
			var tableHitsOnJobDocumentExclusion = Factory.GetTableHitCount(JobDocumentExclusion.Schema.TableName);
			AssertEquals(0, tableHitsOnJobDocumentExclusion);
			AssertEquals(0, tableHitsOnJobDocumentExclusion);

			var instructionsWithoutJobSpecificRecipient = GetDeliveryInstructions(out _, out _, out _, out _);
			Factory.Save();

			recipients = instructionsWithoutJobSpecificRecipient.Recipients;
			tableHitsOnJobDocumentDelivery = Factory.GetTableHitCount(JobDocumentDelivery.Schema.TableName);
			tableHitsOnJobDocumentExclusion = Factory.GetTableHitCount(JobDocumentExclusion.Schema.TableName);
			AssertEquals(1, tableHitsOnJobDocumentExclusion);
			AssertEquals(1, tableHitsOnJobDocumentExclusion);
		}

		public void TestAutoDeliveryRecipientsWithJobDocumentDeliveryExclusionByDocument()
		{
			var instructions = GetDeliveryInstructions(out var org, out var orgDocumentWithGroup, out var orgDocumentWithDocument, out var menuItem);

			orgDocumentWithDocument.OD_SU_MenuItem = Factory.NewWithValidTestData<StmMenuItem>().PK;
			var exclusion = Factory.NewWithValidTestData<JobDocumentDelivery>();
			exclusion.JDC_ParentID = org.PK;
			exclusion.JDC_ParentTableCode = org.TablePrefix;
			exclusion.JDC_SU_MenuItem = menuItem.PK;
			exclusion.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			exclusion.JDC_OC_Contact = orgDocumentWithGroup.OD_OC;
			Factory.Save();

			AssertOnlySystemDefaultContactReturned(instructions);

			var jobRecipient = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobRecipient.JDC_ParentID = org.PK;
			jobRecipient.JDC_ParentTableCode = org.TablePrefix;
			jobRecipient.JDC_DocumentGroup = ContactType.Consignee.Code;
			jobRecipient.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobRecipient.JDC_ContactName = "Just1n";
			jobRecipient.EmailToRecipients.Value = "i@just1n.net";
			Factory.Save();

			AssertOnlyJobSpecificContactReturned(instructions);

			orgDocumentWithGroup.OD_DocumentGroup = ContactType.Warehouse.Code;
			orgDocumentWithDocument.OD_SU_MenuItem = menuItem.PK;
			Factory.Save();

			AssertOnlyJobSpecificContactReturned(instructions);
		}

		public void TestAutoDeliveryRecipientsWithJobDocumentDeliveryExclusionByGroup()
		{
			var instructions = GetDeliveryInstructions(out var org, out var orgDocumentWithGroup, out var orgDocumentWithDocument, out _);

			orgDocumentWithGroup.OD_DocumentGroup = ContactType.Warehouse.Code;
			var exclusion = Factory.NewWithValidTestData<JobDocumentDelivery>();
			exclusion.JDC_ParentID = org.PK;
			exclusion.JDC_ParentTableCode = org.TablePrefix;
			exclusion.JDC_DocumentGroup = ContactType.Consignee.Code;
			exclusion.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			exclusion.JDC_OC_Contact = orgDocumentWithDocument.OD_OC;
			Factory.Save();

			AssertOnlySystemDefaultContactReturned(instructions);

			var jobRecipient = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobRecipient.JDC_ParentID = org.PK;
			jobRecipient.JDC_ParentTableCode = org.TablePrefix;
			jobRecipient.JDC_DocumentGroup = ContactType.Consignee.Code;
			jobRecipient.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobRecipient.JDC_ContactName = "Just1n";
			jobRecipient.EmailToRecipients.Value = "i@just1n.net";
			Factory.Save();

			AssertOnlyJobSpecificContactReturned(instructions);

			orgDocumentWithGroup.OD_DocumentGroup = ContactType.Consignee.Code;
			orgDocumentWithDocument.OD_SU_MenuItem = Factory.NewWithValidTestData<StmMenuItem>().PK;
			Factory.Save();

			AssertOnlyJobSpecificContactReturned(instructions);
		}

		void AssertOnlySystemDefaultContactReturned(DeliveryInstructions instructions)
		{
			instructions.ClearRecipients();
			AssertEquals(1, instructions.Recipients.Count);

			var autoContact = instructions.Recipients[0];
			Assert("Only 1 system default contact returned because orgDocumentWithDocument is suppressed", autoContact.IsSystemDefaultContact);
		}

		void AssertOnlyJobSpecificContactReturned(DeliveryInstructions instructions)
		{
			instructions.ClearRecipients();
			AssertEquals(1, instructions.Recipients.Count);

			var autoContact = instructions.Recipients[0];
			Assert(!autoContact.IsSystemDefaultContact);
			AssertEquals("Only job specific contact returned", "Just1n", autoContact.Name);
		}

		public void TestAutoDeliveryRecipientsWithJobSpecific()
		{
			var instructions = GetDeliveryInstructions(out var org, out _, out _, out var menuItem);

			var jobRecipient1 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobRecipient1.JDC_ParentID = org.PK;
			jobRecipient1.JDC_ParentTableCode = org.TablePrefix;
			jobRecipient1.JDC_SU_MenuItem = menuItem.PK;
			jobRecipient1.JDC_ContactName = "Just1n Job 1";
			jobRecipient1.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobRecipient1.JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
			jobRecipient1.JDC_EmailSubjectMacro = "Email Subject Macro";

			var jobRecipient2 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobRecipient2.JDC_ParentID = org.PK;
			jobRecipient2.JDC_ParentTableCode = org.TablePrefix;
			jobRecipient2.JDC_DocumentGroup = ContactType.Consignee.Code;
			jobRecipient2.JDC_ContactName = "Just1n Job 2";
			jobRecipient2.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			Factory.Save();

			AssertEquals(3, instructions.Recipients.Count);

			var recipients = instructions.Recipients.OfType<DocDeliveryContact>();
			var contactAuto = recipients.Single(r => r.Name == "Just1n");
			var contactJobEmail = recipients.Single(r => r.Name == "Just1n Job 1" && r.DeliveryMethod == Core.Constants.ContactNotifyModes.Email);
			var contactJobPrint = recipients.Single(r => r.Name == "Just1n Job 2" && r.DeliveryMethod == Core.Constants.ContactNotifyModes.Print);

			AssertNotNull(contactAuto);
			AssertEquals("Email Subject Macro", contactJobEmail.EmailSubjectMacro);
			AssertEquals(OrgConstants.AttachmentType.PDF, contactJobEmail.AttachmentType);
			AssertNotNull(contactJobPrint);
		}

		public void TestAutoDeliveryRecipientsWithExclusion()
		{
			var instructions = GetDeliveryInstructions(out var org, out var orgDocumentWithGroup, out var orgDocumentWithDocument, out _);
			AssertEquals(1, instructions.Recipients.Count);

			var docContact = instructions.Recipients[0];
			AssertEquals(Core.Constants.ContactNotifyModes.Email, docContact.DeliveryMethod);
			AssertEquals("Just1n", docContact.Name);
			AssertEquals("i@just1n.net", docContact.DeliveryAddress);
			AssertEquals(0, docContact.EmailCarbonCopyRecipients.Count);

			var exclusion1 = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion1.JDE_ParentID = org.PK;
			exclusion1.JDE_ParentTableCode = org.TablePrefix;
			exclusion1.JDE_OD_Document = orgDocumentWithGroup.PK;
			Factory.Save();

			instructions.ClearRecipients();
			AssertEquals(1, instructions.Recipients.Count);

			docContact = instructions.Recipients[0];
			AssertEquals(Core.Constants.ContactNotifyModes.Email, docContact.DeliveryMethod);
			AssertEquals("Just1n", docContact.Name);
			AssertEquals("i@just1n.net", docContact.DeliveryAddress);
			AssertEquals(0, docContact.EmailCarbonCopyRecipients.Count);

			exclusion1.Delete();
			var exclusion2 = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion2.JDE_ParentID = org.PK;
			exclusion2.JDE_ParentTableCode = org.TablePrefix;
			exclusion2.JDE_OD_Document = orgDocumentWithDocument.PK;
			Factory.Save();

			instructions.ClearRecipients();
			AssertEquals(1, instructions.Recipients.Count);

			docContact = instructions.Recipients[0];
			AssertNotEquals("Just1n", docContact.Name);
		}

		DeliveryInstructions GetDeliveryInstructions(out OrgHeader org, out OrgDocument orgDocumentWithGroup, out OrgDocument orgDocumentWithDocument, out StmMenuItem menuItem, bool supportJobDocumentRecipient = true)
		{
			org = supportJobDocumentRecipient ? Factory.NewWithValidTestData<OrgHeaderThatSupportJobDocumentRecipient>() : Factory.NewWithValidTestData<OrgHeader>();
			menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignee.Code;
			var docPack = new DocumentPack(menuItem);
			docPack.DocumentSupporter = new AutoDeliveryBizO(org);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Just1n";
			contact.OC_Email = "i@just1n.net";
			orgDocumentWithGroup = contact.Documents.AddNew();
			orgDocumentWithGroup.OD_DocumentGroup = ContactType.Consignee.Code;
			orgDocumentWithGroup.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			orgDocumentWithGroup.CarbonCopyRecipients.Value = "cc@cc.com";
			orgDocumentWithDocument = contact.Documents.AddNew();
			orgDocumentWithDocument.OD_SU_MenuItem = menuItem.PK;
			orgDocumentWithDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;

			return new DeliveryInstructions(docPack);
		}

		public void TestDeliveryGroupsAlwaysAtLeastContainOneDeliveryGroup()
		{
			AssertEquals(1, Instructions.DeliveryGroups.Count);

			var oldGroup = Instructions.DeliveryGroups[0];
			Instructions.DeliveryGroups.Clear();
			AssertEquals(1, Instructions.DeliveryGroups.Count);

			var newGroup = Instructions.DeliveryGroups[0];
			AssertNotEquals(oldGroup, newGroup);
		}

		public void TestGetRelatedBusinessObjectsForEmailSubject()
		{
			AssertEquals(8, Instructions.GetRelatedBusinessObjectsForEmailSubject(new DocDeliveryContact(Factory.GetCachedReadOnlyFactory())).Length);
		}

		public void TestGetRelatedBusinessObjectsForEmailSubject_ShouldStartWithDocumentSupporterBusinessObjectFirst()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			using (var documentPack = new DocumentPack(documentCommand))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "JERRY";
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				documentPack.DocumentSupporter = new AutoDeliveryBizO(org);
				var orgHeader = (OrgHeader)deliveryInstructions.GetRelatedBusinessObjectsForEmailSubject(new DocDeliveryContact(Factory.GetCachedReadOnlyFactory()))[1];

				AssertEquals("JERRY", orgHeader.OH_Code);
			}
		}

		public void TestIsDeliveryForm()
		{
			Assert("Default value for IsDeliveringFormDocument is false", !Instructions.IsDeliveringFormDocument);
		}

		public void TestRecipientsDefaultContactNameCantBeTooLong()
		{
			var documentCommand = Factory.New<DocumentCommand>();

			using (var documentPack = new DocumentPack(documentCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				var oldLanguage = deliveryInstructions.Language;

				deliveryContact.SystemDefaultContactName = (NoResString)"Carlos";
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.Spanish;

				AssertEquals("Language should change with a valid name", deliveryInstructions.Language, Enterprise.Core.Constants.Languages.Spanish);

				deliveryInstructions.Language = oldLanguage;
				deliveryContact.SystemDefaultContactName = (NoResString)"This name will overflow the salutation message";
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.Spanish;

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(Core.SharedConstants.Languages.Spanish, deliveryInstructions.Language);

				deliveryContact.SystemDefaultContactName = (NoResString)"This name will overflow the salutation message,This name will overflow the salutation message";
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.Russian;

				string errorMessage = "Error Language could not be set because the following recipient's names would exceed the length limit of the default salutation:\r\n" +
					"  - This name will overflow the salutation message,This name will overflow the salutation message\r\n" +
					"Change the recipients names or choose a valid language.";

				AssertEquals("Error message should've been shown to user", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(Core.SharedConstants.Languages.Spanish, deliveryInstructions.Language);
			}
		}

		public void TestRecipientsDefaultContactNameCantBeTooLong_WithResString()
		{
			var documentCommand = Factory.New<DocumentCommand>();

			using (var grmMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			using (var documentPack = new DocumentPack(documentCommand))
			{
				var strInEng = "This name will not overflow while in English";
				var strInGrm = "Dieser Name wird nicht in englischer Sprache überlaufen , während";
				grmMockData.Put("0", new ResourceStringData("0", strInGrm));

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliveryContact = deliveryInstructions.Recipients.AddNew();

				deliveryContact.SystemDefaultContactName = (NoResString)strInEng;
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.German;

				AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestRecipients_WithDefaultEmailFromAddress()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;
			menuItem.SU_EmailSenderOverride = "test@test.com";

			using (var documentPack = new DocumentPack(menuItem))
			using (var dummyReport = documentPack.AddNew())
			{
				dummyReport.MenuItem = menuItem;
				var instructions = new DeliveryInstructions(documentPack);

				AssertEquals("test@test.com", instructions.Recipients.AddNew().DefaultEmailFromAddress);
			}
		}

		public void TestRecipients_WithDefaultEmailFromAddress_NoReport()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;
			menuItem.SU_EmailSenderOverride = "test@test.com";

			using (var documentPack = new DocumentPack(menuItem))
			{
				var instructions = new DeliveryInstructions(documentPack);

				AssertEquals("test@test.com", instructions.Recipients.AddNew().DefaultEmailFromAddress);
			}
		}

		public void TestRecipients_WithDefaultEmailFromAddress_WithOtherReport()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;
			menuItem.SU_EmailSenderOverride = "test@test.com";

			var otherMenuItem = Factory.New<StmMenuItem>();
			otherMenuItem.SU_ContactType = ContactType.Warehouse.Code;
			otherMenuItem.SU_EmailSenderOverride = "test2@test2.com";

			using (var documentPack = new DocumentPack(menuItem))
			using (var dummyReport = documentPack.AddNew())
			{
				dummyReport.MenuItem = otherMenuItem;
				var instructions = new DeliveryInstructions(documentPack);

				AssertEquals("test@test.com", instructions.Recipients.AddNew().DefaultEmailFromAddress);
			}
		}

		public void TestAttachmentTypesForDocumentCommand()
		{
			var documentCommand = Factory.New<DocumentCommand>();

			using (var documentPack = new DocumentPack(documentCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				var attachmentTypes = deliveryContact.AttachmentTypes;

				AssertEquals(8, attachmentTypes.Count);
				Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain PDFC.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfc));
				Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			}
		}

		public void TestAttachmentTypesForReportCommandWithoutColumnHeaders()
		{
			var reportCommand = Factory.New<ReportCommand>();

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				var attachmentTypes = deliveryContact.AttachmentTypes;

				AssertEquals(11, attachmentTypes.Count);
				Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain CSV.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
				Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
			}
		}

		public void TestAttachmentTypesForReportCommandWithColumnHeaders()
		{
			var reportCommand = Factory.LoadTop1<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Staff Profile Report"));

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				var attachmentTypes = deliveryContact.AttachmentTypes;

				AssertEquals(13, attachmentTypes.Count);
				Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain CSV.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
				Assert("Attachment types should contain CS2.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.CsvWithHeadings));
				Assert("Attachment types should contain XML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xml));
				Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
			}
		}

		public void TestAttachmentTypesForReportCommandWithAutoDeliveryNotAllowed()
		{
			var reportCommand = Factory.New<ReportCommand>();

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.AllowAutoDelivery = false;
				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				var attachmentTypes = deliveryContact.AttachmentTypes;

				AssertEquals(11, attachmentTypes.Count);
				Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain CSV.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
				Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself()
		{
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsChartAndDISABLEXLSXEXPORT.xls",
				"",
				"XLS template that contains chart objects can't be exported as XLSX files.\nTemplate that contains the parameter 'DisableXlsxExport' can't be exported as XLSX files.");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsNoChartAndDISABLEXLSXEXPORT.xls",
				"",
				"Template that contains the parameter 'DisableXlsxExport' can't be exported as XLSX files.");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsChartAndNoDISABLEXLSXEXPORT.xls",
				"",
				"XLS template that contains chart objects can't be exported as XLSX files.");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsNoChartAndNoDISABLEXLSXEXPORT.xls",
				"",
				"");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsxChartAndDISABLEXLSXEXPORT.xlsx",
				"XLSX template that contains chart objects can't be exported as XLS files.",
				"Template that contains the parameter 'DisableXlsxExport' can't be exported as XLSX files.");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsxNoChartAndDISABLEXLSXEXPORT.xlsx",
				"",
				"Template that contains the parameter 'DisableXlsxExport' can't be exported as XLSX files.");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsxChartAndNoDISABLEXLSXEXPORT.xlsx",
				"XLSX template that contains chart objects can't be exported as XLS files.",
				"");
			AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(
				"XlsxNoChartAndNoDISABLEXLSXEXPORT.xlsx",
				"",
				"");
		}

		void AssertAttachmentTypeForReportCommandShouldOnlyContainTemplateFormatItself(string templateName, string expectedContact1ErrorMessage, string expectedContact2ErrorMessage)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_ContactType = ContactType.Warehouse.Code;
			var dummy = Factory.New<DummyDocumentSupportable>();
			using var documentPack = new DocumentPack(menuItem);
			using var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false);
			documentPack.Add(report);
			report.PrepareForRender();
			var instructions = new DeliveryInstructions(documentPack);
			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = OrgConstants.AttachmentType.XLS;
			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.AttachmentType = OrgConstants.AttachmentType.XLSX;

			instructions.RunPreSaveValidation();
			var actualContact1ErrorMessage = contact1.AttachmentTypeInfo.GetErrors().ToMessageListString();
			var actualContact2ErrorMessage = contact2.AttachmentTypeInfo.GetErrors().ToMessageListString();

			AssertEquals($"Contact1's error message when using {templateName}", expectedContact1ErrorMessage, actualContact1ErrorMessage);
			AssertEquals($"Contact2's error message when using {templateName}", expectedContact2ErrorMessage, actualContact2ErrorMessage);
		}

		public void TestChangingTheDeliveryContactDoesNotChangeTheOfficialContact()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;

			OrgHeader officialOrganization = Factory.New<OrgHeader>();
			officialOrganization.OH_Code = "Crow";
			officialOrganization.OH_FullName = "Crow's Foot Pty Ltd";
			officialOrganization.MainAddress.OA_Address1 = "Official Address";

			OrgContact contact = officialOrganization.Contacts.AddNew();
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact.OC_ContactName = "Crow's Foot";

			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Warehouse.Code;

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(officialOrganization);
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("Precondition: 1 auto delivery contact added", 1, instructions.Recipients.Count);
			AssertEquals("instructions.Recipients[0].Name", "Crow's Foot", instructions.Recipients[0].Name);
			AssertEquals("instructions.Recipients[0].Address1", "Official Address", instructions.Recipients[0].Address1);
			AssertEquals("instructions.OfficialRecipient.Name", "Crow's Foot", instructions.OfficialRecipient.Name);
			AssertEquals("instructions.OfficialRecipient.Address1", "Official Address", instructions.OfficialRecipient.Address1);

			OrgHeader deliveryOrganization = Factory.New<OrgHeader>();
			deliveryOrganization.OH_Code = "Charles";
			deliveryOrganization.MainAddress.OA_Address1 = "Delivery Address";
			Factory.Save();

			instructions.Recipients[0].OrgHeaderPK = deliveryOrganization.PK;
			instructions.Recipients[0].Name = "Charles Babbage";

			AssertEquals("instructions.Recipients.Count", 1, instructions.Recipients.Count);
			AssertEquals("instructions.Recipients[0].Name", "Charles Babbage", instructions.Recipients[0].Name);
			AssertEquals("instructions.Recipients[0].Address1", "Delivery Address", instructions.Recipients[0].Address1);
			AssertEquals("instructions.OfficialRecipient.Name", "Crow's Foot", instructions.OfficialRecipient.Name);
			AssertEquals("instructions.OfficialRecipient.Address1", "Official Address", instructions.OfficialRecipient.Address1);

			instructions.Recipients.RemoveAndDeleteAll();
			AssertEquals("instructions.Recipients.Count", 0, instructions.Recipients.Count);
			AssertEquals("instructions.OfficialRecipient.Name", "Crow's Foot", instructions.OfficialRecipient.Name);
			AssertEquals("instructions.OfficialRecipient.Address1", "Official Address", instructions.OfficialRecipient.Address1);

			DocDeliveryContact andersHejlsberg = instructions.Recipients.AddNew();
			andersHejlsberg.OrgHeaderPK = deliveryOrganization.PK;
			andersHejlsberg.Name = "Anders Hejlsberg";

			AssertEquals("instructions.Recipients.Count", 1, instructions.Recipients.Count);
			AssertEquals("instructions.Recipients[0].Name", "Anders Hejlsberg", instructions.Recipients[0].Name);
			AssertEquals("instructions.Recipients[0].Address1", "Delivery Address", instructions.Recipients[0].Address1);
			AssertEquals("instructions.OfficialRecipient.Name", "Crow's Foot", instructions.OfficialRecipient.Name);
			AssertEquals("instructions.OfficialRecipient.Address1", "Official Address", instructions.OfficialRecipient.Address1);
		}

		public void TestSystemDefaultContactSalutation()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Sales.Code;

			OrgHeader officialOrganization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(officialOrganization);
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, instructions.Language);
			AssertEquals("1 auto delivery contact added", 1, instructions.Recipients.Count);
			AssertEquals("instructions.Recipients[0].Name", ContactType.Sales.DefaultName.ToString(Enterprise.Core.Constants.Languages.EnglishAmerican), instructions.Recipients[0].Name);
			AssertEquals("instructions.Recipients[0].Salutation", "Dear The Sales Manager", instructions.Recipients[0].Salutation.ToString());

			instructions.Language = Enterprise.Core.Constants.Languages.ChineseSimplified;
			AssertEquals("instructions.Recipients[0].Name", ContactType.Sales.DefaultName.ToString(Enterprise.Core.Constants.Languages.ChineseSimplified), instructions.Recipients[0].Name);
		}

		#region Document Pack

		public void TestDocPack()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			AssertNotNull(instructions.DocPack);

			DocumentPack pack = new DocumentPack();
			instructions = new DeliveryInstructions(pack);
			AssertNotNull(instructions.DocPack);
			AssertEquals(pack, instructions.DocPack);
		}

		public void TestDocPackWithParentGuid()
		{
			DocumentPack pack = new DocumentPack();
			Guid parentGuidValue = new Guid();
			Instructions = new DeliveryInstructions(pack, new ZGuid(parentGuidValue));
			AssertNotNull(Instructions.DocPack);
			AssertEquals(new ZGuid(parentGuidValue), Instructions.ParentGuid);
		}

		#endregion

		public void TestDocumentPackTitle()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			AssertEquals(ZString.Empty, instructions.DocumentPackTitle);

			instructions.DocumentPackTitle = "TITLE";
			AssertEquals("TITLE", instructions.DocumentPackTitle);

			DocumentCommand menu = Factory.New<DocumentCommand>();
			menu.SU_MenuName = "MENU NAME";
			DocumentPack pack = new DocumentPack(menu);
			instructions = new DeliveryInstructions(pack);
			AssertEquals("MENU NAME", instructions.DocumentPackTitle);

			instructions.DocumentPackTitle = "TITLE";
			AssertEquals("TITLE", instructions.DocumentPackTitle);
		}

		public void TestDocumentPackTitleInfo()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			AssertEquals("DocumentPackTitle", instructions.DocumentPackTitleInfo.Name);
			AssertEquals(50, instructions.DocumentPackTitleInfo.MaxLength);
			AssertEquals(false, instructions.DocumentPackTitleInfo.ReadOnly);
		}

		public void TestCompanyLogoOverrideForClientBrandingOnEmailCoversheet()
		{
			using (var testImage = Image.FromFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TransparentPNG.png")))
			{
				DocDeliveryContact contact = Instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;

				var mockDeliveryInstructions = new Mock<DeliveryInstructions>() { CallBase = true };
				var instructions = mockDeliveryInstructions.Object;
				instructions.Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
				instructions.Recipients.Add(contact);

				SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testImage);
				AssertEquals("Default is SystemDataRegistry.Instance.CompanyLogo.Value", testImage.Size, instructions.CompanyLogo.Size);

				var contactOrg = Factory.NewWithValidTestData<OrgHeader>();
				contactOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 22);
				Dictionary<string, object> documentConstants = new Dictionary<string, object>();
				documentConstants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
				documentConstants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
				((IBODocDataProvider)instructions).SetDocWrapperContext(documentConstants);
				Factory.Save();

				DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				BrandingTestHelperClass.SetClientBrandRegistryImage();

				AssertEquals("Brand image from client's tariff level", new Size(2, 2), instructions.CompanyLogo.Size);
			}
		}

		public void TestTIFAttachmentsOnlyUpdatesAttachmentTypes()
		{
			DocDeliveryContact contact = Instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			Assert("PDF available", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("PDF/A available", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDFA));
			Assert("XLS available", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("TIF available", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));

			Instructions = new DeliveryInstructions();
			Instructions.TIFAttachmentsOnly = true;
			Instructions.Recipients.Add(contact);

			AssertEquals("Only 1 items", 1, contact.AttachmentTypes.Count);
			Assert("PDF NOT available", !contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("PDF/A NOT available", !contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDFA));
			Assert("XLS NOT available", !contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("TIF available", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));
		}

		public void TestOverrideNotifyModesList()
		{
			var contact = Instructions.Recipients.AddNew();

			Assert("E-Mail available", contact.NotifyModes.ContainsCode("E-Mail"));
			Assert("Fax available", contact.NotifyModes.ContainsCode("Fax"));
			Assert("Print available", contact.NotifyModes.ContainsCode("Print"));
			Assert("ePrint available", contact.NotifyModes.ContainsCode("ePrint"));

			var newList = new CodeDescriptionPairList { new CodeDescriptionPair("code", "description") };
			Instructions.OverrideNotifyModesList(newList);

			Assert("New code available", contact.NotifyModes.Contains(new CodeDescriptionPair("code", "description")));
			AssertEquals("Other codes removed", 1, contact.NotifyModes.Count);

			var newContact = Instructions.Recipients.AddNew();
			AssertSame("Should override on new codes", contact.NotifyModes, newContact.NotifyModes);
		}

		public void TestOutputDirectory()
		{
			AssertEquals("Blank by default", "", Instructions.OutputDirectory);

			Instructions.OutputDirectory = Temp.TempPath;
			AssertEquals("Setter works", Temp.TempPath, Instructions.OutputDirectory);
		}

		public void TestAllowedDeliveryOptions()
		{
			Instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			AssertEquals("Getter works", AllowedDeliveryOptions.All, Instructions.DeliveryOptions);

			Instructions.DeliveryOptions = AllowedDeliveryOptions.HardCopyOnly;
			AssertEquals("Getter works", AllowedDeliveryOptions.HardCopyOnly, Instructions.DeliveryOptions);
		}

		public void TestIsDefaultDeliveryRecipient()
		{
			OrgHeader documentSupportable = Factory.New<OrgHeader>();
			DocumentCommand menuItem = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery());

			DocumentPack pack = new DocumentPack(menuItem, documentSupportable, null, null);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			//AssertEquals("No delivery information was available from the IDocumentSupportable object so a default recipient should be created", true, Instructions.IsDefaultDeliveryRecipient);

			instructions.Recipients.RemoveAll();
			DocDeliveryContact newDeliveryContact = instructions.Recipients.AddNew();
			newDeliveryContact.Name = "Clinty";
			newDeliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("Delivery information was populated, so the recipient(s) are not the default", false, instructions.IsDefaultDeliveryRecipient);
		}

		public void TestDestination()
		{
			Instructions.Destination = DeliveryInstructionDestination.Auto;
			AssertEquals("Getter works", DeliveryInstructionDestination.Auto, Instructions.Destination);

			Instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			AssertEquals("Getter works", DeliveryInstructionDestination.TakenFromContact, Instructions.Destination);
		}

		public void TestDocPackIndexer()
		{
			AssertEquals("Precondition - 0 by default", 0, Instructions.DocumentPackIndexer);

			Instructions.DocumentPackIndexer = 5;
			AssertEquals("Getter works", 5, Instructions.DocumentPackIndexer);
		}

		public void TestAllowProperties()
		{
			AssertEquals("Email allowed by default", true, Instructions.AllowEmail);
			AssertEquals("Print allowed by default", true, Instructions.AllowPrint);
			AssertEquals("Fax allowed by default", true, Instructions.AllowFax);
			AssertEquals("Preview allowed by default", true, Instructions.AllowPreview);
			AssertEquals("Modify option allowed by default", true, Instructions.AllowModify);
			AssertEquals("False by default", false, Instructions.AllowSaveDefaults);
			AssertEquals("False by default", false, Instructions.AllowSaveDefaults);
			AssertEquals("False by default", false, Instructions.TIFAttachmentsOnly);
			AssertEquals("False by default", false, Instructions.AllowModifyAndPreviewInExcel);

			Instructions.AllowEmail = false;
			AssertEquals("Setter works", false, Instructions.AllowEmail);

			Instructions.AllowPrint = false;
			AssertEquals("Setter works", false, Instructions.AllowPrint);

			Instructions.AllowPreview = false;
			AssertEquals("Setter works", false, Instructions.AllowPreview);

			Instructions.AllowFax = false;
			AssertEquals("Setter works", false, Instructions.AllowFax);

			Instructions.AllowAutoDelivery = false;
			AssertEquals("Setter works", false, Instructions.AllowAutoDelivery);

			Instructions.AllowModify = false;
			AssertEquals("Setter works", false, Instructions.AllowModify);

			Instructions.TIFAttachmentsOnly = true;
			AssertEquals("Setter works", true, Instructions.TIFAttachmentsOnly);

			Instructions.AllowSaveDefaults = true;
			AssertEquals("Setter works", true, Instructions.AllowSaveDefaults);

			Instructions.AllowModifyAndPreviewInExcel = true;
			AssertEquals("Setter works", true, Instructions.AllowModifyAndPreviewInExcel);
		}

		public void TestDocumentPackCountAsString()
		{
			Instructions.DocumentPackCountAsString = "0";
			AssertEquals("Parsed correctly", 0, Instructions.DocumentPackCount);
			AssertEquals("Return string value", "0", Instructions.DocumentPackCountAsString);

			Instructions.DocumentPackCountAsString = "99";
			AssertEquals("Parsed correctly", 99, Instructions.DocumentPackCount);
			AssertEquals("Return string value", "99", Instructions.DocumentPackCountAsString);

			Instructions.DocumentPackCountAsString = "19m";
			AssertEquals("Set to nothing", 0, Instructions.DocumentPackCount);
			AssertEquals("Retained bad value", "19m", Instructions.DocumentPackCountAsString);

			var largeInt = (long)int.MaxValue * 2;
			Instructions.DocumentPackCountAsString = largeInt.ToString();
			AssertEquals("Set to nothing", 0, Instructions.DocumentPackCount);
			AssertEquals("Retained bad value", largeInt.ToString(), Instructions.DocumentPackCountAsString);
		}

		public void TestMultiDocPack()
		{
			Assert("Initial value", !Instructions.MultipleDocumentPacks);

			Instructions.DocumentPackCount = 50;
			Assert(Instructions.MultipleDocumentPacks);

			Instructions.DocumentPackCount = 0;
			Assert(!Instructions.MultipleDocumentPacks);
		}

		public void TestPrinterDelivery()
		{
			AssertNotNull(Instructions.PrinterDelivery);

			DocDeliveryPrintDetails deliveryDetails = new DocDeliveryPrintDetails(Factory);
			Instructions.PrinterDelivery = deliveryDetails;
			AssertNotNull(Instructions.PrinterDelivery);
			AssertEquals(deliveryDetails, Instructions.PrinterDelivery);
		}

		public void TestCoverNoteReadOnly()
		{
			AssertEquals("Cover note not included", false, Instructions.IncludeCoverNote);
			AssertEquals("Cover note readonly", true, Instructions.CoverNoteInfo.ReadOnly);

			Instructions.IncludeCoverNote = true;
			AssertEquals("Cover note not readonly", false, Instructions.CoverNoteInfo.ReadOnly);

			Instructions.CoverNote = "Hello";
			Instructions.IncludeCoverNote = false;
			AssertEquals("Cover note readonly", true, Instructions.CoverNoteInfo.ReadOnly);
			AssertEquals("Cover note empty", "", Instructions.CoverNote);
		}

		public void TestRecipientsLoadedForSingleDocPackOnly()
		{
			AssertEquals("Recipients is only loaded when there is a SINGLE doc pack", 0, Instructions.Recipients.Count);

			Instructions = new DeliveryInstructions(new DocumentPack());
			Assert("Recipients is loaded", Instructions.Recipients.Count > 0);
		}

		public void TestRecipientsHasPrintModeForAutoDeliveryNotAllowed()
		{
			DeliveryInstructions instructions = new DeliveryInstructions(new DocumentPack());
			instructions.AllowAutoDelivery = false;

			AssertEquals("Recipients is loaded with a single contact set to Print Mode", 1, instructions.Recipients.Count);
			AssertEquals("Set to print mode", Core.Constants.ContactNotifyModes.Print, instructions.Recipients[0].DeliveryMethod);
		}

		public void TestRecipientsDeliverables()
		{
			using (DocumentPack pack = new DocumentPack())
			{
				DeliveryInstructions newInstructions = new DeliveryInstructions(pack);
				AssertEquals("Recipients.Documents", pack, newInstructions.Recipients.Deliverables);
			}
		}

		public void TestMostOfficialRecipient()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			AddContactWithDocumentGroup(organisation, "Contact1", ContactType.Warehouse.Code, false);
			AddContactWithDocumentGroup(organisation, "Contact2", ContactType.Warehouse.Code, true);
			AddContactWithDocumentGroup(organisation, "Contact3", ContactType.NotifyParty.Code, false);
			AddContactWithDocumentGroup(organisation, "Contact4", ContactType.NotifyParty.Code, true);
			AddContactWithDocumentGroup(organisation, "Contact5", ContactType.All.Code, false);
			AddContactWithDocumentGroup(organisation, "Contact6", ContactType.All.Code, true);

			AssertRecipients(organisation, ContactType.NotifyParty.Code, @"
Recipients in Order: 
   Contact3 - NOT - Fallback
   Contact4 - NOT - Default
   Contact5 - ALL - Fallback
   Contact6 - ALL - Default
Official Recipient: Contact4
Official Recipient Language: FR-FR");
		}

		void AssertRecipients(OrgHeader organisation, string contactTypeCode, string expectedResult)
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = contactTypeCode;
			DocumentPack documentPack = new DocumentPack(menuItem);
			documentPack.DocumentSupporter = new AutoDeliveryBizO(organisation);
			DeliveryInstructions instructions = new DeliveryInstructions(documentPack);
			instructions.Language = Core.SharedConstants.Languages.French;

			ZStringBuilder results = new ZStringBuilder();
			results.Append("Recipients in Order: ");
			foreach (DocDeliveryContact recipient in instructions.Recipients)
			{
				OrgDocument document = recipient.Contact.Documents[0];
				results.Append("   " + recipient.Name + " - " + document.OD_DocumentGroup + " - " + (document.OD_DefaultContact ? "Default" : "Fallback"));
			}

			results.Append("Official Recipient: " + instructions.OfficialRecipient.Name);
			results.Append("Official Recipient Language: " + instructions.Language);

			AssertMultilineASCIIEquals("Complete Results:", expectedResult.Trim(), results.ToStringWithNewLineBetweenAppends());
		}

		static void AddContactWithDocumentGroup(OrgHeader organisation, string contactName, string docGroup, bool isDefault)
		{
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = docGroup;
			document.OD_DefaultContact = isDefault;
		}

		public void TestOfficialRecipientDoesNotResetAddress()
		{
			var wHSMenuItem = Factory.New<StmMenuItem>();
			wHSMenuItem.SU_ContactType = ContactType.Warehouse.Code;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Company1";
			org.MainAddress.OA_Address1 = "Office";

			OrgAddress postalAddress = org.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
			contact1.OC_ContactName = "Contact1";
			contact1.OC_Fax = "123123";
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			contact1.WorkingAddressPK = postalAddress.PK;

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(wHSMenuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(org);
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("Precondition: 1 auto delivery contact added", 1, instructions.Recipients.Count);
			AssertEquals("Recipient Address respects Working Address", "Postal", instructions.Recipients[0].Address1);

			contact1.WorkingAddressPK = org.MainAddress.PK;
			instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Recipient Address respects Working Address", "Office", instructions.Recipients[0].Address1);
		}

		public void TestHasPrintedDocuments()
		{
			DeliveryInstructions instructions = new DeliveryInstructions(new DocumentPack());
			AssertEquals("No contacts yet, so no printed docs", false, instructions.HasPrintedDocuments);

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			instructions.Recipients.Add(contact1);
			AssertEquals("1 contact with Print Mode", true, instructions.HasPrintedDocuments);
		}

		public void TestCoverNotReadOnly()
		{
			AssertEquals("Cover note not on", false, Instructions.IncludeCoverNote);
			AssertEquals("Cover note readonly", true, Instructions.CoverNoteInfo.ReadOnly);

			Instructions.IncludeCoverNote = true;
			AssertEquals("Cover note editable", false, Instructions.CoverNoteInfo.ReadOnly);
		}

		public void TestFactorySaveStrategyIsNotNullByDefault()
		{
			AssertNotNull("Not null strategy", Instructions.FactorySaveStrategy);
		}

		public void TestNewDeliveryGroupGetsCreatedByGetterButDoesNotSaveFactory()
		{
			AssertNotNull("Not null", Instructions.DeliveryGroups[0]);
			Assert("Should not be in DB", !Instructions.DeliveryGroups[0].IsInDatabase);
		}

		public void TestNewDeliveryGroupIsCreatedInSameFactoryAsDeliveryInstructionFactory()
		{
			AssertNotNull(Instructions.DeliveryGroups[0]);
			AssertEquals("Delivery group should be created using the factory on the delivery instructions", Instructions.Factory, Instructions.DeliveryGroups[0].Factory);
		}

		public void TestSetAndSaveDeliveryGroupSubjectLine()
		{
			Instructions.SetAndSaveDeliveryGroupSubjectLine(DocumentPack.EmptyPack, new PrintTask.ReportSubjectLineMapping(null, "Testing"));
			Assert("Should be in DB", Instructions.DeliveryGroups[0].IsInDatabase);
			AssertEquals("Subject line should be set", "Testing", Instructions.DeliveryGroups[0].SB_EmailSubjectLine);
		}

		public void TestSetAndSaveMultipleDeliveryGroupSubjectLines()
		{
			Instructions.SetAndSaveDeliveryGroupSubjectLine(DocumentPack.EmptyPack, new PrintTask.ReportSubjectLineMapping(null, "Testing1"));
			Instructions.SetAndSaveDeliveryGroupSubjectLine(DocumentPack.EmptyPack, new PrintTask.ReportSubjectLineMapping(null, "Testing2"));
			Assert("Should be in DB", Instructions.DeliveryGroups[0].IsInDatabase);
			Assert("Should be in DB", Instructions.DeliveryGroups[1].IsInDatabase);
			AssertEquals("Subject line should be set", "Testing1", Instructions.DeliveryGroups[0].SB_EmailSubjectLine);
			AssertEquals("Subject line should be set", "Testing2", Instructions.DeliveryGroups[1].SB_EmailSubjectLine);
		}

		public void TestParentForm()
		{
			AssertNull("Instructions.ParentForm should be null by default.", Instructions.ParentForm);

			var form = new Mock<IDeliverCapableForm>();
			Instructions.ParentForm = form.Object;
			AssertEquals(form.Object, Instructions.ParentForm);
		}

		public void TestRunPreSaveValidation_ForRecipients()
		{
			DeliveryInstructions instructions = new DeliveryInstructions(new DocumentPack());
			DocDeliveryContact deliveryContact = instructions.Recipients.AddNew();

			instructions.RunPreSaveValidation();
			AssertEquals("Instructions object has errors from contact", true, instructions.HasErrors);
		}

		public void TestRunPreSaveValidation_ForPrinterDelivery()
		{
			Instructions.Destination = DeliveryInstructionDestination.Print;
			DocDeliveryContact deliveryContact = Instructions.Recipients.AddNew();
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			Instructions.RunPreSaveValidation();
			AssertEquals("PrinterDelivery should be validated on pre-save", true, Instructions.PrinterDelivery.HasErrors);
		}

		public void TestConstructor()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			AssertEquals("Default constructor creates factory save strategy of SaveInChunks", typeof(FactoryStrategy.SaveInChunks), instructions.FactorySaveStrategy.GetType());
			AssertEquals("Covernote info should be readonly", true, instructions.CoverNoteInfo.ReadOnly);

			FactoryStrategy strategy = new FactoryStrategy.PopulateButDoNotSave(Factory);
			instructions = new DeliveryInstructions(strategy);
			AssertEquals("Factory strategy should be the same instance passed in", strategy, instructions.FactorySaveStrategy);
			AssertEquals("Covernote info should be readonly", true, instructions.CoverNoteInfo.ReadOnly);

			DocumentPack pack = new DocumentPack();
			instructions = new DeliveryInstructions(pack);
			Assert("Pack should be set", instructions.DocPackIsSet);
			AssertEquals("Factory strategy should be SaveInChunks", typeof(FactoryStrategy.SaveInChunks), instructions.FactorySaveStrategy.GetType());
			AssertEquals("Covernote info should be readonly", true, instructions.CoverNoteInfo.ReadOnly);
		}

		public void TestDeliveryInstructionsUseSameFactoryAsStrategyWhenItIsPopulateButDoNotSave()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			Assert("by default, GetFactory returns a differnt factory instance to the one on the DeliveryInstructions because the strategy returns a new factory every time",
				instructions.FactorySaveStrategy.GetFactory() != instructions.Factory);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			FactoryStrategy strategy = new FactoryStrategy.PopulateButDoNotSave(newFactory);
			instructions = new DeliveryInstructions(strategy);
			AssertEquals("Factory instance should be the same", strategy.GetFactory(), instructions.Factory);
			AssertEquals("Factory instance should be the same", newFactory, instructions.Factory);
		}

		[ExpectException(typeof(NullReferenceException))]
		public void TestDeliveryInstructionsCantUseNullFactoryStrategy()
		{
			FactoryStrategy strategy = null;
			DeliveryInstructions instructions = new DeliveryInstructions(strategy);
		}

		public void TestUsesDeliveryGroupAuto()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.Auto, true);
		}

		public void TestUsesDeliveryGroupDisk()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.Disk, false);
		}

		public void TestUsesDeliveryGroupDocManager()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.DocManager, true);
		}

		public void TestUsesDeliveryGroupNone()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.None, false);
		}

		public void TestUsesDeliveryGroupPreview()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.Preview, false);
		}

		public void TestUsesDeliveryGroupPrint()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.Print, true);
		}

		public void TestUsesDeliveryGroupSms()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.Sms, true);
		}

		public void TestUsesDeliveryGroupTakenFromContact()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.TakenFromContact, true);
		}

		public void TestUsesDeliveryGroupUserCancelled()
		{
			TestDeliveryGroup(DeliveryInstructionDestination.UserCancelled, false);
		}

		void TestDeliveryGroup(DeliveryInstructionDestination destination, bool expected)
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = destination;
			AssertEquals(expected, instructions.UsesDeliveryGroup);
		}

		public void TestJsonConverter()
		{
			var result = JsonConverterHelper.Serialize(Instructions);
			var deserialisedInstructions = JsonConverterHelper.Deserialize<DeliveryInstructions>(result);

			AssertEquals("CoverNote", "", deserialisedInstructions.CoverNote);
			AssertEquals("IncludeCoverNote", false, deserialisedInstructions.IncludeCoverNote);
			AssertEquals("Language", Core.SharedConstants.Languages.EnglishAmerican, deserialisedInstructions.Language);

			Instructions.CoverNote = "Cover not noted.";
			Instructions.IncludeCoverNote = true;
			Instructions.Language = Core.SharedConstants.Languages.ChineseSimplified;

			result = JsonConverterHelper.Serialize(Instructions);
			deserialisedInstructions = JsonConverterHelper.Deserialize<DeliveryInstructions>(result);

			AssertEquals("CoverNote", "Cover not noted.", deserialisedInstructions.CoverNote);
			AssertEquals("IncludeCoverNote", true, deserialisedInstructions.IncludeCoverNote);
			AssertEquals("Language", Core.SharedConstants.Languages.ChineseSimplified, deserialisedInstructions.Language);
		}

		public void TestClone()
		{
			DeliveryInstructions original = new DeliveryInstructions();
			original.CoverNote = "Test";
			original.IncludeCoverNote = true;
			original.Language = Core.SharedConstants.Languages.French;
			DeliveryInstructions clone = (DeliveryInstructions)original.Clone();
			AssertNotEquals(original, clone);
			AssertEquals(original.CoverNote, clone.CoverNote);
			AssertEquals(original.IncludeCoverNote, clone.IncludeCoverNote);
			AssertEquals(original.Language, clone.Language);
		}

		#region TestNotDeliverableValidation

		public void TestNotDeliverableValidation()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			OrgDocument document = orgHeader.SuppressedDocuments.AddNew();
			StmMenuItem documentMenuItem = Factory.New<StmMenuItem>();
			documentMenuItem.SU_MenuName = "Not Deliverable Document";
			document.OD_SU_MenuItem = documentMenuItem.PK;
			orgHeader.OH_Code = "Org1";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			OrgDocument otherDocument = otherOrg.SuppressedDocuments.AddNew();
			StmMenuItem otherMenuItem = Factory.New<StmMenuItem>();
			otherMenuItem.SU_MenuName = "Some Other Restriction";
			otherDocument.OD_SU_MenuItem = otherMenuItem.PK;
			otherOrg.OH_Code = "Org2";

			Factory.Save();

			DeliveryInstructions instructions = new DeliveryInstructions();
			var deliverable = new DummyDeliverable();
			deliverable.DocumentName = "Human Readable Name";
			deliverable.MenuItem = Factory.New<StmMenuItem>();
			deliverable.MenuItem.SU_MenuName = "Not Deliverable Document";
			instructions.DeliverablesToBePrinted.Add(deliverable);

			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.OrgHeaderPK = orgHeader.PK;

			instructions.RunPreSaveValidation();
			Assert("Validation error should be set.", contact.OrgHeaderPKInfo.HasError("Document 'Human Readable Name' is marked as Never to Deliver to this organization."));

			contact.OrgHeaderPK = otherOrg.PK;

			instructions.RunPreSaveValidation();
			Assert("No validation errors for organization without restrictions.", !contact.OrgHeaderPKInfo.HasErrors());
		}

		public void TestNotDeliverableValidationByDocumentGroup()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			OrgDocument document = orgHeader.SuppressedDocuments.AddNew();
			StmMenuItem documentMenuItem = Factory.New<StmMenuItem>();
			documentMenuItem.SU_MenuName = "Some Menu Item";
			document.OD_DocumentGroup = "XYZ";
			document.OD_SU_MenuItem = documentMenuItem.PK;
			orgHeader.OH_Code = "Org1";

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			OrgDocument otherDocument = otherOrg.SuppressedDocuments.AddNew();
			StmMenuItem otherMenuItem = Factory.New<StmMenuItem>();
			otherMenuItem.SU_MenuName = "Other Menu Item";
			otherDocument.OD_DocumentGroup = "ABC";
			otherDocument.OD_SU_MenuItem = otherMenuItem.PK;
			otherOrg.OH_Code = "Org2";

			Factory.Save();

			DeliveryInstructions instructions = new DeliveryInstructions();
			var deliverable = new DummyDeliverable();
			deliverable.DocumentName = "Human Readable Name";
			deliverable.MenuItem = Factory.New<StmMenuItem>();
			deliverable.MenuItem.SU_MenuName = "Extra Menu Item";
			deliverable.MenuItem.SU_ContactType = "XYZ";
			instructions.DeliverablesToBePrinted.Add(deliverable);

			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.OrgHeaderPK = orgHeader.PK;

			instructions.RunPreSaveValidation();
			Assert("Validation error should be set.", contact.OrgHeaderPKInfo.HasError("Document group 'XYZ' is marked as Never to Deliver to this organization."));

			contact.OrgHeaderPK = otherOrg.PK;

			instructions.RunPreSaveValidation();
			Assert("No validation errors for organization without restrictions.", !contact.OrgHeaderPKInfo.HasErrors());
		}

		#endregion

		public void TestSendToEDocsDefaultsToFalse()
		{
			AssertEquals(false, new DeliveryInstructions().SendToEDocs);
		}

		public void TestLocalLanguageAddress()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.All.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Company1";
			org.MainAddress.OA_Address1 = "Main Address";
			OrgAddress chsAddress = org.Addresses.AddNew();
			chsAddress.OA_Address1 = "主要地址";
			chsAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			chsAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			chsAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			AddContactWithDocumentGroup(org, "Contact", ContactType.All.Code, true);

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(org);
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("Precondition: 1 auto delivery contact added", 1, instructions.Recipients.Count);
			AssertEquals("Main Address", instructions.Recipients[0].Address1);

			org.Contacts[0].WorkingAddressPK = chsAddress.PK;
			instructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("Recipient Delivery Language", Core.SharedConstants.Languages.ChineseSimplified, instructions.Recipients[0].DeliveryLanguage);
			AssertEquals("主要地址", instructions.Recipients[0].Address1);

			org.Contacts[0].WorkingAddressPK = org.MainAddress.PK;
			instructions.Language = Core.SharedConstants.Languages.French;
			AssertEquals("Recipient Delivery Language", Core.SharedConstants.Languages.French, instructions.Recipients[0].DeliveryLanguage);
			AssertEquals("Main Address", instructions.Recipients[0].Address1);
		}

		public void TestLanguage_OfficialRecipient()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.All.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Payables;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Company1";
			org.MainAddress.OA_Address1 = "Main Address";

			OrgAddress engAddress = org.Addresses.AddNew();
			engAddress.OA_Address1 = "EN-US Address";
			engAddress.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			engAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables);
			engAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Payables.Code);

			OrgAddress chsAddress = org.Addresses.AddNew();
			chsAddress.OA_Address1 = "ZH-CN Address";
			chsAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			chsAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			chsAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Payables.Code);

			Factory.Save();

			var task = new PrintTask();
			var pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(org);
			task.Add(pack);

			var instructions = new DeliveryInstructions(pack);
			AssertEquals("EN-US Address", instructions.OfficialRecipient.Address1);

			instructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("Recipient Delivery Language", Core.SharedConstants.Languages.ChineseSimplified, instructions.Recipients[0].DeliveryLanguage);
			AssertEquals("ZH-CN Address", instructions.OfficialRecipient.Address1);

			instructions.Language = Core.SharedConstants.Languages.French;
			AssertEquals("Recipient Delivery Language", Core.SharedConstants.Languages.French, instructions.Recipients[0].DeliveryLanguage);
			AssertEquals("EN-US Address", instructions.OfficialRecipient.Address1);
		}

		public void TestReportsDoNotResetRecipientsWhenLanguageChanged()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Company1";
			org.MainAddress.OA_Address1 = "Main Address";

			Factory.Save();

			var pack = new DocumentPack(reportCommand);
			var instructions = new DeliveryInstructions(pack);
			instructions.AllowAutoDelivery = false;
			var docDeliveryContact = instructions.Recipients.AddNew();
			docDeliveryContact.OrgHeaderPK = org.PK;
			docDeliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			docDeliveryContact.DeliveryAddress = "test@test.com";

			var recipient = instructions.Recipients.Cast<DocDeliveryContact>().FirstOrDefault(x => x.DeliveryMethod == Core.Constants.ContactNotifyModes.Email);
			AssertNotNull(recipient);
			AssertEquals("test@test.com", recipient.DeliveryAddress);

			instructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("test@test.com", recipient.DeliveryAddress);
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
				var deliveryInstructions = new DeliveryInstructions();
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, deliveryInstructions.Language);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.Spanish;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, deliveryInstructions.Language);
				deliveryInstructions.Language = "XXX";
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, deliveryInstructions.Language);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.German;
				AssertEquals(Enterprise.Core.Constants.Languages.German, deliveryInstructions.Language);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.French;
				AssertEquals(Enterprise.Core.Constants.Languages.French, deliveryInstructions.Language);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.EnglishBritish;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishBritish, deliveryInstructions.Language);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.EnglishAmerican;
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, deliveryInstructions.Language);
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
			var deliveryInstructions = new DeliveryInstructions();
			AssertEquals(Enterprise.Core.Constants.Languages.EnglishAmerican, deliveryInstructions.Language);
			AssertNoWarnings(deliveryInstructions.LanguageInfo);
			deliveryInstructions.Language = Enterprise.Core.Constants.Languages.German;
			AssertHasWarning(deliveryInstructions.LanguageInfo, "You have selected to issue this document translated into German.");
			deliveryInstructions.Language = Res.DefaultLanguage;
			AssertNoWarnings(deliveryInstructions.LanguageInfo);
		}

		public void TestDefaultToBritishEnglishWhenAppropriate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var deliveryInstructions = new DeliveryInstructions();
				AssertEquals(Enterprise.Core.Constants.Languages.EnglishBritish, deliveryInstructions.Language);
			}
		}

		public void TestDocumentDeliveryDefaultLanguages()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.Constants.Languages.German;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.OH_Language = Core.Constants.Languages.Dutch;
			branch.GB_OH_OrgProxy = branchProxy.PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var menuItem = Factory.New<StmMenuItem>();
				menuItem.SU_ContactType = ContactType.All.Code;
				menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;

				var pack = new DocumentPack(menuItem) { DocumentSupporter = new AutoDeliveryBizO(org) };

				var instructions1 = new DeliveryInstructions(pack);
				AssertEquals("Language should be English.", (ZString)DataRegistry.Instance.EnglishSpelling, instructions1.Language);
				var resultsCompany = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 1 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsCompany))
				{
					AssertEquals("Language should have been changed to German.", Core.Constants.Languages.German, instructions1.Language);
				}

				var instructions2 = new DeliveryInstructions(pack);
				AssertEquals("Language should be English.", (ZString)DataRegistry.Instance.EnglishSpelling, instructions2.Language);
				var resultsBranch = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 1 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsBranch))
				{
					AssertEquals("Language should have been changed to Dutch.", Core.Constants.Languages.Dutch, instructions2.Language);
				}
			}
		}

		public void TestWrappedObject()
		{
			var instruction1 = new DeliveryInstructions();
			AssertNull(instruction1.WrappedObject);

			var businessObject = Factory.New<Freight.Integration.Agency.IBillOfLading>();
			var documentPack = new DocumentPack(Factory.New<DocumentCommand>(), businessObject as IDocumentSupportable, null, null);
			AssertNotNull(documentPack.BusinessObjectToLogAgainst);

			var instruction2 = new DeliveryInstructions(documentPack);
			AssertEquals(instruction2.WrappedObject, documentPack.BusinessObjectToLogAgainst);
		}

		public void TestDeliveryMethod()
		{
			var instruction = new DeliveryInstructions();
			var deliveryMethod = new DeliveryMethod();
			AssertNull(instruction.DeliveryMethod);

			instruction.DeliveryMethod = deliveryMethod;
			AssertEquals(deliveryMethod, instruction.DeliveryMethod);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDeliveryInfoByFilePath()
		{
			var filePath = Path.Combine(BaseSourcePath, "1.pdf");
			var instruction = new DeliveryInstructions();
			AssertNull(instruction.GetDeliveryInfoByFilePath(filePath));

			var deliveryMethod = DeliveryMethod.FromContact(null, instruction);
			deliveryMethod.AddFile(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) { Name = "1.pdf", FilePath = filePath });
			instruction.DeliveryMethod = deliveryMethod;

			AssertEquals("1.pdf", instruction.GetDeliveryInfoByFilePath(filePath).Name);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliveryInstructionsContextInformation()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			((Forwarding.IForwardingConsol)consol).JK_UniqueConsignRef = "CTEST00001";
			var consolCommand = CreateTemplateForTest(helper, "ConsolTemplate", consol, "ConsolCommand", nameof(Core.Constants.DataContext.Consol));
			consolCommand.SU_ContactType = ContactType.NoContactType.Code;
			CreateCommandDocumentsAndEDocs(helper, consol, consolCommand);
			var shipments = consol.GetType().BaseType.GetField("fShipments", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(consol) as BusinessObjectCollection;
			var shipment1Pk = shipments.ElementAt(0).PK;
			var shipment2Pk = shipments.ElementAt(1).PK;

			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, consolCommand, null);
				loader.LoadAll();
				var instruction = new DeliveryInstructions(printTask[0]);
				using (instruction.ResetEDocsToBeDeliveredIfNeeded())
				{
					instruction.Language = Core.Constants.Languages.ChineseSimplified;
					instruction.PageRangesSpecified = false;
					instruction.SpecifiedPageRangesText = "1-3";
					instruction.PrinterDelivery.NumberOfCopies = 2;
					instruction.IsDraft = false;

					var recipient = instruction.Recipients.AddNew();
					recipient.DeliveryMethod = "EML";
					recipient.DeliveryAddress = "test@test.com";
					recipient.AttachmentType = "PDF";
					recipient.SendIndividually = true;

					var doc1 = instruction.DocumentsToBeDelivered[1];
					doc1.IncludedInPrint = false;

					var edoc1 = instruction.EDocsToBeDelivered[0];
					edoc1.IncludedInPrint = false;

					var context = new SerializableDeliveryInstructions();
					context.SetDeliveryInstructionsContextInformation(instruction);

					AssertEquals(Core.Constants.Languages.ChineseSimplified, context.Language);
					AssertEquals(String.Empty, context.SpecifiedPageRanges);
					AssertEquals(2, context.NumberOfCopies);
					AssertEquals(instruction.PrinterDelivery.PrintQueuePK, context.PrintQueuePK);
					AssertEquals(false, context.IsDraft);
					AssertEquals(instruction.CoverNote, context.CoverNote);
					AssertEquals(instruction.AutoDeliverMultiDocPack, context.AutoDeliverMultiDocPack);
					AssertEquals(instruction.PrintMultiDocPack, context.PrintMultiDocPack);

					var recipients = instruction.Recipients.OfType<DocDeliveryContact>();
					var recipientsInContext = context.Recipients.ToList();

					AssertContainsExactElementsInExactOrder("OrgHeaderPK", recipients.Select(r => r.OrgHeaderPK), recipientsInContext.Select(r => r.OrgHeaderPK));
					AssertContainsExactElementsInExactOrder("DeliveryMethod", recipients.Select(r => r.DeliveryMethod), recipientsInContext.Select(r => r.DeliveryMethod));
					AssertContainsExactElementsInExactOrder("AttachmentType", recipients.Select(r => r.AttachmentType), recipientsInContext.Select(r => r.AttachmentType));
					AssertContainsExactElementsInExactOrder("SendIndividually", recipients.Select(r => r.SendIndividually), recipientsInContext.Select(r => r.SendIndividually));
					AssertContainsExactElementsInExactOrder("DeliveryAddress", recipients.Select(r => r.DeliveryAddress), recipientsInContext.Select(r => r.DeliveryAddress));
					AssertContainsExactElementsInExactOrder("Salutation", recipients.Select(r => r.Salutation), recipientsInContext.Select(r => r.Salutation));
					AssertContainsExactElementsInExactOrder("ContactName", recipients.Select(r => r.Contact?.OC_ContactName ?? ""), recipientsInContext.Select(r => r.ContactName ?? ""));
					AssertContainsExactElementsInExactOrder("SendFrom", recipients.Select(r => r.EmailFromAddressWithType), recipientsInContext.Select(r => r.SendFrom));
					AssertContainsExactElementsInExactOrder("EmailCarbonCopyRecipientsAsString", recipients.Select(r => r.EmailCarbonCopyRecipientsAsString), recipientsInContext.Select(r => r.EmailCarbonCopyRecipientsAsString));
					AssertContainsExactElementsInExactOrder("EmailBlindCarbonCopyRecipientsAsString", recipients.Select(r => r.EmailBlindCarbonCopyRecipientsAsString), recipientsInContext.Select(r => r.EmailBlindCarbonCopyRecipientsAsString));
					AssertContainsExactElementsInExactOrder("DeliveryAddress", recipients.Select(r => r.EmailSubjectMacro), recipientsInContext.Select(r => r.EmailSubjectMacro));

					var deliverailesInContext = context.DocumentsToBeDelivered;

					AssertEquals("when IncludedInPrint is false, should not in context", instruction.DocumentsToBeDelivered.Count - 1, deliverailesInContext.Count());
					AssertEquals("not included doc is doc1", false, deliverailesInContext.Any(d => d.Identifier == doc1.Identifier));

					var includedDeliverables = instruction.DocumentsToBeDelivered.OfType<Report>().Where(d => d.IncludedInPrint);
					AssertContainsExactElementsInExactOrder("Index", includedDeliverables.Select(d => d.Index), deliverailesInContext.Select(d => d.Index));
					AssertContainsExactElementsInExactOrder("PrintQueuePK", includedDeliverables.Select(d => d.PrinterDetails.PrintQueuePK), deliverailesInContext.Select(d => d.PrintQueuePK));
					AssertContainsExactElementsInExactOrder("NumberOfCopies", includedDeliverables.Select(d => d.PrinterDetails.NumberOfCopies), deliverailesInContext.Select(d => d.Copies));
					AssertContainsExactElementsInExactOrder("Identifier", includedDeliverables.Select(d => d.Identifier), deliverailesInContext.Select(d => d.Identifier));
					Assert("Identifier should be unique", includedDeliverables.GroupBy(d => d.Identifier).All(g => g.Count() == 1));

					Assert(includedDeliverables.Any(d => d.IdentifiablePK == shipment1Pk));
					Assert(includedDeliverables.Any(d => d.IdentifiablePK == shipment2Pk));
					Assert(includedDeliverables.Any(d => d.IdentifiablePK == consol.PK));

					var edocsInContext = context.EDocsToBeDelivered;
					AssertEquals("not included doc is edoc1", false, edocsInContext.Any(d => d.Identifier == edoc1.Identifier));

					var edocs = instruction.EDocsToBeDelivered.OfType<IDeliverable>().Where(d => d.IncludeInPrint);

					AssertContainsExactElementsInExactOrder("Index", edocs.Select(e => e.Index), edocsInContext.Select(e => e.Index));
					AssertContainsExactElementsInExactOrder("Identifier", edocs.Select(e => e.Identifier), edocsInContext.Select(e => e.Identifier));
					Assert("Identifier should be unique", edocs.GroupBy(d => d.Identifier).All(g => g.Count() == 1));
				}
			}
		}

		public void TestUpdateDeliveryInstructions()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.AddShipment(shipment);
			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);
			var documentCommand = customization.Menus.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.SU_MenuName = "TestSetDeliveryInstructionsContextInformation";
			documentCommand.SU_BusinessContext = ".DummyBusinessObject";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");

			var stmDocumentDelivery = Factory.New<StmDocumentDelivery>();
			stmDocumentDelivery.SDL_SU = documentCommand.PK;
			stmDocumentDelivery.SDL_ParentId = shipment.PK;
			stmDocumentDelivery.SDL_ParentControllerIdOrTableCode = "JobShipment";
			stmDocumentDelivery.SDL_GS = staff.PK;
			stmDocumentDelivery.SDL_GB = branch.PK;
			stmDocumentDelivery.SDL_GE = department.PK;
			stmDocumentDelivery.SDL_Instructions = @"
{
    ""Language"": ""zh-CN"",
    ""SpecifiedPageRanges"": ""SpecifiedPageRanges"",
    ""NumberOfCopies"": ""2"",
    ""PrintQueuePK"": ""0880e018-318e-4885-bb86-f8bd69a51590"",
    ""IsDraft"": true,
    ""CoverNote"": ""CoverNote"",
    ""AutoDeliverMultiDocPack"": true,
    ""PrintMultiDocPack"": true,
    ""Recipients"": [
        {
            ""OrgHeaderPK"": ""3de09de4-c351-48a4-afea-07bcb61339d7"",
            ""DeliveryMethod"": ""EML"",
            ""AttachmentType"": ""PDF"",
            ""SendIndividually"": ""true"",
            ""DeliveryAddress"": ""DeliveryAddress@test.com"",
            ""Salutation"": ""Salutation"",
            ""ContactName"": ""ContactName"",
            ""SendFrom"": ""SendFrom"",
            ""EmailCarbonCopyRecipientsAsString"": ""EmailCarbonCopyRecipientsAsString"",
            ""EmailBlindCarbonCopyRecipientsAsString"": ""EmailBlindCarbonCopyRecipientsAsString"",
            ""EmailSubjectMacro"": ""EmailSubjectMacro""
        }
    ],
    ""DocumentsToBeDelivered"": [
        {
            ""Index"": ""3"",
            ""PrintQueuePK"": ""c47f56c7-6d63-4ba5-aeec-75cdadbf0403"",
            ""Copies"": ""2"",
            ""Identifier"": """ + getIdentifier(dummy.PK) + @"""
        }
    ],
    ""EDocsToBeDelivered"": [
        {
            ""Index"": ""3"",
            ""Identifier"": """ + getIdentifier(dummy.PK) + @"""
        }
    ]
}";
			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				documentPack.Add(report);
				var serializableDeliveryInstructions = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(stmDocumentDelivery);
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				BackgroundDeliveryHelper.UpdateDeliveryInstructions(stmDocumentDelivery, deliveryInstructions);

				CombineAssertions("deliveryInstructions info", () =>
				{
					AssertEquals("Language", serializableDeliveryInstructions.Language, deliveryInstructions.Language);
					AssertEquals("IsDraft", serializableDeliveryInstructions.IsDraft, deliveryInstructions.IsDraft);
					AssertEquals("PrintQueuePK", serializableDeliveryInstructions.PrintQueuePK, deliveryInstructions.PrinterDelivery.PrintQueuePK);
					AssertEquals("NumberOfCopies", serializableDeliveryInstructions.NumberOfCopies, deliveryInstructions.PrinterDelivery.NumberOfCopies);
					AssertEquals("SpecifiedPageRanges", serializableDeliveryInstructions.SpecifiedPageRanges, deliveryInstructions.SpecifiedPageRangesText);
					AssertEquals("IncludeCoverNote", true, deliveryInstructions.IncludeCoverNote);
					AssertEquals("CoverNote", serializableDeliveryInstructions.CoverNote, deliveryInstructions.CoverNote);
					AssertEquals("PrintMultiDocPack", serializableDeliveryInstructions.PrintMultiDocPack, deliveryInstructions.PrintMultiDocPack);
					AssertEquals("AutoDeliverMultiDocPack", serializableDeliveryInstructions.AutoDeliverMultiDocPack, deliveryInstructions.AutoDeliverMultiDocPack);
				});

				AssertEquals("Recipients count", serializableDeliveryInstructions.Recipients.Count(), deliveryInstructions.Recipients.Count);
				CombineAssertions("Recipient info", () =>
				{
					AssertContainsExactElementsInExactOrder("OrgHeaderPK", serializableDeliveryInstructions.Recipients.Select(r => r.OrgHeaderPK), deliveryInstructions.Recipients.Select(r => r.OrgHeaderPK));
					AssertContainsExactElementsInExactOrder("DeliveryMethod", serializableDeliveryInstructions.Recipients.Select(r => r.DeliveryMethod), deliveryInstructions.Recipients.Select(r => r.DeliveryMethod));
					AssertContainsExactElementsInExactOrder("AttachmentType", serializableDeliveryInstructions.Recipients.Select(r => r.AttachmentType), deliveryInstructions.Recipients.Select(r => r.AttachmentType));
					AssertContainsExactElementsInExactOrder("SendIndividually", serializableDeliveryInstructions.Recipients.Select(r => r.SendIndividually), deliveryInstructions.Recipients.Select(r => r.SendIndividually));
					AssertContainsExactElementsInExactOrder("DeliveryAddress", serializableDeliveryInstructions.Recipients.Select(r => r.DeliveryAddress), deliveryInstructions.Recipients.Select(r => r.DeliveryAddress));
					AssertContainsExactElementsInExactOrder("Salutation", serializableDeliveryInstructions.Recipients.Select(r => r.Salutation), deliveryInstructions.Recipients.Select(r => r.Salutation));
					AssertContainsExactElementsInExactOrder("SendFrom", serializableDeliveryInstructions.Recipients.Select(r => r.SendFrom), deliveryInstructions.Recipients.Select(r => r.EmailFromAddressWithType));
					AssertContainsExactElementsInExactOrder("EmailCarbonCopyRecipientsAsString", serializableDeliveryInstructions.Recipients.Select(r => r.EmailCarbonCopyRecipientsAsString), deliveryInstructions.Recipients.Select(r => r.EmailCarbonCopyRecipientsAsString));
					AssertContainsExactElementsInExactOrder("EmailBlindCarbonCopyRecipientsAsString", serializableDeliveryInstructions.Recipients.Select(r => r.EmailBlindCarbonCopyRecipientsAsString), deliveryInstructions.Recipients.Select(r => r.EmailBlindCarbonCopyRecipientsAsString));
					AssertContainsExactElementsInExactOrder("EmailSubjectMacro", serializableDeliveryInstructions.Recipients.Select(r => r.EmailSubjectMacro), deliveryInstructions.Recipients.Select(r => r.EmailSubjectMacro));
				});

				var documentsToBeDelivered = deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>();
				AssertEquals("DocumentsToBeDelivered count", serializableDeliveryInstructions.DocumentsToBeDelivered.Count(), deliveryInstructions.DocumentsToBeDelivered.Count);
				CombineAssertions("DocumentsToBeDelivered info", () =>
				{
					AssertContainsExactElementsInExactOrder("Index", serializableDeliveryInstructions.DocumentsToBeDelivered.Select(r => r.Index), documentsToBeDelivered.Select(d => d.Index));
					AssertContainsExactElementsInExactOrder("PrintQueuePK", serializableDeliveryInstructions.DocumentsToBeDelivered.Select(r => r.PrintQueuePK), documentsToBeDelivered.Select(d => d.PrinterDetails.PrintQueuePK));
					AssertContainsExactElementsInExactOrder("Copies", serializableDeliveryInstructions.DocumentsToBeDelivered.Select(r => r.Copies), documentsToBeDelivered.Select(d => d.PrinterDetails.NumberOfCopies));
					AssertContainsExactElementsInExactOrder("Identifier", serializableDeliveryInstructions.DocumentsToBeDelivered.Select(r => r.Identifier), documentsToBeDelivered.Select(d => d.Identifier));
					Assert("Identifier should be unique", documentsToBeDelivered.GroupBy(d => d.Identifier).All(g => g.Count() == 1));
				});

				stmDocumentDelivery.SDL_Instructions =  @"
{
    ""Language"": ""zh-CN"",
    ""SpecifiedPageRanges"": ""SpecifiedPageRanges"",
    ""NumberOfCopies"": ""2"",
    ""PrintQueuePK"": ""0880e018-318e-4885-bb86-f8bd69a51590"",
    ""IsDraft"": true,
    ""CoverNote"": """",
    ""AutoDeliverMultiDocPack"": true,
    ""PrintMultiDocPack"": true,
    ""Recipients"": [
        {
            ""OrgHeaderPK"": ""3de09de4-c351-48a4-afea-07bcb61339d7"",
            ""DeliveryMethod"": ""EML"",
            ""AttachmentType"": ""PDF"",
            ""DeliveryAddress"": ""DeliveryAddress@test.com"",
            ""Salutation"": ""Salutation"",
            ""ContactName"": ""ContactName"",
            ""SendFrom"": ""SendFrom"",
            ""EmailCarbonCopyRecipientsAsString"": ""EmailCarbonCopyRecipientsAsString"",
            ""EmailBlindCarbonCopyRecipientsAsString"": ""EmailBlindCarbonCopyRecipientsAsString"",
            ""EmailSubjectMacro"": ""EmailSubjectMacro""
        }
    ],
    ""DocumentsToBeDelivered"": [
        {
            ""Index"": ""3"",
            ""PrintQueuePK"": ""c47f56c7-6d63-4ba5-aeec-75cdadbf0403"",
            ""Copies"": ""2"",
            ""Identifier"": """ + getIdentifier(dummy.PK) + @"""
        }
    ],
    ""EDocsToBeDelivered"": [
        {
            ""Index"": ""3"",
            ""Identifier"": """ + getIdentifier(dummy.PK) + @"""
        }
    ]
}";
				var deliveryInstructions2 = new DeliveryInstructions(documentPack);
				BackgroundDeliveryHelper.UpdateDeliveryInstructions(stmDocumentDelivery, deliveryInstructions2);
				CombineAssertions("deliveryInstructions info - CoverNote", () =>
				{
					AssertEquals("IncludeCoverNote", false, deliveryInstructions2.IncludeCoverNote);
					AssertNullOrEmpty(deliveryInstructions2.CoverNote);
				});
			}

			string getIdentifier(ZGuid sourceBOPK)
			{
				var key = string.Join("+",
					Guid.Empty,
					sourceBOPK,
					"Test",
					""
				);
				using var md5 = MD5.Create();
				return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));
			}
		}

		public void TestDeliveryInstructionsContextInformationAddStaffRecipient()
		{
			var instruction = new DeliveryInstructions();
			var recipient = instruction.Recipients.AddNew();
			recipient.StaffCode = "TTT";

			var context = new SerializableDeliveryInstructions();
			context.SetDeliveryInstructionsContextInformation(instruction);
			var recipientsInContext = context.Recipients.First();
			AssertEquals("TTT", recipientsInContext.StaffCode);
		}

		public void TestDeliveryInstructionsContextInformationAddOrgContactName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "test name";
			Factory.Save();

			var instruction = new DeliveryInstructions();
			var recipient = instruction.Recipients.AddNew();
			recipient.Name = "test name";
			recipient.OrgHeaderPK = orgHeader.PK;

			var context = new SerializableDeliveryInstructions();
			context.SetDeliveryInstructionsContextInformation(instruction);
			var recipientsInContext = context.Recipients.First();
			AssertEquals("test name", recipientsInContext.ContactName);
		}

		public void TestUpdateDeliveryInstructionsStaffRecipient()
		{
			var stmDocumentDelivery = Factory.New<StmDocumentDelivery>();
			stmDocumentDelivery.SDL_Instructions = @"
{
    ""Recipients"": [
        {
            ""ContactName"": ""ContactName"",
        }
    ]
}";

			var deliveryInstructions = new DeliveryInstructions();
			BackgroundDeliveryHelper.UpdateDeliveryInstructions(stmDocumentDelivery, deliveryInstructions);

			var docDeliveryContact = deliveryInstructions.Recipients.First() as DocDeliveryContact;
			AssertEquals("ContactName", docDeliveryContact?.Name);
		}

		public void TestUpdateDeliveryInstructionsContactName()
		{
			var stmDocumentDelivery = Factory.New<StmDocumentDelivery>();
			stmDocumentDelivery.SDL_Instructions = @"
{
    ""Recipients"": [
        {
            ""StaffCode"": ""StaffCode"",
        }
    ]
}";

			var deliveryInstructions = new DeliveryInstructions();
			BackgroundDeliveryHelper.UpdateDeliveryInstructions(stmDocumentDelivery, deliveryInstructions);

			var docDeliveryContact = deliveryInstructions.Recipients.First() as DocDeliveryContact;
			AssertEquals("StaffCode", docDeliveryContact?.StaffCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Instructions = new DeliveryInstructions();
		}
		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		DeliveryInstructions Instructions;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
