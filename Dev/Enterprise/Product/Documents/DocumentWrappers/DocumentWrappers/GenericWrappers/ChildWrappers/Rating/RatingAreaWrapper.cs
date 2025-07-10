using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Name")]
	[WrapperTypeName("Rating Area")]
	public class RatingAreaWrapper : GenericWrapper
	{
		public RatingAreaWrapper(ZString code, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.location = LocationHelper.GetLocationFromString(code, factory);
		}

		public RatingAreaWrapper(ILocation location, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.location = location;
		}

		public ZString Code
		{
			get { return location == null ? ZString.Empty : location.Code; }
		}

		public ZString Name
		{
			get { return location == null ? ZString.Empty : location.Description; }
		}

		public ZBool IsCountry
		{
			get { return location is RefCountry; }
		}

		public ZBool IsPort
		{
			get { return location is RefUNLOCO; }
		}

		public ZBool IsZone
		{
			get { return location is RefZoneHeader; }
		}

		public LocationWrapper Port
		{
			get
			{
				if (port == null)
				{
					RefUNLOCO unloco;

					if (location != null && (unloco = location.UNLOCO) != null)
					{
						port = new LocationWrapper(unloco.RL_Code, Factory);
					}
					else
					{
						port = new LocationWrapper("", Factory);
					}
				}

				return port;
			}
		}
		LocationWrapper port;

		public CountryWrapper Country
		{
			get
			{
				if (country == null)
				{
					country = new CountryWrapper(location == null ? null : location.Country, Factory);
				}

				return country;
			}
		}
		CountryWrapper country;

		readonly ILocation location;
	}
}
