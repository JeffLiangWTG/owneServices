using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalIdentifier_MaxLength()
		{
			AssertEquals(35, goodsLocation.AdditionalIdentifierInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => goodsLocation;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => goodsLocation;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => goodsLocation;

		public void SetDefaultValuesIfNeeded()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = "OrgHeader name";

				var address = orgHeader.MainAddress;
				address.OA_Address1 = "OrgAddress Street 1";
				address.OA_Address2 = "OrgAddress Street 2";
				address.OA_City = "OrgAddress City";
				address.OA_PostCode = "11111";
				address.OA_RN_NKCountryCode = "DE";
				address.OA_RL_NKRelatedPortCode = "DEBER";

				var geography = new ZGeography();

				var supplierPickupAddress = entryInstruction.JobDeclaration.SupplierPickupAddress;
				supplierPickupAddress.OrganisationPK = orgHeader.PK;
				supplierPickupAddress.E2_CompanyName = "Override name";
				supplierPickupAddress.E2_Address1 = "Override Street 1";
				supplierPickupAddress.E2_Address2 = "Override Street 2";
				supplierPickupAddress.E2_City = "Override City";
				supplierPickupAddress.E2_Postcode = "00000";
				supplierPickupAddress.E2_RN_NKCountryCode = "XY";
				supplierPickupAddress.E2_GeoLocation = geography;

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertEquals("CGL_Qualifier is set to 'Z'", "OrgHeader name", goodsLocation.LoadingPlace);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "OrgAddress Street 1", goodsLocation.Address.E2_Address1);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "OrgAddress Street 2", goodsLocation.Address.E2_Address2);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "OrgAddress City", goodsLocation.Address.E2_City);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "11111", goodsLocation.Address.E2_Postcode);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "DE", goodsLocation.Address.E2_RN_NKCountryCode);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertEquals("Unloco value defaulted", "DEBER", goodsLocation.Unlocode);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.LoadingPlace);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.Address.E2_Address1);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.Address.E2_Address2);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.Address.E2_City);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.Address.E2_Postcode);
				AssertEquals("Clear when CGL_Qualifier is set to 'U'", ZString.Empty, goodsLocation.Address.E2_RN_NKCountryCode);

				supplierPickupAddress.E2_AddressOverride = true;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertEquals("Unloco value cleaned", ZString.Empty, goodsLocation.Unlocode);
				AssertEquals("CGL_Qualifier is set to 'Z'", "Override name", goodsLocation.LoadingPlace);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "Override Street 1", goodsLocation.Address.E2_Address1);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "Override Street 2", goodsLocation.Address.E2_Address2);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "Override City", goodsLocation.Address.E2_City);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "00000", goodsLocation.Address.E2_Postcode);
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'Z'", "XY", goodsLocation.Address.E2_RN_NKCountryCode);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertEquals("Unloco value cleaned", ZString.Empty, goodsLocation.Unlocode);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.LoadingPlace);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.Address.E2_Address1);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.Address.E2_Address2);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.Address.E2_City);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.Address.E2_Postcode);
				AssertEquals("Clear when CGL_Qualifier is set to 'V'", ZString.Empty, goodsLocation.Address.E2_RN_NKCountryCode);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				AssertEquals("Set from pickup address when CGL_Qualifier is set to 'W'", geography, goodsLocation.Address.E2_GeoLocation);
			});
		}

		public void TestSetTypeOfLocation()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals("A", goodsLocation.CGL_Type);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertEquals("B", goodsLocation.CGL_Type);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals("D", goodsLocation.CGL_Type);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertEquals("D", goodsLocation.CGL_Type);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertEquals("D", goodsLocation.CGL_Type);
		}

		public void TestAdditionalIdentifierMaxLenght()
		{
			AssertEquals(35, goodsLocation.CGL_AdditionalIdentifierInfo.MaxLength);
		}

		public void TestLoadingPlaceMaxLenght()
		{
			AssertEquals(goodsLocation.Address.E2_CompanyNameInfo.MaxLength, goodsLocation.LoadingPlaceInfo.MaxLength);
		}

		public void TestLoadingPlaceMaxCaption()
		{
			AssertEquals("Loading Place", DataBoundResourceStrings.GetDataForProperty(goodsLocation.LoadingPlaceInfo.PropertyDescriptor).Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_LocationUse = "DEP";
		}

		JobDeclaration declaration;
		Declaration.CusEntryInstruction entryInstruction;
		CusGoodsLocation goodsLocation;
	}
}
