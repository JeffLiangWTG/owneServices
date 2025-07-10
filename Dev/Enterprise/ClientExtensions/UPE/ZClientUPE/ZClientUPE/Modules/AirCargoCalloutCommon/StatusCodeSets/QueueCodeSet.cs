using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class QueueCodeSet : CodeSet
	{
		public QueueReasonCodeSet GetReasonCodeSet(string queueName)
		{
			QueueReasonCodeSet result = (QueueReasonCodeSet)ReasonCodeSets[queueName];
			if (result == null)
			{
				result = new QueueReasonCodeSet(queueName);
				ReasonCodeSets[queueName] = result;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public ZQuery GetMultipleCodeFilter(SchemaStringColumn queueNameColumn, SchemaStringColumn queueReasonColumn, SchemaStringColumn queueStatusColumn)
		{
			ZQuery result = new ZQuery();
			foreach (ZString queueCode in this)
			{
				QueueReasonCodeSet reasonCodeSet = GetReasonCodeSet(queueCode);
				ZQuery reasonFilter = reasonCodeSet.GetMultipleCodeFilter(queueNameColumn, queueReasonColumn, queueStatusColumn);
				result.AddToFilter(reasonFilter, JoinCondition.Or);
			}
			return result;
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			foreach (QueueReasonCodeSet codeSet in ReasonCodeSets.Values)
			{
				codeSet.Clear();
			}
		}

		readonly Hashtable ReasonCodeSets = new Hashtable();
	}
}
