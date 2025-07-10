using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Business;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public class AUSOrgSupplierPartDataLoad : AUOrgSupplierPartDataLoad
	{
		public void ImportProductData(string dataLocation, bool updateParts, ZGuid importerPK, ZGuid supplierPK)
		{
			SavedPartImporterPK = importerPK;
			SavedPartSupplierPK = supplierPK;
			RunTime = ZDateTime.Now;
			ImportData(dataLocation, "Product");
		}

		#region Importer

		public OrgHeader Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = (OrgHeader)Factory.Load(typeof(OrgHeader), SavedPartImporterPK);
				}

				return fImporter;
			}
		}

		OrgHeader fImporter;

		#endregion

		#region Supplier

		public OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = (OrgHeader)Factory.Load(typeof(OrgHeader), SavedPartSupplierPK);
				}

				return fSupplier;
			}
		}

		OrgHeader fSupplier;

		#endregion

		#region Registry Data

		public ClientAUSProductImportRegistry RegistryData
		{
			get
			{
				if (fRegistryData == null)
				{
					ZQuery registryFilter = new ZQuery(ClientAUSProductImportRegistrySchema.T6_OH_Importer, SavedPartImporterPK);
					registryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, SavedPartSupplierPK);
					fRegistryData = (ClientAUSProductImportRegistry)Factory.LoadTop1(typeof(ClientAUSProductImportRegistry), registryFilter);
				}

				return fRegistryData;
			}
		}

		ClientAUSProductImportRegistry fRegistryData;

		#endregion

		#region Implementation

		protected override void ParseHeaderLine(OCsvLine headerLine)
		{
			if (headerLine.FieldValues.Length >= 3)
			{
				headerLine.FieldValues[0] = "Part Number";
				headerLine.FieldValues[1] = "Description";
				headerLine.FieldValues[2] = "Import Classification";
			}
			base.ParseHeaderLine(headerLine);
		}

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			PartsDataToLoad data = GetDataFromLine(line);

			if (data != null)
			{
				ProcessPartData(data);
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage("Row " + RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture) + " excluded... data is inconsistent with the required Austin format.");
				RejectedProduct(data, "Unknown data");
			}

			OnProgressChanged();
		}

		protected override void RunSubsequentDataParsingIfRequired()
		{
			SaveImportedLog(RegistryData.DirectoryForImportedPartsLog);
			SaveRejectionLog(RegistryData.DirectoryForRejectedPartsLog);
		}

		protected override OrgSupplierPart ProcessPartData(MasterFiles.Business.PartsDataToLoad partData)
		{
			OrgSupplierPart product = null;
			PartsDataToLoad partData1 = (PartsDataToLoad)partData;
			try
			{
				if (partData1.HasNoValidOwnerOrSupplier)
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(partData1.PartNo, partData1.PartDesc, "Owner or Supplier not found");
					RejectedProduct(partData1, "Owner or Supplier not found");
				}
				else
				{
					bool invalidClassification = false;
					if (!ClassificationLookupExists(partData1.PartClassification))
					{
						invalidClassification = true;
						partData1.PartDivision = partData1.PartClassification;
						partData1.PartClassification = ZString.Empty;
					}

					product = LoadEnterpriseProduct(partData1);

					if (product == null)
					{
						RejectedProduct(partData1, "Existing Product");
					}
					else
					{
						string note = "";
						if (invalidClassification && partData1.PartDivision != ZString.Empty)
						{
							DisplayFormattedLookupError(partData1.PartNo, partData1.PartDivision);
							note = "Invalid Lookup";
						}

						LoadClientAUSProductInterface(product, invalidClassification, partData1);
						RecordImportedProduct(partData1, note);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (product != null && !product.IsInDatabase)
				{
					product.Delete();
				}
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(partData1.PartNo, partData1.PartDesc, ex.Message);
			}
			return product;
		}

		protected void LoadClientAUSProductInterface(OrgSupplierPart product, bool invalidClassification, PartsDataToLoad partData)
		{
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData dataToLoad = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData();
			dataToLoad.OriginalProductCode = partData.PartNo;
			dataToLoad.OriginalProductDesc = partData.PartDesc;
			string status = "";
			if (invalidClassification)
			{
				dataToLoad.OriginalImportClassification = partData.PartDivision;
				status = "Invalid Classification";
			}
			else
			{
				dataToLoad.OriginalImportClassification = partData.PartClassification;
			}

			dataToLoad.ProductPK = product.PK;
			dataToLoad.ImportDateTime = RunTime;
			dataToLoad.Indicator = 'I';
			dataToLoad.Status = status;

			ClientAUSProductInterfaceMediator mediator = new ClientAUSProductInterfaceMediator(partData.PartOwnerCodePK[0], partData.PartSupplierCodePK[0]);
			mediator.InsertRow(dataToLoad);
		}

		protected PartsDataToLoad GetDataFromLine(OCsvLine line)
		{
			PartsDataToLoad record = new PartsDataToLoad();
			record.PartSupplierCode = new List<ZString>();
			record.PartSupplierCodePK = new List<ZGuid>();
			record.PartOwnerCode = new List<ZString>();
			record.PartOwnerCodePK = new List<ZGuid>();
			if (line.FieldValues.Length > 1)
			{
				try
				{
					record.PartNo = TryGetStringValue(line, "Part Number").Left(30);
					if (line.FieldValues.Length > 1)
					{
						record.PartDesc = TryGetStringValue(line, "Description").Left(30);
					}

					if (record.PartDesc.Length > 80)
					{
						record.PartFullDesc = record.PartDesc;
						record.PartDesc = record.PartDesc.SubstringSafe(0, 80);
					}
					else
					{
						record.PartFullDesc = "";
					}

					if (record.PartDesc == "")
					{
						record.PartDesc = record.PartNo;
					}

					record.PartClassification = TryGetStringValue(line, "Import Classification");

					record.PartCount = 0;
					record.PartOwnerCodePK.Add(SavedPartImporterPK);
					record.PartSupplierCodePK.Add(SavedPartSupplierPK);
					record.PartUQ = "NO";
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("AUSOrgSupplierPartDataLoad.GetDataFromLine", "Exception while extracting data from CsvLine.", ex);
					record = null;
				}
			}
			else
			{
				record = null;
			}

			return record;
		}

		bool ClassificationLookupExists(ZString impLookup)
		{
			bool classOK = true;
			if (!impLookup.IsEmpty)
			{
				if (GetClassificationPK(impLookup, "IMP").IsEmpty)
				{
					classOK = false;
				}
			}

			return classOK;
		}

		void DisplayFormattedLogMessage(ZString partNo, ZString partDesc, string detailedExceptionMessage)
		{
			string ouputRowNo = "Line " + RunCounters.CurrentRow.ToString() + ": ";
			string logMessage = ouputRowNo + "PART NO/DESC: " + partNo + " / " + partDesc + "  " + detailedExceptionMessage;

			DisplayLogMessage(logMessage);
		}

		void DisplayFormattedLookupError(ZString partNo, ZString impLookup)
		{
			string ouputRowNo = "Line " + RunCounters.CurrentRow.ToString() + ": ";
			string logMessage = ouputRowNo + "PART NO: " + partNo + " will be created but Tariff Lookup " + impLookup + " was not found";

			DisplayLogMessage(logMessage);
		}

		#region Validation

		protected override IEnumerable<string> GetFieldNames()
		{
			yield return "Part Number";
			yield return "Description";
			yield return "Import Classification";
		}

		#endregion

		#region RejectionLogging

		#region RejectedProductsFile

		StringBuilder RejectedProductsFile
		{
			get
			{
				if (fRejectedProductsFile == null)
				{
					fRejectedProductsFile = new StringBuilder();
					string headerLine = @"Product Code, Description, Import Classification, Importer, Supplier, Notes";
					fRejectedProductsFile.Append(headerLine);
					fRejectedProductsFile.Append(System.Environment.NewLine);
				}

				return fRejectedProductsFile;
			}
		}

		StringBuilder fRejectedProductsFile;

		#endregion

		void RejectedProduct(PartsDataToLoad partData, string rejectionReason)
		{
			if (RejectedProductsFile != null)
			{
				string @class = partData.PartClassification != ZString.Empty ? partData.PartClassification : partData.PartDivision;
				string rejectionLine = partData.PartNo + "," + partData.PartDesc + "," + @class + "," + Importer.OH_FullNameTruncated + "," + Supplier.OH_FullNameTruncated + "," + rejectionReason;
				RejectedProductsFile.Append(rejectionLine);
				RejectedProductsFile.Append(System.Environment.NewLine);
			}
		}

		void SaveRejectionLog(string directory)
		{
			if (!Globals.IsTest && RejectedProductsFile.Length > HeaderLineLength)
			{
				CreateRejectedLogFile(RejectedProductsFile, directory);
			}
		}

		void CreateRejectedLogFile(StringBuilder logData, string dataDirectory)
		{
			ZDateTime currentDateTime = RunTime;
			ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			string directoryWithTime = Path.Combine(dataDirectory, currentDateString);
			DirectoryInfo directory = GetDirectoryInfo(directoryWithTime);

			string logFileName = "";
			logFileName += "RejectedProducts." + currentTimeString + ".csv";

			string fileName = Path.Combine(directory.FullName, logFileName);
			using (StreamWriter sw = new StreamWriter(fileName, true))
			{
				sw.WriteLine(logData);
				sw.Flush();
			}

			string logFileLocationNotificationMessage = "Rejected products have been output to: " + Path.Combine(directoryWithTime, logFileName);
			DisplayLogMessage(logFileLocationNotificationMessage);
		}

		DirectoryInfo GetDirectoryInfo(string directoryWithTime)
		{
			var directory = new DirectoryInfo(SharedUtil.GetFinalPath(directoryWithTime));
			if (!directory.Exists)
			{
				directory.Create();
			}
			return directory;
		}

#endregion

		#region ImportedLogging

		#region ImportedProductsFile

		StringBuilder ImportedProductsFile
		{
			get
			{
				if (fImportedProductsFile == null)
				{
					fImportedProductsFile = new StringBuilder();
					string headerLine = @"Product Code, Description, Import Classification, Importer, Supplier, Notes";
					fImportedProductsFile.Append(headerLine);
					fImportedProductsFile.Append(System.Environment.NewLine);
				}

				return fImportedProductsFile;
			}
		}

		StringBuilder fImportedProductsFile;

		#endregion

		void RecordImportedProduct(PartsDataToLoad partData, string notes)
		{
			if (ImportedProductsFile != null)
			{
				string @class = partData.PartClassification != ZString.Empty ? partData.PartClassification : partData.PartDivision;
				string importedLine = partData.PartNo + "," + partData.PartDesc + "," + @class + "," + Importer.OH_FullNameTruncated + "," + Supplier.OH_FullNameTruncated + "," + notes;
				ImportedProductsFile.Append(importedLine);
				ImportedProductsFile.Append(System.Environment.NewLine);
			}
		}

		void SaveImportedLog(string directory)
		{
			if (!Globals.IsTest && ImportedProductsFile.Length > HeaderLineLength)
			{
				CreateImportedLogFile(ImportedProductsFile, directory);
			}
		}

		void CreateImportedLogFile(StringBuilder logData, string dataDirectory)
		{
			ZDateTime currentDateTime = RunTime;
			ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			string directoryWithTime = Path.Combine(dataDirectory, currentDateString);
			DirectoryInfo directory = GetDirectoryInfo(directoryWithTime);

			string logFileName = "";
			logFileName += "ImportedProducts." + currentTimeString + ".csv";

			string fileName = Path.Combine(directory.FullName, logFileName);
			using (StreamWriter sw = new StreamWriter(fileName, true))
			{
				sw.WriteLine(logData);
				sw.Flush();
			}

			string logFileLocationNotificationMessage = "Imported products have been output to: " + Path.Combine(directoryWithTime, logFileName);
			DisplayLogMessage(logFileLocationNotificationMessage);
		}

		const int HeaderLineLength = 80;

		#endregion

		ZGuid SavedPartImporterPK;
		ZGuid SavedPartSupplierPK;
		ZDateTime RunTime;

#endregion
	}
}
