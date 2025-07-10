using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal sealed class LockedMessageBatch : IDisposable
	{
		IDisposable lockKeeper;
		readonly ZGuid[] messagePKs;

		public LockedMessageBatch(IReadOnlyList<ZGuid> messagePKs, IDisposable lockKeeper)
		{
			this.messagePKs = messagePKs.ToArray();
			this.lockKeeper = lockKeeper;
		}

		public void Dispose()
		{
			lockKeeper?.Dispose();
			lockKeeper = null;
		}

		public EDIMessage[] LoadMessages(BusinessObjectFactory factory)
		{
			if (messagePKs.Length == 0)
			{
				return Array.Empty<EDIMessage>();
			}

			var query = new ZQuery(EDIMessageSchema.PK, messagePKs);
			query.IncludeBlob(EDIMessageSchema.EM_MessageText);

			var messages = factory.Load<EDIMessage>(query);
			var messagesIndex = messages.ToDictionary(m => m.PK);
			return messagePKs
				.Select(pk => messagesIndex.TryGetValue(pk, out var message) ? message : null)
				.Where(m => m != null) // message no longer exist in the database
				.ToArray();
		}
	}
}
