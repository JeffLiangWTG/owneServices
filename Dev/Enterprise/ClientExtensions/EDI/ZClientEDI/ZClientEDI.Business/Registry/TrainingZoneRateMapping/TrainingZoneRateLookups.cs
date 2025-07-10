using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TrainingZoneRateLookups : ZLookups
	{
		public TrainingZoneRateLookups(TrainingZoneRate parent)
			: base(parent)
		{
		}

		public new TrainingZoneRate Parent
		{
			get { return (TrainingZoneRate)base.Parent; }
		}

		protected override BusinessObjectFactory Factory
		{
			get { return ((ICurrentFactory)Parent).CurrentFactory; }
		}

		public RefZoneHeaderCollection Zones
		{
			get
			{
				if (zones == null)
				{
					ZQuery filter = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training);
					zones = new RefZoneHeaderCollection(Factory, filter);
				}
				return zones;
			}
		}

		RefZoneHeaderCollection zones;

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (currencies == null)
				{
					currencies = new RefCurrencyCollection(Factory);
				}
				return currencies;
			}
		}

		RefCurrencyCollection currencies;
	}
}

