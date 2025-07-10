using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	#region IInboundInterchangeProcessor

	public interface IInboundInterchangeProcessor
	{
		void Execute(CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
			);
	}

	public interface IInboundMessageCreator
	{
		void CreateMessagesForInterchange(EDIInterchange interchange);
	}

	public sealed class InboundMessageCreatorNoCreation : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			// do nothing; it used to cause EI_Status to be set as 'RCV'
		}
	}

	#endregion

	public abstract class InboundInterchangeProcessor : BaseInboundInterchangeProcessor
	{
		protected InboundInterchangeProcessor()
		{
		}

		protected InboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected abstract IInboundMessageCreator GetMessageCreator(EDIInterchange interchange);

		protected sealed override bool ProcessInterchange(EDIInterchange interchange)
		{
			var messageCreator = GetMessageCreator(interchange);
			messageCreator?.CreateMessagesForInterchange(interchange);
			return messageCreator != null && interchange.EI_Status != Integration.EDIInterchangeStatusList.Codes.Error;
		}

		protected sealed override string GetInterchangeOrderBy()
		{
			return base.GetInterchangeOrderBy();
		}
	}

	public abstract class BaseInboundInterchangeProcessor : BatchProcess, IInboundInterchangeProcessor
	{
		protected BaseInboundInterchangeProcessor()
		{
		}

		protected BaseInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		void IInboundInterchangeProcessor.Execute(CancellationToken token)
		{
			ExecuteBatch(token);
		}

		protected override void Execute(CancellationToken token)
		{
			var interchanges = GetQueuedInboundInterchanges();
			while (interchanges.Length > 0)
			{
				foreach (var interchange in interchanges)
				{
					token.ThrowIfCancellationRequested();
					ProcessInterchangeAndSave(interchange.PK);
				}
				interchanges = ShouldCheckForMore ? GetQueuedInboundInterchanges() : Array.Empty<EDIInterchange>();
			}
		}
		protected virtual bool ShouldCheckForMore => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected abstract string[] ApplicationCodes { get; }

		/// <summary>
		/// Sender ID is not entered on a registry, or a related certificate is invalid, etc...
		/// If a non empty reason is returned, all messages are cancelled.
		/// </summary>
		protected ZString ReasonForCannotProcessInterchange => GetReasonForCannotProcessInterchangeCore();

		protected virtual ZString GetReasonForCannotProcessInterchangeCore() => ZString.Empty;

		EDIInterchange[] GetQueuedInboundInterchanges()
		{
			var factory = new BusinessObjectFactory();
			var query = IsNoBranchFilter ? new ZQuery() : new ZQuery(ValidBranchesForFilter);
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodes);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			AddTypeFilter(query);
			query.OrderBy = GetInterchangeOrderBy();
			return factory.Load<EDIInterchange>(query);
		}

		protected virtual string GetInterchangeOrderBy() => EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + ", " + EDIInterchange.Schema.EI_InterchangeNum;

		protected virtual bool IsNoBranchFilter
		{
			get { return false; }
		}

		ZQuery ValidBranchesForFilter
		{
			get
			{
				var key = GlbCompany.CurrentCompany.PK;

				if (!BranchesForFilterList.TryGetValue(key, out var result))
				{
					var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
					branchQuery.AddToFilter(GlbBranchSchema.GB_GC, key);

					result = new ZDBOnlyQuery(typeof(EDIInterchange));
					((ZDBOnlyQuery)result).AddSubQuery(EDIInterchangeSchema.EI_GB, branchQuery, JoinCondition.And);

					BranchesForFilterList.Add(key, result);
				}

				return result;
			}
		}

		protected virtual void AddTypeFilter(ZQuery query)
		{
		}

		Dictionary<ZGuid, ZQuery> BranchesForFilterList
		{
			get { return branchesForFilterList ?? (branchesForFilterList = new Dictionary<ZGuid, ZQuery>()); }
		}
		Dictionary<ZGuid, ZQuery> branchesForFilterList;

		public virtual bool IsInterchangeNotDeleted(EDIInterchange interchange)
		{
			return !interchange.IsDeleted;
		}

		protected virtual Type TypeOfInterchangeToCreate()
		{
			return typeof(EDIInterchange);
		}

		void ProcessInterchangeAndSave(ZGuid interchangePK)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var interchange = (EDIInterchange)factory.Load(TypeOfInterchangeToCreate(), interchangePK);
			if (interchange != null)
			{
				using (SwithEnvironmentIfNeeded(interchange.EI_GB))
				{
					try
					{
						if (ReasonForCannotProcessInterchange.IsEmpty)
						{
							var processResult = ProcessInterchange(interchange);
							if (IsInterchangeNotDeleted(interchange))
							{
								interchange.EI_Status = processResult ? (ZString)EDIInterchange.Status.Received : GetStatusForProcessFailure(interchange);
							}
						}
						else
						{
							interchange.EI_Status = EDIInterchange.Status.Cancelled;
						}
						int saveCount = 0;
						SaveHandle(interchange, ref saveCount);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleProcessingException(ex, interchange, interchangePK);
					}
				}
			}
		}

		IDisposable SwithEnvironmentIfNeeded(ZGuid branchPK)
		{
			IDisposable result = null;
			if (SupportEnvironmentSwitch)
			{
				result = DisposableEnvironment.ForBranch(branchPK.ToGuid());
			}
			else
			{
				result = DisposableAction.NoAction;
			}
			return result;
		}

		protected virtual bool SupportEnvironmentSwitch => false;

		protected virtual ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return EDIInterchange.Status.Failed;
		}

		protected abstract bool ProcessInterchange(EDIInterchange interchange);

		protected virtual void HandleProcessingException(Exception ex, EDIInterchange interchange, ZGuid interchangePK)
		{
			Logger.LogError(ex.Message);
			MarkInterchangeAsFailed(interchangePK, ex.Message);
		}

		void MarkInterchangeAsFailed(ZGuid interchangePK, ZString exceptionMessage)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var interchange = factory.Load<EDIInterchange>(interchangePK);
			if (interchange != null)
			{
				interchange.EI_Status = EDIInterchange.Status.Failed;
				if (AddNoteOnException)
				{
					var note = interchange.Notes.AddNew();
					note.ST_Description = PredefinedNoteTypes.Instance.DataImportLogNote.Description;
					note.ST_IsCustomDescription = false;
					note.ST_NoteText = exceptionMessage;
					note.ST_NoteContext = "AAA";
				}
				int saveCount = 0;
				try
				{
					SaveHandle(interchange, ref saveCount);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Logger.LogError(Res.GetString("{8BF8E1E8-7273-470B-A9B1-C5D5CA5C27DA}", "The following error was encountered while setting Interchange (Application Code='{1}', Interchange Number='{2}') status to failed:{0}{0}{3}", System.Environment.NewLine, interchange.EI_ApplicationCode, interchange.EI_InterchangeNum, ex.Message));
				}
			}
		}

		protected virtual bool AddNoteOnException => false;

		void SaveHandle(EDIInterchange interchange, ref int count)
		{
			try
			{
				interchange.Factory.Save();
				if (!interchange.IsDeleted && interchange.EI_Status == EDIInterchange.Status.Received)
				{
					Logger.Log(Res.GetString("{C09B3B71-3C18-4F2E-BBF6-067B0F79C8EB}", "Interchange '{0}' has been processed successfully.", interchange.EI_InterchangeNum));
				}
				else if (interchange.EI_Status == EDIInterchange.Status.Cancelled && !ReasonForCannotProcessInterchange.IsEmpty)
				{
					Logger.Log(Res.GetString("{1C242A11-1C5E-4841-B6A3-8D89D2A520BA}", "Interchange '{0}' has not been processed for company '{1}': {2}", interchange.EI_InterchangeNum, GlbCompany.CurrentCompany.GC_Name, ReasonForCannotProcessInterchange));
				}
			}
			catch (ZSaveConcurrencyException)
			{
				if (count > 3)
				{
					throw;
				}
				else
				{
					Thread.Sleep(3000);
					count++;
					SaveHandle(interchange, ref count);
				}
			}
		}
	}
}
