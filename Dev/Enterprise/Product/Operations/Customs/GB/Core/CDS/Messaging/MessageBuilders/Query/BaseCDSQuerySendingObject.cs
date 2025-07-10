using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query
{
	public abstract class BaseCDSQuerySendingObject : IQueryDataProvider
	{
		public bool IsUniversalEvent => UsesUniversalEvent;

		protected virtual bool UsesUniversalEvent => true;

		protected class EntryNumberSet
		{
			public EntryNumberSet(ZGuid entryPK, ZString number)
			{
				EntryPK = entryPK;
				SourcePK = ZGuid.NewZGuid();
				Number = number;
			}
			public ZGuid EntryPK { get; }
			public ZGuid SourcePK { get; }
			public ZString Number { get; }
		}

		protected abstract EntryNumberSet GetEntryNumberSetBySource(ZGuid sourcePK);

		protected virtual ZString EntryNumberType { get { return ZString.Empty; } }

		protected virtual ZString NotificationType { get { return CDSDISQueryHelper.Constants.NotificationTypes.Status; } }

		protected abstract DataContextType GetContextTypeCore();

		protected abstract ZString GetContextReferenceCore();

		protected abstract ZString GetCredentialKeyCore();

		protected virtual ZString GetQueryStringCore() => CDSDISQueryHelper.Constants.QueryStringParameters.PartyRole;

		#region IQueryDataProvider

		IEnumerable<Context> IQueryDataProvider.GetContextCollection(ZGuid sourcePK)
		{
			var contexts = new List<Context>();

			var entrySet = GetEntryNumberSetBySource(sourcePK);

			if (entrySet != null)
			{
				var value = entrySet.Number;
				if (!value.IsEmpty)
				{
					contexts.Add(new Context { Type = CDSDISQueryHelper.Constants.ContextTypes.EntryNumberType, Value = EntryNumberType });
					contexts.Add(new Context { Type = CDSDISQueryHelper.Constants.ContextTypes.EntryNumber, Value = value });
				}
			}
			contexts.Add(new Context { Type = CDSDISQueryHelper.Constants.ContextTypes.NotificationType, Value = NotificationType });
			contexts.Add(new Context { Type = CDSDISQueryHelper.Constants.ContextTypes.QueryString, Value = GetQueryStringCore() });

			return contexts;
		}

		DataContextType IQueryDataProvider.ContextType => GetContextTypeCore();

		ZString IQueryDataProvider.ContextReference => GetContextReferenceCore();

		ZString IQueryDataProvider.CredentialKey => GetCredentialKeyCore();

		#endregion
	}
}
