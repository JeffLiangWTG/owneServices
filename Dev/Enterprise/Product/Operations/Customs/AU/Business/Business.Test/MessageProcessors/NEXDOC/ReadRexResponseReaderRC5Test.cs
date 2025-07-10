using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.NEXDOC.RC5.ReadRex;
using NUnit.Framework;

namespace Enterprise.Customs.AU.MessageProcessors.NEXDOC.Testing
{
	[TestedType(typeof(ReadRexResponseReaderRC5))]
	sealed class ReadRexResponseReaderRC5Test : TestCaseWithFactory
	{
		public void TestPopulateStorageTemperature()
		{
			var quarantineHeader = Factory.New<QuarantineExDocHeader>();
			var exportDetails = new ExportDetailsType();
			var fishExportDetails = new FishExportDetails();
			var transportStorageMinimumTemperature = new TemperatureType();
			fishExportDetails.transportStorageMinimumTemperature = transportStorageMinimumTemperature;
			exportDetails.Item = fishExportDetails;

			var transportDetails = new TransportDetailsType();
			var storeTransportTemperature = new TemperatureType();
			transportDetails.storeTransportTemperature = storeTransportTemperature;
			exportDetails.transportDetails = transportDetails;

			CleanQuarantineExDocHeader(quarantineHeader, EXDOCCommodityCodes.Codes.Dairy);

			storeTransportTemperature.Value = 1.0M;
			storeTransportTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Reader.PopulateStorageTemperature(quarantineHeader, fishExportDetails, transportDetails);
			AssertTemperature("Dairy", quarantineHeader, ZDecimal.Zero, ZDecimal.Zero, 1.0M, EXDOCTemperatureUnitCodes.Codes.Celsius);

			CleanQuarantineExDocHeader(quarantineHeader, EXDOCCommodityCodes.Codes.Fish);
			storeTransportTemperature.Value = 33.8M;
			storeTransportTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			transportStorageMinimumTemperature.Value = 30.2M;
			transportStorageMinimumTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Reader.PopulateStorageTemperature(quarantineHeader, fishExportDetails, transportDetails);
			AssertTemperature("Fish, both Fahrenheit", quarantineHeader, 33.8M, 30.2M, ZDecimal.Zero, EXDOCTemperatureUnitCodes.Codes.Fahrenheit);

			CleanQuarantineExDocHeader(quarantineHeader, EXDOCCommodityCodes.Codes.Fish);
			storeTransportTemperature.Value = 33.8M;
			storeTransportTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			transportStorageMinimumTemperature.Value = -1.0M;
			transportStorageMinimumTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Reader.PopulateStorageTemperature(quarantineHeader, fishExportDetails, transportDetails);
			AssertTemperature("Fish, one Fahrenheit one Celsius", quarantineHeader, 1.0M, -1.0M, ZDecimal.Zero, EXDOCTemperatureUnitCodes.Codes.Celsius);

			CleanQuarantineExDocHeader(quarantineHeader, EXDOCCommodityCodes.Codes.Fish);
			storeTransportTemperature.Value = 1.0M;
			storeTransportTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Celsius;
			transportStorageMinimumTemperature.Value = -1.0M;
			transportStorageMinimumTemperature.unit = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Reader.PopulateStorageTemperature(quarantineHeader, fishExportDetails, transportDetails);
			AssertTemperature("Fish, both Celsius", quarantineHeader, 1.0M, -1.0M, ZDecimal.Zero, EXDOCTemperatureUnitCodes.Codes.Celsius);

			void CleanQuarantineExDocHeader(QuarantineExDocHeader quarantineHeader, string produceType)
			{
				quarantineHeader.QH_ProduceType = produceType;

				quarantineHeader.QH_MaximumTemperature = ZDecimal.Zero;
				quarantineHeader.QH_MinimumTemperature = ZDecimal.Zero;
				quarantineHeader.QH_AbsoluteTemperature = ZDecimal.Zero;
				quarantineHeader.QH_TemperatureUM = ZString.Empty;
			}

			void AssertTemperature(string message, QuarantineExDocHeader quarantineHeader, decimal expectedMaximumTemperature, decimal expectedMinimumTemperature, decimal expectedAbsoluteTemperature, string expectedUnit)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("MaximumTemperature", expectedMaximumTemperature, quarantineHeader.QH_MaximumTemperature);
					AssertEquals("MinimumTemperature", expectedMinimumTemperature, quarantineHeader.QH_MinimumTemperature);
					AssertEquals("AbsoluteTemperature", expectedAbsoluteTemperature, quarantineHeader.QH_AbsoluteTemperature);
					AssertEquals("TemperatureUM", expectedUnit, quarantineHeader.QH_TemperatureUM);
				});
			}
		}

		public void TestProcessFishProductLineDetails()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var quarantineLine = invoiceLine.QuarantineExDocLine;

			var fishProductLineDetails = new FishProductLineDetails();
			AssertNoExceptionThrown(() =>
			{
				Reader.ProcessFishProductLineDetails(invoice, quarantineLine, fishProductLineDetails);
			});

			var fishEstablishments  = new FishEstablishmentType();
			fishProductLineDetails.fishEstablishments = fishEstablishments;
			var harvestArea = new HarvestArea();
			fishEstablishments.harvestArea = new HarvestArea[] { harvestArea };
			AssertNoExceptionThrown("a empty harvestArea", () =>
			{
				Reader.ProcessFishProductLineDetails(invoice, quarantineLine, fishProductLineDetails);
			});
			AssertEquals("not add a quarantineLine.Processes for empty harvestArea", 0, quarantineLine.Processes.Count);

			var depuration = new Depuration();
			depuration.startDate = new DateTime(2024, 8, 1);
			depuration.establishmentNumber = "est001";
			harvestArea.depuration = depuration;
			var offshore = new Offshore();
			offshore.startDate = new DateTime(2024, 7, 1);
			offshore.endDate = new DateTime(2024, 7, 2);
			harvestArea.Item = offshore;
			Reader.ProcessFishProductLineDetails(invoice, quarantineLine, fishProductLineDetails);
			var process = quarantineLine.Processes[0];
			CombineAssertions("Offshore", () =>
			{
				AssertEquals("EE_Depuration", new ZDateTime(2024, 8, 1), process.EE_Depuration);
				AssertEquals("EE_AuthorisationEstablishmentID", "est001", process.EE_AuthorisationEstablishmentID);
				AssertEquals("EE_StartDate", new ZDateTime(2024, 7, 1), process.EE_StartDate);
				AssertEquals("EE_EndDate", new ZDateTime(2024, 7, 2), process.EE_EndDate);
			});

			var harvestAreaType = new HarvestAreaType();
			harvestAreaType.harvestAreaDate = new DateTime(2024, 7, 1);
			harvestAreaType.harvestAreaName = "harvestAreaName";
			harvestAreaType.leaseNumber = "leaseNumber";
			harvestArea.Item = harvestAreaType;
			Reader.ProcessFishProductLineDetails(invoice, quarantineLine, fishProductLineDetails);
			process = quarantineLine.Processes[1];
			CombineAssertions("Offshore", () =>
			{
				AssertEquals("EE_StartDate", new ZDateTime(2024, 7, 1), process.EE_StartDate);
				AssertEquals("EE_HarvestArea", "harvestAreaName", process.EE_HarvestArea);
				AssertEquals("EE_LeaseNumber", "leaseNumber", process.EE_LeaseNumber);
			});
		}

		ReadRexResponseReaderRC5 Reader => reader ??= new ReadRexResponseReaderRC5(Factory, new LoggingInformation());
		ReadRexResponseReaderRC5 reader;
	}
}
