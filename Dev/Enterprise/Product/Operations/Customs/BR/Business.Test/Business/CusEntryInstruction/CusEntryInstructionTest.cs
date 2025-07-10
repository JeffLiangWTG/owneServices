using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestDefaultValues()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertEquals("CEI_LegalDocument is NFE", LegalDocumentList.Codes.ElectronicLogisticInvoice, instruction.CEI_LegalDocument);
			AssertEquals("IsUCROverridden is false", ZBool.False, instruction.IsUCROverridden);
			AssertEquals("UCRNumber Empty", ZString.Empty, instruction.UCRNumber);
			AssertEquals("AdditionalInformation Empty", ZString.Empty, instruction.AdditionalInformation);
		}

		public void TestLoadCorrectCusEntryInstructionOnMerged()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var instruction = Factory.New<Customs.Business.CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			invoiceLine.JI_CEI = instruction.PK;
			Factory.Save();
			var declaration = NewFactory().Load<JobDeclaration>(dec.PK);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.InvoiceLines[0].Validation.ValidateJI_Procedure();
			AssertNoExceptionThrown(() =>
			{
				declaration.DoMerge();
			});
		}

		public void TestValidation()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestCEI_SubStyle()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "X";
			AssertEquals("X", instruction.CEI_SubStyle);
		}

		public void TestJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			AssertType<JobDeclaration>(instruction.JobDeclaration);
		}

		public void TestLookups()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionLookups>(instruction.Lookups);
		}

		public void TestInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = ZGuid.Empty;
			invoiceLine2.JI_CEI = ZGuid.Empty;
			AssertEquals("No invoices", 0, entryInstruction1.InvoiceLines.Count());
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			AssertEquals("One invoice", 1, entryInstruction1.InvoiceLines.Count());
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			AssertEquals("Two invoices", 2, entryInstruction1.InvoiceLines.Count());
			AssertNotNull("Invoice 1", entryInstruction1.InvoiceLines.SingleOrDefault(x => x.JI_JZ == invoiceHeader1.PK));
			AssertNotNull("Invoice 2", entryInstruction1.InvoiceLines.SingleOrDefault(x => x.JI_JZ == invoiceHeader2.PK));
		}

		public void TestBR_AdditionalInformation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.AdditionalInformation = "AdditionalInformation";

			AssertEquals("AdditionalInformation should be", "AdditionalInformation", instruction.AdditionalInformation);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlySystemGenerated;
			AssertEquals("AdditionalInformation should be", "AdditionalInformation", instruction.AdditionalInformation);

			instruction.AdditionalInformationOptionDescription = ZString.Empty;
			Assert("AdditionalInformation should be empty", instruction.AdditionalInformation.IsEmpty);
		}

		public void TestBR_AdditionalInformationReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();

			AssertEquals(dec.JE_MessageType, true, instruction.AdditionalInformationInfo.ReadOnly);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(dec.JE_MessageType, true, instruction.AdditionalInformationInfo.ReadOnly);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(dec.JE_MessageType, false, instruction.AdditionalInformationInfo.ReadOnly);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(dec.JE_MessageType, false, instruction.AdditionalInformationInfo.ReadOnly);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			AssertEquals(dec.JE_MessageType, false, instruction.AdditionalInformationInfo.ReadOnly);
		}

		public void TestBR_AdditionalInformationManual()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_AdditionalInformationOption = string.Empty;
			instruction.AdditionalInformationManual = "AdditionalInformationManual";
			Assert("AdditionalInformationManual should be ReadOnly", instruction.AdditionalInformationManualInfo.ReadOnly);
			AssertEquals("AdditionalInformationManual should be", "AdditionalInformationManual", instruction.AdditionalInformationManual);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlyFreeText;
			Assert("AdditionalInformationManual should NOT be Empty", !instruction.AdditionalInformationManual.IsEmpty);
			Assert("AdditionalInformationManual should NOT be ReadOnly", !instruction.AdditionalInformationManualInfo.ReadOnly);

			instruction.AdditionalInformationManual = "AdditionalInformationManual";
			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlySystemGenerated;
			Assert("AdditionalInformationManual should be Empty", instruction.AdditionalInformationManual.IsEmpty);
			Assert("AdditionalInformationManual should be ReadOnly", instruction.AdditionalInformationManualInfo.ReadOnly);

			instruction.AdditionalInformationManual = "AdditionalInformationManual";
			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.FreeTextAndSystemGenerated;
			Assert("AdditionalInformationManual should NOT be Empty", !instruction.AdditionalInformationManual.IsEmpty);
			Assert("AdditionalInformationManual should NOT be ReadOnly", !instruction.AdditionalInformationManualInfo.ReadOnly);

			instruction.AdditionalInformationManual = "AdditionalInformationManual";
			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.SystemGeneratedAndFreeText;
			Assert("AdditionalInformationManual should NOT be Empty", !instruction.AdditionalInformationManual.IsEmpty);
			Assert("AdditionalInformationManual should NOT be ReadOnly", !instruction.AdditionalInformationManualInfo.ReadOnly);
		}

		public void TestAdditionalInformationOptionDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlyFreeText;
			AssertEquals("AdditionalInformationOptionDescription should be", "Only Free Text", instruction.AdditionalInformationOptionDescription);

			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlySystemGenerated;
			AssertEquals("AdditionalInformationOptionDescription should be", "Only System Generated", instruction.AdditionalInformationOptionDescription);

			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.FreeTextAndSystemGenerated;
			AssertEquals("AdditionalInformationOptionDescription should be", "Free Text + System Generated", instruction.AdditionalInformationOptionDescription);

			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText;
			AssertEquals("AdditionalInformationOptionDescription should be", "System Generated + Free Text", instruction.AdditionalInformationOptionDescription);

			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText;
			AssertEquals("AdditionalInformationOptionDescription should be", "System Generated + Free Text", instruction.AdditionalInformationOptionDescription);

			instruction.CEI_AdditionalInformationOption = "X";
			Assert("AdditionalInformationOptionDescription should be Empty", instruction.AdditionalInformationOptionDescription.IsEmpty);
		}

		public void TestCEI_AdditionalInformationOption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlyFreeText;
			AssertEquals("CEI_AdditionalInformationOption should be", AdditionalInformationOptions.Codes.OnlyFreeText, instruction.CEI_AdditionalInformationOption);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlySystemGenerated;
			AssertEquals("CEI_AdditionalInformationOption should be", AdditionalInformationOptions.Codes.OnlySystemGenerated, instruction.CEI_AdditionalInformationOption);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.FreeTextAndSystemGenerated;
			AssertEquals("CEI_AdditionalInformationOption should be", AdditionalInformationOptions.Codes.FreeTextAndSystemGenerated, instruction.CEI_AdditionalInformationOption);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.SystemGeneratedAndFreeText;
			AssertEquals("CEI_AdditionalInformationOption should be", AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText, instruction.CEI_AdditionalInformationOption);

			instruction.AdditionalInformationOptionDescription = "DUMMY";
			Assert("CEI_AdditionalInformationOption should be Empty", instruction.CEI_AdditionalInformationOption.IsEmpty);
		}

		public void TestAdditionalInformationConcatenated()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var instruction = dec.CustomsEntryInstructions.AddNew();

			instruction.AdditionalInformationOptionDescription = ZString.Empty;
			instruction.AdditionalInformation = "Additional Information Auto";
			instruction.AdditionalInformationManual = "Additional Information Manual";
			Assert("AdditionalInformationConcatenated should be Empty", instruction.AdditionalInformationConcatenated.IsEmpty);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlyFreeText;
			instruction.AdditionalInformation = ZString.Empty;
			instruction.AdditionalInformationManual = ZString.Empty;
			Assert("AdditionalInformationConcatenated should be Empty", instruction.AdditionalInformationConcatenated.IsEmpty);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlyFreeText;
			instruction.AdditionalInformation = "Additional Information Auto";
			instruction.AdditionalInformationManual = "Additional Information Manual";
			AssertEquals("AdditionalInformationConcatenated should be", "Additional Information Manual", instruction.AdditionalInformationConcatenated);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.OnlySystemGenerated;
			instruction.AdditionalInformation = "Additional Information Auto";
			instruction.AdditionalInformationManual = "Additional Information Manual";
			AssertEquals("AdditionalInformationConcatenated should be", "Additional Information Auto", instruction.AdditionalInformationConcatenated);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.FreeTextAndSystemGenerated;
			instruction.AdditionalInformation = "Additional Information Auto";
			instruction.AdditionalInformationManual = "Additional Information Manual";
			AssertEquals("AdditionalInformationConcatenated should be", $"Additional Information Manual{System.Environment.NewLine}Additional Information Auto", instruction.AdditionalInformationConcatenated);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.SystemGeneratedAndFreeText;
			instruction.AdditionalInformation = "Additional Information Auto";
			instruction.AdditionalInformationManual = "Additional Information Manual";
			AssertEquals("AdditionalInformationConcatenated should be", $"Additional Information Auto{System.Environment.NewLine}Additional Information Manual", instruction.AdditionalInformationConcatenated);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.FreeTextAndSystemGenerated;
			instruction.AdditionalInformation = new ZString('a', 7790);
			instruction.AdditionalInformationManual = new ZString('b', 15);
			AssertEquals("AdditionalInformationConcatenated should be", new ZString('b', 15) + System.Environment.NewLine + new ZString('a', 7780) + new ZString("..."), instruction.AdditionalInformationConcatenated);

			instruction.AdditionalInformationOptionDescription = AdditionalInformationOptions.Descriptions.SystemGeneratedAndFreeText;
			instruction.AdditionalInformation = ZString.Empty;
			instruction.AdditionalInformationManual = ZString.Empty;
			Assert("AdditionalInformationConcatenated should be Empty", instruction.AdditionalInformationConcatenated.IsEmpty);
		}

		public void TestCEI_IsConsortedExport()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_IsConsortedExport = true;
			AssertEquals(true, instruction.CEI_IsConsortedExport);
			instruction.CEI_IsConsortedExport = false;
			AssertEquals(false, instruction.CEI_IsConsortedExport);
		}

		public void TestIsUCROverriddenAndUCRNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = instruction.PK;
			instruction.IsUCROverridden = true;
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB1903113CS";
			AssertEquals(false, instruction.UCRNumberInfo.ReadOnly);
			AssertNotEquals(string.Empty, instruction.UCRNumber);
			instruction.IsUCROverridden = false;
			AssertEquals(true, instruction.UCRNumberInfo.ReadOnly);
			AssertEquals(string.Empty, instruction.UCRNumber);
		}

		public void TestUCRNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB1903113CS";
			AssertEquals(35, instruction.UCRNumberInfo.MaxLength);
			AssertEquals("9CN91330302765207767NTINVGWAB190311", instruction.UCRNumber);
		}

		public void TestDeleteUCRNumberWhenSavingInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.IsUCROverridden = true;
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";
			Factory.Save();
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			var cusEntryNum1 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("9CN91330302765207767NTINVGWAB190311", cusEntryNum1.CE_EntryNum);
			instruction.IsUCROverridden = false;
			Factory.Save();
			var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(null, cusEntryNum2);
		}

		public void TestDeleteUCRNumberWhenDeletingInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.IsUCROverridden = true;
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";
			Factory.Save();
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			var cusEntryNum1 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("9CN91330302765207767NTINVGWAB190311", cusEntryNum1.CE_EntryNum);
			instruction.Delete();
			var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(null, cusEntryNum2);
		}

		public void TestIsUCROverriddenAndUCRNumberReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew() as CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV3";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_PreviousEntryLineNumber = 10;

			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			instruction.IsUCROverridden = true;
			Assert(!instruction.UCRNumberInfo.ReadOnly);

			entryHeader.EntryNumber = "ENTRY_TEST";
			Assert(instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			entryHeader.EntryNumber = ZString.Empty;
			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(!instruction.UCRNumberInfo.ReadOnly);

			entryHeader.LoadOrCreateUCRNumber("UCR_TEST");
			Assert(instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(!instruction.UCRNumberInfo.ReadOnly);

			entryHeader.LoadOrCreateUCRNumber(ZString.Empty);
			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(!instruction.UCRNumberInfo.ReadOnly);

			instruction.IsUCROverridden = false;

			entryHeader.EntryNumber = "ENTRY_TEST";
			Assert(instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			entryHeader.EntryNumber = ZString.Empty;
			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			entryHeader.LoadOrCreateUCRNumber("UCR_TEST");
			Assert(instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);

			entryHeader.LoadOrCreateUCRNumber(ZString.Empty);
			Assert(!instruction.IsUCROverriddenInfo.ReadOnly);
			Assert(instruction.UCRNumberInfo.ReadOnly);
		}

		public void TestBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew() as CusEntryInstruction;
			Assert(instruction.BillNumber.IsEmpty);

			instruction.BillNumber = "123456";
			AssertEquals("123456", instruction.BillNumber);

			Factory.Save();
			AssertNotNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			AssertEquals("123456", instruction.BillNumber);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			Assert(instruction.BillNumber.IsEmpty);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			instruction.BillNumber = "123456";
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			Assert(instruction.BillNumber.IsEmpty);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			instruction.BillNumber = "123456";
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			Assert(instruction.BillNumber.IsEmpty);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			instruction.BillNumber = "123456";
			Factory.Save();
			AssertNotNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			AssertEquals("123456", instruction.BillNumber);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			instruction.BillNumber = "123456";
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			Assert(instruction.BillNumber.IsEmpty);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			instruction.BillNumber = "123456";
			Factory.Save();
			AssertNotNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			AssertEquals("123456", instruction.BillNumber);

			instruction.BillNumber = ZString.Empty;
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
			Assert(instruction.BillNumber.IsEmpty);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			instruction.BillNumber = "123456";
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.FractionalDelivery;
			Assert(instruction.BillNumber.IsEmpty);
			Factory.Save();
			AssertNull(CusEntryNumber.Load(instruction, CusEntryNumberTypes.Brazil.BillNumber, Core.Constants.CountryCodes.Brazil));
		}

		public void TestBillType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("BillType should be", BillTypeList.Descriptions.HBL.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertEquals("BillType should be", BillTypeList.Descriptions.HBL.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			AssertEquals("BillType should be", BillTypeList.Descriptions.HBL.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("BillType should be", BillTypeList.Descriptions.UCR.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("BillType should be", BillTypeList.Descriptions.UCR.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("BillType should be", BillTypeList.Descriptions.UCR.ToString(), instruction.BillType);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			Assert("BillType should be EMPTY", instruction.BillType.IsEmpty);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.FractionalDelivery;
			Assert("BillType should be EMPTY", instruction.BillType.IsEmpty);
		}

		public void TestBR_AdditionalInformationMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(CusEntryInstruction.Schema.ExportAdditionalInformationMaxLength, instruction.AdditionalInformationInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(AutoStmNote.Schema.ST_NoteTextMaxLength, instruction.AdditionalInformationInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(CusEntryInstruction.Schema.ImportLicenseAdditionalInformationMaxLength, instruction.AdditionalInformationInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(AutoStmNote.Schema.ST_NoteTextMaxLength, instruction.AdditionalInformationInfo.MaxLength);
		}

		public void TestInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("No invoices", 0, entryInstruction.Invoices.Count());
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.InvoiceLines.AddNew();
			AssertEquals("One invoice", 1, entryInstruction.Invoices.Count());
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			AssertEquals("Two invoices", 2, entryInstruction.Invoices.Count());
		}

		public void TestIsJustificationContactDetailAddressAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(true, entryInstruction.IsJustificationContactDetailAddressAvailable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(false, entryInstruction.IsJustificationContactDetailAddressAvailable);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(false, entryInstruction.IsJustificationContactDetailAddressAvailable);
		}

		public void TestDeleteJustificationContactDetailAddressOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var justificationContactDetailAddress = entryInstruction.JustificationContactDetailAddress;
			justificationContactDetailAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Factory.Save();
			AssertEquals("JustificationContactDetailAddress should be saved", true, justificationContactDetailAddress.IsInDatabase);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals("JustificationContactDetailAddress should be deleted", true, justificationContactDetailAddress.IsDeleted);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			justificationContactDetailAddress = entryInstruction.JustificationContactDetailAddress;
			justificationContactDetailAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			entryInstruction.Delete();
			AssertEquals("JustificationContactDetailAddress should be deleted", true, justificationContactDetailAddress.IsDeleted);
		}

		public void TestCEI_DetailWithoutLegalDoc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			entryInstruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3004;

			AssertEquals("CEI_DetailWithoutLegalDoc should be", DetailWithoutLegalDocList.Codes._3004, entryInstruction.CEI_DetailWithoutLegalDoc);
			AssertEquals("CEI_DetailWithoutLegalDoc_ReadOnly should be", false, entryInstruction.CEI_DetailWithoutLegalDoc_ReadOnly);

			entryInstruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3017;
			entryInstruction.CEI_LegalDocument = "NFF";
			AssertEquals("CEI_DetailWithoutLegalDoc should be", DetailWithoutLegalDocList.Codes._3017, entryInstruction.CEI_DetailWithoutLegalDoc);
			AssertEquals("CEI_DetailWithoutLegalDoc_ReadOnly should be", false, entryInstruction.CEI_DetailWithoutLegalDoc_ReadOnly);

			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			AssertEquals("CEI_DetailWithoutLegalDoc should be", ZString.Empty, entryInstruction.CEI_DetailWithoutLegalDoc);
			AssertEquals("CEI_DetailWithoutLegalDoc_ReadOnly should be", true, entryInstruction.CEI_DetailWithoutLegalDoc_ReadOnly);

			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			AssertEquals("CEI_DetailWithoutLegalDoc should be", DetailWithoutLegalDocList.Codes._3004, entryInstruction.CEI_DetailWithoutLegalDoc);
			AssertEquals("CEI_DetailWithoutLegalDoc_ReadOnly should be", false, entryInstruction.CEI_DetailWithoutLegalDoc_ReadOnly);

			entryInstruction.CEI_SpecialCustomsClearance = ZString.Empty;
			AssertEquals(ZString.Empty, entryInstruction.CEI_DetailWithoutLegalDoc);
			AssertEquals("CEI_DetailWithoutLegalDoc_ReadOnly should be", false, entryInstruction.CEI_DetailWithoutLegalDoc_ReadOnly);
		}

		public void TestCEI_LegalDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			AssertEquals("CEI_LegalDocument should be", LegalDocumentList.Codes.NoInvoice, entryInstruction.CEI_LegalDocument);

			entryInstruction.CEI_SpecialCustomsClearance = ZString.Empty;
			AssertEquals("CEI_LegalDocument should be empty", ZString.Empty, entryInstruction.CEI_LegalDocument);
		}

		public void TestLoadMercosulForeignDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var mercosulForeignDeclaration = Factory.New<MercosulForeignDeclaration>();
			mercosulForeignDeclaration.CSI_ParentID = entryInstruction.PK;
			mercosulForeignDeclaration.CSI_ParentTableCode = entryInstruction.TablePrefix;
			mercosulForeignDeclaration.CSI_Description = "1";
			mercosulForeignDeclaration.CSI_ReferenceNumber = "1";

			Assert("There are MercosulForeignDeclarations", entryInstruction.MercosulForeignDeclarations.Any());
		}

		public void TestSaveMercosulForeignDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var mercosulForeignDeclaration = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeignDeclaration.CSI_Description = "1";
			mercosulForeignDeclaration.CSI_ItemNumber = 1;

			Factory.Save();
			Assert("MercosulForeignDeclaration should be saved for ISW job", entryInstruction.MercosulForeignDeclarations.Any());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();

			Assert("MercosulForeignDeclaration should be saved for IMP job", entryInstruction.MercosulForeignDeclarations.Any());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();

			Assert("MercosulForeignDeclaration should NOT be saved for non-ISW/IMP job", !entryInstruction.MercosulForeignDeclarations.Any());
		}

		public void TestParentEntryInstructionGenPivotOnSave()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var parentEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			parentEntryInstruction.CEI_Description = "PARENT_INST";

			var splitEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			splitEntryInstruction.CEI_Description = "SPLIT_INST";
			splitEntryInstruction.LinkToParentInstruction(parentEntryInstruction);

			Assert("parentEntryInstruction ParentEntryInstructionGenPivot must NOT contain any element", parentEntryInstruction.ParentEntryInstructionGenPivotCollection.Count == 0);
			Assert("splitEntryInstruction ParentEntryInstructionGenPivot must contain any element", splitEntryInstruction.ParentEntryInstructionGenPivotCollection.Count > 0);

			AssertNull("parentEntryInstruction.HasParentEntryInstruction must be null", parentEntryInstruction.ParentEntryInstruction);
			Assert("parentEntryInstruction.HasParentEntryInstruction must be FALSE", !parentEntryInstruction.HasParentEntryInstruction);
			Assert("parentEntryInstruction.ParentEntryInstructionDescription must be Empty", parentEntryInstruction.ParentEntryInstructionDescription.IsEmpty);

			AssertNotNull("splitEntryInstruction.HasParentEntryInstruction must NOT be null", splitEntryInstruction.ParentEntryInstruction);
			Assert("splitEntryInstruction.HasParentEntryInstruction must be TRUE", splitEntryInstruction.HasParentEntryInstruction);
			AssertEquals("splitEntryInstruction.ParentEntryInstructionDescription must be PARENT_INST", "PARENT_INST", splitEntryInstruction.ParentEntryInstructionDescription);

			Factory.Save();
			Assert("splitEntryInstruction.ParentEntryInstructionGenPivotCollection should be saved for LIC job", splitEntryInstruction.ParentEntryInstructionGenPivotCollection.Count > 0);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Factory.Save();
			Assert("splitEntryInstruction.ParentEntryInstructionGenPivotCollection should NOT be saved for non-LIC job", splitEntryInstruction.ParentEntryInstructionGenPivotCollection.Count == 0);
		}

		public void TestParentEntryInstructionGenPivotOnDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var parentEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			parentEntryInstruction.CEI_Description = "PARENT_INST";

			var splitEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			splitEntryInstruction.CEI_Description = "SPLIT_INST";
			splitEntryInstruction.LinkToParentInstruction(parentEntryInstruction);

			var pivot = splitEntryInstruction.ParentEntryInstructionGenPivot;
			Assert("GenPivot must NOT be deleted", !pivot.IsDeleted);

			splitEntryInstruction.Delete();
			Assert("GenPivot must be deleted", pivot.IsDeleted);
		}

		public void TestRemoveParentInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var parentEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			parentEntryInstruction.CEI_Description = "PARENT_INST";

			var splitEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			splitEntryInstruction.CEI_Description = "SPLIT_INST";
			splitEntryInstruction.LinkToParentInstruction(parentEntryInstruction);

			var pivot = splitEntryInstruction.ParentEntryInstructionGenPivot;
			Assert("GenPivot must NOT be deleted", !pivot.IsDeleted);

			splitEntryInstruction.RemoveParentInstruction();
			Assert("parentEntryInstruction must NOT be deleted", !parentEntryInstruction.IsDeleted);
			AssertNull("ParentEntryInstruction must be null", splitEntryInstruction.ParentEntryInstruction);
		}

		public void TestBR_AFRMMMethodOfCalcuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";
			AssertEquals("CEI_AFRMMMethodOfCalculation should be", "FMM1", entryInstruction.CEI_AFRMMMethodOfCalculation);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertEquals("CEI_AFRMMMethodOfCalculation should be", "FMM1", entryInstruction.CEI_AFRMMMethodOfCalculation);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("CEI_AFRMMMethodOfCalculation should be", ZString.Empty, entryInstruction.CEI_AFRMMMethodOfCalculation);

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";

			AssertEquals("CEI_AFRMMMethodOfCalculation should be", "FMM1", entryInstruction.CEI_AFRMMMethodOfCalculation);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("CEI_AFRMMMethodOfCalculation should be", ZString.Empty, entryInstruction.CEI_AFRMMMethodOfCalculation);
		}

		public void TestCEI_AFRMMRateOverride()
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";

			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_AFRMMRateOverride should be", 10m, entryInstruction.CEI_AFRMMRateOverride);
			AssertEquals("CEI_AFRMMRateOverride should be readonly", true, entryInstruction.CEI_AFRMMRateOverrideInfo.ReadOnly);

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 15m;
			AssertEquals("IsAFRMMRateOverridden should be", true, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_AFRMMRateOverride should be", 15m, entryInstruction.CEI_AFRMMRateOverride);
			AssertEquals("CEI_AFRMMRateOverride should be readonly", false, entryInstruction.CEI_AFRMMRateOverrideInfo.ReadOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_AFRMMRateOverride should be", 0m, entryInstruction.CEI_AFRMMRateOverride);
			AssertEquals("CEI_AFRMMRateOverride should be readonly", true, entryInstruction.CEI_AFRMMRateOverrideInfo.ReadOnly);
		}

		public void TestCEI_UtilizationFeeOverride()
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";

			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_UtilizationFeeOverride should be", 0m, entryInstruction.CEI_UtilizationFeeOverride);
			AssertEquals("CEI_UtilizationFeeOverride should NOT be readonly", true, entryInstruction.CEI_UtilizationFeeOverrideInfo.ReadOnly);

			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_UtilizationFeeOverride = 15m;
			AssertEquals("IsAFRMMRateOverridden should be", true, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_UtilizationFeeOverride should be", 15m, entryInstruction.CEI_UtilizationFeeOverride);
			AssertEquals("CEI_UtilizationFeeOverride should NOT be readonly", false, entryInstruction.CEI_UtilizationFeeOverrideInfo.ReadOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);
			AssertEquals("CEI_UtilizationFeeOverride should be", 0m, entryInstruction.CEI_UtilizationFeeOverride);
			AssertEquals("CEI_UtilizationFeeOverride should be readonly", true, entryInstruction.CEI_UtilizationFeeOverrideInfo.ReadOnly);
		}

		public void TestIsAFRMMRateOverridden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";
			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);
			Factory.Save();

			entryInstruction = new BusinessObjectFactory().Load<CusEntryInstruction>(entryInstruction.PK);
			AssertEquals("IsAFRMMRateOverridden should be", false, entryInstruction.IsAFRMMRateOverridden);

			entryInstruction.CEI_AFRMMRateOverride = 15m;
			entryInstruction.Factory.Save();

			entryInstruction = new BusinessObjectFactory().Load<CusEntryInstruction>(entryInstruction.PK);
			AssertEquals("When CEI_AFRMMRateOverride is entered", true, entryInstruction.IsAFRMMRateOverridden);

			entryInstruction.CEI_AFRMMRateOverride = 0m;
			entryInstruction.CEI_UtilizationFeeOverride = 15m;
			entryInstruction.Factory.Save();

			entryInstruction = new BusinessObjectFactory().Load<CusEntryInstruction>(entryInstruction.PK);
			AssertEquals("When CEI_UtilizationFeeOverride is entered", true, entryInstruction.IsAFRMMRateOverridden);

			entryInstruction.CEI_AFRMMRateOverride = 15m;
			entryInstruction.IsAFRMMRateOverridden = false;
			AssertEquals("CEI_AFRMMRateOverride should be cleared", 0m, entryInstruction.CEI_AFRMMRateOverride);
			AssertEquals("CEI_UtilizationFeeOverride should be cleared", 0m, entryInstruction.CEI_UtilizationFeeOverride);
		}

		public void TestCEI_AFRMMRateOverride_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";

			entryInstruction.IsAFRMMRateOverridden = true;
			AssertEquals("CEI_AFRMMRateOverride_ReadOnly should be", false, entryInstruction.CEI_AFRMMRateOverride_ReadOnly);

			entryInstruction.IsAFRMMRateOverridden = false;
			AssertEquals("CEI_AFRMMRateOverride_ReadOnly should be", true, entryInstruction.CEI_AFRMMRateOverride_ReadOnly);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			entryInstruction.IsAFRMMRateOverridden = true;
			AssertEquals("CEI_AFRMMRateOverride_ReadOnly should be", true, entryInstruction.CEI_AFRMMRateOverride_ReadOnly);
		}

		public void TestLinkedImportDeclaration()
		{
			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_DeclarationReference = "LIC_TEST1";
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryHeader = licDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.LIC;
			var entryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_LIC1";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			AssertNull("LinkedImportDeclaration", entryInstruction.LinkedImportDeclaration);

			var iswDeclaration = Factory.New<JobDeclaration>();
			iswDeclaration.JE_DeclarationReference = "ISW_TEST";
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
			AssertNull("LinkedImportDeclaration", entryInstruction.LinkedImportDeclaration);

			Factory.Save();
			AssertEquals("LinkedImportDeclaration", iswDeclaration, entryInstruction.LinkedImportDeclaration);
		}

		public void TestCanDelete()
		{
			var reasonForNotAbleToDelete = "The Entry Instruction cannot be deleted. There is an Import Declaration reference it.";

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_DeclarationReference = "LIC_TEST1";
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryHeader = licDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.LIC;
			var entryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_LIC1";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Assert("CanDelete", entryInstruction.CanDelete);
			AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)entryInstruction).ReasonForNotAbleToDelete);

			var iswDeclaration = Factory.New<JobDeclaration>();
			iswDeclaration.JE_DeclarationReference = "ISW_TEST";
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
			Assert("CanDelete", entryInstruction.CanDelete);
			AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)entryInstruction).ReasonForNotAbleToDelete);

			Factory.Save();
			Assert("CanDelete", !entryInstruction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)entryInstruction).ReasonForNotAbleToDelete);

			iswDeclaration.DetachImportLicense(licDeclaration.CustomsEntryInstructions);
			Factory.Save();

			Assert("CanDelete", entryInstruction.CanDelete);
			AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)entryInstruction).ReasonForNotAbleToDelete);
		}

		public void TestJustification()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3020;
			AssertEquals("MaxLength of the Justification should be", 1000, instruction.JustificationInfo.MaxLength);

			instruction.Justification = "Justification";
			var note = FindNote(instruction.PK, PredefinedNoteTypes.Instance.Justification);
			AssertEquals("Justification should be", "Justification", note.ST_NoteText);

			Factory.Save();
			AssertEquals("Justification should be", ZString.Empty, instruction.Justification);
			AssertEquals("Justification Note should be deleted", true, note.IsDeleted);

			instruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			instruction.Justification = "Justification";
			Factory.Save();
			AssertEquals("Justification should be", "Justification", instruction.Justification);

			StmNote FindNote(ZGuid parentID, PredefinedNoteType predefinedNoteType)
			{
				var query = new ZQuery(StmNoteSchema.ST_Description, predefinedNoteType.MultilingualDescription.GetUnresolvedString());
				query.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
				return Factory.LoadTop1<StmNote>(query);
			}
		}

		public void TestIsJustificationVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			AssertEquals("IsJustificationVisible should be", false, instruction.IsJustificationVisible);
			instruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3005;
			AssertEquals("IsJustificationVisible should be", false, instruction.IsJustificationVisible);

			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3001;
			AssertEquals("IsJustificationVisible should be", false, instruction.IsJustificationVisible);

			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3020;
			AssertEquals("IsJustificationVisible should be", true, instruction.IsJustificationVisible);

			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3021;
			AssertEquals("IsJustificationVisible should be", true, instruction.IsJustificationVisible);

			instruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3026;
			AssertEquals("IsJustificationVisible should be", true, instruction.IsJustificationVisible);
		}

		protected override RefCusProcedure CreateRefCusProcedure()
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "11111";
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = "DESCRIPTION";
			procedure.ZZ6_ShipmentType = "IMP";
			return procedure;
		}
	}
}
