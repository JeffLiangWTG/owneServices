using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExcludedFullyDigitalizedElectronicInvoiceData))]
	public class ExcludedFullyDigitalizedElectronicInvoiceDataTest : RegistryBusinessObjectTemplateTestCase<ExcludedFullyDigitalizedElectronicInvoiceData>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ExcludedFullyDigitalizedElectronicInvoiceData GetBusinessObjectToClone()
		{
			return (ExcludedFullyDigitalizedElectronicInvoiceData)GetNewBusinessObject();
		}

		protected override ExcludedFullyDigitalizedElectronicInvoiceData GetBusinessObjectToSerialise()
		{
			return (ExcludedFullyDigitalizedElectronicInvoiceData)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerAddress = true };
		}

		#endregion
	}
}
