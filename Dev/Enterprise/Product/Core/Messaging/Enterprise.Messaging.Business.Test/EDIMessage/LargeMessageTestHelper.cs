using System;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;

namespace Enterprise.Messaging.Testing
{
	public static class LargeMessageTestHelper
	{
		public static string CreateTestFile(FileType type, long sizeInBytes, bool addXmlDeclaration = true)
		{
			string patternString = "";
			string textPatternString = @"<<MSGNO PLACEHOLDER>> <<SENDERS REFERENCE PLACE HOLDER>><<PRIME ENTRY NUMBER PLACE HOLDER>><<ENTRY NUMBER PLACE HOLDER>><<MSGNO PLACEHOLDER>><<MESSAGE DATE TIME CREATE PLACE HOLDER>><<UNIQUE BATCH NUMBER PLACE HOLDER>><<CONSIGNMENT REFERENCE NUMBER PLACE HOLDER>><<AGENT REFERENCE PLACE HOLDER>><<OWNER REFERENCE PLACE HOLDER>><<OWNER REFERENCE PLACE HOLDER>><<CONTAINED CHECKSUM PLACE HOLDER>>.  maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug.";
			string nTextPatternString = @"Редакция издательства Merriam-Webster в конце декабря 2010г. выбрала Слово года-2010, которым признано существительное. Эксперты лексикографического проекта традиционно основывают свой выбор на количестве англоязычных запросов пользователей, обращающихся за толкованием того или иного слова к сайту Merriam-Webster.com.";
			string xmlPatternString = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer's Guide</title><genre>Computer</genre><price>44.95</price><publish_date>2000-10-01</publish_date><description>An in-depth look at creating applications with XML.</description></book>";

			string filePath = Temp.GetTempFileName();
			StreamWriter writer = new StreamWriter(filePath);

			if (type == FileType.Xml)
			{
				if (addXmlDeclaration)
				{
					writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				}

				writer.Write("<catalog>");
				patternString = xmlPatternString;
			}
			else if (type == FileType.NText)
			{
				patternString = nTextPatternString;
			}
			else
			{
				patternString = textPatternString;
			}

			for (int i = 0; i < sizeInBytes / patternString.Length + 1; i++)
			{
				writer.Write(patternString);
			}

			if (type == FileType.Xml)
			{
				writer.Write("</catalog>");
			}

			writer.Flush();
			writer.Close();
			return filePath;
		}

		public enum FileType
		{
			Binary,
			Text,
			NText,
			Xml
		}

		internal static long GetLengthStoredInDB(BusinessObject row, SchemaColumn column)
		{
			if (row.TableName != column.TableName)
			{
				throw new InvalidOperationException("Table name of row must match table name of column.");
			}

			var dataLengthInDB = Db.Connection.ExecuteScalar("select datalength(" + column.Name + ") from " + row.TableName + " where " + row.PKSchemaColumn.Name + " = '" + row.PK.ToString() + "'");
			if (dataLengthInDB == DBNull.Value)
			{
				return 0;
			}
			return (long)dataLengthInDB;
		}

		#region const string BigChunkOfXML

		internal static string BiggerChunkOfXML { get { return string.Format(BigChunkOfXML, "ARGH".PadRight(64000, '!')); } }

		internal const string BigChunkOfXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:uv=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>CCDB</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <CustomValues>
          <Boolean Name=""String"">true</Boolean>
        </CustomValues>
        <IsActive>true</IsActive>
        <Code>771201</Code>
        <FullName>Gebrüder Weiss GmbH</FullName>
        <IsConsignee>false</IsConsignee>
        <IsConsignor>false</IsConsignor>
        <IsTransportClient>true</IsTransportClient>
        <IsSalesLead>true</IsSalesLead>
        <IsForwarder>true</IsForwarder>
        <Language>EN</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>GWPLU_MAIN</Code>
            <Language>EN</Language>
            <Address1>Handelskai 92/Gate 2/1. OG/Top H</Address1>
            <Address2 />
            <City>Wien</City>
            <State />
            <PostCode>1200</PostCode>
            <Phone />
            <Fax />
            <Email />
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>ATVIE</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <IsDebtor>true</IsDebtor>
            <IsCreditor>true</IsCreditor>
            <APCreditorGroup TableName=""OrgCreditorGroup"">
              <Code>TPY</Code>
            </APCreditorGroup>
            <ARDebtorGroup TableName=""OrgDebtorGroup"">
              <Code>TPY</Code>
            </ARDebtorGroup>
            <APPaymentTerms>INV</APPaymentTerms>
            <APPaymentTermDays>14</APPaymentTermDays>
            <ARTaxApplicable>true</ARTaxApplicable>
            <ARCreditRating>2</ARCreditRating>
            <APTaxApplicable>true</APTaxApplicable>
            <ARVATConfig>DEF</ARVATConfig>
            <APVATConfig>DEF</APVATConfig>
            <OrgARTermsCollection>
              <OrgARTerms Action=""MERGE"">
                <InvoiceClass>ALL</InvoiceClass>
                <InvoiceTerm>INV</InvoiceTerm>
                <InvoiceDays>14</InvoiceDays>
              </OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>SHA</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <IsDebtor>true</IsDebtor>
            <IsCreditor>true</IsCreditor>
            <APCreditorGroup TableName=""OrgCreditorGroup"">
              <Code>TPY</Code>
            </APCreditorGroup>
            <ARDebtorGroup TableName=""OrgDebtorGroup"">
              <Code>TPY</Code>
            </ARDebtorGroup>
            <APPaymentTerms>INV</APPaymentTerms>
            <APPaymentTermDays>14</APPaymentTermDays>
            <ARTaxApplicable>true</ARTaxApplicable>
            <ARCreditRating>2</ARCreditRating>
            <APTaxApplicable>true</APTaxApplicable>
            <ARVATConfig>DEF</ARVATConfig>
            <APVATConfig>DEF</APVATConfig>
            <OrgARTermsCollection>
              <OrgARTerms Action=""MERGE"">
                <InvoiceClass>ALL</InvoiceClass>
                <InvoiceTerm>INV</InvoiceTerm>
                <InvoiceDays>14</InvoiceDays>
              </OrgARTerms>
            </OrgARTermsCollection>
            <GlbCompany>
              <Code>YTO</Code>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>EDR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>ECR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>LSC</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>EDR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>VN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>ECR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>VN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>LSC</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>VN</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>EDR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CA</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>ECR</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CA</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>771201</CustomsRegNo>
            <CodeType>LSC</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>CA</Code>
            </CodeCountry>
          </OrgCusCode>
          <OrgCusCode Action=""MERGE"">
            <CustomsRegNo>U35613608</CustomsRegNo>
            <CodeType>UID</CodeType>
            <CountryDefault>false</CountryDefault>
            <CodeCountry TableName=""RefCountry"">
              <Code>AT</Code>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <TariffLevel>1</TariffLevel>
            <GlbCompany>
              <Code>SHA</Code>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <TariffType>DEF</TariffType>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <TariffLevel>1</TariffLevel>
            <GlbCompany>
              <Code>YTO</Code>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>ATVIE</Code>
        </ClosestPort>
				<VeryBigBlobOfStuff>{0}</VeryBigBlobOfStuff>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion
	}
}
