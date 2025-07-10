using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ClassificationDataLoad : DataLoad
	{
		public void ImportClassificationData(string dataLocation)
		{
			ImportData(dataLocation, "Classification");
		}

		#region FileValidation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 7
				&& line.FieldValues[0].ToUpperInvariant() == CSVCode
				&& line.FieldValues[1].ToUpperInvariant() == CSVType
				&& line.FieldValues[2].ToUpperInvariant() == CSVDescription
				&& line.FieldValues[3].ToUpperInvariant() == CSVTariff
				&& line.FieldValues[4].ToUpperInvariant() == CSVTreatment
				&& line.FieldValues[5].ToUpperInvariant() == CSVInstrument
				&& line.FieldValues[6].ToUpperInvariant() == CSVConcession;
		}

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return CSVCode;
				yield return CSVType;
				yield return CSVDescription;
				yield return CSVTariff;
				yield return CSVTreatment;
				yield return CSVInstrument;
				yield return CSVConcession;
			}
		}

		const string CSVCode = "CODE";
		const string CSVType = "TYPE";
		const string CSVDescription = "DESCRIPTION";
		const string CSVTariff = "TARIFF";
		const string CSVTreatment = "TREATMENT";
		const string CSVInstrument = "INSTRUMENT";
		const string CSVConcession = "CONCESSION";

		#endregion

		#region Import From .csv file

		protected internal void ProcessDataForThisLineInternal(OCsvLine line) => ProcessDataForThisLine(line);
		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			OCsvLine revisedLine = line;

			if (line.FieldValues.Length < 7 && line.FieldValues.Length >= 4)
			{
				string[] fieldValues = line.FieldValues;
				bool[] includeQuotes = line.IncludeQuotes;
				Array.Resize(ref fieldValues, 7);
				Array.Resize(ref includeQuotes, 7);
				revisedLine = new OCsvLine(fieldValues, includeQuotes);
			}
			Guid transactionPK = Guid.Empty;
			if (ValidLine(revisedLine))
			{
				ExtractExcelClassificationData(revisedLine);
				transactionPK = CreateClassification();
			}
			else
			{
				DisplayExcludedRecordMessage(" excluded... data is inconsistent with required format.");
			}

			UpdateAndDisplayIfRequired(transactionPK, CusClassificationSchema.Constants.TableName);
		}

		bool ValidLine(OCsvLine line)
		{
			return line.FieldValues.Length == 7
				&& line.FieldValues[0].Length <= 35
				&& (line.FieldValues[1] == Classification.ClassificationType.IMP || line.FieldValues[1] == Classification.ClassificationType.EXP);
		}

		void ExtractExcelClassificationData(OCsvLine line)
		{
			lookupCode = new ZString(line.FieldValues[0]).Trim();
			classType = new ZString(line.FieldValues[1]).Trim();
			description = new ZString(line.FieldValues[2]).Trim().SubstringSafe(0, 80);
			tariff = new ZString(line.FieldValues[3]).Trim().SubstringSafe(0, 20);
			treatment = new ZString(line.FieldValues[4]).Trim().SubstringSafe(0, 3);
			instrument = new ZString(line.FieldValues[5]).Trim().SubstringSafe(0, 3);
			concession = new ZString(line.FieldValues[6]).Trim().SubstringSafe(0, 8);
			if (description.IsEmpty)
			{
				description = GetTariffDesc(tariff);
			}
		}

		ZString GetTariffDesc(ZString tariffCode)
		{
			var result = classType == Classification.ClassificationType.EXP ? GetExportTariffDesc(tariffCode) : GetImportTariffDesc(tariffCode);
			return result.Left(80);
		}

		ZString GetImportTariffDesc(ZString tariffCode)
		{
			var importTariff = AUCClassWrapper.LoadPartialCode(Factory, tariffCode, ZDateTime.Today);
			return importTariff?.ZZ1_Description ?? ZString.Empty;
		}

		ZString GetExportTariffDesc(ZString tariff)
		{
			ZString result = GetExportTariffDescription(tariff);
			if (result == "Other")
			{
				tariff = tariff.Left(4);
				result = GetExportTariffDescription(tariff);
			}
			return result;
		}

		ZString GetExportTariffDescription(ZString tariffCode)
		{
			var exportTariff = AUCAHECCWrapper.Load(Factory, tariffCode, ZDateTime.Today);
			return exportTariff?.ZZ1_Description ?? ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		Guid CreateClassification()
		{
			Guid transactionPK = Guid.Empty;
			Classification enterpriseClass = LoadClassificationIfItExists();

			if (enterpriseClass == null)
			{
				Classification newEnterpriseClass = Factory.New<Classification>();
				LoadImportedClassificationValues(newEnterpriseClass);
				newEnterpriseClass.Validation.ValidateAll();
				if (newEnterpriseClass.HasErrors)
				{
					DisplayExcludedRecordMessage(": Record Excluded - Tariff values provided are not valid in CargoWise One");
					newEnterpriseClass.Delete();
				}
				else
				{
					transactionPK = newEnterpriseClass.PK.ToGuid();
					RunCounters.RecsCreated++;
					RunCounters.RecsToUpdate++;
				}
			}
			else
			{
				DisplayExcludedRecordMessage(": Record Excluded - Classification '" + lookupCode + "' already exists in CargoWise One");
			}

			return transactionPK;
		}

		void LoadImportedClassificationValues(Classification enterpriseClassification)
		{
			enterpriseClassification.CC_LookupCode = lookupCode;
			enterpriseClassification.CC_Description = description;
			enterpriseClassification.CC_ClassificationType = classType;
			enterpriseClassification.CC_TariffNum = tariff;
			enterpriseClassification.CC_AddInfo = ConstructAddInfo();
			enterpriseClassification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		ZString ConstructAddInfo()
		{
			ZString addInfo = ZString.Empty;
			if (!concession.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "InstrumentCode_Hidden=" + concession);
			}

			if (!instrument.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "InstrumentType_Hidden=" + instrument);
			}

			if (!treatment.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "TreatmentCode_Hidden=" + treatment);
			}

			return addInfo;
		}

		string AppendAddInfo(string addInfo, string newAddInfo)
		{
			return (string.IsNullOrEmpty(addInfo)) ? newAddInfo : addInfo += "*" + newAddInfo;
		}

		Classification LoadClassificationIfItExists()
		{
			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return Factory.LoadTop1<Classification>(classFilter);
		}

		#endregion

		#region ClassificationTable

		protected ZString lookupCode;
		protected ZString classType;
		protected ZString description;
		protected ZString tariff;
		protected ZString treatment;
		protected ZString instrument;
		protected ZString concession;

		#endregion
	}
}
