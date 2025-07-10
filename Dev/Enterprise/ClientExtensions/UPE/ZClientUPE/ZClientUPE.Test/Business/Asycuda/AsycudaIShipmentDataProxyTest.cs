using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Asycuda.Testing
{
	[TestedType(typeof(AsycudaIShipmentDataProxy))]
	internal partial class AsycudaIShipmentDataProxyTest : NonPersistentBusinessObjectTestCase
	{
		public AsycudaBill SetupAsycudaBill(ZGuid id, params ZString[] packedItemValues)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "MAN0000123";
			header.AMA_Voyage = "QF1234";

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			bill.ABL_OA_Consignee = id;
			bill.ABL_RX_NKCustomsValueCurrency = "SGD";

			bill.CustomsEntryNumber = "INC123";
			bill.CustomsEntryNumberType = "ASY";

			foreach (var value in packedItemValues)
			{
				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItem;
				packedItem.API_PackStatus = value;
			}

			return bill;
		}

		public void Test100000_CustomsEntryNumber()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			var asycudaProxy = (AsycudaIShipmentDataProxy)GetNewBusinessObject();
			AssertEquals("INC123", ((IShipmentData)asycudaProxy).CustomsEntryNumber);

			var packedItem = bill.Packs[0].PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "ME51233314";
			tradnetPermit.CE_EntryType = "TNP";
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals("ME51233314", ((IShipmentData)asycudaProxy).CustomsEntryNumber);
		}

		public void Test100000_CustomsValue()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid());
			bill.ABL_CustomsValue = 742.11m;
			var pack1 = bill.Packs.AddNew();
			var packedItem1 = pack1.PackedItem;
			packedItem1.API_CustomsValue = 519m;
			var pack2 = bill.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_CustomsValue = 223.11;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(742.11m, asycudaProxy.CustomsValue);
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

			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.CycleDate = new ZDateTime(2019, 1, 1);
			bill.ABL_RX_NKFreightValueCurrency = "ABC";

			var asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(1.5413216m, ((IShipmentData)asycudaProxy).CustomsExchangeRate);

			bill.CycleDate = ZDateTime.Empty;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(2.9851323m, ((IShipmentData)asycudaProxy).CustomsExchangeRate);

			bill.CycleDate = new ZDateTime(2019, 3, 3);
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(0m, ((IShipmentData)asycudaProxy).CustomsExchangeRate);

			bill.ABL_RX_NKFreightValueCurrency = ZString.Empty;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(0m, ((IShipmentData)asycudaProxy).CustomsExchangeRate);
		}

		public void Test400000_CheckOutput()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid());
			bill.ABL_CustomsValue = 742.11m;

			var pack1 = bill.Packs.AddNew();
			var packedItem1 = pack1.PackedItem;
			AssertEquals("PreCondition", ZString.Empty, packedItem1.API_Tariff);

			var pack2 = bill.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			AssertEquals("PreCondition", ZString.Empty, packedItem2.API_Tariff);

			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var commoditiesData = asycudaProxy.CommoditiesData.ToList();
			AssertEquals(2, commoditiesData.Count);
		}

		public void Test300000_009_CustomsEntryNumber()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.CustomsEntryNumber = "ASY813481";
			bill.CustomsEntryNumberType = "TNP";
			bill.CustomsEntryNumber = "INC123";
			bill.CustomsEntryNumberType = "ASY";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "ME51233314";
			tradnetPermit.CE_EntryType = "TNP";

			UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = 200;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var entryNumberReceiptData = asycudaProxy.ReceiptsData.ToList().First(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.OBCPayDeclarationNumber).ToString().PadLeft(3, '0'));
			AssertEquals("INC123", entryNumberReceiptData.TypeInfomation);
		}

		public void Test300000_006_CycleDate()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.CustomsEntryNumber = "ASY813481";
			bill.CustomsEntryNumberType = "TNP";
			bill.CycleDate = new ZDateTime(2017, 11, 2);
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "ME51233314";
			tradnetPermit.CE_EntryType = "TNP";

			UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = 200;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var cycDataReceiptData = asycudaProxy.ReceiptsData.ToList().First(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.CycleDate).ToString().PadLeft(3, '0'));
			AssertEquals("2017-11-02", cycDataReceiptData.TypeInfomation);
		}

		public void Test300000_007_CycleNumber()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.CustomsEntryNumber = "ASY813481";
			bill.CustomsEntryNumberType = "TNP";
			bill.CycleNumber = "12";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "ME51233314";
			tradnetPermit.CE_EntryType = "TNP";

			UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = 200;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var cycNumberReceiptData = asycudaProxy.ReceiptsData.ToList().First(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.CycleNumber).ToString().PadLeft(3, '0'));
			AssertEquals("12", cycNumberReceiptData.TypeInfomation);
		}

		public void Test300000_004_GIROCode()
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.ABL_CustomsValue = 500m;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			bill.DutyAmount = 100m;
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryType = "TNP";

			IShipmentData asycudaProxy;
			ShipmentReceiptData gIROCodeData;

			foreach (var type in new AsycudaIShipmentDataProxy(bill).GetPermitTypesNeedToBeAssignedWithGSTExempted())
			{
				tradnetPermit.CE_EntryNum = type + "51233314";
				asycudaProxy = new AsycudaIShipmentDataProxy(bill);
				gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
				AssertEquals("Only support types of 2 characters now", ReceiptGIROCodeTypes.GSTExempted, gIROCodeData.TypeInfomation);
			}

			tradnetPermit.CE_EntryNum = "TP51233314";
			var entryPayInfo = Factory.New<Customs.Business.CusEntryPayInfo>();
			entryPayInfo.C9_PaymentReference = "TP51233314";
			entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Broker;

			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTPaidDeducted, gIROCodeData.TypeInfomation);

			bill.SG_PartyStatus = ZString.Empty;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTPaidThruGIRO, gIROCodeData.TypeInfomation);

			entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Importer;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTPaidDeducted, gIROCodeData.TypeInfomation);

			entryPayInfo.Delete();
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTPaidAtCheckPoint, gIROCodeData.TypeInfomation);

			bill.ABL_CustomsValue = ZDecimal.Zero;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTPaidAtCheckPoint, gIROCodeData.TypeInfomation);

			bill.DutyAmount = ZDecimal.Zero;
			asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			gIROCodeData = asycudaProxy.ReceiptsData.FirstOrDefault(x => x.TypeCode == ((int)ShipmentReceiptTypeCode.GIROCode).ToString().PadLeft(3, '0'));
			AssertEquals(ReceiptGIROCodeTypes.GSTWaived, gIROCodeData.TypeInfomation);
		}

		public void TestValuesAreInProxyClass()
		{
			ZGuid id = new ZGuid();
			var bill = SetupAsycudaBill(id, "CR");

			AsycudaIShipmentDataProxy asycudaProxy = (AsycudaIShipmentDataProxy)GetNewBusinessObject();

			AssertEquals("Should set the Shipment Ref to proxy class", "123", ((ILineKey)asycudaProxy).ShipmentRef);
			AssertEquals("Should set the Shipment Ref to proxy class", "MAN0000123", ((IShipmentData)asycudaProxy).MasterBillNumber);
			AssertEquals("Should set the ImporterOrConsigneeMatchedOrgPK to proxy class", id, asycudaProxy.ImporterOrConsigneeMatchedOrgPK);
			AssertEquals("Should set the Flight No (from Header) to proxy class", "QF1234", ((IShipmentData)asycudaProxy).FlightNo);
			AssertEquals("Should set the Customs Status (from AsycudaPackedItem) to proxy class", "CR", ((IShipmentData)asycudaProxy).CustomsStatus);
			AssertEquals("Should set the DVC Currency Code to proxy class", "SGD", ((IShipmentData)asycudaProxy).DVCCurrencyCode);
			AssertEquals("Should set the Customs Entry Number to proxy class", "INC123", ((IShipmentData)asycudaProxy).CustomsEntryNumber);
		}

		public void TestCustomsStatus()
		{
			var id = new ZGuid();
			var bill = SetupAsycudaBill(id, "CR");
			var asycudaProxy = (IShipmentData)new AsycudaIShipmentDataProxy(bill);
			AssertEquals("Should set the Customs Status (from AsycudaPackedItem) in proxy class to CR", "CR", asycudaProxy.CustomsStatus);

			var bill2 = SetupAsycudaBill(id, "CR", "IP");
			var asycudaProxy2 = (IShipmentData)new AsycudaIShipmentDataProxy(bill2);
			AssertEquals("1 IP and 1 CR should set the Customs Status (from AsycudaPackedItem) in proxy class to IP", "IP", asycudaProxy2.CustomsStatus);

			var bill3 = SetupAsycudaBill(id, "CR", "CR", "CR", "CR", "CR", "IP");
			var asycudaProxy3 = (IShipmentData)new AsycudaIShipmentDataProxy(bill3);
			AssertEquals("1 IP and 5 CR should still set the Customs Status (from AsycudaPackedItem) in proxy class to IP", "IP", asycudaProxy3.CustomsStatus);
		}

		public void TestReceiptsData()
		{
			var id = new ZGuid();
			var bill = SetupAsycudaBill(id, "CR");
			bill.CustomsEntryNumber = "ASY813481";
			bill.CustomsEntryNumberType = "TNP";
			bill.CustomsEntryNumber = "INC123";
			bill.CustomsEntryNumberType = "ASY";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "ME51233314";
			tradnetPermit.CE_EntryType = "TNP";

			UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = 200;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var receiptsDataList = asycudaProxy.ReceiptsData.ToList();
			CombineAssertions(() =>
			{
				AssertEquals(5, receiptsDataList.Count);
				AssertEquals("004", receiptsDataList[0].TypeCode);
				AssertEquals(ReceiptGIROCodeTypes.GSTExempted, receiptsDataList[0].TypeInfomation);
				AssertEquals("005", receiptsDataList[1].TypeCode);
				AssertEquals("MAN0000123", receiptsDataList[1].TypeInfomation);
				AssertEquals("008", receiptsDataList[2].TypeCode);
				AssertEquals("SG-0000200", receiptsDataList[2].TypeInfomation);
				AssertEquals(201, UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber);
				AssertEquals("009", receiptsDataList[3].TypeCode);
				AssertEquals("INC123", receiptsDataList[3].TypeInfomation);
				AssertEquals("010", receiptsDataList[4].TypeCode);
				AssertEquals("0001LOT", receiptsDataList[4].TypeInfomation);
			});
		}

		public void TestReceiptsDataIncludesEntryPayData()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryPayment = entryHeader.EntryPayInfos.AddNew();
			entryPayment.C9_PaymentAmount = 575.00m;
			entryPayment.C9_PaymentReference = "DP7K004960D";
			entryPayment.C9_PaymentDate = ZDateTime.Today;
			entryPayment.C9_PaymentParty = UPEOrgRematch.OrgTypes.Importer;

			var bill = SetupAsycudaBill(new ZGuid(), "CR");
			bill.ABL_CustomsValue = 3750m;
			bill.CustomsEntryNumber = "ASY813481";
			bill.CustomsEntryNumberType = "TNP";
			bill.CustomsEntryNumber = "INC123";
			bill.CustomsEntryNumberType = "ASY";

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tradnetPermit = packedItem.CustomsEntryNumbers.AddNew();
			tradnetPermit.CE_EntryNum = "DP7K004960D";
			tradnetPermit.CE_EntryType = "TNP";

			UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber = 200;
			IShipmentData asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var receiptsDataList = asycudaProxy.ReceiptsData.ToList();
			CombineAssertions(() =>
			{
				AssertEquals(5, receiptsDataList.Count);
				AssertEquals("004", receiptsDataList[0].TypeCode);
				AssertEquals(ReceiptGIROCodeTypes.GSTPaidDeducted, receiptsDataList[0].TypeInfomation);
				AssertEquals("005", receiptsDataList[1].TypeCode);
				AssertEquals("MAN0000123", receiptsDataList[1].TypeInfomation);
				AssertEquals("008", receiptsDataList[2].TypeCode);
				AssertEquals("SG-0000200", receiptsDataList[2].TypeInfomation);
				AssertEquals(201, UPEDataRegistry.Instance.BISIOBCTaxCertificateNumber);
				AssertEquals("009", receiptsDataList[3].TypeCode);
				AssertEquals("INC123", receiptsDataList[3].TypeInfomation);
				AssertEquals("010", receiptsDataList[4].TypeCode);
				AssertEquals("0001LOT", receiptsDataList[4].TypeInfomation);
			});
		}

		public void TestDutyType()
		{
			var bill = SetupAsycudaBill(new ZGuid());
			var asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.Dutiable, ((IShipmentData)asycudaProxy).DutyType);
		}

		public void TestShouldBeUploaded()
		{
			CombineAssertions(() =>
			{
				AssertShouldBeUploaded("Low Value, ABL_PrepaidCollect=Empty, DutyAmount=0, should NOT be uploaded", true, ZString.Empty, ZDecimal.Zero, false, false);
				AssertShouldBeUploaded("Low Value, ABL_PrepaidCollect=F/C, DutyAmount=0, should be uploaded", true, BillingTermsCodeDescriptionPairList.Codes.FreightCollect, ZDecimal.Zero, false, true);
				AssertShouldBeUploaded("Low Value, ABL_PrepaidCollect=F/B, DutyAmount=1, should be uploaded", true, BillingTermsCodeDescriptionPairList.Codes.FreeBorder, 1m, false, true);
				AssertShouldBeUploaded("Low Value, ABL_PrepaidCollect=F/C, DutyAmount=1, should be uploaded", true, BillingTermsCodeDescriptionPairList.Codes.FreightCollect, 1m, false, true);
				AssertShouldBeUploaded("High Value, ABL_PrepaidCollect=Empty, DutyAmount=0, should be uploaded", false, ZString.Empty, ZDecimal.Zero, false, true);
				AssertShouldBeUploaded("High Value, ABL_PrepaidCollect=F/C, DutyAmount=0, should be uploaded", false, BillingTermsCodeDescriptionPairList.Codes.FreightCollect, ZDecimal.Zero, false, true);
				AssertShouldBeUploaded("High Value, ABL_PrepaidCollect=P/P, DutyAmount=1, should be uploaded", false, BillingTermsCodeDescriptionPairList.Codes.Prepaid, 1m, false, true);
				AssertShouldBeUploaded("High Value, ABL_PrepaidCollect=F/C, DutyAmount=1, should be uploaded", false, BillingTermsCodeDescriptionPairList.Codes.FreightCollect, 1m, false, true);
				AssertShouldBeUploaded("High Value, ABL_PrepaidCollect=Empty, DutyAmount=0, Has Custom Fields, should be uploaded", false, ZString.Empty, ZDecimal.Zero, true, true);
			});
		}

		public void TestIsHighValue()
		{
			var bill = SetupAsycudaBill(new ZGuid());
			var asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			var isHighValueInfo = typeof(AsycudaIShipmentDataProxy).GetProperty("IsHighValue", BindingFlags.Instance | BindingFlags.NonPublic);

			bill.ABL_CustomsValue = 400m;
			AssertEquals("400 is not high value.", false, isHighValueInfo.GetValue(asycudaProxy));

			bill.ABL_CustomsValue = 400.01m;
			AssertEquals("400.01 is high value.", true, isHighValueInfo.GetValue(asycudaProxy));
		}

		void AssertShouldBeUploaded(ZString message, ZBool lowValue, ZString prepaidCollectValue, ZDecimal dutyAmount, ZBool hasCustomFields, ZBool shouldBeUploadedResult)
		{
			IShipmentData asycudaProxy = SetupShipmentDataProxyForBill(lowValue, bill => bill.SetUserDefinedValue(Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.BillingTerms, prepaidCollectValue), dutyAmount, hasCustomFields);
			AssertEquals(message.Replace("ABL_PrepaidCollect", "Customized Column 'Billing Terms' "), shouldBeUploadedResult, asycudaProxy.ShouldBeUploaded);

			asycudaProxy = SetupShipmentDataProxyForBill(lowValue, bill => bill.ABL_PrepaidCollect = prepaidCollectValue, dutyAmount, hasCustomFields);
			AssertEquals(message, shouldBeUploadedResult, asycudaProxy.ShouldBeUploaded);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPETestHelper.TaxOrFeeTestSetUp(Factory);
		}

		AsycudaIShipmentDataProxy SetupShipmentDataProxyForBill(ZBool lowValue, Action<AsycudaBill> prepaidCollectSetter, ZDecimal dutyAmount, ZBool hasCustomFields)
		{
			var bill = SetupAsycudaBill(ZGuid.NewZGuid(), "CR");
			bill.ABL_CustomsValue = lowValue ? 399m : 401m;
			prepaidCollectSetter?.Invoke(bill);
			bill.DutyAmount = dutyAmount;
			bill.ABL_OA_Consignee = ZGuid.Empty;

			if (hasCustomFields)
			{
				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_Name = "Snakey" + ZGuid.NewZGuid();
				template.P0_ProcessType = "GMB";
				var column1 = template.GenCustomColumnDefinitions.AddNew();
				column1.XC_Name = "Code 1";
				column1.XC_Type = AddOnColumnDataType.Codes.String;
				var column2 = template.GenCustomColumnDefinitions.AddNew();
				column2.XC_Name = "Charge 1";
				column2.XC_Type = AddOnColumnDataType.Codes.Decimal;

				var customPropertiesCollection = new UserDefinedPropertyCollection(bill);
				customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
				foreach (var column in customPropertiesCollection)
				{
					if (column.Info.Type == typeof(ZString))
					{
						column.TrySetValue(bill, new ZString("100"));
					}
					else
					{
						column.TrySetValue(bill, new ZDecimal(10));
					}
				}
			}

			Factory.Save();
			var newBill = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			return new AsycudaIShipmentDataProxy(newBill);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ZGuid id = new ZGuid();
			var bill = SetupAsycudaBill(id, "CR");
			AsycudaIShipmentDataProxy asycudaProxy = new AsycudaIShipmentDataProxy(bill);
			return asycudaProxy;
		}
	}
}
