using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public class JXCImporterFormTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", "Courier New", ImportForm.SummaryTextBox.Font.FontFamily.Name);
			AssertEquals("Should be assigned in the constructor", 8f, ImportForm.SummaryTextBox.Font.Size);
		}

		public void TestFormHeading()
		{
			AssertEquals("Import JXC File", ImportForm.FormHeading);
		}

		public void TestBusinessEntity()
		{
			AssertEquals(DataImporter, ImportForm.BusinessEntity);
		}

		public void TestBrowseButtonClick()
		{
			ImportForm.Show();
			AssertEquals("Pre-condition", "", DataImporter.ImportFilePath);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = "test.txt";
			ImportForm.BrowseButton.PerformClick();
			AssertEquals("Should not assign import file path if cancelled", "", DataImporter.ImportFilePath);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = "test.txt";
			ImportForm.BrowseButton.PerformClick();
			AssertEquals("Should be assigned from the OpenFileDialog.FileName", "test.txt", DataImporter.ImportFilePath);
		}

		public void TestCloseButtonClick()
		{
			ImportForm.Show();
			Assert("Pre-condition", ImportForm.Visible);
			ImportForm.CloseButton.PerformClick();
			Assert("Should be closed", !ImportForm.Visible);
		}

		public void TestImportButtonClick_DataImporterHasErrors()
		{
			ImportForm.Show();
			ImportForm.ImportButton.PerformClick();
			Assert("Should show an error dialog", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Should show an error dialog", "There are errors that need to be fixed before JXC file can be imported", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportButtonClick_ImportFailed()
		{
			string testFileName = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				using (File.Create(testFileName))
				{
				}

				ImportForm.Show();
				DataImporter.ImportFilePath = testFileName;
				ImportForm.ImportButton.PerformClick();
				Assert("Should show an error dialog", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Should show an error dialog", "An error has occurred during import. Review the import log for details.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestImportButtonClick_ImportSuccess()
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
				ImportForm.Show();
				DataImporter.ImportFilePath = testFileName;
				ImportForm.ImportButton.PerformClick();
				Assert("Should show an information dialog", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Should show an information dialog", "JXC file '" + testFileName + "' has been sucessfully imported to the system.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestNotify()
		{
			INotifications notificationSubscriber = ImportForm;
			notificationSubscriber.Notify(new InfoNotification("test"));
			AssertEquals("test", DataImporter.ImportSummary);
			notificationSubscriber.Notify(new InfoNotification("another test"));
			AssertEquals("test\r\nanother test", DataImporter.ImportSummary);
		}

		#region Implementation
		protected override void SetUp()
		{
			DataImporter = new JXCDataImporterBizO();
			ImportForm = new JXCImporterForm(DataImporter);
			base.SetUp();
		}

		protected override void TearDown()
		{
			ImportForm.Dispose();
			base.TearDown();
		}

		JXCDataImporterBizO DataImporter;
		JXCImporterForm ImportForm;
		#endregion
	}
}
