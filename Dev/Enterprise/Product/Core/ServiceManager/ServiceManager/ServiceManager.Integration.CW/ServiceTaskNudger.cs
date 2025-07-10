using System;
using System.Diagnostics;
using CargoWise.Application;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.CW
{
	class ServiceTaskNudger : IServiceTaskNudger
	{
		readonly INudgingController nudgingController;

		public ServiceTaskNudger() : this(ObjectFactory.Get<INudgingController>()) { }

		internal ServiceTaskNudger(INudgingController nudgingController)
		{
			this.nudgingController = nudgingController;
		}

		public virtual void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
		{
			nudgingController.ReportNudgeStarted(new[] { serviceTaskCode }, new StackTrace());
			nudgingController.ScheduleTasks(new[] { serviceTaskCode }, delay: delay);
		}
	}
}
