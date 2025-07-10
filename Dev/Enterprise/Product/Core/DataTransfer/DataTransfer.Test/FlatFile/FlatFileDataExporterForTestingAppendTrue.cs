using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileDataExporterForTestingAppendTrue : FlatFileDataExporterForTesting
	{
		public FlatFileDataExporterForTestingAppendTrue(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportedFileToAppendTo)
			: base(factory, instructions, exportedFileToAppendTo)
		{
		}

		protected override bool AppendToFile
		{
			get { return true; }
		}
	}
}
