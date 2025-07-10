using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FNLine : IImport5FNLine
	{
		public int EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string ModelName { get; set; }
		public string SerialNumber { get; set; }
		public string UseCodeDescription { get; set; }
		public string ProductTypeCode { get; set; }
		public string PostClearanceProcedureYN { get; set; }
		public string Remark { get; set; }
		public string DutyReductionClassification { get; set; }
		public string ReductionRateRegulationGroupNumber { get; set; }
		public string ReductionRateRegulationSeqNumber { get; set; }
		public string ReductionRateRegulationItemNumber { get; set; }
		public string ScheduledReExportCustomsOffice { get; set; }
		public string ReExportDestinationCountryCode { get; set; }
		public string JurisdictionalCustomsOffice { get; set; }
		public DateTime ScheduledReExportDate { get; set; }
		public Organisation GoodsLocation { get; set; }
		public Import5FNHeader Header { get; set; }

		ZInt IImport5FNLine.EntryLineNo => EntryLineNo;
		ZString IImport5FNLine.HSCode => HSCode;
		ZString IImport5FNLine.ModelName => ModelName;
		ZString IImport5FNLine.SerialNumber => SerialNumber;
		ZString IImport5FNLine.UseCodeDescription => UseCodeDescription;
		ZString IImport5FNLine.ProductTypeCode => ProductTypeCode;
		ZString IImport5FNLine.PostClearanceProcedureYN => PostClearanceProcedureYN;
		ZString IImport5FNLine.Remark => Remark;
		ZString IImport5FNLine.DutyReductionClassification => DutyReductionClassification;
		ZString IImport5FNLine.ReductionRateRegulationGroupNumber => ReductionRateRegulationGroupNumber;
		ZString IImport5FNLine.ReductionRateRegulationSeqNumber => ReductionRateRegulationSeqNumber;
		ZString IImport5FNLine.ReductionRateRegulationItemNumber => ReductionRateRegulationItemNumber;
		ZString IImport5FNLine.ScheduledReExportCustomsOffice => ScheduledReExportCustomsOffice;
		ZString IImport5FNLine.ReExportDestinationCountryCode => ReExportDestinationCountryCode;
		ZString IImport5FNLine.JurisdictionalCustomsOffice => JurisdictionalCustomsOffice;
		ZDate IImport5FNLine.ScheduledReExportDate => (ZDate)ScheduledReExportDate;
		IOrganization IImport5FNLine.GoodsLocation => GoodsLocation;
		IImport5FNHeader IImport5FNLine.Header => Header;
	}
}
