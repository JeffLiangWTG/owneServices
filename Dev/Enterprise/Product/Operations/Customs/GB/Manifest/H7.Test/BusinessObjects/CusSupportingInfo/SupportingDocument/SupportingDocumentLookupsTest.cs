using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocumentLookups))]
	sealed class SupportingDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
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
			AssertEquals(1, collection.Count);
			var test = collection.ToArray();
			var codes = collection.Select(l => l.ZZD_Code);
			var descs = collection.Select(l => l.ZZD_Description);
			Assert(codes.Contains("Z983"));
			Assert(descs.Contains("Z983 Desc"));
		}

		public void TestActionList()
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Action Description");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Z", "Test Action", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var actionList = supportingDocument.Lookups.ActionList;
			AssertEquals(1, actionList.Count);
			Assert(actionList.GetAllCodes().Contains("Z"));
			AssertEquals("Test Action", actionList.GetDescriptionFromCode("Z"));
		}

		public void TestAvailabilityList()
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Availability Description");
			var availA = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Z", "Test Availability", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var availabilityList = supportingDocument.Lookups.AvailabilityList;
			AssertEquals(1, availabilityList.Count);
			Assert(availabilityList.GetAllCodes().Contains("Z"));
			AssertEquals("Test Availability", availabilityList.GetDescriptionFromCode("Z"));
		}

		public void TestAvailabilityListWithBillParent()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Availability Description");
			var availA = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Z", "Test Availability", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var availabilityList = supportingDocument.Lookups.AvailabilityList;
			AssertEquals(1, availabilityList.Count);
			Assert(availabilityList.GetAllCodes().Contains("Z"));
			AssertEquals("Test Availability", availabilityList.GetDescriptionFromCode("Z"));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			packedItem = bill.PackedItems.AddNew();

			supportingDocument = packedItem.SupportingDocuments.AddNew();

			helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			Factory.Save();
		}

		SupportingDocument supportingDocument;
		AsycudaPackedItem packedItem;
		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
