using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageBillAbstractTest<TBill, THeader> : AsycudaBillTest
		where TBill : TemporaryStorageBill
		where THeader : TemporaryStorageHeader
	{
		protected TBill GetNewBill() => (TBill)GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tempHeader = factory.New<THeader>();
			tempHeader.AMA_Calc_HasHouseConsignment = true;
			return tempHeader.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TemporaryStorageBillLightValidationTester(bizObjToTest);
		}

		sealed class TemporaryStorageBillLightValidationTester : LightValidationTester
		{
			public TemporaryStorageBillLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return base.ShouldTestProperty(info)
						&& propertyName != TemporaryStorageBill.Schema.ABL_BolType
						&& propertyName != TemporaryStorageHeader.Schema.AMA_RN_NKCountry;
			}
		}
	}

	[TestedType(typeof(TemporaryStorageBill))]
	sealed class TemporaryStorageBillTest : TemporaryStorageBillAbstractTest<TemporaryStorageBill, TemporaryStorageHeader>
	{
		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = bill.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}

			foreach (var status in TemporaryStorageHeaderTest.GetAmendableFieldsEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;

				Assert(bill.ABL_BillNumberInfo.ReadOnly);
				Assert(bill.TypeOfBillDocumentInfo.ReadOnly);
			}
		}

		public void TestReadOnlyPropertiesForDeconsolidation_MasterBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			var bill = header.MasterBill;

			CombineAssertions("Properties should be ReadOnly when AMA_MessageType = 'DC' and type of bill is Master", () =>
			{
				AssertPropertyInfo(bill.ABL_UCRNumberInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_GrossWeightInfo, true, ZDecimal.Zero);
				AssertPropertyInfo(bill.ABL_GrossWeightUQInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ConsignorOrgPKInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_OA_ShipperInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_ShipperNameInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperStreet1Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperStreet2Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperPostcodeInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperCityInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperStateInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_RN_NKShipperCountryInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperPhoneInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperRegNoInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ShipperRegNoTypeInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ConsigneeOrgPKInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_OA_ConsigneeInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeNameInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeStreet1Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeStreet2Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneePostcodeInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeCityInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeStateInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_RN_NKConsigneeCountryInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneePhoneInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeRegNoInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_ConsigneeRegNoTypeInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.NotifyPartyOrgPKInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_OA_NotifyPartyInfo, true, ZGuid.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyNameInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyStreet1Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyStreet2Info, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyPostcodeInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyCityInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyStateInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_RN_NKNotifyPartyCountryInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyPhoneInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyRegNoInfo, true, ZString.Empty);
				AssertPropertyInfo(bill.ABL_NotifyPartyRegNoTypeInfo, true, ZString.Empty);
				AssertEquals("Packs.ReadOnly", true, bill.Packs.ReadOnly);
				AssertEquals("PackedItems.ReadOnly", true, bill.PackedItems.ReadOnly);
				AssertEquals("SupportingDocuments.ReadOnly", true, bill.SupportingDocuments.ReadOnly);
				AssertEquals("PreviousDocuments.ReadOnly", true, bill.PreviousDocuments.ReadOnly);
				AssertEquals("AdditionalInfos.ReadOnly", true, bill.AdditionalInfos.ReadOnly);
				AssertEquals("SupplyChainActors.ReadOnly", true, bill.SupplyChainActors.ReadOnly);
			});
		}

		public void TestAdditionalInfo()
		{
			var bill = GetNewBill();
			var additionalInfos = bill.AdditionalInfos;
			AssertType<TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>>("AdditionalInfos", additionalInfos);
			AssertEquals("Empty additionalInfo collection", 0, additionalInfos.Count);

			var addInfo1 = additionalInfos.AddNew();
			AssertEquals("CSI_Type", "OTH", addInfo1.CSI_Type);
			addInfo1.CSI_ReferenceNumber = "Reference1";

			var addInfo2 = additionalInfos.AddNew();
			addInfo2.CSI_ReferenceNumber = "Reference2";
			Factory.Save();

			var reloadedBill = new BusinessObjectFactory().Load<TemporaryStorageBill>(bill.PK);
			var reloadedAdditionalInfos = reloadedBill.AdditionalInfos;
			CombineAssertions("Reloaded additionalInfo collection", () =>
			{
				AssertEquals("Count", 2, reloadedAdditionalInfos.Count);
				AssertEquals("Element", "Reference1,Reference2", string.Join(",", reloadedAdditionalInfos.Select(x => x.CSI_ReferenceNumber)));
			});
		}

		public void TestRegNoMaxLength()
		{
			var bill = GetNewBill();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestOrg";

			var addressWithCusCode = orgHeader.Addresses.AddNew();
			addressWithCusCode.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			addressWithCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789123456789123456", GlbCompany.CurrentCompany.Country.Code);
			CombineAssertions(() =>
			{
				AssertEquals("pre-condition", ZString.Empty, bill.ABL_ConsigneeRegNo);
				AssertEquals("pre-condition", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals("pre-condition", ZString.Empty, bill.ABL_NotifyPartyRegNo);
			});

			bill.ABL_OA_Consignee = addressWithCusCode.PK;
			bill.ABL_OA_Shipper = addressWithCusCode.PK;
			bill.ABL_OA_NotifyParty = addressWithCusCode.PK;
			CombineAssertions(() =>
			{
				AssertEquals("The maximum truncation length of RegNo is 17", "12345678912345678", bill.ABL_ConsigneeRegNo.ToString());
				AssertEquals("The maximum truncation length of RegNo is 17", "12345678912345678", bill.ABL_ShipperRegNo.ToString());
				AssertEquals("The maximum truncation length of RegNo is 17", "12345678912345678", bill.ABL_NotifyPartyRegNo.ToString());

				AssertEquals(17, bill.ABL_ShipperRegNoInfo.MaxLength);
				AssertEquals(17, bill.ABL_ConsigneeRegNoInfo.MaxLength);
				AssertEquals(17, bill.ABL_NotifyPartyRegNoInfo.MaxLength);
			});
		}

		public void TestConsignorOrgPK()
		{
			var (header1, header2) = SetUpOrganizationData(OrganisationTypes.Consignor);
			var bill = GetNewBill();
			CombineAssertions(() =>
			{
				bill.ConsignorOrgPK = header1.PK;
				AssertContainsExactElementsInAnyOrder("Consignor: Consignor Addresses list should be updated by current consignor header which is header1.", header1.Addresses.Select(a => a.OA_Address1), bill.ABL_OA_Shipper_ZAddress.OrgAddress_List.List.GetAllCodesZString());

				bill.ConsignorOrgPK = header2.PK;
				AssertContainsExactElementsInAnyOrder("Consignor: Consignor Addresses list should be updated by current consignor header which is header2.", header2.Addresses.Select(a => a.OA_Address1), bill.ABL_OA_Shipper_ZAddress.OrgAddress_List.List.GetAllCodesZString());

				bill.ConsignorOrgPK = ZGuid.Empty;
				AssertEquals("Consignor: Consignor Addresses list should be empty if current consignor header is empty.", 0, bill.ABL_OA_Shipper_ZAddress.OrgAddress_List.List.GetAllCodesZString().Length);

				bill.ABL_OA_Shipper = header1.MainAddress.PK;
				AssertEquals("Consignor: get orgPK from consignor address", header1.PK, bill.ConsignorOrgPK);
			});
		}

		public void TestConsigneeOrgPK()
		{
			var (header1, header2) = SetUpOrganizationData(OrganisationTypes.Consignee);
			var bill = GetNewBill();
			CombineAssertions(() =>
			{
				bill.ConsigneeOrgPK = header1.PK;
				AssertContainsExactElementsInAnyOrder("Consignee: Consignee Addresses list should be updated by current consignee header which is header1.", header1.Addresses.Select(a => a.OA_Address1), bill.ABL_OA_Consignee_ZAddress.OrgAddress_List.List.GetAllCodesZString());

				bill.ConsigneeOrgPK = header2.PK;
				AssertContainsExactElementsInAnyOrder("Consignee: Consignee Addresses list should be updated by current consignee header which is header2.", header2.Addresses.Select(a => a.OA_Address1), bill.ABL_OA_Consignee_ZAddress.OrgAddress_List.List.GetAllCodesZString());

				bill.ConsigneeOrgPK = ZGuid.Empty;
				AssertEquals("Consignee: Consignee Addresses list should be empty if current consignee header is empty.", 0, bill.ABL_OA_Consignee_ZAddress.OrgAddress_List.List.GetAllCodesZString().Length);

				bill.ABL_OA_Consignee = header1.MainAddress.PK;
				AssertEquals("Consignee: get orgPK from consignee address", header1.PK, bill.ConsigneeOrgPK);
			});
		}

		(OrgHeader, OrgHeader) SetUpOrganizationData(OrganisationTypes orgType)
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "H1";
			header1.OrganisationTypes = orgType;
			var address1 = header1.Addresses.AddNew();
			address1.OA_Address1 = "ADD1";
			address1.AddAddressType(OrgAddressType.Office);

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "H2";
			header2.OrganisationTypes = orgType;
			var address2 = header2.Addresses.AddNew();
			address2.OA_Address1 = "ADD2";
			address2.AddAddressType(OrgAddressType.Office);

			Factory.Save();
			return (header1, header2);
		}

		public void TestDefaultValues()
		{
			var bill = GetNewBill();
			AssertEquals("Default Gross Unit should be KG", "KG", bill.ABL_GrossWeightUQ);
		}

		public void TestLookupsType()
		{
			var bill = GetNewBill();
			AssertType<TemporaryStorageBillLookups>("Lookups for TemporaryStorageBill should be of type TemporaryStorageBillLookups", bill.Lookups);
		}

		public void TestValidationType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<TemporaryStorageBillValidation>(header.MasterBill.Validation);

			var bill = header.Bills.AddNew();
			AssertType<TemporaryStorageBillValidation>(bill.Validation);
		}

		public void TestParentType()
		{
			var bill = GetNewBill();
			AssertType<TemporaryStorageHeader>(bill.Header);
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertCapions(nameof(TemporaryStorageBill.ABL_Calc_IsMaster), "Is Master?", "Is Master?", "Is Master?");
				AssertCapions(nameof(TemporaryStorageBill.ABL_BolType), "Kind of Bill", "Kind", "Kind");
				AssertCapions(nameof(TemporaryStorageBill.TypeOfBillDocument), "Type of Bill Document", "Type", "Type");
				AssertCapions(nameof(TemporaryStorageBill.ABL_BillNumber), "Bill Number", "Bill #", "Bill");
				AssertCapions(nameof(TemporaryStorageBill.ABL_UCRNumber), "UCR Number", "UCR #", "UCR");
				AssertCapions(nameof(TemporaryStorageBill.ABL_GrossWeight), "Gross Mass", "Gross Mass", "Gross Mass");
				AssertCapions(nameof(TemporaryStorageBill.ABL_GrossWeightUQ), "Gross Mass Unit", "Unit", "Unit");
				AssertCapions(nameof(TemporaryStorageBill.ABL_OA_Shipper), "Consignor Address", string.Empty, string.Empty);
				AssertCapions(nameof(TemporaryStorageBill.ABL_OA_Consignee), "Consignee Address", string.Empty, string.Empty);
				AssertCapions(nameof(TemporaryStorageBill.ABL_OA_NotifyParty), "Notify Party", "Notify", string.Empty);
				AssertCapions(nameof(TemporaryStorageBill.ConsignorOrgPK), "Consignor Code", string.Empty, string.Empty);
				AssertCapions(nameof(TemporaryStorageBill.ConsigneeOrgPK), "Consignee Code", string.Empty, string.Empty);
				AssertCapions(nameof(TemporaryStorageBill.ABL_ShipperRegNoType), "Type of Person", "Type of Person", "Type");
				AssertCapions(nameof(TemporaryStorageBill.ABL_ConsigneeRegNoType), "Type of Person", "Type of Person", "Type");
				AssertCapions(nameof(TemporaryStorageBill.ABL_NotifyPartyRegNoType), "Type of Person", "Type of Person", "Type");
			});

			void AssertCapions(string propertyName, string caption, string mediumCaption, string shortCaption)
			{
				AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageBill), propertyName).Caption);
				AssertEquals($"{propertyName} Medium Caption", mediumCaption, DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageBill), propertyName).MediumCaption);
				AssertEquals($"{propertyName} Short Caption", shortCaption, DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageBill), propertyName).ShortCaption);
			}
		}

		public void TestTypeOfBillDocuemnt()
		{
			var bill = GetNewBill();
			AssertEquals("TypeOfBillDocument MaxLength", 4, bill.TypeOfBillDocumentInfo.MaxLength);
		}

		public void TestSupplyChainActors()
		{
			var bill = GetNewBill();
			var supplyChainActors = bill.SupplyChainActors;
			AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(supplyChainActors);
			AssertEquals("Empty additionalInfo collection", 0, supplyChainActors.Count);
		}

		public void TestSupportingDocument()
		{
			var bill = GetNewBill();
			var supportingDocuments = bill.SupportingDocuments;
			AssertType<TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>>(supportingDocuments);
			AssertEquals("Empty supportingDocument collection", 0, supportingDocuments.Count);

			var supportingDocument1 = supportingDocuments.AddNew();
			supportingDocument1.CSI_ReferenceNumber = "Reference1";
			CombineAssertions("Default value for new supportingDocument", () =>
			{
				AssertEquals("CSI_ParentTableCode", "ABL", supportingDocument1.CSI_ParentTableCode);
				AssertEquals("CSI_ParentID", bill.PK, supportingDocument1.CSI_ParentID);
				AssertEquals("CSI_Type", "SUP", supportingDocument1.CSI_Type);
			});

			var supportingDocument2 = supportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber = "Reference2";
			Factory.Save();

			var reloadedBill = Factory.Load<TemporaryStorageBill>(bill.PK);
			var reloadedSupportingDocuments = reloadedBill.SupportingDocuments;
			CombineAssertions("Reloaded supportingDocument collection", () =>
			{
				AssertEquals("Count", 2, reloadedSupportingDocuments.Count);
				AssertEquals("Element", "Reference1,Reference2", string.Join(",", reloadedSupportingDocuments.Select(x => x.CSI_ReferenceNumber)));
			});
		}

		public void TestDefaultPartyOrgAddressDetails()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone = "+00123456888";
			org.OA_RN_NKCountryCode = "SG";

			bill.ABL_OA_Shipper = org.PK;
			AssertEquals("Shipper", "FullName", bill.ABL_ShipperName);
			AssertEquals("Shipper", "Address1", bill.ABL_ShipperStreet1);
			AssertEquals("Shipper", "Address2", bill.ABL_ShipperStreet2);
			AssertEquals("Shipper", "SIN", bill.ABL_ShipperCity);
			AssertEquals("Shipper", "STATE", bill.ABL_ShipperState);
			AssertEquals("Shipper", "0001", bill.ABL_ShipperPostcode);
			AssertEquals("Shipper", "+00123456888", bill.ABL_ShipperPhone);
			AssertEquals("Shipper", "SG", bill.ABL_RN_NKShipperCountry);

			bill.ABL_OA_Consignee = org.PK;
			AssertEquals("Consignee", "FullName", bill.ABL_ConsigneeName);
			AssertEquals("Consignee", "Address1", bill.ABL_ConsigneeStreet1);
			AssertEquals("Consignee", "Address2", bill.ABL_ConsigneeStreet2);
			AssertEquals("Consignee", "SIN", bill.ABL_ConsigneeCity);
			AssertEquals("Consignee", "STATE", bill.ABL_ConsigneeState);
			AssertEquals("Consignee", "0001", bill.ABL_ConsigneePostcode);
			AssertEquals("Consignee", "+00123456888", bill.ABL_ConsigneePhone);
			AssertEquals("Consignee", "SG", bill.ABL_RN_NKConsigneeCountry);

			bill.ABL_OA_NotifyParty = org.PK;
			AssertEquals("NotifyParty", "FullName", bill.ABL_NotifyPartyName);
			AssertEquals("NotifyParty", "Address1", bill.ABL_NotifyPartyStreet1);
			AssertEquals("NotifyParty", "Address2", bill.ABL_NotifyPartyStreet2);
			AssertEquals("NotifyParty", "SIN", bill.ABL_NotifyPartyCity);
			AssertEquals("NotifyParty", "STATE", bill.ABL_NotifyPartyState);
			AssertEquals("NotifyParty", "0001", bill.ABL_NotifyPartyPostcode);
			AssertEquals("NotifyParty", "+00123456888", bill.ABL_NotifyPartyPhone);
			AssertEquals("NotifyParty", "SG", bill.ABL_RN_NKNotifyPartyCountry);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			org.OA_CompanyNameOverride = "Override Company Name";
			bill.ABL_OA_Shipper = org.PK;
			bill.ABL_OA_Consignee = org.PK;
			bill.ABL_OA_NotifyParty = org.PK;

			AssertEquals("Shipper", "Override Company Name", bill.ABL_ShipperName);
			AssertEquals("Consignee", "Override Company Name", bill.ABL_ConsigneeName);
			AssertEquals("NotifyParty", "Override Company Name", bill.ABL_NotifyPartyName);
		}

		public void TestShipperDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ConsignorOrgPK = org.PK;
			AssertEquals("Should be the pickup", pickupAddress.PK, bill.ABL_OA_Shipper);
		}

		public void TestShipper()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertEquals("bill.ABL_ShipperNameInfo.ReadOnly", false, bill.ABL_ShipperNameInfo.ReadOnly);
			bill.ABL_ShipperName = "BOB";
			AssertEquals("bill.ABL_ShipperStreet1Info.ReadOnly", false, bill.ABL_ShipperStreet1Info.ReadOnly);
			bill.ABL_ShipperStreet1 = "STREET 1";
			AssertEquals("bill.ABL_ShipperStreet2Info.ReadOnly", false, bill.ABL_ShipperStreet2Info.ReadOnly);
			bill.ABL_ShipperStreet2 = "STREET 2";
			AssertEquals("bill.ABL_ShipperCityInfo.ReadOnly", false, bill.ABL_ShipperCityInfo.ReadOnly);
			bill.ABL_ShipperCity = "CT";
			AssertEquals("bill.ABL_ShipperStateInfo.ReadOnly", false, bill.ABL_ShipperStateInfo.ReadOnly);
			bill.ABL_ShipperState = "ST";
			AssertEquals("bill.ABL_ShipperPostcodeInfo.ReadOnly", false, bill.ABL_ShipperPostcodeInfo.ReadOnly);
			bill.ABL_ShipperPostcode = "4343";
			AssertEquals("bill.ABL_RN_NKShipperCountryInfo.ReadOnly", false, bill.ABL_RN_NKShipperCountryInfo.ReadOnly);
			bill.ABL_RN_NKShipperCountry = "NZ";
			AssertEquals("bill.ABL_ShipperRegNoInfo.ReadOnly", false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_ShipperRegNo = "123456";
			AssertEquals("bill.ABL_ShipperPhoneInfo.ReadOnly", false, bill.ABL_ShipperPhoneInfo.ReadOnly);
			bill.ABL_ConsigneePhone = "+4234232";

			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_ShipperNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_ShipperStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_ShipperStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_ShipperCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_ShipperStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_ShipperPostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKShipperCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_ShipperRegNoInfo, true, ZString.Empty);
			AssertPropertyInfo(bill.ABL_ShipperPhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertEquals(orgAddress.PK, bill.ABL_OA_Shipper);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.Shipper;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.OA_RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.OA_Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<TemporaryStorageBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_Shipper);
		}

		public void TestConsigneeDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ConsigneeOrgPK = org.PK;
			AssertEquals("Should be the delivery", deliveryAddress.PK, bill.ABL_OA_Consignee);
		}

		public void TestConsignee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = ZGuid.Empty;
			AssertEquals("bill.ABL_ConsigneeNameInfo.ReadOnly", false, bill.ABL_ConsigneeNameInfo.ReadOnly);
			bill.ABL_ConsigneeName = "BOB";
			AssertEquals("bill.ABL_ConsigneeStreet1Info.ReadOnly", false, bill.ABL_ConsigneeStreet1Info.ReadOnly);
			bill.ABL_ConsigneeStreet1 = "STREET 1";
			AssertEquals("bill.ABL_ConsigneeStreet2Info.ReadOnly", false, bill.ABL_ConsigneeStreet2Info.ReadOnly);
			bill.ABL_ConsigneeStreet2 = "STREET 2";
			AssertEquals("bill.ABL_ConsigneeCityInfo.ReadOnly", false, bill.ABL_ConsigneeCityInfo.ReadOnly);
			bill.ABL_ConsigneeCity = "CT";
			AssertEquals("bill.ABL_ConsigneeStateInfo.ReadOnly", false, bill.ABL_ConsigneeStateInfo.ReadOnly);
			bill.ABL_ConsigneeState = "ST";
			AssertEquals("bill.ABL_ConsigneePostcodeInfo.ReadOnly", false, bill.ABL_ConsigneePostcodeInfo.ReadOnly);
			bill.ABL_ConsigneePostcode = "4343";
			AssertEquals("bill.ABL_RN_NKConsigneeCountryInfo.ReadOnly", false, bill.ABL_RN_NKConsigneeCountryInfo.ReadOnly);
			bill.ABL_RN_NKConsigneeCountry = "NZ";
			AssertEquals("bill.ABL_ConsigneePhoneInfo.ReadOnly", false, bill.ABL_ConsigneePhoneInfo.ReadOnly);
			bill.ABL_ConsigneePhone = "+7554554";
			AssertEquals("bill.ABL_ConsigneeRegoNoInfo.ReadOnly", false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_ConsigneeRegNo = "123456";

			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_ConsigneeNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_ConsigneeStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_ConsigneeStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_ConsigneeCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_ConsigneeStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_ConsigneePostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKConsigneeCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_ConsigneePhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertPropertyInfo(bill.ABL_ConsigneeRegNoInfo, true, ZString.Empty);
			AssertEquals(orgAddress.PK, bill.ABL_OA_Consignee);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.Consignee;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.OA_RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.OA_Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<TemporaryStorageBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_Consignee);
		}

		public void TestNotifyPartyDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.NotifyPartyOrgPK = org.PK;
			AssertEquals("Should be the main office", mainAddress.PK, bill.ABL_OA_NotifyParty);
		}

		public void TestNotifyParty()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			AssertEquals("bill.ABL_NotifyPartyNameInfo.ReadOnly", false, bill.ABL_NotifyPartyNameInfo.ReadOnly);
			bill.ABL_NotifyPartyName = "BOB";
			AssertEquals("bill.ABL_NotifyPartyStreet1Info.ReadOnly", false, bill.ABL_NotifyPartyStreet1Info.ReadOnly);
			bill.ABL_NotifyPartyStreet1 = "STREET 1";
			AssertEquals("bill.ABL_NotifyPartyStreet2Info.ReadOnly", false, bill.ABL_NotifyPartyStreet2Info.ReadOnly);
			bill.ABL_NotifyPartyStreet2 = "STREET 2";
			AssertEquals("bill.ABL_NotifyPartyCityInfo.ReadOnly", false, bill.ABL_NotifyPartyCityInfo.ReadOnly);
			bill.ABL_NotifyPartyCity = "CT";
			AssertEquals("bill.ABL_NotifyPartyStateInfo.ReadOnly", false, bill.ABL_NotifyPartyStateInfo.ReadOnly);
			bill.ABL_NotifyPartyState = "ST";
			AssertEquals("bill.ABL_NotifyPartyPostcodeInfo.ReadOnly", false, bill.ABL_NotifyPartyPostcodeInfo.ReadOnly);
			bill.ABL_NotifyPartyPostcode = "4343";
			AssertEquals("bill.ABL_RN_NKNotifyPartyCountryInfo.ReadOnly", false, bill.ABL_RN_NKNotifyPartyCountryInfo.ReadOnly);
			bill.ABL_RN_NKNotifyPartyCountry = "NZ";
			AssertEquals("bill.ABL_NotifyPartyPhoneInfo.ReadOnly", false, bill.ABL_NotifyPartyPhoneInfo.ReadOnly);
			bill.ABL_NotifyPartyPhone = "+7554554";
			AssertEquals("bill.ABL_NotifyPartyRegNoInfo.ReadOnly", false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.ABL_NotifyPartyRegNo = "123456";

			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_NotifyPartyNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_NotifyPartyStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_NotifyPartyStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_NotifyPartyCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_NotifyPartyStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_NotifyPartyPostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKNotifyPartyCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_NotifyPartyPhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertPropertyInfo(bill.ABL_NotifyPartyRegNoInfo, true, ZString.Empty);
			AssertEquals(orgAddress.PK, bill.ABL_OA_NotifyParty);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.NotifyParty;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.OA_RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.OA_Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<TemporaryStorageBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_NotifyParty);
		}

		void AssertPropertyInfo(ZPropertyInfo info, bool isReadOnly, IZType value)
		{
			var name = info.Name;
			AssertEquals(name + ".ReadOnly", isReadOnly, info.ReadOnly);
			AssertEquals(name + ".Value", value, info.Value);
		}

		public void TestShipperRegNo()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestOrg";

			var addressWithoutCusCode = orgHeader.Addresses.AddNew();
			var oH_Category = orgHeader.OH_Category;
			orgHeader.OH_Category = null;
			bill.ABL_OA_Shipper = addressWithoutCusCode.PK;

			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);

			orgHeader.OH_Category = oH_Category;
			var addressWithCusCode = orgHeader.Addresses.AddNew();
			addressWithCusCode.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			addressWithCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", GlbCompany.CurrentCompany.Country.Code);

			AssertEquals("pre-condition", ZString.Empty, bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = addressWithCusCode.PK;

			AssertEquals("123456", bill.ABL_ShipperRegNo.ToString());
			AssertEquals("2", bill.ABL_ShipperRegNoType);
		}

		public void TestConsigneeRegNo()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestOrg";

			var addressWithoutCusCode = orgHeader.Addresses.AddNew();
			var oH_Category = orgHeader.OH_Category;
			orgHeader.OH_Category = null;
			bill.ABL_OA_Consignee = addressWithoutCusCode.PK;

			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);

			orgHeader.OH_Category = oH_Category;
			var addressWithCusCode = orgHeader.Addresses.AddNew();
			addressWithCusCode.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			addressWithCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", GlbCompany.CurrentCompany.Country.Code);

			AssertEquals("pre-condition", ZString.Empty, bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = addressWithCusCode.PK;

			AssertEquals("123456", bill.ABL_ConsigneeRegNo.ToString());
			AssertEquals("2", bill.ABL_ConsigneeRegNoType);
		}

		public void TestNotifyPartyRegNo()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestOrg";

			var addressWithoutCusCode = orgHeader.Addresses.AddNew();
			var oH_Category = orgHeader.OH_Category;
			orgHeader.OH_Category = null;
			bill.ABL_OA_NotifyParty = addressWithoutCusCode.PK;

			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);

			orgHeader.OH_Category = oH_Category;
			var addressWithCusCode = orgHeader.Addresses.AddNew();
			addressWithCusCode.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			addressWithCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", GlbCompany.CurrentCompany.Country.Code);

			AssertEquals("pre-condition", ZString.Empty, bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_NotifyParty = addressWithCusCode.PK;

			AssertEquals("123456", bill.ABL_NotifyPartyRegNo.ToString());
			AssertEquals("2", bill.ABL_NotifyPartyRegNoType);
		}

		public void TestGrossWeightInKilogramsSafe()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			const decimal testGrossWeight = 8.3m;
			const string testGrossWeightUQ = "LB";

			var expectedInKilogramsSafe = new ZWeight(testGrossWeight, testGrossWeightUQ).InKilogramsSafe;
			bill.ABL_GrossWeight = testGrossWeight;
			bill.ABL_GrossWeightUQ = testGrossWeightUQ;

			AssertEquals(expectedInKilogramsSafe, bill.GrossWeightInKilogramsSafe);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes();

			CombineAssertions(() =>
			{
				AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);

				AssertEquals("Expected type for AdditionalInfo", typeof(TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals("Expected type for SupportingDocument", typeof(TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			});
		}
	}
}
