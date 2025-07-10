using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsLoad))]
	sealed class FreightWrapperFromWhsLoadTest : FreightWrapperTest
	{
		#region TestJobNumber

		public void TestJobNumber()
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_JobID = "TestLoad";

			var wrapper = new FreightWrapperFromWhsLoad(load, Factory);
			AssertEquals(nameof(wrapper.JobNumber), "TestLoad", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber()
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_TransportationUnitNumber = "Transportation Unit";

			var wrapper = new FreightWrapperFromWhsLoad(load, Factory);
			AssertEquals(nameof(wrapper.SecondaryNumber), "Transportation Unit", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var transportCompany = Helper.CreateClient("TC1");
			var load = Factory.New<WhsLoad>();
			load.WLO_OH_TransportCompany = transportCompany.PK;

			var wrapper = new FreightWrapperFromWhsLoad(load, Factory);
			AssertEquals(nameof(wrapper.Carrier), "TC1", wrapper.Carrier.CompanyCode);
		}

		#endregion

		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			var transportCompany = Helper.CreateClient("TC1");
			var service = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			service.PL_CarrierServiceCode = "CSL";
			service.PL_Code = "CSL";

			var load = Factory.New<WhsLoad>();
			load.WLO_OH_TransportCompany = transportCompany.PK;
			load.WLO_PL_NKCarrierServiceLevel = "CSL";

			var wrapper = new FreightWrapperFromWhsLoad(load, Factory);
			AssertEquals(nameof(wrapper.CarrierServiceLevel), "CSL", wrapper.CarrierServiceLevel.Code);
		}

		#endregion

		#region TestWarehouseJob

		public void TestWarehouseJob()
		{
			var load = Factory.New<WhsLoad>();

			var wrapper = new FreightWrapperFromWhsLoad(load, Factory);
			AssertEquals("WarehouseJob", load.PK, wrapper.WarehouseJob.WrappedObjectPK);
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Load" },
					{ "SecondaryHeading", "Transport Unit Number" }
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

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var whsLoad = (WhsLoad)GetNewBusinessObjectToWrap();
			return new FreightWrapperFromWhsLoad(whsLoad, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WhsLoad>();
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
