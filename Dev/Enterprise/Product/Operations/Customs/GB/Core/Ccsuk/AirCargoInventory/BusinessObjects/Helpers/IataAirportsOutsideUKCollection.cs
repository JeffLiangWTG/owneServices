using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class IataAirportsOutsideUKCollection : RefUNLOCOCollection
	{
		public IataAirportsOutsideUKCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.UnitedKingdom);
			result.AddToFilter(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.NotEqual, ZString.Empty);
			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
