using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class SupplyTypeHelperTest : TestCaseWithFactory
	{
		#region Get Supply Type Defaulting

		public void TestGetSupplyTypeDefaulting_DSB()
		{
			AssertGetSupplyTypeDefaulting_DSB(true, AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB);
			AssertGetSupplyTypeDefaulting_DSB(false, string.Empty);
		}

		void AssertGetSupplyTypeDefaulting_DSB(bool registry, string expected)
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);

			var shipment = TestObjectCreator.CreateShipment("S001", false);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var supplyType = job.GetSupplyType(TestObjectCreator.DSBChargeCode, Core.Constants.ChargeType.Disbursement, GlbDepartment.CurrentDepartment.PK);

				AssertEquals(expected, supplyType);
			}
		}

		public void TestGetSupplyTypeDefaulting_SupplyTypeOverrides()
		{
			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_TransportMode = Core.Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_Direction = Core.Constants.TransportModes.All;
			supplyTypeOverride.ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			supplyTypeOverride.ACS_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var supplyType = job.GetSupplyType(TestObjectCreator.FRT, Core.Constants.ChargeType.Margin, GlbDepartment.CurrentDepartment.PK);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, supplyType);
			}
		}

		public void TestGetSupplyTypeDefaulting_SupplyTypeConfigurations()
		{
			var supplyTypeConfigurationByChargeGroupCollection = AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value;
			foreach (SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup in supplyTypeConfigurationByChargeGroupCollection)
			{
				if (supplyTypeConfigurationByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					var supplyTypeConfiguration = supplyTypeConfigurationByChargeGroup.ChargeGroupSettings.AddNew();
					supplyTypeConfiguration.JobType = "SHP";
					supplyTypeConfiguration.Mode = Core.Constants.FreightShipmentDirection.Code.All;
					supplyTypeConfiguration.DirectionCode = Core.Constants.TransportModes.All;
					supplyTypeConfiguration.Incoterm = AccountingMasterFilesConstants.INCOTermCodes.All;
					supplyTypeConfiguration.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
					supplyTypeConfiguration.SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				}
			}

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, supplyTypeConfigurationByChargeGroupCollection);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var supplyType = job.GetSupplyType(TestObjectCreator.FRT, Core.Constants.ChargeType.Margin, GlbDepartment.CurrentDepartment.PK);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, supplyType);
			}
		}

		public void TestGetSupplyTypeDefaulting()
		{
			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_TransportMode = Core.Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_Direction = Core.Constants.TransportModes.All;
			supplyTypeOverride.ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			supplyTypeOverride.ACS_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			Factory.Save();

			var supplyTypeConfigurationByChargeGroupCollection = AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value;
			foreach (SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup in supplyTypeConfigurationByChargeGroupCollection)
			{
				if (supplyTypeConfigurationByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					var supplyTypeConfiguration = supplyTypeConfigurationByChargeGroup.ChargeGroupSettings.AddNew();
					supplyTypeConfiguration.JobType = "SHP";
					supplyTypeConfiguration.Mode = Core.Constants.FreightShipmentDirection.Code.All;
					supplyTypeConfiguration.DirectionCode = Core.Constants.TransportModes.All;
					supplyTypeConfiguration.Incoterm = AccountingMasterFilesConstants.INCOTermCodes.All;
					supplyTypeConfiguration.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
					supplyTypeConfiguration.SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
				}
			}

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, supplyTypeConfigurationByChargeGroupCollection);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var supplyType = job.GetSupplyType(TestObjectCreator.FRT, Core.Constants.ChargeType.Margin, GlbDepartment.CurrentDepartment.PK);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, supplyType);
			}
		}

		public void TestGetSupplyTypeDefaulting_Empty()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertEquals("Precondition", 0, TestObjectCreator.FRT.SupplyTypeOverrides.Count);
			foreach (SupplyTypeConfigurationByChargeGroup group in AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value)
			{
				AssertEquals("Precondition", 0, group.ChargeGroupSettings.Count);
			}

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var supplyType = job.GetSupplyType(TestObjectCreator.FRT, Core.Constants.ChargeType.Margin, GlbDepartment.CurrentDepartment.PK);

				AssertNullOrEmpty(supplyType);
			}
		}

		public void TestGetSupplyTypeByConsolDefaulting_Emtpy()
		{
			Assert(true);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertEquals("Precondition", 0, TestObjectCreator.FRT.SupplyTypeOverrides.Count);
			foreach (SupplyTypeConfigurationByChargeGroup group in AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value)
			{
				AssertEquals("Precondition", 0, group.ChargeGroupSettings.Count);
			}

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001") as IJobCostingPlugIn;
			var supplyType = consol.GetSupplyType(TestObjectCreator.FRT);
			AssertNullOrEmpty(supplyType);
		}

		public void TestGetSupplyTypeByConsolDefaulting_ALLConfiguration()
		{
			var supplyTypeConfigurationByChargeGroupCollection = AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value;
			foreach (SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup in supplyTypeConfigurationByChargeGroupCollection)
			{
				if (supplyTypeConfigurationByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					var supplyTypeConfiguration = SetupSupplyTypeChargeGroup(supplyTypeConfigurationByChargeGroup, "ALL", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT);
				}
			}
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, supplyTypeConfigurationByChargeGroupCollection))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001") as IJobCostingPlugIn;
				SetupFCNSupplyTypeOverrideBySupplyType(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT);
				var supplyType = consol.GetSupplyType(TestObjectCreator.FRT);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT, supplyType);
			}
		}

		public void TestGetSupplyTypeByConsolDefaulting_FCNConfiguration()
		{
			var supplyTypeConfigurationByChargeGroupCollection = AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value;
			foreach (SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup in supplyTypeConfigurationByChargeGroupCollection)
			{
				if (supplyTypeConfigurationByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					var supplyTypeConfiguration = SetupSupplyTypeChargeGroup(supplyTypeConfigurationByChargeGroup, "FCN", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, supplyTypeConfigurationByChargeGroupCollection))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001") as IJobCostingPlugIn;
				SetupFCNSupplyTypeOverrideBySupplyType(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT);
				var supplyType = consol.GetSupplyType(TestObjectCreator.FRT);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INT, supplyType);
			}
		}

		public void TestGetSupplyTypeByConsolDefaulting_EmptyOverride()
		{
			var supplyTypeConfigurationByChargeGroupCollection = AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value;
			foreach (SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup in supplyTypeConfigurationByChargeGroupCollection)
			{
				if (supplyTypeConfigurationByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					var supplyTypeConfiguration = SetupSupplyTypeChargeGroup(supplyTypeConfigurationByChargeGroup, "FCN", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, supplyTypeConfigurationByChargeGroupCollection))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001") as IJobCostingPlugIn;
				var supplyType = consol.GetSupplyType(TestObjectCreator.FRT);

				AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, supplyType);
			}
		}

		SupplyTypeConfiguration SetupSupplyTypeChargeGroup(SupplyTypeConfigurationByChargeGroup supplyTypeConfigurationByChargeGroup, ZString jobType, ZString supplyType)
		{
			var supplyTypeConfiguration = supplyTypeConfigurationByChargeGroup.ChargeGroupSettings.AddNew();
			supplyTypeConfiguration.JobType = jobType;
			supplyTypeConfiguration.Mode = Core.Constants.FreightShipmentDirection.Code.All;
			supplyTypeConfiguration.DirectionCode = Core.Constants.TransportModes.All;
			supplyTypeConfiguration.Incoterm = AccountingMasterFilesConstants.INCOTermCodes.All;
			supplyTypeConfiguration.LineDepartmentPK = ZGuid.Empty;
			supplyTypeConfiguration.SupplyType = supplyType;

			return supplyTypeConfiguration;
		}

		void SetupFCNSupplyTypeOverrideBySupplyType(ZString supplyType)
		{
			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "FCN";
			supplyTypeOverride.ACS_TransportMode = Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_Direction = Constants.TransportModes.All;
			supplyTypeOverride.ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = ZGuid.Empty;
			supplyTypeOverride.ACS_SupplyType = supplyType;
			Factory.Save();
		}

		#endregion Get Supply Type Defaulting

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
