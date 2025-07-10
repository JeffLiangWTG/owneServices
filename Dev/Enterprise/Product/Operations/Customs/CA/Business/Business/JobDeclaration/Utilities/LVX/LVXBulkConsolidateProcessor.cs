using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LVXBulkConsolidateProcessor
	{
		public LVXBulkConsolidateProcessor(IEnumerable<ZGuid> selectedLVXJobPKs)
		{
			this.FactoryProvider = new BusinessObjectFactoryProvider();
			this.selectedLVXJobPKs = selectedLVXJobPKs;
			this.totalCount = selectedLVXJobPKs.Count();
		}

		public void StartProcessing()
		{
			LineNumber = 0;
			countOfSavings = 1;
			Canceled = false;

			RaiseOnNotify(LogType.Information, Res.GetString("694F0354-B8AB-416F-9BB0-FA97671DC3A1", "Process started, total count : {0}", totalCount));

			if (totalCount > 0)
			{
				var associatedLVSDeclarations = new List<JobDeclaration>();
				foreach (var lvxJobPK in selectedLVXJobPKs)
				{
					var currentFactory = FactoryProvider.Current;
					var lvxNeedToConsolidate = currentFactory.Load<JobDeclaration>(lvxJobPK);
					if (lvxNeedToConsolidate == null)
					{
						continue;
					}

					if (Canceled)
					{
						RaiseOnNotify(LogType.Information, Res.GetString("DDA27599-FCB2-422E-8A61-97F33F56EE34", "Process stopped by user, {0} records processed.", LineNumber));
						break;
					}

					RaiseOnProgress(++LineNumber, totalCount, Res.GetString("F3A6B358-D008-47DF-B81F-9A2CFB94E5A9", "Processing {0}", lvxNeedToConsolidate.JE_DeclarationReference));

					LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvxNeedToConsolidate, currentFactory, associatedLVSDeclarations, (lvx) => lvx.JE_DeclarationReference, RaiseOnNotify);

					if (SaveRequired)
					{
						LVXJobsConsolidateHelper.RaiseSaving(associatedLVSDeclarations, () => BatchSave(), RaiseOnNotify);
					}
				}
			}

			RaiseOnProgress(100, 100, Res.GetString("A9085A0B-2302-47AF-A568-A40748BB96BB", "Process completed."));
			RaiseOnNotify(LogType.Information, Res.GetString("B5DA123E-EB65-4520-B203-FEAB6EAF6D0E", "Process completed."));
			RaiseProcessCompleted();
		}

		#region Events
		void RaiseOnProgress(int current, int total, string message)
		{
			OnProgress?.Invoke(this, new BulkConsolidateProgressEventArgs(current, total, message));
		}

		void RaiseOnNotify(LogType logType, string message, params object[] args)
		{
			if (OnNotify != null)
			{
				if (SyncInvoke == null)
				{
					OnNotify(string.Format(CultureInfo.InvariantCulture, message, args));
				}
				else
				{
					SyncInvoke.BeginInvoke(OnNotify, new[] { string.Format(CultureInfo.InvariantCulture, message, args) });
				}
			}
		}

		void RaiseProcessCompleted()
		{
			ProcessCompleted?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		public void Cancel()
		{
			Canceled = true;
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
				RaiseOnNotify(LogType.Error, e.Message);
			}
		}

		bool SaveRequired
		{
			get { return LineNumber / BatchSize >= countOfSavings || LineNumber >= totalCount || Canceled; }
		}

		public ISynchronizeInvoke SyncInvoke { get; set; }
		protected readonly IEnumerable<ZGuid> selectedLVXJobPKs;
		protected readonly int totalCount;
		public event BulkConsolidateProgressEventHandler OnProgress;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event NotifyDelegate OnNotify;
		public event EventHandler ProcessCompleted;

		bool Canceled { get; set; }
		BusinessObjectFactoryProvider FactoryProvider { get; set; }
		const int BatchSize = 50;
		int LineNumber { get; set; }
		int countOfSavings;
		public delegate void NotifyDelegate(string message);
	}
}
