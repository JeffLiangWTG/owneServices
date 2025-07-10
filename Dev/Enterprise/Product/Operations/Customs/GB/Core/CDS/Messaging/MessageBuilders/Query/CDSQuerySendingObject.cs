using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.UniversalDataBuss.Integration;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query
{
	public abstract class CDSQuerySendingObject : BaseCDSQuerySendingObject
	{
		protected CDSQuerySendingObject()
		{
		}

		protected CDSQuerySendingObject(JobDeclaration jobDeclaration)
		{
			Declaration = jobDeclaration;
		}

		List<EntryNumberSet> SetupEntryNumbers()
		{
			var entryNums = new List<EntryNumberSet>();

			if (Declaration != null)
			{
				foreach (Business.Declaration.CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					entryNums.AddRange(GetEntryNumbers(entry));
				}
			}
			else
			{
				GetEntryNumbers(null);
			}

			return entryNums;
		}

		public Dictionary<ZGuid, ZString> GetMenuOptions()
		{
			var options = new Dictionary<ZGuid, ZString>();

			if (Declaration != null && EntryNumbers.Count > 1)
			{
				foreach (var entryNum in EntryNumbers)
				{
					var entry = GetEntryHeaderCore(entryNum.EntryPK);
					var value = entryNum.Number;
					var caption = Invariant($"{entry.EntryTypeFriendlyName} {(value.IsEmpty ? new ZString(Invariant($"(No {EntryNumberType})")) : value)}");

					options.Add(entryNum.SourcePK, caption);
				}
			}

			return options;
		}

		protected override EntryNumberSet GetEntryNumberSetBySource(ZGuid sourcePK) => EntryNumbers.FirstOrDefault(x => x.SourcePK == sourcePK || sourcePK.IsEmpty);

		public Business.Declaration.CusEntryHeader GetEntryHeader(ZGuid sourcePK)
		{
			var entryPK = sourcePK;
			if (!sourcePK.IsEmpty)
			{
				var entrySet = GetEntryNumberSetBySource(sourcePK);
				entryPK = entrySet?.EntryPK ?? sourcePK;
			}

			return GetEntryHeaderCore(entryPK);
		}

		Business.Declaration.CusEntryHeader GetEntryHeaderCore(ZGuid entryHeaderPK)
		{
			return (Business.Declaration.CusEntryHeader)Declaration?.ActiveEntryHeaders?.FirstOrDefault(x => x.PK == entryHeaderPK || entryHeaderPK.IsEmpty);
		}

		public bool CanSend(ZGuid sourcePK, MessageSendingNotificationCollection notificationCollector)
		{
			bool canSend;
			var checker = new CdsGlbExternalPasswordCheckerWithNotifications(Declaration, notificationCollector);

			if (canSend = checker.PasswordExistsAndOkToSendToCds)
			{
				var entrySet = GetEntryNumberSetBySource(sourcePK);
				if ((entrySet?.Number ?? ZString.Empty).IsEmpty)
				{
					var message = Res.GetString("27CCE7D6-C971-482C-A09C-C0B48321DF99", "This entry does not have a value for its {0}", EntryNumberType);
					notificationCollector.AddError(message);
					canSend = false;
				}
			}

			return canSend;
		}

		protected virtual List<EntryNumberSet> GetEntryNumbers(Business.Declaration.CusEntryHeader entry) => new List<EntryNumberSet>() { new EntryNumberSet(ZGuid.Empty, ZString.Empty) };

		protected override DataContextType GetContextTypeCore() => DataContextType.CustomsDeclaration;

		protected override ZString GetContextReferenceCore() => Declaration?.JobNumber ?? ZString.Empty;

		protected override ZString GetCredentialKeyCore() => Declaration?.GetCredentialsKey() ?? ZString.Empty;

		protected readonly JobDeclaration Declaration;

		List<EntryNumberSet> EntryNumbers => entryNumbers ?? (entryNumbers = SetupEntryNumbers());

		List<EntryNumberSet> entryNumbers;
	}

	public enum QueryNotificationType { Status, Full }

	public class CDSQueryMRNSendingObject : CDSQuerySendingObject
	{
		public CDSQueryMRNSendingObject(JobDeclaration jobDeclaration, QueryNotificationType queryNotificationType) : base(jobDeclaration)
		{
			this.queryNotificationType = queryNotificationType;
		}

		protected override List<EntryNumberSet> GetEntryNumbers(Business.Declaration.CusEntryHeader entry) => new List<EntryNumberSet> { new EntryNumberSet(entry.PK, entry.MovementReferenceNumber) };
		protected override ZString EntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.MRN;
		protected override ZString NotificationType => queryNotificationType == QueryNotificationType.Full
			? (ZString)CDSDISQueryHelper.Constants.NotificationTypes.Full
			: base.NotificationType;

		readonly QueryNotificationType queryNotificationType;
	}

	public class CDSQueryDUCRSendingObject : CDSQuerySendingObject
	{
		public CDSQueryDUCRSendingObject(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		protected override List<EntryNumberSet> GetEntryNumbers(Business.Declaration.CusEntryHeader entry)
		{
			var numbers = new List<EntryNumberSet>();

			if (entry != null)
			{
				foreach (var pd in entry.PreviousDocuments.Where(x => x.CSI_Code == PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr))
				{
					numbers.Add(new EntryNumberSet(entry.PK, pd.CSI_ReferenceNumber));
				}
			}

			return numbers;
		}

		protected override ZString EntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.DUCR;
	}

	public class CDSQueryUCRSendingObject : CDSQuerySendingObject
	{
		public CDSQueryUCRSendingObject(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		protected override List<EntryNumberSet> GetEntryNumbers(Business.Declaration.CusEntryHeader entry) => new List<EntryNumberSet>() { new EntryNumberSet(entry.PK, entry.CH_BGMReference) };
		protected override ZString EntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.UCR;
	}

	public class CDSQueryInventorySendingObject : CDSQuerySendingObject
	{
		public CDSQueryInventorySendingObject(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}
		readonly JobDeclaration jobDeclaration;

		protected override bool UsesUniversalEvent => jobDeclaration.IsImport;
		protected override List<EntryNumberSet> GetEntryNumbers(Business.Declaration.CusEntryHeader entry) => new List<EntryNumberSet>() { new EntryNumberSet(entry.PK, entry.CH_MasterUCR) };
		protected override ZString EntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.Inventory;
	}

	public class CDSQueryListSendingObject : CDSQuerySendingObject
	{
		public CDSQueryListSendingObject(CDSDISQueryMessage queryMessage)
		{
			this.queryMessage = queryMessage;
		}
		readonly CDSDISQueryMessage queryMessage;

		protected override ZString NotificationType => CDSDISQueryHelper.Constants.NotificationTypes.List;
		protected override ZString GetCredentialKeyCore() => $"{GBExtensions.GetEnterpriseCode()}.{queryMessage.EM_MessageOwner}";
		protected override ZString GetQueryStringCore() => GetQueryString();

		protected ZString GetQueryString()
		{
			var reference = queryMessage.EM_ApplicationReference;
			return reference.IsEmpty ? base.GetQueryStringCore() : CDSDISQueryHelper.FormatQueryString(reference);
		}
	}
}
