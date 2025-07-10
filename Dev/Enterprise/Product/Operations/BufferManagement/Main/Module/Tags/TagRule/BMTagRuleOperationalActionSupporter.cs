using System;
using CargoWise.Definitions;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagRuleOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType
		{
			get { return typeof(TagRule); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TagRule; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.TagRule;
	}
}
