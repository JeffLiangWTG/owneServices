using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration;

public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
{
	public AddInfoCusEntryInstructionValidation(EU.Business.Declaration.AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	CusEntryInstruction EntryInstruction => (CusEntryInstruction)Parent.Parent;

	protected override void CheckZG_RequestType()
	{
		base.CheckZG_RequestType();
		if (MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS() && IsT2LAndExport)
		{
			if (Parent.ZG_RequestType.IsEmpty)
			{
				Parent.ZG_RequestTypeInfo.AddMessageError(Res.GetString("B20D63C4-7F83-4092-8BE7-3BE0440C7BAE", "Request Type is required"));
			}
			else if (Parent.ZG_RequestType == RequestTypeList.Codes.RegistrationRequest && !HasAnyAuthorizationACP)
			{
				Parent.ZG_RequestTypeInfo.AddMessageError(Res.GetString("7CE5AC44-4C2F-4FB9-A87B-CBBBE855926F", "Authorization Type ACP (C511) is required for Request Type 02"));
			}
		}
	}

	protected override void CheckZG_SealsCount()
	{
		// In ES we have no Seal Quantity on Screen and in Messaging. 
	}

	bool IsT2LAndExport => EntryInstruction.IsT2L && EntryInstruction.JobDeclaration.IsExport;
	bool HasAnyAuthorizationACP => EntryInstruction.CusAuthorizationUsages.Any(x => ((CusAuthorizationUsage)x).AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer);
}
