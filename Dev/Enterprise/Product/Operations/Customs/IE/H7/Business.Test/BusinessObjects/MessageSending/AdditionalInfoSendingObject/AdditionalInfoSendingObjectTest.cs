using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObject))]
	sealed class AdditionalInfoSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<AdditionalInfoSendingObjectValidation>(sendingObject.Validation);
		}

		public void TestValidationConfiguration()
		{
			AssertType<ValidationConfiguration>(sendingObject.ValidationConfiguration);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testBizObj = header.Bills.AddNew();
			var requestedDocument = testBizObj.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			return new AdditionalInfoSendingObject(testBizObj, null, requestedDocument);
		}

		AdditionalInfoSendingObject sendingObject => GetNewBusinessObject() as AdditionalInfoSendingObject;
	}
}
