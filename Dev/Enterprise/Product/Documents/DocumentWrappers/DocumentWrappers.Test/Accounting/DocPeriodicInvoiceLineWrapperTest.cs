using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocPeriodicInvoiceLineWrapperTest : TestCaseWithFactory
	{
		[TestDate(2023, 08, 28)]
		public void TestTaxDate()
		{
			using (AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code))
			{
				var taxDate = ZDate.Today.AddDays(-2);
				var goodsChargeCode = TestObjectCreator.CreateChargeCode("GOODS");
				goodsChargeCode.AC_GovtChargeCode = "TAX1";
				goodsChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

				var shipment = TestObjectCreator.CreateShipment("S00001");
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
				var lineGST = CreateInvoice(TestObjectCreator.GST1, taxDate);
				var lineNOT = CreateInvoice(TestObjectCreator.ExtraServiceTax, taxDate);
				var lineEXL = CreateInvoice(TestObjectCreator.ExcludedTax, taxDate);
				var lineWithoutTaxRate = CreateInvoice(null, ZDate.Empty);
				var lineWithInvalidTaxDate = CreateInvoice(TestObjectCreator.GST1, ZDate.Invalid);

				foreach (var layout in new[] { "INV", "CHG", "NON" })
				{
					AssertLineTaxDate("expect empty if tax rate is NOT rate type", lineNOT, layout, ZDate.Empty);
					AssertLineTaxDate("expect empty if tax rate is EXL rate type", lineEXL, layout, ZDate.Empty);
					AssertLineTaxDate("expect the line tax date if rate type is different from NOT and EXL", lineGST, layout, taxDate);
					AssertLineTaxDate("expect empty date if there is no tax rate", lineWithoutTaxRate, layout, ZDate.Empty);
					AssertLineTaxDate("expect empty date if tax date is invalid", lineWithInvalidTaxDate, layout, ZDate.Empty);
				}

				InvoicingLineBase CreateInvoice(AccTaxRate taxRate, ZDate taxRateDate)
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
					var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, goodsChargeCode, TestObjectCreator.AUD, 1.0M, "GOODS Desc", 200M);
					line.AL_GovtChargeCode = "TAX1";

					line.AL_AT = taxRate?.PK ?? ZGuid.Empty;
					line.AL_TaxDate = taxRateDate;
					TestObjectCreator.CreateJobCharge(line, job, goodsChargeCode);
					return line;
				}

				void AssertLineTaxDate(string message, InvoicingLineBase line, string layout, ZDateTime expectedTaxDate)
				{
					var docLine = DocARInvoiceLine.New(line, Factory);
					var wrapper = new DocPeriodicInvoiceLineWrapper(docLine, layout);
					AssertEquals(message, expectedTaxDate, wrapper.TaxDate);
				}
			}
		}

		public void TestProperties()
		{
			var goodsChargeCode = TestObjectCreator.CreateChargeCode("GOODS");
			goodsChargeCode.AC_GovtChargeCode = "TAX1";
			goodsChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;

			var shipment = TestObjectCreator.CreateShipment("S00001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, goodsChargeCode, TestObjectCreator.AUD, 1.0M, "GOODS Desc", 200M);
			line.AL_GovtChargeCode = "TAX1";
			TestObjectCreator.CreateJobCharge(line, job, goodsChargeCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.India))
			{
				foreach (var enableGovtChargeCode in new[] { true, false })
				{
					AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableGovtChargeCode);

					foreach (var layout in new[] { "INV", "CHG", "NON" })
					{
						var docLine = DocARInvoiceLine.New(line, Factory);
						var wrapper = new DocPeriodicInvoiceLineWrapper(docLine, layout);
						var expectedDescription = enableGovtChargeCode ? "[HSN: TAX1] GOODS Desc" : "GOODS Desc";
						AssertEquals(nameof(wrapper.LineDescription), expectedDescription, wrapper.LineDescription);
						AssertEquals(nameof(wrapper.LayoutWhenPrintedInPeriodicInvoice), layout, wrapper.LayoutWhenPrintedInPeriodicInvoice);
						AssertEquals(nameof(wrapper.SecondaryLayoutWhenPrintedInPeriodicInvoice), string.Empty, wrapper.SecondaryLayoutWhenPrintedInPeriodicInvoice);
						AssertEquals(nameof(wrapper.CreatingUserName), docLine.CreatingUserName, wrapper.CreatingUserName);
						AssertEquals(nameof(wrapper.CreatedDate), docLine.CreatedDate, wrapper.CreatedDate);
						AssertEquals(nameof(wrapper.ChargeCode), docLine.ChargeCode, wrapper.ChargeCode);
						AssertContainsExactElementsInAnyOrder(nameof(wrapper.ARGlobalChargeCodes), docLine.ARGlobalChargeCodes, wrapper.ARGlobalChargeCodes);
						AssertEquals(nameof(wrapper.GLAccount), docLine.GLAccount?.WrappedObject, wrapper.GLAccount?.WrappedObject);
						AssertEquals(nameof(wrapper.PercentOfGLAccount), docLine.PercentOfGLAccount?.WrappedObject, wrapper.PercentOfGLAccount?.WrappedObject);
						AssertEquals(nameof(wrapper.Invoice), docLine.Invoice?.WrappedObject, wrapper.Invoice?.WrappedObject);
						AssertEquals(nameof(wrapper.TaxRate), docLine.TaxRate?.WrappedObject, wrapper.TaxRate?.WrappedObject);
						AssertEquals(nameof(wrapper.TaxRateAmount_Raw), docLine.TaxRateAmount_Raw, wrapper.TaxRateAmount_Raw);
						AssertEquals(nameof(wrapper.TaxExtraRateAmount), docLine.TaxExtraRateAmount, wrapper.TaxExtraRateAmount);
						AssertEquals(nameof(wrapper.WithholdingTaxRate), docLine.WithholdingTaxRate?.WrappedObject, wrapper.WithholdingTaxRate?.WrappedObject);
						AssertEquals(nameof(wrapper.InvoiceLineDescriptionForPeriodicInvoice), docLine.InvoiceLineDescriptionForPeriodicInvoice, wrapper.InvoiceLineDescriptionForPeriodicInvoice);
						AssertEquals(nameof(wrapper.LineDescriptionAndExchangeRate), docLine.LineDescriptionAndExchangeRate, wrapper.LineDescriptionAndExchangeRate);
						AssertEquals(nameof(wrapper.ExchangeRate), docLine.ExchangeRate, wrapper.ExchangeRate);
						AssertEquals(nameof(wrapper.Branch), docLine.Branch?.WrappedObject, wrapper.Branch?.WrappedObject);
						AssertEquals(nameof(wrapper.Department), docLine.Department?.WrappedObject, wrapper.Department?.WrappedObject);
						AssertEquals(nameof(wrapper.GSTVAT), docLine.GSTVAT, wrapper.GSTVAT);
						AssertEquals(nameof(wrapper.OSTaxDisplay), docLine.OSTaxDisplay, wrapper.OSTaxDisplay);
						AssertEquals(nameof(wrapper.OSExTaxAmount), docLine.OSExTaxAmount, wrapper.OSExTaxAmount);
						AssertEquals(nameof(wrapper.OSTaxAmount), docLine.OSTaxAmount, wrapper.OSTaxAmount);
						AssertEquals(nameof(wrapper.OSSERAmount), docLine.OSSERAmount, wrapper.OSSERAmount);
						AssertEquals(nameof(wrapper.JobNumber), docLine.JobNumber, wrapper.JobNumber);
						AssertEquals(nameof(wrapper.JobHeader), docLine.JobHeader?.WrappedObject, wrapper.JobHeader?.WrappedObject);
						AssertEquals(nameof(wrapper.JobTypeForPeriodicInvoice), docLine.JobTypeForPeriodicInvoice, wrapper.JobTypeForPeriodicInvoice);
						AssertEquals(nameof(wrapper.OperationsJob), docLine.OperationsJob?.WrappedObject, wrapper.OperationsJob?.WrappedObject);
						AssertEquals(nameof(wrapper.LineAmount), docLine.LineAmount, wrapper.LineAmount);
						AssertEquals(nameof(wrapper.ChargeExchangeRate), docLine.ChargeExchangeRate, wrapper.ChargeExchangeRate);
						AssertEquals(nameof(wrapper.ChargeOSAmount), docLine.ChargeOSAmount, wrapper.ChargeOSAmount);
						AssertEquals(nameof(wrapper.ChargeOSAmountForCLC), docLine.ChargeOSAmountForCLC, wrapper.ChargeOSAmountForCLC);
						AssertEquals(nameof(wrapper.ChargeCurrency), docLine.ChargeCurrency, wrapper.ChargeCurrency);
						AssertEquals(nameof(wrapper.LineType), docLine.LineType, wrapper.LineType);
						AssertEquals(nameof(wrapper.Organisation), docLine.Organisation?.WrappedObject, wrapper.Organisation?.WrappedObject);
						AssertEquals(nameof(wrapper.OSAmount), docLine.OSAmount, wrapper.OSAmount);
						AssertEquals(nameof(wrapper.OSUnitPrice), docLine.OSUnitPrice, wrapper.OSUnitPrice);
						AssertEquals(nameof(wrapper.PercentageOfPeriod), docLine.PercentageOfPeriod, wrapper.PercentageOfPeriod);
						AssertEquals(nameof(wrapper.PostDate), docLine.PostDate, wrapper.PostDate);
						AssertEquals(nameof(wrapper.PostPeriod), docLine.PostPeriod, wrapper.PostPeriod);
						AssertEquals(nameof(wrapper.PostToGL), docLine.PostToGL, wrapper.PostToGL);
						AssertEquals(nameof(wrapper.PreventInvoicePrintGrouping), docLine.PreventInvoicePrintGrouping, wrapper.PreventInvoicePrintGrouping);
						AssertEquals(nameof(wrapper.ReverseDate), docLine.ReverseDate, wrapper.ReverseDate);
						AssertEquals(nameof(wrapper.ReversePeriod), docLine.ReversePeriod, wrapper.ReversePeriod);
						AssertEquals(nameof(wrapper.ReverseToGL), docLine.ReverseToGL, wrapper.ReverseToGL);
						AssertEquals(nameof(wrapper.Currency), docLine.Currency?.WrappedObject, wrapper.Currency?.WrappedObject);
						AssertEquals(nameof(wrapper.DisplayCurrency), docLine.DisplayCurrency?.WrappedObject, wrapper.DisplayCurrency?.WrappedObject);
						AssertEquals(nameof(wrapper.Sequence), docLine.Sequence, wrapper.Sequence);
						AssertEquals(nameof(wrapper.UnitPrice), docLine.UnitPrice, wrapper.UnitPrice);
						AssertEquals(nameof(wrapper.UnitQty), docLine.UnitQty, wrapper.UnitQty);
						AssertEquals(nameof(wrapper.WithholdingTax), docLine.WithholdingTax, wrapper.WithholdingTax);
						AssertEquals(nameof(wrapper.Shipment), docLine.Shipment?.WrappedObject, wrapper.Shipment?.WrappedObject);
						AssertEquals(nameof(wrapper.FKToShipment), docLine.FKToShipment, wrapper.FKToShipment);
						AssertEquals(nameof(wrapper.ExchangeRateAndAmount), docLine.ExchangeRateAndAmount, wrapper.ExchangeRateAndAmount);
						AssertEquals(nameof(wrapper.ChargeOSAmountAndCurrency), docLine.ChargeOSAmountAndCurrency, wrapper.ChargeOSAmountAndCurrency);
						AssertEquals(nameof(wrapper.HeaderOrganisation), docLine.HeaderOrganisation?.WrappedObject, wrapper.HeaderOrganisation?.WrappedObject);
						AssertEquals(nameof(wrapper.HeaderCurrency), docLine.HeaderCurrency?.WrappedObject, wrapper.HeaderCurrency?.WrappedObject);
						AssertEquals(nameof(wrapper.LocalAmountAndTax), docLine.LocalAmountAndTax, wrapper.LocalAmountAndTax);
						AssertEquals(nameof(wrapper.Charge), docLine.Charge?.WrappedObject, wrapper.Charge?.WrappedObject);
						AssertEquals(nameof(wrapper.TaxRateDisplay), docLine.TaxRateDisplay, wrapper.TaxRateDisplay);
						AssertEquals(nameof(wrapper.OSAmountDisplay), docLine.OSAmountDisplay, wrapper.OSAmountDisplay);
						AssertEquals(nameof(wrapper.TaxAmountDisplay), docLine.TaxAmountDisplay, wrapper.TaxAmountDisplay);
						AssertEquals(nameof(wrapper.TaxGroupCode), docLine.TaxGroupCode, wrapper.TaxGroupCode);
						AssertEquals(nameof(wrapper.ExchangeRateDisplay), docLine.ExchangeRateDisplay, wrapper.ExchangeRateDisplay);
						AssertEquals(nameof(wrapper.IsSubTotalLine), docLine.IsSubTotalLine, wrapper.IsSubTotalLine);
						AssertEquals(nameof(wrapper.IsSpacerLine), docLine.IsSpacerLine, wrapper.IsSpacerLine);
						AssertEquals(nameof(wrapper.IsCommentLine), docLine.IsCommentLine, wrapper.IsCommentLine);
						AssertEquals(nameof(wrapper.IsRollUpLine), docLine.IsRollUpLine, wrapper.IsRollUpLine);
						AssertEquals(nameof(wrapper.HasBeenSubTotalled), docLine.HasBeenSubTotalled, wrapper.HasBeenSubTotalled);
						AssertEquals(nameof(wrapper.ClientReference), docLine.ClientReference, wrapper.ClientReference);
						AssertEquals(nameof(wrapper.SubInvoiceRef), docLine.SubInvoiceRef, wrapper.SubInvoiceRef);
						AssertEquals(nameof(wrapper.ShippersReference), docLine.ShippersReference, wrapper.ShippersReference);
						AssertEquals(nameof(wrapper.IsLoadListJob), docLine.IsLoadListJob, wrapper.IsLoadListJob);
						AssertEquals(nameof(wrapper.IsCFSShipmentJob), docLine.IsCFSShipmentJob, wrapper.IsCFSShipmentJob);
						AssertEquals(nameof(wrapper.IsCustomJob), docLine.IsCustomJob, wrapper.IsCustomJob);
						AssertEquals(nameof(wrapper.IsLocalCartage), docLine.IsLocalCartage, wrapper.IsLocalCartage);
						AssertEquals(nameof(wrapper.LoadList), docLine.LoadList?.WrappedObject, wrapper.LoadList?.WrappedObject);
						AssertEquals(nameof(wrapper.OtherReference), docLine.OtherReference, wrapper.OtherReference);
						AssertEquals(nameof(wrapper.Customs), docLine.Customs?.WrappedObject, wrapper.Customs?.WrappedObject);
						AssertEquals(nameof(wrapper.LocalTransport), docLine.LocalTransport?.WrappedObject, wrapper.LocalTransport?.WrappedObject);
						AssertEquals(nameof(wrapper.OSAmountForTotal), docLine.OSAmountForTotal, wrapper.OSAmountForTotal);
						AssertEquals(nameof(wrapper.OSExTaxAmountForTotal), docLine.OSExTaxAmountForTotal, wrapper.OSExTaxAmountForTotal);
						AssertEquals(nameof(wrapper.LineAmountForTotal), docLine.LineAmountForTotal, wrapper.LineAmountForTotal);
						AssertEquals(nameof(wrapper.GSTVATForTotal), docLine.GSTVATForTotal, wrapper.GSTVATForTotal);
						AssertEquals(nameof(wrapper.LocalAmountAndTaxForTotal), docLine.LocalAmountAndTaxForTotal, wrapper.LocalAmountAndTaxForTotal);
						AssertEquals(nameof(wrapper.TaxAmountDisplayForTotal), docLine.TaxAmountDisplayForTotal, wrapper.TaxAmountDisplayForTotal);
						AssertEquals(nameof(wrapper.OSTaxDisplayForTotal), docLine.OSTaxDisplayForTotal, wrapper.OSTaxDisplayForTotal);
						AssertEquals(nameof(wrapper.TaxRateAsterisks), docLine.TaxRateAsterisks, wrapper.TaxRateAsterisks);
						AssertEquals(nameof(wrapper.TaxRateAsterisksAsNumbers), docLine.TaxRateAsterisksAsNumbers, wrapper.TaxRateAsterisksAsNumbers);
						AssertEquals(nameof(wrapper.OSTaxDisplayNoAsterisks), docLine.OSTaxDisplayNoAsterisks, wrapper.OSTaxDisplayNoAsterisks);
						AssertEquals(nameof(wrapper.OSTaxDisplayNoAsterisksWithRegistryRule), docLine.OSTaxDisplayNoAsterisksWithRegistryRule, wrapper.OSTaxDisplayNoAsterisksWithRegistryRule);
						AssertEquals(nameof(wrapper.OSGSTAmount), docLine.OSGSTAmount, wrapper.OSGSTAmount);
						AssertEquals(nameof(wrapper.OSQSTAmount), docLine.OSQSTAmount, wrapper.OSQSTAmount);
						AssertEquals(nameof(wrapper.OSSBCAmount), docLine.OSSBCAmount, wrapper.OSSBCAmount);
						AssertEquals(nameof(wrapper.OSKKCAmount), docLine.OSKKCAmount, wrapper.OSKKCAmount);
						AssertEquals(nameof(wrapper.IsExtraTaxSBCAndKKC), docLine.IsExtraTaxSBCAndKKC, wrapper.IsExtraTaxSBCAndKKC);
						AssertEquals(nameof(wrapper.IsExtraTaxSBCOrKKC), docLine.IsExtraTaxSBCOrKKC, wrapper.IsExtraTaxSBCOrKKC);
						AssertEquals(nameof(wrapper.OSEDUAmount), docLine.OSEDUAmount, wrapper.OSEDUAmount);
						AssertEquals(nameof(wrapper.OSRETAmount), docLine.OSRETAmount, wrapper.OSRETAmount);
						AssertEquals(nameof(wrapper.OSSPVAmount), docLine.OSSPVAmount, wrapper.OSSPVAmount);
						AssertEquals(nameof(wrapper.IncludeTaxAmountInOsTaxDisplay), docLine.IncludeTaxAmountInOsTaxDisplay, wrapper.IncludeTaxAmountInOsTaxDisplay);
						AssertEquals(nameof(wrapper.ShowPercentInGSTDisplay), docLine.ShowPercentInGSTDisplay, wrapper.ShowPercentInGSTDisplay);
						AssertEquals(nameof(wrapper.ContainerNumbers), docLine.ContainerNumbers, wrapper.ContainerNumbers);
						AssertContainsExactElementsInAnyOrder(nameof(wrapper.AmountSplittedByChargeCode), docLine.AmountSplittedByChargeCode, wrapper.AmountSplittedByChargeCode);
						AssertEquals(nameof(wrapper.SellRecognition), docLine.SellRecognition, wrapper.SellRecognition);
						AssertEquals(nameof(wrapper.CostRecognition), docLine.CostRecognition, wrapper.CostRecognition);
						AssertEquals(nameof(wrapper.ProductName), docLine.ProductName, wrapper.ProductName);
						AssertEquals(nameof(wrapper.IsApproved), docLine.IsApproved, wrapper.IsApproved);
						AssertEquals(nameof(wrapper.SellAccount), docLine.SellAccount, wrapper.SellAccount);
						AssertEquals(nameof(wrapper.CostAccount), docLine.CostAccount, wrapper.CostAccount);
						AssertEquals(nameof(wrapper.IsApportioned), docLine.IsApportioned, wrapper.IsApportioned);
						AssertEquals(nameof(wrapper.CostPosted), docLine.CostPosted, wrapper.CostPosted);
						AssertEquals(nameof(wrapper.SellPosted), docLine.SellPosted, wrapper.SellPosted);
						AssertEquals(nameof(wrapper.CFXJnl), docLine.CFXJnl, wrapper.CFXJnl);
						AssertEquals(nameof(wrapper.ChargeCodePrintSequence), docLine.ChargeCodePrintSequence, wrapper.ChargeCodePrintSequence);
						AssertEquals(nameof(wrapper.DisplaySequence), docLine.DisplaySequence, wrapper.DisplaySequence);
						AssertEquals(nameof(wrapper.EstimatedCostWithCurrency), docLine.EstimatedCostWithCurrency, wrapper.EstimatedCostWithCurrency);
						AssertEquals(nameof(wrapper.EstimatedCost), docLine.EstimatedCost, wrapper.EstimatedCost);
						AssertEquals(nameof(wrapper.EstimatedRevenueWithCurrency), docLine.EstimatedRevenueWithCurrency, wrapper.EstimatedRevenueWithCurrency);
						AssertEquals(nameof(wrapper.EstimatedRevenue), docLine.EstimatedRevenue, wrapper.EstimatedRevenue);
						AssertEquals(nameof(wrapper.LocalCostAmountWithCurrency), docLine.LocalCostAmountWithCurrency, wrapper.LocalCostAmountWithCurrency);
						AssertEquals(nameof(wrapper.LocalCostAmount), docLine.LocalCostAmount, wrapper.LocalCostAmount);
						AssertEquals(nameof(wrapper.LocalSellAmountWithCurrency), docLine.LocalSellAmountWithCurrency, wrapper.LocalSellAmountWithCurrency);
						AssertEquals(nameof(wrapper.LocalSellAmount), docLine.LocalSellAmount, wrapper.LocalSellAmount);
						AssertEquals(nameof(wrapper.CostOSAmountAndCurrency), docLine.CostOSAmountAndCurrency, wrapper.CostOSAmountAndCurrency);
						AssertEquals(nameof(wrapper.CostOSAmount), docLine.CostOSAmount, wrapper.CostOSAmount);
						AssertEquals(nameof(wrapper.CostExchangeRate), docLine.CostExchangeRate, wrapper.CostExchangeRate);
						AssertEquals(nameof(wrapper.CostCurrency), docLine.CostCurrency, wrapper.CostCurrency);
						AssertEquals(nameof(wrapper.OSIntegratedGSTAmount), docLine.OSIntegratedGSTAmount, wrapper.OSIntegratedGSTAmount);
						AssertEquals(nameof(wrapper.OSCentreGSTAmount), docLine.OSCentreGSTAmount, wrapper.OSCentreGSTAmount);
						AssertEquals(nameof(wrapper.OSStateGSTAmount), docLine.OSStateGSTAmount, wrapper.OSStateGSTAmount);
						AssertEquals(nameof(wrapper.GovernmentReportingCode), docLine.GovernmentReportingCode, wrapper.GovernmentReportingCode);
						AssertEquals(nameof(wrapper.GovernmentReportingCodeHeading), docLine.GovernmentReportingCodeHeading, wrapper.GovernmentReportingCodeHeading);
						AssertEquals(nameof(wrapper.Quantity), docLine.Quantity, wrapper.Quantity);
						AssertEquals(nameof(wrapper.DisplayTaxGroupCode), docLine.DisplayTaxGroupCode, wrapper.DisplayTaxGroupCode);
					}
				}
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
