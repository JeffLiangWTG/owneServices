using Enterprise.Client.EDI.EndpointManagement.GUI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedMessagingConfigController))]
	public class EdiTrustedMessagingConfigControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiTrustedMessagingConfig;
		}

		public override void TestNewForm()
		{
			using (var form = (EDITrustedMessagingConfigForm)Controller.ShowNewForm())
			{
				var bizo = (EdiTrustedMessagingConfig)form.BusinessEntity;
				AssertEquals("CSC", bizo.ETM_CertificateType);
			}
		}
	}
}
