namespace Enterprise.Messaging.Testing
{
	sealed class LargeEBondMessageInsertScript
	{
		public const string EDIMessagePK = "5b2b8037-3755-4f9f-8e58-c0acbf786694";

		public static string Script
		{
			get
			{
				return $@"
INSERT [dbo].[EDIInterchange] ([EI_PK], [EI_IsActive], [EI_ApplicationCode], [EI_InterchangeType], [EI_ReceiveTransmit], [EI_From], [EI_To], [EI_Priority], [EI_Status], [EI_InterchangeNum], [EI_SessionGUID], [EI_ServerID], [EI_RetryCount], [EI_HeaderText], [EI_HeaderNText], [EI_BodyText], [EI_BodyNText], [EI_FooterText], [EI_FooterNText], [EI_SystemCreateTimeUtc], [EI_SystemCreateUser], [EI_SystemLastEditTimeUtc], [EI_SystemLastEditUser], [EI_GB], [EI_BodyData], [EI_TransportType], [EI_DeliveredTime], [EI_GP]) VALUES (N'6b0d0c25-1448-48c9-b3a1-654b861c6e96', 1, N'UXB', N'XDC', N'TRX', N'WTLEDIMXU', N'USCustomsEBond', N'   ', N'HQU', N'1', N'57824cc4-bce3-4b0d-bda3-28aecd693bec', 0, 0, N'', N'', N'<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>HYEDUSCMT</SenderID>
    <RecipientID>USCustomsEBond</RecipientID>
    <DeliveryMetadata>
      <ValueCollection>
        <Value>
          <Name>InsuranceAgent</Name>
          <Type>String</Type>
          <Data>WZP</Data>
        </Value>
        <Value>
          <Name>UserName</Name>
          <Type>String</Type>
          <Data>8SFgAVPbhM5xW4XVGMfLaIdYWMdtjM2W+tTLKhMYUpM=</Data>
        </Value>
        <Value>
          <Name>Password</Name>
          <Type>Base64Binary</Type>
          <Data>XTDMpQicewcgudFICLvFCd7jYx5V7PdIWF6RzR7u/ra2DpTawjxZHP2fnikcXXb0OJdR2AiOCmGWmaP2mcwAthSeAoQtgn4FCEzSXixknIfrgWCaHUtwEPu0TdOW8a6JbwEuRv7RtTJuqi8+mRyHZXwXnt2lfHfDnLVIvNRqmMw=</Data>
        </Value>
      </ValueCollection>
    </DeliveryMetadata>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>	
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>B00223702</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <AgentsReference></AgentsReference>
    <Branch>
      <Code>PHL</Code>
      <Name>PHL</Name>
    </Branch>
    <CommercialInfo>
      <Name>All Invoices</Name>

      <CommercialInvoiceCollection Content=""Complete"">
        <CommercialInvoice>
          <InvoiceNumber>GKFJK596958</InvoiceNumber>
          <AdditionalTerms></AdditionalTerms>
          <AgreedExchangeRate>1.000000000</AgreedExchangeRate>
          <ExchangeRateType>
            <Code></Code>
          </ExchangeRateType>
          <IncoTerm>
            <Code>FOB</Code>
            <Description>Free On Board</Description>
          </IncoTerm>
          <InvoiceAmount>300.0000</InvoiceAmount>
          <InvoiceCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </InvoiceCurrency>
          <InvoiceDate></InvoiceDate>
          <LandedCostExchangeRate>1.000000000</LandedCostExchangeRate>
          <MessageStatus>
            <Code></Code>
            <Description>Not Sent</Description>
          </MessageStatus>
          <NetWeight>0.000</NetWeight>
          <NetWeightUQ>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </NetWeightUQ>
          <NoOfPacks>0.000</NoOfPacks>
          <PaymentAmount>0.0000</PaymentAmount>
          <PaymentDate></PaymentDate>
          <PaymentExchangeRate>1.000000000</PaymentExchangeRate>
          <PaymentNumber></PaymentNumber>
          <RelatedIndicator>
            <Code></Code>
          </RelatedIndicator>
          <Supplier>
            <AddressType>Supplier</AddressType>
            <Address1>174 GLOUCESTER ROAD</Address1>
            <Address2>WAN CHAI DISTRICT</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
            <City>HONG KONG</City>
            <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>123456789</GovRegNum>
            <GovRegNumType>
              <Code>DUN</Code>
              <Description>DUNS Data Universal Numbering Syste</Description>
            </GovRegNumType>
            <OrganizationCode>ACETESHKG</OrganizationCode>
            <Phone>+22555567651</Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>GMC</Code>
                  <Description>Glazing Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>1</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>PFR</Code>
                  <Description>Food Facility Registration Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>19148237698</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>HKACETES174HON</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>TMC</Code>
                  <Description>Tire Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>FPI</Code>
                  <Description>Foreign Producer Identifier</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>WCHATEA3310021</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>AMS</Code>
                  <Description>AMS (USDA) Assigned ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>7880212180</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>FSR</Code>
                  <Description>FDA Foreign Seller Registration Num</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123456789</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </Supplier>
          <ValuationCode>
            <Code></Code>
          </ValuationCode>
          <ValuationDateOverride></ValuationDateOverride>
          <Volume>0.000</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0.000</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>

          <AddInfoCollection>
            <AddInfo>
              <Key>DateOfExport</Key>
              <Value>2022-04-03 00:00:00.000</Value>
            </AddInfo>
            <AddInfo>
              <Key>DestinationState</Key>
              <Value>MI</Value>
            </AddInfo>
            <AddInfo>
              <Key>ExportCode</Key>
              <Value>OS</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactEmail</Key>
              <Value>phil.pearse@wisetechglobal.com</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactName</Key>
              <Value>Phil Pearse</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactPhoneNo</Key>
              <Value>8473645600</Value>
            </AddInfo>
            <AddInfo>
              <Key>GenAIIForSup</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>InvoiceType</Key>
              <Value>IN</Value>
            </AddInfo>
            <AddInfo>
              <Key>IsLineGrouping</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>LicenseType</Key>
              <Value>C33</Value>
            </AddInfo>
            <AddInfo>
              <Key>UC_NKCountryOfExport</Key>
              <Value>HK</Value>
            </AddInfo>
          </AddInfoCollection>

          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <BondedWarehouseQuantity>0.00000</BondedWarehouseQuantity>
              <BondedWarehouseQuantityUnit>
                <Code></Code>
              </BondedWarehouseQuantityUnit>
              <BrandName></BrandName>
              <ClassificationCode></ClassificationCode>
              <ClassUsageComment></ClassUsageComment>
              <Commodity>
                <Code></Code>
              </Commodity>
              <ConcessionOrder></ConcessionOrder>
              <ContainerMode>
                <Code></Code>
              </ContainerMode>
              <CountryOfExport>
                <Code></Code>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code></Code>
              </CountryOfOrigin>
              <CustomsQuantity>3.00000</CustomsQuantity>
              <CustomsQuantityUnit>
                <Code>KG</Code>
                <Description>1,000 Grams</Description>
              </CustomsQuantityUnit>
              <CustomsSecondQuantity>0.00000</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code></Code>
              </CustomsSecondQuantityUnit>
              <CustomsThirdQuantity>0.00000</CustomsThirdQuantity>
              <CustomsThirdQuantityUnit>
                <Code></Code>
              </CustomsThirdQuantityUnit>
              <CustomsValue>200.00</CustomsValue>
              <DataImportMatchingKey></DataImportMatchingKey>
              <Description></Description>
              <EntryLineNumber>1</EntryLineNumber>
              <EntryNumber>73054965</EntryNumber>
              <HarmonisedCode>9701100000</HarmonisedCode>
              <HazardousMaterial>
                <Code></Code>
                <CodeType>
                  <Code></Code>
                </CodeType>
              </HazardousMaterial>
              <InvoiceQuantity>0.00000</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code></Code>
              </InvoiceQuantityUnit>
              <LinePrice>200.0000</LinePrice>
              <Link>1</Link>
              <LocalDescription></LocalDescription>
              <Model></Model>
              <NetWeight>0.000</NetWeight>
              <NetWeightUnit>
                <Code></Code>
              </NetWeightUnit>
              <OrderLineLink>0</OrderLineLink>
              <OrderNumber></OrderNumber>
              <ParentLineNo>0</ParentLineNo>
              <PartNo></PartNo>
              <PreviousEntryLineNumber>0</PreviousEntryLineNumber>
              <PreviousEntryNumber></PreviousEntryNumber>
              <PrimaryPreference></PrimaryPreference>
              <Procedure></Procedure>
              <RelatedIndicator>
                <Code></Code>
              </RelatedIndicator>
              <SecondaryPreference></SecondaryPreference>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code></Code>
              </TaxType>
              <UnitPrice>0</UnitPrice>
              <ValuationCode>
                <Code></Code>
              </ValuationCode>
              <ValuationMarkup>0.000</ValuationMarkup>
              <Volume>0.000</Volume>
              <VolumeUnit>
                <Code></Code>
              </VolumeUnit>
              <Weight>3.000</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>

              <AddInfoCollection>
                <AddInfo>
                  <Key>ADD_NA</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>BOMLineExpanded</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CustomsValue</Key>
                  <Value>200</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CVD_NA</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DateOfExport</Key>
                  <Value>2022-04-03 00:00:00.000</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DestinationState</Key>
                  <Value>MI</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedDuty</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedHMF</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedMPF</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedTax</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWCalcDutyWithAdValoremRate</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWCDInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWClaimAmountOverriden_New</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpBOLInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpNoticeInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpWavInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWImportInvoiceLineNo</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForExportSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForImportSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForManufacturerSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWOldData</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWQuarterlyHMF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>ExportCode</Key>
                  <Value>OS</Value>
                </AddInfo>
                <AddInfo>
                  <Key>FlavorContentCreditInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>FTAPayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>HasMPF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsBondedADD</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsBondedCVD</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsExcludedFromAII</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsNAFTANet</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsParent</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsUsedVehicle</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>LicenseType</Key>
                  <Value>C33</Value>
                </AddInfo>
                <AddInfo>
                  <Key>ManifestUQ</Key>
                  <Value>CS</Value>
                </AddInfo>
                <AddInfo>
                  <Key>NonFTAPayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>OverrideDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>OverrideSupDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>PayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_HTSChanged4ValueInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigHasMPF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigOverrideDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigOverrideSupDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_Textile</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>SchDLoading</Key>
                  <Value>58201</Value>
                </AddInfo>
                <AddInfo>
                  <Key>SPI</Key>
                  <Value>N/A</Value>
                </AddInfo>
                <AddInfo>
                  <Key>TransactionsRelated</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UC_NKCountryOfExport</Key>
                  <Value>HK</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UC_NKCountryOfOrigin</Key>
                  <Value>HK</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UI_NKCarrierSCAC</Key>
                  <Value>AA</Value>
                </AddInfo>
              </AddInfoCollection>

              <CustomizedFieldCollection>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute1</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute2</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute3</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Alex''s attribute #</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Serial Number</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>CBO PiggyPART1</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>CBO PiggyPART2</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>STR Piggy</Key>
                  <Value></Value>
                </CustomizedField>
              </CustomizedFieldCollection>

              <CustomsReferenceCollection>
                <CustomsReference>
                  <Type>
                    <Code>FEE</Code>
                    <Description>Fee</Description>
                  </Type>
                  <IsOverridden>false</IsOverridden>
                  <Order>0</Order>
                  <Reference> 0.69000</Reference>
                  <SubType>
                    <Code>499</Code>
                    <Description>Merchandise Processing Fee</Description>
                  </SubType>

                  <DateCollection>
                    <Date>
                      <Type>DateAtOffice</Type>
                      <Value></Value>
                    </Date>
                  </DateCollection>
                </CustomsReference>
              </CustomsReferenceCollection>

              <EntryReferenceCollection>
                <EntryReference>
                  <LineNumber>1</LineNumber>
                  <Reference></Reference>
                  <Type>
                    <Code>SE</Code>
                    <Description>ACE Cargo Release</Description>
                  </Type>
                </EntryReference>
              </EntryReferenceCollection>

              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>SoldToParty</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>Manufacturer</AddressType>
                  <Address1>174 GLOUCESTER ROAD</Address1>
                  <Address2>WAN CHAI DISTRICT</Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
                  <City>HONG KONG</City>
                  <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
                  <Country>
                    <Code>HK</Code>
                    <Name>Hong Kong</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <GovRegNum>123456789</GovRegNum>
                  <GovRegNumType>
                    <Code>DUN</Code>
                    <Description>DUNS Data Universal Numbering Syste</Description>
                  </GovRegNumType>
                  <OrganizationCode>ACETESHKG</OrganizationCode>
                  <Phone>+22555567651</Phone>
                  <Port>
                    <Code>HKHKG</Code>
                    <Name>Hong Kong</Name>
                  </Port>
                  <Postcode></Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State></State>

                  <RegistrationNumberCollection>
                    <RegistrationNumber>
                      <Type>
                        <Code>GMC</Code>
                        <Description>Glazing Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>1</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>PFR</Code>
                        <Description>Food Facility Registration Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>19148237698</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>MID</Code>
                        <Description>Supplier/Manufacturer ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>HKACETES174HON</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>TMC</Code>
                        <Description>Tire Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FPI</Code>
                        <Description>Foreign Producer Identifier</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>WCHATEA3310021</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>AMS</Code>
                        <Description>AMS (USDA) Assigned ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>7880212180</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FSR</Code>
                        <Description>FDA Foreign Seller Registration Num</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123456789</Value>
                    </RegistrationNumber>
                  </RegistrationNumberCollection>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>Seller</AddressType>
                  <Address1>174 GLOUCESTER ROAD</Address1>
                  <Address2>WAN CHAI DISTRICT</Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
                  <City>HONG KONG</City>
                  <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
                  <Country>
                    <Code>HK</Code>
                    <Name>Hong Kong</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <GovRegNum>123456789</GovRegNum>
                  <GovRegNumType>
                    <Code>DUN</Code>
                    <Description>DUNS Data Universal Numbering Syste</Description>
                  </GovRegNumType>
                  <OrganizationCode>ACETESHKG</OrganizationCode>
                  <Phone>+22555567651</Phone>
                  <Port>
                    <Code>HKHKG</Code>
                    <Name>Hong Kong</Name>
                  </Port>
                  <Postcode></Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State></State>

                  <RegistrationNumberCollection>
                    <RegistrationNumber>
                      <Type>
                        <Code>GMC</Code>
                        <Description>Glazing Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>1</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>PFR</Code>
                        <Description>Food Facility Registration Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>19148237698</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>MID</Code>
                        <Description>Supplier/Manufacturer ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>HKACETES174HON</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>TMC</Code>
                        <Description>Tire Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FPI</Code>
                        <Description>Foreign Producer Identifier</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>WCHATEA3310021</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>AMS</Code>
                        <Description>AMS (USDA) Assigned ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>7880212180</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FSR</Code>
                        <Description>FDA Foreign Seller Registration Num</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123456789</Value>
                    </RegistrationNumber>
                  </RegistrationNumberCollection>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>ShipToParty</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>UltimateConsignee</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>

          <CustomizedFieldCollection>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>CBO testPART1</Key>
              <Value></Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>CBO testPART2</Key>
              <Value></Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>STR test</Key>
              <Value></Value>
            </CustomizedField>
          </CustomizedFieldCollection>

          <OrganizationAddressCollection>
            <OrganizationAddress>
              <AddressType>Manufacturer</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>Seller</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>ShipToParty</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>SoldToParty</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>UltimateConsignee</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>FDAShipper</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>Importer</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
          </OrganizationAddressCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ConsolidatedCargoStatus>
      <Code></Code>
    </ConsolidatedCargoStatus>
    <ContainerCount>0</ContainerCount>
    <CustomsBroker>
      <Code>PMP</Code>
      <Name>Phil Pearse</Name>
    </CustomsBroker>
    <CustomsContainerMode>
      <Code>NCT</Code>
      <Description>Non-Containerized (Trans. Mode: 10,</Description>
    </CustomsContainerMode>
    <CustomsOffice>
      <Code></Code>
    </CustomsOffice>
    <CustomsProfileIdentifier>
      <Type>UserName</Type>
      <Value></Value>
    </CustomsProfileIdentifier>
    <CustomsValuationPort>
      <Code></Code>
    </CustomsValuationPort>
    <DeclarantType>
      <Code></Code>
    </DeclarantType>
    <DefermentAccountNumber></DefermentAccountNumber>
    <EFTMode>
      <Code></Code>
    </EFTMode>
    <EntryStatus>
      <Code></Code>
    </EntryStatus>
    <ExportGoodsType>
      <Code></Code>
    </ExportGoodsType>
    <Folio></Folio>
    <GoodsDescription>INTEGRO - WZP EHUB TEST</GoodsDescription>
    <GoodsOrigin>
      <Code></Code>
    </GoodsOrigin>
    <IsPersonalEffects>false</IsPersonalEffects>
    <JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>0.000000000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>CHI</Code>
        <Name>CHI</Name>
      </Branch>
      <ClientContractNumber></ClientContractNumber>
      <Currency>
        <Code>USD</Code>
        <Description>United States Dollar</Description>
      </Currency>
      <Department>
        <Code>CIA</Code>
        <Name>Clearance Import Air</Name>
      </Department>
      <HomeBranch>
        <Code>PHL</Code>
        <Name>PHL</Name>
      </HomeBranch>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>PMP</Code>
        <Name>Phil Pearse</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0.000000000</TotalAccrual>
      <TotalCost>0.000000000</TotalCost>
      <TotalJobProfit>0.000000000</TotalJobProfit>
      <TotalRevenue>0.000000000</TotalRevenue>
      <TotalWIP>0.000000000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>0.000000000</WIPRecognized>

      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>DUT</Code>
            <Description>ESTIMATED DUTIES</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>CDS</Code>
            <Description>Customs Duty / Tax / Entry Fees and other disbursements</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>ESTIMATED DUTIES</Description>
          <DisplaySequence>1</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Freight</Description>
          <DisplaySequence>2</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>DCART</Code>
            <Description>Delivery Cartage</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>DST</Code>
            <Description>Destination</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Delivery Cartage</Description>
          <DisplaySequence>3</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>OCART</Code>
            <Description>Pickup Cartage</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>TBC</Code>
            <Description>Transport Booking</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Pickup Cartage</Description>
          <DisplaySequence>4</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
    <LloydsIMO></LloydsIMO>
    <LocationAtClearance>
      <Code></Code>
      <Description></Description>
    </LocationAtClearance>
    <MergeBy>
      <Code>NON</Code>
      <Description>No Merge</Description>
    </MergeBy>
    <MessageStatus>
      <Code></Code>
      <Description>Not Sent</Description>
    </MessageStatus>
    <MessageSubType>
      <Code></Code>
    </MessageSubType>
    <MessageType>
      <Code>IMP</Code>
      <Description>Import</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>ACE</Code>
      <Description>ACE ABI</Description>
    </MessagingApplicationCode>
    <OperationalStatus>
      <Code></Code>
    </OperationalStatus>
    <OwnerRef></OwnerRef>
    <PaidBy>
      <Code></Code>
    </PaidBy>
    <PaymentMethod>
      <Code>BRK</Code>
      <Description>Broker</Description>
    </PaymentMethod>
    <PortOfDestination>
      <Code>USWAS</Code>
      <Name>Washington</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>USPHL</Code>
      <Name>Philadelphia</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfOrigin>
    <ScreeningStatus>
      <Code>CLR</Code>
      <Description>Clear</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>REG</Code>
      <Description>Regular</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <SubLocationAtClearance>
      <Code></Code>
      <Description></Description>
    </SubLocationAtClearance>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
    <TotalNoOfPacksPackageType>
      <Code>CS</Code>
      <Description>Case</Description>
    </TotalNoOfPacksPackageType>
    <TotalNoOfPieces>0</TotalNoOfPieces>
    <TotalNoOfPiecesLanded>0</TotalNoOfPiecesLanded>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code></Code>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air (Non Container, Container) (40, 41)</Description>
    </TransportMode>
    <TransportNationality>
      <Code></Code>
    </TransportNationality>
    <VesselName></VesselName>
    <VoyageFlightNo>AA007</VoyageFlightNo>
    <WarehouseReleaseStatus>
      <Code></Code>
    </WarehouseReleaseStatus>
    <WayBillNumber>PMP040422</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <AddInfoCollection>
      <AddInfo>
        <Key>7501Agent</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>7501Purchased</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>AcceleratedClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>AccLiqReq</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondAmount</Key>
        <Value>228</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondCalcCode</Key>
        <Value>DEF</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondDesignationCode</Key>
        <Value>B</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondProducerAccNo</Key>
        <Value>1125749</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondSuperseding</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondType</Key>
        <Value>9</Value>
      </AddInfo>
      <AddInfo>
        <Key>Box29IncludeContainers</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>CargoReleaseType</Key>
        <Value>ACE</Value>
      </AddInfo>
      <AddInfo>
        <Key>CertifyCargoRelease</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsolACE</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DateOfExport</Key>
        <Value>2022-04-03 00:00:00.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>DestinationState</Key>
        <Value>MI</Value>
      </AddInfo>
      <AddInfo>
        <Key>DomesticCargo</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWBillOfFormula</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWDestroyedValuation</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWElectManufPetroleumCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWElectPetroleumCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWEnableMerge</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWExamWitness</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWOilSpillTaxCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWOneTimeWaiverInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWPrintSubTotals</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWSameCondNAFTA</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWUnUsedWine</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableAII</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableCRL</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableENS</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableINB</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableSPN</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryFilerCode</Key>
        <Value>SV9</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryType</Key>
        <Value>01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExportCode</Key>
        <Value>OS</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExporterSummaryInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_DirectDelivery</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_IncludePTT</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_PTTWOExc</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactEmail</Key>
        <Value>phil.pearse@wisetechglobal.com</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactName</Key>
        <Value>Phil Pearse</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactPhoneNo</Key>
        <Value>8473645600</Value>
      </AddInfo>
      <AddInfo>
        <Key>FileTheirOwnRecon</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixDefTaxDueDate</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixPSD</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixRecon</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IBBTAFDA</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImmediateDelivery</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>InBondMoveFromFTZ</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>InsuranceAgent</Key>
        <Value>WZP</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsAggregate</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsAIIRequested</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsFinalWHS</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsHMFApplicable</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsInvoiceByRequest</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JobReadyForPost</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>LicenseType</Key>
        <Value>C33</Value>
      </AddInfo>
      <AddInfo>
        <Key>ManEntry</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MonthlyFiling</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAClaimStat</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAReconIndicator</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NoDutyCalc</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NonAMS</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherReconIndicator</Key>
        <Value>NA</Value>
      </AddInfo>
      <AddInfo>
        <Key>PaymentType</Key>
        <Value>2</Value>
      </AddInfo>
      <AddInfo>
        <Key>PetroleumClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PGAExpeditedRelease</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PreInspectionInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PreliminaryStatementPrintDate</Key>
        <Value>2022-04-14 00:00:00.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>PriorDisclosure</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ProtestStat</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PSC</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>RetailSalesSubstitutionIndicator</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>RN_NKCountryOfDestination</Key>
        <Value>US</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDArrival</Key>
        <Value>1101</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDEntry</Key>
        <Value>1101</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDEntryDescription</Key>
        <Value>PHILADELPHIA, PA</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDLoading</Key>
        <Value>58201</Value>
      </AddInfo>
      <AddInfo>
        <Key>SEMultiCargoDispInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>SuretyCode</Key>
        <Value>422</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxDeferIndicator</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalEnteredValue</Key>
        <Value>200</Value>
      </AddInfo>
      <AddInfo>
        <Key>UC_NKCountryOfExport</Key>
        <Value>HK</Value>
      </AddInfo>
      <AddInfo>
        <Key>UI_NKCarrierSCAC</Key>
        <Value>AA</Value>
      </AddInfo>
      <AddInfo>
        <Key>US_NKLocationOfGoods</Key>
        <Value>I317</Value>
      </AddInfo>
      <AddInfo>
        <Key>USMCAClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>WaiverNoticeInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterWayBillNumber</Key>
        <Value>00198653214</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterWayBillIssuerSCAC</Key>
        <Value>AA</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>00198653214</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <IssueDate></IssueDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <NoOfPacks>0.0000</NoOfPacks>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>

        <AddInfoCollection>
          <AddInfo>
            <Key>UI_NKBillIssuerSCAC</Key>
            <Value>AA</Value>
          </AddInfo>
        </AddInfoCollection>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>PMP040422</BillNumber>
        <BillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </BillType>
        <IssueDate></IssueDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <NoOfPacks>10.0000</NoOfPacks>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>
        <ParentBillNumber>00198653214</ParentBillNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>ParentBillIssuerSCAC</Key>
            <Value>AA</Value>
          </AddInfo>
        </AddInfoCollection>
      </AdditionalBill>
    </AdditionalBillCollection>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection>
    </ContainerCollection>

    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>custom text 1</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>custom txt 2</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>date override reg setting</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Document Received Date</Key>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2022-04-03T00:00:00</Value>
      </Date>
      <Date>
        <Type>LoadingDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-04-03T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DischargeDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-04-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntrySubmitted</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryAuthorisation</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>WarehouseRelease</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <EntryHeaderCollection>
      <EntryHeader>
        <Type>
          <Code>ENS</Code>
          <Description>Entry Summary</Description>
        </Type>
        <Reference></Reference>
        <BondValidToDate></BondValidToDate>
        <EntryReleaseDate></EntryReleaseDate>
        <EntryStatus>
          <Code></Code>
        </EntryStatus>
        <EntrySubmittedDate></EntrySubmittedDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <TotalAmountPaid>27.75000000</TotalAmountPaid>

        <AddInfoCollection>
          <AddInfo>
            <Key>DestinationState</Key>
            <Value>MI</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsDeactivated</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>NAFTAClaimStat</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>PriorDisclosure</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>ProtestStat</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>SentLatestFDA</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>ShouldBeReportToCustoms</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>UseConsigneeNameAddress</Key>
            <Value>N</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryHeaderChargeCollection>
          <EntryHeaderCharge>
            <Type>
              <Code>499</Code>
              <Description>Merchandise Processing Fee</Description>
            </Type>
            <Amount>27.75000000</Amount>
          </EntryHeaderCharge>
        </EntryHeaderChargeCollection>

        <EntryLineCollection>
          <EntryLine>
            <LineNumber>1</LineNumber>
            <CustomsStatus>
              <Code>ACT</Code>
              <Description>Active</Description>
            </CustomsStatus>
            <CustomsValue>200.00</CustomsValue>
            <Description></Description>
            <DutyRateFlatAmount>0</DutyRateFlatAmount>
            <DutyRateFlatAmountUnit>
              <Code></Code>
            </DutyRateFlatAmountUnit>
            <DutyRatePercent>0</DutyRatePercent>
            <HarmonisedCode>9701100000</HarmonisedCode>

            <AddInfoCollection>
              <AddInfo>
                <Key>DutyRateDesc</Key>
                <Value>Free</Value>
              </AddInfo>
              <AddInfo>
                <Key>HasMPF</Key>
                <Value>Y</Value>
              </AddInfo>
              <AddInfo>
                <Key>IJAccepted</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>SupLine</Key>
                <Value>N</Value>
              </AddInfo>
            </AddInfoCollection>

            <EntryLineChargeCollection>
              <EntryLineCharge>
                <Type>
                  <Code>499</Code>
                  <Description>Merchandise Processing Fee</Description>
                </Type>
                <Amount>0.69</Amount>
                <BaseValue>0</BaseValue>
                <IsLandedCostOnly>false</IsLandedCostOnly>
                <MethodOfCalculation>
                  <Code></Code>
                </MethodOfCalculation>
                <MethodOfPayment>
                  <Code></Code>
                </MethodOfPayment>
                <Rate>0</Rate>
                <RateOverrideReason>
                  <Code></Code>
                </RateOverrideReason>
              </EntryLineCharge>
            </EntryLineChargeCollection>
          </EntryLine>
        </EntryLineCollection>

        <RelatedEntryHeaderCollection>
          <RelatedEntryHeader>
            <Type>
              <Code>SE</Code>
              <Description>ACE Cargo Release</Description>
            </Type>
            <Reference></Reference>
            <BondValidToDate></BondValidToDate>
            <EntryReleaseDate></EntryReleaseDate>
            <EntryStatus>
              <Code></Code>
            </EntryStatus>
            <EntrySubmittedDate></EntrySubmittedDate>
            <MessageStatus>
              <Code></Code>
              <Description>Not Sent</Description>
            </MessageStatus>
            <TotalAmountPaid>0</TotalAmountPaid>

            <AddInfoCollection>
              <AddInfo>
                <Key>IsDeactivated</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>NAFTAClaimStat</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>PriorDisclosure</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ProtestStat</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>SentLatestFDA</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ShouldBeReportToCustoms</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>UseConsigneeNameAddress</Key>
                <Value>N</Value>
              </AddInfo>
            </AddInfoCollection>

            <EntryLineCollection>
              <EntryLine>
                <LineNumber>1</LineNumber>
                <CustomsStatus>
                  <Code>ACT</Code>
                  <Description>Active</Description>
                </CustomsStatus>
                <CustomsValue>200.00</CustomsValue>
                <Description></Description>
                <DutyRateFlatAmount>0</DutyRateFlatAmount>
                <DutyRateFlatAmountUnit>
                  <Code></Code>
                </DutyRateFlatAmountUnit>
                <DutyRatePercent>0</DutyRatePercent>
                <HarmonisedCode>9701100000</HarmonisedCode>

                <AddInfoCollection>
                  <AddInfo>
                    <Key>DutyRateDesc</Key>
                    <Value>Free</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>HasMPF</Key>
                    <Value>N</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>IJAccepted</Key>
                    <Value>N</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>SupLine</Key>
                    <Value>N</Value>
                  </AddInfo>
                </AddInfoCollection>
              </EntryLine>
            </EntryLineCollection>
          </RelatedEntryHeader>
        </RelatedEntryHeaderCollection>
      </EntryHeader>
    </EntryHeaderCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Number>73054965</Number>
        <Type>
          <Code>ENS</Code>
          <Description>Entry Summary</Description>
        </Type>
        <CountryOfIssue>
          <Code>US</Code>
          <Name>United States</Name>
        </CountryOfIssue>
        <EntryIsSystemGenerated>true</EntryIsSystemGenerated>
        <IssueDate></IssueDate>
      </EntryNumber>
    </EntryNumberCollection>

    <MilestoneCollection>
      <Milestone>
        <Description>ORG Received</Description>
        <EventCode>CMI</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLine</AddressType>
        <Address1>PO BOX 619616</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: P.O. BOX 619616</AddressShortCode>
        <City>DALLAS</City>
        <CompanyName>AMERICAN AIRLINES</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>36-310911100</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>AMEAIR_US</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>USDFW</Code>
          <Name>Dallas-Fort Worth Int Apt</Name>
        </Port>
        <Postcode>75261-9616</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""Texas"">TX</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Seller</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SoldToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>UltimateConsignee</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FDASubmitter</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterPickupDeliveryAddress</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierDocumentaryAddress</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Contact>Ian Chen</Contact>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email>ian.chen@fisherman.com</Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+61888888999</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierPickupDeliveryAddress</AddressType>
        <Address1>173 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>OFFICE</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>1234 8714</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LEI</Code>
              <Description>Legal Entity Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1234567890ABCDEFGHIJ</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>GLN</Code>
              <Description>Global Location Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1234567890123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterOfRecord</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FDASubmitter</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Seller</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BondContact</AddressType>
        <Contact>Phil Pearse</Contact>
        <Email>phil.pearse@wisetechglobal.com</Email>
        <Phone>+18473645600</Phone>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>SNP</Code>
              <Description>Secondary Notify Party</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>3901SV9</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <BillNumber>PMP040422</BillNumber>
        <BillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </BillType>
        <CustomsOuterPacks>0</CustomsOuterPacks>
        <InBondPackQty>0</InBondPackQty>
        <Link>1</Link>
        <MarksAndNos></MarksAndNos>
        <PackQty>0</PackQty>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>
        <ShippingSymbol></ShippingSymbol>

        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge>
          <Code>USPHL</Code>
          <Name>Philadelphia</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <AircraftType>
          <Code></Code>
        </AircraftType>
        <BookingStatus>
          <Code>PLN</Code>
          <Description>Planned</Description>
        </BookingStatus>
        <Carrier>
          <AddressType>Carrier</AddressType>
          <Address1>PO BOX 619616</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <AddressShortCode>PST: P.O. BOX 619616</AddressShortCode>
          <City>DALLAS</City>
          <CompanyName>AMERICAN AIRLINES</CompanyName>
          <Country>
            <Code>US</Code>
            <Name>United States</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum>36-310911100</GovRegNum>
          <GovRegNumType>
            <Code>EIN</Code>
            <Description>Employer Identification Number</Description>
          </GovRegNumType>
          <OrganizationCode>AMEAIR_US</OrganizationCode>
          <Phone></Phone>
          <Port>
            <Code>USDFW</Code>
            <Name>Dallas-Fort Worth Int Apt</Name>
          </Port>
          <Postcode>75261-9616</Postcode>
          <ScreeningStatus>
            <Code>CLR</Code>
            <Description>Clear</Description>
          </ScreeningStatus>
          <State Description=""Texas"">TX</State>
        </Carrier>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <DocumentCutOff></DocumentCutOff>
        <EstimatedArrival>2022-04-04T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2022-04-03T00:00:00</EstimatedDeparture>
        <FCLAvailability></FCLAvailability>
        <FCLCutOff></FCLCutOff>
        <FCLReceivalCommences></FCLReceivalCommences>
        <FCLStorage></FCLStorage>
        <IsCargoOnly>true</IsCargoOnly>
        <LCLAvailability></LCLAvailability>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LCLStorageDate></LCLStorageDate>
        <LegNotes></LegNotes>
        <LegType>Flight1</LegType>
        <ScheduledArrival>2022-04-04T00:00:00</ScheduledArrival>
        <ScheduledDeparture>2022-04-03T00:00:00</ScheduledDeparture>
        <TransportMode>Air</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName></VesselName>
        <VGMCutOff></VGMCutOff>
        <VoyageFlightNo>AA007</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>

  </Body>
</UniversalInterchange>', N'', N'', N'', CAST(N'2022-05-30T04:14:18.703' AS DateTime), N'E', CAST(N'2022-06-21T03:03:00' AS SmallDateTime), N'E', N'27a55065-ac88-4ec3-8bed-e575e79172cb', NULL, N'HUB', NULL, NULL)
INSERT [dbo].[EDIMessage] ([EM_PK], [EM_IsActive], [EM_IsTestMessage], [EM_ApplicationCode], [EM_MessageOwner], [EM_MessageType], [EM_MessageSubType], [EM_ReceiveTransmit], [EM_Status], [EM_HeldUntilDate], [EM_SendWithMessageErrors], [EM_MessageText], [EM_MessageNText], [EM_EI], [EM_GB], [EM_GE], [EM_LinkTable], [EM_LinkUniqueID], [EM_SystemCreateTimeUtc], [EM_SystemCreateUser], [EM_SystemLastEditTimeUtc], [EM_SystemLastEditUser], [EM_MessageData], [EM_MessageNum], [EM_GP], [EM_ApplicationReference]) VALUES (N'{EDIMessagePK}', 1, 0, N'UXB', N'', N'XDC', N'XUS', N'TRX', N'FAL', NULL, 0, N'<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>B00223702</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <AgentsReference></AgentsReference>
    <Branch>
      <Code>PHL</Code>
      <Name>PHL</Name>
    </Branch>
    <CommercialInfo>
      <Name>All Invoices</Name>

      <CommercialInvoiceCollection Content=""Complete"">
        <CommercialInvoice>
          <InvoiceNumber>GKFJK596958</InvoiceNumber>
          <AdditionalTerms></AdditionalTerms>
          <AgreedExchangeRate>1.000000000</AgreedExchangeRate>
          <ExchangeRateType>
            <Code></Code>
          </ExchangeRateType>
          <IncoTerm>
            <Code>FOB</Code>
            <Description>Free On Board</Description>
          </IncoTerm>
          <InvoiceAmount>300.0000</InvoiceAmount>
          <InvoiceCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </InvoiceCurrency>
          <InvoiceDate></InvoiceDate>
          <LandedCostExchangeRate>1.000000000</LandedCostExchangeRate>
          <MessageStatus>
            <Code></Code>
            <Description>Not Sent</Description>
          </MessageStatus>
          <NetWeight>0.000</NetWeight>
          <NetWeightUQ>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </NetWeightUQ>
          <NoOfPacks>0.000</NoOfPacks>
          <PaymentAmount>0.0000</PaymentAmount>
          <PaymentDate></PaymentDate>
          <PaymentExchangeRate>1.000000000</PaymentExchangeRate>
          <PaymentNumber></PaymentNumber>
          <RelatedIndicator>
            <Code></Code>
          </RelatedIndicator>
          <Supplier>
            <AddressType>Supplier</AddressType>
            <Address1>174 GLOUCESTER ROAD</Address1>
            <Address2>WAN CHAI DISTRICT</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
            <City>HONG KONG</City>
            <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>123456789</GovRegNum>
            <GovRegNumType>
              <Code>DUN</Code>
              <Description>DUNS Data Universal Numbering Syste</Description>
            </GovRegNumType>
            <OrganizationCode>ACETESHKG</OrganizationCode>
            <Phone>+22555567651</Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>GMC</Code>
                  <Description>Glazing Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>1</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>PFR</Code>
                  <Description>Food Facility Registration Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>19148237698</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>HKACETES174HON</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>TMC</Code>
                  <Description>Tire Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>FPI</Code>
                  <Description>Foreign Producer Identifier</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>WCHATEA3310021</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>AMS</Code>
                  <Description>AMS (USDA) Assigned ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>7880212180</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>FSR</Code>
                  <Description>FDA Foreign Seller Registration Num</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123456789</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </Supplier>
          <ValuationCode>
            <Code></Code>
          </ValuationCode>
          <ValuationDateOverride></ValuationDateOverride>
          <Volume>0.000</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0.000</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>

          <AddInfoCollection>
            <AddInfo>
              <Key>DateOfExport</Key>
              <Value>2022-04-03 00:00:00.000</Value>
            </AddInfo>
            <AddInfo>
              <Key>DestinationState</Key>
              <Value>MI</Value>
            </AddInfo>
            <AddInfo>
              <Key>ExportCode</Key>
              <Value>OS</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactEmail</Key>
              <Value>phil.pearse@wisetechglobal.com</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactName</Key>
              <Value>Phil Pearse</Value>
            </AddInfo>
            <AddInfo>
              <Key>FDAContactPhoneNo</Key>
              <Value>8473645600</Value>
            </AddInfo>
            <AddInfo>
              <Key>GenAIIForSup</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>InvoiceType</Key>
              <Value>IN</Value>
            </AddInfo>
            <AddInfo>
              <Key>IsLineGrouping</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>LicenseType</Key>
              <Value>C33</Value>
            </AddInfo>
            <AddInfo>
              <Key>UC_NKCountryOfExport</Key>
              <Value>HK</Value>
            </AddInfo>
          </AddInfoCollection>

          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <BondedWarehouseQuantity>0.00000</BondedWarehouseQuantity>
              <BondedWarehouseQuantityUnit>
                <Code></Code>
              </BondedWarehouseQuantityUnit>
              <BrandName></BrandName>
              <ClassificationCode></ClassificationCode>
              <ClassUsageComment></ClassUsageComment>
              <Commodity>
                <Code></Code>
              </Commodity>
              <ConcessionOrder></ConcessionOrder>
              <ContainerMode>
                <Code></Code>
              </ContainerMode>
              <CountryOfExport>
                <Code></Code>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code></Code>
              </CountryOfOrigin>
              <CustomsQuantity>3.00000</CustomsQuantity>
              <CustomsQuantityUnit>
                <Code>KG</Code>
                <Description>1,000 Grams</Description>
              </CustomsQuantityUnit>
              <CustomsSecondQuantity>0.00000</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code></Code>
              </CustomsSecondQuantityUnit>
              <CustomsThirdQuantity>0.00000</CustomsThirdQuantity>
              <CustomsThirdQuantityUnit>
                <Code></Code>
              </CustomsThirdQuantityUnit>
              <CustomsValue>200.00</CustomsValue>
              <DataImportMatchingKey></DataImportMatchingKey>
              <Description></Description>
              <EntryLineNumber>1</EntryLineNumber>
              <EntryNumber>73054965</EntryNumber>
              <HarmonisedCode>9701100000</HarmonisedCode>
              <HazardousMaterial>
                <Code></Code>
                <CodeType>
                  <Code></Code>
                </CodeType>
              </HazardousMaterial>
              <InvoiceQuantity>0.00000</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code></Code>
              </InvoiceQuantityUnit>
              <LinePrice>200.0000</LinePrice>
              <Link>1</Link>
              <LocalDescription></LocalDescription>
              <Model></Model>
              <NetWeight>0.000</NetWeight>
              <NetWeightUnit>
                <Code></Code>
              </NetWeightUnit>
              <OrderLineLink>0</OrderLineLink>
              <OrderNumber></OrderNumber>
              <ParentLineNo>0</ParentLineNo>
              <PartNo></PartNo>
              <PreviousEntryLineNumber>0</PreviousEntryLineNumber>
              <PreviousEntryNumber></PreviousEntryNumber>
              <PrimaryPreference></PrimaryPreference>
              <Procedure></Procedure>
              <RelatedIndicator>
                <Code></Code>
              </RelatedIndicator>
              <SecondaryPreference></SecondaryPreference>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code></Code>
              </TaxType>
              <UnitPrice>0</UnitPrice>
              <ValuationCode>
                <Code></Code>
              </ValuationCode>
              <ValuationMarkup>0.000</ValuationMarkup>
              <Volume>0.000</Volume>
              <VolumeUnit>
                <Code></Code>
              </VolumeUnit>
              <Weight>3.000</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>

              <AddInfoCollection>
                <AddInfo>
                  <Key>ADD_NA</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>BOMLineExpanded</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CustomsValue</Key>
                  <Value>200</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CVD_NA</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DateOfExport</Key>
                  <Value>2022-04-03 00:00:00.000</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DestinationState</Key>
                  <Value>MI</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedDuty</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedHMF</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedMPF</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRW99ClaimedTax</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWCalcDutyWithAdValoremRate</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWCDInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWClaimAmountOverriden_New</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpBOLInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpNoticeInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWExpWavInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWImportInvoiceLineNo</Key>
                  <Value></Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForExportSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForImportSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWIsForManufacturerSection</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWOldData</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DRWQuarterlyHMF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>ExportCode</Key>
                  <Value>OS</Value>
                </AddInfo>
                <AddInfo>
                  <Key>FlavorContentCreditInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>FTAPayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>HasMPF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsBondedADD</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsBondedCVD</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsExcludedFromAII</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsNAFTANet</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsParent</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsUsedVehicle</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>LicenseType</Key>
                  <Value>C33</Value>
                </AddInfo>
                <AddInfo>
                  <Key>ManifestUQ</Key>
                  <Value>CS</Value>
                </AddInfo>
                <AddInfo>
                  <Key>NonFTAPayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>OverrideDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>OverrideSupDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>PayableMPF</Key>
                  <Value>27.75</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_HTSChanged4ValueInd</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigHasMPF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigOverrideDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_OrigOverrideSupDuty</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>R_Textile</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>SchDLoading</Key>
                  <Value>58201</Value>
                </AddInfo>
                <AddInfo>
                  <Key>SPI</Key>
                  <Value>N/A</Value>
                </AddInfo>
                <AddInfo>
                  <Key>TransactionsRelated</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UC_NKCountryOfExport</Key>
                  <Value>HK</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UC_NKCountryOfOrigin</Key>
                  <Value>HK</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UI_NKCarrierSCAC</Key>
                  <Value>AA</Value>
                </AddInfo>
              </AddInfoCollection>

              <CustomizedFieldCollection>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute1</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute2</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Attribute3</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Alex''s attribute #</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>Serial Number</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>CBO PiggyPART1</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>CBO PiggyPART2</Key>
                  <Value></Value>
                </CustomizedField>
                <CustomizedField>
                  <DataType>String</DataType>
                  <Key>STR Piggy</Key>
                  <Value></Value>
                </CustomizedField>
              </CustomizedFieldCollection>

              <CustomsReferenceCollection>
                <CustomsReference>
                  <Type>
                    <Code>FEE</Code>
                    <Description>Fee</Description>
                  </Type>
                  <IsOverridden>false</IsOverridden>
                  <Order>0</Order>
                  <Reference> 0.69000</Reference>
                  <SubType>
                    <Code>499</Code>
                    <Description>Merchandise Processing Fee</Description>
                  </SubType>

                  <DateCollection>
                    <Date>
                      <Type>DateAtOffice</Type>
                      <Value></Value>
                    </Date>
                  </DateCollection>
                </CustomsReference>
              </CustomsReferenceCollection>

              <EntryReferenceCollection>
                <EntryReference>
                  <LineNumber>1</LineNumber>
                  <Reference></Reference>
                  <Type>
                    <Code>SE</Code>
                    <Description>ACE Cargo Release</Description>
                  </Type>
                </EntryReference>
              </EntryReferenceCollection>

              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>SoldToParty</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>Manufacturer</AddressType>
                  <Address1>174 GLOUCESTER ROAD</Address1>
                  <Address2>WAN CHAI DISTRICT</Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
                  <City>HONG KONG</City>
                  <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
                  <Country>
                    <Code>HK</Code>
                    <Name>Hong Kong</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <GovRegNum>123456789</GovRegNum>
                  <GovRegNumType>
                    <Code>DUN</Code>
                    <Description>DUNS Data Universal Numbering Syste</Description>
                  </GovRegNumType>
                  <OrganizationCode>ACETESHKG</OrganizationCode>
                  <Phone>+22555567651</Phone>
                  <Port>
                    <Code>HKHKG</Code>
                    <Name>Hong Kong</Name>
                  </Port>
                  <Postcode></Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State></State>

                  <RegistrationNumberCollection>
                    <RegistrationNumber>
                      <Type>
                        <Code>GMC</Code>
                        <Description>Glazing Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>1</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>PFR</Code>
                        <Description>Food Facility Registration Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>19148237698</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>MID</Code>
                        <Description>Supplier/Manufacturer ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>HKACETES174HON</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>TMC</Code>
                        <Description>Tire Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FPI</Code>
                        <Description>Foreign Producer Identifier</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>WCHATEA3310021</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>AMS</Code>
                        <Description>AMS (USDA) Assigned ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>7880212180</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FSR</Code>
                        <Description>FDA Foreign Seller Registration Num</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123456789</Value>
                    </RegistrationNumber>
                  </RegistrationNumberCollection>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>Seller</AddressType>
                  <Address1>174 GLOUCESTER ROAD</Address1>
                  <Address2>WAN CHAI DISTRICT</Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
                  <City>HONG KONG</City>
                  <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
                  <Country>
                    <Code>HK</Code>
                    <Name>Hong Kong</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <GovRegNum>123456789</GovRegNum>
                  <GovRegNumType>
                    <Code>DUN</Code>
                    <Description>DUNS Data Universal Numbering Syste</Description>
                  </GovRegNumType>
                  <OrganizationCode>ACETESHKG</OrganizationCode>
                  <Phone>+22555567651</Phone>
                  <Port>
                    <Code>HKHKG</Code>
                    <Name>Hong Kong</Name>
                  </Port>
                  <Postcode></Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State></State>

                  <RegistrationNumberCollection>
                    <RegistrationNumber>
                      <Type>
                        <Code>GMC</Code>
                        <Description>Glazing Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>1</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>PFR</Code>
                        <Description>Food Facility Registration Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>19148237698</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>MID</Code>
                        <Description>Supplier/Manufacturer ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>HKACETES174HON</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>TMC</Code>
                        <Description>Tire Manufacturer Code</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FPI</Code>
                        <Description>Foreign Producer Identifier</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>WCHATEA3310021</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>AMS</Code>
                        <Description>AMS (USDA) Assigned ID Number</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>7880212180</Value>
                    </RegistrationNumber>
                    <RegistrationNumber>
                      <Type>
                        <Code>FSR</Code>
                        <Description>FDA Foreign Seller Registration Num</Description>
                      </Type>
                      <CountryOfIssue>
                        <Code>US</Code>
                        <Name>United States</Name>
                      </CountryOfIssue>
                      <Value>123456789</Value>
                    </RegistrationNumber>
                  </RegistrationNumberCollection>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>ShipToParty</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
                <OrganizationAddress>
                  <AddressType>UltimateConsignee</AddressType>
                  <Address1>1300 PENNSYLVANIA AVE NW</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
                  <City>WASHINGTON</City>
                  <CompanyName>SEB TEST IMPORTER</CompanyName>
                  <Country>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </Country>
                  <Email>test@test.com</Email>
                  <Fax></Fax>
                  <GovRegNum>211101-12345</GovRegNum>
                  <GovRegNumType>
                    <Code>CBN</Code>
                    <Description>CBP Assigned Number</Description>
                  </GovRegNumType>
                  <OrganizationCode>SEBTESWAS</OrganizationCode>
                  <Phone>+12155551212</Phone>
                  <Port>
                    <Code>USWAS</Code>
                    <Name>Washington</Name>
                  </Port>
                  <Postcode>20004</Postcode>
                  <ScreeningStatus>
                    <Code>CLR</Code>
                    <Description>Clear</Description>
                  </ScreeningStatus>
                  <State Description=""District of Columbia"">DC</State>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>

          <CustomizedFieldCollection>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>CBO testPART1</Key>
              <Value></Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>CBO testPART2</Key>
              <Value></Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>STR test</Key>
              <Value></Value>
            </CustomizedField>
          </CustomizedFieldCollection>

          <OrganizationAddressCollection>
            <OrganizationAddress>
              <AddressType>Manufacturer</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>Seller</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>ShipToParty</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>SoldToParty</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>UltimateConsignee</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>FDAShipper</AddressType>
              <Address1>174 GLOUCESTER ROAD</Address1>
              <Address2>WAN CHAI DISTRICT</Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
              <City>HONG KONG</City>
              <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
              <Country>
                <Code>HK</Code>
                <Name>Hong Kong</Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum>123456789</GovRegNum>
              <GovRegNumType>
                <Code>DUN</Code>
                <Description>DUNS Data Universal Numbering Syste</Description>
              </GovRegNumType>
              <OrganizationCode>ACETESHKG</OrganizationCode>
              <Phone>+22555567651</Phone>
              <Port>
                <Code>HKHKG</Code>
                <Name>Hong Kong</Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State></State>

              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type>
                    <Code>GMC</Code>
                    <Description>Glazing Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>1</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>PFR</Code>
                    <Description>Food Facility Registration Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>19148237698</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>MID</Code>
                    <Description>Supplier/Manufacturer ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>HKACETES174HON</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>TMC</Code>
                    <Description>Tire Manufacturer Code</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FPI</Code>
                    <Description>Foreign Producer Identifier</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>WCHATEA3310021</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>AMS</Code>
                    <Description>AMS (USDA) Assigned ID Number</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>7880212180</Value>
                </RegistrationNumber>
                <RegistrationNumber>
                  <Type>
                    <Code>FSR</Code>
                    <Description>FDA Foreign Seller Registration Num</Description>
                  </Type>
                  <CountryOfIssue>
                    <Code>US</Code>
                    <Name>United States</Name>
                  </CountryOfIssue>
                  <Value>123456789</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
              <AddressType>Importer</AddressType>
              <Address1>1300 PENNSYLVANIA AVE NW</Address1>
              <Address2></Address2>
              <AddressOverride>false</AddressOverride>
              <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
              <City>WASHINGTON</City>
              <CompanyName>SEB TEST IMPORTER</CompanyName>
              <Country>
                <Code>US</Code>
                <Name>United States</Name>
              </Country>
              <Email>test@test.com</Email>
              <Fax></Fax>
              <GovRegNum>211101-12345</GovRegNum>
              <GovRegNumType>
                <Code>CBN</Code>
                <Description>CBP Assigned Number</Description>
              </GovRegNumType>
              <OrganizationCode>SEBTESWAS</OrganizationCode>
              <Phone>+12155551212</Phone>
              <Port>
                <Code>USWAS</Code>
                <Name>Washington</Name>
              </Port>
              <Postcode>20004</Postcode>
              <ScreeningStatus>
                <Code>CLR</Code>
                <Description>Clear</Description>
              </ScreeningStatus>
              <State Description=""District of Columbia"">DC</State>
            </OrganizationAddress>
          </OrganizationAddressCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ConsolidatedCargoStatus>
      <Code></Code>
    </ConsolidatedCargoStatus>
    <ContainerCount>0</ContainerCount>
    <CustomsBroker>
      <Code>PMP</Code>
      <Name>Phil Pearse</Name>
    </CustomsBroker>
    <CustomsContainerMode>
      <Code>NCT</Code>
      <Description>Non-Containerized (Trans. Mode: 10,</Description>
    </CustomsContainerMode>
    <CustomsOffice>
      <Code></Code>
    </CustomsOffice>
    <CustomsProfileIdentifier>
      <Type>UserName</Type>
      <Value></Value>
    </CustomsProfileIdentifier>
    <CustomsValuationPort>
      <Code></Code>
    </CustomsValuationPort>
    <DeclarantType>
      <Code></Code>
    </DeclarantType>
    <DefermentAccountNumber></DefermentAccountNumber>
    <EFTMode>
      <Code></Code>
    </EFTMode>
    <EntryStatus>
      <Code></Code>
    </EntryStatus>
    <ExportGoodsType>
      <Code></Code>
    </ExportGoodsType>
    <Folio></Folio>
    <GoodsDescription>INTEGRO - WZP EHUB TEST</GoodsDescription>
    <GoodsOrigin>
      <Code></Code>
    </GoodsOrigin>
    <IsPersonalEffects>false</IsPersonalEffects>
    <JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>0.000000000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>CHI</Code>
        <Name>CHI</Name>
      </Branch>
      <ClientContractNumber></ClientContractNumber>
      <Currency>
        <Code>USD</Code>
        <Description>United States Dollar</Description>
      </Currency>
      <Department>
        <Code>CIA</Code>
        <Name>Clearance Import Air</Name>
      </Department>
      <HomeBranch>
        <Code>PHL</Code>
        <Name>PHL</Name>
      </HomeBranch>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>PMP</Code>
        <Name>Phil Pearse</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0.000000000</TotalAccrual>
      <TotalCost>0.000000000</TotalCost>
      <TotalJobProfit>0.000000000</TotalJobProfit>
      <TotalRevenue>0.000000000</TotalRevenue>
      <TotalWIP>0.000000000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>0.000000000</WIPRecognized>

      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>DUT</Code>
            <Description>ESTIMATED DUTIES</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>CDS</Code>
            <Description>Customs Duty / Tax / Entry Fees and other disbursements</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>ESTIMATED DUTIES</Description>
          <DisplaySequence>1</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Freight</Description>
          <DisplaySequence>2</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>DCART</Code>
            <Description>Delivery Cartage</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>DST</Code>
            <Description>Destination</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Delivery Cartage</Description>
          <DisplaySequence>3</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>CHI</Code>
            <Name>CHI</Name>
          </Branch>
          <ChargeCode>
            <Code>OCART</Code>
            <Description>Pickup Cartage</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>TBC</Code>
            <Description>Transport Booking</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1.000000000</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <CostRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </CostRatingBehaviour>
          <Debtor>
            <Type>Organization</Type>
            <Key>SEBTESWAS</Key>
          </Debtor>
          <Department>
            <Code>CIA</Code>
            <Name>Clearance Import Air</Name>
          </Department>
          <Description>Pickup Cartage</Description>
          <DisplaySequence>4</DisplaySequence>
          <SellExchangeRate>1.000000000</SellExchangeRate>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0</SellOSGSTVATAmount>
          <SellRatingBehaviour>
            <Code>REA</Code>
            <Description>Clear and Re-autorate this charge</Description>
          </SellRatingBehaviour>

          <CostRatingBasisCollection>
          </CostRatingBasisCollection>

          <SellRatingBasisCollection>
          </SellRatingBasisCollection>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
    <LloydsIMO></LloydsIMO>
    <LocationAtClearance>
      <Code></Code>
      <Description></Description>
    </LocationAtClearance>
    <MergeBy>
      <Code>NON</Code>
      <Description>No Merge</Description>
    </MergeBy>
    <MessageStatus>
      <Code></Code>
      <Description>Not Sent</Description>
    </MessageStatus>
    <MessageSubType>
      <Code></Code>
    </MessageSubType>
    <MessageType>
      <Code>IMP</Code>
      <Description>Import</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>ACE</Code>
      <Description>ACE ABI</Description>
    </MessagingApplicationCode>
    <OperationalStatus>
      <Code></Code>
    </OperationalStatus>
    <OwnerRef></OwnerRef>
    <PaidBy>
      <Code></Code>
    </PaidBy>
    <PaymentMethod>
      <Code>BRK</Code>
      <Description>Broker</Description>
    </PaymentMethod>
    <PortOfDestination>
      <Code>USWAS</Code>
      <Name>Washington</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>USPHL</Code>
      <Name>Philadelphia</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfOrigin>
    <ScreeningStatus>
      <Code>CLR</Code>
      <Description>Clear</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>REG</Code>
      <Description>Regular</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <SubLocationAtClearance>
      <Code></Code>
      <Description></Description>
    </SubLocationAtClearance>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
    <TotalNoOfPacksPackageType>
      <Code>CS</Code>
      <Description>Case</Description>
    </TotalNoOfPacksPackageType>
    <TotalNoOfPieces>0</TotalNoOfPieces>
    <TotalNoOfPiecesLanded>0</TotalNoOfPiecesLanded>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code></Code>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air (Non Container, Container) (40, 41)</Description>
    </TransportMode>
    <TransportNationality>
      <Code></Code>
    </TransportNationality>
    <VesselName></VesselName>
    <VoyageFlightNo>AA007</VoyageFlightNo>
    <WarehouseReleaseStatus>
      <Code></Code>
    </WarehouseReleaseStatus>
    <WayBillNumber>PMP040422</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <AddInfoCollection>
      <AddInfo>
        <Key>7501Agent</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>7501Purchased</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>AcceleratedClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>AccLiqReq</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondAmount</Key>
        <Value>228</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondCalcCode</Key>
        <Value>DEF</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondDesignationCode</Key>
        <Value>B</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondProducerAccNo</Key>
        <Value>1125749</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondSuperseding</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>BondType</Key>
        <Value>9</Value>
      </AddInfo>
      <AddInfo>
        <Key>Box29IncludeContainers</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>CargoReleaseType</Key>
        <Value>ACE</Value>
      </AddInfo>
      <AddInfo>
        <Key>CertifyCargoRelease</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsolACE</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DateOfExport</Key>
        <Value>2022-04-03 00:00:00.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>DestinationState</Key>
        <Value>MI</Value>
      </AddInfo>
      <AddInfo>
        <Key>DomesticCargo</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWBillOfFormula</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWDestroyedValuation</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWElectManufPetroleumCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWElectPetroleumCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWEnableMerge</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWExamWitness</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWOilSpillTaxCert</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWOneTimeWaiverInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWPrintSubTotals</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWSameCondNAFTA</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>DRWUnUsedWine</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableAII</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableCRL</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableENS</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableINB</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EnableSPN</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryFilerCode</Key>
        <Value>SV9</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryType</Key>
        <Value>01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExportCode</Key>
        <Value>OS</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExporterSummaryInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_DirectDelivery</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_IncludePTT</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>F_PTTWOExc</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactEmail</Key>
        <Value>phil.pearse@wisetechglobal.com</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactName</Key>
        <Value>Phil Pearse</Value>
      </AddInfo>
      <AddInfo>
        <Key>FDAContactPhoneNo</Key>
        <Value>8473645600</Value>
      </AddInfo>
      <AddInfo>
        <Key>FileTheirOwnRecon</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixDefTaxDueDate</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixPSD</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FixRecon</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IBBTAFDA</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImmediateDelivery</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>InBondMoveFromFTZ</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>InsuranceAgent</Key>
        <Value>WZP</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsAggregate</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsAIIRequested</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsFinalWHS</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsHMFApplicable</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsInvoiceByRequest</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JobReadyForPost</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>LicenseType</Key>
        <Value>C33</Value>
      </AddInfo>
      <AddInfo>
        <Key>ManEntry</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MonthlyFiling</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAClaimStat</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NAFTAReconIndicator</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NoDutyCalc</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NonAMS</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherReconIndicator</Key>
        <Value>NA</Value>
      </AddInfo>
      <AddInfo>
        <Key>PaymentType</Key>
        <Value>2</Value>
      </AddInfo>
      <AddInfo>
        <Key>PetroleumClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PGAExpeditedRelease</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PreInspectionInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PreliminaryStatementPrintDate</Key>
        <Value>2022-04-14 00:00:00.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>PriorDisclosure</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ProtestStat</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PSC</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>RetailSalesSubstitutionIndicator</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>RN_NKCountryOfDestination</Key>
        <Value>US</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDArrival</Key>
        <Value>1101</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDEntry</Key>
        <Value>1101</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDEntryDescription</Key>
        <Value>PHILADELPHIA, PA</Value>
      </AddInfo>
      <AddInfo>
        <Key>SchDLoading</Key>
        <Value>58201</Value>
      </AddInfo>
      <AddInfo>
        <Key>SEMultiCargoDispInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>SuretyCode</Key>
        <Value>422</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxDeferIndicator</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalEnteredValue</Key>
        <Value>200</Value>
      </AddInfo>
      <AddInfo>
        <Key>UC_NKCountryOfExport</Key>
        <Value>HK</Value>
      </AddInfo>
      <AddInfo>
        <Key>UI_NKCarrierSCAC</Key>
        <Value>AA</Value>
      </AddInfo>
      <AddInfo>
        <Key>US_NKLocationOfGoods</Key>
        <Value>I317</Value>
      </AddInfo>
      <AddInfo>
        <Key>USMCAClaimInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>WaiverNoticeInd</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterWayBillNumber</Key>
        <Value>00198653214</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterWayBillIssuerSCAC</Key>
        <Value>AA</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>00198653214</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <IssueDate></IssueDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <NoOfPacks>0.0000</NoOfPacks>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>

        <AddInfoCollection>
          <AddInfo>
            <Key>UI_NKBillIssuerSCAC</Key>
            <Value>AA</Value>
          </AddInfo>
        </AddInfoCollection>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>PMP040422</BillNumber>
        <BillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </BillType>
        <IssueDate></IssueDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <NoOfPacks>10.0000</NoOfPacks>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>
        <ParentBillNumber>00198653214</ParentBillNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>ParentBillIssuerSCAC</Key>
            <Value>AA</Value>
          </AddInfo>
        </AddInfoCollection>
      </AdditionalBill>
    </AdditionalBillCollection>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection>
    </ContainerCollection>

    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>custom text 1</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>custom txt 2</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>date override reg setting</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Document Received Date</Key>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2022-04-03T00:00:00</Value>
      </Date>
      <Date>
        <Type>LoadingDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-04-03T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DischargeDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-04-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntrySubmitted</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryAuthorisation</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>WarehouseRelease</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <EntryHeaderCollection>
      <EntryHeader>
        <Type>
          <Code>ENS</Code>
          <Description>Entry Summary</Description>
        </Type>
        <Reference></Reference>
        <BondValidToDate></BondValidToDate>
        <EntryReleaseDate></EntryReleaseDate>
        <EntryStatus>
          <Code></Code>
        </EntryStatus>
        <EntrySubmittedDate></EntrySubmittedDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <TotalAmountPaid>27.75000000</TotalAmountPaid>

        <AddInfoCollection>
          <AddInfo>
            <Key>DestinationState</Key>
            <Value>MI</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsDeactivated</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>NAFTAClaimStat</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>PriorDisclosure</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>ProtestStat</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>SentLatestFDA</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>ShouldBeReportToCustoms</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>UseConsigneeNameAddress</Key>
            <Value>N</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryHeaderChargeCollection>
          <EntryHeaderCharge>
            <Type>
              <Code>499</Code>
              <Description>Merchandise Processing Fee</Description>
            </Type>
            <Amount>27.75000000</Amount>
          </EntryHeaderCharge>
        </EntryHeaderChargeCollection>

        <EntryLineCollection>
          <EntryLine>
            <LineNumber>1</LineNumber>
            <CustomsStatus>
              <Code>ACT</Code>
              <Description>Active</Description>
            </CustomsStatus>
            <CustomsValue>200.00</CustomsValue>
            <Description></Description>
            <DutyRateFlatAmount>0</DutyRateFlatAmount>
            <DutyRateFlatAmountUnit>
              <Code></Code>
            </DutyRateFlatAmountUnit>
            <DutyRatePercent>0</DutyRatePercent>
            <HarmonisedCode>9701100000</HarmonisedCode>

            <AddInfoCollection>
              <AddInfo>
                <Key>DutyRateDesc</Key>
                <Value>Free</Value>
              </AddInfo>
              <AddInfo>
                <Key>HasMPF</Key>
                <Value>Y</Value>
              </AddInfo>
              <AddInfo>
                <Key>IJAccepted</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>SupLine</Key>
                <Value>N</Value>
              </AddInfo>
            </AddInfoCollection>

            <EntryLineChargeCollection>
              <EntryLineCharge>
                <Type>
                  <Code>499</Code>
                  <Description>Merchandise Processing Fee</Description>
                </Type>
                <Amount>0.69</Amount>
                <BaseValue>0</BaseValue>
                <IsLandedCostOnly>false</IsLandedCostOnly>
                <MethodOfCalculation>
                  <Code></Code>
                </MethodOfCalculation>
                <MethodOfPayment>
                  <Code></Code>
                </MethodOfPayment>
                <Rate>0</Rate>
                <RateOverrideReason>
                  <Code></Code>
                </RateOverrideReason>
              </EntryLineCharge>
            </EntryLineChargeCollection>
          </EntryLine>
        </EntryLineCollection>

        <RelatedEntryHeaderCollection>
          <RelatedEntryHeader>
            <Type>
              <Code>SE</Code>
              <Description>ACE Cargo Release</Description>
            </Type>
            <Reference></Reference>
            <BondValidToDate></BondValidToDate>
            <EntryReleaseDate></EntryReleaseDate>
            <EntryStatus>
              <Code></Code>
            </EntryStatus>
            <EntrySubmittedDate></EntrySubmittedDate>
            <MessageStatus>
              <Code></Code>
              <Description>Not Sent</Description>
            </MessageStatus>
            <TotalAmountPaid>0</TotalAmountPaid>

            <AddInfoCollection>
              <AddInfo>
                <Key>IsDeactivated</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>NAFTAClaimStat</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>PriorDisclosure</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ProtestStat</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>SentLatestFDA</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ShouldBeReportToCustoms</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>UseConsigneeNameAddress</Key>
                <Value>N</Value>
              </AddInfo>
            </AddInfoCollection>

            <EntryLineCollection>
              <EntryLine>
                <LineNumber>1</LineNumber>
                <CustomsStatus>
                  <Code>ACT</Code>
                  <Description>Active</Description>
                </CustomsStatus>
                <CustomsValue>200.00</CustomsValue>
                <Description></Description>
                <DutyRateFlatAmount>0</DutyRateFlatAmount>
                <DutyRateFlatAmountUnit>
                  <Code></Code>
                </DutyRateFlatAmountUnit>
                <DutyRatePercent>0</DutyRatePercent>
                <HarmonisedCode>9701100000</HarmonisedCode>

                <AddInfoCollection>
                  <AddInfo>
                    <Key>DutyRateDesc</Key>
                    <Value>Free</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>HasMPF</Key>
                    <Value>N</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>IJAccepted</Key>
                    <Value>N</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>SupLine</Key>
                    <Value>N</Value>
                  </AddInfo>
                </AddInfoCollection>
              </EntryLine>
            </EntryLineCollection>
          </RelatedEntryHeader>
        </RelatedEntryHeaderCollection>
      </EntryHeader>
    </EntryHeaderCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Number>73054965</Number>
        <Type>
          <Code>ENS</Code>
          <Description>Entry Summary</Description>
        </Type>
        <CountryOfIssue>
          <Code>US</Code>
          <Name>United States</Name>
        </CountryOfIssue>
        <EntryIsSystemGenerated>true</EntryIsSystemGenerated>
        <IssueDate></IssueDate>
      </EntryNumber>
    </EntryNumberCollection>

    <MilestoneCollection>
      <Milestone>
        <Description>ORG Received</Description>
        <EventCode>CMI</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLine</AddressType>
        <Address1>PO BOX 619616</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: P.O. BOX 619616</AddressShortCode>
        <City>DALLAS</City>
        <CompanyName>AMERICAN AIRLINES</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>36-310911100</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>AMEAIR_US</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>USDFW</Code>
          <Name>Dallas-Fort Worth Int Apt</Name>
        </Port>
        <Postcode>75261-9616</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""Texas"">TX</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Seller</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SoldToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>UltimateConsignee</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FDASubmitter</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterPickupDeliveryAddress</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierDocumentaryAddress</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Contact>Ian Chen</Contact>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email>ian.chen@fisherman.com</Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+61888888999</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierPickupDeliveryAddress</AddressType>
        <Address1>173 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>OFFICE</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>1234 8714</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LEI</Code>
              <Description>Legal Entity Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1234567890ABCDEFGHIJ</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>GLN</Code>
              <Description>Global Location Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1234567890123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterOfRecord</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FDASubmitter</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Seller</AddressType>
        <Address1>174 GLOUCESTER ROAD</Address1>
        <Address2>WAN CHAI DISTRICT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
        <City>HONG KONG</City>
        <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>123456789</GovRegNum>
        <GovRegNumType>
          <Code>DUN</Code>
          <Description>DUNS Data Universal Numbering Syste</Description>
        </GovRegNumType>
        <OrganizationCode>ACETESHKG</OrganizationCode>
        <Phone>+22555567651</Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GMC</Code>
              <Description>Glazing Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>PFR</Code>
              <Description>Food Facility Registration Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>19148237698</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>MID</Code>
              <Description>Supplier/Manufacturer ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>HKACETES174HON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TMC</Code>
              <Description>Tire Manufacturer Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FPI</Code>
              <Description>Foreign Producer Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>WCHATEA3310021</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AMS</Code>
              <Description>AMS (USDA) Assigned ID Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>7880212180</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>FSR</Code>
              <Description>FDA Foreign Seller Registration Num</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>123456789</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <Address1>1300 PENNSYLVANIA AVE NW</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1300 PENNSYLVANIA AVE NW</AddressShortCode>
        <City>WASHINGTON</City>
        <CompanyName>SEB TEST IMPORTER</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>test@test.com</Email>
        <Fax></Fax>
        <GovRegNum>211101-12345</GovRegNum>
        <GovRegNumType>
          <Code>CBN</Code>
          <Description>CBP Assigned Number</Description>
        </GovRegNumType>
        <OrganizationCode>SEBTESWAS</OrganizationCode>
        <Phone>+12155551212</Phone>
        <Port>
          <Code>USWAS</Code>
          <Name>Washington</Name>
        </Port>
        <Postcode>20004</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State Description=""District of Columbia"">DC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BondContact</AddressType>
        <Contact>Phil Pearse</Contact>
        <Email>phil.pearse@wisetechglobal.com</Email>
        <Phone>+18473645600</Phone>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>SNP</Code>
              <Description>Secondary Notify Party</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>3901SV9</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <BillNumber>PMP040422</BillNumber>
        <BillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </BillType>
        <CustomsOuterPacks>0</CustomsOuterPacks>
        <InBondPackQty>0</InBondPackQty>
        <Link>1</Link>
        <MarksAndNos></MarksAndNos>
        <PackQty>0</PackQty>
        <PackType>
          <Code>CS</Code>
          <Description>Case</Description>
        </PackType>
        <ShippingSymbol></ShippingSymbol>

        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge>
          <Code>USPHL</Code>
          <Name>Philadelphia</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <AircraftType>
          <Code></Code>
        </AircraftType>
        <BookingStatus>
          <Code>PLN</Code>
          <Description>Planned</Description>
        </BookingStatus>
        <Carrier>
          <AddressType>Carrier</AddressType>
          <Address1>PO BOX 619616</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <AddressShortCode>PST: P.O. BOX 619616</AddressShortCode>
          <City>DALLAS</City>
          <CompanyName>AMERICAN AIRLINES</CompanyName>
          <Country>
            <Code>US</Code>
            <Name>United States</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum>36-310911100</GovRegNum>
          <GovRegNumType>
            <Code>EIN</Code>
            <Description>Employer Identification Number</Description>
          </GovRegNumType>
          <OrganizationCode>AMEAIR_US</OrganizationCode>
          <Phone></Phone>
          <Port>
            <Code>USDFW</Code>
            <Name>Dallas-Fort Worth Int Apt</Name>
          </Port>
          <Postcode>75261-9616</Postcode>
          <ScreeningStatus>
            <Code>CLR</Code>
            <Description>Clear</Description>
          </ScreeningStatus>
          <State Description=""Texas"">TX</State>
        </Carrier>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <DocumentCutOff></DocumentCutOff>
        <EstimatedArrival>2022-04-04T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2022-04-03T00:00:00</EstimatedDeparture>
        <FCLAvailability></FCLAvailability>
        <FCLCutOff></FCLCutOff>
        <FCLReceivalCommences></FCLReceivalCommences>
        <FCLStorage></FCLStorage>
        <IsCargoOnly>true</IsCargoOnly>
        <LCLAvailability></LCLAvailability>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LCLStorageDate></LCLStorageDate>
        <LegNotes></LegNotes>
        <LegType>Flight1</LegType>
        <ScheduledArrival>2022-04-04T00:00:00</ScheduledArrival>
        <ScheduledDeparture>2022-04-03T00:00:00</ScheduledDeparture>
        <TransportMode>Air</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName></VesselName>
        <VGMCutOff></VGMCutOff>
        <VoyageFlightNo>AA007</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
', N'', N'6b0d0c25-1448-48c9-b3a1-654b861c6e96', N'27a55065-ac88-4ec3-8bed-e575e79172cb', N'86bb1c22-0865-4685-996e-d56cbd136491', N'', NULL, CAST(N'2022-05-30T03:45:46.353' AS DateTime), N'E', CAST(N'2022-06-21T03:03:00' AS SmallDateTime), N'E', 0x505AED7D5973E3B6B2F0FB5775FF036BEEC39753373396E46D9C525CA1B5D8CC683B923CBEE7BCA4680AB25843913A24E558F9F5B70170010980A4165AB6C3546A6604F486ADD168349ACD7BDB7C46AEA75B9385B95A22DB575E9696EDFDFA69E1FBAB5F4E4EFEFCF3CF2F86EE3E397F9A1EFA6238CB9389B1404BDD3B89304F1AB57AFDA45EFFA4E0DFA663FFFAA9FEA5FEE9FABFFE9FA23443B2E417FC6EEBBEDE726C1FBD844541E1C459BB066A3996850C1F8844B5897AA614CAA79B15BA6EAD3DDF597A6D6458BAAB63D4E609A948807E439BEB9B5AADD138BDAC359A27F827C3E044C0812DE4C4A2957143825275363331946E4D91BBF4AE9B27E99210F009FAC41BA33972910D4C0130551200DEB8BA6D2C62A15ACE0C5D8FEE7ACD13F2AFA87CA02F8372F2AF50CA043A202F97C8354CDDD2ECB99342562D4BD1EC67C73490175161F8C6A80426EE12857483EDFFFA09805616F2D127B66F39CCE4C0048583F5F211B9D7B7DFBABF7F3BBFBAB83AFFDA3C495625B08A767408FEE42234EBBC180BDD7E4263DD47D7F52FB5F03FDCF95C7D029FADE1E656382AA921A1039089098D371C2CAE905E77782320899703F20CD75C91E9D805C195A1ADDC38BA3B8359C954250511B20A7B5F5D3A6B58A3A7B5DA17DA21C972114A6BEDE289BA118A7E3F69E78B0E0AC4473365E243D7784A1BE693EE66B740CE3894AA8D872E026D7303D9D3ED199AB51CCF974F06094C824E1F799EFE84B0EC6BAFE86C48B57FE0F8CA04D64D569BE57C9A03E43F20F369E15F934183251B1588E1EEFF2914F4DB6DBEA8DF4CCB7972F5A59725AB8C5173E00CE723DDF8E14592460509B891BEC15B4530E9C2A9982C15210483CEFE1281C9475C0420A21068A20841A499C6C802F49966CF4C43F71DB7B8AAC8C46C4ED6AB9565228E1EE83D172609D12D210CD18651A918A17E5DBF3C536E7BC3FB566732ED8C95F1506D4788750956E3FA411D28AD3B5553DADA643AD65AD308A721C1198259E09AD0DCB96E7928028F8AC5589385E3FAA497EA97F5CFF5CB8654D61832DDD5A6BFB9BE1B0E6E956FF007F439FECD0DC772A5DB1BBA03B63ACA14182893FBD1A8A7019FBB6F78A462080E19A6A49BA6190E3245E6C40AF7DB3BC77E52BEC11F892D3B9E0E62E2CDCE52372D9840F4EF5465577F812AFC67AAE2D6791EA32798AFD7F5C6E9D9F9C5E5D7ABE6495C288316CC9FB075EDFB81AC79AC8A00B089826D2525B21715BA6C4C68FE64E3F948AE53703764C9D21CBA4FBA6DFE45AC3E220B0C218CE01DD6695C5D0A77B4706C74FD3F8DC639FC777179715E87754DCAD28030BDA4437C27519F454759401C187ABE41D544F4CF14C8C4807DDF862E14EE40A178ADDEB8C818B52C94B5F36231B3F935C91E0EE2D2BF63B331A88601343D9F1AE774F08576BE149A83096D7FBE3C6CFA6DBF25693AD7FC5B4BFF0B4FC6BE6EAFE7BAE1AF5DE42A1835AB4B68B748840815C370AE79DE3A4BCCFB49969464C624CC24E12CA2B2E4F26C7ED72DA880694EFFC175FC49819E3FCCE88CBAB28949601206AEE3CC94AE6E9816686F85E51EE8910F3A4A57F5B3AF7058BDB8FAFA06C6ABAF89EDF900861D80D00639492C27ADFDA147EBEE1BDD78C0A4027BE30D0CD8B4B8FA9B9A2EFADBE8BEC6E91B189CEE482BAEFD5C3852D9CAC875666B032FA4199C3BCCB9F95157D2039C2DA61DF5F4B45EAB35DEC256A5F6333B811D0300557EBA9FB4D57F28AAE7C1B041C77C70C577F9F52B0C53A3FEB5F60686AA3B296E55B455255C5B1304B6A8CB99161F73C098E3DFCEE325029118F4702410F92D8830D29399DC4522478B2962FF4FE45B605012E54954C75A43B706CEA9E09700028FC516B28A719A49A79DC8631778D1B6E296C04954A9B319F6F64B4F5C21003F13F02509E9B579E76505E7D4F4B509D3F3E8BA516B343ED7CE3ED74E955AED17F27FD09F8269467C3702A639B220CF376D3298643D64CAD3D70EC99A7600358C32980E2787640A4A0A5FAD8059469C3D999C570BD3FAB28273BC877EC357753E32164F96F3A85BF8D2AE1CA9B032CA146A0442292322543912107FCDC0C914E2EBD9E5E905E8BCC3CEC55B64AB9A065B08E8B74CF692D3C06E5C837B0DBCF16432D50ECBD5EB9936BA759DF5CAC48EACD76A6D0F9A6A7BF9AD6D9D4AACFADDD8DEB7FE187C8BF6D302AA0F3B7A8BB38F4A131A3901C15D99E2FE972B7021382F2C2E1D38D8FF13FC8B83B871F0CDD783EEA285B3F6D03FD73A9C35FC4D701503EB4706509492604F0B5A20BB31C3FDB50D3172613E236612BDFD9E094DA666CBD2C1449FE39B96D09C00FE7CA118F11E5FCA916EB7FD102F51C6A341853313F5556EF3E5A880681BC8C331174377466EA6D22522145F87F177FB62C33357982CF4666AD9EC423F9B40CC61E89A4FA6D0302FC84142A01904944493FB349CFDE98A3CCCCCD92EBD37489D54EA3F036FE536FBF235685901F6A18C1364C0AAE257B8B8BA1895DD5677715221D7E9C274E5A2276B0BD1D84BF07C4A21CFD050C61247E8925318BE32D3967809F475DF58C0BE8B37201A76C497F3E8CC44C999361DBC1AC88E40CF7AB03FA48BC43841E5E569EDFCECEAE23C4093A1DCE9EED2B1C1329D91EEBCBAACD5EBC1C0A5AA04A87FE9EE0C543F3419B9B0CD6D3B5641758EFB408E7C22C5C6C2E70917C6A6F033365D9187B9DB3C2D428418072317476735A220A0B84C04FE23B0247E086A1D43B79253902BE290F086822FB6E9DF5C75C1709724EC6E1D968DDE241B2BEE1BD2F81ABE69660BC4F051E808FB8B031D818D63FB817946025398DF22681FCA09982F0470D1B3095333BD9E31654955368D380046502A403597BABB01D838CC912F13A039069AAD5D0A1EFE9B03CB09B42130D9E39C4BA14937A5741B44A53CAA4F3C27BB5A29D9E8CDA9FE22F73E66D1952136F15CA78B1D6647FC4378E4919BC7B9FCB3D163EA7DDDFDB15E5D333E24B698C7CB73E13150BB29850CDCD0A5772A75E9315007310A0BC4E351A9A56E41529DE71A6481C41B27363ED476FB8F812A3E2113A04CD7009553CE249FFFCDB08F15181C1848E0E6F104612DBA5C291A327FD401E4F87EDC01C9F71927FA610BBFF101642BE44366E493F9910F20CAF8E1EAAA65E9E612CDDA6B7F932BCA6B0872D7EFBE0939FAA3B72107EC97C794A3A55B069E1B0FA6BF5067C0C671D1725C64DE96B7BA41AA36D84CC795008F0E0D090FEFF4EC3F06E8CFA30A051AEF66D83B76D7801403C707DBED0D08F2A03F1F5B0AEA2C611CE3B2EB22469E32C5F1BA8E4BB7C609B5B98EDB3B581CDA456F461C3616ED2D0835B466D8EB765419FEB986233E72AD4D91FDB93441F26EC119196437E1FB0BD1B5F467C70DDE1CB65C3433FDA3EA98EE541DE91BFDD142456C96C6E597CBF3B244B9D3BD223294D6159A47AFEAE044787C21E014744C213A2F86B50631BAAEB35435ED98A20C5498A303947F162B5106EAC93CA604F71E9A7D470BD3B08E683CE78634304248C31AF61703B658730E27E1FB7FE64B519A221F38F61BD29DE131A2D049BCB4F9114A3159AF8E2BC85B1996F11F77D3498B3CCB9D9D111647DDECC77F608FFCB1F7592AC5DB98B149598E3E6FC77F4CD18B6F1E53C74F8C45BBE7E83369AC1C23C4F9D7464DFACA616F414605CC8E13B52CF65357B73D9D9CDFBCE0AAED7883523CA88F914616D8776871E82DDF31C5D1B0383A2C613872B7D456AE28EA4EB3263FF49140D17B14F32F309E4D64CDB2AFA752C062A1F1F99D6648F05DB22CA3026997A83E803EAE7D54DFCB5D942F5EF92DE0F21DBDBB169CBEDB1658E8E5FF7B8A1E3644F9EFF7DA9209096E8A1EB3BDCF46B46E86CAC87C7ADA8CD4F1F4DD2EEC442BDEEDE29E4CC7B415A536808328B2F5C469D88AEC3D5CD2B61494B41B94F0DD62A793F10E5049BD5C44B96FB315F9DB44A8D2C2CC3B33648729791265422C1A4D1E06BB8961E27E506A5F2EAEC82D7E4EDF4CD68FB9DD73767555BC7BFAC8C5499C66A68714123CE67938B747C15E8BC591CDF2EC19114149EAC2D98041547F389F9B06CA182B8290B706A8E452A6B42A7B1E9FE44E641E247B1D91B8C082AB28092B6E44323E39235E9140B37915F75D9C932DD6264E20D5C2C9321538EF20DDDB67A506E1D4594392D1C91C309B0D29C89B953D2A020471035436019963CDA60E0E47DD64E5204B62D6AFEBA7B59A32EA0C06937FF5BEAB034D55D4EF1D65F0204D479624D0B896E6204B026E97892C89CBE423CB935696902C2048D2903DA8933B6D703BC5D9420479C9425026F7D8A47343B39369FDD1703CED8CB3529345F8E204655175DE237465AB67E88A346D59504B93960109FF37FC077D9A2ACA6416C0CBF29905D57102B346BD5EAFD53F93D7EDF2C46669B4DCC5DFBA9125390BA01269B46E4671DE8762491F949C0467010C97CA0CE6024C059843B969CE020A41B2B37AA38EB39DC15F0D71B2B3105CFC762BEE987BCA3C77DE3CE81E7EA3E2E32BFC8C4923E517E7426B803971264D881640E7A545639B204F8E16406D95222D6C491109686CB8C210FBF5531BE753300D5F71E64A0B871A3F9AFAA7EB762BCEA9C6F32AA6A277D3E46C10C656AA7C8BA49249DC6D524B26310FA4D6774C3319D02B906C3284DC23E56444225FAF4BD34F064005D21306904574FA61D47881E494699C5C1D2E4F5419401D305D65D861BBE8F4E2A92B030A45135886E0793A3D2B996500B7C59C29A0D23F92369726BF0C80B64A8129C5C93956CA6AC34ECA4E8A19401E243566402BE7685B2051122B7E8EA54A00B7B256A98C05A5C84B9F19922B3C6A871FDFECB49A01E441936B868DFE80E39C9F8033247CC411CF4ECC19401E303D67D8E48F37DEC5527886B48F38E4D9A93D03C8FD137C862DFD78239D9104342478C4E1CD4E0E1A401E284568D8DC8F37C6C5D28886B48F38DCD9E94503C88325190D1BFCF1063C3F116948F7986B3B3341690079E034A561B33FDE90E7A6320DC9161CC72DD29A8608657AE0E87857BEB7CAF756F9DE2ADF5BE57BAB7C6FD5C9ED0DDB7695EFADF2BD09202BDF5BE57BAB3478E57BFB1B8D71E57BFB9B0D78E57BFBDB0DF947F7BD2DCC5515C75CC5315771CC551C7315C7FC9EE398EF2DDF5C9207503699D1A8D2E7953EE76A2B7DCE7758A5CF2B7D5E9E3E1742C93F717692FF8D33094CC657D60AA619C97D01BFC5EBF7E8FD3E5633194908325FFDE6BD7A2F556049BE81B72530CE2D80053EB8ACC5D3096CF5E2B6906DB3E3FBAC5DE343768B0DD9D5A439644C48B178903D6341326C98DC18906277F9729B2527EE23C33CD936DE23D726C98FF3386C8C479E5DB25F6CC716711D325BA4483C47D1F117B22814C351C0E2C8B736B6B534F2AD8CDC788D6D63358AFA89F3F33CE4C5671C2A36232B354B211F6C99A7A3CC4FD6E7C66014F4F61E6ECCF2622E0E1D6FF1EEC7AE485CC5AB8F625E1CC5616328DEFB18168D9578F561CC8B8D38485CC47B1FBDCCF887571FB2BC7887C3C53ABCF7712B1AD3F0EA439817C370C8F885F73E8845E2145E7F0DE6C4251C3E26E1BD0F6381D88342A3B855CC4111AFE6F65E9C226F7C2AFF4DE5BF89AB2AFF4DE5BF6105ADFC3795FFA6F2DFFC4D0CD8CA7FF3FEC7B0F2DFBCE7D1ABFC372251DEFEB855FE1BE5FD0F62E5BF099AF8BE87F123F96F8ABE13D92BA6B8503CF1011D37BBC711178C21DE277E38D76F9331AF8BCFE95CDF4DA158E1628E9CA231C2057D3959B1C1FBC5056FEFBBD9261E788B58E06CDF4D760C70C1F8DF5CE74D5EDCEF9BF6E0EC13E35B92262DFAE5904A9326C12A4D5A69D24A93569A74D7176F953E4D8255FAB4D2A7953EADF469A430BB6D151FF357559C8658A756711A559C4615A7F116756915A7F19EBDE46FE88EA38AD3D872ECAA388D773F86559CC67B1EBD2A4E4324CADB1FB72A4E4379FF8358C569044D7CDFC3F881E234B4E50A8E785B78702A5778E50A572A57B8D47153B9C2198877E90A2F9E804D90582DAA15D47134923073272CC617948E65CE40FA594B779F9C546FD09E4F743BFE91858469FABA69E30D0634C3758D20B025211CC9E2E5DDB8CE0F66C70ABC1DFD516AACE9741C2D4C4B19C1F0628DCDCCC7302758925AC822E2DF4F5024F407D8C59FE4C44E9C81637F8ED071C631E5A7A9ABDBDE1705D3FA45A9D77E16CDB4481E11EB50ACE17C9E1847615F0B40437C389DCC4D0BC567939814BD85F6908BFB2865040953AF459C644443A618894CD7C4B2CF925D84017D6C583A74A49F144D4446040AF873E42E4146D530F0A40AEC1E0C2DAC08D03ADDA9600EA41826819A1DBCBD1559173C60B3F3828D9E5BC79979F90D150337BBB0D61CBC3B92BF834202C5CE3B6D30EDDC8E87CA67E5E1DF23A573774F8D05BC15A520590A43D77C32ED6CB178C0A6E68D90EB39B66E75E67350345E6844F11501C6EFCE630B7600D0B73133182177AD5B03C71F23C379B2F1F2BAAE7D81FD027486B0328DCAE1D512C842CC27981B63F48C6C98FB1894FD1D41DDC07C3316AC02A65BD01DEF44A08A89D42436C8E6498A48B36599C00B6B045737E2192B2C8E91D6AE8B6C63C3C902675B5E16769C13E69BD2862D41B62BE2A59AE2029456BAEBE385C4F782A6CA7A01EFBCD068A4506B5F514D97EB1601E5E69DB344923E1FDDF524DC484D8AB88050B3E718BA457B991978416984315C217A52F2A0F7E6735E266E7BCADBA0C8662FA1DA1CFA0BE4B6D1A3EFB88C8082D258BF3BBE6E05B33C39F513354978BC0005C0A4380909AB95EC0322F0B82E891349CE6188A57FD04602585C1AC1C10FB172E02A581499524856C5F771CDD6028C183E516D3CE4517DD27AE3262CB344048A8259269CB2A013444030602E2215DC46A7CD97A0965DE7B00F687D75DA692B00AE752672DB186889F93172DCBACE7A256E755B7C9C485AE7D42A50DA6B7FA39C2853FD05FE245BA7D245A0A8747BA63878EA2B33D37B5CC342C2AAC22B26332F5B134FEFCE8BB1D0ED2734C6567A9D9D145C2D870B5B1AFC093326D8E212651C34D12AEA925ABE5F621E6C3987349C0830A24201B86053884741B43170A3507C7308BA58CE3790E97632FDAE4EC386841889520E0B7A1C2C821BB4D09F4D679D765BD1E68C3BFC6EC34F2ABCF190A933469FF535A84C7C78F317A6A7186472E4B52D4B922655C269FEC446630F70426723CD851C1FEFD3C990B1601C75F1EE1BF789680726B54577E190B390CBAEBA038EC72B4BDF4CD07FD63049C8F579BA28018FBDBAF295C9D572B8C16197468869038AC216F218A9B59C28E3A0456B395DCE21716B395128007FFDB59CC1379029BD9605A51CD6DB58CB6249648A47F74C5942F0845AE0E064ADCF2458042EDCCC52D6C69BB641BAE302364817DFE42CFCB24C8FC3CB5099129529519912873425F2975FDA5C685416446541249A5359101FD18268B7D422FB771B5938907EA3B440D7E8399A780F63A23D29240C766707DABA32282A8342D69CCAA0A0B5873728B6D0076933E2B4B22C2ACB22D19CCAB2F88896C5B0986531328D1FEB55D976C5F446FC6C20F95C00879710D578E3383FE887F62AEBA2B22E24CDA9AC0B5A7B78EBA2B04E481B1267956D51D91689E654B6C54EB6055B280869E562CA9A3DCBD9CC3CAD3FBC6E9EC4FF0E2B61FA92385B3F521331273EF02D353AE2284F39CD661F81E4379B148BC1301D749E0A3755089E985B922470F03CD04CF9A1891C135F9980EA9331E1A946ACD68FF9618C42D89082005DE3637D13818D449367CACAF38139A1E2C7B3061F461FBC256B75B298E21C19EA8D96C555CA228E1ED3AD2261A332F0E6F04F1BB963340784E89F41D5483767DCDC4AD14DC000C6066F777DE42F9C590AF1669CCEDF91E80C1A532DEE0B1159F2EA6038671C64297EA2870799CF0DE8230311C99097E9511DC971E2C307E3403D7D86ACD5C2D4C5BC5224034E5DD3F57CD575CD6736BE4ED8FF32F88052CFD167897858822DCAA691954323E493A416B01086F7EECA2119022C7B3F217BAB51EC8586EC5D06EC35EE33FE9A3C7A46E96E1F77B8D6B084C7E8692D310CF0D6C4916DE24C43784E6BB6E14C91BB4CB1EB0E6FB2D875417C6568C3E14D776712A662064DD09865EC519964692CE6C019CE47BAF1C3BB0EE330E312215C1B19E6320A3D4DE384B54254FC87782368713A213167744FB22DE6D16704309181528DA44562C81E5863388C94830F2A58ACEFF8415010FE1AC007453C14B65CB395871838889C25D7A8094E41110F25E0C42F7EB653BF9996F3E4EA4B61C065825B42AED06921785DA16A99BA008E68CA4F03C756A2A7323FC7FFFC87F2D359ED67E5ACFE0F893802BEB13083605B659F568A7B5B8ED1FC0EBB3DB2883E6C9E303FC26A6703B3AD6BE11E1938D7AA5AAB5D025CB234807DD05DB470D61E1A2358865E01CBAD79928903143737A665058F0646FD51EDAC76D6686034B622092D587A770F994AED0E0BA000FA23A08B4722499B31B9AD91EBC08AF1924F3FE886189CADA98DC39731B250477F50A7CE9E4D0F961F9644582143C4EF652DE48B50E32A0EB9A73FC2718A1E45228D27AC94A04E4D327904851CC218FD676DBA081B6E3102532845E8BACE5280428A39A4A9BB367E3CE8A62F6954BA5E4E20D5B4643983B6845337F4F2D08EBA3DCD5906914524E22EAE11A152AF8E9C7BA25E4E80E7CC9447681D8FE6819D8562E13C765C190F4E69B1C041490C4ADE8A11D787F08D4E7A359FC8109ADD564F7DD6C1187EB4305AE2270B148ADB86456293234FB20F334032C9B4F50DD991A57599D8D8DCCAA210D58BA874608D104B6C80D02CE109937462015C4C9F0E56464789013248304D14D56460A63A4856CB53D8A573B23131ED09766B116DBBC48E518FCE38AE3442B9D3F18BD485F9885D7DD4B4C31B49E0C794D446D89AEDAD89B51BAAC3E89D225711E39037BB40063B09834D377EDE28A88B307BB0744C376C4B72D0457562443C1C4317765C978C99B42E81CD2CE09E700143691BBB49C3F416F40D77D82E492D8B2D183851698412E8D1F4862D2C1623B19BB5A4228528DCA8055542B440A3734529E0C406CD15498083CD59509842906DCCE25A1972A229922D79E49AB63F24365CD77183439337B4D5871B6EA9B73BDDECA7A66D34D7D796AFFCD43697DECF0ACC2AEB11282ABEA3C0194668BB13BF42AE0CC133CD840919D4A8B319CE6420F0268755ACB4F8F2E9F2BC5627CF7DD37751C18BF87FA593EC90442E094A59A4476BD7588042981D9EBC6A18A06C5C9270C1D2CDA566CB987059F2B662D233FF03B3F3F0A46F1C7B46EF4F24B41B0D2E4BE376D45BBA65E0192AA14F66F03EF461029B4F76E4EA95B0B9D98F49986E0E4662E04858D4EB8DF3CB332E1FD4768C26EB157261A692E88232061B1F032594F712FDA571A5D986B5265B14751178876F01496A126CEF192D2177083BF340AE6FCE372CABC3EB0DBA8F13390FDD47D85E18CEE9A942B6A86B8DC6E7DAD9E7DAA952ABFD42FEA72EAB9D79C6EE7F729091F0ED6B7BB07096988941C6A5845E1B3F602FC9700EFBDD726DE9A570C0DDE43A1B348B32AF94C2A683F75D92D77484809F85D64B3CA9CBE3F50A6C6C6C350737B065D07FD1970FA66F832D530AFDA1694D56D80BA7BF94D647431B614B122C4A382C94628800136219E21B64EC6E2EA7AF26FA126F22B381DA9DAAA570B8B7EF618F7D804DEAF0E4E94C5535AD2CD2AD71AF2CD29D01170DB6FF5647496B839BB2A49E8C066590F6DD4DD7B4706A34A95539F9BE87CD4438641831352EC3EF16B4C9EE9F21F970B22F71E44ED6CBA5EE6E4AD134DD3FDA701237FCD0A9570687C05E1D4DA7655007B20FC3CE8B5102EDB64A6C6CC327393B250C560BD3FAB222F98E7EFBD3F4908F8CC593E53CEA16CDF7B93F779AC14EC83C916C697F4E2487A6F4E0F5F5ECF2F4E2ECFC621F0316AFF4E90299EEF04F1BE7222AC130EB9A2F6D340703A0BD466DB98DBC1F87110ECF2C816E497DA2DDDC4C5518E712282F97708A866E2E4F8368363E50F79D67841D86DDE9BFCB601178C3B3BC630FFF1EEDC1C1539F9E5CF454CA8404E29A86BDAA08C7339741BF0BA74EEBE14E66B6EC45FBAEDF0DE208C1C828834110197EB309BAE8F03C7E771EC7489F6DB013D72983410FE4B7337D31AD53EE6313C5C9C3219698498717BC0F3BCBC2C2165E29AE36727629CF231C93C7EE9692E8139D0FE2E3405AC72D81898333CD61FF7019B46DF2518A43D325A91F8B758CBA3B97207637635535F6201E396B4A9B9EA35B150E09B0FF82D6CF76A1EEC3C445B03DAEE8155339CD70C17658C20E43930793F00CE2FFC830DF42D76AFDEC40AE5560E8B838E4D972C01628A51F1DFC15807214C968D23A3CD131F2E1DC33D12DE44DD68F9E6FFAEB600E94A5AAC6833F06DFA28F8324124C0859DDEF71BC9E188B76107127BD69DAC73780E9676DAA07A2CE5C2CCB0E89775A4F6D777AF0B7FAB332DA435F62A64198BD84D7F9D7C65EADEAF4D7966F92BB07FC82B014753381E50DFBA1DC6943E24777A58E0F9E38037BDE2AD94355119F300C3E72E90D875C47EEC1E4BEC52EC5CC6B2EFCF1DB9DD968988D0EEB10B993962AD362EA1EB3F67E022CC20700C339C9E62E61A39DD62FF761D46F95688CD20B8781E3C359A014067D1D8E8F6E22705A36796BF5ABAF17E7A78DFAD981D8910F41ED3703A2DF82079B58069346B6636E9298190682158089234F349C8B2F8FA0F947C82478A8CF0599933A363688764A56A4396DAA804B93F4619B7E7337FE3703207E2F19CB97275CD6BB492A968C43937960134474A51FD810305C20E93CEE710C279EEC810C158D21CD148BA60C439E9FBAA438D25CD95397C0CAA62F954AB03684D398A913CDD262B397791EB1F5DCE51F4870FD9FFB488236E1FD4EDDFA1B98BB0C3B17DA23D34D5CED81667D4CF7F8D33E5D9CADF7C7D82AC331BA8287F47950011CF31927C1E7A54475212249F98F3FBBD035913513353105C2F60CAC059D8CFEC477693A9CB020B5A31A8486E2A3175FA94BF6517E1795713EA8482051E37524C29A03C767C865C2DFED529C20D25B71D193E221DF971F6A5E5DC0B663ACB1134419230381D13753323C2145A5E3CA451315F311CDCEB43AA60D21396488B384D7E7E1C39D6BDF5D9387035181E09C1286C94D435F0EDFA88404627982A329EDAB0C89A2B70CE58BC43E7ED7ECE038B5AF703B4912BDE63F68F79CEDD73D91FF65AFF9B3136BFAADB2F5E3D2F4C99DD911868488A0AEFD85E39A9E388DD46B88917E607BBCBE38C4D4CCE44E7F88141FE17F87F499F0B3C06C35273A6FEE91F8AE6C7B8F7EEF2708ED911B7EE9BE88CC936BFC41E3F0DFAC05EFD8D82764CEA64E604EA74B1860224430EA01345794062F6E818BBE07C8D009971ECB3859F836CC7FFA3D31F276032794B96E5C7EB93C0F7395A52B0F6465E7C57913403ACFF9586FDA1E11F52C969AD7463A48F9AC0B4208187E9C7F693776D937AB87E79773BD530643E975CFE199E1A9DDC3EF177D41904F09EC60BBB06637B06160AFF0D409BE68563EE37B8FBCCCC41FA246382C4EA55F0C3E04E32CEF21A967778820E9A8785D738069C9C49FC4260AECECEA4A98272FA5CEFA08BFB4B367A68794F86920FE7C5CD6579FC5492A83B47E09B5264AF57792D52E416D663F4ABF35C802A425C5658137034EB7CCAF145C301D65DFB50ED29F8993E2A63A5AC51A31A753B3D8B11F0D2617325F488E57B630859097CE28065CFB1B9C1EB36BE97E9CB051509A8B97489193EC2671D2460117018D88D308662B4A8A1716A530EE7477E9D8F84934617B7559ABD7832999AA4A6662CCDD6C5910AE22DC7403D9709F8B340A01A46386B30688140BED1D199F3C01EE74AF3FEAE6B0E65E08ECCF57FB1D3FAF5D49F67D86B75099EEC77BB25EF504EF43F6659CA7CD094CAC89B2F4B9085424A950AB939A1CCD4E60F6D7EEB4D912299A516EDA8B2BA17A0FC06EC0EC8FEEA7E31F02482D480146723DDBD6263E9BA5CA05B83459E2708E63D2D6962E694CA60EA2AD2D4628E217C47BEDC74B4EA449B21543C771598B99FA30C3C4184E58BB36BB009D6043964E59AE5E9ABC56B41DB3D8926D1ECE90F824917DAE96406E61304DD2594CA3EA7436531254A2443E8E6DCDA4BC933701DAE6F44D10B63B81C72899B68D64D39623EE70222768F253798E305B9ECEA9FC59DCB8537ACED99CE0EC6F32E41E9A0964699B67FE21BA5CFE050ED5650B9079C82E9779EEA1BB64F6C50FE1E50AB2C5A17C1F41B634E98A187322E90A1E32096CF6419382641F3609CC76074EDA1779ACB73E787292E4CAB0E30154822B3C84C65D98610D15A6B5FD819460ED7E2825E8F9BB0C0B26ACDCE2804A80F30FA9B4E7B278161128FBC0CA88225DEC879123FF00FB7AB2E41C680F214856A84C0C2375A3496C771622D32417550B837678F4A49B50760D4715AEF41A8E0F5B0B4A2E4F6BE7675717E7CD131EE4D56FEAA2D069122725607D2FE44C322B27BEEBC27DBE07A34968D30ED2BCC9C6F3D1F216D934655A70872EA964D0B3C3F0020AC994CA27B2210BEAFBA6051692239C6C71252B04DBC3C3F16D140423EDFBCE33686E1A67876FC3E29F0C4CF421A23ACE79CF7D94A809FBEE5AB782A6333F12236AD32831F628282814A190D9C140A7674B9C1337387B257EC70390EE30A644D4F9EC27AA02B34C340C02B044DFD0221AEF856C58BA1E494AD8B24C726863EB79B4FA75FDB45653469DC160F2AFDE7775A0A98AFABDA30C1E22CCBA00AD711D553704D5A1FB23743BA58B790CB09769AA8E7C796250762C714AF4077572A70D6EA7E423366C567532D8CB956E6FC86A9D746E9469673255B4FE68389E764802F8B89AD713A52888C40CC3392CAEF169E537FC074D4E410B19A8AEFE8213D2C29F4CE1ADF33C464FB0C4AF1B75FC6AE973BD717A065A362E1701CB42616FD2DFFF21758958D89B91A27AE4403153C2770832BD2BE39798D481AB28FA121B57C7E0912418D7FF536FD4CFE13FF8ABD13CA1652C104C10E190F1DF7589474DF26D17DA8E14C5267E476E603A60BBD770406FF89B556A924F9030BDCD7D8684EF6DD9A748A85C721E4D32FD1406F1D74F6DD3F35DD3F01567AEB4F0C71B1E4DFDD375BB0574FCA426CBD2395B29A485B95A8178D4E8CA5345A3A17233FC5FE5A27E7551BF3896FE194DA6BF28A32FC32F2259E4BAA7ADF67A647665E81DB5DF196B2D75A0A8DAB8A70D3A9337A277B65735A7179F4FEBB52BAC706A3BAA9A8E96AB6A3ACB95E56C90AB68339C0F7C1E7CCCEA805A074604C6E28FFB425A671B4DD3EE3E48C7A9AD5B96EE7DEEE22F403EC01F0B45B37D455DF9DB289ECBF3C645FD339D9CEF43FB4CD18BEE7DBA9EFE6F79EA86A47BD40D1CF5EC16B07C2ECF94DBDEF0BE05A64067AC8C876A3B47E93CC0DA6DDDA99AD2D6265358CAD3F2ACA04BD8C82F1B52F9E48AE86E38B855BEC11F39BAA8D5A136D0E47E34EA6940FFEEDB0EEAE82EFD69B6789A8B3FD74507FC80AA88983B17975FAF76D443EDFB5C3D0420131CE4AF2BA06261F43CDD0A7410BEF92527B68328A3560746847CFDAC8009D468600BE8E2F2E2BC5E5C31893EAE566CC4A48AE8EDAA9FEB58D130553018D80C225D2BF565482013F522BF45DCBCDBBEF863DAA926DE5AFA5F7812B19A4BA1AFDFB7BD03CDF069C46209CD07525DC084A0BC33F984E913C4817D393DBA5F8F8FBAA209C5F578D771664A57374CFC152D85E5986357BC939EBFAA9F7D6D9C5E5E5C7129EB5F610CFA9AF8C3C8A93198AC714A31E49E24A6BDD6FE102370F78D2A72302FEE869208D75207615A4CF54C4D177D38BDD3E0F2ACBD428777475A31CDE322F3C956C2EF2844C79AF73EE31FC0269E76D4533812D61AC750FD24CB598140A3FE44F909CE66EA3F620FD607513A975FBF42D737EA5FB9A77EAFD0FDDD49B19DB7AD2AE11AC01F5D871590DE7EDFF720304791ADC740542DBCB73BA46B6EEA8C74D7DF549704D164A92E09AA4B82EA92E0952F09C85E50F9EB2A7F9D5240FB54FEBA422356F9EB2A7F1DE55DF9EBDE74CF57FEBACA5F57F9EB2A7F5DE5AFE306A0F2D755FEBA3207E17DF9EB1C6B56F9EB2A7F5DE5AF4B0D59E5AF7B657FDDBD459F3F44EF4A2B7D144D804A1F55FAA8D247AFAB8FC04E0D136114B945A85451A58A2A5554A9A2325491B6A45F050F73C4EBEE264AB85129A66026548AA9524C95623A8E621A99C68FF52AFC1C78A59B2ADD54E9A6D49055BAE9B5E3BF82DBE59D8CA62A26AC704C18F4ACE15F6BBAADB416C826F94E4849215576E8C03153B7BF1820C66F73D35B2077897FEEA2CFDE6D3859DF79342D1CF314FC234BEF6D17697651FF4AFEBBC2498DAB48B32AD2EC4DDDB9569166C7EAF92AD2ECC82350459A1DAFEFAB48B363F47A156976F421A822CDDEC020BCAF48B3600FDED5577879BAE391BCB493F8B0DBD55A9DEA315650F8014ECFFB1D9171B395AF97E453ECD5F9F88D9D8F7B9D4236530F3DC14CEA80990487B58F622F45EBB1A6DEB4DA9DEEED9DF6FB11B6EDDB9E68597343706B398F300638DDF0C739254723709C134365B05606EB1B18847765B08697DBC3F918198E3BABEEB3A31953DD6757F7D9D57D76158F5CA9A278FE54AAA852457F135554A556AABC79C5B5CF3BF5E655A995DEB643AF0A78A9025E8ED2F355C0CB9147A00A78395EDF57012FC7E8F52AE0E5E84350DD1FBC81417857F707552AF4CA5F57F9EB044356F9EB5ED95F77E3D8B3E05996541385CFB6460BD35246D05CAC31046FB9E8725801D4971581FAED4FD3433E32164F246E43B268C219F3F5ECF2F4027438FEC06738635EDF7730198C0A9DA290013DA7BB1B65E08019B9510265FE9EB7D0D3AB5A7DF2FD481BA8B04EF411F3916EFC083EF01B572B783A8241FFEB27BC4F5830E93EC55C190C56E61BD3B282968CFAA3DA59EDAC01BA8A294DC14A34F2DDC34D9E8EB873D61E521EF4CD239091EB0A0193666BEDF9CED21BAEF1537A6887770DAB832F6430341BAF695CFC4FD860013A59C0404287FCC0DE22F23753DED7DD1F9E6ACF068E875F0B32BFD8551B331090C645B21D4CA8E1132A55F7327CBA02D2CDF0ABCF93CDF2D1C18EEC54414291600268A6F968295420948314805627A753A24C3467A7AE6E7B3864A4879E0A4E5A1625BD750EE7B051180BDD7D1275F0FD6474D793EEA25889EB3364819AD625FBA8987CC0B9E7E83368A96821ECE5DF16516E42DB87EE0C96229EA5E1BFD99DCCF0D7BAA582CDFAACE3614FFEE600DB68055A7AEDA208342E61814DD770F5B92F99C05C13819818A379E3383FB2EC8B9130EC31B11846966E832197A135243C9A2D1DFA217DD664B7F80040B6EFB36790FC0FA5E71E3D76397C888E1F5B7F2E3D3C7C483E989E3A7A14FE64BAE4F091B36717DBB1454710457E4D27397928DB7D403DF3F411364BFC11F5D49CDDED33EA59C711D181A4D8A7D42303933F86880E22F1F0893FA81E8E60F14FAA0B4E27ECF924E7B3EA39C794EC83CA364795ECC38AF8B822FDC03A99C19CFA093552A0B1C6688E5C641B44A34A6A78E409729F4D03F5D033B2D2AB51AC9CB3119B611290D61AF6A039104815B0071C8FE6A19D853B4CA3D6687CAE9DC1FFD35AED17F23FACCE34948842BCF144344E4534841B54B7D5539F4105E88FE4220D2FFC544912386A5AFCEF24C01819088B0A9A6E89FBDDA3B07C71126DE23BAEFE842870F88335453DE8FB2767685B9B6BDF5D83AA674BD87D9E6B50BA24091C35A8276A504FDC206171122D68439B5E60A70A9276091CFB28D5F09FC96A1A9767994F0B9FDA2E9CC96A2CD06C6DE5CC250E4A4421672E09E0182A919DD9273AD50405912C6260BFC30E8BAC1EE8F699A7F587D0FA7409074C9461089736FCBEDFF6A3918CFFCD02381BE87DDA8D03E75A556BB54B004D96C696B9C0664E16A6EC797A4EC06B1D7E364FA20013A6F4FF00, N'00000000000000000001', NULL, N'')
";
			}
		}
	}
}
