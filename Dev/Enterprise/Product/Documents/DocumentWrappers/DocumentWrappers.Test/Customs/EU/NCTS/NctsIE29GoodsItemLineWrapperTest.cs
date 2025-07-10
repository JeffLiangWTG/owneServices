using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class NctsIE29GoodsItemLineWrapperTest : TestCaseWithFactory
	{
		public void TestBOX442SPECIALMENTIONS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "PRO05", "Maritime vessels and parts", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			SetUpData();

			var lineItemWithoutAMessage = new NctsGoodsItemResponseData();
			var sm1 = new SpecialMentionResponseData();
			sm1.TypeCode = "PRO05";
			sm1.Description = "Daniel";
			lineItemWithoutAMessage.SpecialMentions.Add(sm1);
			var sm2 = new SpecialMentionResponseData();
			sm2.NctsExportFromCountry = "US";
			lineItemWithoutAMessage.SpecialMentions.Add(sm2);
			var sm3 = new SpecialMentionResponseData();
			sm3.NctsExportFromCountry = "AU";
			lineItemWithoutAMessage.SpecialMentions.Add(sm3);
			var sm4 = new SpecialMentionResponseData();
			sm4.NctsExportFromEC = true;
			lineItemWithoutAMessage.SpecialMentions.Add(sm4);
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("Maritime vessels and parts:Daniel; Export from US subject to restrictions; Export from AU subject to restrictions; Export from EU subject to restrictions", wrapper.BOX442SPECIALMENTIONS);
		}

		public void TestBOX7REFERENCE()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData();
			lineItemWithoutAMessage.CommercialReferenceNumber = "Ref me baby";
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("Ref me baby", wrapper.BOX7REFERENCE);
		}

		public void TestBOX7UCR()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType("D");
			nctsHeader.BH_ApplicationCode = "NC5";
			var ediMessage = nctsHeader.Messages.AddNew();
			var nctsEdiMessage = Factory.Load<NctsEdiMessage>(ediMessage.PK);
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { ReferenceNumberUCR = "ITEMREF1" };
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, nctsEdiMessage);
			AssertEquals("ITEMREF1", wrapper.BOX7UCR);
		}

		public void TestBOX2CONSIGNOR()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { Consignor = GetAddress() };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("WTG, 5 Rockingham Drive, Milton Keynes, MK13 7UQ, GB", wrapper.BOX2CONSIGNOR);
		}

		public void TestBOX2CONSIGNORSECURITY()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { SecurityConsignor = GetAddress() };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("WTG, 5 Rockingham Drive, Milton Keynes, MK13 7UQ, GB", wrapper.BOX2CONSIGNORSECURITY);
		}

		public void TestBOX8CONSIGNEE()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { Consignee = GetAddress() };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("WTG, 5 Rockingham Drive, Milton Keynes, MK13 7UQ, GB", wrapper.BOX8CONSIGNEE);
		}

		public void TestBOX8CONSIGNEESECURITY()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { SecurityConsignee = GetAddress() };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("WTG, 5 Rockingham Drive, Milton Keynes, MK13 7UQ, GB", wrapper.BOX8CONSIGNEESECURITY);
		}

		public void TestBOX444UNDG()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { UNDangerousGoodsCode = "ABC" };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("ABC", wrapper.BOX444UNDG);
		}

		public void TestBOX311MARKS()
		{
			var wrapper = MakeLineAndWrapperWithPackages();
			AssertEquals("M&N\r\nS&M", wrapper.BOX311MARKS);
		}

		public void TestBOX312NUMBERS()
		{
			var wrapper = MakeLineAndWrapperWithPackages();
			AssertEquals("6 - Package\r\n69 - Carton", wrapper.BOX312NUMBERS);
		}

		public void TestBOX314SENSITIVE()
		{
			var wrapper = MakeLineAndWrapperWithSGI();
			AssertEquals("Z\r\nC", wrapper.BOX314SENSITIVE);
		}

		public void TestBOX315SENSITIVEQTY()
		{
			var wrapper = MakeLineAndWrapperWithSGI();
			AssertEquals("XY\r\nAB", wrapper.BOX315SENSITIVEQTY);
		}

		public void TestBOX312DESCRIPTION()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { DescriptionOfGoods = "Daniel" };
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			AssertEquals("Daniel", wrapper.BOX312DESCRIPTION);
		}

		public void TestBOXS28SEALS()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_JobReference = "NCT0001";
			var container11 = header.DepartureHeaderContainers.AddNew();
			container11.BC_ContainerNum = "CONT11";
			var container12 = header.DepartureHeaderContainers.AddNew();
			container12.BC_ContainerNum = "CONT12";
			var container21 = header.DepartureHeaderContainers.AddNew();
			container21.BC_ContainerNum = "CONT21";
			var container22 = header.DepartureHeaderContainers.AddNew();
			container22.BC_ContainerNum = "CONT22";
			container11.BC_Seal1 = "Seal1";
			container11.BC_Seal2 = "Seal2";
			container12.BC_Seal1 = "Seal3";
			container12.BC_Seal2 = "Seal4";
			container21.BC_Seal1 = "Seal5";
			container21.BC_Seal2 = "Seal6";
			container22.BC_Seal1 = "Seal7";
			container22.BC_Seal2 = "Seal8";
			var item1 = header.Bills.AddNew().GoodsItems.AddNew();
			var item2 = header.Bills.AddNew().GoodsItems.AddNew();
			var cont11 = item1.ContainersPivots.AddNew();
			cont11.Container = container11;
			cont11.ContainerSelected = true;
			var cont12 = item1.ContainersPivots.AddNew();
			cont12.Container = container12;
			cont12.ContainerSelected = true;
			var cont21 = item2.ContainersPivots.AddNew();
			cont21.Container = container21;
			cont21.ContainerSelected = true;
			var cont22 = item2.ContainersPivots.AddNew();
			cont22.Container = container22;
			cont22.ContainerSelected = true;

			var headerResponseData = new NctsIE29CusdecResponseData(Factory);
			headerResponseData.LocalReferenceNumber = "NCT0001";
			var responseLine1 = new NctsGoodsItemResponseData(headerResponseData);
			var responseLine2 = new NctsGoodsItemResponseData(headerResponseData);
			responseLine1.ItemNumber = "1";
			responseLine2.ItemNumber = "2";
			responseLine1.ContainerNumbers.Add("CONT11");
			responseLine1.ContainerNumbers.Add("CONT12");
			responseLine2.ContainerNumbers.Add("CONT22");

			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper1 = NctsIE29GoodsItemLineWrapper.New(responseLine1, ediMessage);
			var wrapper2 = NctsIE29GoodsItemLineWrapper.New(responseLine2, ediMessage);
			AssertEquals("Seal1-4", wrapper1.BOXS28SEALS);
			AssertEquals("Seal7-8", wrapper2.BOXS28SEALS);
		}

		void SetUpData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "Bag", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"CT", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"LT", "Lot", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"PK", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();
		}

		AddressResponseData GetAddress()
		{
			return new AddressResponseData() { Address1 = "5 Rockingham Drive", City = "Milton Keynes", CompanyName = "WTG", CountryCode = Core.Constants.CountryCodes.UnitedKingdom, GovRegNum = "not used", Postcode = "MK13 7UQ" };
		}

		NctsIE29GoodsItemLineWrapper MakeLineAndWrapperWithPackages()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData();
			lineItemWithoutAMessage.Packages.Add(new PackageResponseData() { MarksAndNumbers = "M&N", NumberOfPackages = "6", NumberOfPieces = "7", PackageType = "PK" });
			lineItemWithoutAMessage.Packages.Add(new PackageResponseData() { MarksAndNumbers = "S&M", NumberOfPackages = "69", NumberOfPieces = "79", PackageType = "CT" });
			SetUpData();
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			return wrapper;
		}

		NctsIE29GoodsItemLineWrapper MakeLineAndWrapperWithSGI()
		{
			var lineItemWithoutAMessage = new NctsGoodsItemResponseData() { DescriptionOfGoods = "Daniel" };
			lineItemWithoutAMessage.SgiCodes.Add(new SgiCodesResponseData() { Description = "XY", TypeCode = "Z" });
			lineItemWithoutAMessage.SgiCodes.Add(new SgiCodesResponseData() { Description = "AB", TypeCode = "C" });
			var ediMessage = Factory.New<NctsEdiMessage>();
			var wrapper = NctsIE29GoodsItemLineWrapper.New(lineItemWithoutAMessage, ediMessage);
			return wrapper;
		}
	}
}
