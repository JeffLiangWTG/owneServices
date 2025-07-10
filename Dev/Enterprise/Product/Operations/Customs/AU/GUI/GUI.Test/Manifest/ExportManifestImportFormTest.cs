using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ExportManifestImportFormTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestOnSaveButtonClickNew()
		{
			Manifest.CalcExportManifest.EmptyContainerCount = 3;
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("precondition", Header);
				AssertEquals("precondition", 3, CalcHeader.EmptyContainerCount);
				AssertFormClosed(form, () => form.SaveButton.PerformClick());
				AssertEquals("holder was saved", true, Header.IsInDatabase);
				AssertEquals("holder was saved", (ZShort)3, Header.ED_NoOfEmptyContainers);
				AssertEquals("Should be NO messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestOnSaveButtonClickExistsNotSubmittedDecline()
		{
			Manifest.ExportManifest = GetNewHeader(false);
			Factory.Save();
			ZString expected = "Question The manifest for TestVessel - 123 - AUSYD - HK already exists, but the customs process has not started yet.\r\n" + "Are you sure you would like to override the existing Customs Export Manifest for AUSYD with the latest data from Bookings/Bills?\r\n\r\n" + "Note. If you've added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("precondition", (ZShort)0, Header.ED_NoOfEmptyContainers);
				CalcHeader.EmptyContainerCount = 3;
				AssertFormClosed(form, () => form.SaveButton.PerformClick());
				AssertEquals("holder was NOT saved", (ZShort)0, Header.ED_NoOfEmptyContainers);
				AssertEquals("Should be Question message", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestOnSaveButtonClickExistsNotSubmittedAccept()
		{
			Manifest.ExportManifest = GetNewHeader(false);
			CalcHeader.EmptyContainerCount = 3;
			Factory.Save();
			ZString expected = "Question The manifest for TestVessel - 123 - AUSYD - HK already exists, but the customs process has not started yet.\r\n" + "Are you sure you would like to override the existing Customs Export Manifest for AUSYD with the latest data from Bookings/Bills?\r\n\r\n" + "Note. If you've added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("precondition", (ZShort)0, Header.ED_NoOfEmptyContainers);
				AssertEquals("precondition", 3, CalcHeader.EmptyContainerCount);
				AssertFormClosed(form, () => form.SaveButton.PerformClick());
				AssertEquals("holder was saved", (ZShort)3, Header.ED_NoOfEmptyContainers);
				AssertEquals("Should be Question message", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestOnSaveButtonClickExistsSubmittedDecline()
		{
			List<ExportCustomsManifestHeader> headers = new List<ExportCustomsManifestHeader>();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object formObject) =>
			{
				var form = formObject as ExportManifestForm;
				if (form != null)
				{
					headers.Add((ExportCustomsManifestHeader)form.BusinessEntity);
				}
			});
			Manifest.ExportManifest = GetNewHeader(true);
			CalcHeader.EmptyContainerCount = 3;
			Factory.Save();
			ZString expected = "Question Manifest for TestVessel - 123 - AUSYD - HK is already reported to Customs.\r\n" + "If you press OK you will be redirected to Customs Export Manifest screen to review the changes and generate an amendment message to Customs.\r\n\r\n" + "Note. If you have added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("precondition", (ZShort)0, Header.ED_NoOfEmptyContainers);
				AssertEquals("precondition", 3, CalcHeader.EmptyContainerCount);
				AssertFormClosed(form, () => form.SaveButton.PerformClick());
				AssertEquals("holder was NOT saved", (ZShort)0, Header.ED_NoOfEmptyContainers);
				AssertEquals("Should be Error messages", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should NOT be shown the ExportManifestForm", 0, headers.Count);
			}
		}

		[GuiTest]
		public void TestOnSaveButtonClickExistsSubmittedAccept()
		{
			List<ExportCustomsManifestHeader> headers = new List<ExportCustomsManifestHeader>();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object formObject) =>
			{
				var form = formObject as ExportManifestForm;
				if (form != null)
				{
					headers.Add((ExportCustomsManifestHeader)form.BusinessEntity);
				}
			});
			Manifest.ExportManifest = GetNewHeader(true);
			CalcHeader.EmptyContainerCount = 3;
			Factory.Save();
			ZString expected = "Question Manifest for TestVessel - 123 - AUSYD - HK is already reported to Customs.\r\n" + "If you press OK you will be redirected to Customs Export Manifest screen to review the changes and generate an amendment message to Customs.\r\n\r\n" + "Note. If you have added any CAN's manually to the Customs Export Manifest, they will be OVERRIDDEN by the latest data gathered from Bookings/Bills.";
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("precondition", (ZShort)0, Header.ED_NoOfEmptyContainers);
				AssertEquals("precondition", 3, CalcHeader.EmptyContainerCount);
				AssertFormClosed(form, () => form.SaveButton.PerformClick());
				AssertEquals("Should be Error messages", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should be shown the ExportManifestForm", 1, headers.Count);
				AssertEquals("Should be the same header", Header.PK, headers[0].PK);
				AssertEquals("Header should be changed", (ZShort)3, headers[0].ED_NoOfEmptyContainers);
			}
		}

		[GuiTest]
		public void TestOnCancelButtonClick()
		{
			Manifest.ExportManifest = GetNewHeader(false);
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				AssertEquals("precondition", false, Header.IsInDatabase);
				AssertFormClosed(form, () => form.CancellButton.PerformClick());
				AssertEquals("holder was NOT saved", false, Header.IsInDatabase);
				AssertEquals("Should be NO messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestOnFormClose()
		{
			Manifest.ExportManifest = GetNewHeader(false);
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				AssertEquals("precondition", false, Header.IsInDatabase);
				AssertFormClosed(form, () => form.Close());
				AssertEquals("holder was NOT saved", false, Header.IsInDatabase);
				AssertEquals("Should be NO messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestFormCaption()
		{
			Holder.VesselName = "TestVessel";
			Holder.VoyageNumber = "111";
			using (ExportManifestImportForm form = new ExportManifestImportForm(holder))
			{
				form.Show();
				AssertEquals("FormCaption", "Create Export Manifests for Vessel: TestVessel, Voyage: 111", form.Text);
			}
		}

		[GuiTest]
		public void TestDuplicatedManifests()
		{
			ExportCustomsManifestHeader header1 = GetNewHeader(false);
			header1.ED_FolioReference = "Folio1";
			ExportCustomsManifestHeader header2 = GetNewHeader(false);
			header2.ED_FolioReference = "Folio2";
			TemporaryManifest manifest1 = new TemporaryManifest(Factory, CalcHeader);
			manifest1.IsDuplicated = true;
			manifest1.ExportManifest = header1;
			TemporaryManifest manifest2 = new TemporaryManifest(Factory, CalcHeader);
			manifest2.IsDuplicated = true;
			manifest2.ExportManifest = header2;
			TemporaryManifestHolder manifestHolder = new TemporaryManifestHolder(Factory);
			manifestHolder.Manifests.Add(manifest1);
			manifestHolder.Manifests.Add(manifest2);
			string expected = "Error You have already created under Customs Export Manifest manifests with the following identical details:\r\n" + "TestVessel-123-Port of Departure:AUSYD (Folio:Folio1)\r\n" + "TestVessel-123-Port of Departure:AUSYD (Folio:Folio2)\r\n\r\n" + "In order to continue with automated upgrade you can do one of the following:\r\n" + "1) manually combine the above manifests into a single manifest;\r\n" + "2) ensure each of the Customs Export Manifests have a unique combination of Vessel-Voyage-Port of Departure.\r\n" + "3) proceed with manual Export Manifest data entry.";
			using (ExportManifestImportForm form = new ExportManifestImportForm(manifestHolder))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SaveButton.PerformClick();
				AssertEquals("Should be Error message", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation
		delegate void DoFormAction();
		void AssertFormClosed(ExportManifestImportForm form, DoFormAction formAction)
		{
			bool formClosed = false;
			form.FormClosed += (object sender, FormClosedEventArgs args) => formClosed = true;
			form.Show();
			AssertEquals("precondition", false, formClosed);
			formAction();
			AssertEquals("form has been closed after action", true, formClosed);
		}

		ExportCustomsManifestHeader GetNewHeader(bool submitted)
		{
			ExportCustomsManifestHeader result = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			if (submitted)
			{
				result.ED_CAN = "12345";
			}

			result.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			result.ED_DepartureDate = ZDateTime.Now;
			result.ED_RL_NKPortOfDeparture = "AUSYD";
			result.ED_RN_NKCountryOfDestination = "HK";
			result.ED_TransportMode = Core.Constants.TransportModes.Sea;
			result.ED_VesselName = Vessel.RV_Code;
			result.ED_VoyageNumber = "123";
			return result;
		}

		TemporaryManifestHolder Holder
		{
			get
			{
				if (holder == null)
				{
					holder = new TemporaryManifestHolder(Factory);
					holder.Manifests.Add(new TemporaryManifest(Factory, CalcHeader));
				}

				return holder;
			}
		}

		TemporaryManifestHolder holder;
		TemporaryManifest Manifest
		{
			get
			{
				return Holder.Manifests[0];
			}
		}

		CalcExportManifestHeader CalcHeader
		{
			get
			{
				if (calcHeader == null)
				{
					calcHeader = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest)
					{ Departure = "AUSYD", DepartureDate = ZDateTime.Now, DischargeCountry = "HK", VesselName = Vessel.RV_Code, VoyageNumber = "123", };
				}

				return calcHeader;
			}
		}

		CalcExportManifestHeader calcHeader;
		ExportCustomsManifestHeader Header
		{
			get
			{
				return Manifest.ExportManifest;
			}
		}

		RefVessel Vessel
		{
			get
			{
				if (vessel == null)
				{
					vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Code = "TestVessel";
				}

				return vessel;
			}
		}

		RefVessel vessel;
		#endregion
	}
}
