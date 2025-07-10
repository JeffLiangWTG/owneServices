using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(ClientAndAgentBrandingControl))]
	internal class ClientAndAgentBrandingControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AgentDocumentBrandCollection(null, Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ClientAndAgentBrandingControl clientAndAgentBrandingControl = (ClientAndAgentBrandingControl)control;
			return clientAndAgentBrandingControl.CodeAndDescriptionGridForTest.ReadOnly && clientAndAgentBrandingControl.ImageControlForTest.ReadOnly;
		}

		protected override void BashForDescriptionColumn(Control controlToBash)
		{
		}
	}
}
