using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class TriggerMetaData
	{
		public TriggerMetaData(DeferTriggerAndRunBeforeCommitAttribute attribute, Type bizOType, SchemaColumn column, IEnumerable<ZGuid> values)
		{
			Attribute = Argument.NotNull(attribute, nameof(attribute));
			BizOType = Argument.NotNull(bizOType, nameof(bizOType));
			Column = column;
			Values = Argument.NotNull(values, nameof(values));
		}

		public DeferTriggerAndRunBeforeCommitAttribute Attribute { get; }
		public Type BizOType { get; }
		public SchemaColumn Column { get; }
		public IEnumerable<ZGuid> Values;
	}
}
