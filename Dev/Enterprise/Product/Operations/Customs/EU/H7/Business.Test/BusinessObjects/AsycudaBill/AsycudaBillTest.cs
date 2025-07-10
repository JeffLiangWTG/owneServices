using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.H7.Business.AsycudaBill;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestLockBillIfConverted()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			bill.EntrySummaryReferenceNumber = "B1234";
			bill.ABL_IsActive = false;
			Factory.Save();

			var billInNewFactory = NewFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(true, billInNewFactory.ReadOnly);
		}

		public void TestLocalReferenceNumber_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.LocalReferenceNumberInfo, multipleResourceKey: null, "Local Reference Number", "LRN", "LRN", "A system-generated local reference number to uniquely identify each single declaration.");
		}

		public void TestIDataGroupingProviderMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.FrenchGuyana))
			{
				var bill = GetNewBusinessObject() as AsycudaBill;
				IDataGroupingProvider provider = bill;
				AssertEquals("bill.DataGrouping", Constants.CountryCodes.France, bill.DataGrouping);
				AssertEquals("provider.DataGrouping", Constants.CountryCodes.France, provider.DataGrouping);
			}
		}

		public void TestLocalReferenceNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var header = bill.Header;
			AssertEquals("PreCondition: header.AMA_RN_NKCountry", Core.Constants.CountryCodes.Latvia, header.AMA_RN_NKCountry);
			var lrnGF = Common.CusEntryNumber.LoadOrCreate(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Latvia);
			lrnGF.CE_EntryNum = "GF1234";
			var lrnFR = Common.CusEntryNumber.LoadOrCreate(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.France);
			lrnFR.CE_EntryNum = "FR1234";
			AssertEquals("LocalReferenceNumber should use Customs country", "GF1234", bill.LocalReferenceNumber);
			var bill2 = header.Bills.AddNew();
			bill2.LocalReferenceNumber = "LRN5678";
			var bill2LRN = CusEntryNumber.Load(bill2).Single(x => x.CE_EntryType == CusEntryNumberTypes.Standard.LocalReferenceNumber);
			AssertEquals("bill2LRN.CE_RN_NKCountryCode", Core.Constants.CountryCodes.Latvia, bill2LRN.CE_RN_NKCountryCode);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(AdditionalDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalDocument]);
				AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalInfos()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(bill.AdditionalInfos);
		}

		public void TestAdditionalDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AdditionalDocumentCollection<AdditionalDocument>>(bill.AdditionalDocuments);
		}

		public void TestPreviousDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<PreviousDocumentCollection<PreviousDocument>>(bill.PreviousDocuments);
		}

		public void TestSupportingDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(bill.SupportingDocuments);
		}

		public void TestABLSellerRegNoTypeInfoReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			Assert("Readonly", bill.ABL_SellerRegNoTypeInfo.ReadOnly);
		}

		public void TestABLBillStatusInfoReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			Assert("Readonly", bill.ABL_BillStatusInfo.ReadOnly);
		}

		public void TestABLSellerRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("SellerRegNoTypes length should be 1", 1, bill.SellerRegNoTypes().Length);
			AssertEquals("SellerRegNoTypes element should be IOS", OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration, bill.SellerRegNoTypes()[0]);
		}

		public void TestMovementReferenceNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.MovementReferenceNumberInfo, multipleResourceKey: null, "Movement Reference Number", "MRN", "MRN", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.");
			Assert("Readonly", bill.MovementReferenceNumberInfo.ReadOnly);

			AssertEquals("When No MRN CusEntryNumber Records, Returns empty string", ZString.Empty, bill.MovementReferenceNumber);
			AssertEquals("Begin with no entries in CustomsEntryNumbers", 0, bill.CustomsEntryNumbers.Count);

			bill.MovementReferenceNumber = "";
			AssertEquals("Setting blank MRN does not add entry for CustomsEntryNumbers", 0, bill.CustomsEntryNumbers.Count);

			bill.MovementReferenceNumber = "01234";
			AssertEquals("Setting MRN does add entry for CustomsEntryNumbers", 1, bill.CustomsEntryNumbers.Count);
			AssertEquals("MRN", bill.CustomsEntryNumbers[0].CE_EntryType);

			AssertEquals("01234", bill.MovementReferenceNumber);
			Factory.Save();

			var newFactory = NewFactory();
			var billInNewFactory = newFactory.Load(AsycudaBillSchema.Constants.Prefix, bill.PK) as AsycudaBill;
			newFactory.Save();
			AssertEquals("Saving bill does not remove MRN entry", "01234", billInNewFactory.MovementReferenceNumber);
		}

		public void TestMovementReferenceNumber_DependsOnEntryType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			var wrongTypeNum = bill.CustomsEntryNumbers.AddNew();
			wrongTypeNum.CE_EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber;
			wrongTypeNum.CE_EntryNum = "11111";
			wrongTypeNum.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			var mrNum = bill.CustomsEntryNumbers.AddNew();
			mrNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			mrNum.CE_EntryNum = "01234";
			mrNum.CE_SystemCreateTimeUtc = ZDateTime.Today;
			Assert("Precondition: tested CusEntryNum needs to have creation time after wrong one", wrongTypeNum.CE_SystemCreateTimeUtc < mrNum.CE_SystemCreateTimeUtc);

			AssertEquals("01234", bill.MovementReferenceNumber);
		}

		public void TestCustomsEntryNumbers_Validation()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_ConsigneeRegNo = "PL123456789123456";
			bill.ABL_SellerRegNo = "IM1234567891";

			var country = bill.Header.AMA_RN_NKCountry;
			var lrn = Common.CusEntryNumber.New<CusEntryNumber>(bill, CusEntryNumberTypes.EU.LocalReferenceNumber, country);
			lrn.CE_EntryNum = "LRN1234";
			bill.LocalReferenceNumber = "LRN1234";
			var mrn = Common.CusEntryNumber.New<CusEntryNumber>(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, country);
			mrn.CE_EntryNum = "MRN000";

			Factory.Save();

			var newFactory = NewFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "Customs Entry Number Types");
			helper.CreateNewOrGetExistingCusCodeList(country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "PRV", "Customs Entry Number Types", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "PL", "Poland", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			newFactory.Save();

			var billInNewFactory = newFactory.Load<AsycudaBill>(bill.PK);
			AssertGreaterThan("Precondition: Ensure there are entries in the list, to trigger list validation", billInNewFactory.Lookups.CustomsEntryNumberTypes.Count, 0);

			CombineAssertions(() =>
			{
				AssertEquals("MRN and LRN should be identified as CustomsEntryNumbers", 2, billInNewFactory.CustomsEntryNumbers.Count);
				billInNewFactory.RunPreSaveValidation();
				AssertNoMessageErrors(billInNewFactory.CustomsEntryNumbers[0]);
				AssertNoMessageErrors(billInNewFactory.CustomsEntryNumbers[1]);
			});

			var invalidEntryType = billInNewFactory.CustomsEntryNumbers.AddNew();
			invalidEntryType.CE_EntryType = "PRV";
			AssertHasMessageError(invalidEntryType.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestMRNIsIdentifiedWithCustomsEntryNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var country = bill.Header.AMA_RN_NKCountry;
			var lrn = Common.CusEntryNumber.New<CusEntryNumber>(bill, CusEntryNumberTypes.EU.LocalReferenceNumber, country);
			lrn.CE_EntryNum = "LRN1234";
			lrn.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			bill.LocalReferenceNumber = "LRN1234";
			var mrn = Common.CusEntryNumber.New<CusEntryNumber>(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, country);
			mrn.CE_EntryNum = "MRN000";
			mrn.CE_SystemCreateTimeUtc = ZDateTime.Today;

			Factory.Save();

			var newFactory = NewFactory();
			var billInNewFactory = newFactory.Load<AsycudaBill>(bill.PK);
			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryNumber", "MRN000", billInNewFactory.CustomsEntryNumber);
				AssertEquals("CustomsEntryNumberType", "MRN", billInNewFactory.CustomsEntryNumberType);
				AssertEquals("MovementReferenceNumber", "MRN000", billInNewFactory.MovementReferenceNumber);
			});
		}

		public void TestEntrySummaryReferenceNumberReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			Assert("Readonly", bill.EntrySummaryReferenceNumberInfo.ReadOnly);
		}

		public void TestBillIsReadOnlyWhenHasBeenConvertedToStandAloneDeclaration()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(string.Empty, bill.EntrySummaryReferenceNumber);
			AssertEquals(false, bill.HasBeenConvertedToStandaloneDeclaration);
			AssertEquals(false, bill.ReadOnly);

			bill.EntrySummaryReferenceNumber = "B1234";
			bill.ABL_IsActive = false;
			Factory.Save();

			var billInNewFactory = NewFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(true, billInNewFactory.HasBeenConvertedToStandaloneDeclaration);
			AssertEquals(true, billInNewFactory.ReadOnly);
		}

		public void TestCanConvertToAndEditStandAloneDeclaration()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(string.Empty, bill.EntrySummaryReferenceNumber);
			AssertEquals(true, bill.CanConvertToStandAloneDeclaration);
			AssertEquals(false, bill.CanEditStandAloneDeclaration);
			AssertEquals(false, bill.ReadOnly);

			bill.EntrySummaryReferenceNumber = "B1234";
			bill.ABL_IsActive = false;
			Factory.Save();

			var billInNewFactory = NewFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(true, billInNewFactory.HasBeenConvertedToStandaloneDeclaration);
			AssertEquals(false, billInNewFactory.CanConvertToStandAloneDeclaration);
			AssertEquals(true, billInNewFactory.CanEditStandAloneDeclaration);
			AssertEquals(true, billInNewFactory.ReadOnly);
		}

		public void TestStandAloneDeclarationRetrievesDeclaration()
		{
			var referenceNumber = "B1234";
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.EntrySummaryReferenceNumber = referenceNumber;
			bill.ABL_IsActive = false;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = referenceNumber;
			Factory.Save();

			var query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, referenceNumber);
			var expectedDeclaration = Factory.LoadTop1<JobDeclaration>(query);

			AssertEquals(expectedDeclaration, bill.StandAloneDeclaration);
		}

		public void TestABLEntryNumCreatedOnlyWhenSettingEntrySummaryReferenceNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(string.Empty, bill.EntrySummaryReferenceNumber);
			var ablEntryNumbers = Factory.Load<ABLEntryNum>(new ZQuery());
			AssertEquals(0, ablEntryNumbers.Length);

			bill.EntrySummaryReferenceNumber = "B000001";
			var newAblEntryNumber = Factory.Load<ABLEntryNum>(new ZQuery()).Single();
			AssertEquals("ENS", newAblEntryNumber.CE_EntryType);
			AssertEquals("B000001", newAblEntryNumber.CE_EntryLineReference);
		}

		public void TestTransportValueCaptions()
		{
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_TransportValueInfo, multipleResourceKey: null, "Transport Value", "Transp. Val.", "Transp. Val.", "Transport value of the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RX_NKTransportValueCurrencyInfo, multipleResourceKey: null, "Transport Value Currency", "Curr.", "Currency", "Currency code associated with the Transport Value.");
			});
		}

		public void TestABL_SellerRegNoTypeCaptions()
		{
			var bill = Factory.New<AsycudaBill>();
			var sellerRegNoType = DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerRegNoTypeInfo);

			CombineAssertions(() =>
			{
				AssertEquals("IOSS Number", sellerRegNoType.Caption);
				AssertEquals("IOSS No.", sellerRegNoType.ShortCaption);
			});
		}

		public void TestMessageStatusCaptions()
		{
			var bill = Factory.New<AsycudaBill>();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_MessageStatusInfo, multipleResourceKey: null, "Message Status", "Msg. Status", "Msg. Status", "Code identifying the status of the last customs declaration message sent to the relevant Customs authority.");
		}

		public void TestCustomsStatusCaptions()
		{
			var bill = Factory.New<AsycudaBill>();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_BillStatusInfo, multipleResourceKey: null, "Customs Status", "Cus. Status", "Cus. Status", "Code identifying the customs status of the declaration.");
		}

		public void TestLogEventWhenUpdatingCustomsStatus()
		{
			var billNotConverted = Factory.NewWithValidTestData<AsycudaBill>();

			var billConvertedFromOther = Factory.NewWithValidTestData<AsycudaBill>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			billConvertedFromOther.Header.Logs.AddNew(AutoEvents.EditedARecord, "|JOB=123|TYP=HVL", ZDateTimeOffset.UtcNow, []);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			billConvertedFromOther.Header.Logs.AddNew(AutoEvents.Transferred, ZDateTimeOffset.UtcNow);
			billConvertedFromOther.Header.Logs.AddNew(AutoEvents.Transferred, "|JOB=123|TYP=OTH", ZDateTimeOffset.UtcNow, []);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var billConvertedFirstTime = header.Bills.AddNew();
			billConvertedFirstTime.ABL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			header.Logs.AddNew(AutoEvents.Transferred, "|JOB=123|TYP=HVL", ZDateTimeOffset.UtcNow, []);

			var billConvertedSecondTime = header.Bills.AddNew();
			billConvertedSecondTime.ABL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			header.Logs.AddNew(AutoEvents.Transferred, "|JOB=123|TYP=HVL", ZDateTimeOffset.UtcNow, []);

			var billCreatedAfterLastConversion = header.Bills.AddNew();
			billCreatedAfterLastConversion.ABL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(1);

			CombineAssertions(() =>
			{
				billNotConverted.ABL_BillStatus = "ACC";
				AssertCustomsStatusChangeEventLog("Not converted from any module", billNotConverted, false);

				billConvertedFromOther.ABL_BillStatus = "ACC";
				AssertCustomsStatusChangeEventLog("Not converted from HVLV", billConvertedFromOther, false);

				billConvertedFirstTime.ABL_BillStatus = "CLR";
				AssertCustomsStatusChangeEventLog("Converted from HVLV first time", billConvertedFirstTime, true, "|SER=PCS|TYP=CLR");

				billConvertedFirstTime.ABL_BillStatus = "CLR";
				AssertCustomsStatusChangeEventLog("Customs status has not been changed", billConvertedFirstTime, true);

				billConvertedSecondTime.ABL_BillStatus = "CLR";
				AssertCustomsStatusChangeEventLog("Converted from HVLV second time", billConvertedSecondTime, true, "|SER=PCS|TYP=CLR");

				billCreatedAfterLastConversion.ABL_BillStatus = "ACC";
				AssertCustomsStatusChangeEventLog("Created after converted", billCreatedAfterLastConversion, false);
			});
		}

		void AssertCustomsStatusChangeEventLog(string message, AsycudaBill bill, bool shouldHaveLog, string expectedReference = "")
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, bill.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);

			if (shouldHaveLog)
			{
				AssertEquals(message + " - Should have exactly one log entry", 1, logs.Length);

				if (!string.IsNullOrEmpty(expectedReference))
				{
					AssertEquals(message + " - Expected log reference",
						expectedReference, logs.Single().SL_Reference);
				}
			}
			else
			{
				AssertEquals(message + " - Should not have any log entries", 0, logs.Length);
			}
		}

		public void TestPropertiesWithDecimalPlaces()
		{
			AssertEntity<AsycudaBill>()
				.HasProperty(x => x.ABL_TransportValue)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 2);

			AssertEntity<AsycudaBill>()
				.HasProperty(x => x.ABL_InsuranceValue)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 2);

			AssertEntity<AsycudaBill>()
				.HasProperty(x => x.ABL_CustomsValue)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 2);

			AssertEntity<AsycudaBill>()
				.HasProperty(x => x.ABL_GoodsValue)
				.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 2);
		}

		public void TestInsuranceValueCaptions()
		{
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RX_NKInsuranceValueCurrencyInfo, multipleResourceKey: null, "Insurance Value Currency", "Curr.", "Currency", "Currency code associated with the Insurance Value.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_InsuranceValueInfo, multipleResourceKey: null, "Insurance Value", "Ins. Val.", "Ins. Val.", "Insurance value of the goods.");
			});
		}

		public void TestExporterEntityAttributes()
		{
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				AssertEquals(70, bill.ABL_ShipperNameInfo.MaxLength);
				AssertEquals(35, bill.ABL_ShipperCityInfo.MaxLength);
				AssertEquals(17, bill.ABL_ShipperRegNoInfo.MaxLength);
			});
		}

		public void TestImporterEntityAttributes()
		{
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				AssertEquals(70, bill.ABL_ConsigneeNameInfo.MaxLength);
				AssertEquals(35, bill.ABL_ConsigneeCityInfo.MaxLength);
			});
		}

		public void TestABL_UCRNumberMaxLength()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(35, bill.ABL_UCRNumberInfo.MaxLength);
		}

		public void TestCloneBill()
		{
			var bill = SetupBillForClone();
			SetupItemAndPack(bill);
			Factory.Save();

			var clonedBill = (AsycudaBill)bill.Clone();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(1, (int)clonedBill.ABL_SequenceNumber);
				AssertEquals(string.Empty, clonedBill.ABL_BillNumber);
				AssertEquals("CN", clonedBill.ABL_RL_NKOrigin);
				AssertEquals("AU", clonedBill.ABL_RL_NKFinalDestination);
				AssertEquals("Description", clonedBill.ABL_GoodsDescription);
				AssertEquals(string.Empty, clonedBill.ABL_UCRNumber);
				AssertEquals(string.Empty, clonedBill.CustomsJobNumber);
				AssertEquals(string.Empty, clonedBill.MovementReferenceNumber);
				AssertEquals(5m, clonedBill.ABL_GoodsValue);
				AssertEquals("AUD", clonedBill.ABL_RX_NKGoodsValueCurrency);
				AssertEquals(string.Empty, clonedBill.ABL_MessageStatus);
				AssertEquals(string.Empty, clonedBill.ABL_BillStatus);
				AssertEquals("reference", clonedBill.ABL_CarrierReference);
				AssertEquals("CCL", clonedBill.ABL_PrepaidCollect);
				AssertEquals(10m, clonedBill.DiscountValue);
				AssertEquals("AUD", clonedBill.DiscountValueCurrency);
				AssertEquals(10m, clonedBill.OtherChargesValue);
				AssertEquals("AUD", clonedBill.OtherChargesValueCurrency);
				AssertEquals("U", clonedBill.CusGoodsLocation.CGL_Qualifier);
				AssertEquals("A", clonedBill.CusGoodsLocation.CGL_Type);
				AssertEquals("123456", clonedBill.CusGoodsLocation.Unlocode);
				AssertEquals("name", clonedBill.CusGoodsLocation.Address.E2_Contact);
				AssertEquals("123456", clonedBill.CusGoodsLocation.Address.E2_Phone);
				AssertEquals("test@org.com", clonedBill.CusGoodsLocation.Address.E2_Email);
				AssertEquals("A", clonedBill.AdditionalDocuments.Single().CSI_Code);
				AssertEquals("S", clonedBill.SupportingDocuments.Single().CSI_Code);
				AssertEquals("P", clonedBill.PreviousDocuments.Single().CSI_Code);
				AssertEquals(0, clonedBill.RequestedDocuments.Count);
				AssertEquals(0, clonedBill.Messages.Count);
				AssertEquals(2, clonedBill.PackedItems.Count);
				AssertEquals("item des", clonedBill.PackedItems.FirstOrDefault().API_GoodsDescription);
				AssertEquals(true, clonedBill.PackedItems.FirstOrDefault().AsycudaPackPackedItemLinks[0].IsLinked);
				AssertEquals(1, clonedBill.PackedItems.FirstOrDefault().API_LineNo);
				AssertEquals(2, clonedBill.PackedItems.LastOrDefault().API_LineNo);
				AssertEquals(1, clonedBill.Packs.Count);
				AssertEquals("pack des", clonedBill.Packs[0].APA_GoodsDescription);
			});
		}

		AsycudaBill SetupBillForClone()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_SequenceNumber = 1;
			bill.ABL_BillNumber = "123";
			bill.ABL_RL_NKOrigin = "CN";
			bill.ABL_RL_NKFinalDestination = "AU";
			bill.ABL_GoodsDescription = "Description";
			bill.ABL_UCRNumber = "UCR";
			bill.CustomsJobNumber = "345";
			bill.MovementReferenceNumber = "12345";
			bill.ABL_GoodsValue = 5;
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			bill.ABL_MessageStatus = "ACC";
			bill.ABL_BillStatus = "ACC";
			bill.ABL_CarrierReference = "reference";
			bill.ABL_PrepaidCollect = "CCL";
			bill.DiscountValue = 10;
			bill.DiscountValueCurrency = "AUD";
			bill.OtherChargesValue = 10;
			bill.OtherChargesValueCurrency = "AUD";

			var locationOfGoods = bill.CusGoodsLocation;
			locationOfGoods.CGL_Qualifier = "U";
			locationOfGoods.CGL_Type = "A";
			locationOfGoods.Unlocode = "123456";

			var address = locationOfGoods.Address;
			address.E2_Contact = "name";
			address.E2_Phone = "123456";
			address.E2_Email = "test@org.com";

			var additionalDoc = bill.AdditionalDocuments.AddNew();
			additionalDoc.CSI_Code = "A";

			var supportingDoc = bill.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "S";

			var previousDoc = bill.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = "P";

			var requestedDoc = bill.RequestedDocuments.AddNew();
			requestedDoc.CSI_Code = "R";

			var msg = bill.Messages.AddNew();
			var messageNumberStrategyMock = new Mock<IMessageNumberStrategy>();
			msg.MessageNumberStrategy = messageNumberStrategyMock.Object;
			messageNumberStrategyMock.Setup(x => x.GetMessageReferenceNumber()).Returns("ENT1234");

			return bill;
		}

		void SetupItemAndPack(AsycudaBill bill)
		{
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "pack des";

			var item1 = bill.PackedItems.AddNew();
			item1.API_GoodsDescription = "item des";
			item1.ToggleLinkageWithPackage(pack, true);
			var item2 = bill.PackedItems.AddNew();
			AssertEquals(1, item1.API_LineNo);
			AssertEquals(2, item2.API_LineNo);
		}

		public void TestGetReleasedStatus()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(AISEntryStatusList.Codes.Released, bill.GetReleasedStatus());
		}

		public void TestGetReleasedStatusDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(AISEntryStatusList.Descriptions.Released, bill.GetReleasedStatusDescription());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_GrossWeight = 10;
			return bill;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		public void TestCusGoodsLocationType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusGoodsLocationProvider = bill as ICusGoodsLocationProvider;
			
			AssertNotNull("AsycudaBill implements ICusGoodsLocationProvider", cusGoodsLocationProvider);

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", bill.CusGoodsLocation);
			});
		}

		public void TestCusGoodsLocationProviderKey()
		{
			var cusGoodsLocationProvider = GetNewBusinessObject() as ICusGoodsLocationProvider;

			AssertEquals("CusGoodsLocationProvider key", "LVH7D", cusGoodsLocationProvider.ProviderKey);
		}

		public void TestDocManagerInfo()
		{
			var docManagerSupport = GetNewBusinessObject() as IDocManagerSupport;
			AssertType<AsycudaBillDocManagerInfo>(docManagerSupport.DocManagerInfo);
		}

		public void TestCodePropertyAttribute()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_BillNumber = "BillNumber";
			bill.Header.AMA_JobReference = "HeaderJobReference";

			AssertEquals("HeaderJobReference|BillNumber", CodePropertyAttribute.CodeFromBusinessObject(bill));
		}

		public void TestHumanReadableShortcutName()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_BillNumber = "1234";

			AssertEquals("Manifest Bill 1234", bill.HumanReadableShortcutName);
		}

		public void TestABL_Incoterm()
		{
			var bill = Factory.New<AsycudaBill>();
			var incotermData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_IncotermInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Incoterm", incotermData.Caption);
				AssertEquals("INCO", incotermData.MediumCaption);
				AssertEquals("INCO", incotermData.ShortCaption);
				AssertEquals("Incoterm relevant to the commercial invoice.", incotermData.FullDescription);
			});
		}

		public void TestGetSupportingDocSendingObject()
		{
			var bill = Factory.New<AsycudaBill>();
			var sendingObject = bill.GetSupportingDocSendingObject();
			AssertType(typeof(SupportingDocSendingObject), sendingObject);
		}

		public void TestRequestedDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var requestedDocument1 = Factory.New<RequestedDocument>();
			requestedDocument1.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument;
			requestedDocument1.CSI_ParentTableCode = AsycudaBillSchema.Constants.Prefix;
			requestedDocument1.CSI_ParentID = bill.PK;

			AssertEquals(1, bill.RequestedDocuments.Count);
			AssertEquals(true, bill.RequestedDocuments.ReadOnly);
		}

		public void TestIsDocumentationRequested()
		{
			var bill = Factory.New<AsycudaBill>();
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var requestedDocument2 = bill.RequestedDocuments.AddNew();
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			Assert("bill has documentation requested", bill.IsDocumentationRequested);

			var bill2 = Factory.New<AsycudaBill>();
			var requestedDocument3 = bill2.RequestedDocuments.AddNew();
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			Assert("Bill2 does not have documentation requested", !bill2.IsDocumentationRequested);
		}

		public void TestContainerNumber()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ContainerNumber = "C123";
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, bill.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, GenAddOnColumnConstants.ContainerNumberColumnName);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			var containerNumber = Factory.LoadTop1<GenAddOnColumn>(query)?.XA_Data;

			AssertEquals("containerNumber should be stored in GenAddOnColumn", "C123", containerNumber);
		}

		public void TestConsigneeCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ConsigneeOrgPKInfo, multipleResourceKey: null, "Consignee", "CNE", "Consignee", "The party to whom the goods are shipped.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneeNameInfo, multipleResourceKey: null, "Consignee Name", "CNE Name", "Consignee Name", "Consignee full name and where applicable the legal form of the party.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneeStreet1Info, multipleResourceKey: null, "Consignee Street 1", "CNE St. 1", "Consignee Street 1", "Name of the street of the consignee party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneeStreet2Info, multipleResourceKey: null, "Consignee Street 2", "CNE St. 2", "Consignee Street 2", "Name of the street of the consignee party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneeCityInfo, multipleResourceKey: null, "Consignee City", "CNE City", "Consignee City", "City name of the consignee party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneePostcodeInfo, multipleResourceKey: null, "Consignee Postcode", "CNE PC", "Consignee PC", "Postcode of the consignee party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneeStateInfo, multipleResourceKey: null, "Consignee State", "CNE St.", "Consignee State", "State code of the consignee party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RN_NKConsigneeCountryInfo, multipleResourceKey: null, "Consignee Country/Region", "CNE Ctry/Rgn.", "Consignee Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the consignee party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ConsigneePhoneInfo, multipleResourceKey: null, "Consignee Phone", "CNE Ph.", "Consignee Phone", "Telephone number of of the consignee party.");
			});
		}

		public void TestNotifyPartyCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.NotifyPartyOrgPKInfo, multipleResourceKey: null, "Notify Party", "Notify Party", "Notify Party", "The party to be notified at entry of the arrival of the goods, as stipulated in the master bill of lading or master air waybill.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyNameInfo, multipleResourceKey: null, "Notify Party Name", "NP Name", "Notify Party Name", "Notify Party full name and where applicable the legal form of the party.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyStreet1Info, multipleResourceKey: null, "Notify Party Street 1", "NP St. 1", "Notify Party Street 1", "Name of the street of the notify party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyStreet2Info, multipleResourceKey: null, "Notify Party Street 2", "NP St. 2", "Notify Party Street 2", "Name of the street of the notify party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyCityInfo, multipleResourceKey: null, "Notify Party City", "NP City", "Notify Party City", "City name of the notify party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyPostcodeInfo, multipleResourceKey: null, "Notify Party Postcode", "NP PC", "Notify Party PC", "Postcode of the notify party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyStateInfo, multipleResourceKey: null, "Notify Party State", "NP St.", "Notify Party State", "State code of the notify party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RN_NKNotifyPartyCountryInfo, multipleResourceKey: null, "Notify Party Country/Region", "NP Ctry/Rgn.", "Notify Party Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the notify party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_NotifyPartyPhoneInfo, multipleResourceKey: null, "Notify Party Phone", "NP Ph.", "Notify Party Phone", "Telephone number of of the notify party.");
			});
		}

		public void TestSellerCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.SellerOrgPKInfo, multipleResourceKey: null, "Seller", "Seller", "Seller", "The party selling the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerNameInfo, multipleResourceKey: null, "Seller Name", "Sell. Name", "Seller Name", "Seller full name and where applicable the legal form of the party.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerStreet1Info, multipleResourceKey: null, "Seller Street 1", "Sell. St. 1", "Seller Street 1", "Name of the street of the seller party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerStreet2Info, multipleResourceKey: null, "Seller Street 2", "Sell. St. 2", "Seller Street 2", "Name of the street of the seller party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerCityInfo, multipleResourceKey: null, "Seller City", "Sell. City", "Seller City", "City name of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerPostcodeInfo, multipleResourceKey: null, "Seller Postcode", "Sell. PC", "Seller PC", "Postcode of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerStateInfo, multipleResourceKey: null, "Seller State", "Sell. St.", "Seller State", "State code of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RN_NKSellerCountryInfo, multipleResourceKey: null, "Seller Country/Region", "Sell. Ctry/Rgn.", "Seller Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SellerPhoneInfo, multipleResourceKey: null, "Seller Phone", "Sell. Ph.", "Seller Phone", "Telephone number of of the seller party.");
			});
		}

		public void TestShipperCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ShipperOrgPKInfo, multipleResourceKey: null, "Shipper", "Shipper", "Shipper", "The party consigning the goods as stipulated in the transport contract by the party ordering the transport.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperNameInfo, multipleResourceKey: null, "Shipper Name", "Ship. Name", "Shipper Name", "Shipper full name and where applicable the legal form of the party.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperCityInfo, multipleResourceKey: null, "Shipper City", "Ship. City", "Shipper City", "City name of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RN_NKShipperCountryInfo, multipleResourceKey: null, "Shipper Country/Region", "Ship. Ctry/Rgn.", "Shipper Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperPostcodeInfo, multipleResourceKey: null, "Shipper Postcode", "Ship. PC", "Shipper PC", "Postcode of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperStateInfo, multipleResourceKey: null, "Shipper State", "Ship. St.", "Shipper State", "State code of the seller party's address.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperStreet1Info, multipleResourceKey: null, "Shipper Street 1", "Ship. St. 1", "Shipper Street 1", "Name of the street of the seller party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperStreet2Info, multipleResourceKey: null, "Shipper Street 2", "Ship. St. 2", "Shipper Street 2", "Name of the street of the seller party's address and the number of the building or facility.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipperPhoneInfo, multipleResourceKey: null, "Shipper Phone", "Ship. Ph.", "Shipper Phone", "Telephone number of of the seller party.");
			});
		}

		public void TestCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_SequenceNumberInfo, multipleResourceKey: null, "Sequence Number", "Seq No.", "Seq No.", "Bill sequence number.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_BillNumberInfo, multipleResourceKey: null, "Bill Number", "Bill No.", "Bill No.", "The Transport Document Number used to identify the relevant Consignment on the declaration.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RL_NKOriginInfo, multipleResourceKey: null, "Origin", "Origin", "Origin", "The UNLOCO of the port from which the Consignment first departs.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RL_NKFinalDestinationInfo, multipleResourceKey: null, "Final Destination", "Dest.", "Destination", "The UNLOCO of the port where the Consignment is intended to go to.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_GoodsDescriptionInfo, multipleResourceKey: null, "Goods Description (on Bill)", "Desc.", "Description", "Description of goods, as stipulated on the manifest.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_MarksAndNumbersInfo, multipleResourceKey: null, "Marks and Numbers (on Bill)", "Marks", "Marks & Nums.", "Free form description of the marks and numbers stipulated on the manifest.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RemarksInfo, multipleResourceKey: null, "Remarks", "Remarks", "Remarks", "Free form description of the remarks stipulated on the manifest.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_UCRNumberInfo, multipleResourceKey: null, "UCR Number", "UCR", "UCR No.", "Unique Consignment Reference (UCR) number assigned to the declaration.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.CustomsJobNumberInfo, multipleResourceKey: null, "Customs Job Number", "Job No.", "Cus. Job No.", "A system-generated number to uniquely identify a customs job in CW1.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ShipmentTypeInfo, multipleResourceKey: null, "Additional Declaration Type", "Add. Decl. Type", "Add. Decl. Type", "Code indicating the type of declaration. ");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ProcedureInfo, multipleResourceKey: null, "Additional Procedure(s)", "ACP", "Add. Proc.", "Code identifying any relevant additional procedure(s) associated with the bill.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_CargoStatusInfo, multipleResourceKey: null, "Cargo Status", "Cargo St.", "Cargo St.", "Code indicating the fulfillment status of the shipment.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_CarrierReferenceInfo, multipleResourceKey: null, "Carrier Reference", "Carr. Ref.", "Carrier Ref.", "Reference number assigned by the carrier to the means of transport on which the goods are directly loaded at the time of presentation at the customs office where the destination formalities are completed.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_PrepaidCollectInfo, multipleResourceKey: null, "Prepaid/Collect", "PPD/CLT", "PPD/CLT", "Code denoting the responsibility of the freight expenses associated with the Consignment.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.GoodsLocationDescriptionInfo, multipleResourceKey: null, "Location of Goods", "Location", "Location", "Location where the goods may be examined. The location must be precise enough to allow Customs to carry out the physical control of the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_OA_ContainerAgentInfo, multipleResourceKey: null, "Agent", "Agent", "Agent", "The party acting on behalf of the shipper or consignor.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_LocationInformationInfo, multipleResourceKey: null, "Location Information", "Loc. Info", "Location Info.", "Any additional location information.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ContainerModeInfo, multipleResourceKey: null, "Container Mode", "Cont. M.", "Container M.", "The Container Mode.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ContainerNumberInfo, multipleResourceKey: null, "Container", "Cont. No.", "Container No.", "The Container Number.");

				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ManifestQtyInfo, multipleResourceKey: null, "Quantity (on Bill)", "Qty.", "Quantity", "Number of pieces manifested.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_ManifestUQInfo, multipleResourceKey: null, "Quantity Unit", "UQ", "Qty. UQ", "Measurement unit of the number of pieces manifested.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_GrossWeightInfo, multipleResourceKey: null, "Gross Weight", "Wt.", "Weight", "Gross Weight of the Consignment.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_GrossWeightUQInfo, multipleResourceKey: null, "Weight Unit", "UQ", "Wt. UQ", "Measurement unit of the Gross Weight of the Consignment.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_VolumeInfo, multipleResourceKey: null, "Volume (on Bill)", "Vol.", "Volume", "Manifested volume.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_VolumeUQInfo, multipleResourceKey: null, "Volume Unit", "Vol. UQ", "Vol. UQ", "Measurement unit of the manifested volume.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_GoodsValueInfo, multipleResourceKey: null, "Goods Value", "Goods Val.", "Goods Val.", "Intrinsic value of the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RX_NKGoodsValueCurrencyInfo, multipleResourceKey: null, "Goods Value Currency", "Curr.", "Currency", "Currency code associated with the Goods Value.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_CustomsValueInfo, multipleResourceKey: null, "Customs Value", "Cus. Val.", "Cus. Val.", "Customs value of the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.ABL_RX_NKCustomsValueCurrencyInfo, multipleResourceKey: null, "Customs Value Currency", "Curr.", "Currency", "Currency code associated with the Customs Value.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.DiscountValueInfo, multipleResourceKey: null, "Discount Value", "Disc. Val.", "Disc. Val.", "Discount value of the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.DiscountValueCurrencyInfo, multipleResourceKey: null, "Discount Value Currency", "Curr.", "Currency", "Currency code associated with the Discount Value.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.OtherChargesValueInfo, multipleResourceKey: null, "Other Charges", "Oth. Ch.", "Oth. Ch.", "Value of any other charges associated with the goods.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.OtherChargesValueCurrencyInfo, multipleResourceKey: null, "Other Charges Currency", "Curr.", "Currency", "Currency code associated with the Other Charges.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.EntrySummaryReferenceNumberInfo, multipleResourceKey: null, "Stand Alone Declaration", "Stand Alone Decl.", "Stand Alone Decl.", "The Job Number of the Stand Alone Declaration linked to the H7 Bill.");
			});
		}

		public void TestRequiredCurrencyCode()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(Constants.CurrencyCodes.EuropeanUnion, bill.RequiredCurrencyCode);
		}

		public void TestSumOfGoodsValue()
		{
			CreateExchangeRate("AUD", 2m);
			CreateExchangeRate("CNY", 5m);

			var bill = GetNewBusinessObject() as AsycudaBill;

			CreatePackItem(bill, 10m, "AUD");
			CreatePackItem(bill, 20m, "EUR");
			CreatePackItem(bill, 30m, "CNY");

			AssertEquals(31m, bill.SumOfGoodsValue);
		}

		RefExchangeRate CreateExchangeRate(string currency, decimal sellRate)
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = currency;
			exchangeRate.RE_SellRate = sellRate;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			return exchangeRate;
		}

		AsycudaPackedItem CreatePackItem(AsycudaBill bill, decimal goodsValue, string currency)
		{
			var packItem = bill.PackedItems.AddNew();
			packItem.API_GoodsValue = goodsValue;
			packItem.API_RX_NKGoodsValueCurrency = currency;

			return packItem;
		}

		public void TestDefaultUQ()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(Weight.Kilograms, bill.ABL_GrossWeightUQ);
		}

		public void TestDefaultCurrency()
		{
			AssertDefaultCurrency(Constants.CountryCodes.UnitedStates, "USD");
			AssertDefaultCurrency(Constants.CountryCodes.Australia, "AUD");
		}

		void AssertDefaultCurrency(string country, string currency)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var bill = GetNewBusinessObject() as AsycudaBill;
				CombineAssertions(() =>
				{
					AssertEquals(currency, bill.ABL_RX_NKGoodsValueCurrency);
					AssertEquals(currency, bill.ABL_RX_NKTransportValueCurrency);
					AssertEquals(currency, bill.ABL_RX_NKInsuranceValueCurrency);
				});
			}
		}
	}
}
