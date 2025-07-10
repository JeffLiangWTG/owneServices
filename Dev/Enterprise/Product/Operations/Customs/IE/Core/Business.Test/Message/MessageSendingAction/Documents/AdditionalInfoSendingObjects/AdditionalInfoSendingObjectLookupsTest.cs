using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AdditionalInfoSendingObjectLookupsTest : TestCaseWithFactory
	{
		public void TestDocumentTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "ITEM" } },
				{ "ACTAV", new string[] { "AC", "AE", "AX", "HE" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var collection = (ZZRefCusCodeListCombinedCollection)additionalInfoSendingObject.Lookups.DocumentTypeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);
		}

		public void TestCL010CountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "ITEM" } },
				{ "ACTAV", new string[] { "AC", "AE", "AX", "HE" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010 }, "ABC12", "ABC 1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var collection = (ZZRefCusCodeListCombinedCollection)additionalInfoSendingObject.Lookups.CL010CountryCodes;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("ABC12", collection[0].ZZD_Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var requestedDocument = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			additionalInfoSendingObject = new AdditionalInfoSendingObject(testBizObjs.entryHeaderWrapper.EntryHeader, null, requestedDocument);
		}
		AdditionalInfoSendingObject additionalInfoSendingObject;
	}
}
