using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(XXXImportManager))]
	public class XXXImportManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			XXXImportManager manager = new XXXImportManager(Factory, ValidFilename);
			AssertNotNull("Manager should not be null", manager);
			AssertNotNull("Manager.MasterAirCargo should not be null", manager.MasterAirCargo);
		}

		#region TestLoadFromFile
		public void TestLoadFromFile_ValidFile()
		{
			XXXImportManager manager = new XXXImportManager(Factory, ValidFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Manager.MasterAirCargo.AirCargos should not be null", manager.MasterAirCargo.AirCargos);
			AssertEquals("AirCargo Count", 6, manager.MasterAirCargo.AirCargos.Count);
		}

		public void TestLoadFromFile_InvalidFile()
		{
			XXXImportManager manager = new XXXImportManager(Factory, InvalidFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.InvalidFileFormat));
			string errorMessage = "First record is not a valid XXX Flight Record";
			AssertEquals("Bufer should contain error message '" + errorMessage + "': Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
		}

		public void TestLoadFromFile_LoadAllRecordsCorrectly()
		{
			XXXImportManager manager = new XXXImportManager(Factory, SmallFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Manager.MasterAirCargo.AirCargos should not be null", manager.MasterAirCargo.AirCargos);
			AssertEquals("AirCargo Count", 1, manager.MasterAirCargo.AirCargos.Count);
			AssertNotNull("AirCargo's ConsignmentDetails is not null", manager.MasterAirCargo.AirCargos[0].Consignment);
			AssertNotNull("AirCargo's ConsignmentNotes is not null", manager.MasterAirCargo.AirCargos[0].ConsignmentNotes);
			AssertEquals("AirCargo's ConsignmentNotes should have 2 records", 2, manager.MasterAirCargo.AirCargos[0].ConsignmentNotes.Count);
		}

		#endregion
		#region Implementation
		ZString ValidFilename;
		ZString InvalidFilename;
		ZString SmallFilename;

		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			ValidFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.XXXInterface.Testing.SYD.20050809.141533.xxx");
			InvalidFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.XXXInterface.Testing.SYD.20050811.091011.xxx");
			SmallFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.XXXInterface.Testing.SYD.20050822.201010.xxx");
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new XXXImportManager(Factory, "blah");
		}
		#endregion
	}
}
