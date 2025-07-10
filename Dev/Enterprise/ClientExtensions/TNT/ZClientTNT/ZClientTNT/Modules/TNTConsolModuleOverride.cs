using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.TNT.NZ;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT
{
	public class TNTConsolModuleOverride : JobConsolModule
	{
		public TNTConsolModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.NewZealand:
					AddImportDataMenuItem("Quantum File", new EventHandler(NZDataMenuItem_Click));
					break;

				default:
					AddImportDataMenuItem("Quantum Exit&2", new EventHandler(AUDataMenuItem_Click));
					break;
			}
		}

		#region Data Import Button Clicks

		void AUDataMenuItem_Click(object sender, EventArgs e)
		{
			string fileFullPath = GetFilePathToImport(TNTDataRegistry.Instance.QuantumFileSourceDirectoryForManualImport).Trim();

			if (!string.IsNullOrEmpty(fileFullPath))
			{
				using (ZOpenFileDialog.ForceLocalFile(ref fileFullPath))
				{
					Exit2ImportManager importManager = new Exit2ImportManager(new BusinessObjectFactory(), fileFullPath);
					if (importManager.Exit2Mawbs.Count > 0)
					{
						Exit2ImportForm form = new Exit2ImportForm(importManager);
						form.Show();
					}
					else
					{
						ShowWarning();
					}
				}
			}
		}

		void NZDataMenuItem_Click(object sender, EventArgs e)
		{
			string fileFullPath = GetFilePathToImport(TNTDataRegistry.Instance.QuantumFileSourceDirectoryForManualImport).Trim();

			if (!string.IsNullOrEmpty(fileFullPath))
			{
				using (ZOpenFileDialog.ForceLocalFile(ref fileFullPath))
				{
					NZImportManager importManager = new NZImportManager(new BusinessObjectFactory(), fileFullPath);
					if (importManager.Exit2Mawbs.Count > 0)
					{
						QuantumNZForm form = new QuantumNZForm(importManager);
						form.Show();
					}
					else
					{
						ShowWarning();
					}
				}
			}
		}

		void ShowWarning()
		{
			Globals.Message.ShowWarning("File contains no segments");
		}

		protected string GetFilePathToImport(string initialDirectory)
		{
			string result = "";

			using (ZOpenFileDialog dialog = new ZOpenFileDialog())
			{
				dialog.InitialDirectory = initialDirectory;
				dialog.Filter = String.Format(CultureInfo.CurrentCulture, "Quantum{0}Files (*{1})|*{1}", FileType, QuantumFile.Extension);
				dialog.Title = "Select a Quantum" + FileType + "file to import";

				if (ShowDialog(dialog) == DialogResult.OK)
				{
					FileInfo fileInfo = new FileInfo(dialog.UnmappedFileName);
					if (Regex.IsMatch(fileInfo.Name, FilePattern, RegexOptions.IgnoreCase))
					{
						result = fileInfo.FullName;
					}
					else
					{
						Globals.Message.ShowError("Incorrect File Name Format", "File Name Format");
					}
				}
			}

			return result;
		}

		protected virtual
 DialogResult ShowDialog(ZOpenFileDialog dialog)
		{
			return dialog.ShowDialog();
		}

		#endregion

		#region Implementation

		string FileType
		{
			get { return CurrentCountryIsAustralia ? " Exit2 " : " "; }
		}

		bool CurrentCountryIsAustralia
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia; }
		}

		string FilePattern
		{
			get { return @"^\w{3}\." + (CurrentCountryIsAustralia ? "[X][12]" : "(X2|IND)") + @"\.\d{8}\.\d{6}\." + QuantumFile.Extension.Replace(".", "") + "$"; }
		}

		#endregion
	}
}
