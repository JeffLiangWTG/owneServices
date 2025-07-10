using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ManifestImporterTest : TestCaseWithFactory
	{
		public void TestGetImporter()
		{
			AssertEquals(typeof(RCLShippingManifestImporter), ManifestImporter.GetImporter(Factory, "kakaka.csv").GetType());
			AssertEquals(typeof(EuroPacificManifestImporter), ManifestImporter.GetImporter(Factory, "kakaka.txt").GetType());
		}

		public void TestErrorIfThereAreAlreadyLines()
		{
			var embeddedResourceRetriever = new EmbeddedResourceRetriever();
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport6.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var header = Factory.New<ExportCustomsManifestHeader>();
				header.Lines.AddNew();
				var success = importer.ImportDataToHeader(header, data, buffer);
				Assert("!Success", !success);
				AssertEquals("You can't import to this record because it already has one or more lines entered for it.\r\n", buffer.AsString);
			}
			embeddedResourceRetriever.Dispose();
		}

		protected void AssertHeadersTheSame(ExportCustomsManifestHeader header1, ExportCustomsManifestHeader header2)
		{
			AssertEquals("ED_ManifestType", header1.ED_ManifestType, header2.ED_ManifestType);
			AssertEquals("ED_DepartureDate", header1.ED_DepartureDate, header2.ED_DepartureDate);
			AssertEquals("ED_NoOfContainer", header1.ED_NoOfContainer, header2.ED_NoOfContainer);
			AssertEquals("ED_NoOfEmptyContainers", header1.ED_NoOfEmptyContainers, header2.ED_NoOfEmptyContainers);
			AssertEquals("ED_NoOfPacks", header1.ED_NoOfPacks, header2.ED_NoOfPacks);
			AssertEquals("ED_TransportMode", header1.ED_TransportMode, header2.ED_TransportMode);

			if (header1.ED_ManifestType == ManifestTypeList.Codes.ExportMainManifest)
			{
				AssertEquals("ED_AirWayBill", header1.ED_AirWayBill, header2.ED_AirWayBill);
				AssertEquals("ED_FlightNumber", header1.ED_FlightNumber, header2.ED_FlightNumber);
				AssertEquals("ED_RL_NKPortOfDeparture", header1.ED_RL_NKPortOfDeparture, header2.ED_RL_NKPortOfDeparture);
				AssertEquals("ED_RL_NKPortOfDestination", header1.ED_RL_NKPortOfDestination, header2.ED_RL_NKPortOfDestination);
				AssertEquals("ED_RN_NKCountryOfDestination", header1.ED_RN_NKCountryOfDestination, header2.ED_RN_NKCountryOfDestination);
				AssertEquals("ED_VoyageNumber", header1.ED_VoyageNumber, header2.ED_VoyageNumber);
			}

			AssertEquals("NumberOfLines", header1.Lines.Count, header2.Lines.Count);
			for (int i = 0; i < header1.Lines.Count; i++)
			{
				AssertLinesTheSame(i, header1.Lines[i], header2.Lines[i]);
			}
		}

		protected void AssertLinesTheSame(int lineNumber, ExportCustomsManifestLines line1, ExportCustomsManifestLines line2)
		{
			AssertEquals("Lines[" + lineNumber + "].EL_CAN", line1.EL_CAN, line2.EL_CAN);
			AssertEquals("Lines[" + lineNumber + "].EL_LineNo", line1.EL_LineNo, line2.EL_LineNo);
			AssertEquals("Lines[" + lineNumber + "].EL_NumberOfContainers", line1.EL_NumberOfContainers, line2.EL_NumberOfContainers);
			AssertEquals("Lines[" + lineNumber + "].EL_NumberOfPackages", line1.EL_NumberOfPackages, line2.EL_NumberOfPackages);
			AssertEquals("Lines[" + lineNumber + "].EL_TypeOfCAN", line1.EL_TypeOfCAN, line2.EL_TypeOfCAN);
			AssertEquals("Lines[" + lineNumber + "].EL_NumberOfContainers", line1.EL_NumberOfContainers, line2.EL_NumberOfContainers);
			if (line1.IsPersonalEffectsOrLowValue)
			{
				AssertEquals("Lines[" + lineNumber + "].EL_GoodsDescription", line1.EL_GoodsDescription, line2.EL_GoodsDescription);
				AssertEquals("Lines[" + lineNumber + "].EL_GoodsOwner", line1.EL_GoodsOwner, line2.EL_GoodsOwner);
				AssertEquals("Lines[" + lineNumber + "].EL_GoodsOwnerPartyID", line1.EL_GoodsOwnerPartyID, line2.EL_GoodsOwnerPartyID);
				AssertEquals("Lines[" + lineNumber + "].EL_RN_NKCountryOfDestination", line1.EL_RN_NKCountryOfDestination, line2.EL_RN_NKCountryOfDestination);
			}
		}

		protected void AddNewLine(ExportCustomsManifestHeader header, ZString cAN, int numberOfContainers, int numberOfPackages)
		{
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			if (cAN.Length == 4 && cAN.StartsWith("EX"))
			{
				line1.EL_TypeOfCAN = cAN;
			}
			else
			{
				line1.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
				line1.EL_CAN = cAN;
			}
			line1.EL_NumberOfContainers = (short)numberOfContainers;
			line1.EL_NumberOfPackages = (short)numberOfPackages;
		}

		protected override void SetUp()
		{
			base.SetUp();
			buffer = new NotificationBuffer(null);
		}

		protected NotificationBuffer buffer;
		protected ManifestImporter importer;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Business.ExportCustomsManifestHeader.DataImport.TestFiles." + fileName;
	}
}
