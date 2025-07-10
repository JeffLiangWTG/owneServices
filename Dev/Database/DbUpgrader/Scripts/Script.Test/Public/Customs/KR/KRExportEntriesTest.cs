using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	[TestedType(typeof(KRExportEntries))]
	class KRExportEntriesTest : DbCreateScriptTest
	{
		public void TestJobDeclarationData()
		{
			var declarationAddInfoDics = new Dictionary<string, string>();
			declarationAddInfoDics.Add("ExporterType", "A");
			declarationAddInfoDics.Add("CustomsDivision", "20");
			declarationAddInfoDics.Add("ReturnReason", "12");
			declarationAddInfoDics.Add("SimpleDRWApp", "AD");
			declarationAddInfoDics.Add("ProcedureType", "M");
			declarationAddInfoDics.Add("ContainerPackMode", "BU");
			declarationAddInfoDics.Add("LocationIDInBondedArea", "1234567890");

			var declarationItems = GetItemList(new List<string> { "JE_CustomsOffice", "JE_ExportGoodsType", "JE_MessageSubType", "JE_GoodsDestination",
																	"JE_RL_NKPortOfLoading", "JE_IATALoadPort", "JE_TotalNoOfPacksPackType", "JE_ExportDate", "JE_EntryDate", "JE_AddInfo" },
												 new List<object> { "010", "11", "B", "US",
																	"KRSEL", "CHF", "CNT", new DateTime(2023, 1, 11), new DateTime(2023, 1, 20), GenerateAddInfoData(declarationAddInfoDics) });

			var declarationPK = CreateJobDeclaration(11, "EXP", branchPK, companyPK, declarationItems);

			var entryPK = CreateCusEntryHeader(declarationPK, 11);
			var entryLinePK = CreateCusEntryLine(entryPK, 11);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 11);
			CreateJobComInvoiceLine(invoiceHeaderPK, 11, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("A", reader["ExporterType"]);
						AssertEquals("010", reader["CustomsOffice"]);
						AssertEquals("20", reader["Department"]);
						AssertEquals("11", reader["ExportGoodsType"]);
						AssertEquals("B", reader["ExportType"]);
						AssertEquals("US", reader["CountryofDestination"]);
						AssertEquals("KRSEL", reader["PortofLoading"]);
						AssertEquals("CHF", reader["IATA"]);
						AssertEquals("12", reader["ReturnReason"]);
						AssertEquals("AD", reader["AutoDrawback"]);
						AssertEquals("CNT", reader["PackUQ"]);
						AssertEquals(new DateTime(2023, 1, 11), (DateTime)reader["DepartureDate"]);
						AssertEquals("M", reader["ProcedureType"]);
						AssertEquals("BU", reader["ContainerPack"]);
						AssertEquals(new DateTime(2023, 1, 20), (DateTime)reader["ActualDateOfLoading"]);
						AssertEquals("1234567890", reader["LocationIDInBondedArea"]);
					});
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("A", reader["ExporterType"]);
						AssertEquals("010", reader["CustomsOffice"]);
						AssertEquals("20", reader["Department"]);
						AssertEquals("11", reader["ExportGoodsType"]);
						AssertEquals("B", reader["ExportType"]);
						AssertEquals("US", reader["CountryofDestination"]);
						AssertEquals("KRSEL", reader["PortofLoading"]);
						AssertEquals("CHF", reader["IATA"]);
						AssertEquals("12", reader["ReturnReason"]);
						AssertEquals("AD", reader["AutoDrawback"]);
						AssertEquals("CNT", reader["PackUQ"]);
						AssertEquals(new DateTime(2023, 1, 11), (DateTime)reader["DepartureDate"]);
						AssertEquals("M", reader["ProcedureType"]);
						AssertEquals("BU", reader["ContainerPack"]);
						AssertEquals("1234567890", reader["LocationIDInBondedArea"]);
					});
				}
			}
		}

		public void TestJobDeclarationData_LocationOfGoods()
		{
			var declarationItems = GetItemList(new List<string> { "JE_LocationQualifier", "JE_SubLocationOfGoods", "JE_LocationOfGoods", "JE_LocationOtherInformation" },
												 new List<object> { "46767", "Name", "Address", "12312" });

			var declarationPK = CreateJobDeclaration(12, "EXP", branchPK, companyPK, declarationItems);

			var entryPK = CreateCusEntryHeader(declarationPK, 12);
			var entryLinePK = CreateCusEntryLine(entryPK, 12);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 12);
			CreateJobComInvoiceLine(invoiceHeaderPK, 12, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("46767", reader["GoodsLocPostCode"]);
						AssertEquals("Name", reader["GoodsLocDetails"]);
						AssertEquals("Address", reader["GoodsLocAddress"]);
						AssertEquals("12312", reader["BondedAreaCode"]);
					});
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("46767", reader["GoodsLocPostCode"]);
						AssertEquals("Name", reader["GoodsLocDetails"]);
						AssertEquals("Address", reader["GoodsLocAddress"]);
						AssertEquals("12312", reader["BondedAreaCode"]);
					});
				}
			}
		}

		public void TestJobDeclarationData_TransportDetails()
		{
			var declarationItems = GetItemList(new List<string> { "JE_TransportMode", "JE_VoyageFlightNo", "JE_VesselName" },
												 new List<object> { "AIR", "KRC1131", "VesselName" });

			var declarationPK = CreateJobDeclaration(13, "EXP", branchPK, companyPK, declarationItems);

			var entryPK = CreateCusEntryHeader(declarationPK, 13);
			var entryLinePK = CreateCusEntryLine(entryPK, 13);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 13);
			CreateJobComInvoiceLine(invoiceHeaderPK, 13, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("AIR", reader["TransportMode"]);
						AssertEquals("KRC1131", reader["VesselOrFlightNo"]);
					});
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("AIR", reader["TransportMode"]);
						AssertEquals("KRC1131", reader["VesselOrFlightNo"]);
					});
				}
			}

			var sql = @"
UPDATE dbo.JobDeclaration
SET
	JE_TransportMode = 'SEA',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK;";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("SEA", reader["TransportMode"]);
					AssertEquals("VesselName", reader["VesselOrFlightNo"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("SEA", reader["TransportMode"]);
					AssertEquals("VesselName", reader["VesselOrFlightNo"]);
				}
			}
		}

		public void TestCusEntryNumData()
		{
			var declarationPK = CreateJobDeclaration(14, "EXP", branchPK, companyPK);
			CreateCusEntryNum(declarationPK, "JobDeclaration", "UNDERBOND110", "UDM", "OTH", new DateTime(2023, 02, 19), new DateTime(2023, 03, 04));
			CreateCusEntryNum(declarationPK, "JobDeclaration", "UNDERBOND111", "UDM", "OTH", new DateTime(2023, 02, 20), new DateTime(2023, 03, 05));

			var entryPK = CreateCusEntryHeader(declarationPK, 14);
			CreateCusEntryNum(entryPK, "CusEntryHeader", "6N00221000024X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));
			CreateCusEntryNum(entryPK, "CusEntryHeader", "6N00221000025X", "EXP", "OTH", new DateTime(2023, 02, 25), new DateTime(2023, 03, 10));

			var entryLinePK = CreateCusEntryLine(entryPK, 14);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 14);
			CreateJobComInvoiceLine(invoiceHeaderPK, 14, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(new DateTime(2023, 02, 20), reader["BondedTransportationStart"]);
					AssertEquals(new DateTime(2023, 03, 05), reader["BondedTransportationEnd"]);

					AssertEquals("6N00221000025X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 02, 25), reader["AcceptedDate"]);
					AssertEquals(new DateTime(2023, 03, 10), reader["DueDateOfLoading"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(new DateTime(2023, 02, 20), reader["BondedTransportationStart"]);
					AssertEquals(new DateTime(2023, 03, 05), reader["BondedTransportationEnd"]);

					AssertEquals("6N00221000025X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 02, 25), reader["AcceptedDate"]);
					AssertEquals(new DateTime(2023, 03, 10), reader["DueDateOfLoading"]);
				}
			}
		}

		public void TestCusEntryHeaderAndLineData()
		{
			var declarationPK = CreateJobDeclaration(15, "EXP", branchPK, companyPK);

			CreateRefExchangeRate(companyPK, "USD", 0.1m, "CUE", new DateTime(2023, 02, 01), new DateTime(2023, 02, 28));
			CreateRefExchangeRate(companyPK, "USD", 0m, "CUE", new DateTime(2023, 03, 01), new DateTime(2023, 03, 31));

			var entryHeaderItems = GetItemList(new List<string> { "CH_EntryReleaseDate" }, new List<object> { new DateTime(2023, 02, 20) });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, 15, entryHeaderItems);
			CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1000 });
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK1, 15, entryLineItems);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK1, 15, entryLineItems);

			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, 15);
			CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00221000025X", "EXP", "CUS", new DateTime(2023, 03, 02), new DateTime(2023, 03, 10));

			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK2, 15, entryLineItems);
			var entryLinePK4 = CreateCusEntryLine(entryHeaderPK2, 15, entryLineItems);
			var entryLinePK5 = CreateCusEntryLine(entryHeaderPK2, 15, entryLineItems);

			var entryHeaderPK3 = CreateCusEntryHeader(declarationPK, 15);
			CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00221000026X", "EXP", "CUS", new DateTime(2023, 04, 01), new DateTime(2023, 04, 10));

			var entryLinePK6 = CreateCusEntryLine(entryHeaderPK3, 15, entryLineItems);

			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK, 15);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 15, entryLinePK1);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 15, entryLinePK2);

			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 15);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 15, entryLinePK3);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 15, entryLinePK4);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 15, entryLinePK5);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 15);
			CreateJobComInvoiceLine(invoiceHeaderPK3, 15, entryLinePK6);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(new DateTime(2023, 02, 20), reader["ClearedDate"]);
					AssertEquals(2, reader["TotalEntryLineCount"]);
					AssertEquals(2000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(20000m, reader["TotalCustomsValueUSD"]);
					AssertEquals(0.1m, reader["USDRate"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(new DateTime(2023, 02, 20), reader["ClearedDate"]);
					AssertEquals(2, reader["TotalEntryLineCount"]);
					AssertEquals(2000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(20000m, reader["TotalCustomsValueUSD"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["TotalEntryLineCount"]);
					AssertEquals(3000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(0m, reader["TotalCustomsValueUSD"]);
					AssertEquals(0m, reader["USDRate"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["TotalEntryLineCount"]);
					AssertEquals(3000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(0m, reader["TotalCustomsValueUSD"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["TotalEntryLineCount"]);
					AssertEquals(1000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(0m, reader["TotalCustomsValueUSD"]);
					AssertEquals(0m, reader["USDRate"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '') WHERE EntryPK = '{1}'", companyPK, entryHeaderPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["TotalEntryLineCount"]);
					AssertEquals(1000m, reader["TotalCustomsValueKRW"]);
					AssertEquals(0m, reader["TotalCustomsValueUSD"]);
				}
			}
		}

		public void TestJobComInvoiceHeaderData()
		{
			var declarationPK = CreateJobDeclaration(16, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 16);
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, 16);
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, 16);

			var invoiceHeaderAddInfoDics = new Dictionary<string, string>();
			invoiceHeaderAddInfoDics.Add("DRWApplicantType", "1");
			invoiceHeaderAddInfoDics.Add("ImportCargoManagementNumber", "01234567890123A");
			var invoiceHeader1Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks", "JZ_InvoiceAmount", "JZ_PaymentTerms", "JZ_LetterOfCreditNumber", "JZ_IncoTerm", "JZ_RX_NKInvoice_Currency", "JZ_AddInfo" },
				new List<object> { 1000, "G", 500, 20000, "LS", "7654321", "FOB", "KRW", GenerateAddInfoData(invoiceHeaderAddInfoDics) });
			var invoiceHeader2Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks", "JZ_InvoiceAmount", "JZ_PaymentTerms", "JZ_LetterOfCreditNumber", "JZ_IncoTerm", "JZ_RX_NKInvoice_Currency", "JZ_AddInfo" },
				new List<object> { 100, "KG", 50, 10000, "LS", "7654321", "FOB", "KRW", GenerateAddInfoData(invoiceHeaderAddInfoDics) });

			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK, 16, invoiceHeader1Items);
			var invoiceHeader2PK = CreateJobComInvoiceHeader(declarationPK, 16, invoiceHeader2Items);
			CreateJobComInvoiceLine(invoiceHeader1PK, 16, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, 16, entryLine2PK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(101m, reader["TotalGrossWeightKG"]);
					AssertEquals(550m, reader["TotalPackages"]);
					AssertEquals(30000m, reader["TotalInvoiceAmount"]);
					AssertEquals("LS", reader["PaymentMethod"]);
					AssertEquals("7654321", reader["LetterOfCreditNumber"]);
					AssertEquals("FOB", reader["Incoterm"]);
					AssertEquals("KRW", reader["Currency"]);
					AssertEquals("1", reader["DrawbackApplicantType"]);
					AssertEquals("01234567890123A", reader["ImportCargoManagementNo"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(101m, reader["TotalGrossWeightKG"]);
					AssertEquals(550m, reader["TotalPackages"]);
					AssertEquals(30000m, reader["TotalInvoiceAmount"]);
					AssertEquals("LS", reader["PaymentMethod"]);
					AssertEquals("7654321", reader["LetterOfCreditNumber"]);
					AssertEquals("FOB", reader["Incoterm"]);
					AssertEquals("KRW", reader["Currency"]);
					AssertEquals("1", reader["DrawbackApplicantType"]);
					AssertEquals("01234567890123A", reader["ImportCargoManagementNo"]);
				}
			}
		}
		public void TestJobComInvHeaderCharges()
		{
			var declarationPK = CreateJobDeclaration(20, "EXP", branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, 20);
			var entryLinePK = CreateCusEntryLine(entryPK, 20);
			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, 20);
			var invoicePK2 = CreateJobComInvoiceHeader(declarationPK, 20);
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, 20, entryLinePK);
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK2, 20, entryLinePK);

			var headerChargeQueryItem1 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 10000m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem1);
			var headerChargeQueryItem2 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "ONS", 200m, "AUD", 1200m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem2);
			var headerChargeQueryItem3 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 200m, "AUD", 1200m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem3);
			var headerChargeQueryItem4 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "FIF", 200m, "USD", 1500m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem4);
			var lineChargeQueryItem1 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 9999m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invLinePK1, "JI", lineChargeQueryItem1);

			var headerChargeQueryItem5 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "ONS", 20m, "USD", 1500m });
			CreateJobComInvHeaderCharge(invoicePK2, "JZ", headerChargeQueryItem5);
			var headerChargeQueryItem6 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "EXW", 2000m, "USD", 1500m });
			CreateJobComInvHeaderCharge(invoicePK2, "JZ", headerChargeQueryItem6);
			var headerChargeQueryItem7 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "ONS", 100000m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invoicePK2, "JZ", headerChargeQueryItem7);
			var headerChargeQueryItem8 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 20m, "USD", 1500m });
			CreateJobComInvHeaderCharge(invoicePK2, "JZ", headerChargeQueryItem8);
			var lineChargeQueryItem2 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "ONS", 9999m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invLinePK2, "JI", lineChargeQueryItem2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(280000m, reader["Freight"]);
					AssertEquals(370000m, reader["Insurance"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(280000m, reader["Freight"]);
					AssertEquals(370000m, reader["Insurance"]);
				}
			}
		}

		public void TestJobComInvHeaderChargesWithTotalPackages()
		{
			var declarationPK = CreateJobDeclaration(21, "EXP", branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, 21);
			var entryLinePK = CreateCusEntryLine(entryPK, 21);
			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, 21, GetItemList(new List<string> { "JZ_NoOfPacks" }, new List<object> { 1 }));
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, 21, entryLinePK);

			var headerChargeQueryItem1 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 10000m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem1);
			var headerChargeQueryItem2 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "ONS", 200m, "AUD", 1200m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem2);
			var headerChargeQueryItem3 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 200m, "USD", 1500m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem3);
			var headerChargeQueryItem4 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "FIF", 200m, "AUD", 1200m });
			CreateJobComInvHeaderCharge(invoicePK1, "JZ", headerChargeQueryItem4);
			var lineChargeQueryItem1 = GetItemList(new List<string> { "J7_ChargeType", "J7_Amount", "J7_RX_NKCurrency", "J7_ExchangeRate" },
				new List<object> { "OFT", 9999m, "KRW", 1m });
			CreateJobComInvHeaderCharge(invLinePK1, "JI", lineChargeQueryItem1);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1m, reader["TotalPackages"]);
					AssertEquals(310000m, reader["Freight"]);
					AssertEquals(240000m, reader["Insurance"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1m, reader["TotalPackages"]);
					AssertEquals(310000m, reader["Freight"]);
					AssertEquals(240000m, reader["Insurance"]);
				}
			}
		}

		public void TestItems1To15()
		{
			var declarationAddInfoDics = new Dictionary<string, string>();
			declarationAddInfoDics.Add("ProcedureType", "E");
			var declarationItems = GetItemList(new List<string> { "JE_AddInfo" }, new List<object> { GenerateAddInfoData(declarationAddInfoDics) });
			var declarationPK = CreateJobDeclaration(17, "EXP", branchPK, companyPK, declarationItems);

			var entryHeaderItems = GetItemList(new List<string> { "CH_EntryReleaseDate" }, new List<object> { new DateTime(2023, 02, 20) });
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 17, entryHeaderItems);
			CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "6N00221000025X", "EXP", "OTH", new DateTime(2023, 02, 25), new DateTime(2023, 03, 10));

			var entryLine1Items = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 100 });
			var entryLine2Items = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 200 });
			var entryLine3Items = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 300 });
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, 17, entryLine1Items);
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, 17, entryLine2Items);
			var entryLine3PK = CreateCusEntryLine(entryHeaderPK, 17, entryLine3Items);

			var invoiceHeaderAddInfoDics = new Dictionary<string, string>();
			invoiceHeaderAddInfoDics.Add("DRWApplicantType", "1");
			invoiceHeaderAddInfoDics.Add("ImportCargoManagementNumber", "01234567890123A");
			var invoiceHeader1Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks", "JZ_InvoiceAmount", "JZ_PaymentTerms", "JZ_LetterOfCreditNumber", "JZ_IncoTerm", "JZ_RX_NKInvoice_Currency", "JZ_AddInfo" },
				new List<object> { 1000, "G", 500, 20000, "LS", "7654321", "FOB", "KRW", GenerateAddInfoData(invoiceHeaderAddInfoDics) });
			var invoiceHeader2Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks", "JZ_InvoiceAmount", "JZ_PaymentTerms", "JZ_LetterOfCreditNumber", "JZ_IncoTerm", "JZ_RX_NKInvoice_Currency", "JZ_AddInfo" },
				new List<object> { 100, "KG", 50, 10000, "LS", "7654321", "FOB", "KRW", GenerateAddInfoData(invoiceHeaderAddInfoDics) });
			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK, 17, invoiceHeader1Items);
			var invoiceHeader2PK = CreateJobComInvoiceHeader(declarationPK, 17, invoiceHeader2Items);

			CreateJobComInvoiceLine(invoiceHeader1PK, 17, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeader1PK, 17, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeader1PK, 17, entryLine2PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, 17, entryLine3PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, 17, entryLine3PK);

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(5, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("E", reader["ProcedureType"]);
					AssertEquals("6N00221000025X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 02, 25), reader["AcceptedDate"]);
					AssertEquals(new DateTime(2023, 03, 10), reader["DueDateOfLoading"]);
					AssertEquals(new DateTime(2023, 02, 20), reader["ClearedDate"]);
					AssertEquals(600m, reader["TotalCustomsValueKRW"]);
					AssertEquals(101m, reader["TotalGrossWeightKG"]);
					AssertEquals(550m, reader["TotalPackages"]);
					AssertEquals(30000m, reader["TotalInvoiceAmount"]);
					AssertEquals("LS", reader["PaymentMethod"]);
					AssertEquals("7654321", reader["LetterOfCreditNumber"]);
					AssertEquals("FOB", reader["Incoterm"]);
					AssertEquals("KRW", reader["Currency"]);
					AssertEquals("1", reader["DrawbackApplicantType"]);
					AssertEquals("01234567890123A", reader["ImportCargoManagementNo"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("E", reader["ProcedureType"]);
					AssertEquals("6N00221000025X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 02, 25), reader["AcceptedDate"]);
					AssertEquals(new DateTime(2023, 03, 10), reader["DueDateOfLoading"]);
					AssertEquals(new DateTime(2023, 02, 20), reader["ClearedDate"]);
					AssertEquals(600m, reader["TotalCustomsValueKRW"]);
					AssertEquals(101m, reader["TotalGrossWeightKG"]);
					AssertEquals(550m, reader["TotalPackages"]);
					AssertEquals(30000m, reader["TotalInvoiceAmount"]);
					AssertEquals("LS", reader["PaymentMethod"]);
					AssertEquals("7654321", reader["LetterOfCreditNumber"]);
					AssertEquals("FOB", reader["Incoterm"]);
					AssertEquals("KRW", reader["Currency"]);
					AssertEquals("1", reader["DrawbackApplicantType"]);
					AssertEquals("01234567890123A", reader["ImportCargoManagementNo"]);
				}
			}
		}

		public void TestDeclarantData()
		{
			var declarationPK1 = CreateJobDeclaration(30, "EXP", branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK1, 30);
			var entryLinePK = CreateCusEntryLine(entryPK, 30);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK1, 30);
			CreateJobComInvoiceLine(invoiceHeaderPK, 30, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("READY KOREA", reader["DeclarantCompanyName"]);
					AssertEquals("12345", reader["DeclarantUnipassID"]);
					AssertEquals("Kim", reader["DeclarantRepresentativeName"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("READY KOREA", reader["DeclarantCompanyName"]);
					AssertEquals("12345", reader["DeclarantUnipassID"]);
					AssertEquals("Kim", reader["DeclarantRepresentativeName"]);
				}
			}

			var companyPK2 = TestDataCreator.CreateCompany("KC2", "KR", "KRW");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, "KB2", "KRSEL");

			var orgHeaderPK2 = TestDataCreator.CreateOrganisation("RDKO2", "READY KOREA 2");
			CreateOrgAddress(orgHeaderPK2, "TES2", true, "XXX", GetItemList(new List<string> { "OA_Address1", "OA_RN_NKCountryCode" }, new List<object> { "Street 1st", "KR" }));
			CreateStmData(companyPK2, "UNIPASSDeclarantID", "45678");
			CreateContact(orgHeaderPK2, "Kim", "010-0000-0000", false);
			var companySQL = @"UPDATE dbo.GlbBranch SET GB_OH_OrgProxy = @orgHeaderPK2, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = @branchPK2";
			using (var command = Db.Connection.Command(companySQL))
			{
				command.AddParameter("@branchPK2", SqlDbType.UniqueIdentifier, branchPK2);
				command.AddParameter("@orgHeaderPK2", SqlDbType.UniqueIdentifier, orgHeaderPK2);
				command.ExecuteNonQuery();
			}
			var declarationPK2 = CreateJobDeclaration(31, "EXP", branchPK2, companyPK2);
			var entryPK2 = CreateCusEntryHeader(declarationPK2, 31);
			var entryLinePK2 = CreateCusEntryLine(entryPK2, 31);

			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK2, 31);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 31, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(DBNull.Value, reader["DeclarantCompanyName"]);
					AssertEquals("45678", reader["DeclarantUnipassID"]);
					AssertEquals(DBNull.Value, reader["DeclarantRepresentativeName"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(DBNull.Value, reader["DeclarantCompanyName"]);
					AssertEquals("45678", reader["DeclarantUnipassID"]);
					AssertEquals(DBNull.Value, reader["DeclarantRepresentativeName"]);
				}
			}

			var companyPK3 = TestDataCreator.CreateCompany("KC3", "KR", "KRW");
			var branchPK3 = TestDataCreator.CreateBranch(companyPK3, "KB3", "KRSEL");

			var orgHeaderPK3 = TestDataCreator.CreateOrganisation("RDKO3", "READY KOREA 3");
			CreateOrgAddress(orgHeaderPK3, "TES3", true, "XXX", GetItemList(new List<string> { "OA_Address1", "OA_RN_NKCountryCode" }, new List<object> { "Street 1st", "KR" }));
			CreateStmData(companyPK3, "UNKNOWN", "89012");
			CreateContact(orgHeaderPK3, "Kim", "010-0000-0000", true);
			var branchSQL = @"UPDATE dbo.GlbCompany SET GC_OH_OrgProxy = @orgHeaderPK3, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @companyPK3";
			using (var command = Db.Connection.Command(branchSQL))
			{
				command.AddParameter("@companyPK3", SqlDbType.UniqueIdentifier, companyPK3);
				command.AddParameter("@orgHeaderPK3", SqlDbType.UniqueIdentifier, orgHeaderPK3);
				command.ExecuteNonQuery();
			}
			var declarationPK3 = CreateJobDeclaration(32, "EXP", branchPK3, companyPK3);
			var entryPK3 = CreateCusEntryHeader(declarationPK3, 32);
			var entryLinePK3 = CreateCusEntryLine(entryPK3, 32);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK3, 32);
			CreateJobComInvoiceLine(invoiceHeaderPK3, 32, entryLinePK3);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("READY KOREA 3", reader["DeclarantCompanyName"]);
					AssertEquals(DBNull.Value, reader["DeclarantUnipassID"]);
					AssertEquals("Kim", reader["DeclarantRepresentativeName"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("READY KOREA 3", reader["DeclarantCompanyName"]);
					AssertEquals(DBNull.Value, reader["DeclarantUnipassID"]);
					AssertEquals("Kim", reader["DeclarantRepresentativeName"]);
				}
			}
		}

		public void TestJobDeclarationData_ETC()
		{
			var declarationAddInfoDics = new Dictionary<string, string>();
			declarationAddInfoDics.Add("ContainerPackMode", "CT");
			var declarationItems = GetItemList(new List<string> { "JE_OH_Forwarder", "JE_AddInfo" },
												 new List<object> { forwarderPK, GenerateAddInfoData(declarationAddInfoDics) });
			var declarationPK = CreateJobDeclaration(30, "EXP", branchPK, companyPK, declarationItems);
			var jobDocsAndCartage = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");
			CreateJobService(jobDocsAndCartage, "JP", "STD", new DateTime(2023, 03, 13));
			CreateJobService(jobDocsAndCartage, "JP", "XIN", new DateTime(2023, 03, 16));
			CreateJobService(jobDocsAndCartage, "JP", "XIN", new DateTime(2023, 03, 18));
			var entryPK = CreateCusEntryHeader(declarationPK, 30);
			var entryLinePK = CreateCusEntryLine(entryPK, 30);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 30);
			CreateJobComInvoiceLine(invoiceHeaderPK, 30, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Forwarder Test Data", reader["ForwarderCompanyName"]);
					AssertEquals("CT", reader["ContainerPack"]);
					AssertEquals(new DateTime(2023, 03, 16), reader["PreferredInspectionDate"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Forwarder Test Data", reader["ForwarderCompanyName"]);
					AssertEquals("CT", reader["ContainerPack"]);
					AssertEquals(new DateTime(2023, 03, 16), reader["PreferredInspectionDate"]);
				}
			}
		}

		public void TestDeclarantDesc_DifferentRemark()
		{
			var declarationPK = CreateJobDeclaration(15, "EXP", branchPK, companyPK);

			var entryPK = CreateCusEntryHeader(declarationPK, 15);
			var entryLinePK1 = CreateCusEntryLine(entryPK, 15);
			var entryLinePK2 = CreateCusEntryLine(entryPK, 15);

			var testKRDescription = "On 830, 600 bytes can be sent as customs remarks. In most cases, it is written in Korean so we restrict the length to 300. In this test two same strings are used just to check if concatenation works regardless of the order.";
			var invoiceHeaderItems1 = GetItemList(new List<string> { "JZ_Remarks" }, new List<object> { testKRDescription });
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK, 15, invoiceHeaderItems1);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 15, entryLinePK1);

			var invoiceHeaderItems2 = GetItemList(new List<string> { "JZ_Remarks" }, new List<object> { testKRDescription });
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 15, invoiceHeaderItems2);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 15, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(testKRDescription + " On 830, 600 bytes can be sent as customs remarks. In most cases, it is writt", reader["DeclarantDesc"]);
					AssertEquals(300, reader["DeclarantDesc"].ToString().Length);
					Assert("Should contain no other records than the test record", !reader.Read());
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(testKRDescription + " On 830, 600 bytes can be sent as customs remarks. In most cases, it is writt", reader["DeclarantDesc"]);
					AssertEquals(300, reader["DeclarantDesc"].ToString().Length);
					Assert("Should contain no other records than the test record", !reader.Read());
				}
			}
		}

		public void TestDeclarantDesc_SameRemark()
		{
			var declarationPK = CreateJobDeclaration(15, "EXP", branchPK, companyPK);

			var entryPK = CreateCusEntryHeader(declarationPK, 15);
			var entryLinePK1 = CreateCusEntryLine(entryPK, 15);
			var entryLinePK2 = CreateCusEntryLine(entryPK, 15);

			var invoiceHeaderItems1 = GetItemList(new List<string> { "JZ_Remarks" }, new List<object> { "Invoice Remark" });
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK, 15, invoiceHeaderItems1);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 15, entryLinePK1);

			var invoiceHeaderItems2 = GetItemList(new List<string> { "JZ_Remarks" }, new List<object> { "Invoice Remark" });
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 15, invoiceHeaderItems2);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 15, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Invoice Remark Invoice Remark", reader["DeclarantDesc"]);
					AssertEquals(false, reader.Read());
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Invoice Remark Invoice Remark", reader["DeclarantDesc"]);
					AssertEquals(false, reader.Read());
				}
			}
		}

		public void TestExporterData()
		{
			var sellerAddressPK = TestDataCreator.CreateAddress(exporterPK, "Test", "Test Address1");
			var declarationItems = GetItemList(new List<string> { "JE_OA_SellerAddress" }, new List<object> { sellerAddressPK });
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK, declarationItems);
			var entryPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryPK, 40);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("ALOVERSEL", reader["ExporterCode"]);
					AssertEquals("ALOE VERA KOREA", reader["ExporterCompanyName"]);
					AssertEquals("UNKNOWN9999990000", reader["ExporterUnipassID"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("ALOVERSEL", reader["ExporterCode"]);
					AssertEquals("ALOE VERA KOREA", reader["ExporterCompanyName"]);
					AssertEquals("UNKNOWN9999990000", reader["ExporterUnipassID"]);
				}
			}
		}

		public void TestSupplierData()
		{
			var supplierPK = TestDataCreator.CreateOrganisation("KRSUPPLIER", "KR Supplier");
			var supllierAddressPK = TestDataCreator.CreateAddress(supplierPK, "Test", "Address1", "Address2", "Seoul", "14", "12345");
			TestDataCreator.CreateOrgCusCode(supplierPK, "GBR", "1234567890123", "KR");
			TestDataCreator.CreateOrgCusCode(supplierPK, "06", "123456789012345", "KR");
			TestDataCreator.CreateOrgCusCode(supplierPK, "GBR", "XXXXXXXXXXXXX", "JP");
			TestDataCreator.CreateOrgCusCode(supplierPK, "06", "XXXXXXXXXXXXXXX", "JP");
			TestDataCreator.CreateContact(supplierPK, "Supplier Test1", "");
			TestDataCreator.CreateContact(supplierPK, "Supplier Test2", "");

			CreateContact(supplierPK, "Supplier Name", "", true);

			var declarationItems = GetItemList(new List<string> { "JE_OH_Supplier", "JE_OA_SupplierAddress" }, new List<object> { supplierPK, supllierAddressPK });

			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK, declarationItems);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KRSUPPLIER", reader["SupplierCode"]);
					AssertEquals("KR Supplier", reader["SupplierCompanyName"]);
					AssertEquals("Supplier Name", reader["SupplierRepresentativeName"]);
					AssertEquals("Address1", reader["SupplierAddressLine1"]);
					AssertEquals("Address2", reader["SupplierAddressLine2"]);
					AssertEquals("1234567890123", reader["SupplierBusinessNumber"]);
					AssertEquals("123456789012345", reader["SupplierUnipassID"]);
					AssertEquals("12345", reader["SupplierPostCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KRSUPPLIER", reader["SupplierCode"]);
					AssertEquals("KR Supplier", reader["SupplierCompanyName"]);
					AssertEquals("Supplier Name", reader["SupplierRepresentativeName"]);
					AssertEquals("Address1", reader["SupplierAddressLine1"]);
					AssertEquals("Address2", reader["SupplierAddressLine2"]);
					AssertEquals("1234567890123", reader["SupplierBusinessNumber"]);
					AssertEquals("123456789012345", reader["SupplierUnipassID"]);
					AssertEquals("12345", reader["SupplierPostCode"]);
				}
			}
		}

		public void TestManufacturerFullData()
		{
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var manufacturerPK = TestDataCreator.CreateOrganisation("MANUFACTURE", "KR Manufacturer");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "Test", "Address1", "Address2", "Seoul", "14", "12345");
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "06", "123456789012345", "KR");
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "IPC", "123", "KR", manufacturerAddressPK);
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "06", "XXXXXXXXXXXXXXX", "JP", manufacturerAddressPK);
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "IPC", "XXX", "JP", manufacturerAddressPK);

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_OA_ManufacturerAddress" },	new List<object> { manufacturerAddressPK });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40, invoiceHeaderItems);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals("123456789012345", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("123", reader["ManufacturerIPCCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals("123456789012345", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("123", reader["ManufacturerIPCCode"]);
				}
			}
		}

		public void TestManufacturerNoExists()
		{
			var sellerAddressPK = TestDataCreator.CreateAddress(exporterPK, "Test", "Test Address1", "Test Address2", "Seoul", "14", "12345");
			var declarationItems = GetItemList(new List<string> { "JE_OA_SellerAddress" }, new List<object> { sellerAddressPK });
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK, declarationItems);

			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(DBNull.Value, reader["ManufacturerCode"]);
					AssertEquals("미상", reader["ManufacturerCompanyName"]);
					AssertEquals("제조미상9999000", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("999", reader["ManufacturerIPCCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(DBNull.Value, reader["ManufacturerCode"]);
					AssertEquals("미상", reader["ManufacturerCompanyName"]);
					AssertEquals("제조미상9999000", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("999", reader["ManufacturerIPCCode"]);
				}
			}
		}

		public void TestManufacturerEmptyAndMessageSubTypeIsNotE()
		{
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var manufacturerPK = TestDataCreator.CreateOrganisation("MANUFACTURE", "KR Manufacturer");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "Test", "Address1", "Address2", "Seoul", "14", "12345");

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_OA_ManufacturerAddress" }, new List<object> { manufacturerAddressPK });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40, invoiceHeaderItems);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals("제조미상9999000", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("999", reader["ManufacturerIPCCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals("제조미상9999000", reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals("999", reader["ManufacturerIPCCode"]);
				}
			}
		}

		public void TestManufacturerEmptyAndMessageSubTypeIsE()
		{			
			var declarationAddInfoDics = new Dictionary<string, string>();
			declarationAddInfoDics.Add("ProcedureType", "E");
			var declarationItems = GetItemList(new List<string> { "JE_AddInfo" }, new List<object> { GenerateAddInfoData(declarationAddInfoDics) });
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK, declarationItems);

			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var manufacturerPK = TestDataCreator.CreateOrganisation("MANUFACTURE", "KR Manufacturer");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "Test", "Address1", "Address2", "Seoul", "14", "12345");

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_OA_ManufacturerAddress" }, new List<object> { manufacturerAddressPK });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40, invoiceHeaderItems);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals(DBNull.Value, reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals(DBNull.Value, reader["ManufacturerIPCCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("MANUFACTURE", reader["ManufacturerCode"]);
					AssertEquals("KR Manufacturer", reader["ManufacturerCompanyName"]);
					AssertEquals(DBNull.Value, reader["ManufacturerUnipassID"]);
					AssertEquals("12345", reader["ManufacturerPostCode"]);
					AssertEquals(DBNull.Value, reader["ManufacturerIPCCode"]);
				}
			}
		}

		public void TestImporterData()
		{
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var buyerPK = TestDataCreator.CreateOrganisation("KRBUYER", "KR Buyer");
			TestDataCreator.CreateOrgCusCode(buyerPK, "07", "1234567", "KR");
			TestDataCreator.CreateOrgCusCode(buyerPK, "07", "XXXXXXX", "JP");

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_OH_Buyer" },
				new List<object> { buyerPK });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40, invoiceHeaderItems);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KRBUYER", reader["BuyerCode"]);
					AssertEquals("KR Buyer", reader["BuyerCompanyName"]);
					AssertEquals("1234567", reader["BuyerID"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KRBUYER", reader["BuyerCode"]);
					AssertEquals("KR Buyer", reader["BuyerCompanyName"]);
					AssertEquals("1234567", reader["BuyerID"]);
				}
			}
		}

		public void TestManufacturerWhenTwoAddressHasIPCCode()
		{
			var declarationPK = CreateJobDeclaration(40, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 40);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 40);

			var manufacturerPK = TestDataCreator.CreateOrganisation("MANUFACTURE", "KR Manufacturer");
			var manufacturerAddress1PK = TestDataCreator.CreateAddress(manufacturerPK, "Test1", "Address1", "Address2", "Seoul", "14", "12345");
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "IPC", "123", "KR", manufacturerAddress1PK);

			var manufacturerAddress2PK = TestDataCreator.CreateAddress(manufacturerPK, "Test2", "Address1", "Address2", "Seoul", "14", "12345");
			TestDataCreator.CreateOrgCusCode(manufacturerPK, "IPC", "321", "KR", manufacturerAddress2PK);

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_OA_ManufacturerAddress" }, new List<object> { manufacturerAddress1PK });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 40, invoiceHeaderItems);
			CreateJobComInvoiceLine(invoiceHeaderPK, 40, entryLinePK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("123", reader["ManufacturerIPCCode"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("123", reader["ManufacturerIPCCode"]);
				}
			}
		}

		public void TestAddColumnQuery()
		{
			int clusterKey = 50;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, clusterKey);

			var invoiceHeader1Items = GetItemList(new List<string> { "JZ_InvoiceCurrExRate" }, new List<object> { 1250 });

			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeader1Items);
			var invoiceHeader2PK = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeader1Items);
			CreateJobComInvoiceLine(invoiceHeader1PK, clusterKey, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, clusterKey, entryLine2PK);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KG", reader["TotalGrossWeightUQ"]);
					AssertEquals(1250m, reader["InvoiceCurrExchangeRate"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KG", reader["TotalGrossWeightUQ"]);
					AssertEquals(1250m, reader["InvoiceCurrExchangeRate"]);
				}
			}
		}

		public void TestProcessStatus()
		{
			var declarationPK1 = CreateJobDeclaration(13, "EXP", branchPK, companyPK);
			CreateCusEntryNum(declarationPK1, "JobDeclaration", "UNDERBOND110", "UDM", "OTH", new DateTime(2023, 02, 19), new DateTime(2023, 03, 04));
			var entryPK1 = CreateCusEntryHeader(declarationPK1, 13);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));
			var entryLinePK1 = CreateCusEntryLine(entryPK1, 13);
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK1, 13);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 13, entryLinePK1);

			var declarationPK2 = CreateJobDeclaration(14, "EXP", branchPK, companyPK);
			CreateCusEntryNum(declarationPK2, "JobDeclaration", "UNDERBOND110", "UDM", "OTH", new DateTime(2023, 02, 19), new DateTime(2023, 03, 04));
			var entryHeaderItems1 = GetItemList(new List<string> { "CH_EntryReleaseDate" }, new List<object> { new DateTime(2023, 02, 20) });
			var entryPK2 = CreateCusEntryHeader(declarationPK2, 14, entryHeaderItems1);
			CreateCusEntryNum(entryPK2, "CusEntryHeader", "6N00221000025X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));
			var entryLinePK2 = CreateCusEntryLine(entryPK2, 14);
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK2, 14);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 14, entryLinePK2);

			var declarationItems = GetItemList(new List<string> { "JE_EntryDate" }, new List<object> { new DateTime(2023, 1, 20) });
			var declarationPK3 = CreateJobDeclaration(15, "EXP", branchPK, companyPK, declarationItems);
			CreateCusEntryNum(declarationPK3, "JobDeclaration", "UNDERBOND110", "UDM", "OTH", new DateTime(2023, 02, 19), new DateTime(2023, 03, 04));
			var entryHeaderItems2 = GetItemList(new List<string> { "CH_EntryReleaseDate" }, new List<object> { new DateTime(2023, 02, 20) });
			var entryPK3 = CreateCusEntryHeader(declarationPK3, 15, entryHeaderItems2);
			CreateCusEntryNum(entryPK3, "CusEntryHeader", "6N00221000026X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));
			var entryLinePK3 = CreateCusEntryLine(entryPK3, 15);
			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK3, 15);
			CreateJobComInvoiceLine(invoiceHeaderPK3, 15, entryLinePK3);

			var entryHeaderItems4 = GetItemList(new List<string> { "CH_Status" }, new List<object> { "CAP" });
			var entryPK4 = CreateCusEntryHeader(declarationPK3, 15, entryHeaderItems4);
			CreateCusEntryNum(entryPK4, "CusEntryHeader", "6N00221000027X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryHeaderItems5 = GetItemList(new List<string> { "CH_Status" }, new List<object> { "CAB" });
			var entryPK5 = CreateCusEntryHeader(declarationPK3, 15, entryHeaderItems5);
			CreateCusEntryNum(entryPK5, "CusEntryHeader", "6N00221000028X", "EXP", "OTH", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', 'A')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', 'B')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', 'C')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', 'A') " +
																	  "WHERE ISNULL('A', '') = '' " +
																	  "OR ('A' = 'A' AND AcceptedDate IS NOT NULL) " +
																	  "OR ('A' = 'B' AND ClearedDate IS NOT NULL) " +
																	  "OR ('A' = 'C' AND ActualDateOfLoading IS NOT NULL)", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Error when parentheses are missing.", 3, reader["RowCount"]);
				}
			}
		}

		public void TestCountryPK()
		{
			var krCountryPK = GetRefCountry("KR");
			var usCountryPK = GetRefCountry("US");

			var declarationItems1 = GetItemList(new List<string> { "JE_GoodsDestination" }, new List<object> { "KR" });
			var declarationPK1 = CreateJobDeclaration(13, "EXP", branchPK, companyPK, declarationItems1);
			var entryPK1 = CreateCusEntryHeader(declarationPK1, 13);
			var entryLinePK1 = CreateCusEntryLine(entryPK1, 13);
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK1, 13);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 13, entryLinePK1);

			var declarationItems2 = GetItemList(new List<string> { "JE_GoodsDestination" }, new List<object> { "US" });
			var declarationPK2 = CreateJobDeclaration(14, "EXP", branchPK, companyPK, declarationItems2);
			var entryPK2 = CreateCusEntryHeader(declarationPK2, 14);
			var entryLinePK2 = CreateCusEntryLine(entryPK2, 14);
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK2, 14);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 14, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(CountryOfDestination) AS CountryOfDestination  FROM [dbo].[KRExportEntries]('{0}', '') WHERE CountryPK = '{1}'", companyPK, krCountryPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("KR", reader["CountryOfDestination"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(CountryOfDestination) AS CountryOfDestination FROM [dbo].[KRExportEntries]('{0}', '') WHERE CountryPK = '{1}'", companyPK, usCountryPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("US", reader["CountryOfDestination"]);
				}
			}
		}
		Guid GetRefCountry(string countryCode)
		{
			Guid result;

			using (var command = TestConnection.Command(string.Format("SELECT RN_PK FROM [dbo].[RefCountry] WHERE RN_Code = '{0}'", countryCode)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					result = (Guid)reader["RN_PK"];
				}
			}

			return result;
		}

		Guid companyPK;
		Guid branchPK;
		Guid exporterPK;
		Guid forwarderPK;

		protected override bool RequiresSchemaBinding => false;
		protected override void SetUp()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("KR", "KR is your country code");

			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRSEL");

			var orgHeaderPK = TestDataCreator.CreateOrganisation("RDKOR", "READY KOREA");
			CreateOrgAddress(orgHeaderPK, "TEST", true, "CST", GetItemList(new List<string> { "OA_Address1", "OA_RN_NKCountryCode", "OA_RL_NKRelatedPortCode" }, new List<object> { "Street 1st", "KR", "KRSEL" }));
			CreateStmData(companyPK, "UNIPASSDeclarantID", "12345");
			CreateContact(orgHeaderPK, "Kim", "010-0000-0000", true);
			CreateContact(orgHeaderPK, "Lee", "010-1234-1234", false);
			TestDataCreator.CreateOrgCusCode(orgHeaderPK, "06", "RK000000", "KR");
			var branchSQL = @"UPDATE dbo.GlbBranch SET GB_OH_OrgProxy = @orgHeaderPK, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = @branchPK";
			using (var command = Db.Connection.Command(branchSQL))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.ExecuteNonQuery();
			}

			exporterPK = TestDataCreator.CreateOrganisation("ALOVERSEL", "ALOE VERA KOREA", "KRSEL");
			TestDataCreator.CreateOrgCusCode(exporterPK, "06", "UNKNOWN9999990000", "KR");
			forwarderPK = TestDataCreator.CreateOrganisation("FORWARDER", "Forwarder Test Data");
			CreateContact(forwarderPK, "James", "010-1234-5678", true);
		}
	}
}
