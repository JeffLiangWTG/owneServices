using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIStatusSupporter_SupportsPackLevelMessages()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			headerSG.FillWithValidTestData();
			var billSG = headerSG.Bills.AddNew();
			var packSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)billSG.Packs.AddNew();
			var packedItemSG = packSG.PackedItem;
			Assert(((IStatusSupporter)packedItemSG).SupportsPackLevelMessages);

			var headerER = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			headerER.FillWithValidTestData();
			headerER.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var billER = headerER.Bills.AddNew();
			var packER = billER.Packs.AddNew();
			var packedItemER = packER.PackedItemForTesting();
			Assert(!((IStatusSupporter)packedItemER).SupportsPackLevelMessages);

			var headerUS = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var billUS = headerUS.Bills.AddNew();
			var packUS = billUS.Packs.AddNew();
			var packedItemUS = packUS.PackedItemForTesting();
			Assert(!((IStatusSupporter)packedItemUS).SupportsPackLevelMessages);
		}

		public void TestRegistrationNumber_WhenEntryNumberTypeIsNotAsy()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var entryNum = packedItem.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNum.CE_EntryNum = "123";
			AssertEquals("123", packedItem.RegistrationNumber);
			entryNum.CE_EntryType = "";
			AssertEquals("", packedItem.RegistrationNumber);
		}

		public void TestRegistrationEntryNumberIsCorrectType()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var entryNum = packedItem.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			AssertNull(((IBusiness)packedItem).Children.OfType<CusEntryNumber>().FirstOrDefault(x => x.PK == entryNum.PK));
			AssertEquals("", packedItem.RegistrationNumber);
			var registrationEntryNumber = ((IBusiness)packedItem).Children.OfType<CusEntryNumber>().First(x => x.PK == entryNum.PK);
			AssertEquals(entryNum, registrationEntryNumber);
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.CreatePackedItemForTesting();
			var entryNumber = packedItem.CustomsEntryNumbers.AddNew();
			var registrationNumber = CusEntryNumber.New<AsycudaPackedItemEntryNum>(packedItem, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			Factory.Save();

			entryNumber.NotificationsChanged += (sender, e) =>
			{
				if (packedItem.IsDeleted)
				{
					Assert("This is wrong Entry Numbers should not be deleted after the Packed Item", false);
				}
			};

			registrationNumber.NotificationsChanged += (sender, e) =>
			{
				if (packedItem.IsDeleted)
				{
					Assert("This is wrong Registration Number should not be deleted after the Packed Item", false);
				}
			};

			packedItem.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPackedItem)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPackedItemEntryNum)));
		}

		[TestDate(2018, 1, 8)]
		public void TestEffectiveDateForDutyRate()
		{
			var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			headerSG.AMA_E_ARV = new ZDateTime(2018, 1, 2);
			headerSG.AMA_E_DEP = new ZDateTime(2017, 11, 2);
			var billSG = headerSG.Bills.AddNew();
			var packSG = billSG.Packs.AddNew();
			var packedItemSG = packSG.PackedItemForTesting();

			headerSG.AMA_ManifestType = "MGI";
			AssertEquals(new ZDateTime(2018, 1, 2), packedItemSG.EffectiveDateForDutyRate);
			headerSG.AMA_E_ARV = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2018, 1, 8), packedItemSG.EffectiveDateForDutyRate);

			headerSG.AMA_ManifestType = "MGE";
			AssertEquals(new ZDateTime(2017, 11, 2), packedItemSG.EffectiveDateForDutyRate);
			headerSG.AMA_E_DEP = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2018, 1, 8), packedItemSG.EffectiveDateForDutyRate);

			var headerER = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			var billER = headerER.Bills.AddNew();
			var packER = billER.Packs.AddNew();
			var packedItemER = packER.CreatePackedItemForTesting();
			AssertEquals(new ZDateTime(2018, 1, 8), packedItemSG.EffectiveDateForDutyRate);
		}

		public void TestConsingeeFromPackedItem()
		{
			var org = Factory.New<OrgHeader>();
			var companyName = "SNOWSILL, SONYA";
			org.OH_FullName = companyName;
			org.OH_Code = "CGNTEST1";
			org.MainAddress.OA_Address1 = companyName + " STR 1";
			org.MainAddress.OA_Address2 = companyName + " STR 2";
			org.MainAddress.OA_City = companyName + " CITY";
			org.MainAddress.OA_State = companyName + " STATE";
			org.MainAddress.OA_PostCode = "8AU049POST";
			org.CustomsCodes.AddNew("UAN", "8AU0495926", Core.Constants.CountryCodes.Singapore);
			org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, "Y", Core.Constants.CountryCodes.Singapore);

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			header.FillWithValidTestData();
			header.AMA_E_DEP = new ZDateTime(2017, 12, 2);

			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = org.MainAddress.PK;

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			AssertEquals(org.PK, packedItem.Consignee.PK);
		}

		public void TestMakeBillApportionmentDirty()
		{
			AssertApportionmentDirtyOnBill(AutoAsycudaPackedItem.Schema.API_CustomsValue);
			AssertApportionmentDirtyOnBill(AutoAsycudaPackedItem.Schema.API_TaxAmount);
			AssertApportionmentDirtyOnBill(AutoAsycudaPackedItem.Schema.API_DutyAmount);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsBlank_ExpectFalse()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(false, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsERR_ExpectFalse()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(false, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsNOT_ExpectFalse()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(false, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsACP_ExpectTrue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(true, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsAWA_ExpectTrue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(true, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsSNT_ExpectTrue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(true, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsUNK_ExpectTrue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Unknown;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(true, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsUPD_ExpectTrue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
			AssertEquals(false, packedItem.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(packedItem));
			AssertEquals(true, packedItem.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHumanReadableName()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertEquals("Pack Country SG", packedItem.HumanReadableName);
		}

		public void TestIStatusSupporterMembers()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			IStatusSupporter supporter = packedItem;
			supporter.CustomsStatus = "C1";
			supporter.MessageStatus = "M1";
			AssertEquals("country.API_PackStatus", "C1", packedItem.API_PackStatus);
			AssertEquals("country.API_MessageStatus", "M1", packedItem.API_MessageStatus);
			supporter.CustomsStatus = "C2";
			supporter.MessageStatus = "M3";
			AssertEquals("country.API_PackStatus", "C2", packedItem.API_PackStatus);
			AssertEquals("country.API_MessageStatus", "M3", packedItem.API_MessageStatus);
		}

		public void TestPurgeCusEntryNumbersWhenCountryIsDeleted()
		{
			var country = Factory.New<AsycudaPackedItem>();

			var entryNum1 = country.CustomsEntryNumbers.AddNew();
			var entryNum2 = country.CustomsEntryNumbers.AddNew();

			AssertEquals(true, country.CustomsEntryNumbers.Any());
			AssertEquals(false, entryNum1.IsDeleted);
			AssertEquals(false, entryNum2.IsDeleted);

			country.Delete();

			AssertEquals(true, country.IsDeleted);
			AssertEquals(true, entryNum1.IsDeleted);
			AssertEquals(true, entryNum2.IsDeleted);
		}

		public void TestAPI_TariffIsUnformatted()
		{
			const string formattedTariff = "4901.99.90";
			const string unformattedTariff = "49019990";
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			packedItem.API_Tariff = formattedTariff;
			AssertEquals(unformattedTariff, packedItem.API_Tariff);
			AssertEquals(formattedTariff, packedItem.API_FormattedTariff);
			packedItem.API_Tariff = unformattedTariff;
			AssertEquals(unformattedTariff, packedItem.API_Tariff);
			AssertEquals(formattedTariff, packedItem.API_FormattedTariff);
			packedItem.API_Tariff = "12 34.56. 78";
			AssertEquals("12345678", packedItem.API_Tariff);
			AssertEquals("1234.56.78", packedItem.API_FormattedTariff);
		}

		public void TestImportAPI_TariffDefaults()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();

			packedItem.API_Tariff = "4901.99.90";
			AssertEquals("CT", packedItem.GoodsType);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			packedItem.API_Tariff = ZString.Empty;
			AssertEquals("NT", packedItem.GoodsType);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			packedItem.API_Tariff = "8703.23.92";
			AssertEquals("ME", packedItem.GoodsType);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billSG.SG_PartyStatus = "Y";
			packedItem.API_Tariff = ZString.Empty;
			AssertEquals("ME", packedItem.GoodsType);
		}

		public void TestExportAPI_TariffDefaults()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGE";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();

			packedItem.API_Tariff = "4901.99.90";
			AssertEquals("NT", packedItem.GoodsType);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			packedItem.API_Tariff = ZString.Empty;
			AssertEquals("NT", packedItem.GoodsType);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			packedItem.API_Tariff = "9306.30.19";
			AssertEquals("CT", packedItem.GoodsType);
		}

		public void TestGSTCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			((Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill).CycleDate = ZDateTime.Today;
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();

			//Test Duty Changes update GST
			AssertEquals(0m, packedItem.API_TaxAmount);
			packedItem.API_CustomsValue = 400.34;
			AssertEquals(28.02m, packedItem.API_TaxAmount);
			packedItem.API_Tariff = "2203.00.10";
			packedItem.API_CustomsUQ = "LPA";
			packedItem.API_CustomsQty = 6m;
			AssertEquals(0m, packedItem.API_DutyAmount);
			AssertEquals(28.02m, packedItem.API_TaxAmount);

			//Test Customs Value Changes update GST
			AssertEquals(28.02m, packedItem.API_TaxAmount);
			packedItem.API_CustomsValue = 900.34;
			AssertEquals(63.02m, packedItem.API_TaxAmount);
		}

		public void TestAPI_CustomsQuantitiesDecimalPlaces()
		{
			const decimal decimalValueN19N6 = 1234567890123.123456m;

			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.API_CustomsQty = decimalValueN19N6;
			packedItem.API_CustomsQty2 = decimalValueN19N6;
			packedItem.API_CustomsQty3 = decimalValueN19N6;

			Factory.Save();

			var loadedPackedItem = Factory.Load<AsycudaPackedItem>(packedItem.PK);
			CombineAssertions("Customs Quantities should be in the format n19.n6 (accurate to 6.p)", () =>
			{
				AssertEquals(nameof(loadedPackedItem.API_CustomsQty), decimalValueN19N6, loadedPackedItem.API_CustomsQty);
				AssertEquals(nameof(loadedPackedItem.API_CustomsQty2), decimalValueN19N6, loadedPackedItem.API_CustomsQty2);
				AssertEquals(nameof(loadedPackedItem.API_CustomsQty3), decimalValueN19N6, loadedPackedItem.API_CustomsQty3);
			});
		}

		public void TestAPI_CustomsValueDecimalPlaces()
		{
			var packedItem = GetPackedItem(Core.Constants.CountryCodes.VietNam, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for VND", 0, packedItem.API_CustomsValueDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Singapore, "MGI");
			AssertEquals("CustomsValueDecimalPlaces is 0 for SGD", 2, packedItem.API_CustomsValueDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Iraq, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for IQD", 3, packedItem.API_CustomsValueDecimalPlaces);
		}

		public void TestAPI_DutyAmountDecimalPlaces()
		{
			var packedItem = GetPackedItem(Core.Constants.CountryCodes.VietNam, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for VND", 0, packedItem.API_DutyAmountDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Singapore, "MGI");
			AssertEquals("CustomsValueDecimalPlaces is 0 for SGD", 2, packedItem.API_DutyAmountDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Iraq, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for IQD", 3, packedItem.API_DutyAmountDecimalPlaces);
		}

		public void TestAPI_TaxAmountDecimalPlaces()
		{
			var packedItem = GetPackedItem(Core.Constants.CountryCodes.VietNam, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for VND", 0, packedItem.API_TaxAmountDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Singapore, "MGI");
			AssertEquals("CustomsValueDecimalPlaces is 0 for SGD", 2, packedItem.API_TaxAmountDecimalPlaces);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Iraq, "ASY");
			AssertEquals("CustomsValueDecimalPlaces is 0 for IQD", 3, packedItem.API_TaxAmountDecimalPlaces);
		}

		public void TestLogMessageStatusChangeEventOnParent()
		{
			var packedItem = GetPackedItem(Core.Constants.CountryCodes.VietNam, "ASY");
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, MessageStatusCodeList.Codes.Sent);

			Assert(packedItem.Pack.Logs.HasLogWith(logQuery));
		}

		public void TestLogCustomsManifestStatusEventOnParent()
		{
			var packedItem = GetPackedItem(Core.Constants.CountryCodes.VietNam, "ASY");
			packedItem.API_PackStatus = "8";

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsManifestStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "8");

			Assert(packedItem.Pack.Logs.HasLogWith(logQuery));
		}

		public void TestReadonlyForStatus()
		{
			var pack = (AsycudaPackedItem)GetNewBusinessObject();
			Assert(pack.API_MessageStatusInfo.ReadOnly);
			Assert(pack.API_PackStatusInfo.ReadOnly);
		}

		public void TestCustomsPackTypeConversionUsesValidPackLineDetailsWhenTariffHasInvalidUnitOfQuantity()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "BAG";
			var packedItem = pack.GetSGPackedItemForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 5m, packedItem.API_CustomsQty);
				AssertEquals("Unit of Quantity", "BAG", packedItem.API_CustomsUQ);
			});
		}

		public void TestCustomsPackTypeConversionUsesValidPackLineDetailsWhenRefPackHasInvalidCustomsUnitOfQuantity()
		{
			SetupRefPack("BOX", "DEF", 1m);
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "BOX";
			var packedItem = pack.GetSGPackedItemForTesting();
			packedItem.API_Tariff = "1234567890";
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 5m, packedItem.API_CustomsQty);
				AssertEquals("Unit of Quantity", "BOX", packedItem.API_CustomsUQ);
			});
		}

		public void TestCustomsPackTypeConversionFallbackToDefaultWithInvalidTariffUQRefPackUQAndPackLineUQ()
		{
			SetupRefPack("ABC", "DEF", 1m);
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "ABC";
			var packedItem = pack.GetSGPackedItemForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 5m, packedItem.API_CustomsQty);
				AssertEquals("Unit of Quantity", "NMB", packedItem.API_CustomsUQ);
			});
		}

		public void TestIUnitConverterDataProviderType()
		{
			var pack = (AsycudaPackedItem)GetNewBusinessObject();
			AssertEquals("Pack Conversion Type should be Global Manifest Line", RPTypeList.Codes.GlobalManifestLine, ((IUnitConverterDataProvider)pack).Type);
		}

		public void TestUNDGs()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "1009", "B", "IMO").FirstOrDefault();
			var packedItem = pack.PackedItem;
			packedItem.UNDGs.FirstItemForBinding[0].DI_DG = subs.PK;
			packedItem.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			AssertEquals("1009b", packedItem.UNDGs.FirstItemForBinding[0].SubstanceCode);
		}

		public void TestCaption()
		{
			var pack = (AsycudaPackedItem)GetNewBusinessObject();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(pack.API_RN_NKGoodsOriginInfo,
				multipleResourceKeys: null, "Goods Origin Country/Region", "Origin", "Goods Origin Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the origin of the goods.");
		}

		public void TestAPI_CustomsUQMaxLength()
		{
			var maxLength = 6;

			CombineAssertions(() =>
			{
				AssertEntity<AsycudaPackedItem>()
					.HasProperty(x => x.API_CustomsUQ)
					.WithMaxLength(maxLength);

				AssertEntity<AsycudaPackedItem>()
					.HasProperty(x => x.API_CustomsUQ2)
					.WithMaxLength(maxLength);

				AssertEntity<AsycudaPackedItem>()
					.HasProperty(x => x.API_CustomsUQ3)
					.WithMaxLength(maxLength);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = (AsycudaManifestHeader)factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			return packedItem;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "49019990", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var com1 = helper.CreateCommodity(tariff1, "CUPBOKEXE", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "93063019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff2, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "VAL");
			var com2 = helper.CreateCommodity(tariff2, "ANEXPL018", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com2);
			helper.CreateTariffAttribute("ISEXPORTCONTROL", "Y", com2);
			helper.CreateTariffAttribute("ISTRANSHIPMENTCONTROL", "Y", com2);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "87032392", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff3, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			helper.CreateTariffAttribute("COMMODITYTYPE", "VEH", tariff3);
			Factory.Save();
		}

		void SetupRefPack(ZString commercialUnitOfQuantity, ZString customsUnitOfQuantity, ZDecimal conversionFactor)
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = commercialUnitOfQuantity;
			refPack.RP_CustomsPack = customsUnitOfQuantity;
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.Singapore;
			refPack.RP_ConversionFactor = conversionFactor;
			Factory.Save();
		}

		void AssertApportionmentDirtyOnBill(string propertyName)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");
			Factory.Save();

			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var header = packedItem.Header;
			header.AMA_ManifestType = "MGE";
			var bill = packedItem.Pack.Bill;

			bill.ApportionmentDirty = false;

			packedItem[propertyName] = 0m;
			AssertEquals("Should be false as there is no change on " + propertyName, false, bill.ApportionmentDirty);

			packedItem[propertyName] = 12m;
			AssertEquals("Should be true as there is a change on " + propertyName, true, bill.ApportionmentDirty);

			bill.ApportionmentDirty = false;

			packedItem[propertyName] = 12m;
			AssertEquals("Should be false as the API_RN_NKCountry is not SG.", false, bill.ApportionmentDirty);
		}

		AsycudaPackedItem GetPackedItem(ZString countryCode, ZString manifestType)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, countryCode, manifestType);
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RN_NKCountry = countryCode;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			return packedItem;
		}
	}
}
