using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class MaritimeINVCDTLineTestCase : ShipmentINVCDTLineTestCase
	{
		#region TestLineAsString
		protected override void SetupInvoiceWrapperForTestLineAsString()
		{
			base.SetupInvoiceWrapperForTestLineAsString();
			InvoiceWrapper.Shipment.JS_HouseBill = "MEHMEH";
			InvoiceWrapper.Shipment.JS_RL_NKOrigin = "USBOS";
			InvoiceWrapper.Shipment.JS_RL_NKDestination = "ESMAD";
			JASForwardingConsol consol = (JASForwardingConsol)InvoiceWrapper.Shipment.Consols[0];
			consol.SetDefaultShippingLineAddress(Factory.New<JASOrgHeader>());
			OrgPatternMatchOverride @override = consol.ShippingLine.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
			@override.OO_ForeignCode = "ABC";
			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "VOY123";
			transport.JW_Vessel = "VESSEL 123";
		}

		protected override ZString[] GetExpectedLineContentForTestLineAsString()
		{
			ZString[] result = base.GetExpectedLineContentForTestLineAsString();
			result[ExpectedMINVCDTFieldPositions.EstimatedShippingDate] = "10/12/2005";
			result[ExpectedMINVCDTFieldPositions.OBLSerialNumber] = "MEHMEH";
			result[ExpectedMINVCDTFieldPositions.PortOfDischarge] = "ESMAD";
			result[ExpectedMINVCDTFieldPositions.PortOfLoading] = "USBOS";
			result[ExpectedMINVCDTFieldPositions.SSLCode] = "ABC";
			result[ExpectedMINVCDTFieldPositions.VesselAndVoyage] = "VESSEL 123 / VOY123";
			return result;
		}

		#endregion
		JXCConstants.MINVCDTFieldPositions ExpectedMINVCDTFieldPositions
		{
			get
			{
				return (JXCConstants.MINVCDTFieldPositions)ExpectedFieldPositions;
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.MINVCDTFieldCount;
			}
		}

		protected override JXCConstants.INVCDTFieldPositions ExpectedFieldPositions
		{
			get
			{
				return new JXCConstants.MINVCDTFieldPositions();
			}
		}

		protected override INVCDTLine GetINVCDTLine(InvoiceWrapper invoiceWrapper)
		{
			return new MaritimeINVCDTLine(invoiceWrapper);
		}

		protected override ZString ShipmentTransportMode
		{
			get
			{
				return Core.Constants.TransportModes.Sea;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
		}
	}
}
