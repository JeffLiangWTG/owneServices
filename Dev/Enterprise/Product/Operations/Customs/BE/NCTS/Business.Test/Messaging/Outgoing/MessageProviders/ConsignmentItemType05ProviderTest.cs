using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class ConsignmentItemType05ProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentItemType05Provider>
	{
		public void TestGoodsItemNumber()
		{
			item.BY_LineNo = 2;
			AssertEquals(2, Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber() => CombineAssertions(() =>
		{
			item.BY_UnloadedState = "NEW";
			item.BY_DeclarationGoodsItemNumber = 3;
			AssertEquals("NEW", 3, Provider.DeclarationGoodsItemNumber);

			item.BY_UnloadedState = "DIF";
			AssertEquals("DIF", 3, GetProvider().DeclarationGoodsItemNumber);
		});

		public void TestDeclarationGoodsItemNumber_Conditional()
		{
			item.BY_DeclarationGoodsItemNumber = 3;
			item.BY_UnloadedState = "DEC";
			AssertEquals(0, Provider.DeclarationGoodsItemNumber);
		}

		public void TestCommodity() => CombineAssertions(() =>
		{
			item.BY_UnloadedState = "NEW";
			AssertNotNull("NEW", Provider.Commodity);

			item.BY_UnloadedState = "DIF";
			AssertNotNull("DIF", GetProvider().Commodity);
		});

		public void TestCommodity_Conditional()
		{
			item.BY_UnloadedState = "DEC";
			AssertNull(Provider.Commodity);
		}

		public void TestCommodity_UnloadedItem()
		{
			CombineAssertions(() =>
			{
				item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				item.BY_NetWeight = 5;
				AssertNull("should be null because provider has no unloaded item", Provider.Commodity);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new ConsignmentItemType05Provider(item);
				AssertEquals("should be 5 because value from item is taken on NEW item", 5M, provider.Commodity.NetMass);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedItem = item.UnloadedGoodsItem;
				unloadedItem.BY_NetWeight = 10;
				unloadedItem.BY_NetWeightUnit = "KG";
				item.BY_BY_Commodity = unloadedItem.PK;
				provider = new ConsignmentItemType05Provider(item);
				AssertEquals("should be new unloaded item's value", 10M, provider.Commodity.NetMass);
			});
		}

		public void TestPackagings()
		{
			var package = item.Packages.AddNew();
			package.B5_TypeOfDifference = "NEW";
			package = item.Packages.AddNew();
			package.B5_TypeOfDifference = "NEW";
			AssertEquals(2, Provider.Packagings.Count);
		}

		public void TestPackagings_Conditional()
		{
			var package = item.Packages.AddNew();
			package.B5_TypeOfDifference = "DEC";
			AssertEquals(0, Provider.Packagings.Count);
		}

		public void TestSupportingDocuments()
		{
			var supportingDocument1 = item.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Status = "NEW";
			var supportingDocument2 = item.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Status = "NEW";
			AssertEquals(2, Provider.SupportingDocuments.Count);
		}

		public void TestSupportingDocuments_Conditional()
		{
			var supportingDocument = item.SupportingDocuments.AddNew();
			supportingDocument.CSI_Status = "DEC";
			AssertEquals(0, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			var ref1 = item.AdditionalInfos.AddNew();
			var ref2 = item.AdditionalInfos.AddNew();
			ref1.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
			ref2.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
			AssertEquals(2, Provider.AdditionalReferences.Count);
		}

		public void TestAdditionalReferences_Conditional()
		{
			var reference = item.AdditionalInfos.AddNew();
			reference.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
			reference.CSI_Status = "DEC";
			AssertEquals(0, Provider.AdditionalReferences.Count);
		}

		public void TestTransportDocuments()
		{
			AssertEquals(0, Provider.TransportDocuments.Count);
		}

		protected override ConsignmentItemType05Provider GetProvider() => provider;

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType("A");
			item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			provider = new ConsignmentItemType05Provider(item);
		}
		ConsignmentItemType05Provider provider;
		NctsArrivalCargoDesc item;
	}
}
