using System.Linq;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration;

public class OfficeCodeValidation : EuOfficeCodeValidation
{
	public OfficeCodeValidation(OfficeCode parent) : base(parent)
	{
	}

	protected override void CheckCY_Data()
	{
		var parent = Parent;
		if(parent.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch && parent.Parent is JobDeclaration declaration && declaration.IsUCC6AndIsImport)
		{
			CheckNoDuplicateCodeDataPairs(EuOfficeCodesTypes.Codes.OfficeOfDispatch);
		}
		base.CheckCY_Data();
	}

	void CheckNoDuplicateCodeDataPairs(string officeCodeType)
	{
		var allOffices = OfficeCodeProvider.CustomsOffices.OfType<OfficeCode>().Where(office => office.CY_Code == officeCodeType).ToList();

		var duplicateDataValues = allOffices.Count(office => office.CY_Data == Parent.CY_Data);

		if (duplicateDataValues > 1)
		{
			Parent.CY_DataInfo.AddMessageError(Res.GetString("77EBAC6C-A66A-452C-A5C3-31C54DE74ACE", $"Office {Parent.CY_Data} is already used with code '{officeCodeType}'", Parent.CY_DataInfo));
		}
	}
}
