using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceDetailGroupByRegistryItem))]
	sealed class InvoiceDetailGroupByRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoiceDetailGroupBy>
	{
		protected override StronglyTypedRegistryItem<InvoiceDetailGroupBy, InvoiceDetailGroupBy> GetNewRegistryItem()
		{
			return new InvoiceDetailGroupByRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceDetailGroupBy());
		}

		protected override InvoiceDetailGroupBy ValidValue
		{
			get { return new InvoiceDetailGroupBy(); }
		}
	}
}
