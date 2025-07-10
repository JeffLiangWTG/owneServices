using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoStandAloneMenu : SeaCargoMenu
	{
		public SeaCargoStandAloneMenu(CusSCAOceanBillMessageManager messageManager) : base(messageManager)
		{
			oceanBill = messageManager.OceanBill;
		}

		readonly CusSCAOceanBill oceanBill;

		protected override ZArchitecture.Data.Mutex.ZGlobalMutex GetMutex()
		{
			return oceanBill != null ? oceanBill.SendSEACRMutex : null;
		}

		protected override void InitializeMenu()
		{
			base.InitializeMenu();

			var scheduleOriginalSendingMenuItem = new ZMenuItem("Schedule Out-of-Hours Original Message Sending", ScheduleOriginalSendingMenuItem_Click);
			MenuItems.Add(scheduleOriginalSendingMenuItem);
			MenuItems.Add("-");
			MenuItems.Add(new ZMenuItem(Res.GetString("54B638BB-835C-4A13-86C6-E16313C86640", "Create Contingency Data"), new EventHandler(ContingencyMenuItem_Click)));
			MenuItems.Add(new ZMenuItem(Res.GetString("9F3A912D-4AF5-41FD-A007-BB8041807FF7", "Export Sea Cargo Containers Data"), new EventHandler(ExportSeaCargoContainerDataMenuItem_Click)));
		}

		protected void ScheduleOriginalSendingMenuItem_Click(object sender, EventArgs e)
		{
			if (manager != null)
			{
				var mutex = GetMutex();
				if (mutex != null && mutex.IsLocked)
				{
					DisplayLockedMessage(mutex.GetLockInfo());
					return;
				}

				SeaCargoMenuHelper.ScheduleOriginalSending(oceanBill);
			}
			else
			{
				Globals.Message.Show("The Sea Cargo job has NOT been created. Please click on the Sea Cargo tab before scheduling message sending.", "No Sea Cargo", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void ContingencyMenuItem_Click(object sender, EventArgs args)
		{
			new CMRExportForm(MainForm, new CusSCAOceanBillExporter(oceanBill)).Export();
		}

		#region ExportSeaCargoContainerData

		void ExportSeaCargoContainerDataMenuItem_Click(object sender, EventArgs e)
		{
			if (oceanBill != null)
			{
				if (oceanBill.HasChanges)
				{
					if (Globals.Message.Show(Res.GetString("1088C82D-D0F6-4668-8881-63ABE516DD63", "There are changes on this form. Do you want to save changes first?"), Res.GetString("2F0685C7-5B34-4221-8464-B44492EA21B8", "Save"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
					{
						if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
						{
							ExportSeaCargoContainerData();
						}
					}
				}
				else
				{
					ExportSeaCargoContainerData();
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ExportSeaCargoContainerData()
		{
			if (oceanBill != null)
			{
				var exportFileName = ZString.Format("ExportOceanBill_{0}_{1}.csv", oceanBill.CB_OceanBill, ZDateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture));

				using (var saveFileDialog = new ZFolderBrowserDialog())
				{
					saveFileDialog.CreateDirectory = false;
					saveFileDialog.RequireMappablePath = true;
					saveFileDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
					saveFileDialog.ShowNewFolderButton = true;

					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						try
						{
							var exporter = new SeaCargoContainersExporter(oceanBill);
							exporter.Generate();
							string exportFilePath = Path.Combine(saveFileDialog.UnmappedSelectedPath, exportFileName);
							using (var stream = exporter.GetStream(exportFilePath))
							{
								if (stream != Stream.Null)
								{
									exporter.SaveToFile(stream);
									Globals.Message.ShowInformation(Res.GetString("B4E362AD-2A6A-4A11-9389-830168508D1E", "The data has been exported to {0}", exportFilePath));
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Globals.Message.ShowError(ex.Message);
						}
					}
				}
			}
		}

		#endregion
	}
}
