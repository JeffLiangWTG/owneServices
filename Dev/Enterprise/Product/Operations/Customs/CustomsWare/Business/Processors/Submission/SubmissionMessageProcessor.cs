using System;
using System.Linq;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.CustomsWare.Services;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class SubmissionMessageProcessor
	{
		readonly EDIMessage message;
		readonly BusinessObjectFactory factory;
		readonly LoggingInformation logger;

		public SubmissionMessageProcessor(EDIMessage message, BusinessObjectFactory factory, LoggingInformation logger)
		{
			this.message = message;
			this.factory = factory;
			this.logger = logger;
		}

		public void Process()
		{
			if (message.EM_LinkedObject is BaseJobDeclaration declaration)
			{
				var integration = new CustomsWareIntegrationOutOfLine();
				try
				{
					var submissionResult = new CustomsWareServices().ExecAPI(Settings.Instance, message.EM_MessageText);
					if (integration.SubmitSucceeded(submissionResult))
					{
						message.EM_Status = EDIMessage.Status.Sent;
						declaration.JE_EntryStatus = CustomsWareEntryStatusList.Codes.Submitted;
						declaration.LogCustomsCommencedIfNeeded();
						new MessageDataExportImportLogLinker(Events.DataExport, factory).LinkMessageToParentBOLogs(message, declaration);
					}
					else
					{
						OnFailed(integration.GetSubmitFailedReasons(submissionResult), declaration, false);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					var shouldRetry = false;
					if (IsTransientError(e))
					{
						shouldRetry = message.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription)
							.Where(x => x.ST_NoteDataAsText.StartsWith(SubmitFailurePrefix))
							.Take(RetryCount).Count() < RetryCount;
					}
					else
					{
						ExceptionReporter.Instance.ReportDeveloperException("ba937ffa-c458-4a03-a579-4d1ae6744411"
							, Res.GetString("f65b89e5-a106-4364-b5bc-0ee147c0a6ea", "Not transient error for CustomsWare Submission {0}", declaration.HumanReadableName)
							, e);
					}
					OnFailed(integration.GetSubmitFailedReasons(CustomsWareServices.GetExceptionSubmissionResult(e)), declaration, shouldRetry);
				}
			}
			else
			{
				logger.LogWarning(Res.GetString("89835e3b-d712-4a40-a100-a68caa259412", "Unable to find business object for CustomsWare Submission message (Number:{0}); message status set to ERROR.", message.EM_MessageNum));
				message.EM_Status = EDIMessage.Status.Error;
			}
		}

		static bool IsTransientError(Exception exception) => exception is TimeoutException || exception is CommunicationException;

		string SubmitFailurePrefix => submitFailurePrefix ?? (submitFailurePrefix = Res.GetString("{97A78447-2013-4F92-AF30-ECDC6008B734}", "Web Service Submit Failed:"));
		string submitFailurePrefix;

		void OnFailed(ZString submitFailedReasons, BaseJobDeclaration declaration, bool shouldRetry)
		{
			message.Notes.AddNew(true, InterchangeProviderBase.ProcessingLogDescription, SubmitFailurePrefix + submitFailedReasons);
			if (shouldRetry)
			{
				message.EM_Status = EDIMessage.Status.Pending;
				message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(5);
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				EmailUserFailure(declaration);
			}
		}

		void EmailUserFailure(BaseJobDeclaration declaration)
		{
			var email = new EmailDef
			{
				Subject = Res.GetString("3e2fed50-21ce-40fe-8bae-09c531df1ab4", "Submit {0} Failed", declaration.HumanReadableName),
				Body = GetEmailBody()
			};
			var emailAddress = message.UserWhoQueuedThisRecord?.GS_EmailAddress ?? ZString.Empty;
			if (!emailAddress.IsEmpty)
			{
				email.AddRecipientForUserCommunication(emailAddress, RecipientDef.RecipientTypes.TO);
				Env.OutgoingMailManager.Create(factory, email);
			}
		}

		ZString GetEmailBody()
		{
			var processingLogs = message.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription)
				.OrderBy(x =>
				{
					var createdDateUtc = x.ST_CreatedDateUtc;
					return createdDateUtc.IsEmpty ? ZDateTime.MaxSmallDateTimeUtc : createdDateUtc;
				})
				.Select(x => x.ST_NoteDataAsText).ToArray();
			return ZString.Join(processingLogs);
		}

		const int RetryCount = 3;
	}
}
