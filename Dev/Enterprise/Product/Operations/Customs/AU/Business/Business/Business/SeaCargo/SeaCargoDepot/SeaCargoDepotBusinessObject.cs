using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCargoDepotBusinessObject : BusinessObjectWrapper, IObsoleteValidation
	{
		#region Constants

		#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
		public class Schema
		{
			public const string MessageStateText = "MessageStateText";
			public const string LastErrorMessage = "LastErrorMessage";
		}
		#pragma warning restore CA1052
		#endregion

		public SeaCargoDepotBusinessObject(BusinessObject bizO)
			: base(bizO)
		{
			if (bizO == null)
			{
				throw new ArgumentNullException(nameof(bizO));
			}
		}

		#region Properties

		[CargoWise.ComponentModel.MaxLength(128)]
		public virtual ZString MessageStateText
		{
			get
			{
				ZString result = "Not Registered";
				StmALog lastMessageStateTextEvent = LastSuccessfulEvent;
				if (lastMessageStateTextEvent != null)
				{
					result = lastMessageStateTextEvent.SL_Reference;
				}
				return result;
			}
		}

		public ZPropertyInfo MessageStateTextInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStateText); }
		}

		public virtual DepotState MessageState
		{
			get
			{
				DepotState result = DepotState.Unknown;
				StmALog lastMessageEvent = LastSuccessfulEvent;
				if (lastMessageEvent != null)
				{
					result = GetDepotStateFromEventReference(LastSuccessfulEvent.SL_Reference);
				}
				return result;
			}
		}

		StmALog LastSuccessfulEvent
		{
			get
			{
				int errorEvents = 0;
				foreach (StmALog depotEvent in DepotLog)
				{
					if (depotEvent.SL_Reference.StartsWith(DepotEvents.CargoArrivedError)
						|| depotEvent.SL_Reference.StartsWith(DepotEvents.CargoArrivedError)
						|| depotEvent.SL_Reference.StartsWith(DepotEvents.CargoArrivedError))
					{
						errorEvents++;
					}
					else
					{
						if (errorEvents > 0)
						{
							errorEvents--;
						}
						else
						{
							return depotEvent;
						}
					}
				}
				return null;
			}
		}

		public DepotState State
		{
			get
			{
				DepotState result = DepotState.Unknown;
				foreach (StmALog depotEvent in DepotLog)
				{
					result = GetDepotStateFromEventReference(depotEvent.SL_Reference);
					break;
				}
				return result;
			}
		}

		public virtual EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(WrappedBusinessObject, Factory);
					fMessages.Load();
				}
				return fMessages;
			}
		}

		[CargoWise.ComponentModel.MaxLength(1024)]
		public ZString LastErrorMessage
		{
			get
			{
				return fLastErrorMessage;
			}
			set
			{
				CheckMaximumLength(LastErrorMessageInfo, value); // you are likely missing this line!
				SetNonPersistentPropertyValue(LastErrorMessageInfo, ref fLastErrorMessage, value);
			}
		}

		public ZPropertyInfo LastErrorMessageInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.LastErrorMessage);
			}
		}

		public abstract CFSLoadListConsol ParentConsol { get; }

		protected DepotState GetDepotStateFromEventReference(ZString reference)
		{
			if (reference.StartsWith(DepotEvents.ImpendingCargo))
			{
				return DepotState.ImpendingCargo;
			}
			else if (reference.StartsWith(DepotEvents.ImpendingCargoCancelled))
			{
				return DepotState.ImpendingCargoCancelled;
			}
			else if (reference.StartsWith(DepotEvents.CargoArrived))
			{
				return DepotState.CargoArrived;
			}
			else if (reference.StartsWith(DepotEvents.CargoArrivedError))
			{
				return DepotState.CargoArrivedError;
			}
			else if (reference.StartsWith(DepotEvents.CargoUnpacked))
			{
				return DepotState.CargoUnpacked;
			}
			else if (reference.StartsWith(DepotEvents.CargoUnpackedError))
			{
				return DepotState.CargoUnpackedError;
			}
			else if (reference.StartsWith(DepotEvents.CargoDelivered))
			{
				return DepotState.CargoDelivered;
			}
			else if (reference.StartsWith(DepotEvents.CargoDeliveredError))
			{
				return DepotState.CargoDeliveredError;
			}
			else if (reference.StartsWith(DepotEvents.CargoStatusAdviceClear))
			{
				return DepotState.CargoClear;
			}
			return DepotState.Unknown;
		}

		#endregion

		#region Implementation

		EDIMessageCollection fMessages;
		StmALogCollection fDepotLog;
		ZString fLastErrorMessage;

		protected StmALogCollection DepotLog
		{
			get
			{
				if (fDepotLog == null)
				{
					ZQuery depotEventsFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SeaCargoDepotEvent.Code);
					depotEventsFilter.AddToFilter(StmALogSchema.SL_Parent, WrappedBusinessObject.PK);
					depotEventsFilter.OrderBy = StmALog.Schema.SL_EventTime + " DESC";
					fDepotLog = new StmALogCollection(Factory, depotEventsFilter);
					fDepotLog.Load();
					WrappedBusinessObject.GetLogs().GetAllLogs().CountChanged += new CollectionCountChangedEventHandler(VisibleLogs_CountChanged);
				}
				return fDepotLog;
			}
		}

		void VisibleLogs_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				StmALog @event = e.BizObject as StmALog;
				if (@event != null && @event.SL_SE_NKEvent == Events.SeaCargoDepotEvent.Code)
				{
					DepotLog.Add(@event);
					DepotLog.Sort(StmALog.Schema.SL_EventTime, System.ComponentModel.ListSortDirection.Descending);
				}
			}
		}

		protected bool CanSendDepotMessage()
		{
			LastErrorMessage = "";
			if (SeaCargoSenderIdRetriever.GetSenderID(Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch, ((ILinkable)this).LinkTableName).IsEmpty)
			{
				LastErrorMessage += "Sender mailbox has not been configured for the current company/branch.  Please set this up in <Current Branch>->AUCustoms->Sea Cargo Depot Mailbox\r\n";
			}
			return LastErrorMessage.Length == 0;
		}

		#region Logging Events

		protected void LogCancelImpendingArrival()
		{
			WrappedBusinessObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled, ZDateTimeOffset.Now);
			DepotLog.Load();
			MessageStateTextInfo.RefreshBinding();
		}

		protected void LogImpendingArrival()
		{
			WrappedBusinessObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.ImpendingCargo, ZDateTimeOffset.Now);
			DepotLog.Load();
			MessageStateTextInfo.RefreshBinding();
		}

		protected void LogCancelCargoStatusAdvice()
		{
			WrappedBusinessObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, "CANCEL " + DepotEvents.CargoStatusAdviceClear, ZDateTimeOffset.Now);
			DepotLog.Load();
			MessageStateTextInfo.RefreshBinding();
		}

		protected void LogCargoStatusAdvice()
		{
			WrappedBusinessObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceClear, ZDateTimeOffset.Now);
			DepotLog.Load();
			MessageStateTextInfo.RefreshBinding();
		}

		protected ZString GetConditionsOfReleaseFromRelatedMessage(ZGuid messagePK)
		{
			ZString result = null;
			var message = Factory.Load<EDIMessage>(messagePK);
			if (message != null)
			{
				var conditionsLocation = message.EM_MessageText.IndexOf("+CRC:");
				if (conditionsLocation != -1)
				{
					var conditions = message.EM_MessageText.SubstringSafe(conditionsLocation + 5, 2);
					if (conditions.IndexOf("Q") != -1)
					{
						result = "Quarantined";
					}
					if (conditions.IndexOf("D") != -1)
					{
						if (result.Length > 0)
						{
							result += ", ";
						}

						result += "Documentary Clearance";
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

	}
}
