using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class RefundApplicationDocumentSendingObjectLookupsTest : BusinessObjectLookupsTestCase
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

			var collection = (ZZRefCusCodeListCombinedCollection)lookups.DocumentTypeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var (entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var mainSendingAction = new RefundApplicationMessageSendingAction(entryHeaderWrapper.EntryHeader);
			var documentSendingObject = new RefundApplicationDocumentSendingObject(entryHeaderWrapper.EntryHeader, mainSendingAction);
			lookups = new RefundApplicationDocumentSendingObjectLookups(documentSendingObject);
		}
		RefundApplicationDocumentSendingObjectLookups lookups;
	}
}
