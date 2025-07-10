using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransShipmentListenerTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Human Readable Name", "CaroTrans Shipment Export", Listener.HumanReadableName);
		}

		public void TestIsSendingAgentDeclaredInTheRegistry()
		{
			AssertEquals("IsSendingAgentDeclaredInTheRegistry", false, Listener.IsSendingAgentDeclaredInTheRegistry(null));
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("IsSendingAgentDeclaredInTheRegistry", false, Listener.IsSendingAgentDeclaredInTheRegistry(consol));
			consol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			AssertEquals("IsSendingAgentDeclaredInTheRegistry", false, Listener.IsSendingAgentDeclaredInTheRegistry(consol));
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { consol.SendingForwarderPK.ToGuid() };
			AssertEquals("IsSendingAgentDeclaredInTheRegistry", true, Listener.IsSendingAgentDeclaredInTheRegistry(consol));
		}

		public void TestGetNewExporter()
		{
			CaroTransShipmentDataExporter currentExporter = Listener.GetNewExporter(Constants.CargoTransDefaultFileExtension);
			AssertEquals("Exporter type", typeof(CaroTransShipmentDataExporter), currentExporter.GetType());
			DeleteIfExists(currentExporter.ExportedFileForTesting);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Listener = new MockCaroTransShipmentListener();
		}

		public class MockCaroTransShipmentListener : CaroTransShipmentListener
		{
			public MockCaroTransShipmentListener() : base(Env.TempPath)
			{
			}

			public override string BusinessObjectTableName
			{
				get
				{
					return "";
				}
			}

			public override Type BusinessObjectType
			{
				get
				{
					return typeof(BusinessObject);
				}
			}

			protected override ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject)
			{
				return new ShipmentCollection(Factory);
			}

			public new bool IsSendingAgentDeclaredInTheRegistry(CommonConsol consol)
			{
				return base.IsSendingAgentDeclaredInTheRegistry(consol);
			}

			public new CaroTransShipmentDataExporter GetNewExporter(ZString fileExtension)
			{
				return base.GetNewExporter(fileExtension);
			}
		}

		MockCaroTransShipmentListener Listener;
		#endregion
	}
}
