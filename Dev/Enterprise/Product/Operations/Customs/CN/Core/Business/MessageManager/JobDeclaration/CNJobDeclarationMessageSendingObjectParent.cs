using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<CNJobDeclarationMessageSendingObject>
	{
		public CNJobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		static readonly ImmutableArray<string> ColumnsInOrder = new[]
		{
			CNJobDeclarationMessageSendingObject.Schema.IntelligentDeclarationType,
			CNJobDeclarationMessageSendingObject.Schema.LocalReferenceNumber,
			CNJobDeclarationMessageSendingObject.Schema.DeclarationUnifiedNumber,
			CNJobDeclarationMessageSendingObject.Schema.EntryNumber,
			CNJobDeclarationMessageSendingObject.Schema.MessageTypeDescription,
			CNJobDeclarationMessageSendingObject.Schema.DeclarationTypeDescription,
			CNJobDeclarationMessageSendingObject.Schema.MessageStatusDescription,
			CNJobDeclarationMessageSendingObject.Schema.EntryStatus
		}.ToImmutableArray();

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => base.MessageSendingObjectProperties.Union(new[]
		{
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.IntelligentDeclarationType, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.LocalReferenceNumber, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.EntryNumber, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.DeclarationTypeDescription, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.DeclarationUnifiedNumber, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.MessageStatusDescription, true, 150),
			new MessageSendingObjectProperty(CNJobDeclarationMessageSendingObject.Schema.MessageTypeDescription, true, 150),
		}).Where(x => ColumnsInOrder.Contains(x.PropertyName)).OrderBy(x => ColumnsInOrder.IndexOf(x.PropertyName));

		public ZString ValidateCanSubmit()
		{
			var result = CNSWClientSettingChecker.CheckForDeclarationMessageSending(ParentDeclaration);
			if (result.IsEmpty)
			{
				result = CheckDeniedParty(ParentDeclaration);
			}

			return result;
		}

		public int SendMessages()
		{
			var messages = new List<EDIMessage>();

			var messageManagers = SelectedSendingObjects.Cast<CNJobDeclarationMessageSendingObject>().Where(x => x.Header != null).Select(x => new DeclarationMessageManager(x));
			messages.AddRange(messageManagers.SelectMany(m => m.GenerateMessages()));

			var countOfMessages = messages.Count;
			if (countOfMessages > 0)
			{
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					countOfMessages = 0;
					messages.ForEach(m => m.Delete());
					messageManagers.ForEach(m => m.RollbackOnSavingFailed());
					ZExceptionReporting.HandleSaveException(ex);
				}
			}

			return countOfMessages;
		}

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new CNJobDeclarationMessageSendingObject(header as CusEntryHeader);
		}

		protected override ZString GetAdditionalWarningsCore()
		{
			var result = new ZStringBuilder(base.GetAdditionalWarningsCore());

			var declaration = ParentDeclaration;
			var entriesNotBeenLodged = SelectedSendingObjects.Cast<JobDeclarationMessageSendingObject>().Where(x => !x.Header.HasBeenLodgedAtCustoms).ToArray();
			var deadline = declaration.DeclarationDeadline;
			if (entriesNotBeenLodged.Any() && deadline.IsValid)
			{
				var today = ZDateTime.Today.Date;
				var delayedDays = (today - deadline.Date).Days;

				if (declaration.JE_DateOfArrival.AddMonths(3).Date < today)
				{
					result.AppendLine(Res.GetString("174F1E7E-3C31-47AD-9CDB-D3BCEEAA3658", "The Date of Arrival is more than three months ago."));
				}
				else if (delayedDays > 0)
				{
					entriesNotBeenLodged.ForEach(x =>
					{
						var entry = (CusEntryHeader)x.Header;
						var fee = entry.CalculateDelayedFee(today);
						result.AppendLine(Res.GetString("739E4DFC-6AAF-477A-8B22-FC725680F265", "The declaration of {0} has been delayed for {1} day(s). The estimated fee for that is {2} CNY.", entry.LocalReferenceNumber, delayedDays, fee));
					});
				}
			}

			var jobBranch = declaration.Branch;
			if (jobBranch != null && jobBranch.PK != GlbBranch.CurrentBranch.PK)
			{
				result.AppendLine(Res.GetString("390cc3a0-ceea-4ee7-9b0c-5c2c91773aa3", "The branch of this job is not the current branch. The entry will be declared on behalf of branch '{0}'.", jobBranch.GB_Code));
			}

			foreach (CNJobDeclarationMessageSendingObject sendingObject in SelectedSendingObjects)
			{
				var entry = sendingObject.Header;

				if (!entry.CH_EntryStatus.IsEmpty || JobMessageStatusList.IsAcknowledged(entry.CH_Status))
				{
					result.AppendLine(Res.GetString("f8e78d2a-9a43-40cc-b84d-9bac100fd442", "{0} has already be acknowledged by China Customs. Are you sure that you want to resend it?", entry.LocalReferenceNumber));
				}
				else if (JobMessageStatusList.IsAwaiting(entry.CH_Status))
				{
					result.AppendLine(Res.GetString("0f88e37e-da2d-4076-a7ee-7596087ca783", "{0} is waiting for response. Are you sure that you want to resend it?", entry.LocalReferenceNumber));
				}
			}
			return result.ToString();
		}

		ZString CheckDeniedParty(JobDeclaration dec)
		{
			var creditCheckManager = new MessageManagerCreditCheckWithSecurityHelper(dec);
			return creditCheckManager.IsDeniedPartyOKToSend ? string.Empty : creditCheckManager.ReasonForNotAllowed;
		}
	}
}
