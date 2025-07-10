using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.NZ;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumSegmentTest : TestCaseWithFactory
	{
		public void TestValidSegment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			ZString fileName = "BNE.X1.20040618.135245.ok";
			var resourceRetriever = new EmbeddedResourceRetriever();
			string fileData = resourceRetriever.GetString("Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing." + fileName);
			NotificationBuffer buffer = new NotificationBuffer();
			int endLine = 0;
			TNTDataImporter importer = new TNTDataImporter();
			string[] fileLines = fileData.Split(new char[] { '\n' });
			QuantumSegment segment = QuantumSegment.FromData(fileName.Left(3), fileLines, 0, out endLine, buffer);
			AssertNotNull("File contains valid data, expecting populated segment.", segment);
			AssertEquals("EndLine should be 7", 7, endLine);
			AssertNotNull(segment.Consol);
			AssertEquals("Expecting 2 shipments in the segment.", 2, segment.Shipments.Length);
			AssertEquals("Expecting QuantumShipmentRecord to be created", typeof(QuantumShipmentRecord), segment.Shipments[0].GetType());
			AssertEquals("Expecting QuantumShipmentRecord to be created", typeof(QuantumShipmentRecord), segment.Shipments[1].GetType());
			AssertEquals("Expecting 2 shipments' notes in the segment.", 2, segment.ShipmentsNotes.Length);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			segment = QuantumSegment.FromData(fileName.Left(3), fileLines, 0, out endLine, buffer);
			AssertEquals("Expecting 2 shipments in the segment", 2, segment.Shipments.Length);
			AssertEquals("Expecting NZQuantumShipmentRecord to be created", typeof(NZQuantumShipmentRecord), segment.Shipments[0].GetType());
			AssertEquals("Expecting NZQuantumShipmentRecord to be created", typeof(NZQuantumShipmentRecord), segment.Shipments[1].GetType());
		}
	}
}
