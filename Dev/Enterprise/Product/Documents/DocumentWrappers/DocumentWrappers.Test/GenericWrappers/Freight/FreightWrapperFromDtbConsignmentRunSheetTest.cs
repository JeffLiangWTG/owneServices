using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbConsignmentRunSheet))]
	sealed class FreightWrapperFromDtbConsignmentRunSheetTest : FreightWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment.KM_JobID = "K123";
			var instruction1 = consignment.PickupInstruction;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment2.KM_JobID = "K456";
			var instruction2 = consignment2.PickupInstruction;

			var consignment3 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment3.KM_JobID = "K789";
			var instruction3 = consignment3.PickupInstruction;

			var truck = Helper.CreateVehicle("VHCL");
			truck.RQ_Registration = "Rego123";
			var runSheet = Helper.CreateRunSheet(ZDateTimeOffset.Today);
			var runSheetInstruction1 = Helper.CreateRunSheetInstruction(runSheet, new[] { instruction1.Confirmations[0], instruction2.Confirmations[0] });
			var runSheetInstruction2 = Helper.CreateRunSheetInstruction(runSheet, new[] { instruction3.Confirmations[0] });
			var depotRunSheetInstruction = runSheet.RunSheetInstructions.Single(i => i.IsOwnDepot);
			runSheet.KG_RunSheetNumber = "RS12345";
			runSheet.KG_RQ_Truck = truck.PK;
			runSheet.KG_GS_NKTruckDriver = Helper.CreateDriver("Bob", "Bob").GS_Code;

			var wrapper = new FreightWrapperFromDtbConsignmentRunSheet(runSheet, Factory);

			AssertEquals("JobNumberHeading", "Run Sheet", wrapper.JobNumberHeading);
			AssertEquals("JobNumber", "RS12345", wrapper.JobNumber);
			AssertEquals("Driver", "Bob", wrapper.RunSheet.DriversName);
			AssertEquals("Truck", "Rego123", wrapper.RunSheet.VehicleRegistration);
			var wrappedInstructions = wrapper.BookingInstructions.Cast<InstructionWrapper>().Select(i => i.WrappedObject);
			AssertContainsExactElementsInAnyOrder("Should have 6 runSheet instructions (but first is duplicated).", new[] { runSheetInstruction1, runSheetInstruction1, runSheetInstruction2, depotRunSheetInstruction, depotRunSheetInstruction, depotRunSheetInstruction }, wrappedInstructions);

			var wrappedInstructionsWithConsignment = wrapper.BookingInstructions.Cast<InstructionWrapper>().Select(i => i.Transport.JobNumber);
			AssertContainsExactElementsInAnyOrder("Should have 6 instructions, group per instruction and consignment (confirmation).", new ZString[] { "K123", "K456", "K789", "K123", "K456", "K789" }, wrappedInstructionsWithConsignment);
		}

		#endregion

		#region TestCosts

		public void TestRunSheetCosts()
		{
			var runSheet = GetRunSheet();
			var wrapper = new FreightWrapperFromDtbConsignmentRunSheet(runSheet, Factory);
			var costs = new JobConsolCostCollection(Factory, runSheet);

			JobConsolCost cost1 = costs.TryAddNew();
			JobConsolCost cost2 = costs.TryAddNew();
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost1.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			cost1.E6_OSCostAmount = 12.00m;
			cost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost2.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			cost2.E6_OSCostAmount = 46.50m;

			AssertEquals("Costs.Count: 2", 2, wrapper.Costs.Count);
			AssertEquals("Costs[0]: Populated", cost1, wrapper.Costs[0].WrappedObject);
			AssertEquals("Costs[1]: Populated", cost2, wrapper.Costs[1].WrappedObject);
			AssertEquals("Costs[0].CostAmount: 12.00", cost1.E6_OSCostAmount, wrapper.Costs[0].OSCost.Amount);
			AssertEquals("Costs[1].Currency: USD", cost2.E6_RX_NKCurrency, wrapper.Costs[1].OSCost.Currency.Code);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<DtbConsignmentRunSheet>();
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Run Sheet" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
RunSheet : (No Default Field Value Available on RunSheetFromCommonRunSheet)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromRunSheet(null, Factory);
		}

		#endregion

		#region Implementation

		DtbConsignmentRunSheet GetRunSheet()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment.KM_JobID = "K123";
			var instruction1 = consignment.PickupInstruction;

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment2.KM_JobID = "K456";
			var instruction2 = consignment2.PickupInstruction;

			var consignment3 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment3.KM_JobID = "K789";
			var instruction3 = consignment3.PickupInstruction;

			var truck = Helper.CreateVehicle("VHCL");
			truck.RQ_Registration = "Rego123";
			var runSheet = Helper.CreateRunSheet(ZDateTimeOffset.Today);
			var runSheetInstruction1 = Helper.CreateRunSheetInstruction(runSheet, new[] { instruction1.Confirmations[0], instruction2.Confirmations[0] });
			var runSheetInstruction2 = Helper.CreateRunSheetInstruction(runSheet, new[] { instruction3.Confirmations[0] });
			var depotRunSheetInstruction = runSheet.RunSheetInstructions.Single(i => i.IsOwnDepot);
			runSheet.KG_RunSheetNumber = "RS12345";
			runSheet.KG_RQ_Truck = truck.PK;
			runSheet.KG_GS_NKTruckDriver = Helper.CreateDriver("Bob", "Bob").GS_Code;

			return runSheet;
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
