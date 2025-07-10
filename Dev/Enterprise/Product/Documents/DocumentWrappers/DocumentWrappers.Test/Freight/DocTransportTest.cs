using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocTransport))]
	sealed class DocTransportTest : DocumentWrapperTestCase
	{
		#region TestStaticNewMethods

		public void TestStaticNewMethods_Transport()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];

			AssertNull(DocTransport.New(consol, null, Factory));
			AssertEquals(typeof(TransportSource), DocTransport.New(consol, transport, Factory).WrappedObject.GetType());
		}

		public void TestStaticNewMethods_Sailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			AssertNull(DocTransport.New((JobSailing)null, Factory));
			AssertEquals(typeof(SailingSource), DocTransport.New(sailing, Factory).WrappedObject.GetType());
		}

		public void TestStaticNewMethods_Declaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			AssertNull(DocTransport.New((BaseJobDeclaration)null, Factory));
			AssertEquals(typeof(DeclarationSource), DocTransport.New(declaration, Factory).WrappedObject.GetType());
		}

		public void TestStaticNewMethods_Order()
		{
			Order order = Factory.New<Order>();

			foreach (OrderSource.Leg leg in Enum.GetValues(typeof(OrderSource.Leg)))
			{
				AssertNull(DocTransport.New(null, leg, Factory));

				DocTransport wrapper = DocTransport.New(order, leg, Factory);
				AssertEquals(typeof(OrderSource), wrapper.WrappedObject.GetType());
				AssertEquals(leg, (OrderSource.Leg)(int)wrapper.LegOrder);
			}
		}

		#endregion

		#region TestTransportHeading

		public void TestTransportHeading()
		{
			DetailsMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			AssertEquals("Transport heading", ZString.Empty, TransportWrapper.TransportHeading);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			AssertEquals("Transport heading", "FLIGHT & DATE", TransportWrapper.TransportHeading);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			AssertEquals("Transport heading", "VESSEL / VOYAGE / IMO(Lloyds)", TransportWrapper.TransportHeading);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			AssertEquals("Transport heading", "JOURNEY NAME / JOURNEY NUMBER", TransportWrapper.TransportHeading);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			AssertEquals("Transport heading", "JOURNEY NAME / JOURNEY NUMBER", TransportWrapper.TransportHeading);
		}

		#endregion

		#region TestConsolTransport

		public void TestTransportInfo()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			DetailsMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			AssertEquals("Transport", ZString.Empty, TransportWrapper.ConsolTransportInfo);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"VOY123");
			DetailsMock.Setup(m => m.Vessel).Returns(vessel.RV_Code);
			DetailsMock.Verify(m => m.Vessel, Times.AtMost(3));
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / " + vessel.RV_LloydsNumber, TransportWrapper.ConsolTransportInfo);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"JNUM");
			DetailsMock.Setup(m => m.Vessel).Returns((ZString)"JNAME");
			AssertEquals("Transport", "JNAME / JNUM", TransportWrapper.ConsolTransportInfo);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"TruckRego");
			DetailsMock.Setup(m => m.Vessel).Returns((ZString)"Irrelivent");
			AssertEquals("Transport", "Irrelivent / TruckRego", TransportWrapper.ConsolTransportInfo);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"FL123");
			DetailsMock.Setup(m => m.ETD).Returns(ZDateTime.Empty);
			DetailsMock.Setup(m => m.Discharge).Returns(port.RL_Code);
			AssertEquals("Transport", "FL123 / " + port.RL_Code + " /    ", TransportWrapper.ConsolTransportInfo);
		}

		#endregion

		#region TestLCLReceivalCommences

		public void TestLCLReceivalCommences()
		{
			DetailsMock.Setup(m => m.LCLReceivalCommences).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.LCLReceivalCommences);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.LCLReceivalCommences).Returns(now);
			AssertEquals(now, TransportWrapper.LCLReceivalCommences);
		}

		#endregion

		#region TestLCLCutOff

		public void TestLCLCutOff()
		{
			DetailsMock.Setup(m => m.LCLCutOff).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.LCLCutOff);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.LCLCutOff).Returns(now);
			AssertEquals(now, TransportWrapper.LCLCutOff);
		}

		#endregion

		#region TestLCLAvailabilityDate

		public void TestLCLAvailabilityDate()
		{
			DetailsMock.Setup(m => m.LCLAvailabilityDate).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.LCLAvailabilityDate);

			ZDateTime now = ZDateTime.Empty;
			DetailsMock.Setup(m => m.LCLAvailabilityDate).Returns(now);
			AssertEquals(now, TransportWrapper.LCLAvailabilityDate);
		}

		#endregion

		#region TestLCLStorageDate

		public void TestLCLStorageDate()
		{
			DetailsMock.Setup(m => m.LCLStorageDate).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.LCLStorageDate);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.LCLStorageDate).Returns(now);
			AssertEquals(now, TransportWrapper.LCLStorageDate);
		}

		#endregion

		#region TestFCLReceivalCommences

		public void TestFCLReceivalCommences()
		{
			DetailsMock.Setup(m => m.FCLReceivalCommences).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.FCLReceivalCommences);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.FCLReceivalCommences).Returns(now);
			AssertEquals(now, TransportWrapper.FCLReceivalCommences);
		}

		#endregion

		#region TestFCLCutOff

		public void TestFCLCutOff()
		{
			DetailsMock.Setup(m => m.FCLCutOff).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.FCLCutOff);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.FCLCutOff).Returns(now);
			AssertEquals(now, TransportWrapper.FCLCutOff);
		}

		#endregion

		#region TestAvailabilityDate

		public void TestAvailabilityDate()
		{
			DetailsMock.Setup(m => m.FCLAvailabilityDate).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.AvailabilityDate);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.FCLAvailabilityDate).Returns(now);
			AssertEquals(now, TransportWrapper.AvailabilityDate);
		}

		#endregion

		#region TestStorageDate

		public void TestStorageDate()
		{
			DetailsMock.Setup(m => m.FCLStorageDate).Returns(ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, TransportWrapper.StorageDate);

			ZDateTime now = ZDateTime.Now;
			DetailsMock.Setup(m => m.FCLStorageDate).Returns(now);
			AssertEquals(now, TransportWrapper.StorageDate);
		}

		#endregion

		#region TestLegOrder

		public void TestLegOrder()
		{
			DetailsMock.Setup(m => m.LegOrder).Returns((ZByte)3);
			AssertEquals("LegOrder set to 3", (byte)3, TransportWrapper.LegOrder);

			DetailsMock.Setup(m => m.LegOrder).Returns((ZByte)5);
			AssertEquals("LegOrder changed to 5", (byte)5, TransportWrapper.LegOrder);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			ZDateTime now = ZDateTime.Now;

			DetailsMock.Setup(m => m.ETD).Returns(ZDateTime.Empty);
			AssertEquals("ETD", ZDateTime.Empty, TransportWrapper.ETD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETD", now, TransportWrapper.ETD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETD", now, TransportWrapper.ETD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETD", now, TransportWrapper.ETD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETD", now, TransportWrapper.ETD);
		}

		#endregion

		#region TestETDString

		public void TestETDString()
		{
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.ETD });

			ZDateTime now = ZDateTime.Now;
			ZString nowShort = now.ToShortDateString();
			ZString nowLong = now.ToLongTimeString();

			DetailsMock.Setup(m => m.SuppressingBizO).Returns(new SuppressionTest.DummySuppressionBizO());

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETD).Returns(ZDateTime.Empty);
			AssertEquals("ETDString", "", TransportWrapper.ETDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETDString only shows date", nowShort, TransportWrapper.ETDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETDString shows date and time", nowLong, TransportWrapper.ETDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETDString shows date and time", nowLong, TransportWrapper.ETDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			AssertEquals("ETDString shows date and time", nowLong, TransportWrapper.ETDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETD).Returns(now);
			DetailsMock.Setup(m => m.SuppressingBizO).Returns(new SuppressionTest.DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export });
			AssertEquals("ETDString suppressed", "", TransportWrapper.ETDString);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			ZDateTime now = ZDateTime.Now;

			DetailsMock.Setup(m => m.ETA).Returns(ZDateTime.Empty);
			AssertEquals("ETA", ZDateTime.Empty, TransportWrapper.ETA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETA", now, TransportWrapper.ETA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETA", now, TransportWrapper.ETA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETA", now, TransportWrapper.ETA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETA", now, TransportWrapper.ETA);
		}

		#endregion

		#region TestETAString

		public void TestETAString()
		{
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.ETA });

			ZDateTime now = ZDateTime.Now;
			ZString nowShort = now.ToShortDateString();
			ZString nowLong = now.ToLongTimeString();

			DetailsMock.Setup(m => m.SuppressingBizO).Returns(new SuppressionTest.DummySuppressionBizO());

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETA).Returns(ZDateTime.Empty);
			AssertEquals("ETAString", "", TransportWrapper.ETAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETAString only shows date", nowShort, TransportWrapper.ETAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETAString shows date and time", nowLong, TransportWrapper.ETAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETAString shows date and time", nowLong, TransportWrapper.ETAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			AssertEquals("ETAString shows date and time", nowLong, TransportWrapper.ETAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ETA).Returns(now);
			DetailsMock.Setup(m => m.SuppressingBizO).Returns(new SuppressionTest.DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export });
			AssertEquals("ETAString suppressed", "", TransportWrapper.ETAString);
		}

		#endregion

		#region TestATD

		public void TestATD()
		{
			ZDateTime now = ZDateTime.Now;

			DetailsMock.Setup(m => m.ATD).Returns(ZDateTime.Empty);
			AssertEquals("ATD", ZDateTime.Empty, TransportWrapper.ATD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATD", now, TransportWrapper.ATD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATD", now, TransportWrapper.ATD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATD", now, TransportWrapper.ATD);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATD", now, TransportWrapper.ATD);
		}

		#endregion

		#region TestATDString

		public void TestATDString()
		{
			ZDateTime now = ZDateTime.Now;
			ZString nowShort = now.ToShortDateString();
			ZString nowLong = now.ToLongTimeString();

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATD).Returns(ZDateTime.Empty);
			AssertEquals("ATDString", "", TransportWrapper.ATDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATDString only shows date", nowShort, TransportWrapper.ATDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATDString shows date and time", nowLong, TransportWrapper.ATDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATDString shows date and time", nowLong, TransportWrapper.ATDString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ATD).Returns(now);
			AssertEquals("ATDString shows date and time", nowLong, TransportWrapper.ATDString);
		}

		#endregion

		#region TestATA

		public void TestATA()
		{
			ZDateTime now = ZDateTime.Now;

			DetailsMock.Setup(m => m.ATA).Returns(ZDateTime.Empty);
			AssertEquals("ATA", ZDateTime.Empty, TransportWrapper.ATA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATA", now, TransportWrapper.ATA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATA", now, TransportWrapper.ATA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATA", now, TransportWrapper.ATA);

			DetailsMock.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATA", now, TransportWrapper.ATA);
		}

		#endregion

		#region TestATAString

		public void TestATAString()
		{
			ZDateTime now = ZDateTime.Now;
			ZString nowShort = now.ToShortDateString();
			ZString nowLong = now.ToLongTimeString();

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATA).Returns(ZDateTime.Empty);
			AssertEquals("ATAString", "", TransportWrapper.ATAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATAString only shows date", nowShort, TransportWrapper.ATAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATAString shows date and time", nowLong, TransportWrapper.ATAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATAString shows date and time", nowLong, TransportWrapper.ATAString);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			DetailsMock.Setup(m => m.ATA).Returns(now);
			AssertEquals("ATAString shows date and time", nowLong, TransportWrapper.ATAString);
		}

		#endregion

		#region TestPortOfDischarge

		public void TestPortOfDischarge()
		{
			DetailsMock.Setup(m => m.Discharge).Returns(ZString.Empty);
			AssertNull("PortOfDischarge null", TransportWrapper.PortOfDischarge);

			DetailsMock.Setup(m => m.Discharge).Returns(ZString.Empty);
			AssertEquals("PortOfDischargeCode is empty", "", TransportWrapper.PortOfDischargeCode);

			DetailsMock.Setup(m => m.Discharge).Returns((ZString)"USLAX");
			AssertEquals("PortOfDischarge is USLAX", "USLAX", TransportWrapper.PortOfDischarge.Code);

			DetailsMock.Setup(m => m.Discharge).Returns((ZString)"USLAX");
			AssertEquals("PortOfDischargeCode is USLAX", "USLAX", TransportWrapper.PortOfDischargeCode);
		}

		#endregion

		#region TestPortOfLoading

		public void TestPortOfLoading()
		{
			DetailsMock.Setup(m => m.Load).Returns(ZString.Empty);
			AssertNull("PortOfLoading null", TransportWrapper.PortOfLoading);

			DetailsMock.Setup(m => m.Load).Returns(ZString.Empty);
			AssertEquals("PortOfLoadingCode is empty", "", TransportWrapper.PortOfLoadingCode);

			DetailsMock.Setup(m => m.Load).Returns((ZString)"AUSYD");
			AssertEquals("PortOfLoading is AUSYD", "AUSYD", TransportWrapper.PortOfLoading.Code);

			DetailsMock.Setup(m => m.Load).Returns((ZString)"AUSYD");
			AssertEquals("PortOfLoadingCode is AUSYD", "AUSYD", TransportWrapper.PortOfLoadingCode);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			DetailsMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			AssertEquals("Transport mode empty", "", TransportWrapper.TransportMode);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			AssertEquals("Transport mode SEA", Core.Constants.TransportModes.Sea, TransportWrapper.TransportMode);
		}

		#endregion

		#region TestTransportModeDescription

		public void TestTransportModeDescription()
		{
			DetailsMock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			AssertEquals("Transport mode empty", "", TransportWrapper.TransportModeDescription);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Air);
			AssertEquals("Transport mode AIR", "Air", TransportWrapper.TransportModeDescription);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Rail);
			AssertEquals("Transport mode RAI", "Rail", TransportWrapper.TransportModeDescription);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Road);
			AssertEquals("Transport mode ROA", "Road", TransportWrapper.TransportModeDescription);

			DetailsMock.Setup(m => m.TransportMode).Returns((ZString)Core.Constants.TransportModes.Storage);
			AssertEquals("Transport mode STO", "Storage", TransportWrapper.TransportModeDescription);
		}

		#endregion

		#region TestTransportModeDescriptionMaxLength

		public void TestTransportModeDescriptionMaxLength()
		{
			foreach (System.Reflection.FieldInfo info in typeof(Core.Constants.TransportModes).GetFields())
			{
				if (info.FieldType == typeof(string))
				{
					DetailsMock.Setup(m => m.TransportMode).Returns(new ZString(info.GetValue(null)));
					ZString description = TransportWrapper.TransportModeDescription;
					string message = string.Format("\"{0}\" is to long. The description should be no more than 8 characters", description);
					Assert(message, description.Length <= 8);
				}
			}
		}

		#endregion

		#region TestTransportType

		public void TestTransportType()
		{
			DetailsMock.Setup(m => m.TransportType).Returns(ZString.Empty);
			AssertEquals("Transport type empty", "", TransportWrapper.TransportType);

			DetailsMock.Setup(m => m.TransportType).Returns((ZString)Core.Constants.TransportPlanningType.MainVessel);
			AssertEquals("Transport type MAI", "MAI", TransportWrapper.TransportType);
		}

		#endregion

		#region TestTransportTypeDescription

		public void TestTransportTypeDescription()
		{
			DetailsMock.Setup(m => m.TransportTypeDescription).Returns(ZString.Empty);
			AssertEquals("Transport type description empty", "", TransportWrapper.TransportTypeDescription);

			DetailsMock.Setup(m => m.TransportTypeDescription).Returns((ZString)"Main Vessel");
			AssertEquals("Transport type description Main Vessel", "Main Vessel", TransportWrapper.TransportTypeDescription);
		}

		#endregion

		#region TestVessel

		public void TestVessel()
		{
			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			DetailsMock.Setup(m => m.Vessel).Returns(ZString.Empty);
			AssertNull("Vessel null", TransportWrapper.Vessel);

			DetailsMock.Setup(m => m.Vessel).Returns((ZString)"Invalid");
			AssertNull("Vessel is invalid", TransportWrapper.Vessel);

			DetailsMock.Setup(m => m.Vessel).Returns(vessel.RV_Code);
			AssertEquals("Vessel is " + vessel.RV_Code, vessel.RV_Code, TransportWrapper.Vessel.Code);
		}

		#endregion

		#region TestVesselName

		public void TestVesselName()
		{
			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			DetailsMock.Setup(m => m.Vessel).Returns(ZString.Empty);
			AssertEquals("VesselName is empty", "", TransportWrapper.VesselName);

			DetailsMock.Setup(m => m.Vessel).Returns((ZString)"Invalid");
			AssertEquals("VesselName is invalid", "Invalid", TransportWrapper.VesselName);

			DetailsMock.Setup(m => m.Vessel).Returns(vessel.RV_Code);
			AssertEquals("VesselName is " + vessel.RV_Code, vessel.RV_Code, TransportWrapper.VesselName);
		}

		#endregion

		#region TestVesselVoyageFlight

		public void TestVesselVoyageFlight()
		{
			DetailsMock.Setup(m => m.SuppressingBizO).Returns(new SuppressionTest.DummySuppressionBizO());
			DetailsMock.Setup(m => m.Vessel).Returns(ZString.Empty);
			DetailsMock.Setup(m => m.VoyageFlight).Returns(ZString.Empty);
			AssertEquals("VesselVoyageFlight", "", TransportWrapper.VesselVoyageFlight);
			DetailsMock.Verify(m => m.Vessel, Times.Exactly(2));

			DetailsMock.Reset();
			DetailsMock.Setup(m => m.Vessel).Returns("A Vessel");
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"1234");
			AssertEquals("VesselVoyageFlight", "A Vessel / 1234", TransportWrapper.VesselVoyageFlight);
			DetailsMock.Verify(m => m.Vessel, Times.Exactly(2));
			DetailsMock.Verify(m => m.VoyageFlight, Times.Exactly(2));

			DetailsMock.Reset();
			DetailsMock.Setup(m => m.Vessel).Returns(ZString.Empty);
			DetailsMock.Setup(m => m.VoyageFlight).Returns((ZString)"QF11");
			AssertEquals("VesselVoyageFlight", "QF11", TransportWrapper.VesselVoyageFlight);
			DetailsMock.Verify(m => m.Vessel, Times.Exactly(2));

			DetailsMock.Reset();
			DetailsMock.Setup(m => m.Vessel).Returns((ZString)"A Vessel");
			DetailsMock.Setup(m => m.VoyageFlight).Returns(ZString.Empty);
			AssertEquals("VesselVoyageFlight", "A Vessel", TransportWrapper.VesselVoyageFlight);
			DetailsMock.Verify(m => m.Vessel, Times.Exactly(2));
			DetailsMock.Verify(m => m.VoyageFlight, Times.Exactly(2));
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bob's Meat Mart";

			DetailsMock.Setup(m => m.Carrier).Returns(ZGuid.Empty);
			AssertNull("Carrier", TransportWrapper.Carrier);

			DetailsMock.Setup(m => m.Carrier).Returns(org.PK);
			AssertEquals("Carrier", org.OH_FullName, TransportWrapper.Carrier.Name);
		}

		#endregion

		#region Implementation

		#region DetailsMock

		Mock<ITransportDetails> DetailsMock
		{
			get
			{
				if (detailsMock == null)
				{
					detailsMock = new Mock<ITransportDetails>();
				}
				return detailsMock;
			}
		}

		Mock<ITransportDetails> detailsMock;

		#endregion

		#region DocTransportForTest

		public class DocTransportForTest : DocTransport
		{
			protected DocTransportForTest(ITransportDetails transport, BusinessObjectFactory factoryToWrap) : base(transport, factoryToWrap)
			{
			}

			public static DocTransport New(Mock<ITransportDetails> mock, BusinessObjectFactory factory)
			{
				return (mock == null) ? null : new DocTransport(mock.Object, factory);
			}
		}

		#endregion

		DocTransport TransportWrapper
		{
			get { return DocTransportForTest.New(DetailsMock, Factory); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			Order order = Factory.New<Order>();

			CommonConsol consol = Factory.New<CommonConsol>();
			Transport consolTransport = consol.Transports.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			Transport shipmentTransport = shipment.Transports.AddNew();

			return new DocumentWrapper[]
			{
				DocTransport.New(consol, consolTransport, Factory),
				DocTransport.New(shipment, shipmentTransport, Factory),
				DocTransport.New(sailing, Factory),
				DocTransport.New(declaration, Factory),
				DocTransport.New(order, OrderSource.Leg.SingleLeg, Factory),
				DocTransport.New(order, OrderSource.Leg.Departure, Factory),
				DocTransport.New(order, OrderSource.Leg.Intermediate, Factory),
				DocTransport.New(order, OrderSource.Leg.Arrival, Factory)
			};
		}

		#endregion
	}
}
