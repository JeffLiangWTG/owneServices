using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(PreValidateTraderUniversalEventBuilder))]
	sealed class PreValidateTraderUniversalEventBuilderTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor()
		{
			_ = new PreValidateTraderNotifications(null);
		}

		[TestDate(2024, 01, 16, 13, 14, 15, 678)]
		public void TestBuildUniversalEvent()
		{
			var exciseCodes = PreValidateTraderHelper.GetProductCodes(traderInfo);
			AssertEquals("Count", 2, exciseCodes.Count());
			AssertCollectionContains(exciseCodes, s => s == "1,10,11,12,2,3,4,5,6,7");
			AssertCollectionContains(exciseCodes, s => s == "8,9");

			var traders = PreValidateTraderHelper.GetDistinctTraders(traderInfo);
			AssertEquals("Count", 3, traders.Count());
			var expectedTraders = new PreValidateTraderInfo.TraderData[]
			{
				new PreValidateTraderInfo.TraderData("TEN0000001", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN0000001", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
				new PreValidateTraderInfo.TraderData("TEN0000002", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
			};
			AssertContainsExactElementsInAnyOrder(expectedTraders, traders);

			var sender = new PreValidateTraderSendingObject(declaration, traders.First(), exciseCodes.First());
			using var universalEvent = new PreValidateTraderUniversalEventBuilder(sender).BuildUniversalEvent();
			var company = GlbCompany.CurrentCompany;
			var expectedQuery = string.Format(ExpectedEvent, company.GC_Code, company.GC_RN_NKCountryCode, company.Country.Description, company.CompanyName);
			var xml = PreValidateTraderHelper.ConvertToXml(universalEvent);
			AssertMultilineASCIIEquals("Universal Event Expected", expectedQuery, xml);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN0000001", Core.Constants.CountryCodes.UnitedKingdom).PK;
			declaration.JE_CustomsProfile = "AR1";
			declaration.ImporterDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN0000001", Core.Constants.CountryCodes.UnitedKingdom).PK;
			declaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN0000001", Core.Constants.CountryCodes.Portugal, isWarehouse: true).PK;
			declaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN0000002", Core.Constants.CountryCodes.UnitedKingdom, isWarehouse: true).PK;

			var invoice = declaration.Invoices.AddNew();
			for (var i = 1; i < 13; i++)
			{
				var value = i.ToString();
				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_PartNo = value;
				line1.JI_Tariff = value;
				line1.ZG_ExciseProductCode = value;
				line1.JI_NDescription = value;
			}

			traderInfo = new PreValidateTraderInfo(declaration);
		}

		EMCSJobDeclaration declaration;
		PreValidateTraderInfo traderInfo;

		const string ExpectedEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>{0}</Code>
        <Country>
          <Code>{1}</Code>
          <Name>{2}</Name>
        </Country>
        <Name>{3}</Name>
      </Company>
      <DataProvider>ENTSVR{0}</DataProvider>
      <EnterpriseID>ENT</EnterpriseID>
      <ServerID>SVR</ServerID>
    </DataContext>

    <EventTime>2024-01-16T13:14:15.678+00:00</EventTime>
    <EventType>EVC</EventType>
    <EventReference>|MST=QUERY|SER=GBEMCS</EventReference>
    <ContextCollection>
      <Context>
        <Type>PreValidateTraderBody</Type>
        <Value>eyJleGNpc2VUcmFkZXJWYWxpZGF0aW9uUmVxdWVzdCI6eyJleGNpc2VUcmFkZXJSZXF1ZXN0Ijp7ImV4Y2lzZVJlZ2lzdHJhdGlvbk51bWJlciI6IlRFTjAwMDAwMDEiLCJlbnRpdHlHcm91cCI6IlVLIFJlY29yZCIsInZhbGlkYXRlUHJvZHVjdEF1dGhvcmlzYXRpb25SZXF1ZXN0IjpbeyJwcm9kdWN0Ijp7ImV4Y2lzZVByb2R1Y3RDb2RlIjoiMSJ9fSx7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiIxMCJ9fSx7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiIxMSJ9fSx7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiIxMiJ9fSx7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiIyIn19LHsicHJvZHVjdCI6eyJleGNpc2VQcm9kdWN0Q29kZSI6IjMifX0seyJwcm9kdWN0Ijp7ImV4Y2lzZVByb2R1Y3RDb2RlIjoiNCJ9fSx7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiI1In19LHsicHJvZHVjdCI6eyJleGNpc2VQcm9kdWN0Q29kZSI6IjYifX0seyJwcm9kdWN0Ijp7ImV4Y2lzZVByb2R1Y3RDb2RlIjoiNyJ9fV19fX0=</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>AR1</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
	}
}
