using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class WrapperHelperTest : TestCaseWithFactory
{
	public void TestAllLineDestinationsEqualToDeclaration()
	{
		var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		var invoiceLines = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>();
		invoiceLines.ForEach(x => x.ZG_CountryOfDestination = "NL");
		CombineAssertions(() =>
		{
			AssertEquals(true, entryHeader.AllLineDestinationsEqualToDeclaration());

			invoiceLines.First().ZG_CountryOfDestination = "SB";
			AssertEquals(false, entryHeader.AllLineDestinationsEqualToDeclaration());
		});
	}

	public void TestAllLineBuyersAreEmptyOrEqualToDeclaration()
	{
		var orgHeaderBuyer = WrapperTestHelper.CreateOrgHeader(Factory, "Buyer Full Name", "BUYER", "321654");
		var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		entryHeader.Declaration.ConsigneeAddressOrgPK = orgHeaderBuyer.PK;
		var invoiceLines = entryHeader.EntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();
		invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = orgHeaderBuyer.PK);
		CombineAssertions(() =>
		{
			AssertEquals("All line buyers are equal to declaration", true, entryHeader.AllLineBuyersAreEmptyOrEqualToDeclaration());

			var orgHeaderBuyer2 = WrapperTestHelper.CreateOrgHeader(Factory, "Buyer Full Name 22", "Buye", "651321");
			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = orgHeaderBuyer2.PK);
			AssertEquals("Line buyers are different from declaration", false, entryHeader.AllLineBuyersAreEmptyOrEqualToDeclaration());

			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = ZGuid.Empty);
			AssertEquals("All line buyers are empty", true, entryHeader.AllLineBuyersAreEmptyOrEqualToDeclaration());
		});
	}

	public void TestAllLineSellersAreEmptyOrEqualToDeclaration()
	{
		var orgHeaderSeller = WrapperTestHelper.CreateOrgHeader(Factory, "Seller Full Name", "SELLER", "123456");
		var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		entryHeader.Declaration.SellerOrgPK = orgHeaderSeller.PK;
		var invoiceLines = entryHeader.EntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();
		invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = orgHeaderSeller.PK);
		CombineAssertions(() =>
		{
			AssertEquals("All line sellers are equal to declaration", true, entryHeader.AllLineSellersAreEmptyOrEqualToDeclaration());

			var orgHeaderSeller2 = WrapperTestHelper.CreateOrgHeader(Factory, "Seller Full Name 2", "SELLRe", "654321");
			invoiceLines.First().SellerDocAddress.OrganisationPK = orgHeaderSeller2.PK;
			AssertEquals("Line sellers are different from declaration", false, entryHeader.AllLineSellersAreEmptyOrEqualToDeclaration());

			invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = ZGuid.Empty);
			AssertEquals("All line sellers are empty", true, entryHeader.AllLineSellersAreEmptyOrEqualToDeclaration());
		});
	}

	public void TestAllLineConsigneesAreEmptyOrEqualToDeclaration()
	{
		var orgHeaderConsignee = WrapperTestHelper.CreateOrgHeader(Factory, "Consignee Full Name", "CONSIGNEE", "123456");
		var orgAddressConsignee = WrapperTestHelper.CreateAddress(orgHeaderConsignee, "CON", "Rijksweg 102", "Deventer", "7201MG");
		var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		var invoiceLines = entryHeader.EntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();

		entryHeader.Declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddressConsignee.PK;
		invoiceLines.ForEach(x => x.JI_OA_ConsigneeAddress = orgAddressConsignee.PK);
		CombineAssertions(() =>
		{
			AssertEquals("All line consignees are equal to declaration", true, entryHeader.AllLineConsigneesAreEmptyOrEqualToDeclaration());

			var orgHeaderConsignee2 = WrapperTestHelper.CreateOrgHeader(Factory, "Consignee Full Name 2", "SELLRe", "654321");
			var orgAddressConsignee2 = WrapperTestHelper.CreateAddress(orgHeaderConsignee2, "CON", "Rijksweg 102", "Deventer", "7201MG");

			invoiceLines.First().JI_OA_ConsigneeAddress = orgAddressConsignee2.PK;
			AssertEquals("Line consignees are different from declaration", false, entryHeader.AllLineConsigneesAreEmptyOrEqualToDeclaration());

			invoiceLines.ForEach(x => x.JI_OA_ConsigneeAddress = ZGuid.Empty);
			AssertEquals("All line consignees are empty", true, entryHeader.AllLineConsigneesAreEmptyOrEqualToDeclaration());
		});
	}

	public void TestConvertTransportMode_1()
	{
		AssertEquals("1", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.Sea));
	}

	public void TestConvertTransportMode_2()
	{
		AssertEquals("2", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.Rail));
	}

	public void TestConvertTransportMode_3()
	{
		AssertEquals("3", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.Road));
	}

	public void TestConvertTransportMode_4()
	{
		AssertEquals("4", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.Air));
	}

	public void TestConvertTransportMode_5()
	{
		AssertEquals("5", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.Mail));
	}

	public void TestConvertTransportMode_7()
	{
		AssertEquals("7", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.FixedTransportInstallations));
	}

	public void TestConvertTransportMode_8()
	{
		AssertEquals("8", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.InlandWaterwayTransport));
	}

	public void TestConvertTransportMode_9()
	{
		AssertEquals("9", WrapperHelper.ConvertTransportMode(Core.Constants.TransportModes.OwnPropulsion));
	}

	public void TestConvertAuthorizationUsageCode_ReturnInput()
	{
		AssertEquals("ABC", WrapperHelper.ConvertAuthorizationUsageCode("ABC"));
	}

	public void TestConvertAuthorizationUsageCode_OPO()
	{
		AssertEquals("C019", WrapperHelper.ConvertAuthorizationUsageCode("OPO"));
	}

	public void TestConvertAuthorizationUsageCode_AEOC()
	{
		AssertEquals("C501", WrapperHelper.ConvertAuthorizationUsageCode("AEOC"));
	}

	public void TestConvertAuthorizationUsageCode_AEOS()
	{
		AssertEquals("C502", WrapperHelper.ConvertAuthorizationUsageCode("AEOS"));
	}

	public void TestConvertAuthorizationUsageCode_AEOF()
	{
		AssertEquals("C503", WrapperHelper.ConvertAuthorizationUsageCode("AEOF"));
	}

	public void TestConvertAuthorizationUsageCode_CVA()
	{
		AssertEquals("C504", WrapperHelper.ConvertAuthorizationUsageCode("CVA"));
	}

	public void TestConvertAuthorizationUsageCode_CGU()
	{
		AssertEquals("C505", WrapperHelper.ConvertAuthorizationUsageCode("CGU"));
	}

	public void TestConvertAuthorizationUsageCode_DPO()
	{
		AssertEquals("C506", WrapperHelper.ConvertAuthorizationUsageCode("DPO"));
	}

	public void TestConvertAuthorizationUsageCode_REP()
	{
		AssertEquals("C507", WrapperHelper.ConvertAuthorizationUsageCode("REP"));
	}

	public void TestConvertAuthorizationUsageCode_REM()
	{
		AssertEquals("C508", WrapperHelper.ConvertAuthorizationUsageCode("REM"));
	}

	public void TestConvertAuthorizationUsageCode_TST()
	{
		AssertEquals("C509", WrapperHelper.ConvertAuthorizationUsageCode("TST"));
	}

	public void TestConvertAuthorizationUsageCode_SDE()
	{
		AssertEquals("C512", WrapperHelper.ConvertAuthorizationUsageCode("SDE"));
	}

	public void TestConvertAuthorizationUsageCode_CCL()
	{
		AssertEquals("C513", WrapperHelper.ConvertAuthorizationUsageCode("CCL"));
	}

	public void TestConvertAuthorizationUsageCode_EIR()
	{
		AssertEquals("C514", WrapperHelper.ConvertAuthorizationUsageCode("EIR"));
	}

	public void TestConvertAuthorizationUsageCode_TEA()
	{
		AssertEquals("C516", WrapperHelper.ConvertAuthorizationUsageCode("TEA"));
	}

	public void TestConvertAuthorizationUsageCode_CWP()
	{
		AssertEquals("C517", WrapperHelper.ConvertAuthorizationUsageCode("CWP"));
	}

	public void TestConvertAuthorizationUsageCode_CW2()
	{
		AssertEquals("C519", WrapperHelper.ConvertAuthorizationUsageCode("CW2"));
	}

	public void TestConvertAuthorizationUsageCode_AWB()
	{
		AssertEquals("C526", WrapperHelper.ConvertAuthorizationUsageCode("AWB"));
	}

	public void TestConvertAuthorizationUsageCode_IPO()
	{
		AssertEquals("C601", WrapperHelper.ConvertAuthorizationUsageCode("IPO"));
	}

	public void TestConvertAuthorizationUsageCode_BTI()
	{
		AssertEquals("C626", WrapperHelper.ConvertAuthorizationUsageCode("BTI"));
	}

	public void TestConvertAuthorizationUsageCode_BOI()
	{
		AssertEquals("C627", WrapperHelper.ConvertAuthorizationUsageCode("BOI"));
	}

	public void TestConvertAuthorizationUsageCode_BES1()
	{
		AssertEquals("C990", WrapperHelper.ConvertAuthorizationUsageCode("BES1"));
	}

	public void TestConvertAuthorizationUsageCode_BES2()
	{
		AssertEquals("D019", WrapperHelper.ConvertAuthorizationUsageCode("BES2"));
	}

	public void TestConvertAuthorizationUsageCode_EUS()
	{
		AssertEquals("N990", WrapperHelper.ConvertAuthorizationUsageCode("EUS"));
	}

	public void TestIsDestinationSpecified()
	{
		var list = new string[]
		{
			NLConstants.EntryStyles.ExportReExport,
			NLConstants.EntryStyles.SpecialProcessing, NLConstants.EntryStyles.UnionGoods,
			NLConstants.EntryStyles.SpecialFiscalTerritory, NLConstants.EntryStyles.ExportDeclarationC1,
			NLConstants.EntryStyles.DeclarationForEndUse, NLConstants.EntryStyles.DeclarationForCustWarehouse,
			NLConstants.EntryStyles.DeclarationTemporaryAdmission, NLConstants.EntryStyles.DeclarationInwardProcessing,
			NLConstants.EntryStyles.ImportSpecialFiscalTerritoriesDeclaration, NLConstants.EntryStyles.ImportSimplifiedDeclaration
		};

		CombineAssertions(() =>
		{
			AssertEquals("Invalid", false, WrapperHelper.IsDestinationSpecified("XX"));

			foreach (var code in list)
			{
				AssertEquals(code, true, WrapperHelper.IsDestinationSpecified(code));
			}
		});
	}
}
