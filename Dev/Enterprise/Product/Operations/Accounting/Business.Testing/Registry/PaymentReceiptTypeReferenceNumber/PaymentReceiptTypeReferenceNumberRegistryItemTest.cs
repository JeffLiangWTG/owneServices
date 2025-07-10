using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumberRegistryItem))]
	public class PaymentReceiptTypeReferenceNumberRegistryItemTest : StronglyTypedRegistryItemTestCase<PaymentReceiptTypeReferenceNumberCollection>
	{
		protected override StronglyTypedRegistryItem<PaymentReceiptTypeReferenceNumberCollection, PaymentReceiptTypeReferenceNumberCollection> GetNewRegistryItem()
		{
			return new PaymentReceiptTypeReferenceNumberRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, AccountingConfigurationRegistry.PaymentReceiptTypeReferenceNumberDefaultValueGetter);
		}
	}
}
