using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	[TestedType(typeof(Level1DataImportForm))]
	public class Level1DataImportFormTest : ZFormBasherTest
	{
		[TestDate(2016, 7, 7, 16, 15, 19, 123)]
		public void TestImportAndSave()
		{
			Level1DataImport level1DataImportBizo = Level1DataImportBizo;
			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(level1DataImportBizo, TempDir.DirectoryName))
			{
				CusMAWB cusMAWB = Factory.New<CusMAWB>();
				cusMAWB.CM_MAWB = "23212345678";
				UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
				uPECusHAWB.CS_HAWB = "1Z4A14T96650435917";
				uPECusHAWB.CS_CM = cusMAWB.PK;
				JobRelatedWayBill wayBill = Factory.NewWithValidTestData<JobRelatedWayBill>(TestBusinessObjectKind.MinimumRequiredToSave);
				wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
				wayBill.EB_ParentID = uPECusHAWB.PK;
				wayBill.EB_WaybillShortNumber = "4A14T9J3YYD";

				Factory.Save();

				level1DataImportBizo.PecentageOfDuplicateHAWBs = 10;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.DoLoad();
				AssertEquals("Information All errors must be corrected before loading is possible.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(0, level1DataImportBizo.PecentageOfDuplicateHAWBs);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				level1DataImportBizo.ArrivalDate = ZDateTime.Now;

				ZDateTime arrivalDate = ZDateTime.Now;
				level1DataImportBizo.ArrivalDate = arrivalDate;
				form.DoLoad();
				AssertEquals(UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639Level1Sample.txt"), level1DataImportBizo.FileName);
				AssertNotNull(form.DataImporter);
				AssertEquals(1, form.MainTabControl.SelectedIndex);
				AssertEquals(14, level1DataImportBizo.PecentageOfDuplicateHAWBs);
				AssertEquals(7, form.DataImporter.TotalNoOfShipments);
				AssertEquals(true, form.LoadButton.Enabled);

				ZString expected =
					"Flight Details Summary\r\n" +
					"======================\r\n" +
					"Flight Number     : QF123\r\n" +
					"Port Of Loading   : SGSIN\r\n" +
					"Port Of Discharge : AUSYD\r\n" +
					"Arrival Date      : " + arrivalDate.ToShortDateString() + "\r\n" +
					"Master Bill       : 08111111111\r\n" +
					"\r\n" +
					"File Summary\r\n" +
					"============\r\n" +
					"File Name                         : " + UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639Level1Sample.txt") + "\r\n" +
					"Total Number of Shipments         : 7\r\n" +
					"Total Number of Pieces Manifested : 12\r\n" +
					"Total Number of Child Packages    : 3\r\n" +
					"\r\n" +
					"Duplicate HAWB %                  : 14\r\n" +
					"\r\n" +
					"Duplicate HAWB's\r\n" +
					"================\r\n" +
					"{0}" +
					"\r\n" +
					"No of Pieces Manifested <> No of Child Packs\r\n" +
					"============================================\r\n" +
					"1Z3824AR6640806327: 0 childpack/s, 1 expected\r\n" +
					"1Z3824AR6641499935: 0 childpack/s, 2 expected\r\n" +
					"1Z3947806616452202: 2 childpack/s, 1 expected\r\n" +
					"\r\n" +
					"Port of Destination Not Matched % : 100\r\n" +
					"\r\n" +
					"Port of Destination <> Port of Discharge: 'AUSYD'\r\n" +
					"=================================================\r\n" +
					"1Z3824AR6640806327 PortCode: AU9639\r\n" +
					"1Z3824AR6641499935 PortCode: AU9639\r\n" +
					"1Z3947806616452202 PortCode: AU9639\r\n" +
					"1Z4AX5856644068241 PortCode: AU9639\r\n" +
					"1Z4A14T96650435917 PortCode: AU9639\r\n" +
					"4R15V3KYSM8 PortCode: AU9639\r\n" +
					"1Z40E3726627413340 PortCode: AU9639\r\n\r\n" +
					"Empty Shipments Not Imported\r\n" +
					"============================\r\n" +
					"W3188389763\r\n" +
					"Number of Empty Shipments = 1\r\n\r\n";

				expected = String.Format(expected, "1Z4A14T96650435917 Matched Shipment: House Bill[1Z4A14T96650435917]; Master Bill[23212345678]\r\n");

				AssertMultilineASCIIEquals("Load Summary Information for use on save", expected, level1DataImportBizo.LoadSummaryInformation);
				AssertEquals(true, form.SaveButton.Enabled);
				AssertEquals(true, form.CloseButton.Enabled);
				AssertEquals(100, form.progressBar.Value);
			}
		}

		public void TestSave()
		{
			Level1DataImport level1DataImportBizo = Level1DataImportBizo;
			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(level1DataImportBizo, TempDir.DirectoryName))
			{
				form.DoLoad();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.DoSave();
				AssertEquals("Information All errors must be corrected before saving is possible.", UnitTestUserNotification.Instance.LastMessage.ToString());
				level1DataImportBizo.ArrivalDate = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.DoSave();
				AssertEquals("Information Please load the file before you save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				form.DoLoad();

				int cusHAWBRecordCountBefore = Factory.GetDatabaseCount(typeof(UPECusHAWB));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.DoSave();
				AssertEquals(true, form.SaveButton.Enabled);
				AssertEquals(true, form.CloseButton.Enabled);
				AssertEquals(cusHAWBRecordCountBefore, Factory.GetDatabaseCount(typeof(UPECusHAWB)));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.DoSave();
				AssertEquals(1, form.MainTabControl.SelectedIndex);
				AssertEquals(true, form.LoadButton.Enabled);
				AssertEquals(true, form.SaveButton.Enabled);
				AssertEquals(true, form.CloseButton.Enabled);

				AddRefLocoMapForSyd();
				form.DoSave();
				AssertEquals(1, form.MainTabControl.SelectedIndex);
				AssertEquals(false, form.LoadButton.Enabled);
				AssertEquals(false, form.SaveButton.Enabled);
				AssertEquals(true, form.CloseButton.Enabled);

				AssertEquals(cusHAWBRecordCountBefore + 7, Factory.GetDatabaseCount(typeof(UPECusHAWB)));
			}
		}
		public void TestSave_DetectLargeTotalPiecesManifested()
		{
			Level1DataImport level1DataImportBizo = Level1DataImportBizo;
			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(level1DataImportBizo, TempDir.DirectoryName))
			{
				form.Level1FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "InvalidPiecesManifested.txt");
				level1DataImportBizo.ArrivalDate = ZDateTime.Now;
				level1DataImportBizo.UnmatchedFilenameNote = "TEST";
				form.DoLoad();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.DoSave();
				AssertEquals("Information The level 1 file is invalid, total pieces manifested should be < 32767. \r\n", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		void AddRefLocoMapForSyd()
		{
			RefLocoMap sYDLocoMap = Factory.New<RefLocoMap>();
			sYDLocoMap.RY_LocalPortCode = "9639";
			sYDLocoMap.RY_RL_NKLocoPort = "AUSYD";
			sYDLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "AU").PK;
			sYDLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
		}

		public void TestSetControlState()
		{
			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(Level1DataImportBizo, TempDir.DirectoryName))
			{
				form.progressBar.Value = 39;

				form.SetControlState(false);
				AssertEquals(false, form.LoadButton.Enabled);
				AssertEquals(false, form.SaveButton.Enabled);
				AssertEquals(false, form.CloseButton.Enabled);
				AssertEquals(0, form.progressBar.Value);

				form.progressBar.Value = 39;

				form.SetControlState(true);
				AssertEquals(true, form.LoadButton.Enabled);
				AssertEquals(true, form.SaveButton.Enabled);
				AssertEquals(true, form.CloseButton.Enabled);
				AssertEquals(39, form.progressBar.Value);
			}
		}

		public void TestControlVisibility()
		{
			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(Level1DataImportBizo, TempDir.DirectoryName))
			{
				form.Show();
				Assert(form.detailControl.Visible);
				Assert(form.reasonsControl.Visible);
				Assert(!form.detailForManifestControl.Visible);
				Assert(!form.reasonsForManifestControl.Visible);
			}

			using (Level1DataImportForm_ForTest form = new Level1DataImportForm_ForTest(Level1DataImportBizoForManifest, TempDir.DirectoryName))
			{
				form.Show();
				Assert(!form.detailControl.Visible);
				Assert(!form.reasonsControl.Visible);
				Assert(form.detailForManifestControl.Visible);
				Assert(form.reasonsForManifestControl.Visible);
			}
		}

		#region Setup
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		Level1DataImport Level1DataImportBizo
		{
			get
			{
				Level1DataImport result = new Level1DataImport(Factory);
				result.FlightNumber = "QF123";
				result.MasterBill = "08111111111";
				result.PortOfDischarge = "AUSYD";
				result.PortOfLoading = "SGSIN";
				result.FlightNotInScheduleNote = "Notes";
				return result;
			}
		}

		Level1DataImport Level1DataImportBizoForManifest
		{
			get
			{
				Level1DataImport result = new Level1DataImport(Factory, isImportToManifest: true);
				result.FlightNumber = "QF123";
				result.MasterBill = "08111111111";
				result.PortOfDischarge = "AUSYD";
				result.PortOfLoading = "SGSIN";
				result.FlightNotInScheduleNote = "Notes";
				return result;
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new Level1DataImportForm(new Level1DataImport(new BusinessObjectFactory()));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "SummaryTextBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		class Level1DataImportForm_ForTest : Level1DataImportFormTestClass
		{
			public Level1DataImportForm_ForTest(Level1DataImport businessEntity, string testFilePath)
				: base(businessEntity)
			{
				this.testFilePath = testFilePath;
			}

			public override string Level1FileName
			{
				get
				{
					return level1FileName ?? (level1FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, testFilePath, "AU9639Level1Sample.txt"));
				}
				set
				{
					level1FileName = value;
				}
			}
			string level1FileName;
			readonly string testFilePath;
		}

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}

		#endregion
	}
}

