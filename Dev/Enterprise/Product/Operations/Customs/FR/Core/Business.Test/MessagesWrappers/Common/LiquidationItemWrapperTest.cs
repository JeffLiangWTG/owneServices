using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class LiquidationItemWrapperTest : TestCaseWithFactory
	{
		public void TestLiquidationWrapper()
		{
			SetUp();
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondUnitQty = "XXXY";
			invoiceLine.JI_CustomsSecondQuantity = 888;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine = entryHeader.MergedLines[0];

			var entryLineFee = entryLine.Fees.AddNew();
			entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.U165;
			entryLineFee.CF_Rate = 50;
			entryLineFee.CF_BaseValue = 1000;
			entryLineFee.CF_ChargeAmount = 500;

			var liquidationWrapper = new LiquidationItemWrapper(entryLineFee);

			entryLineFee.CF_MethodOfCalculation = "%";
			AssertEquals("The liquidation Article Number should be the one of the entry line", entryLine.CL_LineNumber, liquidationWrapper.ArticleNumber);
			AssertEquals("The liquidation TaxCode should be the entry line fee NationalFeeTypeCode", entryLineFee.NationalFeeTypeCode, liquidationWrapper.TaxDetail.Tax.TaxCode);
			AssertEquals("The liquidation TaxType should infered from the NationalFeeTypeCode", FeeTypeCodeConverter.GetFeeCodeTaxType(entryLineFee.NationalFeeTypeCode), liquidationWrapper.TaxDetail.Tax.TaxType);
			AssertEquals("The liquidation TaxAssessed should be the entry line fee  charge base value", entryLineFee.CF_BaseValue, liquidationWrapper.TaxDetail.Tax.TaxAssessed);
			AssertEquals("The liquidation TaxAmount should be the entry line fee charge amount", entryLineFee.CF_ChargeAmount, liquidationWrapper.TaxDetail.Tax.TaxAmount);
			AssertEquals("The liquidation TaxMethodOfPayment should be the entry line fee method of payment", entryLineFee.CF_MethodOfPayment, liquidationWrapper.TaxDetail.Tax.TaxMethodOfPayment);
			AssertEquals("The liquidation PortCode should be the entry line  fee port code", ZString.Empty, liquidationWrapper.TaxDetail.Tax.ChargePaymentOrDestinationID);
			AssertEquals("The liquidation Supplementary unit code should be empty for entry line fees using ad valorum calculation", ZString.Empty, liquidationWrapper.TaxDetail.SuppUnit.Code);
			AssertEquals("The liquidation Supplementary unit qualifier should be empty for entry line fees using ad valorum calculation", ZString.Empty, liquidationWrapper.TaxDetail.SuppUnit.Qualif);
			AssertEquals("The liquidation Supplementary unit quantity should be 0 for entry line fees using ad valorum calculation", ZDecimal.Zero, liquidationWrapper.TaxDetail.SuppUnit.Qty);

			entryLineFee.CF_MethodOfCalculation = "XXX";
			AssertEquals("The liquidation TaxMethodOfPayment should be the entry line fee method of payment", entryLineFee.CF_MethodOfPayment, liquidationWrapper.TaxDetail.Tax.TaxMethodOfPayment);
			AssertEquals("The liquidation Supplementary unit code should be the 3 first digits of the invoice line supplementary unit", invoiceLine.JI_CustomsSecondUnitQty.SubstringSafe(0, 3), liquidationWrapper.TaxDetail.SuppUnit.Code);
			AssertEquals("The liquidation Supplementary unit qualifier should be the 4th digit of the invoice line supplementary unit", invoiceLine.JI_CustomsSecondUnitQty.SubstringSafe(3, 1), liquidationWrapper.TaxDetail.SuppUnit.Qualif);
			AssertEquals("The liquidation Supplementary unit quantity should be the invoice line supplementary qty", invoiceLine.JI_CustomsSecondQuantity, liquidationWrapper.TaxDetail.SuppUnit.Qty);

			entryLineFee.NationalFeeTypeCode = "A325";
			AssertEquals("The liquidation TaxType should be infered from the NationalFeeTypeCode", FeeTypeCodeConverter.GetFeeCodeTaxType(entryLineFee.NationalFeeTypeCode), liquidationWrapper.TaxDetail.Tax.TaxType);
			AssertEquals("The liquidation Supplementary unit code should be empty for entry line fees based on third units", ZString.Empty, liquidationWrapper.TaxDetail.SuppUnit.Code);
			AssertEquals("The liquidation Supplementary unit qualifier should be empty for entry line fees based on third units", ZString.Empty, liquidationWrapper.TaxDetail.SuppUnit.Qualif);
			AssertEquals("The liquidation Supplementary unit quantity should be 0 for entry line fees based on third units", ZDecimal.Zero, liquidationWrapper.TaxDetail.SuppUnit.Qty);

			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			var entryHeaderCharge = entryHeader.Charges.AddNew();
			entryHeaderCharge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.U165;
			entryHeaderCharge.C1_ChargeAmount = 1832m;
			declaration.ChargePaymentOrDestinationID = "010";

			var liquidationWrapper2 = new LiquidationItemWrapper(entryHeaderCharge);
			AssertEquals("The liquidation Article Number should always be 1 for an entry header charge", (ZShort)1, liquidationWrapper2.ArticleNumber);
			AssertEquals("The liquidation TaxCode should be the entry header Charge Type", entryHeaderCharge.C1_ChargeType, liquidationWrapper2.TaxDetail.Tax.TaxCode);
			AssertEquals("The liquidation TaxType should always be 0 for an entry header charge", "0", liquidationWrapper2.TaxDetail.Tax.TaxType);
			AssertEquals("The liquidation TaxAssessed should be the entry header charge amount", entryHeaderCharge.C1_ChargeAmount, liquidationWrapper2.TaxDetail.Tax.TaxAssessed);
			AssertEquals("The liquidation TaxAmount should be the entry header charge amount", entryHeaderCharge.C1_ChargeAmount, liquidationWrapper2.TaxDetail.Tax.TaxAmount);
			AssertEquals("The liquidation TaxMethodOfPayment should be the entry header charge method of payment", entryHeaderCharge.C1_MethodOfPayment, liquidationWrapper2.TaxDetail.Tax.TaxMethodOfPayment);
			AssertEquals("The liquidation PortCode should be the declaration JE_RL_NKPortOfArrival", "010", liquidationWrapper2.TaxDetail.Tax.ChargePaymentOrDestinationID);
			AssertEquals("The liquidation Supplementary unit code should be empty for an entry header charge", ZString.Empty, liquidationWrapper2.TaxDetail.SuppUnit.Code);
			AssertEquals("The liquidation Supplementary unit qualifier should be empty for an entry header charge", ZString.Empty, liquidationWrapper2.TaxDetail.SuppUnit.Qualif);
			AssertEquals("The liquidation Supplementary unit quantity should be 0 for an entry header charge", ZDecimal.Zero, liquidationWrapper2.TaxDetail.SuppUnit.Qty);
		}
	}
}
