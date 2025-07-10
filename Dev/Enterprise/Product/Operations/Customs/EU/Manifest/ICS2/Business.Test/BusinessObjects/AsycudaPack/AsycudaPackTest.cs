using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSpecificCircumstanceCompatibilityAtPackLevel()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var supportingDocument = pack.SupportingDocuments.AddNew();

			var applicableSpecificCircumstanceIndicatorList = new string[] {
				EUICS2SpecificCircumstanceList.Codes.F10,
				EUICS2SpecificCircumstanceList.Codes.F11,
				EUICS2SpecificCircumstanceList.Codes.F12,
				EUICS2SpecificCircumstanceList.Codes.F13,
				EUICS2SpecificCircumstanceList.Codes.F14,
				EUICS2SpecificCircumstanceList.Codes.F15,
				EUICS2SpecificCircumstanceList.Codes.F20,
				EUICS2SpecificCircumstanceList.Codes.F22,
				EUICS2SpecificCircumstanceList.Codes.F26,
				EUICS2SpecificCircumstanceList.Codes.F27,
				EUICS2SpecificCircumstanceList.Codes.F30,
				EUICS2SpecificCircumstanceList.Codes.F32,
				EUICS2SpecificCircumstanceList.Codes.F50,
				EUICS2SpecificCircumstanceList.Codes.F51
			};

			foreach (var item in new EUICS2SpecificCircumstanceList().GetAllCodes())
			{
				manifestHeader.SpecificCircumstanceIndicator = item;
				supportingDocument.RunPreSaveValidation();

				if (applicableSpecificCircumstanceIndicatorList.Contains(item))
				{
					AssertNoRowWarnings(supportingDocument);
				}
				else
				{
					AssertHasRowWarning("Has row warning as specific circumstance is not applicable.", supportingDocument, "The Specific Circumstance does not support Supporting Document details at this level. These will not be sent in the ICS2 message.");
				}
			}
		}

		public void TestPackDescription_PopulatesPackedItemGoodsDescriptionWhenBlank()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Goods description on pack is empty", string.Empty, pack.APA_GoodsDescription);
				AssertEquals("Precondition: Goods description on packed item is empty", string.Empty, packedItem.API_GoodsDescription);

				pack.APA_GoodsDescription = "ABCD1234";
				AssertEquals("Goods description on packed item is updated", "ABCD1234", packedItem.API_GoodsDescription);

				pack.APA_GoodsDescription = "ABCD\r\n1234";
				AssertEquals("Goods description on packed has no CRLF", "ABCD1234", Regex.Replace(packedItem.API_GoodsDescription, @"[\r\n]", ""));
			});
		}

		public void TestPackDescription_PopulatesPackedItemGoodsDescription()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var packedItem = pack.PackedItem;

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Goods description on pack is empty", string.Empty, pack.APA_GoodsDescription);
				AssertEquals("Precondition: Goods description on packed item is empty", string.Empty, packedItem.API_GoodsDescription);
				AssertEquals("Precondition: Only one Packed items exist" , 1, pack.PackedItems.Count);
			});

			pack.APA_GoodsDescription = "ABCD1234";
			AssertEquals("Goods description on packed item updated if there is exactly ONE PackItem with empty description", "ABCD1234", packedItem.API_GoodsDescription);

			pack.APA_GoodsDescription = "EFGH5678";
			AssertEquals("Goods description on packed item updated if there is exactly ONE PackItem with filled description", "EFGH5678", packedItem.API_GoodsDescription);
		}

		public void TestPackDescription_DoesNotPopulateWhenMultiplePackedItemsExist()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var packItem1 = pack.PackedItem;
			var packItem1Desc = packItem1.API_GoodsDescription = "Item1";

			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			var packItem2Desc = packedItem2.API_GoodsDescription = "Item2";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Goods description on pack is empty", string.Empty, pack.APA_GoodsDescription);
				Assert("Precondition: Multiple Packed items exist", pack.PackedItems.Count > 1);
			});

			pack.APA_GoodsDescription = "ABCD1234";
			AssertEquals("Goods description should not be populated when multiple packed items exist", "Item1", packItem1Desc);
			AssertEquals("Goods description should not be populated when multiple packed items exist", "Item2", packItem2Desc);
		}

		public void TestGrossWeightInKG()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>(); 
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.APA_Weight = 123.4567894m;
			pack.APA_WeightUQ = "G";

			AssertEquals("Pack weight should be rounded to 6 decimals in KG", 0.123457m, pack.GrossWeightInKG);
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertType<AsycudaPackLookups>(pack.Lookups);
		}

		public void TestLineNo_ShouldBeReadOnly_WhenManifestIsRegisteredAndF14F15()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: true, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: false, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: true, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: false, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isRegistered: true, expectedReadonly: false),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			foreach (var (specificCircumstanceIndicator, isRegistered, expectedReadonly) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationNumber = isRegistered ? "123" : string.Empty;

				AssertEquals("LineNo", expectedReadonly, pack.APA_LineNoInfo.ReadOnly);
			}
		}

		public void TestLineNo_ShouldNotBeReadOnly_WhenCustomsStatusIsCancelledAndF14F15()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: true, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: false, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: true, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: false, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isCancelled: true, expectedReadonly: false),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.RegistrationNumber = "123";
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var (specificCircumstanceIndicator, isCancelled, expectedReadonly) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationStatus = isCancelled ? EUICS2CustomsStatusList.Codes.CAN : EUICS2CustomsStatusList.Codes.REG;

				AssertEquals("LineNo", expectedReadonly, pack.APA_LineNoInfo.ReadOnly);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
