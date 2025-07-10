using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceModule))]
	sealed class CommercialInvoiceModuleTest : Customs.Module.Testing.CommercialInvoiceModuleTest
	{
		public void TestGetNewController()
		{
			using (var module = new CommercialInvoiceModule())
			{
				AssertType<CommercialInvoiceController>($"Should load {typeof(CommercialInvoiceController).FullName} in BR.", module.GetNewController());
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new CommercialInvoiceModule())
			{
				AssertType<CommercialInvoiceFilterBusinessObject>($"Should load {typeof(CommercialInvoiceFilterBusinessObject).FullName} in BR.", module.FilterBusinessObject);
			}
		}
	}
}
