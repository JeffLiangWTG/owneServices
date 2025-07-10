using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CalcExportManifestHeader))]
	sealed class CalcExportManifestHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLines()
		{
			var header = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);
			AssertType("LinesType", typeof(CalcExportManifestLineCollection), header.Lines);
		}

		public void TestApplyToFactory()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "TestVessel";
			Factory.Save();

			ZString voyage = "111";

			ZString departure = "AUSYD";
			var departureDate = ZDateTime.Now;
			ZString dischargeCountry = "US";

			var calcHeader = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest)
			{
				Departure = departure,
				DepartureDate = departureDate,
				DischargeCountry = dischargeCountry,
				VesselName = vessel.RV_Code,
				VoyageNumber = voyage,
			};

			AddLine(calcHeader, "CAN", "222", "", 20, 20, 20);
			AddLine(calcHeader, "CAN", "333", "", 30, 30, 30);

			var newFactory = new BusinessObjectFactory();

			var header = calcHeader.ApplyTo(newFactory);
			var headers = Factory.Load<ExportCustomsManifestHeader>(new ZQuery());
			var newHeaders = newFactory.Load<ExportCustomsManifestHeader>(new ZQuery());

			AssertEquals("Old Factory should be clear", 0, headers.Length);
			AssertEquals("New Factory should has Header", header, newHeaders[0]);
			AssertEquals("Should be 2 lines", 2, header.Lines.Count);
			AssertEquals("Should be Empty Containers", (ZShort)50, header.ED_NoOfEmptyContainers);
			AssertEquals("Should be CAN Number", "222", header.Lines[0].EL_CAN);
			AssertEquals("Should be Containers", (ZShort)20, header.Lines[0].EL_NumberOfContainers);
			AssertEquals("Should be Packages", 20, header.Lines[0].EL_NumberOfPackages);
			AssertEquals("Should be CAN Number", "333", header.Lines[1].EL_CAN);
			AssertEquals("Should be Containers", (ZShort)30, header.Lines[1].EL_NumberOfContainers);
			AssertEquals("Should be Packages", 30, header.Lines[1].EL_NumberOfPackages);
		}

		public void TestApplyToManifest()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "TestVessel";
			vessel.RV_LloydsNumber = "5789401";
			ZString voyage = "111";

			ZString departure = "AUSYD";
			var departureDate = ZDateTime.Now;
			ZString dischargeCountry = "US";

			var header = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_DepartureDate = departureDate;
			header.ED_RL_NKPortOfDeparture = departure;
			header.ED_RN_NKCountryOfDestination = dischargeCountry;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_VesselName = vessel.RV_Code;
			header.ED_LloydsIMO = vessel.RV_LloydsNumber;
			header.ED_VoyageNumber = voyage;

			AddLine(header, "CAN", "000", "", 0, 0, 0);
			AddLine(header, "CAN", "111", "", 1, 1, 1);
			AddLine(header, "CAN", "222", "", 2, 2, 2);
			AddLine(header, "CAN", "222", "", 3, 3, 3);

			var calcHeader = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest)
			{
				Departure = departure,
				DepartureDate = departureDate,
				DischargeCountry = dischargeCountry,
				VesselName = vessel.RV_Code,
				VoyageNumber = voyage,
			};

			AddLine(calcHeader, "CAN", "222", "", 20, 20, 20);
			AddLine(calcHeader, "CAN", "333", "", 30, 30, 30);

			AssertEquals("precondition", 4, header.Lines.Count);
			AssertEquals("precondition", (ZShort)6, header.ED_NoOfEmptyContainers);

			AssertEquals("precondition", "000", header.Lines[0].EL_CAN);
			AssertEquals("precondition", (ZShort)0, header.Lines[0].EL_NumberOfContainers);
			AssertEquals("precondition", 0, header.Lines[0].EL_NumberOfPackages);

			AssertEquals("precondition", "111", header.Lines[1].EL_CAN);
			AssertEquals("precondition", (ZShort)1, header.Lines[1].EL_NumberOfContainers);
			AssertEquals("precondition", 1, header.Lines[1].EL_NumberOfPackages);

			AssertEquals("precondition", "222", header.Lines[2].EL_CAN);
			AssertEquals("precondition", (ZShort)2, header.Lines[2].EL_NumberOfContainers);
			AssertEquals("precondition", 2, header.Lines[2].EL_NumberOfPackages);

			AssertEquals("precondition", "222", header.Lines[3].EL_CAN);
			AssertEquals("precondition", (ZShort)3, header.Lines[3].EL_NumberOfContainers);
			AssertEquals("precondition", 3, header.Lines[3].EL_NumberOfPackages);

			calcHeader.ApplyTo(header);

			AssertEquals("Should be 2 lines", 2, header.Lines.Count);
			AssertEquals("Should be Empty Containers", (ZShort)50, header.ED_NoOfEmptyContainers);

			AssertEquals("Should be CAN Number", "222", header.Lines[0].EL_CAN);
			AssertEquals("Should be Containers", (ZShort)20, header.Lines[0].EL_NumberOfContainers);
			AssertEquals("Should be Packages", 20, header.Lines[0].EL_NumberOfPackages);

			AssertEquals("Should be CAN Number", "333", header.Lines[1].EL_CAN);
			AssertEquals("Should be Containers", (ZShort)30, header.Lines[1].EL_NumberOfContainers);
			AssertEquals("Should be Packages", 30, header.Lines[1].EL_NumberOfPackages);
		}

		public void TestManifestType()
		{
			ZString manifestType = "AAA";
			AssertExceptionThrown("Should be Exception when incorrect type", typeof(ArgumentException), "Invalid Manifest Type: AAA", () => new CalcExportManifestHeader(new BusinessObjectFactory(), manifestType));

			manifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertNoExceptionThrown("Should be NO Exceptions", () => new CalcExportManifestHeader(new BusinessObjectFactory(), manifestType));

			manifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertNoExceptionThrown("Should be NO Exceptions", () => new CalcExportManifestHeader(new BusinessObjectFactory(), manifestType));
		}

		protected override BusinessObject GetNewBusinessObject() => new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);

		CalcExportManifestLine AddLine(CalcExportManifestHeader header, ZString entryType, ZString entryNo, ZString goodsDescription, ZInt containerCount, ZInt emptyContainerCount, ZInt packageCount)
		{
			var result = header.Lines.AddNew();
			result.EntryNumber = entryType;
			result.EntryNumber = entryNo;
			result.GoodsDescription = goodsDescription;
			result.ContainerCount = containerCount;
			result.PackageCount = packageCount;
			header.EmptyContainerCount += emptyContainerCount;

			return result;
		}

		ExportCustomsManifestLines AddLine(ExportCustomsManifestHeader header, ZString entryType, ZString entryNo, ZString goodsDescription, ZInt containerCount, ZInt emptyContainerCount, ZInt packageCount)
		{
			var result = header.Lines.AddNew();
			result.EL_TypeOfCAN = entryType;
			result.EL_CAN = entryNo;
			result.EL_GoodsDescription = goodsDescription;
			result.EL_NumberOfContainers = (ZShort)containerCount;
			result.EL_NumberOfPackages = packageCount;
			header.ED_NoOfEmptyContainers += (ZShort)emptyContainerCount;

			return result;
		}
	}
}
