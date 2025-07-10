using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public abstract class StmDocumentDeliveryManager
	{
		public virtual void Execute(StmDocumentDelivery[] jobs, ILogger logger = null)
		{
			if (jobs.Length == 0)
			{
				logger?.Log(LogType.Information, jobs.Length + (NoResString)" document(s) to process.");
				return;
			}

			UndoJobs.Clear();
			logger?.Log(LogType.Information, jobs.Length + (NoResString)" document(s) to process.");

			foreach (var stmDocumentDelivery in jobs)
			{
				try
				{
					ProcessDocumentDelivery(stmDocumentDelivery);
					Done(stmDocumentDelivery);
				}
				catch (Exception e) 
				{
					logger?.Log(LogType.Error, (NoResString)"Process " + stmDocumentDelivery.PK + (NoResString)" failed, Retry count: " + stmDocumentDelivery.SDL_RetryAttempts, e);
					ErrorReporter.ReportOnce("BDDFailure: Execute", "BDD(Background Document Delivery) Run Failure when Execute.", e);
					Undo(stmDocumentDelivery);
				}
			}

			if (UndoJobs.Count > 0)
			{
				Redo(logger);
			}
		}

		public virtual int CountProcessedJobs()
		{
			return SuccessJobs.Count;
		}

		public virtual void PurgeOld(ILogger logger = null)
		{
			logger?.Log(LogType.Information, (NoResString)"Purging documents previously processed");
			try
			{
				var query = new ZQuery();
				query.AddToFilter(StmDocumentDeliverySchema.SDL_IsProcessed, SQLComparisonOperator.Equal, 1);

				var factory = new BusinessObjectFactory();
				Delete(factory.Load<StmDocumentDelivery>(query));
			}
			catch (SqlException e)
			{
				logger?.Log(LogType.Error, (NoResString)"Failure: Purging documents previously processed.", e);
				ErrorReporter.ReportOnce("BDDFailure: PurgeOld", "BDD(Background Document Delivery) Run Failure when Purge Old", e);
			}
		}

		void Delete(StmDocumentDelivery[] jobs)
		{
			if (jobs.Length == 0)
			{
				return;
			}

			var jobFactory = jobs[0].Factory;
			foreach (StmDocumentDelivery job in jobs)
			{
				job.Delete();
			}

			try
			{
				jobFactory.Save();
			}
			catch (ZSaveConcurrencyException) { }
		}

		public abstract void ProcessDocumentDelivery(StmDocumentDelivery stmDocumentDelivery);

		void Undo(StmDocumentDelivery job)
		{
			if (job.SDL_RetryAttempts < MaxRetryAttempts)
			{
				UndoJobs.Enqueue(job);
			}
		}

		void Redo(ILogger logger = null)
		{
			try
			{
				logger?.Log(LogType.Information, (NoResString)"BDD - Redo");

				var undoJobs = UndoJobs.ToArray();
				foreach (var undoJob in undoJobs)
				{
					undoJob.SDL_RetryAttempts += 1;
				}
				undoJobs[0].Factory.Save();
			}
			catch (ZSaveConcurrencyException e)
			{
				ErrorReporter.ReportOnce("BDDFailure: Redo", "BDD(Background Document Delivery) Run Failure when Redo, SqlException.", e);
			}

			Execute(UndoJobs.ToArray(), logger);
		}

		void Done(StmDocumentDelivery stmDocumentDelivery)
		{
			try
			{
				stmDocumentDelivery.SDL_IsProcessed = ZBool.True;
				stmDocumentDelivery.Factory.Save();
			}
			catch (ZSaveConcurrencyException e)
			{
				ErrorReporter.ReportOnce("BDDFailure: Done", "BDD(Background Document Delivery) Run Failure when Done, SqlException.", e);
			}

			SuccessJobs.Enqueue(stmDocumentDelivery);
		}

		internal const int MaxRetryAttempts = 3;

		internal Queue<StmDocumentDelivery> UndoJobs
		{
			get
			{
				if (fUndoJobs == null)
				{
					fUndoJobs = new Queue<StmDocumentDelivery>();
				}

				return fUndoJobs;
			}
		}
		Queue<StmDocumentDelivery> fUndoJobs;

		internal Queue<StmDocumentDelivery> SuccessJobs
		{
			get
			{
				if (fSuccessJobs == null)
				{
					fSuccessJobs = new Queue<StmDocumentDelivery>();
				}

				return fSuccessJobs;
			}
		}
		Queue<StmDocumentDelivery> fSuccessJobs;
	}
}
