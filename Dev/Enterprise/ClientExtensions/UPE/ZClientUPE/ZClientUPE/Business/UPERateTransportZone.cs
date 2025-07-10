using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPERateTransportZone : RateTransportZone
	{
		public UPERateTransportZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public const string MetroKeyword = "metro";
		public const string CountryKeyword = "country";

		#endregion

		protected override RateTransportZonesValidation GetNewValidation()
		{
			return new UPERateTransportZoneValidation(this);
		}
	}
}
