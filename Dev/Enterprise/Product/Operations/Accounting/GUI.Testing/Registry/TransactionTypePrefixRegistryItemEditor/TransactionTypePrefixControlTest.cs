using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(TransactionTypePrefixControl))]
	class TransactionTypePrefixControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TransactionTypePrefixCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TransactionTypePrefixControl)control).TransactionTypePrefixGrid.ReadOnly;
		}
	}
}
