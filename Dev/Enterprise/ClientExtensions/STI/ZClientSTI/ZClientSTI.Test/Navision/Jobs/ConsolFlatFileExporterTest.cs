using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(ConsolFlatFileExporter))]
	public class ConsolFlatFileExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter()
		{
			return new ConsolFlatFileExporter(Factory, Instructions, "blah", "");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetDataExporter();
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			MainFormConsolCollection consols = new MainFormConsolCollection(Factory);
			CommonConsol consol = NavisionTestHelper.ConsolForTesting(Factory);
			consols.Add(consol);
			return consols;
		}

		#region Instructions
		ExportInstructions Instructions
		{
			get
			{
				if (fInstructions == null)
				{
					fInstructions = new ExportInstructions();
					fInstructions.BasePath = Env.TempPath;
					fInstructions.FileExtension = FileExtensionType.Csv;
					fInstructions.MethodOfExport = ExportType.File;
				}

				return fInstructions;
			}
		}

		ExportInstructions fInstructions;
		#endregion
	}
}
