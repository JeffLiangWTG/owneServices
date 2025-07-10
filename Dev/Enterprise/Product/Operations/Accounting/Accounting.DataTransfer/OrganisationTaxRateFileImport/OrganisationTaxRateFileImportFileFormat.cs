using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	class OrganisationTaxRateFileImportFileFormat : DelimitedFlatFileFormat
	{
		internal OrganisationTaxRateFileImportFileFormat(IOrgTaxRateImportFileFormat fileFormat, Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> regNumbersToImport)
		{
			LocalFormatRecord = fileFormat;
			RegNumbersToImport = RemoveNonNumericCharactersFromImportDictionary(regNumbersToImport);
		}

		IOrgTaxRateImportFileFormat LocalFormatRecord { get; }
		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> RegNumbersToImport { get; }

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			var readedRowFormat = new OCsvLine(rawRow, Delimiter);

			if (readedRowFormat.FieldValues.Length > 0 && readedRowFormat.FieldValues.Length == LocalFormatRecord?.NumberOfColumnsInFormat)
			{
				string regNumber = readedRowFormat.FieldValues[LocalFormatRecord.RegistrationCode];

				if (RegNumbersToImport.TryGetValue(regNumber, out var regNumberValue))
				{
					string perceptionRate = readedRowFormat.FieldValues[LocalFormatRecord.PerceptionRate];
					string startDate = readedRowFormat.FieldValues[LocalFormatRecord.StartDate];
					string endDate = readedRowFormat.FieldValues[LocalFormatRecord.EndDate];

					var result = new OrganisationTaxRateFileImportDataRow();

					result.RegistrationCode = regNumber;
					result.OrganisationDataCollection = regNumberValue;

					result.SetField(OrganisationTaxRateFileImportDataRow.Schema.StartDate, startDate);
					result.SetField(OrganisationTaxRateFileImportDataRow.Schema.EndDate, endDate);
					result.SetField(OrganisationTaxRateFileImportDataRow.Schema.Rate, perceptionRate);

					return result;
				}
			}

			return null;
		}

		public override FileExtensionType FileExtensionForExport => FileExtensionType.ClientSpecific;

		public override FileExtensionType FileExtensionForImport => FileExtensionType.ClientSpecific;

		protected override char Delimiter => LocalFormatRecord?.Delimiter ?? ';';

		public override ZString ConvertToLine(FlatFileDataRow row) => throw new NotSupportedException("Export to Organisation Tax Rate File Import file format is not supported");

		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> RemoveNonNumericCharactersFromImportDictionary(Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> regNumbers)
		{
			var regNumbersToImport = new Dictionary<ZString, List<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>>();

			if (regNumbers != null)
			{
				foreach (KeyValuePair<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> regNumber in regNumbers)
				{
					var regNumberKey = Regex.Replace(regNumber.Key, @"[^0-9]", string.Empty);

					if (!regNumbersToImport.ContainsKey(regNumberKey))
					{
						regNumbersToImport.Add(regNumberKey, regNumber.Value.ToList());
					}
					else
					{
						regNumbersToImport[regNumberKey].AddRange(regNumber.Value.ToList());
					}
				}
			}

			return regNumbersToImport.ToDictionary(x => x.Key, x => (IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>)x.Value);
		}
	}
}
