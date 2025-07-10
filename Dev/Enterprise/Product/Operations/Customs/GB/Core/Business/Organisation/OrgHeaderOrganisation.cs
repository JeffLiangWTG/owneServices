using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class OrgHeaderOrganisation : IOrganisation
	{
		public OrgHeaderOrganisation(OrgAddress address)
		{
			if (address == null)
			{
				throw new ArgumentNullException(nameof(address), "Use OrganisationProvider to support returning 'null' organisations");
			}
			this.address = address;
		}
		readonly OrgAddress address;

		#region IOrganisation Members

		ZBool IOrganisation.IsNotMissing => true;

		ZString IOrganisation.CountryCode
		{
			get
			{
				if (!address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					return address.OA_RL_NKRelatedPortCode.Left(2);
				}
				return ZString.Empty;
			}
		}

		ZString IOrganisation.City
		{
			get { return address.OA_City; }
		}

		ZString IOrganisation.EoriCode
		{
			get { return address.Header?.GetEuIdentificationNumber() ?? ZString.Empty; }
		}

		ZString IOrganisation.ShortCode
		{
			get { return address.Header?.OH_Code ?? ZString.Empty; }
		}

		ZString IOrganisation.Name
		{
			get { return address.Header?.OH_FullNameTruncated ?? ZString.Empty; }
		}

		ZString IOrganisation.PostCode
		{
			get { return address.OA_PostCode; }
		}

		ZString IOrganisation.Street
		{
			get { return address.OA_Address1; }
		}

		#endregion
	}
}
