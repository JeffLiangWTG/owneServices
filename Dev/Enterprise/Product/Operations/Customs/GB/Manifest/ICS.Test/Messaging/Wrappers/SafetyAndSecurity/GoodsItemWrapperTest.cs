using System.Linq;
using CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.Testing
{
	internal class GoodsItemWrapperTest : TestCaseWithFactory
	{
		public void TestGoodsItems_ItemNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Goods Item Number 1", "1", wrapperGoodsItem1.ItemNumber);
				AssertEquals("Goods Item Number 2", "2", wrapperGoodsItem2.ItemNumber);
			});
		}

		public void TestGoodsItems_MeansOfTransportIdentities_ROA()
		{
			manifest.AMA_TransportMode = "ROA";
			AssertEquals("Goods Item Means Of Transport Identities sent at header level", null, wrapperGoodsItem1.MeansOfTransportIdentities);
		}

		public void TestGoodsItems_MeansOfTransportIdentities_RAI()
		{
			manifest.AMA_TransportMode = "RAI";
			manifest.AMA_Voyage = "VOYAGE";
			AssertNotNull("Goods Item Means Of Transport Identities should appear when transport mode is RAI", wrapperGoodsItem1.MeansOfTransportIdentities);
			AssertEquals("Goods Item Means Of Transport Identities should have Identity equal to VOYAGE", "VOYAGE", wrapperGoodsItem1.MeansOfTransportIdentities.First()?.Identity);
		}

		public void TestGoodsItems_Containers()
		{
			manifest.Containers.RemoveAndDeleteAll();
			var container1 = manifest.Containers.AddNew();
			container1.ACN_ContainerNumber = "Container1";
			var container2 = manifest.Containers.AddNew();
			container2.ACN_ContainerNumber = "Container2";
			var container3 = manifest.Containers.AddNew();
			container3.ACN_ContainerNumber = "Container3";
			var container4 = manifest.Containers.AddNew();
			container4.ACN_ContainerNumber = "Container4";
			var container5 = manifest.Containers.AddNew();
			container5.ACN_ContainerNumber = "NotUsedContainer";

			var pack1 = goodsItem1.Packs.AddNew();
			pack1.SetContainer("Container1", manifest);
			var pack2 = goodsItem1.Packs.AddNew();
			pack2.SetContainer("Container2", manifest);
			var pack3 = goodsItem2.Packs.AddNew();
			pack3.SetContainer("Container3", manifest);
			var pack4 = goodsItem2.Packs.AddNew();
			pack4.SetContainer("Container4", manifest);

			CombineAssertions(() =>
			{
				AssertEquals("Goods Item 1 Container count", 2, wrapperGoodsItem1.Containers.Count());
				AssertEquals("Goods Item 2 Container count", 2, wrapperGoodsItem2.Containers.Count());
				AssertContainsExactElementsInAnyOrder("Goods Item 1 Containers", new[] { "Container1", "Container2" }, wrapperGoodsItem1.Containers.Cast<IContainer>().Select(x => x.ContainerNumber));
				AssertContainsExactElementsInAnyOrder("Goods Item 2 Containers", new[] { "Container3", "Container4" }, wrapperGoodsItem2.Containers.Cast<IContainer>().Select(x => x.ContainerNumber));
			});
		}

		public void TestGoodsItems_Consignee()
		{
			goodsItem1.ABL_ConsigneeName = "Consignee";
			goodsItem1.ABL_ConsigneeStreet1 = "Street";
			goodsItem1.ABL_ConsigneePostcode = "PC12";
			goodsItem1.ABL_ConsigneeCity = "City";
			goodsItem1.ABL_RN_NKConsigneeCountry = "GB";
			goodsItem1.ABL_ConsigneeRegNo = "GB12345678";

			CombineAssertions(() =>
			{
				AssertEquals("Goods Item Consignee Company name", "Consignee", wrapperGoodsItem1.Consignee.Name);
				AssertEquals("Goods Item Consignee Street", "Street", wrapperGoodsItem1.Consignee.StreetAndNumber);
				AssertEquals("Goods Item Consignee Post code", "PC12", wrapperGoodsItem1.Consignee.PostalCode);
				AssertEquals("Goods Item Consignee City", "City", wrapperGoodsItem1.Consignee.City);
				AssertEquals("Goods Item Consignee Country code", "GB", wrapperGoodsItem1.Consignee.CountryCode);
				AssertEquals("Goods Item Consignee Language code", string.Empty, wrapperGoodsItem1.Consignee.LanguageCode);
				AssertEquals("Goods Item Consignee EORI", "GB12345678", wrapperGoodsItem1.Consignee.ConfigCode);
			});
		}

		public void TestGoodsItems_Commodity()
		{
			goodsItem1.Packs.RemoveAndDeleteAll();
			var pack = goodsItem1.Packs.AddNew();
			pack.APA_CommodityCode = "12.34 56 78 90";

			AssertEquals("Goods Item Commodity", "12345678", wrapperGoodsItem1.Commodity.CombinedNomenclature);
		}

		public void TestGoodsItems_Consignor()
		{
			goodsItem1.ABL_ShipperName = "Consignor";
			goodsItem1.ABL_ShipperStreet1 = "Street";
			goodsItem1.ABL_ShipperPostcode = "PC12";
			goodsItem1.ABL_ShipperCity = "City";
			goodsItem1.ABL_RN_NKShipperCountry = "GB";
			goodsItem1.ABL_ShipperRegNo = "GB12345678";

			CombineAssertions(() =>
			{
				AssertEquals("Goods Item Consignor Company name", "Consignor", wrapperGoodsItem1.Consignor.Name);
				AssertEquals("Goods Item Consignor Street", "Street", wrapperGoodsItem1.Consignor.StreetAndNumber);
				AssertEquals("Goods Item Consignor Post code", "PC12", wrapperGoodsItem1.Consignor.PostalCode);
				AssertEquals("Goods Item Consignor City", "City", wrapperGoodsItem1.Consignor.City);
				AssertEquals("Goods Item Consignor Country code", "GB", wrapperGoodsItem1.Consignor.CountryCode);
				AssertEquals("Goods Item Consignor Language code", string.Empty, wrapperGoodsItem1.Consignor.LanguageCode);
				AssertEquals("Goods Item Consignor EORI", "GB12345678", wrapperGoodsItem1.Consignor.ConfigCode);
			});
		}

		public void TestGoodsItems_SpecialMentions()
		{
			goodsItem1.SpecialMentions = "11111";
			AssertContainsExactElementsInAnyOrder("Goods Item Special Mentions", new[] { "11111" }, wrapperGoodsItem1.SpecialMentions.Cast<ISpecialMention>().Select(x => x.AdditionalInformationCoded));
		}

		public void TestGoodsItems_ProducedDocuments()
		{
			AssertEquals("Goods Item Produced Documents not set", null, wrapperGoodsItem1.ProducedDocuments);
		}

		public void TestGoodsItems_PlaceOfUnloadingLNG()
		{
			AssertEquals("Goods Item Place Of Unloading Language not set", string.Empty, wrapperGoodsItem1.PlaceOfUnloadingLNG);
		}

		public void TestGoodsItems_PlaceOfUnloading()
		{
			goodsItem1.ABL_RL_NKFinalDestination = "UL123";
			AssertEquals("Goods Item Place Of Unloading", "UL123", wrapperGoodsItem1.PlaceOfUnloading);

			manifest.AMA_RL_NKPortOfDischarge = "GB456";
			goodsItem1.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertEquals("Goods Item Place Of Unloading (from header)", "GB456", wrapperGoodsItem1.PlaceOfUnloading);
		}

		public void TestGoodsItems_PlaceOfLoadingLNG()
		{
			AssertEquals("Goods Item Place Of Loading language not set", string.Empty, wrapperGoodsItem1.PlaceOfLoadingLNG);
		}

		public void TestGoodsItems_PlaceOfLoading()
		{
			goodsItem1.ABL_RL_NKOrigin = "LD123";
			AssertEquals("Goods Item Place Of Loading", "LD123", wrapperGoodsItem1.PlaceOfLoading);

			manifest.AMA_RL_NKPortOfLoading = "FI789";
			goodsItem1.ABL_RL_NKOrigin = ZString.Empty;
			AssertEquals("Goods Item Place Of Loading (from header)", "FI789", wrapperGoodsItem1.PlaceOfLoading);
		}

		public void TestGoodsItems_UNDangerousGoodsCode()
		{
			var pack = goodsItem1.Packs.AddNew();
			var undg = Factory.New<UNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "UNDG1";
			undg.SubstancePK = subs.PK;
			pack.UNDGs.Add(undg);
			_ = goodsItem1.Packs.AddNew();

			AssertEquals("Goods Item UN Dangerous Goods Code", "UNDG1", wrapperGoodsItem1.UNDangerousGoodsCode);
		}

		public void TestGoodsItems_CommercialReferenceNumber()
		{
			goodsItem1.ABL_BillNumber = "123456789";
			AssertEquals("Goods Item Commercial Reference Number", "123456789", wrapperGoodsItem1.CommercialReferenceNumber);
		}

		public void TestGoodsItems_TransportChargesOrMethodOfPayment()
		{
			goodsItem1.ABL_PrepaidCollect = "C";
			AssertEquals("Goods Item Transport Charges Or Method Of Payment", "C", wrapperGoodsItem1.TransportChargesOrMethodOfPayment);
		}

		public void TestGoodsItems_GrossMass()
		{
			goodsItem1.ABL_GrossWeight = 123.456m;
			AssertEquals("Goods Item Gross Mass", 123.456m, wrapperGoodsItem1.GrossMass);
		}

		public void TestGoodsItems_GoodsDescriptionLNG()
		{
			AssertEquals("Goods Item Goods Description language not set", string.Empty, wrapperGoodsItem1.GoodsDescriptionLNG);
		}

		public void TestGoodsItems_GoodsDescription()
		{
			goodsItem1.ABL_GoodsDescription = "GoodsDescription";
			AssertEquals("Goods Item Goods Description", "GoodsDescription", wrapperGoodsItem1.GoodsDescription);
		}

		public void TestGoodsItems_Packages()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			goodsItem1.Packs.RemoveAndDeleteAll();
			var pack1 = goodsItem1.Packs.AddNew();
			pack1.APA_PackUQ = "PKG";
			pack1.APA_PackQty = 11;
			pack1.APA_MarksAndNumbers = "Mark";
			_ = goodsItem1.Packs.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Goods Item Packages count", 2, wrapperGoodsItem1.Packages.Count());
				var wrapperGoodsItem1Pack1 = wrapperGoodsItem1.Packages.ElementAt(0);
				AssertEquals("Goods Item Pack UQ", "PKG", wrapperGoodsItem1Pack1.KindOfPackages);
				AssertEquals("Goods Item Pack Marks", "Mark", wrapperGoodsItem1Pack1.MarksAndNumbersOfPackages);
				AssertEquals("Goods Item Pack Marks Language not set", string.Empty, wrapperGoodsItem1Pack1.MarksAndNumbersOfPackagesLNG);

				pack1.APA_PackUQ = "NE";
				AssertEquals("Goods Item Pack Count (not bulk)", string.Empty, wrapperGoodsItem1Pack1.NumberOfPackages);
				AssertEquals("Goods Item Pack Piece Quantity (not bulk)", "11", wrapperGoodsItem1Pack1.NumberOfPieces);

				pack1.APA_PackUQ = "VG";
				AssertEquals("Goods Item Pack Count (bulk)", string.Empty, wrapperGoodsItem1Pack1.NumberOfPackages);
				AssertEquals("Goods Item Pack Piece Quantity (bulk)", "11", wrapperGoodsItem1Pack1.NumberOfPieces);
			});
		}

		public void TestGoodsItems_NotifyParty()
		{
			goodsItem1.ABL_NotifyPartyName = "GoodsItemNotifyParty";
			goodsItem1.ABL_NotifyPartyStreet1 = "Street";
			goodsItem1.ABL_NotifyPartyPostcode = "PC12";
			goodsItem1.ABL_NotifyPartyCity = "City";
			goodsItem1.ABL_RN_NKNotifyPartyCountry = "GB";
			goodsItem1.ABL_NotifyPartyRegNo = "GB12345678";

			CombineAssertions(() =>
			{
				AssertEquals("Goods Item Notify Party Company name", "GoodsItemNotifyParty", wrapperGoodsItem1.NotifyParty.Name);
				AssertEquals("Goods Item Notify Party Street", "Street", wrapperGoodsItem1.NotifyParty.StreetAndNumber);
				AssertEquals("Goods Item Notify Party Post code", "PC12", wrapperGoodsItem1.NotifyParty.PostalCode);
				AssertEquals("Goods Item Notify Party City", "City", wrapperGoodsItem1.NotifyParty.City);
				AssertEquals("Goods Item Notify Party Country code", "GB", wrapperGoodsItem1.NotifyParty.CountryCode);
				AssertEquals("Goods Item Notify Party Language code", string.Empty, wrapperGoodsItem1.NotifyParty.LanguageCode);
				AssertEquals("Goods Item Notify Party EORI", "GB12345678", wrapperGoodsItem1.NotifyParty.ConfigCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.Branch.Address1 = "BranchAddress";
			manifest.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			goodsItem1 = manifest.Bills.AddNew();
			goodsItem2 = manifest.Bills.AddNew();

			wrapperGoodsItem1 = new GoodsItemWrapper(goodsItem1);
			wrapperGoodsItem2 = new GoodsItemWrapper(goodsItem2);
		}

		protected AsycudaManifestHeader manifest;
		protected AsycudaBill goodsItem1;
		protected AsycudaBill goodsItem2;
		protected IGoodsItem wrapperGoodsItem1;
		protected IGoodsItem wrapperGoodsItem2;
	}
}
