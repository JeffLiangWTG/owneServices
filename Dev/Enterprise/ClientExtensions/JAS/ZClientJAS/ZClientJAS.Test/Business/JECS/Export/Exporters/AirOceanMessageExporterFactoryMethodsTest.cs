using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class AirOceanMessageExporterFactoryMethodsTest : TestCaseWithFactory
	{
		public void TestNewMessageExporterShouldNotReturnNull_FromConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(consol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Courier;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(consol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(consol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Unknown;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(consol));
		}

		public void TestNewMessageExporter_FromConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JXCMessageExporter exporter = AirOceanMessageExporter.New(consol);
			AssertJXCMessageExporterReturned(typeof(AirMessageExporter), exporter);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			exporter = AirOceanMessageExporter.New(consol);
			AssertJXCMessageExporterReturned(typeof(OceanMessageExporter), exporter);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNewMessageExporter_NullConsol()
		{
			JASForwardingConsol consol = null;
			AirOceanMessageExporter.New(consol);
		}

		public void TestNewMessageExporterShouldNotReturnNull_FromPreShipment()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			PreShipmentWrapper preShipmentWrapper = new PreShipmentWrapper(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(preShipmentWrapper));
			shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(preShipmentWrapper));
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(preShipmentWrapper));
			shipment.JS_TransportMode = Core.Constants.TransportModes.Unknown;
			AssertNotNull("Should not return null regardless of transport mode", AirOceanMessageExporter.New(preShipmentWrapper));
		}

		public void TestNewMessageExporter_FromPreShipment()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			PreShipmentWrapper preShipmentWrapper = new PreShipmentWrapper(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			JXCMessageExporter exporter = AirOceanMessageExporter.New(preShipmentWrapper);
			AssertJXCMessageExporterReturned(typeof(AirMessageExporter), exporter, shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			exporter = AirOceanMessageExporter.New(preShipmentWrapper);
			AssertJXCMessageExporterReturned(typeof(AirMessageExporter), exporter, shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			exporter = AirOceanMessageExporter.New(preShipmentWrapper);
			AssertJXCMessageExporterReturned(typeof(OceanMessageExporter), exporter, shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			exporter = AirOceanMessageExporter.New(preShipmentWrapper);
			AssertJXCMessageExporterReturned(typeof(OceanMessageExporter), exporter, shipment);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNewMessageExporter_NullPreShipment()
		{
			PreShipmentWrapper preShipmentWrapper = null;
			AirOceanMessageExporter.New(preShipmentWrapper);
		}

		void AssertJXCMessageExporterReturned(Type expectedExporterType, JXCMessageExporter exporter)
		{
			AssertJXCMessageExporterReturned(expectedExporterType, exporter, exporter.HeaderData);
		}

		void AssertJXCMessageExporterReturned(Type expectedExporterType, JXCMessageExporter exporter, IBusiness expectedExportLoggerSource)
		{
			AssertEquals(expectedExporterType, exporter.GetType());
			AssertEquals(expectedExportLoggerSource, ((JXCExportLogger)exporter.NotificationSubscriber).ExportSource);
		}
	}
}
