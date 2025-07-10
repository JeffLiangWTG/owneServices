using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public static class OrganisationProvider
	{
		/// <summary>
		/// Returns either a wrapped orgHeader or a null pattern object
		/// </summary>
		/// <param name="orgHeader"></param>
		/// <returns></returns>
		public static IOrganisation Get(OrgHeader orgHeader)
		{
			return orgHeader != null ? Get(orgHeader.MainAddress) : new NullOrganisation();
		}

		public static IOrganisation Get(BusinessObjectFactory factory, ZGuid orgHEADERpk)
		{
			return Get(factory.Load<OrgHeader>(orgHEADERpk));
		}

		public static IOrganisation Get(OrgAddress address)
		{
			return address != null ? new OrgHeaderOrganisation(address) : new NullOrganisation();
		}

		public static IOrganisation Get(ZGuid orgADDRESSpk, BusinessObjectFactory factory)
		{
			return Get(factory.Load<OrgAddress>(orgADDRESSpk));
		}

		public static JobDocAddressOrganisation Get(IDocAddress address)
		{
			return address != null ? new JobDocAddressOrganisation(address) : new NullOrganisation();
		}

		class NullOrganisation : JobDocAddressOrganisation, IOrganisation
		{
			public NullOrganisation()
				: base()
			{ }

			ZBool IOrganisation.IsNotMissing => false;

			ZString IOrganisation.CountryCode => ZString.Empty;

			ZString IOrganisation.City => ZString.Empty;

			ZString IOrganisation.ShortCode => ZString.Empty;

			ZString IOrganisation.EoriCode => ZString.Empty;

			ZString IOrganisation.Name => ZString.Empty;

			ZString IOrganisation.PostCode => ZString.Empty;

			ZString IOrganisation.Street => ZString.Empty;
		}
	}
}
