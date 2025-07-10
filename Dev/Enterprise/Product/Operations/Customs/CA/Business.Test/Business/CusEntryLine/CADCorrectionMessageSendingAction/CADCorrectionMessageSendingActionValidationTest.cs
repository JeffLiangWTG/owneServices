using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADCorrectionMessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryLineSequence()
		{
			action.EntryLineSequence = 0;
			AssertHasErrorContaining(action.EntryLineSequenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckInvoiceSequence()
		{
			action.InvoiceSequence = 1;
			AssertNoWarningContaining(action.InvoiceSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceSequence = 2;
			AssertNoWarningContaining(action.InvoiceSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceSequence = 3;
			AssertHasWarningContaining(action.InvoiceSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceSequence = 0;
			AssertHasErrorContaining(action.InvoiceSequenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckInvoiceLineSequence()
		{
			action.InvoiceLineSequence = 1;
			AssertNoWarningContaining(action.InvoiceLineSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceLineSequence = 2;
			AssertNoWarningContaining(action.InvoiceLineSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceLineSequence = 3;
			AssertHasWarningContaining(action.InvoiceLineSequenceInfo, CADCorrectionMessageSendingActionValidation.UnableToFindInvoiceLineMessage);
			action.InvoiceLineSequence = 0;
			AssertHasErrorContaining(action.InvoiceLineSequenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCSI_Code()
		{
			action.CSI_Code = "XXX";
			AssertHasErrorContaining(action.CSI_CodeInfo, ListValidation.InvalidCodeError);
			action.CSI_Code = ZString.Empty;
			AssertHasErrorContaining(action.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCSI_SubType()
		{
			action.CSI_SubType = "X";
			AssertHasErrorContaining(action.CSI_SubTypeInfo, ListValidation.InvalidCodeError);
			action.CSI_SubType = ZString.Empty;
			AssertHasErrorContaining(action.CSI_SubTypeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCSI_Description()
		{
			action.CSI_Description = ZString.Empty;
			AssertHasErrorContaining(action.CSI_DescriptionInfo, MandatoryValidation.MustBeEntered);
		}

		CADCorrectionMessageSendingAction action;

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;

			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			entryLine.CL_LineNumber = 1;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew().JI_CL = entryLine.PK;
			invoice1.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JobComInvoiceLines.AddNew();

			Factory.Save();
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			action = wrapper.SendingActions.AddNew();
			action.EntryLineSequence = 1;
		}
	}
}
