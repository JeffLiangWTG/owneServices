using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class ImportJobDeclarationValidation : JobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_OA_SellerAddress()
	{
		base.CheckJE_OA_SellerAddress();

		var parent = Parent;
		if (parent.ZG_IsHighValueOvrd)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_OA_SellerAddressInfo);
		}

		CheckTheAbsenceOfEoriNumber(parent.JE_OA_SellerAddressInfo, parent.SellerAddress, Res.GetString("4B3CDA82-B961-40E1-A34E-3895CF4A6E19", "Seller"));
	}

	protected override void CheckJE_OA_ConsigneeAddress()
	{
		base.CheckJE_OA_ConsigneeAddress();

		var parent = Parent;
		if (parent.ZG_IsHighValueOvrd)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_OA_ConsigneeAddressInfo);
			CheckTheAbsenceOfEoriNumber(parent.JE_OA_ConsigneeAddressInfo, parent.ConsigneeAddress, Res.GetString("5F75507C-076D-45D6-8A31-3DD662142D27", "Buyer"));
		}
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();

		ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
	}

	protected override void CheckJE_OH_ControllingCustomer()
	{
		base.CheckJE_OH_ControllingCustomer();

		if (Parent.ControllingCustomer is OrgHeader controllingCustomer && controllingCustomer.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty)
		{
			Parent.JE_OH_ControllingCustomerInfo.AddMessageError(Res.GetString("024CCDFB-4ACD-402F-8E48-D1445A3DD6A1", "The Guarantee Provider Party requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)"));
		}
	}

	static void CheckTheAbsenceOfEoriNumber(ZPropertyInfo orgAddressPropertyInfo, OrgAddress orgAddress, ZString orgName)
	{
		if (orgAddress != null)
		{
			bool MissingEoriNumber()
			{
				var cusCodes = orgAddress.Header?.getEoriRegNo();
				return cusCodes == null || cusCodes.Length != 1;
			}

			if (MissingEoriNumber())
			{
				orgAddressPropertyInfo.AddMessageError(Res.GetString("F6012DA0-BACB-4E87-89DE-42D3175296BA", "{0} must have an EORI Number entered in Organizations Registration Numbers / Codes.", orgName));
			}
		}
	}
}
