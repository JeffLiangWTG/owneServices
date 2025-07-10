
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public abstract class NavisionFlatFileExporter : FlatFileDataExporter
	{
		public NavisionFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile) : base(factory, instructions, exportFile)
		{
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get
			{
				if (fFlatFileFormat == null)
				{
					fFlatFileFormat = new CsvFlatFileFormat(false);
				}
				return fFlatFileFormat;
			}
		}
		IFlatFileFormat fFlatFileFormat;

		protected override bool AppendToFile
		{
			get { return true; }
		}

		public override ZString EnglishDescription
		{
			get { return "Navision Export"; }
		}
	}
}
