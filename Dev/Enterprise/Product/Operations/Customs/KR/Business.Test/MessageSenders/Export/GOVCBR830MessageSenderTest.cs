using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR830MessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR830Sender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR830SenderForTest(EntriesToSend, Factory) : new GOVCBR830Sender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CusEntryHeader> parents;

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
					TestOrgDataSetUpHelper.AddOrgContact(supplier, "최석구이태윤", true);
					TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");

					var declaration = Factory.New<JobDeclaration>();
					var invHeader = declaration.Invoices.AddNew();
					invHeader.JZ_RX_NKInvoice_Currency = "USD";
					invHeader.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine1 = entryHeader1.AllEntryLines.AddNew();
					var invLine1 = invHeader.InvoiceLines.AddNew();
					invLine1.JI_CL = entryLine1.PK;
					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine2 = entryHeader2.AllEntryLines.AddNew();
					var invLine2 = invHeader.InvoiceLines.AddNew();
					invLine2.JI_CL = entryLine2.PK;
					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine3 = entryHeader3.AllEntryLines.AddNew();
					var invLine3 = invHeader.InvoiceLines.AddNew();
					invLine3.JI_CL = entryLine3.PK;
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;

		public override void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entry.CH_Status);
				AssertEquals(ZShort.Zero, entry.CH_VersionID);
			}
		}

		[TestDate(2022, 12, 27, 12, 30, 45)]
		public void TestCH_EntrySubmittedDateSaveWhenSend()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals(new ZDateTime(2022, 12, 27, 12, 30, 45), entry.CH_EntrySubmittedDate);
			}
		}

		public void TestRoundDecimalPlaces()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			new TestDataSetupHelper(Factory).SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1234.56128m, usdCurrency);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 1002.8m;
			invoice.JZ_WeightUQ = "G";
			invoice.JZ_InvoiceAmount = 2000.7281m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100m;
			invoiceLine.JI_LinePrice = 100.15788m;
			invoiceLine.JI_NetWeight = 100.12346m;
			invoiceLine.JI_NetWeightUQ = "G";
			invoiceLine.JI_InvoiceQuantity = 1.23456;
			invoiceLine.JI_FormattedTariff = "1111.11-1111";
			invoiceLine.JI_CustomsUnitQty = "CT";
			invoiceLine.JI_CustomsQuantity = 1.23456;
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100.123m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 455.5m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100.333788m;
			invoiceLine.JI_LinePrice = 150.8542m;
			invoiceLine.JI_NetWeight = 100.4531m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 3.36479;
			invoiceLine.JI_FormattedTariff = "1111.11-1111";
			invoiceLine.JI_CustomsQuantity = 5.888123;
			invoiceLine.JI_CustomsUnitQty = "CT";

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100.333788m;
			invoiceLine.JI_LinePrice = 102.65411m;
			invoiceLine.JI_NetWeight = 100.45311m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 3.00000;
			invoiceLine.JI_FormattedTariff = "2222.22-2222";
			invoiceLine.JI_CustomsQuantity = 1.91233;
			invoiceLine.JI_CustomsUnitQty = "CT";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
			MessageSender.Send();

			var message = Parents.Single().Messages[0];
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var result = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR830.Declaration>(textReader);

				AssertEquals(354m, result.InvoiceAmount.Value);
				AssertEquals(1.003m, result.TotalGrossMassMeasure.Value);
				AssertEquals(1234.5613m, result.CurrencyExchange.RateNumeric.Value);
				AssertEquals(2000.73m, result.Consignment.ConsignmentItem.Commodity.ValueAmount.Value);
				AssertEquals(456m, result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount.Value);
				AssertEquals(100m, result.GoodsShipment.CustomsValuation.FreightChargeAmount.Value);
				AssertEquals(7m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.Value);
				AssertEquals(1.2346m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
				AssertEquals(81.130119m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].UnitPriceAmount.Value);
				AssertEquals(100.16m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].ValueAmount.Value);
				AssertEquals(3.3648m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].CountQuantity.Value);
				AssertEquals(44.831921m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].UnitPriceAmount.Value);
				AssertEquals(150.85m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].ValueAmount.Value);
				AssertEquals(100.553m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
				AssertEquals(2m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CountQuantity.Value);
				AssertEquals(3m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DetailedCommodity[0].CountQuantity.Value);
				AssertEquals(34.216667m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DetailedCommodity[0].UnitPriceAmount.Value);
				AssertEquals(102.65m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DetailedCommodity[0].ValueAmount.Value);
			}
		}

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;
	}

	class GOVCBR830SenderForTest : GOVCBR830Sender
	{
		public GOVCBR830SenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override ExportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
	}
}
