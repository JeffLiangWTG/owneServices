using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestIdentificationHolderPK_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.IdentificationHolderPKInfo).Caption, NUnit.Framework.Is.EqualTo("Organization"));
		}

		[ExpectNoExceptions]
		public void TestIdentificationHolderPK_Getter()
		{
			CombineAssertions(() =>
			{
				locationAddress.E2_AdditionalAddressInformation = ZString.Empty;
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "Empty string");

				locationAddress.E2_AdditionalAddressInformation = ZGuid.Empty.ToString();
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "ZGuid.Empty");

				locationAddress.E2_AdditionalAddressInformation = ZGuid.Invalid.ToString();
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Invalid), "ZGuid.Invalid");

				locationAddress.E2_AdditionalAddressInformation = ZGuid.Missing.ToString();
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Missing), "ZGuid.Missing");

				locationAddress.E2_AdditionalAddressInformation = ZGuid.BrettsGuid.ToString();
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.BrettsGuid), "Valid ZGuid");

				locationAddress.E2_AdditionalAddressInformation = "TDGPD";
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "Invalid string");
			});
		}

		[ExpectNoExceptions]
		public void TestIdentificationHolderPK_Setter()
		{
			CombineAssertions(() =>
			{
				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				NUnit.Framework.Assert.That(locationAddress.E2_AdditionalAddressInformation, NUnit.Framework.Is.EqualTo(ZGuid.Empty.ToString()).Using(CustomComparers.TypeComparison), "ZGuid.Empty");

				locationAddress.IdentificationHolderPK = ZGuid.Invalid;
				NUnit.Framework.Assert.That(locationAddress.E2_AdditionalAddressInformation, NUnit.Framework.Is.EqualTo(ZGuid.Invalid.ToString()).Using(CustomComparers.TypeComparison), "ZGuid.Invalid");

				locationAddress.IdentificationHolderPK = ZGuid.Missing;
				NUnit.Framework.Assert.That(locationAddress.E2_AdditionalAddressInformation, NUnit.Framework.Is.EqualTo(ZGuid.Missing.ToString()).Using(CustomComparers.TypeComparison), "ZGuid.Missing");

				locationAddress.IdentificationHolderPK = ZGuid.BrettsGuid;
				NUnit.Framework.Assert.That(locationAddress.E2_AdditionalAddressInformation, NUnit.Framework.Is.EqualTo(ZGuid.BrettsGuid.ToString()).Using(CustomComparers.TypeComparison), "Valid ZGuid");
			});
		}

		[ExpectNoExceptions]
		public void TestIdentificationHolderPKChanged()
		{
			var identificationHolderPKCalled = false;
			locationAddress.IdentificationHolderPKChanged += delegate
			{
				identificationHolderPKCalled = true;
			};

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(identificationHolderPKCalled, NUnit.Framework.Is.EqualTo(false), "Before changing IdentificationHolderPK");

				locationAddress.IdentificationHolderPK = ZGuid.BrettsGuid;
				NUnit.Framework.Assert.That(identificationHolderPKCalled, NUnit.Framework.Is.EqualTo(true), "After changing IdentificationHolderPK");
			});
		}

		[ExpectNoExceptions]
		public void TestIdentificationHolder()
		{
			CombineAssertions(() =>
			{
				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolder, NUnit.Framework.Is.EqualTo(default(OrgHeader)), "ZGuid.Empty - should be [null]");

				locationAddress.IdentificationHolderPK = ZGuid.Invalid;
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolder, NUnit.Framework.Is.EqualTo(default(OrgHeader)), "ZGuid.Invalid - should be [null]");

				locationAddress.IdentificationHolderPK = ZGuid.Missing;
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolder, NUnit.Framework.Is.EqualTo(default(OrgHeader)), "ZGuid.Missing - should be [null]");

				var organisation = Factory.New<OrgHeader>();
				locationAddress.IdentificationHolderPK = organisation.PK;
				NUnit.Framework.Assert.That(locationAddress.IdentificationHolder, NUnit.Framework.Is.EqualTo(organisation), "Valid ZGuid");
			});
		}

		[ExpectNoExceptions]
		public void TestE2_Postcode_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_PostcodeInfo).Caption, NUnit.Framework.Is.EqualTo("Postcode"));
		}

		[ExpectNoExceptions]
		public void TestE2_City_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_CityInfo).Caption, NUnit.Framework.Is.EqualTo("City"));
		}

		[ExpectNoExceptions]
		public void TestE2_RN_NKCountryCode_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_RN_NKCountryCodeInfo).Caption, NUnit.Framework.Is.EqualTo("Country"));
		}

		[ExpectNoExceptions]
		public void TestE2_Address1_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_Address1Info).Caption, NUnit.Framework.Is.EqualTo("Street + Number"));
		}

		[ExpectNoExceptions]
		public void TestE2_Address1AndE2_Address2_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_Address1AndE2_Address2Info).Caption, NUnit.Framework.Is.EqualTo("Street + Number"));
		}

		[ExpectNoExceptions]
		public void TestE2_Contact_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_ContactInfo).Caption, NUnit.Framework.Is.EqualTo("Name"));
		}

		[ExpectNoExceptions]
		public void TestE2_Phone_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_PhoneInfo).Caption, NUnit.Framework.Is.EqualTo("Phone Number"));
		}

		[ExpectNoExceptions]
		public void TestE2_Email_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_EmailInfo).Caption, NUnit.Framework.Is.EqualTo("Email"));
		}

		[ExpectNoExceptions]
		public void TestE2_GovRegNum_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_GovRegNumInfo).Caption, NUnit.Framework.Is.EqualTo("EORI Number"));
		}

		[ExpectNoExceptions]
		public void TestE2_GovRegNumReadOnly()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "By default E2_GovRegNum is editable");

				locationAddress.E2_GovRegNumReadOnly = true;
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "E2_GovRegNum read-only");

				locationAddress.E2_GovRegNumReadOnly = false;
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "E2_GovRegNum editable");
			});
		}

		[ExpectNoExceptions]
		public void TestAuthorisationNumber_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.AuthorisationNumberInfo).Caption, NUnit.Framework.Is.EqualTo("Authorization Number"));
		}

		[ExpectNoExceptions]
		public void TestAuthorisationNumber_ReadOnly()
		{
			CombineAssertions(() =>
			{
				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				NUnit.Framework.Assert.That(locationAddress.AuthorisationNumberInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "IdentificationHolderPK empty");

				locationAddress.IdentificationHolderPK = ZGuid.BrettsGuid;
				NUnit.Framework.Assert.That(locationAddress.AuthorisationNumberInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "IdentificationHolderPK valid");

				locationAddress.IdentificationHolderPK = ZGuid.Invalid;
				NUnit.Framework.Assert.That(locationAddress.AuthorisationNumberInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "IdentificationHolderPK invalid");
			});
		}

		[ExpectNoExceptions]
		public void TestE2_Latitude_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_LatitudeInfo).Caption, NUnit.Framework.Is.EqualTo("GNSS Latitude"), "Caption");
		}

		[ExpectNoExceptions]
		public void TestE2_Longitude_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(locationAddress.E2_LongitudeInfo).Caption, NUnit.Framework.Is.EqualTo("GNSS Longitude"), "Caption");
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(locationAddress.Lookups, NUnit.Framework.Is.TypeOf<CusGoodsLocationAddressLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(locationAddress.Validation, NUnit.Framework.Is.TypeOf<CusGoodsLocationAddressValidation>());
		}

		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			var goodsLocation = Factory.New<CusGoodsLocation>();
			var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
			goodsLocationAddress.E2_ParentID = goodsLocation.PK;
			goodsLocationAddress.E2_ParentTableCode = goodsLocation.TablePrefix;
			NUnit.Framework.Assert.That(goodsLocationAddress.GoodsLocation, NUnit.Framework.Is.EqualTo(goodsLocation));
		}

		[ExpectNoExceptions]
		public void TestE2_RN_NKCountryCodeReadonlyCoreReadonly()
		{
			NUnit.Framework.Assert.That(locationAddress.E2_RN_NKCountryCodeInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "E2_RN_NKCountryCode readonly");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "DEP";
			cusGoodsLocation.Parent = Factory.NewWithValidTestData<Business.Declaration.CusEntryInstruction>();
			locationAddress = cusGoodsLocation.Address;
		}
		CusGoodsLocationAddress locationAddress;
	}
}
