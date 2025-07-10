using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.ZArchitecture.Business
{
	public class LineProcessTaskHandler : IProcessTaskHandler
	{
		public LineProcessTaskHandler(IStmALogParent parent, IStmALogParent line, IStmALog log, ZString lineTriggerType)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(line, nameof(line));
			Argument.NotNull(log, nameof(log));
			Argument.NotNullOrEmpty(lineTriggerType, nameof(lineTriggerType));
			this.parent = parent;
			this.line = line;
			this.log = log;
			this.lineTriggerType = lineTriggerType;
		}

		readonly IStmALogParent parent;
		readonly IStmALogParent line;
		readonly IStmALog log;
		readonly ZString lineTriggerType;

		public void Fire() => FireLineTriggers(log.SL_EventTimeOffset);
		public void Withdraw() => FireLineTriggers(ZDateTimeOffset.Empty);

		void FireLineTriggers(ZDateTimeOffset dateTime)
		{
			var triggers = ObjectFactory.Get<ITriggerProvider>().LoadAllMilestonesAndLineTriggersForEvent(parent, log, lineTriggerType);
			FireLineTriggers(dateTime, triggers.Select(s => s.trigger), log, line);
		}

		internal static void FireLineTriggers(ZDateTimeOffset value, IEnumerable<IBaseTrigger> triggers, IStmALog logBeingAdded, IStmALogParent targetBusinessObject)
		{
			var triggersMatchingTriggerConditions = triggers.Where(p => p.AreTriggerConditionsMet(logBeingAdded, targetBusinessObject));

			foreach (var trigger in triggersMatchingTriggerConditions)
			{
				if (!EventRecursionHandler.Instance.IsBlockingDuplicateFire(trigger.Identifier, value))
				{
					if (!value.IsEmpty)
					{
						trigger.SetEventTimeWithoutFiringWorkflow(value);
						trigger.Fire(targetBusinessObject, logBeingAdded);
					}
					else
					{
						trigger.Withdraw(targetBusinessObject, logBeingAdded);
					}
				}
			}
		}
	}
}
