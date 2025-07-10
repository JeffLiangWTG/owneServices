using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Money = Enterprise.MasterFiles.Business.Money;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestAdditionalInfosDoesNotCauseExceptionWhenLoadChildEditableObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var info = bill.AdditionalDocuments.AddNew();
			info.CSI_SubType = "TST";
			info.CSI_Code = "INF";
			info.CSI_ReferenceNumber = "RN001";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedHeader = anotherFactory.Load<AsycudaManifestHeader>(header.PK);

			AssertNoExceptionThrown(() => loadedHeader.LoadChildEditableObjects());
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals(typeof(RequestedDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.InstructionRequestedDocument]);
			});
		}

		public void TestAdditionalDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AdditionalDocumentCollection<AdditionalDocument>>(bill.AdditionalDocuments);
		}

		public void TestSupportingDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(bill.SupportingDocuments);
		}

		public void TestPreviousDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<PreviousDocumentCollection<PreviousDocument>>(bill.PreviousDocuments);
		}

		public void TestRequestedDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var requestedDocument1 = Factory.New<RequestedDocument>();
			requestedDocument1.CSI_Type = CusSupportingInfoTypeList.Codes.InstructionRequestedDocument;
			requestedDocument1.CSI_ParentTableCode = AsycudaBillSchema.Constants.Prefix;
			requestedDocument1.CSI_ParentID = bill.PK;

			AssertEquals(1, bill.RequestedDocuments.Count);
			AssertEquals(true, bill.RequestedDocuments.ReadOnly);
		}

		public void TestDefaultShipmentType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("Shipment type is defaulted to normal declaration", SubStyleCodeList.Codes.NormalDeclaration, bill.ABL_ShipmentType);
		}

		public void TestDefaultABL_SellerRegNoType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("ABL_SellerRegNoType is defaulted to IOS", OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration, bill.ABL_SellerRegNoType);
		}

		public void TestABL_BillStatus()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.ABL_BillStatusInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Customs Status", resourceStringDataAttribute.Caption);
				Assert("ReadOnly", bill.ABL_BillStatusInfo.ReadOnly);
			});
		}

		public void TestABL_ShipmentType_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.ABL_ShipmentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Additional Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("[11 02 001 000] Additional Declaration Type", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestABL_Procedure_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.ABL_ProcedureInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Additional Procedure(s)", resourceStringDataAttribute.Caption);
				AssertEquals("Add. Proc.", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ACP", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Code identifying any relevant additional procedure(s) associated with the bill.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestABL_ConsigneeRegNoType_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.ABL_ConsigneeRegNoTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("ID NO. Type", resourceStringDataAttribute.Caption);
		}

		public void TestConsigneeRegNoAndType_CountryCodeIsIE()
		{
			TestConsigneeRegNoAndType("IE", "EOR", "IE1234560", "EOR", "CGT", "ITX", "PYE");
			TestConsigneeRegNoAndType("IE", "CGT", "1234560", "CGT", "ITX", "PYE");
			TestConsigneeRegNoAndType("IE", "ITX", "1234560", "ITX", "PYE");
			TestConsigneeRegNoAndType("IE", "PYE", "1234560", "PYE");
		}

		public void TestConsigneeRegNoAndType_CountryCodeIsNotIE()
		{
			TestConsigneeRegNoAndType("AU", "EOR", "AU1234560", "EOR", "CGT", "ITX", "PYE");
			TestConsigneeRegNoAndType("AU", "", "", "CGT", "ITX", "PYE");
			TestConsigneeRegNoAndType("AU", "", "", "ITX", "PYE");
			TestConsigneeRegNoAndType("AU", "", "", "PYE");
		}

		void TestConsigneeRegNoAndType(string countryCode, string expectedType, string expectedRegNo, params string[] types)
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_Consignee = GetOrgAddress(countryCode, types).PK;

			AssertEquals(expectedType, bill.ABL_ConsigneeRegNoType);
			AssertEquals(expectedRegNo, bill.ABL_ConsigneeRegNo);
		}

		public void TestABL_ShipperRegNo()
		{
			var orgWithIEEOR = Factory.New<OrgHeader>();
			orgWithIEEOR.CustomsCodes.AddNew("EOR", "1234_1", "IE");

			var orgWithIEEORStartsWithIE = Factory.New<OrgHeader>();
			orgWithIEEORStartsWithIE.CustomsCodes.AddNew("EOR", "IE1234_2", "IE");

			var orgWithGBAndIEEOR = Factory.New<OrgHeader>();
			orgWithGBAndIEEOR.CustomsCodes.AddNew("EOR", "5678_3", "GB");
			orgWithGBAndIEEOR.CustomsCodes.AddNew("EOR", "1234_3", "IE");

			var orgWithGBEOR = Factory.New<OrgHeader>();
			orgWithGBEOR.CustomsCodes.AddNew("EOR", "5678_4", "GB");

			var orgWithoutEOR = Factory.New<OrgHeader>();
			orgWithoutEOR.CustomsCodes.AddNew("PAS", "1234_5", "IE");

			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				bill.ABL_OA_Shipper = orgWithIEEOR.MainAddress.PK;
				AssertEquals(nameof(orgWithIEEOR), "IE1234_1", bill.ABL_ShipperRegNo);
				AssertEquals("EOR", bill.ABL_ShipperRegNoType);

				bill.ABL_OA_Shipper = orgWithIEEORStartsWithIE.MainAddress.PK;
				AssertEquals(nameof(orgWithIEEORStartsWithIE), "IE1234_2", bill.ABL_ShipperRegNo);
				AssertEquals("EOR", bill.ABL_ShipperRegNoType);

				bill.ABL_OA_Shipper = orgWithGBAndIEEOR.MainAddress.PK;
				AssertEquals(nameof(orgWithGBAndIEEOR), "IE1234_3", bill.ABL_ShipperRegNo);
				AssertEquals("EOR", bill.ABL_ShipperRegNoType);

				bill.ABL_OA_Shipper = orgWithGBEOR.MainAddress.PK;
				AssertEquals(nameof(orgWithGBEOR), "GB5678_4", bill.ABL_ShipperRegNo);
				AssertEquals("EOR", bill.ABL_ShipperRegNoType);

				bill.ABL_OA_Shipper = orgWithoutEOR.MainAddress.PK;
				AssertEquals(nameof(orgWithoutEOR), ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			});
		}

		public void TestIAISMessageAttacheeMembers()
		{
			var agent = Factory.NewWithValidTestData<GlbStaff>();
			agent.GS_Code = "TTT";

			Factory.Save();

			var bill = GetNewBusinessObject() as AsycudaBill;
			var header = bill.Header;
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_GS_NKCustomsAgent = "TTT";
			bill.ABL_MessageStatus = "MS1";
			bill.ABL_BillStatus = "BS1";

			var dummyEDIMessage = Factory.NewWithValidTestData<EDIMessage>();
			bill.Messages.Add(dummyEDIMessage);

			var aisMessageAttachee = bill as IAISMessageAttachee;
			aisMessageAttachee.MovementReferenceNumberSetter("Test123");
			aisMessageAttachee.SetEntryReleaseDate(ZDateTime.BrettsBirthday);

			CombineAssertions("IAISMessageAttachee Members", () =>
			{
				AssertEquals("RelatedJob", header, aisMessageAttachee.RelatedJob);
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, aisMessageAttachee.Branch.PK);
				AssertEquals("CustomsAgent", agent, aisMessageAttachee.CustomsAgent);
				AssertEquals("LogicalStatus", "MS1", aisMessageAttachee.LogicalStatus);
				AssertEquals("EntryStatus", "BS1", aisMessageAttachee.EntryStatus);
				AssertEquals("Messages", dummyEDIMessage, aisMessageAttachee.Messages.Single());
				AssertEquals("MovementReferenceNumberSetter sets MRN", "Test123", bill.MovementReferenceNumber);
				AssertEquals("EntryReleaseDate", ZDate.BrettsBirthday, bill.ABL_ReleaseDate);
			});
		}

		[TestDate(2024, 02, 04)]
		public void TestGetLRNAndSetIfNeeded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Assert("Pre-condition", bill.LocalReferenceNumber.IsEmpty);

			bill.GetLRNAndSetIfNeeded();
			AssertEquals("EDIDATBNE2400000001V01", bill.LocalReferenceNumber);
		}

		public void TestValidationType()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillValidationForRegularBill>(bizObj.Validation);

			bizObj.ABL_BolType = "BOL";
			AssertType<AsycudaBillValidationForMasterChild>(bizObj.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		OrgAddress GetOrgAddress(string countryCode, params string[] types)
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			var count = 0;
			foreach (var type in types)
			{
				var config = org.CustomsCodes.AddNew();
				config.OK_RN_NKCodeCountry = countryCode;
				config.OK_CodeType = type;
				config.SecuredCustomsRegNo = "123456" + count;
				count++;
			}
			return address;
		}

		public void TestCusGoodsLocationType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusGoodsLocationProvider = bill as ICusGoodsLocationProvider;

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", bill.CusGoodsLocation);
			});
		}

		public void TestTransportAndInsuranceCurrencySynchronization()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_RX_NKTransportValueCurrency = "AUD";
			bill.ABL_TransportValue = 10;
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.ABL_InsuranceValue = 3;

			var converter = new RefCurrencyCurrencyConverter(Factory, ZDateTime.Now, ExchangeRateType.Customs, 15);
			var insuranceMoney = new Money(bill.ABL_InsuranceValue, new Currency(bill.ABL_RX_NKInsuranceValueCurrency));
			var convertedMoney =
				converter.ConvertRounded(insuranceMoney, new Currency(bill.ABL_RX_NKTransportValueCurrency));

			Factory.Save();
			AssertEquals(bill.ABL_InsuranceValue, convertedMoney.Amount);
		}

		public void TestCusGoodsLocationProviderKey()
		{
			var cusGoodsLocationProvider = GetNewBusinessObject() as ICusGoodsLocationProvider;

			AssertEquals("CusGoodsLocationProvider key", "IEH7D", cusGoodsLocationProvider.ProviderKey);
		}

		public void TestTryConvert()
		{
			var bill = BuildBill();
			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill, BuildBill() });

			var newFactory = new BusinessObjectFactory();
			var reloadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			var declaration = newFactory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reloadedBill.EntrySummaryReferenceNumber));

			foreach (var instruction in declaration.CustomsEntryInstructions)
			{
				AssertEquals("H1", instruction.CEI_Style);
				AssertEquals(bill.ABL_ShipmentType, instruction.CEI_SubStyle);
			}
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SellerRegNoType };
		}

		AsycudaBill BuildBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			bill.ABL_BillNumber = "BN123";
			bill.ABL_GoodsDescription = "goods description";
			bill.ABL_GrossWeight = 0.9;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 1.1;
			bill.ABL_VolumeUQ = "L";
			bill.ABL_ManifestQty = 13;
			bill.ABL_GoodsValue = 110;
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_SellerRegNo = "1234";

			pack1.APA_PackUQ = "BX";
			pack2.APA_PackUQ = "FR";
			item1.API_FormattedTariff = "11081990009";
			item1.API_GoodsDescription = "item1 goods description";
			item1.API_RN_NKGoodsOrigin = "AU";
			item1.API_RX_NKGoodsValueCurrency = "AUD";
			item2.API_FormattedTariff = "11081990010";
			item2.API_GoodsDescription = "item2 goods description";
			item2.API_RN_NKGoodsOrigin = "US";
			item2.API_RX_NKGoodsValueCurrency = "USD";
			Factory.Save();
			return bill;
		}
	}
}
