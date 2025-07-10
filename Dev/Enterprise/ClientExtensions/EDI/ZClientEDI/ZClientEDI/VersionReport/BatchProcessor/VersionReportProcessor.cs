using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	abstract class VersionReportProcessor : IEmailAttachmentProcessor
	{
		BusinessObjectFactory factory;

		public VersionReportProcessor(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		public VersionReportProcessor()
			: this(null)
		{
		}

		public ILogger ServiceLogger { get; private set; }

		protected BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory()
				{
#if DEBUG
					// Refresh enabled for unit tests - currently rely on records in the unit test factory updating when this factory is saved
#else
					RefreshEnabled = false
#endif
				});
			}
		}

		protected virtual LicenceDatabase GetLicenceDatabase(EDIVersionReport report)
		{
			LicenceDatabase db = null;
			if (report.DatabaseNumberSpecified)
			{
				db = Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, report.DatabaseNumber));
			}
			else
			{
				db = LicenceDatabase.Load(Factory, report.EnterpriseCode, null, report.PhysicalServerID);
			}
			return db;
		}

		protected ReleaseBuild GetReleaseBuild(string version)
		{
			VersionNumber versionNumber = new VersionNumber(version);
			ZQuery filter = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, versionNumber.Major);
			filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, versionNumber.Minor);
			filter.AddToFilter(ReleaseBuildSchema.HL_Release, versionNumber.Release);
			filter.AddToFilter(ReleaseBuildSchema.HL_Patch, versionNumber.Patch);
			filter.AddToFilter(ReleaseBuildSchema.HL_Product, ProductTypes.EnterpriseFamilyProducts);
			return Factory.LoadTop1<ReleaseBuild>(filter);
		}

		public void Process(string xmlData)
		{
			EDIVersionReport report = RunProcess(xmlData);
			ReportProcessorHelper.SaveReport(ServiceLogger, GetReportId(), report.EnterpriseCode, report.PhysicalServerID, xmlData);
		}

		public void ProcessWithoutSavingReport(string xmlData)
		{
			RunProcess(xmlData);
		}

		EDIVersionReport RunProcess(string xmlData)
		{
			var report = new EDIVersionReport(xmlData);

			const int maximumTrials = 3;
			int countOfTrials = 0;
			while (countOfTrials < maximumTrials)
			{
				try
				{
					ProcessVersionReport(report);
					break;
				}
				catch (Exception ex) when (ex is ZDataConcurrencyException || ex is DBConcurrencyException || ex is IConcurrencyException)
				{
					countOfTrials++;
					factory = null;
				}
			}

			return report;
		}

		protected abstract void ProcessVersionReport(EDIVersionReport report);
		protected abstract string GetReportId();
	}
}
