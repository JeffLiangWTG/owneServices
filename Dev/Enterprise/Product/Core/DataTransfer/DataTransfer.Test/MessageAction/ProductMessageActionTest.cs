using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DataTransfer.Testing
{
	sealed class ProductMessageActionTest : ImportMessageActionTest
	{
		public void TestRunTask_DoesNotRaiseArithmeticOverflow()
		{
			var messageBody = GetMessageBody();
			int noOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Products;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "00000000000000000001";

			string bigOutOfSqlRangeDecimal = Decimal.MaxValue.ToString();
			string outOfRangeDecimalsMessage = messageBody.Replace("1.100</GrossWeight>", string.Format("{0}</GrossWeight>", bigOutOfSqlRangeDecimal))
				.Replace("1.000</NetWeight>", string.Format("{0}</NetWeight>", bigOutOfSqlRangeDecimal));

			message.EM_MessageText = outOfRangeDecimalsMessage;
			Factory.Save();

			var processor = new StandardXMLMessageServiceTask();
			processor.ServiceLogger = new DummyLogger();
			processor.RunTask();
			AssertEquals(noOfProducts + 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
		}

		public void TestExecuteAction_ImportProduct()
		{
			var messageBody = GetMessageBody();
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			int noOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

			var buffer = new NotificationBuffer();
			var message = CreateMessage(Factory);
			message.EM_MessageText = messageBody;

			message.Interchange.EI_HeaderText = interchangeHeader;
			message.Interchange.EI_FooterText = interchangeFooter;
			message.Interchange.EI_From = "BLAH";
			message.Interchange.EI_From = "BLAH BLAH";

			var action = new ProductMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));

			Factory.Save();
			AssertEquals(noOfProducts + 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
		}

		public void TestExecuteAction_ImportProductForUSCompany()
		{
			var messageBody = GetMessageBody();
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var buffer = new NotificationBuffer();
			var message = CreateMessage(Factory);
			message.EM_MessageText = messageBody;

			message.Interchange.EI_HeaderText = interchangeHeader;
			message.Interchange.EI_FooterText = interchangeFooter;
			message.Interchange.EI_From = "BLAH";
			message.Interchange.EI_From = "BLAH BLAH";

			var action = new ProductMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			var companyCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			Assert(action.ExecuteAction(message, buffer, out forSave));

			Factory.Save();
			bool foundUSBusinessOrgSupplierPart = false;
			var temp = Factory as IBusinessObjectFactoryInternals;
			var temp2 = temp.AllBusinessObjects;
			foreach (var each in temp2)
			{
				if (each.GetType().FullName == "Enterprise.Customs.US.Business.OrgSupplierPart")
				{
					foundUSBusinessOrgSupplierPart = true;
				}
			}
			Assert(foundUSBusinessOrgSupplierPart);
		}
		#region XML
		const string interchangeHeader = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-02-08T13:25:12.293+11:00</Date><XmlType>Verbose</XmlType><Source><EnterpriseCode>HYE</EnterpriseCode><CompanyCode>DAU</CompanyCode><OriginServer>KSD</OriginServer><LoginName>EDISupport</LoginName></Source><Target /><EDIOrganisation EDICode=""CAREDIBNE"" OwnerCode=""CAREDIBNE""><OrganisationDetails><Name>CARGOWISE EDI</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressCode>Pick Up Address</AddressCode><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""false"" AddressType=""PIC"" /></AddressCapabilities></Address><Address><AddressLine1>PO BOX 6400</AddressLine1><AddressCode>PO BOX 6400</AddressCode><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9000 4567</TelephoneNumber></TelephoneNumbers><Language>EN</Language><CompanyName>CARGOWISE EDI PTY LTD</CompanyName><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""true"" AddressType=""PST"" /></AddressCapabilities></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>UNC</NumberType><Number>JASUNC1</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>UOC</NumberType><Number>JASUOC1</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ABN</NumberType><Number>14 001 592 650</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>14 001 592 650</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><Products>";
		const string interchangeFooter = @"</Products></Payload></XmlInterchange>";

		//XML message must specify a valid warehouse in Client Warehouse Detail.
		string GetMessageBody()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("New Warehouse", "A");
			warehouse.WW_GB_RelatedCompanyBranch = Env.CurrentBranchPK;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			warehouse.WW_OA_WarehouseAddress = address.PK;

			return $@"<Product><ProductCode>PRODUCT1</ProductCode><ProductDescription>PRODUCT DESCRIPTION 1</ProductDescription><StockUnit>UNT</StockUnit><RelatedOrganisations><RelatedOrganisation><Organisation EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>222 BRISBANE CRESCENT</AddressLine1><AddressCode>BRISBANE CRESCENT</AddressCode><CityOrSuburb>BRISBANE</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4000</PostCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address><Address><AddressLine1>111 WAREHOUSE STREET</AddressLine1><AddressCode>C34</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Sequence>3</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""true"" AddressType=""DLV"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""true"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>                    Port=DESTR, vip=VIP TEST                    hallo                  </NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>                    Kirsten Test Data                    asdflasldf                  </NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></Organisation><RelationshipType>OWN</RelationshipType><RFAttributeConfirm>NON</RFAttributeConfirm></RelatedOrganisation></RelatedOrganisations><DimensionDetails><DimensionUnit>CM</DimensionUnit><GrossWeight DimensionType=""KG"">1.100</GrossWeight><NetWeight>1.000</NetWeight><Volume DimensionType=""M3"">0.100</Volume></DimensionDetails><UnitConversions><UnitConversion><Package DimensionType=""PLT"">10.000000</Package><ParentUQ>UNT</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""UNT"">0.100000</Package><ParentUQ>M3</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""KG"">1.100000</Package><ParentUQ>UNT</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""M3"">0.100000</Package><ParentUQ>UNT</ParentUQ></UnitConversion></UnitConversions><BillOfMaterials><BillOfMaterial><AllowResale>true</AllowResale><AllowDisassemblyOfKit>true</AllowDisassemblyOfKit><AllowAutoReplenishKit>true</AllowAutoReplenishKit></BillOfMaterial></BillOfMaterials><ClientDefinedDetails /><BasicStockControl /><ClientWarehouseDetails><ClientWarehouseDetail><Client EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>222 BRISBANE CRESCENT</AddressLine1><AddressCode>BRISBANE CRESCENT</AddressCode><CityOrSuburb>BRISBANE</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4000</PostCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address><Address><AddressLine1>111 WAREHOUSE STREET</AddressLine1><AddressCode>C34</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Sequence>3</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""true"" AddressType=""DLV"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""true"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>                    Port=DESTR, vip=VIP TEST                    hallo                  </NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>                    Kirsten Test Data                    asdflasldf                  </NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></Client><WarehouseCode>{warehouse.PK}</WarehouseCode></ClientWarehouseDetail></ClientWarehouseDetails></Product>";
		}

		#endregion
	}
}
