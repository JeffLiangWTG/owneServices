using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemTransferHeader))]
	sealed class FreightWrapperFromWhsItemTransferHeaderTest : FreightWrapperTest
	{
		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;
			var innerPackage = package2.Packages.AddNew("PLT", "P201");
			innerPackage.KP_Weight = 14;
			innerPackage.KP_Volume = 15;

			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var location = Helper.CreateLocation(warehouse, "RNO");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");

			var packageStateRow1 = Helper.CreatePackageState(package1, "ARV", rcn, rtu, dcn, dispatchLoadList: dll);
			var packageStateRow2 = Helper.CreatePackageState(package2, "ARV", rcn, rtu, dcn, dispatchLoadList: dll);

			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_WW_Warehouse = warehouse.PK;
			transferHeader.WTH_TransferType = "TRF";
			transferHeader.WTH_ReferenceNumber = "TRF1";
			transferHeader.WTH_IsFinalised = false;
			var toLocation = Helper.CreateLocation(warehouse);
			var transferLine1 = Helper.CreateTransferLine(transferHeader, warehouse.DefaultLocation, location, packageStateRow1);
			var transferLine2 = Helper.CreateTransferLine(transferHeader, warehouse.DefaultLocation, location, packageStateRow2);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemTransferHeader(transferHeader, Factory);
			AssertEquals(typeof(WhsItemTransferHeaderWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("TRF1", wrapper.JobNumber);
			AssertEquals("Transfer", wrapper.JobNumberHeading);
			AssertEquals(2, wrapper.Packages.Cast<PackageWrapper>().Count());
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(36m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Transfer" },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WhsItemTransferHeader>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemTransferHeader((WhsItemTransferHeader)GetNewBusinessObjectToWrap(), Factory);
		}

		#endregion

		#region Helpers

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
