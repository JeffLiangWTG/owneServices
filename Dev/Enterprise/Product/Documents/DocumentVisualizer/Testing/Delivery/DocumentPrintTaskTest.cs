using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentPrintTaskTest : TestCaseWithFactory
	{
		#region TestSetNumberOfCopies_SingleDeliverable

		public void TestSetNumberOfCopies_SingleDeliverable()
		{
			const int numberOfCopies = 2;

			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocParent)
			{
				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 1"
						},
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.EML)
							},
							NumberOfCopies = numberOfCopies
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocParent
						},
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact = deliveryInstructions
				.Recipients
				.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			deliveryInstructions.DeliveryGroups.Clear();

			using (var printTask = new DocumentPrintTask())
			{
				printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);

				AssertEquals("Number of copies has been set from print instructions",
					1, deliveryInstructions.PrinterDelivery.NumberOfCopies);
			}
		}

		#endregion

		#region TestSetNumberOfCopies_MultipleDeliverable_SameNumberOfCopies

		public void TestSetNumberOfCopies_MultipleDeliverable_SameNumberOfCopies()
		{
			const int numberOfCopies = 2;

			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocParent)
			{
				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 1"
						},
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.EML)
							},
							NumberOfCopies = numberOfCopies
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocParent
						},
						LogParent = logParent
					},
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 2"
						},
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 2",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.EML)
							},
							NumberOfCopies = numberOfCopies
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocParent
						},
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact = deliveryInstructions
				.Recipients
				.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			deliveryInstructions.DeliveryGroups.Clear();

			using (var printTask = new DocumentPrintTask())
			{
				printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);

				AssertEquals("Number of copies has been set from print instructions",
					1, deliveryInstructions.PrinterDelivery.NumberOfCopies);
			}
		}

		#endregion

		#region TestSetNumberOfCopies_MultipleDeliverable_DifferentNumberOfCopies

		public void TestSetNumberOfCopies_MultipleDeliverable_DifferentNumberOfCopies()
		{
			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocParent)
			{
				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 1"
						},
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.EML)
							},
							NumberOfCopies = 3
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocParent
						},
						LogParent = logParent
					},
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 2"
						},
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 2",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.EML)
							},
							NumberOfCopies = 5
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocParent
						},
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact = deliveryInstructions
				.Recipients
				.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			deliveryInstructions.DeliveryGroups.Clear();

			using (var printTask = new DocumentPrintTask())
			{
				printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);

				AssertEquals("Number of copies could not be determined",
					1, deliveryInstructions.PrinterDelivery.NumberOfCopies);
			}
		}

		#endregion

		#region TestRun

		public void TestRun()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			var document = new DummyDocument { Name = "test doc 1" };

			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocsParent)
			{
				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = document,
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.ALL)
							}
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocsParent
						},
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact = deliveryInstructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			int broadcastCount = 0;
			IDocument broadcastDocumentDelivered = null;
			IStmPrintJob[] broadcastPrintJobs = null;

			var broker = new EventBroker();
			broker.GetEvent<PrintJobsCreatedEvent>().Subscribe(eventData =>
			{
				broadcastCount = broadcastCount + 1;
				broadcastDocumentDelivered = eventData.Document;
				broadcastPrintJobs = eventData.PrintJobs;
			});

			using (var printTask = new DocumentPrintTask(broker))
			{
				printTask.Run(deliveryInstructions);
			}

			var savedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());

			AssertMultilineASCIIEquals("print jobs created",
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : unit.test@cargowise.com
SP_EmailAttachments     : test doc 1.XLS",
				string.Join("\r\n-----\r\n", savedPrintJobs.Select(FormatPrintJob)));

			var deliveryGroups = savedPrintJobs
				.Select(pj => pj.DeliveryGroup)
				.ToArray();

			Assert("All delivery groups have SB_IsProcessed set to true", deliveryGroups.All(dg => dg.SB_IsProcessed));

			AssertEquals("Number of document delivered broadcasts", 1, broadcastCount);
			AssertEquals("Broadcast delivered document", document, broadcastDocumentDelivered);
			AssertMultilineASCIIEquals("broadcast print jobs",
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : unit.test@cargowise.com
SP_EmailAttachments     : test doc 1.XLS",
				string.Join("\r\n-----\r\n", broadcastPrintJobs?.Select(FormatPrintJob) ?? Array.Empty<string>()));
		}

		public void TestRun_MultipleRecipients()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			var document = new DummyDocument { Name = "test doc 1" };

			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocsParent)
			{
				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = document,
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.ALL)
							}
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = false,
							Parent = eDocsParent
						},
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact1 = deliveryInstructions.Recipients.AddNew();
			contact1.DeliveryMethod = "EML";
			contact1.AttachmentType = "PDF";
			contact1.Email = "joe@cargowise.com";

			var contact2 = deliveryInstructions.Recipients.AddNew();
			contact2.DeliveryMethod = "EML";
			contact2.AttachmentType = "PDF";
			contact2.Email = "melinda@cargowise.com";

			int broadcastCount = 0;
			IDocument broadcastDocumentDelivered = null;
			IStmPrintJob[] broadcastPrintJobs = null;

			var broker = new EventBroker();
			broker.GetEvent<PrintJobsCreatedEvent>().Subscribe(eventData =>
			{
				broadcastCount = broadcastCount + 1;
				broadcastDocumentDelivered = eventData.Document;
				broadcastPrintJobs = eventData.PrintJobs;
			});

			using (var printTask = new DocumentPrintTask(broker))
			{
				printTask.Run(deliveryInstructions);
			}

			var savedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery());

			AssertContainsExactElementsInAnyOrder("print jobs created",
new[]
{
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : joe@cargowise.com
SP_EmailAttachments     : test doc 1.XLS",
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : melinda@cargowise.com
SP_EmailAttachments     : test doc 1.XLS"
},
				savedPrintJobs.Select(FormatPrintJob));

			var deliveryGroups = savedPrintJobs
				.Select(pj => pj.DeliveryGroup)
				.ToArray();

			Assert("All delivery groups have SB_IsProcessed set to true", deliveryGroups.All(dg => dg.SB_IsProcessed));

			AssertEquals("Number of document delivered broadcasts", 1, broadcastCount);
			AssertEquals("Broadcast delivered document", document, broadcastDocumentDelivered);
			AssertContainsExactElementsInAnyOrder("broadcast print jobs",
new[]
{
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : joe@cargowise.com
SP_EmailAttachments     : test doc 1.XLS",
@"SP_JobType              : EML
SP_EmailAttachmentFormat: PDF
SP_Destination          : melinda@cargowise.com
SP_EmailAttachments     : test doc 1.XLS"
},
				 broadcastPrintJobs?.Select(FormatPrintJob) ?? Array.Empty<string>());
		}

		public void TestRun_DeliveryOceanCarrierMessagingAsPDF()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveriesForTestRun);

			var contact = deliveryInstructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";
			contact.EmailFromAddress = "no-reply@cargowise.com";

			using (var printTask = new DocumentPrintTask(emailSubject: "[Test]"))
			{
				printTask.Run(deliveryInstructions);
			}

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

			AssertEquals(1, printJobs.Length);
			AssertEquals("[Test]", printJobs[0].SP_EmailSubjectLine);
			AssertEquals("no-reply@cargowise.com", printJobs[0].SP_EmailFromAddress);
		}

		public void TestRun_NewDeliveryGroupFactory()
		{
			var deliveryInstructions = GetDeliveryInstructions(GetDeliveriesForTestRun);

			var contact = deliveryInstructions
				.Recipients
				.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			deliveryInstructions.DeliveryGroups.Clear();

			using (var printTask = new DocumentPrintTask())
			{
				printTask.Run(deliveryInstructions);

				var group = deliveryInstructions.DeliveryGroups.FirstOrDefault();
				AssertNotNull("Should be created in the process of Run.", group);
				AssertEquals("New delivery group created in the same factory.", group.Factory, deliveryInstructions.Factory);
				Assert("delivery group should have SB_IsProcessed set to true", group.SB_IsProcessed);
			}
		}

		IDocumentDelivery[] GetDeliveriesForTestRun(IStmALogParent logParent, object eDocsParent)
		{
			var document = new DummyDocument { Name = "test doc 1" };

			return new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = document,
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.ALL)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
						Parent = eDocsParent
					},
					LogParent = logParent
				},
				new DummyDocumentDelivery
				{
					Document = document,
					DeliveryMode = nameof(PrintCopyType.ALL),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 2",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.ALL)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				}
			};
		}

		#endregion

		#region TestRun_SaveToEDocs

		public void TestRun_SaveToEDocs()
		{
			IDocumentDelivery[] GetDeliveries(IStmALogParent logParent, object eDocsParent)
			{
				var document = new DummyDocument { Name = "test doc 1" };

				return new IDocumentDelivery[]
				{
					new DummyDocumentDelivery
					{
						Document = document,
						DeliveryMode = nameof(PrintCopyType.EML),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 1",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.ALL)
							}
						},
						EDocsInstructions = new DummyEDocsInstructions
						{
							SaveCopyToEDocs = true,
							Parent = eDocsParent
						},
						LogParent = logParent
					},
					new DummyDocumentDelivery
					{
						Document = document,
						DeliveryMode = nameof(PrintCopyType.ALL),
						PrintInstructions = new DummyPrintInstructions
						{
							Title = "title 2",
							DeliveryModes = new[]
							{
								nameof(PrintCopyType.ALL)
							}
						},
						EDocsInstructions = new DummyEDocsInstructions(),
						LogParent = logParent
					}
				};
			}

			var deliveryInstructions = GetDeliveryInstructions(GetDeliveries);

			var contact = deliveryInstructions
				.Recipients
				.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";
			contact.EmailFromAddress = "no-reply@cargowise.com";

			deliveryInstructions.DeliveryGroups.Clear();

			using (var printTask = new DocumentPrintTask())
			{
				printTask.Run(deliveryInstructions);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

				var emlPrintJob = printJobs.First(x => x.SP_JobType == "EML");
				var ddsPrintJob = printJobs.First(x => x.SP_JobType == "DDS");
				var suffix = StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling;

				CombineAssertions("Email print job", () =>
				{
					Assert("delivery group is marked as processed", emlPrintJob.DeliveryGroup.SB_IsProcessed);
					AssertEquals($"test doc 1{suffix}", emlPrintJob.SP_DocumentName);
					AssertEquals("Eagle Datamation International - BN - AUBNE - test doc 1", emlPrintJob.SP_EmailSubjectLine);
					AssertStartsWith("SP_EmailSignature", "Eagle Datamation International", emlPrintJob.SP_EmailSignature);
					AssertEquals("SP_EmailFromAddress", "no-reply@cargowise.com", emlPrintJob.SP_EmailFromAddress);
				});

				CombineAssertions("edoc print job", () =>
				{
					Assert("delivery group is marked as processed", ddsPrintJob.DeliveryGroup.SB_IsProcessed);
					AssertEquals($"test doc 1 (title 1){suffix}", ddsPrintJob.SP_DocumentName);
					AssertEquals("Eagle Datamation International - BN - AUBNE - test doc 1 (title 1)", ddsPrintJob.SP_EmailSubjectLine);
					AssertStartsWith("SP_EmailSignature", "Eagle Datamation International", ddsPrintJob.SP_EmailSignature);
					AssertEquals("SP_EmailFromAddress", "no-reply@cargowise.com", emlPrintJob.SP_EmailFromAddress);
				});
			}
		}

		#endregion

		#region TestRun_EmailSubjectLineFromMacro

		public void TestRun_EmailSubjectLineShouldLessThanOrEqualTo256Chars()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";

			Factory.Save();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_EmailSubjectLine = "Code: <Z0_VarCharMax>";

			var visualizerTemplate = Factory.New<VisualizerTemplate>();
			visualizerTemplate.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = visualizerTemplate.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = new string('1', 257);
			dummy.Z0_Code = "AAA";

			Assert(dummy.Z0_VarCharMax.Length > StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength);

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
						Parent = dummy
					},
					LogParent = logParent
				}
			};

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				documentPack.DocumentSupporter = dummy.DocumentSupporter;
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				var contact = deliveryInstructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Address1 = "Address 1 for test";
				contact.Email = "test@test.com";

				printTask.Run(deliveryInstructions);

				var printJob = Factory.Load<StmPrintJob>(new ZQuery()).FirstOrDefault();
				AssertEquals("default subject line", "ZZZ Comp - Los Branchos - test doc 1", printJob.SP_EmailSubjectLine);
				AssertEquals("from macro: 'Code: <Z0_VarCharMax>'", string.Format("Code: {0}", new string('1', 250)), printJob.DeliveryGroup.SB_EmailSubjectLine);
				AssertEquals(StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength, printJob.DeliveryGroup.SB_EmailSubjectLine.Length);
			}
		}

		public void TestRun_PreviewAndDODAndDelivery()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";

			Factory.Save();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_EmailSubjectLine = "Code: <Z0_Code>";

			var visualizerTemplate = Factory.New<VisualizerTemplate>();
			visualizerTemplate.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = visualizerTemplate.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "AAA";

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
						Parent = dummy
					},
					LogParent = logParent
				}
			};

			var mockPrintTaskUIProvider = new Moq.Mock<IPrintTaskUIProvider>();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				documentPack.DocumentSupporter = dummy.DocumentSupporter;
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.Preview;

				var contact = deliveryInstructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Address1 = "Address 1 for test";
				contact.Email = "test@test.com";

				printTask.Run(deliveryInstructions);

				CleanupStmDeliveryGroup();

				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
			}
		}

		internal void CleanupStmDeliveryGroup()
		{
			var topRowCount = 1000;
			var sql = string.Format(CultureInfo.InvariantCulture, @"
DELETE TOP ({0})
	dbo.StmDeliveryGroup WITH (READPAST, READCOMMITTEDLOCK)
WHERE 1=1
	AND SB_IsProcessed = @isProcessed
  AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmPrintJob
		WHERE SP_SB_DeliveryGroup = SB_PK
			AND SP_SB_DeliveryGroup is NOT NULL
	)
	AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmPrintJob WITH (NOLOCK)
		WHERE SP_SB_DeliveryGroup = SB_PK
			AND SP_SB_DeliveryGroup is NOT NULL
	)
"
				, topRowCount
				);

			if (Db.Connection.TryGetLock("DeleteDeliveryGroups", out var deliveryGroupsMutex))
			{
				using (deliveryGroupsMutex)
				using (var cmd = Db.Connection.Command(sql)) // Requires direct SQL
				{
					cmd.AddParameterBasedOnDbColumn("@isProcessed", true, StmDeliveryGroupSchema.SB_IsProcessed);

					while (cmd.ExecuteNonQuery() == topRowCount)
					{ }
				}
			}
		}

		public void TestRun_EmailSubjectLineFromMacro()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";

			Factory.Save();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_EmailSubjectLine = "Code: <Z0_Code>";

			var visualizerTemplate = Factory.New<VisualizerTemplate>();
			visualizerTemplate.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = visualizerTemplate.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "AAA";

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
						Parent = dummy
					},
					LogParent = logParent
				}
			};

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				documentPack.DocumentSupporter = dummy.DocumentSupporter;
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				var contact = deliveryInstructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Address1 = "Address 1 for test";
				contact.Email = "test@test.com";

				printTask.Run(deliveryInstructions);

				var printJob = Factory.Load<StmPrintJob>(new ZQuery()).FirstOrDefault();
				AssertEquals("default subject line", "ZZZ Comp - Los Branchos - test doc 1", printJob.SP_EmailSubjectLine);
				AssertEquals("from macro: 'Code: <Z0_Code>'", "Code: AAA", printJob.DeliveryGroup.SB_EmailSubjectLine);
			}
		}

		public void TestRun_EmailSubjectLineFromMacro_Override()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";

			Factory.Save();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_EmailSubjectLine = "Code: <Z0_Code>";

			var visualizerTemplate = Factory.New<VisualizerTemplate>();
			visualizerTemplate.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = visualizerTemplate.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "AAA";

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
						Parent = dummy
					},
					LogParent = logParent
				}
			};

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				documentPack.DocumentSupporter = dummy.DocumentSupporter;
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				var contact1 = deliveryInstructions.Recipients[0];
				contact1.DeliveryMethod = "EML";
				contact1.EmailSubjectMacro = "Email: <Address1>";
				contact1.Address1 = "Address 1 for test";
				contact1.Email = "test1@test.com";

				var contact2 = deliveryInstructions.Recipients.AddNew();
				contact2.DeliveryMethod = "EML";
				contact2.Address1 = "Address 1 for test";
				contact2.Email = "test2@test.com";

				var contact3 = deliveryInstructions.Recipients.AddNew();
				contact3.DeliveryMethod = "EML";
				contact3.EmailSubjectMacro = "Email: <Address1>";
				contact3.Address1 = "Address 1 for test";
				contact3.Email = "test3@test.com";

				printTask.Run(deliveryInstructions);

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(3, printJobs.Length);

				var printJob1 = printJobs.First(t => t.SP_Destination == contact1.Email);
				var printJob2 = printJobs.First(t => t.SP_Destination == contact2.Email);
				var printJob3 = printJobs.First(t => t.SP_Destination == contact3.Email);
				AssertEquals("default subject line", "ZZZ Comp - Los Branchos - test doc 1", printJob1.SP_EmailSubjectLine);
				AssertEquals("from macro: 'Email: <Address1>'", "Email: Address 1 for test", printJob1.DeliveryGroup.SB_EmailSubjectLine);
				AssertEquals("default subject line", "ZZZ Comp - Los Branchos - test doc 1", printJob2.SP_EmailSubjectLine);
				AssertEquals("from macro: 'Code: <Z0_Code>'", "Code: AAA", printJob2.DeliveryGroup.SB_EmailSubjectLine);
				AssertEquals("default subject line", "ZZZ Comp - Los Branchos - test doc 1", printJob3.SP_EmailSubjectLine);
				AssertEquals("from macro: 'Email: <Address1>'", "Email: Address 1 for test", printJob3.DeliveryGroup.SB_EmailSubjectLine);
				AssertEquals("the same groups", true, printJob3.DeliveryGroup.PK == printJob1.DeliveryGroup.PK);
				AssertEquals("the different groups", false, printJob2.DeliveryGroup.PK == printJob1.DeliveryGroup.PK);
			}
		}

		#endregion

		#region TestRun_BillOfLadingWithIssueDateUpdated

		[TestDate(2020, 7, 25)]
		public void TestRun_BillOfLadingWithIssueDateUpdated()
		{
			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "AAA";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";

			Factory.Save();

			using (var documentPack = new DocumentPack())
			using (var printTask = new DocumentPrintTask())
			{
				var supportable = Factory.New<DummyDocumentSupportable>();

				var billOfLadingOriginalData = new DummyHouseBill().MakeDocDataDynamic();

				var billOfLadingOriginal = new DummyDocument
				{
					Name = "ORIGINAL",
					DataContext = DataContext.HouseBill,
					Data = billOfLadingOriginalData
				};

				var billOfLadingOriginalPivot = new Mock<IDocumentPivot>();
				billOfLadingOriginalPivot
					.SetupGet(p => p.DocumentTitle)
					.Returns("ORIGINAL");

				var billOfLadingCopyData = new DummyHouseBill().MakeDocDataDynamic();

				var billOfLadingCopy = new DummyDocument
				{
					Name = "COPY",
					DataContext = DataContext.HouseBill,
					Data = billOfLadingCopyData
				};

				var billOfLadingCopyPivot = new Mock<IDocumentPivot>();
				billOfLadingCopyPivot
					.SetupGet(p => p.DocumentTitle)
					.Returns("COPY");

				var documentDeliveries = new[]
				{
					new DummyDocumentDelivery
					{
						Document = billOfLadingOriginal,
						PrintInstructions = new HouseBillPrintInstructions(billOfLadingOriginalPivot.Object, null, shipment),
						EDocsInstructions = new DummyEDocsInstructions(),
						LogParent = logParent
					},
					new DummyDocumentDelivery
					{
						Document = billOfLadingCopy,
						PrintInstructions = new HouseBillPrintInstructions(billOfLadingCopyPivot.Object, null, shipment),
						EDocsInstructions = new DummyEDocsInstructions(),
						LogParent = logParent
					}
				};

				documentPack.DocumentSupporter = supportable.DocumentSupporter;

				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, documentDeliveries)
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};

				var contact1 = deliveryInstructions.Recipients[0];
				contact1.DeliveryMethod = "PRN";
				contact1.EmailSubjectMacro = "Email: <Address1>";
				contact1.Address1 = "Address 1 for test";
				contact1.Email = "test1@test.com";

				foreach (DocumentDeliverable deliverable in deliveryInstructions.DeliverablesToBePrinted)
				{
					deliverable.IncludedInPrint = deliverable.DocumentName.Equals(billOfLadingCopy.Name, StringComparison.OrdinalIgnoreCase);
				}

				AssertEquals("prerequisite: 1 document is to be included for printing",
					1, deliveryInstructions.DeliverablesToBePrinted.Cast<DocumentDeliverable>().Count(d => d.IncludedInPrint));

				printTask.Run(deliveryInstructions);

				AssertEquals("Issue Date updated on ForwardingShipment", ZDateTime.Today, shipment.JS_HouseBillIssueDate);

				CombineAssertions(() =>
				{
					var billOfLadingCopyDateOfIssue = billOfLadingCopyData.GetDynamicProperty(nameof(IHouseBill.DateOfIssue));
					AssertEquals("Issue Date updated on COPY document", ZDateTime.Today, billOfLadingCopyDateOfIssue.Value);
					Assert("Issue Date on COPY document is not marked as having changes", !billOfLadingCopyDateOfIssue.HasChanges);
					Assert("Issue Date on COPY document is not marked as overridden", !billOfLadingCopyDateOfIssue.IsOverridden);
				});

				CombineAssertions(() =>
				{
					var billOfLadingOriginalDateOfIssue = billOfLadingOriginalData.GetDynamicProperty(nameof(IHouseBill.DateOfIssue));
					AssertEquals("Issue Date updated on ORIGINAL document, even though it wasn't delivered", ZDateTime.Today, billOfLadingOriginalDateOfIssue.Value);
					Assert("Issue Date on ORIGINAL document is not marked as having changes", !billOfLadingOriginalDateOfIssue.HasChanges);
					Assert("Issue Date on ORIGINAL document is not marked as overridden", !billOfLadingOriginalDateOfIssue.IsOverridden);
				});
			}
		}

		#endregion

		#region TestDocumentPrintTest_IsDraft_ReadOnly

		public void TestDocumentPrintTest_IsDraft_ReadOnly()
		{
			using (var documentPack = new DocumentPack())
			using (var printTask = new DocumentPrintTask())
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				printTask.Form_DeliveryRequested(this, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
				AssertEquals("deliveryInstructions.IsDraft_ReadOnly", false, deliveryInstructions.IsDraft_ReadOnly);
			}
		}

		#endregion

		#region TestDocumentPrintTest_EventLog

		public void TestDocumentPrintTest_EventLog_Original_NonDraft() => AssertDocumentPrintTest_EventLog("Original", false, "|NAM=dummy|TYP=Original");
		public void TestDocumentPrintTest_EventLog_Original_Draft() => AssertDocumentPrintTest_EventLog("Original", true);

		public void TestDocumentPrintTest_EventLog_Copy_NonDraft() => AssertDocumentPrintTest_EventLog("Copy", false, "|NAM=dummy|TYP=Copy");
		public void TestDocumentPrintTest_EventLog_Copy_Draft() => AssertDocumentPrintTest_EventLog("Copy", true);

		public void TestDocumentPrintTest_EventLog_Dummy_NonDraft() => AssertDocumentPrintTest_EventLog("dummy", false, "|NAM=dummy");
		public void TestDocumentPrintTest_EventLog_Dummy_Draft() => AssertDocumentPrintTest_EventLog("dummy", true);

		void AssertDocumentPrintTest_EventLog(string documentTitle, bool isDraft, string expectedEventReference = null)
		{
			var supporter = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();
			var documentDelivery = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					DeliveryMode = nameof(PrintCopyType.ALL),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = documentTitle,
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.ALL)
						},
						NumberOfCopies = 1
					},
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				}
			};

			var documentPack = new DocumentPack
			{
				DocumentSupporter = supporter.DocumentSupporter
			};

			var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, documentDelivery)
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			var contact = deliveryInstructions.Recipients[0];
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.Email = "unit.test@cargowise.com";

			using (var printTask = new DocumentPrintTask())
			{
				AssertEquals("deliveryInstructions.IsDraft_ReadOnly", false, deliveryInstructions.IsDraft_ReadOnly);
				AssertEquals("deliveryInstructions.IsDraft", false, deliveryInstructions.IsDraft_ReadOnly);

				deliveryInstructions.IsDraft = isDraft;
				printTask.Run(deliveryInstructions);

				var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentDeliveredCode));
				AssertNotNull(logs);

				var documentDeliveredLog = logs
					.OrderBy(l => l.SL_EventTime)
					.LastOrDefault();

				if (isDraft)
				{
					AssertNull("no DDV log should be created for drafts", documentDeliveredLog);
					return;
				}

				AssertNotNull(documentDeliveredLog);
				AssertEquals(expectedEventReference, documentDeliveredLog.SL_Reference);

				var query = new ZQuery(StmPrintJobSchema.SP_DSNTemplate, SQLComparisonOperator.Contains, logParent.PK.ToString());
				var printJob = Factory.Load<StmPrintJob>(query).FirstOrDefault();
				AssertNotNull("found print job with DSN template", printJob);

				var logTemplate = XDocument.Parse(printJob.SP_DSNTemplate)
					.Element("Root")
					.Element("LogTemplate");

				AssertEquals("DDV and DSN template point to the same parent pk",
					documentDeliveredLog.SL_Parent.ToString(), logTemplate.Element("ParentPK").Value);

				AssertEquals("DDV and DSN template point to the same parent table name",
					documentDeliveredLog.SL_Table, logTemplate.Element("ParentTableName").Value);

				var logTemplateParameters = new Dictionary<string, string>();

				foreach (var element in logTemplate.Element("Parameters").Elements())
				{
					var key = element.Element("Type").Value;
					var value = element.Element("Value").Value;
					logTemplateParameters[key] = value;
				}

				AssertEquals("DDV and DSN template have sam number of parameters",
					logTemplateParameters.Count, documentDeliveredLog.Parameters.Count);

				foreach (var key in documentDeliveredLog.Parameters.Keys)
				{
					AssertEquals($"DDV and DSN template {key} parameter are equal",
						documentDeliveredLog.Parameters[key], logTemplateParameters[key]);
				}

				AssertEquals("log parent has been saved in the database", true, logParent.IsInDatabase);
			}
		}

		#endregion

		#region TestDocumentPrintTest_EventLogNotUsingDefault

		public void TestDocumentPrintTest_EventLogNotUsingDefault()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_EmailSubjectLine = "Code: <Z0_VarCharMax>";

			var visualizerTemplate = Factory.New<VisualizerTemplate>();
			visualizerTemplate.SO_Template = TemplateXlsBlob;

			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_DocType = "TST";
			refDocType.RT_Desc = "Test";
			refDocType.RT_SE_NKDocumentReceivedEvent = "BKP";

			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_DocType = "ABC";
			refDocType2.RT_Desc = "Test2";
			refDocType2.RT_SE_NKDocumentReceivedEvent = "XP1";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = visualizerTemplate.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";
			pivot.SI_RT_DocType = refDocType.PK;

			var pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SU = menuItem.PK;
			pivot2.SI_SO = visualizerTemplate.PK;
			pivot2.SI_DocumentTitle = "abracadabra2";
			pivot2.SI_DataStoreName = "7-11";
			pivot2.SI_RT_DocType = refDocType2.PK;

			var logParent = Factory.New<DummyWithLogs>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			dummy.Z0_Code = "AAA";

			Factory.Save();

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 1",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
					},
					LogParent = logParent,
					DocumentType = "TST"
				},
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 2"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "title 2",
						DeliveryModes = new[]
						{
							nameof(PrintCopyType.EML)
						}
					},
					EDocsInstructions = new DummyEDocsInstructions
					{
						SaveCopyToEDocs = false,
					},
					LogParent = logParent,
					DocumentType = "ABC"
				}
			};

			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				documentPack.DocumentSupporter = dummy.DocumentSupporter;
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				var contact = deliveryInstructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Address1 = "Address 1 for test";
				contact.Email = "test@test.com";

				printTask.Run(deliveryInstructions);

				var queryXp1 = new ZQuery(StmALogSchema.SL_SE_NKEvent, "XP1");
				queryXp1.AddToFilter(StmALogSchema.SL_Parent, deliveries.FirstOrDefault().LogParent.LogsParentPK);
				var logs = Factory.Load<StmALog>(queryXp1);
				AssertEquals("1 log with SL_SE_NKEvent XP1 is expected.", 1, logs.Length);

				var queryBkp = new ZQuery(StmALogSchema.SL_SE_NKEvent, "BKP");
				queryBkp.AddToFilter(StmALogSchema.SL_Parent, deliveries.LastOrDefault().LogParent.LogsParentPK);
				var logsBkp = Factory.Load<StmALog>(queryBkp);
				AssertEquals("1 log with SL_SE_NKEvent BKP is expected.", 1, logs.Length);
			}
		}

		#endregion

		#region TestRun_PreventPossibleSelfConcurrencyError

		public void TestRun_PreventPossibleSelfConcurrencyError()
		{
			var billOfLadingMenuItem = GetIAUBillOfLadingMenuItem();
			var original = billOfLadingMenuItem.Documents.Cast<StmMenuTemplatePivotBase>().First(p => p.SI_DocumentTitle.EqualsIgnoringCase("ORIGINAL"));

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";
			((ILightValidationInternals)shipment).IsValid = true;

			var logParent = Factory.New<VisualizerDocumentData>();
			logParent.JDD_Name = original.SI_DataStoreName;
			logParent.JDD_ParentID = shipment.PK;
			logParent.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			ReleaseFactory();

			var originallDocumentPivot = DocumentPivot.Create(new[] { original }).Single();

			var delivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument
				{
					Name = "test doc 1"
				},
				PrintInstructions = new HouseBillPrintInstructions(originallDocumentPivot, null, shipment),
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = logParent
			};

			var documentPack = new DocumentPack(billOfLadingMenuItem, (IDocumentSupportable)shipment, new DocumentEngine.RuntimeOptions.UserControlProviderList(), null);
			documentPack.Factory.NameForDebugging = "DocumentPack Factory";

			var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, new[] { delivery })
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			deliveryInstructions.Factory.NameForDebugging = "DocumentDeliveryInstructions Factory";

			var shipmentLoadedInDocumentPackFactory = documentPack.Factory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			((BusinessObject)shipmentLoadedInDocumentPackFactory).HasChangesChanged += (s, e) =>
			{
				((ILightValidationInternals)shipmentLoadedInDocumentPackFactory).IsValid = false;
			};

			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = "PRN";

			foreach (DocumentDeliverable deliverable in deliveryInstructions.DeliverablesToBePrinted)
			{
				deliverable.IncludedInPrint = true;
			}

			var otherUserFactory = new BusinessObjectFactory();
			otherUserFactory.RefreshEnabled = false;

			var otherUserShipment = otherUserFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			otherUserShipment.JS_GoodsDescription = "updated by another user";

			otherUserFactory.Save();

			AssertEquals("prerequisite: other user changes have not been propagated", ZString.Empty, shipment.JS_GoodsDescription);

			using (var printTask = new DocumentPrintTask())
			{
				printTask.Run(deliveryInstructions);
			}
		}

		DocumentCommand GetIAUBillOfLadingMenuItem() => Factory.Load<DocumentCommand>(new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244"));

		#endregion

		public void TestIsDraft_EmailSubjectMacro()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			var logParent = Factory.New<DummyWithLogs>();

			Factory.Save();

			var deliveries = new IDocumentDelivery[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument
					{
						Name = "test doc 1"
					},
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = new DummyPrintInstructions(),
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent,
					DocumentType = "TST"
				},
			};

			using (var documentPack = new DocumentPack(menuItem))
			using (var printTask = new DocumentPrintTask())
			{
				var deliveryInstructions = new DocumentDeliveryInstructions(Factory, documentPack, deliveries);
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.IsDraft = true;
				var contact = deliveryInstructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Address1 = "Address 1 for test";
				contact.Email = "test@test.com";
				contact.EmailSubjectMacro = "<If(\"<IsDraft>\"==\"Y\",\"DRAFT Arrival Notice\",\"<ReportName>\")>";
				printTask.Run(deliveryInstructions);

				AssertEquals("DRAFT Arrival Notice", deliveryInstructions.DeliveryGroups[0].SB_EmailSubjectLine);
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ZBlob templateXlsBlob;
		ZBlob TemplateXlsBlob
		{
			get
			{
				if (templateXlsBlob == null)
				{
					var tempFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
					templateXlsBlob = StmTemplateBase.GetTemplateBlobFromFile(tempFilePath);
				}
				return templateXlsBlob;
			}
		}

		DocumentDeliveryInstructions GetDeliveryInstructions(Func<IStmALogParent, object, IDocumentDelivery[]> deliveriesProvider)
		{
			var logParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var deliveries = deliveriesProvider(logParent, eDocsParent);

			var deliveryInstructions = new DocumentDeliveryInstructions(Factory, DocumentPack.EmptyPack, deliveries)
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			return deliveryInstructions;
		}

		string FormatPrintJob(IStmPrintJob printJob)
		{
			return
$@"SP_JobType              : {printJob.SP_JobType}
SP_EmailAttachmentFormat: {printJob.SP_EmailAttachmentFormat}
SP_Destination          : {printJob.SP_Destination}
SP_EmailAttachments     : {printJob.SP_EmailAttachments}";
		}

		#endregion
	}

	[UseSnapshotProtection]
	sealed class DocumentPrintTaskSnapshotTest : TestCase {
		[TestDate(2020, 7, 25)]
		public void TestRun_ShipmentWithIssueDateUpdated_Concurrency()
		{
			var factory = new BusinessObjectFactory();
			var billofLadingMenuItem = factory.Load<StmMenuItemBase>(new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244"));
			var pivot = (StmMenuTemplatePivotBase)billofLadingMenuItem.Documents.First();

			var logParent = factory.New<DummyWithLogs>();
			var dummy = factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "AAA";

			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_HouseBillOfLadingType = "IAU";

			factory.Save();

			using (var documentPack = new DocumentPack(billofLadingMenuItem))
			using (var printTask = new DocumentPrintTask())
			{
				var factory2 = new BusinessObjectFactory();
				var pivotOnFactory2 = factory2.Load<StmMenuTemplatePivotBase>(pivot.PK);
				var shipmentOnFactory2 = factory2.Load<Forwarding.IForwardingShipment>(shipment.PK);

				var documentPivot = DocumentPivot.Create(new[] { pivotOnFactory2 }).Single();

				var documentDeliveries = new[]
				{
					new DummyDocumentDelivery
					{
						Document = new DummyDocument
						{
							Name = "test doc 1",
							Data = new DummyHouseBill().MakeDocDataDynamic()
						},
						PrintInstructions = new HouseBillPrintInstructions(documentPivot, null, shipmentOnFactory2),
						EDocsInstructions = new DummyEDocsInstructions(),
						LogParent = logParent
					}
				};

				documentPack.DocumentSupporter = dummy.DocumentSupporter;

				var deliveryInstructions = new DocumentDeliveryInstructions(factory, documentPack, documentDeliveries)
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};

				var contact1 = deliveryInstructions.Recipients[0];
				contact1.DeliveryMethod = "PRN";
				contact1.EmailSubjectMacro = "Email: <Address1>";
				contact1.Address1 = "Address 1 for test";
				contact1.Email = "test1@test.com";

				deliveryInstructions.DeliverablesToBePrinted[0].IncludedInPrint = true;

				var factory3 = new BusinessObjectFactory
				{
					RefreshEnabled = false
				};

				var shipmentOnFactory3 = factory3.Load<Forwarding.IForwardingShipment>(shipment.PK);
				shipmentOnFactory3.JS_RL_NKOrigin = "AUMEL";
				factory3.Save();

				printTask.Run(deliveryInstructions);

				AssertEquals("Issue Date updated", new ZDateTime(2020, 7, 25), shipment.JS_HouseBillIssueDate);
			}
		}
	}
}
