using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(TP5MessageSendingObjectParent))]
	sealed class TP5MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendAndSaveNctsMessage()
		{
			var parent = (TP5MessageSendingObjectParent)GetNewBusinessObject();
			var sentResult = parent.SendAndSaveMessages();
			AssertEquals(true, sentResult);
		}

		public void TestRegularJustificationCodeObjectProperty()
		{
			var parent = (TP5MessageSendingObjectParent)GetNewBusinessObject();
			var propertiesList = parent.MessageSendingObjectProperties.ToList();
			AssertEquals($"MessageSendingObjectProperties should contain '{TP5MessageSendingObject.Schema.JustificationCode}'.", true, propertiesList.Exists(item => item.PropertyName == TP5MessageSendingObject.Schema.JustificationCode));
		}

		protected override BusinessObject GetNewBusinessObject() => new TP5MessageSendingObjectParent(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}

		FR.Business.NCTS.NctsHeader nctsHeader;
	}
}
