using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.ExportCustomsManifest.Testing
{
	sealed class UPEManifestImporterTest : TestCaseWithFactory
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			new UPEManifestImporter(Factory);
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestGetImporter()
		{
			AssertEquals(typeof(UPEManifestImporter), UPEManifestImporter.GetImporter(Factory, "lalala").GetType());
		}

		public void TestGetLineMapper()
		{
			Type lineType = Importer.GetLineMapper("AU9639IT348904091517625597913   DA18704GQHRF201000                                                                                                                                                                                                                                                                                                                                        ");
			AssertEquals(null, lineType);

			lineType = Importer.GetLineMapper("AU9639SE375004091517625597913   DA18704GKLHY5000001   UNTDIAGNOSTIC KITS FOR LAB TESTING                     ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			AssertEquals(typeof(InvoiceDetailsLine), lineType);

			lineType = Importer.GetLineMapper("AU9639SE375004091517625597913   DA18704GKLHY5100001   UNTDIAGNOSTIC KITS FOR LAB TESTING                     ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			AssertEquals(typeof(InvoiceDetailsLine), lineType);

			lineType = Importer.GetLineMapper("AU0000      04091517625597913               100000 AU9639040914      EK413       040914                                                                              EK413                                                                                                                                                                                                                ");
			AssertEquals(typeof(FlightDetailsLine), lineType);

			lineType = Importer.GetLineMapper("AU9639AT512904091417625597913   DA18704GMT3F200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			AssertEquals(typeof(ShipmentDetailsLine), lineType);

			lineType = Importer.GetLineMapper("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			AssertEquals(typeof(ShipperDetailsLine), lineType);
		}

		public void TestProcessHeaderLine()
		{
			Importer.ProcessLine("AU0000      04091517625597913               100000 AU9639040914      EK413       040914                                                                              EK413                                                                                                                                                                                                                ");
			AssertEquals(ManifestTypeList.Codes.ConsolidationExportSubManifest, Importer.GeneratedHeader.ED_ManifestType);
			AssertEquals(Core.Constants.TransportModes.Air, Importer.GeneratedHeader.ED_TransportMode);
			AssertEquals("17625597913", Importer.GeneratedHeader.ED_AirWayBill);
			AssertEquals("EK413", Importer.GeneratedHeader.ED_FlightNumber);
			AssertEquals(new ZDateTime(2004, 9, 14), Importer.GeneratedHeader.ED_DepartureDate);
		}

		public void TestProcessDetailsLinesWithCANLineFirst()
		{
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessOutstandingMappers();
			AssertEquals("DIAGNOSTIC KITS Testing 2", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsDescription);
			AssertEquals((short)0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfContainers);
			AssertEquals(1, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfPackages);
			AssertEquals("TECRA INTERNATIONAL", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsOwner);
		}

		public void TestProcessDetailsLinesWithCANLineLast()
		{
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF2020001ZAA00350498771908                 340    180    2    N                    XXXXXXXXXXXXXXXX                                  E                AU09639  S2AU9639T5.087B2011-06-08                              Y                                                                             AUDNNNNNBI                                  ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# EX2                                            311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessOutstandingMappers();
			AssertEquals("DIAGNOSTIC KITS Testing 2", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsDescription);
			AssertEquals((short)0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfContainers);
			AssertEquals(1, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfPackages);
			AssertEquals("TECRA INTERNATIONAL", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsOwner);
			AssertEquals("1ZAA00350498771908", Importer.GeneratedHeader.CurrentManifestLine.EL_AirWayBill);
		}

		public void TestProcessDetailsLinesWithDifferentShipmentNumbers()
		{
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GMT3F200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GKLHY5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessOutstandingMappers();
			AssertEquals("DIAGNOSTIC KITS Testing 2", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsDescription);
			AssertEquals((short)0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfContainers);
			AssertEquals(0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfPackages);
			AssertEquals("", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsOwner);
		}

		public void TestProcessDetailsLinesWithDuplicateCANLines()
		{
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessOutstandingMappers();
			AssertEquals("DIAGNOSTIC KITS Testing 2", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsDescription);
			AssertEquals((short)0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfContainers);
			AssertEquals(1, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfPackages);
			AssertEquals("TECRA INTERNATIONAL", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsOwner);
		}

		public void TestProcessDetailsLinesWithInvalidLines()
		{
			Importer.ProcessLine("AU9639IT348904091517625597913   DA1870");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF100000A18704GMT3F           ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ");
			Importer.ProcessLine("AU9639IT348904091517625597913   DA18704GQHRF30000017059639A18704    TECRA INTERNATIONAL                13 RODBOROUGH ROAD                                                    FRENCHS FOREST                                           2086     AU +61-2-89773000                                                                            SIN014891                          IAN CHISHOL");
			Importer.ProcessLine("AU9639AT512904091417625597913   DA18704GQHRF200000A18704GMT3F           N 1 30   KGSNAU          AUDNNNN N         EUR            9901305             EUR         EUR48400     EURN0NN   NSIVA 13SEP200430 KGS         AUDD1     NN NN  NN  N EUR           EUR    T1                      13SEP20041800           48400      P/PNPS            KGSNNB0929523126        EK413   N Y 1   N ");
			Importer.ProcessLine("AU9639SE375004091517625597913   DA18704GQHRF5000001   UNTDIAGNOSTIC KITS Testing 2                           ECN# 1M042571991LUC                                 311618    AUDSIN014890           AU                                   SE                                                                                                                                                 ");
			Importer.ProcessOutstandingMappers();
			AssertEquals("DIAGNOSTIC KITS Testing 2", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsDescription);
			AssertEquals((short)0, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfContainers);
			AssertEquals(1, Importer.GeneratedHeader.CurrentManifestLine.EL_NumberOfPackages);
			AssertEquals("TECRA INTERNATIONAL", Importer.GeneratedHeader.CurrentManifestLine.EL_GoodsOwner);
		}

		public void TestProcessDetailsLinesTwiceToGetCANFromTranshipmentNumberWithoutCreatingNewLine()
		{
			const string hawb = "1ZA178826647781798";

			var cusHAWB = Factory.NewWithValidTestData<Customs.Business.CusHAWB>();
			cusHAWB.CS_TranshipmentEntryNum = "ABC";
			cusHAWB.CS_HAWB = hawb;
			Factory.Save();

			cusHAWB = Factory.NewWithValidTestData<Customs.Business.CusHAWB>();
			cusHAWB.CS_TranshipmentEntryNum = "JAY";
			cusHAWB.CS_HAWB = hawb;
			Factory.Save();

			var lines = new System.Collections.Generic.List<string>();
			lines.Add("AU9639TW8879040831              DA17882HJWF8200000A17882HJWF8           N 1 40   KGSNAU          AUDNNNN N         AUD              08873             AUD         AUD1175      AUDN0NN   NSYANT30AUG200440 KGS         AUDD1    YNN NN  NN  N AUD           AUD    T1                      30AUG20041730           1175       P/PNVK       11   KGSNNB0929519748        QF083   N Y 1   N ");
			lines.Add("AU9639TW8879040831              DA17882HJWF8201000                                                                                                                                                                                                                                                                                                                                        ");
			lines.Add("AU9639TW8879040831              DA17882HJWF82020001ZA178826647781798                 40     40     1    N                                                                      W                AU09639  S4AU9639T0.243B          QF083   2004-08-30            N                                            01                               AUDNNNNNBI                                  ");
			lines.Add("AU9639TW8879040831              DA17882HJWF830000017059639A17882    KAS AUSTRALIA                      78 O'RIORDAN STREET                                                   ALEXANDRIA                                               2015     AU +61-2-9317-570                                                                                                               KAREN DAVID");
			lines.Add("AU9639TW8879040831              DA17882HJWF8400000                  FAR EASTERN APPAREL                SOPHIA                   330 SEC.1 SU CHUAN ROAD                                               TAIPEI                             PANCHIAO              10099    TW 0229551888                                                                                       000           ");
			lines.Add("AU9639TW8879040831              DA17882HJWF8401000                  FAR EASTERN APPAREL CO LTD                                  330 SEC.1 SU CHUAN ROAD                                               TAIPEI                             PANCHIAO              00000    TW                                                                                                                ");
			lines.Add("AU9639TW8879040831              DA17882HJWF85000003   EA QUILT SETs                                          ECN#                                                975       AUD                    AU                                   TW                                                                                                                                                 ");
			lines.Add("AU9639TW8879040831              DA17882HJWF85010004   EA PILLOW CASES                                                                                            200       AUD                    AU                                   TW                                                                                                                                                  ");

			foreach (string line in lines)
			{
				Importer.ProcessLine(line);
			}

			Importer.ProcessOutstandingMappers();

			AssertEquals("JAY", Importer.GeneratedHeader.CurrentManifestLine.EL_CAN);

			foreach (string line in lines)
			{
				Importer.ProcessLine(line);
			}

			Importer.ProcessOutstandingMappers();

			AssertEquals(1, Importer.GeneratedHeader.Lines.Count);
			AssertEquals("JAY", Importer.GeneratedHeader.CurrentManifestLine.EL_CAN);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileEndToEndAU9639T1_262()
		{
			TestImportFileEndToEnd("AU9639T1.262", 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileEndToEndAU9639T1_261()
		{
			TestImportFileEndToEnd("AU9639T1.261", 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithNoConsolLine()
		{
			TestImportFileEndToEnd("AU9639T0.243", 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithMoreThanOneConsolLine()
		{
			TestImportFileEndToEnd("AU9639TD.225", 2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportManifestFromNonCurrentCountry200000RecordEvenWithNo500000RecordProvided()
		{
			TestImportFileEndToEnd("200000RecordWithNo5000000.txt", 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSuccessIfThereAreAlreadyLines()
		{
			using (var data = new StreamReader(UPETestHelper.TestFiles.DataImport.ExportCustomsManifestFolder + "AU9639TD.225", Encoding.ASCII))
			{
				Assert("Precondition - No Errors", !TestHelper.Buffer.HasErrors);
				var header = Factory.New<ExportCustomsManifestHeader>();
				header.Lines.AddNew();
				Assert("Success", Importer.ImportDataToHeader(header, data, TestHelper.Buffer));
				Assert("You can import to this file whehn it already has one or more lines entered for it.\r\n", !TestHelper.Buffer.AsString.Contains("You can't import to this record because it already has one or more lines entered for it.\r\n"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
		}

		void AssertHeadersTheSame(ExportCustomsManifestHeader header1, ExportCustomsManifestHeader header2)
		{
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_ManifestType, header1.ED_ManifestType, header2.ED_ManifestType);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_TransportMode, header1.ED_TransportMode, header2.ED_TransportMode);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_DepartureDate, header1.ED_DepartureDate, header2.ED_DepartureDate);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_NoOfContainer, header1.ED_NoOfContainer, header2.ED_NoOfContainer);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_NoOfPacks, header1.ED_NoOfPacks, header2.ED_NoOfPacks);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_AirWayBill, header1.ED_AirWayBill, header2.ED_AirWayBill);
			AssertEquals(ExportCustomsManifestHeader.Schema.ED_FlightNumber, header1.ED_FlightNumber, header2.ED_FlightNumber);
			AssertEquals("NumberOfLines", header1.Lines.Count, header2.Lines.Count);
			for (int i = 0; i < header1.Lines.Count; i++)
			{
				AssertLinesTheSame(i, header1.Lines[i], header2.Lines[i]);
			}
		}

		void AssertLinesTheSame(int lineNumber, ExportCustomsManifestLines line1, ExportCustomsManifestLines line2)
		{
			AssertEquals("Lines[" + lineNumber + "].EL_CAN", line1.EL_CAN, line2.EL_CAN);
			AssertEquals("Lines[" + lineNumber + "].EL_LineNo", line1.EL_LineNo, line2.EL_LineNo);
			AssertEquals("Lines[" + lineNumber + "].EL_NumberOfContainers", line1.EL_NumberOfContainers, line2.EL_NumberOfContainers);
			AssertEquals("Lines[" + lineNumber + "].EL_NumberOfPackages", line1.EL_NumberOfPackages, line2.EL_NumberOfPackages);
			AssertEquals("Lines[" + lineNumber + "].EL_TypeOfCAN", line1.EL_TypeOfCAN, line2.EL_TypeOfCAN);

			if (line1.IsPersonalEffectsOrLowValue)
			{
				AssertEquals("Lines[" + lineNumber + "].EL_GoodsDescription", line1.EL_GoodsDescription, line2.EL_GoodsDescription);
				AssertEquals("Lines[" + lineNumber + "].EL_GoodsOwner", line1.EL_GoodsOwner, line2.EL_GoodsOwner);
				AssertEquals("Lines[" + lineNumber + "].EL_RN_NKCountryOfDestination", line1.EL_RN_NKCountryOfDestination, line2.EL_RN_NKCountryOfDestination);
			}
		}

		ExportCustomsManifestHeader GetExpectedHeader(string fileName)
		{
			ExportCustomsManifestHeader result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Air;

			if (fileName == "AU9639T1.261")
			{
				result.ED_DepartureDate = new ZDateTime(2004, 9, 15);
				result.ED_AirWayBill = "16010130129";
				result.ED_FlightNumber = "AUHK";
				result.ED_NoOfPacks = 14;
				AddNewLine(result, "EX2", 2, "VIDEO CONFERENCING EQUIP", "POLYCOM GLOBAL PTY LIMITE", "HK");
				AddNewLine(result, "1M042591534DDC", 1, "HEARING AIDS", "DENISE DALY", "HK");
				AddNewLine(result, "EX2", 1, "HEARING AID; MICROPHONE FOR HEARING AID; RECEIVERS FOR HEARING AIDS", "STARKEY LABORATORIES PTY", "TW");
				AddNewLine(result, "EX2", 1, "DOCUMENTS", "FOOTWORKS AUSTRALASIA", "HK");
				AddNewLine(result, "EX2", 1, "GARMENT LABELS", "W L GORE & ASSOCIATES (AU", "HK");
				AddNewLine(result, "", 1, "SAMPLE GARMENT #1172, 1129", "PHYSICO CLOTHING COMPANY", "CN");
				AddNewLine(result, "EX2", 1, "SILICONE BELT LUBRICATION FLUID", "QANTAS EXPRESS PARCEL SER", "MO");
				AddNewLine(result, "EX2", 1, "TOUCHSCREEN", "JAVELIN SYSTEMS ASIA PACI", "TW");
				AddNewLine(result, "EX2", 1, "MOTHERBOARD; MEMORY COMPONENT", "JAVELIN SYSTEMS ASIA PACI", "TW");
				AddNewLine(result, "EX2", 1, "SAMPLE OF PLACEMATS", "BAYSWISS PTY LTD", "CN");
				AddNewLine(result, "EX2", 1, "GARMENT SAMPLE; FABRIC SWATCH CUTTINGS", "MONTI AUSTRALIA PTY LTD", "CN");
				AddNewLine(result, "EX2", 1, "GARMENT SAMPLE; FABRIC SWATCH CUTTINGS", "MONTI AUSTRALIA PTY LTD", "CN");
				AddNewLine(result, "EX2", 1, "DOCS AND PALSTIC BAG CONTAINING A STONE", "MASTERFOODS", "CN");
				AddNewLine(result, "EX2", 1, "BARCODES", "BLADER UNNER", "CN");
			}
			else if (fileName == "AU9639T1.262")
			{
				result.ED_DepartureDate = new ZDateTime(2004, 9, 14);
				result.ED_AirWayBill = "81060065762";
				result.ED_FlightNumber = "AUSG";
				result.ED_NoOfPacks = 9;
				AddNewLine(result, "EX2", 2, "GAMES, MINIATURES AND BOOKS", "GAMES WORKSHOP OZ", "SG");
				AddNewLine(result, "EX2", 1, "STEEL SAMPLE", "QANTAS EXPRESS PARCEL SER", "KR");
				AddNewLine(result, "EX2", 1, "1X CD", "QANTAS EXPRESS PARCEL SER", "KR");
				AddNewLine(result, "EX2", 2, "TELSTRA PARTS / FIBRE OPTIC INSULATING PART", "CHANNELL AUSTRALIA", "MY");
				AddNewLine(result, "EX2", 1, "VIDEOS / ROB ALLEN BLUE WATER DREAMING SOUTH MOOZAMB", "WHOLESALE DIVING SUPPLIES", "QA");
				AddNewLine(result, "EX2", 1, "EXTREME BLUEWATER ACTION/ IMMERSION SPEARFISHING THE", "WHOLESALE DIVING SUPPLIES", "SA");
				AddNewLine(result, "1M042581205GVC", 1, "FUEL QUANTITY INDICATOR / INDICATOR, FUEL QUANTITY", "SMITHS AEROSPACE AUSTRALI", "SG");
			}
			else if (fileName == "AU9639T0.243")
			{
				result.ED_NoOfPacks = 1;
				AddNewLine(result, "EX2", 1, "QUILT SET; PILLOW CASES", "KAS AUSTRALIA", "TW");
			}
			else if (fileName == "AU9639TD.225")
			{
				result.ED_DepartureDate = new ZDateTime(2004, 8, 13);
				result.ED_AirWayBill = "08143395601";
				result.ED_FlightNumber = "QF011";
				result.ED_NoOfPacks = 1;
				AddNewLine(result, "EX2", 1, "QUILT SET; PILLOW CASES", "KAS AUSTRALIA", "TW");
			}
			else if (fileName == "200000RecordWithNo5000000.txt")
			{
				result.ED_DepartureDate = new ZDateTime(2012, 7, 11);
				result.ED_AirWayBill = "40674235361";
				result.ED_FlightNumber = "UPS0034";
				result.ED_NoOfPacks = 2;
				AddNewLine(result, "", 1, "DES CARD", "CITI", "NZ");
				AddNewLine(result, "", 1, "Documents Only", "SEC CORPORATE HQ", "");
			}

			return result;
		}

		void AddNewLine(ExportCustomsManifestHeader header, ZString cAN, int numberOfPackages, ZString description, ZString owner, ZString destinationCountry)
		{
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			if (cAN.StartsWith("EX"))
			{
				line1.EL_CAN = ZString.Empty;
				line1.EL_TypeOfCAN = CMRExportExemptionCodes.GetFromExit2Exemption(cAN);
			}
			else
			{
				line1.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
				line1.EL_CAN = cAN;
			}
			line1.EL_NumberOfContainers = (short)0;
			line1.EL_NumberOfPackages = (short)numberOfPackages;
			line1.EL_GoodsDescription = description;
			line1.EL_GoodsOwner = owner;
			line1.EL_RN_NKCountryOfDestination = destinationCountry;
		}

		void TestImportFileEndToEnd(string fileName, int expectedConsolNo)
		{
			using (var dataStream = new StreamReader(UPETestHelper.TestFiles.DataImport.ExportCustomsManifestFolder + fileName, Encoding.ASCII))
			{
				Assert("Precondition - No Errors", !TestHelper.Buffer.HasErrors);
				Assert("Success", Importer.ImportData(dataStream, fileName, TestHelper.Buffer, SourceInfo.EmptySourceInfo));
				Assert("Post condition - No Errors", !TestHelper.Buffer.HasErrors);

				var importedHeader = Importer.GeneratedHeader;
				var expectedHeader = GetExpectedHeader(fileName);
				AssertHeadersTheSame(expectedHeader, importedHeader);
				AssertNotNull(Importer.Consols);
				AssertEquals(expectedConsolNo, Importer.Consols.Count);

				for (var i = 0; i < expectedConsolNo; i++)
				{
					var consol = Importer.Consols[i];
					Assert("Ensure that consol factory is different to the ManifestHeader factory", consol.Factory != Importer.GeneratedHeader.Factory);
					var factoryTheSame = false;
					for (var j = i; j > 0; j--)
					{
						factoryTheSame |= consol.Factory == Importer.Consols[j - 1].Factory;
					}
					Assert("Ensure that the consol factories are different", !factoryTheSame);
				}
			}
		}

		void Importer_FreightConsolCreated(object sender, EventArgs e)
		{
			AssertEquals(typeof(ForwardingConsol), sender.GetType());
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;

		UPEManifestImporterForTest Importer
		{
			get { return importer ?? (importer = new UPEManifestImporterForTest(Factory)); }
		}
		UPEManifestImporterForTest importer;

		sealed class UPEManifestImporterForTest : UPEManifestImporter
		{
			public UPEManifestImporterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			internal new void ProcessLine(string line) => base.ProcessLine(line);

			internal new Type GetLineMapper(string line) => base.GetLineMapper(line);
		}
	}
}
