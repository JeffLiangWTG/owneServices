using System.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.MFI.CaroTrans.Export
{
	public class CaroTransExportInstructions : ExportInstructions
	{
		protected override ZString ConstructFilePathWithExtension()
		{
			return Path.Combine(BasePath, SpecifiedFilename);
		}
	}
}
