using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class ImportLogger : Disposable
	{
		public ImportLogger()
		{
			stats = new Dictionary<ImportStatus, int>();
			foreach (ImportStatus status in Enum.GetValues(typeof(ImportStatus)))
			{
				stats.Add(status, 0);
			}
			tempFile = TempFile.New();
		}

		public void Log(string file, string key, ImportStatus result)
		{
			stats[result]++;
			File.AppendAllText(tempFile.Filename, file + "\t" + key + "\t" + result.ToString() + "\r\n");
		}

		public void Save(Stream stream)
		{
			using (var source = File.OpenRead(tempFile.Filename))
			{
				source.CopyTo(stream);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			tempFile.Dispose();
		}

		public Dictionary<ImportStatus, int> Statistics { get { return stats; } }
		readonly Dictionary<ImportStatus, int> stats = new Dictionary<ImportStatus, int>();

		readonly TempFile tempFile;
	}
}
