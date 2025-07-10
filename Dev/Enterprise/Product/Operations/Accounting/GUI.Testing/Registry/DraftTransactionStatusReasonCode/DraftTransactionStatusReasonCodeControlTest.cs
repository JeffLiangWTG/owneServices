using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCodeControl))]
	class DraftTransactionStatusReasonCodeControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DraftTransactionStatusReasonCodeControl)control).DraftTransactionStatusReasonCodeGrid_ForTestOnly.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DraftTransactionStatusReasonCodeCollection();
		}
	}
}
