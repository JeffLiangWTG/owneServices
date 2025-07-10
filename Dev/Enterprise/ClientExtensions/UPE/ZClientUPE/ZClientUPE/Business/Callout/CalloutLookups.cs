using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutLookups : UPECusHAWBLookups
	{
		public CalloutLookups(Callout parent)
			: base(parent)
		{
		}

		#region Bill To

		#region BillToOrganisationList

		public OrganisationsFindBoxCollection BillToOrganisationList
		{
			get
			{
				if (fBillToOrganisationList == null)
				{
					fBillToOrganisationList = new OrganisationsFindBoxCollection(Factory);
				}
				return fBillToOrganisationList;
			}
		}

		OrganisationsFindBoxCollection fBillToOrganisationList;

		#endregion

		#region BillToCountryList

		public RefCountryCollection BillToCountryList
		{
			get
			{
				if (fBillToCountryList == null)
				{
					fBillToCountryList = new RefCountryCollection(Factory);
				}
				return fBillToCountryList;
			}
		}

		RefCountryCollection fBillToCountryList;

		#endregion

		#endregion

		#region Importer

		#region ImporterOrganisationList

		public OrganisationsFindBoxCollection ImporterOrganisationList
		{
			get
			{
				if (fImporterOrganisationList == null)
				{
					fImporterOrganisationList = new OrganisationsFindBoxCollection(Factory);
				}
				return fImporterOrganisationList;
			}
		}

		OrganisationsFindBoxCollection fImporterOrganisationList;

		#endregion

		#region ImporterCountryList

		public RefCountryCollection ImporterCountryList
		{
			get
			{
				if (fImporterCountryList == null)
				{
					fImporterCountryList = new RefCountryCollection(Factory);
				}
				return fImporterCountryList;
			}
		}

		RefCountryCollection fImporterCountryList;

		#endregion

		#endregion
	}
}
