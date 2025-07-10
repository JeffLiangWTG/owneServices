using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class ItineraryCountryCollection : CusCodeDataCollection<ItineraryCountry>, ISequenceNumberHeader
	{
		public ItineraryCountryCollection(BusinessObject parent) : base(parent, CusCodeDataTypeList.Codes.CountryOfRoutingCode)
		{
		}

		public new JobDeclaration Master => (JobDeclaration)base.Master;

		public void PopulateItineraryCountryCollection()
		{
			var uniqueVoyageIdentifier = Master.UniqueVoyageIdentifier;
			for (int i = 0; i < uniqueVoyageIdentifier.Length; i = i + 2)
			{
				var itineraryCountry = AddNew();
				using (itineraryCountry.SuspendSettingHasChanges())
				{
					itineraryCountry.CY_Code = uniqueVoyageIdentifier.SubstringSafe(i, 2);
					itineraryCountry.CY_Order = (CargoWise.Types.ZShort)(i / 2 + 1);
				}
			}
		}

		protected override bool AllowNewCore => Count < 99;

		#region ISequenceNumberHeader

		public ShortSequenceNumberGenerator SequenceNumberCalculator => sequenceNumberCalculator ?? (sequenceNumberCalculator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator sequenceNumberCalculator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;

		#endregion
	}
}
