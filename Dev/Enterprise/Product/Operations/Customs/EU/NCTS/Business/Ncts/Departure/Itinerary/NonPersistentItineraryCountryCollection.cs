using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentItineraryCountryCollection : NonPersistentBusinessObjectCollection<NonPersistentItineraryCountry>
	{
		public NonPersistentItineraryCountryCollection(NctsHeader header)
			: base(header.Factory)
		{
			this.header = header;
			MaxCountValidationEnable(maxItineraryRows);
			PopulateItineraryCountryCollection();
		}
		readonly NctsHeader header;

		public void PopulateItineraryCountryCollection()
		{
			var uniqueVoyageIdentifier = header.BH_UniqueVoyageIdentifier;
			for (int i = 0; i < uniqueVoyageIdentifier.Length; i = i + 2)
			{
				var itineraryCountry = this.AddNew();
				using (itineraryCountry.SuspendSettingHasChanges())
				{
					itineraryCountry.CountryCode = uniqueVoyageIdentifier.Substring(i, 2);
					itineraryCountry.Sequence = i / 2 + 1;
					itineraryCountry.Header = header;
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var result = CreateNewNonPersistentItineraryCountry(HighestSequenceNumberSoFar + 1, Factory);
			result.Header = header;

			return result;
		}

		protected virtual NonPersistentItineraryCountry CreateNewNonPersistentItineraryCountry(ZInt sequenceNumber, BusinessObjectFactory factory)
		{
			return new NonPersistentItineraryCountry(sequenceNumber, factory);
		}

		protected override bool AllowNewCore
		{
			get { return Count < maxItineraryRows; }
		}

		int maxItineraryRows { get { return 15; } } // restrict to 15 two character country codes to fit into BH_UniqueVoyageIdentifier varchar(30)

		ZInt HighestSequenceNumberSoFar
		{
			get { return this.Any() ? (from NonPersistentItineraryCountry i in this select i.Sequence).Max() : ZInt.Zero; }
		}
	}
}
