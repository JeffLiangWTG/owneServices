using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class VisualizerEDICommunicationsMode : EDICommunicationsMode
	{
		public VisualizerEDICommunicationsMode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FXL;
		}

		public override bool IsSavedByFactory => false;
	}
}