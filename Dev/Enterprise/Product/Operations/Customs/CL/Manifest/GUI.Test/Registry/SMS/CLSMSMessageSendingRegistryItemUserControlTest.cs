using CargoWise.EntityFramework;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLSMSMessageSendingRegistryItemUserControl))]
	sealed class CLSMSMessageSendingRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CLSMSMessageSending();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CLSMSMessageSendingRegistryItemUserControl)control).ReadOnly;
	}
}
