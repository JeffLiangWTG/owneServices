using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineTestExport : JobComInvoiceLineTest
	{
		public void TestTariffFormatter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var formatProvider = (Common.ITariffFormatProvider)invoiceLine;
			AssertType<AUExportTariffUniversalFormatter>(formatProvider.TariffFormatter);
		}

		public void TestTariffNumberIsSavedDotFormatted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_Tariff = "22030031";
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var invoiceLineIOF = otherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("2203.00.31", invoiceLineIOF["JI_Tariff"]);
			}

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				invoiceLine.JI_Tariff = "22030031";
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var invoiceLineIOF = otherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("2203.00.31", invoiceLineIOF["JI_Tariff"]);
			}
		}

		public void TestCustomsUnitDefaultingStrategy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var testExpTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "22030090", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testExpTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "L");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var line1 = Header.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "2203.00.90";
				AssertNotNull("PreCondition : this tariff exists", line1.ExportTariff);
				AssertEquals("UQ1", "L", line1.JI_CustomsUnitQty);
				AssertEquals("UQ2", "", line1.JI_CustomsSecondUnitQty);
			}
		}

		public void TestCustomsUnitDefaultingStrategy_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var line1 = Header.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "2203.00.90";
				AssertNotNull("PreCondition : this tariff exists", line1.ExportTariff);
				AssertEquals("UQ1", "L", line1.JI_CustomsUnitQty);
				AssertEquals("UQ2", "", line1.JI_CustomsSecondUnitQty);
			}
		}
		public void TestJI_Drawback()
		{
			AssertEquals("Drawback", false, DetatchedLine.JI_Drawback);
			DetatchedLine.JI_Drawback = false;
			AssertEquals("Drawback", false, DetatchedLine.JI_Drawback);
			DetatchedLine.JI_Drawback = true;
			AssertEquals("Drawback", true, DetatchedLine.JI_Drawback);
			DetatchedLine.JI_Drawback = true;
			AssertEquals("Drawback", true, DetatchedLine.JI_Drawback);
			DetatchedLine.JI_Drawback = false;
			AssertEquals("Drawback", false, DetatchedLine.JI_Drawback);
		}

		public void TestJI_Texco()
		{
			AssertEquals("Texco", false, Line.JI_Texco);
			Line.JI_Texco = false;
			AssertEquals("Texco", false, Line.JI_Texco);
			Line.JI_Texco = true;
			AssertEquals("Texco", true, Line.JI_Texco);
			Line.JI_Texco = true;
			AssertEquals("Texco", true, Line.JI_Texco);
			Line.JI_Texco = false;
			AssertEquals("Texco", false, Line.JI_Texco);
		}

		public void TestJI_MotorVehiclePlan()
		{
			AssertEquals("MotorVehiclePlan", false, Line.JI_MotorVehiclePlan);
			Line.JI_MotorVehiclePlan = false;
			AssertEquals("MotorVehiclePlan", false, Line.JI_MotorVehiclePlan);
			Line.JI_MotorVehiclePlan = true;
			AssertEquals("MotorVehiclePlan", true, Line.JI_MotorVehiclePlan);
			Line.JI_MotorVehiclePlan = true;
			AssertEquals("MotorVehiclePlan", true, Line.JI_MotorVehiclePlan);
			Line.JI_MotorVehiclePlan = false;
			AssertEquals("MotorVehiclePlan", false, Line.JI_MotorVehiclePlan);
		}

		public void TestJI_LineNo_ReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			dec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert(invLine.JI_LineNoInfo.ReadOnly);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!invLine.JI_LineNoInfo.ReadOnly);
		}

		public void TestJI_Calc_Invoice_ReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			dec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert(invLine.JI_Calc_InvoiceInfo.ReadOnly);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!invLine.JI_Calc_InvoiceInfo.ReadOnly);
		}

		public void TestEncryptionNumbers()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123:456";
			var encryptionNumbers = Line.EncryptionNumbers;
			AssertEquals(1, encryptionNumbers.Count());
			AssertEquals("456", encryptionNumbers.First());
		}

		public void TestGetJI_AUState()
		{
			Line.JI_CountryOfOrigin = "NZ";
			AssertEquals("Origin Code", "YY-FO", Line.JI_GoodsOriginCode);

			Line.JI_AUState = "NSW";
			Line.JI_CountryOfOrigin = "AU";
			AssertEquals("Origin Code", "AU-NS", Line.JI_GoodsOriginCode);
		}

		public void TestCustomsQuantityIsNotReadOnlyOnTariffRequiringKG()
		{
			AssertEquals("JI_CustomsQuantityInfo.Readonly", true, Line.JI_CustomsQuantityInfo.ReadOnly);
			Line.JI_Tariff = "4801.00.02";
			AssertEquals("JI_CustomsQuantityInfo.Readonly", false, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestReproduceQuantitySentForNRProblem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "10011011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 1 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "T");

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "98090001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 2 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "1001.10.11";
				Line.JI_InvoiceQuantity = 10m;
				Line.JI_InvoiceUQ = "T";
				AssertEquals("JI_CustomsUnitQty", "T", Line.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity", 10m, Line.JI_CustomsQuantity);
				Line.JI_Tariff = "9809.00.01";
				AssertEquals("JI_CustomsUnitQty", ZString.Empty, Line.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity", 0m, Line.JI_CustomsQuantity);
			}
		}

		public void TestReproduceQuantitySentForNRProblem_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Line.JI_Tariff = "1001.10.11";
				Line.JI_InvoiceQuantity = 10m;
				Line.JI_InvoiceUQ = "T";
				AssertEquals("JI_CustomsUnitQty", "T", Line.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity", 10m, Line.JI_CustomsQuantity);
				Line.JI_Tariff = "9809.00.01";
				AssertEquals("JI_CustomsUnitQty", "NR", Line.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity", 0m, Line.JI_CustomsQuantity);
			}
		}

		public void TestTransportAndInsuranceDoNotFallBackToApportionedFreightInsuranceIfTILVOverriden()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine1.AddInfo.ZA_TILV = "0.00AUD";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;

			declaration.ResumeApportionment();

			AssertEquals("PreCondition:invoice line 1 has an apportioned freight", 60m, invoiceLine1.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));

			AssertEquals("PreCondition:invoice line 2 has an apportioned freight", 40m, invoiceLine2.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));

			AssertEquals("TransportAndInsurance should not fall back to apportioned freight and insurance if TILV is overriden with zero amount", Money.Empty, invoiceLine1.TransportAndInsurance);
			AssertEquals("TransportAndInsurance should return an apportioned TILV from AddInfo", new Money(100m, JobDeclaration.GetLocalCurrency()), invoiceLine2.TransportAndInsurance);

			AssertEquals("JI_Calc_TILVAmount", 0m, invoiceLine1.JI_Calc_TNI);
			AssertEquals("JI_Calc_TILVAmount", 100m, invoiceLine2.JI_Calc_TNI);

			invoiceLine1.AddInfo.ZA_TILV = "";
			declaration.ResumeApportionment();
			AssertEquals("JI_Calc_TILVAmount", 60m, invoiceLine1.JI_Calc_TNI);
			AssertEquals("JI_Calc_TILVAmount", 40m, invoiceLine2.JI_Calc_TNI);
		}

		#region Implementation

		protected override JobDeclaration JobDec
		{
			get
			{
				JobDeclaration fJobDec = base.JobDec;
				fJobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				return fJobDec;
			}
		}

		#endregion
	}
}
