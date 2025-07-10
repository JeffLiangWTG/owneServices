using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObjectCollection))]
	sealed class AdditionalInfoSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalInfoSendingObjectCollection>
	{
		public void TestMaxCount()
		{
			AssertEquals("Should have set MaxCount.", 99, ((ISupportMaxCountValidation)Collection).MaxCountValidator.MaxCount);
		}

		protected override AdditionalInfoSendingObjectCollection GetCollectionToTest() => new AdditionalInfoSendingObjectCollection(nctsHeader);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AdditionalInfoSendingObject(Factory, Core.Constants.CountryCodes.Ireland);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
