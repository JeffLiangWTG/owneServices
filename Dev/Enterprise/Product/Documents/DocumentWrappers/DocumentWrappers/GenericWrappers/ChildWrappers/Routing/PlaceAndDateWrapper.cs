using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Location")]
	public class PlaceAndDateWrapper : GenericWrapper
	{
		public PlaceAndDateWrapper(ZString uNLOCO, ZDateTime estimatedDate, ZDateTime actualDate, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fUNLOCO = uNLOCO;
			fEstimatedDate = estimatedDate;
			fActualDate = actualDate;
		}

		public LocationWrapper Location
		{
			get
			{
				if (fLocation == null)
				{
					fLocation = new LocationWrapper(fUNLOCO, Factory);
				}
				return fLocation;
			}
		}

		public ZDateTime EstimatedDate
		{
			get { return fEstimatedDate; }
		}

		public ZDateTime ActualDate
		{
			get { return fActualDate; }
		}

		public ZDateTime Date
		{
			get { return GetBestValueWithFallback(fActualDate, fEstimatedDate); }
		}

		#region Implementation
		readonly ZString fUNLOCO;
		readonly ZDateTime fEstimatedDate;
		readonly ZDateTime fActualDate;
		LocationWrapper fLocation;
		#endregion
	}
}
