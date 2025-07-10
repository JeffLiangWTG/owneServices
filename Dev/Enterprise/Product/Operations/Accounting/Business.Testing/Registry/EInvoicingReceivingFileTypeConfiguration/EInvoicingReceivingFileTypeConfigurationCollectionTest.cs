using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingReceivingFileTypeConfigurationCollection))]
	public class EInvoicingReceivingFileTypeConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EInvoicingReceivingFileTypeConfigurationCollection>
	{
		#region Implementation

		protected override EInvoicingReceivingFileTypeConfigurationCollection GetCollectionToTest()
		{
			return new EInvoicingReceivingFileTypeConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EInvoicingReceivingFileTypeConfiguration();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion
	}
}
