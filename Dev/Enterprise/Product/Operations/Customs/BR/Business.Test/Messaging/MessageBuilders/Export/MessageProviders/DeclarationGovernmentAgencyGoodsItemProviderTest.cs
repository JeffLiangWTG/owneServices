using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationGovernmentAgencyGoodsItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGovernmentAgencyGoodsItemProvider()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var declaration = Factory.New<JobDeclaration>();
				var cusEntryLine = Factory.New<CusEntryLine>();
				cusEntryLine.CL_LineNumber = 2;

				var invHeader = declaration.Invoices.AddNew();
				invHeader.JZ_InvoiceAmount = 120m;
				invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
				invHeader.JZ_RX_NKInvoice_Currency = invHeader.LocalCurrencyCode;

				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CL = cusEntryLine.PK;
				invLine.JI_Tariff = "02011001";
				invLine.JI_LineNo = 1;
				invLine.JI_LinePrice = 100m;
				invLine.JI_InvoiceQuantity = 10m;
				invLine.JI_InvoiceUQ = "KG";
				invLine.JI_CustomsQuantity = 10m;
				invLine.JI_RN_NKCountryOfExport = "AU";
				invLine.JI_NetWeight = 150m;
				invLine.JI_NFeNumber = "00000000000000000000000000000000000000000000";
				invLine.JI_NFeItemNumber = "1";
				invLine.JI_Procedure = "80000";
				invLine.JI_SecondCPC = "81000";
				invLine.JI_ThirdCPC = "82000";
				invLine.JI_FourthCPC = "83000";
				invLine.ComplementaryDescription = "COMPLEMENTARY DESCRIPTION";
				invLine.JI_Description = "DESCRIPTION";
				var lpco = invLine.LPCOJobComInvLineRefsCollection.AddNew();
				lpco.JG_ReferenceNumber = "123";
				invLine.JI_IntendedTermDays = 100;
				invLine.JI_CargoPriority = "5001";
				invLine.JI_DigitalServiceDossier = "000000000";

				var addCharge = invHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 20m, invHeader.LocalCurrencyCode);
				addCharge.J7_IsIncludedInITOT = true;
				addCharge.J7_IsDutiable = false;
				declaration.ResumeApportionment();

				var drawback = invLine.SuspensionDrawbackCollection.AddNew();
				drawback.CSI_ReferenceNumber2 = "1111111";
				drawback.CSI_Tariff = "88888888";
				drawback.CSI_ReferenceNumber = "0000000000000";
				drawback.CSI_Value = 25m;
				drawback.CSI_LineNo = 1;
				drawback.CSI_Quantity = 10m;

				var previousDoc = invLine.PreviousDocuments.AddNew();
				previousDoc.CSI_ReferenceNumber2 = "1111111";
				previousDoc.CSI_ReferenceNumber = "0000000000000";
				previousDoc.CSI_Code = ExportPreviousDocumentList.Codes.DI;
				previousDoc.CSI_LineNo = 1;
				previousDoc.CSI_Quantity = 10m;

				var goodsAgencyItem = new DeclarationGovernmentAgencyGoodsItemProvider(cusEntryLine);

				CombineAssertions(() =>
				{
					AssertEquals("CustomsValueAmount should be", 80m, goodsAgencyItem.CustomsValueAmount);
					AssertEquals("ValueAmount should be", 100m, goodsAgencyItem.ValueAmount);
					AssertEquals("CustomsQuantity should be", 10m, goodsAgencyItem.CustomsQuantity);
					AssertEquals("InvoiceQuantity should be", 10m, goodsAgencyItem.InvoiceQuantity);
					AssertEquals("FinancedValueAmount should be", ZDecimal.Zero, goodsAgencyItem.FinancedValueAmount);
					AssertEquals("SequenceNumeric should be", 2, goodsAgencyItem.SequenceNumeric);
					AssertEquals("DestinationCountry should be", "AU", goodsAgencyItem.DestinationCountry);
					AssertEquals("LPCONumbers should be", "123", goodsAgencyItem.LPCONumbers.ElementAt(0));
					AssertEquals("PreviousDocuments should be", 1, goodsAgencyItem.PreviousDocuments.Count());
					AssertEquals("DigitalServiceDossiers should be", "000000000", goodsAgencyItem.DigitalServiceDossiers);
					AssertEquals("IntendedTerms should be", 100, goodsAgencyItem.IntendedTerms);
					AssertEquals("CargoPriority should be", "5001", goodsAgencyItem.CargoPriority);
					AssertEquals("NetWeight should be", 150m, goodsAgencyItem.NetWeight);
					AssertEquals("AgentCommission should be", ZDecimal.Zero, goodsAgencyItem.AgentCommission);
					AssertEquals("NfeItemNumber should be", (ZShort)1, goodsAgencyItem.NfeItemNumber);
					AssertEquals("CPCCode should be", "80000", goodsAgencyItem.CPCCode);
					AssertEquals("CPCCodeSecond should be", "81000", goodsAgencyItem.CPCCodeSecond);
					AssertEquals("CPCCodeThird should be", "82000", goodsAgencyItem.CPCCodeThird);
					AssertEquals("CPCCodeFourth should be", "83000", goodsAgencyItem.CPCCodeFourth);
					AssertEquals("ComplementaryDescription should be", "COMPLEMENTARY DESCRIPTION", goodsAgencyItem.Commodity.ComplementaryDescription);
					AssertEquals("Description should be", "DESCRIPTION", goodsAgencyItem.Commodity.GoodsDescription);
					AssertEquals("TariffCode should be", "02011001", goodsAgencyItem.Commodity.TariffCode);
					AssertEquals("InvoiceQuantityUQCode should be", "KG", goodsAgencyItem.InvoiceQuantityUQCode);
					AssertEquals("SuspensionDrawbacks count should be", 1, goodsAgencyItem.SuspensionDrawbacks.Count());
				});
			}
		}
	}
}
