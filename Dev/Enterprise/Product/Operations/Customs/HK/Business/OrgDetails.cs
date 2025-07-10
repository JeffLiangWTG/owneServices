using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.HK.Business
{
	public interface IOrgDetails
	{
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString City { get; }
		ZString ContactDetails { get; set; }
		ZString Country { get; }
		bool IsEmpty { get; }
		ZString Name { get; }
		ZString PostCode { get; }
		ZString State { get; }
		ZString PhoneNo { get; }
		ZString FaxNo { get; }
	}

	public class UnmatchedOrgDetails : IOrgDetails
	{
		public UnmatchedOrgDetails(UnmatchOrgRecord record)
		{
			this.record = record;
		}

		readonly UnmatchOrgRecord record;

		#region IOrgDetails Members

		ZString IOrgDetails.Address1
		{
			get { return record != null ? record.AddressLine1 : ZString.Empty; }
		}

		ZString IOrgDetails.Address2
		{
			get { return record != null ? record.AddressLine2 : ZString.Empty; }
		}

		ZString IOrgDetails.City
		{
			get { return record != null ? record.City : ZString.Empty; }
		}

		ZString IOrgDetails.ContactDetails
		{
			get;
			set;
		}

		ZString IOrgDetails.Country
		{
			get { return record != null ? record.Country : ZString.Empty; }
		}

		bool IOrgDetails.IsEmpty
		{
			get { return record == null; }
		}

		ZString IOrgDetails.Name
		{
			get { return record != null ? record.OrganisationName : ZString.Empty; }
		}

		ZString IOrgDetails.PostCode
		{
			get { return record != null ? record.PostCode : ZString.Empty; }
		}

		ZString IOrgDetails.State
		{
			get { return record != null ? record.StateOrProvince : ZString.Empty; }
		}

		ZString IOrgDetails.PhoneNo => ZString.Empty;

		ZString IOrgDetails.FaxNo => ZString.Empty;

		#endregion
	}

	public class JobDocAddressDetails : IOrgDetails
	{
		public JobDocAddressDetails(JobDocAddress address)
		{
			this.address = address;
		}

		readonly JobDocAddress address;

		#region IOrgDetails Members

		ZString IOrgDetails.Address1
		{
			get { return address != null ? address.E2_Address1 : ZString.Empty; }
		}

		ZString IOrgDetails.Address2
		{
			get { return address != null ? address.E2_Address2 : ZString.Empty; }
		}

		ZString IOrgDetails.City
		{
			get { return address != null ? address.E2_City : ZString.Empty; }
		}

		ZString IOrgDetails.ContactDetails
		{
			get;
			set;
		}

		ZString IOrgDetails.Country
		{
			get { return address != null ? address.E2_RN_NKCountryCode : ZString.Empty; }
		}

		bool IOrgDetails.IsEmpty
		{
			get { return address == null || (!address.E2_AddressOverride && !address.HasRealAddress); }
		}

		ZString IOrgDetails.Name
		{
			get { return address != null ? address.E2_CompanyNameTruncated : ZString.Empty; }
		}

		ZString IOrgDetails.PostCode
		{
			get { return address != null ? address.E2_Postcode : ZString.Empty; }
		}

		ZString IOrgDetails.State
		{
			get { return address != null ? address.E2_State : ZString.Empty; }
		}

		ZString IOrgDetails.PhoneNo
		{
			get { return address != null ? address.E2_Phone : ZString.Empty; }
		}

		ZString IOrgDetails.FaxNo
		{
			get { return address != null ? address.E2_Fax : ZString.Empty; }
		}

		#endregion
	}
}
