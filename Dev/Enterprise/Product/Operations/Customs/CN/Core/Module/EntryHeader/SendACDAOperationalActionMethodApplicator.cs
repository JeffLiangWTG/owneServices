using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CN.Module
{
	public class SendACDAOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public SendACDAOperationalActionMethodApplicator(BusinessObjectFactory factory) : base((NoResString)"Send ACDA Message", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			var sender = new AcdAgrMessageSender()
			{
				ShowEnvironmentErrorMessage = (msg) => log.Notify(OperationalActionLogErrorLevel.Error, msg),
				ShowValidationErrorMessage = (msg, entry) => { },
			};

			sender.AskUserToContinueIfSendWithMessageErrors = (msg, entry) =>
			{
				if (!SendWithMessageErrors)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, NotSentDueToMessageErrorsMessage, GetLinkForEntryHeader(entry));
					log.BumpSectionProgress();
				}
				return SendWithMessageErrors;
			};

			sender.AskUserToContinueIfAcdaNumberAlreadyExists = (msg, entry) =>
			{
				if (NotSendIfACDANumberExist)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, NotSentDueToACDAExistsMessage, GetLinkForEntryHeader(entry));
					log.BumpSectionProgress();
				}
				return !NotSendIfACDANumberExist;
			};

			sender.OnMessageCreated = (entry, message) =>
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					message.EM_SendWithMessageErrors ? SendWithMessageErrorsMessage : SentSuccessfullyMessage,
					GetLinkForEntryHeader(entry));
				log.BumpSectionProgress();
			};

			sender.Send(targets.Cast<CusEntryHeader>());
		}

		LogControllerLink GetLinkForEntryHeader(CusEntryHeader entryHeader)
		{
			return new LogControllerLink(entryHeader.HumanReadableName, ControllerIDs.Customs.EntryHeader, entryHeader.PK);
		}

		static readonly MultilingualString NotSentDueToACDAExistsMessage = ResString.GetMultilingualString("2C391F51-4923-4658-B019-7266820236FA", "ACDA Message for {0} was not sent due to ACDA number already exists.");
		static readonly MultilingualString NotSentDueToMessageErrorsMessage = ResString.GetMultilingualString("57C75337-955E-4CD6-A052-50FF935BADAC", "ACDA Message for {0} was not sent due to some message errors.");
		static readonly MultilingualString SendWithMessageErrorsMessage = ResString.GetMultilingualString("4A479BC6-D016-410E-A58D-AE3DA6E35B67", "ACDA Message for {0} was sent with some message errors.");
		static readonly MultilingualString SentSuccessfullyMessage = ResString.GetMultilingualString("4B780957-00F2-439C-8121-A50E0711F3B6", "ACDA Message for {0} was sent successfully.");

		public static class Schema
		{
			public const string NotSendWithMessageErrors = "NotSendWithMessageErrors";
			public const string SendWithMessageErrors = "SendWithMessageErrors";
			public const string NotSendIfACDANumberExist = "NotSendIfACDANumberExist";
		}

		#region Not Send With Message Errors

		public ZBool NotSendWithMessageErrors
		{
			get { return notSendWithMessageErrors; }
			set { SetNonPersistentPropertyValue(NotSendWithMessageErrorsInfo, ref notSendWithMessageErrors, value); }
		}
		ZBool notSendWithMessageErrors = true;

		public ZPropertyInfo NotSendWithMessageErrorsInfo
		{
			get { return GetZPropertyInfo(Schema.NotSendWithMessageErrors); }
		}

		#endregion

		#region Send With Message Errors

		public ZBool SendWithMessageErrors
		{
			get { return sendWithMessageErrors; }
			set { SetNonPersistentPropertyValue(SendWithMessageErrorsInfo, ref sendWithMessageErrors, value); }
		}
		ZBool sendWithMessageErrors;

		public ZPropertyInfo SendWithMessageErrorsInfo
		{
			get { return GetZPropertyInfo(Schema.SendWithMessageErrors); }
		}

		#endregion

		#region Not Send If ACDA Number Exist

		public ZBool NotSendIfACDANumberExist
		{
			get { return notSendIfACDANumberExist; }
			set { SetNonPersistentPropertyValue(NotSendIfACDANumberExistInfo, ref notSendIfACDANumberExist, value); }
		}
		ZBool notSendIfACDANumberExist;

		public ZPropertyInfo NotSendIfACDANumberExistInfo
		{
			get { return GetZPropertyInfo(Schema.NotSendIfACDANumberExist); }
		}

		#endregion
	}
}
