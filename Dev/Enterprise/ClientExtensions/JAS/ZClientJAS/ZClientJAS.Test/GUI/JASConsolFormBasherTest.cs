using System;
using System.Windows.Forms;
using Enterprise.Client.JAS.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Testing
{
	[TestedType(typeof(JASConsolForm))]
	internal class JASConsolFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				JASConsolForm result = new JASConsolForm(consol);
				result.ControllerID = ControllerIDs.JobConsol;
				return result;
			}
		}
	}
}
