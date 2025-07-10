using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public interface IJobDeclarationCustomsMessageWrapper
{
	ZString BorderMeansOfTransportNationality { get; }
	ZString GoodsCountryOfDestination { get; }
	ZString GoodsCountryOfOrigin { get; }
	ZInt ContainerModeForImportMessage { get; }
	ZBool IsContainerizedTransport { get; }
	ZString Ucr { get; }
	IArrivalMeansOfTransport ArrivalMeansOfTransport { get; }
	int BorderTransportMode { get; }
	IEoriTrader ImportDeclarant { get; }
	ZString DeclarationCustomsOffice { get; }
	IEoriTrader Importer { get; }
	int? InlandTransportMode { get; }
	ZString RegionOfDestination { get; }
	IRepresentative Representative { get; }
	IEoriTrader Seller { get; }
	ILocationOfGoods ImportLocationOfGoods { get; }
	ZString SupervisingCustomsOffice { get; }
	ZString PresentationCustomsOffice { get; }
	IEoriTrader Buyer { get; }
	ZString DutyPayerIdentificationNumber { get; }
	IEoriTrader Supplier { get; }
	string EntryStyle { get; }

	#region  Only Export

	ZString CountryOfExport { get; }

	IMeansOfTransport GetExportBorderMeansOfTransport(CusEntryInstruction entryInstruction);

	CargoWise.Customs.IT.MessageContracts.Declaration.Export.ILocationOfGoods ExportLocationOfGoods { get; }
	string CarrierIdentificationNumber { get; }
	string ExitCustomsOffice { get; }
	string ExportCustomsOffice { get; }
	IEoriTrader Exporter { get; }
	ZBool IsSecurityDeclaration { get; }
	ZString SpecificCircumstanceIndicator { get; }
	IEoriTrader ExportDeclarant { get; }
	IReadOnlyCollection<IConsignmentCountryRouting> ConsignmentRoutings { get; }

	#endregion
}
