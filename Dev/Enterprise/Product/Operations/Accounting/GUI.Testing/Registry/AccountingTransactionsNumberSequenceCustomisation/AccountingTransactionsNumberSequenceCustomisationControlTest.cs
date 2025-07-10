using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(AccountingTransactionsNumberSequenceCustomisationControl))]
	internal class AccountingTransactionsNumberSequenceCustomisationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TransactionNumberSequenceCustomisationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AccountingTransactionsNumberSequenceCustomisationControl)control).elementsGrid.ReadOnly;
		}
	}
}
