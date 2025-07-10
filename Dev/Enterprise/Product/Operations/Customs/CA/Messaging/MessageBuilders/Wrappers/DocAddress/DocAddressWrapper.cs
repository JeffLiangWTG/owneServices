using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Messaging
{
	public class DocAddressWrapper : IDocAddress
	{
		public DocAddressWrapper(ZString name)
		{
			E2_CompanyName = name;
		}

		public ZString E2_CompanyName { get; set; }
		public ZString E2_CompanyNameTruncated
		{
			get { return E2_CompanyName.Substring(0, JobDocAddress.Schema.E2_CompanyNameTruncatedLength); }
		}

		public ZString E2_Postcode { get; set; }
		public ZString E2_State { get; set; }
		public ZString CountryCode { get; set; }

		#region IDocAddress Members Plug

		public ZString E2_AdditionalAddressInformation
		{
			get { return ZString.Empty; }
		}

		public ZString E2_Address1
		{
			get { return ZString.Empty; }
		}

		public ZString E2_Address2
		{
			get { return ZString.Empty; }
		}

		public ZBool E2_AddressOverride
		{
			get { return false; }
		}

		public ZString E2_AddressType
		{
			get { return ZString.Empty; }
		}

		public ZString E2_City
		{
			get { return ZString.Empty; }
		}

		public ZString E2_GovRegNum
		{
			get { return ZString.Empty; }
		}

		public ZString E2_GovRegNumType
		{
			get { return ZString.Empty; }
		}

		public ZGuid E2_OA_Address
		{
			get { return ZGuid.Empty; }
		}

		public ZString E2_PortCode
		{
			get { return ZString.Empty; }
		}

		public ZString ParentDescription
		{
			get { return ZString.Empty; }
		}

		public ZString AddressCaption
		{
			get { return ZString.Empty; }
		}

		public ZString E2_Fax
		{
			get { return ZString.Empty; }
		}

		public ZString E2_Phone
		{
			get { return ZString.Empty; }
		}

		public IOrgHeader Organisation
		{
			get { return null; }
		}

		public ZString AddressFull => ZString.Empty;

		public ZString CountryDescription => ZString.Empty;

		ZString IDocAddress.E2_PassportID
		{
			get { return ZString.Empty; }
		}

		ZString IDocAddress.E2_PassportCountryOfIssue
		{
			get { return ZString.Empty; }
		}

		ZDateTime IDocAddress.E2_PassportDateOfBirth
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDocAddress.E2_RN_NKCountryCode
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
