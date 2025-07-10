using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.YAS.Business.Testing
{
	public class CargoImportBranchSetterTest : TestCaseWithFactory
	{
		static string anotherShipmentNumber = "";

		public void TestSetShipmentNumberJS_UniqueConsignRefEqualToJH_JobNum()
		{
			string shipmentNumber = CreateShipmentAndHeaderAndCompareJobNumber();
			AssertNotEquals("Shipment Numbert Uncustomized", anotherShipmentNumber, shipmentNumber);
			anotherShipmentNumber = shipmentNumber;
		}

		public void TestSetShipmentNumberCustomized()
		{
			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetBillOfLadingNumberCustomisation());
			string shipmentNumber = CreateShipmentAndHeaderAndCompareJobNumber();
			AssertNotEquals("Shipment Numbert Uncustomized", anotherShipmentNumber, shipmentNumber);
			anotherShipmentNumber = shipmentNumber;
		}

		string CreateShipmentAndHeaderAndCompareJobNumber()
		{
			YASForwardingShipment shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment.JS_UniqueConsignRef = ZString.Empty;
			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.Job.JH_GB = Factory.LoadTop1<GlbBranch>(GetBranchRelatedToPortQuery("AUSYD")).PK;
			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			AssertEquals("UniqueConsignRef==JobNum", shipment.JS_UniqueConsignRef, shipment.ShipmentJobHeader.JH_JobNum);
			return shipment.JS_UniqueConsignRef;
		}

		static BillOfLadingNumberCustomisation GetBillOfLadingNumberCustomisation()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			var branchElement = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode];
			branchElement.Include = true;
			branchElement.Order = 1;
			return customisation;
		}

		static ZDBOnlyQuery GetBranchRelatedToPortQuery(ZString portName)
		{
			ZDBOnlyQuery branchQuery = new ZDBOnlyQuery(typeof(GlbBranch));
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			branchQuery.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_RL_NKHomePort, portName);

			ZDBOnlySubQuery extraPortsQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
			extraPortsQuery.AddToFilter(JoinCondition.And, GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, portName);

			branchQuery.AddSubQuery(extraPortsQuery, JoinCondition.Or);

			return branchQuery;
		}
	}
}
		