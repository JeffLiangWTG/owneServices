using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[TestedType(typeof(AsycudaPackCollectionSynchroniser))]
	sealed class AsycudaPackCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronise_ShouldNotThrowException_WhenDeletedAndSimultaneouslyUpdatedFromRefreshBus()
		{
			var forwardingFactory = new BusinessObjectFactory();

			var forwardingConsol = forwardingFactory.NewWithValidTestData<ForwardingConsol>();
			var forwardingConsolContainer = forwardingConsol.Containers.AddNew();
			var forwardingShipment = forwardingConsol.Shipments.AddNew();
			forwardingShipment.OuterPackLines.RemoveAndDeleteAll();
			var packLine = forwardingShipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_JC = forwardingConsolContainer.PK;
			forwardingFactory.Save();

			var wrapper = new ManifestHeadersWrapper(forwardingConsol);
			var header = wrapper.CreateCountry("EU", "ENS");
			forwardingFactory.Save();

			var newFactory = new BusinessObjectFactory();

			var headerInNewFactory = newFactory.Load<AsycudaManifestHeader>(header.PK);
			headerInNewFactory.Synchroniser.Synchronise(true);
			headerInNewFactory.Bills[0].Packs.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(headerInNewFactory.Factory.Save);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			});
		}

		public void TestAsycudaPackCollectionSynchroniser()
		{
			var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var repackingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, departurePort.RL_RN_NKCountryCode }));
			var destinationPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			string containerNumber1 = "CNHK1234567";
			string containerNumber2 = "CRXU1234569";

			var containerType = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = departurePort.Code;
			shipment.JS_RL_NKDestination = destinationPort.Code;

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = departurePort.Code;
			consol.JK_RL_NKDischargePort = repackingPort.Code;

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			AssertEquals("Precondition", 0, manifestBill.Packs.Count);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			AssertEquals("1 Pack Sychronized", 1, manifestBill.Packs.Count);

			packLine1.JL_HarmonisedCode = "00001623";
			AssertEquals("Should sync the JL_HarmonisedCode.", packLine1.JL_HarmonisedCode, manifestBill.Packs[0].PackedItem.API_Tariff);

			var harmonisedCodes = packLine1.HarmonisedCodes.AddNew();
			harmonisedCodes.JLH_Code = "237832";
			harmonisedCodes.JLH_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Should sync the JLH_Code.", "237832", manifestBill.Packs[0].PackedItem.API_Tariff);

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = containerType.PK;
			container1.JC_ContainerNum = containerNumber1;
			container1.PackLines.Add(packLine1);
			AssertEquals("1 Pack Sychronized", containerNumber1, manifestBill.Packs[0].Container.ACN_ContainerNumber);
			AssertEquals("Number of Container Packages - 1", 1, manifestBill.Packs[0].Container.ACN_NumberOfPackages);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			var container2 = consol.Containers.AddNew();
			container2.JC_RC = containerType.PK;
			container2.JC_ContainerNum = containerNumber2;
			container2.PackLines.Add(packLine2);
			AssertEquals("2 Pack Sychronized", 2, manifestBill.Packs.Count);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 3;
			container1.PackLines.Add(packLine3);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("3 Pack Sychronized", 3, manifestBill.Packs.Count);
			AssertEquals("Number of Packages - 3", 3, manifestBill.Packs[2].APA_PackQty);
			AssertEquals("Number of Container Packages - 4", 4, manifestBill.Packs[2].Container.ACN_NumberOfPackages);
		}

		public void TestAsycudaPackSynchroniseOnlyWhenSupportsAsycudaPacksIsTrue()
		{
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationBusinessProvider").Cast<ApplicationBusinessProvider>().Append(new DummyApplicationBusinessProvider("XYZ", new string[] { "US" })).ToList()))
			{
				var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				var repackingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, departurePort.RL_RN_NKCountryCode }));
				var destinationPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = departurePort.Code;
				shipment.JS_RL_NKDestination = destinationPort.Code;

				var consol = shipment.Consols.AddNew();
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
				consol.JK_RL_NKLoadPort = departurePort.Code;
				consol.JK_RL_NKDischargePort = repackingPort.Code;

				Factory.Save();

				shipment.OuterPackLines.AddNew();
				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.SetParent(consol);
				manifestHeader.Synchroniser.SetEnabled(true, false);
				manifestHeader.Synchroniser.Synchronise();
				var manifestBill = manifestHeader.Bills[0];
				Assert("A AsycudaManifestHeader whose SupportsAsycudaPacks should be true", manifestHeader.FeatureProvider.SupportsAsycudaPacks);
				AssertEquals("Should sychronize when SupportsAsycudaPacks is true", 1, manifestBill.Packs.Count);

				var usManifestHeader = Factory.New<DummyAsycudaManifestHeader>();
				usManifestHeader.AMA_ManifestType = "XYZ";
				usManifestHeader.SetParent(consol);
				usManifestHeader.Synchroniser.SetEnabled(true, false);
				usManifestHeader.Synchroniser.Synchronise();
				var usManifestBill = usManifestHeader.Bills[0];
				Assert("A AsycudaManifestHeader whose SupportsAsycudaPacks should be false", !usManifestHeader.FeatureProvider.SupportsAsycudaPacks);
				AssertEquals("Should not sychronize when SupportsAsycudaPacks is false", 0, usManifestBill.Packs.Count);
			}
		}

		public void TestAsycudaPackHasEmptyHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			Factory.Save();
			manifestHeader.Delete();

			var manifestHeader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader2.SetParent(consol);
			var bill = manifestHeader.Bills.AddNew();

			var synchroniser = new AsycudaPackCollectionSynchroniser(shipment, bill);
			AssertNoExceptionThrown(() => synchroniser.Synchronise(true));
		}
	}
}
