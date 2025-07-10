using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class HolderOfTransitProcedureContactPersonProvider : IContactPerson
	{
		readonly OrgContact orgContact;
		readonly JobDocAddress jobDocAddress;
		readonly string contactName;

		public static HolderOfTransitProcedureContactPersonProvider NewOrNull(JobDocAddress jobDocAddress)
		{
			var orgContactTuple = GetContact(jobDocAddress);
			return orgContactTuple.Contact == null ? null : new HolderOfTransitProcedureContactPersonProvider(jobDocAddress, orgContactTuple);
		}

		HolderOfTransitProcedureContactPersonProvider(JobDocAddress jobDocAddress, (OrgContact Contact, string PersonName) orgContact)
		{
			this.jobDocAddress = jobDocAddress;
			this.orgContact = orgContact.Contact;
			this.contactName = orgContact.PersonName;
		}

		public string Name => contactName;

		public string PhoneNumber => orgContact.OC_Phone.IsEmpty ? jobDocAddress.E2_Phone : orgContact.OC_Phone;

		public string EMailAddress => orgContact.Email.IsEmpty ? jobDocAddress.E2_Email : orgContact.Email;

		static (OrgContact Contact, string PersonName) GetContact(JobDocAddress jobDocAddress)
		{
			OrgContact result = null;
			var tuple = (result, ZString.Empty);
			if (jobDocAddress?.Parent is NctsHeader header)
			{
				var guarantees = header.MovementHeader.Guarantees;
				var nctsGuarantee = guarantees.Cast<NctsGuarantee>()?.FirstOrDefault(x => !x.PW_Password.IsEmpty);
				if (nctsGuarantee != null)
				{
					var accessCode = nctsGuarantee.PW_Password;
					if (nctsGuarantee.CusGuaranteeWithSubTypeFilter is CusGuaranteeHeader cusGuarantee)
					{
						var activeContacts = cusGuarantee.PermitHolder?.GetActiveContacts().Cast<OrgContact>().ToArray();
						foreach (var ac in activeContacts)
						{
							if (ac.OC_ContactName.EqualsIgnoringCase(cusGuarantee.MainAccessPersonName) && cusGuarantee.MainAccessCode == accessCode)
							{
								tuple = (ac, cusGuarantee.MainAccessPersonName);
								break;
							}
							else
							{
								var additionalAccessCode = cusGuarantee.AdditionalAccessCodes.FirstOrDefault(x => x.CPR_Description.EqualsIgnoringCase(ac.OC_ContactName) && x.CPR_ValueFrom == accessCode);
								if (additionalAccessCode != null)
								{
									tuple = (ac, additionalAccessCode.CPR_Description);
									break;
								}
							}
						}
					}
				}
				else
				{
					result = jobDocAddress.Organisation?.GetActiveContacts().Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>().Any(allocation => allocation.PC_Type == OrgConstants.ContactAllocationType.CUS));
					tuple = (result, result?.Name ?? ZString.Empty);
				}
			}
			return tuple;
		}
	}
}
