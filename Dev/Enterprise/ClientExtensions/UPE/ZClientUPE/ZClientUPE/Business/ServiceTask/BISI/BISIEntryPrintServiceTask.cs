using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	BISIEntryPrintServiceTask.Code,
	"Entry Print Upload",
	"CSP",
	typeof(BISIEntryPrintServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "1hours",
	DefaultScheduleEndAtLocal = "2hours"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class BISIEntryPrintServiceTask : UPEServiceTask
	{
		public const string Code = "ZU2";

		public BISIEntryPrintServiceTask()
		{
		}

		public BISIEntryPrintServiceTask(ILogger logger)
			: base(logger)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override void RunTask(CancellationToken token)
		{
			try
			{
				TryUpload(token);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInBISIEntryPrintBatchProcessor", ex.Message, ex);
			}
		}

		#region Implementation

		protected virtual FtpUploader GetNewBISIUploader()
		{
			return new BISIEntryPrintUploader(Notifications);
		}

		protected virtual BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal void TryUpload(CancellationToken token)
		{
			var branches = UPETools.Instance.UPECustomisationBranches(false);

			foreach (var branch in branches)
			{
				using (branch.SetAsTemporaryContext())
				{
					try
					{
						WaitFor<Object>.Run(TimeSpan.FromSeconds(UPEDataRegistry.Instance.SftpServerTimeout), () =>
						{
							FtpUploader uploader = GetNewBISIUploader();
							if (uploader.CanUpload())
							{
								Notifications.Notify(new InfoNotification(ZString.Format("Start Entry Print Uploading for Company {0}", branch.CompanyName)));
								int entryPrintCount = 0;
								try
								{
									entryPrintCount = UploadEntries(uploader, token);
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									Notifications.Notify(new ErrorNotification(ErrorType.Error, "An error has occured while connecting to the FTP server:" + ex.Message));
								}
								finally
								{
									Notifications.Notify(new InfoNotification(ZString.Format("Finish Entry Print Uploading ({0} Uploaded) for Company {1}", entryPrintCount, branch.CompanyName)));
								}
							}
							return null;
						});
					}
					catch (TimeoutException)
					{
						UPETools.Instance.SendTimeoutEmail(Code);
					}
				}
			}
		}

		int UploadEntries(FtpUploader uploader, CancellationToken token)
		{
			BusinessObjectFactory factory = NewFactory();
			int entryPrintCount = 0;
			string uploadFileContent = GenerateBatchOfEntriesNotUploadedAndMarkAsProcessed(factory, out entryPrintCount, token);

			if (entryPrintCount > 0)
			{
				SaveInTransactionDelegateAction uploadEntriesAction = new SaveInTransactionDelegateAction(factory, () => { UploadEntries(factory, uploader, entryPrintCount, uploadFileContent); return ChangedTableNames.Empty; });
				try
				{
					BusinessObjectFactory.SaveTogether(factory, uploadEntriesAction);
				}
				catch (RollbackTransactionException)
				{
					entryPrintCount = 0;
				}
			}
			return entryPrintCount;
		}

		void UploadEntries(BusinessObjectFactory factory, FtpUploader uploader, int entryPrintCount, string uploadFileContent)
		{
			long batchNumber = UPENumberFountains.Instance.BISIEntryPrintBatchNumber.GetNext(factory);
			uploadFileContent = GenerateHeaderOrTrailer("HDR", entryPrintCount, batchNumber) + uploadFileContent;
			uploadFileContent = uploadFileContent + GenerateHeaderOrTrailer("FTR", entryPrintCount, batchNumber);

			using (var sr = new StringReader(uploadFileContent))
			{
				if (!uploader.UploadToFtpServerAndArchive(sr))
				{
					throw new RollbackTransactionException();
				}
			}
		}

#if NETFRAMEWORK
		[Serializable]
#endif
		public class RollbackTransactionException : Exception
		{
			public RollbackTransactionException() : base() { }

#if NETFRAMEWORK
			protected RollbackTransactionException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		string GenerateBatchOfEntriesNotUploadedAndMarkAsProcessed(BusinessObjectFactory factory, out int entryPrintCount, CancellationToken token)
		{
			ZQuery filter = GetOutstandingDeclarationFilter();
			FilteredBusinessObjectReader unprocessedDeclarationBatch = new FilteredBusinessObjectReader(filter, typeof(UPEJobDeclaration));

			StringBuilder result = new StringBuilder();
			entryPrintCount = AppendEntriesAndMarkAsProcessed(factory, unprocessedDeclarationBatch, result, token);

			return result.ToString();
		}

		int AppendEntriesAndMarkAsProcessed(BusinessObjectFactory factory, IEnumerable unprocessedDeclarations, StringBuilder content, CancellationToken token)
		{
			int entryPrintCount = 0;
			foreach (var declaration in unprocessedDeclarations.Cast<UPEJobDeclaration>())
			{
				token.ThrowIfCancellationRequested();

				if (AppendEntryPrint(declaration, content))
				{
					entryPrintCount++;
				}
				ProcessQueue queue = (ProcessQueue)factory.Load(typeof(ProcessQueue), declaration.CurrentQueue.PK);
				queue[UPEJobDeclaration.IsEntryPrintToBISIPendingProcessQueueColumn.Name] = false;

				if (entryPrintCount >= MaximumDeclarationsToProcessPerDay)
				{
					break;
				}
			}
			return entryPrintCount;
		}

		protected virtual int MaximumDeclarationsToProcessPerDay
		{
			get { return UPEDataRegistry.Instance.EntryPrintMaximumDeclarationsToSendPerDay; }
		}

		bool AppendEntryPrint(UPEJobDeclaration declaration, StringBuilder content)
		{
			bool entryPrintFound = false;
			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrint(entryHeader, false);
				if (!entryPrint.EntryPrint.IsEmpty)
				{
					content.Append(entryPrint.EntryPrint);
					entryPrintFound = true;
				}
			}
			return entryPrintFound;
		}

		ZQuery GetOutstandingDeclarationFilter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			ZDBOnlySubQuery processQueueSubQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
			processQueueSubQuery.AddToFilter(UPEJobDeclaration.IsEntryPrintToBISIPendingProcessQueueColumn, ZBool.True);
			query.AddSubQuery(processQueueSubQuery, JoinCondition.And);
			ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompanyPK);
			query.AddSubQuery(JobDeclarationSchema.JE_GB, branchSubQuery, JoinCondition.And);
			return query;
		}

		string GenerateHeaderOrTrailer(string prefix, int totalEntryPrints, long batchNumber)
		{
			string result = string.Format(CultureInfo.InvariantCulture, "{0}-{1:0000}{2:0000}AU{3}\r\n", prefix, batchNumber, totalEntryPrints, ZDateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff", CultureInfo.InvariantCulture));
			return result;
		}

#endregion
	}
}

