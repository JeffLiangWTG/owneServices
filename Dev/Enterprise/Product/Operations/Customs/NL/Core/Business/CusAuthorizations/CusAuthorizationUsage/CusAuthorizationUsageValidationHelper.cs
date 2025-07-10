using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

static class CusAuthorizationUsageValidationHelper
{
	public static void CheckAgcCodeMOP(CusEntryInstruction entryInstruction, BusinessObject businessObject, IEnumerable<CusAuthorizationUsage> cusAuthorizationUsages)
	{
		var errorMessageMissing = Res.GetString("025467FA-9489-4157-8378-2BC7DF33083E", "DPO authorization and Method of Payment missing. Please fill one of these fields related to Duty payments");
		var errorMessageBothFilled = Res.GetString("35CF65F0-D1F6-49AE-828F-77212A5E48AC", "Both DPO authorization and Method of payment are filled");
		if (entryInstruction?.JobDeclaration is JobDeclaration declaration
			&& declaration.IsImport
			&& !entryInstruction.HasProcedureStartingWithAny(NLConstants.ProcedureCodes._51, NLConstants.ProcedureCodes._53, NLConstants.ProcedureCodes._71))
		{
			var check1 = cusAuthorizationUsages.Any(x => x != null && x.AGC_Code.Equals(NLCusAuthorisationHeaderTypeList.Codes.C506) && !x.AGC_Number.IsEmpty);
			var check2 = entryInstruction.AllEntryLineFees().Cast<CusEntryLineFee>().Any(x => x != null && !x.CF_MethodOfPayment.IsEmpty);

			if (check1 && check2)
			{
				businessObject.AddRowWarning(errorMessageBothFilled);
			}

			if (!check1 && !check2)
			{
				businessObject.AddRowMessageError(errorMessageMissing);
			}
		}
	}
}
