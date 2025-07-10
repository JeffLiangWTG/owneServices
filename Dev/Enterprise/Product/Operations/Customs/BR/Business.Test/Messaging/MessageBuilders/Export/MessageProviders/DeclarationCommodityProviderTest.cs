using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationCommodityProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGovernmentAgencyGoodsItemProvider()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "02011001", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			var refCusTariffBRCharacteristic = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "AA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceAmount = 10m;
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			invLine.JI_Tariff = "02011001";
			invLine.JI_LineNo = 1;
			invLine.JI_LinePrice = 10m;
			invLine.JI_InvoiceQuantity = 10m;
			invLine.JI_InvoiceUQ = "KG";
			invLine.JI_CustomsQuantity = 10m;
			invLine.ComplementaryDescription = "COMPLEMENTARY DESCRIPTION";
			invLine.JI_Description = "DESCRIPTION";

			var attributeItem = invLine.Attributes[0];
			attributeItem.TariffProfileQuestion = TariffProfileQuestion.New(refCusTariffBRCharacteristic);

			var logisticInvoice = invLine.ElectronicLogisticInvoiceCollection.AddNew();
			logisticInvoice.CSI_ReferenceNumber = "00000000000000000000000000000000000000000000";
			logisticInvoice.CSI_LineNo = 1;
			logisticInvoice.CSI_Quantity = 5m;

			var commodity = new DeclarationCommodityProvider(invLine);

			AssertEquals("ID should be", ZString.Empty, commodity.ID);
			AssertEquals("GoodsDescription should be", "DESCRIPTION", commodity.GoodsDescription);
			AssertEquals("ComplementaryDescription should be", "COMPLEMENTARY DESCRIPTION", commodity.ComplementaryDescription);
			AssertEquals("TariffCode should be", "02011001", commodity.TariffCode);
			AssertEquals("SequenceNumeric should be", 1, commodity.SequenceNumeric);
			AssertEquals("ReferenceInvoices count should be", 1, commodity.ReferenceInvoices.Count());
			AssertEquals("InvoiceAmount should be", 10m, commodity.InvoiceAmount);
			AssertEquals("ProductCharacteristics count should be", 1, commodity.ProductCharacteristics.Count());
		}
	}
}
