using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GLJournalApprovalThresholdControl))]
	class GLJournalApprovalThresholdControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GLJournalApprovalThresholdControl)control).ApprovalThresholdSetupGrid_ForTestOnly.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new GLJournalApprovalThresholdCollection();
			collection.AddNew();

			return collection;
		}
	}
}
