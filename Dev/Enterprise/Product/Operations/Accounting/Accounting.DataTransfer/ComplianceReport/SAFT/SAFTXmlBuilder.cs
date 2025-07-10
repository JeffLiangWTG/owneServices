using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	public class SAFTXmlBuilder : ComplianceReportXmlBuilder, IProgressFormSupportable
	{
		public SAFTXmlBuilder(ReportModeAndCreditorSelector reportModeAndCreditorSelector, AccComplianceReport report)
		{
			Argument.NotNull(report, nameof(report));
			Report = report;

			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IReportSAFTWriter>)?.Get();
			if (provider != null)
			{
				saftXMLWriter = SAFTXMLWriterHelper.GetSAFTXMLWriter(provider.GetSAFTVersion, report, reportModeAndCreditorSelector, UpdateProgressStatus);
			}
		}

		public SAFTXmlBuilder(ReportModeAndCreditorSelector reportModeAndCreditorSelector, AccComplianceReport[] reports)
		{
			Argument.NotNull(reports, nameof(reports));
			Argument.GreaterThanZero(reports.Length, nameof(reports) + ".Length");

			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IReportSAFTWriter>)?.Get();
			if (provider != null)
			{
				saftXMLWriter = SAFTXMLWriterHelper.GetSAFTXMLWriter(provider.GetSAFTVersion, reports, reportModeAndCreditorSelector, UpdateProgressStatus);
			}
		}

		readonly AccComplianceReport Report;
		readonly ISAFTXMLWriter saftXMLWriter;

		#region Constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded format string")]
		internal const string DateFormat = "yyyy-MM-dd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded format string")]
		internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

		#endregion

		#region IProgressFormSupportable

		public string CurrentStatusText => currentStatusText;

		public string Log => string.Empty;

		public int CompletedItems => completedItems;

		public int TotaItemsToComplete => totaItemsToComplete;

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		#region Implementation

		string currentStatusText;
		int completedItems;
		int totaItemsToComplete;

		void UpdateProgressStatus(string statusText, int? itemsToComplete = null)
		{
			currentStatusText = statusText;
			if (itemsToComplete.HasValue)
			{
				this.totaItemsToComplete = itemsToComplete.Value;
				completedItems = 0;
			}
			completedItems++;

			RaiseProgressUpdateEvent?.Invoke(this, completedItems == this.totaItemsToComplete);
		}

		#endregion

		#endregion

		public (IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) WriteXmlToStream(Func<Stream> openXmlFile, string unmappedFileName)
		{
			if (Report != null)
			{
				return saftXMLWriter?.WriteSingleReportXmlToStream(openXmlFile, unmappedFileName) ?? (null, null, false);
			}
			else
			{
				return saftXMLWriter?.WriteAnnualReportXmlToStream(openXmlFile, unmappedFileName) ?? (null, null, false);
			}
		}

		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			return saftXMLWriter?.BuildSingleReportXml(report, line, additionalData);
		}

		internal override XStreamingElement BuildAnnualXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			return saftXMLWriter?.BuildAnnualReportXml(reports, additionalData);
		}
	}
}
