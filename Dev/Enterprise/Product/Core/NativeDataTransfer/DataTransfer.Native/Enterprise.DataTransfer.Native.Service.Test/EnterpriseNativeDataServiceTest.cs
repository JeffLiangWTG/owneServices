using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Moq;

namespace Enterprise.DataTransfer.Native.Service
{
	public class EnterpriseNativeDataServiceTest : TestCaseWithFactory
	{
		#region Request With OrgPartRelation in CusClassPartPivot

		const string RequestWithOrgPartRelationInCusClassPartPivot = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDIDUSCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version=""2.0"">
      <OrgSupplierPart Action=""MERGE"">
        <IsActive>true</IsActive>
        <PartNum>PRODUCTA_AAA</PartNum>
        <StockKeepingUnit>BAG</StockKeepingUnit>
        <Desc>BEN WANTS TO BE YOUR MOTHER</Desc>
        <CusClassPartPivotCollection>
          <CusClassPartPivot Action=""MERGE"">
            <UsageComment></UsageComment>
            <TariffNum>2222</TariffNum>
            <ChildType>HTI</ChildType>
            <SupplementalTariff>9801001010</SupplementalTariff>
            <Country TableName=""RefCountry"">
              <Code>US</Code>
            </Country>
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
            </OrgHeader>
          </CusClassPartPivot>
        </CusClassPartPivotCollection>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
            </OrgHeader>
          </OrgPartRelation>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ABCEXPBNE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

		#endregion // Request With OrgPartRelation in CusClassPartPivot

		public void TestWI00083695_1()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCEXPBNE";
			org1.OH_FullName = "ABC EXPORTERS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CRAIMPCHI";

			Factory.Save();

			var result = CallUpdateOnService(RequestWithOrgPartRelationInCusClassPartPivot).ToString();
			Assert(result, result.Contains("<Status>Accepted</Status>"));

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PRODUCTA_AAA");
			var product = factory.LoadTop1<Customs.US.Business.OrgSupplierPart>(query);
			AssertEquals(1, product.PivotsForBinding.Count);
			AssertNotNull("OrgPartRelation was correctly imported", product.PivotsForBinding[0].RelatedOrganisation);
			AssertEquals("PRODUCTA_AAA", product.PivotsForBinding[0].SupplierPart.OP_PartNum);
		}

		public void TestWI00083695_2()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCEXPBNE";
			org1.OH_FullName = "ABC EXPORTERS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CRAIMPCHI";

			var product1 = Factory.New<Customs.US.Business.OrgSupplierPart>();
			product1.OP_PartNum = "XXX";
			product1.OP_Desc = "XXX DESC";
			var orgPartRelation11 = product1.RelatedOrganisations.AddOwner(org1);
			orgPartRelation11.OU_LocalPartNumber = "ABC ON XXX";
			var orgPartRelation12 = product1.RelatedOrganisations.AddOwner(org2);
			orgPartRelation12.OU_LocalPartNumber = "CRA ON XXX";

			var product2 = Factory.New<Customs.US.Business.OrgSupplierPart>();
			product2.OP_PartNum = "PRODUCTA_AAA";
			product2.OP_Desc = "AAA DESC";
			var orgPartRelation21 = product2.RelatedOrganisations.AddOwner(org1);
			orgPartRelation21.OU_LocalPartNumber = "ABC ON AAA";
			var orgPartRelation22 = product2.RelatedOrganisations.AddOwner(org2);
			orgPartRelation22.OU_LocalPartNumber = "CRA ON AAA";
			var pivot21 = product2.PivotsForBinding.AddNew();
			pivot21.CI_ChildType = "HTI";
			pivot21.CI_TariffNum = "332";
			pivot21.CI_OH = orgPartRelation21.OU_OH;

			Factory.Save();

			var result = CallUpdateOnService(RequestWithOrgPartRelationInCusClassPartPivot).ToString();
			Assert(result, result.Contains("<Status>Accepted</Status>"));

			var factory = new BusinessObjectFactory();
			var product2Copy = factory.Load<Customs.US.Business.OrgSupplierPart>(product2.PK);
			AssertEquals(2, product2Copy.PivotsForBinding.Count);
			AssertEquals("PRODUCTA_AAA", product2Copy.PivotsForBinding[0].SupplierPart.OP_PartNum);
			AssertEquals("ABCEXPBNE", product2Copy.PivotsForBinding[0].RelatedOrganisation.OH_Code);
			AssertEquals("PRODUCTA_AAA", product2Copy.PivotsForBinding[1].SupplierPart.OP_PartNum);
			AssertEquals("CRAIMPCHI", product2Copy.PivotsForBinding[1].RelatedOrganisation.OH_Code);
		}

		public void TestGeographyValid()
		{
			GlobalDefinition.Instance.AddDummyBizoToTableMappings();
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDIDUSCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <Geography>POINT (139.75516 35.65784)</Geography>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";
			var result = CallUpdateOnService(xml).ToString();
			Assert(result, result.Contains("<Status>Accepted</Status>"));
			var dummy = Factory.Load<DummyBusinessObject>(new ZQuery()).FirstOrDefault();
			AssertEquals("POINT (139.75516 35.65784)", dummy.Z0_Geography.ToString());
		}

		public void TestGeographyEmpty()
		{
			GlobalDefinition.Instance.AddDummyBizoToTableMappings();
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDIDUSCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <Geography></Geography>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";
			var result = CallUpdateOnService(xml).ToString();
			Assert(result, result.Contains("<Status>Accepted</Status>"));
			var dummy = Factory.Load<DummyBusinessObject>(new ZQuery()).FirstOrDefault();
			AssertEquals("staff.Geography", ZGeography.Empty, dummy.Z0_Geography);
		}

		public void TestGeographyInvalid()
		{
			GlobalDefinition.Instance.AddDummyBizoToTableMappings();
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDIDUSCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <Geography>North Pole I'm coming to get you, Santa Claus, love Brett Shearer.</Geography>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";
			var result = CallUpdateOnService(xml).ToString();
			Assert(result, result.Contains("<Status>Rejected</Status>"));
			Assert(result,
				result.Contains(
					"<Item>Error - [DummyBizo.Geography] : Type of value has a mismatch with column typeCouldn't store &lt;North Pole I'm coming to get you, Santa Claus, love Brett Shearer.&gt; in Geography Column.  Expected type is SqlGeography.</Item>"));
		}

		public void TestPartialRetrival_WrongFieldName()
		{
			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""WrongFiledName"">AUSY%</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(),
				result.ToString().Contains("Error - There is no FieldName 'WrongFiledName' in table 'RefUNLOCO'"));
		}

		public void TestXmlWithBadDate()
		{
			const string request = @"<CurrencyExchangeRate xmlns=""http://www.cargowise.com/Schemas/Native"">
  <CriteriaGroup Type=""Partial"">
	<Criteria Entity=""RefExchangeRate"" FieldName=""ExRateType"">CUS</Criteria>
	<Criteria Entity=""RefExchangeRate"" FieldName=""StartDate"">0001-01-02</Criteria>
	<Criteria Entity=""RefExchangeRate.GlbCompany"" FieldName=""Code"">QMJ</Criteria>
  </CriteriaGroup>
</CurrencyExchangeRate>";

			var result = CallRetrieveOnService(request);

			AssertEquals(true, result.ToString().Contains(@"<EntityInfo>
    <Name>CurrencyExchangeRate</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Rejected</Status>
  <Information>
    <Item>Error - The conversion of a varchar data type to a smalldatetime data type resulted in an out-of-range value.</Item>
    <Item>Information - 0 matches found.</Item>
  </Information>
  <Data />
</Response>"));
		}

		public void TestPartialRetrival_MutipleCriteriaForSingleTable()
		{
			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSY%</Criteria>
		<Criteria Entity=""RefUNLOCO"" FieldName=""PortName"">Sydney</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request).ToString();

			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
 "));
		}

		public void TestPartialRetrival_OrgHeaderAndQuery()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "TestOrg1";
			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "TestOrg2";
			var headerAlt1 = Factory.New<OrgHeader>();
			headerAlt1.OH_Code = "TestAltOrg1";
			var headerAlt2 = Factory.New<OrgHeader>();
			headerAlt2.OH_Code = "TestAltOrg2";
			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Body>
		<Organization version=""2.0"">
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestO%</Criteria>
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">%tOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>";

			var result = CallRetrieveOnService(request).ToString();

			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
"));
			Assert(result, result.Contains(header1.PK.ToString()));
			Assert(result, !result.Contains(header2.PK.ToString()));
			Assert(result, !result.Contains(headerAlt1.PK.ToString()));
			Assert(result, !result.Contains(headerAlt2.PK.ToString()));
		}

		public void TestPartialRetrival_OrgHeaderOrQuery()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "TestOrg1";
			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "TestOrg2";
			var headerAlt1 = Factory.New<OrgHeader>();
			headerAlt1.OH_Code = "TestAltOrg1";
			var headerAlt2 = Factory.New<OrgHeader>();
			headerAlt2.OH_Code = "TestAltOrg2";
			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Body>
		<Organization version=""2.0"">
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestO%</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">%tOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>";

			var result = CallRetrieveOnService(request).ToString();

			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 3 matches found.</Item>
  </Information>
"));
			Assert(result, result.Contains(header1.PK.ToString()));
			Assert(result, result.Contains(header2.PK.ToString()));
			Assert(result, result.Contains(headerAlt1.PK.ToString()));
			Assert(result, !result.Contains(headerAlt2.PK.ToString()));
		}

		public void TestPartialRetrival_OrgHeaderOrQuery_SingleResult()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "TestOrg1";
			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Body>
		<Organization version=""2.0"">
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOr%</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>
";

			var result = CallRetrieveOnService(request).ToString();
			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
"));
			Assert(result, result.Contains(header1.PK.ToString()));
		}

		public void TestPartialRetrival_OrgHeaderOrQuery_SingleResultWithNoMatch()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "TestOrg1";
			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Body>
		<Organization version=""2.0"">
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOr%</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">NoOrgMatch</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>
";

			var result = CallRetrieveOnService(request).ToString();
			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
"));
			Assert(result, result.Contains(header1.PK.ToString()));
		}

		public void TestPartialRetrival_OrgHeaderOrAndQuery()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "TestOrg1";
			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "TestOrg2";
			var headerAlt1 = Factory.New<OrgHeader>();
			headerAlt1.OH_Code = "TestAltOrg1";
			var headerAlt2 = Factory.New<OrgHeader>();
			headerAlt2.OH_Code = "TestAltOrg2";
			var headerNoMatch = Factory.New<OrgHeader>();
			headerNoMatch.OH_Code = "TestNoMatch";
			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Body>
		<Organization version=""2.0"">
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">%Org1</Criteria>
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">Test%</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">NoOrgMatch</Criteria>
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">%Org%</Criteria>
			</CriteriaGroup>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">%Org2</Criteria>
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestAlt%</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>
";

			var result = CallRetrieveOnService(request).ToString();
			Assert(result, result.Contains(@"
  <Information>
    <Item>Information - 3 matches found.</Item>
  </Information>
"));
			Assert(result, result.Contains(header1.PK.ToString()));
			Assert(result, !result.Contains(header2.PK.ToString()));
			Assert(result, result.Contains(headerAlt1.PK.ToString()));
			Assert(result, result.Contains(headerAlt2.PK.ToString()));
			Assert(result, !result.Contains(headerNoMatch.PK.ToString()));
		}

		public void TestCityQueryForCAMustNotRetrieveCitiesFromAU_CompositeKeys()
		{
			XNamespace ns = "http://www.cargowise.com/Schemas/Native";
			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
	<Body>
		<Country>
			<CriteriaGroup Type=""Partial"">
			<Criteria Entity=""RefCountry"" FieldName=""Code"">CA</Criteria>
			</CriteriaGroup>
		</Country>
	</Body>
</Native>";

			var result = CallRetrieveOnService(request);
			Assert(string.Format("Found unexpected {0} in \r\n{1}", "<Code>AU</Code>", result.ToString()),
				!result.DescendantsAndSelf(ns + "RefCityTownCollection").DescendantsAndSelf(ns + "Code")
						.Any(x => x.Value == "AU"));
		}

		public void TestCityQueryForAUMustRetrieveCitiesFromAU_CompositeKeys()
		{
			XNamespace ns = "http://www.cargowise.com/Schemas/Native";
			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
	<Body>
		<Country>
			<CriteriaGroup Type=""Partial"">
			<Criteria Entity=""RefCountry"" FieldName=""Code"">AU</Criteria>
			</CriteriaGroup>
		</Country>
	</Body>
</Native>";

			var result = CallRetrieveOnService(request);
			var countryCodeElements = result.DescendantsAndSelf(ns + "RefCityTownCollection")
											.Elements(ns + "RefCityTown")
											.Elements(ns + "Country").Elements(ns + "Code");
			AssertEquals(16065, countryCodeElements.Count());
			Assert(countryCodeElements.All(x => x.Value == "AU"));
		}

		public void TestRemoveDuplicatedProductInResponse()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "Org1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "Org2";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_Desc = "Desc1";
			part1.OP_PartNum = "PartNum1";

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_Desc = "Desc2";
			part2.OP_PartNum = "PartNum2";

			var relation2 = Factory.New<OrgPartRelation>();
			relation2.OU_OH = header1.PK;
			relation2.OU_OP = part1.PK;
			relation2.OU_Relationship = "OWN";

			var relation3 = Factory.New<OrgPartRelation>();
			relation3.OU_OH = header2.PK;
			relation3.OU_OP = part1.PK;
			relation3.OU_Relationship = "OWN";

			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""Desc"">Desc%</Criteria>
         <Criteria Entity=""OrgSupplierPart.OrgPartRelation"" FieldName=""Relationship"">OWN</Criteria>
		  <Criteria Entity=""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName=""Code"">Org1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 1 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteriaForMulipleTables()
		{
			SetupDataForMutipleTableRetrival();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum%</Criteria>
        <Criteria Entity=""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName=""Code"">Org1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 1 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteria_ActsAsAndNotOR_PartRelationship()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "Org1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "Org2";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";

			var relation1 = Factory.New<OrgPartRelation>();
			relation1.OU_OH = header1.PK;
			relation1.OU_OP = part1.PK;
			relation1.OU_Relationship = "OWN";

			var relation2 = Factory.New<OrgPartRelation>();
			relation2.OU_OH = header2.PK;
			relation2.OU_OP = part2.PK;
			relation2.OU_Relationship = "OWN";

			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
		<Criteria Entity = ""OrgSupplierPart.OrgPartRelation"" FieldName = ""Relationship"">OWN</Criteria>
		<Criteria Entity = ""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName = ""Code"">ORG1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 1 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteria_ActsAsAndNotOR_PartRelationship2()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "Org1";
			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "Org2";
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";
			var relation1 = Factory.New<OrgPartRelation>();
			relation1.OU_OH = header1.PK;
			relation1.OU_OP = part1.PK;
			relation1.OU_Relationship = "SUP";
			var relation2 = Factory.New<OrgPartRelation>();
			relation2.OU_OH = header2.PK;
			relation2.OU_OP = part1.PK;
			relation2.OU_Relationship = "OWN";
			var relation3 = Factory.New<OrgPartRelation>();
			relation3.OU_OH = header1.PK;
			relation3.OU_OP = part2.PK;
			relation3.OU_Relationship = "OWN";

			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
		<Criteria Entity = ""OrgSupplierPart.OrgPartRelation"" FieldName = ""Relationship"">OWN</Criteria>
		<Criteria Entity = ""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName = ""Code"">ORG1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 2 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteria_ActsAsAndNotOR()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "Org1";
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";
			var relation1 = Factory.New<OrgPartRelation>();
			relation1.OU_OH = header1.PK;
			relation1.OU_OP = part1.PK;
			var relation3 = Factory.New<OrgPartRelation>();
			relation3.OU_OH = header1.PK;
			relation3.OU_OP = part2.PK;

			Factory.Save();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum1</Criteria>
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum2</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 0 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteria_ActsAsAndNotOR1()
		{
			SetupDataForMutipleTableRetrival();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum%</Criteria>
        <Criteria Entity=""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName=""Code"">Pork</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 0 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteria_ActsAsAndNotOR2()
		{
			SetupDataForMutipleTableRetrival();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">Chungus</Criteria>
        <Criteria Entity=""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName=""Code"">Org1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(), result.ToString().Contains("<Item>Information - 0 matches found.</Item>"));
		}

		public void TestPartialRetrival_MutipleCriteriaForMulipleTables_NotMatchQueryType()
		{
			SetupDataForMutipleTableRetrival();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<UNLOCO>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum%</Criteria>
        <Criteria Entity=""OrgSupplierPart.OrgPartRelation.OrgHeader"" FieldName=""Code"">Org1</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var result = CallRetrieveOnService(request);

			Assert(result.ToString(),
				result.ToString().Contains(
					"<Item>Error - Criteria Entity=\"OrgSupplierPart\" is not a valid Criteria element of RefUNLOCO</Item>"));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestPartialRetrival_MutipleCriteriaForMulipleTables_JoinNotRelatedTables()
		{
			SetupDataForMutipleTableRetrival();

			const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Product>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""OrgSupplierPart"" FieldName=""PartNum"">PartNum%</Criteria>
        <Criteria Entity=""OrgSupplierPart.OrgPartRelation.RefUNLOCO"" FieldName=""Code"">Org1</Criteria>
      </CriteriaGroup>
    </Product>
  </Body>
</Native>";

			var pretestProvider = Env.GetCurrentProvider();
			try
			{
				var result = CallRetrieveOnService(request);

				Assert(result.ToString(),
					result.ToString().Contains(
						"Error - Could not found entity with name: OrgSupplierPart.OrgPartRelation.RefUNLOCO. Make sure you use Full Name for the Entity"));
			}
			finally
			{
				var postTestProvider = Env.GetCurrentProvider();
				if (!ReferenceEquals(pretestProvider, postTestProvider))
				{
					pretestProvider.Enable();
					postTestProvider.Dispose();
				}
			}
		}

		public void TestPartialRetrivalWorks()
		{
			var request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSY%</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			XElement result = CallRetrieveOnService(request);

			CombineAssertions(delegate
			{
				AssertXMLStartsWith("service.Retrieve(xml)", string.Format(@"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>UNLOCO</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 3 matches found.</Item>
  </Information>
  <Data>
    <DataItem>
      <UNLOCO version=""{0}"">
        <RefUNLOCO>".Trim(), NativeXmlInfo.Version_2011_11), result.ToString());

				AssertEndsWith("service.Retrieve(xml)", @"
        </RefUNLOCO>
      </UNLOCO>
    </DataItem>
  </Data>
</Response>".Trim(), result.ToString());
			});
		}

		public void TestNativeDataUpdateCanUpdateOK()
		{
			const string incomingXML = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <RefUNLOCO Action=""MERGE"">
        <Code>AUSYD</Code>
        <PortName>Syd-en-eee</PortName>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</Native>";

			XElement result = CallUpdateOnService(incomingXML);

			CombineAssertions(delegate
			{
				AssertXMLStartsWith("service.Retrieve(xml)", @"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>UNLOCO</Name>
    <PrimaryKey>".Trim(), result.ToString());

				AssertEndsWith("service.Retrieve(xml)", @"</PrimaryKey>
    <LocalCode>AUSYD</LocalCode>
    <ExternalCode>AUSYD</ExternalCode>
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - RefUNLOCO - 0 inserts, 1 updates, 0 deletes</Item>
  </Information>
</Response>".Trim(), result.ToString());

				var updatedUnLoco = new RowFactory().LoadFromNaturalKey(RefUNLOCOSchema.Constants.TableName,
					RefUNLOCOSchema.RL_Code, new ZString("AUSYD"), true);
				AssertNotNull("Precondition: updatedUnLoco", updatedUnLoco);
				AssertEquals("updatedUnLoco.RL_PortName", "Syd-en-eee",
					updatedUnLoco[RefUNLOCOSchema.Constants.RL_PortName]);
			});
		}

		public void TestReferenceDataUpdateCanUpdateOK()
		{
			const string incomingXML = @"
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
    <UNLOCO>
      <RefUNLOCO Action=""MERGE"">
        <Code>AUSYD</Code>
        <PortName>Syd-en-eee</PortName>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</ReferenceData>";

			XElement result = CallUpdateOnService(incomingXML);

			CombineAssertions(delegate
			{
				AssertXMLStartsWith("service.Retrieve(xml)", @"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <EntityInfo>
    <Name>UNLOCO</Name>
    <PrimaryKey>".Trim(), result.ToString());

				AssertEndsWith("service.Retrieve(xml)", @"</PrimaryKey>
    <LocalCode>AUSYD</LocalCode>
    <ExternalCode>AUSYD</ExternalCode>
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - RefUNLOCO - 0 inserts, 1 updates, 0 deletes</Item>
  </Information>
</Response>".Trim(), result.ToString());
			});
		}

		public void TestNativeDataRetrieveCanGetValueBack()
		{
			const string incomingXML = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">
          AUSYD
        </Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			XElement result = CallRetrieveOnService(incomingXML);

			CombineAssertions(delegate
			{
				AssertXMLStartsWith("service.Retrieve(xml)", string.Format(@"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>UNLOCO</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
  <Data>
    <DataItem>
      <UNLOCO version=""{0}"">
        <RefUNLOCO>".Trim(), NativeXmlInfo.Version_2011_11), result.ToString());

				AssertEndsWith("service.Retrieve(xml)", @"
        </RefUNLOCO>
      </UNLOCO>
    </DataItem>
  </Data>
</Response>".Trim(), result.ToString());
			});
		}

		public void TestReferenceDataRetrieveCanGetValueBack()
		{
			const string incomingXML = @"
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">
          AUSYD
        </Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</ReferenceData>";

			XElement result = CallRetrieveOnService(incomingXML);

			CombineAssertions(delegate
			{
				AssertXMLStartsWith("service.Retrieve(xml)", string.Format(@"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <EntityInfo>
    <Name>UNLOCO</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 1 matches found.</Item>
  </Information>
  <Data>
    <DataItem>
      <UNLOCO version=""{0}"">
        <RefUNLOCO>".Trim(), NativeXmlInfo.Version_2011_11), result.ToString());

				AssertEndsWith("service.Retrieve(xml)", @"
        </RefUNLOCO>
      </UNLOCO>
    </DataItem>
  </Data>
</Response>".Trim(), result.ToString());
			});
		}

		public void TestNativeDataRetrieveWithNoMatchesFound()
		{
			const string incomingXML = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Declaration>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""JobDeclaration"" FieldName=""DeclarationReference"">SAE00021</Criteria>
      </CriteriaGroup>
    </Declaration>
  </Body>
</Native>";

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();

			XElement result = CallRetrieveOnService(incomingXML);

			AssertXMLEquals("service.Retrieve(xml)",
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>Declaration</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Rejected</Status>
  <Information>
    <Item>Error - The 'Declaration' Native XML dataset has been deprecated. Please use the Universal Shipment XML instead.</Item>
  </Information>
  <Data />
</Response>", result.ToString());

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();

			result = CallRetrieveOnService(incomingXML);

			AssertXMLEquals("service.Retrieve(xml)", @"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>Declaration</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 0 matches found.</Item>
  </Information>
  <Data />
</Response>
				".Trim(), result.ToString());
		}

		public void TestNativeDataRetrieve_DisableNativeXML()
		{
			const string incomingXML = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Declaration>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""JobDeclaration"" FieldName=""DeclarationReference"">SAE00021</Criteria>
      </CriteriaGroup>
    </Declaration>
  </Body>
</Native>";

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();

			XElement result = CallRetrieveOnService(incomingXML);

			AssertXMLEquals("service.Retrieve(xml)",
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>Declaration</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Rejected</Status>
  <Information>
    <Item>Error - The 'Declaration' Native XML dataset has been deprecated. Please use the Universal Shipment XML instead.</Item>
  </Information>
  <Data />
</Response>", result.ToString());

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();

			result = CallRetrieveOnService(incomingXML);

			AssertXMLEquals("service.Retrieve(xml)", @"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <EntityInfo>
    <Name>Declaration</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 0 matches found.</Item>
  </Information>
  <Data />
</Response>
				".Trim(), result.ToString());
		}

		public void TestReferenceDataRetrieveWithNoMatchesFound()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			const string incomingXML = @"
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Body>
    <Declaration>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""JobDeclaration"" FieldName=""DeclarationReference"">SAE00021</Criteria>
      </CriteriaGroup>
    </Declaration>
  </Body>
</ReferenceData>";

			XElement result = CallRetrieveOnService(incomingXML);

			AssertXMLEquals("service.Retrieve(xml)", @"
<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <EntityInfo>
    <Name>Declaration</Name>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - 0 matches found.</Item>
  </Information>
  <Data />
</Response>
				".Trim(), result.ToString());
		}

		public void TestInitialEnterpriseNativeDataService()
		{
			AssertNoExceptionThrown("No exception should be thrown",
				delegate { new TestEnterpriseNativeDataService(); });
		}

		public void TestUseDisposableActionForDbConnection()
		{
			Exception threadEx = null;
			ThreadStart threadstart1 = new ThreadStart(delegate
			{
				var service = new TestEnterpriseNativeDataService();
				const string request = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">
          AUSYD
        </Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";
				service.ShouldGetDbConnectionToFromAnotherThread = true;
				try
				{
					var result = service.RetrieveData(request);
					Assert(result, result.Contains("<Item>Information - 1 matches found.</Item>"));
				}
				catch (Exception ex)
				{
					threadEx = ex;
				}
				finally
				{
					service.ShouldGetDbConnectionToFromAnotherThread = false;
				}
			});
			Thread thread1 = new Thread(threadstart1);
			thread1.Start();
			thread1.Join(1000);
			var message = threadEx != null ? threadEx.Message : "Expect No Exception";
			Assert(message, threadEx == null);
		}

		public void TestNativeRequestSchema()
		{
			var validationNotification = string.Empty;
			var validationHandler = new ValidationEventHandler((s, e) => validationNotification += $"Line:{((IXmlLineInfo)s)?.LineNumber}\r\n\t{e.Message}\r\n");
			var readerSettings = new XmlReaderSettings
			{
				ValidationType = System.Xml.ValidationType.Schema,
				ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
			};
			var schema = XmlSchema.Read(new StringReader(new NativeXsdGenerator().GenerateNativeRequestSchema()), validationHandler);
			readerSettings.Schemas.Add(schema);
			readerSettings.ValidationEventHandler += validationHandler;

			#region xml

			#region goodXml1
			const string goodXml1 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <CurrencyExchangeRate>
      <CriteriaGroup Type=""Partial"">
        <Criteria Entity=""RefExchangeRate.RefCurrency"" FieldName =""Code"">HUF</Criteria>
      </CriteriaGroup>
    </CurrencyExchangeRate>
  </Body>
</Native>";
			#endregion

			#region goodXml2

			const string goodXml2 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">
          AUSYD
        </Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";
			#endregion

			#region xmlWithMultipleCriteriaGroups
			const string xmlWithMultipleCriteriaGroups = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
    <Body>
        <Organization>
            <CriteriaGroup Type=""Partial"">
                <Criteria Entity=""OrgHeader"" FieldName=""Code""> A% </Criteria>
                <Criteria Entity=""OrgHeader"" FieldName=""IsBroker""> True</Criteria>
            </CriteriaGroup>
            <CriteriaGroup Type=""Partial"">
                <Criteria Entity=""OrgHeader"" FieldName=""FullName""> BA% </Criteria>
            </CriteriaGroup>
        </Organization>
    </Body>
</Native>";
			#endregion

			#region badXml_TooManyRequests

			const string badXml_TooManyRequests = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
    <Organization>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""OrgHeader.OrgCusCode"" FieldName=""Code"">AUSYD</Criteria>
        <Criteria Entity=""OrgHeader.OrgCusCode.CodeCountry"" FieldName=""Code"">AU</Criteria>
      </CriteriaGroup>
    </Organization>
  </Body>
</Native>";
			const string badXml_TooManyRequests_ExpectedError = @"Line:9
	The element 'Body' in namespace 'http://www.cargowise.com/Schemas/Native' has invalid child element 'Organization' in namespace 'http://www.cargowise.com/Schemas/Native'.
";

			#endregion

			#region badXml_IncorrectType

			const string badXml_IncorrectType = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <RickAstley>
     <CriteriaGroup>
      <Criteria Entity=""GlbStaff"" FieldName=""FullName"">Rick Astley</Criteria>
      <Criteria Entity=""GlbStaff"" FieldName=""Code"">RAS</Criteria>
     </CriteriaGroup>
    </RickAstley>
  </Body>
</Native>";
			const string badXml_IncorrectType_ExpectedError = @"Line:4
	The element 'Body' in namespace 'http://www.cargowise.com/Schemas/Native' has invalid child element 'RickAstley' in namespace 'http://www.cargowise.com/Schemas/Native'. List of possible elements expected: 'AcceptabilityBand, Airline, BMSystem, CommodityCode, Company, Container, Country, CurrencyExchangeRate, CusStatement, DangerousGood, Organization, Product, Rate, ServiceLevel, Staff, Tag, TagRule, UNLOCO, Vessel, WorkflowTemplate' in namespace 'http://www.cargowise.com/Schemas/Native'.
";
			#endregion

			#region badXml_MissingType

			const string badXml_MissingType = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"" version=""2.0"">
  <Body>
  </Body>
</Native>";
			const string badXml_MissingType_ExpectedError = @"Line:2
	The 'version' attribute is not declared.
Line:4
	The element 'Body' in namespace 'http://www.cargowise.com/Schemas/Native' has incomplete content. List of possible elements expected: 'AcceptabilityBand, Airline, BMSystem, CommodityCode, Company, Container, Country, CurrencyExchangeRate, CusStatement, DangerousGood, Organization, Product, Rate, ServiceLevel, Staff, Tag, TagRule, UNLOCO, Vessel, WorkflowTemplate' in namespace 'http://www.cargowise.com/Schemas/Native'.
";
			#endregion

			#region badXml_FieldName

			const string badXml_MissingFieldName = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"">
          AUSYD
        </Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			const string badXml_MissingFieldName_ExpectedError = @"Line:6
	The required attribute 'FieldName' is missing.
";
			#endregion

			#endregion

			var goodXMLs = new[] { goodXml1, goodXml2, xmlWithMultipleCriteriaGroups };
			foreach (var xml in goodXMLs)
			{
				using (var reader = new StringReader(xml))
				using (var xmlReader = XmlReader.Create(reader, readerSettings))
				{
					XDocument.Load(xmlReader);
					AssertEquals(string.Empty, validationNotification);
					var result = new EnterpriseNativeDataService().RetrieveData(xml);
					AssertXMLContains("<Status>Accepted</Status>", result);
				}
			}

			ValidateBadXML(ref validationNotification, validationHandler, readerSettings, badXml_TooManyRequests, badXml_TooManyRequests_ExpectedError);
			ValidateBadXML(ref validationNotification, validationHandler, readerSettings, badXml_IncorrectType, badXml_IncorrectType_ExpectedError);
			ValidateBadXML(ref validationNotification, validationHandler, readerSettings, badXml_MissingType, badXml_MissingType_ExpectedError);
			ValidateBadXML(ref validationNotification, validationHandler, readerSettings, badXml_MissingFieldName, badXml_MissingFieldName_ExpectedError);
		}

		static void ValidateBadXML(ref string validationNotification, ValidationEventHandler validationHandler, XmlReaderSettings readerSettings, string badXml, string expectedValidationError)
		{
			using (var reader = new StringReader(badXml))
			using (var xmlReader = XmlReader.Create(reader, readerSettings))
			{
				var doc = XDocument.Load(xmlReader);
				AssertEquals(expectedValidationError, validationNotification);
			}

			validationNotification = string.Empty;
		}

		#region Implementation

		static XElement CallRetrieveOnService(string incomingXML)
		{
			var service = new TestEnterpriseNativeDataService();
			var xml = XElement.Parse(incomingXML);
			return service.Retrieve(xml);
		}

		static XElement CallUpdateOnService(string incomingXML)
		{
			var service = new TestEnterpriseNativeDataService();
			var xml = XElement.Parse(incomingXML);
			return service.Update(xml);
		}

		public class TestEnterpriseNativeDataService : EnterpriseNativeDataService
		{
			protected void SetAssemblyLoader()
			{
				new WebServiceEnvironmentProvider().Enable();
				AssemblyLoader.Instance = new WebAssemblyLoader();
			}
		}

		protected override void SetUp()
		{
			if (Env.CurrentBranch is GlbBranch branch && string.IsNullOrEmpty(branch.GB_WebAddress))
			{
				branch.GB_WebAddress = "http://www.wisetechglobal.com/";
				branch.Factory.Save();
			}

			DataRegistry.Instance.WebBranch = Env.CurrentBranch.PK;
			base.SetUp();
		}

		void SetupDataForMutipleTableRetrival()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "Org1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "Org2";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "PartNum3";

			var relation1 = Factory.New<OrgPartRelation>();
			relation1.OU_OH = header1.PK;
			relation1.OU_OP = part1.PK;

			var relation3 = Factory.New<OrgPartRelation>();
			relation3.OU_OH = header2.PK;
			relation3.OU_OP = part3.PK;

			Factory.Save();
		}

		public void TestExceptionHandling()
		{
			var service = new EnterpriseNativeDataService();
			var result = service.Update(null);
			AssertEquals(
				@"<Response xmlns=""http://www.cargowise.com/Schemas/Native"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <EntityInfo>
    <PrimaryKey xsi:nil=""true"" />
  </EntityInfo>
  <Status>Rejected</Status>
  <Information>
    <Item>Error - Could not find any root element to process.</Item>
  </Information>
  <Data />
</Response>", result.ToString());
		}

		#endregion
	}

	class EnterpriseNativeDataServiceUntransactionedTest : TestCaseWithFactory
	{
		public void TestExceptionHandling_ArgumentException_IsReported()
		{
			using (DbEnvironmentWithMockGuidPluginForTest.SetTemporaryDbEnv())
			using (ErrorReporter.SetTemporaryInstanceForTest(new WebExceptionReporter()))
			{
				var exceptionToThrow = new ArgumentException();
				var service = new EnterpriseNativeDataService { ExceptionToThrow = exceptionToThrow };
				try
				{
					service.Service(XElement.Parse("<Native></Native>"), ServiceAction.Update);
					Fail("We expect an exception to be thrown.");
				}
				catch (Exception exception)
				{
					AssertEquals(exception, exceptionToThrow);
					AssertEquals(exceptionToThrow, ExceptionReporterTestListener.Instance[0].InnerException);
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
		}

		public void TestExceptionHandling_UpgradeException_IsNotReported()
		{
			using (DbEnvironmentWithMockGuidPluginForTest.SetTemporaryDbEnv())
			using (ErrorReporter.SetTemporaryInstanceForTest(new WebExceptionReporter()))
			{
				var exceptionToThrow = new DatabaseUpgradeInProgressException();
				var service = new EnterpriseNativeDataService { ExceptionToThrow = exceptionToThrow };
				try
				{
					service.Service(XElement.Parse("<Native></Native>"), ServiceAction.Update);
					Fail("We expect an exception to be thrown.");
				}
				catch (Exception exception)
				{
					AssertEquals(exception, exceptionToThrow);
				}
			}
		}
	}

	class DbEnvironmentWithMockGuidPluginForTest : BaseDbEnvironment
	{
		readonly IDbConnectionGuiPlugin connectionGuiPlugin = new Mock<IDbConnectionGuiPlugin>().Object;

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;

		public static IDisposable SetTemporaryDbEnv()
		{
			var existingEnv = DbEnv.Instance;
			DbEnv.SetDbEnvironment(new DbEnvironmentWithMockGuidPluginForTest());
			return new DisposableAction(() => DbEnv.SetDbEnvironment(existingEnv));
		}
	}
}
