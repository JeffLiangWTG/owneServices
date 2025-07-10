using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.TNT
{
	public struct TNTOrganisation
	{
		public static TNTOrganisation Consignor(ConsignmentRecord record)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = record.ConsignorLegacyCode;
			result.CompanyName = record.ConsignorName;
			result.Address1 = record.ConsignorAddress1;
			result.Address2 = record.ConsignorAddress2;
			result.City = record.ConsignorCity;
			result.PostCode = record.ConsignorPostCode;
			result.State = record.ConsignorState;
			result.Country = record.ConsignorCountry;
			result.Phone = record.ConsignorPhone;
			result.Fax = "";
			result.Email = "";
			result.Language = "";
			result.ContactName = record.ConsignorContactName;
			result.ContactPhone = record.ConsignorContactPhone;
			return result;
		}

		public static TNTOrganisation Consignor(CusHAWB houseBill)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = houseBill.CS_ConsignorName;
			result.Address1 = houseBill.CS_ConsignorStreet;
			result.Address2 = houseBill.CS_ConsignorStreet2;
			result.City = houseBill.CS_ConsignorCity;
			result.State = houseBill.CS_ConsignorState;
			result.PostCode = houseBill.CS_ConsignorPostcode;
			result.Country = houseBill.CS_RN_NKConsignorCountry;
			result.Phone = houseBill.CS_ConsignorPhone;
			result.Fax = "";
			result.Language = "";
			result.ContactName = houseBill.CS_ConsignorContactName;
			result.ContactPhone = "";
			return result;
		}

		public static TNTOrganisation Consignee(CusHAWB houseBill)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = houseBill.CS_ConsigneeName;
			result.Address1 = houseBill.CS_ConsigneeStreet;
			result.Address2 = houseBill.CS_ConsigneeStreet2;
			result.City = houseBill.CS_ConsigneeCity;
			result.State = houseBill.CS_ConsigneeState;
			result.PostCode = houseBill.CS_ConsigneePostcode;
			result.Country = houseBill.CS_RN_NKConsigneeCountry;
			result.Phone = houseBill.CS_ConsigneePhone;
			result.Fax = "";
			result.Language = "";
			result.ContactName = houseBill.CS_ConsigneeContactName;
			result.ContactPhone = "";
			return result;
		}

		public static TNTOrganisation Consignee(ConsignmentRecord record)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = record.ConsigneeName;
			result.Address1 = record.ConsigneeAddress1;
			result.Address2 = record.ConsigneeAddress2;
			result.City = record.ConsigneeCity;
			result.PostCode = record.ConsigneePostCode;
			result.State = record.ConsigneeState;
			result.Country = record.ConsigneeCountry;
			result.Phone = record.ConsigneePhone;
			result.Fax = record.ConsigneeFax;
			result.Email = "";
			result.Language = "";
			result.ContactName = record.ConsigneeContactName;
			result.ContactPhone = record.ConsigneeContactPhone;
			return result;
		}

		public static TNTOrganisation Pickup(ConsignmentRecord record)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = record.PickupName;
			result.Address1 = record.PickupAddress1;
			result.Address2 = record.PickupAddress2;
			result.City = record.PickupCity;
			result.PostCode = record.PickupPostCode;
			result.State = record.PickupState;
			result.Country = record.PickupCountry;
			result.Phone = record.PickupPhone;
			result.Fax = "";
			result.Email = "";
			result.Language = "";
			result.ContactName = record.PickupContactName;
			result.ContactPhone = record.PickupContactPhone;
			return result;
		}

		public static TNTOrganisation Delivery(ConsignmentRecord record)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = record.DeliveryName;
			result.Address1 = record.DeliveryAddress1;
			result.Address2 = record.DeliveryAddress2;
			result.City = record.DeliveryCity;
			result.PostCode = record.DeliveryPostCode;
			result.State = record.DeliveryState;
			result.Country = record.DeliveryCountry;
			result.Phone = record.DeliveryPhone;
			result.Fax = "";
			result.Email = "";
			result.Language = "";
			result.ContactName = record.DeliveryContactName;
			result.ContactPhone = record.DeliveryContactPhone;
			return result;
		}

		public static TNTOrganisation JobDocAddress(JobDocAddress docAddress)
		{
			TNTOrganisation result = new TNTOrganisation();
			result.LegacyCode = "";
			result.CompanyName = docAddress.E2_CompanyNameTruncated;
			result.Address1 = docAddress.E2_Address1;
			result.Address2 = docAddress.E2_Address2;
			result.City = docAddress.E2_City;
			result.PostCode = docAddress.E2_Postcode;
			result.State = docAddress.E2_State;
			result.Country = docAddress.E2_RN_NKCountryCode;
			result.Phone = docAddress.E2_Phone;
			result.Fax = docAddress.E2_Fax;
			result.Email = docAddress.E2_Email;
			result.ContactName = docAddress.E2_Contact;
			result.ContactPhone = docAddress.E2_Phone;
			return result;
		}

		public bool IsValid
		{
			get { return !CompanyName.IsEmpty; }
		}

		public ZString LegacyCode;
		public ZString CompanyName;
		public ZString Address1;
		public ZString Address2;
		public ZString City;
		public ZString PostCode;
		public ZString State;
		public ZString Country;
		public ZString Phone;
		public ZString Fax;
		public ZString Email;
		public ZString Language;
		public ZString ContactName;
		public ZString ContactPhone;
	}
}
