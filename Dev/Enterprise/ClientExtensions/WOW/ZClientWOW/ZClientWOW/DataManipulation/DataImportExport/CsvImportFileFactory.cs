using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	internal static class CsvImportFileFactory
	{
		internal static CsvImportFile TryCreate(BusinessObjectFactoryProvider factoryProvider, StreamReader reader, INotifications notify)
		{
			CsvImportFile result = null;
			string[] topLineFieldValues = null;

			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;
			string topLine = reader.ReadLine();

			if (topLine != null)
			{
				topLineFieldValues = new OCsvLine(topLine).FieldValues;
				result = TryCreateManifestFile(factoryProvider, reader, topLineFieldValues, notify);

				if (result == null)
				{
					string recordTypeID = topLineFieldValues[0];
					if (recordTypeID == "0")
					{
						MIHeaderCsvRecord header = new MIHeaderCsvRecord(topLine);
						result = TryCreateFromEI(factoryProvider, reader, topLineFieldValues[1].Trim());
					}
				}
			}

			if (result == null)
			{
				notify.Notify(new ErrorNotification(WowErrorType.MissingHeader, ""));
			}
			return result;
		}

		internal static CsvImportFile TryCreateFromEI(BusinessObjectFactoryProvider factoryProvider, StreamReader reader, string fileType)
		{
			CsvImportFile result = null;
			switch (fileType)
			{
				case "MI-PRODUCT":
					result = new ProductsCsvImportFile(factoryProvider, reader);
					break;
				case "MI-ORDER":
					result = new OrdersCsvImportFile(factoryProvider, reader);
					break;
				case WowConstants.EdiTrackMessageTypeName:
					result = new UnprocessedOrdersAndContainersForEdiTrackCsvImportFile(factoryProvider, reader);
					break;
				case WowConstants.EdiTrackAllOrdersMessage:
					result = new UnprocessedOrdersAndContainersForEdiTrackCsvImportFile(factoryProvider, reader);
					break;
			}
			return result;
		}

		internal static CsvImportFile TryCreateManifestFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader, string[] topLineFieldValues, INotifications notify)
		{
			CsvImportFile result = null;
			if (topLineFieldValues[0].ToLower() == "document" && topLineFieldValues[1].ToLower() == "manifest")
			{
				result = new ContainerManifestCsvImportFile(factoryProvider, reader);
			}
			return result;
		}
	}
}
