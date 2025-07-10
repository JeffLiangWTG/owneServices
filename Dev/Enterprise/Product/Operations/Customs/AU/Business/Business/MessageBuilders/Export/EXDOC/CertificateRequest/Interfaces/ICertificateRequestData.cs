using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface ICertificateRequestData
	{
		BusinessObject Object { get; }
		ZString CommodityType { get; }
		ZString DischargePort { get; }
		ZString DestinationCity { get; }
		ZString CertificateRequiredLocation { get; }
		ZString ExporterCertificateReference { get; }
		ZString NotifyPartyText { get; }
		ZString LetterOfCreditText { get; }
		ZBool SeparateCertificateContainerInd { get; }
		ZBool SeparateCertificateMarksInd { get; }
		ZBool SeparateCertificatePackerInd { get; }
		IEnumerable<IImportPermit> ImportPermits { get; }
		ZString OwnerExporterNumber { get; }
		OrgHeader Consignee { get; }
		OrgHeader Forwarder { get; }
		ZString TransportMode { get; }
		ZString VoyageFlightNumber { get; }
		ZString CarrierName { get; }
		ZString VesselName { get; }
		ZDateTime DepartureDate { get; }
		IEnumerable<ICertificateLine> CertificateLines { get; }
	}
}
