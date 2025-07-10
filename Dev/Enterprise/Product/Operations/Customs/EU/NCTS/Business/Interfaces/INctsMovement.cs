using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface INctsMovement : ICusEntryNumFilterProvider
	{
		BusinessObjectFactory Factory { get; }
		BusinessObject BusinessObject { get; }

		// These members don't cover the entire functionality of the NctsHeader... but cover just what is needed for a TAD or TSAD

		JobDocAddress Consignee { get; }
		JobDocAddress Consignor { get; }
		ZString CountryCode { get; }
		NctsEuOfficeCodeCollection CustomsOffices { get; }
		ZString DeclarantId { get; }
		ZString DeclarationPlace { get; }
		ZString DepartureCustomsOfficeCode { get; }
		INctsGuaranteeCollection<NctsGuarantee> Guarantees { get; }
		ZBool IsSecurityDeclaration { get; }
		NonPersistentItineraryCountryCollection Itinerary { get; }
		ZString ItineraryCountries { get; }
		ZString LocalReferenceNumber { get; }
		ZString MovementReferenceNumber { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingCode { get; set; }
		JobDocAddress Principal { get; }
		JobDocAddress SecurityConsignee { get; }
		JobDocAddress SecurityConsignor { get; }
		List<ICustomsOffice> TransitCustomsOfficeCodeList { get; }
	}
}
