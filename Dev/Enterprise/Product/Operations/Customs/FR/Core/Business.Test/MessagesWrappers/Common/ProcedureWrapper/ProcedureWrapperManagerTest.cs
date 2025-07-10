using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class ProcedureWrapperManagerTest : TestCaseWithFactory
	{
		public void TestNewProcedureWrapper()
		{
			var message = CreateMessageObject(OrgCusAccountDeltaGTypeList.Codes.G1);
			var declaration = message.Header.Declaration;
			var errorCollector = new EU.Business.ErrorCollector();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryInstruction = Factory.New<Customs.Business.CusEntryInstruction>();
			entryInstruction.CEI_SubStyle = "A";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(1971, 9, 18, 20, 15, 12);

			var messageDate = ZDate.Empty;

			#region DeltaC
			var actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT).ToString();
			var procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<ANTProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be equal to the one of the entryinstruction CEI_DateForDuty for ANT.", procedureWrapperTest.EstimatedAssessmentDate, ZDateTime.BrettsBirthday.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
			AssertEquals("EstimatedAssessmentHour should be equal to the one of the entryinstruction CEI_DateForDuty for ANT.", procedureWrapperTest.EstimatedAssessmentHour, entryInstruction.CEI_DateForDuty.ToShortTimeString());
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDV).ToString();
			var mdvProcedureWrapper = new MDVProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertEquals("EstimatedAssessmentDate should be equal to the one of the entryinstruction CEI_DateForDuty for MDV.", mdvProcedureWrapper.EstimatedAssessmentDate, ZDateTime.BrettsBirthday.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
			AssertEquals("EstimatedAssessmentHour should be equal to the one of the entryinstruction CEI_DateForDuty for MDV.", mdvProcedureWrapper.EstimatedAssessmentHour, entryInstruction.CEI_DateForDuty.ToShortTimeString());

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDA).ToString();
			var mdaProcedureWrapper = new MDAProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertEquals("EstimatedAssessmentDate should be equal to the one of the entryinstruction CEI_DateForDuty for MDA.", mdaProcedureWrapper.EstimatedAssessmentDate, ZDateTime.BrettsBirthday.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
			AssertEquals("EstimatedAssessmentHour should be equal to the one of the entryinstruction CEI_DateForDuty for MDA.", mdaProcedureWrapper.EstimatedAssessmentHour, entryInstruction.CEI_DateForDuty.ToShortTimeString());

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<VALProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for VAL.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for VAL.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MAP).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<MAPProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be equal to the one of the entryinstruction CEI_DateForDuty for MAP.", procedureWrapperTest.EstimatedAssessmentDate, ZDateTime.BrettsBirthday.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
			AssertEquals("EstimatedAssessmentHour should be equal to the one of the entryinstruction CEI_DateForDuty for MAP.", procedureWrapperTest.EstimatedAssessmentHour, entryInstruction.CEI_DateForDuty.ToShortTimeString());
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANA).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<ANAProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for ANA.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for ANA.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<VAAProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for VAA.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for VAA.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.EAV).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<EAVProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for EAV.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for EAV.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<INVProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for INV.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for INV.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.CMP).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<CMPProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for CMP.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for CMP.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<RPSProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for RPS.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for RPS.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == messageDate.ToString("dd/MM/yyyy"), true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<RECProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for REC.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for REC.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAR).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<VARProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for VAR.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for VAR.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANR).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<ANRProcedureWrapper>(procedureWrapperTest);
			AssertEquals(procedureWrapperTest.EntryStyleCode == "A", true);
			AssertEquals("EstimatedAssessmentDate should be empty for ANR.", procedureWrapperTest.EstimatedAssessmentDate, ZString.Empty);
			AssertEquals("EstimatedAssessmentHour should be empty for ANR.", procedureWrapperTest.EstimatedAssessmentHour, ZString.Empty);
			AssertEquals(procedureWrapperTest.DeclEmergencyProcDate == ZString.Empty, true);
			#endregion

			message = CreateMessageObject(OrgCusAccountDeltaGTypeList.Codes.G2);
			declaration = message.Header.Declaration;
			entryHeader = declaration.CustomsEntryHeaders[0];

			#region DeltaD
			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<VALProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.D2M, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<D2MProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<ANTProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDV, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<MDVProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<RPSProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDA, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<MDAProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<VAAProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANN, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<ANNProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<RECProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV, entryHeader.Declaration.IsDeltaC).ToString();
			AssertType<INVProcedureWrapper>(ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector));
			#endregion
		}

		public void TestWithNoEntryInstruction()
		{
			var message = CreateMessageObject(OrgCusAccountDeltaGTypeList.Codes.G1);
			var declaration = message.Header.Declaration;
			var errorCollector = new EU.Business.ErrorCollector();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryHeader.CH_CEI_Instruction = ZGuid.Empty;
			var messageDate = ZDate.Empty;

			var actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT).ToString();
			var procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<ANTProcedureWrapper>(procedureWrapperTest);
			AssertNullOrEmpty("date is empty as there is no entryinstruction for ANT.", procedureWrapperTest.EstimatedAssessmentDate);
			AssertNullOrEmpty("hour is empty as there is no entryinstruction for ANT.", procedureWrapperTest.EstimatedAssessmentHour);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDV).ToString();
			var mDVProcedureWrapper = new MDVProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertNullOrEmpty("EstimatedAssessmentDate is empty as there is no entryinstruction for MDV.", mDVProcedureWrapper.EstimatedAssessmentDate);
			AssertNullOrEmpty("EstimatedAssessmentHour is empty as there is no entryinstruction for MDV.", mDVProcedureWrapper.EstimatedAssessmentHour);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDA).ToString();
			var mDAProcedureWrapper = new MDAProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertNullOrEmpty("EstimatedAssessmentDate is empty as there is no entryinstruction for MDA.", mDAProcedureWrapper.EstimatedAssessmentDate);
			AssertNullOrEmpty("EstimatedAssessmentHour is empty as there is no entryinstruction for MDA.", mDAProcedureWrapper.EstimatedAssessmentHour);

			actionCode = EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MAP).ToString();
			procedureWrapperTest = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);
			AssertType<MAPProcedureWrapper>(procedureWrapperTest);
			AssertNullOrEmpty("EstimatedAssessmentDate is empty as there is no entryinstruction for MAP.", procedureWrapperTest.EstimatedAssessmentDate);
			AssertNullOrEmpty("EstimatedAssessmentHour is empty as there is no entryinstruction for MAP.", procedureWrapperTest.EstimatedAssessmentHour);
		}

		public void TestDeclEmergencyProcDate()
		{
			AssertDeclEmergencyProcDateIsFromSpecialMention(EntryActionCodeList.Codes.ANT, OrgCusAccountDeltaGTypeList.Codes.G1);

			AssertDeclEmergencyProcDateIsFromSpecialMention(EntryActionCodeList.Codes.VAL, OrgCusAccountDeltaGTypeList.Codes.G1);

			AssertDeclEmergencyProcDateIsFromSpecialMention(EntryActionCodeList.Codes.MAP, OrgCusAccountDeltaGTypeList.Codes.G1);

			AssertDeclEmergencyProcDateIsFromSpecialMention(EntryActionCodeList.Codes.REC, OrgCusAccountDeltaGTypeList.Codes.G1);

			AssertDeclEmergencyProcDateIsFromSpecialMention(EntryActionCodeList.Codes.MDA, OrgCusAccountDeltaGTypeList.Codes.G2);
		}

		void AssertDeclEmergencyProcDateIsFromSpecialMention(string actionCode, string deltaGType)
		{
			var isDeltaC = deltaGType == OrgCusAccountDeltaGTypeList.Codes.G1;
			actionCode = EntryActionCodeList.GetMessageCodeNumber(actionCode, isDeltaC).ToString();
			var message = CreateMessageObject(deltaGType);
			var declaration = message.Header.Declaration;
			var errorCollector = new EU.Business.ErrorCollector();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryInstruction = Factory.New<Customs.Business.CusEntryInstruction>();
			entryInstruction.CEI_SubStyle = "A";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(1971, 9, 18, 20, 15, 12);
			var messageDate = ZDate.Empty;

			var procedureWrapper = ProcedureWrapperManager.NewProcedureWrapper(entryHeader, actionCode, messageDate, errorCollector);

			AssertEquals(string.Empty, procedureWrapper.DeclEmergencyProcDate);

			var additionalInfo1 = entryHeader.Declaration.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "51000";
			additionalInfo1.CSI_DateOfIssue = ZDate.Today;

			var additionalInfo2 = entryHeader.Declaration.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "52000";
			additionalInfo2.CSI_DateOfIssue = ZDate.Today.AddDays(-1);

			AssertEquals(additionalInfo1.CSI_DateOfIssue.ToString("dd/MM/yyyy"), procedureWrapper.DeclEmergencyProcDate);

			additionalInfo2.CSI_Code = "51000";
			AssertEquals(additionalInfo2.CSI_DateOfIssue.ToString("dd/MM/yyyy"), procedureWrapper.DeclEmergencyProcDate);
		}

		MessageSending.DeltaGJobDeclarationMessageSendingObject CreateMessageObject(string whichDelta)
		{
			var declaration = CreateDeclaration(whichDelta);

			var cusEntryHeader = declaration.CustomsEntryHeaders[0];

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);

			return messageObject;
		}
		JobDeclaration CreateDeclaration(string whichDelta)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var declarantAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;

			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			#region Organisation Registration number : SRT

			var orgCusCodeSrt = Factory.New<OrgCusCode>();
			orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeSrt.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
			orgCusCodeSrt.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeSrt);

			#endregion

			#region Organisation Registration number : CBR

			var orgCusCodeCbr = Factory.New<OrgCusCode>();
			orgCusCodeCbr.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeCbr.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			orgCusCodeCbr.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeCbr.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeCbr.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeCbr);

			#endregion

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.FillWithValidTestData();

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var countryCodeImporter = Factory.NewWithValidTestData<RefCountry>();

			declaration.Importer.FillWithValidTestData();
			declaration.Importer.MainAddress.OA_PostCode = "24130";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_DeltaMode = whichDelta;

			var shutUp = new SendsMessagesToCustomsShutterUpperer();

			var mergeResult = declaration.DoMerge(shutUp);

			return declaration;
		}
	}
}
