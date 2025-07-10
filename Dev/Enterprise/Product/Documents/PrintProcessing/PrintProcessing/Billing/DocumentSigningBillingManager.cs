using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PrintProcessing.Billing
{
	public class DocumentSigningBillingManager
	{
		public DocumentSigningBillingManager()
		{
			Factory = new BusinessObjectFactory();
		}

		public BusinessObjectFactory Factory { get; }

		public void LogUsage(StmPrintJob printJob , string documentSigningProviderCode, DocumentSigningStatus signingStatus, string transactionId, ILogger logger)
		{
			var warningMessage = string.Empty;
			if (printJob == null)
			{
				warningMessage = (NoResString)"Print Job cannot be null";
			}
			else if (printJob.SP_SignBy != "DOS")
			{
				warningMessage = $"Print Job isn't set for document signing: {printJob.SP_SignBy}";
			}
			else if (!printJob.SP_IsSigned && signingStatus == DocumentSigningStatus.Success)
			{
				warningMessage = $"Print Job must be signed or Failed";
			}
			else if (printJob.Branch == null)
			{
				warningMessage = $"Branch does not exist: {printJob.SP_GB}";
			}
			else if (printJob.Branch.Company == null)
			{
				warningMessage = $"Company does not exist: {printJob.Branch.GB_GC}";
			}
			else if (string.IsNullOrEmpty(documentSigningProviderCode))
			{
				warningMessage = (NoResString)"Missing provider code";
			}

			if (!string.IsNullOrEmpty(warningMessage))
			{
				logger.Log(LogType.Warning, $"Could not report usage. Warning: {warningMessage}.");
				return;
			}

			var properties = new List<(string name, object value)>();
			properties.Add((UsageProperties.CountryDocumentCode, $"{printJob.Branch.GB_RN_NKCountryCode}1"));
			properties.Add((UsageProperties.Mode, printJob.SP_DocumentType));
			properties.Add((UsageProperties.JobType, printJob.SP_JobType));
			properties.Add((UsageProperties.DocumentName, printJob.SP_DocumentName));
			properties.Add((UsageProperties.ParentGuid, printJob.SP_ParentGuid));
			properties.Add((UsageProperties.ParentTableName, printJob.SP_ParentTableName));
			properties.Add((UsageProperties.ProcessorName, documentSigningProviderCode));
			properties.Add((UsageProperties.RunDateTime, printJob.SP_RunDateTime.ToLongTimeString()));
			properties.Add((UsageProperties.SystemCreateUser, printJob.SP_SystemCreateUser));
			properties.Add((UsageProperties.SignStatus, signingStatus.ToString()));
			properties.Add((UsageProperties.TransactionId, transactionId));

			try
			{
				CallApi(properties, printJob.Branch);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"Could not report usage. Error: {ex.Message}", ex);
			}
		}

		protected virtual void CallApi(List<(string name, object value)> properties, GlbBranch branch)
		{
			UsageCollector.Report(Factory, UsageFeatures.Codes.DocumentSigning, branch, properties.ToArray());
		}
	}

	public enum DocumentSigningStatus
	{
		Success,
		Fail
	}
}
