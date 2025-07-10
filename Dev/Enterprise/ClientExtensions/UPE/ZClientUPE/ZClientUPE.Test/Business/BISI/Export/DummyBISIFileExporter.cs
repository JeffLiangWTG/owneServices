using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class DummyBISIFileExporter : UPEFileExporter, IBISIFileExporter
	{
		public void SaveDateUploadedAndBISIUploadData()
		{
			if (SaveDateUploadedAndBISIUploadDataShouldFail)
			{
				throw new Exception("SaveFailsException");
			}

			if (ThrowDeveloperException)
			{
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Concurrency Issue"), null, null), new BusinessObjectFactoryProvider().Current);
			}

			SaveDateUploadedAndBISIUploadDataCalled = true;
		}

		public BISIExportResult ExportToFile(string targetFileName, ExportInformation exportInformation)
		{
			BISIExportResult result = ExpectedExportResult;
			if (result == BISIExportResult.ExportSuccess)
			{
				try
				{
					using (StreamWriter writer = new StreamWriter(targetFileName))
					{
						writer.WriteLine("{0},{1:yyyyMMddHHmmss},{2:yyyyMMddHHmmss}", exportInformation.BatchNumber, exportInformation.EveryDayStartDate, exportInformation.EveryDayEndDate);
					}

					LastTargetFileName = targetFileName;
					result = BISIExportResult.ExportSuccess;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = BISIExportResult.ExportFails;
				}
			}

			return result;
		}

		public IReadOnlyList<IShipmentData> LastUploadedCompletedShipments
		{
			get
			{
				ShipmentDataForTest shipmentData1 = new ShipmentDataForTest();
				ShipmentDataForTest shipmentData2 = new ShipmentDataForTest();
				return new IShipmentData[] { shipmentData1, shipmentData2 };
			}
		}

		public BISIExportResult ExpectedExportResult;
		public bool SaveDateUploadedAndBISIUploadDataShouldFail;
		public bool SaveDateUploadedAndBISIUploadDataCalled;
		public string LastTargetFileName;
		public bool ThrowDeveloperException;
	}
}
