using System;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IE429ProcessorTest : DeltaIEBaseProcessorTest<CC429BType, IE429Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>STA<br><strong>Status Date: </strong>2023-04-19T23:28:57<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>MRN: </strong>MRN099999999</p><style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table><tr><td rowspan=""4"" colspan=""1""><strong><p style=""font-size: 120%"">Control Result</p></strong></td><td rowspan=""1"" colspan=""3"">Code</td><td rowspan=""1"" colspan=""1"">A1 Satisfying</td></tr><tr><td rowspan=""1"" colspan=""3"">Date</td><td rowspan=""1"" colspan=""1"">2411-60-72</td></tr><tr><td rowspan=""1"" colspan=""3"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfasdfadsf</td></tr><tr><td rowspan=""1"" colspan=""3"">Pending Sampling Results</td><td rowspan=""1"" colspan=""1"">No</td></tr><tr><td rowspan=""62"" colspan=""1""><strong><p style=""font-size: 120%"">Control Results</p></strong></td><td rowspan=""1"" colspan=""3"">Sequence Number</td><td rowspan=""1"" colspan=""1"">4250</td></tr><tr><td rowspan=""1"" colspan=""3"">Declaration Goods Item Number</td><td rowspan=""1"" colspan=""1"">85</td></tr><tr><td rowspan=""1"" colspan=""3"">Control Result Code</td><td rowspan=""1"" colspan=""1"">A1 Satisfying</td></tr><tr><td rowspan=""28"" colspan=""1""><strong><p style=""font-size: 120%"">Results of Control</p></strong></td><td rowspan=""1"" colspan=""2"">Sequence Number</td><td rowspan=""1"" colspan=""1"">967</td></tr><tr><td rowspan=""1"" colspan=""2"">Risk Area Code</td><td rowspan=""1"" colspan=""1"">1003000 Explosifs</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Type</td><td rowspan=""1"" colspan=""1"">45 Sampling</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Date</td><td rowspan=""1"" colspan=""1"">0746-86-90</td></tr><tr><td rowspan=""1"" colspan=""2"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfghjkjll</td></tr><tr><td rowspan=""12"" colspan=""1""><strong><p style=""font-size: 120%"">Control Details</p></strong></td><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">2945</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">qrewqwrqrwe</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">dfsfaad</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfadsffdasad</td></tr><tr><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">432</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">xcvcxxv</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">sdfsf</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdad</td></tr><tr><td rowspan=""1"" colspan=""2"">Sequence Number</td><td rowspan=""1"" colspan=""1"">967</td></tr><tr><td rowspan=""1"" colspan=""2"">Risk Area Code</td><td rowspan=""1"" colspan=""1"">402000 Chemicals and poison</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Type</td><td rowspan=""1"" colspan=""1"">45 Sampling</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Date</td><td rowspan=""1"" colspan=""1"">0746-86-90</td></tr><tr><td rowspan=""1"" colspan=""2"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfghjkjll</td></tr><tr><td rowspan=""6"" colspan=""1""><strong><p style=""font-size: 120%"">Control Details</p></strong></td><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">2342</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">qrewqwrqrwe</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">dfsfaad</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfadsffdasad</td></tr><tr><td rowspan=""1"" colspan=""3"">Sequence Number</td><td rowspan=""1"" colspan=""1"">3455</td></tr><tr><td rowspan=""1"" colspan=""3"">Declaration Goods Item Number</td><td rowspan=""1"" colspan=""1"">85</td></tr><tr><td rowspan=""1"" colspan=""3"">Control Result Code</td><td rowspan=""1"" colspan=""1"">A1 Satisfying</td></tr><tr><td rowspan=""28"" colspan=""1""><strong><p style=""font-size: 120%"">Results of Control</p></strong></td><td rowspan=""1"" colspan=""2"">Sequence Number</td><td rowspan=""1"" colspan=""1"">967</td></tr><tr><td rowspan=""1"" colspan=""2"">Risk Area Code</td><td rowspan=""1"" colspan=""1"">402000 Chemicals and poison</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Type</td><td rowspan=""1"" colspan=""1"">45 Sampling</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Date</td><td rowspan=""1"" colspan=""1"">0746-86-90</td></tr><tr><td rowspan=""1"" colspan=""2"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfghjkjll</td></tr><tr><td rowspan=""12"" colspan=""1""><strong><p style=""font-size: 120%"">Control Details</p></strong></td><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">2945</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">qrewqwrqrwe</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">dfsfaad</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfadsffdasad</td></tr><tr><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">432</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">xcvcxxv</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">sdfsf</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdad</td></tr><tr><td rowspan=""1"" colspan=""2"">Sequence Number</td><td rowspan=""1"" colspan=""1"">967</td></tr><tr><td rowspan=""1"" colspan=""2"">Risk Area Code</td><td rowspan=""1"" colspan=""1"">1003000 Explosifs</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Type</td><td rowspan=""1"" colspan=""1"">45 Sampling</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Date</td><td rowspan=""1"" colspan=""1"">0746-86-90</td></tr><tr><td rowspan=""1"" colspan=""2"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfghjkjll</td></tr><tr><td rowspan=""6"" colspan=""1""><strong><p style=""font-size: 120%"">Control Details</p></strong></td><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">2342</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">qrewqwrqrwe</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">dfsfaad</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1"">asdfadsffdasad</td></tr><table>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.Released;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessageWithoutLRN.json");

		protected override ZString GetMessageTextForFees() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessageWithTaxes.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>STA<br><strong>Status Date: </strong>2023-04-19T23:28:57<br><strong>MRN: </strong>MRN099999999</p><style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table><tr><td rowspan=""4"" colspan=""1""><strong><p style=""font-size: 120%"">Control Result</p></strong></td><td rowspan=""1"" colspan=""3"">Code</td><td rowspan=""1"" colspan=""1"">IY </td></tr><tr><td rowspan=""1"" colspan=""3"">Date</td><td rowspan=""1"" colspan=""1"">2411-60-72</td></tr><tr><td rowspan=""1"" colspan=""3"">Remarks</td><td rowspan=""1"" colspan=""1""> *%I(L#/%lqS/kXmN""B7DOhL:^5w fEv$srBc#@.G}.a<r86if-Xjr!-?(|r=-GVKJ9>mF;xMkqR*H3-@];B{mg>C8SH\mf$gQ[4F'$O}roznxz[~>6R@22""uFm671atMcyChyj08RMSImRWl>x%<e1+H9g4\=\*b>WRj!u|P2-d,gMHdi03F{ntvjUN\QR$qw=@c,O)]%t/w}nZb3D~rG|T7 sVt7'C;;RTPHQR8.XBL#jH5:YUtNMcFXT6LMh[&Wve[{p3POu</td></tr><tr><td rowspan=""1"" colspan=""3"">Pending Sampling Results</td><td rowspan=""1"" colspan=""1"">No</td></tr><table>");

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 3;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-04-19 23:28:57";

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZDateTime GetExpectedCustomsEntryIssueDate() => new ZDateTime(2021, 06, 01, 00, 00, 00);

		protected override ZDateTime GetExpectedReleaseDate() => new ZDateTime(2021, 05, 1);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult, "CL047");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult, "A1", "Satisfying", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "CL716");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "45", "Sampling", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "CL740");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "1003000", "Explosifs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "CL740");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "402000", "Chemicals and poison", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfDiscrepancies, "CL790");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfDiscrepancies, "D1", "Additional quantities", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessageForMissingField.json");

		public void TestUpdateAdditionalDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CorrelationID = GetExpectedLRN();
			entry.CH_CEI_Instruction = instruction.PK;

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());

			CombineAssertions("For Standard Declaration,", () =>
			{
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				AssertEquals("Prerequisite: IsSimplified should be false for standard declaration.", false, instruction.IsSimplified);
				processor.ProcessMessage(message);
				AssertEquals($"CEI_SubStyle must be updated from '{EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA}' to '{EntrySubStyleList.Codes.NormalDeclaration}'.", EntrySubStyleList.Codes.NormalDeclaration, instruction.CEI_SubStyle);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
				processor.ProcessMessage(message);
				AssertEquals($"CEI_SubStyle should remain unchanged when it is not {EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA}.", EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, instruction.CEI_SubStyle);
			});

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CorrelationID = GetExpectedLRN();
			entry2.CH_CEI_Instruction = instruction.PK;

			CombineAssertions("For Simplified Declaration,", () =>
			{
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				AssertEquals("Prerequisite: IsSimplified should be true for simplified declaration.", true, instruction.IsSimplified);
				processor.ProcessMessage(message);
				AssertEquals($"CEI_SubStyle must be updated from '{EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC}' to '{EntrySubStyleList.Codes.SimplifiedDeclaration}'.", EntrySubStyleList.Codes.SimplifiedDeclaration, instruction.CEI_SubStyle);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				processor.ProcessMessage(message);
				AssertEquals($"CEI_SubStyle should remain unchanged when it is not {EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC}.", EntrySubStyleList.Codes.SimplifiedDeclaration, instruction.CEI_SubStyle);
			});
		}

		public void TestUpdateTemporaryStorageIfApplicable()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ_IST1";
			ist.DDTNumber = "DDT1";
			ist.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var istLine = ist.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine.TSL_PackageQty = 100;
			istLine.TSL_PackageType = "1A";
			istLine.TSL_GrossWeight = 1000;
			istLine.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = ist.DDTNumber;
			registerHeader.SRH_InternalReference = ist.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;
			registerLine.SRL_PackagesRemaining = 100;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;
			oblTransaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			oblTransaction.SRT_Reference = "FRJ_IST1";
			AssertEquals("Transaction count after adding an opening balance transaction.", 1, registerLine.CusTempStorageRegLineTransactions.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CorrelationID = GetExpectedLRN();
			cusEntryHeader.CH_BGMReference = "5FR12345B00176178";
			cusEntryHeader.CH_SequenceNumber = 1;

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "FRJ_IST1";
			previousDocument.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackQty = 10;
			previousDocument.CSI_Quantity = 500m;
			previousDocument.CSI_UnitOfQuantity = "KGM";

			var previousIST = cusEntryHeader.MergedLines[0].PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist, previousIST);

			var processor = GetDeltaIEBaseProcessor();
			var message1 = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			processor.ProcessMessage(message1);
			Factory.Save();

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertEquals("Transaction count after processing the first IE429 response.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			CombineAssertions("Assert created transaction values.", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals("5FR12345B00176178", createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(500m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-10, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(GetExpectedEntryNumber(), createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			var message2 = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			processor.ProcessMessage(message2);
			Factory.Save();

			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			AssertEquals("No new transaction after processing the second IE429 response.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No email should have been sent.", 0, mails.Count);
		}

		public void TestUpdateTemporaryStorageIfApplicable_LogsAndEmailNotification()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ_IST1";
			ist.DDTNumber = "DDT1";
			ist.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var istLine = ist.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine.TSL_PackageQty = 100;
			istLine.TSL_PackageType = "1A";
			istLine.TSL_GrossWeight = 1000;
			istLine.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = ist.DDTNumber;
			registerHeader.SRH_InternalReference = ist.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;
			registerLine.SRL_PackagesRemaining = 100;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;
			oblTransaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			oblTransaction.SRT_Reference = "FRJ_IST1";
			AssertEquals("Transaction count after adding an opening balance transaction.", 1, registerLine.CusTempStorageRegLineTransactions.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CorrelationID = GetExpectedLRN();
			cusEntryHeader.CH_BGMReference = "5FR12345B00176178";
			cusEntryHeader.CH_SequenceNumber = 1;

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "FRJ_IST1";
			previousDocument.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackQty = 200;
			previousDocument.CSI_Quantity = 500m;
			previousDocument.CSI_UnitOfQuantity = "KGM";

			var previousIST = cusEntryHeader.MergedLines[0].PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist, previousIST);

			var processor = GetDeltaIEBaseProcessor();
			var message1 = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			processor.ProcessMessage(message1);
			Factory.Save();

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			AssertEquals("No new transaction should be created as previousDocument CSI_PackQty exceeds the available temporary storage capacity.", 1, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			Assert("CusEntryHeader logs should be captured.", cusEntryHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains($"Register {reloadedRegisterHeader.SRH_Reference} has not been updated")));

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions("Assert email notification.", () =>
			{
				AssertEquals("An email should be sent.", 1, mails.Count);
				AssertEquals("Warning: New Delta I response received. Declaration: B00001000 IST Reference: DDT1", mails[0].Subject);
				AssertEquals(@"A Delta I response has been received. Temporary Storage: DDT1 has not been updated by Declaration: B00001000. There are not enough packages remaining.", mails[0].Body);
			});
		}
	}
}
