using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OutturnLineCollectionExportHelper : OutturnLineCollectionHelper
	{
		readonly OutturnLineCollection collection;

		public OutturnLineCollectionExportHelper(OutturnLineCollection collection)
		{
			this.collection = Argument.NotNull(collection, "OutturnLineCollection");
		}

		public string ExportToScanner(string filePath)
		{
			string errorMessage;
			var exportWizard = GetExportWizard();
			exportWizard.FileName = filePath;
			exportWizard.ShowDefaultValue = true;
			exportWizard.ExportCollection(collection, out errorMessage);
			return errorMessage;
		}

		protected virtual ExportWizard GetExportWizard()
		{
			var impl = new ImportCollectionInfoImpl(collection);
			AddCollectionProperties(impl);
			var fileHeader = string.Format("CargoWise ScanCheckManifest,v1.0,Total Consignments:,{0}", collection.Count); // File header
			var fileMapper = new ExportToScannerFileMapper(fileHeader);
			return new ExportWizard(impl, null, fileMapper);
		}

		public class ExportToScannerFileMapper : IFileMapper
		{
			readonly string header;

			public ExportToScannerFileMapper(string header)
			{
				this.header = header;
			}

			public Stream OpenWrite(string unmappedPath)
			{
				var stream = fileMapper.OpenWrite(unmappedPath);
				AppendHeader(stream);
				return stream;
			}

			void AppendHeader(Stream stream)
			{
				var writer = new StreamWriter(stream);
				writer.WriteLine(header);
				writer.Flush();
			}

			public string GetFolderPath(System.Environment.SpecialFolder folder)
			{
				return fileMapper.GetFolderPath(folder);
			}

			public bool IsRemote
			{
				get { return fileMapper.IsRemote; }
			}

			public Stream OpenRead(string unmappedPath)
			{
				return fileMapper.OpenRead(unmappedPath);
			}

			readonly IFileMapper fileMapper = ObjectFactory.Get<IFileMapper>();
		}
	}
}
