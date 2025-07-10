using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT
{
	public abstract class VATDataFileWriter : IProgressFormSupportable
	{
		protected VATDataFileWriter(AccComplianceReport report)
		{
			Report = report;
		}

		protected readonly AccComplianceReport Report;

		#region Constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded format string")]
		internal const string DateFormat = "yyyy-MM-dd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded format string")]
		internal const string DateTimeFormat = "yyyy-MM-ddThh:mm:ss";

		#endregion

		#region IProgressFormSupportable

		public string CurrentStatusText { get; private set; }
		public int CompletedItems { get; private set; }
		public int TotaItemsToComplete { get; private set; }

		public string Log => string.Empty;

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		void UpdateProgressStatus(string statusText, int processItem, int totalItemToProcess, bool isProcessCompleted)
		{
			CurrentStatusText = statusText;
			CompletedItems = processItem;
			TotaItemsToComplete = totalItemToProcess;

			RaiseProgressUpdateEvent?.Invoke(this, isProcessCompleted);
		}

		#endregion

		public void WriteDataToStream(Stream stream)
		{
			var status = Res.GetString("744137fe-dce1-4350-ba28-c81df23508ed", "Loading Compliance Report Data...");
			UpdateProgressStatus(status, 1, 3, false);

			using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, true))
			{
				status = Res.GetString("7D682027-E289-43FC-8510-2336DE08A6A2", "Exporting VAT file...");
				UpdateProgressStatus(status, 2, 3, false);

				foreach (var documentData in GetDocumentHeaderDetails())
				{
					writer.WriteLine(BuildDocumentData(documentData));
				}
			}

			status = Res.GetString("906B2A8C-39AB-4CF1-95D1-AAD023B0760D", "VAT File Export Completed.");
			UpdateProgressStatus(status, 3, 3, true);
		}

		internal abstract ComplianceDocumentHeaderDetails[] GetDocumentHeaderDetails();

		internal abstract string BuildDocumentData(ComplianceDocumentHeaderDetails headerDetail);

		protected ZString ProxyVATRegistrationNumber => proxyVATRegistrationNumber ?? (proxyVATRegistrationNumber = ComplianceDocumentHelper.GetCompanyProxyVATNumber());
		string proxyVATRegistrationNumber;

		protected ZString ProxyGTXRegistrationNumber => proxyGTXRegistrationNumber ?? (proxyGTXRegistrationNumber = ComplianceDocumentHelper.GetCompanyProxyGTXNumber());
		string proxyGTXRegistrationNumber;

		protected AccountingPeriodCalculator AccountingPeriodCalculator => accountingPeriodCalculator ?? (accountingPeriodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory()));
		AccountingPeriodCalculator accountingPeriodCalculator;

		protected ZString GetVATRegistrationNumber(bool matchingLedgers, ZString vatRegistrationNum)
		{
			var result = matchingLedgers ? vatRegistrationNum : ProxyVATRegistrationNumber;

			return result.PadLeft(8);
		}

		protected internal ZString GetPeriodYear(ZInt period)
		{
			return ((period / 100) - 1911).ToString(CultureInfo.InvariantCulture);
		}

		protected internal ZString GetPeriodMonth(ZInt period)
		{
			return period.ToString().Substring(4, 2);
		}

		internal virtual ZString GetExTaxAmount(ComplianceDocumentHeaderDetails detail)
		{
			return Math.Abs(detail.ExTaxAmount.Normalize()).ToString(CultureInfo.InvariantCulture).PadLeft(12, '0');
		}
	}
}
