using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(UnRelatedConsolsToAddCollection))]
	class UnRelatedConsolsToAddCollectionTests : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(UnRelatedConsolsToAddCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parentConsol = Factory.New<ForwardingConsol>();
			parentConsol.JK_RL_NKLoadPort = "GBLHR";
			parentConsol.JK_RL_NKDischargePort = "USATL";
			parentConsol.JK_TransportMode = "AIR";
			var wrapper = new CustomsExportConsolIntegrationWrapper(parentConsol, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			MakeUnrelatedConsols();
			return wrapper.UnRelatedConsolsToAdd;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var child = Factory.New<ForwardingConsol>();
			child.JK_RL_NKLoadPort = "GBLHR";
			child.JK_RL_NKDischargePort = "USATL";
			child.JK_TransportMode = "AIR";
			return child;
		}

		public void TestCollectionOnWrapper()
		{
			MakeUnrelatedConsols();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "AIR";
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			wrapper.RelatedConsols.Add(unrelatedExportAirConsol1);
			wrapper.UnRelatedConsolsToAdd.Load();
			AssertContainsExactElementsInAnyOrder("Collection contains only one unrelated consol of the 4 possible, because two are not relevant and the other is already related", new ForwardingConsol[] { unrelatedExportAirConsol2 }, wrapper.UnRelatedConsolsToAdd);
		}

		void MakeUnrelatedConsols()
		{
			unrelatedExportAirConsol1 = Factory.New<ForwardingConsol>();
			unrelatedExportAirConsol2 = Factory.New<ForwardingConsol>();
			unrelatedImportAirConsol = Factory.New<ForwardingConsol>();
			unrelatedExportSeaConsol = Factory.New<ForwardingConsol>();

			unrelatedExportAirConsol1.JK_RL_NKLoadPort = "GBLHR";
			unrelatedExportAirConsol2.JK_RL_NKLoadPort = "GBLHR";
			unrelatedImportAirConsol.JK_RL_NKLoadPort = "AUSYD";
			unrelatedExportSeaConsol.JK_RL_NKLoadPort = "GBFXT";

			unrelatedExportAirConsol1.JK_RL_NKDischargePort = "AUSYD";
			unrelatedExportAirConsol2.JK_RL_NKDischargePort = "USATL";
			unrelatedImportAirConsol.JK_RL_NKDischargePort = "GBLHR";
			unrelatedExportSeaConsol.JK_RL_NKDischargePort = "SGSIN";

			unrelatedExportAirConsol1.JK_TransportMode = "AIR";
			unrelatedExportAirConsol2.JK_TransportMode = "AIR";
			unrelatedImportAirConsol.JK_TransportMode = "AIR";
			unrelatedExportSeaConsol.JK_TransportMode = "SEA";
		}

		ForwardingConsol unrelatedExportAirConsol1;
		ForwardingConsol unrelatedExportAirConsol2;
		ForwardingConsol unrelatedImportAirConsol;
		ForwardingConsol unrelatedExportSeaConsol;
	}
}
