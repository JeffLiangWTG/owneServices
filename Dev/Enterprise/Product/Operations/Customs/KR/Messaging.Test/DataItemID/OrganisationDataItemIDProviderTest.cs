using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class OrganisationDataItemIDProviderTest : TestCaseWithFactory
	{
		public void TestGetItemID()
		{
			var supplier = new OrganisationForTest();
			supplier.Role = RoleType.Supplier;
			AssertEquals("A301", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._830, supplier.Role, nameof(supplier.CompanyName)));
			AssertEquals("A303", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._830, supplier.Role, nameof(supplier.AddressLine1)));
			AssertEquals("A601", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._929, supplier.Role, nameof(supplier.CompanyName)));
			AssertNullOrEmpty(OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._5DP, supplier.Role, nameof(supplier.CompanyName)));

			var manufacturer = new OrganisationForTest();
			manufacturer.Role = RoleType.Manufacturer;
			AssertEquals("A401", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._830, manufacturer.Role, nameof(manufacturer.CompanyName)));
			AssertEquals("A406", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._830, manufacturer.Role, nameof(manufacturer.Postcode)));
			AssertNullOrEmpty(OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._929, manufacturer.Role, nameof(supplier.CompanyName)));
			AssertNullOrEmpty(OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._5DP, manufacturer.Role, nameof(supplier.CompanyName)));

			var shipper = new OrganisationForTest();
			shipper.Role = RoleType.Shipper;
			AssertEquals("A616", OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._929, shipper.Role, nameof(supplier.CompanyName)));
			AssertNullOrEmpty(OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._830, shipper.Role, nameof(manufacturer.CompanyName)));
			AssertNullOrEmpty(OrganisationDataItemIDProvider.GetItemID(ElectronicDocumentTypeList.Codes._5DP, shipper.Role, nameof(supplier.CompanyName)));
		}

		public void TestIsOrganisationDataItemID()
		{
			Assert(OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, ExportAmendmentDataItemIDList.Codes.A201));
			Assert(!OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, FTAAmendmentDataItemIDList.Codes._03K));

			Assert(OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, ExportAmendmentDataItemIDList.Codes.A202));
			Assert(!OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, FTAAmendmentDataItemIDList.Codes._03L));

			Assert(!OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, ExportAmendmentDataItemIDList.Codes.A202));
			Assert(OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, FTAAmendmentDataItemIDList.Codes._03L));

			Assert(!OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, ExportAmendmentDataItemIDList.Codes.A201));
			Assert(OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, FTAAmendmentDataItemIDList.Codes._03K));
		}

		class OrganisationForTest : IOrganization
		{
			public RoleType Role { get; set; }
			public ZString CompanyName { get; set; }
			public ZString AddressLine1 { get; set; }
			public ZString AddressLine2 { get; set; }
			public ZString Postcode { get; set; }
			public ZString RoadNameCode { get; set; }
			public ZString BuildingNumber { get; set; }
			public ZString CountryCode { get; set; }
			public ZString PhoneNumber { get; set; }
			public ZString ExtensionNumber { get; set; }
			public ZString Email { get; set; }
			public ZString MobileNumber { get; set; }
			public ZString FaxNumber { get; set; }
			public ZString RepresentativeName { get; set; }
			public ZBool IsIndividual { get; set; }

			public ZString BusinessRegNo { get; }
			public ZString KoreanRegNoForResident { get; }
			public ZString UnipassIDForOrganization { get; }
			public ZString BuyerID { get; }
			public ZString OfficeID { get; }
			public ZString KoreanRegNoForForeigner { get; }
			public ZString PassportNo { get; }
			public ZString UnipassIDForIndividual { get; }
			public ZString CertificateOfOriginExporterNumber { get; }
			public ZString CorporationCode { get; }
			public ZString CarrierCode { get; }
			public ZString ECommerceCompanyID { get; }
			public ZString ForeignCompanyID { get; }
		}
	}
}
