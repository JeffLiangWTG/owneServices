using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Testing.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class EuCommonGoodsItemsIntegratorTest : BaseCommonGoodsItemsIntegratorTest<JobDeclaration, CusEntryHeader, EuCommonGoodsItemsIntegrator, NctsHeaderToAttachCollection>
	{
		protected override (Customs.Business.CusEntryHeader, BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2) CreateEntryWithTwoInvoiceLines(BusinessObjectFactory factory)
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "AR1";
			var cusRate = foreignCurrency.ExchangeRates.AddNew();
			cusRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			cusRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusRate.RE_SellRate = 0.85m;

			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "AR1";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryinstruction.PK;
			entryHeader.CH_CEI_Instruction = entryinstruction.PK;

			entryHeader.CH_BGMReference = "123ABC";
			entryHeader.MovementReferenceNumberSetter("MRN123");

			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Description = "Goods Description";
			invoiceLine1.JI_Weight = 1.5M;
			invoiceLine1.JI_NetWeight = 2.9M;
			invoiceLine1.JI_WeightUQ = Weight.Kilograms;
			invoiceLine1.JI_NetWeightUQ = Weight.Kilograms;
			invoiceLine1.JI_Tariff = "1202300000";
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.ZG_CountryOfSupply = CountryCodes.Portugal;
			invoiceLine1.ZG_CountryOfDestination = CountryCodes.UnitedKingdom;
			invoiceLine1.JI_CustomsSecondQuantity = 1.2m;
			invoiceLine1.JI_CustomsSecondUnitQty = "NAR";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryinstruction.PK;
			invoiceLine2.FillWithValidTestData();

			return (entryHeader, invoiceLine1, invoiceLine2);
		}

		protected override void AssertExpectedPropertiesOnGoodsItem(ICommonGoodsItem goodsItem, BaseJobComInvoiceLine line1)
		{
			AssertEquals("GoodsDescription", "Goods Description", goodsItem.GoodsDescription);
			AssertEquals("GrossMass", 1.5m, goodsItem.GrossMass);
			AssertEquals("NetMass", 2.9m, goodsItem.NetMass);
			AssertEquals("GrossMassUnit", Weight.Kilograms, goodsItem.GrossMassUnit);
			AssertEquals("NetMassUnit", Weight.Kilograms, goodsItem.NetMassUnit);
			AssertEquals("CommodityCode", "1202.30.00 00", goodsItem.CommodityCode);
			AssertEquals("DispatchCountry", CountryCodes.Portugal, goodsItem.DispatchCountry);
			AssertEquals("DestinationCountry", CountryCodes.UnitedKingdom, goodsItem.DestinationCountry);
			AssertEquals("Prerequisite: Invoice exchange rate is 0.85", 0.85m, line1.InvoiceHeader.JZ_InvoiceCurrExRate);
			AssertEquals("Value", 117.65m, goodsItem.Value);
			AssertEquals("JobReference", ZString.Empty, goodsItem.JobReference);
			AssertEquals("EntryNumber", ((ZString)"Z", (ZString)"N830", (ZString)"MRN123", (ZInt?)null), goodsItem.EntryNumber);
			AssertEquals("SupplementaryQuantity", 1.2m, goodsItem.SupplementaryQuantity);
			AssertEquals("SupplementaryQuantityUnit", "NAR", goodsItem.SupplementaryQuantityUnit);
		}

		protected override ICommonGoodsItem CreateCommonGoodsItem()
		{
			return new CommonGoodsItem()
			{
				GoodsDescription = "description",
				GrossMass = 12000m,
				NetMass = 10m,
				GrossMassUnit = "G",
				NetMassUnit = "KG",
				CommodityCode = "1202.30.00 01",
				Value = 8m,
				JobReference = "jobreference",
				EntryNumber = (PreviousDocumentClassList.Codes.PreviousDocument, SupportingDocumentTypes.Other, "MRN123", 0),
				DispatchCountry = "FR",
				DestinationCountry = "US"
			};
		}

		protected override void AssertExpectedPropertiesOnInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			CombineAssertions("Value should have been copied from goods item to the invoiceline.", () =>
			{
				AssertEquals("GoodsDescription", "description", invoiceLine.JI_Description);
				AssertEquals("GrossMass", 12000m, invoiceLine.JI_Weight);
				AssertEquals("NetMass", 10m, invoiceLine.JI_NetWeight);
				AssertEquals("commodity code", "1202.30.00 01", invoiceLine.JI_FormattedTariff);
				AssertEquals("GrossMassUnit", "G", invoiceLine.JI_WeightUQ);
				AssertEquals("NetMassUnit", "KG", invoiceLine.JI_NetWeightUQ);
				AssertEquals("DestinationCountry", "US", ((JobComInvoiceLine)invoiceLine).ZG_CountryOfDestination);
				AssertEquals("DispatchCountry", "FR", ((JobComInvoiceLine)invoiceLine).ZG_CountryOfSupply);
				AssertEquals("Value", 8m, invoiceLine.JI_LinePrice);
			});

			CombineAssertions("2 Previous documents should have been created in the invoiceline.", () =>
			{
				AssertEquals("Previous Document count", 2, ((JobComInvoiceLine)invoiceLine).PreviousDocuments.Count);

				var docTarget = ((JobComInvoiceLine)invoiceLine).PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_ReferenceNumber == "jobreference");
				AssertEquals("Previous Document CSI_Code", "ZZZ", docTarget.CSI_Code);
				AssertEquals("Previous Document CSI_SubType", "Z", docTarget.CSI_SubType);
				AssertEquals("Previous Document CSI_ReferenceNumber", "jobreference", docTarget.CSI_ReferenceNumber);

				var docMrn = ((JobComInvoiceLine)invoiceLine).PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_ReferenceNumber == "MRN123");
				AssertEquals("Previous Document CSI_Code", "ZZZ", docMrn.CSI_Code);
				AssertEquals("Previous Document CSI_SubType", "Z", docMrn.CSI_SubType);
				AssertEquals("Previous Document CSI_ReferenceNumber", "MRN123", docMrn.CSI_ReferenceNumber);
			});
		}
	}
}
