using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class QueueReasonCodeSet : CodeSet
	{
		public QueueReasonCodeSet(string queueName)
		{
			this.QueueName = queueName;
		}

		public readonly string QueueName;

		public QueueStatusCodeSet GetStatusCodeSet(string reasonCode)
		{
			QueueStatusCodeSet result = (QueueStatusCodeSet)StatusCodeSets[reasonCode];
			if (result == null)
			{
				result = new QueueStatusCodeSet(reasonCode);
				StatusCodeSets[reasonCode] = result;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public ZQuery GetMultipleCodeFilter(SchemaStringColumn queueNameColumn, SchemaStringColumn queueReasonColumn, SchemaStringColumn queueStatusColumn)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(queueNameColumn, QueueName);

			ZQuery queueNameFilter = new ZQuery();
			foreach (ZString code in this)
			{
				QueueStatusCodeSet statusCodeSet = GetStatusCodeSet(code);
				ZQuery statusFilter = statusCodeSet.GetMultipleCodeFilter(queueReasonColumn, queueStatusColumn);
				queueNameFilter.AddToFilter(statusFilter, JoinCondition.Or);
			}

			result.AddToFilter(queueNameFilter, JoinCondition.And);
			return result;
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			foreach (QueueStatusCodeSet codeSet in StatusCodeSets.Values)
			{
				codeSet.Clear();
			}
		}

		readonly Hashtable StatusCodeSets = new Hashtable();
	}
}
