using System;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(ProjectClientOrgLicenceController))]
	public class ProjectClientOrgLicenceControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ProjectClientOrgLicenceController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ProjectClientOrgLicence;
		}
	}
}
