using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingObject))]
sealed class JobDeclarationMessageSendingObjectTest : Customs.Business.Testing.JobDeclarationMessageSendingObjectTest
{
	public void TestMessageTypePreFilledDEC()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.DEC, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty);
	}

	public void TestMessageTypePreFilledDEC_FFT()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.DEC, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.DEC, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday, "", "FFT");
	}

	public void TestMessageTypePreFilledPRE()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, ZDateTime.Empty);
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, ZDateTime.Empty);
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, ZDateTime.Empty);
	}

	public void TestMessageTypePreFilledPRE_FFT()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, ZDateTime.Empty, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, ZDateTime.Empty, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, ZDateTime.Empty, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, ZDateTime.BrettsBirthday, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, ZDateTime.BrettsBirthday, "", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, ZDateTime.BrettsBirthday, "", "FFT");
	}

	public void TestMessageTypePreFilledGPR()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty, "I2");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty, "C2");
	}

	public void TestMessageTypePreFilledGPR_FFT()
	{
		SetupImport();
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty, "I2", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday, "I2", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.Empty, "C2", "FFT");
		TestMessageTypePreFilled(ImportSendMessageTypes.Codes.PRE, EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday, "C2", "FFT");
	}

	public void TestMessageTypePreFilledEmpty()
	{
		SetupImport();
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday);
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, ZDateTime.BrettsBirthday);
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, ZDateTime.BrettsBirthday);
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, ZDateTime.BrettsBirthday);
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday, "I2");
		TestMessageTypePreFilled("", EntrySubStyleList.Codes.NormalDeclaration, ZDateTime.BrettsBirthday, "C2");
	}

	void TestMessageTypePreFilled(string messageType, string subStyle, ZDateTime submittedDate, string style = "", string entryStatus = "")
	{
		instruction.CEI_Style = style;
		instruction.CEI_SubStyle = subStyle;
		entry.CH_EntrySubmittedDate = submittedDate;
		entry.CH_EntryStatus = entryStatus;
		var message = string.Format("Substyle {0}, Style {1}, Prefilled Message Type {2}, {3}, IsFailedFromTransmission {4}", subStyle, style, messageType, submittedDate.IsEmpty ? "empty date" : "date filled", entry.IsFailedFromTransmission);
		SetupTestItem();

		AssertEquals(message, messageType, testItem.MessageType);
	}

	public void TestShouldSend()
	{
		entry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
		SetupTestItem();
		testItem.MessageType = ImportSendMessageTypes.Codes.PRE;
		testItem.ShouldSend = true;

		testItem.MessageType = ImportSendMessageTypes.Codes.CAN;
		AssertEquals("When Send is true and Send Message Type is changed, set Send to false", false, testItem.ShouldSend);
	}

	public void TestDate()
	{
		entry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
		SetupTestItem();
		testItem.MessageType = ImportSendMessageTypes.Codes.PRE;
		testItem.ShouldSend = true;

		testItem.MessageType = ImportSendMessageTypes.Codes.CAN;
		AssertEquals("When Send Message Type is changed, empty the Date", ZDateTime.Empty, testItem.Date);
	}

	public void TestReasonForInvalidationMaxLength()
	{
		SetupTestItem();
		AssertEquals(512, testItem.ReasonForInvalidationInfo.MaxLength);
	}

	public void TestReasonForInvalidationReadOnly()
	{
		SetupTestItem();

		testItem.MessageType = ExportSendMessageTypes.Codes.CAN;
		AssertEquals(string.Format("Reason For Invalidation is not read only with {0}", testItem.MessageType), false, testItem.ReasonForInvalidationInfo.ReadOnly);

		testItem.MessageType = ExportSendMessageTypes.Codes.CRE;
		AssertEquals(string.Format("Reason For Invalidation is read only with {0}", testItem.MessageType), true, testItem.ReasonForInvalidationInfo.ReadOnly);

		testItem.MessageType = ExportSendMessageTypes.Codes.DEC;
		AssertEquals(string.Format("Reason For Invalidation is read only with {0}", testItem.MessageType), true, testItem.ReasonForInvalidationInfo.ReadOnly);

		testItem.MessageType = ExportSendMessageTypes.Codes.CAN;
		AssertEquals(string.Format("Reason For Invalidation is read only with {0}", testItem.MessageType), false, testItem.ReasonForInvalidationInfo.ReadOnly);
	}

	public void TestExitType()
	{
		SetupExport();
		SetupTestItem();
		CombineAssertions(() =>
		{
			testItem.ExitType = "1";
			AssertEquals("Exit Type", "1", testItem.ExitType);
			testItem.ExitType = null;
			AssertNullOrEmpty("Exit Type set to null", testItem.ExitType);
			AssertEquals(1, testItem.ExitTypeInfo.MaxLength);
		});
	}

	public void TestExitDate()
	{
		SetupExport();
		SetupTestItem();
		testItem.ExitDate = ZDateTime.BrettsBirthday;
		AssertEquals("Exit Date", ZDateTime.BrettsBirthday, testItem.ExitDate);
		testItem.ExitDate = ZDateTime.Empty;
		AssertEquals("Exit Date set to null", ZDateTime.Empty, testItem.ExitDate);
	}

	public void TestExitCustomsOffice()
	{
		SetupExport();
		SetupTestItem();
		AssertEquals("Exit Customs Office", "ECO", testItem.ExitCustomsOffice);
	}

	public void TestExportCustomsOffice()
	{
		SetupExport();
		SetupTestItem();
		AssertEquals("Export Customs Office", "EOF", testItem.ExportCustomsOffice);
	}

	public void TestSecurity()
	{
		SetupExport();
		SetupTestItem();
		testItem.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.NotUsed;
		AssertEquals("Security", ExportSecurityTypeList.Codes.NotUsed, testItem.ZG_TypeOfSecurity);
		testItem.ZG_TypeOfSecurity = null;
		AssertNullOrEmpty("Security set to null", testItem.ZG_TypeOfSecurity);
	}

	public void TestHasEvidencesExitProof()
	{
		SetupTestItem();

		var alternativeEvidence = testItem.AlternativeEvidences.AddNew();
		alternativeEvidence.EvidenceType = AAZAlternativeEvidenceTypeList.Codes.EXPEXIT;

		AssertEquals("There should be no exit proof evidence", false, testItem.HasEvidencesExitProof);

		alternativeEvidence.EvidenceType = AAZAlternativeEvidenceTypeList.Codes.EXITPROOF;

		AssertEquals("There should have exit proof evidence", true, testItem.HasEvidencesExitProof);
	}

	public void TestUpdate()
	{
		SetupExport();
		SetupTestItem();
		CombineAssertions(() =>
		{
			var updateInfo = testItem.UpdateInfo;
			AssertEquals("ReadOnly ''", false, updateInfo.ReadOnly);
			AssertEquals("Update value ''", true, testItem.Update);
			testItem.MessageType = "DEC";
			AssertEquals("ReadOnly DEC", true, updateInfo.ReadOnly);
			AssertEquals("Update value DEC", false, testItem.Update);
			testItem.MessageType = "SUP";
			AssertEquals("ReadOnly SUP", true, updateInfo.ReadOnly);
			AssertEquals("Update value SUP", true, testItem.Update);
			testItem.MessageType = "AMD";
			AssertEquals("ReadOnly AMD", true, updateInfo.ReadOnly);
			AssertEquals("Update value AMD", true, testItem.Update);
			testItem.MessageType = "FBK";
			AssertEquals("ReadOnly FBK", true, updateInfo.ReadOnly);
			AssertEquals("Update value FBK", false, testItem.Update);
			testItem.MessageType = "CRE";
			AssertEquals("ReadOnly CRE", true, updateInfo.ReadOnly);
			AssertEquals("Update value CRE", true, testItem.Update);
			testItem.MessageType = "CAN";
			AssertEquals("ReadOnly CAN", true, updateInfo.ReadOnly);
			AssertEquals("Update value CAN", false, testItem.Update);
		});
	}

	#region Overrides of BusinessObjectBaseTestCase

	public override void TestProperties()
	{
		SetupExport();
		SetupTestItem();

		CombineAssertions(() =>
		{
			AssertEquals("(Export) MovementReferenceNumber", "MRN", testItem.MovementReferenceNumber);
			AssertEquals("(Export) DeclarationType", "11", testItem.DeclarationType);
			AssertEquals("(Export) Variant", "SUB", testItem.Variant);
			AssertEquals("(Export) Description", "Desc", testItem.Description);
			AssertEquals("(Export) Entry Status", "09", testItem.EntryStatus);
			AssertEquals("(Export) Export Customs Office", "EOF", testItem.ExportCustomsOffice);
			AssertEquals("(Export) Exit Customs Office", "ECO", testItem.ExitCustomsOffice);
			AssertEquals("(Export) Sub Style", "SUB", testItem.SubStyle);
			AssertEquals("(Export) Entry Type", "11 (EXSUB)", testItem.EntryType);
			AssertEquals("(Export) Update", true, testItem.Update);
			AssertEquals("(Export) Message Type", "", testItem.MessageType);
		});

		SetupImport();
		SetupTestItem();

		CombineAssertions(() =>
		{
			AssertEquals("(Import) MovementReferenceNumber", "MRN", testItem.MovementReferenceNumber);
			AssertEquals("(Import) DeclarationType", "11", testItem.DeclarationType);
			AssertEquals("(Import) Variant", "SUB", testItem.Variant);
			AssertEquals("(Import) Description", "Desc", testItem.Description);
			AssertEquals("(Import) Entry Status", "09", testItem.EntryStatus);
			AssertEquals("(Import) Sub Style", "SUB", testItem.SubStyle);
			AssertEquals("(Import) Entry Type", "11 (IMSUB)", testItem.EntryType);
			AssertEquals("(Import) Update", true, testItem.Update);
			AssertEquals("(Import) Message Type", "", testItem.MessageType);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		declaration = Factory.New<JobDeclaration>();
		instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		return new JobDeclarationMessageSendingObject(entry);
	}
	#endregion

	#region Setup
	JobDeclaration declaration;
	CusEntryInstruction instruction;
	CusEntryHeader entry;
	JobDeclarationMessageSendingObjectParent messageSendingObjectParent;
	JobDeclarationMessageSendingObject testItem;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
	}

	void SetupExport()
	{
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		declaration.JE_CustomsOffice = "EOF";
		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
		customsOffice.CY_Data = "ECO";

		SetupGeneral();
	}

	void SetupImport()
	{
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		SetupGeneral();
	}

	void SetupGeneral()
	{
		instruction.CEI_Style = "11";
		instruction.CEI_SubStyle = "SUB";
		instruction.CEI_Description = "Desc";
		entry.CH_MessageType = "CUS";
		entry.CH_EntryStatus = "09";
		entry.CH_Status = "CLO";
		entry.CH_BGMReference = "REF000000001";
		entry.EntryNumber = "ENT00000001";
		entry.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
		entry.MovementReferenceNumberSetter("MRN", ZDateTime.BrettsBirthday);
	}

	void SetupTestItem()
	{
		messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		testItem = messageSendingObjectParent.SendingObjectsCollection[0];
		testItem.Update = true;
	}
	#endregion
}
