
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.Rohlig.HarleyDavidson
{
	public class HarleyDavidsonFlatFileFormat : CsvFlatFileFormat
	{
		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow row = null;
			if (rawRow != "PDLITM,PDDSC1,PDUORG,COSTAUS,PDCNID")
			{
				row = base.ConvertToRow(rawRow);
			}
			return row;
		}
	}
}
