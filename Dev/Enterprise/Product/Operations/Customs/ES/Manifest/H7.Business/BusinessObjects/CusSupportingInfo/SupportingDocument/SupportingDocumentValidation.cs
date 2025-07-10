using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Codes = Enterprise.Customs.ES.Manifest.H7.Business.ESH7AdditionalProcedureCodeList.Codes;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class SupportingDocumentValidation : EU.H7.Business.SupportingDocumentValidation
{
	public SupportingDocumentValidation(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
		{
			CheckAcceptedTypeList(bill, Procedures07, AcceptedTypeForProcedure07, AcceptedTypeMessageForProcedure07);
			CheckAcceptedTypeList(bill, [Codes.C08], AcceptedTypeForProcedure08, AcceptedTypeMessageForProcedure08);
			CheckAcceptedTypeList(bill, Procedures163536, AcceptedTypeForProcedure163536, AcceptedTypeMessageForProcedure163536);
			SupportingDocumentValidationHelper.CheckRequiredTypesForProcedureC07(bill, Parent.CSI_CodeInfo.AddMessageError);
			SupportingDocumentValidationHelper.CheckIfType1018IsRequired(bill, Parent.CSI_CodeInfo.AddMessageError);
			CheckNoRepeatedTypes(bill);
		}
	}

	void CheckAcceptedTypeList(AsycudaBill bill, List<string> procedures, List<string> acceptedTypes, string errorMsg)
	{
		if (procedures.Contains(bill.ABL_Procedure) && !acceptedTypes.Contains(Parent.CSI_Code))
		{
			Parent.CSI_CodeInfo.AddMessageError(errorMsg);
		}
	}

	void CheckNoRepeatedTypes(AsycudaBill bill)
	{
		if (bill.SupportingDocuments.Any(s => s.PK != Parent.PK && s.CSI_Code == Parent.CSI_Code))
		{
			Parent.CSI_CodeInfo.AddMessageError(NoRepeatedSupportingDocumentTypesMessage);
		}
	}

	protected string AcceptedTypeMessageForProcedure07 => Res.GetString("dd25ae98-7c4e-4ac3-b770-0a4f276881fd", "Supporting Documents Type can only be '1014', '1018', '1230', '1315', '1316', '1317', '1318', '1319', '1320', 'N325' and/or 'N380'.");

	protected string AcceptedTypeMessageForProcedure08 => Res.GetString("7b2d1065-ae02-45f9-b3a9-3b7b8fa0397b", "Supporting Documents Type can only be '1003', '1018', '1315', '1316', '1317', '1318', '1319' and/or '1320'.");

	protected string AcceptedTypeMessageForProcedure163536 => Res.GetString("5bd419ab-2e95-4ed9-9712-f22288654edd", "Supporting Documents Type can only be '1018', '1315', '1316', '1317', '1318', '1319', '1320' and/or 'N325'.");

	protected string NoRepeatedSupportingDocumentTypesMessage => Res.GetString("0ea3ed0d-7a90-4f16-a375-85d417b0c6a0", "Selected Supporting Documents Type has already been provided.");

	protected readonly List<string> AcceptedTypeForProcedure07 = ["1014", "1018", "1230", "1315", "1316", "1317", "1318", "1319", "1320", "N325", "N380"];

	protected readonly List<string> AcceptedTypeForProcedure08 = ["1003", "1018", "1315", "1316", "1317", "1318", "1319", "1320"];

	protected readonly List<string> AcceptedTypeForProcedure163536 = ["1018", "1315", "1316", "1317", "1318", "1319", "1320", "N325"];

	protected readonly List<string> Procedures07 = [Codes.C07, Codes.C07F48, Codes.C07F49];

	protected readonly List<string> Procedures163536 = [Codes.C16, Codes.C35, Codes.C36];
}
