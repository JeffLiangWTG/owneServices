using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(EngineeringTaskController))]
	internal class EngineeringTaskControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(EngineeringTaskController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EngineeringTask;
		}
	}
}
