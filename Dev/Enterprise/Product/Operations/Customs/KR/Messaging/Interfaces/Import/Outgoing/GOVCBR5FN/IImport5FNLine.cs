using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5FNLine : IMessageDataProvider
	{
		ZInt EntryLineNo { get; }
		ZString HSCode { get; }
		ZString ModelName { get; }
		ZString SerialNumber { get; }
		ZString UseCodeDescription { get; }
		ZString ProductTypeCode { get; }
		ZString PostClearanceProcedureYN { get; }
		ZString Remark { get; }
		ZString DutyReductionClassification { get; }
		ZString ReductionRateRegulationGroupNumber { get; }
		ZString ReductionRateRegulationSeqNumber { get; }
		ZString ReductionRateRegulationItemNumber { get; }
		ZString ScheduledReExportCustomsOffice { get; }
		ZString ReExportDestinationCountryCode { get; }
		ZString JurisdictionalCustomsOffice { get; }
		ZDate ScheduledReExportDate { get; }
		IOrganization GoodsLocation { get; }
		IImport5FNHeader Header { get; }
	}
}
