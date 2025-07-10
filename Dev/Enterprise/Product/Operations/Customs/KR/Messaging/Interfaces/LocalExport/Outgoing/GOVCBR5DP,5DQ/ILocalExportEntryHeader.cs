using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportEntryHeader : IMessageDataProvider, IEntryHeaderWithEntryLines
	{
		ZString DeclarationType { get; }
		ZString DeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		[DataItemID("11A")]
		ZString BondedAreaCode { get; }
		ZDate DeclarationDate { get; }
		IOrganization Supplier { get; }
		IOrganization Exporter { get; }
		IOrganization Manufacturer { get; }
		IOrganization Importer { get; }
		ZString FlightNoOrVesselName { get; }
		[DataItemID("13")]
		ZString MRNNo { get; }
		[DataItemID("38")]
		ZInt CrewCount { get; }
		[DataItemID("39")]
		ZInt ScheduledSailingDays { get; }
		[DataItemID("14")]
		ZString GoodsType { get; }
		[DataItemID("15")]
		ZString DrawbackApplicantType { get; }
		[DataItemID("12")]
		ZString VesselRadioCallSign { get; }
		ZDecimal TotalDeclarationAmount { get; }
		[DataItemID("18")]
		ZDecimal TotalGrossWeight { get; }
		[DataItemID("16")]
		ZInt TotalPackages { get; }
		IEnumerable<ILocalExportStevedore> Stevedores { get; }
		new IEnumerable<ILocalExportEntryLine> EntryLines { get; }
		IEnumerable<ILocalExportOtherTransportMeans> OtherTransportMeans { get; }
	}
}
