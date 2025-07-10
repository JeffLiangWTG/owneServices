using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObject))]
	class AdditionalInfoSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Additional Info", sendingObject.HumanReadableName);
		}

		public void TestDocumentType()
		{
			AssertEquals("DocumentType should default CSI_Code", "9002", sendingObject.DocumentType);
		}

		public void TestDocumentType_CaptionAndMaxLength()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentTypeInfo);
			AssertEquals("Caption", "Document Type", resData.Caption);
			AssertEquals("ShortCaption", "Type", resData.ShortCaption);
		}

		public void TestCheckDocumentType()
		{
			sendingObject.DocumentType = string.Empty;
			AssertHasErrorContaining("DocumentType should have a validation.", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEntered);

			sendingObject.DocumentType = "9002";
			AssertNoErrors("DocumentType passed.", sendingObject.DocumentTypeInfo);
		}

		public void TestCheckDocumentInformation()
		{
			sendingObject.DocumentInformation = "Document Information";
			sendingObject.DocumentInformation = string.Empty;
			AssertHasErrorContaining("DocumentInformation should have a validation.", sendingObject.DocumentInformationInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestDocumentInformation_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentInformationInfo);
			AssertEquals("Caption", "Document Information", resData.Caption);
			AssertEquals("ShortCaption", "Info.", resData.ShortCaption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sendingObject = new AdditionalInfoSendingObject("9002");
		}

		protected override BusinessObject GetNewBusinessObject() => sendingObject;
		AdditionalInfoSendingObject sendingObject;
	}
}
