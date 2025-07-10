using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters.CustomsFiles
{
	#region ImportValidationException
	[Serializable]
	public class ImportValidationException : Exception
	{
		public ImportValidationException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected ImportValidationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
#endregion

	public class Converter : CustomsDataImporter
	{
		public Converter(string dataLocation, OleDBConnectionTypes dataType, bool classification, bool excludeRecs, bool limitedClass) : base(dataLocation, classification, limitedClass)
		{
			this.FileName = dataLocation;

			Excel = (dataType == OleDBConnectionTypes.Excel);

			ExcludeExistingRecords = excludeRecs;

			Log = new StringCollection();
		}

		public void Stop()
		{
			IsStop = true;
		}

		public void ImportData()
		{
			if (Excel)
			{
				GetImportedRecsFromExcel();
			}
			else
			{
				throw new ArgumentException("Invalid import option");
			}
		}

		public event EventHandler OnProgress;
		public event EventHandler OnError;

		public readonly StringCollection Log;

		#region Implementation

		protected readonly bool Excel;
		protected readonly bool ExcludeExistingRecords;

		protected bool IsStop;
		protected string FileName;

		protected FileStream Schema;
		protected string CurrentLine;

		#region ImportFromSpreadsheet

		protected void GetImportedRecsFromExcel()
		{
			if (UpdateClassifications)
			{
				ProcessExcelClassificationCreation();
			}
			else
			{
				ProcessExcelProductCreation();
			}
		}

		protected void ProcessExcelClassificationCreation()
		{
			InitialiseCounters();
			Factory = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			RecordsToImport = CountExcelRows();

			using (StreamReader sr = new StreamReader(FileName))
			{
				while ((CurrentLine = sr.ReadLine()) != null)
				{
					CurrentRow++;
					var line = new OCsvLine(CurrentLine);

					if (CurrentRow == 1)
					{
						if (!IsRecsToImportFileValid(line))
						{
							throw new ImportValidationException("File Header information is incorrect. The import of classification and product data requires a specific .CSV format: \nPlease contact CargoWise for the correct format");
						}
					}

					if (CurrentRow == 1 && line.FieldValues[0].ToUpper().StartsWith("LOOKUP"))
					{
						//exclude header row if present
						RecsExcluded++;
						Log.Add("File header row excluded... ");
					}
					else
					{
						ExtractExcelClassificationData(line);
						LoadEnterpriseClassification();
					}
				}
			}

			OutputFinalTotals(RecordsToImport, RecsCreated, RecsUpdated, RecsExcluded);
		}

		protected void ProcessExcelProductCreation()
		{
			InitialiseCounters();
			Factory = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			RecordsToImport = CountExcelRows();

			using (StreamReader sr = new StreamReader(FileName))
			{
				while ((CurrentLine = sr.ReadLine()) != null)
				{
					CurrentRow++;
					var line = new OCsvLine(CurrentLine);

					if (CurrentRow == 1)
					{
						if (!IsRecsToImportFileValid(line))
						{
							throw new ImportValidationException("File Header information is incorrect. The import of classification and product data requires a specific .CSV format: \nPlease contact CargoWise for the correct format");
						}
					}

					if (CurrentRow == 1 && line.FieldValues[0].ToUpper().StartsWith("PART"))
					{
						//exclude header row if present
						RecsExcluded++;
						Log.Add("File header row excluded... ");
					}
					else
					{
						ExtractExcelPartData(line);
						LoadEnterpriseProduct();
					}
				}
			}

			OutputFinalPartTotals(RecordsToImport, RecsCreated, RecsUpdated, RecsExcluded);
		}

		protected int CountExcelRows()
		{
			int recordCount = 0;
			using (StreamReader sr = new StreamReader(FileName))
			{
				while ((CurrentLine = sr.ReadLine()) != null)
				{
					recordCount++;
				}
			}

			return recordCount;
		}

		#endregion

		#region LoadValues

		protected void LoadEnterpriseClassification()
		{
			Guid transactionPK = Guid.Empty;
			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, ClassCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, ClassType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Classification enterpriseClass = Factory.LoadTop1<Classification>(classFilter);
			if (enterpriseClass == null)
			{
				try
				{
					Classification newEnterpriseClass = Factory.New<Classification>();
					LoadImportedClassificationValues(newEnterpriseClass);
					transactionPK = newEnterpriseClass.PK.ToGuid();
					RecsToUpdate++;
					RecsCreated++;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RecsExcluded++;
					DisplayFormatLogMessage(ex.Message);
				}
			}
			else
			{
				if (ExcludeExistingRecords)
				{
					RecsExcluded++;
					Log.Add("Classification excluded: " + ClassCode + " / " + Description + " - already exists in a " + BrandingFactory.Instance.ProductName + " table");
					Factory.ClearQueryCache();
				}
				else // update existing classification
				{
					try
					{
						LoadImportedClassificationValues(enterpriseClass);
						transactionPK = enterpriseClass.PK.ToGuid();
						RecsToUpdate++;
						RecsUpdated++;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						DisplayFormatLogMessage(ex.Message);
					}
				}
			}

			string dataImportType = "Customs classifications";
			if (ProcessingExportClass)
			{
				dataImportType = "Export customs classifications";
			}

			UpdateAndDisplayIfRequired(dataImportType);
		}

		protected void LoadEnterpriseProduct()
		{
			AUOrgSupplierPart enterpriseProduct = GetPartIfItExists(PartNo, PartOwnerCodePK, PartSupplierCodePK);

			if (enterpriseProduct == null)
			{
				OutputToLogIfError(CreateAndLoadEnterprisePart());
			}
			else
			{
				string updateMsg = UpdateExistingPartWithExportLookupIfRequired(enterpriseProduct);
				if (!string.IsNullOrEmpty(updateMsg))
				{
					Log.Add(updateMsg);
				}
				else
				{
					if (ExcludeExistingRecords)
					{
						Log.Add(PartExcludedAsAlreadyInEnterpriseOrLookupNotFound(false));
					}
					else // update existing product
					{
						try
						{
							LoadImportedPartValues(enterpriseProduct);
							TransactionPK = enterpriseProduct.PK.ToGuid();
							RecsToUpdate++;
							RecsUpdated++;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							DisplayFormatLogMessage(ex.Message);
						}
					}
				}
			}

			UpdateAndDisplayIfRequired("Products");
		}

		protected override void LoadCusClassPartPivot(OrgSupplierPart enterprisePart, ZGuid cusClassPK, ZString addInfo, ZString trtCode)
		{
			ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, cusClassPK);
			cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
			CusClassPartPivot cusClassPartLink = Factory.LoadTop1<CusClassPartPivot>(cusClassPartFilter);
			if (cusClassPartLink == null)
			{
				CusClassPartPivot newCusClassPartLink = Factory.New<CusClassPartPivot>();
				newCusClassPartLink.CI_CC = cusClassPK;
				newCusClassPartLink.CI_OP = enterprisePart.PK;
				newCusClassPartLink.CI_AddInfo = addInfo;
				if (!trtCode.IsEmpty)
				{
					newCusClassPartLink.AddInfo.ZA_TreatmentCode_Hidden = trtCode.Left(newCusClassPartLink.AddInfo.ZA_TreatmentCode_HiddenInfo.MaxLength);
				}
			}
			else
			{
				cusClassPartLink.CI_AddInfo = addInfo;
				if (!trtCode.IsEmpty)
				{
					cusClassPartLink.AddInfo.ZA_TreatmentCode_Hidden = trtCode.Left(cusClassPartLink.AddInfo.ZA_TreatmentCode_HiddenInfo.MaxLength);
				}
			}
		}

		#endregion

		#region ExtractData

		protected void ExtractRequiredBKPClassificationFields(DataRow classRecord)
		{
			ClassCode = new ZString(classRecord[Constants.BKPDataFields.Tlcode]).Trim();
			string tariffStat = new ZString(classRecord[Constants.BKPDataFields.Tlstat]).Trim();
			if (tariffStat.Length == 4)
			{
				tariffStat = tariffStat.Substring(2, 2);
			}

			Tariff = new ZString(classRecord[Constants.BKPDataFields.Tlitem]).Trim() + tariffStat;
			Description = new ZString(classRecord[Constants.BKPDataFields.Tldesc]).Trim() + new ZString(classRecord[Constants.BKPDataFields.Tldsc2]).Trim();
			AddInfo = new ZString(classRecord[Constants.BKPDataFields.Tladdi]).Trim();
			Pref = new ZString(classRecord[Constants.BKPDataFields.Tlpref]).Trim();
			Origin = new ZString(classRecord[Constants.BKPDataFields.Tlorgn]).Trim();
			InstrumentCode = new ZString(classRecord[Constants.BKPDataFields.Tlinsno]).Trim();
			InstrumentType = new ZString(classRecord[Constants.BKPDataFields.Tlitype]).Trim();
			TreatmentCode = "";
			if (classRecord[Constants.BKPDataFields.Tltrmt].ToString().Trim() != "000")
			{
				TreatmentCode = new ZString(classRecord[Constants.BKPDataFields.Tltrmt]).Trim();
			}

			ClassType = "IMP";
		}

		protected void ExtractRequiredPartFields(OleDbDataReader reader)
		{
			if (ProcessingExportParts)
			{
				ExtractExportPartFields(reader);
			}
			else
			{
				ExtractRequiredDeliverancePartFields(reader);
			}
		}

		protected void GetOwnerAndSupplierPKs()
		{
			PartOwnerCodePK = ZGuid.Empty;
			PartSupplierCodePK = ZGuid.Empty;
			if (!PartOwnerCode.IsEmpty)
			{
				PartOwnerCodePK = GetOwnerSupplierCodePK(PartOwnerCode);
			}

			if (!PartSupplierCode.IsEmpty)
			{
				PartSupplierCodePK = GetOwnerSupplierCodePK(PartSupplierCode);
			}
		}

		protected void ExtractRequiredBKPPartFields(DataRow partRecord)
		{
			PartNo = new ZString(partRecord[Constants.BKPDataFields.cpitem]).Trim().SubstringSafe(0, 30);
			PartDesc = new ZString(partRecord[Constants.BKPDataFields.cpdescr1]).Trim() + " " + new ZString(partRecord[Constants.BKPDataFields.cpdescr2]).Trim();
			if (PartDesc.Trim().IsEmpty)
			{
				PartDesc = PartNo;
			}

			PartDesc = PartDesc.SubstringSafe(0, 80);
			PartClass = new ZString(partRecord[Constants.BKPDataFields.cptlfcode]).Trim();
			PartOwnerCode = new ZString(partRecord[Constants.BKPDataFields.cpcompfk]).Trim();
			PartSupplierCode = "";
			PartCount = 0;
			PartUQ = new ZString(partRecord[Constants.BKPDataFields.cpuqitm]).Trim();
			PartLastCostAmount = (decimal)partRecord[Constants.BKPDataFields.cplastpr];
			GetOwnerAndSupplierPKs();
		}

		protected void ExtractExcelClassificationData(OCsvLine line)
		{
			ClassCode = new ZString(line.FieldValues[0]).Trim().SubstringSafe(0, 35);
			ClassType = new ZString(line.FieldValues[1]).Trim().SubstringSafe(0, 3);
			Description = new ZString(line.FieldValues[2]).Trim().SubstringSafe(0, 80);
			Tariff = new ZString(line.FieldValues[3]).Trim().SubstringSafe(0, 15);
			if (UsesTariffStat)
			{
				if (line.FieldValues.Length > 4)
				{
					if (line.FieldValues[4].Trim().Length > 0)
					{
						TariffStat = new ZString(line.FieldValues[4]).Trim().SubstringSafe(0, 2).PadLeft(2, '0');
						Tariff = Tariff + TariffStat;
					}
					if (line.FieldValues.Length > 5)
					{
						TreatmentCode = new ZString(line.FieldValues[5]).Trim().SubstringSafe(0, 3);
						if (line.FieldValues.Length > 6)
						{
							InstrumentType = new ZString(line.FieldValues[6]).Trim();

							if (line.FieldValues.Length > 7)
							{
								InstrumentCode = new ZString(line.FieldValues[7]).Trim();
							}
						}
					}
				}
			}
			else
			{
				if (line.FieldValues.Length > 4)
				{
					TreatmentCode = new ZString(line.FieldValues[4]).Trim().SubstringSafe(0, 3);

					if (line.FieldValues.Length > 5)
					{
						InstrumentType = new ZString(line.FieldValues[5]).Trim();

						if (line.FieldValues.Length > 6)
						{
							InstrumentCode = new ZString(line.FieldValues[6]).Trim();
						}
					}
				}
			}

			if (Description.IsEmpty)
			{
				Description = GetTariffDesc(Tariff);
			}
			AddInfo = "";
		}

		protected void ExtractExcelPartData(OCsvLine line)
		{
			PartNo = new ZString(line.FieldValues[0]).Trim().SubstringSafe(0, 30);
			PartDesc = new ZString(line.FieldValues[1]).Trim();
			if (PartDesc.Length > 80)
			{
				PartFullDesc = PartDesc;
				PartDesc = PartDesc.SubstringSafe(0, 80);
			}
			else
			{
				PartFullDesc = "";
			}

			if (PartDesc == "")
			{
				PartDesc = PartNo;
			}

			PartUQ = new ZString(line.FieldValues[2]).Trim().SubstringSafe(0, 3);
			ExportPartClass = new ZString(line.FieldValues[3]).Trim();
			PartClass = new ZString(line.FieldValues[4]).Trim();
			PartOwnerCode = new ZString(line.FieldValues[5]).Trim();
			PartSupplierCode = new ZString(line.FieldValues[6]).Trim();
			PartDivision = new ZString(line.FieldValues[7]).Trim();
			PartCount = 0;
			if (line.FieldValues[8].Trim() is var trimmed && trimmed.Length > 0 && trimmed != "0")
			{
				string stockCountValue = new ZString(line.FieldValues[8]).Trim().SubstringSafe(0, 10);
				PartCount = Convert.ToDecimal(stockCountValue, CultureInfo.InvariantCulture);
			}

			if (PartUQ.IsEmpty)
			{
				if (!PartClass.IsEmpty)
				{
					PartUQ = GetUQFromTariff(PartClass, "IMP");
				}
			}

			PartLastCostAmount = 0;
			GetOwnerAndSupplierPKs();
		}

		protected void ExtractCCParts(OCsvLine line)
		{
			ValidPartNo = new ZString(line.FieldValues[0]).Trim().SubstringSafe(0, 30);
			PartNo = new ZString(line.FieldValues[1]).Trim().SubstringSafe(0, 30);
		}

		#endregion

		#region Utilities

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "Calling only every 30000 records")]
		void UpdateAndDisplayIfRequired(string importType)
		{
			if (RecsToUpdate > 0)
			{
				try
				{
					SaveChanges();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					DisplayFormatLogMessage(ex.Message);
				}

				RecsToUpdate = 0;
				DisplayCount++;
				Factory = new BusinessObjectFactory();
				Factory.RefreshEnabled = false;
			}

			if (DisplayCount > 4999)
			{
				DisplayCount = 0;
				Log.Add(importType + " created = " + RecsCreated + ", " + importType + " updated = " + RecsUpdated);
			}

			if (ExcludeRecordCount > 499)
			{
				ExcludeRecordCount = 0;
				Factory = new BusinessObjectFactory();
				Factory.RefreshEnabled = false;
			}

			if (BulkRecordsToImportCount > 30000)
			{
				Log.Add("Reclaiming memory... Please wait.");
				GC.Collect(); // Calling only every 30000 records
				BulkRecordsToImportCount = 0;
			}

			UpdateDisplay(RecordsToImport, RecsCreated, RecsUpdated, RecsExcluded);
		}

		void OutputFinalTotals(int importing, int created, int updated, int excluded)
		{
			if (ProcessingExportClass)
			{
				Log.Add(System.Environment.NewLine + "F I N A L   T O T A L S : Export Classifications created = " + created + ", Classifications updated = " + updated + ", Export Classifications excluded = " + excluded + System.Environment.NewLine);
			}
			else
			{
				Log.Add(System.Environment.NewLine + "F I N A L   T O T A L S : Classifications created = " + created + ", Classifications updated = " + updated + ", Classifications excluded = " + excluded + System.Environment.NewLine);
			}

			UpdateDisplay(importing, created, updated, excluded);
		}

		void OutputFinalPartTotals(int importing, int created, int updated, int excluded)
		{
			Log.Add(System.Environment.NewLine + "F I N A L   T O T A L S : Products created = " + created + ", Products updated = " + updated + ", Products excluded = " + excluded + System.Environment.NewLine);
			UpdateDisplay(importing, created, updated, excluded);
		}

		void UpdateDisplay(int importing, int created, int updated, int excluded)
		{
			if (OnProgress != null)
			{
				OnProgress(new ConverterData(importing, CurrentRow, excluded, updated + created, Log), EventArgs.Empty);
			}
		}

		protected override void DisplayFormatLogMessage(string detailedExceptionMessage)
		{
			int rowNumber = CurrentRow;
			if (Excel)
			{
				rowNumber++;
			}

			string rowData = "Line " + rowNumber.ToString() + ": ";

			if (Excel)
			{
				var line = new OCsvLine(CurrentLine);
				for (int i = 0; i < 3; i++)
				{
					rowData += " "  + line.FieldValues[i];
				}
			}

			string logMessage = rowData + "  " + PartNo + " / " + PartDesc + "  " + detailedExceptionMessage;
			if (UpdateClassifications)
			{
				logMessage = rowData + "  " + ClassCode + " / " + Description + "  " + detailedExceptionMessage;
			}

			Log.Add(logMessage);

			if (OnError != null)
			{
				OnError(logMessage, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Search for organisation will first try and match to a Deliverance Code if loading from Deliverance
		/// Search will then try and match to a Legacy Code.
		/// Finally, if not found via these searches, it will search using the code directly as the OrgCode
		/// </summary>
		/// <param name="OwnerSupplierForeignLookupCode"></param>
		/// <returns></returns>
		protected override ZGuid GetOwnerSupplierCodePK(string ownerSupplierLegacyCode)
		{
			var ownerSupplierPK = ZGuid.Empty;

			OrgHeader enterpriseOrg = OrgHeader.FindByAccountID(Factory, ownerSupplierLegacyCode);
			if (enterpriseOrg != null)
			{
				ownerSupplierPK = enterpriseOrg.PK;
			}
			else
			{
				enterpriseOrg = FindOrganisationByOrgCode(ownerSupplierLegacyCode);
				if (enterpriseOrg != null)
				{
					ownerSupplierPK = enterpriseOrg.PK;
				}
			}

			return ownerSupplierPK;
		}

		OrgHeader FindOrganisationByOrgCode(ZString orgCode)
		{
			ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_Code, orgCode);
			return Factory.LoadTop1<OrgHeader>(orgFilter);
		}

		protected string GetTariffDesc(ZString tariff)
		{
			string tariffDescription = "";
			tariff = tariff.SubstringSafe(0, 4);

			ZQuery tariffFilter = new ZQuery(AUCClassSchema.UJ_Code, tariff);
			AUCClass result = Factory.LoadTop1<AUCClass>(tariffFilter);
			if (result != null)
			{
				tariffDescription = result.ImportDescription.SubstringSafe(0, 80);
			}

			return tariffDescription;
		}

		protected string GetUQFromTariff(ZString classificationLookup, ZString classType)
		{
			string tariffUQ = "";

			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, classificationLookup);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Classification enterpriseClass = Factory.LoadTop1<Classification>(classFilter);
			if (enterpriseClass != null)
			{
				ZQuery tariffFilter = new ZQuery(AUCClassSchema.UJ_Code, enterpriseClass.CC_TariffNum);
				AUCClass result = Factory.LoadTop1<AUCClass>(tariffFilter);
				if (result != null)
				{
					tariffUQ = result.UJ_UQ1.ToUpper();
				}
			}

			return tariffUQ;
		}

		void OutputToLogIfError(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Log.Add(value);
			}
		}

		#endregion

		#region Validation

		protected bool IsRecsToImportFileValid(OCsvLine line)
		{
			if (line.FieldValues.Length == 8)
			{
				UsesTariffStat = true;
			}

			if (UpdateClassifications)
			{
				return line.FieldValues.Length == 7 || line.FieldValues.Length == 8;
			}
			else
			{
				return line.FieldValues.Length == 9 || line.FieldValues.Length == 10 || line.FieldValues.Length == 11;
			}
		}

		#endregion

		#endregion
	}
}
