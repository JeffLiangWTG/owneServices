using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTotalRoundingCollection))]
	public class ComplianceDocumentSupportingReasonCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceTotalRoundingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override InvoiceTotalRoundingCollection GetCollectionToTest() => new InvoiceTotalRoundingCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new InvoiceTotalRounding();

		#endregion
	}
}
