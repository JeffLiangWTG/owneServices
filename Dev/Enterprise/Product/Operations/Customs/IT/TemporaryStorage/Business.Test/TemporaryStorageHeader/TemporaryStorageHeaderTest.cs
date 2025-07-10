using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using GlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageHeader))]
sealed class TemporaryStorageHeaderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
{
	public void TestClone()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.TransportType = "10";
		header.ArrivalTransportMeansCode = "TAWU4184089";
		header.PresentationCustomsOffice = "IT279100";

		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "C";
		goodsLocation.AdditionalIdentifier = "86053L";

		var holder = Factory.New<OrgHeader>();
		var address = goodsLocation.Address;
		address.IdentificationHolderPK = holder.PK;
		address.AuthorisationNumber = "14396Q";

		var clonedHeader = (TemporaryStorageHeader)header.Clone();

		AssertEquals("TransportType", "10", clonedHeader.TransportType);
		AssertEquals("Should not copy ArrivalTransportMeansCode", ZString.Empty, clonedHeader.ArrivalTransportMeansCode);
		AssertEquals("PresentationCustomsOffice", "IT279100", clonedHeader.PresentationCustomsOffice);
		AssertEquals("GoodsLocation.CGL_Qualifier", "Y", clonedHeader.GoodsLocation.CGL_Qualifier);
		AssertEquals("GoodsLocation.CGL_Type", "C", clonedHeader.GoodsLocation.CGL_Type);
		AssertEquals("GoodsLocation.AdditionalIdentifier", "86053L", clonedHeader.GoodsLocation.AdditionalIdentifier);
		AssertEquals("GoodsLocation.Address.IdentificationHolderPK", holder.PK, clonedHeader.GoodsLocation.Address.IdentificationHolderPK);
		AssertEquals("GoodsLocation.Address.AuthorisationNumber", "14396Q", clonedHeader.GoodsLocation.Address.AuthorisationNumber);
	}

	public void TestCusGoodsLocationProviderKey()
	{
		ICusGoodsLocationProvider header = Factory.New<TemporaryStorageHeader>();
		AssertEquals("ITPNTS", header.ProviderKey);
	}

	protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>);

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
	{
		return new TemporaryStorageHeaderLightValidationTester(bizObjToTest);
	}

	public void TestBills()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertType<EU.Business.CusTempStorage.TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(header.Bills);
	}

	public void TestGetBillType()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertEquals(typeof(TemporaryStorageBill), header.GetBillType());
	}

	public void TestCustomsStatusTransition_WhenAllNonMRNBillsDeleted()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		var billWithMRN = header.Bills[0];
		var item1 = billWithMRN.PackedItems.AddNew();
		item1.API_LineNo = 1;

		var billWithoutMRN1 = header.Bills.AddNew();
		var billWithoutMRN2 = header.Bills.AddNew();

		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(billWithMRN, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber.CE_EntryNum = "TestMRN123";
		header.CustomsStatus = PNTSCustomsStatusList.Codes.PartialActivated;

		Factory.Save();
		CombineAssertions(() =>
		{
			AssertEquals("[PRE-CONDITION] Initial bills count", 3, header.Bills.Count);
			AssertEquals("[PRE-CONDITION] CustomsStatus must be PartialActivated", PNTSCustomsStatusList.Codes.PartialActivated, header.CustomsStatus);
		});

		header.Bills.RemoveAndDelete(billWithoutMRN1);
		Factory.Save();
		CombineAssertions("At least one bill doesn't have MRN", () =>
		{
			AssertEquals("After deleting one non-MRN bill", 2, header.Bills.Count);
			AssertEquals("CustomsStatus must remain PartialActivated until all bills without MRN are deleted", PNTSCustomsStatusList.Codes.PartialActivated, header.CustomsStatus);
		});

		header.Bills.RemoveAndDelete(billWithoutMRN2);
		Factory.Save();
		CombineAssertions("When all bills have MRN", () =>
		{
			AssertEquals("After deleting all non-MRN bills", 1, header.Bills.Count);
			AssertEquals("CustomsStatus must transition to FullyActivated when no bills without MRN remain", PNTSCustomsStatusList.Codes.FullyActivated, header.CustomsStatus);
		});
	}

	public void TestAMA_AgentType_Default()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertEquals(ZString.Empty, header.AMA_AgentType);
	}

	public void TestAMA_AgentType_Caption()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var resStringData = header.AMA_AgentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
		AssertEquals("Caption", "Representative Qualification", resStringData.Caption);
		AssertEquals("MediumCaption", "Representative Qualification", resStringData.MediumCaption);
		AssertEquals("ShortCaption", "Repres. Qual.", resStringData.ShortCaption);
	}

	public void TestReadOnlyPropertiesWhenCustomsStatusAMG()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions("When Customs status != AMG", () =>
		{
			AssertEquals("Customs Office should not be read-only", false, header.AMA_CustomsOfficeInfo.ReadOnly);
			AssertEquals("Transport Mode should not be read-only", false, header.AMA_TransportModeInfo.ReadOnly);
			AssertEquals("Arrival Transport Means Code should not be read-only", false, header.ArrivalTransportMeansCodeInfo.ReadOnly);
			AssertEquals("Transport Type should not be read-only", false, header.TransportTypeInfo.ReadOnly);
		});

		header.CustomsStatus = "AMG";
		CombineAssertions("When Customs status = AMG", () =>
		{
			AssertEquals("Customs Office should be read-only", true, header.AMA_CustomsOfficeInfo.ReadOnly);
			AssertEquals("Transport Mode should be read-only", true, header.AMA_TransportModeInfo.ReadOnly);
			AssertEquals("Arrival Transport Means Code should be read-only", true, header.ArrivalTransportMeansCodeInfo.ReadOnly);
			AssertEquals("Transport Type should be read-only", true, header.TransportTypeInfo.ReadOnly);
		});
	}

	sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
	{
		public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			return base.ShouldTestProperty(info) && info.Name != ManifestBase.AutoAsycudaBill.Schema.ABL_RL_NKPortOfDischarge;
		}
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		var result = base.GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues();
		result.Add(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_OA_Declarant);
		return result;
	}

	public void TestAuthorizationPropertiesChangeWithGoodsLocationType()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		AssertEquals("AuthorizationType should be empty as AGC_Code is empty", ZString.Empty, header.AuthorizationType);

		header.GoodsLocation.CGL_Type = "B";

		AssertEquals("AuthorizationType should remain empty, when Goods location CGL_Type is B", ZString.Empty, header.AuthorizationType);

		header.AuthorizationType = "TST";
		header.AuthorizationOwner = orgHeader.PK;
		header.AuthorizationNumber = "123";

		header.GoodsLocation.CGL_Type = "C";

		CombineAssertions("When GoodsLocation CGL_type is changed", () =>
		{
			AssertEquals("AuthorizationType should remain filled", "TST", header.AuthorizationType);
			AssertEquals("AuthorizationOwner should remain filled", orgHeader.PK, header.AuthorizationOwner);
			AssertEquals("AuthorizationNumber should remain filled", "123", header.AuthorizationNumber);
		});
	}

	public void TestLookupsType()
	{
		AssertType<TemporaryStorageHeaderLookups>(Factory.New<TemporaryStorageHeader>().Lookups);
	}

	public void TestValidationType()
	{
		AssertType<TemporaryStorageHeaderValidation>(Factory.New<TemporaryStorageHeader>().Validation);
	}

	public void TestAMA_CustomsProfile_Caption()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var customsProfileStringdata = header.AMA_CustomsProfileInfo.GetAttribute<ResourceStringDataAttribute>();
		AssertEquals("Caption", "Account", customsProfileStringdata.Caption);
	}

	public void TestAMA_CustomsProfile_MaxLength()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertEquals("Max length", 20, header.AMA_CustomsProfileInfo.MaxLength);
	}

	public void TestAMA_Calc_HasHouseConsignment()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertEquals(true, header.AMA_Calc_HasHouseConsignment);
	}

	public void TestDefaultCustomsProfile()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "10", "AA", "11-001");
		currentCompany.Factory.Save();

		CombineAssertions(() =>
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("When company has one account, declarant and representative are empty, default to company node", "10", header.AMA_CustomsProfile);

			ClearCustomsProfilesLookupsCache(Factory, currentCompany.PK);
			AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "40", "BB", "44-001");
			currentCompany.Factory.Save();

			header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("When company has more than one account", "", header.AMA_CustomsProfile);

			var declarantAddressBB = Factory.NewWithValidTestData<OrgAddress>();
			declarantAddressBB.Header.OH_Code = "BB";

			header.AMA_OA_Declarant = declarantAddressBB.PK;
			AssertEquals("When Declarant match only one company account, representative is empty, default to declarant node", "40", header.AMA_CustomsProfile);

			var representativeAddressAA = Factory.NewWithValidTestData<OrgAddress>();
			representativeAddressAA.Header.OH_Code = "AA";

			header.AMA_OA_Declarant = ZGuid.Empty;
			header.AMA_OA_Representative = representativeAddressAA.PK;
			AssertEquals("When Representative match only one company account, declarant is empty, default to representative node", "10", header.AMA_CustomsProfile);
		});
	}

	public void TestTransportTypeCaption()
	{
		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(storageHeader.TransportTypeInfo);
		AssertEquals("Caption", "Transport Type", resourceStringData.Caption);
	}

	public void TestArrivalTransportMeansType()
	{
		CombineAssertions(() =>
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<ArrivalTransportMeans>("ArrivalTransportMeans Type when creating", header.ArrivalTransportMeans);
			Factory.Save();

			var headerOnSeparateFactory = new BusinessObjectFactory().Load<TemporaryStorageHeader>(header.PK);
			AssertType<ArrivalTransportMeans>("ArrivalTransportMeans Type when loading", headerOnSeparateFactory.ArrivalTransportMeans);
		});
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var supportingInfoTypes = header.GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("SupportingInfoTypes Count", 1, supportingInfoTypes.Count);
			AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestTypeOfPreviousDocuments()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(header.PreviousDocuments);
	}

	public void TestGetDocumentSupporterOverride()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var wrappers = ((IDocumentSupportable)header).DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EuPnts, null);
		AssertEquals("Wrappers count", 1, wrappers.Length);
		AssertEquals("Wrapper type", "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeaderWrapper", wrappers[0].GetType().FullName);
	}

	public void TestICustomsLinkedObjectAdapterProviderMembers()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var customsLinkedObjectAdapterProvider = header as IT.Business.ICustomsLinkedObjectAdapterProvider;

		AssertNotNull("TemporaryStorageHeader must be implement ICustomsLinkedObjectAdapterProvider", customsLinkedObjectAdapterProvider);
		CombineAssertions("Assert ICustomsLinkedObjectAdapterProviderMembers members", () =>
		{
			AssertExceptionThrown<NotImplementedException>("GetSadCustomsLinkedObjectAdapter", () => customsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter());
			AssertExceptionThrown<NotImplementedException>("GetNewSingleWindowCustomsLinkedObjectAdapter", () => customsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter());
			AssertType<TemporaryStorageCustomsLinkedObjectAdapter>("GetNewXmlCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter());
		});
	}

	void ClearCustomsProfilesLookupsCache(BusinessObjectFactory factory, ZGuid effectiveCompanyPK)
	{
		var key = $"IT.AccountListLookups|AccountsForCompany_{effectiveCompanyPK}";
		factory.ClearCachedValue<CodeDescriptionPairList>(key);
	}

	public void TestTemporaryStorageContainerType()
	{
		CombineAssertions(() =>
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.Containers.AddNew();
			AssertType<TemporaryStorageContainer>("Type of TemporaryStorageContainer when creating", header.Containers[0]);
			Factory.Save();

			var headerOnSeparateFactory = new BusinessObjectFactory().Load<TemporaryStorageHeader>(header.PK);
			headerOnSeparateFactory.Containers.AddNew();
			AssertType<TemporaryStorageContainer>("Type of TemporaryStorageContainer when loading", headerOnSeparateFactory.Containers[0]);
		});
	}

	public void TestTemporaryStorageContainerCollectionType()
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		AssertType<AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>>(temporaryStorageHeader.Containers);
	}

	public void TestTemporaryStoragePackedItems()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();

		var firstBillItem1 = bill1.PackedItems.AddNew();
		var secondBillItem = bill2.PackedItems.AddNew();
		var firstBillItem2 = bill1.PackedItems.AddNew();

		var allPackedItems = header.TemporaryStoragePackedItems;

		AssertEquals("Total number of packed items", 3, allPackedItems.Count);
		AssertContainsExactElementsInExactOrder("Packed Items list:", [firstBillItem1, firstBillItem2, secondBillItem], allPackedItems);
	}

	public void TestDeclarantDefaultAddress()
	{
		var org = CreateOrgWithAddresses();
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_OA_Declarant_ZAddress.OrgPK = org.PK;
		AssertEquals("Default should be Office address", org.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK, header.AMA_OA_Declarant);
	}

	public void TestRepresentativeDefaultAddress()
	{
		var org = CreateOrgWithAddresses();
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_OA_Representative_ZAddress.OrgPK = org.PK;
		AssertEquals("Default should be Office address", org.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK, header.AMA_OA_Representative);
	}

	public void TestSetCustomsStatusAsRegistered()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;

		var bill1 = header.Bills[0];
		var item1 = bill1.PackedItems.AddNew();
		item1.API_LineNo = 1;
		bill1.ABL_AMA = header.PK;
		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill1, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber.CE_EntryNum = "TestMRN123A";

		var bill2 = header.Bills.AddNew();

		header.SetCustomsStatusAsRegistered(ZDateTime.Today);
		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus should be PartialActivated", PNTSCustomsStatusList.Codes.PartialActivated, header.CustomsStatus);
			AssertEquals("AMA_MessageStatus should be empty", ZString.Empty, header.AMA_MessageStatus);
			AssertEquals("CustomsStatusDate should be today", ZDateTime.Today, header.CustomsStatusDate);
		});

		var mrnEntryNumber2 = TemporaryStorageTestHelper.CreateCusEntryNumber(bill2, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber2.CE_EntryNum = "TestMRN123B";

		header.SetCustomsStatusAsRegistered(ZDateTime.Today.AddDays(1));
		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus should be updated to FullyActivated", PNTSCustomsStatusList.Codes.FullyActivated, header.CustomsStatus);
			AssertEquals("AMA_MessageStatus should remain unchanged", ZString.Empty, header.AMA_MessageStatus);
			AssertEquals("CustomsStatusDate should be updated to today + 1", ZDateTime.Today.AddDays(1), header.CustomsStatusDate);
		});

		header.SetCustomsStatusAsRegistered(ZDateTime.Today);
		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus should remain FullyActivated", PNTSCustomsStatusList.Codes.FullyActivated, header.CustomsStatus);
			AssertEquals("AMA_MessageStatus should remain unchanged", ZString.Empty, header.AMA_MessageStatus);
			AssertEquals("CustomsStatusDate should remain unchanged", ZDateTime.Today.AddDays(1), header.CustomsStatusDate);
		});
	}

	OrgHeader CreateOrgWithAddresses()
	{
		var org = Factory.New<OrgHeader>();
		org.OH_FullName = "FULLNAME 1";

		org.MainAddress.OA_Address1 = "MAIN ADDRESS";
		org.MainAddress.OA_Address2 = "ADDRESS 1";
		org.MainAddress.OA_City = "CITY 1";
		org.MainAddress.OA_State = "STATE 1";
		org.MainAddress.OA_PostCode = "102034";

		var officeAddress = org.Addresses.AddNew(OrgAddressType.Office, isDefault: true);
		officeAddress.OA_Address1 = "OFFICE ADDRESS";
		officeAddress.OA_Address2 = "ADDRESS 2";
		officeAddress.OA_City = "CITY 2";
		officeAddress.OA_State = "STATE 2";
		officeAddress.OA_PostCode = "102035";

		var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, isDefault: true);
		deliveryAddress.OA_Address1 = "DELIVERY ADDRESS";
		deliveryAddress.OA_Address2 = "ADDRESS 3";
		deliveryAddress.OA_City = "CITY 3";
		deliveryAddress.OA_State = "STATE 3";
		deliveryAddress.OA_PostCode = "102036";

		return org;
	}
}
