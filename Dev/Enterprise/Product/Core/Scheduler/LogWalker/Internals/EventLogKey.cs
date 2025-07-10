using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.LogWalker
{
	sealed class EventLogKey : IEquatable<EventLogKey>
	{
		public EventLogKey(IQueuedLog log)
		{
			keyProperties = new IZType[]
			{
				log.SJ_ParentTableCode,
				log.SJ_SE_NKEvent,
				log.SJ_Reference,
				log.SJ_IsEstimate,
				log.SJ_GS_NKUser,
				log.SJ_ParentID,
			};
		}

		public EventLogKey(StmALog log)
		{
			keyProperties = new IZType[]
			{
				new ZString(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(log.SL_Table)),
				log.SL_SE_NKEvent,
				log.SL_Reference,
				log.SL_IsEstimate,
				log.SL_GS_NKUser,
				log.SL_Parent,
			};
		}

		readonly IZType[] keyProperties;

		public bool Equals(EventLogKey other)
		{
			return keyProperties.SequenceEqual(other.keyProperties);
		}

		public override bool Equals(object obj)
		{
			return (obj as EventLogKey)?.Equals(this) ?? base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return keyProperties.Aggregate(0, (k1, k2) => k1.GetHashCode() ^ k2.GetHashCode());
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} [{1}]", base.ToString(), string.Join(", ", keyProperties.Select(k => k.ToString())));
		}
	}
}
