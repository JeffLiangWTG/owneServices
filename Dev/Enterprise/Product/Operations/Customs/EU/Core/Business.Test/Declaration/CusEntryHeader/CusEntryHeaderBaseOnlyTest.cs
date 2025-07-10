using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs.TemporaryStorage;
using EUInterfaces = Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderBaseOnlyTest : CusEntryHeaderTest<CusEntryHeader>
	{
		public void TestUpdateHightestLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetLockNumberOfEntryLinesForRegisteredEntryConfiguration(declaration, configurationValue: true))
				{
					entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingChange;
					AssertEquals("LockNumberOfEntryLines false", (ZShort)0, entryHeader.CH_HighestLineNumber);

					var message = entryHeader.Messages.AddNew();
					message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingReplace;
					AssertEquals("LockNumberOfEntryLines true as IsWaitingForResponse", (ZShort)1, entryHeader.CH_HighestLineNumber);

					message.Delete();
					entryHeader.EntryNumber = "123456";
					entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;
					AssertEquals("LockNumberOfEntryLines true as HasBeenLodgedAtCustoms", (ZShort)1, entryHeader.CH_HighestLineNumber);
				}
			});
		}

		public void TestTotalCustomsQuantity_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.TotalCustomsQuantity), false, x => x.DecimalPlaces == 6);
		}

		public void TestTotalGrossWeightInKG_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.TotalGrossWeightInKG), false, x => x.DecimalPlaces == 6);
		}

		public void TestTotalNetWeightInKG_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.TotalNetWeightInKG), false, x => x.DecimalPlaces == 6);
		}

		public void TestSADDocumentPrinting()
		{
			EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string expectedDocumentType = "EPR";
			const string expectedDocumentName = "SADH C88 - BGMReference";
			const string expectedAttachmentName = "SAD/H for BGMReference for B00001114 .XLSX";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001114";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGMReference";

			Factory.Save();
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader.PK));
			var printJobFound = printJobs.Any(s => s.SP_DocumentType == expectedDocumentType && s.SP_DocumentName.Contains(expectedDocumentName) && s.SP_EmailAttachments == expectedAttachmentName);
			Assert("SAD document print job should not be created", !printJobFound);

			entryHeader.Logs.AddNew(declaration.CustomsClearedEventType);
			Factory.Save();
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader.PK));
			AssertEquals("There should be 1 print job created if all conditions are satisfied for the entry.", 1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("Printed document should be enqueued to eDocs.", "DDS", printJob.SP_JobType);
			AssertEquals("Printed document should have the document type as EPR.", expectedDocumentType, printJob.SP_DocumentType);
			AssertContains("There should be a print job with the expected document name.", expectedDocumentName, printJob.SP_DocumentName);
			AssertEquals("There should be a print job with the expected attachment name.", expectedAttachmentName, printJob.SP_EmailAttachments);

			EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001115";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "BGMReference";
			entryHeader.Logs.AddNew(declaration.CustomsClearedEventType);

			Factory.Save();
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader2.PK));

			printJobFound = printJobs.Any(s => s.SP_DocumentType == expectedDocumentType && s.SP_DocumentName.Contains(expectedDocumentName) && s.SP_EmailAttachments == expectedAttachmentName);
			Assert("SAD document print job should not be created", !printJobFound);
		}

		public void TestNoNestedFactorySave_WhenCallingNewFactoryToPublishCEN()
		{
			var entryHeader = GetEntryHeaderForCENOrCRNEventTest();
			entryHeader.ManuallySet_IsCustomsNumberEnteredEventSupported = false;
			AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: false);
			entryHeader.EntryNumber = "12345";
			Factory.Save();
			AssertEquals("CEN not logged as IsCustomsNumberEnteredEventSupported is forced to be false.", 0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered).Count());

			entryHeader.ManuallySet_IsCustomsNumberEnteredEventSupported = true;
			entryHeader.CH_BGMReference = "UpdateAnyField";
			Factory.Save();
			AssertEquals("CEN can be successfully logged.", 1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered).Count());
			AssertEquals("CEN can be successfully published.", 1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DataExport).Count());
		}

		public void TestEntryNumberType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.CH_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				entryHeader.ManuallySet_IsMrnEntryNumberTheOneWeWantToShow = true;
				AssertEquals("EntryNumberType = MRN", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryHeader.EntryNumberType_Exposed);

				entryHeader.ManuallySet_IsMrnEntryNumberTheOneWeWantToShow = false;
				AssertEquals("EntryNumberType = IMP (CH_MessageType)", MessageTypeList.Codes.Import, entryHeader.EntryNumberType_Exposed);

				entryHeader.CH_MessageType = ZString.Empty;
				AssertEquals("EntryNumberType = EXP (declaration.JE_MessageType)", MessageTypeList.Codes.Export, entryHeader.EntryNumberType_Exposed);
			});
		}

		public void TestNoNestedFactorySave_WhenCallingNewFactoryToPublishCRN()
		{
			var entryHeader = GetEntryHeaderForCENOrCRNEventTest();
			entryHeader.ManuallySet_IsCustomsReleaseNumberEnteredEventSupported = false;
			AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: false);
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Factory.Save();
			AssertEquals("CRN not logged as IsCustomsNumberEnteredEventSupported is forced to be false.", 0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered).Count());

			entryHeader.ManuallySet_IsCustomsReleaseNumberEnteredEventSupported = true;
			entryHeader.CH_BGMReference = "UpdateAnyField";
			Factory.Save();
			AssertEquals("CRN can be successfully logged.", 1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsReleaseNumberEntered).Count());
			AssertEquals("CRN can be successfully published.", 1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DataExport).Count());
		}

		public void TestCENEventPublishedOnSaving_IfAnyEntryLineApplicable()
		{
			CombineAssertions("Event should NOT be published, when no entry lines.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					entryHeader.EntryNumber = "EntryNumber123";
				}, false);
			});

			CombineAssertions("Event should NOT be published, when one entry line with isIntoRegime=Y and isOutOfRegime=Y.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					entryHeader.EntryNumber = "EntryNumber123";
				}, false);
			});

			CombineAssertions("Event should be published, when one entry line with isIntoRegime=N and isOutOfRegime=Y.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: true);
					entryHeader.EntryNumber = "EntryNumber123";
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("Event should be published, when one entry line with isIntoRegime=Y and isOutOfRegime=N.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.EntryNumber = "EntryNumber123";
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("Event should be published, when one entry line with isIntoRegime=N and isOutOfRegime=N.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: false);
					entryHeader.EntryNumber = "EntryNumber123";
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("Event should be published, when multiple entry lines with at least one whose isIntoRegime&isOutOfRegime not true at the same time.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.EntryNumber = "EntryNumber123";
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("RecipientRoleType should be ATW when for import jobs.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					entryHeader.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.EntryNumber = "EntryNumber123";
				}, true, RecipientRoleType.ATW);
			});
		}

		public void TestCRNEventPublishedOnSaving_IfAnyEntryLineApplicable()
		{
			CombineAssertions("Event should NOT be published, when no entry lines.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, false);
			});
			CombineAssertions("Event should NOT be published, when one entry line with isIntoRegime=Y and isOutOfRegime=Y.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, false);
			});
			CombineAssertions("Event should be published, when one entry line with isIntoRegime=N and isOutOfRegime=Y.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: true);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, true, RecipientRoleType.DTW);
			});
			CombineAssertions("Event should be published, when one entry line with isIntoRegime=Y and isOutOfRegime=N.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, true, RecipientRoleType.DTW);
			});
			CombineAssertions("Event should be published, when one entry line with isIntoRegime=N and isOutOfRegime=N.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: false, isOutOfRegime: false);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("Event should be published, when multiple entry lines with at least one whose isIntoRegime&isOutOfRegime not true at the same time.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, true, RecipientRoleType.DTW);
			});

			CombineAssertions("RecipientRoleType should be ATW when for import jobs.", () =>
			{
				AssertDataExportAccordingEntryLinesApplicability(entryHeader =>
				{
					entryHeader.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: true);
					AddEntryLineWithSpecifiedCusProcedure(entryHeader, isIntoRegime: true, isOutOfRegime: false);
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				}, true, RecipientRoleType.ATW);
			});
		}

		void AssertDataExportAccordingEntryLinesApplicability(Action<CusEntryHeader> entryLineSetUpAndFieldUpdate, bool expectDataExport, RecipientRoleType expectRole = default(RecipientRoleType))
		{
			var entryHeader = GetEntryHeaderForCENOrCRNEventTest();
			entryLineSetUpAndFieldUpdate(entryHeader);
			Factory.Save();
			var logsOfDEX = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DataExport);
			AssertEquals(expectDataExport ? 1 : 0, logsOfDEX.Count());
			if (expectDataExport)
			{
				var addedLog = logsOfDEX.First();
				addedLog.AssertLogHasXMLMessage("BGMReference", EDIMessageSubTypeList.Codes.XmlUniversalEvent, expectRole, null, DataContextType.WarehouseCustomsEntry);
			}
		}

		void AddEntryLineWithSpecifiedCusProcedure(CusEntryHeader entry, bool isIntoRegime, bool isOutOfRegime)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("LV");
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure("LV", "AA", "01", "01", "011", "DESC.", "IMP,EXP", "A");
			procedure1.ZZ6_IntoWarehouse = "Y";
			procedure1.ZZ6_OutOfWarehouse = "Y";
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure("LV", "AA", "02", "02", "022", "DESC.", "IMP,EXP", "A");
			procedure2.ZZ6_IntoWarehouse = "Y";
			procedure2.ZZ6_OutOfWarehouse = "N";
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure("LV", "AA", "03", "03", "033", "DESC.", "IMP,EXP", "A");
			procedure3.ZZ6_IntoWarehouse = "N";
			procedure3.ZZ6_OutOfWarehouse = "Y";
			var procedure4 = helper.CreateOrFindExistingRefCusProcedure("LV", "AA", "04", "04", "044", "DESC.", "IMP,EXP", "A");
			procedure4.ZZ6_IntoWarehouse = "N";
			procedure4.ZZ6_OutOfWarehouse = "N";

			var declaration = entry.Declaration;
			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			var validProcedureCode = isIntoRegime && isOutOfRegime ? "0101011" :
						isIntoRegime && !isOutOfRegime ? "0202022" :
						!isIntoRegime && isOutOfRegime ? "0303033" : "0404044";
			if (invoiceLine1.JI_Procedure.IsEmpty)
			{
				invoiceLine1.JI_Procedure = validProcedureCode;
			}
			else
			{
				invoiceLine2.JI_Procedure = validProcedureCode;
			}
		}

		[TestDate(2023, 11, 14, 23, 23, 23)]
		public void TestLogCENEventOnSaving()
		{
			var entryHeader = GetEntryHeaderForCENOrCRNEventTest();

			CombineAssertions("When EntryNumber is never assigned and trigger OnSaving.", () =>
			{
				Factory.Save();
				AssertEquals("Event count:", 0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered).Count());
			});

			CombineAssertions("When EntryNumber is assigned and trigger OnSaving.", () =>
			{
				var releaseDateTime = new ZDateTime(2023, 11, 13, 16, 42, 30);
				entryHeader.EntryNumber = "EntryNumber123";
				entryHeader.CusEntryNumber.CE_IssueDate = releaseDateTime;
				Factory.Save();

				var logsOfCEN = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered);
				AssertEquals("Event count:", 1, logsOfCEN.Count());
				var log = logsOfCEN.First();
				AssertEquals("Event Reference:", "|CRF=EntryNumber123|HBL=HouseBill|IPQ=50|OTY=100", log.SL_Reference);
				AssertEquals("Event DateTime:", releaseDateTime, log.SL_EventTime);
			});

			CombineAssertions("When another OnSaving triggered, no more CEN added as there is already one.", () =>
			{
				Factory.Save();
				var logsOfCEN = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered);
				AssertEquals("Event count:", 1, logsOfCEN.Count());
			});

			CombineAssertions("When EntryNumber is assigned and trigger OnSaving, when CE_IssueDate is empty.", () =>
			{
				var anotherEntryHeader = GetEntryHeaderForCENOrCRNEventTest();
				anotherEntryHeader.EntryNumber = "EntryNumber333";
				anotherEntryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				Factory.Save();

				var logsOfCEN = anotherEntryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsNumberEntered);
				AssertEquals("Event count:", 1, logsOfCEN.Count());
				var log = logsOfCEN.First();
				AssertEquals("Event Reference:", "|CRF=EntryNumber333|HBL=HouseBill|IPQ=50|OTY=100", log.SL_Reference);
				AssertEquals("Event DateTime should be now if CE_IssueDate is empty.", new ZDateTime(2023, 11, 14, 23, 23, 23), log.SL_EventTime);
			});
		}

		[TestDate(2023, 12, 14, 11, 11, 11)]
		public void TestLogCRNEventOnSaving()
		{
			var entryHeader = GetEntryHeaderForCENOrCRNEventTest();

			CombineAssertions("When EntryStatus is never assigned and trigger OnSaving.", () =>
			{
				entryHeader.EntryNumber = "EntryNumber123";
				Factory.Save();
				AssertEquals("Event count:", 0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsReleaseNumberEntered).Count());
			});

			CombineAssertions("When EntryNumber is assigned to CLEAR and trigger OnSaving.", () =>
			{
				var releaseDateTime = new ZDateTime(2023, 12, 13, 13, 4, 20);
				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				entryHeader.CH_EntryReleaseDate = releaseDateTime;
				Factory.Save();

				var logsOfCRN = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsReleaseNumberEntered);
				AssertEquals("Event count:", 1, logsOfCRN.Count());
				var log = logsOfCRN.First();
				AssertEquals("Event Reference:", "|CRF=EntryNumber123|HBL=HouseBill|IPQ=50|OTY=100", log.SL_Reference);
				AssertEquals("Event Date: ", releaseDateTime, log.SL_EventTime);
			});

			CombineAssertions("When another OnSaving triggered, no more CEN added as there is already one.", () =>
			{
				Factory.Save();
				AssertEquals("Event count:", 1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsReleaseNumberEntered).Count());
			});

			CombineAssertions("When EntryNumber is assigned to CLEAR and trigger OnSaving, when CH_EntryReleaseDate is empty.", () =>
			{
				var anotherEntryHeader = GetEntryHeaderForCENOrCRNEventTest();
				anotherEntryHeader.EntryNumber = "EntryNumber333";
				anotherEntryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				Factory.Save();

				var logsOfCRN = anotherEntryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsReleaseNumberEntered);
				AssertEquals("Event count:", 1, logsOfCRN.Count());
				var log = logsOfCRN.First();
				AssertEquals("Event Reference:", "|CRF=EntryNumber333|HBL=HouseBill|IPQ=50|OTY=100", log.SL_Reference);
				AssertEquals("Event DateTime should be now when CH_EntryReleaseDate is empty.", new ZDateTime(2023, 12, 14, 11, 11, 11), log.SL_EventTime);
			});
		}

		CusEntryHeaderForTest GetEntryHeaderForCENOrCRNEventTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_HouseBill = "HouseBill";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_TotalInnerPackages = 50;

			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var package = declaration.Packages.AddNew();
			package.CW_CR_HouseContainer = declaration.PrimaryHouseBill.PackingGroups[0].PK;
			package.CW_PackQty = 100;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var packagePivot = invoiceLine1.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;
			packagePivot.CHC_NumberOfPacks = 100;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_BGMReference = "BGMReference";
			Factory.Save();
			AssertEquals("Prerequisite: entryHeader.PackageCount.", 100, entryHeader.PackagesCount);
			return entryHeader;
		}

		public void TestIsCustomsNumberEnteredEventSupported()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert("IsCustomsNumberEnteredEventSupported is false in EU.", !entryHeader.IsCustomsNumberEnteredEventSupported_Exposed);
		}

		public void TestIsCustomsReleaseNumberEnteredEventSupported()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert("IsCustomsReleaseNumberEnteredEventSupported is false in EU.", !entryHeader.IsCustomsReleaseNumberEnteredEventSupported_Exposed);
		}

		public void TestShouldLogCustomsNumberEnteredEvent()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert("False as EntryNumber is empty.", !entryHeader.ShouldLogCustomsNumberEnteredEvent_Exposed);

			entryHeader.EntryNumber = "11";
			Assert("True as EntryNumber has value and no CEN event exists.", entryHeader.ShouldLogCustomsNumberEnteredEvent_Exposed);
		}

		public void TestShouldLogCustomsReleaseNumberEnteredEvent()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert("False as CH_EntryStatus is empty.", !entryHeader.ShouldLogCustomsReleaseNumberEnteredEvent_Exposed);

			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Assert("True as CH_EntryStatus is Clear and no CRN event exists.", entryHeader.ShouldLogCustomsReleaseNumberEnteredEvent_Exposed);
		}

		public void TestLogMSCEventOnSaving_ShouldLogStatusIsFalse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			entryHeader.CH_Status = "XXX";
			Factory.Save();

			AssertEquals(0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageStatusChange).Count());
		}

		public void TestLogMSCEventOnSaving_ShouldLogStatusIsTrue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			Factory.Save();

			entryHeader.CH_Status = "XXX";
			Factory.Save();

			AssertEquals(1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageStatusChange).Count());
		}

		public void TestLogPSCEventOnSaving_ShouldLogPhaseStatusIsFalse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			entryHeader.CH_PhaseStatus = "XXX";
			Factory.Save();

			AssertEquals(0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.PhaseStatusChange).Count());
		}

		public void TestLogPSCEventOnSaving_ShouldLogPhaseStatusIsTrue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			Factory.Save();

			entryHeader.CH_PhaseStatus = "XXX";
			Factory.Save();

			AssertEquals(1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.PhaseStatusChange).Count());
		}

		public void TestIsMrnEntryNumberTheOneWeWantToShow()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			AssertEquals("IsMrnEntryNumberTheOneWeWantToShow is false.", false, entryHeader.IsMrnEntryNumberTheOneWeWantToShow_Exposed);
		}

		public void TestTotalPaid()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.CH_TotalPaid), false, x
				=> x.Caption == "Total Duties and Taxes Amount" && x.ShortCaption == "Duties + Taxes");
		}

		public void TestTotalNetWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1.JI_NetWeight = 100000.00m;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Hectograms;
			invoiceLine2.JI_NetWeight = 2000.00m;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
			invoiceLine3.JI_NetWeight = 200000000.00m;

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine4.JI_NetWeight = 500.00m;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine4.JI_CL = entryLine2.PK;

			AssertEquals("Total Net Weight must be equal to sum of Merged Entry lines Net Weight", 1000.00m, entryHeader.TotalNetWeightInKG);
		}

		public void TestTotalGrossWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1.JI_Weight = 100000.00m;
			invoiceLine1.JI_CL = entryLine1.PK;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Milligrams;
			invoiceLine2.JI_Weight = 200000000.00m;
			invoiceLine2.JI_CL = entryLine1.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Hectograms;
			invoiceLine3.JI_Weight = 2000.00m;
			invoiceLine3.JI_CL = entryLine2.PK;

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine4.JI_Weight = 500.00m;
			invoiceLine4.JI_CL = entryLine2.PK;

			AssertEquals("Total Gross Weight must be equal to sum of Merged Entry lines gross Weight", 1000.00m, entryHeader.TotalGrossWeightInKG);
		}

		public void TestIsFailedFromTransmission()
		{
			var entry = CusEntryHeaderTestHelper.SetupEntryForFeesTest<CusEntryHeader>(Factory);
			Assert(!entry.IsFailedFromTransmission);
			entry.CH_EntryStatus = MessageStatusList.Codes.FailedFromTransmission;
			Assert(entry.IsFailedFromTransmission);
		}

		public void TestSetAsFailedFromTransmission_True()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryMock = CusEntryHeaderTestHelper.CreateMoqEntryForFeesTest<CusEntryHeader>(Factory);
			entryMock.Protected().Setup<ZBool>("ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction").Returns(true);
			var entryHeader = entryMock.Object;
			entryHeader.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entryHeader);

			entryHeader.SetAsFailedFromTransmission();

			CombineAssertions("Checking Status", () =>
			{
				AssertEquals("CH_Status", MessageStatusList.Codes.FailedFromTransmission, entryHeader.CH_Status);
				AssertEquals("When ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction is true, CH_EntryStatus", MessageStatusList.Codes.FailedFromTransmission, entryHeader.CH_EntryStatus);
				AssertEquals("When ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction is true, JE_EntryStatus", MessageStatusList.Codes.FailedFromTransmission, declaration.JE_EntryStatus);
			});
		}

		public void TestSetAsFailedFromTransmission_False()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryMock = CusEntryHeaderTestHelper.CreateMoqEntryForFeesTest<CusEntryHeader>(Factory);
			entryMock.Protected().Setup<ZBool>("ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction").Returns(false);
			var entryHeader = entryMock.Object;
			entryHeader.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entryHeader);

			entryHeader.SetAsFailedFromTransmission();

			CombineAssertions("Checking Status", () =>
			{
				AssertEquals("CH_Status", MessageStatusList.Codes.FailedFromTransmission, entryHeader.CH_Status);
				AssertEquals("When ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction is false, CH_EntryStatus", "", entryHeader.CH_EntryStatus);
				AssertEquals("When ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction is false, JE_EntryStatus", "", declaration.JE_EntryStatus);
			});
		}

		public void TestAdditionalInfosIsCached()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			var addInfo = dec.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "9002";
			addInfo.CSI_Description = "9002 Desc";
			var data = entry.AdditionalInfos;
			AssertSame(data, entry.AdditionalInfos);
			Factory.InvalidateCachedProperties();
			AssertEquals(false, object.ReferenceEquals(data, entry.AdditionalInfos));
		}

		public void TestSupportingDocumentsIsCached()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			var document = inv.SupportingDocuments.AddNew();
			document.CSI_Code = "9002";
			document.CSI_Description = "9002 Desc";
			var data = entry.SupportingDocuments;
			AssertSame(data, entry.SupportingDocuments);
			Factory.InvalidateCachedProperties();
			AssertEquals(false, object.ReferenceEquals(data, entry.SupportingDocuments));
		}

		public void TestPreviousDocumentsIsCached()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			var document = inv.PreviousDocuments.AddNew();
			document.CSI_Code = "9002";
			document.CSI_Description = "9002 Desc";
			var data = entry.PreviousDocuments;
			AssertSame(data, entry.PreviousDocuments);
			Factory.InvalidateCachedProperties();
			AssertEquals(false, object.ReferenceEquals(data, entry.PreviousDocuments));
		}

		[TestDate(2020, 11, 30, 10, 15, 01)]
		public void TestCH_Status_ReCalculateStatusDetails_CH_EntrySubmittedDate_Empty()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(new ZDateTime(2020, 11, 30, 10, 15, 01), entryHeader.CH_EntrySubmittedDate);
		}

		public void TestCH_Status_ReCalculateStatusDetails_CH_EntrySubmittedDate_Set()
		{
			var submittedDate = new ZDateTime(2020, 11, 29, 8, 14, 12);
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntrySubmittedDate = submittedDate;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(submittedDate, entryHeader.CH_EntrySubmittedDate);
		}

		public void TestCH_Status_ReCalculateStatusDetails_CH_EntryStatus_NotSent()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.NotSent;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
		}

		public void TestCH_Status_ReCalculateStatusDetails_CH_EntryStatus_SentAndInitiallyRejected()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.SentAndInitiallyRejected;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
		}

		public void TestCH_Status_ReCalculateStatusDetails_CH_EntryStatus_FailedFromTransmission()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = MessageStatusList.Codes.FailedFromTransmission;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_EntryStatus);
		}

		public void TestCH_Status_DeriveDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(MessageStatusList.Codes.AwaitingResponse, declaration.JE_EntryStatus);
		}

		public void TestIAddInfoChildOverrideTypeSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			IAddInfoChildOverrideTypeSupporter supporter = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(typeof(CusEUEntryHeader), supporter.AddInfoChildType);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				Factory.New<JobDeclaration>();
				supporter = declaration.CustomsEntryHeaders.AddNew();
				AssertEquals(CargoWise.Application.ObjectFactory.GetType<Integration.Customs.TR.ICusEUEntryHeader>(), supporter.AddInfoChildType);
			}
		}

		public void TestIsFeePaidByBroker_NoEntryFeePaymentPartyUnderstander()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var header = Factory.New<CusEntryHeader>();
			header.CH_JE = declaration.PK;

			AssertEquals("", declaration.JE_PaidBy);
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
			AssertEquals(false, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));
		}

		public void TestIsFeePaidByBroker_WithEntryFeePaymentPartyUnderstander()
		{
			var declaration = Factory.New<JobDeclarationWithEntryFeePaymentPartyUnderstander>();
			var header = Factory.New<CusEntryHeader>();
			header.CH_JE = declaration.PK;

			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));
		}

		public void TestSupplierEoriOfMainOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var uk = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedKingdom);

			var supplier = Factory.New<OrgHeader>();
			supplier.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, uk, "123456789123");
			declaration.JE_OH_Supplier = supplier.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Eori number comes out verbatim", "GB123456789123", entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.RemoveAll();
			supplier.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, uk, "123456789123");
			AssertEquals("Turn number comes out as 000", "GB123456789000", entryHeader.SupplierEoriOfMainOffice);
		}

		public void TestImporterEoriOfMainOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var uk = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedKingdom);

			var importer = Factory.New<OrgHeader>();
			importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, uk, "987654321555");
			declaration.JE_OH_Importer = importer.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Eori number comes out verbatim", "GB987654321555", entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.RemoveAll();
			importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, uk, "987654321555");
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Turn number comes out as 000", "GB987654321000", entryHeader.ImporterEoriOfMainOffice);
		}

		public void TestEntryTypeFriendlyName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "000000";
			entryInstruction.CEI_SubStyle = "13";

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				AssertEquals("ProcedureCodeWithoutConcession is empty", "000000 (EX13)", entryHeader.EntryTypeFriendlyName);

				invoiceLine.JI_Procedure = "4000456";
				AssertEquals("ProcedureCodeWithoutConcession isn't empty", "000000 (EX13 / 4000)", entryHeader.EntryTypeFriendlyName);
			});
		}

		public void TestGetTotalChargeValueFor_WithDeferredFees_onlyCW1Fees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var lvCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(lvCode, "Latvia");
			Factory.Save();

			var a00 = helper.CreateNewOrGetExistingRateType(lvCode, "A00");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(lvCode, "B00");
			helper.LoadOrCreateNewCusRateCode(Factory, "B00", b00.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			AddNewFee(entryLine1, "A00", "X", 111m);
			AddNewFee(entryLine1, "A00", "Y", 222m);
			AddNewFee(entryLine1, "A00", "DEF", 222m);
			AddNewFee(entryLine1, "B00", "X", 333m);
			AddNewFee(entryLine1, "B00", "Y", 444m);
			AddNewFee(entryLine1, "B00", "DEF", 444m);

			AddNewFee(entryLine2, "A00", "X", 1000m);
			AddNewFee(entryLine2, "A00", "Y", 2000m);
			AddNewFee(entryLine2, "A00", "DEF", 2000m);
			AddNewFee(entryLine2, "B00", "X", 3000m);
			AddNewFee(entryLine2, "B00", "Y", 4000m);
			AddNewFee(entryLine2, "B00", "DEF", 4000m);

			var a00Charge = new EntryChargeType(null, "A00", "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");

			var customsChargeEntry = (ICustomsChargeEntry)entry;

			CombineAssertions(() =>
			{
				AssertEquals("Expected TotalChargeValue = 111m + 1000m for A00 X", 1111m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 222m + 2000m for A00 Y", 2222m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 333m + 3000m for B00 X", 3333m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 444m + 4000m for B00 Y", 4444m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 111m + 1000m + 222m + 2000m + 222m + 2000m for A00", 5555m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, ""));
				AssertEquals("Expected TotalChargeValue = 333m + 3000m + 444m + 4000m + 444m + 4000m for B00", 12221m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, ""));
			});
		}

		public void TestGetTotalChargeValueFor_WithDeferredFees_OnlyCUSFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var lvCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(lvCode, "Latvia");
			Factory.Save();

			var a00 = helper.CreateNewOrGetExistingRateType(lvCode, "A00");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(lvCode, "B00");
			helper.LoadOrCreateNewCusRateCode(Factory, "B00", b00.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			AddNewFee(entryLine1, "A00", "X", 111m, true);
			AddNewFee(entryLine1, "A00", "Y", 222m, true);
			AddNewFee(entryLine1, "A00", "DEF", 222m, true);
			AddNewFee(entryLine1, "B00", "X", 333m, true);
			AddNewFee(entryLine1, "B00", "Y", 444m, true);
			AddNewFee(entryLine1, "B00", "DEF", 444m, true);

			AddNewFee(entryLine2, "A00", "X", 1000m, true);
			AddNewFee(entryLine2, "A00", "Y", 2000m, true);
			AddNewFee(entryLine2, "A00", "DEF", 2000m, true);
			AddNewFee(entryLine2, "B00", "X", 3000m, true);
			AddNewFee(entryLine2, "B00", "Y", 4000m, true);
			AddNewFee(entryLine2, "B00", "DEF", 4000m, true);

			var a00Charge = new EntryChargeType(null, "A00", "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");

			var customsChargeEntry = (ICustomsChargeEntry)entry;

			CombineAssertions(() =>
			{
				AssertEquals("Expected TotalChargeValue = 111m + 1000m for A00 X", 1111m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 222m + 2000m for A00 Y", 2222m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 333m + 3000m for B00 X", 3333m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 444m + 4000m for B00 Y", 4444m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 111m + 1000m + 222m + 2000m + 222m + 2000m for A00", 5555m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, ""));
				AssertEquals("Expected TotalChargeValue = 333m + 3000m + 444m + 4000m + 444m + 4000m for B00", 12221m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, ""));
			});
		}

		public void TestGetTotalChargeValueFor_WithDeferredFees_MixedCUSAndCW1Fees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var lvCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(lvCode, "Latvia");
			Factory.Save();

			var a00 = helper.CreateNewOrGetExistingRateType(lvCode, "A00");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(lvCode, "B00");
			helper.LoadOrCreateNewCusRateCode(Factory, "B00", b00.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			AddNewFee(entryLine1, "A00", "X", 111m); // entryline1 not include confirmed fees
			AddNewFee(entryLine1, "A00", "Y", 222m);
			AddNewFee(entryLine1, "A00", "DEF", 222m);
			AddNewFee(entryLine1, "B00", "X", 333m);
			AddNewFee(entryLine1, "B00", "Y", 444m);
			AddNewFee(entryLine1, "B00", "DEF", 444m);

			AddNewFee(entryLine2, "A00", "X", 1000m, true);
			AddNewFee(entryLine2, "A00", "Y", 2000m, true);
			AddNewFee(entryLine2, "A00", "DEF", 3000m, true);
			AddNewFee(entryLine2, "B00", "X", 3000m);  // entryline2 include confirmed fees and CW1 fee
			AddNewFee(entryLine2, "B00", "Y", 4000m, true);
			AddNewFee(entryLine2, "B00", "DEF", 5000m, true);

			var a00Charge = new EntryChargeType(null, "A00", "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");

			var customsChargeEntry = (ICustomsChargeEntry)entry;

			CombineAssertions("Only confirmed fees considered as it include confirmed fees in entry level", () =>
			{
				AssertEquals("Expected TotalChargeValue = 1000m for A00 X", 1000m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 2000m for A00 Y", 2000m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 0m for B00 X", 0m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "X"));
				AssertEquals("Expected TotalChargeValue = 4000m for B00 Y", 4000m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "Y"));
				AssertEquals("Expected TotalChargeValue = 1000m + 2000m + 3000m for A00", 6000m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, ""));
				AssertEquals("Expected TotalChargeValue = 4000m + 5000m for B00", 9000m, customsChargeEntry.GetTotalChargeValueFor(b00Charge, ""));
			});
		}

		public void TestEntryHeaderPackageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 20;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 5;

			var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine3 = entryHeader2.AllEntryLines.AddNew();
			var entryLine4 = entryHeader2.AllEntryLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();

			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			var packing3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0];
			packing3.IsLinked = true;
			packing3.PackQty = 2;

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			var packing4 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[0];
			packing4.IsLinked = true;
			packing4.PackQty = 10;

			AssertEquals(8, entryHeader.PackagesCount);
			AssertEquals(12, entryHeader2.PackagesCount);
		}

		public void TestShouldCalculatePackagesCountBasedOnLinesPackagesPivot()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			AssertEquals("ShouldCalculatePackagesCountBasedOnLinesPackagesPivot", true, entryHeader.ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed);
		}

		public void TestOrganisationsProxiedFromDec()
		{
			JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertNull(entry.Supplier.Organisation);
			AssertNull(entry.Importer.Organisation);
			AssertContains("EDI CUSTOMS BROKERS", entry.DeclarantOrganisation.OH_FullName);
			AssertNull(entry.SellerOrganisation);
			AssertNull(entry.Buyer.Organisation);
			AssertNull(entry.RepresentativeOrganisation);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader import = Factory.New<OrgHeader>();
			OrgHeader declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Daniel's forwarder Ltd";
			OrgAddress declarantAddress = Factory.New<OrgAddress>();
			declarant.Addresses.Add(declarantAddress);
			var seller = Factory.New<OrgHeader>();
			var buyer = Factory.New<OrgHeader>();
			var representative = Factory.New<OrgHeader>();

			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OH_Importer = import.PK;
			dec.JE_OA_SellerAddress = seller.MainAddress.PK;
			dec.BuyerDocAddress.OrganisationPK = buyer.PK;
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			dec.JE_OA_Representative = representative.MainAddress.PK;

			AssertEquals(supplier.PK, entry.Supplier.Organisation.PK);
			AssertEquals(import.PK, entry.Importer.Organisation.PK);
			AssertEquals(entry.DeclarantOrganisation.OH_FullName, "Daniel's forwarder Ltd");
			AssertEquals(seller.PK, entry.SellerOrganisation.PK);
			AssertEquals(buyer.PK, entry.Buyer.Organisation.PK);
			AssertEquals(representative.PK, entry.RepresentativeOrganisation.PK);
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		void AddNewFee(CusEntryLine entryLine, string feeType, string mop, decimal amount, bool confirmed = false)
		{
			var fee = confirmed ? (CusEntryLineFee)entryLine.ConfirmedFees.AddNew() : entryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = mop;
			fee.CF_ChargeType = feeType;
			fee.CF_ChargeAmount = amount;
		}

		public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<EuCommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
		}

		public void TestNoExceptionWhileSavingInDifferentCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
			{
				var declaration = (JobDeclaration)GetNewDeclaration();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();

				AssertNoExceptionThrown(() => Factory.Save());

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.HongKong);

				var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals(1, decLoaded.CustomsEntryHeaders.Count);
				decLoaded.CustomsEntryHeaders[0].HasChanges = true;

				AssertNoExceptionThrown(() => decLoaded.Factory.Save());
			}
		}

		public void TestMovementReferenceNumber_Caption()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("MRN", resStringData.Caption);
		}

		public void TestCH_EntryStatus_Caption()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.CH_EntryStatusInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Entry Status", resStringData.Caption);
		}

		public void TestCH_Status_Caption()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.CH_StatusInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Message Status", resStringData.Caption);
		}

		#region IUcc6ValueProvider

		public void TestIUcc6ValueProvider_IsUCC6()
		{
			var (declaration, entryHeader) = CreateDeclarationAndEntryHeader();
			IUcc6ValueProvider ucc6ValueProvider = entryHeader;

			AssertEquals("Non-UCC6", false, ucc6ValueProvider.IsUCC6);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("UCC6", true, ucc6ValueProvider.IsUCC6);
			}
		}

		public void TestIUcc6ValueProvider_IsExport()
		{
			var (declaration, entryHeader) = CreateDeclarationAndEntryHeader();
			IUcc6ValueProvider ucc6ValueProvider = entryHeader;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsExport", true, ucc6ValueProvider.IsExport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsExport", false, ucc6ValueProvider.IsExport);
		}

		public void TestIUcc6ValueProvider_IsImport()
		{
			var (declaration, entryHeader) = CreateDeclarationAndEntryHeader();
			IUcc6ValueProvider ucc6ValueProvider = entryHeader;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsImport", false, ucc6ValueProvider.IsImport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsImport", true, ucc6ValueProvider.IsImport);
		}

		#endregion

		public void TestGetSupportsBondedWarehousingForEntry()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping("LV");
			var procedure1 = universalReferenceTestDataHelper.CreateOrFindExistingRefCusProcedure("LV", "AA", "01", "01", "011", "DESC.", "IMP,EXP", "A");

			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew() as CusEntryInstruction;
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = helper.Owner.PK;
			instruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = "0101011";
			entryHeader.CH_CEI_Instruction = instruction.PK;

			AssertEquals("ClientIsBondedWarehousing is false, Warehouse2IsBondedWarehousing is false, IntoVATWarehouse is false", false, entryHeader.GetSupportsBondedWarehousingForEntry_Exposed(instruction));

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("ClientIsBondedWarehousing is true, Warehouse2IsBondedWarehousing is false, IntoVATWarehouse is false", false, entryHeader.GetSupportsBondedWarehousingForEntry_Exposed(instruction));

			procedure1.ZZ6_IntoVATWarehouse = "Y";
			AssertEquals("ClientIsBondedWarehousing is true, Warehouse2IsBondedWarehousing is false, IntoVATWarehouse is true", true, entryHeader.GetSupportsBondedWarehousingForEntry_Exposed(instruction));

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("ClientIsBondedWarehousing is false, Warehouse2IsBondedWarehousing is true, IntoVATWarehouse is true", true, entryHeader.GetSupportsBondedWarehousingForEntry_Exposed(instruction));

			procedure1.ZZ6_IntoVATWarehouse = "N";
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("ClientIsBondedWarehousing is true, Warehouse2IsBondedWarehousing is true, IntoVATWarehouse is false", false, entryHeader.GetSupportsBondedWarehousingForEntry_Exposed(instruction));
		}

		#region ConfirmTemporaryStorageGoodsConsumption

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "AAA";

					AssertEquals("CH_EntryStatus is changed to AAA", "AAA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInEntry: ZString.Empty);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "AAA";

					AssertEquals("CH_EntryStatus is changed to AAA", "AAA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInPremises: "9999000005");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "AAA";

					AssertEquals("CH_EntryStatus is changed to AAA", "AAA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_FlagFalse()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldConfirm: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "AAA";

					AssertEquals("CH_EntryStatus is changed to AAA", "AAA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToCancel()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "AAA";

					AssertEquals("CH_EntryStatus is changed to AAA", "AAA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpRefData();
				Factory.Save();

				var oldComment = "Extra Old Comment";

				var orgHeader = SetUpOrgHeader();
				var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);

				var regHeader1 = SetUpTmpRegHeader();

				var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine1.SRL_LineNumber = 1;
				regLine1.SRL_PackageType = "NE";
				var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine2.SRL_LineNumber = 2;
				regLine2.SRL_PackageType = "VQ";

				var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 10, 6);
				regLineTransaction1.SRT_Comments = oldComment;
				var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2);

				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
				var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);
				var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

				var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

				var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine3.SRL_LineNumber = 1;
				regLine3.SRL_PackageType = "AA";
				var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine4.SRL_LineNumber = 3;
				regLine4.SRL_PackageType = "VG";

				var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
				var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("CH_EntryStatus is changed to BBB", "BBB", entryHeader.CH_EntryStatus);

					AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, comment: ExpectedComment + " - " + oldComment);
					AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2);
					AssertEquals("regLineTransaction3 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4);
					AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6);
					AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7);

					AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
					AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

					AssertEquals("regLine3 CustomsStatus is not changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
					AssertEquals("regLine4 CustomsStatus is not changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
					AssertEquals("regHeader2 Status is not changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 2, grossWeight: -2);
				var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 1, grossWeight: -1);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

					var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", RegHeaderReference, 2.0m);
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", RegHeaderReference, 1.0m);
				});
			}

			void AssertWriteOffTransaction(BaseCusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue)
			{
				AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
				AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
				AssertEquals(transactionName + "'s CPL_TransactionDate is Entry Release Date", releaseDate, transaction.CPL_TransactionDate);
				AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference + MRN", string.Format("Write-off TS {0} {1}", expectedReference, MRNCode), transaction.CPL_Comment);
				AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
				AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

					var expectedError = "|RES=Reference reference has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
					AssertEquals("New event in logs", expectedError, entryHeader.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				});
			}
		}

		#region ConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CCC";

					AssertEquals("CH_EntryStatus is changed to CCC", "CCC", entryHeader.CH_EntryStatus);
					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CCC";

					AssertEquals("CH_EntryStatus is changed to CCC", "CCC", entryHeader.CH_EntryStatus);
					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CDA";

					AssertEquals("CH_EntryStatus is changed to CDA", "CDA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CDA";

					AssertEquals("CH_EntryStatus is changed to CDA", "CDA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CDA";

					AssertEquals("CH_EntryStatus is changed to CDA", "CDA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference + "AAA", docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "CDA";

					AssertEquals("CH_EntryStatus is changed to CDA", "CDA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsBBB()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("CH_EntryStatus is changed to BBB", "BBB", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsBBB_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("CH_EntryStatus is changed to BBB", "BBB", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsBBB_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("CH_EntryStatus is changed to BBB", "BBB", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsBBB_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference + "AAA", docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "BBB";

					AssertEquals("CH_EntryStatus is changed to BBB", "BBB", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					entryHeader.CH_EntryStatus = "PDA";

					AssertEquals("CH_EntryStatus is changed to PDA", "PDA", entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					entryHeader.CH_EntryStatus = ZString.Empty;
					AssertEquals("CH_EntryStatus is changed to empty", ZString.Empty, entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not created (empty)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		#endregion

		CusEntryHeader SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry
			, bool shouldConfirm = true, bool shouldGetPreviousDocuments = false, bool shouldFormatDocumentNumber = false, string docReference = RegHeaderReference)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = DeclarationReference;
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.CH_BGMReference = EntryReference;
			entryHeader.MovementReferenceNumberSetter(MRNCode, issueDate);
			entryHeader.CH_EntryReleaseDate = releaseDate;
			entryHeader.ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption = shouldConfirm;
			entryHeader.ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore = EntryReference;
			entryHeader.ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore = InternalReferenceType;
			entryHeader.ManuallySet_TemporaryStorageTransactionCommentPrefix = CommentPrefix;
			entryHeader.ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions = new ZString[] { "AAA" };
			entryHeader.ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions = new ZString[] { "BBB" };
			entryHeader.ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions = new ZString[] { "PDA", ZString.Empty };
			entryHeader.ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation = shouldFormatDocumentNumber;
			entryHeader.ShouldGetPreviousDocumentsDeclaredForImport = shouldGetPreviousDocuments;
			entryHeader.ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods = new ZString[] { "SUM" };
			entryHeader.SetCodeForGetPreviousDocumentsDeclaredForImport = "SUM";
			entryHeader.SetReferenceForGetPreviousDocumentsDeclaredForImport = docReference;
			entryHeader.SetLineNoForGetPreviousDocumentsDeclaredForImport = 1;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.GoodsLocation.Address.AuthorisationNumber = locationInEntry;

			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = locationInPremises;
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			return entryHeader;
		}

		(CusEntryHeader entryHeader, ICusTempStorageRegLineTransaction regLineTransaction, ICusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
			(string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, bool shouldConfirm = true, bool createTransaction = true, bool shouldGetPreviousDocuments = true
			, bool shouldFormatDocumentNumber = false, string regHeaderReference = RegHeaderReference, string docReference = RegHeaderReference)
		{
			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, locationInEntry: locationInEntry, locationInPremises: locationInPremises, shouldConfirm: shouldConfirm, shouldGetPreviousDocuments: shouldGetPreviousDocuments, shouldFormatDocumentNumber: shouldFormatDocumentNumber, docReference: docReference);

			var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
			var regLine = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_CustomsStatus = "OPN";
			regLine.SRL_PackageType = "VQ";
			regLine.SRL_SRH = regHeader.PK;
			var regLineTransaction = (ICusTempStorageRegLineTransaction)null;
			if (createTransaction)
			{
				regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
				regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
				regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
				regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
				regLineTransaction.SRT_SRL = regLine.PK;
			}

			var regLineItem = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine.PK;
			Factory.Save();

			return (entryHeader, regLineTransaction, regLine);
		}

		ICusTempStorageRegLineTransaction SetUpTransaction(ICusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
		{
			var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = transactionType;
			regLineTransaction.SRT_TransactionStatus = transactionStatus;
			regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction.SRT_PackageQty = packageQty;
			regLineTransaction.SRT_GrossWeight = grossWeight;

			if (transactionType == "OBL")
			{
				regLineTransaction.SRT_BondAmount = bondAmount;
			}
			else
			{
				regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
			}

			return regLineTransaction;
		}

		void AssertConfirmedTransaction(ZString transactionName, ICusTempStorageRegLineTransaction transaction, string comment = ExpectedComment)
		{
			AssertEquals(transactionName + "'s SRT_TransactionStatus was changed", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
			AssertEquals(transactionName + "'s SRT_ReferenceType was changed", "MRN", transaction.SRT_ReferenceType);
			AssertEquals(transactionName + "'s SRT_Reference was changed", MRNCode, transaction.SRT_Reference);
			AssertEquals(transactionName + "'s SRT_Comments was changed", comment, transaction.SRT_Comments);
			AssertEquals(transactionName + "'s SRT_TransactionDate was changed", issueDate.ToOffset(), transaction.SRT_TransactionDate);
			AssertEquals(transactionName + "'s SRT_PhysicalInOutDate was changed", releaseDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		}

		EUInterfaces.ICusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = RegHeaderReference)
		{
			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = appCode;
			regHeader.SRH_Reference = reference;

			return regHeader;
		}

		CusGuaranteeHeader SetUpGauranteeForTempStorage(EUInterfaces.ICusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
		{
			var cusGuarantee = Factory.New<CusGuaranteeHeader>();
			cusGuarantee.CPH_Number = "Test1";
			cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
			cusGuarantee.CPH_SubType = "1";
			cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
			cusGuarantee.CPH_Balance = 1000.0m;

			var commonGuarantee = Factory.New<CommonGuarantee>();
			commonGuarantee.PW_BondNumber = "Test1";
			commonGuarantee.PW_ParentID = regHeader.PK;
			commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
			commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

			var guarantee = ((CommonGuarantee)(regHeader.Guarantee)).CusGuarantee;
			var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = RegHeaderReference;
			guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			guaranteeLineTransaction.CPL_TranValue = value;

			guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			return guarantee;
		}

		ICusTempStorageRegLine SetUpRegLine(ICusTempStorageRegHeader regHeader)
		{
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_PackageType = "BX";

			return regLine;
		}

		OrgHeader SetUpOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			return orgHeader;
		}

		#endregion

		#region ReserveTemporaryStorageGoods

		public void TestGetEntryLineDataDeclaredForImportToReserveTSGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();
			var entryLine4 = entryHeader.MergedLines.AddNew();

			CombineAssertions(() =>
			{
				var (listReturned, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
				AssertEquals("Method returns empty enumerable when there are no documents in the declaration", 0, listReturned.Count());
				AssertEquals("Method returns empty string when there are no documents in the declaration", ZString.Empty, messageReturned);

				var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
				var previousDoc1 = invoiceLine1.PreviousDocuments.AddNew();
				previousDoc1.CSI_ReferenceNumber = "Reference";
				previousDoc1.CSI_Code = "BBB";
				invoiceLine1.JI_CL = entryLine1.PK;

				var invoiceLine2 = (JobComInvoiceLine)entryLine2.InvoiceLines.AddNew();
				var previousDoc2 = invoiceLine2.PreviousDocuments.AddNew();
				previousDoc2.CSI_ReferenceNumber = "Reference2";
				previousDoc2.CSI_Code = "SUM";
				invoiceLine2.JI_CL = entryLine2.PK;

				var invoiceLine3 = (JobComInvoiceLine)entryLine3.InvoiceLines.AddNew();
				var previousDoc3 = invoiceLine3.PreviousDocuments.AddNew();
				previousDoc3.CSI_ReferenceNumber = "Reference3";
				previousDoc3.CSI_Code = "AAA";
				invoiceLine3.JI_CL = entryLine3.PK;

				var invoiceLine4 = (JobComInvoiceLine)entryLine4.InvoiceLines.AddNew();
				var previousDoc4 = invoiceLine4.PreviousDocuments.AddNew();
				previousDoc4.CSI_ReferenceNumber = "Reference";
				previousDoc4.CSI_Code = "SUM";
				invoiceLine4.JI_CL = entryLine4.PK;

				(listReturned, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
				AssertEquals("Method returns empty enumerable even when there are documents in the declaration", 0, listReturned.Count());
				AssertEquals("Method returns empty string even when there are documents in the declaration", ZString.Empty, messageReturned);
			});
		}

		public void TestTemporaryStorageTransactionInternalReferenceNumber_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.CH_BGMReference = EntryReference;

				AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is always set to empty", ZString.Empty, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			});
		}

		#endregion

		const string RegHeaderReference = "reference";
		const string EntryReference = "ES00001";
		const string InternalReferenceType = "OTH";
		const string MRNCode = "20ES00999930006184";
		const string LocationInEntry = "9999000002";
		const string CommentPrefix = "Prefix";
		const string DeclarationReference = "B00000001";
		const string ExpectedComment = CommentPrefix + " " + DeclarationReference;
		readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);
		readonly ZDateTime releaseDate = new ZDateTime(2024, 06, 14, 11, 11, 11);

		(JobDeclaration, CusEntryHeader) CreateDeclarationAndEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return (declaration, entryHeader);
		}

		internal sealed class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				ManuallySet_IsCustomsNumberEnteredEventSupported = true;
				ManuallySet_IsCustomsReleaseNumberEnteredEventSupported = true;
				ManuallySet_IsMrnEntryNumberTheOneWeWantToShow = true;
			}

			public bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed => ShouldCalculatePackagesCountBasedOnLinesPackagesPivot;

			protected override bool IsCustomsNumberEnteredEventSupported => ManuallySet_IsCustomsNumberEnteredEventSupported;

			public bool ManuallySet_IsCustomsNumberEnteredEventSupported { private get; set; }

			protected override bool IsCustomsReleaseNumberEnteredEventSupported => ManuallySet_IsCustomsReleaseNumberEnteredEventSupported;

			public bool ManuallySet_IsCustomsReleaseNumberEnteredEventSupported { private get; set; }

			public bool IsCustomsNumberEnteredEventSupported_Exposed => base.IsCustomsNumberEnteredEventSupported;

			public bool IsCustomsReleaseNumberEnteredEventSupported_Exposed => base.IsCustomsReleaseNumberEnteredEventSupported;

			public bool ShouldLogCustomsNumberEnteredEvent_Exposed => base.ShouldLogCustomsNumberEnteredEvent;

			public bool ShouldLogCustomsReleaseNumberEnteredEvent_Exposed => base.ShouldLogCustomsReleaseNumberEnteredEvent;

			public bool ManuallySet_IsMrnEntryNumberTheOneWeWantToShow { private get; set; }

			protected override bool IsMrnEntryNumberTheOneWeWantToShow => ManuallySet_IsMrnEntryNumberTheOneWeWantToShow;

			public bool IsMrnEntryNumberTheOneWeWantToShow_Exposed => base.IsMrnEntryNumberTheOneWeWantToShow;

			public ZString EntryNumberType_Exposed => base.EntryNumberType;

			protected override bool ShouldLogStatus => true;

			protected override bool ShouldLogPhaseStatus => true;

			protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore() => ManuallySet_GetEntryLineDataDeclaredToReserveTSGoodsCore();
			public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) ManuallySet_GetEntryLineDataDeclaredToReserveTSGoodsCore()
			{
				if (ShouldGetPreviousDocumentsDeclaredForImport)
				{
					var doc = Factory.New<PreviousDocument>();
					doc.CSI_Code = SetCodeForGetPreviousDocumentsDeclaredForImport;
					doc.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclaredForImport;
					doc.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclaredForImport;
					var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
					{
						new ()
						{
							Document = doc,
							Packages = new (ZString, ZInt, ZString, ZBool)[]
							{
								("FR", 1, "VIN1", false),
								("BX", 8, "", false),
								("VQ", 0, "", true),
							},
							TotalGrossWeight = 30.6m,
							TotalGrossWeightForVINs = 0m,
						}
					};

					if (ShouldHaveSecondPreviousDocumentDeclaredForImport)
					{
						var doc2 = Factory.New<PreviousDocument>();
						doc2.CSI_Code = SetCodeForGetPreviousDocumentsDeclaredForImport;
						doc2.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclaredForImport2;
						doc2.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclaredForImport;
						dataToReturn.Add(
							new DeclarationDataToReserveTSGoods()
							{
								Document = doc2,
								Packages = new (ZString, ZInt, ZString, ZBool)[]
								{
									("VQ", 1, "", true),
								},
								TotalGrossWeight = 20m
							});
					}

					return (dataToReturn, ZString.Empty);
				}
				else
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);
				}
			}

			public ZBool ShouldGetPreviousDocumentsDeclaredForImport { private get; set; }
			public ZBool ShouldHaveSecondPreviousDocumentDeclaredForImport { private get; set; }
			public ZString SetCodeForGetPreviousDocumentsDeclaredForImport { private get; set; }
			public ZString SetReferenceForGetPreviousDocumentsDeclaredForImport { private get; set; }
			public ZString SetReferenceForGetPreviousDocumentsDeclaredForImport2 { private get; set; }
			public ZInt SetLineNoForGetPreviousDocumentsDeclaredForImport { private get; set; }

			protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption;

			public bool ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption { private get; set; }

			protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore;

			public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore { private get; set; }

			protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore;

			public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore { private get; set; }

			protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions;

			public ZString[] ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions { private get; set; }

			protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions;

			public ZString[] ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions { private get; set; }

			protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions;

			public ZString[] ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions { private get; set; }

			protected override IReadOnlyList<ZString> PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods;

			public ZString[] ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods { private get; set; }

			protected override ZString TemporaryStorageTransactionCommentPrefix => ManuallySet_TemporaryStorageTransactionCommentPrefix;

			public ZString ManuallySet_TemporaryStorageTransactionCommentPrefix { private get; set; }

			protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation;

			public ZBool ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation { private get; set; }

			protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN + "AAA";

			public bool GetSupportsBondedWarehousingForEntry_Exposed(Customs.Business.CusEntryInstruction entryInstruction) => base.GetSupportsBondedWarehousingForEntry(entryInstruction);

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				if (!IsThisEntryIsBeingSavedByAnotherFactory())
				{
					MarkThisEntryIsUnderSaving_UsingAPersistentNote();
				}
				else
				{
					HtmlFail("Infinite invocation of Factory.Save() is happening!");
				}
				base.OnFactorySavingBeforeTransactionCore();
				MarkThisEntryIsSaved_ByRemovingTheNote();
			}

			bool IsThisEntryIsBeingSavedByAnotherFactory()
			{
				var newFactory = new BusinessObjectFactory();
				return newFactory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, this.PK)).FirstOrDefault() != null;
			}

			void MarkThisEntryIsUnderSaving_UsingAPersistentNote()
			{
				var newFactory = new BusinessObjectFactory();
				var note = newFactory.New<StmNote>();
				note.ST_ParentID = this.PK;
				note.ST_Table = nameof(CusEntryHeaderSchema.Constants.TableName);
				note.ST_Description = "Marks related entry is under process of saving.";
				newFactory.Save();
			}

			void MarkThisEntryIsSaved_ByRemovingTheNote()
			{
				var newFactory = new BusinessObjectFactory();
				var note = newFactory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, this.PK)).FirstOrDefault();
				note.Delete();
				newFactory.Save();
			}
		}

		sealed class JobDeclarationWithEntryFeePaymentPartyUnderstander : JobDeclaration
		{
			public JobDeclarationWithEntryFeePaymentPartyUnderstander(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(CusEntryHeader header) => new EntryFeePaymentPartyUnderstander(this);
		}
	}
}
