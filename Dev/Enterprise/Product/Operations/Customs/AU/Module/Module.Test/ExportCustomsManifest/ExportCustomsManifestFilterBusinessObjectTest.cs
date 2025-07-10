using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ExportCustomsManifestFilterBusinessObject))]
	class ExportCustomsManifestFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLineCANFilter()
		{
			CreateHeaderWithLineCAN("AAAAMEJHS");
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, true);
			ModuleTextFilter canFilter = (ModuleTextFilter)filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CAN];
			canFilter.IsActive = true;
			canFilter.Property = "AAAAMEJHS";
			canFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertNotNull(Factory.LoadTop1<ExportCustomsManifestHeader>(filter.Filter));
		}

		public void TestHeaderCANFilter()
		{
			CreateHeaderWithCAN("AAAAMEJHS");
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, true);
			ModuleTextFilter canFilter = (ModuleTextFilter)filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CAN];
			canFilter.IsActive = true;
			canFilter.Property = "AAAAMEJHS";
			canFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertNotNull(Factory.LoadTop1<ExportCustomsManifestHeader>(filter.Filter));
		}

		public void TestLineAWBFilter()
		{
			CreateHeaderWithLineAWB("AAAAMEJHS");
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, true);
			ModuleTextFilter awbFilter = (ModuleTextFilter)filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.AirWayBill];
			awbFilter.IsActive = true;
			awbFilter.Property = "AAAAMEJHS";
			awbFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertNotNull(Factory.LoadTop1<ExportCustomsManifestHeader>(filter.Filter));
		}

		public void TestHeaderAWBFilter()
		{
			CreateHeaderWithAWB("AAAAMEJHS");
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, true);
			ModuleTextFilter awbFilter = (ModuleTextFilter)filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.AirWayBill];
			awbFilter.IsActive = true;
			awbFilter.Property = "AAAAMEJHS";
			awbFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertNotNull(Factory.LoadTop1<ExportCustomsManifestHeader>(filter.Filter));
		}

		public void TestIsSeaAndIsAir()
		{
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, true);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.AirWayBill]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CAN]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CountryOfDestination]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DepartureDate]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatus]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatusConditions]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Flight]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.FolioReference]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.JobNo]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.ManifestType]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDeparture]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDestination]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.TransportMode]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Vessel]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Voyage]);
		}

		public void TestOnlyIsSea()
		{
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(false, true);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.AirWayBill]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CAN]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CountryOfDestination]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DepartureDate]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatus]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatusConditions]);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Flight]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.FolioReference]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.JobNo]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.ManifestType]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDeparture]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDestination]);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.TransportMode]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Vessel]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Voyage]);
		}

		public void TestOnlyIsAir()
		{
			ExportCustomsManifestFilterBusinessObject filter = new ExportCustomsManifestFilterBusinessObject(true, false);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.AirWayBill]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CAN]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.CountryOfDestination]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DepartureDate]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatus]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.DocumentStatusConditions]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Flight]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.FolioReference]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.JobNo]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.ManifestType]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDeparture]);
			AssertNotNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.PortOfDestination]);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.TransportMode]);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Vessel]);
			AssertNull(filter[ExportCustomsManifestFilterBusinessObject.FilterConstants.Voyage]);
		}

		public void TestAllowSeaOnlyTransportModeFilter()
		{
			ExportCustomsManifestHeader seaHeader = Factory.New<ExportCustomsManifestHeader>();
			seaHeader.ED_TransportMode = Core.Constants.TransportModes.Sea;
			ExportCustomsManifestHeader airHeader = Factory.New<ExportCustomsManifestHeader>();
			airHeader.ED_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();
			ExportCustomsManifestFilterBusinessObject filterBO = new ExportCustomsManifestFilterBusinessObject(false, true);
			ExportCustomsManifestHeader[] foundHeaders = Factory.Load<ExportCustomsManifestHeader>(filterBO.Filter);
			AssertEquals(1, foundHeaders.Length);
			AssertEquals(seaHeader, foundHeaders[0]);
		}

		public void TestAllowAirOnlyTransportModeFilter()
		{
			ExportCustomsManifestHeader seaHeader = Factory.New<ExportCustomsManifestHeader>();
			seaHeader.ED_TransportMode = Core.Constants.TransportModes.Sea;
			ExportCustomsManifestHeader airHeader = Factory.New<ExportCustomsManifestHeader>();
			airHeader.ED_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();
			ExportCustomsManifestFilterBusinessObject filterBO = new ExportCustomsManifestFilterBusinessObject(true, false);
			ExportCustomsManifestHeader[] foundHeaders = Factory.Load<ExportCustomsManifestHeader>(filterBO.Filter);
			AssertEquals(1, foundHeaders.Length);
			AssertEquals(airHeader, foundHeaders[0]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExportCustomsManifestFilterBusinessObject(true, true);

		void CreateHeaderWithCAN(ZString cAN)
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_CAN = cAN;
			Factory.Save();
		}

		void CreateHeaderWithLineCAN(ZString cAN)
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.Lines.AddNew().EL_CAN = cAN;
			Factory.Save();
		}

		void CreateHeaderWithAWB(ZString aWB)
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_AirWayBill = aWB;
			Factory.Save();
		}

		void CreateHeaderWithLineAWB(ZString aWB)
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.Lines.AddNew().EL_AirWayBill = aWB;
			Factory.Save();
		}
	}
}
