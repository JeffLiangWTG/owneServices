using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocBuilderDataSourceControl))]
	sealed class DocBuilderDataSourceControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DocBuilderDataSource();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var theControl = (DocBuilderDataSourceControl)control;
			return theControl.FreightRadioButton.ReadOnly && theControl.BrokerageRadioButton.ReadOnly;
		}
	}
}
