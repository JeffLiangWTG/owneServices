using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	[TestedType(typeof(JXCDataImporterBizO))]
	class JXCDataImporterBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportFilePath()
		{
			AssertEquals("MaxLength should be 250", 250, DataImporter.ImportFilePathInfo.MaxLength);
			AssertEquals("Pre-condition", "", DataImporter.ImportFilePath);
			DataImporter.ImportFilePath = "test";
			AssertEquals("test", DataImporter.ImportFilePath);
			Assert("Not a valid path, should be calling Validate method in the setter", DataImporter.ImportFilePathInfo.HasErrors());
		}

		public void TestImportSummaryAndAppendImportSummary()
		{
			AssertEquals("Pre-condition", "", DataImporter.ImportSummary);
			DataImporter.AppendImportSummary("MEH1");
			AssertEquals("MEH1", DataImporter.ImportSummary);
			DataImporter.AppendImportSummary("MEH2");
			AssertEquals("MEH1\r\nMEH2", DataImporter.ImportSummary);
			DataImporter.AppendImportSummary("MEH3");
			AssertEquals("MEH1\r\nMEH2\r\nMEH3", DataImporter.ImportSummary);
		}

		public void TestRefreshBindingCalledWhenImportSummaryAppended()
		{
			DataImporter.ImportSummaryInfo.ValueChanged += new EventHandler(ImportSummaryInfo_ValueChanged);
			Assert("Pre-condition", !ImportSummaryInfoRefreshBindingCalled);
			DataImporter.AppendImportSummary("MEH");
			Assert("Refresh binding should be called on ImportSummaryInfo", ImportSummaryInfoRefreshBindingCalled);
		}

		#region Test Import
		public void TestImport_Failed()
		{
			string testFileName = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				using (File.Create(testFileName))
				{
				}

				NotificationBuffer notificationBuffer = new NotificationBuffer();
				DataImporter.ImportFilePath = testFileName;
				bool importResult = DataImporter.Import(notificationBuffer);
				Assert("Empty file should not be valid", !importResult);
				Assert("Should have error", notificationBuffer.HasErrors);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestImport_Success()
		{
			string testFileName = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			JASOrgHeader aUCOROrg = Factory.NewWithValidTestData<JASOrgHeader>();
			aUCOROrg.NettingCode = "AUCOR";
			aUCOROrg.OfficeCode = "AUSYD";
			aUCOROrg.OH_Code = "AUCORTST";
			Factory.Save();
			try
			{
				#region Test File
				using (StreamWriter writer = File.CreateText(testFileName))
				{
					writer.Write("HEAD3100;AUSYD;FRLYS;AUCOR;FRPAR;AUSYD\r\n" + "MAWB3100;n;1;LYS;129;37465514;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;69125 LYON ST EXUPERY, FRANCE;JAS (JET AIR SERV;FRPAR;JAS FORWARDING WORLDWIDE (PTY);UNIT 12, BUILDING C;BOTANY BAY INDUSTRIAL ESTATE;2-12 BEAUCHAMP RD, BANKSMEADOW NSW;JAS FORWARDING WO;AUCOR;MARTINAIR HOLLAND;1118 ZG SCHIPHOL AIRPORT;AMSTERDAM / THE NETHERLANDS;;MARTINAIR HOLLAND;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;69125 LYON ST EXU;20-4/7164/6915;FRPAR;LYON;;;;;;;;SYD;MP;;;;;EUR;PP;P;P;NVD;EUR;NCV;EUR;SYDNEY;MP9177;19/09/2004;;;;DR NO. 698252;ONE POUCH ATTACHED;FRED;;;;;;X;ZZ;0001;0011.00;K;000000556.32;000000556.32;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000009.45;000000000.00;000000029.15;000000000.00;000000594.92;000000000.00;0011.00;K;000000556.32;000000556.32;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000009.45;000000000.00;000000029.15;000000000.00;000000594.92;000000000.00;JAS FRANCE;17/09/2004;LYON;;;;;;;;;;FR;AU;BP 519 - BAT. SFS;JAS (JET AIR SERV;;;UNIT 12, BUILDING C;JAS FORWARDING WO;;;;\r\n" + "OTHR3100;P;AWA;AWA;000000009.45;;;;;;\r\n" + "OTHR3100;P;CHC;CHC;000000013.95;;;;;;\r\n" + "OTHR3100;P;SCC;SCC;000000010.80;;;;;;\r\n" + "OTHR3100;P;IRC;IRC;000000001.65;;;;;;\r\n" + "OTHR3100;P;MYC;MYC;000000002.75;;;;;;\r\n" + "FBDN3100;0001;;0011.00;K;Q;;;;0000019;00029.28;000000556.32; CONSOL SHIPMENT; CONSOL SHIPMENT;;;\r\n" + "HAWB3100;n;2004698252;FRLYS;FR;698252;LYS;129;37465514;MARTINAIR HOLLAND;1118 ZG SCHIPHOL AIRPORT;AMSTERDAM / THE NETHERLANDS;;;THALES ELECTRON DEVICES;ZI DE VONGY;BP 84;74202 THONON LES BAINS;THONON LES BAINS;74;74202;FR;;N;;F016705;;ENGINEERING DESIGN & SYSTEMS;KARTEL HOLDINGS PTY LTD;3 RACHAEL CLOSE SLOUGH ESTATE;SILVERWATER NSW 2141;SILVERWATER NSW 2;AU;STRALIA;AU;;N;;E044691;;;;;;;;N;;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;JAS (JET AIR SERV;20-4/7164/6915;FRPAR;LYON;;;;MAWB 129 3746 5514;;;;SYD;;MP;;;;;EUR;CC;C;C;NVD;EUR;NCV;EUR;SYDNEY;MP9177;19/09/2004;;;;HAWB 698252;INV. 450035802;FRED;;;;;;;;;0001;0011.00;K;000000080.00;000000000.00;000000080.00;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000101.80;000000000.00;000000000.00;000000000.00;000000181.80;JAS FRANCE;17/09/2004;LYON;;;;;;NoFhl;N;;;;;;;;;;;\r\n" + "REFR3100;HAWB 698252;S\r\n" + "FBDN3100;0001;;0011.00;K;Q;;;;0000019;ASAGREED;000000080.00; ELECTRON TUBE; ELECTRON TUBE;;;\r\n" + "OTHR3100;C;LTA;TAXE LTA / AWB FEE;000000023.40;;;;;;\r\n" + "OTHR3100;C;FOB;FRAIS DE FOB/FOB CHARGES;000000018.00;;;;;;\r\n" + "OTHR3100;C;EXP;DOU EXP/EXP CUST CLEARANC;000000042.00;;;;;;\r\n" + "OTHR3100;C;SCC;SAFETY COLLECT CHARGE;000000012.00;;;;;;\r\n" + "OTHR3100;C;IRC;RISK SURCHARGE;000000001.65;;;;;;\r\n" + "OTHR3100;C;MYC;FUEL SURCHARGE;000000004.75;;;;;;\r\n" + "TRLR3100\r\n");
				}

				#endregion
				DataImporter.ImportFilePath = testFileName;
				Assert("Should be successful", DataImporter.Import(new NotificationBuffer()));
				AssertImportedData();
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		void AssertImportedData()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolSchema.JK_MasterBillNum, "12937465514");
			JASForwardingConsol consol = Factory.LoadTop1<JASForwardingConsol>(filter);
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("FRPAR", consol.SendingForwarder.NettingCode);
			AssertEquals("FRLYS", consol.SendingForwarder.OfficeCode);
			AssertEquals("AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("AUSYD", consol.ReceivingForwarder.OfficeCode);
			AssertEquals("AUCORTST", consol.ReceivingForwarder.OH_Code);
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			Assert(consol.JK_OverrideWaybillDefaults);
			AssertEquals(5, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals(556.32m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals(1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("698252", shipment.JS_HouseBill);
			AssertEquals("HAWB 698252", shipment.JS_BookingReference);
			Assert(shipment.JS_OverrideWaybillDefaults);
			AssertEquals(80m, shipment.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals(6, shipment.AWBHeader.AWBOtherCharges.Count);
		}

		#endregion
		public void TestValidation()
		{
			AssertEquals(typeof(JXCDataImporterBizOValidation), DataImporter.Validation.GetType());
		}

		public void TestRunPreSaveValidation()
		{
			Assert("Pre-condition", !DataImporter.HasErrors);
			DataImporter.RunPreSaveValidation();
			Assert("RunPreSaveValidation should be calling Validation.ValidateAll", DataImporter.HasErrors);
			Assert("RunPreSaveValidation should be calling Validation.ValidateAll", DataImporter.ImportFilePathInfo.HasErrors());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JXCDataImporterBizO();
		}

		void ImportSummaryInfo_ValueChanged(object sender, EventArgs e)
		{
			ImportSummaryInfoRefreshBindingCalled = true;
		}

		JXCDataImporterBizO DataImporter
		{
			get
			{
				if (fDataImporter == null)
				{
					fDataImporter = new JXCDataImporterBizO();
				}

				return fDataImporter;
			}
		}

		JXCDataImporterBizO fDataImporter;
		bool ImportSummaryInfoRefreshBindingCalled;
		#endregion
	}
}
