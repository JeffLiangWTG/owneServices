using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptSendingActionValidation : ZValidation
	{
		public ReportOfReceiptSendingActionValidation(ReportOfReceiptSendingAction parent)
			: base(parent)
		{ }

		ReportOfReceiptSendingAction Parent => (ReportOfReceiptSendingAction)ParentFilter;

		public override Type AutoValidationType => typeof(ReportOfReceiptSendingActionValidation);

		public void ValidateArrivalDate()
		{
			ValidateCalculatedProperty(Parent.ArrivalDateInfo);
		}

		protected void CheckArrivalDate()
		{
			var parent = Parent;

			MandatoryValidation.CheckEntered(parent.ArrivalDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(parent.ArrivalDateInfo);

			if (!parent.ArrivalDate.IsEmpty)
			{
				if (parent.ArrivalDate > ZDateTime.Now)
				{
					parent.ArrivalDateInfo.AddError(Res.GetString("85186ECF-7B6B-43EF-BCEB-B5F7802E9047", "Arrival Date must not be in the future."));
				}
				if (parent.ArrivalDate < parent.JobDeclaration.JE_DateAtOrigin)
				{
					parent.ArrivalDateInfo.AddError(Res.GetString("D5EE303D-B857-4C34-9862-0EF6F3823839", "Arrival Date must not be prior to the Dispatch Time."));
				}
			}
		}

		public void ValidateReceiptResult()
		{
			ValidateCalculatedProperty(Parent.ReceiptResultInfo);
		}

		protected void CheckReceiptResult()
		{
			var parent = Parent;
			var receiptResultInfo = parent.ReceiptResultInfo;

			MandatoryValidation.CheckEntered(receiptResultInfo);
			ListValidation.ErrorIfInvalidCode(receiptResultInfo);
			if (parent.ReceiptResult == EMCSReceiptResultList.Codes.ReceiptPartiallyRefused && parent.JobDeclaration.InvoiceLines.OfType<EMCSJobComInvoiceLine>().All(invLine => invLine.Outturn.C5_RejectedQuantity == 0))
			{
				receiptResultInfo.AddMessageError(Res.GetString("09569841-DA0E-4CB0-873D-5AAC27E4ABD1", "You have not entered a Refused Quantity on an Invoice Line."));
			}
			if (ReasonCodeMissingForCertainReceiptResults())
			{
				receiptResultInfo.AddMessageError(Res.GetString("A243D7A1-C8EA-44D7-A037-521F3AFE4315", "Please enter a Reason Code on at least one line."));
			}

			bool ReasonCodeMissingForCertainReceiptResults() => parent.ReceiptResult.In(new ZString[] { EMCSReceiptResultList.Codes.ReceiptAcceptedAlthoughUnsatisfactory, EMCSReceiptResultList.Codes.ReceiptRefused, EMCSReceiptResultList.Codes.ReceiptPartiallyRefused })
				&& parent.JobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>()
					.All(invLine => invLine.Outturn.ReportOfReceiptReasons.Cast<ReportOfReceiptReason>()
						.All(reason => reason.CY_Code == ZString.Empty));
		}

		public void ValidateComplementaryInformation()
		{
			ValidateCalculatedProperty(Parent.ComplementaryInformationInfo);
		}

		protected void CheckComplementaryInformation()
		{
		}

		public override void ValidateAll()
		{
			ValidateArrivalDate();
			ValidateReceiptResult();
			ValidateComplementaryInformation();
		}
	}
}
