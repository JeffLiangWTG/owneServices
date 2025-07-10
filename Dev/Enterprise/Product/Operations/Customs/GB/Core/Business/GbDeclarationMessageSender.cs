using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Business
{
	public abstract class GbDeclarationMessageSender : IGBDeclarationMessageSender
	{
		public virtual void Send(BaseJobDeclaration baseDeclaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			if (baseDeclaration is Declaration.JobDeclaration declaration)
			{
				var entriesToSend = declaration.CustomsEntryHeaders.OfType<EU.Business.Declaration.CusEntryHeader>();
				SendCore(declaration, entriesToSend, sendMessagesToCustoms, how, () => GetTransmissionGenerator(how));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SendCore(JobDeclaration declaration
			, IEnumerable<EU.Business.Declaration.CusEntryHeader> entriesToSend
			, ISendsMessagesToCustoms sendMessagesToCustoms
			, CusdecMessageFunction how
			, Func<IMessageGenerator<EU.Business.Declaration.CusEntryHeader>> transmissionGeneratorGetter)
		{
			if (declaration != null)
			{
				bool isOKToSend = CanSendMessage(declaration, entriesToSend, sendMessagesToCustoms, how);
				if (isOKToSend && (declaration.ActiveEntryHeaders.Count == 0 || declaration.MergeManager.RequiresMerge))
				{
					isOKToSend = declaration.DoMerge(sendMessagesToCustoms);
				}

				if (isOKToSend && GetEnhancedValidationParticipation(declaration, entriesToSend, sendMessagesToCustoms))
				{
					GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
					var transmissionGenerator = transmissionGeneratorGetter?.Invoke();
					if (transmissionGenerator != null)
					{
						var gbTransmissionGenerator = ((GbTransmissionMessageGenerator)transmissionGenerator);

						if (gbTransmissionGenerator != null)
						{
							gbTransmissionGenerator.SendMessagesToCustoms = sendMessagesToCustoms;
						}

						var manager = GetDeclarationManager(transmissionGenerator, declaration);

						using (IsAutoSendCustomsMessageProcessor ? new MessageManagerFactorySaveSuspender(manager) : null)
						{
							declaration.SendMessageWithBondedWarehouseAutomation(() => DeclareDeclaration(manager, sendMessagesToCustoms), GetMessageAction(how), reportHasChanges: false);
						}
					}
				}
			}
		}

		MessageAction GetMessageAction(CusdecMessageFunction how)
		{
			switch (how)
			{
				case CusdecMessageFunction.Amended _:
					return MessageAction.Amendment;
				case CusdecMessageFunction.Deleted _:
					return MessageAction.Withdrawal;
				default:
					return MessageAction.Original;
			}
		}

		bool DeclareDeclaration(JobDeclarationMessageManager manager, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return manager.DeclareDeclaration(sendMessagesToCustoms);
		}

		protected void GetRequiredServiceTasksAndWarnIfNotRunning(ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var (listOfBadTasks, wasCheckFailedDueToHostNotRunning) = GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
			if (listOfBadTasks.Count > 0 && !wasCheckFailedDueToHostNotRunning)
			{
				string humanListOfBadTask = System.Environment.NewLine + string.Join(System.Environment.NewLine, listOfBadTasks.ToArray());
				sendMessagesToCustoms.WarnUserAboutSomething("Not all the required service tasks are currently running. The message will be queued, but until the service tasks are all started it may not be sent. Please have your administrator start/activate tasks with these codes:"
										+ humanListOfBadTask, "Required service task(s) not running.");
			}
			else if (wasCheckFailedDueToHostNotRunning)
			{
				sendMessagesToCustoms.WarnUserAboutSomething("Not all the required service tasks are currently running. The message will be queued, but until the service tasks are all started it may not be sent. The process controller was reported to be not found or not running.",
					"Required service(s) not running.");
			}
		}

		public abstract IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction);

		public virtual bool CanSendMessage(JobDeclaration declaration, IEnumerable<EU.Business.Declaration.CusEntryHeader> entryCollection, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			if (!AnyEntryHeadersAreFailed(entryCollection) && AreAnyHeadersAwaitingAResponse(entryCollection))
			{
				bool shouldAbortSendingCosWeAreStillAwaitingResponse = true;
#if DEBUG
				if (true)
				{
					if (sendMessagesToCustoms.YesNoQuery(" ** DEBUG ONLY ** \n\nThis entry is awaiting response. \nIf you want to continue waiting (like a chump) or to change its status by setting it to 'failed from transmission' (like a user would have to), click no. \nIf you want to send regardless, you luckly thing, click yes.\n\n\nDo you want to send even though you're awaiting a response?", "DEBUG ONLY - send even though you're awaiting a response?"))
					{
						shouldAbortSendingCosWeAreStillAwaitingResponse = false;
					}
				}
#endif

				if (shouldAbortSendingCosWeAreStillAwaitingResponse)
				{
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation("A message cannot be sent while there are still queued messages for this declaration.\nIf the transmission failed and you wish to resend, go to the 'Entries' tab, select the entry, right-click in its gutter and set it as 'failed from transmission.'");
					return false;
				}
			}

			return ValidateAndShowUserAnyWarningsOrErrors(declaration, sendMessagesToCustoms, functionNewDeletedAmended);
		}

		protected abstract bool ValidateAndShowUserAnyWarningsOrErrors(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended);

		public virtual bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return true;
		}

		public static bool AreAnyHeadersAwaitingAResponse(IEnumerable<EU.Business.Declaration.CusEntryHeader> cusEntryHeaderCollection)
		{
			foreach (EU.Business.Declaration.CusEntryHeader header in cusEntryHeaderCollection)
			{
				var lastMessage = header.Messages.GetLastMessage(ZString.Empty, ZString.Empty, EDIMessage.Direction.Transmit, Array.Empty<ZString>(), Array.Empty<ZString>(), new ZString[] { EDIMessage.Status.Discarded }, Array.Empty<ZString>());
				if (lastMessage != null
					&& lastMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
					&& lastMessage.EM_Status != EDIMessage.Status.Rejected
					&& lastMessage.EM_Status != EDIMessage.Status.Cancelled
					&& lastMessage.EM_Status != EDIMessage.Status.Acknowledged
					&& lastMessage.EM_Status != EDIMessage.Status.Received
					&& !(lastMessage.AssumeMessageClearIfAcknowledgedAndNoResponse && lastMessage.HasHadItsInterchangeAcknowledged)
					)
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool AnyEntryHeadersAreFailed(IEnumerable<EU.Business.Declaration.CusEntryHeader> cusEntryHeaderCollection)
		{
			foreach (EU.Business.Declaration.CusEntryHeader entryHeader in cusEntryHeaderCollection)
			{
				if (entryHeader.CH_Status == Enterprise.Customs.Common.EU.MessageStatusList.Codes.FailedFromTransmission)
				{
					return true;
				}
			}
			return false;
		}

		protected (List<string> badTasks, bool wasCheckFailedDueToHostNotRunning) GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState()
		{
			var wasCheckFailedDueToHostNotRunning = false;
			var codesToCheck = GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending();
			var badTasks = new List<string>();

			foreach (var oneCode in codesToCheck)
			{
				var status = ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(oneCode);
				if (status != ServiceTaskStatus.AtLeastOneHostIsRunningHealthily)
				{
					badTasks.Add(oneCode);
				}
				if (status == ServiceTaskStatus.NoAvailableHosts)
				{
					wasCheckFailedDueToHostNotRunning = true;
					break;
				}
			}
			return (badTasks, wasCheckFailedDueToHostNotRunning);
		}

		public abstract string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending();

		protected virtual JobDeclarationMessageManager GetDeclarationManager(IMessageGenerator<EU.Business.Declaration.CusEntryHeader> transmissionGenerator, JobDeclaration declaration)
		{
			return new GBJobDeclarationMessageManager(declaration, transmissionGenerator);
		}

		protected bool GetEnhancedValidationParticipation(JobDeclaration declaration, IEnumerable<EU.Business.Declaration.CusEntryHeader> entriesToSend, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var @continue = true;
			if (declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
				&& sendMessagesToCustoms is IEnhancedValidation enhancedValidation)
			{
				var entriesToShowEnhancedValidation = entriesToSend.Cast<Declaration.CusEntryHeader>().Where(CanEntryParticipateEnhancedValidation);
				EnhancedValidationParticipationHelper.EnhancedValidationParticipation((Declaration.JobDeclaration)declaration, entriesToShowEnhancedValidation).ForEach(header =>
				{
					if (@continue)
					{
						@continue = enhancedValidation.ShowEnhancedValidation(new EnhancedValidationEntryWrapper(header));
					}
				});
			}
			return @continue;
		}

		protected virtual bool CanEntryParticipateEnhancedValidation(Declaration.CusEntryHeader entry) => false;

		public bool IsAutoSendCustomsMessageProcessor { get; set; }
	}
}
