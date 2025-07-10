CREATE XML SCHEMA COLLECTION [xmlJobRequiredDocumentAddInfo] AS '<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:element name="placeholder">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base="xs:string">
          <xs:attribute name="LazyLoading" type="xs:string" fixed="Yes">
          </xs:attribute>
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>
<xs:schema targetNamespace="http://www.cargowise.com/Schemas/DISDocument" version="1" elementFormDefault="qualified" xmlns="http://www.cargowise.com/Schemas/DISDocument" xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:element name="DISDocument" type="DISDocument0" />
  <xs:complexType name="DISAdditionalData1">
    <xs:all>
      <xs:element name="Data" minOccurs="0" type="string_maxLength35" />
      <xs:element name="Name" minOccurs="0" type="string_maxLength35" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISAdditionalNumber12">
    <xs:all>
      <xs:element name="Number" minOccurs="0" type="string_maxLength35" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISBondData2">
    <xs:all>
      <xs:element name="AgentIDNumber" minOccurs="0" type="string_maxLength50" />
      <xs:element name="BondAmount" minOccurs="0" type="decimal_15_2" />
      <xs:element name="BondName" minOccurs="0" type="string_maxLength3" />
      <xs:element name="BondNumber" minOccurs="0" type="string_maxLength50" />
      <xs:element name="BondType" minOccurs="0" type="string_maxLength3" />
      <xs:element name="DefaultBondCode" minOccurs="0" type="string_maxLength3" />
      <xs:element name="SuretyCode" minOccurs="0" type="string_maxLength3" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISCBPRequest3">
    <xs:all>
      <xs:element name="ID" minOccurs="0" type="string_maxLength15" />
      <xs:element name="RequestDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="Type" minOccurs="0" type="string_maxLength3" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISCertificateData4">
    <xs:all>
      <xs:element name="CertificateNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="CertificateType" minOccurs="0" type="string_maxLength3" />
      <xs:element name="IssueDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="ExpiryDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="InspectionLocation" minOccurs="0" type="string_maxLength10" />
      <xs:element name="Statement" minOccurs="0" type="string_maxLength500" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISCommodityLine5">
    <xs:all>
      <xs:element name="InvoiceLineNumber" minOccurs="0" type="xs:int" />
      <xs:element name="InvoiceLineTo" minOccurs="0" type="xs:int" />
      <xs:element name="InvoiceNumber" minOccurs="0" type="string_maxLength50" />
      <xs:element name="VNELineNumber" minOccurs="0" type="xs:int" />
      <xs:element name="VNELineNumberTo" minOccurs="0" type="xs:int" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISDocument0">
    <xs:all>
      <xs:element name="Comment" minOccurs="0" type="string_maxLength100" />
      <xs:element name="DocumentDescription" minOccurs="0" type="string_maxLength50" />
      <xs:element name="DocumentLabel" minOccurs="0" type="string_maxLength10" />
      <xs:element name="EDocsDocumentPK" minOccurs="0" type="guid" />
      <xs:element name="IDSuffix" minOccurs="0" type="xs:int" />
      <xs:element name="SubmitDateUTC" minOccurs="0" type="xs:dateTime" />
      <xs:element name="ShipmentNo" minOccurs="0" type="string_maxLength35" />
      <xs:element name="ITN" minOccurs="0" type="string_maxLength35" />
      <xs:element name="XTN" minOccurs="0" type="string_maxLength35" />
      <xs:element name="AdditionalData" minOccurs="0">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="DISAdditionalData" minOccurs="0" maxOccurs="unbounded" type="DISAdditionalData1" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element name="BondData" minOccurs="0" type="DISBondData2" />
      <xs:element name="CBPRequest" minOccurs="0" type="DISCBPRequest3" />
      <xs:element name="CertificateData" minOccurs="0" type="DISCertificateData4" />
      <xs:element name="CommodityData" minOccurs="0">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="DISCommodityLine" minOccurs="0" maxOccurs="unbounded" type="DISCommodityLine5" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element name="Invoice" minOccurs="0" type="DISInvoice6" />
      <xs:element name="PackingList" minOccurs="0" type="DISPackingListData8" />
      <xs:element name="PermitData" minOccurs="0" type="DISPermitData9" />
      <xs:element name="PGAs" minOccurs="0">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="DISPGA" minOccurs="0" maxOccurs="unbounded" type="DISPGA10" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element name="ToxicSubstanceData" minOccurs="0" type="DISToxicSubstanceData11" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISInvoice6">
    <xs:all>
      <xs:element name="InvoiceNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="InvoiceLineRanges" minOccurs="0">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="DISInvoiceLineRange" minOccurs="0" maxOccurs="unbounded" type="DISInvoiceLineRange7" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISInvoiceLineRange7">
    <xs:all>
      <xs:element name="InvoiceLineFrom" minOccurs="0" type="xs:int" />
      <xs:element name="InvoiceLineTo" minOccurs="0" type="xs:int" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISPackingListData8">
    <xs:all>
      <xs:element name="InvoiceNumber" minOccurs="0" type="string_maxLength50" />
      <xs:element name="PackingListNumber" minOccurs="0" type="string_maxLength50" />
      <xs:element name="PurchaseOrderNumber" minOccurs="0" type="string_maxLength50" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISPermitData9">
    <xs:all>
      <xs:element name="ApprovalNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="EndDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="PermitNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="PermitType" minOccurs="0" type="string_maxLength3" />
      <xs:element name="StartDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="Statement" minOccurs="0" type="string_maxLength500" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISPGA10">
    <xs:all>
      <xs:element name="Code" minOccurs="0" type="string_maxLength10" />
    </xs:all>
  </xs:complexType>
  <xs:complexType name="DISToxicSubstanceData11">
    <xs:all>
      <xs:element name="EPAProducerEstNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="EPARegistrationNumber" minOccurs="0" type="string_maxLength35" />
      <xs:element name="CASNumbers" minOccurs="0">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="DISAdditionalNumber" minOccurs="0" maxOccurs="unbounded" type="DISAdditionalNumber12" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
  <xs:simpleType name="decimal_15_2">
    <xs:restriction base="xs:decimal">
      <xs:totalDigits value ="15" />
      <xs:fractionDigits value="2" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="guid">
    <xs:restriction base="xs:string">
      <xs:pattern value="[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength10">
    <xs:restriction base="xs:string">
      <xs:maxLength value="10" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength100">
    <xs:restriction base="xs:string">
      <xs:maxLength value="100" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength15">
    <xs:restriction base="xs:string">
      <xs:maxLength value="15" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength3">
    <xs:restriction base="xs:string">
      <xs:maxLength value="3" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength35">
    <xs:restriction base="xs:string">
      <xs:maxLength value="35" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength50">
    <xs:restriction base="xs:string">
      <xs:maxLength value="50" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength500">
    <xs:restriction base="xs:string">
      <xs:maxLength value="500" />
    </xs:restriction>
  </xs:simpleType>
</xs:schema>
<xs:schema targetNamespace="http://www.cargowise.com/Schemas/DIFDocument" version="1" elementFormDefault="qualified" xmlns="http://www.cargowise.com/Schemas/DIFDocument" xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:element name="DIFDocument" type="DIFDocument0" />
  <xs:complexType name="DIFDocument0">
    <xs:all>
      <xs:element name="BusinessNumber" minOccurs="0" type="string_maxLength15" />
      <xs:element name="Comment" minOccurs="0" type="string_maxLength100" />
      <xs:element name="DocumentDescription" minOccurs="0" type="string_maxLength50" />
      <xs:element name="DocumentNumber" minOccurs="0" type="string_maxLength70" />
      <xs:element name="DocumentType" minOccurs="0" type="string_maxLength5" />
      <xs:element name="EDocsDocumentPK" minOccurs="0" type="guid" />
      <xs:element name="EffectiveDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="ExpiryDate" minOccurs="0" type="xs:dateTime" />
      <xs:element name="PGA" minOccurs="0" type="string_maxLength5" />
    </xs:all>
  </xs:complexType>
  <xs:simpleType name="guid">
    <xs:restriction base="xs:string">
      <xs:pattern value="[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength5">
    <xs:restriction base="xs:string">
      <xs:maxLength value="5" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength15">
    <xs:restriction base="xs:string">
      <xs:maxLength value="15" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength50">
    <xs:restriction base="xs:string">
      <xs:maxLength value="50" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength70">
    <xs:restriction base="xs:string">
      <xs:maxLength value="70" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name="string_maxLength100">
    <xs:restriction base="xs:string">
      <xs:maxLength value="100" />
    </xs:restriction>
  </xs:simpleType>
</xs:schema>'
;

CREATE XML SCHEMA COLLECTION [xmlOrgSalesProductFormLayoutData] AS '<xs:schema targetNamespace="http://cargowise.com/SalesProductFormLayoutData.xsd" xmlns="http://cargowise.com/SalesProductFormLayoutData.xsd" xmlns:xs="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified">
       <xs:simpleType name="guid">
              <xs:restriction base="xs:string">
                     <xs:pattern value="[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}" />
              </xs:restriction>
       </xs:simpleType>
       <xs:complexType name="SalesProductGridColumnDefinitionData">
              <xs:sequence>
                     <xs:element name="GenCustomColumnDefinitionFk" type="guid" />
                     <xs:element name="Order" type="xs:int" />
                     <xs:element name="IsVisible" type="xs:boolean" />
              </xs:sequence>
       </xs:complexType>
       <xs:complexType name="SalesProductFieldLayoutDefinitionData">
              <xs:sequence>
                     <xs:element name="GenCustomColumnDefinitionFk" type="guid" />
                     <xs:element name="Order" type="xs:int" />
              </xs:sequence>
       </xs:complexType>
       <xs:element name="SalesProductFormLayoutData">
              <xs:complexType>
                     <xs:sequence>
                           <xs:element name="TradeLaneCustomColumnDefinitionFks" type="guid" minOccurs="0" maxOccurs="unbounded" />
                           <xs:element name="TradeLaneGridColumnDefinitionData" type="SalesProductGridColumnDefinitionData" minOccurs="0" maxOccurs="unbounded" />
                           <xs:element name="TradeLaneFieldLayoutDefinitionData" type="SalesProductFieldLayoutDefinitionData" minOccurs="0" maxOccurs="unbounded" />
                           <xs:element name="TradeDetailCustomColumnDefinitionFks" type="guid" minOccurs="0" maxOccurs="unbounded" />
                           <xs:element name="TradeDetailGridColumnDefinitionData" type="SalesProductGridColumnDefinitionData" minOccurs="0" maxOccurs="unbounded" />
                     </xs:sequence>
              </xs:complexType>
       </xs:element>
</xs:schema>'
;

CREATE TABLE [AccAccountFee] (
   [AAF_PK] UNIQUEIDENTIFIER NOT NULL,
   [AAF_AG_GLAccount] UNIQUEIDENTIFIER NOT NULL,
   [AAF_RX_NKFeeCurrency] CHAR(3) NOT NULL DEFAULT '',
   [AAF_FeeAmount] MONEY NOT NULL DEFAULT 0,
   [AAF_Rule] CHAR(3) NOT NULL DEFAULT '',
   [AAF_OB_CompanyData] UNIQUEIDENTIFIER NULL,
   [AAF_OJ_DebtorGroup] UNIQUEIDENTIFIER NULL,
   [AAF_GC_Company] UNIQUEIDENTIFIER NOT NULL,
   [AAF_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [AAF_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [AAF_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [AAF_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);

CREATE TABLE [AccBankAccount] (
   [AB_PK] UNIQUEIDENTIFIER NOT NULL,
   [AB_AutoVersion] SMALLINT NOT NULL DEFAULT 0,
   [AB_IsActive] BIT NOT NULL DEFAULT 1,
   [AB_AccountType] VARCHAR(3) NOT NULL DEFAULT '',
   [AB_Code] VARCHAR(10) NOT NULL DEFAULT '',
   [AB_Desc] NVARCHAR(50) NOT NULL DEFAULT '',
   [AB_BankAccountName] NVARCHAR(50) NOT NULL DEFAULT '',
   [AB_BankName] NVARCHAR(50) NOT NULL DEFAULT '',
   [AB_BankAddress] NVARCHAR(192) NOT NULL DEFAULT '',
   [AB_BankAbbreviation] CHAR(3) NOT NULL DEFAULT '',
   [AB_IsDefaultReceiptBankAccount] BIT NOT NULL DEFAULT 0,
   [AB_RN_NKBankAccountCountry] VARCHAR(2) NOT NULL DEFAULT '',
   [AB_BSB] VARCHAR(20) NOT NULL DEFAULT '',
   [AB_AccountNum] VARCHAR(35) NOT NULL DEFAULT '',
   [AB_AccountNumber] VARCHAR(50) NOT NULL DEFAULT '',
   [AB_FullAccountNumber] VARCHAR(100) NOT NULL DEFAULT '',
   [AB_SWIFT] VARCHAR(35) NOT NULL DEFAULT '',
   [AB_DebitCreditCardName] NVARCHAR(35) NOT NULL DEFAULT '',
   [AB_DebitCreditCardNumber] VARCHAR(64) NOT NULL DEFAULT '',
   [AB_DebitCreditCardExpiry] VARCHAR(4) NOT NULL DEFAULT '',
   [AB_DetailedDepositSlip] BIT NOT NULL DEFAULT 0,
   [AB_AutoDDRFormat] CHAR(3) NOT NULL DEFAULT '',
   [AB_OpenPeriod] INT NOT NULL DEFAULT 0,
   [AB_OpenBalance] MONEY NOT NULL DEFAULT 0,
   [AB_ClosingBalance] MONEY NOT NULL DEFAULT 0,
   [AB_OpenOSBalance] MONEY NOT NULL DEFAULT 0,
   [AB_ClosingOSBalance] MONEY NOT NULL DEFAULT 0,
   [AB_StatementBalance] MONEY NOT NULL DEFAULT 0,
   [AB_AccountEFTUserID] VARCHAR(20) NOT NULL DEFAULT '',
   [AB_AllowAutoDDR] BIT NOT NULL DEFAULT 0,
   [AB_ShowDetailsOnDirectDebits] BIT NOT NULL DEFAULT 1,
   [AB_LastReconcileDate] SMALLDATETIME NULL,
   [AB_LastStatementDate] SMALLDATETIME NULL,
   [AB_ChequeNumDigits] TINYINT NOT NULL DEFAULT 6,
   [AB_GC] UNIQUEIDENTIFIER NOT NULL,
   [AB_GB] UNIQUEIDENTIFIER NULL,
   [AB_RX_NKAccountCurrency] VARCHAR(3) NOT NULL DEFAULT '',
   [AB_AG] UNIQUEIDENTIFIER NOT NULL,
   [AB_SO_ChequeTemplate] UNIQUEIDENTIFIER NULL,
   [AB_PaymentProvider] VARCHAR(3) NOT NULL DEFAULT '',
   [AB_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [AB_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [AB_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [AB_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);

CREATE TABLE [AccAPAccountDetails] (
   [A1_PK] UNIQUEIDENTIFIER NOT NULL,
   [A1_RX_NKAccountCurrency] VARCHAR(3) NOT NULL DEFAULT '',
   [A1_AccountName] NVARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankName] NVARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankBranchName] NVARCHAR(30) NOT NULL DEFAULT '',
   [A1_BankAddress1] NVARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankAddress2] NVARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankAddress3] NVARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankAccount] VARCHAR(35) NOT NULL DEFAULT '',
   [A1_BankBsb] VARCHAR(15) NOT NULL DEFAULT '',
   [A1_BankSwift] VARCHAR(35) NOT NULL DEFAULT '',
   [A1_PaymentMethod] VARCHAR(3) NOT NULL DEFAULT 'DEF',
   [A1_IBANNumber] VARCHAR(35) NOT NULL DEFAULT '',
   [A1_EPaymentBeneficiaryId] UNIQUEIDENTIFIER NULL,
   [A1_IsDefaultAccount] BIT NOT NULL DEFAULT 0,
   [A1_RN_NKCountryCode] VARCHAR(2) NOT NULL DEFAULT '',
   [A1_OB] UNIQUEIDENTIFIER NOT NULL,
   [A1_EPaymentReasonCode] VARCHAR(3) NOT NULL DEFAULT '',
   [A1_EPaymentReference] VARCHAR(30) NOT NULL DEFAULT '',
   [A1_EPaymentReferenceType] VARCHAR(3) NOT NULL DEFAULT '',
   [A1_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [A1_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [A1_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [A1_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);

CREATE TABLE [AccAlternateChart] (
   [AAC_PK] UNIQUEIDENTIFIER NOT NULL,
   [AAC_Code] VARCHAR(10) NOT NULL DEFAULT '',
   [AAC_Description] NVARCHAR(255) NOT NULL DEFAULT '',
   [AAC_IsGlobal] BIT NOT NULL DEFAULT 0,
   [AAC_IsFixedLength] BIT NOT NULL DEFAULT 0,
   [AAC_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
   [AAC_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [AAC_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
   [AAC_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);