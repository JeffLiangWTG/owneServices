using System;
using System.IO;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	class NativeXmlProcessor : IProcessor
	{
		readonly INativeObjectImporter importer;

		public NativeXmlProcessor(INativeObjectImporter importer)
		{
			this.importer = Argument.NotNull(importer, "importer");
		}

		void IProcessor.Process(Stream stream)
		{
			string errorLog;
			importer.Import(stream, out errorLog, new string[] { typeof(NativeXmlProcessor).Assembly.FullName });
			if (!string.IsNullOrEmpty(errorLog))
			{
				throw new Exception(errorLog);
			}
		}
	}
}