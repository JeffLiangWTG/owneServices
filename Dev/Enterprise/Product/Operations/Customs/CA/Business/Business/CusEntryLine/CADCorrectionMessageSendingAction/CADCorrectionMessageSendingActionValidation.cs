using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CADCorrectionMessageSendingActionValidation : CusSupportingInfoValidation
	{
		public CADCorrectionMessageSendingActionValidation(CADCorrectionMessageSendingAction parent)
			: base(parent)
		{
		}

		public new CADCorrectionMessageSendingAction Parent
		{
			get { return (CADCorrectionMessageSendingAction)base.Parent; }
		}

		bool CheckIfInvoiceLineExists()
		{
			if (Parent.Parent is CADCorrectionMessageSendingActionWrapper wrapper)
			{
				var invoiceSequence = Parent.InvoiceSequence;
				var invoiceLineSequence = Parent.InvoiceLineSequence;
				return wrapper.CADEntryHeader.Declaration.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.JI_LineNo == invoiceLineSequence && x.InvoiceHeader.JZ_InvoiceDisplaySequence == invoiceSequence);
			}
			return false;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntryLineSequence();
			ValidateInvoiceSequence();
			ValidateInvoiceLineSequence();
		}

		internal const string UnableToFindInvoiceLineMessage = "Unable to find invoice line.";

		public void ValidateInvoiceSequence()
		{
			ValidateCalculatedProperty(Parent.InvoiceSequenceInfo);
		}

		protected void CheckInvoiceSequence()
		{
			MandatoryValidation.CheckEntered(Parent.InvoiceSequenceInfo);
			if (!Parent.InvoiceSequence.IsEmpty && !CheckIfInvoiceLineExists())
			{
				Parent.InvoiceSequenceInfo.AddWarning(UnableToFindInvoiceLineMessage);
			}
		}

		internal void ValidateInvoiceLineSequence()
		{
			ValidateCalculatedProperty(Parent.InvoiceLineSequenceInfo);
		}

		protected void CheckInvoiceLineSequence()
		{
			MandatoryValidation.CheckEntered(Parent.InvoiceLineSequenceInfo);
			if (!Parent.InvoiceLineSequence.IsEmpty && !CheckIfInvoiceLineExists())
			{
				Parent.InvoiceLineSequenceInfo.AddWarning(UnableToFindInvoiceLineMessage);
			}
		}

		public void ValidateEntryLineSequence()
		{
			ValidateCalculatedProperty(Parent.EntryLineSequenceInfo);
		}

		protected void CheckEntryLineSequence()
		{
			MandatoryValidation.CheckEntered(Parent.EntryLineSequenceInfo);
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.ErrorIfInvalidCode(Parent.CSI_CodeInfo);
			MandatoryValidation.CheckEntered(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.ErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
			MandatoryValidation.CheckEntered(Parent.CSI_SubTypeInfo);
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			MandatoryValidation.CheckEntered(Parent.CSI_DescriptionInfo);
		}
	}
}
