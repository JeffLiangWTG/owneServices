using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.ZMGermany
{
	public sealed class ZMFileExporter
	{
		public enum OrganisationStatus
		{
			New = 1,
			Modified = 2
		}

		public class ZMFileExportResult
		{
			public ZMFileExportResult(int newRecords, int updatedRecords, string fileContent, string filename)
			{
				NewRecords = newRecords;
				UpdatedRecords = updatedRecords;
				FileContent = fileContent;
				Filename = filename;
			}

			public readonly int NewRecords;
			public readonly int UpdatedRecords;
			public readonly string FileContent;
			public readonly string Filename;
		}

		readonly ZMGermanyReport ZmReport;
		readonly AccComplianceReport ComplianceReport;
		readonly Dictionary<string, AccTaxReturnLine> HighestAlreadyExportedVersionDictionary;
		readonly int VersionToExport;

		public ZMFileExporter(ZMGermanyReport zmReport, AccComplianceReport complianceReport)
		{
			ZmReport = zmReport;
			ComplianceReport = complianceReport;

			VersionToExport = ZmReport.SelectedVersionNumber;
			if (VersionToExport == 0)
			{
				VersionToExport = zmReport.CurrentTaxReturn?.ATR_Version ?? 0;
				zmReport.SelectedVersion = VersionToExport.ToString();
			}

			HighestAlreadyExportedVersionDictionary = new Dictionary<string, AccTaxReturnLine>();
			foreach (var taxReturnHeader in zmReport.TaxReturnHeaderList.Where(v => v.ATR_Version < VersionToExport).OrderByDescending(v => v.ATR_Version))
			{
				foreach (AccTaxReturnLine taxReturnLine in taxReturnHeader.Lines)
				{
					var key = ZMGermanyReport.GetLineKey(taxReturnLine);
					if (!HighestAlreadyExportedVersionDictionary.ContainsKey(key))
					{
						HighestAlreadyExportedVersionDictionary.Add(key, taxReturnLine);
					}
				}
			}
		}

		public ZMFileExportResult Export()
		{
			var reportData = new ZStringBuilder();
			var reportUpdates = new ZStringBuilder();
			var newRecords = 0;
			var updatedRecords = 0;
			var recordsForSelectedVersion = ZmReport.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == VersionToExport);
			var reportRange = GetReportRange();

			reportData.Append(ProcessCompanyData());
			foreach (var organisationRecord in recordsForSelectedVersion.Lines.Cast<AccTaxReturnLine>())
			{
				switch (CheckOrganisationStatus(organisationRecord))
				{
					case OrganisationStatus.New:
						newRecords++;
						reportData.Append(ProcessOrganisationData(organisationRecord, ComplianceReport.Company.GC_BusinessRegNo, false, reportRange));
						break;
					case OrganisationStatus.Modified:
						updatedRecords++;
						reportUpdates.Append(ProcessOrganisationData(organisationRecord, ComplianceReport.Company.GC_BusinessRegNo, true, reportRange));
						break;
					default:
						throw new NotSupportedException();
				}
			}
			reportData.Append(reportUpdates);
			reportData.Append(CreateSummary(recordsForSelectedVersion.Lines.Cast<AccTaxReturnLine>().ToList().AsReadOnly(), ComplianceReport.Company.GC_BusinessRegNo, reportRange));

			return new ZMFileExportResult(newRecords, updatedRecords, reportData.ToString(), GenerateFilename(ZmReport.SenderID));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1110:DoNotUseColumnNamesDirectly", Justification = "This is a filename not a column name")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filename")]
		internal string GenerateFilename(string senderId)
		{
			var processId = VersionToExport;
			var supplierId = ComplianceReport.Company.GC_Code;
			var processInterval = CalculateProcessInterval();
			var dateId = processInterval.Substring(0, 1) == "j" ? ComplianceReport.ACR_DateFrom.ToString("yyyy1231") : ComplianceReport.ACR_DateFrom.ToString("yyyyMMdd");
			return $"m5_zm_{senderId}_{processId:D3}{supplierId}_v01_z{dateId}_{processInterval}_ca.mgp";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "prefixes for monthly indicator, quarterly indicator, yearly indicator")]
		internal string CalculateProcessInterval()
		{
			var periodCalculator = new AccountingPeriodCalculator(ComplianceReport.Factory);
			var firstPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime());
			var lastPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateTo.ToDateTime());
			var numberOfPeriods = lastPeriod - firstPeriod + 1;

			var prefix = "m";
			if (numberOfPeriods == 3)
			{
				prefix = "q";
			}
			if (numberOfPeriods == 12)
			{
				prefix = "j";
			}

			var processStartDate = ComplianceReport.ACR_DateTo.AddDays(1);
			while (processStartDate.DayOfWeek != DayOfWeek.Monday)
			{
				processStartDate = processStartDate.AddDays(1);
			}

			return $"{prefix}{processStartDate:yy}{processStartDate.DayOfYear:D3}";
		}

		internal string GetReportRange()
		{
			var periodCalculator = new AccountingPeriodCalculator(ComplianceReport.Factory);
			var firstPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime());
			var lastPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateTo.ToDateTime());
			var periodCount = lastPeriod - firstPeriod + 1;
			var monthOfFirstPeriod = firstPeriod % 100;

			string monthIndicator;
			switch (periodCount)
			{
				case 1:
					monthIndicator = (monthOfFirstPeriod + 20).ToString();
					break;
				case 3:
					monthIndicator = ((monthOfFirstPeriod - 1) / 3 + 1).ToString("D2");
					break;
				case 12:
					monthIndicator = "05";
					break;
				default:
					return "";
			}

			return $"{monthIndicator}{firstPeriod.ToString().Substring(2, 2)}";
		}

		internal string ProcessCompanyData()
		{
			var record = new StringBuilder(new string(' ', 120));
			var orgProxy = ComplianceReport.Company.OrgProxy ??
						   throw new InvalidOperationException($"No organization proxy is found for the company {ComplianceReport.Company.HumanReadableShortcutName}");
			var reportAddress = AddressHelper.GetReportAddress(orgProxy) ?? throw new InvalidOperationException(
				"Please define a single office address as main address for the organizational proxy of the reporting company. This address will be used for the compliance report.");

			InsertData(record, 1, 1, "0");
			InsertData(record, 2, 6, ZmReport.RegistrationID);
			InsertData(record, 8, 8, ZDate.Today.ToString("yyyyMMdd"));
			InsertData(record, 16, 45, orgProxy.OH_FullName);
			InsertData(record, 61, 25, $"{reportAddress.Address1} {reportAddress.Address2}");
			InsertData(record, 86, 5, reportAddress.Postcode);
			InsertData(record, 91, 25, reportAddress.CityFallback);

			return record.ToString();
		}

		internal OrganisationStatus CheckOrganisationStatus(AccTaxReturnLine organisationData)
		{
			return HighestAlreadyExportedVersionDictionary.ContainsKey(ZMGermanyReport.GetLineKey(organisationData)) ? OrganisationStatus.Modified : OrganisationStatus.New;
		}

		internal string ProcessOrganisationData(AccTaxReturnLine organisationData, string sendersBusinessRegNo, bool isCorrection, string reportRange)
		{
			var record = new StringBuilder(new string(' ', 120));
			InsertData(record, 1, 1, "1");
			InsertData(record, 2, 11, CreateUSTIDNumber(sendersBusinessRegNo, "DE"));
			InsertData(record, 13, 2, isCorrection ? "11" : "10");
			InsertData(record, 15, 4, reportRange);
			InsertData(record, 19, 14, CreateUSTIDNumber(organisationData.ARL_OrgRegNo, organisationData.ARL_RN_NKCountryCode));
			InsertData(record, 33, 12, Math.Floor(organisationData.ARL_TotalAmountIncludingTax).ToString("000000000000;00000000000-;000000000000"));
			InsertData(record, 45, 1, "S");
			InsertData(record, 46, 2, "10");
			InsertData(record, 48, 2, "10");
			return record.ToString();
		}

		internal string CreateUSTIDNumber(string registrationNumber, string countryCode)
		{
			var prefix = registrationNumber.Substring(0, 2).ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			return prefix == countryCode ? countryCode + registrationNumber.Substring(2) : countryCode + registrationNumber;
		}

		internal string CreateSummary(IReadOnlyCollection<AccTaxReturnLine> includedRecords, string sendersBusinessRegNo, string reportRange)
		{
			var record = new StringBuilder(new string(' ', 120));
			InsertData(record, 1, 1, "2");
			InsertData(record, 2, 11, CreateUSTIDNumber(sendersBusinessRegNo, "DE"));
			InsertData(record, 13, 4, reportRange);
			var totalAmount = includedRecords.Sum(v => Math.Floor(v.ARL_TotalAmountIncludingTax));
			InsertData(record, 17, 14, totalAmount.ToString("00000000000000;0000000000000-;00000000000000"));
			var recordCount = includedRecords.Count;
			InsertData(record, 31, 5, recordCount.ToString("D5"));
			return record.ToString();
		}

		void InsertData(StringBuilder record, int startOneBased, int length, string dataToInsert)
		{
			if (startOneBased < 1 || startOneBased > record.Length)
			{
				throw new IndexOutOfRangeException();
			}

			dataToInsert = dataToInsert.PadRight(length, ' ').Substring(0, length);
			record.Remove(startOneBased - 1, length);
			record.Insert(startOneBased - 1, dataToInsert);
		}
	}
}
