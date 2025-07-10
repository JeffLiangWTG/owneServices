using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoicePostingExRateOptionCollection))]
	class InvoicePostingExRateOptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoicePostingExRateOptionCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override bool SupportsAddNew => false;

		protected override InvoicePostingExRateOptionCollection GetCollectionToTest() => new InvoicePostingExRateOptionCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new InvoicePostingExRateOption();
	}
}
