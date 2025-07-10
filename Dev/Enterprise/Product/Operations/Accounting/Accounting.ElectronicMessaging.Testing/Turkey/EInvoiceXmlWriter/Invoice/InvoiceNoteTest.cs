using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.Accounting.Export.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	sealed class InvoiceNoteTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNote()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMultilineForeignCurrencyInvoice(addCommentLines: true);

				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 47, invoiceNotes.Length);
						AssertNoteValue(invoiceNotes, "InvoiceDescription", "Test Invoice");
						AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: Reşitpaşa Mah. Katar Cad. İTÜ Ayazağa Kamüsü Teknokent ARI 1 Binası No:2/5/7 Maslak");
						AssertNoteValue(invoiceNotes, "CurrencyRate", "USD Kuru: 2.00000,EUR Kuru: 3.00000");
						AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız onyedibinyüzaltmışbir TRY seksenaltı kuruş");
						AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "14543.95,2617.91");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "14543.95");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "17161.86");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "17161.86");
						AssertNoteValueByParts(invoiceNotes, "ExternalDebtorCode", "Test-External-Debtor-Code");

						AssertNoteValue(invoiceNotes, "ShipmentNumber", "S00001000", "S00001001");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPlaceOfDelivery", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPlaceOfReceipt", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "1456 CTN");
						AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "1456 Carton");
						AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "896.000 M3");
						AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "896.000 Cubic Meters");
						AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "16610.000 KG");
						AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "16610.000 Kilograms");
						AssertNoteValue(invoiceNotes, "ShipmentVesselName", "MILLENIUM FALCON", "BLACK PEARL");
						AssertNoteValue(invoiceNotes, "ShipmentVoyageFlightNo", "QF105");
						AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "AIR");
						AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Air Freight");
						AssertNoteValue(invoiceNotes, "ShipmentAWBIssueDate", "29-01-2020");
						AssertNoteValue(invoiceNotes, "ShipmentContainerCount", "2");
						AssertNoteValue(invoiceNotes, "ShipmentContainerNumber", "EDCA 234567-5", "DCBA 234567-5");
						AssertNoteValue(invoiceNotes, "ShipmentContainerTypeCategory", "DRY - Dry Storage", "TOP - Open Top");
						AssertNoteValue(invoiceNotes, "ShipmentContainerFclLclAir", "LCL");
						AssertNoteValue(invoiceNotes, "ShipmentContainerByType", "1x40T3", "1x40T2");
						AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "NOTEBOOK", "PENCIL");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfFirstArrival", "SGSIN - Singapore", "MXCAN - Canatlan");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "AUSYD - Sydney", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentConsignee", "CONSIGNEE 1", "CONSIGNEE 2", "Test Company Name");
						AssertNoteValue(invoiceNotes, "ShipmentShipper", "Test Company Name");
						AssertNoteValue(invoiceNotes, "ShipmentAWBIssuePlace", "SYDNEY", "MELBOURNE");
						AssertNoteValue(invoiceNotes, "ShipmentAWBNumber", "AWBNumber 1", "AWBNumber 2");
						AssertNoteValue(invoiceNotes, "ShipmentMasterWaybillNumber", "MAWBNUMBER1", "MAWBNUMBER2");
						AssertNoteValue(invoiceNotes, "ShipmentHouseWaybillNumber", "800-43567890");
						AssertNoteValue(invoiceNotes, "ShipmentIncoTermCode", "FOB");
						AssertNoteValue(invoiceNotes, "ShipmentIncoTermDescription", "Free On Board");
						AssertNoteValue(invoiceNotes, "ShipmentTotalChargeableWeight", "1.700");

						AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 1");
						AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 2");
						AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 3");

						AssertNoteIsNull(invoiceNotes, "ShipmentWaybillNumbers");
					});
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNoteWithForeignCurrency()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "euro sent");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateForeignCurrencyInvoice(Helper.TestObjectCreator.EUR, 1.0234m);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 43, invoiceNotes.Length);
						AssertNoteValue(invoiceNotes, "InvoiceDescription", "Test Invoice");
						AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: Reşitpaşa Mah. Katar Cad. İTÜ Ayazağa Kamüsü Teknokent ARI 1 Binası No:2/5/7 Maslak");
						AssertNoteValue(invoiceNotes, "CurrencyRate", "EUR Kuru: 1.02340");
						AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız binsekizyüzyirmibir EUR seksenaltı euro sent");
						AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "1580.08,284.41");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "1580.08");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "1864.49");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "1864.49");

						AssertNoteValue(invoiceNotes, "ShipmentNumber", "S00001000");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPlaceOfDelivery", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentPlaceOfReceipt", "AUMEL - Melbourne");
						AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "1200 CTN");
						AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "1200 Carton");
						AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "536.000 M3");
						AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "536.000 Cubic Meters");
						AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "15265.000 KG");
						AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "15265.000 Kilograms");
						AssertNoteValue(invoiceNotes, "ShipmentVesselName", "MILLENIUM FALCON");
						AssertNoteValue(invoiceNotes, "ShipmentVoyageFlightNo", "QF105");
						AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "AIR");
						AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Air Freight");
						AssertNoteValue(invoiceNotes, "ShipmentAWBIssueDate", "29-01-2020");
						AssertNoteValue(invoiceNotes, "ShipmentContainerCount", "1");
						AssertNoteValue(invoiceNotes, "ShipmentContainerNumber", "DCBA 234567-5");
						AssertNoteValue(invoiceNotes, "ShipmentContainerTypeCategory", "TOP - Open Top");
						AssertNoteValue(invoiceNotes, "ShipmentContainerFclLclAir", "LCL");
						AssertNoteValue(invoiceNotes, "ShipmentContainerByType", "1x40T2");
						AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "PENCIL");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfFirstArrival", "SGSIN - Singapore");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "AUSYD - Sydney");
						AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "AUSYD - Sydney");
						AssertNoteValue(invoiceNotes, "ShipmentConsignee", "CONSIGNEE 1");
						AssertNoteValue(invoiceNotes, "ShipmentShipper", "SHIPPER 1");
						AssertNoteValue(invoiceNotes, "ShipmentAWBIssuePlace", "SYDNEY");
						AssertNoteValue(invoiceNotes, "ShipmentAWBNumber", "AWBNumber 1");
						AssertNoteValue(invoiceNotes, "ShipmentMasterWaybillNumber", "MAWBNUMBER1");
						AssertNoteValue(invoiceNotes, "ShipmentIncoTermCode", "FOB");
						AssertNoteValue(invoiceNotes, "ShipmentIncoTermDescription", "Free On Board");
						AssertNoteValue(invoiceNotes, "ShipmentTotalChargeableWeight", "1.700");
					});
				}
			}
		}

		public void TestInvoiceNoteWithXUT_SEA()
		{
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransactionSEA, createCommentCharge: true);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 44, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "S2100002849");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 29 Ekim Cad. Istanbul Vizyon Park A-1 Plaza Kat: 7  No:71 Yenibosna");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız sekizbinyediyüzyetmişaltı TRY otuzaltı kuruş");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "4694.20,844.96");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-311", "3237.20,0.00");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18-TRRA", "4694.20,844.96");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "7931.40");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "8776.36");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "8776.36");
				AssertNoteValueByParts(invoiceNotes, "ExternalDebtorCode", "DTNESOTO");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "S2100002849");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfDelivery", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfReceipt", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "15 PLT");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "15 Pallet");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "19.872 M3");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "19.872 Cubic Meters");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "4500.000 KG");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "4500.000 Kilograms");
				AssertNoteValue(invoiceNotes, "ShipmentVesselName", "MSC OLIVER");
				AssertNoteValue(invoiceNotes, "ShipmentVoyageFlightNo", "126E");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentContainerCount", "2");
				AssertNoteValue(invoiceNotes, "ShipmentContainerNumber", "HLCU 122365-0", "HLXU 456321-5");
				AssertNoteValue(invoiceNotes, "ShipmentContainerTypeCategory", "DRY - Dry Storage");
				AssertNoteValue(invoiceNotes, "ShipmentContainerFclLclAir", "GRP - Groupage / Freight All Kinds");
				AssertNoteValue(invoiceNotes, "ShipmentContainerByType", "1x20GP", "1x40HC");
				AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "Goods3");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "TRIZM - Izmir");
				AssertNoteValue(invoiceNotes, "ShipmentMasterWaybillNumber", "BOL9508");
				AssertNoteValue(invoiceNotes, "ShipmentHouseWaybillNumber", "IST210000005");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermCode", "FOB");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermDescription", "Free On Board");
				AssertNoteValue(invoiceNotes, "ShipmentTotalChargeableWeight", "19.872");
				AssertNoteValue(invoiceNotes, "ShipmentConsignee", "Eita Technologies Pte. Ltd.");
				AssertNoteValue(invoiceNotes, "ShipmentShipper", "Nesan Otomotiv A.S.");

				AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 1");
				AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 2");
				AssertNoteValue(invoiceNotes, "InvoiceNote|INVOICE NOTE 3");
			});
		}

		public void TestInvoiceNoteWithXUT_SEA_StandaloneShipment()
		{
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransactionSEA_StandaloneShipment);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 41, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "S2100002849");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 29 Ekim Cad. Istanbul Vizyon Park A-1 Plaza Kat: 7  No:71 Yenibosna");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız sekizbinyediyüzyetmişaltı TRY otuzaltı kuruş");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "DTNESOTO");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "4694.20,844.96");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-311", "3237.20,0.00");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18-TRRA", "4694.20,844.96");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "7931.40");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "8776.36");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "8776.36");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "S2100002849");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfDelivery", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfReceipt", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "57 PLT");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "57 Pallet");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "78.912 M3");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "78.912 Cubic Meters");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "20500.000 KG");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "20500.000 Kilograms");
				AssertNoteValue(invoiceNotes, "ShipmentVesselName", "MILLENIUM FALCON");
				AssertNoteValue(invoiceNotes, "ShipmentVoyageFlightNo", "126E");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentContainerCount", "2");
				AssertNoteValue(invoiceNotes, "ShipmentContainerNumber", "HLCU 122365-0", "HLXU 456321-5");
				AssertNoteValue(invoiceNotes, "ShipmentContainerTypeCategory", "DRY - Dry Storage");
				AssertNoteValue(invoiceNotes, "ShipmentContainerFclLclAir", "GRP - Groupage / Freight All Kinds");
				AssertNoteValue(invoiceNotes, "ShipmentContainerByType", "1x20GP", "1x40HC");
				AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "Goods3");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfFirstArrival", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "SGSIN - Singapore");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "TRIZM - Izmir");
				AssertNoteValue(invoiceNotes, "ShipmentHouseWaybillNumber", "IST210000005");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermCode", "FOB");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermDescription", "Free On Board");
				AssertNoteValue(invoiceNotes, "ShipmentTotalChargeableWeight", "19.872");
				AssertNoteValue(invoiceNotes, "ShipmentConsignee", "Eita Technologies Pte. Ltd.");
				AssertNoteValue(invoiceNotes, "ShipmentShipper", "Nesan Otomotiv A.S.");
			});
		}

		public void TestInvoiceNoteWithXUT_SEA_ShipmentErrorAndNullValues()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransactionSEA_ShipmentErrorAndNullValues);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 32, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "SSRXHZZZ00001329");
				AssertNoteValue(invoiceNotes, "BranchAddress", "184 Test Street");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız ikibinsekizyüzdokuz USD seksen sent");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-201", "17387.14,0.00");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-301", "6173.04,0.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "23560.18");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "23560.18");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "23560.18");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "SSRXHZZZ00001329");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "USCHI - Chicago");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "AUSYD - Sydney");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfDelivery", "USCHI - Chicago");
				AssertNoteValue(invoiceNotes, "ShipmentPlaceOfReceipt", "AUSYD - Sydney");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "1 PLT");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "1 Pallet");
				AssertNoteValue(invoiceNotes, "ShipmentVesselName", "ANL ADDAX");
				AssertNoteValue(invoiceNotes, "ShipmentVoyageFlightNo", "V151");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentContainerCount", "1");
				AssertNoteValue(invoiceNotes, "ShipmentContainerNumber", "TTNU 515151-7");
				AssertNoteValue(invoiceNotes, "ShipmentContainerTypeCategory", "DRY - Dry Storage");
				AssertNoteValue(invoiceNotes, "ShipmentContainerFclLclAir", "FCL - Full Container Load");
				AssertNoteValue(invoiceNotes, "ShipmentContainerByType", "1x20GP");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "USCHI - Chicago");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "AUSYD - Sydney");
				AssertNoteValue(invoiceNotes, "ShipmentMasterWaybillNumber", "42342143234234");
				AssertNoteValue(invoiceNotes, "ShipmentHouseWaybillNumber", "HB33334");
				AssertNoteValue(invoiceNotes, "ShipmentConsignee", "MILLER BODY SHOP");
				AssertNoteValue(invoiceNotes, "ShipmentShipper", "RAISING GARMENT LTD");

				AssertNoteIsNull(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentGoodsDescription", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentIncoTermCode", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentIncoTermDescription", "");
				AssertNoteIsNull(invoiceNotes, "ShipmentTotalChargeableWeight", "");
			});
		}

		public void TestInvoiceNoteWithXUT_CFSLoadListConsolWithCFSShipment()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_CFSLoadListConsolWithCFSShipment);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 24, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "C00002802");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 1 MAIN STREET");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız binseksen TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "6262101027");
				AssertNoteValue(invoiceNotes, "OrganizationContact", "İlgili: AKN");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-20", "900.00,180.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "900.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "1080.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "1080.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "S00005220");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "4 PKG");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "4 Package");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "FRPAR - Paris");
				AssertNoteValue(invoiceNotes, "ShipmentWaybillNumbers", "OB2311232,HB8787");
				AssertNoteValue(invoiceNotes, "ShipmentConsignee", "TEMEL ORGANIZATION");
				AssertNoteValue(invoiceNotes, "ShipmentShipper", "ACCU GREEN MANUFACTURING (NL) CORPORATION");
			});
		}

		public void TestInvoiceNoteWithXUT_CFSLoadListConsolWithCFSShipmentV2()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_CFSLoadListConsolWithCFSShipmentV2);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 23, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "H00001004");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: BESTEKAR SK NO:5");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız yetmişbeşbinbeşyüzonaltı TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-20", "62930.00,12586.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "62930.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "75516.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "75516.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "H00001004");
				AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "DESCRIPTION");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "100 PKG");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "100 Package");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "500.000 M3");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "500.000 Cubic Meters");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "1200.000 KG");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "1200.000 Kilograms");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "NZAKL - Auckland");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "NZAKL - Auckland");
				AssertNoteValue(invoiceNotes, "ShipmentWaybillNumbers", "OCEAN BILL 2,H00001004");
				AssertNoteValue(invoiceNotes, "ShipmentConsignee", "ACCU GREEN MANUFACTURING (NZ) CORPORATION");
				AssertNoteValue(invoiceNotes, "ShipmentShipper", "ACCU GREEN MANUFACTURING (NZ) CORPORATION");
			});
		}

		public void TestInvoiceNoteWithXUT_CFSLoadListConsolWithPortTransport()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_CFSLoadListConsolWithPortTransport);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 14, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "T00001148");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 1 MAIN STREET");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız binseksen TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "6262101027");
				AssertNoteValue(invoiceNotes, "OrganizationContact", "İlgili: AKN");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-20", "900.00,180.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "900.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "1080.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "1080.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "T00001148");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentWaybillNumbers", "OB2311231");
			});
		}

		public void TestInvoiceNoteWithXUT_CFSLoadListConsol_Standalone()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_CFSLoadListConsol_Standalone);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 16, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "C00002801");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 1 MAIN STREET");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız beşyüz TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "6262101027");
				AssertNoteValue(invoiceNotes, "OrganizationContact", "İlgili: AKN");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-311", "500.00,0.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "500.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "500.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "500.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "C00002801");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "DEBER - Berlin");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentWaybillNumbers", "OB2311231");
			});
		}

		public void TestInvoiceNoteWithXUT_LocalTransport_Standalone()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_LocalTransport_Standalone);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 18, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "T00001147");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: 1 MAIN STREET");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız ikibindörtyüz TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "6262101027");
				AssertNoteValue(invoiceNotes, "OrganizationContact", "İlgili: AKN");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-20", "2000.00,400.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "2000.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "2400.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "2400.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "T00001147");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitCode", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalVolumeAndUnitDescription", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitCode", "");
				AssertNoteValue(invoiceNotes, "ShipmentTotalWeightAndUnitDescription", "");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "SEA");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Sea Freight");
				AssertNoteValue(invoiceNotes, "ShipmentWaybillNumbers", "WB2211231");
			});
		}

		public void TestUniversalTransactionDoesntThrowIfHasAdditionalReferenceWithoutCountryInfo()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();

			AssertNoExceptionThrown(() => BuildInvoiceNotesWithXUT(UniversalTransactionDoesntThrowIfHasAdditionalReferenceWithoutCountryInfo));
		}

		public void TestInvoiceNoteWithXUT_CustomsDeclaration()
		{
			var customs = Factory.New<ZZRefCusCodeListCombined>();
			customs.ZZD_Code = "TR999888";
			customs.ZZD_Description = "TEST TR CUSTOMS OFFICE";
			customs.ZZD_CodeType = "CUSOF";
			customs.ZZD_CountryOrGrouping = CountryCodes.Turkey;
			customs.ZZD_StartDate = ZDateTime.Now.Date.AddMonths(-1);
			customs.ZZD_EndDate = ZDateTime.Now.Date.AddMonths(1);
			Factory.Save();

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoiceNotes = BuildInvoiceNotesWithXUT(UniversalTransaction_CustomsDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Count of notes", 40, invoiceNotes.Length);
				AssertNoteValue(invoiceNotes, "InvoiceDescription", "BIS2000203");
				AssertNoteValue(invoiceNotes, "BranchAddress", "Şube Adresi: BAGLAR MAHALLESI MIMAR SINAN CADDESI NO:43 BAGCILAR");
				AssertNoteValue(invoiceNotes, "CurrencyToString", "Yalnız altıyüz TRY");
				AssertNoteValue(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
				AssertNoteValue(invoiceNotes, "ExternalDebtorCode", "TRUY0002");
				AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-20", "500.00,100.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "500.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "600.00");
				AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "600.00");

				AssertNoteValue(invoiceNotes, "ShipmentNumber", "BIS2000203");
				AssertNoteValue(invoiceNotes, "ShipmentGoodsDescription", "TELEFON");
				AssertNoteValue(invoiceNotes, "ShipmentHouseWaybillNumber", "CZ25");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermCode", "CIF");
				AssertNoteValue(invoiceNotes, "ShipmentIncoTermDescription", "Cost, Insurance And Freight");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDischarge", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfLoading", "NLAMS - Amsterdam");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfDestination", "TRIST - Istanbul");
				AssertNoteValue(invoiceNotes, "ShipmentPortOfOrigin", "NLAMS - Amsterdam");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeCode", "AIR");
				AssertNoteValue(invoiceNotes, "ShipmentTransportModeDescription", "Air Freight");
				AssertNoteValue(invoiceNotes, "ShipmentRefBKR", "25-00070,25-00073");
				AssertNoteValue(invoiceNotes, "ShipmentRefPON", "123456");
				AssertNoteValue(invoiceNotes, "ShipmentRefSRN", "25340300IM08092025 / 13 MART 2025");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvSupplier", "FARFETCH EUROPE TRADING B.V.");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvoiceAmount", "1000.0000 - USD");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvLocalAmount", "36605.9000000000000");
				AssertNoteValue(invoiceNotes, "ShipmentAgreedExchangeRate", "36.605900000");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvoiceNo", "REF1");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvoiceDate", "13-03-2025");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialLineCount", "1");
				AssertNoteValue(invoiceNotes, "ShipmentImporterOrganization", "UYUMSOFT KURUMSAL ISSISTEMLERI VE TEKNO.A.S.");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitCode", "15 PK");
				AssertNoteValue(invoiceNotes, "ShipmentTotalNoOfPacksAndUnitDescription", "15 Package");
				AssertNoteValue(invoiceNotes, "ShipmentEntryReleaseDate", "13-03-2025");
				AssertNoteValue(invoiceNotes, "ShipmentMRNNumber", "25340300IM08092025");

				AssertNoteValue(invoiceNotes, "ShipmentCommercialInvoiceProcedure", "PrcTest");
				AssertNoteValue(invoiceNotes, "ShipmentCommercialLineValuation", "17 - Özel Hesap");
				AssertNoteValue(invoiceNotes, "ShipmentCustomsOfficeCode", "999888");
				AssertNoteValue(invoiceNotes, "ShipmentCustomsOfficeDescription", "TEST TR CUSTOMS OFFICE");

				AssertNoteValue(invoiceNotes, "InvoiceNote", @"
          Kıymet 391,09 USD, 14.250,13 TL
          391,09 USD x 0,00001 = 3.590,00 TL (ALT LİMİT), 3.590,00 TL
          Toplam = 3.590,00 TL Kur=36,4372
          Toplam = 3.590,00 TL
          Toplam = 18,00 TL
          Kıymet 1,00 AD
          1,00 AD");
			});
		}

		NoteType[] BuildInvoiceNotesWithXUT(string xutContent, bool createCommentCharge = false)
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var currencyHelper = new RefCurrencyTestHelper(Factory);
			currencyHelper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				if (createCommentCharge)
				{
					_ = Helper.TestObjectCreator.CommentChargeCode;
					Helper.Factory.Save();
				}

				var xmlDocumentFile = new XmlDocument();
				xmlDocumentFile.LoadXml(xutContent);
				var xutXmlAsText = xmlDocumentFile.OuterXml;
				var importer = new TransactionImporter();
				var transaction = importer.ImportUniversalTransactionFromXml(xutXmlAsText, Helper.Factory, false);
				var uInvoice = (TransactionInfo)transaction.Item1;
				uInvoice.TransactionReference = "TESTREFERENCE";
				var invoiceBatch = Helper.CreateInvoiceBatch(Helper.TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", Helper.TestObjectCreator.AUD, 1M, Helper.TestObjectCreator.ABIGAS));
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.SetTransaction(transactionBatch, uInvoice);
					var invoiceNotes = eInvoice.Invoice.Note;
					return invoiceNotes;
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNotesWithForeignCurrencyWithholdingInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMultilineInvoiceWithOnlyWithholding(Helper.TestObjectCreator.EUR, 9.0073m);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 20, invoiceNotes.Length);
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-50", "1801.46,900.73");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-30", "2522.04,756.61");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-50", "3242.62,1621.31");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-70", "12159.85,8511.90");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-8", "54043.80,4323.50");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "85569.35,15402.47");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "139613.15");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "159339.12");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "147548.57");
					});
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNotesWithForeignCurrencyVATandWithholdingInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMultilineInvoiceWithVATandWithholding(Helper.TestObjectCreator.EUR, 9.0073m);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 20, invoiceNotes.Length);
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-50", "1801.46,900.73");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-30", "2522.04,756.61");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-50", "3242.62,1621.31");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-70", "4863.94,3404.76");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-8", "67554.75,5404.38");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "90073.00,16213.13");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "157627.75");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "179245.26");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "172561.85");
					});
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNotesWithForeignCurrencyExemptInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMixedMultilineInvoiceWithExemption(Helper.TestObjectCreator.EUR, 9.0873m);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 18, invoiceNotes.Length);
						AssertNoteValueByParts(invoiceNotes, "InvoiceDescription", "Test Invoice");
						AssertNoteValueByParts(invoiceNotes, "BranchAddress", "Şube Adresi: Reşitpaşa Mah. Katar Cad. İTÜ Ayazağa Kamüsü Teknokent ARI 1 Binası No:2/5/7 Maslak");
						AssertNoteValueByParts(invoiceNotes, "CurrencyRate", "EUR Kuru: 1.00000");
						AssertNoteValueByParts(invoiceNotes, "CurrencyToString", "Yalnız onaltıbinikiyüzyirmi EUR");
						AssertNoteValueByParts(invoiceNotes, "InvoiceMessage", "Please contact us within 7 days should there be any discrepancies.");
						AssertNoteValueByParts(invoiceNotes, "ShipmentMasterWaybillNumber", "MAWBNUMBER1");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-301", "37257.93,0.00");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-351", "41801.58,0.00");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-8", "13630.95,1090.48");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "45436.50,8178.57");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "138126.96");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "147396.01");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "147396.01");
					});
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestInvoiceNotesWithVATWithholdingAndExemptInSingleInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMultilineInvoiceWithVATandDifferentWHTandEXEMPTandFREEVAT(Helper.TestObjectCreator.EUR, 9.0873m);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var invoiceNotes = eInvoice.Invoice.Note;

					CombineAssertions(() =>
					{
						AssertEquals("Count of notes", 22, invoiceNotes.Length);
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-50", "1817.46,908.73");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-8-606-30", "2544.44,763.33");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-50", "3271.42,1635.71");
						AssertNoteValueByParts(invoiceNotes, "LocalWHTTotalTax-18-606-70", "12267.85,8587.50");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-8", "81785.70,6542.86");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-18", "177202.35,31896.41");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-301", "116317.44,0.00");
						AssertNoteValueByParts(invoiceNotes, "LocalTotalTax-0-MSG3", "41801.58,0.00");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxExclusiveAmount", "417107.07");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-TaxInclusiveAmount", "455546.34");
						AssertNoteValueByParts(invoiceNotes, "LocalLegalMoney-PayableAmount", "443651.07");
					});
				}
			}
		}

		void AssertNoteValue(NoteType[] notes, string expectedValue)
		{
			var note = notes.FirstOrDefault(x => x.Value.Contains(expectedValue));
			AssertNotNull(expectedValue, note);
		}

		void AssertNoteValue(NoteType[] notes, string noteLabel, params string[] expectedValues)
		{
			var note = notes.FirstOrDefault(x => x.Value.Contains(noteLabel));
			AssertNotNull(noteLabel, note);
			AssertNotEquals(noteLabel, expectedValues.Length, 0);
			if (note != null)
			{
				expectedValues.ForEach(x => AssertContains(noteLabel, x, note.Value));
			}
		}

		void AssertNoteValueByParts(NoteType[] notes, string noteLabel, params string[] expectedValues)
		{
			var note = notes.FirstOrDefault(x => x.Value.Contains(noteLabel));
			AssertNotNull(noteLabel, note);

			var actualKeyAndValue = note.Value.Split('|');
			var actualKeyParts = actualKeyAndValue[0].Split('-');
			var actualValueParts = actualKeyAndValue[1].Split(',');
			var expectedKeyParts = noteLabel.Split('-');
			var expectedValueParts = expectedValues[0].Split(',');

			AssertEquals("Note key elements are missing or more", expectedKeyParts.Length, actualKeyParts.Length);
			AssertEquals("Note value elements are missing or more", expectedValueParts.Length, actualValueParts.Length);

			for (int i = 0; i < expectedKeyParts.Length; i++)
			{
				AssertEquals("Wrong key in" + actualKeyAndValue[0] + " note", expectedKeyParts[i], actualKeyParts[i]);
			}

			for (int i = 0; i < expectedValueParts.Length; i++)
			{
				AssertEquals("Wrong value in" + actualKeyAndValue[0] + " note", expectedValueParts[i], actualValueParts[i]);
			}
		}

		void AssertNoteIsNull(NoteType[] notes, string noteLabel, params string[] expectedValues)
		{
			var note = notes.FirstOrDefault(x => x.Value.Contains(noteLabel));
			AssertNull(noteLabel, note);
		}

		string UniversalTransactionSEA => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransactionSEA)}.xml");

		string UniversalTransactionSEA_StandaloneShipment => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransactionSEA_StandaloneShipment)}.xml");

		string UniversalTransactionSEA_ShipmentErrorAndNullValues => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransactionSEA_ShipmentErrorAndNullValues)}.xml");

		string UniversalTransaction_CFSLoadListConsolWithCFSShipment => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CFSLoadListConsolWithCFSShipment)}.xml");

		string UniversalTransaction_CFSLoadListConsolWithCFSShipmentV2 => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CFSLoadListConsolWithCFSShipmentV2)}.xml");

		string UniversalTransaction_CFSLoadListConsolWithPortTransport => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CFSLoadListConsolWithPortTransport)}.xml");

		string UniversalTransaction_CFSLoadListConsol_Standalone => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CFSLoadListConsol_Standalone)}.xml");

		string UniversalTransaction_LocalTransport_Standalone => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_LocalTransport_Standalone)}.xml");

		string UniversalTransactionDoesntThrowIfHasAdditionalReferenceWithoutCountryInfo => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransactionDoesntThrowIfHasAdditionalReferenceWithoutCountryInfo)}.xml");

		string UniversalTransaction_CustomsDeclaration => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CustomsDeclaration)}.xml");

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
