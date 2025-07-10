using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	public class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestMaxLength()
		{
			AssertEquals(4, supportingDocument.CSI_CodeInfo.MaxLength);
			AssertEquals(1, supportingDocument.CSI_AvailabilityInfo.MaxLength);
			AssertEquals(1, supportingDocument.CSI_ActionsInfo.MaxLength);
		}

		public void TestCaptionResourceString()
		{
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_CodeInfo, (string[])null, "Type", "Type", "Type", "Supporting document type.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_ActionsInfo, (string[])null, "Actions");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_AvailabilityInfo, (string[])null, "Availability", "Avail.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_DateOfExpiryInfo, (string[])null, "Date of Expiry");
			});
		}

		public void TestLookups()
		{
			AssertType<SupportingDocumentLookups>(supportingDocument.Lookups);
		}

		public void TestCSI_DateOfIssueReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", ["ITEM"] }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				[importCodeType, exportCodeType], "Z983", "Z983 Desc", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();

			supportingDocument.CSI_Code = ZString.Empty;
			AssertEquals(true, supportingDocument.CSI_DateOfIssueInfo.ReadOnly);

			supportingDocument.CSI_Code = "Z983";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoNotifications(supportingDocument.CSI_CodeInfo);
			AssertEquals(false, supportingDocument.CSI_DateOfIssueInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber2Readonly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", ["ITEM"] }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				[importCodeType, exportCodeType], "Z983", "Z983 Desc", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();

			supportingDocument.CSI_Code = ZString.Empty;
			AssertEquals(true, supportingDocument.CSI_ReferenceNumber2Info.ReadOnly);

			supportingDocument.CSI_Code = "Z983";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertEquals(false, supportingDocument.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestRefCusCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", eun);

			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { exportCodeType }, "1111", "Code desc", new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var supportingDoc = (SupportingDocument)GetNewBusinessObject();
			supportingDoc.CSI_Code = "2222";
			AssertNull(supportingDoc.RefCusCode);
			supportingDoc.CSI_Code = "1111";
			var test = supportingDoc.RefCusCode;
			AssertEquals("1111", supportingDoc.RefCusCode.ZZD_Code);
		}

		public void TestDefaultCSI_ReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var document = bill.SupportingDocuments.AddNew();
			var permit = SetupDeclarantAndPermit(header);

			CombineAssertions(() =>
			{
				document.CSI_Code = "code";
				AssertNullOrEmpty("CSI_Code is not 1BRD", document.CSI_ReferenceNumber);

				document.CSI_Code = "1BRD";
				AssertEquals(permit.CPH_Number, document.CSI_ReferenceNumber);

				document.CSI_Code = "code";
				AssertNullOrEmpty("CSI_Code isn't 1BRD anymore.", document.CSI_ReferenceNumber);

				header.AMA_OA_Declarant = Guid.Empty;
				document.CSI_Code = "1BRD";
				AssertNullOrEmpty("Permit header is null", document.CSI_ReferenceNumber);
			});
		}

		CusPermitHeader SetupDeclarantAndPermit(AsycudaManifestHeader header)
		{
			var declarantOrg = Factory.New<OrgHeader>();
			declarantOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "GB");
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Test Address 1";
			header.AMA_OA_Declarant = declarantAddress.PK;

			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_OA_AppliesTo = declarantAddress.PK;
			permit.CPH_Type = "BRD";
			permit.CPH_Number = "GB123456";

			return permit;
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			Factory.Save();

			var supportingDocumentOnPackedItem = packedItem.SupportingDocuments.AddNew();
			Factory.Save();

			yield return supportingDocumentOnPackedItem;
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItems = bill.PackedItems.AddNew();

			supportingDocument = packedItems.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
	}
}
