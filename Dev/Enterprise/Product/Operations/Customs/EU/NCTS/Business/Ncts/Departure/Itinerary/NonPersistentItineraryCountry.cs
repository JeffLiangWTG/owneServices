using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentItineraryCountry : AutoNonPersistentItineraryCountry
	{
		public NonPersistentItineraryCountry()
			: base(new BusinessObjectFactory())
		{
		}

		public NonPersistentItineraryCountry(ZString countryCode)
			: base(new BusinessObjectFactory())
		{
			CountryCode = countryCode;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("C0705D51-7BDF-4D04-970D-D5E366F551D5", "Itinerary Country");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public NonPersistentItineraryCountry(int sequence, BusinessObjectFactory factory)
			: base(factory)
		{
			using (SuspendSettingHasChanges())
			{
				Sequence = sequence;
			}
		}

		[List(nameof(ItineraryCountries))]
		public override ZString CountryCode
		{
			get { return base.CountryCode; }
			set
			{
				base.CountryCode = value;
				CountryCodeInfo.RefreshBinding();
			}
		}

		public CodeDescriptionPairList ItineraryCountries
		{
			get
			{
				return Factory.GetCachedValue("EU.NCTS.Business.Ncts.Departure.Itinerary.NonPersistentItineraryCountry.ItineraryCountries", delegate
				{
					var countryList = new CodeDescriptionPairList();
					countryList.AddRange(new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_IsActive, true)));
					countryList.AddPairIfNotExist(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Res.GetString("5210C431-6234-4BCC-B941-36B0EDADF84E", "Northern Ireland"));
					countryList.SortByDescription();
					return countryList;
				});
			}
		}

		public NctsHeader Header { get; set; }
	}
}
