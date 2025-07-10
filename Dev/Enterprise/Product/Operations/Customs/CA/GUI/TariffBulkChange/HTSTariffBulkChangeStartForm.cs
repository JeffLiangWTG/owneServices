using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	[SuppressBindingMemberBashingTest] // to suppress AutomaticConvertZCheckBox
	public partial class HTSTariffBulkChangeStartForm : ZChildForm
	{
		public HTSTariffBulkChangeStartForm(CAHTSTariffBulkChange businessEntity)
			: base(businessEntity)
		{
		}

		public new CAHTSTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as CAHTSTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("49b503e2-8923-4c2e-97a3-8f992e67a257", "Tariff Bulk Change"); }
		}

		void TariffBulkChangeFile(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = Enterprise.Customs.Business.BaseTariffBulkChange.TariffBulkChangeConcordanceFileTitle;
				dialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt|All files (*.*)|*.*";
																									   //Dialog.FileName;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;
				dialog.DefaultExt = "csv";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (Stream str = dialog.OpenFile())
						{
							BusinessEntity.LoadConcordance(str, AutomaticConvertZCheckBox.Checked, isSaveAllowed: true);
							ZFormModaliser.ShowDialogAndDispose(new HTSTariffBulkChangeForm(BusinessEntity, AutomaticConvertZCheckBox.Checked));
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

		void TariffBulkChangeImbeded(object sender, EventArgs e)
		{
			//BusinessEntity.CheckAndRemoveAnyExistingPendingChanges();
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.CA.GUI.TariffBulkChange.CAHTSTariffConcordance.csv");
			BusinessEntity.LoadConcordance(stream, AutomaticConvertZCheckBox.Checked, isSaveAllowed: true, isImbeddedConcordance: true);
			ZFormModaliser.ShowDialogAndDispose(new HTSTariffBulkChangeForm(BusinessEntity, AutomaticConvertZCheckBox.Checked));
			ClearHasChangesAndClose();
		}

		void ApplyTariffChanges(object sender, EventArgs e)
		{
			if (Globals.Message.Show(BusinessEntity.ApplyTariffChangesMessage, Enterprise.Customs.Business.BaseTariffBulkChange.TariffBulkChangeFinalConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.ApplyPendingTariffChanges();
				ClearHasChangesAndClose();
			}
		}
	}
}
