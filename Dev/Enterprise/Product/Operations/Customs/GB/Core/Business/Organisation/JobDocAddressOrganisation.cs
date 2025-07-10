using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class JobDocAddressOrganisation : IOrganisation
	{
		protected JobDocAddressOrganisation()
		{ }

		public JobDocAddressOrganisation(IDocAddress address)
		{
			if (address == null)
			{
				throw new ArgumentNullException(nameof(address), "Use OrganisationProvider to support returning 'null' organisations");
			}
			this.address = address;
		}
		readonly IDocAddress address;

		#region IOrganisation Members

		public ZBool IsNotMissing
		{
			get
			{
				return !address.E2_Address1.IsEmpty;
			}
		}

		ZString IOrganisation.CountryCode
		{
			get { return address.CountryCode; }
		}

		ZString IOrganisation.City
		{
			get { return address.E2_City; }
		}

		ZString IOrganisation.EoriCode
		{
			get
			{
				if (address.E2_AddressOverride)
				{
					// We are overriding the address, for a one-time client, so we must use the EORI that the user gave in the GovRegNo field
					return address.E2_GovRegNum;
				}
				else
				{
					// We are attached to a REAL org, so we can use their OrgCusCode
					return address.Organisation != null ? ((OrgHeader)address.Organisation)?.GetEuIdentificationNumber() ?? ZString.Empty : ZString.Empty;
				}
			}
		}

		ZString IOrganisation.ShortCode
		{
			get
			{
				return address.E2_AddressOverride ? ZString.Empty :
							address.Organisation != null ? address.Organisation?.Code ?? ZString.Empty : ZString.Empty;
			}
		}

		ZString IOrganisation.Name
		{
			get { return address.E2_CompanyNameTruncated; }
		}

		ZString IOrganisation.PostCode
		{
			get { return address.E2_Postcode; }
		}

		ZString IOrganisation.Street
		{
			get { return address.E2_Address1; }
		}

		#endregion
	}
}
