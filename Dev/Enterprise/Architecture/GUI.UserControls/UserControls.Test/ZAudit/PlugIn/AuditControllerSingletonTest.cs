using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.Testing
{
	[TestedType(typeof(AuditController))]
	sealed class AuditControllerSingletonTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Audit;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(AuditController); }
		}
	}
}
