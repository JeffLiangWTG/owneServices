using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
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

		public void TestDocumentType_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AdditionalInfoSendingObject), AdditionalInfoSendingObject.Schema.DocumentType, includesInherit: false, a => a.ListDataSourceMember == "Lookups.DocumentTypeList");
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

		public void TestIDataGroupingProviderMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.FrenchGuyana))
			{
				var provider = GetNewBusinessObject() as IDataGroupingProvider;
				AssertEquals("provider.DataGrouping", Core.Constants.CountryCodes.France, provider.DataGrouping);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			sendingObject = GetNewBusinessObject() as AdditionalInfoSendingObject;
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

		AdditionalInfoSendingObject sendingObject;
	}
}
