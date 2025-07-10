using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	abstract class MessageStatusHandler : MessageHandler
	{
		protected MessageStatusHandler()
		{
			LockTimeout = TimeSpan.FromMinutes(5);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "For IssueManager only")]
		public const string InterchangeNotFoundErrorKey = "eHub Message Acknowledgement - Interchange not found";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "For IssueManager only")]
		public const string DuplicateInterchangeErrorKey = "eHub Message Acknowledgement - Duplicate Interchange found";

		#region Abstract Members

		protected virtual bool CanUpdateInterchangeStatus(EDIInterchange interchange)
		{
			if (SupportedLogOnlyMatchOutgoingInterchangeStatuses.Contains(interchange.EI_Status.ToString()))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				interchange.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Unable to change status: {0}->{1}", interchange.EI_Status.ToString(), ResultStatus));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				return false;
			}
			return true;
		}

		protected abstract void UpdateInterchangeStatus(EDIInterchange interchange);

		protected abstract string BestMatchOutgoingInterchangeStatus { get; }
		protected abstract string[] SupportedMatchOutgoingInterchangeStatuses { get; }
		protected abstract string[] SupportedLogOnlyMatchOutgoingInterchangeStatuses { get; }
		protected abstract string ResultStatus { get; }
		internal TimeSpan LockTimeout { get; set; }

		#endregion

		protected override EDIInterchange SaveMessage(BillingDataSource dataSource)
		{
			if (!DbConnection.TryGetLock(MutexConstants.MessageMutexPrefix + Message.TrackingID, LockTimeout, out SqlApplicationLock mutex))
			{
				throw new TimeoutException($"MessageStatusHandler failed to acquire interchange lock for TrackingID = '{Message.TrackingID}'");
			}

			EDIInterchange outgoingInterchange = null;
			var factorySaved = false;
			ExceptionAggregation.Using(mutex, () =>
			{
				ExceptionAggregation.ExecuteWithFinally(() =>
				{
					outgoingInterchange = FindOutgoingInterchange(Message.TrackingID);
					if (outgoingInterchange != null)
					{
						if (CanUpdateInterchangeStatus(outgoingInterchange))
						{
							UpdateInterchangeStatus(outgoingInterchange);
						}
						SaveFactory();
						factorySaved = true;
					}
				}, () =>
				{
					if (!factorySaved)
					{
						FactoryProvider.CreateNewWithoutSave();
					}
				});
			});

			return factorySaved ? outgoingInterchange : null;
		}

		#region FindOutgoingInterchange

		internal virtual EDIInterchange FindOutgoingInterchange(Guid trackingId)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, SQLComparisonOperator.Equal, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, SQLComparisonOperator.Equal, BestMatchOutgoingInterchangeStatus);
			var matchingInterchanges = FactoryProvider.Current.Load<EDIInterchange>(query);
			return FindOutgoingInterchange(matchingInterchanges, FindPossibleOutgoingInterchanges);
		}

		EDIInterchange FindOutgoingInterchange(EDIInterchange[] interchanges, Func<EDIInterchange> defaultAction)
		{
			if (interchanges.Length > 0)
			{
				if (interchanges.Length > 1)
				{
					HandleDuplicateOutgoingInterchange(interchanges);
				}

				return interchanges[0];
			}
			else
			{
				return defaultAction();
			}
		}

		EDIInterchange FindPossibleOutgoingInterchanges()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, Message.TrackingID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, SQLComparisonOperator.Equal, EDIInterchange.Direction.Transmit);
			var possibleInterchanges = FactoryProvider.Current.Load<EDIInterchange>(query);
			var supportedInterchanges = possibleInterchanges.Where((i) => SupportedMatchOutgoingInterchangeStatuses.Contains(i.EI_Status.ToString())).ToArray();
			return FindOutgoingInterchange(supportedInterchanges, () =>
			{
				HandlePossibleOutgoingInterchanges(possibleInterchanges);
				return null;
			});
		}

		void HandlePossibleOutgoingInterchanges(EDIInterchange[] possibleInterchanges)
		{
			if (!possibleInterchanges.Any())
			{
				Notifier.AddWarning(Res.GetString("ac0f197f-5c3d-4bdd-9fff-b3121d5c0cc8", "Failed to acknowledge message - Interchange with {0} '{1}' could not be found", ZPropertyInfo.GetFriendlyColumnNameShared(EDIInterchangeSchema.EI_SessionGUID.Name), Message.TrackingID));
			}
			else
			{
				var wrongInterchangesDetail = BuildInterchangeDetails(possibleInterchanges);
				ReportError(
					InterchangeNotFoundErrorKey,
					Res.GetString(
						"d92008dc-9225-4a0c-8b91-20369a90d98b",
						"Failed to acknowledge message - Interchange with {0} '{1}' could not be found, but {2} other(s) with the same {0} exist. Current Status Message Type: {3}, Details: {4}",
						ZPropertyInfo.GetFriendlyColumnNameShared(EDIInterchangeSchema.EI_SessionGUID.Name),
						Message.TrackingID,
						possibleInterchanges.Length,
						GetInterchangeType(),
						wrongInterchangesDetail));
			}
		}

		StringBuilder BuildInterchangeDetails(EDIInterchange[] possibleInterchanges)
		{
			var wrongInterchangesDetail = new StringBuilder();
			foreach (var ediInterchange in possibleInterchanges)
			{
				wrongInterchangesDetail.Append(string.Format("{{{0}: {1}, {2}: {3}}}", ediInterchange.EI_ReceiveTransmitInfo.HumanReadableName, ediInterchange.EI_ReceiveTransmit, ediInterchange.EI_StatusInfo.HumanReadableName, ediInterchange.EI_Status));
			}
			return wrongInterchangesDetail;
		}

		void HandleDuplicateOutgoingInterchange(EDIInterchange[] matchingInterchanges)
		{
			var errorMessage = Res.GetString("D135A10C-4B5A-4E4D-8EA4-18B2BA5A5294", "Duplicate Interchanges found - Interchange with {0} '{1}' was found {2} time(s)",
				matchingInterchanges[0].EI_SessionGUIDInfo.HumanReadableName, Message.TrackingID, matchingInterchanges.Length);
			ReportError(DuplicateInterchangeErrorKey, errorMessage);
		}

		void ReportError(string key, string errorMessage)
		{
			Notifier.Add(new WarningNotification(errorMessage));
			ErrorReporter.ReportOnce(key, errorMessage);
		}

		#endregion

		protected virtual string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.MSS;
		}
	}
}
