
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SummaryDataRow : BaseDataRow
	{
		public SummaryDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.Summary)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.Summary.NumberOfMTContainers, GetValue(ShipnetConstants.Summary.NumberOfMTContainersPosition, ShipnetConstants.Summary.NumberOfMTContainersMaxLength));
			SetField(ShipnetConstants.Summary.NumberOfFullContainers, GetValue(ShipnetConstants.Summary.NumberOfFullContainersPosition, ShipnetConstants.Summary.NumberOfFullContainersMaxLength));
			SetField(ShipnetConstants.Summary.TotalGrossWeightKGS, GetValue(ShipnetConstants.Summary.TotalGrossWeightKGSPosition, ShipnetConstants.Summary.TotalGrossWeightKGSMaxLength));
			SetField(ShipnetConstants.Summary.TotalNetWeightKGS, GetValue(ShipnetConstants.Summary.TotalNetWeightKGSPosition, ShipnetConstants.Summary.TotalNetWeightKGSMaxLength));
			SetField(ShipnetConstants.Summary.FileTotalTareWeight, GetValue(ShipnetConstants.Summary.FileTotalTareWeightPosition, ShipnetConstants.Summary.FileTotalTareWeightMaxLength));
			SetField(ShipnetConstants.Summary.TotalGrossCubeCBM, GetValue(ShipnetConstants.Summary.TotalGrossCubeCBMPosition, ShipnetConstants.Summary.TotalGrossCubeCBMMaxLength));
			SetField(ShipnetConstants.Summary.TotalNetCubeCBM, GetValue(ShipnetConstants.Summary.TotalNetCubeCBMPosition, ShipnetConstants.Summary.TotalNetCubeCBMMaxLength));
			SetField(ShipnetConstants.Summary.TotalNoOfPackages, GetValue(ShipnetConstants.Summary.TotalNoOfPackagesPosition, ShipnetConstants.Summary.TotalNoOfPackagesMaxLength));
			SetField(ShipnetConstants.Summary.TotalNumberOfRecords, GetValue(ShipnetConstants.Summary.TotalNumberOfRecordsPosition, ShipnetConstants.Summary.TotalNumberOfRecordsMaxLength));
		}
	}
}
