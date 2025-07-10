using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.DocumentScanning.Integration
{
	// If you add members to these types, create a new file for them.

	public interface IStorageDocsValueObjectDataAdapter { }
	public interface IStorageFilesValueObjectDataAdapter { }
	public interface IAssemblyDataLookup { }

	public interface IDocManagerDBHelper
	{
		int LastWritableDatabaseWithFreeSpace(int sizeRequiredInMB = 0, bool forceNewDatabase = false, StringBuilder builder = null);
		string GetLastWritableDatabaseName();
		IEnumerable<int> GetStorageDocDbNumbersIncludingMainDb();
	}

	public interface IDocumentUtilities
	{
		void ConvertFileToTiff(string existingFile, string destinationFile);
		void AppendMultiPageImageToFile(string sourceMultiPageImageFile, string outputFilePath);
		Byte[] GetFileAsBytes(string filePath);
	}

	public interface IEDocsPlugIn : IMonitorEDocAttachment
	{
		void ForceSetup();
		void DisableInsert();
	}

	public interface IMonitorEDocAttachment
	{
		void StartMonitorEDocAttachment();
		void EndMonitorEDocAttachment();
		bool IsMonitoringDocumentAttachment { get; set; }
	}

	public interface IDragDropSupportBase
	{
		string[] Add(string[] files);
	}
}
