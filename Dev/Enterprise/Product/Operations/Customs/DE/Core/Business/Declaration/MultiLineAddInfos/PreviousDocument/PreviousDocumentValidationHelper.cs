using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public static class PreviousDocumentValidationHelper
	{
		public static void ReferenceNumberValid(this PreviousDocument previousDocument)
		{
			var procedureCode = previousDocument.CSI_Procedure;
			var subType = previousDocument.CSI_SubType;
			var referenceNumber = previousDocument.CSI_ReferenceNumber;
			if (procedureCode == PreviousProcedureList.Codes._ATNEU && subType == "REG" && !referenceNumber.IsEmpty)
			{
				var pattern = new Regex("^AT[A-Z][0-9]{2}[0-9]{6}(0[1-9]|1[0-2])20[0-9]{2}[0-9]{4}$");
				if (!pattern.Match(referenceNumber).Success)
				{
					previousDocument.CSI_ReferenceNumber = referenceNumber.ToUpper();
				}
			}
		}

		public static ZBool PreviousProcedureIsRequiredForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var result = false;
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			var procedureWithConcession = invoiceLine.JI_Procedure;
			if (cusEntryInstruction != null && !procedureWithConcession.IsEmpty)
			{
				var previousProcedureCode = procedureWithConcession.SubstringSafe(2, 2);
				if (!cusEntryInstruction.SubStyle1stDigitIs1() &&
					!cusEntryInstruction.SubStyle1stDigitIs2() &&
					(previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._51 || previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._71))
				{
					result = true;
				}
			}
			return result;
		}
	}
}
