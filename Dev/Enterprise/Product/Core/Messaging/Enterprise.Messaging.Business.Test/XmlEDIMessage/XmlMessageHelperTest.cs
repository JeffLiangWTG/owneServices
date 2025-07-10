using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.XmlMessaging.Testing
{
	public class XmlMessageHelperTest : TestCaseWithFactory
	{
		public void TestGetMessageSchemaName()
		{
			AssertEquals("AgencyBillsOfLading", EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.AgencyBillsOfLading)));
			AssertEquals("BankStatements", EDIMessageSchemaNameList.Descriptions.BankStatements, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.BankStatements)));
			AssertEquals("Consols", EDIMessageSchemaNameList.Descriptions.Consols, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Consols)));
			AssertEquals("ContainerMovements", EDIMessageSchemaNameList.Descriptions.ContainerMovements, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.ContainerMovements)));
			AssertEquals("Events", EDIMessageSchemaNameList.Descriptions.Events, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Events)));
			AssertEquals("Organizations", EDIMessageSchemaNameList.Descriptions.Organizations, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Organizations)));
			AssertEquals("FinancialTransactions", EDIMessageSchemaNameList.Descriptions.FinancialTransactions, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.FinancialTransactions)));
			AssertEquals("ISFs", EDIMessageSchemaNameList.Descriptions.ISFs, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.ISFs)));
			AssertEquals("Orders", EDIMessageSchemaNameList.Descriptions.Orders, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Orders)));
			AssertEquals("Invoices", EDIMessageSchemaNameList.Descriptions.Invoices, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Invoices)));
			AssertEquals("Products", EDIMessageSchemaNameList.Descriptions.Products, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Products)));
			AssertEquals("Schedules", EDIMessageSchemaNameList.Descriptions.Schedules, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Schedules)));
			AssertEquals("Shipments", EDIMessageSchemaNameList.Descriptions.Shipments, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.Shipments)));
			AssertEquals("ShipmentBookings", EDIMessageSchemaNameList.Descriptions.ShipmentBookings, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.ShipmentBookings)));
			AssertEquals("WhsDockets", EDIMessageSchemaNameList.Descriptions.WhsDockets, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.WhsDockets)));
			AssertEquals("DocumentMessages", EDIMessageSchemaNameList.Descriptions.DocumentMessages, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, EDIMessageSubTypeList.Codes.DocumentMessages)));

			AssertEquals("FHL", EDIMessageSchemaNameList.Descriptions.FHL, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.CIM, "", EDIMessageTypeList.Codes.FHL)));
			AssertEquals("FWB", EDIMessageSchemaNameList.Descriptions.FWB, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.CIM, "", EDIMessageTypeList.Codes.FWB)));

			AssertEquals("Empty", String.Empty, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.XMS, "Bla")));
			AssertEquals("Empty", String.Empty, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.CIM, "", "Bla")));

			AssertEquals("AU", "AU", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.USeManifest, "", "AU")));
			AssertEquals("AU", "AU", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.USAMS, "", "AU")));
			AssertEquals("AU", "AU", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.USAMA, "", "AU")));
			AssertEquals("UA", "UA", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.USCustomsExport, "", "UA")));
			AssertEquals("AE", "AE", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.USCustomsImport, "", "AE")));
			AssertEquals("AU", "AU", XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.StowPlan, "", "AU")));

			AssertEquals("Universal Data", EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.UniversalDataMessaging, EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessageTypeList.Codes.XDC)));
			AssertEquals("Native Data", EDIMessageSchemaNameList.Descriptions.NativeDataMessaging, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.NativeDataMessaging, EDIMessageSubTypeList.Codes.XmlNativeOrganization, EDIMessageTypeList.Codes.XDC)));
			AssertEquals("VersionReport", EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.UniversalDataMessaging, EDIMessageSubTypeList.Codes.VersionReport, EDIMessageTypeList.Codes.XDC)));
			AssertEquals("Inttra", EDIMessageSchemaNameList.Descriptions.InttraEdifact, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.Inttra, "FOO")));

			AssertEquals("NZCustoms", EDIMessageSchemaNameList.Descriptions.NZCustoms, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.NZCustoms, EDIMessageSubTypeList.Codes.DocumentMessages)));
			AssertEquals("NZMAFeBACCa", EDIMessageSchemaNameList.Descriptions.NZCustoms, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.NZMAFeBACCa, EDIMessageSubTypeList.Codes.DocumentMessages)));

			AssertEquals("AUCustoms", EDIMessageSchemaNameList.Descriptions.AUCustoms, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.AUCMR, EDIMessageSubTypeList.Codes.DocumentMessages)));

			AssertEquals("ChinaInterface", EDIMessageSchemaNameList.Descriptions.ChinaInterface, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.ChinaInterfaceMapping, EDIMessageSubTypeList.Codes.ChinaInterface)));

			AssertEquals("TR Customs Declaration", EDIInterchangeTypeList.Descriptions.TRCustomsGlobalManifest, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.TRCustoms, "FOO")));
			AssertEquals("TR Customs E-Trade", EDIInterchangeTypeList.Descriptions.TRCustomsETradeRegistration, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.TRETrade, "FOO")));

			AssertEquals("Air Cargo Advance Screening", EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging, XmlMessageHelper.GetMessageSchemaName(CreateInterchangeWithData(ApplicationCodeList.Codes.AirCargoAdvanceScreening, EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessageTypeList.Codes.XDC)));
		}

		public void TestGetMessageSubTypeByNodeName()
		{
			CombineAssertions(delegate
			{
				AssertEquals("AgencyBillsOfLading", EDIMessageSubTypeList.Codes.AgencyBillsOfLading, XmlMessageHelper.GetMessageSubType("AgencyBillsOfLading"));
				AssertEquals("BankStatements", EDIMessageSubTypeList.Codes.BankStatements, XmlMessageHelper.GetMessageSubType("BankStatements"));
				AssertEquals("Consols", EDIMessageSubTypeList.Codes.Consols, XmlMessageHelper.GetMessageSubType("Consols"));
				AssertEquals("Declaration", EDIMessageSubTypeList.Codes.Brokerage, XmlMessageHelper.GetMessageSubType("Consols", "Declaration"));
				AssertEquals("CFSLoadList", EDIMessageSubTypeList.Codes.CFSLoadList, XmlMessageHelper.GetMessageSubType("Consols", "CFSLoadList"));
				AssertEquals("ContainerMovements", EDIMessageSubTypeList.Codes.ContainerMovements, XmlMessageHelper.GetMessageSubType("ContainerMovements"));
				AssertEquals("DocumentMessages", EDIMessageSubTypeList.Codes.DocumentMessages, XmlMessageHelper.GetMessageSubType("DocumentMessages"));
				AssertEquals("Events", EDIMessageSubTypeList.Codes.Events, XmlMessageHelper.GetMessageSubType("Events"));
				AssertEquals("Organizations", EDIMessageSubTypeList.Codes.Organizations, XmlMessageHelper.GetMessageSubType("Organisations"));
				AssertEquals("FinancialTransactions", EDIMessageSubTypeList.Codes.FinancialTransactions, XmlMessageHelper.GetMessageSubType("FinancialTransactions"));
				AssertEquals("ISFs", EDIMessageSubTypeList.Codes.ISFs, XmlMessageHelper.GetMessageSubType("ISFs"));
				AssertEquals("Orders", EDIMessageSubTypeList.Codes.Orders, XmlMessageHelper.GetMessageSubType("Orders"));
				AssertEquals("Invoices", EDIMessageSubTypeList.Codes.Invoices, XmlMessageHelper.GetMessageSubType("Invoices"));
				AssertEquals("Products", EDIMessageSubTypeList.Codes.Products, XmlMessageHelper.GetMessageSubType("Products"));
				AssertEquals("Schedules", EDIMessageSubTypeList.Codes.Schedules, XmlMessageHelper.GetMessageSubType("Schedules"));
				AssertEquals("Shipments", EDIMessageSubTypeList.Codes.Shipments, XmlMessageHelper.GetMessageSubType("Shipments"));
				AssertEquals("ShipmentBookings", EDIMessageSubTypeList.Codes.ShipmentBookings, XmlMessageHelper.GetMessageSubType("ShipmentBookings"));
				AssertEquals("WhsDockets", EDIMessageSubTypeList.Codes.WhsDockets, XmlMessageHelper.GetMessageSubType("WhsDockets"));
				AssertEquals("LocalCartageStatus", EDIMessageSubTypeList.Codes.LocalCartageStatus, XmlMessageHelper.GetMessageSubType("CartageJobs", "LocalCartageStatus"));
				AssertEquals("LocalCartageBooking", EDIMessageSubTypeList.Codes.LocalCartageBooking, XmlMessageHelper.GetMessageSubType("CartageJobs"));
				AssertEquals("UniversalShipment", EDIMessageSubTypeList.Codes.XmlUniversalShipment, XmlMessageHelper.GetMessageSubType("UniversalShipment"));
				AssertEquals("UniversalEvent", EDIMessageSubTypeList.Codes.XmlUniversalEvent, XmlMessageHelper.GetMessageSubType("UniversalEvent"));
				AssertEquals("UniversalSchedule", EDIMessageSubTypeList.Codes.XmlUniversalSchedule, XmlMessageHelper.GetMessageSubType("UniversalSchedule"));
				AssertEquals("UniversalTransaction", EDIMessageSubTypeList.Codes.XmlUniversalTransaction, XmlMessageHelper.GetMessageSubType("UniversalTransaction"));
				AssertEquals("UniversalTransactionBatch", EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch, XmlMessageHelper.GetMessageSubType("UniversalTransactionBatch"));

				AssertEquals("Universal Organization", EDIMessageSubTypeList.Codes.XmlNativeOrganization, XmlMessageHelper.GetMessageSubType("ReferenceData", "Organization"));

				AssertEquals("Native Shipment", EDIMessageSubTypeList.Codes.XmlNativeShipment, XmlMessageHelper.GetMessageSubType("Native", "Shipment"));
				AssertEquals("Native UNLOCO", EDIMessageSubTypeList.Codes.XmlNativeUNLOCO, XmlMessageHelper.GetMessageSubType("Native", "UNLOCO"));
				AssertEquals("Native Vessel", EDIMessageSubTypeList.Codes.XmlNativeVessel, XmlMessageHelper.GetMessageSubType("Native", "Vessel"));
				AssertEquals("Native Airline", EDIMessageSubTypeList.Codes.XmlNativeAirline, XmlMessageHelper.GetMessageSubType("Native", "Airline"));
				AssertEquals("Native CommodityCode", EDIMessageSubTypeList.Codes.XmlNativeCommodityCode, XmlMessageHelper.GetMessageSubType("Native", "CommodityCode"));
				AssertEquals("Native Container", EDIMessageSubTypeList.Codes.XmlNativeContainer, XmlMessageHelper.GetMessageSubType("Native", "Container"));
				AssertEquals("Native Country", EDIMessageSubTypeList.Codes.XmlNativeCountry, XmlMessageHelper.GetMessageSubType("Native", "Country"));
				AssertEquals("Native CurrencyExchangeRate", EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate, XmlMessageHelper.GetMessageSubType("Native", "CurrencyExchangeRate"));
				AssertEquals("Native DangerousGood", EDIMessageSubTypeList.Codes.XmlNativeDangerousGood, XmlMessageHelper.GetMessageSubType("Native", "DangerousGood"));
				AssertEquals("Native Declaration", EDIMessageSubTypeList.Codes.XmlNativeDeclaration, XmlMessageHelper.GetMessageSubType("Native", "Declaration"));
				AssertEquals("Native Order", EDIMessageSubTypeList.Codes.XmlNativeOrder, XmlMessageHelper.GetMessageSubType("Native", "Order"));
				AssertEquals("Native Organization", EDIMessageSubTypeList.Codes.XmlNativeOrganization, XmlMessageHelper.GetMessageSubType("Native", "Organization"));
				AssertEquals("Native EDI Code Mapping", EDIMessageSubTypeList.Codes.XmlNativeEDICodeMapping, XmlMessageHelper.GetMessageSubType("Native", "EDICodeMapping"));
				AssertEquals("Native Product", EDIMessageSubTypeList.Codes.XmlNativeProduct, XmlMessageHelper.GetMessageSubType("Native", "Product"));
				AssertEquals("Native Product", EDIMessageSubTypeList.Codes.XmlNativeRate, XmlMessageHelper.GetMessageSubType("Native", "Rate"));
				AssertEquals("Native CarrierAccount", EDIMessageSubTypeList.Codes.XmlNativeCarrierAccount, XmlMessageHelper.GetMessageSubType("Native", "CarrierAccount"));
				AssertEquals("Native SalesChannel", EDIMessageSubTypeList.Codes.XmlNativeSalesChannel, XmlMessageHelper.GetMessageSubType("Native", "SalesChannel"));
				AssertEquals("Native ServiceLevel", EDIMessageSubTypeList.Codes.XmlNativeServiceLevel, XmlMessageHelper.GetMessageSubType("Native", "ServiceLevel"));
				AssertEquals("Native WarehouseClientAccountAssociation", EDIMessageSubTypeList.Codes.XMLNativeWarehouseClientAccountAssociation, XmlMessageHelper.GetMessageSubType("Native", "WarehouseClientAccountAssociation"));
				AssertEquals("Native CusStatement", EDIMessageSubTypeList.Codes.XmlNativeCusStatement, XmlMessageHelper.GetMessageSubType("Native", "CusStatement"));
				AssertEquals("Native BMSystem", EDIMessageSubTypeList.Codes.XmlNativeBMSystem, XmlMessageHelper.GetMessageSubType("Native", "BMSystem"));
				AssertEquals("Native Tag", EDIMessageSubTypeList.Codes.XmlNativeTag, XmlMessageHelper.GetMessageSubType("Native", "Tag"));
				AssertEquals("Native Putaway Group", EDIMessageSubTypeList.Codes.XmlNativePutawayGroup, XmlMessageHelper.GetMessageSubType("Native", "PutawayGroup"));
				AssertEquals("Native Transport Zone", EDIMessageSubTypeList.Codes.XmlNativeTransportZone, XmlMessageHelper.GetMessageSubType("Native", "TransportZone"));
				AssertEquals("Native DangerousGoods Country Reference Mapping", EDIMessageSubTypeList.Codes.XmlNativeDangerousGoodsCountryReferenceMapping, XmlMessageHelper.GetMessageSubType("Native", "DangerousGoodsCountryReferenceMapping"));

				AssertEquals("VersionReport", EDIMessageSubTypeList.Codes.VersionReport, XmlMessageHelper.GetMessageSubType("VersionReport"));

				AssertEquals("Unknown", EDIMessageSubTypeList.Codes.Unknown, XmlMessageHelper.GetMessageSubType("BlaBla"));

				foreach (ICodeDescription nativeDataType in new NativeDataTypeList())
				{
					AssertNotEquals("Native " + nativeDataType.Code + " should be handled by GetMessageSubType.", EDIMessageSubTypeList.Codes.Unknown, XmlMessageHelper.GetMessageSubType("Native", nativeDataType.Code));
				}
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetMessageSubTypeFromXml()
		{
			AssertEquals("AgencyBillsOfLading", EDIMessageSubTypeList.Codes.AgencyBillsOfLading, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.AgencyBillsOfLading)));
			AssertEquals("BankStatements", EDIMessageSubTypeList.Codes.BankStatements, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.BankStatements)));
			AssertEquals("Consols", EDIMessageSubTypeList.Codes.Consols, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Consols)));
			AssertEquals("ContainerMovements", EDIMessageSubTypeList.Codes.ContainerMovements, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.ContainerMovements)));
			AssertEquals("Events", EDIMessageSubTypeList.Codes.Events, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Events)));
			AssertEquals("Organizations", EDIMessageSubTypeList.Codes.Organizations, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Organizations)));
			AssertEquals("FinancialTransactions", EDIMessageSubTypeList.Codes.FinancialTransactions, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.FinancialTransactions)));
			AssertEquals("ISFs", EDIMessageSubTypeList.Codes.ISFs, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.ISFs)));
			AssertEquals("Orders", EDIMessageSubTypeList.Codes.Orders, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Orders)));
			AssertEquals("Invoices", EDIMessageSubTypeList.Codes.Invoices, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Invoices)));
			AssertEquals("Products", EDIMessageSubTypeList.Codes.Products, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Products)));
			AssertEquals("Schedules", EDIMessageSubTypeList.Codes.Schedules, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Schedules)));
			AssertEquals("Shipments", EDIMessageSubTypeList.Codes.Shipments, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Shipments)));
			AssertEquals("ShipmentBookings", EDIMessageSubTypeList.Codes.ShipmentBookings, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.ShipmentBookings)));
			AssertEquals("WhsDockets", EDIMessageSubTypeList.Codes.WhsDockets, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.WhsDockets)));
			AssertEquals("LocalCartageBooking", EDIMessageSubTypeList.Codes.LocalCartageBooking, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.CartageJobs)));
			AssertEquals("Unknown", EDIMessageSubTypeList.Codes.Unknown, XmlMessageHelper.GetMessageSubTypeFromXml(GetMesageStream(EDIMessageSubTypeList.Codes.Unknown)));
		}

		public void TestShouldReturnValidSubTypeWhenNodeNameIsRates()
		{
			var resultingSubType = XmlMessageHelper.GetMessageSubType("Rates");
			AssertEquals(EDIMessageSubTypeList.Codes.Rates, resultingSubType);
		}

		public void TestMessageSubTypeMapping()
		{
			var xmlList = new EDIMessageSubTypeXMLElementList();
			var displayList = new EDIMessageSubTypeList();
			var errorMessage = "Lists EDIMessageSubTypeXMLElementList and EDIMessageSubTypeList should be in sync. EDIMessageSubTypeXMLElementList is used for XML element names and EDIMessageSubTypeList is translatable display values. If you add a new code, please add it to both lists";

			AssertEquals(errorMessage, xmlList.Count, displayList.Count);

			foreach (var xmlListItem in xmlList)
			{
				Assert(errorMessage, displayList.ContainsCode(xmlListItem));
			}
		}

		EDIInterchange CreateInterchangeWithData(string interchangeType, string messageSubType, string messageType = "")
		{
			EDIInterchange interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = interchangeType;
			interchange.ContainedMessages.RemoveAll();
			EDIMessage message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageType = messageType;
			interchange.ContainedMessages.Add(message);

			return interchange;
		}

		public static Stream GetMesageStream(string messageSubTypeCode)
		{
			var content = string.Empty;

			switch (messageSubTypeCode)
			{
				case EDIMessageSubTypeList.Codes.AgencyBillsOfLading:
					content = GetXmlString("AgencyBillsOfLading");
					break;
				case EDIMessageSubTypeList.Codes.BankStatements:
					content = GetXmlString("BankStatements");
					break;
				case EDIMessageSubTypeList.Codes.Consols:
					content = GetXmlString("Consols");
					break;
				case EDIMessageSubTypeList.Codes.ContainerMovements:
					content = GetXmlString("ContainerMovements");
					break;
				case EDIMessageSubTypeList.Codes.Events:
					content = GetXmlString("Events");
					break;
				case EDIMessageSubTypeList.Codes.Organizations:
					content = GetXmlString("Organizations");
					break;
				case EDIMessageSubTypeList.Codes.FinancialTransactions:
					content = GetXmlString("FinancialTransactions");
					break;
				case EDIMessageSubTypeList.Codes.ISFs:
					content = GetXmlString("ISFs");
					break;
				case EDIMessageSubTypeList.Codes.Orders:
					content = GetXmlString("Orders");
					break;
				case EDIMessageSubTypeList.Codes.Invoices:
					content = GetXmlString("Invoices");
					break;
				case EDIMessageSubTypeList.Codes.Products:
					content = GetXmlString("Products");
					break;
				case EDIMessageSubTypeList.Codes.Schedules:
					content = GetXmlString("Schedules");
					break;
				case EDIMessageSubTypeList.Codes.Shipments:
					content = GetXmlString("Shipments");
					break;
				case EDIMessageSubTypeList.Codes.ShipmentBookings:
					content = GetXmlString("ShipmentBookings");
					break;
				case EDIMessageSubTypeList.Codes.WhsDockets:
					content = GetXmlString("WhsDockets");
					break;
				case EDIMessageSubTypeList.Codes.CartageJobs:
					content = GetXmlString("CartageJobs");
					break;
				case EDIMessageSubTypeList.Codes.Unknown:
					content = @"Non-comformant format -- should generate MessageType = ""UNK""";
					break;
			}

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(content);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		static string GetXmlString(string fileName)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(XmlMessageHelperTest).Assembly))
			{
				var testFilePath = resourceRetriever.SaveResourceToFile(string.Format(@"{0}.xml", fileName));
				return File.ReadAllText(testFilePath);
			}
		}
	}
}
