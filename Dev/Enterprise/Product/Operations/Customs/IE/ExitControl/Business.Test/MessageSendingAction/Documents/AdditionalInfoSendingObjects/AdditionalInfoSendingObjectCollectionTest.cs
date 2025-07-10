using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObjectCollection))]
	class AdditionalInfoSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalInfoSendingObjectCollection>
	{
		public void TestMaxCount()
		{
			AssertEquals("Should have set MaxCount.", 99, ((ISupportMaxCountValidation)Collection).MaxCountValidator.MaxCount);
		}

		protected override AdditionalInfoSendingObjectCollection GetCollectionToTest() => new AdditionalInfoSendingObjectCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AdditionalInfoSendingObject("9001");
		}
	}
}
