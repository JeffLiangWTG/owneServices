using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumberCollection))]
	public class PaymentReceiptTypeReferenceNumberCollectionTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectCollectionTemplateTestCase<PaymentReceiptTypeReferenceNumberCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PaymentReceiptTypeReferenceNumberCollection GetCollectionToTest()
		{
			return new PaymentReceiptTypeReferenceNumberCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentReceiptTypeReferenceNumber();
		}
	}
}
