using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Asycuda
{
	[TestedType(typeof(TradeNetIShipmentDataProxy))]
	internal class TradeNetIShipmentDataProxyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldBeUploaded()
		{
			var regValue = new DecimalEffectiveDate
			{
				EffectiveDate = ZDateTime.Today.AddDays(-1),
				NewValue = 400
			};
			var declaration = SetupJobDeclaration();
			var proxy = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration) as IShipmentData;
			Assert(!proxy.ShouldBeUploaded);

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Snakey";
			template.P0_ProcessType = "BRK";
			var column1 = template.GenCustomColumnDefinitions.AddNew();
			column1.XC_Name = "Code 1";
			column1.XC_Type = AddOnColumnDataType.Codes.String;
			var column2 = template.GenCustomColumnDefinitions.AddNew();
			column2.XC_Name = "Charge 1";
			column2.XC_Type = AddOnColumnDataType.Codes.Decimal;

			var customPropertiesCollection = new UserDefinedPropertyCollection(declaration);
			customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
			foreach (var column in customPropertiesCollection)
			{
				if (column.Info.Type == typeof(ZString))
				{
					column.TrySetValue(declaration, new ZString("100"));
				}
				else
				{
					column.TrySetValue(declaration, new ZDecimal(10));
				}
			}

			ResetCustomBusinessObject();
			proxy = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			Assert(proxy.ShouldBeUploaded);

			return;

			void ResetCustomBusinessObject()
			{
				((ICustomFieldProvider)declaration).GetCustomBusinessObject(true);
			}
		}

		public void Test300000_004_GIROCode()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryNum = "MC123";
			cusEntryNum.CE_EntryIsSystemGenerated = true;
			cusEntryNum.CE_ParentID = entry.PK;
			cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			cusEntryNum.CE_EntryType = "CER";

			entry.EntryNumber = "MC123";
			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G01", receiptdata.TypeInfomation);

			entry.EntryNumber = "ME456";
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G01", receiptdata.TypeInfomation);

			entry.EntryNumber = "123";
			var entryPayInfo = entry.EntryPayInfos.AddNew();
			entryPayInfo.C9_PaymentReference = "123";
			entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Importer;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G04", receiptdata.TypeInfomation);

			entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Broker;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G05", receiptdata.TypeInfomation);

			entryPayInfo.C9_PaymentParty = "";
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G03", receiptdata.TypeInfomation);

			cusEntryNum.Delete();
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.GIROCode));
			AssertEquals("G03", receiptdata.TypeInfomation);
		}

		public void Test300000_005_MAWBNumber()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_MasterBill = "123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.MAWBNumber));
			AssertEquals("123", receiptdata.TypeInfomation);

			entry.Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			entry.Declaration.SG_OutwardMAWB = "456";

			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.MAWBNumber));
			AssertEquals("456", receiptdata.TypeInfomation);
		}

		[TestDate(2018, 1, 1)]
		public void Test300000_006_CycleDate_DOK()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			Assert(tradenet.ReceiptsData.All(x => x.TypeCode != GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.CycleDate)));

			declaration.Logs.AddNew(Events.CustomsEntryStatus, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, ZDateTimeOffset.Now);
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.CycleDate));
			AssertEquals("2018-01-01", receiptdata.TypeInfomation);
		}

		[TestDate(2018, 1, 1)]
		public void Test300000_006_CycleDate_DCP()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			Assert(tradenet.ReceiptsData.All(x => x.TypeCode != GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.CycleDate)));

			declaration.Logs.AddNew(Events.DeclarationQueued, IShipmentDataExtension.BISIReference + "VWG", ZDateTimeOffset.Now);
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.CycleDate));
			AssertEquals("2018-01-01", receiptdata.TypeInfomation);
		}

		public void Test300000_007_CycleNumber()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.CycleNumber));
			AssertEquals("01", receiptdata.TypeInfomation);
		}

		public void Test300000_008_OBCTaxCertificateNumber()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			Assert(tradenet.ReceiptsData.All(x => x.TypeCode != GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.OBCTaxCertificateNumber)));
		}

		public void Test300000_009_OBCPayDeclarationNumber()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			Assert(tradenet.ReceiptsData.All(x => x.TypeCode != GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.OBCPayDeclarationNumber)));

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryNum = "MC123";
			cusEntryNum.CE_EntryIsSystemGenerated = true;
			cusEntryNum.CE_ParentID = entry.PK;
			cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			cusEntryNum.CE_EntryType = "CER";
			entry.EntryNumber = "VWG";

			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.OBCPayDeclarationNumber));
			AssertEquals("VWG", receiptdata.TypeInfomation);
		}

		public void Test300000_010_InvoiceQuantity()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();
			invoiceline.JI_JZ = invoice.PK;

			IShipmentData tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var receiptdata = tradenet.ReceiptsData.First(x => x.TypeCode == GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode.InvoiceQuantity));
			AssertEquals("0001LOT", receiptdata.TypeInfomation);
		}

		[TestDate(2019, 2, 2)]
		public void Test100000_ExchangeRate()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ABC";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2019, 1, 1);
			rate.RE_ExpiryDate = new ZDateTime(2019, 1, 1);
			rate.RE_SellRate = 1.5413216m;

			var rate2 = currency.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "CUS";
			rate2.RE_StartDate = new ZDateTime(2019, 2, 2);
			rate2.RE_ExpiryDate = new ZDateTime(2019, 2, 2);
			rate2.RE_SellRate = 2.9851323m;
			Factory.Save();

			var declaration = SetupJobDeclaration();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 1, 1);
			invoice.JZ_RX_NKInvoice_Currency = "ABC";
			invoice.JZ_InvoiceCurrExRate = 3.1231323m;

			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals(3.1231323m, ((IShipmentData)tradenet).CustomsExchangeRate);

			invoice.JZ_InvoiceCurrExRate = 0m;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Should use JZ_ValuationDateOverride as valuation date when it has a valid date", 1.5413216m, ((IShipmentData)tradenet).CustomsExchangeRate);

			invoice.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice.JZ_InvoiceCurrExRate = 0m;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Should use today as valuation date when JZ_ValuationDateOverride is invalid", 2.9851323m, ((IShipmentData)tradenet).CustomsExchangeRate);

			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 3, 3);
			invoice.JZ_InvoiceCurrExRate = 0m;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Should use today as valuation date when trying to set a future date in JZ_ValuationDateOverride", 2.9851323m, ((IShipmentData)tradenet).CustomsExchangeRate);

			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals(0m, ((IShipmentData)tradenet).CustomsExchangeRate);
		}

		public void TestDutyType()
		{
			var dec = SetupJobDeclaration();
			var tradenetProxy = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, dec);
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.Dutiable, tradenetProxy.DutyType);
		}

		static ZString GetShipmentReceiptTypeCode(ShipmentReceiptTypeCode typeCode)
		{
			return ((int)typeCode).ToString().PadLeft(3, '0');
		}

		ZGuid pk;
		JobDeclaration SetupJobDeclaration()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_IsCancelled = ZBool.False;
			dec.JE_DeclarationReference = "dec ref";
			dec.JE_HouseBill = "housebill";
			dec.JE_MasterBill = "masterbill";
			dec.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
			dec.JE_RS_NKServiceLevel = "slv";
			dec.JE_TotalNoOfPacks = 110;
			dec.JE_GB = Env.CurrentBranchPK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "importer";
			importer.OH_FullName = "importer fullname";
			importer.MainAddress.OA_Address1 = "importer address 1";
			importer.MainAddress.OA_Address2 = "importer address 2";
			importer.MainAddress.OA_City = "importer city";
			importer.MainAddress.OA_Phone = "importer phone";
			pk = importer.PK;
			dec.JE_OH_Importer = importer.PK;

			Factory.Save();

			return dec;
		}

		public void TestImporterOrConsigneeMatchedOrgPK()
		{
			var declaration = SetupJobDeclaration();
			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("ImporterOrConsigneeMatchedOrgPK should be same as Importer PK", pk, tradenet.ImporterOrConsigneeMatchedOrgPK);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		public void TestFlightNo()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_VoyageFlightNo = "Flight123";
			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Flight No should be same as Declaration FlightNo", "Flight123", ((IShipmentData)tradenet).FlightNo);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		public void TestMasterBillNumber()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_MasterBill = "MasterBill123";
			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Master Bill Number should be same as Declaration Master Bill Number", "MasterBill123", ((IShipmentData)tradenet).MasterBillNumber);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		public void TestImportDate()
		{
			var declaration = SetupJobDeclaration();
			var date = new DateTime(2017, 8, 9);
			declaration.JE_DateOfArrival = date;
			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Import Date should be same as Declaration Date of Arrival", date, ((IShipmentData)tradenet).ImportDate);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		public void TestShipmentRef()
		{
			var declaration = SetupJobDeclaration();
			declaration.JE_HouseBill = "Bill123";
			var tradenet = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			AssertEquals("Shipment Reference should be same as Declaration House Bill", "Bill123", ((ILineKey)tradenet).ShipmentRef);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		public void TestCustoms()
		{
			var declaration = SetupJobDeclaration();
			var date = new DateTime(2017, 8, 9);
			var tradenet = new TradeNetIShipmentDataProxy("DOK", date, declaration);
			AssertEquals("Customs Status should be DOK", "DOK", ((IShipmentData)tradenet).CustomsStatus);
			AssertEquals("Customs Entry Date should be the same as constructor for Trade Net proxy class", date, ((IShipmentData)tradenet).CustomsEntryDate);
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			return new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore);
			UPETestHelper.TaxOrFeeTestSetUp(Factory);
		}
	}
}
