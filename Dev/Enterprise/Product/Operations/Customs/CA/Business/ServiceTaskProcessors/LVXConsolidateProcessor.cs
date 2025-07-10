using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class LVXConsolidateProcessor
	{
		public LVXConsolidateProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Process(IEnumerable<ZGuid> selectedLVXJobs)
		{
			logger.Log(LogType.Information, "Start Courier LVS Declaration processing");

			LineNumber = 0;
			this.totalCount = selectedLVXJobs.Count();
			this.countOfSavings = 1;
			do
			{
				ConsolidateLVXJobs(selectedLVXJobs);
			}
			while (LineNumber < totalCount);

			if (totalCount > 0)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Successfully processed {0} Courier LVS Declarations.", totalCount));
			}
			else
			{
				logger.Log(LogType.Information, "There is no Courier LVS Declaration ready for consolication.");
			}
		}

		BusinessObjectFactory Factory => FactoryProvider.Current;
		BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider();
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		public void ConsolidateLVXJobs(IEnumerable<ZGuid> selectedLVXJobPKs)
		{
			if (selectedLVXJobPKs.Any())
			{
				List<JobDeclaration> associatedDeclarations = new List<JobDeclaration>();
				foreach (var lvxJobPK in selectedLVXJobPKs)
				{
					LineNumber++;
					var lvxNeedToConsolidate = Factory.Load<JobDeclaration>(lvxJobPK);
					if (lvxNeedToConsolidate == null)
					{
						continue;
					}

					LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvxNeedToConsolidate, Factory, associatedDeclarations, (lvx) => lvx.JE_DeclarationReference, AddLog);

					if (LineNumber / BatchSize >= countOfSavings || LineNumber >= totalCount)
					{
						LVXJobsConsolidateHelper.RaiseSaving(associatedDeclarations, () => BatchSave(), AddLog);
					}
				}
			}
		}

		void AddLog(LogType logType, string message, params object[] args)
		{
			logger.Log(logType, string.Format(CultureInfo.InvariantCulture, message, args));
		}

		void BatchSave()
		{
			try
			{
				FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
				countOfSavings++;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				logger.Log(LogType.Error, e.Message);
			}
		}

		readonly ILogger logger;
		const int BatchSize = 50;
		int LineNumber { get; set; }
		protected int totalCount;
		int countOfSavings;
	}
}
