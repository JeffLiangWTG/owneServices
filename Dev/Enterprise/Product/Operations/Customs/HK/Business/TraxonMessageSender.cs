using System;
using System.IO;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.HK.Traxon.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.HK.Business
{
	/// <summary>
	/// Provides interface for sending ISAC (Traxon) messages
	/// </summary>
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class TraxonMessageSender : IProcessor,
		Integration.Customs.HK.IISCProcessor
	{
		/// <summary>
		/// Initializes a new instance of the TraxonMessageSender that will send Traxon messages for the specified ForwardingConsol
		/// </summary>
		/// <param name="consol">The consolidation to send messages</param>
		public TraxonMessageSender(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));
			Status = new TraxonConsolStatus(consol);
		}

		/// <summary>
		/// Gets status of the Traxon message
		/// </summary>
		public readonly TraxonConsolStatus Status;
		public const string SuccessfullyResult = "ISAC message has been sent.";
		public const string SuccessfullyResultWithError = "ISAC message has been sent(probably has error in it).";

		public static class SendingResult
		{
			public const string Success = "SNT";
			public const string Failed = "FAL";
		}

		/// <summary>
		/// Gets validation of the ForwardingConsol for Traxon message sending
		/// </summary>
		public TraxonForwardingConsolValidation Validation => validation ?? (validation = new TraxonForwardingConsolValidation(Status.Consol));

		/// <summary>
		/// Sends or withdraws Traxon message
		/// </summary>
		/// <param name="cancel">if true, Traxon message will be withdrawn</param>
		/// <param name="emailErrorNotification">if true, notification email will be sent about unsuccessful sending of Traxon message</param>
		/// <param name="actionOnValidateSuccess">user interaction when validation success, will be called after generating</param>
		/// <param name="continueOnValidateFail">user interaction when validation fail, if return true, then continue generating, or terminate generating</param>
		/// <returns>Status of the message sending</returns>
		public string SendMessage(bool cancel, bool emailErrorNotification, Action<string> actionOnValidateSuccess = null, Func<string, bool> continueOnValidateFail = null)
		{
			var result = SuccessfullyResult;
			var sendMessageSuccess = false;
			if (!Status.IsWaitingForResponse)
			{
				bool continueSending;
				if (Validation.ValidateAll(out var errors))
				{
					continueSending = true;
				}
				else
				{
					result = string.Join(System.Environment.NewLine, errors);
					continueSending = continueOnValidateFail != null && continueOnValidateFail(result);
				}
				if (continueSending)
				{
					var generator = new TraxonMessageGenerator(Status);
					if (cancel)
					{
						generator.GenerateWithdrawMessages();
					}
					else
					{
						generator.GenerateSendMessages();
					}
					Status.Traxon_MessageStatusInfo.RefreshBinding();
					sendMessageSuccess = true;
					if (actionOnValidateSuccess != null)
					{
						actionOnValidateSuccess(errors.Length == 0 ? SuccessfullyResult : SuccessfullyResultWithError);
					}
				}
				else
				{
					if (emailErrorNotification)
					{
						SendErrorNotification(errors);
					}
				}
			}
			else
			{
				result = "There is a message pending. Please wait for the response before trying to send.";
			}

			InsertSendMessageWorkflowEvent(sendMessageSuccess, result);
			return result;
		}

		void InsertSendMessageWorkflowEvent(bool success, string referenceMessage)
		{
			var consol = Status.Consol;
			var referencePreFix = success ? SendingResult.Success : SendingResult.Failed;
			consol.GetLogs().AddNew(Events.MessageStatusChange, $"{referencePreFix}: {referenceMessage}", new ZDateTimeOffset(ZDateTime.Now),
				false);
		}

		/// <summary>
		/// Sends Traxon message using Traxon registry settings for the company (or branch) whose Organization matches the Receiving or Sending Agent on the Consol
		/// </summary>
		/// <param name="Notifications">Not used</param>
		public void Process(INotifications notifications, CancellationToken token)
		{
			var consol = Status.Consol;
			if (consol.IsInDatabase)
			{
				var branchLoader = new GlbBranch.Loader(consol.Factory);

				GlbBranch branch = null;

				if (consol.GetImportTransport(Core.Constants.CountryCodes.HongKong) != null)
				{
					branch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.ReceivingForwarder, Core.Constants.CountryCodes.HongKong);
					if (branch == null)
					{
						SendErrorNotification(new[] { "Can't find any company or branch whose Organization matches the Receiving Agent on the Consol" });
					}
				}
				else if (consol.GetExportTransport(Core.Constants.CountryCodes.HongKong) != null)
				{
					branch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.SendingForwarder, Core.Constants.CountryCodes.HongKong);
					if (branch == null)
					{
						SendErrorNotification(new[] { "Can't find any company or branch whose Organization matches the Sending Agent on the Consol" });
					}
				}

				if (branch != null)
				{
					if (!TraxonForwardingConsolValidation.IsProcessingISACMessageValid(branch))
					{
						SendErrorNotification(new[] { ISACMessageSendingRegistryConfigError });
					}
					else
					{
						using (branch.SetAsTemporaryContext())
						{
							var status = SendMessage(false, true);

							if (status == SuccessfullyResult)
							{
								ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.Manifest, true, consol.Factory);
							}
						}
					}
				}
			}
		}

		#region Implementation

		TraxonForwardingConsolValidation validation;

		/// <summary>
		/// Sends error notification email to the 'Group to Copy Traxon Response Emails To'
		/// </summary>
		/// <param name="errors">The descriptions of the errors caused message sending failure</param>
		void SendErrorNotification(string[] errors)
		{
			try
			{
				string emailTemplateHtml;
				using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.HK.Business.TraxonMessageErrorEmailTemplate.htm"))
				using (var reader = new StreamReader(stream))
				{
					emailTemplateHtml = reader.ReadToEnd();
					emailTemplateHtml = emailTemplateHtml.Replace("<#console_uri#>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(Status.Consol));
					emailTemplateHtml = emailTemplateHtml.Replace("<#console_name#>", Status.Consol.HumanReadableName);
					emailTemplateHtml = emailTemplateHtml.Replace("<#errors#>", string.Join(System.Environment.NewLine + "<br />" + System.Environment.NewLine, errors));
				}

				var emailSender = new HtmlNotificationEmailSender();
				var email = emailSender.CreateEmail("ISAC Send Message Error", emailTemplateHtml);
				var emailGroup = HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo;
				Env.OutgoingCustomsMailManager.Create(
					Status.Consol.Factory,
					email,
					emailGroup.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.Company.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty),
					GroupSourceLocator.GetFromRegistryItem(emailGroup));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		const string ISACMessageSendingRegistryConfigError = "ISAC Message sending configuration has not been completed in the registry";

		#endregion // Implementation
	}
}
