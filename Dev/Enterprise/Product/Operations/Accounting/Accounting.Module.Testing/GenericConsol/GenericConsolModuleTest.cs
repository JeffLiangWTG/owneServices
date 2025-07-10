using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericConsolModule))]
	public class GenericConsolModuleTest : ZModuleBasherTest
	{
		#region TestGetNewController

		public void TestGetNewController()
		{
			using (var module = new TestGenericConsolModule())
			{
				var controller1 = module.GetNewController(null);
				AssertEquals("Should create Job Consol Controller if BizO is null.", ControllerIDs.JobConsol, controller1.ID);

				var forwardingShipment = Factory.New<ForwardingConsol>();
				var controller2 = module.GetNewController(forwardingShipment);
				AssertEquals("Should create Job Consol Controller if BizO is not GenericConsol.", ControllerIDs.JobConsol, controller2.ID);

				var genericConsol = Factory.New<GenericConsol>();
				var controller3 = module.GetNewController(genericConsol);
				AssertEquals("Should create Job Consol Controller if BizOs Parent Table Prefix not specified.", ControllerIDs.JobConsol, controller3.ID);

				genericConsol.VX_ParentTableCode = DtbBookingConsolidationSchema.Constants.Prefix;
				var controller4 = module.GetNewController(genericConsol);
				AssertEquals("Should create DtbBookingConsolidation Controller if BizOs Parent Table Prefix is DtbBookingConsolidation.", ControllerIDs.DtbBookingConsolidation, controller4.ID);

				genericConsol.VX_ParentTableCode = DtbConsignmentRunSheetSchema.Constants.Prefix;
				var controller5 = module.GetNewController(genericConsol);
				AssertEquals("Should create DtbConsignmentRunSheet Controller if BizOs Parent Table Prefix is DtbConsignmentRunSheet.", ControllerIDs.DtbConsignmentRunSheet, controller5.ID);
			}
		}

		#endregion

		#region TestAllowExcelExport

		public void TestAllowExcelExport()
		{
			using (TestGenericConsolModule module = new TestGenericConsolModule())
			{
				AssertEquals("This is unsupported because there is no index on the PK which is required by " + nameof(FilteredBusinessObjectReader), false, module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		#endregion

		#region TestGetParentConsolFromGenericConsol

		public void TestGetParentConsolFromGenericConsol()
		{
			var bookingConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			bookingConsolidation[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var forwardingConsolidation = Factory.NewWithValidTestData<ForwardingConsol>();
			var runSheet = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			Factory.Save();

			var genericConsols = new GenericConsolCollection(Factory);
			AssertEquals("All 3 consols should be found.", 3, genericConsols.Count);

			using (var module = new TestGenericConsolModule())
			{
				var genericForwardingConsol = genericConsols.Single(c => c.VX_ParentTableCode == JobConsolSchema.Constants.Prefix);
				var genericTransportBookingConsol = genericConsols.Single(c => c.VX_ParentTableCode == DtbBookingConsolidationSchema.Constants.Prefix);
				var genericRunSheetConsol = genericConsols.Single(c => c.VX_ParentTableCode == DtbConsignmentRunSheetSchema.Constants.Prefix);
				AssertEquals(forwardingConsolidation.PK, module.GetParentConsolFromGenericConsol(genericForwardingConsol).PK);
				AssertEquals(bookingConsolidation.PK, module.GetParentConsolFromGenericConsol(genericTransportBookingConsol).PK);
				AssertEquals(runSheet.PK, module.GetParentConsolFromGenericConsol(genericRunSheetConsol).PK);
			}
		}

		public void TestShowNewForm()
		{
			using (var module = new TestGenericConsolModule())
			{
				AssertEquals(null, module.ShowNewForm());
				AssertEquals("You cannot create a new Consol here", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestGetSecurityCheckpointForPopups()
		{
			using (var module = new TestGenericConsolModule())
			{
				var result = module.GetSecurityCheckpointForPopups();
				AssertEquals("The security check point for popup control should be NewPayablesTransaction", 1, result.Length);
				AssertEquals("The security check point for popup control should be NewPayablesTransaction", Env.Security.NewPayablesTransaction, result[0]);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GenericConsol;
		}

		// This module does its own thing for showing Delete forms, bypassing the thread-safety measures of the base class. This could result in defects if the module is used in a background thread.
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		class TestGenericConsolModule : GenericConsolModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return base.GetNewController(selectedBusinessObject);
			}

			public new IModuleDecisionProvider ModuleDecisionProvider
			{
				get { return base.ModuleDecisionProvider; }
			}

			public new BusinessObject GetParentConsolFromGenericConsol(GenericConsol sourceEntity)
			{
				return base.GetParentConsolFromGenericConsol(sourceEntity);
			}

			public new IZForm ShowNewForm()
			{
				return base.ShowNewForm();
			}
		}
	}
}
