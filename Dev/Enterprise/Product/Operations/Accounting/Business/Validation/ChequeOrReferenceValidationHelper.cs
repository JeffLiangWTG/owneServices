using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Validation
{
	public static class ChequeOrReferenceValidationHelper
	{
		public static string CheckIsNumbersLettersAllowed(bool isCheque, string input)
		{
			string message = "";
			if (!string.IsNullOrEmpty(input))
			{
				string mask;
				string errorMessage;

				if (isCheque)
				{
					mask = (NoResString)"^\\d{1,}$";
					errorMessage = Res.GetString("b586797a-c72b-4fae-9150-e7e04bbae60b", "Only numbers are allowed in this field.");
				}
				else
				{
					mask = (NoResString)"^[-A-Za-z0-9 /]+$";
					errorMessage = Res.GetString("751cf609-70e1-4643-97c7-15f4779ef52e", "Only numbers or letters are allowed in this field.");
				}
				Regex r = new Regex(mask);
				Match result = r.Match(input);
				if (!result.Success)
				{
					message = errorMessage;
				}
			}
			return message;
		}

		public static string GetInUseErrorMessage(string chequeNumber, AccBankAccount bankAccount, BusinessObjectFactory factory)
		{
			string result = Res.GetString("437A36A7-19E4-4e64-A3A4-559BAD8AE666", "Check number {0} is already in use.", chequeNumber);

			JobCharge[] jobCharges = bankAccount.GetJobChargesUsingChequeNumber(chequeNumber, ZGuid.Empty);
			if (jobCharges.Length > 0)
			{
				JobCharge charge = jobCharges[0];

				string referenceToDisplay = string.Empty;
				IJobNumber jobNumber = null;
				if (charge.ParentConsolCost != null)
				{
					jobNumber = factory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(((JobConsolCost)charge.ParentConsolCost).E6_ParentID) as IJobNumber;
				}
				if (jobNumber != null)
				{
					referenceToDisplay = Res.GetString("c5af5298-09a4-4ad3-8f43-dd0c1ba60fbe", "Consol") + " ";
				}
				else
				{
					referenceToDisplay = Res.GetString("737a7469-49a1-49a6-bcd5-09d705cfbcc5", "Job") + " ";
					jobNumber = charge.Job;
				}
				result = Res.GetString("DCFF3281-FD40-420f-BD9F-60DFA38870CD", "Check number {2} is already used on {0}{1}.", referenceToDisplay, jobNumber.JobNumber, chequeNumber);
			}

			return result;
		}

		public static string CheckIsChequeNumberNotInBook(AccChequeBook chequeBook, params ZString[] chequeNumbers)
		{
			string errorMessage = string.Empty;

			string missingChequeNumber = chequeNumbers.Where(x => !IsChequeNumberInBook(x, chequeBook)).FirstOrDefault();
			if (chequeNumbers.Where(x => x.IsEmpty).Any() || !string.IsNullOrEmpty(missingChequeNumber))
			{
				errorMessage = Res.GetString("E554330C-EFBC-45e0-8C81-CC21DE65CACB",
@"Check number {2} is not contained in the selected check book.
The check number must be between {0} and {1}.",
				chequeBook.AK_Calc_StartNoString, chequeBook.AK_Calc_LastNoString, missingChequeNumber);
			}
			return errorMessage;
		}

		static bool IsChequeNumberInBook(string chequeNumber, AccChequeBook chequeBook)
		{
			ZDecimal chequeNumberAsDecimal;
			bool result = chequeBook == null || (ZDecimal.TryParse(chequeNumber, out chequeNumberAsDecimal) && chequeNumberAsDecimal.IsInteger && chequeBook.IsChequeInBook(chequeNumberAsDecimal));
			return result;
		}
	}
}
