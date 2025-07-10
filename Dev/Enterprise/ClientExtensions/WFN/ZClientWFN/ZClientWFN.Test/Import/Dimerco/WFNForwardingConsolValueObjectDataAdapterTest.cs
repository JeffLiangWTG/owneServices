using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WFN.Testing
{
	public class WFNForwardingConsolValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestExistingObjectIsNotUpdatedWhenImporting()
		{
			ZDateTime currentDate = new ZDateTime(2007, 4, 20);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "023-07174974";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBLON";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Transport transport = consol.Transports[0];
			transport.JW_ETD = currentDate;
			transport.JW_ETA = currentDate.AddDays(1);
			Factory.Save();
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = consolValue.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = "023-07174974";
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.TransportModeSpecified = true;
			Xsd.Movement discharge = new Xsd.Movement();
			Xsd.UNLOCO portOfDischarge = new Xsd.UNLOCO();
			portOfDischarge.Value = "AUSYD";
			discharge.Port = portOfDischarge;
			consolValue.ConsolDetail.PortOfDischarge = discharge;
			Xsd.Movement loading = new Xsd.Movement();
			Xsd.UNLOCO portOfLoading = new Xsd.UNLOCO();
			portOfLoading.Value = "GBLON";
			loading.Port = portOfLoading;
			consolValue.ConsolDetail.PortOfLoading = loading;
			consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = currentDate;
			consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = currentDate.AddDays(1);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, buffer);
			WFNForwardingConsolValueObjectDataAdapter adapter = new WFNForwardingConsolValueObjectDataAdapter();
			adapter.CreateOrUpdateFromValueObject(consolValue, importContext);
			AssertEquals("Consol should not have changes", false, consol.HasChanges);
			Assert(buffer.ContainsNotificationType(ErrorType.ImportingDataError));
			string expectedMessage = string.Format("{0} had already been imported and cannot be updated. XML file emailed to notification group.", "Consol C00001000" + string.Format(" (Master Bill='{0}')", "023-07174974"));
			Assert("Expected to contain:\r\n" + expectedMessage + "\r\n\r\nActual:\r\n" + buffer.AsString, buffer.AsString.Contains(expectedMessage));
		}
	}
}
