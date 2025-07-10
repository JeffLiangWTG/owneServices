using System;
using System.Collections.Generic;
using System.Linq;
using Codes = Enterprise.Customs.ES.Manifest.H7.Business.ESH7AdditionalProcedureCodeList.Codes;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public static class SupportingDocumentValidationHelper
	{
		public static void CheckRequiredTypesForProcedureC07(AsycudaBill bill, Action<string> addMessageError)
		{
			if (addMessageError == null)
			{
				throw new ArgumentNullException(nameof(addMessageError), "The addMessageError function cannot be null.");
			}

			if (Procedures07.Contains(bill.ABL_Procedure))
			{
				if (!BillContainsN325OrN380(bill))
				{
					addMessageError(requiredTypeMessageForProcedureC07);
				}

				if (!BillContainsAtLeastTwoSupportingDocuments(bill))
				{
					addMessageError(RequireAtLeast2SupportingDocumentsMessage);
				}
			}
		}

		public static void CheckIfType1018IsRequired(AsycudaBill bill, Action<string> addMessageError)
		{
			if (addMessageError == null)
			{
				throw new ArgumentNullException(nameof(addMessageError), "The addMessageError function cannot be null.");
			}

			if ((MeetsProcedureAgentAndConsigneeRequirements(bill, Procedures0708163536, AgentTypesDIRICA)
				|| (MeetsProcedureAgentAndConsigneeRequirements(bill, Procedures070816, AgentTypesINDDCA) && MeetsCustomsOfficeRequirements(bill)))
				&& bill.SupportingDocuments.All(c => c.CSI_Code != "1018"))
			{
				addMessageError(RequireSupportingDocumentType1018Message);
			}
		}

		static bool BillContainsN325OrN380(AsycudaBill bill) => bill.SupportingDocuments.Any(s => RequiredTypesForProcedure07.Contains(s.CSI_Code));

		static bool BillContainsAtLeastTwoSupportingDocuments(AsycudaBill bill) => bill.SupportingDocuments.Count >= 2;

		static bool MeetsProcedureAgentAndConsigneeRequirements(AsycudaBill bill, List<string> procedures, List<string> agentTypes) => procedures.Contains(bill.ABL_Procedure) && agentTypes.Contains(bill.Header.AMA_AgentType) && bill.ABL_ConsigneeRegNo != "89890029A";

		static bool MeetsCustomsOfficeRequirements(AsycudaBill bill) => !(bill.Header.AMA_CustomsOffice.StartsWith("ES0055") || bill.Header.AMA_CustomsOffice.StartsWith("ES0056"));

		static string requiredTypeMessageForProcedureC07 => Res.GetString("e2831360-0615-4b7f-8ed4-d7b0918a606f", "Please enter a Supporting Documents Reference Number with Type 'N325' and/or 'N380'.");

		static string RequireAtLeast2SupportingDocumentsMessage => Res.GetString("7a222b69-9639-4664-b9db-b46062ec7046", "Please enter at least two Supporting Documents Reference Numbers.");

		static string RequireSupportingDocumentType1018Message => Res.GetString("3092c72c-2e60-4dfa-bbbf-35192f13ffc9", "Please enter a Supporting Documents Reference Number with Type '1018'.");

		static readonly List<string> Procedures0708163536 = new List<string> { Codes.C07, Codes.C08, Codes.C16, Codes.C35, Codes.C36 };

		static readonly List<string> Procedures070816 = new List<string> { Codes.C07, Codes.C08, Codes.C16 };

		static readonly List<string> RequiredTypesForProcedure07 = new List<string> { "N325", "N380" };

		static readonly List<string> AgentTypesDIRICA = new List<string> { "DIR", "ICA" };

		static readonly List<string> AgentTypesINDDCA = new List<string> { "IND", "DCA" };

		static readonly List<string> Procedures07 = new List<string> { Codes.C07, Codes.C07F48, Codes.C07F49 };
	}
}
