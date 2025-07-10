using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class RTSTranshipmentDetailsLookups : UPECusHAWBFlagDetailsLookups
	{
		public RTSTranshipmentDetailsLookups(RTSTranshipmentDetails parent)
			: base(parent)
		{
		}

		public RefUNLOCOCollection DestinationPortList
		{
			get
			{
				if (fDestinationPortList == null)
				{
					fDestinationPortList = new RefUNLOCOCollection(Factory);
				}
				return fDestinationPortList;
			}
		}

		public RefUNLOCOCollection OriginPortList
		{
			get
			{
				if (fOriginPortList == null)
				{
					fOriginPortList = new RefUNLOCOCollection(Factory);
				}
				return fOriginPortList;
			}
		}

		public RefCountryCollection CountryList
		{
			get
			{
				if (fCountryList == null)
				{
					fCountryList = new RefCountryCollection(Factory);
				}
				return fCountryList;
			}
		}

		RefUNLOCOCollection fDestinationPortList;
		RefUNLOCOCollection fOriginPortList;
		RefCountryCollection fCountryList;
	}
}
