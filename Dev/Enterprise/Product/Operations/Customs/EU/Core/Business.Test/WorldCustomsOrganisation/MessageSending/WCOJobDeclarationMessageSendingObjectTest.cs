using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending.Testing
{
	[TestedType(typeof(WCOJobDeclarationMessageSendingObject))]
	class WCOJobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmendmentDetails()
		{
			messageSendingObject.AmendmentDetails = new AmendmentDetails(null, "", "");
			AssertType<AmendmentDetails>(messageSendingObject.AmendmentDetails);
		}

		#region Overrides of BusinessObjectBaseTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			return messageSendingObject;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			messageSendingObject = new WCOJobDeclarationMessageSendingObject(entryHeader);
		}
		WCOJobDeclarationMessageSendingObject messageSendingObject;
		#endregion
	}
}
