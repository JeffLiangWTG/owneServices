using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT
{
	public class ZeroRatedSalesVATWriter : VATDataFileWriter
	{
		public ZeroRatedSalesVATWriter(AccComplianceReport report) : base(report)
		{
		}

		internal override ComplianceDocumentHeaderDetails[] GetDocumentHeaderDetails()
		{
			var collector = new ComplianceReportDocumentDataCollector(Report);
			return collector.ComplianceDocumentHeader.OrderBy(x => x.DocumentNumber).ToArray();
		}

		internal override string BuildDocumentData(ComplianceDocumentHeaderDetails headerDetail)
		{
			var result = new StringBuilder();
			result.Append(ProxyVATRegistrationNumber);
			result.Append(ProvinceCode);
			result.Append(ProxyGTXRegistrationNumber);
			result.Append(EndPeriod);

			var isZNGComlianceSubType = headerDetail.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG;

			result.Append(isZNGComlianceSubType ? new ZString(' ', 3) : GetPeriodYear(headerDetail.ReportingPeriod));
			result.Append(isZNGComlianceSubType ? new ZString(' ', 2) : GetPeriodMonth(headerDetail.ReportingPeriod));
			result.Append(isZNGComlianceSubType ? new ZString(' ', 10) : headerDetail.DocumentNumber);
			result.Append(GetVATRegistrationNumber(true, headerDetail.VATRegistrationNum));
			result.Append(GetSupportingReason(headerDetail.SupportingReason));
			result.Append(GetCustomRelated(headerDetail.CustomRelated));
			result.Append(GetSupportingDocumentType(headerDetail.SupportingDocumentType));
			result.Append(GetSupportingDocumentNumber(headerDetail.SupportingDocumentNumber));
			result.Append(GetExTaxAmount(headerDetail));
			result.Append(GetFormatDocumentDate(headerDetail.DocumentDate));
			return result.ToString();
		}

		#region StateCode and ProvinceCode Mapping

		readonly string[] StateCodes = new string[] {
			"TPE",
			"TXG",
			"KEE",
			"TNN",
			"KHH",
			"NWT",
			"ILA",
			"TAO",
			"CYI",
			"HSQ",
			"MIA",
			"NAN",
			"HSZ",
			"YUN",
			"CYQ",
			"PIF",
			"HUA",
			"TTT",
			"KIN",
			"PEN",
			"LIE" };

		readonly string[] ProvinceCodes = new string[] {
			"A",
			"B",
			"C",
			"D",
			"E",
			"F",
			"G",
			"H",
			"I",
			"J",
			"K",
			"M",
			"O",
			"P",
			"Q",
			"T",
			"U",
			"V",
			"W",
			"X",
			"Z" };

		#endregion

		string ProvinceCode
		{
			get
			{
				if (string.IsNullOrEmpty(provinceCode))
				{
					var stateCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("VAT", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.PremisesAddress?.StateCode ?? ZString.Empty;
					var index = Array.IndexOf<string>(StateCodes, stateCode);
					provinceCode = index >= 0 ? ProvinceCodes[index] : " ";
				}

				return provinceCode;
			}
		}
		string provinceCode;

		string EndPeriod
		{
			get
			{
				if (string.IsNullOrEmpty(endPeriod))
				{
					var period =  AccountingPeriodCalculator.GetPeriodFromDate(Report.ACR_DateTo);
					endPeriod = GetPeriodYear(period) + GetPeriodMonth(period);
				}

				return endPeriod;
			}
		}
		string endPeriod;

		ZString GetSupportingReason(ZString supportingReason)
		{
			return supportingReason.SubstringSafe(0, 1).PadLeft(1);
		}

		ZString GetCustomRelated(ZBool customRelated)
		{
			return customRelated ? "2" : "1";
		}

		ZString GetSupportingDocumentType(ZString supportingDocumentType)
		{
			return supportingDocumentType.SubstringSafe(0, 2).PadRight(2);
		}

		ZString GetSupportingDocumentNumber(ZString supportingDocumentNumber)
		{
			return supportingDocumentNumber.SubstringSafe(0, 14).PadRight(14);
		}

		ZString GetFormatDocumentDate(ZDateTime documentDate)
		{
			return (documentDate.Year - 1911) + documentDate.Month.ToString("D2", CultureInfo.InvariantCulture) + documentDate.Day.ToString("D2", CultureInfo.InvariantCulture);
		}

#if DEBUG
		public string ProvinceCode_ForTestOnly
		{
			get
			{
				provinceCode = null;
				return ProvinceCode;
			}
		}
#endif
	}
}
