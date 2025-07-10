using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeAddressValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportFromValueObjectCollection()
		{
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.SysMergeOrgAddressCollection xsdAddressCollection = new Xsd.SysMergeOrgAddressCollection();
			Xsd.SysMergeOrgAddress xsdMainAddress = xsdAddressCollection.AddNew();
			xsdMainAddress.AddressLine1 = "Main";
			xsdMainAddress.AdditionalAddressInformation = "AdditionalAdd";
			xsdMainAddress.Country = "US";
			xsdMainAddress.PK = Guid.NewGuid().ToString();
			Xsd.SysMergeOrgAddress otherNewAddressValue = xsdAddressCollection.AddNew();
			otherNewAddressValue.AddressLine1 = "NewOther";
			otherNewAddressValue.PK = Guid.NewGuid().ToString();

			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notify);
			AddressHelper.ImportFromValueObjectCollection(xsdAddressCollection, org, context);

			ZQuery query = new ZQuery(OrgAddressSchema.OA_OH, org.PK);
			OrgAddress[] addresses = org.Factory.Load<OrgAddress>(query);
			AssertEquals("There should be 2 addresses imported", 2, addresses.Length);
			AssertEquals("Main Address (imported first)", "Main", addresses[0].OA_Address1);
			AssertEquals("Main Additional Address", "AdditionalAdd", addresses[0].OA_AdditionalAddressInformation);
			AssertEquals("Main Country Code", "US", addresses[0].OA_RN_NKCountryCode);
			AssertEquals("Main Address PK", xsdMainAddress.PK, addresses[0].PK.ToString());
			AssertEquals("New address (imported last)", "NewOther", addresses[1].OA_Address1);
			AssertEquals("New Address PK", otherNewAddressValue.PK, addresses[1].PK.ToString());
		}

		public void TestPhoneNumbers()
		{
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.SysMergeOrgAddressCollection xsdAddressCollection = new Xsd.SysMergeOrgAddressCollection();
			Xsd.SysMergeOrgAddress xsdMainAddress = xsdAddressCollection.AddNew();
			xsdMainAddress.PK = Guid.NewGuid().ToString();
			xsdMainAddress.Phone = "1233333333333333333333333333333333333333333123";
			xsdMainAddress.Fax = "01234567890123456789";
			xsdMainAddress.Mobile = "0432100000";
			xsdMainAddress.RL_NKRelatedPortCode = "AUSYD";

			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notify);
			OrgAddress mainAddress = AddressHelper.CreateFromValueObject_Exposed(organisation, xsdMainAddress, context);

			ZString expected = new ZString("1233333333333333333333333333333333333333333123").SubstringSafe(0, mainAddress.OA_PhoneInfo.MaxLength);
			AssertEquals(expected, mainAddress.OA_Phone);
			expected = new ZString("01234567890123456789").SubstringSafe(0, mainAddress.OA_FaxInfo.MaxLength);
			AssertEquals(expected, mainAddress.OA_Fax);
			expected = new ZString("0432100000").SubstringSafe(0, mainAddress.OA_MobileInfo.MaxLength);
			AssertEquals(expected, mainAddress.OA_Mobile);
			AssertEquals("Main Address PK", xsdMainAddress.PK, mainAddress.PK.ToString());
		}

		#endregion

		#region Export

		public void TestExportPhoneNumbers()
		{
			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Address1 = "XXX";
			address.OA_Phone = "02 9999 8888";
			address.OA_Mobile = "0405 234 987";
			address.OA_Fax = "02 4444 9999";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.SysMergeOrgAddressCollection xsdAddressCollection = new Xsd.SysMergeOrgAddressCollection();
			AddressHelper.ExportToValueObjectCollection(org, xsdAddressCollection, notify);

			AssertEquals("There should be 1 Address exported", 1, xsdAddressCollection.Count);
			Xsd.SysMergeOrgAddress xsdAddress = xsdAddressCollection[0];
			AssertEquals("Phone", "02 9999 8888", xsdAddress.Phone);
			AssertEquals("Mobile", "0405 234 987", xsdAddress.Mobile);
			AssertEquals("Fax", "02 4444 9999", xsdAddress.Fax);
		}

		public void TestExportToValueObjectCollection()
		{
			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			OrgAddress mainAddress = Factory.New<OrgAddress>();
			mainAddress.OA_OH = org.PK;
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			mainAddress.OA_Address1 = "MainAddress";
			mainAddress.OA_Latitude = 1;

			OrgAddress payablesAddress = Factory.New<OrgAddress>();
			payablesAddress.OA_OH = org.PK;
			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			payablesAddress.OA_Address1 = "PayablesAddress";
			payablesAddress.OA_Email = "payables@example.com";
			payablesAddress.OA_Language = Core.Constants.Languages.ChineseSimplified;

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.SysMergeOrgAddressCollection addressValueCollection = new Xsd.SysMergeOrgAddressCollection();
			AddressHelper.ExportToValueObjectCollection(org, addressValueCollection, notify);

			AssertEquals("There should be 2 Addresses exported", 2, addressValueCollection.Count);

			for (int i = 0; i < addressValueCollection.Count; i++)
			{
				switch (addressValueCollection[i].AddressLine1)
				{
					case "MainAddress":
						AssertEquals("Latitude (MainAddress)", new ZDecimal(1), addressValueCollection[i].Latitude);
						AssertEquals("Email (MainAddress)", "", addressValueCollection[i].Email);
						AssertEquals("Is Main Address?", true, addressValueCollection[i].OrgAddressCapabilities[0].IsMainAddress);
						break;

					case "PayablesAddress":
						AssertEquals("Language (PayablesAddress)", Core.Constants.Languages.ChineseSimplified, addressValueCollection[i].Language);
						AssertEquals("Latitude (PayablesAddress)", new ZDecimal(0), addressValueCollection[i].Latitude);
						AssertEquals("Email (PayablesAddress)", "payables@example.com", addressValueCollection[i].Email);
						AssertEquals("Is Main Address?", false, addressValueCollection[i].OrgAddressCapabilities[0].IsMainAddress);
						AssertEquals("Address Type", "APM", addressValueCollection[i].OrgAddressCapabilities[0].AddressType);
						break;

					default:
						Fail("Unmatching Address: " + addressValueCollection[i].AddressLine1);
						break;
				}
			}
		}

		public void TestExportToValueObject()
		{
			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>(TestBusinessObjectKind.All);
			address.OA_RN_NKCountryCode = "US";
			Xsd.SysMergeOrgAddress addressValue = AddressHelper.ExportToValueObject(address, new NotificationBuffer());

			AssertEquals("#1", addressValue.AddressLine1);
			AssertEquals("Address2", addressValue.AddressLine2);
			AssertEquals("AdditionalAddressInformation", addressValue.AdditionalAddressInformation);
			AssertEquals("US", addressValue.Country);
			AssertEquals("City", addressValue.CityOrSuburb);
			AssertEquals("State", addressValue.StateOrProvince);
			AssertEquals("PostCode", addressValue.PostCode);
			AssertEquals("Email", addressValue.Email);
			AssertEquals("#1", addressValue.AddressCode);
		}

		#endregion

		#region Implementation

		readonly SysMergeAddressValueObjectHelperForTesting AddressHelper = new SysMergeAddressValueObjectHelperForTesting();

		#endregion
	}
}
