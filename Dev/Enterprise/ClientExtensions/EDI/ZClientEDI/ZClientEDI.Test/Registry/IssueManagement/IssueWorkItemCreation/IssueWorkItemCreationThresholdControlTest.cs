using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IssueWorkItemCreationThresholdControl))]
	class IssueWorkItemCreationThresholdControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IssueWorkItemCreationThresholdCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((IssueWorkItemCreationThresholdControl)control).IssueWorkItemCreationThresholdGrid.ReadOnly;
		}
	}
}
