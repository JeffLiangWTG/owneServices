using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public class CDSMessageSender : ChiefDeclarationMessageSender
	{
		readonly JobDeclarationMessageSendingObjectParent decWrapper;
		readonly IEnumerable<JobDeclarationMessageSendingObject> objectsToSend;

		public CDSMessageSender(JobDeclarationMessageSendingObjectParent decWrapper)
		{
			this.decWrapper = Argument.NotNull(decWrapper, nameof(decWrapper));
			objectsToSend = decWrapper.ObjectsToSend;
		}

		public void Send(ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var declaration = decWrapper.ParentDeclaration;
			var entriesToSend = objectsToSend.Select(x => x.Header);
			Action action;
			if (decWrapper.LockDeclarationUntilResponseReceived)
			{
				action = () => MessageResponseSemaphoreHelper.CreateSemaphoreForDeclaration(decWrapper.ParentDeclaration);
			}
			else
			{
				action = null;
			}
			SendCore(declaration, entriesToSend, sendMessagesToCustoms, null, () => new CDSTransmissionMessageGenerator(objectsToSend, action));
		}

		internal MessageResponseSemaphoreHelper MessageResponseSemaphoreHelper => messageResponseSemaphoreHelper ??= new MessageResponseSemaphoreHelper();
		MessageResponseSemaphoreHelper messageResponseSemaphoreHelper;

		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction ignore)
		{
			return null;
		}

		protected override bool ValidateForMessageType(Business.Declaration.JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended, MessageSendingNotificationCollection existingNotifications)
		{
			bool canSend = false;

			canSend = WarnIfEntryLineCountsAreDifferent(sendMessagesToCustoms);

			if (canSend && IsNewOrAmended())
			{
				canSend = ValidateFullyForNewOrAmended(declaration, sendMessagesToCustoms, true, existingNotifications);
			}

			if (canSend)
			{
				canSend = CanSendAmendments(sendMessagesToCustoms);
			}

			return canSend;
		}

		bool WarnIfEntryLineCountsAreDifferent(ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			bool canSend = true;
			var entriesToWarnAbout = objectsToSend.Where(x => x.MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration && x.ShouldSend && x.Header.IsAcceptedAndHasMisMatchedLineCount);

			if (entriesToWarnAbout.Any())
			{
				canSend = sendMessagesToCustoms.ContinueWithAction(string.Format(CultureInfo.CurrentCulture, "The following entries have a different line count to when they were accepted by HMRC.\r\n\r\n{0}\r\n\r\nDo you wish to continue?"
																			, string.Join("\r\n", entriesToWarnAbout.Select(x => x.Header.CH_BGMReference)))
																			, "Entry Line Count");
			}

			return canSend;
		}

		bool CanSendAmendments(ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			bool canSend = true;
			var objectsThatCannotBeSent = new List<JobDeclarationMessageSendingObject>();
			var objectsLeftToSend = objectsToSend.Where(x => x.MessageType != CDSEDIMessageTypeList.Codes.AmendDeclaration && x.ShouldSend);
			foreach (var amendmentObjectToSend in objectsToSend.OfType<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend && (x.MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration || x.MessageType == CDSEDIMessageTypeList.Codes.ArrivalNotification)))
			{
				if (!AmendmentMessageHelper.Instance.CanBeAmended(amendmentObjectToSend) && amendmentObjectToSend.IsAmend)
				{
					objectsThatCannotBeSent.Add(amendmentObjectToSend);
				}
			}

			if (objectsThatCannotBeSent.Any())
			{
				var warning = new ZStringBuilder();

				AddWarningForComplexAmendmentsIfNeeded(objectsThatCannotBeSent, warning);

				AddWarningForAmendmentsWithNoDifferences(objectsThatCannotBeSent, warning);

				if (objectsLeftToSend.Any())
				{
					AddWarningsForObjectsStillLeftToBeSent(objectsLeftToSend, warning);

					sendMessagesToCustoms.WarnUserAboutSomething(warning.ToString(), "Warning");
				}
				else
				{
					warning.AppendLine().AppendLine("There are no other entries selected to be sent");
					sendMessagesToCustoms.WarnUserAboutSomething(warning.ToString(), "Error");
					canSend = false;
				}
			}

			return canSend;
		}

		void AddWarningsForObjectsStillLeftToBeSent(IEnumerable<JobDeclarationMessageSendingObject> objectsLeftToSend, ZStringBuilder warning)
		{
			warning.AppendLine().AppendLine("The following entries will still be sent");
			warning.AppendLine().AppendLine(objectsLeftToSend.Select(x =>
			{
				ZString result = x.MessageTypeDescription + " - " + x.LocalReferenceNumber;
				return result;
			}).JoinAsString("\r\n"));
		}

		void AddWarningForAmendmentsWithNoDifferences(List<JobDeclarationMessageSendingObject> objectsThatCannotBeSent, ZStringBuilder warning)
		{
			var objectsThatHaveNoDifferences = objectsThatCannotBeSent.Where(x => (!x.AmendmentDetails?.HasDifferences ?? false) && x.IsAmend);
			if (objectsThatHaveNoDifferences.Any())
			{
				warning.AppendLine("The following entries cannot be amended as they have no differences.");
				warning.AppendLine().AppendLine(objectsThatHaveNoDifferences.Select(x =>
				{
					ZString result = x.MessageTypeDescription + " - " + x.LocalReferenceNumber;
					return result;
				}).JoinAsString("\r\n"));
			}
		}

		void AddWarningForComplexAmendmentsIfNeeded(List<JobDeclarationMessageSendingObject> objectsThatCannotBeSent, ZStringBuilder warning)
		{
			var objectsThatAreTooComplex = objectsThatCannotBeSent.Where(x => x.AmendmentDetails?.HasDifferences ?? false);
			if (objectsThatAreTooComplex.Any())
			{
				warning.AppendLine("The following entries cannot be amended, please cancel and re-submit.  Please refer to learning unit 1BGB048");
				warning.AppendLine().AppendLine(objectsThatCannotBeSent.Select(x =>
				{
					ZString result = x.MessageTypeDescription + " - " + x.LocalReferenceNumber;
					return result;
				}).JoinAsString("\r\n"));
			}
		}

		bool IsNewOrAmended()
		{
			return objectsToSend.Any(x => x.MessageType.EqualsAny(new ZString[] { CDSEDIMessageTypeList.Codes.NewDeclaration, CDSEDIMessageTypeList.Codes.AmendDeclaration }));
		}

		public override bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var isDirectToCdsWithoutCsp = declaration.ZG_Gateway == GatewayList.Codes.CDS;
			if (isDirectToCdsWithoutCsp)
			{
				// No need for creds in registry, but need to look for GlbExternalPassword
				return new CdsGlbExternalPasswordChecker(declaration, sendMessagesToCustoms).PasswordExistsAndOkToSendToCds;
			}
			else
			{
				// CSP - need to check creds exist, but don't care about lockout
				return new Chief.GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, -1, declaration.ZG_Gateway);
			}
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new[] { "EHO", "EHI", CDSServiceTaskConstants.CDSMessageRetrieverServiceTaskCode, CDSServiceTaskConstants.CDSMessageSenderServiceTaskCode };
		}

		protected override JobDeclarationMessageManager GetDeclarationManager(IMessageGenerator<EU.Business.Declaration.CusEntryHeader> transmissionGenerator, JobDeclaration declaration)
		{
			return new CDSGBJobDeclarationMessageManager(declaration, (CDSTransmissionMessageGenerator)transmissionGenerator);
		}

		protected override bool CanEntryParticipateEnhancedValidation(Business.Declaration.CusEntryHeader entry)
		{
			return objectsToSend.FirstOrDefault(x => x.Header.PK == entry.PK)?.CanParticipateEnhancedValidation ?? false;
		}
	}
}
