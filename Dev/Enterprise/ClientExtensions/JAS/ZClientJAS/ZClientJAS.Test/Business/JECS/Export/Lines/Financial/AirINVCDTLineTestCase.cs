using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class AirINVCDTLineTestCase : ShipmentINVCDTLineTestCase
	{
		#region TestLineAsString
		protected override void SetupInvoiceWrapperForTestLineAsString()
		{
			base.SetupInvoiceWrapperForTestLineAsString();
			InvoiceWrapper.Shipment.JS_RL_NKOrigin = "USBOS";
			InvoiceWrapper.Shipment.JS_RL_NKDestination = "ESMAD";
			JASForwardingConsol consol = (JASForwardingConsol)InvoiceWrapper.Shipment.Consols[0];
			consol.JK_MasterBillNum = "08112345678";
			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF099";
		}

		protected override ZString[] GetExpectedLineContentForTestLineAsString()
		{
			ZString[] result = base.GetExpectedLineContentForTestLineAsString();
			result[ExpectedAINVCDTFieldPositions.AirlinePrefix] = "081";
			result[ExpectedAINVCDTFieldPositions.MAWBSerialNumber] = "12345678";
			result[ExpectedAINVCDTFieldPositions.AirportOfDestination] = "MAD";
			result[ExpectedAINVCDTFieldPositions.AirportOfOrigin] = "BOS";
			result[ExpectedAINVCDTFieldPositions.FirstFlightDate] = "10/12/2005";
			result[ExpectedAINVCDTFieldPositions.FirstFlightNumber] = "QF099";
			result[ExpectedAINVCDTFieldPositions.ImportOrExportShipment] = "I";
			return result;
		}

		#endregion
		public void TestLineAsStringWhenUNLOCODoNotHaveIATACode()
		{
			base.SetupInvoiceWrapperForTestLineAsString();
			AssertEquals("Sanity-check, make sure this port does not have IATA port", "", new RefUNLOCO.Loader(Factory).Load("DKHEL").RL_IATA);
			AssertEquals("Sanity-check, make sure this port does not have IATA port", "", new RefUNLOCO.Loader(Factory).Load("AUGLL").RL_IATA);
			InvoiceWrapper.Shipment.JS_RL_NKOrigin = "DEHEL";
			InvoiceWrapper.Shipment.JS_RL_NKDestination = "AUGLL";
			ZString[] lineContent = base.GetExpectedLineContentForTestLineAsString();
			lineContent[ExpectedAINVCDTFieldPositions.AirportOfOrigin] = "HEL";
			lineContent[ExpectedAINVCDTFieldPositions.AirportOfDestination] = "GLL";
			lineContent[ExpectedAINVCDTFieldPositions.ImportOrExportShipment] = "E";
			lineContent[ExpectedAINVCDTFieldPositions.FirstFlightDate] = "10/12/2005";
			ZString expectedLineAsString = ExpectedLineType + JXCConstants.Version + JXCConstants.Delimiter + ZString.Join(JXCConstants.Delimiter.ToString(), lineContent);
			AssertEquals("LineAsString", expectedLineAsString, Line.LineAsString);
		}

		protected override ZString[] GetExpectedLineContentAsStringArray_ShipmentHasNoConsol()
		{
			ZString[] result = base.GetExpectedLineContentAsStringArray_ShipmentHasNoConsol();
			result[ExpectedAINVCDTFieldPositions.ImportOrExportShipment] = "E";
			return result;
		}

		protected override ZString[] GetExpectedLineContentAsStringArray_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength()
		{
			ZString[] result = base.GetExpectedLineContentAsStringArray_ShipperDetailsShouldBeTrimmedIfExceedingMaxLength();
			result[ExpectedAINVCDTFieldPositions.ImportOrExportShipment] = "E";
			return result;
		}

		protected override ZString[] GetExpectedLineContentForTestLineAsString_NonJobInvoice()
		{
			ZString[] result = base.GetExpectedLineContentForTestLineAsString_NonJobInvoice();
			result[ExpectedAINVCDTFieldPositions.ImportOrExportShipment] = "E";
			return result;
		}

		JXCConstants.AINVCDTFieldPositions ExpectedAINVCDTFieldPositions
		{
			get
			{
				return (JXCConstants.AINVCDTFieldPositions)ExpectedFieldPositions;
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.AINVCDTFieldCount;
			}
		}

		protected override JXCConstants.INVCDTFieldPositions ExpectedFieldPositions
		{
			get
			{
				return new JXCConstants.AINVCDTFieldPositions();
			}
		}

		protected override INVCDTLine GetINVCDTLine(InvoiceWrapper invoiceWrapper)
		{
			return new AirINVCDTLine(invoiceWrapper);
		}

		protected override ZString ShipmentTransportMode
		{
			get
			{
				return Core.Constants.TransportModes.Air;
			}
		}
	}
}
