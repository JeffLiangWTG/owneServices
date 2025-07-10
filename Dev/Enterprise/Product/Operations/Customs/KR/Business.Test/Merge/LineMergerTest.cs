using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestCreationOfEntryAndEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(entryInstruction.PK, entry.CH_CEI_Instruction);
			AssertEquals(JobMessageTypeList.Codes.Export, entry.CH_MessageType);

			entry.EntryNumber = "2";
			AssertEquals(JobMessageTypeList.Codes.Export, entry.CusEntryNumber.CE_EntryType);
			AssertEquals(1, entry.MergedLines.Count);

			var entryLine = entry.MergedLines[0];
			AssertEquals(invoiceLine.JI_CL, entryLine.PK);
			AssertEquals(1, entryLine.InvoiceLines.Count);
		}

		public void TestLinePackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_PackType = "CT";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_PackType = "CT";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);

			var entryLine1 = entry.MergedLines[0];
			AssertEquals(invoiceLine1.JI_CL, entryLine1.PK);
			AssertEquals(invoiceLine2.JI_CL, entryLine1.PK);
			AssertEquals(2, entryLine1.InvoiceLines.Count);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_PackType = "BA";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(2, entryLine1.InvoiceLines.Count);

			var entryLine2 = entry.MergedLines[1];
			AssertEquals(invoiceLine3.JI_CL, entryLine2.PK);
			AssertEquals(1, entryLine2.InvoiceLines.Count);
		}

		public void TestInvoiceLineSequenceNumberIsManagedByMessageType()
		{
			AssertInvoiceLineSequenceNumberIsManagedByMessageType(KRJobMessageTypeList.Codes.Export);
			AssertInvoiceLineSequenceNumberIsManagedByMessageType(KRJobMessageTypeList.Codes.Import);
			AssertInvoiceLineSequenceNumberIsManagedByMessageType(KRJobMessageTypeList.Codes.LocalExport);
		}

		void AssertInvoiceLineSequenceNumberIsManagedByMessageType(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LineNo = 2;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LineNo = 3;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			CreateOriginalMessageSnapshot(entry, messageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			if (MessageTypesManagingSequenceNumber.Contains(messageType))
			{
				AssertEquals((short)1, entry.CH_HighestLineNumber);
				AssertEquals((short)3, invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			}
			else
			{
				AssertEquals((short)3, entry.CH_HighestLineNumber);
				AssertEquals((short)0, invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			}

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LineNo = 4;

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "1";
			invoiceLine5.JI_LineNo = 5;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			if (MessageTypesManagingSequenceNumber.Contains(messageType))
			{
				AssertEquals((short)4, invoiceLine4.JI_SequenceNumber);
				AssertEquals((short)5, invoiceLine5.JI_SequenceNumber);

				invoiceLine4.JI_LineNo = 5;
				invoiceLine5.JI_LineNo = 4;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
				AssertEquals((short)5, invoiceLine4.JI_SequenceNumber);
				AssertEquals((short)4, invoiceLine5.JI_SequenceNumber);
			}
			else
			{
				AssertEquals((short)0, invoiceLine4.JI_SequenceNumber);
				AssertEquals((short)0, invoiceLine5.JI_SequenceNumber);
			}
		}

		public void TestLineNumbers()
		{
			AssertLineNumbers(KRJobMessageTypeList.Codes.Export);
			AssertLineNumbers(KRJobMessageTypeList.Codes.Import);
		}

		void AssertLineNumbers(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";
			invoiceLine3.JI_LineNo = 3;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(3, entry.MergedLines.Count);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)3, invoiceLine3.CusEntryLine.CL_LineNumber);

			AssertEquals((short)1, invoiceLine.JI_SequenceNumber);
			AssertEquals((short)1, invoiceLine2.JI_SequenceNumber);
			AssertEquals((short)1, invoiceLine3.JI_SequenceNumber);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, invoiceLine2.JI_SequenceNumber);
			AssertEquals((short)2, invoiceLine3.JI_SequenceNumber);
		}

		public void TestWhenOriginalHasBeenAcceptedAndChangeIsMade()
		{
			AssertWhenOriginalHasBeenAcceptedAndChangeIsMade(KRJobMessageTypeList.Codes.Export);
			AssertWhenOriginalHasBeenAcceptedAndChangeIsMade(KRJobMessageTypeList.Codes.Import);
		}

		void AssertWhenOriginalHasBeenAcceptedAndChangeIsMade(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";
			invoiceLine3.JI_LineNo = 3;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals((short)1, invoiceLine.JI_SequenceNumber);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals((short)1, invoiceLine2.JI_SequenceNumber);
			AssertEquals((short)2, invoiceLine3.JI_SequenceNumber);

			AssertEquals((short)0, entry.CH_HighestLineNumber);
			AssertEquals((short)0, invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			AssertEquals((short)0, invoiceLine2.CusEntryLine.KR_HighestInvoiceLineSequenceNo);

			entry.CH_MessageType = messageType;
			CreateOriginalMessageSnapshot(entry, messageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals((short)2, entry.CH_HighestLineNumber);
			AssertEquals((short)1, invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.KR_HighestInvoiceLineSequenceNo);

			invoiceLine3.JI_Tariff = "3";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(3, entry.MergedLines.Count);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals((short)1, invoiceLine.JI_SequenceNumber);
			AssertEquals((short)1, invoiceLine2.JI_SequenceNumber);
			AssertEquals((short)1, invoiceLine3.JI_SequenceNumber);

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2";
			invoiceLine4.JI_LineNo = 4;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals("PreCondition", invoiceLine2.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertEquals((short)3, invoiceLine4.JI_SequenceNumber);

			entry.CH_MessageType = messageType;
			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals((short)3, entry.CH_HighestLineNumber);
			AssertEquals((short)1, invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			AssertEquals((short)3, invoiceLine2.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
			AssertEquals((short)1, invoiceLine3.CusEntryLine.KR_HighestInvoiceLineSequenceNo);
		}

		public void TestCusContainerEntryHeaderPivotIsUsed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234567";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX1234568";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = IncotermList.Codes.CostInsuranceAndFreight;

			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;
			var containerPivot1 = invoiceLine.ContainersPivot.AddNew();
			containerPivot1.C2_CO = container1.PK;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;
			var containerPivot2 = invoiceLine2.ContainersPivot.AddNew();
			containerPivot2.C2_CO = container2.PK;

			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";
			invoiceLine3.JI_LineNo = 3;
			var containerPivot3 = invoiceLine3.ContainersPivot.AddNew();
			containerPivot3.C2_CO = container2.PK;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			var entryHeader1 = invoiceLine.CusEntryLine.Header;
			var entryHeader2 = invoiceLine3.CusEntryLine.Header;
			AssertNotEquals(entryHeader1, entryHeader2);

			AssertEquals(2, entryHeader1.PivotsToContainers.Count);

			var pivotContainer1EntryHeader1 = entryHeader1.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().First(x => x.CCE_CO_Container == container1.PK);
			AssertEquals((short)1, pivotContainer1EntryHeader1.CCE_SequenceNumber);

			var pivotContainer2EntryHeader1 = entryHeader1.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().First(x => x.CCE_CO_Container == container2.PK);
			AssertEquals((short)2, pivotContainer2EntryHeader1.CCE_SequenceNumber);

			AssertEquals(1, entryHeader2.PivotsToContainers.Count);

			var pivotContainer2EntryHeader2 = entryHeader2.PivotsToContainers[0];
			AssertEquals((short)1, pivotContainer2EntryHeader2.CCE_SequenceNumber);

			AssertEquals((short)0, entryHeader1.CH_HighestContainerNumber);
			AssertEquals((short)0, entryHeader2.CH_HighestContainerNumber);

			var header = new ExportEntryHeaderCreator().Create(entryHeader1);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader1, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
			entryHeader1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals((short)2, entryHeader1.CH_HighestContainerNumber);
			AssertEquals((short)0, entryHeader2.CH_HighestContainerNumber);

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "3";
			invoiceLine4.JI_LineNo = 4;
			var containerPivot4 = invoiceLine4.ContainersPivot.AddNew();
			containerPivot4.C2_CO = container1.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(2, entryHeader2.PivotsToContainers.Count);
			AssertEquals("They are ordered on CusContainer.CO_ContainerNumber", (short)1, entryHeader2.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().First(x => x.CCE_CO_Container == container1.PK).CCE_SequenceNumber);
			AssertEquals("They are ordered on CusContainer.CO_ContainerNumber", (short)2, entryHeader2.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().First(x => x.CCE_CO_Container == container2.PK).CCE_SequenceNumber);

			invoiceLine2.Delete();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("only one container", 1, entryHeader1.PivotsToContainers.Count);
			AssertEquals("still 2 so that next container can be numbered as 3", (short)2, entryHeader1.CH_HighestContainerNumber);

			var invoiceLine5 = invoice1.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "5";
			invoiceLine5.JI_LineNo = 5;
			var containerPivot5 = invoiceLine5.ContainersPivot.AddNew();
			containerPivot5.C2_CO = container2.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("two one containers now", 2, entryHeader1.PivotsToContainers.Count);
			AssertEquals("A new container should be given max+1", (short)3, entryHeader1.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().First(x => x.CCE_CO_Container == container2.PK).CCE_SequenceNumber);

			CombineAssertions("When all are deleted. This highest number should be set back to zero.", () =>
			{
				entryHeader1.PivotsToContainers.RemoveAndDeleteAll();
				AssertEquals(2u, entryHeader1.CH_HighestContainerNumber);

				header = new ExportEntryHeaderCreator().Create(entryHeader1);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader1, ElectronicDocumentTypeList.Codes._830, stream);
					Factory.Save();
				}
				entryHeader1.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

				AssertEquals(ZShort.Zero, entryHeader1.CH_HighestContainerNumber);
			});
		}

		public void TestCusContainerEntryHeaderPivotLinkedInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";
			invoiceLine3.JI_LineNo = 3;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[0].PivotsToContainers.Count);

			var containerPivot1 = invoiceLine.ContainersPivot.AddNew();
			containerPivot1.C2_CO = container1.PK;
			var containerPivot2 = invoiceLine2.ContainersPivot.AddNew();
			containerPivot2.C2_CO = container1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(1, declaration.CustomsEntryHeaders[0].PivotsToContainers.Count);

			var containerPivot3 = invoiceLine3.ContainersPivot.AddNew();
			containerPivot3.C2_CO = container2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(2, declaration.CustomsEntryHeaders[0].PivotsToContainers.Count);
		}

		public void TestOneBillTwoInvoiceMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LineNo = 1;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LineNo = 1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
		}

		public void TestTwoBillTwoInvoiceLineMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LineNo = 1;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LineNo = 1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
		}

		protected override void CreateCurrentEntrySnapShot()
		{
			var entryHeader = (CusEntryHeader)TestJobDeclaration.CustomsEntryHeaders[0];
			if (entryHeader != null)
			{
				var header = new ImportEntryHeaderCreator().Create(entryHeader);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._929, stream);
					Factory.Save();
				}
			}
		}

		[TestDate(2022, 06, 20)]
		public void TestChangeMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			Factory.Save();

			oldEntry = declaration.CustomsEntryHeaders[0];

			AssertEquals("1234522000001X", oldEntry.EntryNumber);

			AssertChangedMessageType(declaration, KRJobMessageTypeList.Codes.Import, "1234522000001M");
			AssertChangedMessageType(declaration, KRJobMessageTypeList.Codes.Export, "1234522000002X");
			AssertChangedMessageType(declaration, KRJobMessageTypeList.Codes.LocalExport, "1234522000001", LocalExportTransactionNatureCodeList.Codes._01);
		}

		public void TestGAApprovalNoAndVehicleNumberNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.GAApprovalDataCollection.AddNew();
			invoiceLine.GAApprovalDataCollection.AddNew();
			invoiceLine.VehicleNumbers.AddNew();
			invoiceLine.VehicleNumbers.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			AssertEquals(1, invoiceLine.GAApprovalDataCollection[0].CSI_LineNo);
			AssertEquals(2, invoiceLine.GAApprovalDataCollection[1].CSI_LineNo);
			AssertEquals(1u, invoiceLine.VehicleNumbers[0].CY_Order);
			AssertEquals(2u, invoiceLine.VehicleNumbers[1].CY_Order);
			AssertEquals(ZShort.Zero, invoiceLine.KR_HighestGAApprovalSeqNo);
			AssertEquals(ZShort.Zero, invoiceLine.KR_HighestVehicleSeqNo);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var header = new ExportEntryHeaderCreator().Create(entryHeader);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
			entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(2u, invoiceLine.KR_HighestGAApprovalSeqNo);
			AssertEquals(2u, invoiceLine.KR_HighestVehicleSeqNo);

			invoiceLine.GAApprovalDataCollection.AddNew();
			invoiceLine.VehicleNumbers.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(3, invoiceLine.GAApprovalDataCollection[2].CSI_LineNo);
			AssertEquals(3u, invoiceLine.VehicleNumbers[2].CY_Order);

			header = new ExportEntryHeaderCreator().Create(entryHeader);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
			entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

			CombineAssertions("The logic does not change line items whose sequence number is < the highest number.", () =>
			{
				AssertEquals(3u, invoiceLine.KR_HighestGAApprovalSeqNo);
				AssertEquals(3u, invoiceLine.KR_HighestVehicleSeqNo);

				invoiceLine.GAApprovalDataCollection.Cast<GAApproval>().FirstOrDefault(x => x.CSI_LineNo == 2).Delete();
				invoiceLine.VehicleNumbers.Cast<VehicleNumber>().FirstOrDefault(x => x.CY_Order == 2).Delete();
				invoiceLine.GAApprovalDataCollection.AddNew();
				invoiceLine.VehicleNumbers.AddNew();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

				AssertEquals(3, invoiceLine.GAApprovalDataCollection.Count);
				AssertEquals(3, invoiceLine.VehicleNumbers.Count);

				AssertEquals(1, invoiceLine.GAApprovalDataCollection[0].CSI_LineNo);
				AssertEquals(3, invoiceLine.GAApprovalDataCollection[1].CSI_LineNo);
				AssertEquals(4, invoiceLine.GAApprovalDataCollection[2].CSI_LineNo);
				AssertEquals(1u, invoiceLine.VehicleNumbers[0].CY_Order);
				AssertEquals(3u, invoiceLine.VehicleNumbers[1].CY_Order);
				AssertEquals(4u, invoiceLine.VehicleNumbers[2].CY_Order);
			});

			CombineAssertions("When all are deleted. This highest number should be set back to zero.", () =>
			{
				invoiceLine.VehicleNumbers.RemoveAndDeleteAll();
				invoiceLine.GAApprovalDataCollection.RemoveAndDeleteAll();

				AssertEquals(3u, invoiceLine.KR_HighestGAApprovalSeqNo);
				AssertEquals(3u, invoiceLine.KR_HighestVehicleSeqNo);
				header = new ExportEntryHeaderCreator().Create(entryHeader);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._830, stream);
					Factory.Save();
				}
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

				AssertEquals(ZShort.Zero, invoiceLine.KR_HighestGAApprovalSeqNo);
				AssertEquals(ZShort.Zero, invoiceLine.KR_HighestVehicleSeqNo);
			});
		}

		public void TestGetEntryCreationStrategiesByMessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(declaration.CustomsEntryHeaders[0].CH_MessageType, ElectronicDocumentTypeList.Codes._5DQ);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(declaration.CustomsEntryHeaders[0].CH_MessageType, ElectronicDocumentTypeList.Codes._5DP);
		}

		public void TestReassignInvoiceLineSequenceNumber()
		{
			AssertReassignInvoiceLineSequenceNumber(KRJobMessageTypeList.Codes.Export);
			AssertReassignInvoiceLineSequenceNumber(KRJobMessageTypeList.Codes.Import);
		}

		void AssertReassignInvoiceLineSequenceNumber(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			var entry = declaration.CustomsEntryHeaders[0];
			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Pre-Condition", 1u, invoiceLine.JI_SequenceNumber);
			AssertEquals("Pre-Condition", 1u, invoiceLine2.JI_SequenceNumber);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";
			invoiceLine3.JI_LineNo = 3;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LineNo = 4;

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "2";
			invoiceLine5.JI_LineNo = 5;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(5, entry.MergedLines.Count);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals("Merge by 'NON'", 1u, invoiceLine.JI_SequenceNumber);
			AssertEquals("Merge by 'NON'", 1u, invoiceLine2.JI_SequenceNumber);
			AssertEquals("Merge by 'NON'", 1u, invoiceLine3.JI_SequenceNumber);
			AssertEquals("Merge by 'NON'", 1u, invoiceLine4.JI_SequenceNumber);
			AssertEquals("Merge by 'NON'", 1u, invoiceLine5.JI_SequenceNumber);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals("Merge by 'TRF'", 1u, invoiceLine.JI_SequenceNumber);
			AssertEquals("Merge by 'TRF'", 1u, invoiceLine2.JI_SequenceNumber);
			AssertEquals("Merge by 'TRF'", 2u, invoiceLine3.JI_SequenceNumber);
			AssertEquals("Merge by 'TRF'", 2u, invoiceLine4.JI_SequenceNumber);
			AssertEquals("Merge by 'TRF'", 3u, invoiceLine5.JI_SequenceNumber);
		}

		public void TestCH_HighestTransportMeansNo5DQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			declaration.TransportMeans.AddNew();
			declaration.TransportMeans.AddNew();
			AssertEquals(0u, declaration.TransportMeans[0].CY_Order);
			AssertEquals(0u, declaration.TransportMeans[1].CY_Order);
			AssertEquals(0u, entry.CH_HighestTransportMeansNo);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1u, declaration.TransportMeans[0].CY_Order);
			AssertEquals(2u, declaration.TransportMeans[1].CY_Order);
			AssertEquals(0u, entry.CH_HighestTransportMeansNo);

			CreateLocalExportSnapshot(entry, entry.CH_MessageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(1u, declaration.TransportMeans[0].CY_Order);
			AssertEquals(2u, declaration.TransportMeans[1].CY_Order);
			AssertEquals(2u, entry.CH_HighestTransportMeansNo);

			declaration.TransportMeans.RemoveAndDelete(declaration.TransportMeans[0]);
			declaration.TransportMeans.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2u, declaration.TransportMeans[0].CY_Order);
			AssertEquals(3u, declaration.TransportMeans[1].CY_Order);
			AssertEquals(2u, entry.CH_HighestTransportMeansNo);

			CreateLocalExportSnapshot(entry, entry.CH_MessageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(2u, declaration.TransportMeans[0].CY_Order);
			AssertEquals(3u, declaration.TransportMeans[1].CY_Order);
			AssertEquals(3u, entry.CH_HighestTransportMeansNo);
		}
		void CreateLocalExportSnapshot(CusEntryHeader entry, string messageType)
		{
			LocalExportEntryHeader header = null;
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				header = new LocalExport5DPEntryHeaderCreator().Create(entry);
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5DQ)
			{
				header = new LocalExport5DQEntryHeaderCreator().Create(entry);
			}
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				Factory.Save();
			}
		}

		public void TestMergeEntryLineWith200InvoiceLines()
		{
			AssertMergeEntryLineWith200InvoiceLines(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith200InvoiceLines(KRJobMessageTypeList.Codes.Import);
		}

		void AssertMergeEntryLineWith200InvoiceLines(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 200; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];

			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(99L, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(99L, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(2L, entry.MergedLines[2].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestMergeEntryLineWith198InvoiceLines()
		{
			AssertMergeEntryLineWith198InvoiceLines(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith198InvoiceLines(KRJobMessageTypeList.Codes.Import);
		}
		void AssertMergeEntryLineWith198InvoiceLines(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 198; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];

			invoice.InvoiceLines.Remove(invoice.InvoiceLines.Last());
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99L, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(98L, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);

			var invoiceLineToAdd1 = invoice.InvoiceLines.AddNew();
			invoiceLineToAdd1.JI_Tariff = "1";
			invoiceLineToAdd1.JI_LineNo = 199;
			var invoiceLineToAdd2 = invoice.InvoiceLines.AddNew();
			invoiceLineToAdd2.JI_Tariff = "1";
			invoiceLineToAdd2.JI_LineNo = 200;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			CreateOriginalMessageSnapshot(entry, messageType, 2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(99L, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(99L, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(1L, entry.MergedLines[2].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestMergeEntryLineWith99InvoiceLines()
		{
			AssertMergeEntryLineWith99InvoiceLines(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith99InvoiceLines(KRJobMessageTypeList.Codes.Import);
		}

		void AssertMergeEntryLineWith99InvoiceLines(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 99; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];

			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(1, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);

			invoice.InvoiceLines.Remove(invoice.InvoiceLines[50]);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType, 2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(1, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);

			var invoiceLineToAdd1 = invoice.InvoiceLines.AddNew();
			invoiceLineToAdd1.JI_Tariff = "1";
			invoiceLineToAdd1.JI_LineNo = 100;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType, 3);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(1U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestMergeEntryLineWith197InvoiceLines_1()
		{
			AssertMergeEntryLineWith197InvoiceLines_1(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith197InvoiceLines_1(KRJobMessageTypeList.Codes.Import);
		}

		void AssertMergeEntryLineWith197InvoiceLines_1(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 197; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];
			CreateOriginalMessageSnapshot(entry, messageType);

			var header = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(98U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);

			invoice.InvoiceLines.Remove(invoice.InvoiceLines.Last());

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType, 2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(98U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);

			var invoiceLineToAdd1 = invoice.InvoiceLines.AddNew();
			invoiceLineToAdd1.JI_Tariff = "1";
			invoiceLineToAdd1.JI_LineNo = 198;
			var invoiceLineToAdd2 = invoice.InvoiceLines.AddNew();
			invoiceLineToAdd2.JI_Tariff = "1";
			invoiceLineToAdd2.JI_LineNo = 199;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType, 3);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(99U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(1U, entry.MergedLines[2].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestMergeEntryLineWith197InvoiceLines_2()
		{
			AssertMergeEntryLineWith197InvoiceLines_2(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith197InvoiceLines_2(KRJobMessageTypeList.Codes.Import);
		}

		void AssertMergeEntryLineWith197InvoiceLines_2(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 197; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];

			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(98U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);

			for (int i = 0; i < 5; i++)
			{
				invoice.InvoiceLines.Remove(invoice.InvoiceLines.Last());
			}
			for (ZShort i = 1; i <= 3; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = 197 + i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType, 2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(99U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(2U, entry.MergedLines[2].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestMergeEntryLineWith197InvoiceLines_3()
		{
			AssertMergeEntryLineWith197InvoiceLines_3(KRJobMessageTypeList.Codes.Export);
			AssertMergeEntryLineWith197InvoiceLines_3(KRJobMessageTypeList.Codes.Import);
		}

		void AssertMergeEntryLineWith197InvoiceLines_3(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 197; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var entry = declaration.CustomsEntryHeaders[0];

			for (int i = 0; i < 5; i++)
			{
				invoice.InvoiceLines.Remove(invoice.InvoiceLines.Last());
			}
			for (ZShort i = 1; i <= 3; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = 197 + i;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			CreateOriginalMessageSnapshot(entry, messageType);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(99U, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals(96U, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
		}

		void CreateOriginalMessageSnapshot(CusEntryHeader entry, ZString messageType, int version = 1)
		{
			switch (messageType)
			{
				case KRJobMessageTypeList.Codes.Export:
					var exportHeader = new ExportEntryHeaderCreator().Create(entry);
					using (var stream = KRXmlObjectSerializer.Serialize(exportHeader))
					{
						AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream, version);
						Factory.Save();
					}
					break;
				case KRJobMessageTypeList.Codes.Import:
					var importHeader = new ImportEntryHeaderCreator().Create(entry);
					using (var stream = KRXmlObjectSerializer.Serialize(importHeader))
					{
						AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream, version);
						Factory.Save();
					}
					break;
				case KRJobMessageTypeList.Codes.LocalExport:
					var localExportHeader = new LocalExportCommonCreator().Create(entry);
					using (var stream = KRXmlObjectSerializer.Serialize(localExportHeader))
					{
						AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DP, stream, version);
						Factory.Save();
					}
					break;
				default:
					break;
			}
		}
		ZString[] MessageTypesManagingSequenceNumber => new ZString[]
		{
			KRJobMessageTypeList.Codes.Export,
			KRJobMessageTypeList.Codes.Import,
		};

		public void TestMergeEntryLineFromCopiedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();

			for (ZShort i = 1; i <= 100; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1";
				invoiceLine.JI_LineNo = i;
			}
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			Factory.Save();

			var newDeclaration = (JobDeclaration)declaration.TemplateCopy();
			var newInvoice = newDeclaration.Invoices[0];

			newInvoice.InvoiceLines.Remove(newInvoice.InvoiceLines.First());

			newDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var newEntry = newDeclaration.CustomsEntryHeaders[0];

			AssertEquals(1, newEntry.MergedLines.Count);
			AssertEquals(99, newEntry.MergedLines[0].InvoiceLines.Count);
			AssertEquals(0U, newEntry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
		}

		public void TestKR_ImmediateDeliveryNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var delivery1 = invoiceLine.ImmediateDeliveries.AddNew();
			delivery1.CY_Data = "A";
			var delivery2 = invoiceLine.ImmediateDeliveries.AddNew();
			delivery2.CY_Data = "B";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var entryline = entry.MergedLines[0];

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1u, entryline.ImmediateDeliveries[0].CY_Order);
			AssertEquals(2u, entryline.ImmediateDeliveries[1].CY_Order);
			AssertEquals(0u, entryline.KR_HighestImmediateDeliveryNo);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(1u, entryline.ImmediateDeliveries[0].CY_Order);
			AssertEquals(2u, entryline.ImmediateDeliveries[1].CY_Order);
			AssertEquals(2u, entryline.KR_HighestImmediateDeliveryNo);

			invoiceLine.ImmediateDeliveries.RemoveAndDelete(delivery1);
			var delivery3 = invoiceLine.ImmediateDeliveries.AddNew();
			delivery3.CY_Data = "C";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2u, entryline.ImmediateDeliveries[0].CY_Order);
			AssertEquals(3u, entryline.ImmediateDeliveries[1].CY_Order);
			AssertEquals(2u, entryline.KR_HighestImmediateDeliveryNo);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(2u, entryline.ImmediateDeliveries[0].CY_Order);
			AssertEquals(3u, entryline.ImmediateDeliveries[1].CY_Order);
			AssertEquals(3u, entryline.KR_HighestImmediateDeliveryNo);
		}

		public void TestMergedImmediateDeliveries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0102399000";
			invoiceLine1.JI_LineNo = 1;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0102399000";
			invoiceLine2.JI_LineNo = 1;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0102399000";
			invoiceLine3.JI_LineNo = 2;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9875654230";
			invoiceLine4.JI_LineNo = 3;

			var immediateDelivery1A = invoiceLine1.ImmediateDeliveries.AddNew();
			immediateDelivery1A.CY_Data = "A";
			var immediateDelivery1B = invoiceLine1.ImmediateDeliveries.AddNew();
			immediateDelivery1B.CY_Data = "B";
			var immediateDelivery2 = invoiceLine2.ImmediateDeliveries.AddNew();
			immediateDelivery2.CY_Data = "A";
			var immediateDelivery3 = invoiceLine3.ImmediateDeliveries.AddNew();
			immediateDelivery3.CY_Data = "C";
			var immediateDelivery4 = invoiceLine4.ImmediateDeliveries.AddNew();
			immediateDelivery4.CY_Data = "D";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];

			CombineAssertions("Handling Duplicate ImmediateDeliveries & Copy EntryLine.CY_Order to InvoiceLine.CY_Order.", () =>
			{
				AssertEquals("entryLine1.ImmediateDeliveries Count", 3, entryLine1.ImmediateDeliveries.Count);
				AssertEquals("entryLine1.ImmediateDeliveries[0].CY_Data", "A", entryLine1.ImmediateDeliveries[0].CY_Data);
				AssertEquals("entryLine1.ImmediateDeliveries[0].CY_Order", 1u, entryLine1.ImmediateDeliveries[0].CY_Order);
				AssertEquals("immediateDelivery1A.CY_Order: Copied from entryLine1.ImmediateDeliveries[0].CY_Order", 1u, immediateDelivery1A.CY_Order);
				AssertEquals("immediateDelivery2.CY_Order: Copied from entryLine1.ImmediateDeliveries[0].CY_Order", 1u, immediateDelivery2.CY_Order);

				AssertEquals("entryLine1.ImmediateDeliveries[1].CY_Data", "B", entryLine1.ImmediateDeliveries[1].CY_Data);
				AssertEquals("entryLine1.ImmediateDeliveries[1].CY_Order", 2u, entryLine1.ImmediateDeliveries[1].CY_Order);
				AssertEquals("immediateDelivery1B.CY_Order: Copied from entryLine1.ImmediateDeliveries[1].CY_Order", 2u, immediateDelivery1B.CY_Order);

				AssertEquals("entryLine1.ImmediateDeliveries[2].CY_Data", "C", entryLine1.ImmediateDeliveries[2].CY_Data);
				AssertEquals("entryLine1.ImmediateDeliveries[2].CY_Order", 3u, entryLine1.ImmediateDeliveries[2].CY_Order);
				AssertEquals("immediateDelivery3.CY_Order: Copied from entryLine1.ImmediateDeliveries[2].CY_Order", 3u, immediateDelivery3.CY_Order);

				AssertEquals("entryLine2.ImmediateDeliveries Count", 1, entryLine2.ImmediateDeliveries.Count);
				AssertEquals("entryLine2.ImmediateDeliveries[0].CY_Data", "D", entryLine2.ImmediateDeliveries[0].CY_Data);
				AssertEquals("entryLine2.ImmediateDeliveries[0].CY_Order", 1u, entryLine2.ImmediateDeliveries[0].CY_Order);
				AssertEquals("immediateDelivery4.CY_Order: Copied from entryLine2.ImmediateDeliveries[0].CY_Order", 1u, immediateDelivery4.CY_Order);
			});

			invoiceLine2.JI_Tariff = "9875654230";
			invoiceLine3.JI_Tariff = "9875654230";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, entryLine1.InvoiceLines.Count);
			AssertEquals(3, entryLine2.InvoiceLines.Count);
			CombineAssertions("Delete unused ImmediateDeliveries", () =>
			{
				AssertEquals("entryLine1.ImmediateDeliveries Count", 2, entryLine1.ImmediateDeliveries.Count);
				AssertEquals("entryLine1.ImmediateDeliveries[0].CY_Data", "A", entryLine1.ImmediateDeliveries[0].CY_Data);
				AssertEquals("entryLine1.ImmediateDeliveries[0].CY_Order", 1u, entryLine1.ImmediateDeliveries[0].CY_Order);

				AssertEquals("entryLine1.ImmediateDeliveries[1].CY_Data", "B", entryLine1.ImmediateDeliveries[1].CY_Data);
				AssertEquals("entryLine1.ImmediateDeliveries[1].CY_Order", 2u, entryLine1.ImmediateDeliveries[1].CY_Order);

				AssertEquals("entryLine2.ImmediateDeliveries Count", 3, entryLine2.ImmediateDeliveries.Count);
				AssertEquals("entryLine2.ImmediateDeliveries[0].CY_Data", "D", entryLine2.ImmediateDeliveries[0].CY_Data);
				AssertEquals("entryLine2.ImmediateDeliveries[0].CY_Order", 1u, entryLine2.ImmediateDeliveries[0].CY_Order);

				AssertEquals("entryLine2.ImmediateDeliveries[1].CY_Data", "A", entryLine2.ImmediateDeliveries[1].CY_Data);
				AssertEquals("entryLine2.ImmediateDeliveries[1].CY_Order", 2u, entryLine2.ImmediateDeliveries[1].CY_Order);

				AssertEquals("entryLine2.ImmediateDeliveries[2].CY_Data", "C", entryLine2.ImmediateDeliveries[2].CY_Data);
				AssertEquals("entryLine2.ImmediateDeliveries[2].CY_Order", 3u, entryLine2.ImmediateDeliveries[2].CY_Order);
			});
		}

		#region NonGADetails
		public void TestMergedNonGADetailsInvalidRowsAreDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9875654230";
			CreateNonGADetail(invoiceLine, "13", "A", "01");
			var nonGADetail = CreateNonGADetail(invoiceLine, "13", "B", "01");
			CreateNonGADetail(invoiceLine, "13", "C", "01");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines[0];

			AssertEquals(3, entryLine.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine.NonGADetailCollection[0], "13", "A", "01", 1u, "");
			AssertNonGADetail(entryLine.NonGADetailCollection[1], "13", "B", "01", 2u, "");
			AssertNonGADetail(entryLine.NonGADetailCollection[2], "13", "C", "01", 3u, "");

			invoiceLine.NonGADetailCollection.RemoveAndDelete(nonGADetail);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, entryLine.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine.NonGADetailCollection[0], "13", "A", "01", 1u, "");
			AssertNonGADetail(entryLine.NonGADetailCollection[1], "13", "C", "01", 2u, "");
		}

		public void TestMergedNonGADetailsAreDistinct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var tariff1 = "9875654230";
			var tariff2 = "1234567890";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";

			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1;
			invoiceLine1.JI_LineNo = 1;
			CreateNonGADetail(invoiceLine1, "13", "A", "01", "description 1");
			CreateNonGADetail(invoiceLine1, "13", "B", "02", "description 1");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff1;
			invoiceLine2.JI_LineNo = 1;
			CreateNonGADetail(invoiceLine2, "14", "A", "02", "description 2");
			CreateNonGADetail(invoiceLine2, "14", "B", "01", "description 2");

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff1;
			invoiceLine3.JI_LineNo = 2;
			CreateNonGADetail(invoiceLine3, "13", "A", "02", "description 3");
			CreateNonGADetail(invoiceLine3, "13", "B", "01", "description 3");

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariff1;
			invoiceLine4.JI_LineNo = 3;
			CreateNonGADetail(invoiceLine4, "13", "A", "01", "description 4");
			CreateNonGADetail(invoiceLine4, "14", "A", "01", "description 4");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];

			AssertEquals(7, entryLine1.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine1.NonGADetailCollection[0], "13", "A", "01", 1u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[1], "13", "B", "02", 2u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[2], "14", "A", "02", 3u, "description 2");
			AssertNonGADetail(entryLine1.NonGADetailCollection[3], "14", "B", "01", 4u, "description 2");
			AssertNonGADetail(entryLine1.NonGADetailCollection[4], "13", "A", "02", 5u, "description 3");
			AssertNonGADetail(entryLine1.NonGADetailCollection[5], "13", "B", "01", 6u, "description 3");
			AssertNonGADetail(entryLine1.NonGADetailCollection[6], "14", "A", "01", 7u, "description 4");

			invoiceLine4.JI_Tariff = tariff2;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entryLine1 = entry.MergedLines[0];

			AssertEquals(6, entryLine1.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine1.NonGADetailCollection[0], "13", "A", "01", 1u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[1], "13", "B", "02", 2u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[2], "14", "A", "02", 3u, "description 2");
			AssertNonGADetail(entryLine1.NonGADetailCollection[3], "14", "B", "01", 4u, "description 2");
			AssertNonGADetail(entryLine1.NonGADetailCollection[4], "13", "A", "02", 5u, "description 3");
			AssertNonGADetail(entryLine1.NonGADetailCollection[5], "13", "B", "01", 6u, "description 3");

			var entryLine2 = entry.MergedLines[1];
			AssertEquals(2, entryLine2.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine2.NonGADetailCollection[0], "13", "A", "01", 1u, "description 4");
			AssertNonGADetail(entryLine2.NonGADetailCollection[1], "14", "A", "01", 2u, "description 4");

			invoiceLine3.JI_Tariff = tariff2;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entryLine1 = entry.MergedLines[0];

			AssertEquals(4, entryLine1.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine1.NonGADetailCollection[0], "13", "A", "01", 1u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[1], "13", "B", "02", 2u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[2], "14", "A", "02", 3u, "description 2");
			AssertNonGADetail(entryLine1.NonGADetailCollection[3], "14", "B", "01", 4u, "description 2");

			entryLine2 = entry.MergedLines[1];
			AssertEquals(4, entryLine2.NonGADetailCollection.Count);
			AssertNonGADetail(entryLine2.NonGADetailCollection[0], "13", "A", "01", 1u, "description 4");
			AssertNonGADetail(entryLine2.NonGADetailCollection[1], "14", "A", "01", 2u, "description 4");
			AssertNonGADetail(entryLine2.NonGADetailCollection[2], "13", "A", "02", 3u, "description 3");
			AssertNonGADetail(entryLine2.NonGADetailCollection[3], "13", "B", "01", 4u, "description 3");
		}

		public void TestMergedNonGADetailsLineNumberCopied()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var tariff1 = "9875654230";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";

			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1;
			invoiceLine1.JI_LineNo = 1;
			var nonGADetail1 = CreateNonGADetail(invoiceLine1, "12", "A", "01", "description 1");
			var nonGADetail2 = CreateNonGADetail(invoiceLine1, "13", "A", "01", "description 1");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff1;
			invoiceLine2.JI_LineNo = 1;
			var nonGADetail3 = CreateNonGADetail(invoiceLine2, "14", "A", "01", "description 2");
			var nonGADetail4 = CreateNonGADetail(invoiceLine2, "12", "A", "01", "description 2");

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff1;
			invoiceLine3.JI_LineNo = 2;
			var nonGADetail5 = CreateNonGADetail(invoiceLine3, "12", "A", "01", "description 3");

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariff1;
			invoiceLine4.JI_LineNo = 3;
			var nonGADetail6 = CreateNonGADetail(invoiceLine4, "13", "A", "01", "description 4");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			AssertNonGADetail(entryLine1.NonGADetailCollection[0], "12", "A", "01", 1u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[1], "13", "A", "01", 2u, "description 1");
			AssertNonGADetail(entryLine1.NonGADetailCollection[2], "14", "A", "01", 3u, "description 2");

			AssertEquals(1u, nonGADetail1.CSI_LineNo);
			AssertEquals(2u, nonGADetail2.CSI_LineNo);
			AssertEquals(3u, nonGADetail3.CSI_LineNo);
			AssertEquals(1u, nonGADetail4.CSI_LineNo);
			AssertEquals(1u, nonGADetail5.CSI_LineNo);
			AssertEquals(2u, nonGADetail6.CSI_LineNo);
		}

		NonGADetail CreateNonGADetail(JobComInvoiceLine invoiceLine, string procedure, string code, string status, string description = "")
		{
			var nonGADetail = invoiceLine.NonGADetailCollection.AddNew();
			nonGADetail.CSI_Procedure = procedure;
			nonGADetail.CSI_Code = code;
			nonGADetail.CSI_Status = status;
			nonGADetail.CSI_Description = description;
			return nonGADetail;
		}

		static void AssertNonGADetail(NonGADetail nonGADetail, string procedure, string code, string status, uint lineNo, string description)
		{
			CombineAssertions(() => {
				AssertEquals(procedure, nonGADetail.CSI_Procedure);
				AssertEquals(code, nonGADetail.CSI_Code);
				AssertEquals(status, nonGADetail.CSI_Status);
				AssertEquals(lineNo, nonGADetail.CSI_LineNo);
				AssertEquals(description, nonGADetail.CSI_Description);
			});
		}
		#endregion
		#region PreviousExpDecLine
		public void TestMergedPreviousExpDecLinesInvalidRowsAreDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9875654230";
			CreatePreviousExpDecLine(invoiceLine, "1234567", "123", 11, 100m, "U");
			var previousExpDecLine = CreatePreviousExpDecLine(invoiceLine, "1234567", "321", 11, 200m, "KG");
			CreatePreviousExpDecLine(invoiceLine, "1234567", "987", 11, 300m, "L");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines[0];

			AssertEquals(3, entryLine.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[0], "1234567", "123", 11, 1u, 100m, "U");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[1], "1234567", "321", 11, 2u, 200m, "KG");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[2], "1234567", "987", 11, 3u, 300m, "L");

			invoiceLine.PreviousExpDecLineCollection.RemoveAndDelete(previousExpDecLine);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, entryLine.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[0], "1234567", "123", 11, 1u, 100m, "U");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[1], "1234567", "987", 11, 2u, 300m, "L");
		}

		public void TestMergedPreviousExpDecLinesAreDistinct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var tariff1 = "9875654230";
			var tariff2 = "1234567890";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1;
			invoiceLine1.JI_LineNo = 1;
			CreatePreviousExpDecLine(invoiceLine1, "1234567", "123", 11, 100m, "U");
			CreatePreviousExpDecLine(invoiceLine1, "1234567", "321", 21, 200m, "U");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff1;
			invoiceLine2.JI_LineNo = 1;
			CreatePreviousExpDecLine(invoiceLine2, "7654321", "123", 21, 400m, "KG");
			CreatePreviousExpDecLine(invoiceLine2, "7654321", "321", 11, 800m, "KG");

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff1;
			invoiceLine3.JI_LineNo = 2;
			CreatePreviousExpDecLine(invoiceLine3, "1234567", "123", 21, 1600m, "U");
			CreatePreviousExpDecLine(invoiceLine3, "1234567", "321", 11, 3200m, "U");

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariff1;
			invoiceLine4.JI_LineNo = 3;
			CreatePreviousExpDecLine(invoiceLine4, "7654321", "123", 11, 6400m, "U");
			CreatePreviousExpDecLine(invoiceLine4, "1234567", "123", 11, 12800m, "U");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];

			AssertEquals(7, entryLine1.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[0], "1234567", "123", 11, 1u, 12900m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[1], "1234567", "321", 21, 2u, 200m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[2], "7654321", "123", 21, 3u, 400m, "KG");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[3], "7654321", "321", 11, 4u, 800m, "KG");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[4], "1234567", "123", 21, 5u, 1600m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[5], "1234567", "321", 11, 6u, 3200m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[6], "7654321", "123", 11, 7u, 6400m, "U");

			invoiceLine4.JI_Tariff = tariff2;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entryLine1 = entry.MergedLines[0];

			AssertEquals(6, entryLine1.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[0], "1234567", "123", 11, 1u, 100m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[1], "1234567", "321", 21, 2u, 200m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[2], "7654321", "123", 21, 3u, 400m, "KG");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[3], "7654321", "321", 11, 4u, 800m, "KG");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[4], "1234567", "123", 21, 5u, 1600m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[5], "1234567", "321", 11, 6u, 3200m, "U");

			var entryLine2 = entry.MergedLines[1];
			AssertEquals(2, entryLine2.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[0], "7654321", "123", 11, 1u, 6400m, "U");
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[1], "1234567", "123", 11, 2u, 12800m, "U");

			invoiceLine3.JI_Tariff = tariff2;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entryLine1 = entry.MergedLines[0];

			AssertEquals(4, entryLine1.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[0], "1234567", "123", 11, 1u, 100m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[1], "1234567", "321", 21, 2u, 200m, "U");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[2], "7654321", "123", 21, 3u, 400m, "KG");
			AssertPreviousExpDecLine(entryLine1.PreviousExpDecLineCollection[3], "7654321", "321", 11, 4u, 800m, "KG");

			entryLine2 = entry.MergedLines[1];
			AssertEquals(4, entryLine2.PreviousExpDecLineCollection.Count);
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[0], "7654321", "123", 11, 1u, 6400m, "U");
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[1], "1234567", "123", 11, 2u, 12800m, "U");
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[2], "1234567", "123", 21, 3u, 1600m, "U");
			AssertPreviousExpDecLine(entryLine2.PreviousExpDecLineCollection[3], "1234567", "321", 11, 4u, 3200m, "U");
		}

		public void TestMergedPreviousExpDecLinesLineNumberCopied()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var tariff1 = "9875654230";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1;
			invoiceLine1.JI_LineNo = 1;
			var previousExpDecLine1 = CreatePreviousExpDecLine(invoiceLine1, "1000000", "123", 11, 100m, "U");
			var previousExpDecLine2 = CreatePreviousExpDecLine(invoiceLine1, "1234567", "123", 11, 100m, "KG");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff1;
			invoiceLine2.JI_LineNo = 1;
			var previousExpDecLine3 = CreatePreviousExpDecLine(invoiceLine2, "7654321", "123", 11, 100m, "AC");
			var previousExpDecLine4 = CreatePreviousExpDecLine(invoiceLine2, "1000000", "123", 11, 100m, "U");

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff1;
			invoiceLine3.JI_LineNo = 2;
			var previousExpDecLine5 = CreatePreviousExpDecLine(invoiceLine3, "1000000", "123", 11, 100m, "U");

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariff1;
			invoiceLine4.JI_LineNo = 3;
			var previousExpDecLine6 = CreatePreviousExpDecLine(invoiceLine4, "1234567", "123", 11, 100m, "KG");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines[0];
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[0], "1000000", "123", 11, 1u, 300m, "U");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[1], "1234567", "123", 11, 2u, 200m, "KG");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[2], "7654321", "123", 11, 3u, 100m, "AC");

			AssertEquals(1u, previousExpDecLine1.CSI_LineNo);
			AssertEquals(2u, previousExpDecLine2.CSI_LineNo);
			AssertEquals(3u, previousExpDecLine3.CSI_LineNo);
			AssertEquals(1u, previousExpDecLine4.CSI_LineNo);
			AssertEquals(1u, previousExpDecLine5.CSI_LineNo);
			AssertEquals(2u, previousExpDecLine6.CSI_LineNo);
		}

		public void TestMergedPreviousExpDecLinesUnitQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var tariff1 = "9875654230";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1;
			invoiceLine1.JI_LineNo = 1;

			var previousExpDecLine1 = CreatePreviousExpDecLine(invoiceLine1, "1000000", "123", 11, 1200m, "G");
			var previousExpDecLine2 = CreatePreviousExpDecLine(invoiceLine1, "1000000", "123", 11, 1m, "KG");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff1;
			invoiceLine2.JI_LineNo = 1;
			var previousExpDecLine3 = CreatePreviousExpDecLine(invoiceLine2, "7654321", "123", 11, 100m, "CM");
			var previousExpDecLine4 = CreatePreviousExpDecLine(invoiceLine2, "7654321", "123", 11, 2m, "M");

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff1;
			invoiceLine3.JI_LineNo = 2;
			var previousExpDecLine5 = CreatePreviousExpDecLine(invoiceLine3, "1234567", "123", 11, 1234m, "M2");
			var previousExpDecLine6 = CreatePreviousExpDecLine(invoiceLine3, "1234567", "123", 11, 3m, "CM2");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines[0];
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[0], "1000000", "123", 11, 1u, 1201m, "G");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[1], "7654321", "123", 11, 2u, 102m, "CM");
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[2], "1234567", "123", 11, 3u, 1237m, "M2");

			previousExpDecLine1.CSI_Quantity = 4m;
			previousExpDecLine1.CSI_UnitOfQuantity = "M3";
			previousExpDecLine2.CSI_Quantity = 3040m;
			previousExpDecLine2.CSI_UnitOfQuantity = "L";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entryLine = entry.MergedLines[0];
			AssertPreviousExpDecLine(entryLine.PreviousExpDecLineCollection[0], "1000000", "123", 11, 1u, 3044m, "M3");
		}

		PreviousExpDecLine CreatePreviousExpDecLine(JobComInvoiceLine invoiceLine, string referenceNumber, string referenceNumber2, int itemNumber, decimal quantity, string uq)
		{
			var previousExpDecLine = invoiceLine.PreviousExpDecLineCollection.AddNew();
			previousExpDecLine.CSI_ReferenceNumber = referenceNumber;
			previousExpDecLine.CSI_ReferenceNumber2 = referenceNumber2;
			previousExpDecLine.CSI_ItemNumber = itemNumber;
			previousExpDecLine.CSI_Quantity = quantity;
			previousExpDecLine.CSI_UnitOfQuantity = uq;
			return previousExpDecLine;
		}

		static void AssertPreviousExpDecLine(PreviousExpDecLine previousExpDecLine, string referenceNumber, string referenceNumber2, int itemNumber, uint lineNo, decimal quantity, string uq)
		{
			CombineAssertions(() =>
			{
				AssertEquals(referenceNumber, previousExpDecLine.CSI_ReferenceNumber);
				AssertEquals(referenceNumber2, previousExpDecLine.CSI_ReferenceNumber2);
				AssertEquals(itemNumber, previousExpDecLine.CSI_ItemNumber);
				AssertEquals(lineNo, previousExpDecLine.CSI_LineNo);
				AssertEquals(quantity, previousExpDecLine.CSI_Quantity);
				AssertEquals(uq, previousExpDecLine.CSI_UnitOfQuantity);
			});
		}
		#endregion

		void CreateSnapshot(CusEntryHeader entry, ZString messageType)
		{
			if (messageType == ElectronicDocumentTypeList.Codes._929)
			{
				var header = new ImportEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
		}

		public void TestFTASequenceNumberGeneratedByPrimaryPreference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_PrimaryPreference = "FCN";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_PrimaryPreference = "FCN";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3";
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_PrimaryPreference = "F1";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2";
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.JI_PrimaryPreference = "FCN";
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(4, entry.MergedLines.Count);
			AssertNotEquals(invoiceLine2.CusEntryLine, invoiceLine4.CusEntryLine);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)0, invoiceLine3.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)3, invoiceLine4.CusEntryLine.CL_FTASequenceNumber);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)0, invoiceLine3.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine4.CusEntryLine.CL_FTASequenceNumber);

			entry.CH_HighestFTASequenceNumber = 2;
			invoiceLine.JI_PrimaryPreference = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(3, entry.MergedLines.Count);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine4.CusEntryLine);
			AssertEquals((short)0, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)0, invoiceLine3.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine4.CusEntryLine.CL_FTASequenceNumber);
		}

		public void TestHighestFTASequenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice1.PK;
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_PrimaryPreference = "FCN";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice1.PK;
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_PrimaryPreference = "FCN";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			var entry1 = declaration.CustomsEntryHeaders[0];
			AssertEquals((short)0, entry1.CH_HighestFTASequenceNumber);
			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);

			var header = new ImportDHRCreator().Create(entry1);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry1, "DHR", stream);
				Factory.Save();
			}

			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entry1NumDHR = entry1.EntryNumbers.AddNew();
			entry1NumDHR.CE_EntryType = ElectronicDocumentTypeList.Codes._DHR;
			entry1NumDHR.CE_EntryNum = "1001";
			entry1NumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals((short)2, entry1.CH_HighestFTASequenceNumber);
			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_JZ = invoice1.PK;
			invoiceLine3.JI_Tariff = "3";
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_PrimaryPreference = "FCN";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			header = new ImportDHRCreator().Create(entry1);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry1, "DHR", stream);
				Factory.Save();
			}

			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entry1NumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals((short)3, entry1.CH_HighestFTASequenceNumber);
			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)3, invoiceLine3.CusEntryLine.CL_FTASequenceNumber);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			invoiceLine3.JI_LineNo = 1;
			invoiceLine3.JI_JZ = invoice2.PK;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_JZ = invoice1.PK;
			invoiceLine4.JI_Tariff = "4";
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.JI_PrimaryPreference = "FCN";
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			var entry2 = declaration.CustomsEntryHeaders[1];
			header = new ImportDHRCreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, "DHR", stream);
				Factory.Save();
			}

			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entry2NumDHR = entry2.EntryNumbers.AddNew();
			entry2NumDHR.CE_EntryType = ElectronicDocumentTypeList.Codes._DHR;
			entry2NumDHR.CE_EntryNum = "1001";
			entry2NumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals((short)3, entry1.CH_HighestFTASequenceNumber);
			AssertEquals((short)1, entry2.CH_HighestFTASequenceNumber);
			AssertEquals((short)1, invoiceLine3.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals((short)4, invoiceLine4.CusEntryLine.CL_FTASequenceNumber);

			header = new ImportDHRCreator().Create(entry1);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry1, "DHR", stream);
				Factory.Save();
			}

			entry1NumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entry1NumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_JZ = invoice1.PK;
			invoiceLine5.JI_Tariff = "5";
			invoiceLine5.JI_LineNo = 4;
			invoiceLine5.JI_PrimaryPreference = "FCN";
			invoiceLine5.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_JZ = invoice1.PK;
			invoiceLine6.JI_Tariff = "6";
			invoiceLine6.JI_LineNo = 5;
			invoiceLine6.JI_PrimaryPreference = "FCN";
			invoiceLine6.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			AssertEquals((short)4, entry1.CH_HighestFTASequenceNumber);
			AssertEquals(5, entry1.MergedLines.Count);
			AssertEquals(5u, invoiceLine5.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals(6u, invoiceLine6.CusEntryLine.CL_FTASequenceNumber);

			invoiceLine5.JI_LineNo = 5;
			invoiceLine6.JI_LineNo = 4;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(6u, invoiceLine5.CusEntryLine.CL_FTASequenceNumber);
			AssertEquals(5u, invoiceLine6.CusEntryLine.CL_FTASequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
		}

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);
		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(ExportEntryCreationStrategy), typeof(ImportEntryCreationStrategy), typeof(LocalExportEntryCreationStrategy), typeof(LocalExportEntryCreationStrategy), typeof(PIDEntryCreationStrategy), typeof(D87EntryCreationStrategy), typeof(ValuationDeclarationEntryCreationStrategy) };
		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);
		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override string GetClearStatus() => CustomsMessageStatusTypeList.Codes.OriginalAccepted;

		void AssertChangedMessageType(JobDeclaration declaration, ZString changeType, ZString entryNum, string subType = "")
		{
			declaration.JE_MessageType = changeType;
			declaration.JE_MessageSubType = subType;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			Factory.Save();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var newEntry = declaration.CustomsEntryHeaders[0];
			AssertNotEquals(oldEntry, newEntry);
			Assert(oldEntry.IsDeleted);
			AssertEquals(entryNum, newEntry.EntryNumber);

			oldEntry = newEntry;
		}
		CusEntryHeader oldEntry;
	}
}
