using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumberControl))]
	class PaymentReceiptTypeReferenceNumberControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new PaymentReceiptTypeReferenceNumberCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PaymentReceiptTypeReferenceNumberControl)control).PaymentReceiptTypeReferenceNumberGrid.ReadOnly;
		}
	}
}
