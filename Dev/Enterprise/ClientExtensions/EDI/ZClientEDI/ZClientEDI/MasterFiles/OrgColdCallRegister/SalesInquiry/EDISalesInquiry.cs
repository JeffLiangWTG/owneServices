using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDISalesInquiry : SalesEnquiry
	{
		public EDISalesInquiry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override OrgHeader CreateOrg(BusinessObjectFactory factory)
		{
			OrgHeader result = base.CreateOrg(factory);

			OrgContact contact = result.Contacts[0];
			if (contact.OC_Phone.EndsWith(">") && contact.OC_Phone.IndexOf('<') != -1)
			{
				ZString originalPhone = contact.OC_Phone;
				contact.OC_Phone = originalPhone.Substring(0, O1_Phone.LastIndexOf('<'));
				contact.OC_PhoneExtension = Regex.Match(originalPhone, "<.+>").Value.TrimStart('<').TrimEnd('>');
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "EDI Client Specification")]
		public override void LinkToOrganizationByLinkingToAddress(ZGuid addressPk)
		{
			CreateOriginalInquiryRegistrationDetailsNote();

			var address = Factory.Load<OrgAddress>(addressPk);
			if (address == null)
			{
				Globals.Message.ShowInformation(Res.GetString("B22E06D0-A6DF-45D8-9CDB-8607F9F126C4", "Your selected Organization Address is from the Lead Generation Database, which has been decommissioned. Please choose another address."));
			}
			else
			{
				var contact = CreateOrUpdateContactFromManualValues(address.OA_OH);
				var contactPk = contact != null ? contact.PK : ZGuid.Empty;
				LinkToOrganizationDirectly(address.OA_OH, address.PK, contactPk);
			}
		}
	}
}
