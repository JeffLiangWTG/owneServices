using System.Collections.Generic;

using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IDLMHeader : IMessageAttachee
	{
		IDLMOrganisation DLMExporter { get; }
		IDLMOrganisation DLMConsignee { get; }
		IDLMOrganisation DLMServiceProvider { get; }

		IDLMOrganisation DLMCertifier { get; }
		ZString CertifierName { get; }
		ZString CertifierStatus { get; }

		ZDecimal CommodityGrossWeight { get; }
		ZString CommodityGrossWeightUnitOfMeasure { get; }
		ZDecimal FreightCharges { get; }
		ZString CommodityCurrencyOfDeclaredValue { get; }
		ZString ModeOfTransport { get; }
		ZString ReasonForExport { get; }
		ZString VesselName { get; }
		ZString CountryOfFinalDestination { get; }
		ZDateTime DateOfExportation { get; }
		ZString PortOfExit { get; }
		ZString PlaceOfReport { get; }
		ZInt NumberOfPackages { get; }
		ZString KindOfPackages { get; }
		ZString NameOfExportingCompany { get; }
		ZString TransportationDocumentNumber { get; }

		IEnumerable<IDLMDetailLine> Details { get; }
		ZString[] DLMPermits { get; }
		ZString[] DLMContainers { get; }
		ZString[] DLMReferences { get; }
	}
}
