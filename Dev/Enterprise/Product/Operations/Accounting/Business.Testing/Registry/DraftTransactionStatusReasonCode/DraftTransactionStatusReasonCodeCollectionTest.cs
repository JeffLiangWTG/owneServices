using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCodeCollection))]
	public class DraftTransactionStatusReasonCodeCollectionTest : RegistryBusinessObjectCollectionTestCase<DraftTransactionStatusReasonCodeCollection>
	{
		#region Implementation

		protected override DraftTransactionStatusReasonCodeCollection GetCollectionToTest()
		{
			return new DraftTransactionStatusReasonCodeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DraftTransactionStatusReasonCode();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion
	}
}
