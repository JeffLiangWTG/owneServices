using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
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

		public void TestDocumentInformation_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentInformationInfo);
			AssertEquals("Caption", "Document Information", resData.Caption);
			AssertEquals("ShortCaption", "Info.", resData.ShortCaption);
		}

		public void TestReferenceNumber_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.ReferenceNumberInfo);
			AssertEquals("Caption", "Reference Number", resData.Caption);
			AssertEquals("ShortCaption", "Ref.", resData.ShortCaption);
		}

		public void TestCCQualifier_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.CCQualifierInfo);
			AssertEquals("Caption", "CC Qualifier", resData.Caption);
		}

		public void TestLookups()
		{
			AssertType<AdditionalInfoSendingObjectLookups>(sendingObject.Lookups);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var requestedDocument = entryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			sendingObject = new AdditionalInfoSendingObject(entryHeader, null, requestedDocument);
		}

		protected override BusinessObject GetNewBusinessObject() => sendingObject;
		AdditionalInfoSendingObject sendingObject;
		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
	}
}
