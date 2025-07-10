using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
	sealed class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendAndSaveMessages()
		{
			var coDeparture = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			coDeparture.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
			var coDestination = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			coDestination.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
			var parent = (NctsHeaderMessageSendingObjectParent)GetNewBusinessObject();
			parent.SendAndSaveMessages();
			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);
		}

		public void TestDeclarationGoodsItemNumbersAssigned()
		{
			var coDeparture = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			coDeparture.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
			var coDestination = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			coDestination.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
			nctsHeader.Bills.AddNew().GoodsItems.AddNew().BY_DeclarationGoodsItemNumber = 2;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var parent = (NctsHeaderMessageSendingObjectParent)GetNewBusinessObject();
			parent.SendAndSaveMessages();
			AssertEquals(3, goodsItem.BY_DeclarationGoodsItemNumber);
		}

		public void TestGetSendingObjectsCollection()
		{
			var parent = (NctsHeaderMessageSendingObjectParent)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(1, parent.SendingObjectsCollection.Count);
				AssertType<NctsHeaderMessageSendingObject>(parent.SendingObjectsCollection[0]);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		}

		NctsHeader nctsHeader;
	}
}
