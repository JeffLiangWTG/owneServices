using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class GenerateInvoiceRequestPayloadValidationTest : TestCaseWithFactory
	{
		public void TestValidateWithoutError()
		{
			var xml = @"<TaxInvoiceSet>
<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
    <ExchangedDocument>
        <ID>AAAAAAAA</ID>
        <IssueDateTime>20090402104522</IssueDateTime>
        <ReferencedDocument>
            <ID>123456789012345678901234</ID>
        </ReferencedDocument>
    </ExchangedDocument>
    <TaxInvoiceDocument>
        <IssueID>2009091112345678a1b2c3d4</IssueID>
        <TypeCode>0101</TypeCode>
        <DescriptionText>이 문서는 전자(세금)계산서 Sample 문서입니다.</DescriptionText>
        <IssueDateTime>20090402</IssueDateTime>
        <AmendmentStatusCode>01</AmendmentStatusCode>
        <PurposeCode>01</PurposeCode>
    </TaxInvoiceDocument>
    <TaxInvoiceTradeSettlement>
        <InvoicerParty>
            <ID>1234567890</ID>
            <TypeCode>비영리</TypeCode>
            <NameText>한국인터넷진흥원</NameText>
            <ClassificationCode>공공기관</ClassificationCode>
            <SpecifiedOrganization>
                <TaxRegistrationID>1234</TaxRegistrationID>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>백기승</NameText>
            </SpecifiedPerson>
            <DefinedContact>
                <DepartmentNameText>전자문서유통팀</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0611234567</TelephoneCommunication>
                <URICommunication>gildong@kisa.or.kr</URICommunication>
            </DefinedContact>
            <SpecifiedAddress>
                <LineOneText>전라남도 나주시 진흥길 9</LineOneText>
            </SpecifiedAddress>
        </InvoicerParty>
        <InvoiceeParty>
            <ID>1234567890123</ID>
            <TypeCode>개인</TypeCode>
            <NameText>홍길동</NameText>
            <ClassificationCode>개인</ClassificationCode>
            <SpecifiedOrganization>
                <BusinessTypeCode>02</BusinessTypeCode>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>홍길동</NameText>
            </SpecifiedPerson>
            <PrimaryDefinedContact>
                <DepartmentNameText>홍길동</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0212345678</TelephoneCommunication>
                <URICommunication>hkd@aaa.com</URICommunication>
            </PrimaryDefinedContact>
            <SpecifiedAddress>
                <LineOneText>서울시 중구 종로 1가 1번지</LineOneText>
            </SpecifiedAddress>
        </InvoiceeParty>
        <SpecifiedPaymentMeans>
            <TypeCode>10</TypeCode>
            <PaidAmount>123456789012345</PaidAmount>
        </SpecifiedPaymentMeans>
        <SpecifiedMonetarySummation>
            <ChargeTotalAmount>1000000</ChargeTotalAmount>
            <TaxTotalAmount>100000</TaxTotalAmount>
            <GrandTotalAmount>1100000</GrandTotalAmount>
        </SpecifiedMonetarySummation>
    </TaxInvoiceTradeSettlement>
    <TaxInvoiceTradeLineItem>
        <SequenceNumeric>01</SequenceNumeric>
        <DescriptionText>물품 자유기술문</DescriptionText>
        <InvoiceAmount>1000000</InvoiceAmount>
        <ChargeableUnitQuantity>1</ChargeableUnitQuantity>
        <InformationText>EA</InformationText>
        <NameText>전자(세금)계산서</NameText>
        <PurchaseExpiryDateTime>20090911</PurchaseExpiryDateTime>
        <TotalTax>
            <CalculatedAmount>100000</CalculatedAmount>
        </TotalTax>
        <UnitPrice>
            <UnitAmount>1000000</UnitAmount>
        </UnitPrice>
    </TaxInvoiceTradeLineItem>
</TaxInvoice>
</TaxInvoiceSet>
";
			var validator = new GenerateInvoiceRequestPayloadValidation();
			var errorNotification = new Logger();
			var warnningNotification = new Logger();
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				validator.ValidateXml(ms, errorNotification, warnningNotification);

				ms.Position = 0;
				AssertEquals(xml, Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals(false, errorNotification.HasErrors);
			AssertEquals(false, warnningNotification.HasErrors);
		}

		public void TestValidateBatchTaxInvoicesWithoutError()
		{
			var xml = @"<TaxInvoiceSet>
<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
    <ExchangedDocument>
        <ID>AAAAAAAA</ID>
        <IssueDateTime>20090402104522</IssueDateTime>
        <ReferencedDocument>
            <ID>123456789012345678901234</ID>
        </ReferencedDocument>
    </ExchangedDocument>
    <TaxInvoiceDocument>
        <IssueID>2009091112345678a1b2c3d4</IssueID>
        <TypeCode>0101</TypeCode>
        <DescriptionText>이 문서는 전자(세금)계산서 Sample 문서입니다.</DescriptionText>
        <IssueDateTime>20090402</IssueDateTime>
        <AmendmentStatusCode>01</AmendmentStatusCode>
        <PurposeCode>01</PurposeCode>
    </TaxInvoiceDocument>
    <TaxInvoiceTradeSettlement>
        <InvoicerParty>
            <ID>1234567890</ID>
            <TypeCode>비영리</TypeCode>
            <NameText>한국인터넷진흥원</NameText>
            <ClassificationCode>공공기관</ClassificationCode>
            <SpecifiedOrganization>
                <TaxRegistrationID>1234</TaxRegistrationID>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>백기승</NameText>
            </SpecifiedPerson>
            <DefinedContact>
                <DepartmentNameText>전자문서유통팀</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0611234567</TelephoneCommunication>
                <URICommunication>gildong@kisa.or.kr</URICommunication>
            </DefinedContact>
            <SpecifiedAddress>
                <LineOneText>전라남도 나주시 진흥길 9</LineOneText>
            </SpecifiedAddress>
        </InvoicerParty>
        <InvoiceeParty>
            <ID>1234567890123</ID>
            <TypeCode>개인</TypeCode>
            <NameText>홍길동</NameText>
            <ClassificationCode>개인</ClassificationCode>
            <SpecifiedOrganization>
                <BusinessTypeCode>02</BusinessTypeCode>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>홍길동</NameText>
            </SpecifiedPerson>
            <PrimaryDefinedContact>
                <DepartmentNameText>홍길동</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0212345678</TelephoneCommunication>
                <URICommunication>hkd@aaa.com</URICommunication>
            </PrimaryDefinedContact>
            <SpecifiedAddress>
                <LineOneText>서울시 중구 종로 1가 1번지</LineOneText>
            </SpecifiedAddress>
        </InvoiceeParty>
        <SpecifiedPaymentMeans>
            <TypeCode>10</TypeCode>
            <PaidAmount>123456789012345</PaidAmount>
        </SpecifiedPaymentMeans>
        <SpecifiedMonetarySummation>
            <ChargeTotalAmount>1000000</ChargeTotalAmount>
            <TaxTotalAmount>100000</TaxTotalAmount>
            <GrandTotalAmount>1100000</GrandTotalAmount>
        </SpecifiedMonetarySummation>
    </TaxInvoiceTradeSettlement>
    <TaxInvoiceTradeLineItem>
        <SequenceNumeric>01</SequenceNumeric>
        <DescriptionText>물품 자유기술문</DescriptionText>
        <InvoiceAmount>1000000</InvoiceAmount>
        <ChargeableUnitQuantity>1</ChargeableUnitQuantity>
        <InformationText>EA</InformationText>
        <NameText>전자(세금)계산서</NameText>
        <PurchaseExpiryDateTime>20090911</PurchaseExpiryDateTime>
        <TotalTax>
            <CalculatedAmount>100000</CalculatedAmount>
        </TotalTax>
        <UnitPrice>
            <UnitAmount>1000000</UnitAmount>
        </UnitPrice>
    </TaxInvoiceTradeLineItem>
</TaxInvoice>
<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
    <ExchangedDocument>
        <ID>BBBBBBBB</ID>
        <IssueDateTime>20090402104522</IssueDateTime>
        <ReferencedDocument>
            <ID>923456789012345678901234</ID>
        </ReferencedDocument>
    </ExchangedDocument>
    <TaxInvoiceDocument>
        <IssueID>2010091112345678a1b2c3d4</IssueID>
        <TypeCode>0101</TypeCode>
        <DescriptionText>이 문서는 전자(세금)계산서 Sample 문서입니다.</DescriptionText>
        <IssueDateTime>20090402</IssueDateTime>
        <AmendmentStatusCode>01</AmendmentStatusCode>
        <PurposeCode>01</PurposeCode>
    </TaxInvoiceDocument>
    <TaxInvoiceTradeSettlement>
        <InvoicerParty>
            <ID>1234567890</ID>
            <TypeCode>비영리</TypeCode>
            <NameText>한국인터넷진흥원</NameText>
            <ClassificationCode>공공기관</ClassificationCode>
            <SpecifiedOrganization>
                <TaxRegistrationID>1234</TaxRegistrationID>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>백기승</NameText>
            </SpecifiedPerson>
            <DefinedContact>
                <DepartmentNameText>전자문서유통팀</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0611234567</TelephoneCommunication>
                <URICommunication>gildong@kisa.or.kr</URICommunication>
            </DefinedContact>
            <SpecifiedAddress>
                <LineOneText>전라남도 나주시 진흥길 9</LineOneText>
            </SpecifiedAddress>
        </InvoicerParty>
        <InvoiceeParty>
            <ID>1234567890123</ID>
            <TypeCode>개인</TypeCode>
            <NameText>홍길동</NameText>
            <ClassificationCode>개인</ClassificationCode>
            <SpecifiedOrganization>
                <BusinessTypeCode>02</BusinessTypeCode>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>홍길동</NameText>
            </SpecifiedPerson>
            <PrimaryDefinedContact>
                <DepartmentNameText>홍길동</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0212345678</TelephoneCommunication>
                <URICommunication>hkd@aaa.com</URICommunication>
            </PrimaryDefinedContact>
            <SpecifiedAddress>
                <LineOneText>서울시 중구 종로 1가 1번지</LineOneText>
            </SpecifiedAddress>
        </InvoiceeParty>
        <SpecifiedPaymentMeans>
            <TypeCode>10</TypeCode>
            <PaidAmount>123456789012345</PaidAmount>
        </SpecifiedPaymentMeans>
        <SpecifiedMonetarySummation>
            <ChargeTotalAmount>1000000</ChargeTotalAmount>
            <TaxTotalAmount>100000</TaxTotalAmount>
            <GrandTotalAmount>1100000</GrandTotalAmount>
        </SpecifiedMonetarySummation>
    </TaxInvoiceTradeSettlement>
    <TaxInvoiceTradeLineItem>
        <SequenceNumeric>01</SequenceNumeric>
        <DescriptionText>물품 자유기술문</DescriptionText>
        <InvoiceAmount>1000000</InvoiceAmount>
        <ChargeableUnitQuantity>1</ChargeableUnitQuantity>
        <InformationText>EA</InformationText>
        <NameText>전자(세금)계산서</NameText>
        <PurchaseExpiryDateTime>20090911</PurchaseExpiryDateTime>
        <TotalTax>
            <CalculatedAmount>100000</CalculatedAmount>
        </TotalTax>
        <UnitPrice>
            <UnitAmount>1000000</UnitAmount>
        </UnitPrice>
    </TaxInvoiceTradeLineItem>
</TaxInvoice>
</TaxInvoiceSet>
";
			var validator = new GenerateInvoiceRequestPayloadValidation();
			var errorNotification = new Logger();
			var warnningNotification = new Logger();
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				validator.ValidateXml(ms, errorNotification, warnningNotification);

				ms.Position = 0;
				AssertEquals(xml, Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals(false, errorNotification.HasErrors);
			AssertEquals(false, warnningNotification.HasErrors);
		}

		public void TestValidateWithError_XsdValidation()
		{
			var xml = @"<TaxInvoiceSet>
<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
    <TaxInvoiceTradeSettlement>
        <InvoicerParty>
            <ID>1234567890</ID>
            <TypeCode>비영리</TypeCode>
            <NameText>한국인터넷진흥원</NameText>
            <ClassificationCode>공공기관</ClassificationCode>
            <SpecifiedOrganization>
                <TaxRegistrationID>1234</TaxRegistrationID>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>백기승</NameText>
            </SpecifiedPerson>
            <DefinedContact>
                <DepartmentNameText>전자문서유통팀</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0611234567</TelephoneCommunication>
                <URICommunication>gildong@kisa.or.kr</URICommunication>
            </DefinedContact>
            <SpecifiedAddress>
                <LineOneText>전라남도 나주시 진흥길 9</LineOneText>
            </SpecifiedAddress>
        </InvoicerParty>
        <InvoiceeParty>
            <ID>1234567890123</ID>
            <TypeCode>개인</TypeCode>
            <NameText>홍길동</NameText>
            <ClassificationCode>개인</ClassificationCode>
            <SpecifiedOrganization>
                <BusinessTypeCode>02</BusinessTypeCode>
            </SpecifiedOrganization>
            <SpecifiedPerson>
                <NameText>홍길동</NameText>
            </SpecifiedPerson>
            <PrimaryDefinedContact>
                <DepartmentNameText>홍길동</DepartmentNameText>
                <PersonNameText>홍길동</PersonNameText>
                <TelephoneCommunication>0212345678</TelephoneCommunication>
                <URICommunication>hkd@aaa.com</URICommunication>
            </PrimaryDefinedContact>
            <SpecifiedAddress>
                <LineOneText>서울시 중구 종로 1가 1번지</LineOneText>
            </SpecifiedAddress>
        </InvoiceeParty>
        <SpecifiedPaymentMeans>
            <TypeCode>10</TypeCode>
            <PaidAmount>123456789012345</PaidAmount>
        </SpecifiedPaymentMeans>
        <SpecifiedMonetarySummation>
            <ChargeTotalAmount>1000000</ChargeTotalAmount>
            <TaxTotalAmount>100000</TaxTotalAmount>
            <GrandTotalAmount>1100000</GrandTotalAmount>
        </SpecifiedMonetarySummation>
    </TaxInvoiceTradeSettlement>
    <TaxInvoiceTradeLineItem>
        <SequenceNumeric>01</SequenceNumeric>
        <DescriptionText>물품 자유기술문</DescriptionText>
        <InvoiceAmount>1000000</InvoiceAmount>
        <ChargeableUnitQuantity>1</ChargeableUnitQuantity>
        <InformationText>EA</InformationText>
        <NameText>전자(세금)계산서</NameText>
        <PurchaseExpiryDateTime>20090911</PurchaseExpiryDateTime>
        <TotalTax>
            <CalculatedAmount>100000</CalculatedAmount>
        </TotalTax>
        <UnitPrice>
            <UnitAmount>1000000</UnitAmount>
        </UnitPrice>
    </TaxInvoiceTradeLineItem>
</TaxInvoice>
</TaxInvoiceSet>
";
			var validator = new GenerateInvoiceRequestPayloadValidation();
			var errorNotification = new Logger();
			var warnningNotification = new Logger();
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				validator.ValidateXml(ms, errorNotification, warnningNotification);

				ms.Position = 0;
				AssertEquals(xml, Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals("The element 'TaxInvoice' in namespace 'urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0' has invalid child element 'TaxInvoiceTradeSettlement' in namespace 'urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0'. List of possible elements expected: 'ExchangedDocument' in namespace 'urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0'.", errorNotification.ToString());
			AssertEquals(false, warnningNotification.HasErrors);
		}
	}
}
