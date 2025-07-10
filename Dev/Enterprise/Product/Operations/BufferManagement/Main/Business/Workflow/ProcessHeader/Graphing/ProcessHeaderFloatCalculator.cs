using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderFloatCalculator : FloatCalculatorBase
	{
		public static ProcessHeaderFloatCalculator CreateCalculator(ProcessJobHeader jobHeader, BMComponent buffer, bool includeClosedTaskTimes = true)
		{
			if (!buffer.IsBuffer)
			{
				throw new InvalidOperationException("Component must be a buffer");
			}

			return CreateCalculator(jobHeader, jobHeader.ProcessHeaders, WorkingTimeContext.Create(buffer), TimeSpan.FromMinutes(buffer.FC_BufferTimespanInMinutes), includeClosedTaskTimes);
		}

		public static ProcessHeaderFloatCalculator CreateCalculator(ProcessJobHeader jobHeader, WorkingTimeContext context = null, bool includeClosedTaskTimes = true)
		{
			return CreateCalculator(jobHeader, jobHeader.ProcessHeaders, context, TimeSpan.Zero, includeClosedTaskTimes);
		}

		public static ProcessHeaderFloatCalculator CreateCalculator(ProcessJobHeader jobHeader, IEnumerable<ProcessHeader> processHeaders, WorkingTimeContext context, TimeSpan deliveryDateThreatAdditionalTime, bool includeClosedTaskTimes)
		{
			if (jobHeader.DependencyGraph.IsDirectedAcyclicGraph())
			{
				var network = ProcessHeaderGraphProvider.CreateGraph(jobHeader, processHeaders, includeClosedTaskTimes);
				var rootSchedule = network.SchedulesByEntity[jobHeader.PK];
				return new ProcessHeaderFloatCalculator(rootSchedule, network, jobHeader.Factory, context, deliveryDateThreatAdditionalTime);
			}

			return null;
		}

		ProcessHeaderFloatCalculator(ScheduleNode root, ScheduleGraph scheduleScope, BusinessObjectFactory factory, WorkingTimeContext context, TimeSpan deliveryDateThreatAdditionalTime)
			: base(root, scheduleScope, context, factory)
		{
			this.deliveryDateThreatAdditionalTime = deliveryDateThreatAdditionalTime;
		}

		readonly TimeSpan deliveryDateThreatAdditionalTime;

		protected override double GetLatestFinishHoursFromNow(ScheduleNode schedule)
		{
			return base.GetLatestFinishHoursFromNow(schedule) + deliveryDateThreatAdditionalTime.TotalHours;
		}
	}
}
