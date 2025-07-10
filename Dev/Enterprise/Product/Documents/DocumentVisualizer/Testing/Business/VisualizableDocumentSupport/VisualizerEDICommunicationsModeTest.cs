using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(VisualizerEDICommunicationsMode))]
	sealed class VisualizerEDICommunicationsModeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var comms = Factory.New<VisualizerEDICommunicationsMode>();

			AssertEquals(EDICommunicationsModeSchema.EK_CommunicationsTransport.Name,
				EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, comms.EK_CommunicationsTransport);

			AssertEquals(EDICommunicationsModeSchema.EK_CommsDirection.Name,
				EDICommunicationsModeCommsDirectionList.Codes.Transmit, comms.EK_CommsDirection);

			AssertEquals(EDICommunicationsModeSchema.EK_FileFormat.Name,
				EDICommunicationsModeFileFormatList.Codes.FXL, comms.EK_FileFormat);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var comms = Factory.New<VisualizerEDICommunicationsMode>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var query = new ZDBOnlyQuery(typeof(VisualizerEDICommunicationsMode));
			query.AddToFilter(EDICommunicationsModeSchema.PK, comms.PK);

			AssertEquals("expected not to be in the database", false, otherFactory.Load<VisualizerEDICommunicationsMode>(query).Any());
		}
	}
}
