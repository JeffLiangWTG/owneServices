using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class Box68VatValueAdjustmentCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateVATAdjustmentBox68()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalWeight = 99;
			declaration.ZG_ManualCalc = true;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be the minimum value 100 pounds", 100m, declaration.ZG_VATAdjAmt);

			declaration.JE_TotalWeight = 401;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment Amount should be 0.4 times the total weight", 160.4m, declaration.ZG_VATAdjAmt);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container.CO_Weight = 10000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container.CO_Weight = 5000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container.CO_Weight = 8000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container.CO_Weight = 3000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount for this FDL load should be 2200", 2200.00m, declaration.ZG_VATAdjAmt);

			declaration.CusContainers.RemoveAll();
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 10000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 5000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 8000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 3000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount for this LDL load should be 2420", 2420m, declaration.ZG_VATAdjAmt);

			declaration.CusContainers.RemoveAll();
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 10000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container.CO_Weight = 5000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 8000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container.CO_Weight = 3000.00m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount for this mixed FDL/LDL load should be 2800", 2800m, declaration.ZG_VATAdjAmt);

			declaration.ZG_RX_NKVATAdj = "ZAR";
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount currency units should be GBP", "GBP", declaration.ZG_RX_NKVATAdj);

			declaration.CusContainers.RemoveAll();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalWeight = 1000;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be 400 pounds", 400m, declaration.ZG_VATAdjAmt);
			declaration.JE_TotalVolume = 9m;
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicMetres;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be 600 (not 400) pounds because the chargeable weight is 1500kg (not 1000)", 600m, declaration.ZG_VATAdjAmt);

			declaration.CusContainers.RemoveAll();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 900m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be the sea minimum of 170 pounds", 170m, declaration.ZG_VATAdjAmt);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.ZG_VATAdjAmt = 0m;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be the sea/road minimum of 170 pounds", 170m, declaration.ZG_VATAdjAmt);

			declaration.CusContainers.RemoveAll();
			declaration.JE_TotalWeight = 0;
			declaration.JE_TotalVolume = 0;
			declaration.JE_TotalVolumeUnit = "";
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ActualChargeable = 1500;
			declaration.CalculateVATAdjustmentForBox68();
			AssertEquals("The VAT Adjustment amount should be the 600, with chargeable pulled from the shipment", 600m, declaration.ZG_VATAdjAmt);
		}

		public void TestCalculateVATAdjustmentBox68WhenNotManual()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalWeight = 99;
			declaration.ZG_ManualCalc = false;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
			invoice.JZ_InvoiceAmount = 111m;

			declaration.CalculateVATAdjustmentForBox68();
			declaration.ResumeApportionment();
			AssertEquals("The VAT Adjustment amount should be the minimum value 100 pounds", 100m, declaration.ZG_VATAdjAmt);
			AssertEquals("The result is written in a Charge", 1, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("Charge Amount ", 100m, declaration.TopGroupInvoice.Charges[0].J7_Amount);

			declaration.JE_TotalWeight = 401;
			declaration.CalculateVATAdjustmentForBox68();
			declaration.ResumeApportionment();
			AssertEquals("The VAT Adjustment Amount should be 0.4 times the total weight", 160.4m, declaration.ZG_VATAdjAmt);

			AssertEquals("An existing charge needs to be updated", 1, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("Charge Amount", 160.4m, declaration.TopGroupInvoice.Charges[0].J7_Amount);
		}

		public void TestCalculateVATAdjustmentBox68_Invalid_WeightUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_Weight = 900m;
			container.CO_WeightUQ = "ZZ";
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ declaration.CalculateVATAdjustmentForBox68(); });
			container.CO_WeightUQ = "";
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ declaration.CalculateVATAdjustmentForBox68(); });
			container.CO_WeightUQ = container.Lookups.WeightUnits[0].Code;
			AssertNoExceptionThrown(delegate
			{ declaration.CalculateVATAdjustmentForBox68(); });
		}
	}
}
