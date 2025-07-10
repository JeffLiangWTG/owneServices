using System;
using CargoWise.Definitions;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType
		{
			get { return typeof(ProcessHeader); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobWorkflows; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WorkflowHeaders;
	}
}
