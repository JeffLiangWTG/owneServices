using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditCardFeeCollection))]
	public class CreditCardFeeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CreditCardFeeCollection>
	{
		#region Implementation

		protected override CreditCardFeeCollection GetCollectionToTest()
		{
			return new CreditCardFeeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CreditCardFee();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new CreditCardFeeCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion

		public void TestDuplicateRegistryEntry()
		{
			CreditCardFeeCollection collection = new CreditCardFeeCollection();
			ZGuid testGuid = ZGuid.NewZGuid();

			CreditCardFeeCollection creditCardFeeCollection = new CreditCardFeeCollection();
			CreditCardFee creditCardFee = new CreditCardFee();
			creditCardFee.ChargeCodePK = testGuid;
			creditCardFee.Percentage = 20.20;
			creditCardFeeCollection.Add(creditCardFee);
			AssertEquals("Duplicate item not expected", false, creditCardFeeCollection.IsDuplicateItem(creditCardFee));

			CreditCardFee duplicateCreditCardFee = new CreditCardFee();
			duplicateCreditCardFee.ChargeCodePK = testGuid;
			duplicateCreditCardFee.Percentage = 20.20;
			creditCardFeeCollection.Add(duplicateCreditCardFee);
			AssertEquals("Duplicate item expected", true, creditCardFeeCollection.IsDuplicateItem(duplicateCreditCardFee));
		}
	}
}
