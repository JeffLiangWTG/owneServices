using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(IQDownImportManager))]
	public class IQDownImportManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			IQDownImportManager manager = new IQDownImportManager(Factory, ValidFilename);
			AssertNotNull("Manager should not be null", manager);
			AssertNotNull("Manager.AirCargos should not be null", manager.AirCargos);
		}

		#region TestLoadFromFile
		public void TestLoadFromFile_ValidFile()
		{
			IQDownImportManager manager = new IQDownImportManager(Factory, ValidFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Manager.AirCargos should not be null", manager.AirCargos);
			AssertEquals("AirCargo Count", 5, manager.AirCargos.Count);
		}

		public void TestLoadFromFile_InvalidFile()
		{
			IQDownImportManager manager = new IQDownImportManager(Factory, InvalidFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.InvalidFileFormat));
			string errorMessage = "First record is not a valid IQDown Flight Record";
			AssertEquals("Bufer should contain error message '" + errorMessage + "': Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
		}

		public void TestLoadFromFile_LoadAllRecordsCorrectly()
		{
			IQDownImportManager manager = new IQDownImportManager(Factory, SmallFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Manager.AirCargos should not be null", manager.AirCargos);
			AssertEquals("AirCargo Count", 1, manager.AirCargos.Count);
			AssertNotNull("AirCargo's FlightDetail is not null", manager.AirCargos[0].FlightDetail);
			AssertNotNull("AirCargo's ConsignmentDetails is not null", manager.AirCargos[0].ConsignmentDetails);
			AssertEquals("AirCargo's ConsignmentDetails should have 2 records", 2, manager.AirCargos[0].ConsignmentDetails.Count);
			AssertNotNull("AirCargo's ConsignmentNotes is not null", manager.AirCargos[0].ConsignmentNotes);
			AssertEquals("AirCargo's ConsignmentNotes should have 2 records", 2, manager.AirCargos[0].ConsignmentNotes.Count);
		}

		#endregion
		#region Implementation
		ZString ValidFilename
		{
			get
			{
				return fValidFilename;
			}
		}

		ZString InvalidFilename
		{
			get
			{
				return fInvalidFilename;
			}
		}

		ZString SmallFilename
		{
			get
			{
				return fSmallFilename;
			}
		}

		string fValidFilename;
		string fInvalidFilename;
		string fSmallFilename;
		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			fValidFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050809.141533.ok");
			fInvalidFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050811.091011.ok");
			fSmallFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050822.201010.ok");
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IQDownImportManager(Factory, "blah");
		}
		#endregion
	}
}
