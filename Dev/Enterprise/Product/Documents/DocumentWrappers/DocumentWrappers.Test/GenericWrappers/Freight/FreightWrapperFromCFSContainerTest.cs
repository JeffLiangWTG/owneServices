using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCFSContainer))]
	sealed class FreightWrapperFromCFSContainerTest : FreightWrapperTest
	{
		public void TestJobNumber()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_ContainerJobID = "D001234";
			FreightWrapperFromCFSContainer wrapper = new FreightWrapperFromCFSContainer(container, Factory);
			AssertEquals("JobNumber", "D001234", wrapper.JobNumber);
		}

		public void TestContainers()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_ContainerJobID = "D001234";
			FreightWrapperFromCFSContainer wrapper = new FreightWrapperFromCFSContainer(container, Factory);
		}

		public void TestPackagesSortedByShipmentNumber()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			CFSShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00003";
			CFSShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001";
			CFSShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00002";

			CFSPackLine packLine1 = shipment1.OuterPackLines.AddNew();
			CFSPackLine packLine2 = shipment2.OuterPackLines.AddNew();
			CFSPackLine packLine3 = shipment3.OuterPackLines.AddNew();

			CFSContainer container = consol.Containers.AddNew();
			container.AddPackLine(packLine1);
			container.AddPackLine(packLine2);
			container.AddPackLine(packLine3);

			FreightWrapperFromCFSContainer wrapper = new FreightWrapperFromCFSContainer(container, Factory);
			AssertEquals("Packages.Count", 3, wrapper.Packages.Count);

			AssertNotNull("Packages[0].Parent", wrapper.Packages[0].Parent);
			AssertNotNull("Packages[0].Parent.CFSShipment", wrapper.Packages[0].Parent.CFSShipment);
			AssertEquals("Packages[0].Parent.CFSShipment.JS_UniqueConsignRef", "S00001", wrapper.Packages[0].Parent.CFSShipment.JS_UniqueConsignRef);

			AssertNotNull("Packages[1].Parent", wrapper.Packages[1].Parent);
			AssertNotNull("Packages[1].Parent.CFSShipment", wrapper.Packages[1].Parent.CFSShipment);
			AssertEquals("Packages[1].Parent.CFSShipment.JS_UniqueConsignRef", "S00002", wrapper.Packages[1].Parent.CFSShipment.JS_UniqueConsignRef);

			AssertNotNull("Packages[2].Parent", wrapper.Packages[2].Parent);
			AssertNotNull("Packages[2].Parent.CFSShipment", wrapper.Packages[2].Parent.CFSShipment);
			AssertEquals("Packages[2].Parent.CFSShipment.JS_UniqueConsignRef", "S00003", wrapper.Packages[2].Parent.CFSShipment.JS_UniqueConsignRef);
		}

		public override void TestContainerLayoutStyle()
		{
			AssertEquals("SingleContainer", Wrapper.ContainerLayoutStyle);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ContainerLayoutStyle", "SingleContainer" },
					{ "ContainerSummary", " x 1" },
					{ "ContainerCount", "1" },
					{ "JobNumberHeading", "Job Number" }
				};
			}
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CFSContainer>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			return new FreightWrapperFromCFSContainer(container, Factory);
		}
	}
}
