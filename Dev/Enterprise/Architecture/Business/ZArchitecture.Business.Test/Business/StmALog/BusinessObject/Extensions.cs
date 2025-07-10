using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class Extensions
	{
		public static BusinessObjectFactory Factory;

		public static Event WithReferenceFormat(this Event evnt, string referenceFormat, params object[] arguments)
		{
			var stmEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, evnt.Code);
			stmEvent.SE_ReferenceFormat = string.Format(referenceFormat, arguments);

			return evnt;
		}

		public static Event WithOverriddenReferenceFormat(this Event evnt, string referenceFormat, params object[] arguments)
		{
			var stmEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, evnt.Code);
			stmEvent.SE_OverriddenReferenceFormat = string.Format(referenceFormat, arguments);
			stmEvent.SE_IsRefernceFormatOverridden = true;

			return evnt;
		}

		public static StmALog SetEventCodeWithLock(this StmALog log, Event evnt)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = evnt.Code;
			}

			return log;
		}

		public static StmALog SetReferenceWithLock(this StmALog log, string reference, params object[] arguments)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = ZString.Format(reference, arguments);
			}

			return log;
		}

		public static StmALog SetReferenceFreeTextWithLock(this StmALog log, string reference)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.ReferenceFreeText = reference;
			}

			return log;
		}

		public static StmALog Set(this StmALog log, KeyValuePair<string, string> pair)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Parameters[pair.Key] = pair.Value;
			}

			return log;
		}
	}
}
