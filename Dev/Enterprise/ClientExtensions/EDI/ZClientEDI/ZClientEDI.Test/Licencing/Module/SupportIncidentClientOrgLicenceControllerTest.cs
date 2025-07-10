using System;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(SupportIncidentClientOrgLicenceController))]
	public class SupportIncidentClientOrgLicenceControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(SupportIncidentClientOrgLicenceController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.SupportIncidentClientOrgLicence;
		}
	}
}
