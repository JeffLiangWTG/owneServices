using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using static System.FormattableString;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ExceptionHandlingAbortedAfterErrorMessageException : RethrownByExceptionHandlerException
	{
		public ExceptionHandlingAbortedAfterErrorMessageException(string message, Exception innerException)
			: base(innerException)
		{
			this.message = message ?? string.Empty;
		}

		readonly string message;

#if NETFRAMEWORK
		protected ExceptionHandlingAbortedAfterErrorMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public override string Message
		{
			get { return message; }
		}
	}

	public static class ZExceptionReporting
	{
		/// <summary>
		/// Processes an action and tries to handle save exceptions like concurrency, constraints, etc.
		/// If exception happens and it is handled, an action will be repeated again.
		/// </summary>
		/// <param name="action">An action to be run inside exceptions handler.</param>
		/// <param name="recoveryAction">Additional action to be run if exception happen before repeating main action.</param>
		public static void ProcessWithSaveExceptionHandling(Action action, Action recoveryAction)
		{
			ProcessWithSaveExceptionHandling(action, recoveryAction, false, false, 1);
		}

		/// <summary>
		/// Processes an action and tries to handle save exceptions like concurrency, constraints, etc.
		/// If exception happens and it is handled, an action will be repeated again.
		/// </summary>
		/// <param name="action">An action to be run inside exceptions handler.</param>
		/// <param name="recoveryAction">Additional action to be run if exception happen before repeating main action.</param>
		/// <param name="reportErrorsOnly">If true, information reports about exception handling will be silenced.</param>
		public static void ProcessWithSaveExceptionHandling(Action action, Action recoveryAction, bool reportErrorsOnly = false)
		{
			ProcessWithSaveExceptionHandling(action, recoveryAction, reportErrorsOnly, false, 1);
		}

		/// <summary>
		/// Processes an action and tries to handle save exceptions like concurrency, constraints, etc.
		/// If exception happens and it is handled, an action will be repeated again.
		/// </summary>
		/// <param name="action">An action to be run inside exceptions handler.</param>
		/// <param name="recoveryAction">Additional action to be run if exception happen before repeating main action.</param>
		/// <param name="reportErrorsOnly">If true, information reports about exception handling will be silenced.</param>
		/// <param name="throwOnMergeFailure">If true, instead of reporting an error on merge failure, we throw an exception.</param>
		/// <param name="attempts">If set, define the number of attempt to recover.</param>
		public static void ProcessWithSaveExceptionHandling(Action action, Action recoveryAction, bool reportErrorsOnly = false, bool throwOnMergeFailure = false, int attempts = 1, INotificationHandler notifier = null)
		{
			try
			{
				action();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var handled = false;

				try
				{
					var errorsNotificationHandler = notifier ?? new ErrorsNotificationHandler(reportErrorsOnly);
					HandleSaveException(ex, errorsNotificationHandler, saveInitiator: null, throwOnMergeFailure ? ConcurrencyResolver.OnMergeFailureAction.ThrowException : ConcurrencyResolver.OnMergeFailureAction.SendReport);

					var saveException = ex as ZSaveException;
					if (saveException != null && !saveException.CanRecover)
					{
						handled = false;

						if (errorsNotificationHandler is ErrorsNotificationHandler handler && handler.HasErrorsReported)
						{
							throw new ExceptionHandlingAbortedAfterErrorMessageException("Exception handling aborted after error message shown: " + handler.LastErrorMessage, ex);
						}
					}
					else
					{
						recoveryAction?.Invoke();

						var initialFactoryPKs = new HashSet<ZGuid>();
						var exceptionPKs = new HashSet<ZGuid>();
						if (saveException?.BusinessObjects?.Any() ?? false)
						{
							// here we are attempting to handle the fact that the save inside of action may result in several SQL commands all of which may have recoverable concurrency errors etc
							// We want to keep trying until we've run of out business objects that were part of the initial factory.
							// We need to capture the initial objects that were in the factory, as re-running action() may make more.
							initialFactoryPKs.UnionWith(((IBusinessObjectFactoryInternals)saveException.Factory).AllBusinessObjects.Where(x => x.HasChanges).Select(x => x.PK));
							exceptionPKs.UnionWith(saveException.BusinessObjects.Select(o => o.PK));
						}

						for (var i = 0; i < attempts && !handled; ++i)
						{
							try
							{
								action();
								handled = true;
								break;
							}
							catch (Exception recoveryActionEx) when (!recoveryActionEx.IsCriticalException())
							{
								if (recoveryActionEx is ZSaveException recoveryActionSaveException && recoveryActionSaveException.BusinessObjects.Any())
								{
									var newExceptionPKs = recoveryActionSaveException.BusinessObjects.Select(o => o.PK).Where(x => initialFactoryPKs.Contains(x)).ToHashSet();
									if (newExceptionPKs.Count != 0 && !newExceptionPKs.Intersect(exceptionPKs).Any())
									{
										i--;
										exceptionPKs.UnionWith(newExceptionPKs);
									}
								}

								if (i >= attempts - 1)
								{
									ex.Data.Add(FindUniqueErrorKey(ex), recoveryActionEx);
									break;
								}

								TryHandleSaveException(recoveryActionEx, errorsNotificationHandler, ex);
							}
						}
					}
				}
				catch (ExceptionHandlingAbortedAfterErrorMessageException)
				{
					throw;
				}
				catch (Exception exceptionWhileHandling) when (!exceptionWhileHandling.IsCriticalException())
				{
					handled = false;

					// Will be written in error report by ExceptionDetails.WriteExceptionData(Exception exception)
					ex.Data.Add(FindUniqueErrorKey(ex), exceptionWhileHandling);
				}

				if (!handled)
				{
					throw;
				}
			}
		}

		public static void ProcessWithConcurrencyHandling(Action action, Action onRetry, int retries = 3)
		{
			var concurrencyRelatedRetries = retries;
			do
			{
				try
				{
					ProcessWithSaveExceptionHandling(action, onRetry, reportErrorsOnly: true, throwOnMergeFailure: true);
					concurrencyRelatedRetries = -1;
				}
				catch (Exception exception) when (exception is ZSaveConcurrencyException || exception is ZDataConcurrencyException)
				{
					if (concurrencyRelatedRetries == 0)
					{
						throw;
					}
					onRetry?.Invoke();
				}
			}
			while (concurrencyRelatedRetries-- > 0);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Top level exception handler")]
		static bool TryHandleSaveException(Exception ex, INotificationHandler errorsNotificationHandler, Exception mainExceptionMessage)
		{
			try
			{
				HandleSaveException(ex, errorsNotificationHandler);
				return true;
			}
			catch (Exception exceptionWhileHandling)
			{
				// Will be written in error report by ExceptionDetails.WriteExceptionData(Exception exception)
				mainExceptionMessage.Data.Add(FindUniqueErrorKey(mainExceptionMessage), exceptionWhileHandling);
				return false;
			}
		}

		static string FindUniqueErrorKey(Exception ex)
		{
			const string handlingExDataKeyPrefix = "ExceptionHandlingException";

			var handlingExDataKey = handlingExDataKeyPrefix;
			for (var x = 1; ex.Data.Contains(handlingExDataKey); x++)
			{
				handlingExDataKey = handlingExDataKeyPrefix + x;
			}

			return handlingExDataKey;
		}

		public static void HandleSaveException(Exception ex)
		{
			HandleSaveException(ex, NotificationHandler.Instance, null);
		}

		public static void HandleSaveException(Exception ex, ISaveInitiator saveInitiator)
		{
			HandleSaveException(ex, NotificationHandler.Instance, saveInitiator);
		}

		public static void HandleSaveException(Exception ex, INotificationHandler notifier)
		{
			HandleSaveException(ex, notifier, null);
		}

		public static void HandleSaveException(Exception ex, INotificationHandler notifier, ISaveInitiator saveInitiator)
		{
			HandleSaveException(ex, notifier, saveInitiator, ConcurrencyResolver.OnMergeFailureAction.SendReport);
		}

		public static void HandleSaveException(Exception ex, INotificationHandler notifier, bool throwOnMergeFailure)
		{
			HandleSaveException(ex, notifier, null, throwOnMergeFailure ? ConcurrencyResolver.OnMergeFailureAction.ThrowException : ConcurrencyResolver.OnMergeFailureAction.SendReport);
		}

		static void HandleSaveException(Exception ex, INotificationHandler notifier, ISaveInitiator saveInitiator, ConcurrencyResolver.OnMergeFailureAction onMergeFailure)
		{
			if (ex == null)
			{
				throw new ArgumentException("Exception to handle cannot be null");
			}

			var saveException = ex as ZSaveException;
			if (saveInitiator != null && saveException != null && SaveConstraintExceptionHandler.Handle(saveException, saveInitiator))
			{
				return;
			}
			else if (!string.IsNullOrEmpty(saveException?.FriendlyMessage))
			{
				HandleZSaveException(saveException, notifier, onMergeFailure);
			}
			else if (ex is ZSaveConcurrencyException saveConcurrencyException)
			{
				HandleZSaveConcurrencyException(saveConcurrencyException, notifier, onMergeFailure);
			}
			else if (ex is ZCannotSaveException cannotSaveException)
			{
				var message = cannotSaveException.Message;
				if (ex is IHasErrorReportID devEx && !string.IsNullOrEmpty(devEx.ErrorReportID))
				{
					message += Environment.NewLine + Environment.NewLine + Res.GetString("bf1f688b-cd26-4902-b4b5-6692165a986c", "This has been reported using Exception ID: {0}", devEx.ErrorReportID);
				}

				notifier.ReportError(message, cannotSaveException.Heading);
			}
			else if (ex is SqlException sqlException)
			{
				var dbErrorMatch = new DbErrorMatch(sqlException);
				switch (dbErrorMatch.ExceptionType)
				{
					case DbErrorType.ModuleBeingExecutedIsNotTrusted:
					case DbErrorType.ErrorLoadingUntrustedAssembly:
						HandleSqlLoadingUntrustedAssemblyException(sqlException, notifier);
						break;
					case DbErrorType.LoopbackLinkedServerDoesNotExist:
						HandleSqlServerMisconfigurationException(sqlException, dbErrorMatch, notifier);
						break;
					default:
						throw new RethrownByExceptionHandlerException(ex);
				}
			}
			else
			{
				throw new RethrownByExceptionHandlerException(ex);
			}
		}

		static void HandleSqlServerMisconfigurationException(SqlException ex, DbErrorMatch dbErrorMatch, INotificationHandler notifier)
		{
			var message = dbErrorMatch.GetUserFriendlyMessage(Db.Connection);
			var caption = Res.GetString("EC6EAE6F-44E8-4332-85AF-711490FAF194", "Server Misconfiguration Error");
			notifier.ReportError(message, caption, null, ex);
		}

		static void HandleSqlLoadingUntrustedAssemblyException(SqlException sqlException, INotificationHandler notifier)
		{
			using (var adminConn = Db.NewAdminConnection())
			{
				DataUtils.SetTrustworthyOn(adminConn, Db.DatabaseName);
			}

			var message = Res.GetString("31585C5B-CCE4-4337-B401-B3CCAD3D2D95", "A fix has been applied on loading an untrusted assembly exception, please try your last action again.");
			var caption = Res.GetString("B92A279D-E857-4DE0-AD85-9D27FE0127D3", "SQL Loading Untrusted Assembly Exception");
			var key = Invariant($"{Db.ServerName}-{Db.DatabaseName}-SQL Error: {sqlException.Number}");

			ErrorReporter.ReportOnce(key, message, sqlException);
			notifier.ReportError(message, caption);
		}

		public static void HandleZSaveConcurrencyException(ZSaveConcurrencyException ex, INotificationHandler notifier)
		{
			HandleZSaveConcurrencyException(ex, notifier, true);
		}

		public static void HandleZSaveConcurrencyException(ZSaveConcurrencyException ex, INotificationHandler notifier, bool sendReport)
		{
			HandleZSaveConcurrencyException(ex, notifier, sendReport ? ConcurrencyResolver.OnMergeFailureAction.SendReport : ConcurrencyResolver.OnMergeFailureAction.None);
		}

		static void HandleZSaveConcurrencyException(ZSaveConcurrencyException ex, INotificationHandler notifier, ConcurrencyResolver.OnMergeFailureAction onMergeFailure)
		{
			var resolver = new ConcurrencyResolver(ex, ex.Factory.GetChanges(), notifier);
			resolver.OnMergeFailure = onMergeFailure;
			resolver.Resolve();
		}

		static void HandleZSaveException(ZSaveException ex, INotificationHandler notifier, ConcurrencyResolver.OnMergeFailureAction onMergeFailure)
		{
			var handled = false;

			var uniqueIndexName = ex.InnerException.CoreErrorHandler != null ? ex.InnerException.CoreErrorHandler.IndexNameIfUniqueIndexViolation : null;
			if (!string.IsNullOrEmpty(uniqueIndexName))
			{
				foreach (var bizObj in ex.BusinessObjects)
				{
					var handler = bizObj.UniqueIndexFailureHandlers.FirstOrDefault(h => h.HandledUniqueIndexNames.Contains(uniqueIndexName));
					if (handler != null)
					{
						handler.NotifyUserAndAttemptToResolve(notifier, uniqueIndexName);
						handled = true;
						break;
					}
				}
			}

			if (!handled)
			{
				if (onMergeFailure == ConcurrencyResolver.OnMergeFailureAction.SendReport)
				{
					if (ex.ShouldBeReportedToEDI && !string.IsNullOrEmpty(ex.ExtraDebugInfo))
					{
						ErrorReporter.ReportOnce(ex.ExtraDebugInfo, ex);
					}

					notifier.ReportError(ex.FriendlyMessage, Res.GetString("862a583d-f7e7-49e7-84fa-d2cf7f5dd3be", "Cannot Save..."), null, ex);
				}
				else
				{
					throw new RethrownByExceptionHandlerException(ex);
				}
			}
		}
	}
}
