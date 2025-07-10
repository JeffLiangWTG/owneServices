using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationNFnoNFProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalInformation()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Additional Information should be", ZString.Empty, messageBuilder.AdditionalInformation);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.AdditionalInformation = "AdditionalInformation";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Additional Information should be", "AdditionalInformation", messageBuilder.AdditionalInformation);
		}

		public void TestTypeOfOperationExport()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Type Of Operation Export should be", ZString.Empty, messageBuilder.TypeOfOperationExport);
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Type Of Operation Export should be", TypeOfOperationExportList.Codes._1001, messageBuilder.TypeOfOperationExport);
		}

		public void TestSpecialClearance()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Special Customs Clearance should be", ZString.Empty, messageBuilder.SpecialClearance);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2001;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Special Customs Clearance should be", SpecialCustomsClearanceList.Codes._2001, messageBuilder.SpecialClearance);
		}

		public void TestSpecialTransport()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Special Transport should be", ZString.Empty, messageBuilder.SpecialTransport);
			declaration.JE_SpecialTransport = SpecialTransportModesList.Codes._4001;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Special Transport should be", SpecialTransportModesList.Codes._4001, messageBuilder.SpecialTransport);

			declaration.JE_SpecialTransport = SpecialTransportModesList.Codes._4006;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Special Transport should be", SpecialTransportModesList.Codes._4006, messageBuilder.SpecialTransport);
		}

		public void TestIsConsortedExport()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("IsConsortedExport should be", false, messageBuilder.IsConsortedExport);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_IsConsortedExport = true;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("IsConsortedExport should be", true, messageBuilder.IsConsortedExport);
		}

		public void TestRectification()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("IsRectification should be", false, messageBuilder.IsRectification);
			AssertEquals("Rectification Reason should be", ZString.Empty, messageBuilder.RectificationReason);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_IsConsortedExport = true;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var sendObj = new ExportDeclarationMessageSendingObject(entryHeader);
			sendObj.MessageType = ExportEntryActionCodeList.Codes.RET;
			sendObj.VOCReason = "VOCReason";
			messageBuilder = new DeclarationNFnoNFProvider(sendObj);
			AssertEquals("IsConsortedExport should be", true, messageBuilder.IsRectification);
			AssertEquals("Rectification Reason should be", "VOCReason", messageBuilder.RectificationReason);
		}

		public void TestDetailsOfTheOperation()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Details Of The Operation should be", ZString.Empty, messageBuilder.DetailsOfTheOperation);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_SpecialCustomsClearance = SpecialCustomsClearanceList.Codes._2002;
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Details Of The Operation should be", DetailWithoutLegalDocList.Codes._3004, messageBuilder.DetailsOfTheOperation);
		}

		public void TestDeclarationOffice()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("Declaration Office should be", ZString.Empty, messageBuilder.DeclarationOffice.ID);
			declaration.JE_CustomsOffice = "1001";
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("Declaration Office should be", "1001", messageBuilder.DeclarationOffice.ID);
		}

		public void TestUCRNumber()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.IsUCROverridden = false;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("UCRNumber should be", ZString.Empty, messageBuilder.UCRNumber);

			entryInstruction.IsUCROverridden = true;
			entryInstruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";

			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("UCRNumber should be", "9CN91330302765207767NTINVGWAB190311", messageBuilder.UCRNumber);

			entryInstruction.IsUCROverridden = false;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("UCRNumber should be", ZString.Empty, messageBuilder.UCRNumber);

			entryInstruction.IsUCROverridden = true;
			entryInstruction.UCRNumber = ZString.Empty;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("UCRNumber should be", ZString.Empty, messageBuilder.UCRNumber);

			entryInstruction.IsUCROverridden = false;
			entryInstruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("UCRNumber should be", ZString.Empty, messageBuilder.UCRNumber);

			entryInstruction.IsUCROverridden = false;
			entryHeader.UniqueConsignmentReference = "9CN91330302765207767NTINVGWAB190311";
			entryInstruction.UCRNumber = ZString.Empty;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("UCRNumber should be", "9CN91330302765207767NTINVGWAB190311", messageBuilder.UCRNumber);
		}

		public void TestMRNNumber()
		{
			entryHeader.MovementReferenceNumberSetter("20BR0000274180");
			var messageSending = new ExportDeclarationMessageSendingObject(entryHeader);
			messageSending.MessageType = ExportEntryActionCodeList.Codes.ORI;

			var messageBuilder = new DeclarationNFnoNFProvider(messageSending);
			AssertEquals("MRNNumber should be", ZString.Empty, messageBuilder.ID);

			messageSending.MessageType = ExportEntryActionCodeList.Codes.RET;
			messageBuilder = new DeclarationNFnoNFProvider(messageSending);
			AssertEquals("MRNNumber should be", entryHeader.MovementReferenceNumber, messageBuilder.ID);
		}

		public void TestGoodsShipments()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 2;
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals(2, messageBuilder.GoodsShipments.Count());
			AssertEquals(1, messageBuilder.GoodsShipments.First().GovernmentAgencyGoodsItem.SequenceNumeric);
			AssertEquals(2, messageBuilder.GoodsShipments.Last().GovernmentAgencyGoodsItem.SequenceNumeric);
		}

		public void TestJustificationForWaivingTheInvoice()
		{
			var messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("JustificationForWaivingTheInvoice should be", ZString.Empty, messageBuilder.JustificationForWaivingTheInvoice);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.Justification = "JustificationForWaivingTheInvoice";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			messageBuilder = new DeclarationNFnoNFProvider(new ExportDeclarationMessageSendingObject(entryHeader));
			AssertEquals("JustificationForWaivingTheInvoice should be", "JustificationForWaivingTheInvoice", messageBuilder.JustificationForWaivingTheInvoice);
		}

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		CusEntryHeader entryHeader;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageSubTypeList.Codes._09;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = "XXX";
			Factory.Save();
		}

		#endregion
	}
}
