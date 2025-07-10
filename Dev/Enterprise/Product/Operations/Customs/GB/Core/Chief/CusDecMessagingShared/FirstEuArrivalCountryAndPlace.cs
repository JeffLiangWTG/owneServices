using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.CusDec
{
	public class FirstEuArrivalCountryAndPlace
	{
		/// <summary>
		/// Date that the shipment first entered the EU
		/// </summary>
		public ZDateTime DateTime
		{
			get
			{
				//	NB - INTD-ARR-DTM and EU-ARR-LOCN-CODE: Details only to be declared if a combined pre-arrival notification and Customs declaration is being submitted.  Until the rules for pre-arrival notifications are specified and pre-arrival notifications supported by CHIEF, the data element is optional.
				return ZDateTime.Empty;
			}
		}

		/// <summary>
		/// Place in the EU that the shipment first entered the EU
		/// </summary>
		public ZString Place
		{
			get
			{
				//	NB - INTD-ARR-DTM and EU-ARR-LOCN-CODE: Details only to be declared if a combined pre-arrival notification and Customs declaration is being submitted.  Until the rules for pre-arrival notifications are specified and pre-arrival notifications supported by CHIEF, the data element is optional.
				return ZString.Empty;
			}
		}
	}
}
