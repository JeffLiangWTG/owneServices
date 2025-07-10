using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoiceDescriptionCollection))]
	public class JobInvoiceDescriptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobInvoiceDescriptionCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobInvoiceDescriptionCollection GetCollectionToTest()
		{
			return new JobInvoiceDescriptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobInvoiceDescription();
		}

		#endregion
	}
}
