using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.CustomsFiles.AU.DataImporters.DbCyber2
{
	public class LookupDataImporter : InterbaseImporter
	{
		public LookupDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords, ZBool importToCSVFile) : base(logger, dataSourcePath, excludeExistingRecords, importToCSVFile)
		{
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			DataRow lookupRecord = GetNextDataRow();

			ClassificationWriter writer = new ClassificationWriter(factory);

			writer.LookupCode = new ZString(lookupRecord[Cyber2Schema.Lookup.LookupCode]).Trim();
			ZString ahecc				= new ZString(lookupRecord[Cyber2Schema.Lookup.Ahecc]).Trim();
			writer.ClassificationType = ahecc.IsEmpty ? "IMP" : "EXP";
			writer.Description			= new ZString(lookupRecord[Cyber2Schema.Lookup.TariffDescription]).Trim();
			ZString instrType			= new ZString(lookupRecord[Cyber2Schema.Lookup.InstrumentType]).Trim();
			writer.InstrumentType	= SetInstrumentType(instrType);
			writer.InstrumentCode	= new ZString(lookupRecord[Cyber2Schema.Lookup.InstrumentCode]).Trim();
			writer.Treatment			= new ZString(lookupRecord[Cyber2Schema.Lookup.Treatment]).Trim();
			ZString tariffStat			= new ZString(lookupRecord[Cyber2Schema.Lookup.Statistics]).Trim().Right(2);
			writer.TariffCode			= new ZString(lookupRecord[Cyber2Schema.Lookup.Tariff]).Trim() + tariffStat;

			return writer;
		}

		internal ZString SetInstrumentType(ZString code)
		{
			ZString result = ZString.Empty;
			switch (code.Left(2).ToUpper())
			{
				case "TC":
					result = "TC1";
					break;
				case "MD":
					result = "MD1";
					break;
				default:
					result = code.ToUpper();
					break;
			}
			return result;
		}

		protected override string DataTypeDescription
		{
			get { return "Lookups";	}
		}

		protected override internal ZString SqlText
		{
			get { return new ZString("select " + Fields + " from CustPartLookup"); }
		}

		protected string Fields
		{
			get { return "code, tariff, statistic, treat, uq, instument, instr_type, description, add_info1, uq, by_law, drawback, ahecc"; }
		}

		protected override internal ZString CSVOutputHeader
		{
			get {	return "Code,Type,Description,Tariff,Treatment,Instrument,Concession"; }
		}
	}
}
