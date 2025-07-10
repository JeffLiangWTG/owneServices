using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ReExportCusEntryInstructionValidationTest : CommonExportCusEntryInstructionValidationTest<ReExportCusEntryInstructionValidation>
	{
		public void TestAdditionalInfos_TransportDocumentsMandatory()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var message = "Please provide at least one Transport Document. Transport Documents can be entered on the Additional Documents Tab. Select Kind = TRA";

			CombineAssertions(() =>
			{
				var addInfo1 = invoiceHeader.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = "XXX";
				addInfo1.CSI_Description = "XXX Description";

				var addInfo2 = instruction.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = "YYY";
				addInfo2.CSI_Description = "YYY Description";

				instruction.Validation.ValidateAll();
				AssertHasRowMessageError("No Transport Document", instruction, message);

				var addInfo3 = instruction.AdditionalInfos.AddNew();
				addInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo3.CSI_Description = "TRA Description";

				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Transport Document At Entry Instruction", instruction, message);

				instruction.AdditionalInfos.RemoveAll();
				var addInfo4 = invoiceHeader.AdditionalInfos.AddNew();
				addInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo4.CSI_Description = "TRA Description";

				instruction.Validation.ValidateAll();
				AssertNoRowMessageError("Transport Document Doc At Invoice Header", instruction, message);
			});
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ReExport;

		protected override ReExportCusEntryInstructionValidation GetValidation() => new ReExportCusEntryInstructionValidation(instruction);
	}
}
