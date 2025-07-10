using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	[SuppressBindingMemberBashingTest] // to suppress AutomaticConvertZCheckBox
	public partial class AUImportTariffBulkChangeStartForm : ZChildForm
	{
		public AUImportTariffBulkChangeStartForm(AUImportTariffBulkChange businessEntity)
			: base(businessEntity)
		{
		}

		public new AUImportTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as AUImportTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return "Tariff Bulk Change"; }
		}

		void TariffBulkChangeFile(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = "Select Tariff Bulk Change Concordance File";
				dialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt|All files (*.*)|*.*";
				//Dialog.FileName;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;
				dialog.DefaultExt = "csv";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = dialog.OpenFile())
						{
							BusinessEntity.LoadConcordance(stream, AutomaticConvertZCheckBox.Checked, isSaveAllowed: true);
							ZFormModaliser.ShowDialogAndDispose(new AUImportTariffBulkChangeForm(BusinessEntity));
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
			ClearHasChangesAndClose();
		}

		void ClearHasChangesAndClose()
		{
			if (BusinessEntityForHasChanges != null)
			{
				BusinessEntityForHasChanges.ClearHasChangesIncludingChildren();
			}

			Dispose();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void TCOBulkChangeImbeded(object sender, EventArgs e)
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.AU.Declaration.GUI.TariffBulkChange.AUTCOConcordance.csv");
			BusinessEntity.ChangeTCO(stream, isSaveAllowed: true);
			//BusinessEntity.ChangeTCO(stream);
			ZFormModaliser.ShowDialogAndDispose(new AUImportTCOBulkChangeForm(BusinessEntity));
			ClearHasChangesAndClose();
		}

		void TCOBulkChangeFile(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = "Select TCO Bulk Change Concordance File";
				dialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt|All files (*.*)|*.*";
				//Dialog.FileName;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;
				dialog.DefaultExt = "csv";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = dialog.OpenFile())
						{
							BusinessEntity.ChangeTCO(stream, isSaveAllowed: true);
							ZFormModaliser.ShowDialogAndDispose(new AUImportTCOBulkChangeForm(BusinessEntity));
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
			ClearHasChangesAndClose();
		}

		void ApplyTariffChanges(object sender, EventArgs e)
		{
			if (Globals.Message.Show(BusinessEntity.ApplyTariffChangesMessage, "Final Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.ApplyPendingTariffChanges();
				Close();
			}
		}
	}
}
