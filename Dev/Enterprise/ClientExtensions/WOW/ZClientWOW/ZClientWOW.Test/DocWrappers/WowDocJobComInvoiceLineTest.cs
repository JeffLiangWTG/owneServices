using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.Wow.Testing;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.DocWrappers.Testing
{
	[TestedType(typeof(WowDocJobComInvoiceLine))]
	public class WowDocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<WoolworthsJobComInvoiceLine, WowDocJobComInvoiceLine>
	{
		public void TestProperties()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			supplier.OH_FullName = "Supplier Name";
			supplier.OH_IsConsignor = true;
			supplier.MiscServ.OM_CustomDecimal1 = 12.5m;
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "Buyer";
			buyer.OH_FullName = "Buyer Name";
			buyer.OH_IsConsignee = true;
			buyer.MiscServ.OM_CustomDecimal2 = 5m;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Pencil";
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, "BTH");
			product.OP_CustomDecimal1 = 10m;
			product.OP_Department = "123";
			product.OP_VendorPackQty = 1000;
			var otherProduct = Factory.New<OrgSupplierPart>();
			otherProduct.OP_PartNum = "UMB";
			otherProduct.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, "BTH");
			otherProduct.OP_CustomDecimal1 = 23m;
			otherProduct.OP_Department = "456";
			otherProduct.OP_VendorPackQty = 2000;

			var kuwait = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Kuwait);
			var aBC = Factory.New<RefExchangeRate>();
			aBC.RE_StartDate = ZDateTime.Now.AddMonths(-12);
			aBC.RE_ExpiryDate = ZDateTime.Now.AddMonths(10);
			aBC.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			aBC.RE_GC = GlbCompany.CurrentCompany.PK;
			aBC.RE_SellRate = 0.5m;
			aBC.RE_RX_NKExCurrency = kuwait.RX_Code;
			Factory.Save();

			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.OH_FullName = "Importer Name";

			var testHelper = new WowTestHelper();

			var declaration = InvoiceLine.Declaration;

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_JobNum = "123";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var originChargeCode = testHelper.CreateChargeCode("CONSOL", ChargeCodeGroupList.Codes.Origin);
			testHelper.CreateJobCharge(job.Charges, 9.45m, originChargeCode.PK);
			var destinChargeCode = testHelper.CreateChargeCode("DES", ChargeCodeGroupList.Codes.Destination);
			testHelper.CreateJobCharge(job.Charges, 11m, destinChargeCode.PK);
			var otherImportChargeCode = testHelper.CreateChargeCode("NGRP", ChargeCodeGroupList.Codes.NotGrouped);
			testHelper.CreateJobCharge(job.Charges, 12m, otherImportChargeCode.PK);
			var quarantineChargeCode = testHelper.CreateChargeCode("QUARANT", ChargeCodeGroupList.Codes.Brokerage);
			testHelper.CreateJobCharge(job.Charges, 13m, quarantineChargeCode.PK);
			var detentChargeCode = testHelper.CreateChargeCode("DET", ChargeCodeGroupList.Codes.Freight);
			testHelper.CreateJobCharge(job.Charges, 14m, detentChargeCode.PK);

			declaration.JE_DateOfArrival = ZDateTime.Now;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			Factory.Save();

			var exRate = ((IBusinessObjectCollection)declaration.Job["ExchangeRates"]).AddNew();
			exRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = kuwait.RX_Code;
			exRate[JobExRateSchema.Constants.JF_BaseRate] = 2m;
			Factory.Save();

			var invoice = InvoiceLine.InvoiceHeader;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_RX_NKInvoice_Currency = kuwait.RX_Code;
			invoice.JZ_InvoiceCurrLandedCostExRate = 0m;

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ABC";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			var orderline = order.OrderLines.AddNew();
			orderline.JO_CustomAttrib6 = "BUYERNAME";
			orderline.JO_Partno = product.OP_PartNum;
			var delivery = orderline.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			delivery.J4_CustomAttribute1 = "POMNUM";
			var deliveryContainer = delivery.Containers.AddNew();
			deliveryContainer.J5_ContainerNum = "CONT1";
			var invoiceLine1 = InvoiceLine;
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_OP = product.PK;
			invoiceLine1.JI_PartNo = product.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 200m;
			invoiceLine1.JI_OrderNumber = "ABC";
			invoiceLine1.JI_CustomDecimal1 = 1;
			invoiceLine1.JI_CustomAttrib4 = "CONT1";
			invoiceLine1.JI_Volume = 0.5m;
			invoiceLine1.JI_InvoiceQuantity = 200;
			Factory.Save();
			invoiceLine1.JI_OP = product.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_PartNo = "GOODS";
			invoiceLine2.JI_OrderNumber = "ABC";
			invoiceLine2.JI_CustomDecimal1 = 2;
			invoiceLine2.JI_CustomAttrib4 = "CONT1";
			invoiceLine2.JI_Volume = 1.5m;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			exRate[JobExRateSchema.Constants.JF_BaseRate] = 2m;
			Factory.Save();

			AssertEquals("ContingencyAmount - 5/100 * 1000 * 0.5", 25m, InvoiceLineWrapper.ContingencyAmount);
			AssertEquals("RoyaltyAmount - 10 * 1000 * 0.5", 50m, InvoiceLineWrapper.RoyaltyAmount);
			AssertEquals("CommissionInLocalCurrency - 1000 * 0.5 * 12.5/100", 62.5m, InvoiceLineWrapper.CommisionInLocalCurrency);
			AssertEquals("LinePriceInLocalCurrency - 1000 *0.5", 500m, InvoiceLineWrapper.LinePriceInLocalCurrency);
			AssertEquals("UnitPrinceInLocalCurrency - 1000*0.5 /200", 2.5m, InvoiceLineWrapper.UnitPriceInLocalCurrency);
			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = Guid.Empty;
			AssertEquals("ContingencyAmount - 5/100 * 1000 * 0.5", 25m, InvoiceLineWrapper.ContingencyAmount);
			AssertEquals("ImporterOrBuyerName should be the order buyer name", buyer.OH_FullName, InvoiceLineWrapper.ImporterOrBuyerName);
			AssertEquals("BuyerName", "BUYERNAME", InvoiceLineWrapper.BuyerName);
			AssertEquals("POMNum", "POMNUM", InvoiceLineWrapper.POMNum);
			InvoiceLine.InvoiceHeader.JobComInvoiceLines[0].JI_OrderNumber = ZString.Empty;
			AssertEquals("ImporterOrBuyerName should be the declaration importer name", importer.OH_FullName, InvoiceLineWrapper.ImporterOrBuyerName);
		}

		public void TestSuperTypeOverridden()
		{
			var docInvoiceLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			Assert("Constructed doc wrapper of correct overridden type", docInvoiceLine is WowDocJobComInvoiceLine);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected override WowDocJobComInvoiceLine CreateInvoiceLineWrapper(WoolworthsJobComInvoiceLine invoiceLineInternal)
		{
			return (WowDocJobComInvoiceLine)WowDocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
