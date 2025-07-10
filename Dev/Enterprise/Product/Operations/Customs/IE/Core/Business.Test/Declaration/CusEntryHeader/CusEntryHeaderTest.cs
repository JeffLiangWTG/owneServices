using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
	{
		public void TestEntryHeaderStatusDescription()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			entry.CH_Status = string.Empty;
			AssertEquals("Entry status description should be empty", string.Empty, entry.EntryHeaderStatusDescription);
		}

		public void TestEntryNumberValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("21IEDUB11A782454R2", ZDateTime.BrettsBirthday, "ACC");
			AssertEquals("21IEDUB11A782454R2", entry.EntryNumber);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("21IEDUB11A782454R2", entry.EntryNumber);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertNotEquals("21IEDUB11A782454R2", entry.EntryNumber);
		}

		public void TestIMessageAttacheeMembers()
		{
			var branch1 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_GB = branch1.PK;
			var cusAgent = Factory.New<GlbStaff>();
			cusAgent.GS_Code = "!2#";
			declaration.JE_GS_NKCusAgent = "!2#";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "MS1";
			entry.CH_EntryStatus = "ES2";
			var instruction = Factory.New<CusEntryInstruction>();
			entry.CH_CEI_Instruction = instruction.PK;
			var line = entry.MergedLines.AddNew();
			line.CL_LineNumber = 10;

			IAISMessageAttachee messageAttachee = entry;

			CombineAssertions("IAISMessageAttachee Properties", () =>
			{
				AssertEquals("PK", entry.PK, messageAttachee.PK);
				AssertEquals("TableName", CusEntryHeaderSchema.Constants.TableName, messageAttachee.TableName);
				AssertEquals("TablePrefix", CusEntryHeaderSchema.Constants.Prefix, messageAttachee.TablePrefix);
				AssertEquals("Branch", branch1, messageAttachee.Branch);
				AssertEquals("CustomsAgent", cusAgent, messageAttachee.CustomsAgent);
				AssertEquals("RelatedJob", declaration, messageAttachee.RelatedJob);
				AssertEquals("Factory", Factory, messageAttachee.Factory);
				AssertEquals("LogicalStatus", "MS1", messageAttachee.LogicalStatus);
				AssertEquals("EntryStatus", "ES2", messageAttachee.EntryStatus);
				AssertEquals("RequestedDocumentsProvider", instruction, messageAttachee.RequestedDocumentsProvider);
				AssertEquals("Logs", entry.Logs, messageAttachee.Logs);
			});

			var goodsItem = new GoodsShipmentItemIm404Type();
			goodsItem.DeclarationGoodsItemNumber = "10";
			goodsItem.CalculationOfTaxes = new MCalculationOfTaxesType04();
			goodsItem.CalculationOfTaxes.DutiesAndTaxes = new Collection<MDutiesAndTaxesType03>
			{
				new MDutiesAndTaxesType03()
				{
					TaxType = "TAX",
				},
			};
			messageAttachee.PopulateConfirmedDutiesAndTaxes(new[] { new IM404GoodsItemProvider(goodsItem) });
			messageAttachee.SetCustomsRegistrationNumber("ABC");
			messageAttachee.SetEntryReleaseDate(new ZDateTime(2024, 1, 23));

			CombineAssertions("IAISMessageAttachee methods update header and children fields", () =>
			{
				AssertEquals("Should create 1 child CusEntryLineFee", 1, line.ConfirmedFees.Count);
				AssertEquals("CF_ChargeType", "TAX", line.ConfirmedFees.OfType<CusEntryLineFee>().Single().CF_ChargeType);
				AssertEquals("CRN", "ABC", entry.CRN);
				AssertEquals(new ZDateTime(2024, 1, 23), entry.CH_EntryReleaseDate);
			});
		}

		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var lookups = entry.Lookups;
			AssertType<ExportCusEntryHeaderLookups>(lookups);
			AssertNotSame("not cached", lookups, entry.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			lookups = entry.Lookups;
			AssertType<ImportCusEntryHeaderLookups>(lookups);
			AssertNotSame("not cached", lookups, entry.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			entry = declaration.CustomsEntryHeaders.AddNew();
			lookups = entry.Lookups;
			AssertType<CusEntryHeaderLookups>(lookups);
			AssertNotSame("not cached", lookups, entry.Lookups);
		}

		public void TestDuplicationPossibleImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REJ";
			AssertEquals("Warning should not be displayed when entry status is REJ", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "ACC";
			AssertEquals("Warning should be displayed when entry status is ACC", true, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "CAN";
			AssertEquals("Warning should not be displayed when entry status is CAN", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "NOT";
			AssertEquals("Warning should not be displayed when entry status is NOT", false, entryHeader.DuplicationPossible);
		}

		public void TestDuplicationPossibleExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REJ";
			AssertEquals("Warning should not be displayed when entry status is REJ", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "ACC";
			AssertEquals("Warning should be displayed when entry status is ACC", true, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REF";
			AssertEquals("Warning should not be displayed when entry status is REF", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "DRJ";
			AssertEquals("Warning should not be displayed when entry status is DRJ", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "CAN";
			AssertEquals("Warning should not be displayed when entry status is CAN", false, entryHeader.DuplicationPossible);
		}

		public void TestSetAsFailedFromTransmission()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock
				.Protected()
				.Setup<bool>("TaxFeePaymentCodeIsDeferredCore", ItExpr.IsAny<ZString>())
				.Returns<ZString>(target =>
				{
					return target == "X" || target == "Y";
				});
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

		public override void TestAdditionalInfos()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MergeBy = "TRF";
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var instruction = dec.CustomsEntryInstructions.AddNew();
			invLine1.JI_CEI = instruction.PK;

			var addInfo1 = inv.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "9002";
			addInfo1.CSI_Description = "9002 Desc";

			var addInfo2 = inv.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "9002";
			addInfo2.CSI_Description = "9002 Desc";

			var addInfo3 = inv.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "9003";
			addInfo3.CSI_Description = "9003 Desc";

			var addinfo4 = instruction.AdditionalInfos.AddNew();
			addinfo4.CSI_Code = "9004";
			addinfo4.CSI_Description = "9004 Desc";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("ActiveEntryHeaders", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("AdditionalInfos", 3, ((CusEntryHeader)dec.ActiveEntryHeaders[0]).AdditionalInfos.Count());
		}

		public void TestDeclaration()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			AssertType<JobDeclaration>(entry.Declaration);
		}

		public void TestEntryInstructionType()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			AssertType<CusEntryInstruction>(entry.EntryInstruction);
		}

		public void TestMergedLines()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			AssertType<CusEntryLineCollection<CusEntryLine>>(entry.MergedLines);
		}

		public void TestAllEntryLines()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			AssertType<AllCusEntryLineCollection<CusEntryLine>>(entry.AllEntryLines);
		}

		public new void TestWorkflowSupportableBusinessObject()
		{
			Assert("We no longer support workflow on CusEntryHeader", true);
		}

		public void TestSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions[0];
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderSupDoc1 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderSupDoc1.CSI_Code = "111";
			invHeaderSupDoc1.CSI_ReferenceNumber = "111";

			var invHeaderSupDoc2 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderSupDoc2.CSI_Code = "222";
			invHeaderSupDoc2.CSI_ReferenceNumber = "222";

			var invLineSupDoc = invoiceLine.SupportingDocuments.AddNew();
			invLineSupDoc.CSI_Code = "444";
			invLineSupDoc.CSI_ReferenceNumber = "444";

			var instructionSupDoc = instruction.SupportingDocuments.AddNew();
			instructionSupDoc.CSI_Code = "555";
			instructionSupDoc.CSI_ReferenceNumber = "555";

			var supportingDocuments = entryHeader.SupportingDocuments.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header Sup Doc Count", 3, supportingDocuments.Length);
			AssertEquals("aSupDocOnInvHeader.CSI_Code = 111", "111", supportingDocuments[0].CSI_Code);
			AssertEquals("aSupDocOnInvHeader.CSI_Code = 222", "222", supportingDocuments[1].CSI_Code);
			AssertEquals("aSupDocOnInstruction.CSI_Code = 555", "555", supportingDocuments[2].CSI_Code);
		}

		public void TestPreviousDocuments_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions[0];
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderPrevDoc1 = invoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc1.CSI_Code = "111";
			invHeaderPrevDoc1.CSI_ReferenceNumber = "111";

			var invHeaderPrevDoc2 = invoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc2.CSI_Code = "222";
			invHeaderPrevDoc2.CSI_ReferenceNumber = "222";

			var invLinePrevDoc = invoiceLine.PreviousDocuments.AddNew();
			invLinePrevDoc.CSI_Code = "444";
			invLinePrevDoc.CSI_ReferenceNumber = "444";

			var instructionPrevDoc1 = instruction.PreviousDocuments.AddNew();
			instructionPrevDoc1.CSI_Code = "555";
			instructionPrevDoc1.CSI_ReferenceNumber = "555";

			var instructionPrevDoc2 = instruction.PreviousDocuments.AddNew();
			instructionPrevDoc2.CSI_Code = "666";
			instructionPrevDoc2.CSI_ReferenceNumber = "666";

			var previousDocuments = entryHeader.PreviousDocuments.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header Prev Doc Count", 4, previousDocuments.Length);
			AssertEquals("aPrevDocOnInvHeader.CSI_Code = 111", "111", previousDocuments[0].CSI_Code);
			AssertEquals("aPrevDocOnInvHeader.CSI_Code = 222", "222", previousDocuments[1].CSI_Code);
			AssertEquals("aPrevDocOnInstruction.CSI_Code = 555", "555", previousDocuments[2].CSI_Code);
			AssertEquals("aPrevDocOnInstruction.CSI_Code = 666", "666", previousDocuments[3].CSI_Code);
		}

		public void TestPreviousDocuments_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderPrevDoc1 = invoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc1.CSI_Code = "111";
			invHeaderPrevDoc1.CSI_ReferenceNumber = "111";

			var invHeaderPrevDoc2 = invoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc2.CSI_Code = "222";
			invHeaderPrevDoc2.CSI_ReferenceNumber = "222";

			var declarationPrevDoc = instruction.PreviousDocuments.AddNew();
			declarationPrevDoc.CSI_Code = "333";
			declarationPrevDoc.CSI_ReferenceNumber = "333";

			var previousDocuments = entryHeader.PreviousDocuments.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Base - Entry Header Prev Doc Count", 3, previousDocuments.Length);
			AssertEquals("Base - aPrevDocOnInvHeader.CSI_Code = 111", "111", previousDocuments[0].CSI_Code);
			AssertEquals("Base - aPrevDocOnInvHeader.CSI_Code = 222", "222", previousDocuments[1].CSI_Code);
			AssertEquals("Base - aPrevDocOnInstruction.CSI_Code = 333", "333", previousDocuments[2].CSI_Code);
		}

		public void TestSupportingDocuments_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions[0];
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderDoc1 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderDoc1.CSI_Code = "111";
			invHeaderDoc1.CSI_ReferenceNumber = "111";

			var invHeaderDoc2 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderDoc2.CSI_Code = "111";
			invHeaderDoc2.CSI_ReferenceNumber = "111";

			var invLineDoc = invoiceLine.SupportingDocuments.AddNew();
			invLineDoc.CSI_Code = "333";
			invLineDoc.CSI_ReferenceNumber = "333";

			var instructionDoc1 = instruction.SupportingDocuments.AddNew();
			instructionDoc1.CSI_Code = "444";
			instructionDoc1.CSI_ReferenceNumber = "444";

			var instructionDoc2 = instruction.SupportingDocuments.AddNew();
			instructionDoc2.CSI_Code = "555";
			instructionDoc2.CSI_ReferenceNumber = "555";

			var supportingDocuments = entryHeader.SupportingDocuments.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header  Doc Count", 3, supportingDocuments.Length);
			AssertEquals("aDocOnInvHeader.CSI_Code = 111", "111", supportingDocuments[0].CSI_Code);
			AssertEquals("aDocOnInvInstruction.CSI_Code = 444", "444", supportingDocuments[1].CSI_Code);
			AssertEquals("aDocOnInvInstruction.CSI_Code = 555", "555", supportingDocuments[2].CSI_Code);
		}

		public void TestSupportingDocuments_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderDoc1 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderDoc1.CSI_Code = "111";
			invHeaderDoc1.CSI_ReferenceNumber = "111";

			var invHeaderDoc2 = invoiceHeader.SupportingDocuments.AddNew();
			invHeaderDoc2.CSI_Code = "111";
			invHeaderDoc2.CSI_ReferenceNumber = "111";

			var invLineDoc = invoiceLine.SupportingDocuments.AddNew();
			invLineDoc.CSI_Code = "333";
			invLineDoc.CSI_ReferenceNumber = "333";

			var instructionDoc1 = instruction.SupportingDocuments.AddNew();
			instructionDoc1.CSI_Code = "444";
			instructionDoc1.CSI_ReferenceNumber = "444";

			var instructionDoc2 = instruction.SupportingDocuments.AddNew();
			instructionDoc2.CSI_Code = "555";
			instructionDoc2.CSI_ReferenceNumber = "555";

			var supportingDocuments = entryHeader.SupportingDocuments.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header  Doc Count", 3, supportingDocuments.Length);
			AssertEquals("aDocOnInvHeader.CSI_Code = 111", "111", supportingDocuments[0].CSI_Code);
			AssertEquals("aDocOnInvInstruction.CSI_Code = 444", "444", supportingDocuments[1].CSI_Code);
			AssertEquals("aDocOnInvInstruction.CSI_Code = 555", "555", supportingDocuments[2].CSI_Code);
		}

		public void TestAdditionalInfos_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions[0];
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderDoc1 = invoiceHeader.AdditionalInfos.AddNew();
			invHeaderDoc1.CSI_Code = "111";
			invHeaderDoc1.CSI_ReferenceNumber = "111";

			var invHeaderDoc2 = invoiceHeader.AdditionalInfos.AddNew();
			invHeaderDoc2.CSI_Code = "111";
			invHeaderDoc2.CSI_ReferenceNumber = "111";

			var invLineDoc = invoiceLine.AdditionalInfos.AddNew();
			invLineDoc.CSI_Code = "333";
			invLineDoc.CSI_ReferenceNumber = "333";

			var instructionDoc1 = instruction.AdditionalInfos.AddNew();
			instructionDoc1.CSI_Code = "444";
			instructionDoc1.CSI_ReferenceNumber = "444";

			var instructionDoc2 = instruction.AdditionalInfos.AddNew();
			instructionDoc2.CSI_Code = "555";
			instructionDoc2.CSI_ReferenceNumber = "555";

			var additionalInfos = entryHeader.AdditionalInfos.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header  Doc Count", 3, additionalInfos.Length);
			AssertEquals("aDocOnInvHeader.CSI_Code = 111", "111", additionalInfos[0].CSI_Code);
			AssertEquals("aDocOnInstruction.CSI_Code = 444", "444", additionalInfos[1].CSI_Code);
			AssertEquals("aDocOnInstruction.CSI_Code = 555", "555", additionalInfos[2].CSI_Code);
		}

		public void TestAdditionalInfos_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var invHeaderDoc1 = invoiceHeader.AdditionalInfos.AddNew();
			invHeaderDoc1.CSI_Code = "111";
			invHeaderDoc1.CSI_ReferenceNumber = "111";

			var invHeaderDoc2 = invoiceHeader.AdditionalInfos.AddNew();
			invHeaderDoc2.CSI_Code = "111";
			invHeaderDoc2.CSI_ReferenceNumber = "111";

			var invLineDoc = invoiceLine.AdditionalInfos.AddNew();
			invLineDoc.CSI_Code = "333";
			invLineDoc.CSI_ReferenceNumber = "333";

			var instructionDoc1 = instruction.AdditionalInfos.AddNew();
			instructionDoc1.CSI_Code = "444";
			instructionDoc1.CSI_ReferenceNumber = "444";

			var instructionDoc2 = instruction.AdditionalInfos.AddNew();
			instructionDoc2.CSI_Code = "555";
			instructionDoc2.CSI_ReferenceNumber = "555";

			var additionalInfos = entryHeader.AdditionalInfos.OrderBy(x => x.CSI_Code).ToArray();
			AssertEquals("Entry Header  Doc Count", 3, additionalInfos.Length);
			AssertEquals("aDocOnInvHeader.CSI_Code = 111", "111", additionalInfos[0].CSI_Code);
			AssertEquals("aDocOnInstruction.CSI_Code = 444", "444", additionalInfos[1].CSI_Code);
			AssertEquals("aDocOnInstruction.CSI_Code = 555", "555", additionalInfos[2].CSI_Code);
		}

		public void TestHasOutgoingExportPresentationMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals("No IE511 present", false, entryHeader.HasExportPresentationMessage);

			var previous511Message = Factory.New<AESInboundEDIMessage>();
			previous511Message.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportPresentation;
			previous511Message.EM_MessageText = "<text>";
			previous511Message.EM_LinkTable = entryHeader.TableName;
			previous511Message.EM_LinkUniqueID = entryHeader.PK;
			previous511Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			previous511Message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(previous511Message);
			AssertEquals("Not in database ", false, entryHeader.HasExportPresentationMessage);
			Factory.Save();
			Assert("IE511 present", entryHeader.HasExportPresentationMessage);
		}

		public void TestHasInvoiceExporterAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("No Invoice ExporterAddress", false, entryHeader.HasInvoiceExporterAddress);

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OA_ExporterAddress = exporter.MainAddress.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("Has Invoice ExporterAddress", true, entryHeader.HasInvoiceExporterAddress);
		}

		public void TestShouldLogEntryStatus()
		{
			AssertEquals(true, Factory.New<CusEntryHeader>().ShouldLogEntryStatus);
		}

		public void TestLogLogicalStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			TestLogging(declaration);

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				TestLogging(declaration);
			}

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				TestLogging(declaration);
			}

			void TestLogging(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				var entryHeaderLogs = entryHeader.Logs.GetAllLogs();
				AssertEquals("Status not set. No event logs", 0, entryHeaderLogs.Count);
				entryHeader.CH_Status = LogicalStatusList.Codes.Invalid;
				Factory.Save();
				AssertEquals("Status set. Event log should be created", 1, entryHeaderLogs.Count);
				CombineAssertions(() =>
				{
					AssertEquals("MSC", entryHeaderLogs[0].Event.SE_Code);
					AssertEquals(LogicalStatusList.Codes.Invalid, entryHeaderLogs[0].SL_Reference);
				});

				Factory.Save();
				AssertEquals("Status not changed, no further log created", 1, entryHeaderLogs.Count);

				entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
				Factory.Save();
				AssertEquals("Status changed, another log created", 2, entryHeaderLogs.Count);
				CombineAssertions(() =>
				{
					AssertEquals("MSC", entryHeaderLogs[1].Event.SE_Code);
					AssertEquals(LogicalStatusList.Codes.Accepted, entryHeaderLogs[1].SL_Reference);
				});
			}
		}

		public void TestLogEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var entryHeaderLogs = entryHeader.Logs.GetAllLogs();
			AssertEquals("Status not set. No event logs", 0, entryHeaderLogs.Count);
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.PendingControl;
			Factory.Save();
			AssertEquals("Entry Status set. Event log should be created", 1, entryHeaderLogs.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CES", entryHeaderLogs[0].Event.SE_Code);
				AssertEquals(AESEntryStatusList.Codes.PendingControl, entryHeaderLogs[0].SL_Reference);
			});

			Factory.Save();
			AssertEquals("Status not changed, no further log created", 1, entryHeaderLogs.Count);

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			Factory.Save();
			AssertEquals("Status changed, another log created", 2, entryHeaderLogs.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CES", entryHeaderLogs[1].Event.SE_Code);
				AssertEquals(AESEntryStatusList.Codes.ReleasedForExport, entryHeaderLogs[1].SL_Reference);
			});
		}

		public new void TestEntryStatusChangingToClearRecordsLog()
		{
			// EU behaves differently than we want in IE
			// EU will create an ECC event on every entry and the declaration, in IE we only want 1 event on the declaration
			// so need to hide base test
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var entryHeader1Logs = entryHeader1.Logs.GetAllLogs();
			var declarationLogs = declaration.Logs.GetAllLogs();
			AssertEquals("Status not set. No event logs", 0, entryHeader1Logs.Count);
			entryHeader1.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			Factory.Save();
			CombineAssertions("1 of 2 entries released. ECC should not be logged until both entries are released", () =>
			{
				AssertEquals("ECC should never be logged on entry header 1", 0, entryHeader1Logs.Where(x => x.Event.SE_Code == "ECC").Count());
				AssertEquals("ECC should not be logged on declaration as all entries are not cleared yet", 0, declarationLogs.Where(x => x.Event.SE_Code == "ECC").Count());
			});

			entryHeader2.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			var entryHeader2Logs = entryHeader2.Logs.GetAllLogs();
			Factory.Save();
			CombineAssertions("Status set for all entries. ECC should be logged on declaration only", () =>
			{
				AssertEquals("ECC should never be logged on entry header 1", 0, entryHeader1Logs.Where(x => x.Event.SE_Code == "ECC").Count());
				AssertEquals("ECC should never be logged on entry header 2", 0, entryHeader2Logs.Where(x => x.Event.SE_Code == "ECC").Count());
				AssertEquals("ECC should be logged on declaration only once", 1, declarationLogs.Where(x => x.Event.SE_Code == "ECC").Count());
			});
		}

		public void TestLogOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var entryHeaderLogs = entryHeader.Logs.GetAllLogs();
			var declarationLogs = declaration.Logs.GetAllLogs();
			AssertEquals("Status not set. No event logs", 0, entryHeaderLogs.Count);
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			Factory.Save();
			AssertEquals("2 logs should be created, one for logical and one for entry status", 2, entryHeaderLogs.Count);

			CombineAssertions(() =>
			{
				AssertEquals("MSC", entryHeaderLogs[0].Event.SE_Code);
				AssertEquals(LogicalStatusList.Codes.Accepted, entryHeaderLogs[0].SL_Reference);
				AssertEquals("CES", entryHeaderLogs[1].Event.SE_Code);
				AssertEquals(AESEntryStatusList.Codes.ReleasedForExport, entryHeaderLogs[1].SL_Reference);
			});
		}

		protected override ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => true;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CEI_Instruction = instruction.PK;
			header.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			header.PendingDeletionEntryLines.AddNew();
			header.AllEntryLines.Load();
			header.PivotsToContainers.RemoveAndDeleteAll();
			header.PivotsToContainers.GetOrCreatePivotFor(header.Declaration.CusContainers.AddNew());
			return header;
		}
	}
}
