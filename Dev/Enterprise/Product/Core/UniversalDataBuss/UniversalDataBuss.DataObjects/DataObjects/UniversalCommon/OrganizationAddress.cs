using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class OrganizationAddress : IDataObject, IOrganizationAddress
	{
		public OrganizationAddress(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public OrganizationAddress()
		{
		}

		[MaxLength(40), Mandatory]
		public ZString? AddressType { get; set; }
		[MaxLength(25)]
		public ZString? AddressShortCode { get; set; }
		[MaxLength(12), CodeMap(Constants.OrgPatternMatchOverrideRelationships.Organisation)]
		public ZCodeMappedZString? OrganizationCode { get; set; }
		[MaxLength(3)]
		public ZString? OrganizationCategory { get; set; }
		[MaxLength(50)]
		public ZString? AdditionalAddressInformation { get; set; }
		[MaxLength(50)]
		public ZString? Address1 { get; set; }
		[MaxLength(50)]
		public ZString? Address2 { get; set; }
		public ZBool? AddressOverride { get; set; }
		[MaxLength(50)]
		public ZString? City { get; set; }
		[MaxLength(200)]
		public ZString? CompanyName { get; set; }
		[MaxLength(256)]
		public ZString? Contact { get; set; }
		public UNLOCO Port { get; set; }
		public Country Country { get; set; }
		[MaxLength(254)]
		public ZString? Email { get; set; }
		[MaxLength(20)]
		public ZString? Fax { get; set; }
		[MaxLength(254)]
		public ZString? GovRegNum { get; set; }
		public RegistrationNumberType GovRegNumType { get; set; }
		[MaxLength(20)]
		public ZString? Mobile { get; set; }
		[MaxLength(20)]
		public ZString? Phone { get; set; }
		[MaxLength(10)]
		public ZString? Postcode { get; set; }

		public CodeDescriptionPair ScreeningStatus { get; set; }

		public CodeDescriptionPair ValidationStatus { get; set; }

		public OrganizationAddressState State { get; set; }

		[MaxLength(254)]
		public ZString? UniversalNettingCode { get; set; }
		[MaxLength(254)]
		public ZString? UniversalOfficeCode { get; set; }

		public List<RegistrationNumber> RegistrationNumberCollection { get; private set; }

		public ZBool? IsResidential { get; set; }

		public ZBool? SuppressAddressValidationError { get; set; }

		public List<OrganizationLocalAddress> LocalAddressCollection { get; private set; }

		public GeoLocation GeoLocation { get; set; }

		ZString? IOrganizationAddress.OrganizationCode
		{
			get { return OrganizationCode; }
			set { OrganizationCode = value; }
		}

		ZString? IOrganizationAddress.OrganizationCategory
		{
			get { return OrganizationCategory; }
			set { OrganizationCategory = value; }
		}		

		ICodeNameDataObject IOrganizationAddress.Country
		{
			get { return Country; }
		}

		ZString? IOrganizationAddress.State
		{
			get { return State; }
			set { State = value; }
		}
	}
}
