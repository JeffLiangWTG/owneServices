using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public abstract class MessageHandler : IMessageHandler
	{
		protected IeHubMessage Message { get; set; }
		protected GlbCompany Company { get; set; }
		protected INotifications Notifier { get; set; }

		protected MessageHandler()
		{
			FactoryProvider = new BusinessObjectFactoryProvider();
			FactoryProvider.Current.RefreshEnabled = false;
		}

		public event Action<EDIInterchange> InterchangeCreated;
		public event Action<EDIMessage> MessageCreated;
		public event Action<EDIInterchange> BeforeSavingInterchange;

		public bool SaveMessage(IeHubMessage message, GlbCompany company, INotifications notifier)
		{
			Argument.NotNull(message, "message");
			Argument.NotNull(company, "company");
			if (company.Branches == null || company.Branches.Count == 0)
			{
				throw new MessageHandlerException(string.Format("There is no branch for company {0}.", company.GC_Code));
			}

			Message = message;
			Company = company;
			Notifier = notifier;

			var interchange = SaveMessage(BillingDataSource.eHubInbound);
			return interchange != null;
		}

		#region Implementation

		internal protected BusinessObjectFactoryProvider FactoryProvider { get; set; }

		protected virtual bool PrepareToSave()
		{
			return true;
		}

		protected virtual EDIInterchange CreateInterchange()
		{
			var interchange = FactoryProvider.Current.New<XmlEDIInterchange>();

			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = Message.SenderID;
			interchange.EI_To = Message.RecipientID;
			interchange.EI_GB = (Company.FirstActiveBranch ?? Company.Branches[0]).PK;
			interchange.EI_SessionGUID = Message.TrackingID;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_Status = EDIInterchange.Status.Received;

			if (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.Value)
			{
				AddFileNameOnNotes(interchange.Notes);
			}

			InterchangeCreated?.Invoke(interchange);

			return interchange;
		}

		protected virtual EDIMessage CreateEDIMessage(EDIInterchange interchange)
		{
			var message = FactoryProvider.Current.New<XmlEDIMessage>();
			message.EM_IsTestMessage = false;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_EI = interchange.PK;
			message.EM_GB = interchange.EI_GB;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			if (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.Value)
			{
				AddFileNameOnNotes(message.Notes);
			}

			MessageCreated?.Invoke(message);

			return message;
		}

		void AddFileNameOnNotes(Notes notes)
		{
			if (!string.IsNullOrEmpty(Message.Filename))
			{
				notes.AddNew(true, Res.GetString("a4b0fe0d-8070-413f-90a2-3c4c74f3b933", "File Name"), Message.Filename);
			}
		}

		#endregion

		protected virtual EDIInterchange SaveMessage(BillingDataSource dataSource)
		{
			if (!PrepareToSave())
			{
				return null;
			}

			using (FactoryProvider.Current.AddDisposableService())
			{
				var interchange = CreateInterchange();
				if (dataSource == BillingDataSource.eHubInbound && DoesInboundInterchangeExist(interchange))
				{
					Notifier.AddWarning(Res.GetString("6f5f865e-f812-4ae2-a829-27f0998e8aa9", "Duplicate eHub Message received and discarded.  This was most likely caused by the connection to eHub being lost during the download process.  The values of the eHub Tracking ID, From, and To must be unique. The duplicate value(s) are: ({0}, {1}, {2}).", interchange.EI_SessionGUID, interchange.EI_From, interchange.EI_To));
					interchange.Delete();
					return null;
				}

				if (dataSource == BillingDataSource.eAdaptorInbound || dataSource == BillingDataSource.eAdaptorOutbound)
				{
					interchange.EI_TransportType = EDIInterchange.TransportType.eAdaptor;
				}
				else if (dataSource == BillingDataSource.eHubInbound || dataSource == BillingDataSource.eHubOutbound)
				{
					interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
				}
				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_TransportType = interchange.EI_TransportType;
				}

				if (SupportSendingAcknowledgement)
				{
					SendAcknowledgement(interchange, FactoryProvider.Current, Notifier);
				}
				BeforeSavingInterchange?.Invoke(interchange);
				SaveFactory();
				return interchange;
			}
		}

		protected virtual void SendAcknowledgement(EDIInterchange interchange, BusinessObjectFactory factory, INotifications notifier) { }

		protected virtual bool SupportSendingAcknowledgement
		{
			get { return false; }
		}

		internal virtual void SaveFactory()
		{
			FactoryProvider.SaveCurrentAndCreateNew();
		}

		internal string NoteDescriptionFailureLog
		{
			get { return Res.GetString("61ea1838-acc2-4681-bfa5-1e9e16e8f34e", "Failure Log"); }
		}

		internal virtual DbConnection DbConnection
		{
			get { return Db.Connection; }
		}

		bool DoesInboundInterchangeExist(EDIInterchange interchange)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID);
			query.AddToFilter(EDIInterchangeSchema.EI_From, interchange.EI_From);
			query.AddToFilter(EDIInterchangeSchema.EI_To, interchange.EI_To);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			return FactoryProvider.Current.ExistsInDatabase(EDIInterchange.Schema.TableName, query);
		}

		public IMessageHandlerResult SaveMessageFromAdapter(IeHubMessage message)
		{
			if (message.RecipientID.Length != 9)
			{
				throw new MessageHandlerException("RecipientID should be 9 characters in length");
			}

			string companyCode = message.RecipientID.Substring(3, 3);

			var branchCode = string.Empty;

			var company = FactoryProvider.Current.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
			if (company != null)
			{
				var branch = company.FirstActiveBranch;
				if (branch != null)
				{
					branchCode = branch.GB_Code;
				}
			}

			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			using (DisposableEnvironment.ForBranch(branchCode, false, Env.CurrentUser))
			{
				if (GlbCompany.CurrentCompany == null || GlbCompany.CurrentCompany.LicenceKeyIdentifier != message.RecipientID)
				{
					throw new MessageHandlerException(string.Format("The recipient ID is {0}, and it contains the company code {1}. But there is no company with this code found in the database.", message.RecipientID, companyCode));
				}

				var notification = new NotificationBuffer();

				Message = message;
				Company = GlbCompany.CurrentCompany;
				Notifier = notification;

				var interchange = SaveMessage(BillingDataSource.eAdaptorInbound);

				IMessageHandlerResult result;
				if (interchange != null)
				{
					if (interchange.EI_Status != EDIInterchangeStatusList.Codes.Failed)
					{
						result = MessageHandlerResult.Success(interchange);
					}
					else
					{
						var failureLogNote = interchange.Notes.FindByDescription(NoteDescriptionFailureLog)?.MaxBySafe(stmNote => stmNote.ST_CreatedDateUtc)?.ST_NoteDataAsText;
						var failureReason = failureLogNote ?? (ZString)notification.AsString;
						result = MessageHandlerResult.Failure(interchange, failureReason);
					}
				}
				else
				{
					result = MessageHandlerResult.Failure(notification.AsString);
				}

				return result;
			}
		}
	}

	[Serializable]
	public class MessageHandlerException : Exception
	{
		public MessageHandlerException(string errorMessage)
			: base(errorMessage)
		{ }

#if NETFRAMEWORK
		protected MessageHandlerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
