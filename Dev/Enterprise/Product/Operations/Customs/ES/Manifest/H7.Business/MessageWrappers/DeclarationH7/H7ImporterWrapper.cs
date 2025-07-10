using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class H7ImporterWrapper(AsycudaBill bill) : PartyIdWrapper(bill.Consignee?.Header), IH7Importer
{
	public IPartyContactProvider ContactInfo
	{
		get
		{
			var name = bill.ABL_ConsigneeName;
			var email = bill.ABL_ConsigneeEmail;
			var phone = bill.ABL_ConsigneePhone;

			if (bill.Consignee != null)
			{
				var cusContact = bill.Consignee.Header?.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CUS);
				if (cusContact != null)
				{
					email = cusContact.OC_Email;
					phone = !cusContact.OC_Mobile.IsEmpty ? cusContact.OC_Mobile : cusContact.OC_Phone;
				}
			}

			return PartyContactWrapper.New(name, email, phone);
		}
	}

	public ZBool IsParticular => bill.Consignee == null
		|| (bill.Consignee?.Header?.OH_Category ?? ZString.Empty) == OrgConstants.Category.NaturalPersonIndividual;

	public ZString Address => bill.ABL_ConsigneeStreet1 + " " + bill.ABL_ConsigneeStreet2;

	public ZString City => bill.ABL_ConsigneeCity;

	public ZString PostCode => bill.ABL_ConsigneePostcode;

	public ZString Country => bill.ABL_RN_NKConsigneeCountry;

	public ZString Name => bill.ABL_ConsigneeName;

	protected override ZString IdCore
	{
		get
		{
			var result = ZString.Empty;

			if (bill.Consignee == null)
			{
				result = bill.ABL_ConsigneeRegNo;
			}
			else if (CusCodeNif != null)
			{
				result = CusCodeNif.OK_CustomsRegNo;
			}
			else if (CusCodeEor != null)
			{
				result = base.IdCore;
			}

			return result;
		}
	}

	static OrgCusCode GetOrgCusCodeByCodeType(string codeType, AsycudaBill asycudaBill)
	{
		return asycudaBill?.Consignee?.Header.CustomsCodes.Cast<OrgCusCode>()
			.FirstOrDefault(o => o.OK_CodeType.EqualsIgnoringCase(codeType));
	}

	OrgCusCode CusCodeNif => GetOrgCusCodeByCodeType(OrgCusCode.SpainCodeTypes.NIF, bill);
	OrgCusCode CusCodeEor => GetOrgCusCodeByCodeType(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill);
}
